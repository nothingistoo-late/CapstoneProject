using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Complaints",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RefundProcessed",
                table: "Complaints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RefundReason",
                table: "Complaints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "Complaints",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RefundedPaymentRecordId",
                table: "Complaints",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "RefundProcessed",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "RefundedPaymentRecordId",
                table: "Complaints");
        }
    }
}
