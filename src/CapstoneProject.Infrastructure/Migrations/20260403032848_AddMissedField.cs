using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent for DBs that were partially fixed by hand or already had Add1NMapDetail applied.
            migrationBuilder.Sql(
                """
                ALTER TABLE "Hints" DROP CONSTRAINT IF EXISTS "FK_Hints_Maps_GameId";

                DROP INDEX IF EXISTS "IX_UserMapResults_UserId_GameId";
                DROP INDEX IF EXISTS "IX_MapDetails_GameId";

                ALTER TABLE "Games" DROP COLUMN IF EXISTS "TimeLimitMs";
                ALTER TABLE "Games" DROP COLUMN IF EXISTS "Type";
                ALTER TABLE "Games" DROP COLUMN IF EXISTS "WinCondition";

                DO $EF$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'Hints' AND column_name = 'GameId'
                  ) THEN
                    ALTER TABLE "Hints" RENAME COLUMN "GameId" TO "MapDetailId";
                  END IF;
                END $EF$;

                DO $EF$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE n.nspname = 'public' AND c.relname = 'IX_Hints_GameId_OrderNo'
                  ) THEN
                    ALTER INDEX "IX_Hints_GameId_OrderNo" RENAME TO "IX_Hints_MapDetailId_OrderNo";
                  END IF;
                END $EF$;

                -- After rename, column may still hold Games.Id; game to MapDetails.Id (skip rows already valid).
                UPDATE "Hints" h
                SET "MapDetailId" = sub."Id"
                FROM (
                  SELECT DISTINCT ON (md."GameId") md."Id", md."GameId"
                  FROM "MapDetails" md
                  ORDER BY md."GameId", md."Id"
                ) sub
                WHERE sub."GameId" = h."MapDetailId"
                  AND NOT EXISTS (SELECT 1 FROM "MapDetails" d WHERE d."Id" = h."MapDetailId");

                ALTER TABLE "UserMapResults" ADD COLUMN IF NOT EXISTS "MapDetailId" uuid NULL;
                ALTER TABLE "UserMapPlayHistories" ADD COLUMN IF NOT EXISTS "MapDetailId" uuid NULL;
                ALTER TABLE "Submissions" ADD COLUMN IF NOT EXISTS "MapDetailId" uuid NULL;

                ALTER TABLE "MapDetails" ADD COLUMN IF NOT EXISTS "LevelOrder" integer NOT NULL DEFAULT 0;
                ALTER TABLE "MapDetails" ADD COLUMN IF NOT EXISTS "TimeLimitMs" integer NOT NULL DEFAULT 0;
                ALTER TABLE "MapDetails" ADD COLUMN IF NOT EXISTS "Title" text NULL;
                ALTER TABLE "MapDetails" ADD COLUMN IF NOT EXISTS "Type" integer NOT NULL DEFAULT 0;
                ALTER TABLE "MapDetails" ADD COLUMN IF NOT EXISTS "WinCondition" integer NOT NULL DEFAULT 0;
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "MapMedias" (
                    "Id" uuid NOT NULL,
                    "GameId" uuid NOT NULL,
                    "Url" text NOT NULL,
                    "Kind" integer NOT NULL,
                    "SortOrder" integer NOT NULL,
                    "CreatedAt" timestamp without time zone NULL DEFAULT (now() AT TIME ZONE 'Asia/Ho_Chi_Minh'),
                    "CreatedBy" uuid NULL,
                    "UpdatedAt" timestamp without time zone NULL,
                    "UpdatedBy" uuid NULL,
                    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
                    "DeletedBy" uuid NULL,
                    "DeletedAt" timestamp without time zone NULL,
                    "Status" integer NOT NULL,
                    CONSTRAINT "PK_MapMedias" PRIMARY KEY ("Id")
                );

                DO $EF$
                BEGIN
                  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MapMedias_Maps_GameId') THEN
                    ALTER TABLE "MapMedias" ADD CONSTRAINT "FK_MapMedias_Maps_GameId" FOREIGN KEY ("GameId") REFERENCES "Games" ("Id") ON DELETE CASCADE;
                  END IF;
                END $EF$;
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_UserMapResults_MapDetailId" ON "UserMapResults" ("MapDetailId");
                CREATE INDEX IF NOT EXISTS "IX_UserMapResults_GameId" ON "UserMapResults" ("GameId");
                CREATE INDEX IF NOT EXISTS "IX_UserMapResults_UserId_MapDetailId" ON "UserMapResults" ("UserId", "MapDetailId");
                CREATE INDEX IF NOT EXISTS "IX_UserMapPlayHistories_MapDetailId" ON "UserMapPlayHistories" ("MapDetailId");
                CREATE INDEX IF NOT EXISTS "IX_Submissions_MapDetailId" ON "Submissions" ("MapDetailId");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_MapDetails_GameId_LevelOrder" ON "MapDetails" ("GameId", "LevelOrder");
                CREATE INDEX IF NOT EXISTS "IX_MapMedias_GameId_SortOrder" ON "MapMedias" ("GameId", "SortOrder");
                """);

            migrationBuilder.Sql(
                """
                DO $EF$
                BEGIN
                  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Hints_MapDetails_MapDetailId') THEN
                    ALTER TABLE "Hints" ADD CONSTRAINT "FK_Hints_MapDetails_MapDetailId" FOREIGN KEY ("MapDetailId") REFERENCES "MapDetails" ("Id") ON DELETE CASCADE;
                  END IF;
                END $EF$;

                DO $EF$
                BEGIN
                  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Submissions_MapDetails_MapDetailId') THEN
                    ALTER TABLE "Submissions" ADD CONSTRAINT "FK_Submissions_MapDetails_MapDetailId" FOREIGN KEY ("MapDetailId") REFERENCES "MapDetails" ("Id") ON DELETE RESTRICT;
                  END IF;
                END $EF$;

                DO $EF$
                BEGIN
                  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_UserMapResults_MapDetails_MapDetailId') THEN
                    ALTER TABLE "UserMapResults" ADD CONSTRAINT "FK_UserMapResults_MapDetails_MapDetailId" FOREIGN KEY ("MapDetailId") REFERENCES "MapDetails" ("Id") ON DELETE CASCADE;
                  END IF;
                END $EF$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hints_MapDetails_MapDetailId",
                table: "Hints");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_MapDetails_MapDetailId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMapResults_MapDetails_MapDetailId",
                table: "UserMapResults");

            migrationBuilder.DropTable(
                name: "MapMedias");

            migrationBuilder.DropIndex(
                name: "IX_UserMapResults_MapDetailId",
                table: "UserMapResults");

            migrationBuilder.DropIndex(
                name: "IX_UserMapResults_GameId",
                table: "UserMapResults");

            migrationBuilder.DropIndex(
                name: "IX_UserMapResults_UserId_MapDetailId",
                table: "UserMapResults");

            migrationBuilder.DropIndex(
                name: "IX_UserMapPlayHistories_MapDetailId",
                table: "UserMapPlayHistories");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_MapDetailId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_MapDetails_GameId_LevelOrder",
                table: "MapDetails");

            migrationBuilder.DropColumn(
                name: "MapDetailId",
                table: "UserMapResults");

            migrationBuilder.DropColumn(
                name: "MapDetailId",
                table: "UserMapPlayHistories");

            migrationBuilder.DropColumn(
                name: "MapDetailId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LevelOrder",
                table: "MapDetails");

            migrationBuilder.DropColumn(
                name: "TimeLimitMs",
                table: "MapDetails");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "MapDetails");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MapDetails");

            migrationBuilder.DropColumn(
                name: "WinCondition",
                table: "MapDetails");

            migrationBuilder.RenameColumn(
                name: "MapDetailId",
                table: "Hints",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Hints_MapDetailId_OrderNo",
                table: "Hints",
                newName: "IX_Hints_GameId_OrderNo");

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMs",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinCondition",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserMapResults_UserId_GameId",
                table: "UserMapResults",
                columns: new[] { "UserId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapDetails_GameId",
                table: "MapDetails",
                column: "GameId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Hints_Maps_GameId",
                table: "Hints",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
