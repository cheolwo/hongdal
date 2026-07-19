using System.Text.Json;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IKamisJsonClient
{
    Task<JsonDocument> GetDocumentAsync(
        string requestPath,
        CancellationToken cancellationToken = default);
}
