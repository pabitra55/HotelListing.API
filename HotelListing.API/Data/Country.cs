namespace HotelListing.API.Data;

/// <summary>
/// Class representing a country with its associated hotels.
/// </summary>
public class Country
{
    public int CountryId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public IList<Hotel> Hotels { get; set; } = [];
}
