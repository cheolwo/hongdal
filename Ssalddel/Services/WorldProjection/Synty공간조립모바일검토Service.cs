using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Ssalddel.Contracts.Common.WorldProjection;
using 살뜰.Services.Options;

namespace Ssalddel.Services.WorldProjection;

public interface ISynty공간조립검토원장Store
{
    Task<Synty공간조립검토원장Record?> 조회Async(
        string reviewItemStableId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Synty공간조립검토원장Record>> 목록Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default);

    Task<bool> 추가Async(
        Synty공간조립검토원장Record record,
        CancellationToken cancellationToken = default);

    Task<bool> 교체Async(
        Synty공간조립검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default);
}

public interface ISynty공간조립모바일검토Service
{
    Task<Synty공간조립검토Batch등록Response> Batch등록Async(
        Synty공간조립검토Batch등록Request request,
        CancellationToken cancellationToken = default);

    Task<Synty공간조립검토함Response> 검토함조회Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default);

    Task<Synty공간조립검토항목Dto> 결정기록Async(
        string reviewItemStableId,
        Synty공간조립검토결정Request request,
        string reviewerId,
        string reviewerDisplayName,
        CancellationToken cancellationToken = default);
}

public sealed class Synty공간조립모바일검토Service(
    ISynty공간조립검토원장Store store,
    TimeProvider timeProvider,
    ISynty공간조립검토촬영업로드Store? captureUploadStore = null) : ISynty공간조립모바일검토Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Synty공간조립검토Batch등록Response> Batch등록Async(
        Synty공간조립검토Batch등록Request request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var schemaVersion = Require(request.SchemaVersion, nameof(request.SchemaVersion), 80);
        if (!string.Equals(schemaVersion, Synty공간조립검토SchemaVersions.BatchV1, StringComparison.Ordinal)
            && !string.Equals(schemaVersion, Synty공간조립검토SchemaVersions.BatchV2, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"지원하지 않는 검토 batch schema입니다. SchemaVersion={request.SchemaVersion}");
        }

        var batchStableId = Require(request.BatchStableId, nameof(request.BatchStableId), 160);
        var batchRevision = Require(request.BatchRevision, nameof(request.BatchRevision), 120);
        var batchTitle = Require(request.Title, nameof(request.Title), 160);
        if (request.GeneratedAtUtc <= DateTime.UnixEpoch)
        {
            throw new ArgumentException("GeneratedAtUtc에는 Unity 촬영 묶음 생성 시각이 필요합니다.");
        }
        if (request.Items is not { Count: > 0 and <= 100 })
        {
            throw new ArgumentException("한 검토 batch에는 1개 이상 100개 이하 조합물이 필요합니다.");
        }

        var normalizedItems = new List<Synty공간조립검토항목등록Request>(request.Items.Count);
        foreach (var sourceItem in request.Items)
        {
            normalizedItems.Add(await NormalizeItemAsync(
                sourceItem,
                schemaVersion,
                batchStableId,
                cancellationToken));
        }
        if (normalizedItems.Select(item => item.ReviewItemStableId)
            .Distinct(StringComparer.Ordinal).Count() != normalizedItems.Count)
        {
            throw new ArgumentException("한 검토 batch 안에 중복된 ReviewItemStableId가 있습니다.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var createdCount = 0;
        var updatedCount = 0;
        var unchangedCount = 0;
        var staleCount = 0;
        var results = new List<Synty공간조립검토항목Dto>(normalizedItems.Count);

        foreach (var item in normalizedItems)
        {
            var snapshotJson = JsonSerializer.Serialize(item, JsonOptions);
            var snapshotHash = Sha256(snapshotJson);
            var existing = await store.조회Async(item.ReviewItemStableId, cancellationToken);
            if (existing is null)
            {
                if (string.Equals(schemaVersion, Synty공간조립검토SchemaVersions.BatchV2, StringComparison.Ordinal)
                    && (item.ExpectedRevision != 0 || item.ParentCaptureBundleHash.Length != 0))
                {
                    throw new Synty공간조립검토ConcurrencyException(item.ReviewItemStableId, 0);
                }
                var created = new Synty공간조립검토원장Record
                {
                    ReviewItemStableId = item.ReviewItemStableId,
                    BatchStableId = batchStableId,
                    BatchRevision = batchRevision,
                    BatchTitle = batchTitle,
                    Revision = 1,
                    ReviewStateCode = item.Captures.Count == 0
                        ? Synty공간조립검토상태Codes.WaitingForCapture
                        : Synty공간조립검토상태Codes.ReadyForReview,
                    SnapshotHash = snapshotHash,
                    SnapshotJson = snapshotJson,
                    GeneratedAtUtc = request.GeneratedAtUtc.ToUniversalTime(),
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                if (!await store.추가Async(created, cancellationToken))
                {
                    throw new Synty공간조립검토ConcurrencyException(item.ReviewItemStableId, 0);
                }

                createdCount++;
                results.Add(ToDto(created));
                continue;
            }

            if (string.Equals(existing.SnapshotHash, snapshotHash, StringComparison.Ordinal)
                && string.Equals(existing.BatchRevision, batchRevision, StringComparison.Ordinal))
            {
                unchangedCount++;
                results.Add(ToDto(existing));
                continue;
            }

            var expectedRevision = existing.Revision;
            if (string.Equals(schemaVersion, Synty공간조립검토SchemaVersions.BatchV2, StringComparison.Ordinal)
                && item.ExpectedRevision != expectedRevision)
            {
                throw new Synty공간조립검토ConcurrencyException(item.ReviewItemStableId, expectedRevision);
            }
            var hadReviewDecision = existing.History.Any(history =>
                string.Equals(history.EventCode, Synty공간조립검토EventCodes.MobileDecision, StringComparison.Ordinal));
            var previousItem = JsonSerializer.Deserialize<Synty공간조립검토항목등록Request>(
                                   existing.SnapshotJson,
                                   JsonOptions)
                               ?? throw new InvalidDataException("Synty 공간 조립 검토 snapshot이 손상되었습니다.");
            if (string.Equals(schemaVersion, Synty공간조립검토SchemaVersions.BatchV2, StringComparison.Ordinal)
                && !string.Equals(
                    item.ParentCaptureBundleHash,
                    previousItem.CaptureBundleHash,
                    StringComparison.Ordinal))
            {
                throw new Synty공간조립검토ConcurrencyException(item.ReviewItemStableId, expectedRevision);
            }
            var isRequestedRecapture = string.Equals(
                                           existing.ReviewStateCode,
                                           Synty공간조립검토상태Codes.NeedsRevision,
                                           StringComparison.Ordinal)
                                       && item.Captures.Count > 0
                                       && !string.Equals(
                                           previousItem.CaptureBundleHash,
                                           item.CaptureBundleHash,
                                           StringComparison.Ordinal)
                                       && string.Equals(
                                           CompositionBasisHash(previousItem),
                                           CompositionBasisHash(item),
                                           StringComparison.Ordinal);
            existing.BatchStableId = batchStableId;
            existing.BatchRevision = batchRevision;
            existing.BatchTitle = batchTitle;
            existing.Revision++;
            existing.ReviewStateCode = isRequestedRecapture
                ? Synty공간조립검토상태Codes.ReadyForReview
                : hadReviewDecision
                ? Synty공간조립검토상태Codes.Stale
                : item.Captures.Count == 0
                    ? Synty공간조립검토상태Codes.WaitingForCapture
                    : Synty공간조립검토상태Codes.ReadyForReview;
            existing.SnapshotHash = snapshotHash;
            existing.SnapshotJson = snapshotJson;
            existing.GeneratedAtUtc = request.GeneratedAtUtc.ToUniversalTime();
            existing.UpdatedAtUtc = now;
            existing.History.Add(new Synty공간조립검토결정이력Record
            {
                IdempotencyKey = isRequestedRecapture
                    ? $"recapture:{item.CaptureBundleHash}"
                    : $"source:{snapshotHash}",
                EventCode = isRequestedRecapture
                    ? Synty공간조립검토EventCodes.RecaptureSubmitted
                    : Synty공간조립검토EventCodes.SourceUpdated,
                DecisionCode = string.Empty,
                Note = isRequestedRecapture
                    ? "수정 필요 판단에 대한 새 Unity 촬영 묶음이 등록되었습니다."
                    : "Unity 촬영 입력 또는 표현 계획이 변경되었습니다.",
                ReviewerId = "system",
                ReviewerDisplayName = "Unity 촬영 인계",
                DecidedAtUtc = now,
                Revision = existing.Revision
            });

            if (!await store.교체Async(existing, expectedRevision, cancellationToken))
            {
                throw new Synty공간조립검토ConcurrencyException(
                    item.ReviewItemStableId,
                    expectedRevision);
            }

            updatedCount++;
            if (hadReviewDecision && !isRequestedRecapture)
            {
                staleCount++;
            }
            results.Add(ToDto(existing));
        }

        return new Synty공간조립검토Batch등록Response
        {
            BatchStableId = batchStableId,
            BatchRevision = batchRevision,
            CreatedCount = createdCount,
            UpdatedCount = updatedCount,
            UnchangedCount = unchangedCount,
            StaleCount = staleCount,
            Items = results
        };
    }

    public async Task<Synty공간조립검토함Response> 검토함조회Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        var normalizedBatchStableId = Optional(batchStableId, 160);
        var normalizedState = Optional(reviewStateCode, 80);
        if (normalizedState is not null
            && !Synty공간조립검토상태Codes.All.Contains(normalizedState))
        {
            throw new ArgumentException($"알 수 없는 검토 상태입니다. ReviewStateCode={normalizedState}");
        }

        var items = (await store.목록Async(
                normalizedBatchStableId,
                normalizedState,
                Math.Clamp(take, 1, 100),
                cancellationToken))
            .OrderBy(item => ReviewPriority(item.ReviewStateCode))
            .ThenByDescending(item => item.UpdatedAtUtc)
            .Select(ToDto)
            .ToList();

        return new Synty공간조립검토함Response
        {
            TotalCount = items.Count,
            ReadyCount = items.Count(item =>
                item.ReviewStateCode is Synty공간조립검토상태Codes.ReadyForReview
                    or Synty공간조립검토상태Codes.Stale),
            ReviewedCount = items.Count(item =>
                item.ReviewStateCode is Synty공간조립검토상태Codes.ReviewedCandidate
                    or Synty공간조립검토상태Codes.NeedsRevision
                    or Synty공간조립검토상태Codes.OnHold
                    or Synty공간조립검토상태Codes.CompareCandidate),
            Items = items
        };
    }

    public async Task<Synty공간조립검토항목Dto> 결정기록Async(
        string reviewItemStableId,
        Synty공간조립검토결정Request request,
        string reviewerId,
        string reviewerDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var stableId = Require(reviewItemStableId, nameof(reviewItemStableId), 160);
        var normalizedReviewerId = Require(reviewerId, nameof(reviewerId), 160);
        var normalizedReviewerName = Require(reviewerDisplayName, nameof(reviewerDisplayName), 120);
        var idempotencyKey = Require(request.IdempotencyKey, nameof(request.IdempotencyKey), 160);
        var decisionCode = Require(request.DecisionCode, nameof(request.DecisionCode), 80);
        if (!Synty공간조립검토결정Codes.All.Contains(decisionCode))
        {
            throw new ArgumentException($"알 수 없는 모바일 검토 결정입니다. DecisionCode={decisionCode}");
        }

        var issueCodes = (request.IssueCodes ?? [])
            .Select(issue => Require(issue, nameof(request.IssueCodes), 80))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(issue => issue, StringComparer.Ordinal)
            .ToList();
        var unknownIssue = issueCodes.FirstOrDefault(issue =>
            !Synty공간조립검토문제Codes.All.Contains(issue));
        if (unknownIssue is not null)
        {
            throw new ArgumentException($"알 수 없는 검토 문제 꼬리표입니다. IssueCode={unknownIssue}");
        }

        var note = request.Note?.Trim() ?? string.Empty;
        if (note.Length > 500)
        {
            throw new ArgumentException("검토 메모는 500자 이하여야 합니다.");
        }
        if (decisionCode == Synty공간조립검토결정Codes.NeedsRevision
            && issueCodes.Count == 0
            && note.Length == 0)
        {
            throw new ArgumentException("수정 필요 결정에는 문제 꼬리표 또는 메모가 필요합니다.");
        }

        var existing = await store.조회Async(stableId, cancellationToken)
                       ?? throw new KeyNotFoundException(
                           $"모바일 검토 조합물을 찾을 수 없습니다. ReviewItemStableId={stableId}");
        var repeated = existing.History.FirstOrDefault(history =>
            string.Equals(history.IdempotencyKey, idempotencyKey, StringComparison.Ordinal));
        if (repeated is not null)
        {
            if (!string.Equals(repeated.DecisionCode, decisionCode, StringComparison.Ordinal)
                || !repeated.IssueCodes.SequenceEqual(issueCodes, StringComparer.Ordinal)
                || !string.Equals(repeated.Note, note, StringComparison.Ordinal)
                || !string.Equals(repeated.ReviewerId, normalizedReviewerId, StringComparison.Ordinal))
            {
                throw new ArgumentException("같은 IdempotencyKey를 다른 검토 결정에 재사용할 수 없습니다.");
            }
            return ToDto(existing);
        }

        if (request.ExpectedRevision != existing.Revision)
        {
            throw new Synty공간조립검토ConcurrencyException(stableId, existing.Revision);
        }
        if (existing.ReviewStateCode == Synty공간조립검토상태Codes.WaitingForCapture)
        {
            throw new InvalidOperationException("촬영 이미지가 준비되기 전에는 모바일 검토 결정을 기록할 수 없습니다.");
        }

        var expectedRevision = existing.Revision;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        existing.Revision++;
        existing.ReviewStateCode = DecisionState(decisionCode);
        existing.UpdatedAtUtc = now;
        existing.History.Add(new Synty공간조립검토결정이력Record
        {
            IdempotencyKey = idempotencyKey,
            EventCode = Synty공간조립검토EventCodes.MobileDecision,
            DecisionCode = decisionCode,
            IssueCodes = issueCodes,
            Note = note,
            ReviewerId = normalizedReviewerId,
            ReviewerDisplayName = normalizedReviewerName,
            DecidedAtUtc = now,
            Revision = existing.Revision
        });

        if (!await store.교체Async(existing, expectedRevision, cancellationToken))
        {
            throw new Synty공간조립검토ConcurrencyException(stableId, expectedRevision);
        }

        return ToDto(existing);
    }

    private async Task<Synty공간조립검토항목등록Request> NormalizeItemAsync(
        Synty공간조립검토항목등록Request source,
        string schemaVersion,
        string batchStableId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        var reviewItemStableId = Require(source.ReviewItemStableId, nameof(source.ReviewItemStableId), 160);
        var variantCode = Require(source.VariantCode, nameof(source.VariantCode), 8).ToUpperInvariant();
        if (variantCode is not ("A" or "B" or "C"))
        {
            throw new ArgumentException($"공간 변형은 A, B, C 중 하나여야 합니다. VariantCode={variantCode}");
        }

        var packUsages = (source.PackUsages ?? [])
            .Select(pack => new Synty공간조립팩활용Dto
            {
                PackCode = Require(pack.PackCode, nameof(pack.PackCode), 40),
                UsagePercent = pack.UsagePercent,
                RoleCode = Require(pack.RoleCode, nameof(pack.RoleCode), 80)
            })
            .OrderBy(pack => pack.PackCode, StringComparer.Ordinal)
            .ToList();
        var supportedPacks = new HashSet<string>(
            ["Nature", "Farm", "Town", "City", "Construction"],
            StringComparer.Ordinal);
        if (packUsages.Count is < 1 or > 5
            || packUsages.Any(pack => !supportedPacks.Contains(pack.PackCode))
            || packUsages.Select(pack => pack.PackCode).Distinct(StringComparer.Ordinal).Count() != packUsages.Count
            || packUsages.Any(pack => pack.UsagePercent is < 0 or > 100)
            || packUsages.Sum(pack => pack.UsagePercent) != 100)
        {
            throw new ArgumentException("PackUsages는 다섯 주력 팩 중 중복 없이 구성하고 UsagePercent 합계를 100으로 맞춰야 합니다.");
        }

        var renderingProfileHash = RequireSha256(
            source.RenderingProfileHash,
            nameof(source.RenderingProfileHash));
        var sourceCompositionHash = RequireSha256(
            source.CompositionInputHash,
            nameof(source.CompositionInputHash));
        if (source.ExpectedRevision < 0)
        {
            throw new ArgumentException("ExpectedRevision은 0 이상이어야 합니다.");
        }
        var parentCaptureBundleHash = string.IsNullOrWhiteSpace(source.ParentCaptureBundleHash)
            ? string.Empty
            : RequireSha256(source.ParentCaptureBundleHash, nameof(source.ParentCaptureBundleHash));
        var sourceCaptures = source.Captures ?? [];
        var captureBundleHash = sourceCaptures.Count == 0
            ? Optional(source.CaptureBundleHash, 64) ?? string.Empty
            : RequireSha256(source.CaptureBundleHash, nameof(source.CaptureBundleHash));
        var captures = new List<Synty공간조립검토촬영Dto>(sourceCaptures.Count);
        foreach (var capture in sourceCaptures)
        {
            var captureStableId = Require(capture.CaptureStableId, nameof(capture.CaptureStableId), 180);
            var viewCode = Require(capture.ViewCode, nameof(capture.ViewCode), 80);
            var displayName = Require(capture.DisplayName, nameof(capture.DisplayName), 100);
            if (string.Equals(schemaVersion, Synty공간조립검토SchemaVersions.BatchV2, StringComparison.Ordinal))
            {
                if (captureUploadStore is null)
                {
                    throw new InvalidOperationException("v2 촬영 batch를 확인할 업로드 원장이 구성되지 않았습니다.");
                }
                var captureUploadId = Require(
                    capture.CaptureUploadId,
                    nameof(capture.CaptureUploadId),
                    160);
                var uploaded = await captureUploadStore.조회Async(captureUploadId, cancellationToken)
                               ?? throw new ArgumentException(
                                   $"등록된 촬영 업로드 영수증을 찾을 수 없습니다. CaptureUploadId={captureUploadId}");
                if (!string.Equals(uploaded.BatchStableId, batchStableId, StringComparison.Ordinal)
                    || !string.Equals(uploaded.ReviewItemStableId, reviewItemStableId, StringComparison.Ordinal)
                    || !string.Equals(uploaded.CaptureStableId, captureStableId, StringComparison.Ordinal)
                    || !string.Equals(uploaded.ViewCode, viewCode, StringComparison.Ordinal)
                    || !string.Equals(uploaded.CaptureBundleHash, captureBundleHash, StringComparison.Ordinal)
                    || !string.Equals(uploaded.ParentCaptureBundleHash, parentCaptureBundleHash, StringComparison.Ordinal)
                    || !string.Equals(uploaded.SourceCompositionHash, sourceCompositionHash, StringComparison.Ordinal)
                    || uploaded.ExpectedReviewItemRevision != source.ExpectedRevision
                    || !string.Equals(uploaded.RenderingProfileHash, renderingProfileHash, StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        $"촬영 업로드 영수증과 batch 촬영 정보가 일치하지 않습니다. CaptureUploadId={captureUploadId}");
                }
                captures.Add(new Synty공간조립검토촬영Dto
                {
                    CaptureStableId = captureStableId,
                    ViewCode = viewCode,
                    DisplayName = displayName,
                    CaptureUploadId = captureUploadId,
                    StorageProviderCode = uploaded.StorageProviderCode,
                    ContainerName = uploaded.ContainerName,
                    ObjectName = uploaded.ObjectName,
                    ImageUrl = NormalizeImageUrl(uploaded.ImageUrl),
                    ImageSha256 = uploaded.StoredImageSha256,
                    ContentType = uploaded.ContentType,
                    ContentLength = uploaded.ContentLength,
                    ETag = uploaded.ETag,
                    Width = uploaded.Width,
                    Height = uploaded.Height
                });
                continue;
            }

            captures.Add(new Synty공간조립검토촬영Dto
            {
                CaptureStableId = captureStableId,
                ViewCode = viewCode,
                DisplayName = displayName,
                ImageUrl = NormalizeImageUrl(capture.ImageUrl),
                ImageSha256 = RequireSha256(capture.ImageSha256, nameof(capture.ImageSha256)),
                Width = capture.Width,
                Height = capture.Height
            });
        }
        captures = captures.OrderBy(capture => capture.ViewCode, StringComparer.Ordinal).ToList();
        if (captures.Count > 8
            || captures.Any(capture => capture.Width <= 0 || capture.Height <= 0)
            || captures.Select(capture => capture.CaptureStableId).Distinct(StringComparer.Ordinal).Count() != captures.Count
            || captures.Select(capture => capture.ViewCode).Distinct(StringComparer.Ordinal).Count() != captures.Count)
        {
            throw new ArgumentException("촬영 이미지는 최대 8개이며 고유한 CaptureStableId·ViewCode와 양수 크기가 필요합니다.");
        }

        return new Synty공간조립검토항목등록Request
        {
            ExpectedRevision = source.ExpectedRevision,
            ReviewItemStableId = reviewItemStableId,
            CompositionStableId = Require(source.CompositionStableId, nameof(source.CompositionStableId), 160),
            DisplayName = Require(source.DisplayName, nameof(source.DisplayName), 160),
            H1StableId = Require(source.H1StableId, nameof(source.H1StableId), 180),
            H2StableId = Require(source.H2StableId, nameof(source.H2StableId), 180),
            H3StableId = Require(source.H3StableId, nameof(source.H3StableId), 180),
            VariantCode = variantCode,
            StateProfileCode = Require(source.StateProfileCode, nameof(source.StateProfileCode), 100),
            CompositionInputHash = sourceCompositionHash,
            PlanHash = RequireSha256(source.PlanHash, nameof(source.PlanHash)),
            RenderingProfileId = Require(source.RenderingProfileId, nameof(source.RenderingProfileId), 160),
            RenderingProfileRevision = Require(source.RenderingProfileRevision, nameof(source.RenderingProfileRevision), 120),
            RenderingProfileHash = renderingProfileHash,
            ParentCaptureBundleHash = parentCaptureBundleHash,
            CaptureBundleHash = captureBundleHash,
            PackUsages = packUsages,
            Captures = captures
        };
    }

    private static string NormalizeImageUrl(string? value)
    {
        var url = Require(value, "ImageUrl", 2048);
        if (url.StartsWith("/", StringComparison.Ordinal))
        {
            return url;
        }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new ArgumentException("ImageUrl은 사용자 정보가 없는 HTTP(S) 절대 URL 또는 앱 기준 상대 경로여야 합니다.");
        }
        return uri.AbsoluteUri;
    }

    private static string RequireSha256(string? value, string name)
    {
        var normalized = Require(value, name, 64).ToLowerInvariant();
        if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException($"{name}에는 64자리 SHA-256 hex가 필요합니다.");
        }
        return normalized;
    }

    private static string Require(string? value, string name, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0 || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{name}은 1자 이상 {maxLength}자 이하여야 합니다.");
        }
        return normalized;
    }

    private static string? Optional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"선택 문자열은 {maxLength}자 이하여야 합니다.");
        }
        return normalized;
    }

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string CompositionBasisHash(Synty공간조립검토항목등록Request item)
        => Sha256(JsonSerializer.Serialize(new
        {
            item.ReviewItemStableId,
            item.CompositionStableId,
            item.DisplayName,
            item.H1StableId,
            item.H2StableId,
            item.H3StableId,
            item.VariantCode,
            item.StateProfileCode,
            item.CompositionInputHash,
            item.PlanHash,
            item.RenderingProfileId,
            item.RenderingProfileRevision,
            item.RenderingProfileHash,
            PackUsages = item.PackUsages
                .OrderBy(pack => pack.PackCode, StringComparer.Ordinal)
                .Select(pack => new { pack.PackCode, pack.UsagePercent, pack.RoleCode })
        }, JsonOptions));

    private static string DecisionState(string decisionCode)
        => decisionCode switch
        {
            Synty공간조립검토결정Codes.Good => Synty공간조립검토상태Codes.ReviewedCandidate,
            Synty공간조립검토결정Codes.NeedsRevision => Synty공간조립검토상태Codes.NeedsRevision,
            Synty공간조립검토결정Codes.OnHold => Synty공간조립검토상태Codes.OnHold,
            Synty공간조립검토결정Codes.CompareCandidate => Synty공간조립검토상태Codes.CompareCandidate,
            _ => throw new ArgumentOutOfRangeException(nameof(decisionCode))
        };

    private static int ReviewPriority(string stateCode)
        => stateCode switch
        {
            Synty공간조립검토상태Codes.Stale => 0,
            Synty공간조립검토상태Codes.ReadyForReview => 1,
            Synty공간조립검토상태Codes.WaitingForCapture => 3,
            _ => 2
        };

    private static Synty공간조립검토항목Dto ToDto(Synty공간조립검토원장Record record)
        => new()
        {
            ReviewItemStableId = record.ReviewItemStableId,
            BatchStableId = record.BatchStableId,
            BatchRevision = record.BatchRevision,
            BatchTitle = record.BatchTitle,
            Revision = record.Revision,
            ReviewStateCode = record.ReviewStateCode,
            SnapshotHash = record.SnapshotHash,
            GeneratedAtUtc = record.GeneratedAtUtc,
            UpdatedAtUtc = record.UpdatedAtUtc,
            Composition = JsonSerializer.Deserialize<Synty공간조립검토항목등록Request>(
                              record.SnapshotJson,
                              JsonOptions)
                          ?? throw new InvalidDataException("Synty 공간 조립 검토 snapshot이 손상되었습니다."),
            History = record.History.Select(history => new Synty공간조립검토결정이력Dto
            {
                IdempotencyKey = history.IdempotencyKey,
                EventCode = history.EventCode,
                DecisionCode = history.DecisionCode,
                IssueCodes = history.IssueCodes.ToList(),
                Note = history.Note,
                ReviewerDisplayName = history.ReviewerDisplayName,
                DecidedAtUtc = history.DecidedAtUtc,
                Revision = history.Revision
            }).ToList()
        };
}

