using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace My_Study_App.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EventDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudyPlanId = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_StudyPlans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalTable: "StudyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LoginRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoggedInAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_StudyPlanId",
                table: "CalendarEvents",
                column: "StudyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_UserId_EventDate",
                table: "CalendarEvents",
                columns: new[] { "UserId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_UserId_LoggedInAtUtc",
                table: "LoginRecords",
                columns: new[] { "UserId", "LoggedInAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "LoginRecords");
        }
    }
}
