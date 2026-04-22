using Application.DTOs.ListingDTOs;
using Application.DTOs.NotificationDTOs;
using Application.DTOs.PhotoDTOs;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Repositories.Listings.ListingBlocks;
using Application.Interfaces.Repositories.Listings.ListingImages;
using Application.Interfaces.Services.ListingServices;
using Application.Interfaces.Services.NotificationServices;
using Application.Interfaces.Services.PhotoServices;
using AutoMapper;
using Domain.Entities.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ListingService
{
    public class HostListingService(
            IQueryListingRepository queryRepository,
            ICommandListingRepository commandRepository,
            ICommandListingImagesRepository commandImageRepository,
            IQueryListingImagesRepository queryImagesRepository,
            ICommandListingBlockRepository commandListingBlockRepository,
            IAvailabilityService availabilityService,
            IPhotoService photoService,
            INotificationService notificationService,
            IMapper mapper) : IHostListingService
    {
        private readonly IQueryListingRepository _queryRepository = queryRepository;
        private readonly IAvailabilityService _availabilityService = availabilityService;
        private readonly ICommandListingRepository _commandRepository = commandRepository;
        private readonly ICommandListingImagesRepository _commandImageRepository = commandImageRepository;
        private readonly IQueryListingImagesRepository _queryImagesRepository = queryImagesRepository;
        private readonly ICommandListingBlockRepository _commandListingBlockRepository = commandListingBlockRepository;
        private readonly IPhotoService _photoService = photoService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<ListingDTO>> GetListingsByHost(Guid hostId)
        {
            var listings = await _queryRepository.GetListingsByHost(hostId).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        //public async Task CreateListing(ListingCreationDTO listingDTO, IEnumerable<IFormFile> images, Guid hostId)
        //{
        //    var listing = _mapper.Map<Listing>(listingDTO);
        //    listing.HostId = hostId;
        //    listing.IsAvailable = false;

        //    await _commandRepository.CreateAsync(listing);

        //    if (images != null && images.Any())
        //    {
        //        var uploadResults = await _photoService.AddPhotosAsync(images, "BookMe/Listings");
        //        var listingImages = uploadResults.Select(result => new ListingImage
        //        {
        //            ListingId = listing.Id,
        //            Url = result.Url,
        //            PublicId = result.PublicId
        //        });

        //        await _commandImageRepository.CreateRangeAsync(listingImages);
        //    }

        //    var notificationDTO = new NotificationCreationDTO
        //    {
        //        UserId = hostId,
        //        Title = "New Property Registered",
        //        Message = "Your property has been successfully created. Remember to activate its availability when you are ready to receive bookings."
        //    };

        //    await _notificationService.SendNotificationAsync(notificationDTO);
        //}

        public async Task<Guid> CreateListing(ListingCreationDTO listingDTO, Guid hostId)
        {
            var listing = _mapper.Map<Listing>(listingDTO);
            listing.HostId = hostId;
            listing.IsAvailable = false; 

            await _commandRepository.CreateAsync(listing); 

            var notificationDTO = new NotificationCreationDTO
            {
                UserId = hostId,
                Title = "New Property Registered",
                Message = "Your property has been successfully created. Now upload some great photos!"
            };
            await _notificationService.SendNotificationAsync(notificationDTO);

            return listing.Id; 
        }

        public async Task UploadListingImages(Guid listingId, Guid userId, string userRole, IEnumerable<IFormFile> images)
        {
            await CheckHostAuthorization(listingId, userId, userRole);

            if (images != null && images.Any())
            {
                var uploadResults = await _photoService.AddPhotosAsync(images, "BookMe/Listings");
                var listingImages = uploadResults.Select((result, index) => new ListingImage
                {
                    ListingId = listingId,
                    Url = result.Url,
                    PublicId = result.PublicId,
                    Order = index,
                    IsMain = index == 0
                });

                await _commandImageRepository.CreateRangeAsync(listingImages);
            }
        }

        public async Task UpdateFullListingAsync(Guid listingId, Guid userId, string userRole, ListingUpdateDTO updateDTO)
        {
            await CheckHostAuthorization(listingId, userId, userRole);

            var listing = await _queryRepository.GetByIdAsync(listingId);
            if (listing == null) throw new KeyNotFoundException("Listing not found.");

            _mapper.Map(updateDTO, listing);

            await _commandRepository.UpdateAsync(listingId, listing);
        }

        public async Task<ListingDTO> GetListingByIdForHost(Guid id, Guid hostId)
        {
            var listing = await _queryRepository.GetListingForEditAsync(id);

            if (listing == null || listing.HostId != hostId)
                throw new KeyNotFoundException("The requested property was not found or you don't have access.");

            return _mapper.Map<ListingDTO>(listing);
        }

        public async Task<double> GetHostAverageRatingAsync(Guid hostId)
        {
            var listings = await _queryRepository.GetListingsByHost(hostId)
                .Include(l => l.Reviews)
                .ToListAsync();

            var allReviews = listings.SelectMany(l => l.Reviews).ToList();

            if (!allReviews.Any()) return 0.0;

            return Math.Round(allReviews.Average(r => r.Rating), 1);
        }
        public async Task<IEnumerable<PhotoUploadResultDTO>> GetListingImagesAsync(Guid listingId, Guid hostId)
        {
            var listing = await _queryRepository.GetByIdAsync(listingId);
            if (listing == null || listing.HostId != hostId)
                throw new UnauthorizedAccessException("No tienes acceso a estas imágenes.");

            var images = await _queryImagesRepository.GetImagesByListingId(listingId).ToListAsync();

            return images.Select(img => new PhotoUploadResultDTO
            {
                Url = img.Url,
                PublicId = img.PublicId
            });
        }

        public async Task DeleteListing(Guid id, Guid userId, string userRole)
        {
            await CheckHostAuthorization(id, userId, userRole);

            var images = await _queryImagesRepository.GetImagesByListingId(id)
                .ToListAsync();
            
            foreach (var img in images)
            {
                await _photoService.DeletePhotoAsync(img.PublicId);
            }

            await _commandRepository.DeleteAsync(id);

            var notificationDTO = new NotificationCreationDTO
            {
                UserId = userId,
                Title = "Property Deleted",
                Message = "The property has been successfully removed from the system."
            };

            await _notificationService.SendNotificationAsync(notificationDTO);
        }


        public async Task UpdateCapacity(Guid listingId, Guid userId, string userRole, ListingCapacity capacity)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandRepository.UpdateCapacityAsync(listingId, capacity);
        }
        public async Task UpdateLocation(Guid listingId, Guid userId, string userRole, ListingLocation location)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandRepository.UpdateLocationAsync(listingId, location);
        }
        public async Task UpdatePriceAndRules(Guid listingId, Guid userId, string userRole, ListingPricingRules pricingRules)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandRepository.UpdatePriceRulesAsync(listingId, pricingRules);
        }

        //Availability management
        public async Task ToggleListingAvailability(Guid listingId, Guid userId, string userRole, bool availability)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandRepository.UpdateAvailabilityAsync(listingId, availability);
        }
        public async Task BlockDatesAsync(Guid listingId, DateTime startDate, DateTime endDate, string reason, Guid userId, string userRole)
        {
            await CheckHostAuthorization(listingId, userId, userRole);

            if(string.IsNullOrEmpty(reason))
                throw new ArgumentException("Reason for blocking dates cannot be empty.");

            if (startDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Cannot block dates in the past.");

            if (startDate.Date >= endDate.Date)
                throw new ArgumentException("Start date must be before the end date.");

            bool isAvailable = await _availabilityService.IsListingAvailableAsync(listingId, startDate.Date, endDate.Date);

            if (!isAvailable)
                throw new InvalidOperationException("Cannot block these dates because they overlap with an existing confirmed booking or are already blocked.");

            var listingBlock = new ListingBlock
            {
                ListingId = listingId,
                HostId = userId,
                StartDate = startDate.Date,
                Reason = reason,
                EndDate = endDate.Date,
                CreatedAt = DateTime.UtcNow
            };

            await _commandListingBlockRepository.CreateAsync(listingBlock);
        }
        public async Task UnblockDatesAsync(Guid blockId, Guid listingId, Guid userId, string userRole)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandListingBlockRepository.DeleteAsync(blockId);
        }

        //Amenities management
        public async Task AddAmenities(Guid listingId, Guid amenityId, Guid userId, string userRole)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandRepository.AddAmenityAsync(listingId, amenityId);
        }
        public async Task RemoveAmenities(Guid listingId, Guid amenityId, Guid userId, string userRole)
        {
            await CheckHostAuthorization(listingId, userId, userRole);
            await _commandRepository.RemoveAmenityAsync(listingId, amenityId);
        }


        //Helper method to check if the user is authorized to perform actions on the listing
        private async Task CheckHostAuthorization(Guid listingId, Guid userId, string userRole)
        {
            if (string.IsNullOrEmpty(userRole) || !userRole.Contains("Host", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Host role is required to perform this action.");

            var listing = await _queryRepository.GetByIdAsync(listingId);
            if (listing == null)
                throw new KeyNotFoundException($"Property with ID {listingId} was not found.");
            
            if (listing.HostId != userId)
                throw new UnauthorizedAccessException("You do not have permission to modify a property that does not belong to you.");
        }
    }
}