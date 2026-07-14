using Hongdal.Contracts.Common.TraditionalMarkets;
using Hongdal.Domain.TraditionalMarkets;
using Hongdal.Infrastructure.Persistence.TraditionalMarkets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.TraditionalMarkets;

public interface I전통시장생활권협의Service
{
    Task<전통시장생활권협의체목록응답> 내협의체조회Async(string userId, CancellationToken cancellationToken = default);
    Task<전통시장생활권협의체상세응답?> 상세조회Async(Guid 협의체Id, string userId, CancellationToken cancellationToken = default);
    Task<전통시장생활권협의체상세응답> 생성Async(전통시장생활권협의체생성요청 request, string userId, CancellationToken cancellationToken = default);
    Task<전통시장생활권협의체상세응답> 참여수락Async(Guid 협의체Id, 전통시장생활권협의체참여수락요청 request, string userId, CancellationToken cancellationToken = default);
    Task<전통시장교역안건응답> 안건생성Async(Guid 협의체Id, 전통시장교역안건생성요청 request, string userId, CancellationToken cancellationToken = default);
    Task<전통시장교역안건응답> 안건결정Async(Guid 협의체Id, Guid 안건Id, 전통시장교역안건결정요청 request, string userId, CancellationToken cancellationToken = default);
}

