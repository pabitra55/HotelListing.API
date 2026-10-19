namespace HotelListing.API.DTOs.Booking;

public record GetBookingsDto
(
    int Id,
    int HotelId,
    string HotelName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
