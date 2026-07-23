using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매주문자집단화EngineTests
{
    private readonly 공동구매주문자집단화Engine _engine = new();

    [Fact]
    public void 자동집단Id는_상품배송권온도물류방식을_정규화해_생성한다()
    {
        var first = Command("orderer-1", "source-1");
        first.상품키 = " Pork Belly ";
        first.배송권키 = "APT 101";
        first.온도코드 = "Frozen";
        first.물류방식 = "LCL";
        var second = Command("orderer-2", "source-2");
        second.상품키 = "pork-belly";
        second.배송권키 = "apt-101";
        second.온도코드 = "frozen";
        second.물류방식 = "lcl";

        Assert.Equal(_engine.자동집단Id생성(first), _engine.자동집단Id생성(second));
    }

    [Fact]
    public void 같은주문자의_복수수요는_참여자한명으로_계산한다()
    {
        var progress = _engine.진행계산(
        [
            Demand("orderer-1", "source-1", 20, paid: true),
            Demand("orderer-1", "source-2", 20, paid: true)
        ], null, null);

        Assert.Equal(2, progress.수요건수);
        Assert.Equal(2, progress.예약결제건수);
        Assert.Equal(1, progress.참여자수);
        Assert.Equal(1, progress.예약결제참여자수);
        Assert.Equal(공동구매자동집단상태코드.수요수집중, progress.현재상태);
    }

    [Fact]
    public void 기존집단에_새주문자가_합류하면_예상진행과_근거를반환한다()
    {
        var command = Command("orderer-2", "source-2");
        command.희망수량 = 3;
        command.목표참여자수 = 2;
        command.목표수량 = 5;
        var groupId = _engine.자동집단Id생성(command);
        var existingDemand = Demand("orderer-1", "source-1", 2);
        existingDemand.목표참여자수 = 2;
        existingDemand.목표수량 = 5;
        var existingGroup = new 공동구매자동집단응답
        {
            자동집단Id = groupId,
            현재상태 = 공동구매자동집단상태코드.수요수집중,
            목표참여자수 = 2,
            목표수량 = 5,
            수요목록 = [existingDemand]
        };

        var preview = _engine.배치미리보기(command, existingGroup);

        Assert.Equal(공동구매자동집단배치유형코드.기존집단, preview.배치유형);
        Assert.False(preview.기존수요갱신여부);
        Assert.Equal(6, preview.적용기준목록.Count);
        Assert.Contains(
            preview.적용기준목록,
            item => item.기준코드 == 공동구매자동집단배치기준코드.거래유형);
        Assert.Contains(
            preview.적용기준목록,
            item => item.기준코드 == 공동구매자동집단배치기준코드.가격표시기준);
        Assert.Equal(1, preview.현재진행.참여자수);
        Assert.Equal(2, preview.예상진행.참여자수);
        Assert.Equal(5, preview.예상진행.총희망수량);
        Assert.Equal(공동구매자동집단상태코드.확정대기, preview.예상진행.현재상태);
        Assert.Equal(공동구매자동집단다음단계코드.확정검토, preview.예상진행.다음단계코드);
    }

    [Fact]
    public void 같은수요출처를_다시미리보면_수요를추가하지않고_교체한다()
    {
        var command = Command("orderer-1", "source-1");
        command.희망수량 = 5;
        var groupId = _engine.자동집단Id생성(command);
        var existingGroup = new 공동구매자동집단응답
        {
            자동집단Id = groupId,
            수요목록 = [Demand("orderer-1", "source-1", 2)]
        };

        var preview = _engine.배치미리보기(command, existingGroup);

        Assert.True(preview.기존수요갱신여부);
        Assert.Equal(1, preview.예상진행.수요건수);
        Assert.Equal(1, preview.예상진행.참여자수);
        Assert.Equal(5, preview.예상진행.총희망수량);
    }

    [Fact]
    public void 다른주문자는_기존수요출처키를_재사용할수없다()
    {
        var command = Command("orderer-2", "source-1");
        var existingGroup = new 공동구매자동집단응답
        {
            자동집단Id = _engine.자동집단Id생성(command),
            수요목록 = [Demand("orderer-1", "source-1", 2)]
        };

        var error = Assert.Throws<InvalidOperationException>(
            () => _engine.배치미리보기(command, existingGroup));

        Assert.Contains("다른 주문자", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void 모집기한이지나고조건이미달이면_종료상태와재모집안내를반환한다()
    {
        var deadline = new DateTime(2026, 7, 20, 3, 0, 0, DateTimeKind.Utc);

        var progress = _engine.진행계산(
            [Demand("orderer-1", "source-1", 2)],
            목표참여자수: 5,
            목표수량: 30,
            모집종료시각Utc: deadline,
            기준시각Utc: deadline.AddMinutes(1));

        Assert.Equal(공동구매자동집단상태코드.모집종료목표미달, progress.현재상태);
        Assert.True(progress.모집종료여부);
        Assert.False(progress.모집조건충족여부);
        Assert.False(progress.확정검토가능);
        Assert.Equal(공동구매자동집단다음단계코드.모집종료, progress.다음단계코드);
        Assert.Contains("새 모집 회차", progress.안내, StringComparison.Ordinal);
    }

    [Fact]
    public void 모집기한이지났어도조건을충족했다면_확정검토대기를유지한다()
    {
        var deadline = new DateTime(2026, 7, 20, 3, 0, 0, DateTimeKind.Utc);

        var progress = _engine.진행계산(
        [
            Demand("orderer-1", "source-1", 3),
            Demand("orderer-2", "source-2", 3)
        ],
            목표참여자수: 2,
            목표수량: 5,
            모집종료시각Utc: deadline,
            기준시각Utc: deadline.AddMinutes(1));

        Assert.Equal(공동구매자동집단상태코드.확정대기, progress.현재상태);
        Assert.True(progress.모집종료여부);
        Assert.True(progress.모집조건충족여부);
        Assert.True(progress.확정검토가능);
    }

    [Fact]
    public void 기존B2C자동집단Id는_거래문맥확장후에도_유지한다()
    {
        var command = Command("orderer-1", "source-1");

        var legacyId = 공동구매자동집단화계획기.자동집단키생성(
            command.상품키,
            command.배송권키,
            command.온도코드,
            command.물류방식);

        Assert.Equal(legacyId, _engine.자동집단Id생성(command));
    }

    [Fact]
    public void 같은상품과배송권이어도_B2B와B2C는_다른집단으로_배치한다()
    {
        var consumer = Command("orderer-1", "consumer-source");
        var business = Command("orderer-1", "business-source");
        business.거래유형 = 공동구매거래유형코드.B2B;
        business.가격표시기준 = 공동구매가격표시기준코드.부가세별도;
        business.구매조직표시명 = "동네마트";

        Assert.NotEqual(
            _engine.자동집단Id생성(consumer),
            _engine.자동집단Id생성(business));
    }

    [Fact]
    public void B2B는_가격기준과수량단위가_다르면_다른집단으로_배치한다()
    {
        var vatExcluded = Command("orderer-1", "business-source-1");
        vatExcluded.거래유형 = 공동구매거래유형코드.B2B;
        vatExcluded.가격표시기준 = 공동구매가격표시기준코드.부가세별도;
        vatExcluded.구매조직표시명 = "동네마트";
        var vatIncluded = Command("orderer-2", "business-source-2");
        vatIncluded.거래유형 = 공동구매거래유형코드.B2B;
        vatIncluded.가격표시기준 = 공동구매가격표시기준코드.부가세포함;
        vatIncluded.구매조직표시명 = "지역식당";
        var packageUnit = Command("orderer-3", "business-source-3");
        packageUnit.거래유형 = 공동구매거래유형코드.B2B;
        packageUnit.가격표시기준 = 공동구매가격표시기준코드.부가세별도;
        packageUnit.구매조직표시명 = "지역급식소";
        packageUnit.수량단위 = "box";

        Assert.NotEqual(
            _engine.자동집단Id생성(vatExcluded),
            _engine.자동집단Id생성(vatIncluded));
        Assert.NotEqual(
            _engine.자동집단Id생성(vatExcluded),
            _engine.자동집단Id생성(packageUnit));
    }

    [Fact]
    public void B2B수요는_구매조직정보가_없으면_거부한다()
    {
        var command = Command("orderer-1", "business-source");
        command.거래유형 = 공동구매거래유형코드.B2B;

        var error = Assert.Throws<InvalidOperationException>(
            () => _engine.배치미리보기(command, null));

        Assert.Contains("구매 조직", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void B2B는_명시한한개구매조직과수량목표를_충족하면_확정검토대기다()
    {
        var progress = _engine.진행계산(
            [Demand("orderer-1", "business-source", 30)],
            목표참여자수: 1,
            목표수량: 30,
            거래유형: 공동구매거래유형코드.B2B);

        Assert.Equal(공동구매자동집단상태코드.확정대기, progress.현재상태);
        Assert.True(progress.확정검토가능);
    }

    private static 공동구매자동수요등록Command Command(string ordererId, string sourceKey)
        => new()
        {
            수요출처키 = sourceKey,
            상품키 = "apple",
            상품명 = "사과",
            온도코드 = "상온",
            물류방식 = "LCL",
            주문자키 = ordererId,
            배송권키 = "seoul-mapogu",
            희망수량 = 1,
            수량단위 = "kg"
        };

    private static 공동구매자동수요응답 Demand(
        string ordererId,
        string sourceKey,
        decimal quantity,
        bool paid = false)
        => new()
        {
            수요Id = $"demand-{sourceKey}",
            수요출처키 = sourceKey,
            주문자키 = ordererId,
            수요유형 = paid
                ? 공동구매자동수요유형코드.예약결제
                : 공동구매자동수요유형코드.관심표시,
            결제상태 = paid
                ? 공동구매자동결제상태코드.예약됨
                : 공동구매자동결제상태코드.미결제,
            희망수량 = quantity,
            수량단위 = "kg"
        };
}
