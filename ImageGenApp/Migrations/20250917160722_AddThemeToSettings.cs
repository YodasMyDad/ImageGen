using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageGenApp.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Settings");
        }
    }
}
