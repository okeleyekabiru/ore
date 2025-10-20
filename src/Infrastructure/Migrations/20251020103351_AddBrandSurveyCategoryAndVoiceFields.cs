using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandSurveyCategoryAndVoiceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandVoice_Competitors",
                table: "Teams",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandVoice_Goals",
                table: "Teams",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BrandSurveys",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandVoice_Competitors",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "BrandVoice_Goals",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BrandSurveys");
        }
    }
}
