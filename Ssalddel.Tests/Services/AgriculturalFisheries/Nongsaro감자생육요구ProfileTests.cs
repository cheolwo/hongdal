using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Nongsaro감자생육요구ProfileTests
{
    [Fact]
    public async Task NONGSARO_CROP_1_감자콘텐츠와밭농사분류를서로다른식별자로보존한다()
    {
        var sut = new 농사로감자생육요구Profile조회UseCase(new FakeModule());

        var result = await sut.조회Async();

        Assert.Equal("product:potato", result.CanonicalProductStableId);
        Assert.Equal("210005", result.WorkScheduleGroupCode);
        Assert.Equal("밭농사", result.WorkScheduleGroupName);
        Assert.Equal("30699", result.WorkScheduleContentNo);
        Assert.Equal(공통식품품목관계StatusCodes.Unlinked,
            result.NongsaroProductRelationStatusCode);
        Assert.Contains(result.Limitations,
            value => value.Contains("작업군 분류", StringComparison.Ordinal));
    }

    [Fact]
    public async Task NONGSARO_CROP_1_토양물기온햇빛생육작형근거를검토Topic으로만만든다()
    {
        var sut = new 농사로감자생육요구Profile조회UseCase(new FakeModule());

        var result = await sut.조회Async();

        Assert.Equal(6, result.EvidenceTopics.Count);
        Assert.All(result.EvidenceTopics, topic =>
            Assert.Equal(작물생육근거StatusCodes.LocatedNeedsReview, topic.EvidenceStatusCode));
        Assert.Contains(result.EvidenceTopics,
            topic => topic.TopicCode == 작물생육근거TopicCodes.Water
                && topic.ReviewNote.Contains("물수지", StringComparison.Ordinal));
        Assert.Contains(result.EvidenceTopics,
            topic => topic.TopicCode == 작물생육근거TopicCodes.Sunlight
                && topic.ReviewNote.Contains("문맥", StringComparison.Ordinal));
    }

    [Fact]
    public async Task NONGSARO_CROP_1_사람검토전에는SimulationRule게시를차단한다()
    {
        var sut = new 농사로감자생육요구Profile조회UseCase(new FakeModule());

        var result = await sut.조회Async();

        Assert.Equal(작물생육요구검토StatusCodes.PendingHumanReview, result.ReviewStatusCode);
        Assert.False(result.CanPublishSimulationRule);
        Assert.Contains(result.Limitations,
            value => value.Contains("rule revision", StringComparison.Ordinal));
    }

    [Fact]
    public async Task NONGSARO_CROP_1_감자제목을이름유사도로추정하지않는다()
    {
        var module = new FakeModule
        {
            ScheduleTitle = "감자(추정)"
        };
        var sut = new 농사로감자생육요구Profile조회UseCase(module);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.조회Async());

        Assert.Equal("NongsaroPotatoWorkScheduleIdentityInvalid", exception.Message);
        Assert.Equal(0, module.DetailRequestCount);
    }

    [Fact]
    public async Task NONGSARO_CROP_1_상세응답의콘텐츠나작업군이달라지면거부한다()
    {
        var module = new FakeModule
        {
            DetailGroupCode = "210001"
        };
        var sut = new 농사로감자생육요구Profile조회UseCase(module);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.조회Async());

        Assert.Equal("NongsaroPotatoWorkScheduleDetailIdentityInvalid", exception.Message);
    }

    private sealed class FakeModule : I농사로농작업일정Module
    {
        private static readonly DateTimeOffset RetrievedAt =
            DateTimeOffset.Parse("2026-08-11T09:30:00Z");

        public string ScheduleTitle { get; set; } = "감자";
        public string DetailGroupCode { get; set; } = "210005";
        public int DetailRequestCount { get; private set; }

        public Task<Nongsaro공공데이터Response> 작업군조회Async(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Nongsaro공공데이터Response> 일정조회Async(
            string 품목구분Code,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("210005", 품목구분Code);
            return Task.FromResult(Response(
                Nongsaro공공데이터Catalog.농작업일정목록Operation,
                Item(
                    ("cntntsNo", "30699"),
                    ("sj", ScheduleTitle),
                    ("fileName", "감자 농작업일정.hwpx"))));
        }

        public Task<Nongsaro공공데이터Response> 상세조회Async(
            string 콘텐츠번호,
            CancellationToken cancellationToken = default)
        {
            DetailRequestCount++;
            Assert.Equal("30699", 콘텐츠번호);
            return Task.FromResult(Response(
                Nongsaro공공데이터Catalog.농작업일정상세Operation,
                Item(
                    ("cntntsNo", "30699"),
                    ("cntntsSj", "감자"),
                    ("kidofcomdtySeCode", DetailGroupCode),
                    ("kidofcomdtySeCodeNm", "밭농사"),
                    ("cn", "온도 산광 빛 필지 비옥도 밭준비 배토 수확"))));
        }

        public Task<Nongsaro공공데이터Response> 시기정보조회Async(
            string 콘텐츠번호,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("30699", 콘텐츠번호);
            return Task.FromResult(Response(
                Nongsaro공공데이터Catalog.농작업일정시기Operation,
                Item(("htmlCn",
                    "봄재배 여름재배 가을재배 겨울시설재배 관수 배수 가뭄 장마 습해 저온 고온 서리 동해 출현 생육 괴경 수확"))));
        }

        private static Nongsaro공공데이터Response Response(
            string operationName,
            params Nongsaro공공데이터Item[] items)
            => new(
                Nongsaro공공데이터Catalog.농작업일정Service,
                operationName,
                "00",
                "정상",
                RetrievedAt,
                Nongsaro공공데이터Catalog.DocumentationUrl,
                items);

        private static Nongsaro공공데이터Item Item(
            params (string Key, string Value)[] fields)
            => new(fields.ToDictionary(field => field.Key, field => field.Value));
    }
}
