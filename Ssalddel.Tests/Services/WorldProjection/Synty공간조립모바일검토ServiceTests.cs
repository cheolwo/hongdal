using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Ssalddel.Controllers.Platform;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.Storage;
using Ssalddel.Services.WorldProjection;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.WorldProjection;

public sealed class Synty공간조립모바일검토ServiceTests
{
    [Fact]
    public async Task 휴대폰좋음판단은_최종Scene승인이아닌_검토후보로만기록한다()
    {
        var service = CreateService();
        var registered = await service.Batch등록Async(CreateBatch());

        var result = await service.결정기록Async(
            registered.Items[0].ReviewItemStableId,
            Decision(registered.Items[0].Revision, Synty공간조립검토결정Codes.Good, "decision-good"),
            "admin-1",
            "공간 검토자");

        Assert.Equal(Synty공간조립검토상태Codes.ReviewedCandidate, result.ReviewStateCode);
        Assert.Equal(2, result.Revision);
        var history = Assert.Single(result.History);
        Assert.Equal(Synty공간조립검토EventCodes.MobileDecision, history.EventCode);
        Assert.Equal(Synty공간조립검토결정Codes.Good, history.DecisionCode);
        Assert.DoesNotContain("ApprovedForSceneApply", result.ReviewStateCode, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 같은멱등키재전송은_revision과이력을늘리지않는다()
    {
        var service = CreateService();
        var item = (await service.Batch등록Async(CreateBatch())).Items[0];
        var request = Decision(item.Revision, Synty공간조립검토결정Codes.CompareCandidate, "decision-same");

        var first = await service.결정기록Async(
            item.ReviewItemStableId,
            request,
            "admin-1",
            "공간 검토자");
        var repeated = await service.결정기록Async(
            item.ReviewItemStableId,
            request,
            "admin-1",
            "공간 검토자");

        Assert.Equal(first.Revision, repeated.Revision);
        Assert.Single(repeated.History);
    }

    [Fact]
    public async Task 오래된예상개정의판단은_충돌로거부한다()
    {
        var service = CreateService();
        var item = (await service.Batch등록Async(CreateBatch())).Items[0];
        await service.결정기록Async(
            item.ReviewItemStableId,
            Decision(item.Revision, Synty공간조립검토결정Codes.OnHold, "decision-1"),
            "admin-1",
            "공간 검토자");

        var exception = await Assert.ThrowsAsync<Synty공간조립검토ConcurrencyException>(() =>
            service.결정기록Async(
                item.ReviewItemStableId,
                Decision(item.Revision, Synty공간조립검토결정Codes.Good, "decision-2"),
                "admin-1",
                "공간 검토자"));

        Assert.Equal(2, exception.CurrentRevision);
    }

    [Fact]
    public async Task 검토뒤Unity입력Hash가바뀌면_기존판단은Stale이된다()
    {
        var service = CreateService();
        var firstBatch = CreateBatch();
        var item = (await service.Batch등록Async(firstBatch)).Items[0];
        await service.결정기록Async(
            item.ReviewItemStableId,
            Decision(item.Revision, Synty공간조립검토결정Codes.Good, "decision-1"),
            "admin-1",
            "공간 검토자");

        var changedBatch = CreateBatch();
        changedBatch.BatchRevision = "capture-r2";
        changedBatch.Items[0].PlanHash = Hash('b');
        var changed = await service.Batch등록Async(changedBatch);

        Assert.Equal(1, changed.StaleCount);
        Assert.Equal(Synty공간조립검토상태Codes.Stale, changed.Items[0].ReviewStateCode);
        Assert.Equal(3, changed.Items[0].Revision);
        Assert.Contains(changed.Items[0].History, history =>
            history.EventCode == Synty공간조립검토EventCodes.SourceUpdated);
    }

    [Fact]
    public async Task 수정필요판단에는_문제꼬리표나메모가필요하다()
    {
        var service = CreateService();
        var item = (await service.Batch등록Async(CreateBatch())).Items[0];
        var request = Decision(item.Revision, Synty공간조립검토결정Codes.NeedsRevision, "decision-revise");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.결정기록Async(
                item.ReviewItemStableId,
                request,
                "admin-1",
                "공간 검토자"));

        Assert.Contains("문제 꼬리표 또는 메모", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void 모바일검토Api는_서버관리자정책을요구한다()
    {
        var authorize = typeof(Synty공간조립모바일검토Controller)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("서버관리자전용", authorize.Policy);
    }

    [Fact]
    public async Task 촬영업로드는_PNG를재인코딩하고_불변공개경로와두Hash를영수증에기록한다()
    {
        var storage = new RecordingObjectStorageService();
        var uploadStore = new InMemorySynty공간조립검토촬영업로드Store();
        var service = CreateUploadService(storage, uploadStore);
        var png = CreatePng();
        var sourceHash = Sha256(png);
        var command = UploadCommand(
            png,
            Hash('c'),
            string.Empty,
            Hash('a'),
            0,
            sourceHash);

        var first = await service.업로드Async(command);
        var repeated = await service.업로드Async(command);

        Assert.Equal(first.CaptureUploadId, repeated.CaptureUploadId);
        Assert.Equal(1, storage.ImmutableUploadCount);
        Assert.Equal("Local", first.StorageProviderCode);
        Assert.Equal("review-public", first.ContainerName);
        Assert.StartsWith("world-composition-reviews/", first.ObjectName, StringComparison.Ordinal);
        Assert.Equal(sourceHash, first.UploadedSourceSha256);
        Assert.Equal(Sha256(storage.StoredBytes), first.StoredImageSha256);
        Assert.Equal(first.CaptureUploadId, storage.Metadata["captureUploadStableId"]);
        Assert.Equal(first.StoredImageSha256, storage.Metadata["imageSha256"]);
        Assert.True(storage.Metadata.ContainsKey("createdAtUtc"));
        Assert.Equal("image/png", first.ContentType);
        Assert.Equal(1, first.Width);
        Assert.Equal(1, first.Height);
    }

    [Fact]
    public async Task v2수정필요재촬영은_부모묶음_SourceHash_예상개정이맞을때만_ReadyForReview로돌아간다()
    {
        var storage = new RecordingObjectStorageService();
        var uploadStore = new InMemorySynty공간조립검토촬영업로드Store();
        var uploadService = CreateUploadService(storage, uploadStore);
        var ledgerStore = new InMemorySynty공간조립검토원장Store();
        var reviewService = new Synty공간조립모바일검토Service(
            ledgerStore,
            TimeProvider.System,
            uploadStore);
        var png = CreatePng();
        var sourcePngHash = Sha256(png);
        var firstBundle = Hash('c');
        var firstUpload = await uploadService.업로드Async(UploadCommand(
            png,
            firstBundle,
            string.Empty,
            Hash('a'),
            0,
            sourcePngHash));
        var first = await reviewService.Batch등록Async(
            CreateV2Batch(firstUpload, 0, string.Empty, firstBundle, "capture-r1"));
        var needsRevision = Decision(
            first.Items[0].Revision,
            Synty공간조립검토결정Codes.NeedsRevision,
            "decision-recapture");
        needsRevision.Note = "출입구 동선을 다시 보여 주세요.";
        var requested = await reviewService.결정기록Async(
            first.Items[0].ReviewItemStableId,
            needsRevision,
            "admin-1",
            "공간 검토자");

        var secondBundle = Hash('d');
        var secondUpload = await uploadService.업로드Async(UploadCommand(
            png,
            secondBundle,
            firstBundle,
            Hash('a'),
            requested.Revision,
            sourcePngHash));
        var recaptured = await reviewService.Batch등록Async(
            CreateV2Batch(
                secondUpload,
                requested.Revision,
                firstBundle,
                secondBundle,
                "capture-r2"));

        Assert.Equal(0, recaptured.StaleCount);
        Assert.Equal(Synty공간조립검토상태Codes.ReadyForReview, recaptured.Items[0].ReviewStateCode);
        Assert.Equal(3, recaptured.Items[0].Revision);
        Assert.Contains(recaptured.Items[0].History, history =>
            history.EventCode == Synty공간조립검토EventCodes.RecaptureSubmitted);
        var capture = Assert.Single(recaptured.Items[0].Composition.Captures);
        Assert.Equal(secondUpload.ContainerName, capture.ContainerName);
        Assert.Equal(secondUpload.ObjectName, capture.ObjectName);
        Assert.Equal(secondUpload.StoredImageSha256, capture.ImageSha256);
    }

    [Fact]
    public async Task 늦게도착한재촬영은_현재원장예상개정과다르면_충돌한다()
    {
        var uploadStore = new InMemorySynty공간조립검토촬영업로드Store();
        var storage = new RecordingObjectStorageService();
        var uploadService = CreateUploadService(storage, uploadStore);
        var reviewService = new Synty공간조립모바일검토Service(
            new InMemorySynty공간조립검토원장Store(),
            TimeProvider.System,
            uploadStore);
        var png = CreatePng();
        var sourcePngHash = Sha256(png);
        var firstBundle = Hash('c');
        var firstUpload = await uploadService.업로드Async(UploadCommand(
            png, firstBundle, string.Empty, Hash('a'), 0, sourcePngHash));
        var first = await reviewService.Batch등록Async(
            CreateV2Batch(firstUpload, 0, string.Empty, firstBundle, "capture-r1"));
        var hold = await reviewService.결정기록Async(
            first.Items[0].ReviewItemStableId,
            Decision(first.Items[0].Revision, Synty공간조립검토결정Codes.OnHold, "decision-hold"),
            "admin-1",
            "공간 검토자");
        var lateUpload = await uploadService.업로드Async(UploadCommand(
            png, Hash('d'), firstBundle, Hash('a'), first.Items[0].Revision, sourcePngHash));

        var exception = await Assert.ThrowsAsync<Synty공간조립검토ConcurrencyException>(() =>
            reviewService.Batch등록Async(CreateV2Batch(
                lateUpload,
                first.Items[0].Revision,
                firstBundle,
                Hash('d'),
                "capture-late")));

        Assert.Equal(hold.Revision, exception.CurrentRevision);
    }

    private static Synty공간조립모바일검토Service CreateService()
        => new(new InMemorySynty공간조립검토원장Store(), TimeProvider.System);

    private static Synty공간조립검토촬영업로드Service CreateUploadService(
        IObjectStorageService storage,
        ISynty공간조립검토촬영업로드Store uploadStore)
        => new(
            storage,
            uploadStore,
            Options.Create(new ObjectStorageOptions { Provider = ObjectStorageProviderNames.Local }),
            TimeProvider.System);

    private static Synty공간조립검토결정Request Decision(
        long expectedRevision,
        string decisionCode,
        string idempotencyKey)
        => new()
        {
            ExpectedRevision = expectedRevision,
            DecisionCode = decisionCode,
            IdempotencyKey = idempotencyKey
        };

    private static Synty공간조립검토Batch등록Request CreateBatch()
        => new()
        {
            SchemaVersion = Synty공간조립검토SchemaVersions.BatchV1,
            BatchStableId = "review-batch:nature-plants.r1",
            BatchRevision = "capture-r1",
            Title = "심리 영역 발전소 1차 촬영",
            GeneratedAtUtc = new DateTime(2026, 8, 19, 0, 0, 0, DateTimeKind.Utc),
            Items =
            [
                new Synty공간조립검토항목등록Request
                {
                    ReviewItemStableId = "review-item:nature-recovery-plant-a.normal.r1",
                    CompositionStableId = "composition:nature-recovery-plant-a.r1",
                    DisplayName = "회복 발전소 A형",
                    H1StableId = "h1-action:nature-recovery-plant-core.r1",
                    H2StableId = "h2-composition:nature-restoration-recovery.r1",
                    H3StableId = "h3-landscape:nature-threat-recovery.r1",
                    VariantCode = "A",
                    StateProfileCode = "Normal",
                    CompositionInputHash = Hash('a'),
                    PlanHash = Hash('a'),
                    RenderingProfileId = "rendering-profile:mobile-review.r1",
                    RenderingProfileRevision = "r1",
                    RenderingProfileHash = Hash('a'),
                    CaptureBundleHash = Hash('a'),
                    PackUsages =
                    [
                        new() { PackCode = "Nature", UsagePercent = 40, RoleCode = "Lead" },
                        new() { PackCode = "Construction", UsagePercent = 30, RoleCode = "FunctionalLayer" },
                        new() { PackCode = "Farm", UsagePercent = 15, RoleCode = "Support" },
                        new() { PackCode = "Town", UsagePercent = 10, RoleCode = "Support" },
                        new() { PackCode = "City", UsagePercent = 5, RoleCode = "Support" }
                    ],
                    Captures =
                    [
                        new()
                        {
                            CaptureStableId = "capture:nature-recovery-plant-a.normal.hero.r1",
                            ViewCode = "HeroThreeQuarter",
                            DisplayName = "대표 3/4 시점",
                            ImageUrl = "https://example.invalid/reviews/recovery-a-hero.png",
                            ImageSha256 = Hash('a'),
                            Width = 1600,
                            Height = 900
                        }
                    ]
                }
            ]
        };

    private static Synty공간조립검토Batch등록Request CreateV2Batch(
        Synty공간조립검토촬영업로드Response upload,
        long expectedRevision,
        string parentCaptureBundleHash,
        string captureBundleHash,
        string batchRevision)
    {
        var batch = CreateBatch();
        batch.SchemaVersion = Synty공간조립검토SchemaVersions.BatchV2;
        batch.BatchRevision = batchRevision;
        var item = batch.Items[0];
        item.ExpectedRevision = expectedRevision;
        item.ParentCaptureBundleHash = parentCaptureBundleHash;
        item.CaptureBundleHash = captureBundleHash;
        item.Captures =
        [
            new Synty공간조립검토촬영Dto
            {
                CaptureStableId = upload.CaptureStableId,
                ViewCode = upload.ViewCode,
                DisplayName = "대표 3/4 시점",
                CaptureUploadId = upload.CaptureUploadId
            }
        ];
        return batch;
    }

    private static Synty공간조립검토촬영업로드Command UploadCommand(
        byte[] png,
        string captureBundleHash,
        string parentCaptureBundleHash,
        string sourceCompositionHash,
        long expectedRevision,
        string sourcePngHash)
    {
        var stream = new MemoryStream(png, writable: false);
        var file = new FormFile(stream, 0, png.Length, "file", "hero.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        return new Synty공간조립검토촬영업로드Command(
            file,
            "review-batch:nature-plants.r1",
            "review-item:nature-recovery-plant-a.normal.r1",
            "capture:nature-recovery-plant-a.normal.hero.r1",
            "HeroThreeQuarter",
            captureBundleHash,
            parentCaptureBundleHash,
            sourceCompositionHash,
            expectedRevision,
            Hash('a'),
            sourcePngHash,
            1,
            1);
    }

    private static byte[] CreatePng()
    {
        using var bitmap = new SKBitmap(1, 1);
        bitmap.Erase(SKColors.ForestGreen);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Hash(char value) => new(value, 64);

    private sealed class RecordingObjectStorageService : IObjectStorageService
    {
        public int ImmutableUploadCount { get; private set; }
        public byte[] StoredBytes { get; private set; } = [];
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }
            = new Dictionary<string, string>();

        public bool IsConfigured(ObjectStorageAccess access) => true;

        public Task<ObjectStorageUploadResult> UploadAsync(
            Stream stream,
            string originalFileName,
            string? contentType,
            string? folder,
            ObjectStorageAccess access,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public async Task<ObjectStorageUploadResult> UploadImmutableAsync(
            Stream stream,
            string objectName,
            string? contentType,
            ObjectStorageAccess access,
            IReadOnlyDictionary<string, string>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            ImmutableUploadCount++;
            await using var output = new MemoryStream();
            await stream.CopyToAsync(output, cancellationToken);
            StoredBytes = output.ToArray();
            Metadata = metadata ?? new Dictionary<string, string>();
            return new ObjectStorageUploadResult(
                "review-public",
                objectName,
                "https://storage.example.test/review-public/" + objectName,
                "etag-1");
        }

        public Task<byte[]> DownloadAsync(
            string containerName,
            string objectName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(StoredBytes);
    }
}
