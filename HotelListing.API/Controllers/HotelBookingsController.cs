using HotelListing.Api.Controllers;
using HotelListing.API.Contracts;
using HotelListing.API.DTOs.Booking;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.API.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    // GET api/hotels/5/bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBookingsDto>>> GetBookings([FromRoute] int hotelId)
    {
        var result = await bookingService.GetBookingsForHotelsAsync(hotelId);
        return ToActionResult(result);
    }


    [HttpPost]
    public async Task<ActionResult<GetBookingsDto>> CreateBookings([FromRoute] int hotelId, [FromBody] CreateBookingsDto createBookingsDto)
    {
        var result = await bookingService.CreateBookings(createBookingsDto);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingsDto>> UpdateBooking(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId,
        [FromBody] UpdateBookingDto updateBookingDto
        )
    {
        var result = await bookingService.UpdateBookingAsync(hotelId, bookingId, updateBookingDto);
        return ToActionResult(result);
    }



}
