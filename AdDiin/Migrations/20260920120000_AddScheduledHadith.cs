using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdDiin.Migrations
{
    [Migration("20260920120000_AddScheduledHadith")]
    public partial class AddScheduledHadith : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledHadiths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlotTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledHadiths", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledHadiths_SlotDate_SlotTime",
                table: "ScheduledHadiths",
                columns: new[] { "SlotDate", "SlotTime" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledHadiths");
        }
    }
}
