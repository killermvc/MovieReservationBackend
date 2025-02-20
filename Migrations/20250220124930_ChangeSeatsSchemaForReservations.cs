using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReservation.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSeatsSchemaForReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showtime_Seats_SeatId",
                table: "Showtime");

            migrationBuilder.DropIndex(
                name: "IX_Showtime_SeatId",
                table: "Showtime");

            migrationBuilder.DropColumn(
                name: "SeatId",
                table: "Showtime");

            migrationBuilder.AddColumn<int>(
                name: "ShowtimeId",
                table: "Seats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Reservations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_ShowtimeId",
                table: "Seats",
                column: "ShowtimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Showtime_ShowtimeId",
                table: "Seats",
                column: "ShowtimeId",
                principalTable: "Showtime",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Showtime_ShowtimeId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Seats_ShowtimeId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "ShowtimeId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "SeatId",
                table: "Showtime",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Showtime_SeatId",
                table: "Showtime",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtime_Seats_SeatId",
                table: "Showtime",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id");
        }
    }
}
