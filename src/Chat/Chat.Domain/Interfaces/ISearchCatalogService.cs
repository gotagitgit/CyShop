namespace Chat.Domain.Interfaces;

using Chat.Domain.Entities;

public interface ISearchCatalogService
{
    Task<IReadOnlyList<ChatProduct>> SearchAsync(string query, CancellationToken ct = default);
}
