using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalaMin.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTourHotspots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourHotspots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Yaw = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                    Pitch = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourHotspots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourHotspots_TourRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "TourRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourHotspots_TourRooms_TargetRoomId",
                        column: x => x.TargetRoomId,
                        principalTable: "TourRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourHotspots_RoomId",
                table: "TourHotspots",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TourHotspots_TargetRoomId",
                table: "TourHotspots",
                column: "TargetRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TourHotspots_TenantId",
                table: "TourHotspots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TourHotspots_Type",
                table: "TourHotspots",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourHotspots");
        }
    }
}
