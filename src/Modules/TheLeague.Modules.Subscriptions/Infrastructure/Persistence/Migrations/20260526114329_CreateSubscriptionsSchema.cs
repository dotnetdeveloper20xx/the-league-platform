using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheLeague.Modules.Subscriptions.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateSubscriptionsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "subscriptions");

            migrationBuilder.CreateTable(
                name: "AddOns",
                schema: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubAddOns",
                schema: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddOnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubAddOns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubSubscriptions",
                schema: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentTier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledDowngradeTier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledDowngradeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillingPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTrialActive = table.Column<bool>(type: "bit", nullable: false),
                    TrialStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedPaymentAttempts = table.Column<int>(type: "int", nullable: false),
                    LastPaymentFailureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TierConfigs",
                schema: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    MaxStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    MonthlySmsCredits = table.Column<int>(type: "int", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TierConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageRecords",
                schema: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentMemberCount = table.Column<int>(type: "int", nullable: false),
                    CurrentStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    CurrentMonthlySmsUsed = table.Column<int>(type: "int", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubAddOns_ClubId",
                schema: "subscriptions",
                table: "ClubAddOns",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubAddOns_ClubId_AddOnId",
                schema: "subscriptions",
                table: "ClubAddOns",
                columns: new[] { "ClubId", "AddOnId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubSubscriptions_ClubId",
                schema: "subscriptions",
                table: "ClubSubscriptions",
                column: "ClubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TierConfigs_Tier",
                schema: "subscriptions",
                table: "TierConfigs",
                column: "Tier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_ClubId",
                schema: "subscriptions",
                table: "UsageRecords",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_ClubId_PeriodStart_PeriodEnd",
                schema: "subscriptions",
                table: "UsageRecords",
                columns: new[] { "ClubId", "PeriodStart", "PeriodEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddOns",
                schema: "subscriptions");

            migrationBuilder.DropTable(
                name: "ClubAddOns",
                schema: "subscriptions");

            migrationBuilder.DropTable(
                name: "ClubSubscriptions",
                schema: "subscriptions");

            migrationBuilder.DropTable(
                name: "TierConfigs",
                schema: "subscriptions");

            migrationBuilder.DropTable(
                name: "UsageRecords",
                schema: "subscriptions");
        }
    }
}
