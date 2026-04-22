using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Listing_Table_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity_MaxGuest = table.Column<int>(type: "int", nullable: false),
                    Capacity_GuestQuantity = table.Column<int>(type: "int", nullable: false),
                    Capacity_BedroomsQuantity = table.Column<int>(type: "int", nullable: false),
                    Capacity_BedsQuantity = table.Column<int>(type: "int", nullable: false),
                    Capacity_BathroomsQuantity = table.Column<double>(type: "float(3)", precision: 3, scale: 1, nullable: false),
                    Location_Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location_Latitude = table.Column<double>(type: "float", nullable: false),
                    Location_Longitude = table.Column<double>(type: "float", nullable: false),
                    PricingRules_PricePerNight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricingRules_CleaningFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricingRules_CheckInTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PricingRules_CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Amenities_HasPool = table.Column<bool>(type: "bit", nullable: false),
                    Amenities_HasInternet = table.Column<bool>(type: "bit", nullable: false),
                    Amenities_AllowsPets = table.Column<bool>(type: "bit", nullable: false),
                    Amenities_HasParking = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Listings");
        }
    }
}
