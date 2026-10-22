using HotelListing.Api.Controllers;
using HotelListing.API.Contracts;
using HotelListing.API.DTOs.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.API.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
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


    [HttpPut("{bookingId:int}/cancel")]

    public async Task<ActionResult> CancelBooking(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingService.CancelBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }


    [HttpPut("{bookingId:int}/admin/cancel")]
    [Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<ActionResult> AdminCancelBooking(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }


    [HttpPut("{bookingId:int}/admin/confirm")]
    [Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<ActionResult> AdminConfirmBooking(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminConfirmBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }



}
