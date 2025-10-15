﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Ore.Application.Abstractions.Persistence.IApplicationDbContext~",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_RevokedOnUtc",
                table: "RefreshTokens",
                columns: new[] { "UserId", "RevokedOnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");
        }
    }
}
