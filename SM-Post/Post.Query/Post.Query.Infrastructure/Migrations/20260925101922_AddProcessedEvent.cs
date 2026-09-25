using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Query.Infrastructure.Migrations
{
    public partial class AddProcessedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedEvent_AggregateId_Version",
                table: "ProcessedEvent",
                columns: new[] { "AggregateId", "Version" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedEvent");
        }
    }
}
