using HotelListing.Api.Results;
using HotelListing.API.DTOs.Booking;

namespace HotelListing.API.Contracts
{
    public interface IBookingService
    {
        Task<Result<GetBookingsDto>> CreateBookings(CreateBookingsDto dto);
        Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsForHotelsAsync(int hotelId);
        Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
    }
}