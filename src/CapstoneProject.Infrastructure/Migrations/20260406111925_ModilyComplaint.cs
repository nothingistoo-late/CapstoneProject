using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModilyComplaint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryKey",
                table: "Complaints",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContextDataJson",
                table: "Complaints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContextId",
                table: "Complaints",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextKey",
                table: "Complaints",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextType",
                table: "Complaints",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAt",
                table: "Complaints",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComplaintCategoryCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintCategoryCatalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintPolicyRuleConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_ComplaintPolicyRuleConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CategoryKey",
                table: "Complaints",
                column: "CategoryKey");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ContextKey",
                table: "Complaints",
                column: "ContextKey");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_UserId_CategoryKey_ContextKey_ComplaintStatus",
                table: "Complaints",
                columns: new[] { "UserId", "CategoryKey", "ContextKey", "ComplaintStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintCategoryCatalogs_CategoryKey",
                table: "ComplaintCategoryCatalogs",
                column: "CategoryKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintCategoryCatalogs_IsEnabled_SortOrder",
                table: "ComplaintCategoryCatalogs",
                columns: new[] { "IsEnabled", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPolicyRuleConfigs_CategoryKey_RuleKey",
                table: "ComplaintPolicyRuleConfigs",
                columns: new[] { "CategoryKey", "RuleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPolicyRuleConfigs_IsEnabled_Priority",
                table: "ComplaintPolicyRuleConfigs",
                columns: new[] { "IsEnabled", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintCategoryCatalogs");

            migrationBuilder.DropTable(
                name: "ComplaintPolicyRuleConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CategoryKey",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ContextKey",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_UserId_CategoryKey_ContextKey_ComplaintStatus",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CategoryKey",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContextDataJson",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContextId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContextKey",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContextType",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                table: "Complaints");
        }
    }
}
