using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FPTCanteen.Data;
using FPTCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FPTCanteen.PosClient;

internal class Program
{
    private const string Host = "127.0.0.1";
    private const int Port = 9500;
    private const int UdpPort = 9501;
    private static readonly object MenuLock = new();

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

    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var posName = args.Length > 0 ? args[0] : "QUAY01";
        Console.Title = $"FCanteen - {posName}";

        await using var db = CreateDbContext();
        var menuItems = await db.MenuItems.AsNoTracking().Where(m => m.IsAvailable).ToListAsync();

        if (menuItems.Count == 0)
        {
            Console.WriteLine("[PosClient] Khong co mon an nao trong menu.");
            return;
        }

        // Nghe thong bao het mon / co lai tu bep (UDP broadcast)
        _ = UdpListenLoopAsync(menuItems);

        Console.WriteLine($"========== MENU - {posName} ==========");
        Console.WriteLine($"{"STT",-5} {"Ma",-8} {"Ten mon",-25} {"Don gia",12} {"Don vi",10}");
        Console.WriteLine(new string('-', 62));
        for (int i = 0; i < menuItems.Count; i++)
        {
            var m = menuItems[i];
            Console.WriteLine($"{i + 1,-5} {m.Code,-8} {m.Name,-25} {m.Price,12:N0} {m.Unit,-10}");
        }
        Console.WriteLine(new string('-', 62));

        var lines = new List<(int MenuItemId, string Name, decimal Price, int Quantity, string Note)>();

        while (true)
        {
            Console.Write("\nNhap STT mon (0 de thanh toan): ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) continue;

            if (input == "0") break;

            if (!int.TryParse(input, out var stt) || stt < 1 || stt > menuItems.Count)
            {
                Console.WriteLine("[PosClient] STT khong hop le.");
                continue;
            }

            MenuItem selected;
            lock (MenuLock)
            {
                selected = menuItems[stt - 1];
                if (!selected.IsAvailable)
                {
                    Console.WriteLine($"[PosClient] Mon {selected.Name} vua het, vui long chon mon khac.");
                    continue;
                }
            }

            Console.Write($"  So luong {selected.Name}: ");
            if (!int.TryParse(Console.ReadLine()?.Trim(), out var qty) || qty < 1)
            {
                Console.WriteLine("[PosClient] So luong khong hop le.");
                continue;
            }

            Console.Write($"  Ghi chu (Enter de bo qua): ");
            var note = Console.ReadLine()?.Trim() ?? "";

            lines.Add((selected.Id, selected.Name, selected.Price, qty, note));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Da them: {selected.Name} x{qty} = {(selected.Price * qty):N0}d");
            Console.ResetColor();

            if (lines.Count > 0)
            {
                Console.WriteLine("\n--- Tam tinh ---");
                decimal subtotal = 0;
                foreach (var l in lines)
                {
                    Console.WriteLine($"  {l.Name} x{l.Quantity} = {(l.Price * l.Quantity):N0}d");
                    subtotal += l.Price * l.Quantity;
                }
                Console.WriteLine($"  Tong tam tinh: {subtotal:N0}d");
                Console.WriteLine("-----------------");
            }
        }

        if (lines.Count == 0)
        {
            Console.WriteLine("[PosClient] Khong co mon nao. Huy phieu.");
            return;
        }

        // Gui phieu len KitchenServer
        var order = new
        {
            PosName = posName,
            Lines = lines.Select(l => new { l.MenuItemId, l.Quantity, l.Note }).ToList()
        };

        var json = JsonSerializer.Serialize(order);
        var data = Encoding.UTF8.GetBytes(json);

        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(Host, Port);
        }
        catch (SocketException)
        {
            Console.WriteLine("[PosClient] Khong ket noi duoc toi KitchenServer. Hay chay KitchenServer truoc.");
            return;
        }

        Console.WriteLine($"\n[PosClient] Dang gui phieu len KitchenServer...");
        var stream = client.GetStream();
        await stream.WriteAsync(data, 0, data.Length);

        var buffer = new byte[4096];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        if (bytesRead > 0)
        {
            var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var response = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var ticketId = response.GetProperty("TicketId").GetInt32();
            var totalAmount = response.GetProperty("TotalAmount").GetDecimal();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n========== XAC NHAN ==========");
            Console.WriteLine($"  Phieu so: #{ticketId}");
            Console.WriteLine($"  Quay: {posName}");
            Console.WriteLine($"  Tong tien: {totalAmount:N0}d");
            Console.WriteLine($"  Trang thai: Dang cho che bien");
            Console.WriteLine($"================================");
            Console.ResetColor();
        }

        client.Close();
        Console.WriteLine("[PosClient] Da gui phieu. Nhan phim bat ki de thoat.");
        Console.ReadKey();
    }

    // Nhan datagram UDP tu bep: "OUTOFSTOCK|<id>|<ten>" hoac "AVAILABLE|<id>|<ten>".
    // ReuseAddress de nhieu quay (QUAY01/02/03) cung nghe chung mot cong UDP.
    private static async Task UdpListenLoopAsync(List<MenuItem> menu)
    {
        using var udp = new UdpClient();
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, UdpPort));

        while (true)
        {
            UdpReceiveResult result;
            try
            {
                result = await udp.ReceiveAsync();
            }
            catch
            {
                break;
            }

            var msg = Encoding.UTF8.GetString(result.Buffer);
            var parts = msg.Split('|');
            if (parts.Length < 3 || !int.TryParse(parts[1], out var id)) continue;

            lock (MenuLock)
            {
                var item = menu.FirstOrDefault(m => m.Id == id);
                if (item == null) continue;

                if (parts[0] == "OUTOFSTOCK" && item.IsAvailable)
                {
                    item.IsAvailable = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[Thong bao bep] Mon {item.Name} vua HET, khong chon duoc nua.");
                    Console.ResetColor();
                }
                else if (parts[0] == "AVAILABLE" && !item.IsAvailable)
                {
                    item.IsAvailable = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[Thong bao bep] Mon {item.Name} da CO LAI.");
                    Console.ResetColor();
                }
            }
        }
    }
}
