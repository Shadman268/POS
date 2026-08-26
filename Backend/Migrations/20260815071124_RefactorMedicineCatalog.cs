using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMedicineCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Products_ProductId",
                table: "ReceiptItems");

            migrationBuilder.AddColumn<int>(
                name: "InventoryMode",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<bool>(
                name: "PromptPriceWhenUnset",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "LineType",
                table: "ReceiptItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "ReceiptItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "ReceiptItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenericName",
                table: "ReceiptItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicineBatchId",
                table: "ReceiptItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicineId",
                table: "ReceiptItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantMedicineId",
                table: "ReceiptItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Strength = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DosageForm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantMedicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsStockTracked = table.Column<bool>(type: "bit", nullable: false),
                    LocalSku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantMedicines_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantMedicines_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicineBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TenantMedicineId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineBatches_TenantMedicines_TenantMedicineId",
                        column: x => x.TenantMedicineId,
                        principalTable: "TenantMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicineBatches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicineStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TenantMedicineId = table.Column<int>(type: "int", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    ReorderLevel = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineStocks_TenantMedicines_TenantMedicineId",
                        column: x => x.TenantMedicineId,
                        principalTable: "TenantMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicineStocks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TenantMedicineId = table.Column<int>(type: "int", nullable: false),
                    MedicineBatchId = table.Column<int>(type: "int", nullable: true),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    QuantityDelta = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_MedicineBatches_MedicineBatchId",
                        column: x => x.MedicineBatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMovements_TenantMedicines_TenantMedicineId",
                        column: x => x.TenantMedicineId,
                        principalTable: "TenantMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMovements_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Migrate legacy Products into global Medicines + TenantMedicines
            migrationBuilder.Sql(@"
                INSERT INTO Medicines (Name, GenericName, Category, Brand, Unit, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT DISTINCT
                    p.ProductName,
                    p.GenericName,
                    ISNULL(p.Category, 'Medicine'),
                    ISNULL(p.Brand, 'General'),
                    ISNULL(p.Unit, 'Tablet'),
                    1,
                    GETUTCDATE(),
                    GETUTCDATE()
                FROM Products p
                WHERE NOT EXISTS (
                    SELECT 1 FROM Medicines m
                    WHERE m.Name = p.ProductName
                      AND ISNULL(m.GenericName, '') = ISNULL(p.GenericName, '')
                );

                INSERT INTO TenantMedicines (TenantId, MedicineId, SellingPrice, IsStockTracked, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT
                    p.TenantId,
                    m.Id,
                    p.Price,
                    1,
                    1,
                    GETUTCDATE(),
                    GETUTCDATE()
                FROM Products p
                INNER JOIN Medicines m ON m.Name = p.ProductName AND ISNULL(m.GenericName, '') = ISNULL(p.GenericName, '')
                WHERE NOT EXISTS (
                    SELECT 1 FROM TenantMedicines tm
                    WHERE tm.TenantId = p.TenantId AND tm.MedicineId = m.Id
                );

                INSERT INTO MedicineStocks (TenantId, TenantMedicineId, QuantityOnHand, UpdatedAtUtc)
                SELECT
                    tm.TenantId,
                    tm.Id,
                    p.StockQuantity,
                    GETUTCDATE()
                FROM Products p
                INNER JOIN Medicines m ON m.Name = p.ProductName AND ISNULL(m.GenericName, '') = ISNULL(p.GenericName, '')
                INNER JOIN TenantMedicines tm ON tm.TenantId = p.TenantId AND tm.MedicineId = m.Id
                WHERE NOT EXISTS (
                    SELECT 1 FROM MedicineStocks ms WHERE ms.TenantMedicineId = tm.Id
                );

                INSERT INTO MedicineBatches (TenantId, TenantMedicineId, BatchNumber, ExpiryDate, QuantityOnHand, ReceivedAtUtc, IsActive)
                SELECT
                    tm.TenantId,
                    tm.Id,
                    ISNULL(p.BatchNumber, CONCAT('LEG-', p.Id)),
                    ISNULL(p.ExpiryDate, DATEADD(year, 1, GETUTCDATE())),
                    p.StockQuantity,
                    GETUTCDATE(),
                    1
                FROM Products p
                INNER JOIN Medicines m ON m.Name = p.ProductName AND ISNULL(m.GenericName, '') = ISNULL(p.GenericName, '')
                INNER JOIN TenantMedicines tm ON tm.TenantId = p.TenantId AND tm.MedicineId = m.Id
                WHERE p.BatchNumber IS NOT NULL OR p.ExpiryDate IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE ri
                SET
                    ri.LineType = 1,
                    ri.MedicineId = m.Id,
                    ri.TenantMedicineId = tm.Id,
                    ri.GenericName = m.GenericName,
                    ri.BatchNumber = p.BatchNumber,
                    ri.ExpiryDate = p.ExpiryDate
                FROM ReceiptItems ri
                INNER JOIN Products p ON p.Id = ri.ProductId
                INNER JOIN Medicines m ON m.Name = p.ProductName AND ISNULL(m.GenericName, '') = ISNULL(p.GenericName, '')
                INNER JOIN TenantMedicines tm ON tm.TenantId = p.TenantId AND tm.MedicineId = m.Id;
            ");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptItems_ProductId",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ReceiptItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_MedicineBatchId",
                table: "ReceiptItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_MedicineId",
                table: "ReceiptItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_TenantMedicineId",
                table: "ReceiptItems",
                column: "TenantMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_TenantId_ExpiryDate",
                table: "MedicineBatches",
                columns: new[] { "TenantId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_TenantMedicineId_BatchNumber",
                table: "MedicineBatches",
                columns: new[] { "TenantMedicineId", "BatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_GenericName",
                table: "Medicines",
                column: "GenericName");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Name",
                table: "Medicines",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineStocks_TenantId",
                table: "MedicineStocks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineStocks_TenantMedicineId",
                table: "MedicineStocks",
                column: "TenantMedicineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_MedicineBatchId",
                table: "StockMovements",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_TenantId_CreatedAtUtc",
                table: "StockMovements",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_TenantMedicineId",
                table: "StockMovements",
                column: "TenantMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMedicines_MedicineId",
                table: "TenantMedicines",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMedicines_TenantId_MedicineId",
                table: "TenantMedicines",
                columns: new[] { "TenantId", "MedicineId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_MedicineBatches_MedicineBatchId",
                table: "ReceiptItems",
                column: "MedicineBatchId",
                principalTable: "MedicineBatches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Medicines_MedicineId",
                table: "ReceiptItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_TenantMedicines_TenantMedicineId",
                table: "ReceiptItems",
                column: "TenantMedicineId",
                principalTable: "TenantMedicines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_MedicineBatches_MedicineBatchId",
                table: "ReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Medicines_MedicineId",
                table: "ReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_TenantMedicines_TenantMedicineId",
                table: "ReceiptItems");

            migrationBuilder.DropTable(
                name: "MedicineStocks");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "MedicineBatches");

            migrationBuilder.DropTable(
                name: "TenantMedicines");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropColumn(
                name: "InventoryMode",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PromptPriceWhenUnset",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LineType",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "GenericName",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "MedicineBatchId",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "MedicineId",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "TenantMedicineId",
                table: "ReceiptItems");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ReceiptItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GenericName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_ProductId",
                table: "ReceiptItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Products_ProductId",
                table: "ReceiptItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
