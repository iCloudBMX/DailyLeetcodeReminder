using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyLeetcodeReminder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Challengers",
                columns: table => new
                {
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    LeetcodeUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Heart = table.Column<short>(type: "smallint", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalSolvedProblems = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challengers", x => x.TelegramId);
                });

            migrationBuilder.CreateTable(
                name: "ChallengerWithNoAttempt",
                columns: table => new
                {
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    TotalSolvedProblems = table.Column<int>(type: "integer", nullable: false),
                    LeetcodeUserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DailyAttempts",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SolvedProblems = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAttempts", x => new { x.Date, x.UserId });
                    table.ForeignKey(
                        name: "FK_DailyAttempts_Challengers_UserId",
                        column: x => x.UserId,
                        principalTable: "Challengers",
                        principalColumn: "TelegramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challengers_LeetcodeUserName",
                table: "Challengers",
                column: "LeetcodeUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttempts_UserId",
                table: "DailyAttempts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengerWithNoAttempt");

            migrationBuilder.DropTable(
                name: "DailyAttempts");

            migrationBuilder.DropTable(
                name: "Challengers");
        }
    }
}
