using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Versioning;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Services.Orderer;

public interface I공동구매자동집단화저장소
{
    Task<공동구매자동집단응답> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);

    Task<공동구매자동수요철회응답> 수요철회Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단응답?> 집단조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단응답> 개별원함원장연결Async(
        string 자동집단Id,
        string 수요Id,
        string 개별원함원장Id,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단응답> 개별주문원장연결Async(
        string 자동집단Id,
        string 수요Id,
        string 공동구매주문집계원장Id,
        string 개별주문원장Id,
        string 입고예정원장Id,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Infrastructure,
    "공동구매 모집 상태전이, OS 큐, 점검 시각과 사람 승인 인계를 Mongo 원장에 원자적으로 기록합니다.",
    ContractType = typeof(I공동구매수요모집ProcessStore),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "낙관적 동시성과 멱등 키를 검증하며 승인 상태만 기록하고 1.5 원장이나 외부 실행은 생성하지 않습니다.")]
public sealed class Mongo공동구매자동집단화저장소 :
    I공동구매자동집단화저장소,
    I공동구매수요모집ProcessStore
{
    private const string 컬렉션명 = "orderer_group_purchase_auto_groups";
    private const int 최대동시성재시도횟수 = 8;
    private static readonly DateTime Os점검없음Utc = new(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
    private readonly IMongoCollection<공동구매자동집단문서> _컬렉션;
    private readonly I공동구매주문자집단화Engine _집단화Engine;
    private readonly SemaphoreSlim _인덱스Lock = new(1, 1);
    private bool _인덱스준비됨;

    public Mongo공동구매자동집단화저장소(
        IMongoClient mongoClient,
        IOptions<MongoDbOptions> options,
        I공동구매주문자집단화Engine 집단화Engine)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _컬렉션 = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<공동구매자동집단문서>(컬렉션명);
        _집단화Engine = 집단화Engine;
    }

    public async Task<공동구매자동집단응답> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default)
    {
        await 인덱스준비Async(cancellationToken);
        검증(command);

        var now = DateTime.UtcNow;
        var 상품키 = 정규화(command.상품키, "unknown-product", 120);
        var 배송권키 = 정규화(command.배송권키, "unknown-scope", 160);
        var 온도코드 = 정규화(command.온도코드, "상온", 40);
        var 물류방식 = 정규화(command.물류방식, 공동구매자동수요물류방식코드.후속검토, 40);
        var 거래유형 = 공동구매거래유형코드.정규화(command.거래유형);
        var 가격표시기준 = 공동구매가격표시기준코드.정규화(command.가격표시기준, 거래유형);
        var 주문자키 = 정규화(command.주문자키, "anonymous-orderer", 120);
        var 수요출처키 = 정규화(
            command.수요출처키,
            $"orderer:{주문자키}",
            200);
        var 자동집단Id = _집단화Engine.자동집단Id생성(command);
        var 갱신토큰 = ObjectId.GenerateNewId().ToString();
        var 요청멱등키 = 정규화(command.요청멱등키, $"legacy:{갱신토큰}", 160);
        var 요청지문 = 공동구매자동수요멱등정책.저장요청지문(command);

        var 기존출처문서목록 = await _컬렉션
            .Find(Builders<공동구매자동집단문서>.Filter.ElemMatch(
                x => x.수요목록,
                x => x.수요출처키 == 수요출처키))
            .ToListAsync(cancellationToken);
        var 기존출처위치 = 공동구매자동수요동시성정책
            .최신수요위치(기존출처문서목록, 수요출처키);
        var 기존출처수요 = 기존출처위치?.수요;
        if (기존출처수요 is not null
            && !string.IsNullOrWhiteSpace(기존출처수요.주문자키)
            && !string.Equals(기존출처수요.주문자키, 주문자키, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("동일한 수요출처키를 다른 주문자가 사용할 수 없습니다.");
        }

        if (기존출처수요 is not null
            && 공동구매자동수요멱등정책.이미처리됨(
                기존출처수요.명령목록,
                요청멱등키,
                공동구매자동수요명령유형코드.저장,
                요청지문))
        {
            return 응답으로(기존출처위치!.문서);
        }

        수요저장가능검증(기존출처위치?.문서, now);

        await 집단저장Async(
            자동집단Id,
            생성허용: true,
            기존문서 =>
            {
                var 같은집단수요 = 기존문서?.수요목록
                    .Where(x => x.수요출처키 == 수요출처키)
                    .OrderByDescending(수요갱신시각)
                    .ThenByDescending(x => x.갱신토큰, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (같은집단수요 is not null
                    && !string.IsNullOrWhiteSpace(같은집단수요.주문자키)
                    && !string.Equals(같은집단수요.주문자키, 주문자키, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("동일한 수요출처키를 다른 주문자가 사용할 수 없습니다.");
                }

                var 기존수요 = 더최근수요(같은집단수요, 기존출처수요);
                if (기존수요 is not null
                    && 공동구매자동수요멱등정책.이미처리됨(
                        기존수요.명령목록,
                        요청멱등키,
                        공동구매자동수요명령유형코드.저장,
                        요청지문))
                {
                    return 집단저장계획.보존(기존문서!);
                }

                수요저장가능검증(기존문서, now);

                if (같은집단수요 is not null
                    && 공동구매자동수요동시성정책.기존수요보존(같은집단수요, now, 갱신토큰))
                {
                    return 집단저장계획.보존(기존문서!);
                }

                var 수요 = new 공동구매자동수요문서
                {
                    수요Id = 기존수요?.수요Id ?? ObjectId.GenerateNewId().ToString(),
                    수요출처키 = 수요출처키,
                    커뮤니티게시글Id = command.커뮤니티게시글Id,
                    커뮤니티원장Id = 정규화(command.커뮤니티원장Id, string.Empty, 200),
                    상품키 = 상품키,
                    상품명 = 정규화(command.상품명, 상품키, 160),
                    거래유형 = 거래유형,
                    가격표시기준 = 가격표시기준,
                    구매조직참조키 = 정규화(command.구매조직참조키, string.Empty, 160),
                    구매조직표시명 = 정규화(command.구매조직표시명, string.Empty, 160),
                    사업자검증상태 = 거래유형 == 공동구매거래유형코드.B2B
                        ? 기존수요?.사업자검증상태 is 주문자집단사업자검증상태코드.검증완료
                            ? 주문자집단사업자검증상태코드.검증완료
                            : 주문자집단사업자검증상태코드.필요
                        : 주문자집단사업자검증상태코드.불필요,
                    세금계산서필요 = command.세금계산서필요,
                    배송권키 = 배송권키,
                    배송권명 = 정규화(command.배송권명, 배송권키, 160),
                    주문자키 = 주문자키,
                    주문자표시명 = 정규화(command.주문자표시명, "주문자", 80),
                    도착창고Id = command.도착창고Id is > 0 ? command.도착창고Id : null,
                    도착창고유형 = 정규화(command.도착창고유형, string.Empty, 50),
                    도착창고명 = 정규화(command.도착창고명, string.Empty, 200),
                    수령지주소참조키 = 정규화(command.수령지주소참조키, string.Empty, 200),
                    개별원함원장Id = 기존수요?.개별원함원장Id ?? string.Empty,
                    입고의미상태 = 주문확정수요인가(command)
                        ? 공동구매개별주문입고상태코드.입고예정
                        : 기존수요?.입고의미상태 ?? 공동구매개별주문입고상태코드.미지정,
                    공동구매주문집계원장Id = 기존수요?.공동구매주문집계원장Id ?? string.Empty,
                    개별주문원장Id = 기존수요?.개별주문원장Id ?? string.Empty,
                    입고예정원장Id = 기존수요?.입고예정원장Id ?? string.Empty,
                    수요유형 = 수요유형정규화(command.수요유형),
                    결제상태 = 결제상태정규화(command.결제상태),
                    희망수량 = Math.Max(0, command.희망수량),
                    수량단위 = 정규화(command.수량단위, "kg", 20),
                    예약결제금액 = command.예약결제금액,
                    메모 = 정규화(command.메모, string.Empty, 1000),
                    목표참여자수 = 양수값(command.목표참여자수),
                    목표수량 = 양수값(command.목표수량),
                    상태 = 공동구매자동수요상태코드.활성,
                    철회시각Utc = null,
                    철회사유 = string.Empty,
                    명령목록 = 기존수요?.명령목록.ToList() ?? [],
                    생성시각Utc = 기존수요?.생성시각Utc ?? now,
                    갱신시각Utc = now,
                    갱신토큰 = 갱신토큰
                };
                공동구매자동수요멱등정책.기록추가(
                    수요.명령목록,
                    요청멱등키,
                    공동구매자동수요명령유형코드.저장,
                    요청지문,
                    now);

                var 변경문서 = 기존문서 ?? new 공동구매자동집단문서
                {
                    Id = ObjectId.GenerateNewId(),
                    자동집단Id = 자동집단Id,
                    상품키 = 상품키,
                    상품명 = 수요.상품명,
                    HS코드 = 정규화(command.HS코드, string.Empty, 20),
                    온도코드 = 온도코드,
                    물류방식 = 물류방식,
                    거래유형 = 거래유형,
                    가격표시기준 = 가격표시기준,
                    배송권키 = 배송권키,
                    배송권명 = 수요.배송권명,
                    현재상태 = 공동구매자동집단상태코드.수요수집중,
                    생성시각Utc = now,
                    모집종료시각Utc = 공동구매자동집단모집정책.기본모집종료시각Utc(now)
                };

                변경문서.거래유형 = 거래유형;
                변경문서.가격표시기준 = 가격표시기준;

                변경문서.수요목록.RemoveAll(x => x.수요출처키 == 수요출처키);
                변경문서.수요목록.Add(수요);
                var 재활성화 = 기존수요?.상태 == 공동구매자동수요상태코드.철회;
                변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = 기존수요 is null
                        ? "DemandRegistered"
                        : 재활성화
                            ? "DemandReactivated"
                            : "DemandUpdated",
                    요약 = $"{공동구매거래유형코드.표시명(거래유형)} 수요가 {수요.희망수량:N0}{수요.수량단위} {(기존수요 is null ? "등록" : 재활성화 ? "재등록" : "변경")}되었습니다.",
                    발생시각Utc = now
                });
                재계산(변경문서, now);
                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        // 이동 대상에 먼저 기록하고 이전 복사본을 정리한다. 정리 도중 장애가 나도 수요 자체는 유실되지 않으며,
        // 같은 출처의 다음 등록 또는 동시 정리 시 최신 갱신 토큰을 기준으로 중복이 수렴한다.
        await 중복수요정리Async(수요출처키, cancellationToken);

        var 정리후문서목록 = await _컬렉션
            .Find(Builders<공동구매자동집단문서>.Filter.ElemMatch(
                x => x.수요목록,
                x => x.수요출처키 == 수요출처키))
            .ToListAsync(cancellationToken);
        var 최신위치 = 공동구매자동수요동시성정책
            .최신수요위치(정리후문서목록, 수요출처키)
            ?? throw new InvalidOperationException("등록한 공동구매 수요의 최신 소속 집단을 찾을 수 없습니다.");

        return 응답으로(최신위치.문서);
    }

    public async Task<공동구매자동수요철회응답> 수요철회Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default)
    {
        await 인덱스준비Async(cancellationToken);
        철회검증(command);

        var 수요출처키 = 정규화(command.수요출처키, string.Empty, 200);
        var 주문자키 = 정규화(command.주문자키, string.Empty, 120);
        var 요청멱등키 = 정규화(command.요청멱등키, string.Empty, 160);
        var 요청지문 = 공동구매자동수요멱등정책.철회요청지문(command);
        var 문서목록 = await _컬렉션
            .Find(Builders<공동구매자동집단문서>.Filter.ElemMatch(
                x => x.수요목록,
                x => x.수요출처키 == 수요출처키))
            .ToListAsync(cancellationToken);
        var 기존위치 = 공동구매자동수요동시성정책.최신수요위치(문서목록, 수요출처키)
            ?? throw new KeyNotFoundException("철회할 공동구매 수요를 찾을 수 없습니다.");
        본인수요검증(기존위치.수요, 주문자키);

        if (공동구매자동수요멱등정책.이미처리됨(
                기존위치.수요.명령목록,
                요청멱등키,
                공동구매자동수요명령유형코드.철회,
                요청지문))
        {
            return 철회응답으로(기존위치.문서, 기존위치.수요, 요청멱등키, true);
        }

        var 이미처리됨 = false;
        var now = DateTime.UtcNow;
        var 문서 = await 집단저장Async(
            기존위치.문서.자동집단Id,
            생성허용: false,
            기존문서 =>
            {
                var 변경문서 = 기존문서
                    ?? throw new KeyNotFoundException("철회할 공동구매 수요 집단을 찾을 수 없습니다.");
                var 수요 = 변경문서.수요목록
                    .Where(x => x.수요출처키 == 수요출처키)
                    .OrderByDescending(수요갱신시각)
                    .ThenByDescending(x => x.갱신토큰, StringComparer.Ordinal)
                    .FirstOrDefault()
                    ?? throw new KeyNotFoundException("철회할 공동구매 수요를 찾을 수 없습니다.");
                본인수요검증(수요, 주문자키);

                if (공동구매자동수요멱등정책.이미처리됨(
                        수요.명령목록,
                        요청멱등키,
                        공동구매자동수요명령유형코드.철회,
                        요청지문))
                {
                    이미처리됨 = true;
                    return 집단저장계획.보존(변경문서);
                }

                var 활성수요 = 활성수요인가(수요);
                if (활성수요 && 변경문서.현재상태 == 공동구매자동집단상태코드.확정)
                {
                    throw new InvalidOperationException("확정된 공동구매의 수요는 비구속 철회 API에서 철회할 수 없습니다.");
                }

                if (활성수요 && !비구속수요인가(수요))
                {
                    throw new InvalidOperationException("결제 또는 주문 원장에 연결된 수요는 비구속 철회 API에서 철회할 수 없습니다.");
                }

                이미처리됨 = !활성수요;
                if (활성수요)
                {
                    수요.상태 = 공동구매자동수요상태코드.철회;
                    수요.철회시각Utc = now;
                    수요.철회사유 = 정규화(command.철회사유, string.Empty, 500);
                    변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                    {
                        이벤트유형 = "DemandWithdrawn",
                        요약 = $"{수요.주문자표시명} 수요가 철회되었습니다.",
                        발생시각Utc = now
                    });
                }

                공동구매자동수요멱등정책.기록추가(
                    수요.명령목록,
                    요청멱등키,
                    공동구매자동수요명령유형코드.철회,
                    요청지문,
                    now);
                수요.갱신시각Utc = now;
                수요.갱신토큰 = ObjectId.GenerateNewId().ToString();
                if (활성수요)
                {
                    재계산(변경문서, now);
                }
                else
                {
                    변경문서.수정시각Utc = now;
                }

                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        var 철회수요 = 문서.수요목록
            .Where(x => x.수요출처키 == 수요출처키)
            .OrderByDescending(수요갱신시각)
            .ThenByDescending(x => x.갱신토큰, StringComparer.Ordinal)
            .First();
        return 철회응답으로(문서, 철회수요, 요청멱등키, 이미처리됨);
    }

    public async Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default)
    {
        await 인덱스준비Async(cancellationToken);

        var builder = Builders<공동구매자동집단문서>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(조건.상품키))
        {
            filter &= builder.Eq(x => x.상품키, 정규화(조건.상품키, string.Empty, 120));
        }

        if (!string.IsNullOrWhiteSpace(조건.배송권키))
        {
            filter &= builder.Eq(x => x.배송권키, 정규화(조건.배송권키, string.Empty, 160));
        }

        var items = await _컬렉션
            .Find(filter)
            .SortByDescending(x => x.수정시각Utc)
            .Limit(500)
            .ToListAsync(cancellationToken);

        var responses = items
            .Select(응답으로)
            .Where(x => x.수요건수 > 0);
        if (!string.IsNullOrWhiteSpace(조건.현재상태))
        {
            var 현재상태 = 정규화(조건.현재상태, string.Empty, 80);
            responses = responses.Where(x =>
                string.Equals(x.현재상태, 현재상태, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(조건.거래유형))
        {
            var 거래유형 = 공동구매거래유형코드.정규화(조건.거래유형);
            responses = responses.Where(x => string.Equals(x.거래유형, 거래유형, StringComparison.Ordinal));
        }

        return responses
            .Take(100)
            .ToArray();
    }

    public async Task<공동구매자동집단응답?> 집단조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        await 인덱스준비Async(cancellationToken);

        var document = await _컬렉션
            .Find(x => x.자동집단Id == 자동집단Id.Trim())
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : 응답으로(document);
    }

    public async Task<공동구매수요모집Os조율응답> 운영조율Async(
        string 자동집단Id,
        string 트리거코드,
        string 조율멱등키,
        IReadOnlyList<string> 정책코드목록,
        DateTime 기준시각Utc,
        TimeSpan 장기모집점검주기,
        string 실행모드,
        bool 후속워크플로우활성여부,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(트리거코드);
        ArgumentNullException.ThrowIfNull(정책코드목록);
        await 인덱스준비Async(cancellationToken);

        var now = Utc시각(기준시각Utc);
        var 정규화조율키 = 정규화(조율멱등키, string.Empty, 240);
        var 상태변경 = false;
        var 큐변경 = false;
        var 문서 = await 집단저장Async(
            자동집단Id.Trim(),
            생성허용: false,
            기존문서 =>
            {
                var 변경문서 = 기존문서
                    ?? throw new InvalidOperationException("조율할 공동구매 자동집단을 찾을 수 없습니다.");
                if (!string.IsNullOrWhiteSpace(정규화조율키)
                    && 변경문서.최근Os조율멱등키목록.Contains(정규화조율키, StringComparer.Ordinal))
                {
                    상태변경 = false;
                    큐변경 = false;
                    return 집단저장계획.보존(변경문서);
                }

                var 이전상태 = 변경문서.현재상태;
                var 이전큐 = 변경문서.현재Os큐;
                재계산(변경문서, now);
                var 현재큐 = Os큐코드(변경문서);
                상태변경 = !string.Equals(이전상태, 변경문서.현재상태, StringComparison.Ordinal);
                큐변경 = !string.Equals(이전큐, 현재큐, StringComparison.Ordinal);

                변경문서.운영체제Id = OperatingSystemIds.GroupPurchaseDemand;
                변경문서.Os정책버전 = "1.0";
                변경문서.현재Os큐 = 현재큐;
                변경문서.마지막Os트리거 = 정규화(트리거코드, 공동구매수요모집Os트리거코드.수동재조율, 100);
                변경문서.적용Os정책코드목록 = 정책코드목록
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => 정규화(x, string.Empty, 100))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                변경문서.마지막Os조율시각Utc = now;
                변경문서.다음Os운영점검시각Utc = 다음운영점검시각(
                    변경문서,
                    now,
                    장기모집점검주기);
                변경문서.실행모드 = 정규화(실행모드, "Simulation", 40);
                변경문서.후속워크플로우활성여부 = 후속워크플로우활성여부;
                if (변경문서.인계상태 != 공동구매수요모집인계상태코드.승인후속대기)
                {
                    변경문서.인계상태 = 변경문서.현재상태 is 공동구매자동집단상태코드.확정대기
                        or 공동구매자동집단상태코드.확정
                        ? 공동구매수요모집인계상태코드.승인대기
                        : 공동구매수요모집인계상태코드.미요청;
                }

                if (!string.IsNullOrWhiteSpace(정규화조율키))
                {
                    변경문서.최근Os조율멱등키목록.Add(정규화조율키);
                    if (변경문서.최근Os조율멱등키목록.Count > 64)
                    {
                        변경문서.최근Os조율멱등키목록.RemoveRange(
                            0,
                            변경문서.최근Os조율멱등키목록.Count - 64);
                    }
                }

                변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = "GroupPurchaseDemandOsCoordinated",
                    요약 = $"{OperatingSystemIds.GroupPurchaseDemand}가 {변경문서.마지막Os트리거} 트리거를 처리해 {현재큐} 큐로 조율했습니다.",
                    발생시각Utc = now
                });
                변경문서.수정시각Utc = now;
                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        return new 공동구매수요모집Os조율응답
        {
            집단 = 응답으로(문서),
            운영상태 = Os상태응답으로(문서),
            집단상태변경여부 = 상태변경,
            운영큐변경여부 = 큐변경
        };
    }

    public async Task<IReadOnlyList<string>> 운영점검대상조회Async(
        DateTime 기준시각Utc,
        int 최대건수,
        CancellationToken cancellationToken)
    {
        await 인덱스준비Async(cancellationToken);
        var now = Utc시각(기준시각Utc);
        var builder = Builders<공동구매자동집단문서>.Filter;
        var 진행상태 = builder.In(
            x => x.현재상태,
            [공동구매자동집단상태코드.수요수집중, 공동구매자동집단상태코드.확정대기]);
        var 점검시각도래 = builder.Lte(x => x.다음Os운영점검시각Utc, now)
            | builder.Exists(x => x.다음Os운영점검시각Utc, false);

        return await _컬렉션
            .Find(진행상태 & 점검시각도래)
            .SortBy(x => x.모집종료시각Utc)
            .ThenBy(x => x.생성시각Utc)
            .Limit(Math.Clamp(최대건수, 1, 1000))
            .Project(x => x.자동집단Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        await 인덱스준비Async(cancellationToken);
        var 문서 = await _컬렉션
            .Find(x => x.자동집단Id == 자동집단Id.Trim())
            .FirstOrDefaultAsync(cancellationToken);
        return 문서 is null ? null : Os상태응답으로(문서);
    }

    public async Task<공동구매수요모집인계승인응답> 인계승인Async(
        string 자동집단Id,
        공동구매수요모집인계승인요청 요청,
        string 승인자키,
        DateTime 승인시각Utc,
        string 실행모드,
        bool 후속워크플로우활성여부,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(요청);
        ArgumentException.ThrowIfNullOrWhiteSpace(승인자키);
        await 인덱스준비Async(cancellationToken);

        var now = Utc시각(승인시각Utc);
        var 요청멱등키 = 정규화(요청.요청멱등키, string.Empty, 160);
        var 이미처리됨 = false;
        var 문서 = await 집단저장Async(
            자동집단Id.Trim(),
            생성허용: false,
            기존문서 =>
            {
                var 변경문서 = 기존문서
                    ?? throw new InvalidOperationException("인계 승인할 공동구매 자동집단을 찾을 수 없습니다.");
                if (변경문서.인계상태 == 공동구매수요모집인계상태코드.승인후속대기
                    && !string.IsNullOrWhiteSpace(변경문서.인계요청Id))
                {
                    이미처리됨 = true;
                    return 집단저장계획.보존(변경문서);
                }

                재계산(변경문서, now);
                if (변경문서.현재상태 is not 공동구매자동집단상태코드.확정대기
                    and not 공동구매자동집단상태코드.확정)
                {
                    throw new InvalidOperationException("모집 목표를 충족해 확정 검토 큐에 들어온 집단만 1.5 준비 단계로 인계 승인할 수 있습니다.");
                }

                변경문서.현재상태 = 공동구매자동집단상태코드.확정;
                변경문서.운영체제Id = OperatingSystemIds.GroupPurchaseDemand;
                변경문서.Os정책버전 = "1.0";
                변경문서.현재Os큐 = 공동구매수요모집Os큐코드.인계준비;
                변경문서.마지막Os트리거 = 공동구매수요모집Os트리거코드.인계승인;
                변경문서.마지막Os조율시각Utc = now;
                변경문서.다음Os운영점검시각Utc = Os점검없음Utc;
                변경문서.인계상태 = 공동구매수요모집인계상태코드.승인후속대기;
                변경문서.인계요청Id = ObjectId.GenerateNewId().ToString();
                변경문서.인계승인멱등키 = 요청멱등키;
                변경문서.대상운영체제Id = OperatingSystemIds.GroupPurchaseImport;
                변경문서.대상워크플로우코드 = "GroupPurchaseImport";
                변경문서.승인자키 = 정규화(승인자키, "admin", 160);
                변경문서.승인시각Utc = now;
                변경문서.승인사유 = 정규화(요청.승인사유, string.Empty, 1000);
                변경문서.실행모드 = 정규화(실행모드, "Simulation", 40);
                변경문서.후속워크플로우활성여부 = 후속워크플로우활성여부;
                변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = "GroupPurchaseDemandHandoffApproved",
                    요약 = "운영자가 모집 결과를 확인하고 공동주문 수입 준비 단계로의 인계를 승인했습니다. 후속 원장은 별도 UseCase가 생성합니다.",
                    발생시각Utc = now
                });
                변경문서.수정시각Utc = now;
                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        return new 공동구매수요모집인계승인응답
        {
            요청멱등키 = 요청멱등키,
            이미처리됨 = 이미처리됨,
            집단 = 응답으로(문서),
            운영상태 = Os상태응답으로(문서),
            안내 = 이미처리됨
                ? "이미 인계 승인된 모집 결과입니다. 중복 후속 실행은 만들지 않았습니다."
                : 후속워크플로우활성여부
                    ? "인계 승인을 기록했습니다. 별도 승인된 UseCase가 정식 공동수입 원장을 연결하고 그 안에 1.5 준비 블록을 저장해야 합니다."
                    : "인계 승인을 원장에 기록했습니다. 1.5 기능 플래그가 꺼져 있어 후속 원장은 생성하지 않았습니다."
        };
    }

    public async Task<공동구매수요모집Os상태응답> 후속원장연결Async(
        string 자동집단Id,
        string 인계요청Id,
        string 대상원장Id,
        DateTime 연결시각Utc,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(인계요청Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(대상원장Id);
        await 인덱스준비Async(cancellationToken);

        var now = Utc시각(연결시각Utc);
        var normalizedHandoffId = 인계요청Id.Trim();
        var normalizedLedgerId = 대상원장Id.Trim();
        var 문서 = await 집단저장Async(
            자동집단Id.Trim(),
            생성허용: false,
            기존문서 =>
            {
                var 변경문서 = 기존문서
                    ?? throw new InvalidOperationException("후속 원장을 연결할 공동구매 자동집단을 찾을 수 없습니다.");
                if (!string.Equals(
                        변경문서.인계상태,
                        공동구매수요모집인계상태코드.승인후속대기,
                        StringComparison.Ordinal)
                    || !string.Equals(변경문서.인계요청Id, normalizedHandoffId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("승인된 1.0 인계 요청과 일치하는 1.5 원장만 연결할 수 있습니다.");
                }

                if (string.Equals(변경문서.대상원장Id, normalizedLedgerId, StringComparison.Ordinal))
                {
                    return 집단저장계획.보존(변경문서);
                }
                if (!string.IsNullOrWhiteSpace(변경문서.대상원장Id))
                {
                    throw new InvalidOperationException("이 인계 요청에는 이미 다른 1.5 대상 원장이 연결되어 있습니다.");
                }

                변경문서.대상운영체제Id = OperatingSystemIds.GroupPurchaseImport;
                변경문서.대상워크플로우코드 = "GroupPurchaseImport";
                변경문서.대상원장Id = normalizedLedgerId;
                변경문서.마지막Os트리거 = 공동구매수요모집Os트리거코드.후속원장연결;
                변경문서.마지막Os조율시각Utc = now;
                변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = "GroupPurchaseImportReadinessLedgerLinked",
                    요약 = "승인된 수요 인계 요청에 정식 공동수입 원장과 1.5 공급·가격·무역 준비 블록을 연결했습니다. 계약·결제·신고·운송 실행은 열지 않았습니다.",
                    발생시각Utc = now
                });
                변경문서.수정시각Utc = now;
                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        return Os상태응답으로(문서);
    }

    public async Task<공동구매자동집단응답> 개별원함원장연결Async(
        string 자동집단Id,
        string 수요Id,
        string 개별원함원장Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(수요Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(개별원함원장Id);
        await 인덱스준비Async(cancellationToken);

        var normalizedLedgerId = 개별원함원장Id.Trim();
        var now = DateTime.UtcNow;
        var 문서 = await 집단저장Async(
            자동집단Id.Trim(),
            생성허용: false,
            기존문서 =>
            {
                var 변경문서 = 기존문서
                    ?? throw new InvalidOperationException("개별 원함 원장을 연결할 자동집단을 찾을 수 없습니다.");
                var 수요 = 변경문서.수요목록.FirstOrDefault(x => x.수요Id == 수요Id.Trim())
                    ?? throw new InvalidOperationException("개별 원함 원장을 연결할 주문자 수요를 찾을 수 없습니다.");

                if (string.Equals(수요.개별원함원장Id, normalizedLedgerId, StringComparison.Ordinal))
                {
                    return 집단저장계획.보존(변경문서);
                }
                if (!string.IsNullOrWhiteSpace(수요.개별원함원장Id))
                {
                    throw new InvalidOperationException("이 수요에는 이미 다른 개별 원함 원장이 연결되어 있습니다.");
                }

                수요.개별원함원장Id = normalizedLedgerId;
                수요.갱신시각Utc = now;
                수요.갱신토큰 = ObjectId.GenerateNewId().ToString();
                변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = "IndividualDemandLedgerLinked",
                    요약 = "개별 원함 원장을 자동집단 수요의 원본으로 연결했습니다.",
                    발생시각Utc = now
                });
                변경문서.수정시각Utc = now;
                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        return 응답으로(문서);
    }

    public async Task<공동구매자동집단응답> 개별주문원장연결Async(
        string 자동집단Id,
        string 수요Id,
        string 공동구매주문집계원장Id,
        string 개별주문원장Id,
        string 입고예정원장Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(수요Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(공동구매주문집계원장Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(개별주문원장Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(입고예정원장Id);
        await 인덱스준비Async(cancellationToken);

        var now = DateTime.UtcNow;
        var 문서 = await 집단저장Async(
            자동집단Id.Trim(),
            생성허용: false,
            기존문서 =>
            {
                var 변경문서 = 기존문서
                    ?? throw new InvalidOperationException("개별 주문 원장을 연결할 자동집단을 찾을 수 없습니다.");
                var 수요 = 변경문서.수요목록.FirstOrDefault(x => x.수요Id == 수요Id.Trim())
                    ?? throw new InvalidOperationException("개별 주문 원장을 연결할 주문자 수요를 찾을 수 없습니다.");

                변경문서.공동구매주문집계원장Id = 공동구매주문집계원장Id.Trim();
                수요.공동구매주문집계원장Id = 공동구매주문집계원장Id.Trim();
                수요.개별주문원장Id = 개별주문원장Id.Trim();
                수요.입고예정원장Id = 입고예정원장Id.Trim();
                수요.입고의미상태 = 공동구매개별주문입고상태코드.입고예정;
                수요.갱신시각Utc = now;
                수요.갱신토큰 = ObjectId.GenerateNewId().ToString();
                변경문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = "IndividualOrderInboundPlanned",
                    요약 = $"{수요.주문자표시명} 개별 주문을 공동구매 주문집계와 도착 창고 입고 예정 원장에 연결했습니다.",
                    발생시각Utc = now
                });
                변경문서.수정시각Utc = now;
                return 집단저장계획.변경(변경문서);
            },
            cancellationToken);

        return 응답으로(문서);
    }

    private async Task<공동구매자동집단문서> 집단저장Async(
        string 자동집단Id,
        bool 생성허용,
        Func<공동구매자동집단문서?, 집단저장계획> 변경,
        CancellationToken cancellationToken)
    {
        var 저장계획 = await 낙관적동시성재시도기.실행Async(
            async token =>
            {
                var 현재문서 = await _컬렉션
                    .Find(x => x.자동집단Id == 자동집단Id)
                    .FirstOrDefaultAsync(token);
                return new 집단문서스냅샷(현재문서, 현재문서?.버전 ?? 0);
            },
            스냅샷 =>
            {
                if (스냅샷.문서 is null && !생성허용)
                {
                    throw new InvalidOperationException("갱신할 자동집단을 찾을 수 없습니다.");
                }

                var 계획 = 변경(스냅샷.문서);
                if (계획.변경됨)
                {
                    계획.문서.버전 = 스냅샷.버전 + 1;
                }

                return 계획;
            },
            async (스냅샷, 계획, token) =>
            {
                if (!계획.변경됨)
                {
                    return true;
                }

                if (스냅샷.문서 is null)
                {
                    try
                    {
                        await _컬렉션.InsertOneAsync(계획.문서, cancellationToken: token);
                        return true;
                    }
                    catch (MongoWriteException ex) when (
                        ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        return false;
                    }
                }

                var 결과 = await _컬렉션.ReplaceOneAsync(
                    버전필터(자동집단Id, 스냅샷.버전),
                    계획.문서,
                    cancellationToken: token);
                return 결과.MatchedCount == 1;
            },
            최대동시성재시도횟수,
            cancellationToken);

        return 저장계획.문서;
    }

    private async Task 중복수요정리Async(
        string 수요출처키,
        CancellationToken cancellationToken)
    {
        for (var 정리시도 = 0; 정리시도 < 최대동시성재시도횟수; 정리시도++)
        {
            var 문서목록 = await _컬렉션
                .Find(Builders<공동구매자동집단문서>.Filter.ElemMatch(
                    x => x.수요목록,
                    x => x.수요출처키 == 수요출처키))
                .ToListAsync(cancellationToken);
            var 위치목록 = 공동구매자동수요동시성정책.수요위치목록(문서목록, 수요출처키);
            if (위치목록.Count <= 1)
            {
                return;
            }

            var 최신위치 = 공동구매자동수요동시성정책.최신수요위치(위치목록)!;
            var 이전위치목록 = 위치목록.Where(x => !같은수요위치(x, 최신위치)).ToArray();
            foreach (var 이전위치 in 이전위치목록)
            {
                await 관찰수요제거Async(이전위치, cancellationToken);
            }
        }

        throw new InvalidOperationException("동시 요청이 계속되어 공동구매 수요의 소속 집단을 확정하지 못했습니다. 다시 시도해 주세요.");
    }

    private async Task 관찰수요제거Async(
        공동구매자동수요위치 이전위치,
        CancellationToken cancellationToken)
    {
        await 낙관적동시성재시도기.실행Async(
            async token =>
            {
                var 현재문서 = await _컬렉션
                    .Find(x => x.자동집단Id == 이전위치.문서.자동집단Id)
                    .FirstOrDefaultAsync(token);
                return new 집단문서스냅샷(현재문서, 현재문서?.버전 ?? 0);
            },
            스냅샷 =>
            {
                if (스냅샷.문서 is null)
                {
                    return 집단제거계획.변경없음;
                }

                var 제거건수 = 스냅샷.문서.수요목록.RemoveAll(x =>
                    x.수요출처키 == 이전위치.수요.수요출처키
                    && x.수요Id == 이전위치.수요.수요Id
                    && x.갱신토큰 == 이전위치.수요.갱신토큰);
                if (제거건수 == 0)
                {
                    return 집단제거계획.변경없음;
                }

                if (스냅샷.문서.수요목록.Count == 0)
                {
                    return new 집단제거계획(true, true, null);
                }

                var now = DateTime.UtcNow;
                스냅샷.문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
                {
                    이벤트유형 = "DemandMoved",
                    요약 = "수요 참여자가 다른 상품 또는 수령 범위로 변경했습니다.",
                    발생시각Utc = now
                });
                재계산(스냅샷.문서, now);
                스냅샷.문서.버전 = 스냅샷.버전 + 1;
                return new 집단제거계획(true, false, 스냅샷.문서);
            },
            async (스냅샷, 계획, token) =>
            {
                if (!계획.변경됨)
                {
                    return true;
                }

                if (계획.삭제)
                {
                    var 삭제결과 = await _컬렉션.DeleteOneAsync(
                        버전필터(이전위치.문서.자동집단Id, 스냅샷.버전),
                        token);
                    return 삭제결과.DeletedCount == 1;
                }

                var 저장결과 = await _컬렉션.ReplaceOneAsync(
                    버전필터(이전위치.문서.자동집단Id, 스냅샷.버전),
                    계획.문서!,
                    cancellationToken: token);
                return 저장결과.MatchedCount == 1;
            },
            최대동시성재시도횟수,
            cancellationToken);
    }

    private static FilterDefinition<공동구매자동집단문서> 버전필터(
        string 자동집단Id,
        long 버전)
    {
        var builder = Builders<공동구매자동집단문서>.Filter;
        var 버전조건 = builder.Eq(x => x.버전, 버전);
        if (버전 == 0)
        {
            버전조건 |= builder.Exists(x => x.버전, false);
        }

        return builder.Eq(x => x.자동집단Id, 자동집단Id) & 버전조건;
    }

    private static 공동구매자동수요문서? 더최근수요(
        공동구매자동수요문서? 첫번째,
        공동구매자동수요문서? 두번째)
    {
        if (첫번째 is null)
        {
            return 두번째;
        }

        if (두번째 is null)
        {
            return 첫번째;
        }

        var 시각비교 = 수요갱신시각(첫번째).CompareTo(수요갱신시각(두번째));
        if (시각비교 != 0)
        {
            return 시각비교 > 0 ? 첫번째 : 두번째;
        }

        return string.CompareOrdinal(첫번째.갱신토큰, 두번째.갱신토큰) >= 0
            ? 첫번째
            : 두번째;
    }

    private static DateTime 수요갱신시각(공동구매자동수요문서 수요)
        => 수요.갱신시각Utc == default ? 수요.생성시각Utc : 수요.갱신시각Utc;

    private static bool 같은수요위치(공동구매자동수요위치 첫번째, 공동구매자동수요위치 두번째)
        => 첫번째.문서.자동집단Id == 두번째.문서.자동집단Id
           && 첫번째.수요.수요Id == 두번째.수요.수요Id
           && 첫번째.수요.갱신토큰 == 두번째.수요.갱신토큰;

    private void 재계산(공동구매자동집단문서 문서, DateTime now)
    {
        var 모집종료시각Utc = 모집종료시각(문서, now);
        문서.모집종료시각Utc = 모집종료시각Utc;
        var 활성수요목록 = 문서.수요목록.Where(활성수요인가).ToArray();
        문서.예약결제합계 = 활성수요목록.Sum(x => Math.Max(0, x.예약결제금액 ?? 0));
        문서.목표참여자수 = 활성수요목록
            .Where(x => x.목표참여자수 is > 0)
            .Select(x => x.목표참여자수)
            .Min();
        문서.목표수량 = 활성수요목록
            .Where(x => x.목표수량 is > 0)
            .Select(x => x.목표수량)
            .Min();

        var 이전상태 = 문서.현재상태;
        var 진행 = _집단화Engine.진행계산(
            활성수요목록.Select(x => 응답으로(x, 문서.자동집단Id)).ToArray(),
            문서.목표참여자수,
            문서.목표수량,
            문서.현재상태,
            모집종료시각Utc,
            now,
            문서.거래유형);
        문서.현재상태 = 진행.현재상태;
        문서.수요건수 = 진행.수요건수;
        문서.예약결제건수 = 진행.예약결제건수;
        문서.참여자수 = 진행.참여자수;
        문서.예약결제참여자수 = 진행.예약결제참여자수;
        문서.총희망수량 = 진행.총희망수량;
        문서.수량단위 = string.IsNullOrWhiteSpace(진행.수량단위) ? "kg" : 진행.수량단위;
        if (이전상태 != 문서.현재상태)
        {
            문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
            {
                이벤트유형 = "AutoGroupStatusChanged",
                요약 = $"자동 주문자 집단 상태가 {문서.현재상태}(으)로 변경되었습니다.",
                발생시각Utc = now
            });
        }

        문서.수정시각Utc = now;
    }

    private async Task 인덱스준비Async(CancellationToken cancellationToken)
    {
        if (_인덱스준비됨)
        {
            return;
        }

        await _인덱스Lock.WaitAsync(cancellationToken);
        try
        {
            if (_인덱스준비됨)
            {
                return;
            }

            var indexes = new[]
            {
                new CreateIndexModel<공동구매자동집단문서>(
                    Builders<공동구매자동집단문서>.IndexKeys.Ascending(x => x.자동집단Id),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<공동구매자동집단문서>(
                    Builders<공동구매자동집단문서>.IndexKeys
                        .Ascending(x => x.상품키)
                        .Ascending(x => x.배송권키)
                        .Ascending(x => x.거래유형)
                        .Ascending(x => x.현재상태)
                        .Descending(x => x.수정시각Utc)),
                new CreateIndexModel<공동구매자동집단문서>(
                    Builders<공동구매자동집단문서>.IndexKeys
                        .Ascending("수요목록.수요출처키")),
                new CreateIndexModel<공동구매자동집단문서>(
                    Builders<공동구매자동집단문서>.IndexKeys
                        .Ascending(x => x.현재상태)
                        .Ascending(x => x.다음Os운영점검시각Utc)
                        .Ascending(x => x.모집종료시각Utc))
            };

            await _컬렉션.Indexes.CreateManyAsync(indexes, cancellationToken);
            _인덱스준비됨 = true;
        }
        finally
        {
            _인덱스Lock.Release();
        }
    }

    private static void 검증(공동구매자동수요등록Command command)
    {
        if (!string.IsNullOrWhiteSpace(command.요청멱등키)
            && command.요청멱등키.Trim().Length > 160)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자 이하여야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.상품키) ||
            string.IsNullOrWhiteSpace(command.상품명) ||
            string.IsNullOrWhiteSpace(command.배송권키))
        {
            throw new InvalidOperationException("상품키, 상품명, 배송권키를 입력해야 합니다.");
        }

        if (command.희망수량 <= 0)
        {
            throw new InvalidOperationException("희망수량은 0보다 커야 합니다.");
        }

        공동구매주문자집단화Engine.거래문맥검증(command);

        if (주문확정수요인가(command))
        {
            if (string.IsNullOrWhiteSpace(command.주문자키))
            {
                throw new InvalidOperationException("예약 결제 수요에는 주문자 식별키가 필요합니다.");
            }

            if (command.도착창고Id is not > 0)
            {
                throw new InvalidOperationException("예약 결제 수요에는 실물 또는 가상 도착 창고가 필요합니다.");
            }

            if (string.IsNullOrWhiteSpace(command.커뮤니티원장Id))
            {
                throw new InvalidOperationException("예약 결제 수요를 연결할 공동구매 원장 ID가 필요합니다.");
            }
        }
    }

    private static void 철회검증(공동구매자동수요철회Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.수요출처키)
            || string.IsNullOrWhiteSpace(command.주문자키)
            || string.IsNullOrWhiteSpace(command.요청멱등키))
        {
            throw new InvalidOperationException("수요출처키, 주문자 식별키와 요청 멱등 키를 입력해야 합니다.");
        }

        if (command.요청멱등키.Trim().Length > 160
            || command.수요출처키.Trim().Length > 200)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자, 수요출처키는 200자 이하여야 합니다.");
        }
    }

    private static void 본인수요검증(공동구매자동수요문서 수요, string 주문자키)
    {
        if (!string.Equals(수요.주문자키, 주문자키, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("다른 주문자의 공동구매 수요는 변경할 수 없습니다.");
        }
    }

    private 공동구매자동수요철회응답 철회응답으로(
        공동구매자동집단문서 문서,
        공동구매자동수요문서 수요,
        string 요청멱등키,
        bool 이미처리됨)
    {
        var 철회완료 = !활성수요인가(수요);
        var 집단응답 = 응답으로(문서);
        return new 공동구매자동수요철회응답
        {
            요청멱등키 = 요청멱등키,
            수요출처키 = 수요.수요출처키,
            자동집단Id = 문서.자동집단Id,
            철회완료 = 철회완료,
            이미처리됨 = 이미처리됨,
            남은수요건수 = 집단응답.수요건수,
            남은참여자수 = 집단응답.참여자수,
            남은희망수량 = 집단응답.총희망수량,
            현재상태 = 집단응답.현재상태,
            철회시각Utc = 수요.철회시각Utc ?? default,
            안내 = 철회완료
                ? 이미처리됨
                    ? "이미 철회된 비구속 수요입니다. 추가 주문·결제·운송 작업은 실행되지 않았습니다."
                    : "비구속 수요를 철회했습니다. 추가 주문·결제·운송 작업은 실행되지 않았습니다."
                : "같은 멱등 요청은 처리되었지만 이후 수요가 다시 등록되어 현재는 활성 상태입니다."
        };
    }

    private 공동구매자동집단응답 응답으로(공동구매자동집단문서 문서)
    {
        var 기준시각Utc = DateTime.UtcNow;
        var 모집종료시각Utc = 모집종료시각(문서, 기준시각Utc);
        var 수요목록 = 문서.수요목록
            .Where(활성수요인가)
            .OrderByDescending(x => x.생성시각Utc)
            .Select(x => 응답으로(x, 문서.자동집단Id))
            .ToArray();
        var 진행 = _집단화Engine.진행계산(
            수요목록,
            문서.목표참여자수,
            문서.목표수량,
            문서.현재상태,
            모집종료시각Utc,
            기준시각Utc,
            문서.거래유형);

        return new 공동구매자동집단응답
        {
            자동집단Id = 문서.자동집단Id,
            공동구매주문집계원장Id = 문서.공동구매주문집계원장Id,
            상품키 = 문서.상품키,
            상품명 = 문서.상품명,
            HS코드 = 문서.HS코드,
            온도코드 = 문서.온도코드,
            물류방식 = 문서.물류방식,
            거래유형 = 공동구매거래유형코드.정규화(문서.거래유형),
            가격표시기준 = 공동구매가격표시기준코드.정규화(문서.가격표시기준, 문서.거래유형),
            배송권키 = 문서.배송권키,
            배송권명 = 문서.배송권명,
            현재상태 = 진행.현재상태,
            수요건수 = 진행.수요건수,
            예약결제건수 = 진행.예약결제건수,
            참여자수 = 진행.참여자수,
            예약결제참여자수 = 진행.예약결제참여자수,
            총희망수량 = 진행.총희망수량,
            수량단위 = string.IsNullOrWhiteSpace(진행.수량단위) ? 문서.수량단위 : 진행.수량단위,
            예약결제합계 = 문서.예약결제합계,
            목표참여자수 = 진행.목표참여자수,
            목표수량 = 진행.목표수량,
            모집종료시각Utc = 진행.모집종료시각Utc,
            모집종료여부 = 진행.모집종료여부,
            모집조건충족여부 = 진행.모집조건충족여부,
            생성시각Utc = 문서.생성시각Utc,
            수정시각Utc = 문서.수정시각Utc,
            수요목록 = 수요목록,
            이벤트목록 = 문서.이벤트목록.OrderByDescending(x => x.발생시각Utc).Take(20).Select(응답으로).ToArray()
        };
    }

    private static 공동구매수요모집Os상태응답 Os상태응답으로(공동구매자동집단문서 문서)
    {
        var 실행모드 = string.IsNullOrWhiteSpace(문서.실행모드)
            ? "Simulation"
            : 문서.실행모드;
        var 인계상태 = string.IsNullOrWhiteSpace(문서.인계상태)
            ? 문서.현재상태 switch
            {
                공동구매자동집단상태코드.확정 => string.IsNullOrWhiteSpace(문서.인계요청Id)
                    ? 공동구매수요모집인계상태코드.승인대기
                    : 공동구매수요모집인계상태코드.승인후속대기,
                공동구매자동집단상태코드.확정대기 => 공동구매수요모집인계상태코드.승인대기,
                _ => 공동구매수요모집인계상태코드.미요청
            }
            : 문서.인계상태;

        return new 공동구매수요모집Os상태응답
        {
            운영체제Id = string.IsNullOrWhiteSpace(문서.운영체제Id)
                ? OperatingSystemIds.GroupPurchaseDemand
                : 문서.운영체제Id,
            정책버전 = string.IsNullOrWhiteSpace(문서.Os정책버전) ? "1.0" : 문서.Os정책버전,
            자동집단Id = 문서.자동집단Id,
            집단상태 = 문서.현재상태,
            현재큐 = string.IsNullOrWhiteSpace(문서.현재Os큐)
                ? Os큐코드(문서)
                : 문서.현재Os큐,
            마지막트리거 = 문서.마지막Os트리거,
            적용정책코드목록 = 문서.적용Os정책코드목록.ToArray(),
            마지막조율시각Utc = 문서.마지막Os조율시각Utc == default
                ? null
                : 문서.마지막Os조율시각Utc,
            다음운영점검시각Utc = 문서.다음Os운영점검시각Utc == default
                || 문서.다음Os운영점검시각Utc >= Os점검없음Utc
                    ? null
                    : 문서.다음Os운영점검시각Utc,
            인계상태 = 인계상태,
            인계요청Id = 문서.인계요청Id,
            대상운영체제Id = string.IsNullOrWhiteSpace(문서.대상운영체제Id)
                ? OperatingSystemIds.GroupPurchaseImport
                : 문서.대상운영체제Id,
            대상워크플로우코드 = string.IsNullOrWhiteSpace(문서.대상워크플로우코드)
                ? "GroupPurchaseImport"
                : 문서.대상워크플로우코드,
            대상원장Id = 문서.대상원장Id,
            승인자키 = 문서.승인자키,
            승인시각Utc = 문서.승인시각Utc,
            승인사유 = 문서.승인사유,
            실행모드 = 실행모드,
            시뮬레이션여부 = string.Equals(실행모드, "Simulation", StringComparison.OrdinalIgnoreCase),
            후속워크플로우활성여부 = 문서.후속워크플로우활성여부
        };
    }

    private static string Os큐코드(공동구매자동집단문서 문서)
        => 문서.현재상태 switch
        {
            공동구매자동집단상태코드.확정대기 => 공동구매수요모집Os큐코드.확정검토,
            공동구매자동집단상태코드.모집종료목표미달 => 공동구매수요모집Os큐코드.모집종료,
            공동구매자동집단상태코드.확정 =>
                문서.인계상태 == 공동구매수요모집인계상태코드.승인후속대기
                && !string.IsNullOrWhiteSpace(문서.인계요청Id)
                    ? 공동구매수요모집Os큐코드.인계준비
                    : 공동구매수요모집Os큐코드.확정검토,
            _ => 공동구매수요모집Os큐코드.모집중
        };

    private static DateTime 다음운영점검시각(
        공동구매자동집단문서 문서,
        DateTime 기준시각Utc,
        TimeSpan 장기모집점검주기)
    {
        if (문서.현재상태 != 공동구매자동집단상태코드.수요수집중)
        {
            return Os점검없음Utc;
        }

        var 점검주기 = 장기모집점검주기 < TimeSpan.FromHours(1)
            ? TimeSpan.FromHours(1)
            : 장기모집점검주기;
        var 장기모집점검시각Utc = 기준시각Utc.Add(점검주기);
        var 모집마감시각Utc = 모집종료시각(문서, 기준시각Utc);
        return 모집마감시각Utc <= 장기모집점검시각Utc
            ? 모집마감시각Utc
            : 장기모집점검시각Utc;
    }

    private static 공동구매자동수요응답 응답으로(공동구매자동수요문서 문서, string 자동집단Id)
    {
        return new 공동구매자동수요응답
        {
            수요Id = 문서.수요Id,
            수요출처키 = 문서.수요출처키,
            커뮤니티게시글Id = 문서.커뮤니티게시글Id,
            커뮤니티원장Id = 문서.커뮤니티원장Id,
            개별원함원장Id = 문서.개별원함원장Id,
            자동집단Id = 자동집단Id,
            상품키 = 문서.상품키,
            상품명 = 문서.상품명,
            거래유형 = 공동구매거래유형코드.정규화(문서.거래유형),
            가격표시기준 = 공동구매가격표시기준코드.정규화(문서.가격표시기준, 문서.거래유형),
            구매조직참조키 = 문서.구매조직참조키,
            구매조직표시명 = 문서.구매조직표시명,
            사업자검증상태 = string.IsNullOrWhiteSpace(문서.사업자검증상태)
                ? 공동구매거래유형코드.정규화(문서.거래유형) == 공동구매거래유형코드.B2B
                    ? 주문자집단사업자검증상태코드.필요
                    : 주문자집단사업자검증상태코드.불필요
                : 문서.사업자검증상태,
            세금계산서필요 = 문서.세금계산서필요,
            주문자키 = 문서.주문자키,
            주문자표시명 = 문서.주문자표시명,
            배송권키 = 문서.배송권키,
            배송권명 = 문서.배송권명,
            도착창고Id = 문서.도착창고Id,
            도착창고유형 = 문서.도착창고유형,
            도착창고명 = 문서.도착창고명,
            수령지주소참조키 = 문서.수령지주소참조키,
            입고의미상태 = 문서.입고의미상태,
            공동구매주문집계원장Id = 문서.공동구매주문집계원장Id,
            개별주문원장Id = 문서.개별주문원장Id,
            입고예정원장Id = 문서.입고예정원장Id,
            수요유형 = 문서.수요유형,
            결제상태 = 문서.결제상태,
            희망수량 = 문서.희망수량,
            수량단위 = 문서.수량단위,
            예약결제금액 = 문서.예약결제금액,
            목표참여자수 = 문서.목표참여자수,
            목표수량 = 문서.목표수량,
            생성시각Utc = 문서.생성시각Utc
        };
    }

    private static 공동구매자동집단이벤트응답 응답으로(공동구매자동집단이벤트문서 문서)
    {
        return new 공동구매자동집단이벤트응답
        {
            이벤트유형 = 문서.이벤트유형,
            요약 = 문서.요약,
            발생시각Utc = 문서.발생시각Utc
        };
    }

    private static string 수요유형정규화(string? 값)
    {
        return 값 is 공동구매자동수요유형코드.예약결제
            ? 공동구매자동수요유형코드.예약결제
            : 공동구매자동수요유형코드.관심표시;
    }

    private static string 결제상태정규화(string? 값)
    {
        return 값 switch
        {
            공동구매자동결제상태코드.예약됨 => 공동구매자동결제상태코드.예약됨,
            공동구매자동결제상태코드.결제확정 => 공동구매자동결제상태코드.결제확정,
            _ => 공동구매자동결제상태코드.미결제
        };
    }

    private static bool 주문확정수요인가(공동구매자동수요등록Command command)
        => command.수요유형 == 공동구매자동수요유형코드.예약결제
           || command.결제상태 is 공동구매자동결제상태코드.예약됨
               or 공동구매자동결제상태코드.결제확정;

    private static bool 활성수요인가(공동구매자동수요문서 수요)
        => 수요.상태 != 공동구매자동수요상태코드.철회;

    private static bool 비구속수요인가(공동구매자동수요문서 수요)
        => 수요.수요유형 == 공동구매자동수요유형코드.관심표시
           && 수요.결제상태 == 공동구매자동결제상태코드.미결제
           && string.IsNullOrWhiteSpace(수요.개별주문원장Id)
           && string.IsNullOrWhiteSpace(수요.입고예정원장Id);

    private static void 수요저장가능검증(공동구매자동집단문서? 문서, DateTime 기준시각Utc)
    {
        if (문서 is null)
        {
            return;
        }

        if (문서.현재상태 == 공동구매자동집단상태코드.확정)
        {
            throw new InvalidOperationException("확정된 공동구매에는 수요를 등록하거나 변경할 수 없습니다.");
        }

        if (문서.현재상태 == 공동구매자동집단상태코드.모집종료목표미달
            || 기준시각Utc >= 모집종료시각(문서, 기준시각Utc))
        {
            throw new InvalidOperationException("모집이 종료된 자동집단에는 수요를 등록하거나 변경할 수 없습니다.");
        }
    }

    private static DateTime 모집종료시각(공동구매자동집단문서 문서, DateTime 기준시각Utc)
    {
        if (문서.모집종료시각Utc != default)
        {
            return Utc시각(문서.모집종료시각Utc);
        }

        var 시작시각Utc = 문서.생성시각Utc == default
            ? Utc시각(기준시각Utc)
            : Utc시각(문서.생성시각Utc);
        return 공동구매자동집단모집정책.기본모집종료시각Utc(시작시각Utc);
    }

    private static DateTime Utc시각(DateTime 값)
        => 값.Kind switch
        {
            DateTimeKind.Utc => 값,
            DateTimeKind.Local => 값.ToUniversalTime(),
            _ => DateTime.SpecifyKind(값, DateTimeKind.Utc)
        };

    private static string 정규화(string? 값, string 기본값, int 최대길이)
    {
        var 정규화값 = string.IsNullOrWhiteSpace(값) ? 기본값 : 값.Trim();
        return 정규화값.Length <= 최대길이 ? 정규화값 : 정규화값[..최대길이];
    }

    private static int? 양수값(int? 값) => 값 is > 0 ? 값 : null;

    private static decimal? 양수값(decimal? 값) => 값 is > 0 ? 값 : null;

    private sealed record 집단문서스냅샷(공동구매자동집단문서? 문서, long 버전);

    private sealed record 집단저장계획(bool 변경됨, 공동구매자동집단문서 문서)
    {
        public static 집단저장계획 변경(공동구매자동집단문서 문서) => new(true, 문서);

        public static 집단저장계획 보존(공동구매자동집단문서 문서) => new(false, 문서);
    }

    private sealed record 집단제거계획(bool 변경됨, bool 삭제, 공동구매자동집단문서? 문서)
    {
        public static 집단제거계획 변경없음 { get; } = new(false, false, null);
    }

}

