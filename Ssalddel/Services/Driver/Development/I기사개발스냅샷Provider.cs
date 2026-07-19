using Ssalddel.Contracts.Driver.Development;

namespace Ssalddel.Services.Driver.Development;

public interface I기사개발스냅샷Provider
{
    기사개발스냅샷응답 GetSnapshot();

    void ReplaceSnapshot(기사개발스냅샷응답 snapshot);
}
