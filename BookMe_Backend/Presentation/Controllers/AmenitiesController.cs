using Application.DTOs.ListingDTOs;
using Application.Interfaces.Services.ListingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenitiesController(IAmenityService amenityService) : ControllerBase
    {
        private readonly IAmenityService _amenityService = amenityService;

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AmenityDTO>>> GetAll()
        {
            var amenities = await _amenityService.GetAllAsync();
            return Ok(amenities);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AmenityDTO>> GetById(Guid id)
        {
            var amenity = await _amenityService.GetByIdAsync(id);
            return Ok(amenity);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AmenityDTO amenityDto)
        {
            await _amenityService.CreateAsync(amenityDto);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _amenityService.DeleteAsync(id);
            return NoContent();
        }
    }
}