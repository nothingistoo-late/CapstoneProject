using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <summary>
    /// Adds <c>UserMonthlyHintUsages</c> and aligns <c>UserGamePlayHistories</c> PK/index names with the model.
    /// Idempotent: safe when migration <c>20260417130000_RenameLegacyMapSchemaToGame</c> already renamed the play-history table.
    /// </summary>
    public partial class AddMonthlyHintQuotaUsage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'UserMapPlayHistories')
                     AND NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'UserGamePlayHistories') THEN
                    ALTER TABLE "UserMapPlayHistories" RENAME TO "UserGamePlayHistories";
                  END IF;
                END $$;

                DO $$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM pg_constraint c
                    JOIN pg_class t ON c.conrelid = t.oid
                    JOIN pg_namespace n ON t.relnamespace = n.oid
                    WHERE n.nspname = 'public' AND t.relname = 'UserGamePlayHistories' AND c.conname = 'PK_UserMapPlayHistories'
                  ) THEN
                    ALTER TABLE "UserGamePlayHistories" RENAME CONSTRAINT "PK_UserMapPlayHistories" TO "PK_UserGamePlayHistories";
                  END IF;
                END $$;

                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_UserId_GameId_StartTime')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_UserId_GameId_StartTime') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_UserId_GameId_StartTime" RENAME TO "IX_UserGamePlayHistories_UserId_GameId_StartTime"';
                  END IF;

                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_UserId')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_UserId') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_UserId" RENAME TO "IX_UserGamePlayHistories_UserId"';
                  END IF;

                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_SubmissionId')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_SubmissionId') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_SubmissionId" RENAME TO "IX_UserGamePlayHistories_SubmissionId"';
                  END IF;

                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_GameId')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_GameId') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_GameId" RENAME TO "IX_UserGamePlayHistories_GameId"';
                  END IF;

                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_GameDetailId')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_GameDetailId') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_GameDetailId" RENAME TO "IX_UserGamePlayHistories_GameDetailId"';
                  END IF;

                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_MapDetailId')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_GameDetailId') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_MapDetailId" RENAME TO "IX_UserGamePlayHistories_GameDetailId"';
                  END IF;

                  IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserMapPlayHistories_ExecutionsResultId')
                     AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                     WHERE n.nspname = 'public' AND c.relkind IN ('i', 'I') AND c.relname = 'IX_UserGamePlayHistories_ExecutionsResultId') THEN
                    EXECUTE 'ALTER INDEX "IX_UserMapPlayHistories_ExecutionsResultId" RENAME TO "IX_UserGamePlayHistories_ExecutionsResultId"';
                  END IF;
                END $$;

                DO $$
                BEGIN
                  IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'UserMonthlyHintUsages') THEN
                    CREATE TABLE "UserMonthlyHintUsages" (
                        "Id" uuid NOT NULL,
                        "UserId" uuid NOT NULL,
                        "MonthKey" integer NOT NULL,
                        "UsedCount" integer NOT NULL,
                        "CreatedAt" timestamp without time zone NULL DEFAULT (now() AT TIME ZONE 'Asia/Ho_Chi_Minh'),
                        "CreatedBy" uuid NULL,
                        "UpdatedAt" timestamp without time zone NULL,
                        "UpdatedBy" uuid NULL,
                        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
                        "DeletedBy" uuid NULL,
                        "DeletedAt" timestamp without time zone NULL,
                        "Status" integer NOT NULL,
                        CONSTRAINT "PK_UserMonthlyHintUsages" PRIMARY KEY ("Id"),
                        CONSTRAINT "FK_UserMonthlyHintUsages_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                    );
                    CREATE INDEX "IX_UserMonthlyHintUsages_MonthKey" ON "UserMonthlyHintUsages" ("MonthKey");
                    CREATE UNIQUE INDEX "IX_UserMonthlyHintUsages_UserId_MonthKey" ON "UserMonthlyHintUsages" ("UserId", "MonthKey");
                  END IF;
                END $$;
                """);
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
