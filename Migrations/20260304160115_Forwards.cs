using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carbank.Migrations
{
    /// <inheritdoc />
    public partial class Forwards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TelegramLink = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShowroomName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarAds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MileageKm = table.Column<int>(type: "int", nullable: false),
                    InsuranceMonths = table.Column<int>(type: "int", nullable: true),
                    Gearbox = table.Column<int>(type: "int", nullable: false),
                    ChassisNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HasFlash = table.Column<bool>(type: "bit", nullable: false),
                    FlashEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByAdminId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarAds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarAds_Users_ApprovedByAdminId",
                        column: x => x.ApprovedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarAds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBioItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsAdvanced = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBioItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBioItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdId = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdViews_CarAds_AdId",
                        column: x => x.AdId,
                        principalTable: "CarAds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdViews_AdId",
                table: "AdViews",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_AdViews_ViewedAt",
                table: "AdViews",
                column: "ViewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CarAds_ApprovedByAdminId",
                table: "CarAds",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CarAds_UserId",
                table: "CarAds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBioItems_UserId",
                table: "UserBioItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBioItems_UserId_GroupKey",
                table: "UserBioItems",
                columns: new[] { "UserId", "GroupKey" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteDescriptions_Id",
                table: "WebsiteDescriptions",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdViews");

            migrationBuilder.DropTable(
                name: "FlashSettings");

            migrationBuilder.DropTable(
                name: "TelegramMessages");

            migrationBuilder.DropTable(
                name: "UserBioItems");

            migrationBuilder.DropTable(
                name: "WebsiteDescriptions");

            migrationBuilder.DropTable(
                name: "CarAds");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
