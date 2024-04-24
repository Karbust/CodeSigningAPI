using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IPsWhitelist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IP = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FirstUsableIP = table.Column<string>(type: "text", nullable: false),
                    LastUsableIP = table.Column<string>(type: "text", nullable: false),
                    FirstUsableIPNumeric = table.Column<BigInteger>(type: "numeric", nullable: false),
                    LastUsableIPNumeric = table.Column<BigInteger>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPsWhitelist", x => new { x.Id, x.IP });
                });

            migrationBuilder.CreateTable(
                name: "CodeSignings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    OriginalHash = table.Column<string>(type: "text", nullable: false),
                    SignedHash = table.Column<string>(type: "text", nullable: false),
                    Algorithm = table.Column<string>(type: "text", nullable: false),
                    SignedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSignings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeSignings_AuthTokens_SignedById",
                        column: x => x.SignedById,
                        principalTable: "AuthTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AuthTokens",
                columns: new[] { "Id", "CreatedOn", "Description", "IsRevoked", "RevokedOn", "Token", "UpdatedOn" },
                values: new object[] { 1, new DateTime(2024, 2, 19, 18, 22, 41, 561, DateTimeKind.Utc).AddTicks(4828), "System", false, null, "SYSTEM", new DateTime(2024, 2, 19, 18, 22, 41, 561, DateTimeKind.Utc).AddTicks(4842) });

            migrationBuilder.CreateIndex(
                name: "IX_CodeSignings_SignedById",
                table: "CodeSignings",
                column: "SignedById");

            migrationBuilder.CreateIndex(
                name: "IX_IPsWhitelist_IP",
                table: "IPsWhitelist",
                column: "IP",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeSignings");

            migrationBuilder.DropTable(
                name: "IPsWhitelist");

            migrationBuilder.DropTable(
                name: "AuthTokens");
        }
    }
}
