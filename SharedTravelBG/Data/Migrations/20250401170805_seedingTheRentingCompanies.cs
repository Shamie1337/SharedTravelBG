using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedTravelBG.Data.Migrations
{
    public partial class seedingTheRentingCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RentingCompanies",
                columns: new[] { "Id", "Address", "Name", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "16 Tsar Osvoboditel Blvd, Sofia 1000, Bulgaria", "Europcar Bulgaria", "+359 2 123 4567" },
                    { 3, "27 Vitosha Blvd, Sofia 1000, Bulgaria", "Sixt Bulgaria", "+359 2 234 5678" },
                    { 4, "5 Vasil Levski Blvd, Plovdiv 4000, Bulgaria", "Avis Bulgaria", "+359 32 123 456" },
                    { 5, "2 Maritime Blvd, Varna 9000, Bulgaria", "Budget Bulgaria", "+359 52 765 432" },
                    { 6, "10 Georgi Kirkov Blvd, Burgas 8000, Bulgaria", "OK Rent a Car", "+359 56 987 654" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RentingCompanies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RentingCompanies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RentingCompanies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RentingCompanies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RentingCompanies",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
