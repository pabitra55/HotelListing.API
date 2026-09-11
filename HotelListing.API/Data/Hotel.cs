namespace HotelListing.API.Data;

/// <summary>
/// Class representing a hotel with its details and associated country.
/// </summary>
public class Hotel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public double Rating { get; set; }
    public int Countryid { get; set; }
    public Country? Country { get; set; }
}
