using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공동구매자동집단화UseCase
{
    Task<공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>> 목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매자동집단응답>> 상세조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매자동집단배치미리보기응답>> 배치미리보기Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매자동집단응답>> 비구속수요저장Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매자동수요철회응답>> 수요철회Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매자동집단응답>> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);
}

[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[SsalddelUseCase("같이 주문 자동 집단화", Summary = "주문자의 구매 의사를 배송권 기준으로 모아 같이 주문 집단 후보를 형성합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
[SsalddelUseCaseActor(SsalddelActor.OrdererGroupLeader, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Include,
    "공공데이터조회UseCase",
    Condition = "주소, 공동주택, 배송권 기준으로 주문자를 묶는 경우",
    Summary = "주문자 집단화는 주소와 생활권 판단에 필요한 공공 데이터 조회를 포함합니다.")]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Extend,
    "커뮤니티게시글UseCase",
    Condition = "구매 의사를 다른 주문자에게 공개해 모집하거나 토론하는 경우",
    Summary = "자동 집단화 후보를 커뮤니티 게시글과 태그 기반 모집 흐름으로 확장합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Application,
    "사용자 수요 Command를 검증하고 개별 원함 원장을 먼저 저장한 뒤 공동구매 수요·모집 ProcessManager로 전달합니다.",
    ContractType = typeof(I공동구매자동집단화UseCase),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "비구속 수요는 사용자별 개별 원함 원장을 원본으로 삼고 자동집단에는 그 참조만 연결합니다. 주문·결제·운송은 만들지 않으며 상태 확정은 서버 원장 검증 뒤에만 수행합니다.")]
public sealed class 공동구매자동집단화UseCase : I공동구매자동집단화UseCase
{
    private readonly I공동구매자동집단화저장소 _저장소;
    private readonly I공동구매수령창고Service _수령창고Service;
    private readonly I공동구매개별원함원장Service _개별원함원장Service;
    private readonly I공동구매개별주문원장Service _개별주문원장Service;
    private readonly I공동구매주문자집단화Engine _집단화Engine;
    private readonly I공동구매수요모집ProcessManager? _수요모집ProcessManager;
    private readonly I공동구매개별원함자동집단투영Service? _원함투영Service;

    public 공동구매자동집단화UseCase(
        I공동구매자동집단화저장소 저장소,
        I공동구매수령창고Service 수령창고Service,
        I공동구매개별원함원장Service 개별원함원장Service,
        I공동구매개별주문원장Service 개별주문원장Service,
        I공동구매주문자집단화Engine 집단화Engine,
        I공동구매수요모집ProcessManager? 수요모집ProcessManager = null,
        I공동구매개별원함자동집단투영Service? 원함투영Service = null)
    {
        _저장소 = 저장소;
        _수령창고Service = 수령창고Service;
        _개별원함원장Service = 개별원함원장Service;
        _개별주문원장Service = 개별주문원장Service;
        _집단화Engine = 집단화Engine;
        _수요모집ProcessManager = 수요모집ProcessManager;
        _원함투영Service = 원함투영Service;
    }

    public async Task<공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>> 목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default)
    {
        var items = await _저장소.집단목록조회Async(조건, cancellationToken);
        return 공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>.성공결과(items);
    }

    public async Task<공동구매처리결과<공동구매자동집단응답>> 상세조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(자동집단Id))
        {
            return 공동구매처리결과<공동구매자동집단응답>.잘못된요청(
                "같이 주문 ID가 필요합니다.");
        }

        var item = await _저장소.집단조회Async(자동집단Id.Trim(), cancellationToken);
        return item is null
            ? 공동구매처리결과<공동구매자동집단응답>.찾을수없음(
                "같이 주문을 찾을 수 없습니다.")
            : 공동구매처리결과<공동구매자동집단응답>.성공결과(item);
    }

    public async Task<공동구매처리결과<공동구매자동집단배치미리보기응답>> 배치미리보기Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var 자동집단Id = _집단화Engine.자동집단Id생성(command);
            var 기존집단 = await _저장소.집단조회Async(자동집단Id, cancellationToken);
            var 미리보기 = _집단화Engine.배치미리보기(command, 기존집단);
            return 공동구매처리결과<공동구매자동집단배치미리보기응답>.성공결과(미리보기);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매자동집단배치미리보기응답>.잘못된요청(ex.Message);
        }
    }

    public async Task<공동구매처리결과<공동구매자동집단응답>> 비구속수요저장Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            비구속수요검증(command);
            비구속경계적용(command);
            var group = await 개별원함기반수요등록조율Async(
                command,
                비구속원함투영: true,
                cancellationToken);
            return 공동구매처리결과<공동구매자동집단응답>.성공결과(group);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매자동집단응답>.잘못된요청(ex.Message);
        }
    }

    public async Task<공동구매처리결과<공동구매자동수요철회응답>> 수요철회Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            철회검증(command);
            공동구매개별원함원장결과? 원함원장 = null;
            공동구매자동수요철회응답 response;
            if (command.개별원함기대Revision is not null)
            {
                원함원장 = await _개별원함원장Service.철회Async(command, cancellationToken);
                if (원함원장?.원장 is not null
                    && _원함투영Service is not null
                    && _원함투영Service.투영대상(원함원장.원장))
                {
                    var projection = await _원함투영Service.투영Async(
                        원함원장.원장,
                        cancellationToken);
                    response = projection.철회
                               ?? throw new InvalidOperationException("개별 원함 철회 투영 결과를 확인하지 못했습니다.");
                }
                else
                {
                    response = await 수요철회조율Async(command, cancellationToken);
                }
            }
            else
            {
                response = await 수요철회조율Async(command, cancellationToken);
                if (response.철회완료)
                {
                    원함원장 = await _개별원함원장Service.철회Async(
                        command,
                        cancellationToken);
                }
            }

            response.개별원함원장Id = 원함원장?.개별원함원장Id ?? string.Empty;
            return 공동구매처리결과<공동구매자동수요철회응답>.성공결과(response);
        }
        catch (KeyNotFoundException)
        {
            return 공동구매처리결과<공동구매자동수요철회응답>.찾을수없음(
                "철회할 본인 공동구매 수요를 찾을 수 없습니다.");
        }
        catch (UnauthorizedAccessException)
        {
            return 공동구매처리결과<공동구매자동수요철회응답>.찾을수없음(
                "철회할 본인 공동구매 수요를 찾을 수 없습니다.");
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매자동수요철회응답>.잘못된요청(ex.Message);
        }
    }

    public async Task<공동구매처리결과<공동구매자동집단응답>> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var 주문확정수요 = 주문확정수요인가(command);
            if (주문확정수요)
            {
                var warehouse = await _수령창고Service.확보Async(command, cancellationToken);
                command.도착창고Id = warehouse.창고Id;
                command.도착창고유형 = warehouse.창고유형;
                command.도착창고명 = warehouse.창고명;
                command.수령지주소참조키 = warehouse.주소참조키;
            }

            var group = await 개별원함기반수요등록조율Async(
                command,
                비구속원함투영: false,
                cancellationToken);
            if (주문확정수요)
            {
                var demand = group.수요목록.FirstOrDefault(x =>
                    string.Equals(x.수요출처키, command.수요출처키, StringComparison.Ordinal))
                    ?? group.수요목록.FirstOrDefault(x =>
                        string.Equals(x.주문자키, command.주문자키, StringComparison.Ordinal));
                if (demand is null)
                {
                    throw new InvalidOperationException("등록한 주문자 수요를 자동집단에서 찾을 수 없습니다.");
                }

                if (string.IsNullOrWhiteSpace(demand.공동구매주문집계원장Id)
                    || string.IsNullOrWhiteSpace(demand.개별주문원장Id)
                    || string.IsNullOrWhiteSpace(demand.입고예정원장Id))
                {
                    var ledgers = await _개별주문원장Service.생성및연결Async(
                        group,
                        demand,
                        cancellationToken);
                    group = await _저장소.개별주문원장연결Async(
                        group.자동집단Id,
                        demand.수요Id,
                        ledgers.공동구매주문집계원장Id,
                        ledgers.개별주문원장Id,
                        ledgers.입고예정원장Id,
                        cancellationToken);
                }
            }

            return 공동구매처리결과<공동구매자동집단응답>.성공결과(group);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매자동집단응답>.잘못된요청(ex.Message);
        }
    }

    private Task<공동구매자동집단응답> 수요등록조율Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken)
        => _수요모집ProcessManager is null
            ? _저장소.수요등록Async(command, cancellationToken)
            : _수요모집ProcessManager.수요등록조율Async(command, cancellationToken);

    private Task<공동구매자동수요철회응답> 수요철회조율Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken)
        => _수요모집ProcessManager is null
            ? _저장소.수요철회Async(command, cancellationToken)
            : _수요모집ProcessManager.수요철회조율Async(command, cancellationToken);

    private async Task<공동구매자동집단응답> 개별원함기반수요등록조율Async(
        공동구매자동수요등록Command command,
        bool 비구속원함투영,
        CancellationToken cancellationToken)
    {
        var 자동집단Id = _집단화Engine.자동집단Id생성(command);
        var 원함원장 = 비구속원함투영
            ? await _개별원함원장Service.저장및자동집단투영예약Async(
                command,
                자동집단Id,
                cancellationToken)
            : await _개별원함원장Service.저장Async(
                command,
                자동집단Id,
                cancellationToken);
        var group = 비구속원함투영
                    && 원함원장.원장 is not null
                    && _원함투영Service is not null
            ? (await _원함투영Service.투영Async(원함원장.원장, cancellationToken)).자동집단
              ?? throw new InvalidOperationException("개별 원함 자동집단 투영 결과를 확인하지 못했습니다.")
            : await 수요등록조율Async(command, cancellationToken);
        var demand = group.수요목록.FirstOrDefault(x =>
            string.Equals(x.수요출처키, command.수요출처키, StringComparison.Ordinal)
            && string.Equals(x.주문자키, command.주문자키, StringComparison.Ordinal));
        if (demand is null)
        {
            throw new InvalidOperationException("개별 원함을 연결할 주문자 수요를 자동집단에서 찾을 수 없습니다.");
        }

        if (!string.Equals(
                demand.개별원함원장Id,
                원함원장.개별원함원장Id,
                StringComparison.Ordinal))
        {
            group = await _저장소.개별원함원장연결Async(
                group.자동집단Id,
                demand.수요Id,
                원함원장.개별원함원장Id,
                cancellationToken);
        }

        return group;
    }

    private static bool 주문확정수요인가(공동구매자동수요등록Command command)
        => command.수요유형 == 공동구매자동수요유형코드.예약결제
           || command.결제상태 is 공동구매자동결제상태코드.예약됨
               or 공동구매자동결제상태코드.결제확정;

    private static void 비구속수요검증(공동구매자동수요등록Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.요청멱등키))
        {
            throw new InvalidOperationException("비구속 수요 저장에는 요청 멱등 키가 필요합니다.");
        }

        if (command.요청멱등키.Trim().Length > 160)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자 이하여야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.수요출처키))
        {
            throw new InvalidOperationException("비구속 수요 저장에는 수요출처키가 필요합니다.");
        }

        if (command.수요출처키.Trim().Length > 200)
        {
            throw new InvalidOperationException("수요출처키는 200자 이하여야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.주문자키))
        {
            throw new InvalidOperationException("비구속 수요 저장에는 주문자 식별키가 필요합니다.");
        }
    }

    private static void 비구속경계적용(공동구매자동수요등록Command command)
    {
        command.물류방식 = 공동구매자동수요물류방식코드.후속검토;
        command.거래유형 = 공동구매거래유형코드.정규화(command.거래유형);
        command.가격표시기준 = 공동구매가격표시기준코드.정규화(
            command.가격표시기준,
            command.거래유형);
        if (command.거래유형 == 공동구매거래유형코드.B2C)
        {
            command.구매조직참조키 = string.Empty;
            command.구매조직표시명 = string.Empty;
            command.세금계산서필요 = false;
        }
        command.수요유형 = 공동구매자동수요유형코드.관심표시;
        command.결제상태 = 공동구매자동결제상태코드.미결제;
        command.예약결제금액 = null;
        command.도착창고Id = null;
        command.도착창고유형 = string.Empty;
        command.도착창고명 = string.Empty;
        command.수령지주소참조키 = string.Empty;
        command.수령지표시명 = string.Empty;
        command.수령도로명주소 = string.Empty;
        command.수령상세주소 = string.Empty;
    }

    private static void 철회검증(공동구매자동수요철회Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.요청멱등키))
        {
            throw new InvalidOperationException("수요 철회에는 요청 멱등 키가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.수요출처키)
            || string.IsNullOrWhiteSpace(command.주문자키))
        {
            throw new InvalidOperationException("수요 철회에는 수요출처키와 주문자 식별키가 필요합니다.");
        }

        if (command.요청멱등키.Trim().Length > 160
            || command.수요출처키.Trim().Length > 200)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자, 수요출처키는 200자 이하여야 합니다.");
        }
    }
}
