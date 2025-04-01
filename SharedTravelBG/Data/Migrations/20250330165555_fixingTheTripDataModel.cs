using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedTravelBG.Data.Migrations
{
    public partial class fixingTheTripDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Trips");
        }
    }
}
