using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CapstoneProject.Infrastructure.Context;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <summary>
    /// Rename legacy Map* schema objects to Game* for existing databases.
    /// Safe to run on databases that are already using Game* names.
    /// </summary>
    [DbContext(typeof(CapstoneProjectDbContext))]
    [Migration("20260417130000_RenameLegacyMapSchemaToGame")]
    public partial class RenameLegacyMapSchemaToGame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='Maps') THEN
                    ALTER TABLE "Maps" RENAME TO "Games";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MapDetails') THEN
                    ALTER TABLE "MapDetails" RENAME TO "GameDetails";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MapMedias') THEN
                    ALTER TABLE "MapMedias" RENAME TO "GameMedias";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MapTags') THEN
                    ALTER TABLE "MapTags" RENAME TO "GameTags";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MapRatings') THEN
                    ALTER TABLE "MapRatings" RENAME TO "GameRatings";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MapReports') THEN
                    ALTER TABLE "MapReports" RENAME TO "GameReports";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MapSolveScoreConfigs') THEN
                    ALTER TABLE "MapSolveScoreConfigs" RENAME TO "GameSolveScoreConfigs";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='MyMaps') THEN
                    ALTER TABLE "MyMaps" RENAME TO "MyGames";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='UserMapResults') THEN
                    ALTER TABLE "UserMapResults" RENAME TO "UserGameResults";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='UserMapPlayHistories') THEN
                    ALTER TABLE "UserMapPlayHistories" RENAME TO "UserGamePlayHistories";
                  END IF;
                END $$;
                """
            );

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Games' AND column_name='MapStatus') THEN
                    ALTER TABLE "Games" RENAME COLUMN "MapStatus" TO "GameStatus";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Games' AND column_name='RootMapId') THEN
                    ALTER TABLE "Games" RENAME COLUMN "RootMapId" TO "RootGameId";
                  END IF;

                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='GameDetails' AND column_name='MapId') THEN
                    ALTER TABLE "GameDetails" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='GameMedias' AND column_name='MapId') THEN
                    ALTER TABLE "GameMedias" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='GameTags' AND column_name='MapId') THEN
                    ALTER TABLE "GameTags" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='GameRatings' AND column_name='MapId') THEN
                    ALTER TABLE "GameRatings" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='GameReports' AND column_name='MapId') THEN
                    ALTER TABLE "GameReports" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='MyGames' AND column_name='MapId') THEN
                    ALTER TABLE "MyGames" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='UserGameResults' AND column_name='MapId') THEN
                    ALTER TABLE "UserGameResults" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='UserGameResults' AND column_name='MapDetailId') THEN
                    ALTER TABLE "UserGameResults" RENAME COLUMN "MapDetailId" TO "GameDetailId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='UserGamePlayHistories' AND column_name='MapId') THEN
                    ALTER TABLE "UserGamePlayHistories" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='UserGamePlayHistories' AND column_name='MapDetailId') THEN
                    ALTER TABLE "UserGamePlayHistories" RENAME COLUMN "MapDetailId" TO "GameDetailId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Submissions' AND column_name='MapId') THEN
                    ALTER TABLE "Submissions" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Submissions' AND column_name='MapDetailId') THEN
                    ALTER TABLE "Submissions" RENAME COLUMN "MapDetailId" TO "GameDetailId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='LearningPathItems' AND column_name='MapId') THEN
                    ALTER TABLE "LearningPathItems" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='XpTransactions' AND column_name='MapId') THEN
                    ALTER TABLE "XpTransactions" RENAME COLUMN "MapId" TO "GameId";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Hints' AND column_name='MapDetailId') THEN
                    ALTER TABLE "Hints" RENAME COLUMN "MapDetailId" TO "GameDetailId";
                  END IF;
                END $$;
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Games' AND column_name='GameStatus') THEN
                    ALTER TABLE "Games" RENAME COLUMN "GameStatus" TO "MapStatus";
                  END IF;
                  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='Games' AND column_name='RootGameId') THEN
                    ALTER TABLE "Games" RENAME COLUMN "RootGameId" TO "RootMapId";
                  END IF;
                END $$;
                """
            );
        }
    }
}
