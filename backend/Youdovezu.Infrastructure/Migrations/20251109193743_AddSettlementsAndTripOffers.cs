using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Youdovezu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementsAndTripOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripOffers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripId = table.Column<long>(type: "bigint", nullable: false),
                    OffererId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripOffers_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripOffers_Users_OffererId",
                        column: x => x.OffererId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_IsActive",
                table: "Settlements",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_Name",
                table: "Settlements",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_Type",
                table: "Settlements",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TripOffers_OffererId",
                table: "TripOffers",
                column: "OffererId");

            migrationBuilder.CreateIndex(
                name: "IX_TripOffers_Status",
                table: "TripOffers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TripOffers_TripId",
                table: "TripOffers",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripOffers_TripId_OffererId",
                table: "TripOffers",
                columns: new[] { "TripId", "OffererId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "TripOffers");
        }
    }
}
