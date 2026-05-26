using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheLeague.Modules.Analytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateAnalyticsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "ChurnPredictions",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredictionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAtRisk = table.Column<bool>(type: "bit", nullable: false),
                    RiskFactors = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttendanceDropPercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MissedPaymentCount = table.Column<int>(type: "int", nullable: true),
                    LoginDropPercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChurnPredictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberEngagements",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<DateOnly>(type: "date", nullable: false),
                    SessionsAttended = table.Column<int>(type: "int", nullable: false),
                    EventsAttended = table.Column<int>(type: "int", nullable: false),
                    PaymentTimelinessDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PortalLogins = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberEngagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Snapshots",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MemberGrowthRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentCollectionRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SessionAttendanceRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EventParticipationRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HealthScore = table.Column<int>(type: "int", nullable: false),
                    ActiveMemberCount = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    TotalEvents = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChurnPredictions_ClubId_IsAtRisk",
                schema: "analytics",
                table: "ChurnPredictions",
                columns: new[] { "ClubId", "IsAtRisk" });

            migrationBuilder.CreateIndex(
                name: "IX_ChurnPredictions_ClubId_MemberId_PredictionDate",
                schema: "analytics",
                table: "ChurnPredictions",
                columns: new[] { "ClubId", "MemberId", "PredictionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberEngagements_ClubId_Month",
                schema: "analytics",
                table: "MemberEngagements",
                columns: new[] { "ClubId", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberEngagements_MemberId_Month",
                schema: "analytics",
                table: "MemberEngagements",
                columns: new[] { "MemberId", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_ClubId_SnapshotDate",
                schema: "analytics",
                table: "Snapshots",
                columns: new[] { "ClubId", "SnapshotDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChurnPredictions",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "MemberEngagements",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "Snapshots",
                schema: "analytics");
        }
    }
}
