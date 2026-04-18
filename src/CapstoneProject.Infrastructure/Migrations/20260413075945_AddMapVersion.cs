using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMapVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveVersion",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RootGameId",
                table: "Games",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maps_RootGameId",
                table: "Games",
                column: "RootGameId");

            migrationBuilder.CreateIndex(
                name: "IX_Maps_RootGameId_IsActiveVersion",
                table: "Games",
                columns: new[] { "RootGameId", "IsActiveVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Maps_RootGameId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Maps_RootGameId_IsActiveVersion",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsActiveVersion",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RootGameId",
                table: "Games");
        }
    }
}
