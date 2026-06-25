using System.Text.Json;

namespace 홍달.Services
{
    public interface IDriverWorkQueueStore
    {
        Task UpsertAsync(DriverWorkQueueEntry entry, CancellationToken cancellationToken = default);
        Task RemoveAsync(string driverId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DriverWorkQueueEntry>> SnapshotAsync(CancellationToken cancellationToken = default);
    }

    public sealed record DriverWorkQueueEntry(
        string DriverId,
        long ShiftId,
        DateTime StartedAtUtc,
        string StartMode,
        string StartLocation,
        string? ReturnDestination);
}