internal sealed record 공동구매자동수요위치(
    공동구매자동집단문서 문서,
    공동구매자동수요문서 수요);

internal static class 공동구매자동수요동시성정책
{
    internal static IReadOnlyList<공동구매자동수요위치> 수요위치목록(
        IEnumerable<공동구매자동집단문서> 문서목록,
        string 수요출처키)
        => 문서목록
            .SelectMany(문서 => 문서.수요목록
                .Where(수요 => 수요.수요출처키 == 수요출처키)
                .Select(수요 => new 공동구매자동수요위치(문서, 수요)))
            .ToArray();

    internal static 공동구매자동수요위치? 최신수요위치(
        IEnumerable<공동구매자동집단문서> 문서목록,
        string 수요출처키)
        => 최신수요위치(수요위치목록(문서목록, 수요출처키));

    internal static 공동구매자동수요위치? 최신수요위치(
        IEnumerable<공동구매자동수요위치> 위치목록)
        => 위치목록
            .OrderByDescending(x => 갱신시각(x.수요))
            .ThenByDescending(x => x.수요.갱신토큰, StringComparer.Ordinal)
            .ThenByDescending(x => x.문서.자동집단Id, StringComparer.Ordinal)
            .FirstOrDefault();

    internal static bool 기존수요보존(
        공동구매자동수요문서 기존수요,
        DateTime 요청갱신시각Utc,
        string 요청갱신토큰)
    {
        var 기존갱신시각Utc = 기존수요.갱신시각Utc == default
            ? 기존수요.생성시각Utc
            : 기존수요.갱신시각Utc;
        var 시각비교 = 기존갱신시각Utc.CompareTo(요청갱신시각Utc);
        if (시각비교 != 0)
        {
            return 시각비교 > 0;
        }

        return string.CompareOrdinal(기존수요.갱신토큰, 요청갱신토큰) >= 0;
    }

