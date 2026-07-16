using Hongdal.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Hongdal.Services.Orderer;

public interface I공동구매자동집단화저장소
{
    Task<공동구매자동집단응답> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단응답?> 집단조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단응답> 개별주문원장연결Async(
        string 자동집단Id,
        string 수요Id,
        string 개별주문원장Id,
        string 입고예정원장Id,
        CancellationToken cancellationToken = default);
}

public sealed class Mongo공동구매자동집단화저장소 : I공동구매자동집단화저장소
{
    private const string 컬렉션명 = "orderer_group_purchase_auto_groups";
    private readonly IMongoCollection<공동구매자동집단문서> _컬렉션;
    private readonly SemaphoreSlim _인덱스Lock = new(1, 1);
    private bool _인덱스준비됨;

    public Mongo공동구매자동집단화저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _컬렉션 = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<공동구매자동집단문서>(컬렉션명);
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
        var 물류방식 = 정규화(command.물류방식, "LCL", 40);
        var 수요출처키 = 정규화(
            command.수요출처키,
            $"orderer:{정규화(command.주문자키, "anonymous-orderer", 120)}",
            200);
        var 자동집단Id = 공동구매자동집단화계획기.자동집단키생성(
            상품키,
            배송권키,
            온도코드,
            물류방식);

        var 기존출처문서목록 = await _컬렉션
            .Find(Builders<공동구매자동집단문서>.Filter.ElemMatch(
                x => x.수요목록,
                x => x.수요출처키 == 수요출처키))
            .ToListAsync(cancellationToken);
        var 기존문서 = 기존출처문서목록.FirstOrDefault(x => x.자동집단Id == 자동집단Id)
            ?? await _컬렉션
                .Find(x => x.자동집단Id == 자동집단Id)
                .FirstOrDefaultAsync(cancellationToken);
        var 기존수요 = 기존문서?.수요목록.FirstOrDefault(x => x.수요출처키 == 수요출처키);

        var 수요 = new 공동구매자동수요문서
        {
            수요Id = 기존수요?.수요Id ?? ObjectId.GenerateNewId().ToString(),
            수요출처키 = 수요출처키,
            커뮤니티게시글Id = command.커뮤니티게시글Id,
            커뮤니티원장Id = 정규화(command.커뮤니티원장Id, string.Empty, 200),
            상품키 = 상품키,
            상품명 = 정규화(command.상품명, 상품키, 160),
            배송권키 = 배송권키,
            배송권명 = 정규화(command.배송권명, 배송권키, 160),
            주문자키 = 정규화(command.주문자키, "anonymous-orderer", 120),
            주문자표시명 = 정규화(command.주문자표시명, "주문자", 80),
            도착창고Id = command.도착창고Id is > 0 ? command.도착창고Id : null,
            도착창고유형 = 정규화(command.도착창고유형, string.Empty, 50),
            도착창고명 = 정규화(command.도착창고명, string.Empty, 200),
            수령지주소참조키 = 정규화(command.수령지주소참조키, string.Empty, 200),
            입고의미상태 = 주문확정수요인가(command)
                ? 공동구매개별주문입고상태코드.입고예정
                : 기존수요?.입고의미상태 ?? 공동구매개별주문입고상태코드.미지정,
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
            생성시각Utc = now
        };

        var 문서 = 기존문서 ?? new 공동구매자동집단문서
        {
            Id = ObjectId.GenerateNewId(),
            자동집단Id = 자동집단Id,
            상품키 = 상품키,
            상품명 = 수요.상품명,
            HS코드 = 정규화(command.HS코드, string.Empty, 20),
            온도코드 = 온도코드,
            물류방식 = 물류방식,
            배송권키 = 배송권키,
            배송권명 = 수요.배송권명,
            현재상태 = 공동구매자동집단상태코드.수요수집중,
            생성시각Utc = now
        };

