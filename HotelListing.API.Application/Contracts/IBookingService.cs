using HotelListing.Api.Common.Results;
using HotelListing.API.Application.DTOs.Booking;
using HotelListing.API.Common.Models.Paging;

namespace HotelListing.API.Application.Contracts
{
    public interface IBookingService
    {
        Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId);
        Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId);
        Task<Result> CancelBookingAsync(int hotelId, int bookingId);
        Task<Result<GetBookingsDto>> CreateBookings(CreateBookingsDto dto);
        Task<Result<PagedResult<GetBookingsDto>>> GetBookingsForHotelAsync(int hotelId, PaginationParameters paginationParameters); 
        Task<Result<PagedResult<GetBookingsDto>>> GetUserBookingsForHotelAsync(int hotelId, PaginationParameters paginationParameters);
        Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
    }
}