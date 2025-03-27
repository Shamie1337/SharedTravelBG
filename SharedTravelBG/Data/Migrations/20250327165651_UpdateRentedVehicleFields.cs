using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedTravelBG.Data.Migrations
{
    public partial class UpdateRentedVehicleFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "RentedVehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RentedVehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlateNumber",
                table: "RentedVehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "RentedVehicles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RenterPhoneNumber",
                table: "RentedVehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "RentedVehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "LicensePlateNumber",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "RenterPhoneNumber",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "RentedVehicles");
        }
    }
}
