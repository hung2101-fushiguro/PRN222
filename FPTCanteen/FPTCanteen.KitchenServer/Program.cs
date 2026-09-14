using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FPTCanteen.KitchenServer.Dtos;
using FPTCanteen.Data;
using FPTCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FPTCanteen.KitchenServer;

internal class Program
{
    private const int Port = 9500;
    private const int UdpPort = 9501;

    private const string DefaultPriceUrl = "https://raw.githubusercontent.com/hung2101-fushiguro/PRN222/refs/heads/lab01/prices.sample.json";

    private static string GetConnectionString()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        return config.GetConnectionString("FPTCanteenDB")
            ?? throw new InvalidOperationException("Khong tim thay connection string.");
    }

    private static FPTCanteenContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FPTCanteenContext>()
            .UseSqlServer(GetConnectionString())
            .Options;
        return new FPTCanteenContext(options);
    }

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "FPTCanteen Kitchen Server";

        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"[KitchenServer] Dang lang nghe tai port {Port} ...");
        Console.WriteLine("Lenh: menu | het <id> | conban <id> | sync [url]");

        _ = Task.Run(CommandLoopAsync);

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            var endPoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
            Console.WriteLine($"[KitchenServer] Quay ket noi: {endPoint}");
            _ = Task.Run(() => HandleClientAsync(client, endPoint));
        }
    }

    // ------------------------------------------------------- Vong lenh console
    private static async Task CommandLoopAsync()
    {
        while (true)
        {
            var line = Console.ReadLine();
            if (line is null) break;

            var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0].ToLowerInvariant())
            {
                case "menu":
                    await ShowMenuAsync();
                    break;

                case "het":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out var outId))
                        Console.WriteLine("Cu phap: het <id>");
                    else
                        await SetAvailabilityAsync(outId, false);
                    break;

                case "conban":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out var inId))
                        Console.WriteLine("Cu phap: conban <id>");
                    else
                        await SetAvailabilityAsync(inId, true);
                    break;

                case "sync":
                    await SyncPricesAsync(parts.Length > 1 ? parts[1].Trim() : DefaultPriceUrl);
                    break;

                default:
                    Console.WriteLine("Lenh: menu | het <id> | conban <id> | sync [url]");
                    break;
            }
        }
    }

    private static async Task ShowMenuAsync()
    {
        await using var db = CreateDbContext();
        var items = await db.MenuItems.AsNoTracking().OrderBy(m => m.Id).ToListAsync();
        Console.WriteLine($"{"Id",-5} {"Ma",-8} {"Ten mon",-25} {"Don gia",12} {"Trang thai"}");
        foreach (var m in items)
            Console.WriteLine($"{m.Id,-5} {m.Code,-8} {m.Name,-25} {m.Price,12:N0} {(m.IsAvailable ? "Con ban" : "HET")}");
    }

    // ------------------------------------------------------- UDP broadcast
    private static async Task SetAvailabilityAsync(int menuItemId, bool available)
    {
        await using var db = CreateDbContext();
        var item = await db.MenuItems.FindAsync(menuItemId);
        if (item == null)
        {
            Console.WriteLine($"[KitchenServer] Khong tim thay mon id={menuItemId}.");
            return;
        }

        item.IsAvailable = available;
        var kind = available ? "AVAILABLE" : "OUTOFSTOCK";
        db.DeviceLogs.Add(new DeviceLog
        {
            Protocol = "UDP",
            SourceAddress = "127.0.0.1",
            content = $"Broadcast {kind} id={item.Id} name={item.Name}"
        });
        await db.SaveChangesAsync();

        using var udp = new UdpClient();
        udp.EnableBroadcast = true;
        var msg = $"{kind}|{item.Id}|{item.Name}";
        var data = Encoding.UTF8.GetBytes(msg);
        await udp.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, UdpPort));

        Console.WriteLine($"[UDP] Da phat thong bao: {msg}");
    }

    // ------------------------------------------------------- Dong bo gia
    private static async Task SyncPricesAsync(string url)
    {
        // 1. Phan tich dia chi bang Uri
        var uri = new Uri(url);
        Console.WriteLine($"[Sync] Scheme: {uri.Scheme}, Host: {uri.Host}, Port: {uri.Port}");

        // 2. Phan giai ten mien bang Dns
        var entry = await Dns.GetHostEntryAsync(uri.Host);
        Console.WriteLine($"[Sync] {entry.HostName} phan giai duoc {entry.AddressList.Length} IP:");
        foreach (var ip in entry.AddressList)
            Console.WriteLine($"  {ip.AddressFamily,-12} {ip}");

        // 3. Tai file JSON bang HttpClient
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var sw = Stopwatch.StartNew();
        using var response = await http.GetAsync(uri);
        var body = await response.Content.ReadAsStringAsync();
        sw.Stop();

        var status = (int)response.StatusCode;
        Console.WriteLine($"[Sync] HTTP {(int)response.StatusCode} {response.ReasonPhrase} in {sw.ElapsedMilliseconds} ms");

        if (!response.IsSuccessStatusCode)
        {
            await using var errDb = CreateDbContext();
            errDb.DeviceLogs.Add(new DeviceLog
            {
                Protocol = "HTTP",
                SourceAddress = uri.Host,
                content = $"GET {url} => {status} in {sw.ElapsedMilliseconds}ms (failed)"
            });
            await errDb.SaveChangesAsync();
            return;
        }

        var feed = JsonSerializer.Deserialize<List<PriceFeedItem>>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        // 4. So sanh voi DB va cap nhat gia thay doi
        await using var db = CreateDbContext();
        var updated = 0;
        var skipped = 0;
        foreach (var f in feed)
        {
            var item = await db.MenuItems.FirstOrDefaultAsync(m => m.Code == f.Code);
            if (item == null)
            {
                Console.WriteLine($"[Sync] Bo qua ma la {f.Code} (khong co trong DB).");
                skipped++;
                continue;
            }

            if (item.Price != f.Price)
            {
                Console.WriteLine($"[Sync] {item.Code} ({item.Name}): {item.Price:N0} -> {f.Price:N0}");
                item.Price = f.Price;
                updated++;
            }
        }

        db.DeviceLogs.Add(new DeviceLog
        {
            Protocol = "HTTP",
            SourceAddress = uri.Host,
            content = $"GET {url} => {status} in {sw.ElapsedMilliseconds}ms ({updated} prices updated, {skipped} skipped)"
        });
        await db.SaveChangesAsync();
        Console.WriteLine($"[Sync] Xong: {updated} gia cap nhat, {skipped} bo qua.");
    }

    // ------------------------------------------------------- TCP order (YC2)
    private static async Task HandleClientAsync(TcpClient client, string endPoint)
    {
        await using var db = CreateDbContext();

        db.DeviceLogs.Add(new DeviceLog
        {
            Protocol = "TCP",
            SourceAddress = endPoint,
            content = "Connection started"
        });
        await db.SaveChangesAsync();

        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0) return;

            var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var dto = JsonSerializer.Deserialize<OrderTicketDto>(json);

            if (dto == null) return;

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var ticket = new OrderTicket
                {
                    PosName = dto.PosName,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                decimal total = 0;
                foreach (var line in dto.Lines)
                {
                    var menuItem = await db.MenuItems.FindAsync(line.MenuItemId);
                    if (menuItem == null) continue;

                    var unitPrice = menuItem.Price;
                    total += unitPrice * line.Quantity;

                    ticket.TicketLines.Add(new TicketLine
                    {
                        MenuItemId = line.MenuItemId,
                        Quantity = line.Quantity,
                        UnitPrice = unitPrice,
                        Note = line.Note
                    });
                }

                ticket.TotalAmount = total;
                db.OrderTickets.Add(ticket);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = JsonSerializer.Serialize(new
                {
                    TicketId = ticket.Id,
                    TotalAmount = ticket.TotalAmount
                });
                var responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                Console.WriteLine($"[Order] Phieu #{ticket.Id} tu {ticket.PosName} - Tong: {ticket.TotalAmount:N0}d - Dang cho che bien");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
        finally
        {
            db.DeviceLogs.Add(new DeviceLog
            {
                Protocol = "TCP",
                SourceAddress = endPoint,
                content = "Connection closed"
            });
            await db.SaveChangesAsync();
            client.Close();
            Console.WriteLine($"[KitchenServer] Quay ngat ket noi: {endPoint}");
        }
    }
}
