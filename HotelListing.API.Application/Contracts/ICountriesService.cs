using HotelListing.Api.Common.Results;
using HotelListing.API.Application.DTOs.Country;
using HotelListing.API.Application.DTOs.Hotel;
using HotelListing.API.Common.Models.Paging;

namespace HotelListing.API.Application.Contracts;

public interface ICountriesService
{
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createDto);
    Task<Result> DeleteCountryAsync(int id);
    Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync();
    Task<Result<GetCountryDto>> GetCountryAsync(int id);
    Task<Result<PagedResult<GetCountryHotelsDto>>> GetCountryHotelsAsync(int countryId, PaginationParameters paginationParameters);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto);
}