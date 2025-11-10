using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Youdovezu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationFieldsToDriverDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverFirstName",
                table: "DriverDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLastName",
                table: "DriverDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverMiddleName",
                table: "DriverDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ModeratedByUserId",
                table: "DriverDocuments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleBrand",
                table: "DriverDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleColor",
                table: "DriverDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleLicensePlate",
                table: "DriverDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "DriverDocuments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverFirstName",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "DriverLastName",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "DriverMiddleName",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "ModeratedByUserId",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "VehicleBrand",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "VehicleColor",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "VehicleLicensePlate",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "DriverDocuments");
        }
    }
}
