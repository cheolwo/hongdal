namespace Ssalddel.Services.Outbox;

public static class OutboxProcessingStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
}

public static class OutboxProcessingPolicy
{
    public const int MaximumAttempts = 5;

    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan LeaseTimeout = TimeSpan.FromMinutes(5);

    public static bool CanRetry(int attemptCount)
        => attemptCount < MaximumAttempts;
}
