using CapstoneProject.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    [DbContext(typeof(CapstoneProjectDbContext))]
    [Migration("20260417134000_RenamePaymentRecordMapIdToGameId")]
    public partial class RenamePaymentRecordMapIdToGameId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                  IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'PaymentRecords'
                      AND column_name = 'MapId'
                  ) THEN
                    ALTER TABLE "PaymentRecords" RENAME COLUMN "MapId" TO "GameId";
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
                  IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'PaymentRecords'
                      AND column_name = 'GameId'
                  ) THEN
                    ALTER TABLE "PaymentRecords" RENAME COLUMN "GameId" TO "MapId";
                  END IF;
                END $$;
                """
            );
        }
    }
}
