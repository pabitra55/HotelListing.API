namespace HotelListing.API.DTOs.Booking;

public record CreateBookingsDto
(
    int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests
);
