using FCanteenLab06.Components;
using FCanteenLab06.Data;
using Microsoft.EntityFrameworkCore;

namespace FCanteenLab06
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddDbContextFactory<CanteenDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            using (var scope = app.Services.CreateScope())
            {
                var f = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CanteenDbContext>>();
                using var db = f.CreateDbContext();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
