using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheLeague.Modules.Members.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateMembersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "members");

            migrationBuilder.CreateTable(
                name: "CustomFieldDefinitions",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FieldType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependentMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberNotes",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MemberNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address_Line1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address_Line2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_County = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_PostCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryEmergency_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrimaryEmergency_Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrimaryEmergency_Relation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondaryEmergency_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryEmergency_Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondaryEmergency_Relation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Medical_Conditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Medical_Allergies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Medical_DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Medical_DoctorPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Medical_BloodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TwitterHandle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InstagramHandle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomFieldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketingOptIn = table.Column<bool>(type: "bit", nullable: false),
                    SmsOptIn = table.Column<bool>(type: "bit", nullable: false),
                    EmailOptIn = table.Column<bool>(type: "bit", nullable: false),
                    IsFamilyAccount = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    QRCodeData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferredByMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferralSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberStatusTransitions",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberStatusTransitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_PrimaryMemberId_DependentMemberId",
                schema: "members",
                table: "FamilyMembers",
                columns: new[] { "PrimaryMemberId", "DependentMemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClubId_Email",
                schema: "members",
                table: "Members",
                columns: new[] { "ClubId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClubId_MemberNumber",
                schema: "members",
                table: "Members",
                columns: new[] { "ClubId", "MemberNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClubId_Status",
                schema: "members",
                table: "Members",
                columns: new[] { "ClubId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberStatusTransitions_MemberId_ChangedAt",
                schema: "members",
                table: "MemberStatusTransitions",
                columns: new[] { "MemberId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFieldDefinitions",
                schema: "members");

            migrationBuilder.DropTable(
                name: "FamilyMembers",
                schema: "members");

            migrationBuilder.DropTable(
                name: "MemberNotes",
                schema: "members");

            migrationBuilder.DropTable(
                name: "Members",
                schema: "members");

            migrationBuilder.DropTable(
                name: "MemberStatusTransitions",
                schema: "members");
        }
    }
}
