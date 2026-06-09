namespace CareerHub.Api.DTOs;

public record PagedResponse<T>(
    IEnumerable<T> Data, 
    int Page, 
    int PageSize, 
    int TotalCount, 
    int TotalPages, 
    bool HasNextPage, 
    bool HasPreviousPage 
)
{

    //Calculates metadata automatically
    public PagedResponse(IEnumerable<T> data, int totalCount, int page, int pageSize) 
        : this(
            data, 
            page, 
            pageSize, 
            totalCount, 
            (int)Math.Ceiling(totalCount / (double)pageSize), //mathceiling rounds up to nearest whole number
            page < (int)Math.Ceiling(totalCount / (double)pageSize), //Has next page
            page > 1 //Has previous page
        ) {}
}