using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Domain.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "4f42dbe0-a343-4b89-a4f6-bc03fdba2c57",
                ConcurrencyStamp = "b8794ef0-d55f-41d8-bbc6-fc9e735abb20",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
             new IdentityRole
             {
                 Id = "cfeffb6a-682b-4128-b186-b0141e4579f0",
                 ConcurrencyStamp = "6ff9a65a-260d-4fb1-9ed4-e93836b4d09a",
                 Name = "User",
                 NormalizedName = "USER"
             },
             new IdentityRole
             {
                 Id = "cfeffb6a-4128-682b-4128-b0141e4579f0",
                 ConcurrencyStamp = "6ff9a65a-260d-4fb1-9ed4-e93836b4kkjk",
                 Name = "Hotel Admin",
                 NormalizedName = "HOTEL ADMIN"
             }
            );
    }
}
