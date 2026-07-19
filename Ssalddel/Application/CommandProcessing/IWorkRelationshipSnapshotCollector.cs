using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Application.CommandProcessing;

public interface IWorkRelationshipSnapshotCollector
{
    void Add(WorkRelationshipSnapshotRecordRequest snapshot);

    void AddRange(IEnumerable<WorkRelationshipSnapshotRecordRequest> snapshots);

    IReadOnlyList<WorkRelationshipSnapshotRecordRequest> Drain();
}

public sealed class WorkRelationshipSnapshotCollector : IWorkRelationshipSnapshotCollector
{
    private readonly List<WorkRelationshipSnapshotRecordRequest> _snapshots = [];

    public void Add(WorkRelationshipSnapshotRecordRequest snapshot)
    {
        _snapshots.Add(snapshot);
    }

    public void AddRange(IEnumerable<WorkRelationshipSnapshotRecordRequest> snapshots)
    {
        _snapshots.AddRange(snapshots);
    }

    public IReadOnlyList<WorkRelationshipSnapshotRecordRequest> Drain()
    {
        var drained = _snapshots.ToArray();
        _snapshots.Clear();
        return drained;
    }
}
