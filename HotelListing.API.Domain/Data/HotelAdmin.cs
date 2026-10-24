namespace HotelListing.API.Domain.Data;

public class HotelAdmin
{
    public int Id { get; set; }
    public Hotel? Hotel { get; set; }
    public int Hotelid { get; set; }
    public ApplicationUser? User { get; set; }
    public string UserId { get; set; } = string.Empty;
}
