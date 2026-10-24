namespace HotelListing.API.Common.Models.Paging;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public PaginationMetadata MetaData { get; set; } = new();
}
