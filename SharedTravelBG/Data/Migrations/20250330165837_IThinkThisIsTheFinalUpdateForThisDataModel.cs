using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedTravelBG.Data.Migrations
{
    public partial class IThinkThisIsTheFinalUpdateForThisDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizerPhoneNumber",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartTime",
                table: "Trips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizerPhoneNumber",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PlannedStartTime",
                table: "Trips");
        }
    }
}
