using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameReviewCriterionCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameReviewCriterionCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SectionTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_GameReviewCriterionCatalogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameReviewCriterionCatalogs_CriterionKey",
                table: "GameReviewCriterionCatalogs",
                column: "CriterionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameReviewCriterionCatalogs_SectionKey_SortOrder",
                table: "GameReviewCriterionCatalogs",
                columns: new[] { "SectionKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_GameReviewCriterionCatalogs_SortOrder",
                table: "GameReviewCriterionCatalogs",
                column: "SortOrder");

            migrationBuilder.Sql(
                """
                INSERT INTO "GameReviewCriterionCatalogs" ("Id", "CriterionKey", "SectionKey", "SectionTitle", "Label", "SortOrder", "IsEnabled", "IsDeleted", "Status")
                VALUES
                ('f0000001-0000-4000-8000-000000000001', 'validity-start-goal', 'validity', '1. Validity', 'Map has both Start and Goal points', 0, true, false, 1),
                ('f0000001-0000-4000-8000-000000000002', 'validity-solvable', 'validity', '1. Validity', 'Map is solvable', 1, true, false, 1),
                ('f0000001-0000-4000-8000-000000000003', 'validity-soft-lock', 'validity', '1. Validity', 'No soft lock', 2, true, false, 1),
                ('f0000001-0000-4000-8000-000000000004', 'validity-no-wrong-path', 'validity', '1. Validity', 'No invalid path / unreachable goal path', 3, true, false, 1),
                ('f0000001-0000-4000-8000-000000000005', 'validity-objects', 'validity', '1. Validity', 'Objects (door, key, switch...) work correctly', 4, true, false, 1),
                ('f0000001-0000-4000-8000-000000000006', 'difficulty-match-level', 'difficulty', '2. Difficulty', 'Difficulty matches selected level', 5, true, false, 1),
                ('f0000001-0000-4000-8000-000000000007', 'difficulty-not-too-easy', 'difficulty', '2. Difficulty', 'Not too easy (cannot be solved instantly)', 6, true, false, 1),
                ('f0000001-0000-4000-8000-000000000008', 'difficulty-not-too-hard', 'difficulty', '2. Difficulty', 'Not too hard (does not require too many/complex steps)', 7, true, false, 1),
                ('f0000001-0000-4000-8000-000000000009', 'difficulty-step-count', 'difficulty', '2. Difficulty', 'Reasonable number of solution steps', 8, true, false, 1),
                ('f0000001-0000-4000-8000-00000000000a', 'difficulty-time', 'difficulty', '2. Difficulty', 'Reasonable completion time', 9, true, false, 1),
                ('f0000001-0000-4000-8000-00000000000b', 'fairness-no-unfair-traps', 'fairness', '3. Fairness', 'No unpredictable traps', 10, true, false, 1),
                ('f0000001-0000-4000-8000-00000000000c', 'fairness-no-unreasonable-loss', 'fairness', '3. Fairness', 'No unreasonable losing condition', 11, true, false, 1),
                ('f0000001-0000-4000-8000-00000000000d', 'fairness-clear-mechanics', 'fairness', '3. Fairness', 'Mechanics are clear (door/key/hazard are understandable)', 12, true, false, 1),
                ('f0000001-0000-4000-8000-00000000000e', 'technical-no-bug', 'technical-quality', '4. Technical Quality', 'No bugs (collision, trigger...)', 13, true, false, 1),
                ('f0000001-0000-4000-8000-00000000000f', 'technical-object-correct', 'technical-quality', '4. Technical Quality', 'Objects function correctly', 14, true, false, 1),
                ('f0000001-0000-4000-8000-000000000010', 'visual-clear-layout', 'visual-ux', '5. Visual & UX', 'Layout is clear and easy to read', 15, true, false, 1),
                ('f0000001-0000-4000-8000-000000000011', 'visual-no-spam', 'visual-ux', '5. Visual & UX', 'No clutter / object spam', 16, true, false, 1),
                ('f0000001-0000-4000-8000-000000000012', 'visual-important-elements', 'visual-ux', '5. Visual & UX', 'Important elements are easy to identify', 17, true, false, 1),
                ('f0000001-0000-4000-8000-000000000013', 'visual-asset-usage', 'visual-ux', '5. Visual & UX', 'Assets are used appropriately', 18, true, false, 1),
                ('f0000001-0000-4000-8000-000000000014', 'safety-map-name', 'content-safety', '6. Content Safety', 'Game name is appropriate', 19, true, false, 1),
                ('f0000001-0000-4000-8000-000000000015', 'safety-description', 'content-safety', '6. Content Safety', 'Description does not violate policies', 20, true, false, 1),
                ('f0000001-0000-4000-8000-000000000016', 'safety-sensitive', 'content-safety', '6. Content Safety', 'No sensitive/inappropriate content', 21, true, false, 1),
                ('f0000001-0000-4000-8000-000000000017', 'metadata-has-name', 'metadata', '7. Metadata', 'Has game name', 22, true, false, 1),
                ('f0000001-0000-4000-8000-000000000018', 'metadata-has-description', 'metadata', '7. Metadata', 'Has description', 23, true, false, 1),
                ('f0000001-0000-4000-8000-000000000019', 'metadata-has-tags', 'metadata', '7. Metadata', 'Has suitable tags', 24, true, false, 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameReviewCriterionCatalogs");
        }
    }
}
