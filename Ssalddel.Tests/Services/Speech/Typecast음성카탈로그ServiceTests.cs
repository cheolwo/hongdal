using Ssalddel.Domain.Speech;
using Ssalddel.Services.External.Typecast;
using Ssalddel.Services.Speech;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Speech;

public sealed class Typecast음성카탈로그ServiceTests
{
    [Fact]
    public async Task 동기화Async_추가수정비활성화를_한번에_반영한다()
    {
        var 유지음성 = new Typecast음성
        {
            VoiceId = "tc_keep",
            이름 = "이전 이름",
            성별 = "female",
            연령대 = "young_adult",
            음성유형 = "original",
            지원모델 =
            {
                new Typecast음성모델 { 버전 = "ssfm-v21", 지원감정Json = "[\"normal\"]" }
            },
            용도 =
            {
                new Typecast음성용도 { 이름 = "Ads" }
            }
        };
        var 사라진음성 = new Typecast음성
        {
            VoiceId = "tc_removed",
            이름 = "사라진 음성",
            활성화여부 = true
        };
        var 저장소 = new Fake저장소(유지음성, 사라진음성);
        var client = new FakeClient(
            new Typecast음성응답(
                "tc_keep",
                "새 이름",
                [new Typecast음성모델응답("ssfm-v30", ["happy", "normal"])],
                "female",
                "young_adult",
                ["E-learning"],
                "original"),
            new Typecast음성응답(
                "tc_new",
                "새 음성",
                [new Typecast음성모델응답("ssfm-v30", ["normal"])],
                "male",
                "middle_age",
                ["News"],
                "original"));
        var sut = CreateService(client, 저장소);

        var result = await sut.동기화Async(CancellationToken.None);

        Assert.True(result.실행됨);
        Assert.Equal(2, result.수신수);
        Assert.Equal(1, result.추가수);
        Assert.Equal(1, result.수정수);
        Assert.Equal(1, result.비활성화수);
        Assert.Equal("새 이름", 유지음성.이름);
        Assert.Equal("ssfm-v30", Assert.Single(유지음성.지원모델).버전);
        Assert.Equal("E-learning", Assert.Single(유지음성.용도).이름);
        Assert.False(사라진음성.활성화여부);
        Assert.Contains(저장소.Items, x => x.VoiceId == "tc_new" && x.활성화여부);
        Assert.Equal(1, 저장소.SaveCount);
    }

    [Fact]
    public async Task 동기화Async_원격목록이_비어있으면_기존카탈로그를_보호한다()
    {
        var 기존 = new Typecast음성 { VoiceId = "tc_keep", 활성화여부 = true };
        var 저장소 = new Fake저장소(기존);
        var sut = CreateService(new FakeClient(), 저장소);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.동기화Async(CancellationToken.None));

        Assert.Contains("비어 있어", exception.Message);
        Assert.True(기존.활성화여부);
        Assert.Equal(0, 저장소.SaveCount);
    }

    private static Typecast음성카탈로그Service CreateService(
        ITypecastClient client,
        ITypecast음성카탈로그저장소 저장소)
        => new(client, 저장소, Options.Create(new TypecastOptions
        {
            Enabled = true,
            ApiKey = "test-key"
        }));

    private sealed class FakeClient : ITypecastClient
    {
        private readonly IReadOnlyList<Typecast음성응답> _items;

        public FakeClient(params Typecast음성응답[] items)
        {
            _items = items;
        }

        public Task<IReadOnlyList<Typecast음성응답>> 음성목록조회Async(
            Typecast음성조회필터? 필터,
            CancellationToken cancellationToken)
            => Task.FromResult(_items);

        public Task<Typecast음성합성결과> 음성합성Async(
            Typecast음성합성요청 요청,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class Fake저장소 : ITypecast음성카탈로그저장소
    {
        public Fake저장소(params Typecast음성[] items)
        {
            Items.AddRange(items);
        }

        public List<Typecast음성> Items { get; } = [];

        public int SaveCount { get; private set; }

        public Task<List<Typecast음성>> 전체추적조회Async(CancellationToken cancellationToken)
            => Task.FromResult(Items.ToList());

        public Task<IReadOnlyList<Typecast음성>> 검색Async(
            Typecast음성카탈로그검색조건 조건,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Typecast음성>>(Items.ToArray());

        public Task<Typecast음성?> 단건조회Async(string voiceId, CancellationToken cancellationToken)
            => Task.FromResult(Items.SingleOrDefault(x => x.VoiceId == voiceId));

        public void 추가(Typecast음성 음성)
            => Items.Add(음성);

        public Task 저장Async(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