    private static DateTime 갱신시각(공동구매자동수요문서 수요)
        => 수요.갱신시각Utc == default ? 수요.생성시각Utc : 수요.갱신시각Utc;
}

internal static class 낙관적동시성재시도기
{
    internal static async Task<TResult> 실행Async<TSnapshot, TResult>(
        Func<CancellationToken, Task<TSnapshot>> 다시읽기,
        Func<TSnapshot, TResult> 변경,
        Func<TSnapshot, TResult, CancellationToken, Task<bool>> 조건부저장,
        int 최대시도횟수,
        CancellationToken cancellationToken,
        Func<int, CancellationToken, Task>? 충돌대기 = null)
    {
        ArgumentNullException.ThrowIfNull(다시읽기);
        ArgumentNullException.ThrowIfNull(변경);
        ArgumentNullException.ThrowIfNull(조건부저장);
        ArgumentOutOfRangeException.ThrowIfLessThan(최대시도횟수, 1);

        for (var 시도 = 1; 시도 <= 최대시도횟수; 시도++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var 스냅샷 = await 다시읽기(cancellationToken);
            var 변경결과 = 변경(스냅샷);
            if (await 조건부저장(스냅샷, 변경결과, cancellationToken))
            {
                return 변경결과;
            }

            if (시도 < 최대시도횟수)
            {
                if (충돌대기 is not null)
                {
                    await 충돌대기(시도, cancellationToken);
                }
                else
                {
                    var 대기밀리초 = Math.Min(50, 1 << Math.Min(시도, 5));
                    await Task.Delay(TimeSpan.FromMilliseconds(대기밀리초), cancellationToken);
                }
            }
        }

        throw new InvalidOperationException("동시 갱신 충돌이 반복되어 자동집단을 저장하지 못했습니다. 다시 시도해 주세요.");
    }
}

