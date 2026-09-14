using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FPTCanteen.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Protocol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SourceAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderTicketId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLines_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketLines_OrderTickets_OrderTicketId",
                        column: x => x.OrderTicketId,
                        principalTable: "OrderTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Code", "IsAvailable", "Name", "Price", "Unit" },
                values: new object[,]
                {
                    { 1, "COM01", true, "Cơm sườn nướng", 25000m, "Phần" },
                    { 2, "COM02", true, "Cơm gà chiên", 22000m, "Phần" },
                    { 3, "COM03", true, "Cơm chiên dương châu", 20000m, "Phần" },
                    { 4, "PHO01", true, "Phở bò", 30000m, "Tô" },
                    { 5, "PHO02", true, "Phở gà", 28000m, "Tô" },
                    { 6, "MI01", true, "Mì xào bò", 22000m, "Phần" },
                    { 7, "MI02", true, "Mì xào hải sản", 25000m, "Phần" },
                    { 8, "BUN01", true, "Bún bò Huế", 30000m, "Tô" },
                    { 9, "BUN02", true, "Bún chả", 25000m, "Phần" },
                    { 10, "CAN01", true, "Canh chua cá lóc", 18000m, "Phần" },
                    { 11, "RAU01", true, "Rau muống xào tỏi", 12000m, "Phần" },
                    { 12, "TRA01", true, "Trà đá", 5000m, "Ly" },
                    { 13, "TRA02", true, "Trà sen", 8000m, "Ly" },
                    { 14, "NUOC01", true, "Nước ngọt", 10000m, "Chai" },
                    { 15, "SUA01", true, "Sữa chua uống", 12000m, "Chai" },
                    { 16, "CAFE01", true, "Cà phê sữa", 15000m, "Ly" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_MenuItemId",
                table: "TicketLines",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_OrderTicketId",
                table: "TicketLines",
                column: "OrderTicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceLogs");

            migrationBuilder.DropTable(
                name: "TicketLines");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "OrderTickets");
        }
    }
}
