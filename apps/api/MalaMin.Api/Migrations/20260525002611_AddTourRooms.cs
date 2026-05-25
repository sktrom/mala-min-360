using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalaMin.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTourRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PanoramaMediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsStartRoom = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourRooms_MediaFiles_PanoramaMediaId",
                        column: x => x.PanoramaMediaId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourRooms_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourRooms_PanoramaMediaId",
                table: "TourRooms",
                column: "PanoramaMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_TourRooms_PropertyId",
                table: "TourRooms",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_TourRooms_PropertyId_IsStartRoom",
                table: "TourRooms",
                columns: new[] { "PropertyId", "IsStartRoom" });

            migrationBuilder.CreateIndex(
                name: "IX_TourRooms_PropertyId_SortOrder",
                table: "TourRooms",
                columns: new[] { "PropertyId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TourRooms_TenantId",
                table: "TourRooms",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourRooms");
        }
    }
}
