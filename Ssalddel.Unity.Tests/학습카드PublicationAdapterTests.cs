using System.Text.Json;
using Ssalddel.Unity.Learning;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 학습카드PublicationAdapterTests
{
    [Fact]
    public async Task CARD_BIZ_1_승인카드가없으면빈Catalog를정상상태로반환한다()
    {
        var useCase = UseCase(new 학습카드PublicationCatalogApiModel
        {
            SchemaVersion = 학습카드PublicationContract.CatalogSchemaVersion,
        });

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CARD_BIZ_1_미승인카드가Api응답에섞이면FailClosed한다()
    {
        var source = ApprovedFool();
        source.ReviewStatus = 1;
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);
        var useCase = UseCase(new 학습카드PublicationCatalogApiModel
        {
            SchemaVersion = 학습카드PublicationContract.CatalogSchemaVersion,
            Items = new[] { source },
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(CancellationToken.None));

        Assert.Equal("LearningCardPublicationInvalid", error.Message);
    }

    [Fact]
    public async Task CARD_BIZ_1_승인Catalog는검증후저녁학당ReadModel로반환한다()
    {
        var source = ApprovedFool();
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);
        var useCase = UseCase(new 학습카드PublicationCatalogApiModel
        {
            SchemaVersion = 학습카드PublicationContract.CatalogSchemaVersion,
            Items = new[] { source },
        });

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        var card = Assert.Single(result);
        Assert.Equal("learning:hongik.fool.beginner-mind", card.Content.StableId);
        Assert.Equal("BeginnerMind", card.Content.GrantedRuleCode);
        Assert.Equal(source.PublicationHash, card.PublicationHash);
    }

    [Fact]
    public async Task CARD_BIZ_1_같은StableId와Revision중복을거부한다()
    {
        var source = ApprovedFool();
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);
        var useCase = UseCase(new 학습카드PublicationCatalogApiModel
        {
            SchemaVersion = 학습카드PublicationContract.CatalogSchemaVersion,
            Items = new[] { source, source },
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(CancellationToken.None));

        Assert.Equal("LearningCardPublicationRevisionDuplicated", error.Message);
    }

    [Fact]
    public void CARD_BIZ_0_게시계약을JSON으로읽고저녁학당콘텐츠로투영한다()
    {
        var source = ApprovedFool();
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);
        Assert.Equal(
            "1e17e8ad1801d68dbebbd182a208bf3079c7e2596203acef00111f4df8feec17",
            source.PublicationHash);
        var json = JsonSerializer.Serialize(source);
        var wire = JsonSerializer.Deserialize<학습카드PublicationApiModel>(json);

        var result = new 학습카드PublicationAdapter().Map(Assert.IsType<학습카드PublicationApiModel>(wire));

        Assert.Equal("learning:hongik.fool.beginner-mind", result.Content.StableId);
        Assert.Equal("Awareness", result.Content.TargetStatCode);
        Assert.Equal("BeginnerMind", result.Content.GrantedRuleCode);
        Assert.Equal("community-public", result.Image.ContainerName);
        Assert.Equal("hakdang/tarot/cards/major-00.jpg", result.Image.ObjectName);
        Assert.Contains("publication:" + source.PublicationHash, result.Content.SourceStableIds);
    }

    [Fact]
    public void CARD_BIZ_0_게시후내용이변조되면거부한다()
    {
        var source = ApprovedFool();
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);
        source.Interpretation = "근거 없이 바뀐 설명";

        var error = Assert.Throws<InvalidOperationException>(
            () => new 학습카드PublicationAdapter().Map(source));

        Assert.Equal("LearningCardPublicationHashMismatch", error.Message);
    }

    [Fact]
    public void CARD_BIZ_0_일반타로의미가학당효과근거를대체하면거부한다()
    {
        var source = ApprovedFool();
        source.Effect.BasisCode = "GeneralTarotMeaning";
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);

        var error = Assert.Throws<InvalidOperationException>(
            () => new 학습카드PublicationAdapter().Map(source));

        Assert.Equal("LearningCardPublicationEffectInvalid", error.Message);
    }

    [Fact]
    public void CARD_BIZ_0_Blob객체경로대신임의URL을넣으면거부한다()
    {
        var source = ApprovedFool();
        source.Image.ObjectName = "https://example.test/major-00.jpg";
        source.PublicationHash = 학습카드PublicationAdapter.ComputeHash(source);

        var error = Assert.Throws<InvalidOperationException>(
            () => new 학습카드PublicationAdapter().Map(source));

        Assert.Equal("LearningCardPublicationImageInvalid", error.Message);
    }

    private static 학습카드PublicationApiModel ApprovedFool()
        => new()
        {
            SchemaVersion = 학습카드PublicationContract.SchemaVersion,
            LearningContentStableId = "learning:hongik.fool.beginner-mind",
            ContentRevision = 1,
            ArcanaStableId = "tarot-card:major-00",
            Title = "바보 — 모르는 마음",
            KeyPhrase = "분별을 미리 굳히지 않고 모르는 마음으로 묻는다.",
            Interpretation = "행동을 포기하는 무지가 아니라 선입견을 비우고 직접 검증하는 초심이다.",
            ReflectionPrompt = "오늘 이미 안다고 단정한 것은 무엇인가?",
            ReviewStatus = 2,
            AudioReviewStatus = 1,
            HongikAcademySource = new 학습카드SourceProvenanceApiModel
            {
                YoutubeVideoId = "qo1tNkwSBVs",
                StartMilliseconds = 5_181_000,
                EndMilliseconds = 5_365_000,
                CorePassage = "모르는 마음으로 길을 출발하고 실제 행동으로 검증한다.",
                SourceAnalysisId = "analysis:tarot.major-00.v1",
                EvidenceSegmentIds = new[]
                {
                    "transcript-segment:1102",
                    "transcript-segment:1103",
                },
            },
            GeneralMeaning = new 학습카드GeneralMeaningApiModel
            {
                SourceUri = "https://example.test/general-tarot/fool",
                Revision = 1,
                Summary = "일반 타로 참고 해석이며 학당 게임 효과의 근거가 아니다.",
                ReviewStatus = 2,
            },
            Image = new 학습카드ImageBlobApiModel
            {
                ContainerName = "community-public",
                ObjectName = "hakdang/tarot/cards/major-00.jpg",
                Sha256 = "6390bdc08c0c70a5126e3d0feae4e90d962ea5afab5e9c175ecbb78574dcfdae",
                ContentType = "image/jpeg",
                ByteLength = 329_126,
                SourceUri = "https://github.com/yunruse/tarot/blob/de7fac547e15f6b210f73f30e58df0d93c212727/cards/color/0.jpg",
                LicenseCode = "CC0-1.0",
            },
            Effect = new 학습카드EffectApiModel
            {
                BasisCode = 학습카드PublicationContract.HongikAcademyEffectBasis,
                Revision = 1,
                TargetStatCode = "Awareness",
                StatDelta = 1,
                GrantedRuleCode = "BeginnerMind",
                Rationale = "학당 자막의 모르는 마음·직접 검증 취지를 긍정 효과로 번역한다.",
            },
            EditorialReviewStableId = "notion-review:tarot-card.major-00",
            ApprovedBy = "reviewer:test",
            PublishedAtUtc = new DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero),
        };

    private static 저녁학당승인카드조회UseCase UseCase(
        학습카드PublicationCatalogApiModel response)
        => new(new 학습카드PublicationApiRepository(
            new FixedClient(response),
            new 학습카드PublicationAdapter()));

    private sealed class FixedClient(학습카드PublicationCatalogApiModel response)
        : I학습카드PublicationApiClient
    {
        public Task<학습카드PublicationCatalogApiModel> GetCatalogAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(response);
    }
}
