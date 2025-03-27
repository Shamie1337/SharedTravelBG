using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedTravelBG.Data.Migrations
{
    public partial class UpdateRentedVehicleFields2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentedVehicles_Vehicles_VehicleId",
                table: "RentedVehicles");

            migrationBuilder.DropIndex(
                name: "IX_RentedVehicles_VehicleId",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "RentalDate",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "RentedVehicles");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "RentedVehicles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RentalDate",
                table: "RentedVehicles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "RentedVehicles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "RentedVehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RentedVehicles_VehicleId",
                table: "RentedVehicles",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RentedVehicles_Vehicles_VehicleId",
                table: "RentedVehicles",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
