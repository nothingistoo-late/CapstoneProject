using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMapSolveScoreConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MapSolveScoreConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BaseScore = table.Column<int>(type: "integer", nullable: false),
                    TimeScore = table.Column<int>(type: "integer", nullable: false),
                    StepsScore = table.Column<int>(type: "integer", nullable: false),
                    BlocksScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() AT TIME ZONE 'Asia/Ho_Chi_Minh')"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapSolveScoreConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapSolveScoreConfigs_ConfigKey",
                table: "MapSolveScoreConfigs",
                column: "ConfigKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapSolveScoreConfigs");
        }
    }
}
