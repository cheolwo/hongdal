using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

public sealed record 공동구매개별원함원장결과(
    string 개별원함원장Id,
    long Revision,
    커뮤니티원장Dto? 원장 = null);

public interface I공동구매개별원함원장Service
{
    Task<공동구매개별원함원장결과> 저장Async(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동구매개별원함원장결과> 저장및자동집단투영예약Async(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => 저장Async(command, 자동집단Id, cancellationToken);

    Task<공동구매개별원함원장결과?> 철회Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 자동집단보다 먼저 사용자별 비구속 원함을 독립 Mongo 원장으로 보존합니다.
/// 자동집단은 이 원장의 공통 조건을 읽는 투영이며 주문·결제·계약의 원본이 아닙니다.
/// </summary>
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandOperatingSystem,
    SsalddelCodeLayer.Application,
    "사용자별 비구속 공동구매 원함을 독립 원장으로 먼저 저장하고 자동집단이 참조할 원본 식별자를 제공합니다.",
    ContractType = typeof(I공동구매개별원함원장Service),
    FlowOrder = 25,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "개별 원함은 주문·결제·계약이 아니며 상세 주소와 결제 정보를 저장하지 않습니다. 변경과 철회는 원함 주체 본인만 수행합니다.")]
public sealed class 공동구매개별원함원장Service : I공동구매개별원함원장Service
{
    private const string 기본커뮤니티Id = "platform";
    private const string 활성단계 = "individual-demand-active";
    private const string 철회단계 = "individual-demand-withdrawn";
    internal const string 자동집단투영모드 = "NonBindingAutomaticGroup";
    private readonly I커뮤니티원장저장소 _원장저장소;

    public 공동구매개별원함원장Service(I커뮤니티원장저장소 원장저장소)
    {
        _원장저장소 = 원장저장소;
    }

