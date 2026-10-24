using HotelListing.Api.Common.Results;
using HotelListing.API.Application.DTOs.Booking;

namespace HotelListing.API.Application.Contracts
{
    public interface IBookingService
    {
        Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId);
        Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId);
        Task<Result> CancelBookingAsync(int hotelId, int bookingId);
        Task<Result<GetBookingsDto>> CreateBookings(CreateBookingsDto dto);
        Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsForHotelAsync(int hotelId); 
        Task<Result<IEnumerable<GetBookingsDto>>> GetUserBookingsForHotelAsync(int hotelId);
        Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
    }
}