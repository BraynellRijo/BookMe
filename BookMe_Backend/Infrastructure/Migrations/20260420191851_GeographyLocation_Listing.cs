using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GeographyLocation_Listing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricingRules_PricePerNight",
                table: "Listings",
                newName: "PricePerNight");

            migrationBuilder.RenameColumn(
                name: "PricingRules_CleaningFee",
                table: "Listings",
                newName: "CleaningFee");

            migrationBuilder.RenameColumn(
                name: "PricingRules_CheckOutTime",
                table: "Listings",
                newName: "CheckOutTime");

            migrationBuilder.RenameColumn(
                name: "PricingRules_CheckInTime",
                table: "Listings",
                newName: "CheckInTime");

            migrationBuilder.RenameColumn(
                name: "Location_Longitude",
                table: "Listings",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "Location_Latitude",
                table: "Listings",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "Location_Country",
                table: "Listings",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "Location_City",
                table: "Listings",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "Location_Address",
                table: "Listings",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "Capacity_MaxGuests",
                table: "Listings",
                newName: "MaxGuests");

            migrationBuilder.RenameColumn(
                name: "Capacity_BedsQuantity",
                table: "Listings",
                newName: "BedsQuantity");

            migrationBuilder.RenameColumn(
                name: "Capacity_BedroomsQuantity",
                table: "Listings",
                newName: "BedroomsQuantity");

            migrationBuilder.RenameColumn(
                name: "Capacity_BathroomsQuantity",
                table: "Listings",
                newName: "BathroomsQuantity");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Point>(
                name: "SpatialLocation",
                table: "Listings",
                type: "geography",
                nullable: true,
                computedColumnSql: "geography::Point([Latitude], [Longitude], 4326)",
                stored: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "SpatialLocation",
                table: "Listings");

            migrationBuilder.RenameColumn(
                name: "PricePerNight",
                table: "Listings",
                newName: "PricingRules_PricePerNight");

            migrationBuilder.RenameColumn(
                name: "MaxGuests",
                table: "Listings",
                newName: "Capacity_MaxGuests");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Listings",
                newName: "Location_Longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Listings",
                newName: "Location_Latitude");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Listings",
                newName: "Location_Country");

            migrationBuilder.RenameColumn(
                name: "CleaningFee",
                table: "Listings",
                newName: "PricingRules_CleaningFee");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Listings",
                newName: "Location_City");

            migrationBuilder.RenameColumn(
                name: "CheckOutTime",
                table: "Listings",
                newName: "PricingRules_CheckOutTime");

            migrationBuilder.RenameColumn(
                name: "CheckInTime",
                table: "Listings",
                newName: "PricingRules_CheckInTime");

            migrationBuilder.RenameColumn(
                name: "BedsQuantity",
                table: "Listings",
                newName: "Capacity_BedsQuantity");

            migrationBuilder.RenameColumn(
                name: "BedroomsQuantity",
                table: "Listings",
                newName: "Capacity_BedroomsQuantity");

            migrationBuilder.RenameColumn(
                name: "BathroomsQuantity",
                table: "Listings",
                newName: "Capacity_BathroomsQuantity");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Listings",
                newName: "Location_Address");

            migrationBuilder.AlterColumn<string>(
                name: "Location_Country",
                table: "Listings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location_City",
                table: "Listings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location_Address",
                table: "Listings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
