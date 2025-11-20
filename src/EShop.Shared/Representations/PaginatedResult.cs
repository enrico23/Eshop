namespace EShop.Shared.Representations;

public record PaginatedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total);