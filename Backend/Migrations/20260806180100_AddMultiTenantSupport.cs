using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Tenants ON;
                INSERT INTO Tenants (Id, ShopCode, Name, IsActive, CreatedAtUtc)
                VALUES (1, 'demo', 'Demo Shop', 1, GETUTCDATE());
                SET IDENTITY_INSERT Tenants OFF;
            ");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Receipts",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
                BEGIN
                    CREATE TABLE Users (
                        Id int NOT NULL IDENTITY,
                        TenantId int NOT NULL CONSTRAINT DF_Users_TenantId DEFAULT 1,
                        BranchId int NULL,
                        Username nvarchar(50) NOT NULL,
                        PasswordHash nvarchar(max) NOT NULL,
                        Role int NOT NULL,
                        CreatedAt datetime2 NOT NULL,
                        CONSTRAINT PK_Users PRIMARY KEY (Id)
                    );
                END
                ELSE
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TenantId')
                        ALTER TABLE Users ADD TenantId int NOT NULL CONSTRAINT DF_Users_TenantId DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'BranchId')
                        ALTER TABLE Users ADD BranchId int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefreshTokens')
                BEGIN
                    CREATE TABLE RefreshTokens (
                        Id int NOT NULL IDENTITY,
                        Token nvarchar(450) NOT NULL,
                        UserId int NOT NULL,
                        Created datetime2 NOT NULL,
                        Expires datetime2 NOT NULL,
                        Revoked datetime2 NULL,
                        ReplacedByToken nvarchar(max) NULL,
                        CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
                        CONSTRAINT FK_RefreshTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
                    DROP INDEX IX_Users_Username ON Users;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_TenantId",
                table: "Receipts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ShopCode",
                table: "Tenants",
                column: "ShopCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Username",
                table: "Users",
                columns: new[] { "TenantId", "Username" },
                unique: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshTokens_Token' AND object_id = OBJECT_ID('RefreshTokens'))
                    CREATE UNIQUE INDEX IX_RefreshTokens_Token ON RefreshTokens (Token);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId' AND object_id = OBJECT_ID('RefreshTokens'))
                    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens (UserId);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tenants_TenantId",
                table: "Products",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Tenants_TenantId",
                table: "Receipts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tenants_TenantId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Tenants_TenantId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ShopCode",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_TenantId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Receipts");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'BranchId')
                    ALTER TABLE Users DROP COLUMN BranchId;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TenantId')
                    ALTER TABLE Users DROP COLUMN TenantId;
            ");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
