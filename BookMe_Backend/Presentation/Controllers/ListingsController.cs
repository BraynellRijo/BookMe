using Application.Interfaces.Services.ListingServices;
using Domain.Enums.Listing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class ListingsController(IListingService listingService) : ControllerBase
    {
        private readonly IListingService _listingService = listingService;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetListingById(Guid id)
        {
            try
            {
                var listing = await _listingService.GetListingByIdAsync(id);
                return Ok(listing);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetListingImages(Guid id)
        {
            var images = await _listingService.GetListingImagesAsync(id);
            return Ok(images);
        }

        [HttpGet("{id}/main-image")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetMainImage(Guid id)
        {
                var imageUrl = await _listingService.GetMainImageUrlAsync(id);
                return Ok(new { url = imageUrl });
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyListings(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm = 5.0,
            [FromQuery] string? type = null)
        {
            PropertyType? propertyType = null;
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<PropertyType>(type, true, out var parsedType))
                propertyType = parsedType;

            var result = await _listingService.GetNearbyListingsAsync(lat, lng, radiusKm, propertyType);
            return Ok(result);
        }

        [HttpGet("City/{city}")]
        public async Task<IActionResult> GetListingsByCity(string city)
        {
            var listings = await _listingService.FilterByCityAsync(city);
            return Ok(listings);
        }

        [HttpGet("Country/{country}")]
        public async Task<IActionResult> GetListingsByCountry(string country)
        {
            var listings = await _listingService.FilterByCountryAsync(country);
            return Ok(listings);
        }

        [HttpGet("Price/{minPrice}/{maxPrice}")]
        public async Task<IActionResult> GetListingsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            var listings = await _listingService.FilterByPricePerNightRangeAsync(minPrice, maxPrice);
            return Ok(listings);
        }

        [HttpGet("Type/{type}")]
        public async Task<IActionResult> GetListingsByType(string type)
        {
            if (!Enum.TryParse<PropertyType>(type, true, out var propertyType))
                return BadRequest("Invalid property type.");

            var listings = await _listingService.FilterByTypeAsync(propertyType);
            return Ok(listings);
        }

        [HttpGet("Search/{title}")]
        public async Task<IActionResult> GetListingsByTitle(string title)
        {
            var listings = await _listingService.FilterByTitleAsync(title);
            return Ok(listings);
        }

        [HttpGet("Capacity/{minGuests}/{maxGuests}")]
        public async Task<IActionResult> GetListingsByCapacity(int minGuests, int maxGuests)
        {
            var listings = await _listingService.FilterByCapacityAsync(minGuests, maxGuests);
            return Ok(listings);
        }

        [HttpGet("Available-Dates")]
        public async Task<IActionResult> GetListingsByDateRange([FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            var listings = await _listingService.FilterByDateRangeAsync(checkIn, checkOut);
            return Ok(listings);
        }

        [HttpGet("Amenities")]
        public async Task<IActionResult> GetListingsByAmenities([FromQuery] List<Guid> amenityIds)
        {
            var listings = await _listingService.FilterByAmenitiesAsync(amenityIds);
            return Ok(listings);
        }
    }
}