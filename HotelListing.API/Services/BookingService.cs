using HotelListing.Api.Data;
using HotelListing.Api.Results;
using HotelListing.API.Contracts;
using HotelListing.API.Data.Enums;
using HotelListing.API.DTOs.Booking;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security;

namespace HotelListing.API.Services;

public class BookingService(HotelListingDbContext context, IUsersService usersService) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsForHotelsAsync(int hotelId)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);

        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingsDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .Select(b => new GetBookingsDto(
                b.Id,
                b.HotelId,
                b.Hotel!.Name,
                b.CheckIn,
                b.CheckOut,
                b.Guests,
                b.TotalPrice,
                b.Status.ToString(),
                b.CreatedAtUtc,
                b.UpdatedAtUtc
             )).ToListAsync();


        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingsDto>> CreateBookings(CreateBookingsDto dto)
    {
        var userId = usersService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "User is required."));
        }

        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        if (nights <= 0)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "Checkout must be after check-in."));
        }

        if (dto.Guests <= 0)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "Guest must be at least 1."));
        }

        var hotel = await context.Hotels
            .Where(h => h.Id == dto.HotelId)
            .FirstOrDefaultAsync();

        if (hotel is null)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, $"Hotel '{dto.HotelId}' was not found."));
        }

        var overlaps = await context.Bookings.AnyAsync(
            b => b.HotelId == dto.HotelId
            && b.Status != BookingStatus.Cancelled
            && dto.CheckIn < b.CheckOut
            && dto.CheckOut > b.CheckIn
            && b.UserId == userId
            );

        if (overlaps)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, $"The selected dates overlap with an existing booking."));
        }

        var totalPrice = hotel.PerNightrate * nights;

        var booking = new Booking
        {
            HotelId = dto.HotelId,
            UserId = userId,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            Guests = dto.Guests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending
        };


        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var createdBooking = new GetBookingsDto(
            booking.Id,
            hotel.Id,
            hotel.Name,
            dto.CheckIn,
            dto.CheckOut,
            dto.Guests,
            totalPrice,
            BookingStatus.Pending.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
            );


        return Result<GetBookingsDto>.Success(createdBooking);
    }



    public async Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto dto)
    {
        var userId = usersService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "User is required."));
        }

        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        if (nights <= 0)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "Checkout must be after check-in."));
        }

        if (dto.Guests <= 0)
        {
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "Guest must be at least 1."));
        }

        var overlaps = await context.Bookings.AnyAsync(
            b => b.HotelId == hotelId
            && b.Status != BookingStatus.Cancelled
            && dto.CheckIn < b.CheckOut
            && dto.CheckOut > b.CheckIn
            && b.UserId == userId
        );

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

        var perNight = booking.Hotel!.PerNightrate;
        booking.CheckIn = dto.CheckIn;
        booking.CheckOut = dto.CheckOut;
        booking.Guests = dto.Guests;
        booking.TotalPrice = perNight * (dto.CheckOut.DayNumber - dto.CheckIn.DayNumber);
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var updated = new GetBookingsDto(
                        booking.Id,
                        booking.HotelId,
                        booking.Hotel!.Name,
                        booking.CheckIn,
                        booking.CheckOut,
                        booking.Guests,
                        booking.TotalPrice,
                        booking.Status.ToString(),
                        booking.CreatedAtUtc,
                        booking.UpdatedAtUtc
            );
        return Result<GetBookingsDto>.Success(updated);
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
}
