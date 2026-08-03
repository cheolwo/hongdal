using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using 살뜰.Data;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.PublicData;

public interface I공동주택공공정보ArchiveService
{
    Task<SelectedApartmentPublicDataArchiveResponse> 수집Async(
        string scopeKey,
        string complexCode,
        string complexName,
        string month,
        CancellationToken cancellationToken = default);
}

public sealed class 공동주택공공정보ArchiveService(
    SsalddelContext dbContext,
    IApartmentComplexLookupService complexLookupService,
    IApartmentManagementFeeLookupService managementFeeLookupService,
    IOptions<PublicDataOptions> options,
    TimeProvider timeProvider) : I공동주택공공정보ArchiveService
{
    private const int ExpectedRequestCount = 5;
    private readonly ApartmentPublicDataArchiveOptions _options = options.Value.ApartmentManagementFee.Archive;

    public async Task<SelectedApartmentPublicDataArchiveResponse> 수집Async(
        string scopeKey,
        string complexCode,
        string complexName,
        string month,
        CancellationToken cancellationToken = default)
    {
        var normalizedMonth = NormalizeMonth(month);
        var normalizedCode = complexCode?.Trim() ?? string.Empty;
        var normalizedName = complexName?.Trim() ?? string.Empty;
        var normalizedScope = scopeKey?.Trim() ?? string.Empty;

        if (!_options.Enabled)
        {
            return Response(
                공동주택공공정보수집상태Codes.비활성,
                normalizedCode,
                normalizedName,
                normalizedMonth,
                error: "PublicData:ApartmentManagementFee:Archive:Enabled가 false입니다.");
        }

        ValidateSelection(normalizedScope, normalizedCode, normalizedName, normalizedMonth);
        var runKey = BuildRunKey(normalizedScope, normalizedCode, normalizedMonth);
        var existingRun = await dbContext.공동주택공공정보수집Runs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.RunKey == runKey, cancellationToken);
        if (existingRun is not null)
        {
            var existingSnapshot = existingRun.SnapshotId is { } snapshotId
                ? await dbContext.공동주택공공정보Snapshots.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == snapshotId, cancellationToken)
                : null;
            return Response(existingRun.StatusCode, normalizedCode, normalizedName, normalizedMonth,
                existingRun.Id, existingSnapshot, reused: existingSnapshot is not null, existingRun.ErrorMessage);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var run = new 공동주택공공정보수집Run
        {
            RunKey = runKey,
            ScopeKey = normalizedScope,
            ComplexCode = normalizedCode,
            ComplexName = normalizedName,
            TargetMonth = normalizedMonth,
            StartedAtUtc = now
        };
        dbContext.공동주택공공정보수집Runs.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_options.MaxComplexesPerRun < 1 || _options.MaxRequestsPerComplex < ExpectedRequestCount)
        {
            return await FailAsync(run,
                $"선택 단지 1곳과 단지당 최대 {ExpectedRequestCount}회 요청을 허용하는 범위 설정이 필요합니다.",
                cancellationToken);
        }

        try
        {
            var basicResult = await complexLookupService.GetBasicInfoAsync(
                new ApartmentComplexBasicRequest { ComplexCode = normalizedCode },
                cancellationToken);
            run.RequestCount++;
            var basic = basicResult.Items.FirstOrDefault();
            if (!basicResult.Success || basic is null)
            {
                return await FailAsync(run,
                    basicResult.ErrorMessage ?? "선택 단지 기본정보를 읽지 못했습니다.",
                    cancellationToken);
            }

            var feeResult = await managementFeeLookupService.GetSnapshotAsync(
                new ApartmentManagementFeeSnapshotRequest
                {
                    ComplexCode = normalizedCode,
                    Month = normalizedMonth
                },
                cancellationToken);
            run.RequestCount += 4;
            var fee = feeResult.Items.FirstOrDefault();
            if (!feeResult.Success || fee is null || fee.LineItems.Count == 0)
            {
                return await FailAsync(run,
                    feeResult.ErrorMessage ?? "선택 단지 관리비 원문 항목을 읽지 못했습니다.",
                    cancellationToken);
            }

            var normalized = new NormalizedApartmentSnapshot(
                "data-go-kr-kapt",
                normalizedMonth,
                new NormalizedApartmentBasic(
                    normalizedCode,
                    string.IsNullOrWhiteSpace(basic.ComplexName) ? normalizedName : basic.ComplexName.Trim(),
                    basic.HouseholdCount,
                    basic.BuildingCount,
                    basic.ManagementType,
                    basic.HeatingType,
                    basic.ApprovalDate,
                    basic.RoadAddress,
                    basic.LegalDongAddress),
                new NormalizedApartmentFee(
                    fee.PublicManagementFeeAmount,
                    fee.IndividualUsageFeeAmount,
                    fee.LongTermRepairReserveMonthlyAmount,
                    fee.EstimatedTotalMonthlyFeeAmount,
                    fee.EstimatedFeePerHousehold,
                    fee.LineItems
                        .OrderBy(x => x.Category, StringComparer.Ordinal)
                        .ThenBy(x => x.Code, StringComparer.Ordinal)
                        .Select(x => new NormalizedApartmentFeeLine(x.Category, x.Code, x.DisplayName, x.Amount))
                        .ToArray()));
            var normalizedJson = JsonSerializer.Serialize(normalized);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedJson))).ToLowerInvariant();
            var snapshot = await dbContext.공동주택공공정보Snapshots
                .SingleOrDefaultAsync(
                    x => x.ComplexCode == normalizedCode && x.TargetMonth == normalizedMonth,
                    cancellationToken);
            var reused = snapshot is not null && string.Equals(snapshot.ContentSha256, hash, StringComparison.Ordinal);
            snapshot ??= new 공동주택공공정보Snapshot
            {
                ComplexCode = normalizedCode,
                TargetMonth = normalizedMonth
            };
            if (snapshot.Id == 0)
            {
                dbContext.공동주택공공정보Snapshots.Add(snapshot);
            }

            snapshot.SourceVersion = normalizedMonth;
            snapshot.SpatialKey = $"kapt:{normalizedCode}";
            snapshot.ComplexName = normalized.Basic.ComplexName;
            snapshot.CollectedAtUtc = now;
            snapshot.ContentSha256 = hash;
            snapshot.NormalizedJson = normalizedJson;
            snapshot.FreshnessStatusCode = "Current";
            snapshot.UpdatedAtUtc = now;
            await dbContext.SaveChangesAsync(cancellationToken);

            run.StatusCode = 공동주택공공정보수집상태Codes.완료;
            run.SnapshotId = snapshot.Id;
            run.CompletedAtUtc = now;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Response(run.StatusCode, normalizedCode, snapshot.ComplexName, normalizedMonth,
                run.Id, snapshot, reused);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await FailAsync(run, ex.Message, cancellationToken);
        }
    }

    private async Task<SelectedApartmentPublicDataArchiveResponse> FailAsync(
        공동주택공공정보수집Run run,
        string error,
        CancellationToken cancellationToken)
    {
        run.StatusCode = 공동주택공공정보수집상태Codes.실패;
        run.ErrorMessage = error.Length <= 1000 ? error : error[..1000];
        run.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Response(run.StatusCode, run.ComplexCode, run.ComplexName, run.TargetMonth,
            run.Id, error: run.ErrorMessage);
    }

    private static void ValidateSelection(string scopeKey, string complexCode, string complexName, string month)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(complexCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(complexName);
        if (month.Length != 6 || !month.All(char.IsDigit))
        {
            throw new ArgumentException("대상 월은 yyyyMM 형식이어야 합니다.", nameof(month));
        }
    }

    private static string NormalizeMonth(string value)
        => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string BuildRunKey(string scopeKey, string complexCode, string month)
    {
        var scopeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scopeKey)))
            .ToLowerInvariant()[..16];
        return $"apartment:{scopeHash}:{complexCode.ToLowerInvariant()}:{month}";
    }

    private static SelectedApartmentPublicDataArchiveResponse Response(
        string status,
        string complexCode,
        string complexName,
        string month,
        long? runId = null,
        공동주택공공정보Snapshot? snapshot = null,
        bool reused = false,
        string? error = null)
        => new()
        {
            StatusCode = status,
            RunId = runId,
            SnapshotId = snapshot?.Id,
            ComplexCode = complexCode,
            ComplexName = complexName,
            Month = month,
            ContentSha256 = snapshot?.ContentSha256,
            CollectedAtUtc = snapshot?.CollectedAtUtc,
            ReusedExistingSnapshot = reused,
            ErrorMessage = error
        };

    private sealed record NormalizedApartmentSnapshot(
        string SourceKey,
        string TargetMonth,
        NormalizedApartmentBasic Basic,
        NormalizedApartmentFee Fee);

    private sealed record NormalizedApartmentBasic(
        string ComplexCode,
        string ComplexName,
        int? HouseholdCount,
        int? BuildingCount,
        string? ManagementType,
        string? HeatingType,
        string? ApprovalDate,
        string? RoadAddress,
        string? LegalDongAddress);

    private sealed record NormalizedApartmentFee(
        decimal PublicManagementFeeAmount,
        decimal IndividualUsageFeeAmount,
        decimal LongTermRepairReserveMonthlyAmount,
        decimal EstimatedTotalMonthlyFeeAmount,
        decimal? EstimatedFeePerHousehold,
        IReadOnlyList<NormalizedApartmentFeeLine> Lines);

    private sealed record NormalizedApartmentFeeLine(
        string Category,
        string Code,
        string DisplayName,
        decimal Amount);
}
