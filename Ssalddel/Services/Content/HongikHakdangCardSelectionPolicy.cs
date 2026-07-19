using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Services.Content;

public interface IHongikHakdangCardSelectionPolicy
{
    long Select(
        DateOnly selectionDate,
        string timeZoneId,
        IReadOnlyCollection<long> activeCardIds,
        IReadOnlyCollection<long> recentlySelectedCardIds);
}

public sealed class HongikHakdangCardSelectionPolicy : IHongikHakdangCardSelectionPolicy
{
    public long Select(
        DateOnly selectionDate,
        string timeZoneId,
        IReadOnlyCollection<long> activeCardIds,
        IReadOnlyCollection<long> recentlySelectedCardIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        ArgumentNullException.ThrowIfNull(activeCardIds);
        ArgumentNullException.ThrowIfNull(recentlySelectedCardIds);
        if (activeCardIds.Count == 0)
        {
            throw new InvalidOperationException("선택할 수 있는 홍익학당 카드가 없습니다.");
        }

        var active = activeCardIds.Distinct().Order().ToArray();
        var recent = recentlySelectedCardIds.ToHashSet();
        var candidates = active.Where(id => !recent.Contains(id)).ToArray();
        if (candidates.Length == 0)
        {
            candidates = active;
        }

        var seed = SHA256.HashData(Encoding.UTF8.GetBytes($"{selectionDate:yyyy-MM-dd}|{timeZoneId}"));
        var index = (int)(BitConverter.ToUInt32(seed, 0) % candidates.Length);
        return candidates[index];
    }
}
