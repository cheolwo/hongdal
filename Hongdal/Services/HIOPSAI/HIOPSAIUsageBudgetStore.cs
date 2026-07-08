using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.HIOPSAI;

public interface IHIOPSAIUsageBudgetStore
{
    Task<HIOPSAIUsageReservation> TryReserveAsync(decimal estimatedCostUsd, CancellationToken cancellationToken = default);
    Task<HIOPSAIUsageSnapshot> CompleteAsync(Guid reservationId, decimal actualCostUsd, CancellationToken cancellationToken = default);
    Task ReleaseAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<HIOPSAIUsageSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed class FileHIOPSAIUsageBudgetStore : IHIOPSAIUsageBudgetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly HIOPSAIOptions _options;
    private readonly Dictionary<Guid, decimal> _reservations = [];

    public FileHIOPSAIUsageBudgetStore(IOptions<HIOPSAIOptions> options)
    {
        _options = options.Value;
    }

    public async Task<HIOPSAIUsageReservation> TryReserveAsync(decimal estimatedCostUsd, CancellationToken cancellationToken = default)
    {
        if (estimatedCostUsd > _options.MaxEstimatedCostPerCallUsd)
        {
            return HIOPSAIUsageReservation.Blocked(
                $"예상 호출 비용 ${estimatedCostUsd:F4}가 1회 제한 ${_options.MaxEstimatedCostPerCallUsd:F2}를 초과했습니다.",
                _options.MonthlyBudgetUsd);
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var ledger = await LoadAsync(cancellationToken);
            EnsureCurrentMonth(ledger);

            if (ledger.SpentUsd + ledger.ReservedUsd + estimatedCostUsd > _options.MonthlyBudgetUsd)
            {
                return HIOPSAIUsageReservation.Blocked(
                    $"월 예산 ${_options.MonthlyBudgetUsd:F2}를 초과할 수 있어 LLM 호출을 차단했습니다.",
                    ledger.SpentUsd,
                    ledger.ReservedUsd,
                    _options.MonthlyBudgetUsd);
            }

            var reservationId = Guid.NewGuid();
            ledger.ReservedUsd += estimatedCostUsd;
            ledger.UpdatedAtUtc = DateTimeOffset.UtcNow;
            _reservations[reservationId] = estimatedCostUsd;
            await SaveAsync(ledger, cancellationToken);

            return new HIOPSAIUsageReservation(
                Allowed: true,
                BlockedReason: null,
                ReservationId: reservationId,
                EstimatedCostUsd: estimatedCostUsd,
                MonthlySpentUsd: ledger.SpentUsd,
                MonthlyReservedUsd: ledger.ReservedUsd,
                MonthlyBudgetUsd: _options.MonthlyBudgetUsd);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<HIOPSAIUsageSnapshot> CompleteAsync(
        Guid reservationId,
        decimal actualCostUsd,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var ledger = await LoadAsync(cancellationToken);
            EnsureCurrentMonth(ledger);

            if (_reservations.Remove(reservationId, out var reserved))
            {
                ledger.ReservedUsd = Math.Max(0m, ledger.ReservedUsd - reserved);
            }

            ledger.SpentUsd += actualCostUsd;
            ledger.CallsThisMonth++;
            ledger.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await SaveAsync(ledger, cancellationToken);

            return ToSnapshot(ledger);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ReleaseAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!_reservations.Remove(reservationId, out var reserved))
            {
                return;
            }

            var ledger = await LoadAsync(cancellationToken);
            EnsureCurrentMonth(ledger);
            ledger.ReservedUsd = Math.Max(0m, ledger.ReservedUsd - reserved);
            ledger.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await SaveAsync(ledger, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<HIOPSAIUsageSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var ledger = await LoadAsync(cancellationToken);
            EnsureCurrentMonth(ledger);
            await SaveAsync(ledger, cancellationToken);
            return ToSnapshot(ledger);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<HIOPSAIUsageLedger> LoadAsync(CancellationToken cancellationToken)
    {
        var path = ResolveLedgerPath();
        if (!File.Exists(path))
        {
            return HIOPSAIUsageLedger.Create(CurrentMonthKey());
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<HIOPSAIUsageLedger>(stream, JsonOptions, cancellationToken)
               ?? HIOPSAIUsageLedger.Create(CurrentMonthKey());
    }

    private async Task SaveAsync(HIOPSAIUsageLedger ledger, CancellationToken cancellationToken)
    {
        var path = ResolveLedgerPath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, ledger, JsonOptions, cancellationToken);
    }

    private string ResolveLedgerPath()
    {
        if (Path.IsPathRooted(_options.UsageLedgerPath))
        {
            return _options.UsageLedgerPath;
        }

        return Path.Combine(AppContext.BaseDirectory, _options.UsageLedgerPath);
    }

    private void EnsureCurrentMonth(HIOPSAIUsageLedger ledger)
    {
        var currentMonth = CurrentMonthKey();
        if (string.Equals(ledger.MonthKey, currentMonth, StringComparison.Ordinal))
        {
            return;
        }

        ledger.MonthKey = currentMonth;
        ledger.SpentUsd = 0m;
        ledger.ReservedUsd = 0m;
        ledger.CallsThisMonth = 0;
        ledger.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _reservations.Clear();
    }

    private HIOPSAIUsageSnapshot ToSnapshot(HIOPSAIUsageLedger ledger)
        => new(
            ledger.MonthKey,
            ledger.SpentUsd,
            ledger.ReservedUsd,
            _options.MonthlyBudgetUsd,
            _options.BudgetWarningUsd,
            ledger.CallsThisMonth,
            ledger.UpdatedAtUtc);

    private static string CurrentMonthKey()
        => DateTimeOffset.UtcNow.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
}

public sealed record HIOPSAIUsageReservation(
    bool Allowed,
    string? BlockedReason,
    Guid ReservationId,
    decimal EstimatedCostUsd,
    decimal MonthlySpentUsd,
    decimal MonthlyReservedUsd,
    decimal MonthlyBudgetUsd)
{
    public static HIOPSAIUsageReservation Blocked(string reason, decimal monthlyBudgetUsd)
        => Blocked(reason, 0m, 0m, monthlyBudgetUsd);

    public static HIOPSAIUsageReservation Blocked(
        string reason,
        decimal monthlySpentUsd,
        decimal monthlyReservedUsd,
        decimal monthlyBudgetUsd)
        => new(
            Allowed: false,
            BlockedReason: reason,
            ReservationId: Guid.Empty,
            EstimatedCostUsd: 0m,
            MonthlySpentUsd: monthlySpentUsd,
            MonthlyReservedUsd: monthlyReservedUsd,
            MonthlyBudgetUsd: monthlyBudgetUsd);
}

public sealed record HIOPSAIUsageSnapshot(
    string MonthKey,
    decimal MonthlySpentUsd,
    decimal MonthlyReservedUsd,
    decimal MonthlyBudgetUsd,
    decimal BudgetWarningUsd,
    int CallsThisMonth,
    DateTimeOffset UpdatedAtUtc);

internal sealed class HIOPSAIUsageLedger
{
    public string MonthKey { get; set; } = string.Empty;
    public decimal SpentUsd { get; set; }
    public decimal ReservedUsd { get; set; }
    public int CallsThisMonth { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public static HIOPSAIUsageLedger Create(string monthKey)
        => new()
        {
            MonthKey = monthKey,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
}
