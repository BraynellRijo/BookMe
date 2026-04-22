using Application.DTOs.BookingDTOs;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings;
using Application.Validators.BookingValidators;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Bookings;
using Domain.Enums.Bookings;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.BookingService
{
    public class BookingService(ICommandBookingRepository commandBookingRepository,
        IQueryBookingRepository queryBookingRepository,
        IQueryListingRepository queryListingRepository,
        IMapper mapper,
        IValidator<BookingCreationDTO> bookingCreationDTOValidator,
        IValidator<BookingDTO> bookingDTOValidator)
    {
        private readonly ICommandBookingRepository _commandBookingRepository = commandBookingRepository;
        private readonly IQueryBookingRepository _queryBookingRepository = queryBookingRepository;
        private readonly IQueryListingRepository _queryListingRepository = queryListingRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<BookingDTO> _bookingDTOValidator = bookingDTOValidator;

        public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId)
        {
            ValidateBookingId(bookingId);

            var booking = _mapper.Map<BookingDTO>(await _queryBookingRepository
                .GetByIdAsync(bookingId));
            await BookingDTOValidator(booking);

            return booking;
        }
        public async Task<IEnumerable<BookingDTO>> GetAllListingBookingsAsync(Guid listingId)
        {
            ValidateBookingId(listingId);

            var bookings = _queryBookingRepository.GetAllByListing(listingId);

            var bookingsDTO = await bookings.ProjectTo<BookingDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return bookingsDTO;
        }

        //public async Task<IEnumerable<BookedDateRangeDTO>> GetListingBookingsDaysAsync(Guid listingId)
        //{
        //    ValidateBookingId(listingId);

        //    var bookingsQuery = _queryBookingRepository.GetActiveBookingsByListing(listingId);
        //    var bookedDaysDTO = await bookingsQuery
        //        .ProjectTo<BookedDateRangeDTO>(_mapper.ConfigurationProvider)
        //        .ToListAsync();

        //    return bookedDaysDTO;
        //}

        //Private helper methods
        private void ValidateBookingId(Guid bookingId)
        {
            if (bookingId == Guid.Empty)
                throw new ArgumentException("Invalid booking ID.");
        }
        private async Task BookingDTOValidator(BookingDTO booking)
        {
            var validationResult = await _bookingDTOValidator.ValidateAsync(booking);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }
    }
}
