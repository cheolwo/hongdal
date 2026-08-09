using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.InterpretationContracts
{
    public readonly struct SourceStableId : IEquatable<SourceStableId>, IComparable<SourceStableId>
    {
        public SourceStableId(string value)
        {
            StableDataId.EnsureValid(value, nameof(value));
            Value = value.Trim();
        }

        public string Value { get; }
        public bool IsDefined => StableDataId.IsValid(Value);

        public bool Equals(SourceStableId other)
            => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj)
            => obj is SourceStableId other && Equals(other);

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        public int CompareTo(SourceStableId other)
            => StringComparer.Ordinal.Compare(Value, other.Value);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(SourceStableId left, SourceStableId right) => left.Equals(right);
        public static bool operator !=(SourceStableId left, SourceStableId right) => !left.Equals(right);
    }

    public readonly struct WorldStableId : IEquatable<WorldStableId>, IComparable<WorldStableId>
    {
        public WorldStableId(string value)
        {
            StableDataId.EnsureValid(value, nameof(value));
            Value = value.Trim();
        }

        public string Value { get; }
        public bool IsDefined => StableDataId.IsValid(Value);

        public bool Equals(WorldStableId other)
            => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj)
            => obj is WorldStableId other && Equals(other);

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        public int CompareTo(WorldStableId other)
            => StringComparer.Ordinal.Compare(Value, other.Value);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(WorldStableId left, WorldStableId right) => left.Equals(right);
        public static bool operator !=(WorldStableId left, WorldStableId right) => !left.Equals(right);
    }

    /// <summary>서로 다른 World에서 같은 WorldStableId를 안전하게 참조합니다.</summary>
    public readonly struct WorldObjectRef : IEquatable<WorldObjectRef>
    {
        public WorldObjectRef(WorldContextId worldId, WorldStableId objectId)
        {
            if (!objectId.IsDefined) throw new ArgumentException("WorldObjectStableIdMissing", nameof(objectId));
            WorldId = worldId;
            ObjectId = objectId;
        }

        public WorldContextId WorldId { get; }
        public WorldStableId ObjectId { get; }
        public bool Equals(WorldObjectRef other) => WorldId == other.WorldId && ObjectId == other.ObjectId;
        public override bool Equals(object? obj) => obj is WorldObjectRef other && Equals(other);
        public override int GetHashCode() => (WorldId.GetHashCode() * 397) ^ ObjectId.GetHashCode();
        public override string ToString() => WorldId.Value + "/" + ObjectId.Value;
    }

    public readonly struct PresentationStableId : IEquatable<PresentationStableId>, IComparable<PresentationStableId>
    {
        public PresentationStableId(string value)
        {
            StableDataId.EnsureValid(value, nameof(value));
            Value = value.Trim();
        }

        public string Value { get; }
        public bool IsDefined => StableDataId.IsValid(Value);

        public bool Equals(PresentationStableId other)
            => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj)
            => obj is PresentationStableId other && Equals(other);

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        public int CompareTo(PresentationStableId other)
            => StringComparer.Ordinal.Compare(Value, other.Value);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(PresentationStableId left, PresentationStableId right) => left.Equals(right);
        public static bool operator !=(PresentationStableId left, PresentationStableId right) => !left.Equals(right);
    }

    /// <summary>하나 이상의 source 사실이 어떤 World 실체로 해석됐는지 보존합니다.</summary>
    public sealed class WorldIdentityLineage
    {
        public WorldIdentityLineage(WorldStableId worldId, IEnumerable<SourceStableId> sourceIds)
        {
            if (!worldId.IsDefined) throw new ArgumentException("WorldStableIdMissing", nameof(worldId));
            WorldId = worldId;
            SourceIds = Normalize(sourceIds, "WorldIdentitySourceMissing");
        }

        public WorldStableId WorldId { get; }
        public SourceStableId[] SourceIds { get; }

        private static SourceStableId[] Normalize(
            IEnumerable<SourceStableId> values,
            string emptyError)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var input = values.ToArray();
            if (input.Any(value => !value.IsDefined))
                throw new InvalidOperationException("SourceStableIdMissing");
            var result = input.Distinct().OrderBy(value => value).ToArray();
            if (result.Length == 0) throw new InvalidOperationException(emptyError);
            return result;
        }
    }

    /// <summary>하나의 surface 표현이 어떤 World 실체들에서 투영됐는지 보존합니다.</summary>
    public sealed class PresentationIdentityLineage
    {
        public PresentationIdentityLineage(
            PresentationStableId presentationId,
            IEnumerable<WorldStableId> sourceWorldIds)
        {
            if (!presentationId.IsDefined)
                throw new ArgumentException("PresentationStableIdMissing", nameof(presentationId));
            PresentationId = presentationId;
            if (sourceWorldIds == null) throw new ArgumentNullException(nameof(sourceWorldIds));
            var input = sourceWorldIds.ToArray();
            if (input.Any(value => !value.IsDefined))
                throw new InvalidOperationException("PresentationSourceWorldMissing");
            SourceWorldIds = input.Distinct().OrderBy(value => value).ToArray();
            if (SourceWorldIds.Length == 0)
                throw new InvalidOperationException("PresentationSourceWorldMissing");
        }

        public PresentationStableId PresentationId { get; }
        public WorldStableId[] SourceWorldIds { get; }
    }
}