internal static class 공동구매자동수요명령유형코드
{
    internal const string 저장 = "UpsertDemand";
    internal const string 철회 = "WithdrawDemand";
}

internal static class 공동구매자동수요멱등정책
{
    private const int 최대명령기록수 = 32;

    internal static string 저장요청지문(공동구매자동수요등록Command command)
        => 지문(
            command.수요출처키,
            command.커뮤니티게시글Id?.ToString(CultureInfo.InvariantCulture),
            command.커뮤니티원장Id,
            command.상품키,
            command.상품명,
            command.HS코드,
            command.온도코드,
            command.물류방식,
            command.거래유형,
            command.가격표시기준,
            command.구매조직참조키,
            command.구매조직표시명,
            command.세금계산서필요.ToString(),
            command.주문자키,
            command.주문자표시명,
            command.배송권키,
            command.배송권명,
            command.도착창고Id?.ToString(CultureInfo.InvariantCulture),
            command.도착창고유형,
            command.도착창고명,
            command.수령지주소참조키,
            command.수령지표시명,
            command.수령도로명주소,
            command.수령상세주소,
            command.희망수량.ToString("G29", CultureInfo.InvariantCulture),
            command.수량단위,
            command.예약결제금액?.ToString("G29", CultureInfo.InvariantCulture),
            command.수요유형,
            command.결제상태,
            command.메모,
            command.목표참여자수?.ToString(CultureInfo.InvariantCulture),
            command.목표수량?.ToString("G29", CultureInfo.InvariantCulture));

