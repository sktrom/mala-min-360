using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalaMin.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    TourViews = table.Column<int>(type: "integer", nullable: false),
                    WhatsAppClicks = table.Column<int>(type: "integer", nullable: false),
                    QrScans = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyStats_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyStats_PropertyId",
                table: "PropertyStats",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyStats_PropertyId_StatDate",
                table: "PropertyStats",
                columns: new[] { "PropertyId", "StatDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyStats_StatDate",
                table: "PropertyStats",
                column: "StatDate");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyStats_TenantId",
                table: "PropertyStats",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyStats");
        }
    }
}