public sealed class 전통시장생활권협의ConcurrencyException : Exception
{
    public 전통시장생활권협의ConcurrencyException(string message) : base(message) { }
    public 전통시장생활권협의ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class 전통시장생활권협의Service : I전통시장생활권협의Service
{
    private readonly TraditionalMarketDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public 전통시장생활권협의Service(
        TraditionalMarketDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<전통시장생활권협의체목록응답> 내협의체조회Async(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = NormalizeUserId(userId);
        var councils = await _db.NeighborhoodCouncils
            .AsNoTracking()
            .Include(x => x.안건)
            .Where(x => x.아파트대표UserId == actorUserId || x.상인회대표UserId == actorUserId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        var marketCodes = councils.Select(x => x.시장Code).Distinct().ToArray();
        var markets = await _db.Markets
            .AsNoTracking()
            .Where(x => marketCodes.Contains(x.MarketCode))
            .ToDictionaryAsync(x => x.MarketCode, cancellationToken);

        return new 전통시장생활권협의체목록응답
        {
            항목 = councils
                .Where(x => markets.ContainsKey(x.시장Code))
                .Select(x => ToSummary(x, markets[x.시장Code], actorUserId))
                .ToArray()
        };
    }

    public async Task<전통시장생활권협의체상세응답?> 상세조회Async(
        Guid 협의체Id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = NormalizeUserId(userId);
        var council = await _db.NeighborhoodCouncils
            .AsNoTracking()
            .Include(x => x.안건)
            .FirstOrDefaultAsync(x => x.Id == 협의체Id, cancellationToken);
        if (council is null)
        {
            return null;
        }

        EnsureParticipant(council, actorUserId);
        var market = await _db.Markets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MarketCode == council.시장Code, cancellationToken);
        return market is null ? null : ToDetail(council, market, actorUserId);
    }

    public async Task<전통시장생활권협의체상세응답> 생성Async(
        전통시장생활권협의체생성요청 request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateCouncilRequest(request);
        var actorUserId = NormalizeUserId(userId);
        var counterpartUserId = NormalizeUserId(request.상대대표UserId);
        if (string.Equals(actorUserId, counterpartUserId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("요청자와 상대 대표는 서로 다른 사용자여야 합니다.");
        }

        if (await _userManager.FindByIdAsync(counterpartUserId) is null)
        {
            throw new KeyNotFoundException("초대할 상대 대표 회원을 찾을 수 없습니다.");
        }

        var marketCode = NormalizeMarketCode(request.시장Code);
        var market = await _db.Markets.FirstOrDefaultAsync(x => x.MarketCode == marketCode, cancellationToken);
        if (market is null || !market.IsActive)
        {
            throw new KeyNotFoundException("활성 전통시장 기준정보를 찾을 수 없습니다.");
        }

        var requesterRole = 전통시장협의체역할Codes.Normalize(request.요청자역할);
        var apartmentUserId = requesterRole == 전통시장협의체역할Codes.아파트대표 ? actorUserId : counterpartUserId;
        var merchantUserId = requesterRole == 전통시장협의체역할Codes.상인회대표 ? actorUserId : counterpartUserId;
        var apartmentName = requesterRole == 전통시장협의체역할Codes.아파트대표
            ? request.요청자대표명.Trim()
            : request.상대대표명.Trim();
        var merchantName = requesterRole == 전통시장협의체역할Codes.상인회대표
            ? request.요청자대표명.Trim()
            : request.상대대표명.Trim();

        var apartmentCommunityName = request.아파트단지명.Trim();
        var duplicateExists = await _db.NeighborhoodCouncils.AnyAsync(
            x => x.시장Code == marketCode
                 && x.아파트단지명 == apartmentCommunityName
                 && x.아파트대표UserId == apartmentUserId
                 && x.상인회대표UserId == merchantUserId
                 && x.상태 != 전통시장협의체상태Codes.종료,
            cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException("같은 대표 구성으로 진행 중인 생활권 협의체가 이미 있습니다.");
        }

        var now = DateTime.UtcNow;
        var council = new 전통시장생활권협의체
        {
            Id = Guid.NewGuid(),
            시장Code = marketCode,
            협의체명 = request.협의체명.Trim(),
            아파트단지명 = apartmentCommunityName,
            아파트주소 = request.아파트주소?.Trim() ?? string.Empty,
            아파트대표UserId = apartmentUserId,
            아파트대표명 = apartmentName,
            아파트대표수락AtUtc = requesterRole == 전통시장협의체역할Codes.아파트대표 ? now : null,
            상인회명 = request.상인회명.Trim(),
            상인회대표UserId = merchantUserId,
            상인회대표명 = merchantName,
            상인회대표수락AtUtc = requesterRole == 전통시장협의체역할Codes.상인회대표 ? now : null,
            협의목적 = request.협의목적?.Trim() ?? string.Empty,
            상태 = 전통시장협의체상태Codes.초대중,
            CreatedByUserId = actorUserId,
            Revision = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.NeighborhoodCouncils.Add(council);
        await SaveChangesAsync(cancellationToken);
        return ToDetail(council, market, actorUserId);
    }

    public async Task<전통시장생활권협의체상세응답> 참여수락Async(
        Guid 협의체Id,
        전통시장생활권협의체참여수락요청 request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = NormalizeUserId(userId);
        var council = await _db.NeighborhoodCouncils
            .Include(x => x.안건)
            .FirstOrDefaultAsync(x => x.Id == 협의체Id, cancellationToken)
            ?? throw new KeyNotFoundException("생활권 협의체를 찾을 수 없습니다.");
        EnsureRevision(council.Revision, request.예상Revision);
        var role = EnsureParticipant(council, actorUserId);
        if (council.상태 == 전통시장협의체상태Codes.종료)
        {
            throw new InvalidOperationException("종료된 협의체에는 참여할 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        if (role == 전통시장협의체역할Codes.아파트대표)
        {
            council.아파트대표수락AtUtc ??= now;
        }
        else
        {
            council.상인회대표수락AtUtc ??= now;
        }

        if (council.아파트대표수락AtUtc.HasValue && council.상인회대표수락AtUtc.HasValue)
        {
            council.상태 = 전통시장협의체상태Codes.협의중;
        }

        council.UpdatedAtUtc = now;
        council.Revision++;
        await SaveChangesAsync(cancellationToken);
        var market = await _db.Markets.AsNoTracking().SingleAsync(x => x.MarketCode == council.시장Code, cancellationToken);
        return ToDetail(council, market, actorUserId);
    }

    public async Task<전통시장교역안건응답> 안건생성Async(
        Guid 협의체Id,
        전통시장교역안건생성요청 request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateAgendaRequest(request);
        var actorUserId = NormalizeUserId(userId);
        var council = await _db.NeighborhoodCouncils.FirstOrDefaultAsync(x => x.Id == 협의체Id, cancellationToken)
            ?? throw new KeyNotFoundException("생활권 협의체를 찾을 수 없습니다.");
        EnsureParticipant(council, actorUserId);
        if (council.상태 != 전통시장협의체상태Codes.협의중)
        {
            throw new InvalidOperationException("양측 대표가 참여를 수락한 협의체에서만 안건을 만들 수 있습니다.");
        }

        var now = DateTime.UtcNow;
        var agenda = new 전통시장교역안건
        {
            Id = Guid.NewGuid(),
            협의체Id = council.Id,
            교역방향 = 전통시장교역방향Codes.Normalize(request.교역방향),
            품목명 = request.품목명.Trim(),
            품목설명 = request.품목설명?.Trim() ?? string.Empty,
            희망수량 = request.희망수량,
            수량단위 = request.수량단위.Trim(),
            원산지국가 = request.원산지국가?.Trim() ?? string.Empty,
            목적지국가 = request.목적지국가?.Trim() ?? string.Empty,
            희망시작일 = request.희망시작일,
            희망종료일 = request.희망종료일,
            물류조건 = request.물류조건?.Trim() ?? string.Empty,
            예상금액 = request.예상금액,
            통화Code = request.통화Code.Trim().ToUpperInvariant(),
            통관검토필요여부 = request.통관검토필요여부,
            제안내용 = request.제안내용?.Trim() ?? string.Empty,
            상태 = 전통시장교역안건상태Codes.검토중,
            아파트측결정 = 전통시장협의결정Codes.대기,
            상인회측결정 = 전통시장협의결정Codes.대기,
            CreatedByUserId = actorUserId,
            Revision = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.TradeAgendas.Add(agenda);
        council.UpdatedAtUtc = now;
        council.Revision++;
        await SaveChangesAsync(cancellationToken);
        return ToAgenda(agenda);
    }

    public async Task<전통시장교역안건응답> 안건결정Async(
        Guid 협의체Id,
        Guid 안건Id,
        전통시장교역안건결정요청 request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var decision = 전통시장협의결정Codes.NormalizeDecision(request.결정);
        if (string.IsNullOrEmpty(decision))
        {
            throw new InvalidOperationException("결정은 동의, 보완요청, 반대 중 하나여야 합니다.");
        }

        if ((decision == 전통시장협의결정Codes.보완요청 || decision == 전통시장협의결정Codes.반대)
            && string.IsNullOrWhiteSpace(request.의견))
        {
            throw new InvalidOperationException("보완 요청이나 반대 결정에는 의견이 필요합니다.");
        }

        if ((request.의견?.Length ?? 0) > 2000)
        {
            throw new InvalidOperationException("결정 의견은 2000자를 넘을 수 없습니다.");
        }

        var actorUserId = NormalizeUserId(userId);
        var agenda = await _db.TradeAgendas
            .Include(x => x.협의체)
            .FirstOrDefaultAsync(x => x.Id == 안건Id && x.협의체Id == 협의체Id, cancellationToken)
            ?? throw new KeyNotFoundException("협의 안건을 찾을 수 없습니다.");
        var role = EnsureParticipant(agenda.협의체, actorUserId);
        EnsureRevision(agenda.Revision, request.예상Revision);
        if (agenda.협의체.상태 != 전통시장협의체상태Codes.협의중)
        {
            throw new InvalidOperationException("협의 중인 협의체의 안건만 결정할 수 있습니다.");
        }

        if (agenda.상태 is 전통시장교역안건상태Codes.합의 or 전통시장교역안건상태Codes.철회)
        {
            throw new InvalidOperationException("이미 확정되거나 철회된 안건은 다시 결정할 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        if (role == 전통시장협의체역할Codes.아파트대표)
        {
            agenda.아파트측결정 = decision;
            agenda.아파트측의견 = request.의견?.Trim() ?? string.Empty;
            agenda.아파트측결정AtUtc = now;
        }
        else
        {
            agenda.상인회측결정 = decision;
            agenda.상인회측의견 = request.의견?.Trim() ?? string.Empty;
            agenda.상인회측결정AtUtc = now;
        }

        agenda.상태 = 전통시장생활권협의Policy.안건상태(agenda.아파트측결정, agenda.상인회측결정);
        agenda.UpdatedAtUtc = now;
        agenda.Revision++;
        agenda.협의체.UpdatedAtUtc = now;
        agenda.협의체.Revision++;
        await SaveChangesAsync(cancellationToken);
        return ToAgenda(agenda);
    }

    private static 전통시장생활권협의체요약응답 ToSummary(전통시장생활권협의체 council, TraditionalMarket market, string userId)
        => new()
        {
            협의체Id = council.Id,
            협의체명 = council.협의체명,
            시장Code = council.시장Code,
            시장명 = market.Name,
            아파트단지명 = council.아파트단지명,
            상인회명 = council.상인회명,
            상태 = council.상태,
            내역할 = 전통시장생활권협의Policy.참여역할(council, userId),
            안건수 = council.안건.Count,
            합의안건수 = council.안건.Count(x => x.상태 == 전통시장교역안건상태Codes.합의),
            Revision = council.Revision,
            UpdatedAtUtc = council.UpdatedAtUtc
        };

    private static 전통시장생활권협의체상세응답 ToDetail(전통시장생활권협의체 council, TraditionalMarket market, string userId)
        => new()
        {
            협의체Id = council.Id,
            협의체명 = council.협의체명,
            시장Code = council.시장Code,
            시장명 = market.Name,
            아파트단지명 = council.아파트단지명,
            상인회명 = council.상인회명,
            상태 = council.상태,
            내역할 = 전통시장생활권협의Policy.참여역할(council, userId),
            안건수 = council.안건.Count,
            합의안건수 = council.안건.Count(x => x.상태 == 전통시장교역안건상태Codes.합의),
            Revision = council.Revision,
            UpdatedAtUtc = council.UpdatedAtUtc,
            아파트주소 = council.아파트주소,
            아파트대표UserId = council.아파트대표UserId,
            아파트대표명 = council.아파트대표명,
            아파트대표수락AtUtc = council.아파트대표수락AtUtc,
            상인회대표UserId = council.상인회대표UserId,
            상인회대표명 = council.상인회대표명,
            상인회대표수락AtUtc = council.상인회대표수락AtUtc,
            협의목적 = council.협의목적,
            CommunityScopeKey = TraditionalMarketCommunityScopes.Create(council.시장Code),
            협의체참조Key = 전통시장생활권협의참조.협의체(council.Id),
            안건 = council.안건.OrderByDescending(x => x.UpdatedAtUtc).Select(ToAgenda).ToArray(),
            CreatedAtUtc = council.CreatedAtUtc
        };

    private static 전통시장교역안건응답 ToAgenda(전통시장교역안건 agenda)
        => new()
        {
            안건Id = agenda.Id,
            안건참조Key = 전통시장생활권협의참조.안건(agenda.Id),
            교역방향 = agenda.교역방향,
            품목명 = agenda.품목명,
            품목설명 = agenda.품목설명,
            희망수량 = agenda.희망수량,
            수량단위 = agenda.수량단위,
            원산지국가 = agenda.원산지국가,
            목적지국가 = agenda.목적지국가,
            희망시작일 = agenda.희망시작일,
            희망종료일 = agenda.희망종료일,
            물류조건 = agenda.물류조건,
            예상금액 = agenda.예상금액,
            통화Code = agenda.통화Code,
            통관검토필요여부 = agenda.통관검토필요여부,
            제안내용 = agenda.제안내용,
            상태 = agenda.상태,
            아파트측결정 = agenda.아파트측결정,
            아파트측의견 = agenda.아파트측의견,
            아파트측결정AtUtc = agenda.아파트측결정AtUtc,
            상인회측결정 = agenda.상인회측결정,
            상인회측의견 = agenda.상인회측의견,
            상인회측결정AtUtc = agenda.상인회측결정AtUtc,
            CreatedByUserId = agenda.CreatedByUserId,
            Revision = agenda.Revision,
            CreatedAtUtc = agenda.CreatedAtUtc,
            UpdatedAtUtc = agenda.UpdatedAtUtc
        };

    private static void ValidateCouncilRequest(전통시장생활권협의체생성요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.시장Code)
            || string.IsNullOrWhiteSpace(request.협의체명)
            || string.IsNullOrWhiteSpace(request.아파트단지명)
            || string.IsNullOrWhiteSpace(request.상인회명)
            || string.IsNullOrWhiteSpace(request.요청자대표명)
            || string.IsNullOrWhiteSpace(request.상대대표명)
            || string.IsNullOrWhiteSpace(request.상대대표UserId))
        {
            throw new InvalidOperationException("시장, 협의체, 아파트, 상인회와 양측 대표 정보가 필요합니다.");
        }

        if (string.IsNullOrEmpty(전통시장협의체역할Codes.Normalize(request.요청자역할)))
        {
            throw new InvalidOperationException("요청자 역할은 아파트대표 또는 상인회대표여야 합니다.");
        }

        if (request.협의체명.Length > 160
            || request.아파트단지명.Length > 160
            || request.상인회명.Length > 160
            || request.요청자대표명.Length > 100
            || request.상대대표명.Length > 100
            || (request.아파트주소?.Length ?? 0) > 500
            || (request.협의목적?.Length ?? 0) > 2000)
        {
            throw new InvalidOperationException("협의체 입력값의 허용 길이를 초과했습니다.");
        }
    }

    private static void ValidateAgendaRequest(전통시장교역안건생성요청 request)
    {
        var direction = 전통시장교역방향Codes.Normalize(request.교역방향);
        if (string.IsNullOrEmpty(direction))
        {
            throw new InvalidOperationException("교역 방향은 수입 또는 수출이어야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.품목명) || request.희망수량 <= 0 || string.IsNullOrWhiteSpace(request.수량단위))
        {
            throw new InvalidOperationException("품목명, 0보다 큰 희망 수량과 수량 단위가 필요합니다.");
        }

        if (direction == 전통시장교역방향Codes.수입 && string.IsNullOrWhiteSpace(request.원산지국가))
        {
            throw new InvalidOperationException("수입 안건에는 원산지 국가가 필요합니다.");
        }

        if (direction == 전통시장교역방향Codes.수출 && string.IsNullOrWhiteSpace(request.목적지국가))
        {
            throw new InvalidOperationException("수출 안건에는 목적지 국가가 필요합니다.");
        }

        if (request.희망시작일.HasValue && request.희망종료일.HasValue && request.희망시작일 > request.희망종료일)
        {
            throw new InvalidOperationException("희망 종료일은 시작일보다 빠를 수 없습니다.");
        }

        if (request.예상금액 < 0)
        {
            throw new InvalidOperationException("예상 금액은 0 이상이어야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.통화Code) || request.통화Code.Trim().Length != 3)
        {
            throw new InvalidOperationException("통화 코드는 3자리 코드여야 합니다.");
        }

        if (request.품목명.Length > 200
            || (request.품목설명?.Length ?? 0) > 2000
            || request.수량단위.Length > 40
            || (request.원산지국가?.Length ?? 0) > 100
            || (request.목적지국가?.Length ?? 0) > 100
            || (request.물류조건?.Length ?? 0) > 2000
            || (request.제안내용?.Length ?? 0) > 4000)
        {
            throw new InvalidOperationException("교역 안건 입력값의 허용 길이를 초과했습니다.");
        }
    }

    private static string EnsureParticipant(전통시장생활권협의체 council, string userId)
    {
        var role = 전통시장생활권협의Policy.참여역할(council, userId);
        if (string.IsNullOrEmpty(role))
        {
            throw new UnauthorizedAccessException("이 협의체에 참여한 대표만 접근할 수 있습니다.");
        }

        return role;
    }

    private static void EnsureRevision(long currentRevision, long? expectedRevision)
    {
        if (expectedRevision.HasValue && expectedRevision.Value != currentRevision)
        {
            throw new 전통시장생활권협의ConcurrencyException($"협의 정보가 이미 변경되었습니다. 현재 revision은 {currentRevision}입니다.");
        }
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new 전통시장생활권협의ConcurrencyException("협의 정보가 다른 요청에서 먼저 변경되었습니다.", ex);
        }
    }

    private static string NormalizeMarketCode(string value)
        => string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException("시장코드가 필요합니다.")
            : value.Trim().ToLowerInvariant();

    private static string NormalizeUserId(string value)
        => string.IsNullOrWhiteSpace(value)
            ? throw new UnauthorizedAccessException("로그인 사용자 정보를 확인할 수 없습니다.")
            : value.Trim();
}