    internal static string 철회요청지문(공동구매자동수요철회Command command)
        => 지문(command.수요출처키, command.주문자키, command.철회사유);

    internal static bool 이미처리됨(
        IEnumerable<공동구매자동수요명령문서> 명령목록,
        string 요청멱등키,
        string 명령유형,
        string 요청지문)
    {
        var 기존명령 = 명령목록.FirstOrDefault(x =>
            string.Equals(x.요청멱등키, 요청멱등키, StringComparison.Ordinal));
        if (기존명령 is null)
        {
            return false;
        }

        if (!string.Equals(기존명령.명령유형, 명령유형, StringComparison.Ordinal)
            || !string.Equals(기존명령.요청지문, 요청지문, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("같은 멱등 키를 다른 공동구매 수요 명령에 재사용할 수 없습니다.");
        }

        return true;
    }

    internal static void 기록추가(
        List<공동구매자동수요명령문서> 명령목록,
        string 요청멱등키,
        string 명령유형,
        string 요청지문,
        DateTime 처리시각Utc)
    {
        if (이미처리됨(명령목록, 요청멱등키, 명령유형, 요청지문))
        {
            return;
        }

        명령목록.Add(new 공동구매자동수요명령문서
        {
            요청멱등키 = 요청멱등키,
            명령유형 = 명령유형,
            요청지문 = 요청지문,
            처리시각Utc = 처리시각Utc
        });
        if (명령목록.Count > 최대명령기록수)
        {
            명령목록.RemoveRange(0, 명령목록.Count - 최대명령기록수);
        }
    }

    private static string 지문(params string?[] 값목록)
    {
        var 원문 = string.Join('\u001f', 값목록.Select(x => x?.Trim() ?? string.Empty));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(원문)));
    }
}

