using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carbank.Migrations
{
    /// <inheritdoc />
    public partial class upload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "CarAds",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Images",
                table: "CarAds");
        }
    }
}