    public Task<공동구매개별원함원장결과> 저장Async(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => 저장내부Async(
            command,
            자동집단Id,
            자동집단투영대상: false,
            cancellationToken: cancellationToken);

    public Task<공동구매개별원함원장결과> 저장및자동집단투영예약Async(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => 저장내부Async(
            command,
            자동집단Id,
            자동집단투영대상: true,
            cancellationToken: cancellationToken);

    private async Task<공동구매개별원함원장결과> 저장내부Async(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        bool 자동집단투영대상,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.주문자키)
            || string.IsNullOrWhiteSpace(command.수요출처키)
            || string.IsNullOrWhiteSpace(command.상품키)
            || string.IsNullOrWhiteSpace(command.상품명)
            || string.IsNullOrWhiteSpace(command.배송권키)
            || string.IsNullOrWhiteSpace(자동집단Id)
            || command.희망수량 <= 0)
        {
            throw new InvalidOperationException("개별 원함 원장에는 원함 주체, 수요 출처, 상품, 수령 권역, 희망 수량과 자동집단 후보가 필요합니다.");
        }

        var 원장Id = 개별원함원장Id생성(command.주문자키, command.수요출처키);
        var 기존원장 = await _원장저장소.원장조회Async(원장Id, cancellationToken);
        if (기존원장 is not null
            && !string.Equals(기존원장.생성자UserId, command.주문자키, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("다른 사용자의 개별 원함 원장은 변경할 수 없습니다.");
        }
        var 요청지문 = 공동구매자동수요멱등정책.저장요청지문(command);
        if (기존원장 is not null
            && string.Equals(
                기존원장.외부참조.GetValueOrDefault("LastSaveIdempotencyKey"),
                command.요청멱등키,
                StringComparison.Ordinal))
        {
            if (!string.Equals(
                    기존원장.외부참조.GetValueOrDefault("LastSaveRequestFingerprint"),
                    요청지문,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("같은 요청 멱등 키를 다른 원함 변경에 다시 사용할 수 없습니다.");
            }

            return new 공동구매개별원함원장결과(
                기존원장.원장Id,
                기존원장.Revision,
                기존원장);
        }
        if (기존원장 is not null
            && string.Equals(기존원장.상태, 커뮤니티원장상태.닫힘, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "철회되어 닫힌 개별 원함은 다시 활성화할 수 없습니다. 재참여는 새 원함 회차로 등록해 주세요.");
        }
        if (command.개별원함기대Revision is { } expectedRevision
            && (기존원장 is null || 기존원장.Revision != expectedRevision))
        {
            throw new InvalidOperationException("개별 원함이 다른 곳에서 변경되었습니다. 최신 원함을 다시 조회해 주세요.");
        }

        var 커뮤니티Id = 기존원장?.커뮤니티Id
                         ?? 기본커뮤니티Id;
        var 표시명 = string.IsNullOrWhiteSpace(command.주문자표시명)
            ? 기존원장?.생성자표시명 ?? "공동구매 참여자"
            : command.주문자표시명.Trim();
        var data = 원함데이터(command, 자동집단Id, 원장Id, 요청지문);
        var projectionMode = 자동집단투영대상
            || string.Equals(
                기존원장?.확장속성.GetValueOrDefault("ProjectionMode"),
                자동집단투영모드,
                StringComparison.Ordinal)
                ? 자동집단투영모드
                : string.Empty;

        var 저장원장 = await _원장저장소.원장저장Async(
            new 커뮤니티원장저장요청
            {
                원장Id = 원장Id,
                기대Revision = 기존원장?.Revision,
                커뮤니티Id = 커뮤니티Id,
                원장템플릿Key = CommunityLedgerTemplateKeys.IndividualDemand,
                제목 = $"{command.상품명.Trim()} 개별 원함",
                원함 = $"{command.상품명.Trim()} {command.희망수량.ToString("G29", CultureInfo.InvariantCulture)}{단위(command.수량단위)} 구매를 비구속 상태로 원합니다.",
                상태 = 커뮤니티원장상태.진행중,
                현재단계Key = 활성단계,
                대상OsCode = CommunityLedgerOperatingSystemCodes.CommunityTrust,
                대상OsName = "커뮤니티 신뢰 OS",
                생성자UserId = command.주문자키.Trim(),
                생성자표시명 = 표시명,
                참여자목록 =
                [
                    new 커뮤니티원장참여자Dto
                    {
                        UserId = command.주문자키.Trim(),
                        DisplayName = 표시명,
                        RoleLabel = "원함 주체",
                        ParticipationState = "참여중"
                    }
                ],
                블록목록 =
                [
                    new 커뮤니티원장블록Dto
                    {
                        BlockId = "individual-demand",
                        BlockType = CommunityLedgerBlockTypes.Item,
                        Title = "개별 원함·공동 조건",
                        State = 공동구매자동수요상태코드.활성,
                        Data = data
                    }
                ],
                외부참조 = data,
                확장속성 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["SourceOfTruth"] = "IndividualDemandLedger",
                    ["ProjectionTarget"] = "GroupPurchaseAutomaticGroup",
                    ["ProjectionMode"] = projectionMode,
                    ["Binding"] = "NonBinding",
                    ["OwnerControlled"] = bool.TrueString
                }
            },
            command.주문자키.Trim(),
            cancellationToken);

        return new 공동구매개별원함원장결과(
            저장원장.원장Id,
            저장원장.Revision,
            저장원장);
    }

    public async Task<공동구매개별원함원장결과?> 철회Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.주문자키)
            || string.IsNullOrWhiteSpace(command.수요출처키))
        {
            throw new InvalidOperationException("개별 원함 철회에는 원함 주체와 수요 출처가 필요합니다.");
        }

        var 원장Id = 개별원함원장Id생성(command.주문자키, command.수요출처키);
        var 원장 = await _원장저장소.원장조회Async(원장Id, cancellationToken);
        if (원장 is null)
        {
            return null;
        }