public sealed class 공동구매자동집단문서
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string 자동집단Id { get; set; } = string.Empty;
    public string 공동구매주문집계원장Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 현재상태 { get; set; } = 공동구매자동집단상태코드.수요수집중;
    public int 수요건수 { get; set; }
    public int 예약결제건수 { get; set; }
    public int 참여자수 { get; set; }
    public int 예약결제참여자수 { get; set; }
    public decimal 총희망수량 { get; set; }
    public string 수량단위 { get; set; } = "kg";
    public decimal 예약결제합계 { get; set; }
    public int? 목표참여자수 { get; set; }
    public decimal? 목표수량 { get; set; }
    public DateTime 모집종료시각Utc { get; set; }
    public string 운영체제Id { get; set; } = string.Empty;
    public string Os정책버전 { get; set; } = string.Empty;
    public string 현재Os큐 { get; set; } = string.Empty;
    public string 마지막Os트리거 { get; set; } = string.Empty;
    public List<string> 적용Os정책코드목록 { get; set; } = [];
    public DateTime 마지막Os조율시각Utc { get; set; }
    public DateTime 다음Os운영점검시각Utc { get; set; }
    public List<string> 최근Os조율멱등키목록 { get; set; } = [];
    public string 인계상태 { get; set; } = string.Empty;
    public string 인계요청Id { get; set; } = string.Empty;
    public string 인계승인멱등키 { get; set; } = string.Empty;
    public string 대상운영체제Id { get; set; } = string.Empty;
    public string 대상워크플로우코드 { get; set; } = string.Empty;
    public string 대상원장Id { get; set; } = string.Empty;
    public string 승인자키 { get; set; } = string.Empty;
    public DateTime? 승인시각Utc { get; set; }
    public string 승인사유 { get; set; } = string.Empty;
    public string 실행모드 { get; set; } = string.Empty;
    public bool 후속워크플로우활성여부 { get; set; }
    public List<공동구매자동수요문서> 수요목록 { get; set; } = [];
    public List<공동구매자동집단이벤트문서> 이벤트목록 { get; set; } = [];
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
    public long 버전 { get; set; }
}