        문서.수요목록.RemoveAll(x => x.수요출처키 == 수요출처키);
        문서.수요목록.Add(수요);
        문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
        {
            이벤트유형 = 기존수요 is null ? "DemandRegistered" : "DemandUpdated",
            요약 = $"{수요.주문자표시명} 수요가 {수요.희망수량:N0}{수요.수량단위} {(기존수요 is null ? "등록" : "변경")}되었습니다.",
            발생시각Utc = now
        });
        재계산(문서, now);

        foreach (var 이전문서 in 기존출처문서목록.Where(x => x.자동집단Id != 자동집단Id))
        {
            이전문서.수요목록.RemoveAll(x => x.수요출처키 == 수요출처키);
            if (이전문서.수요목록.Count == 0)
            {
                await _컬렉션.DeleteOneAsync(x => x.자동집단Id == 이전문서.자동집단Id, cancellationToken);
                continue;
            }

            이전문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
            {
                이벤트유형 = "DemandMoved",
                요약 = "수요 참여자가 다른 상품 또는 수령 범위로 변경했습니다.",
                발생시각Utc = now
            });
            재계산(이전문서, now);
            await _컬렉션.ReplaceOneAsync(
                x => x.자동집단Id == 이전문서.자동집단Id,
                이전문서,
                cancellationToken: cancellationToken);
        }

        await _컬렉션.ReplaceOneAsync(
            x => x.자동집단Id == 문서.자동집단Id,
            문서,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return 응답으로(문서);
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

        if (!string.IsNullOrWhiteSpace(조건.현재상태))
        {
            filter &= builder.Eq(x => x.현재상태, 정규화(조건.현재상태, string.Empty, 40));
        }

        var items = await _컬렉션
            .Find(filter)
            .SortByDescending(x => x.수정시각Utc)
            .Limit(100)
            .ToListAsync(cancellationToken);

        return items.Select(응답으로).ToArray();
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

    public async Task<공동구매자동집단응답> 개별주문원장연결Async(
        string 자동집단Id,
        string 수요Id,
        string 개별주문원장Id,
        string 입고예정원장Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(수요Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(개별주문원장Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(입고예정원장Id);
        await 인덱스준비Async(cancellationToken);

        var 문서 = await _컬렉션.Find(x => x.자동집단Id == 자동집단Id.Trim())
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("개별 주문 원장을 연결할 자동집단을 찾을 수 없습니다.");
        var 수요 = 문서.수요목록.FirstOrDefault(x => x.수요Id == 수요Id.Trim())
            ?? throw new InvalidOperationException("개별 주문 원장을 연결할 주문자 수요를 찾을 수 없습니다.");

        수요.개별주문원장Id = 개별주문원장Id.Trim();
        수요.입고예정원장Id = 입고예정원장Id.Trim();
        수요.입고의미상태 = 공동구매개별주문입고상태코드.입고예정;
        var now = DateTime.UtcNow;
        문서.이벤트목록.Add(new 공동구매자동집단이벤트문서
        {
            이벤트유형 = "IndividualOrderInboundPlanned",
            요약 = $"{수요.주문자표시명} 개별 주문과 도착 창고 입고 예정 원장을 연결했습니다.",
            발생시각Utc = now
        });
        문서.수정시각Utc = now;

        await _컬렉션.ReplaceOneAsync(
            x => x.자동집단Id == 문서.자동집단Id,
            문서,
            cancellationToken: cancellationToken);
        return 응답으로(문서);
    }

    private static void 재계산(공동구매자동집단문서 문서, DateTime now)
    {
        문서.수요건수 = 문서.수요목록.Count;
        문서.예약결제건수 = 문서.수요목록.Count(x =>
            x.수요유형 == 공동구매자동수요유형코드.예약결제 ||
            x.결제상태 is 공동구매자동결제상태코드.예약됨 or 공동구매자동결제상태코드.결제확정);
        문서.총희망수량 = 문서.수요목록.Sum(x => x.희망수량);
        문서.예약결제합계 = 문서.수요목록.Sum(x => Math.Max(0, x.예약결제금액 ?? 0));
        문서.수량단위 = 문서.수요목록.LastOrDefault()?.수량단위 ?? "kg";
        문서.목표참여자수 = 문서.수요목록
            .Where(x => x.목표참여자수 is > 0)
            .Select(x => x.목표참여자수)
            .Min();
        문서.목표수량 = 문서.수요목록
            .Where(x => x.목표수량 is > 0)
            .Select(x => x.목표수량)
            .Min();

        var 이전상태 = 문서.현재상태;
        문서.현재상태 = 공동구매자동집단화계획기.상태제안(
            문서.수요건수,
            문서.예약결제건수,
            문서.총희망수량,
            문서.목표참여자수,
            문서.목표수량);
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
                        .Ascending(x => x.현재상태)
                        .Descending(x => x.수정시각Utc))
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

    private static 공동구매자동집단응답 응답으로(공동구매자동집단문서 문서)
    {
        return new 공동구매자동집단응답
        {
            자동집단Id = 문서.자동집단Id,
            상품키 = 문서.상품키,
            상품명 = 문서.상품명,
            HS코드 = 문서.HS코드,
            온도코드 = 문서.온도코드,
            물류방식 = 문서.물류방식,
            배송권키 = 문서.배송권키,
            배송권명 = 문서.배송권명,
            현재상태 = 문서.현재상태,
            수요건수 = 문서.수요건수,
            예약결제건수 = 문서.예약결제건수,
            총희망수량 = 문서.총희망수량,
            수량단위 = 문서.수량단위,
            예약결제합계 = 문서.예약결제합계,
            목표참여자수 = 문서.목표참여자수,
            목표수량 = 문서.목표수량,
            생성시각Utc = 문서.생성시각Utc,
            수정시각Utc = 문서.수정시각Utc,
            수요목록 = 문서.수요목록.OrderByDescending(x => x.생성시각Utc).Select(x => 응답으로(x, 문서.자동집단Id)).ToArray(),
            이벤트목록 = 문서.이벤트목록.OrderByDescending(x => x.발생시각Utc).Take(20).Select(응답으로).ToArray()
        };
    }

    private static 공동구매자동수요응답 응답으로(공동구매자동수요문서 문서, string 자동집단Id)
    {
        return new 공동구매자동수요응답
        {
            수요Id = 문서.수요Id,
            수요출처키 = 문서.수요출처키,
            커뮤니티게시글Id = 문서.커뮤니티게시글Id,
            커뮤니티원장Id = 문서.커뮤니티원장Id,
            자동집단Id = 자동집단Id,
            상품키 = 문서.상품키,
            상품명 = 문서.상품명,
            주문자키 = 문서.주문자키,
            주문자표시명 = 문서.주문자표시명,
            배송권키 = 문서.배송권키,
            배송권명 = 문서.배송권명,
            도착창고Id = 문서.도착창고Id,
            도착창고유형 = 문서.도착창고유형,
            도착창고명 = 문서.도착창고명,
            수령지주소참조키 = 문서.수령지주소참조키,
            입고의미상태 = 문서.입고의미상태,
            개별주문원장Id = 문서.개별주문원장Id,
            입고예정원장Id = 문서.입고예정원장Id,
            수요유형 = 문서.수요유형,
            결제상태 = 문서.결제상태,
            희망수량 = 문서.희망수량,
            수량단위 = 문서.수량단위,
            예약결제금액 = 문서.예약결제금액,
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

    private static string 정규화(string? 값, string 기본값, int 최대길이)
    {
        var 정규화값 = string.IsNullOrWhiteSpace(값) ? 기본값 : 값.Trim();
        return 정규화값.Length <= 최대길이 ? 정규화값 : 정규화값[..최대길이];
    }

    private static int? 양수값(int? 값) => 값 is > 0 ? 값 : null;

    private static decimal? 양수값(decimal? 값) => 값 is > 0 ? 값 : null;
}

public sealed class 공동구매자동집단문서
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string 자동집단Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 현재상태 { get; set; } = 공동구매자동집단상태코드.수요수집중;
    public int 수요건수 { get; set; }
    public int 예약결제건수 { get; set; }
    public decimal 총희망수량 { get; set; }
    public string 수량단위 { get; set; } = "kg";
    public decimal 예약결제합계 { get; set; }
    public int? 목표참여자수 { get; set; }
    public decimal? 목표수량 { get; set; }
    public List<공동구매자동수요문서> 수요목록 { get; set; } = [];
    public List<공동구매자동집단이벤트문서> 이벤트목록 { get; set; } = [];
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매자동수요문서
{
    public string 수요Id { get; set; } = string.Empty;
    public string 수요출처키 { get; set; } = string.Empty;
    public long? 커뮤니티게시글Id { get; set; }
    public string 커뮤니티원장Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 주문자키 { get; set; } = string.Empty;
    public string 주문자표시명 { get; set; } = string.Empty;
    public long? 도착창고Id { get; set; }
    public string 도착창고유형 { get; set; } = string.Empty;
    public string 도착창고명 { get; set; } = string.Empty;
    public string 수령지주소참조키 { get; set; } = string.Empty;
    public string 입고의미상태 { get; set; } = 공동구매개별주문입고상태코드.미지정;
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
    public DateTime 생성시각Utc { get; set; }
}

public sealed class 공동구매자동집단이벤트문서
{
    public string 이벤트유형 { get; set; } = string.Empty;
    public string 요약 { get; set; } = string.Empty;
    public DateTime 발생시각Utc { get; set; }
}
