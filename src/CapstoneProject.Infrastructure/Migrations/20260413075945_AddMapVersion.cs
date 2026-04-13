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
                table: "Maps",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RootMapId",
                table: "Maps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maps_RootMapId",
                table: "Maps",
                column: "RootMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Maps_RootMapId_IsActiveVersion",
                table: "Maps",
                columns: new[] { "RootMapId", "IsActiveVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Maps_RootMapId",
                table: "Maps");

            migrationBuilder.DropIndex(
                name: "IX_Maps_RootMapId_IsActiveVersion",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "IsActiveVersion",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "RootMapId",
                table: "Maps");
        }
    }
}
