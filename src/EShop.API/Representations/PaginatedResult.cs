namespace EShop.API.Representations;

public record PaginatedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total);