        if (!string.Equals(원장.생성자UserId, command.주문자키, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("다른 사용자의 개별 원함 원장은 철회할 수 없습니다.");
        }

        if (string.Equals(원장.상태, 커뮤니티원장상태.닫힘, StringComparison.Ordinal)
            && string.Equals(원장.현재단계Key, 철회단계, StringComparison.Ordinal))
        {
            return new 공동구매개별원함원장결과(원장.원장Id, 원장.Revision, 원장);
        }
        if (command.개별원함기대Revision is { } expectedRevision
            && 원장.Revision != expectedRevision)
        {
            throw new InvalidOperationException("개별 원함이 다른 곳에서 변경되었습니다. 최신 원함을 다시 조회해 주세요.");
        }

        var 변경원장 = await _원장저장소.원장상태변경Async(
            new 커뮤니티원장상태변경요청
            {
                원장Id = 원장.원장Id,
                기대Revision = 원장.Revision,
                이전상태 = 원장.상태,
                상태 = 커뮤니티원장상태.닫힘,
                현재단계Key = 철회단계,
                메모 = string.IsNullOrWhiteSpace(command.철회사유)
                    ? "원함 주체가 비구속 수요를 철회했습니다."
                    : command.철회사유.Trim()
            },
            command.주문자키.Trim(),
            cancellationToken)
            ?? throw new InvalidOperationException("개별 원함 원장의 철회 상태를 저장하지 못했습니다.");

        return new 공동구매개별원함원장결과(
            변경원장.원장Id,
            변경원장.Revision,
            변경원장);
    }

    internal static string 개별원함원장Id생성(string 주문자키, string 수요출처키)
    {
        var 원본 = $"{주문자키.Trim()}\n{수요출처키.Trim()}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(원본)))
            .ToLowerInvariant();
        return $"individual-demand-{hash[..32]}";
    }

    private static IReadOnlyDictionary<string, string> 원함데이터(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        string 개별원함원장Id,
        string 요청지문)
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["IndividualDemandLedgerId"] = 개별원함원장Id,
            ["DemandSourceKey"] = command.수요출처키.Trim(),
            ["AutomaticGroupId"] = 자동집단Id.Trim(),
            ["ProductKey"] = command.상품키.Trim(),
            ["ProductName"] = command.상품명.Trim(),
            ["HSCode"] = command.HS코드?.Trim() ?? string.Empty,
            ["DesiredQuantity"] = command.희망수량.ToString("G29", CultureInfo.InvariantCulture),
            ["QuantityUnit"] = 단위(command.수량단위),
            ["DeliveryScopeKey"] = command.배송권키.Trim(),
            ["DeliveryScopeName"] = command.배송권명?.Trim() ?? string.Empty,
            ["TemperatureCode"] = command.온도코드?.Trim() ?? string.Empty,
            ["LogisticsMode"] = 공동구매자동수요물류방식코드.후속검토,
            ["TransactionType"] = 공동구매거래유형코드.정규화(command.거래유형),
            ["PriceBasis"] = 공동구매가격표시기준코드.정규화(command.가격표시기준, command.거래유형),
            ["DemandType"] = 공동구매자동수요유형코드.관심표시,
            ["PaymentStatus"] = 공동구매자동결제상태코드.미결제,
            ["OrdererDisplayName"] = command.주문자표시명?.Trim() ?? string.Empty,
            ["Memo"] = command.메모?.Trim() ?? string.Empty,
            ["LastSaveIdempotencyKey"] = command.요청멱등키.Trim(),
            ["LastSaveRequestFingerprint"] = 요청지문
        };

        if (command.커뮤니티게시글Id is { } sourcePostId)
        {
            data["SourceCommunityPostId"] = sourcePostId.ToString(CultureInfo.InvariantCulture);
        }
        if (command.목표참여자수 is { } targetParticipants)
        {
            data["TargetParticipantCount"] = targetParticipants.ToString(CultureInfo.InvariantCulture);
        }
        if (command.목표수량 is { } targetQuantity)
        {
            data["TargetQuantity"] = targetQuantity.ToString("G29", CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(command.커뮤니티원장Id))
        {
            data["SourceCommunityLedgerId"] = command.커뮤니티원장Id.Trim();
        }

        if (공동구매거래유형코드.정규화(command.거래유형) == 공동구매거래유형코드.B2B)
        {
            data["PurchasingOrganizationReference"] = command.구매조직참조키?.Trim() ?? string.Empty;
            data["PurchasingOrganizationName"] = command.구매조직표시명?.Trim() ?? string.Empty;
            data["TaxInvoiceRequired"] = command.세금계산서필요.ToString();
        }

        return data;
    }

    private static string 단위(string? value)
        => string.IsNullOrWhiteSpace(value) ? "kg" : value.Trim();
}