public sealed class 공동구매자동수요문서
{
    public string 수요Id { get; set; } = string.Empty;
    public string 수요출처키 { get; set; } = string.Empty;
    public long? 커뮤니티게시글Id { get; set; }
    public string 커뮤니티원장Id { get; set; } = string.Empty;
    public string 개별원함원장Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string 구매조직참조키 { get; set; } = string.Empty;
    public string 구매조직표시명 { get; set; } = string.Empty;
    public string 사업자검증상태 { get; set; } = 주문자집단사업자검증상태코드.불필요;
    public bool 세금계산서필요 { get; set; }
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 주문자키 { get; set; } = string.Empty;
    public string 주문자표시명 { get; set; } = string.Empty;
    public long? 도착창고Id { get; set; }
    public string 도착창고유형 { get; set; } = string.Empty;
    public string 도착창고명 { get; set; } = string.Empty;
    public string 수령지주소참조키 { get; set; } = string.Empty;
    public string 입고의미상태 { get; set; } = 공동구매개별주문입고상태코드.미지정;
    public string 공동구매주문집계원장Id { get; set; } = string.Empty;
    public string 개별주문원장Id { get; set; } = string.Empty;
    public string 입고예정원장Id { get; set; } = string.Empty;
    public string 수요유형 { get; set; } = 공동구매자동수요유형코드.관심표시;
    public string 결제상태 { get; set; } = 공동구매자동결제상태코드.미결제;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = "kg";
    public decimal? 예약결제금액 { get; set; }
    public string 메모 { get; set; } = string.Empty;
    public int? 목표참여자수 { get; set; }
    public decimal? 목표수량 { get; set; }
    public string 상태 { get; set; } = 공동구매자동수요상태코드.활성;
    public DateTime? 철회시각Utc { get; set; }
    public string 철회사유 { get; set; } = string.Empty;
    public List<공동구매자동수요명령문서> 명령목록 { get; set; } = [];
    public DateTime 생성시각Utc { get; set; }
    public DateTime 갱신시각Utc { get; set; }
    public string 갱신토큰 { get; set; } = string.Empty;
}

public sealed class 공동구매자동수요명령문서
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public string 명령유형 { get; set; } = string.Empty;
    public string 요청지문 { get; set; } = string.Empty;
    public DateTime 처리시각Utc { get; set; }
}

public sealed class 공동구매자동집단이벤트문서
{
    public string 이벤트유형 { get; set; } = string.Empty;
    public string 요약 { get; set; } = string.Empty;
    public DateTime 발생시각Utc { get; set; }
}
