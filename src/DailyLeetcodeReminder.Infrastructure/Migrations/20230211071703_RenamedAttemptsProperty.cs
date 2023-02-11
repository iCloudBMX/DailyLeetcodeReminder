using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyLeetcodeReminder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamedAttemptsProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Attempts",
                table: "Challengers",
                newName: "Heart");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Challengers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ChallengerWithNoAttempt",
                columns: table => new
                {
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    TotalSolvedProblems = table.Column<int>(type: "int", nullable: false),
                    LeetcodeUserName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengerWithNoAttempt");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Challengers");

            migrationBuilder.RenameColumn(
                name: "Heart",
                table: "Challengers",
                newName: "Attempts");
        }
    }
}
