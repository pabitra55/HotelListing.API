using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Common.Results;
using HotelListing.API.Common.Constants;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Domain.Data.Enums;
using HotelListing.API.Domain.Data;
using HotelListing.API.Application.Contracts;
using HotelListing.API.Application.DTOs.Booking;
namespace HotelListing.API.Application.Services;

public class BookingService(HotelListingDbContext context, IUsersService usersService, IMapper mapper) : IBookingService
{

    public async Task<Result<IEnumerable<GetBookingsDto>>> GetUserBookingsForHotelAsync(int hotelId)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);

        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingsDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingsDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }

    public async Task<Result<IEnumerable<GetBookingsDto>>> GetUserBookingsForHotelsAsync(int hotelId)
    {
        var userId = usersService.UserId;

        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingsDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId && b.UserId == userId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingsDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingsDto>> CreateBookings(CreateBookingsDto dto)
    {
        var userId = usersService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "User is required."));
        }

        var hotel = await context.Hotels
            .Where(h => h.Id == dto.HotelId)
            .FirstOrDefaultAsync();

        if (hotel is null)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, $"Hotel '{dto.HotelId}' was not found."));
        }

        var overlaps = await IsOverlap(dto.HotelId, userId, dto.CheckIn, dto.CheckOut);
        if (overlaps)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, $"The selected dates overlap with an existing booking."));
        }

        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        var totalPrice = hotel.PerNightrate * nights;
        var booking = mapper.Map<Booking>(dto);
        booking.UserId = userId;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var result = mapper.Map<GetBookingsDto>(booking);
        return Result<GetBookingsDto>.Success(result);
    }



    public async Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto dto)
    {
        var userId = usersService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "User is required."));
        }


        var overlaps = await IsOverlap(hotelId, userId, dto.CheckIn, dto.CheckOut);
        if (overlaps)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, $"The selected dates overlap with an existing booking."));
        }

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserId == userId
            );

        if (booking is null)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}'  was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, $"Cancelled bookings cannot be modified."));
        }

        mapper.Map(dto, booking);
        var perNight = booking.Hotel!.PerNightrate;
        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        booking.TotalPrice = perNight * nights;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var updated = mapper.Map<GetBookingsDto>(booking);

        return Result<GetBookingsDto>.Success(updated);
    }


    private async Task<bool> IsOverlap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut)
    {
        return await context.Bookings.AnyAsync(
            b => b.HotelId == hotelId
            && b.Status != BookingStatus.Cancelled
            && checkIn < b.CheckOut
            && checkOut > b.CheckIn
            && b.UserId == userId
        );
    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = usersService.UserId;

        var booking = await context.Bookings
          .Include(b => b.Hotel)
          .FirstOrDefaultAsync(b =>
              b.Id == bookingId
              && b.HotelId == hotelId
              && b.UserId == userId
          );

        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}'  was not found."));
        }


        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, $"Cancelled bookings cannot be modified."));
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success();
    }

    //public Task<Result> ConfirmBookingAsync(int hotelId, int bookingId)
    //{

    //}

    public async Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = usersService.UserId;
        var isHotelAdminUser = await context.HotelAdmins.AnyAsync(q => q.UserId == userId && q.Hotelid == hotelId);

        if (!isHotelAdminUser)
        {
            return Result.Failure(new Error(ErrorCodes.Forbid, $"You are not an admin of the selected Hotel."));
        }

        var booking = await context.Bookings
        .Include(b => b.Hotel)
        .FirstOrDefaultAsync(b =>
            b.Id == bookingId
            && b.HotelId == hotelId
            && b.UserId == userId
        );

        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}'  was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, $"The booking has already been cancelled."));
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId)
    {
        var userId = usersService.UserId;
        var isHotelAdminUser = await context.HotelAdmins.AnyAsync(q => q.UserId == userId && q.Hotelid == hotelId);

        if (!isHotelAdminUser)
        {
            return Result.Failure(new Error(ErrorCodes.Forbid, $"You are not an admin of the selected Hotel."));
        }

        var booking = await context.Bookings
        .Include(b => b.Hotel)
        .FirstOrDefaultAsync(b =>
            b.Id == bookingId
            && b.HotelId == hotelId
            && b.UserId == userId
        );

        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}'  was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, $"The booking has already been cancelled."));
        }

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsForHotelAsync(int hotelId)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
            return Result<IEnumerable<GetBookingsDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingsDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }
}
