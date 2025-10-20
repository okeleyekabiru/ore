using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandContentItemLengths2990 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ContentItems",
                type: "character varying(2990)",
                maxLength: 2990,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "ContentItems",
                type: "character varying(2990)",
                maxLength: 2990,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ContentItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2990)",
                oldMaxLength: 2990);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "ContentItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2990)",
                oldMaxLength: 2990,
                oldNullable: true);
        }
    }
}