public sealed class Synty공간조립검토ConcurrencyException(
    string reviewItemStableId,
    long currentRevision)
    : InvalidOperationException(
        $"Synty 공간 조립 검토 원장이 변경되었습니다. ReviewItemStableId={reviewItemStableId}, CurrentRevision={currentRevision}")
{
    public string ReviewItemStableId { get; } = reviewItemStableId;
    public long CurrentRevision { get; } = currentRevision;
}

public sealed class Synty공간조립검토원장Record
{
    [BsonId]
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string BatchRevision { get; set; } = string.Empty;
    public string BatchTitle { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string ReviewStateCode { get; set; } = Synty공간조립검토상태Codes.WaitingForCapture;
    public string SnapshotHash { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<Synty공간조립검토결정이력Record> History { get; set; } = [];
}

public sealed class Synty공간조립검토결정이력Record
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string DecisionCode { get; set; } = string.Empty;
    public List<string> IssueCodes { get; set; } = [];
    public string Note { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerDisplayName { get; set; } = string.Empty;
    public DateTime DecidedAtUtc { get; set; }
    public long Revision { get; set; }
}

internal sealed class MongoSynty공간조립검토원장Store : ISynty공간조립검토원장Store
{
    private const string CollectionName = "synty_composition_review_ledgers";
    private readonly IMongoCollection<Synty공간조립검토원장Record> collection;
    private readonly SemaphoreSlim indexLock = new(1, 1);
    private bool indexesReady;

    public MongoSynty공간조립검토원장Store(
        IMongoClient client,
        IOptions<MongoDbOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }
        collection = client.GetDatabase(options.Value.Database.Trim())
            .GetCollection<Synty공간조립검토원장Record>(CollectionName);
    }

    public async Task<Synty공간조립검토원장Record?> 조회Async(
        string reviewItemStableId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        return await collection.Find(record => record.ReviewItemStableId == reviewItemStableId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Synty공간조립검토원장Record>> 목록Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var filter = Builders<Synty공간조립검토원장Record>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(batchStableId))
        {
            filter &= Builders<Synty공간조립검토원장Record>.Filter.Eq(
                record => record.BatchStableId,
                batchStableId);
        }
        if (!string.IsNullOrWhiteSpace(reviewStateCode))
        {
            filter &= Builders<Synty공간조립검토원장Record>.Filter.Eq(
                record => record.ReviewStateCode,
                reviewStateCode);
        }
        return await collection.Find(filter)
            .SortByDescending(record => record.UpdatedAtUtc)
            .Limit(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> 추가Async(
        Synty공간조립검토원장Record record,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        try
        {
            await collection.InsertOneAsync(record, cancellationToken: cancellationToken);
            return true;
        }
        catch (MongoWriteException exception) when (
            exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task<bool> 교체Async(
        Synty공간조립검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
        => (await collection.ReplaceOneAsync(
                current => current.ReviewItemStableId == record.ReviewItemStableId
                           && current.Revision == expectedRevision,
                record,
                cancellationToken: cancellationToken))
            .ModifiedCount == 1;

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (indexesReady)
        {
            return;
        }
        await indexLock.WaitAsync(cancellationToken);
        try
        {
            if (indexesReady)
            {
                return;
            }
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<Synty공간조립검토원장Record>(
                    Builders<Synty공간조립검토원장Record>.IndexKeys
                        .Ascending(record => record.BatchStableId)
                        .Ascending(record => record.ReviewStateCode)
                        .Descending(record => record.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_batch_state_updated" }),
                cancellationToken: cancellationToken);
            indexesReady = true;
        }
        finally
        {
            indexLock.Release();
        }
    }
}

public sealed class InMemorySynty공간조립검토원장Store : ISynty공간조립검토원장Store
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, Synty공간조립검토원장Record> records = new(StringComparer.Ordinal);
    private readonly object sync = new();

    public Task<Synty공간조립검토원장Record?> 조회Async(
        string reviewItemStableId,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            return Task.FromResult(records.TryGetValue(reviewItemStableId, out var record)
                ? Clone(record)
                : null);
        }
    }

    public Task<IReadOnlyList<Synty공간조립검토원장Record>> 목록Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            IReadOnlyList<Synty공간조립검토원장Record> result = records.Values
                .Where(record => string.IsNullOrWhiteSpace(batchStableId)
                                 || string.Equals(record.BatchStableId, batchStableId, StringComparison.Ordinal))
                .Where(record => string.IsNullOrWhiteSpace(reviewStateCode)
                                 || string.Equals(record.ReviewStateCode, reviewStateCode, StringComparison.Ordinal))
                .OrderByDescending(record => record.UpdatedAtUtc)
                .Take(Math.Clamp(take, 1, 100))
                .Select(Clone)
                .ToArray();
            return Task.FromResult(result);
        }
    }

    public Task<bool> 추가Async(
        Synty공간조립검토원장Record record,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (records.ContainsKey(record.ReviewItemStableId))
            {
                return Task.FromResult(false);
            }
            records[record.ReviewItemStableId] = Clone(record);
            return Task.FromResult(true);
        }
    }

    public Task<bool> 교체Async(
        Synty공간조립검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (!records.TryGetValue(record.ReviewItemStableId, out var current)
                || current.Revision != expectedRevision)
            {
                return Task.FromResult(false);
            }
            records[record.ReviewItemStableId] = Clone(record);
            return Task.FromResult(true);
        }
    }

    private static Synty공간조립검토원장Record Clone(Synty공간조립검토원장Record record)
        => JsonSerializer.Deserialize<Synty공간조립검토원장Record>(
               JsonSerializer.Serialize(record, JsonOptions),
               JsonOptions)!;
}
