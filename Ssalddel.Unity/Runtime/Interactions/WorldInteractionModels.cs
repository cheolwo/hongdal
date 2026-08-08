using System;

namespace Ssalddel.Unity.Interactions
{
    public static class WorldInteractionStateCodes
    {
        public const string Preview = "Preview";
        public const string AwaitingConfirmation = "AwaitingConfirmation";
        public const string Submitting = "Submitting";
        public const string RefreshingCanonicalState = "RefreshingCanonicalState";
        public const string Completed = "Completed";
        public const string Rejected = "Rejected";
        public const string Failed = "Failed";
    }

    public sealed class WorldInteractionIntent
    {
        public string InteractionId { get; set; } = string.Empty;

        public string ActionCode { get; set; } = string.Empty;

        public string TargetStableId { get; set; } = string.Empty;

        public long ExpectedRevision { get; set; }

        public string EffectCode { get; set; } = string.Empty;

        public bool ExplicitlyConfirmed { get; set; }
    }

    public sealed class WorldInteractionResult
    {
        public string StateCode { get; set; } = string.Empty;

        public string ReasonCode { get; set; } = string.Empty;

        public long? CanonicalRevision { get; set; }

        public bool RequiresCanonicalStateRefresh { get; set; }

        public string[] SuggestedActions { get; set; } = Array.Empty<string>();
    }
}
