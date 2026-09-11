using Microsoft.EntityFrameworkCore;
namespace HotelListing.API.Data;


/// <summary>
/// Class for the database context of the Hotel Listing application, responsible for managing the connection to the database and providing access to the data models.
/// </summary>
public class HotelListingDbContext : DbContext
{
    public HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) : base(options)
    {

    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<Hotel> Hotels { get; set; }



}
