using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClipRateRecorder.Migrations
{
    /// <inheritdoc />
    public partial class AddWindowActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WindowActivities",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    ExePath = table.Column<string>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationSeconds = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindowActivities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WindowActivities_ExePath",
                table: "WindowActivities",
                column: "ExePath");

            migrationBuilder.CreateIndex(
                name: "IX_WindowActivities_StartTime",
                table: "WindowActivities",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_WindowActivities_Title",
                table: "WindowActivities",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WindowActivities");
        }
    }
}
