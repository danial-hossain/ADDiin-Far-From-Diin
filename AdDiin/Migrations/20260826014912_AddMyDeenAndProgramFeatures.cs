using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdDiin.Migrations
{
    /// <inheritdoc />
    public partial class AddMyDeenAndProgramFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructor",
                table: "Activities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Activities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organizer",
                table: "Activities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProgramDate",
                table: "Activities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdhkarLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdhkarType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdhkarLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdhkarLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyDeenGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fajr = table.Column<bool>(type: "bit", nullable: false),
                    Dhuhr = table.Column<bool>(type: "bit", nullable: false),
                    Asr = table.Column<bool>(type: "bit", nullable: false),
                    Maghrib = table.Column<bool>(type: "bit", nullable: false),
                    Isha = table.Column<bool>(type: "bit", nullable: false),
                    QuranRead = table.Column<bool>(type: "bit", nullable: false),
                    MorningAdhkar = table.Column<bool>(type: "bit", nullable: false),
                    EveningAdhkar = table.Column<bool>(type: "bit", nullable: false),
                    DhikrTarget = table.Column<bool>(type: "bit", nullable: false),
                    RuqyahRoutine = table.Column<bool>(type: "bit", nullable: false),
                    CharityGiven = table.Column<bool>(type: "bit", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyDeenGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyDeenGoals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DhikrRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DhikrName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    TargetCount = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTargetAchieved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DhikrRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DhikrRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AdminRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramRegistrations_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramRegistrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QuranReadingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GoalType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DailyTarget = table.Column<int>(type: "int", nullable: false),
                    CurrentSurah = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentAyah = table.Column<int>(type: "int", nullable: false),
                    PagesReadToday = table.Column<int>(type: "int", nullable: false),
                    VersesReadToday = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuranReadingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuranReadingLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuqyahLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoutineType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuqyahLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuqyahLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDeenSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DailyDhikrTarget = table.Column<int>(type: "int", nullable: false),
                    DailyQuranPagesTarget = table.Column<int>(type: "int", nullable: false),
                    MonthlyDonationGoal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrayerReminder = table.Column<bool>(type: "bit", nullable: false),
                    QuranReminder = table.Column<bool>(type: "bit", nullable: false),
                    DhikrReminder = table.Column<bool>(type: "bit", nullable: false),
                    AdhkarReminder = table.Column<bool>(type: "bit", nullable: false),
                    RuqyahReminder = table.Column<bool>(type: "bit", nullable: false),
                    ProgramReminder = table.Column<bool>(type: "bit", nullable: false),
                    CalendarReminder = table.Column<bool>(type: "bit", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeenSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDeenSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdhkarLogs_UserId",
                table: "AdhkarLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyDeenGoals_UserId",
                table: "DailyDeenGoals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DhikrRecords_UserId",
                table: "DhikrRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramRegistrations_ActivityId",
                table: "ProgramRegistrations",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramRegistrations_Status",
                table: "ProgramRegistrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramRegistrations_UserId",
                table: "ProgramRegistrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuranReadingLogs_UserId",
                table: "QuranReadingLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RuqyahLogs_UserId",
                table: "RuqyahLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeenSettings_UserId",
                table: "UserDeenSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdhkarLogs");

            migrationBuilder.DropTable(
                name: "DailyDeenGoals");

            migrationBuilder.DropTable(
                name: "DhikrRecords");

            migrationBuilder.DropTable(
                name: "ProgramRegistrations");

            migrationBuilder.DropTable(
                name: "QuranReadingLogs");

            migrationBuilder.DropTable(
                name: "RuqyahLogs");

            migrationBuilder.DropTable(
                name: "UserDeenSettings");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Instructor",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Organizer",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ProgramDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Activities");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
