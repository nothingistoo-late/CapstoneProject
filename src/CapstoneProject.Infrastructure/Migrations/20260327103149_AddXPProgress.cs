using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddXPProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "XpTransactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "XpTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "XpTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "XpTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentLevel",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CurrentXp",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows before creating unique index.
            // Existing transactions would otherwise all have default empty string and violate uniqueness.
            migrationBuilder.Sql(@"
UPDATE ""XpTransactions""
SET ""IdempotencyKey"" = 'legacy:' || ""Id""::text
WHERE ""IdempotencyKey"" IS NULL OR ""IdempotencyKey"" = '';
");

            migrationBuilder.CreateTable(
                name: "LevelThresholds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    RequiredTotalXp = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LevelThresholds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XpPolicyConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: true),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ActiveTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_XpPolicyConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XpSourceConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BaseXp = table.Column<int>(type: "integer", nullable: false),
                    DailyCap = table.Column<int>(type: "integer", nullable: false),
                    BonusMultiplier = table.Column<double>(type: "double precision", nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_XpSourceConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_IdempotencyKey",
                table: "XpTransactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_SourceType",
                table: "XpTransactions",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentLevel",
                table: "Users",
                column: "CurrentLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentXp",
                table: "Users",
                column: "CurrentXp");

            migrationBuilder.CreateIndex(
                name: "IX_LevelThresholds_Level",
                table: "LevelThresholds",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevelThresholds_RequiredTotalXp",
                table: "LevelThresholds",
                column: "RequiredTotalXp");

            migrationBuilder.CreateIndex(
                name: "IX_XpPolicyConfigs_IsEnabled_Priority",
                table: "XpPolicyConfigs",
                columns: new[] { "IsEnabled", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_XpPolicyConfigs_PolicyKey",
                table: "XpPolicyConfigs",
                column: "PolicyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XpSourceConfigs_IsEnabled",
                table: "XpSourceConfigs",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_XpSourceConfigs_SourceType",
                table: "XpSourceConfigs",
                column: "SourceType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevelThresholds");

            migrationBuilder.DropTable(
                name: "XpPolicyConfigs");

            migrationBuilder.DropTable(
                name: "XpSourceConfigs");

            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_IdempotencyKey",
                table: "XpTransactions");

            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_SourceType",
                table: "XpTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentLevel",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentXp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "XpTransactions");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "XpTransactions");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "XpTransactions");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "XpTransactions");

            migrationBuilder.DropColumn(
                name: "CurrentLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentXp",
                table: "Users");
        }
    }
}
