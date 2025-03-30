using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedTravelBG.Data.Migrations
{
    public partial class UpdatingTheReviewModelAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Trips] WHERE [DepartureTown] = 'Default' AND [DestinationTown] = 'Default')
                 BEGIN
                    INSERT INTO [Trips] ([DepartureTown], [DestinationTown], [TripDate], [OrganizerId])
                        VALUES ('Default', 'Default', GETDATE(), '4edf4ab5-9de8-47b9-bab0-0f0bb79f7e40')
                            END
                                ");

			migrationBuilder.Sql(@"
    DECLARE @DefaultTripId int = (SELECT TOP 1 [Id] FROM [Trips] WHERE [DepartureTown] = 'Default' AND [DestinationTown] = 'Default');
    UPDATE [Reviews] SET [TripId] = @DefaultTripId WHERE [TripId] IS NULL;
");



			migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "Reviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");
        }
    }
}
