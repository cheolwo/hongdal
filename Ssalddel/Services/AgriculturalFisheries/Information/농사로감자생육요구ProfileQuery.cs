using Ssalddel.Contracts.Common.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I농사로감자생육요구Profile조회UseCase
{
    Task<농사로작물생육요구ProfileResponse> 조회Async(
        CancellationToken cancellationToken = default);
}

public sealed class 농사로감자생육요구Profile조회UseCase(
    I농사로농작업일정Module module) : I농사로감자생육요구Profile조회UseCase
{
    public const string 감자ProductStableId = "product:potato";
    public const string 밭농사GroupCode = "210005";
    private const string 감자Name = "감자";
    private const string 밭농사Name = "밭농사";
    private const string ProfileStableId = "crop-requirement-profile:nongsaro.potato.1";

    private static readonly IReadOnlyList<string> Limitations =
    [
        "농사로 원문 위치를 확인한 검토 후보이며 실제 농장의 현재 상태나 재배 처방이 아닙니다.",
        "원문에 있는 수치와 시기를 품종·지역·작형 검토 없이 Simulation 임계값으로 사용할 수 없습니다.",
        "밭농사 210005는 작업군 분류이고 감자 상품코드가 아니므로 canonical 상품 관계로 승격하지 않습니다.",
        "사람 검토와 별도 rule revision 게시 전에는 FARM-ENV 생육 규칙을 만들거나 변경하지 않습니다."
    ];

    public async Task<농사로작물생육요구ProfileResponse> 조회Async(
        CancellationToken cancellationToken = default)
    {
        var scheduleList = await module.일정조회Async(밭농사GroupCode, cancellationToken);
        var potato = FindPotato(scheduleList);
        var contentNo = RequiredField(potato, "cntntsNo", "감자 콘텐츠번호");

        var detail = await module.상세조회Async(contentNo, cancellationToken);
        var era = await module.시기정보조회Async(contentNo, cancellationToken);
        var detailItem = SingleItem(
            detail,
            Nongsaro공공데이터Catalog.농작업일정상세Operation,
            "감자 농작업일정 상세");
        var eraItem = SingleItem(
            era,
            Nongsaro공공데이터Catalog.농작업일정시기Operation,
            "감자 농작업일정 시기");
        ValidateDetail(detailItem, contentNo);

        var detailContent = RequiredField(detailItem, "cn", "감자 상세 본문");
        var eraContent = RequiredField(eraItem, "htmlCn", "감자 시기 본문");
        var detailSourceId = $"source:nongsaro.work-schedule-detail.{contentNo}";
        var eraSourceId = $"source:nongsaro.work-schedule-era.{contentNo}";
        var retrievedAt = new[]
        {
            scheduleList.RetrievedAtUtc,
            detail.RetrievedAtUtc,
            era.RetrievedAtUtc
        }.Max();

        return new 농사로작물생육요구ProfileResponse(
            ProfileStableId,
            1,
            감자ProductStableId,
            감자Name,
            밭농사GroupCode,
            밭농사Name,
            contentNo,
            공통식품품목관계StatusCodes.Unlinked,
            작물생육요구검토StatusCodes.PendingHumanReview,
            false,
            retrievedAt,
            [
                Source(scheduleList, "source:nongsaro.work-schedule-list.210005", 밭농사GroupCode),
                Source(detail, detailSourceId, contentNo),
                Source(era, eraSourceId, contentNo)
            ],
            [
                Topic(작물생육근거TopicCodes.Soil, "토양", detailContent + eraContent,
                    detailSourceId, "필지·밭 준비·배토·비옥도 관련 원문 구간을 사람이 검토해야 합니다.",
                    "필지", "밭준비", "배토", "비옥도"),
                Topic(작물생육근거TopicCodes.Water, "물과 강수", eraContent,
                    eraSourceId, "관수·배수·가뭄·장마·습해를 물수지 규칙과 직접 동일시하지 않습니다.",
                    "관수", "배수", "가뭄", "장마", "습해"),
                Topic(작물생육근거TopicCodes.Temperature, "기온", detailContent + eraContent,
                    eraSourceId, "저온·고온·서리 관련 설명을 작형별로 검토해야 합니다.",
                    "온도", "저온", "고온", "서리", "동해"),
                Topic(작물생육근거TopicCodes.Sunlight, "햇빛", detailContent,
                    detailSourceId, "산광·빛 관련 언급은 씨감자 준비와 포장·저장 문맥을 구분해야 합니다.",
                    "산광", "빛"),
                Topic(작물생육근거TopicCodes.GrowthStage, "생육 단계", eraContent,
                    eraSourceId, "출현·생육·괴경 비대·수확 표현을 FARM-ENV 단계와 사람이 대응해야 합니다.",
                    "출현", "생육", "괴경", "수확"),
                Topic(작물생육근거TopicCodes.CultivationMethod, "재배 작형", eraContent,
                    eraSourceId, "봄·여름·가을·겨울시설재배를 하나의 공통 달력으로 합치지 않습니다.",
                    "봄재배", "여름재배", "가을재배", "겨울시설재배")
            ],
            Limitations);
    }

    private static Nongsaro공공데이터Item FindPotato(Nongsaro공공데이터Response response)
    {
        ValidateResponse(response, Nongsaro공공데이터Catalog.농작업일정목록Operation);
        var matches = response.Items
            .Where(item => string.Equals(item.Get("sj").Trim(), 감자Name, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidOperationException("NongsaroPotatoWorkScheduleIdentityInvalid");
        }

        return matches[0];
    }

    private static Nongsaro공공데이터Item SingleItem(
        Nongsaro공공데이터Response response,
        string expectedOperationName,
        string label)
    {
        ValidateResponse(response, expectedOperationName);
        if (response.Items.Count != 1)
        {
            throw new InvalidOperationException($"{label} 응답은 한 항목이어야 합니다.");
        }

        return response.Items[0];
    }

    private static void ValidateDetail(Nongsaro공공데이터Item detail, string contentNo)
    {
        if (!string.Equals(detail.Get("cntntsNo").Trim(), contentNo, StringComparison.Ordinal)
            || !string.Equals(detail.Get("cntntsSj").Trim(), 감자Name, StringComparison.Ordinal)
            || !string.Equals(detail.Get("kidofcomdtySeCode").Trim(), 밭농사GroupCode, StringComparison.Ordinal)
            || !string.Equals(detail.Get("kidofcomdtySeCodeNm").Trim(), 밭농사Name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("NongsaroPotatoWorkScheduleDetailIdentityInvalid");
        }
    }

    private static void ValidateResponse(Nongsaro공공데이터Response response, string operationName)
    {
        if (response is null
            || response.ServiceName != Nongsaro공공데이터Catalog.농작업일정Service
            || response.OperationName != operationName
            || response.ResultCode is not "00" and not "0"
            || response.RetrievedAtUtc == default
            || !Uri.TryCreate(response.SourceDocumentationUrl, UriKind.Absolute, out var sourceUri)
            || sourceUri.Scheme != Uri.UriSchemeHttps
            || response.Items is null)
        {
            throw new InvalidOperationException("NongsaroPotatoWorkScheduleSourceInvalid");
        }
    }

    private static string RequiredField(
        Nongsaro공공데이터Item item,
        string fieldName,
        string label)
    {
        var value = item.Get(fieldName).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{label}이 없습니다.");
        }

        return value;
    }

    private static 농사로작물생육SourceSnapshot Source(
        Nongsaro공공데이터Response response,
        string stableId,
        string sourceRecordId)
        => new(
            stableId,
            response.ServiceName,
            response.OperationName,
            sourceRecordId,
            response.RetrievedAtUtc,
            response.SourceDocumentationUrl,
            response.RawContentHashSha256);

    private static 농사로작물생육근거Topic Topic(
        string topicCode,
        string displayName,
        string sourceText,
        string sourceStableId,
        string reviewNote,
        params string[] evidenceTerms)
    {
        var located = evidenceTerms.Any(term =>
            sourceText.Contains(term, StringComparison.Ordinal));
        return new(
            topicCode,
            displayName,
            located
                ? 작물생육근거StatusCodes.LocatedNeedsReview
                : 작물생육근거StatusCodes.NotLocated,
            sourceStableId,
            reviewNote);
    }
}
