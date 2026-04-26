using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyHintQuotaUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMapPlayHistories",
                table: "UserMapPlayHistories");

            migrationBuilder.RenameTable(
                name: "UserMapPlayHistories",
                newName: "UserGamePlayHistories");

            migrationBuilder.RenameIndex(
                name: "IX_UserMapPlayHistories_UserId_GameId_StartTime",
                table: "UserGamePlayHistories",
                newName: "IX_UserGamePlayHistories_UserId_GameId_StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_UserMapPlayHistories_UserId",
                table: "UserGamePlayHistories",
                newName: "IX_UserGamePlayHistories_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMapPlayHistories_SubmissionId",
                table: "UserGamePlayHistories",
                newName: "IX_UserGamePlayHistories_SubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMapPlayHistories_GameId",
                table: "UserGamePlayHistories",
                newName: "IX_UserGamePlayHistories_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMapPlayHistories_GameDetailId",
                table: "UserGamePlayHistories",
                newName: "IX_UserGamePlayHistories_GameDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMapPlayHistories_ExecutionsResultId",
                table: "UserGamePlayHistories",
                newName: "IX_UserGamePlayHistories_ExecutionsResultId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGamePlayHistories",
                table: "UserGamePlayHistories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserMonthlyHintUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthKey = table.Column<int>(type: "integer", nullable: false),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_UserMonthlyHintUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMonthlyHintUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMonthlyHintUsages_MonthKey",
                table: "UserMonthlyHintUsages",
                column: "MonthKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserMonthlyHintUsages_UserId_MonthKey",
                table: "UserMonthlyHintUsages",
                columns: new[] { "UserId", "MonthKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMonthlyHintUsages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGamePlayHistories",
                table: "UserGamePlayHistories");

            migrationBuilder.RenameTable(
                name: "UserGamePlayHistories",
                newName: "UserMapPlayHistories");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePlayHistories_UserId_GameId_StartTime",
                table: "UserMapPlayHistories",
                newName: "IX_UserMapPlayHistories_UserId_GameId_StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePlayHistories_UserId",
                table: "UserMapPlayHistories",
                newName: "IX_UserMapPlayHistories_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePlayHistories_SubmissionId",
                table: "UserMapPlayHistories",
                newName: "IX_UserMapPlayHistories_SubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePlayHistories_GameId",
                table: "UserMapPlayHistories",
                newName: "IX_UserMapPlayHistories_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePlayHistories_GameDetailId",
                table: "UserMapPlayHistories",
                newName: "IX_UserMapPlayHistories_GameDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePlayHistories_ExecutionsResultId",
                table: "UserMapPlayHistories",
                newName: "IX_UserMapPlayHistories_ExecutionsResultId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMapPlayHistories",
                table: "UserMapPlayHistories",
                column: "Id");
        }
    }
}
