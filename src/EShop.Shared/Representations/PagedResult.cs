namespace EShop.Shared.Representations;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);