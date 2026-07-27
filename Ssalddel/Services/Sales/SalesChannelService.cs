using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Sales;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.판매;

namespace 살뜰.Services.Sales;

public sealed class SalesChannelService : ISalesChannelService, ISalesChannelCredentialProvider
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I판매상품샘플시드Service _productSampleSeedService;
    private readonly ISalesChannelCredentialEncryptionService _credentialEncryption;

    public SalesChannelService(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor,
        I판매상품샘플시드Service productSampleSeedService,
        ISalesChannelCredentialEncryptionService credentialEncryption)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _productSampleSeedService = productSampleSeedService;
        _credentialEncryption = credentialEncryption;
    }

    public async Task<판매채널계정목록응답> GetAccountsAsync(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var query = _db.판매채널계정.AsNoTracking().AsQueryable();
        if (!IsServerAdmin())
        {
            query = query.Where(x => x.UserId == userId);
        }

        var entities = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToArrayAsync(cancellationToken);

        return new 판매채널계정목록응답
        {
            Items = entities.Select(ToAccountResponse).ToArray()
        };
    }

    public async Task<판매채널계정항목응답?> GetAccountAsync(
        long accountId,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var query = _db.판매채널계정
            .AsNoTracking()
            .Where(x => x.Id == accountId);
        if (!IsServerAdmin())
        {
            query = query.Where(x => x.UserId == userId);
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        return entity is null ? null : ToAccountResponse(entity);
    }

    public async Task<판매채널계정항목응답> CreateAccountAsync(판매채널계정저장요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var credentials = NormalizeCredentials(request.채널종류, request.인증정보);
        var credentialState = BuildCredentialState(request.채널종류, credentials);
        var entity = new 판매채널계정
        {
            UserId = userId,
            채널종류 = request.채널종류.Trim(),
            상점명 = request.상점명.Trim(),
            연결상태 = credentialState.AllRequiredConfigured ? "자격증명저장" : "준비",
            토큰암호화저장값 = credentials.Count == 0
                ? string.Empty
                : ProtectCredentials(credentials),
            마지막동기화일시 = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.판매채널계정.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return ToAccountResponse(entity);
    }

    public async Task<판매채널계정항목응답> UpdateAccountAsync(
        long accountId,
        판매채널계정저장요청 request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = await _db.판매채널계정
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken)
            ?? throw new InvalidOperationException("판매채널 계정을 찾을 수 없습니다.");
        if (!IsServerAdmin() && entity.UserId != userId)
        {
            throw new InvalidOperationException("판매채널 계정을 수정할 권한이 없습니다.");
        }

        var channelChanged = !string.Equals(
            entity.채널종류,
            request.채널종류.Trim(),
            StringComparison.OrdinalIgnoreCase);
        var existingCredentials = channelChanged
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : ReadCredentials(entity.토큰암호화저장값);
        var credentials = MergeCredentials(
            request.채널종류,
            existingCredentials,
            request.인증정보);
        var credentialState = BuildCredentialState(request.채널종류, credentials);

        entity.채널종류 = request.채널종류.Trim();
        entity.상점명 = request.상점명.Trim();
        entity.토큰암호화저장값 = credentials.Count == 0
            ? string.Empty
            : ProtectCredentials(credentials);
        entity.연결상태 = credentialState.AllRequiredConfigured ? "자격증명저장" : "준비";
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return ToAccountResponse(entity);
    }

    public async Task DeleteAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = await _db.판매채널계정
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken)
            ?? throw new InvalidOperationException("판매채널 계정을 찾을 수 없습니다.");
        if (!IsServerAdmin() && entity.UserId != userId)
        {
            throw new InvalidOperationException("판매채널 계정을 삭제할 권한이 없습니다.");
        }

        if (await _db.채널출품.AnyAsync(x => x.판매채널계정Id == accountId, cancellationToken))
        {
            throw new InvalidOperationException("판매채널 계정에 연결된 출품을 먼저 삭제해 주세요.");
        }

        _db.판매채널계정.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<판매채널자격증명Set?> GetAsync(
        long 판매채널계정Id,
        CancellationToken cancellationToken)
    {
        var account = await _db.판매채널계정
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == 판매채널계정Id,
                cancellationToken);
        if (account is null)
        {
            return null;
        }

        return new 판매채널자격증명Set(
            account.Id,
            account.채널종류,
            ReadCredentials(account.토큰암호화저장값));
    }

    public async Task<판매상품목록응답> GetProductsAsync(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var query = _db.판매상품.AsNoTracking().AsQueryable();
        if (!IsServerAdmin())
        {
            query = query.Where(x => x.소유자UserId == userId);
        }

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new 판매상품항목응답
            {
                Id = x.Id,
                입고상품Id = x.입고상품Id,
                대표상품명 = x.대표상품명,
                판매SKU = x.판매SKU,
                판매가 = x.판매가,
                상태 = x.상태,
                샘플데이터여부 = x.샘플데이터여부,
                샘플데이터코드 = x.샘플데이터코드,
                Image_Url = x.이미지Url,
                이미지생성상태 = x.이미지생성상태
            })
            .ToArrayAsync(cancellationToken);

        return new 판매상품목록응답 { Items = items };
    }

    public async Task<판매상품항목응답> CreateProductAsync(판매상품저장요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var inboundItem = await _db.입고상품.FirstOrDefaultAsync(x => x.Id == request.입고상품Id, cancellationToken)
            ?? throw new InvalidOperationException("입고상품을 찾을 수 없습니다.");

        if (!IsServerAdmin() && inboundItem.소유자UserId != userId && inboundItem.판매자UserId != userId)
        {
            throw new InvalidOperationException("판매상품을 생성할 권한이 없습니다.");
        }

        var entity = new 판매상품
        {
            입고상품Id = inboundItem.Id,
            소유자UserId = inboundItem.판매자UserId == userId ? inboundItem.판매자UserId : inboundItem.소유자UserId,
            대표상품명 = request.대표상품명.Trim(),
            판매SKU = request.판매SKU.Trim(),
            판매가 = request.판매가,
            상태 = "준비",
            샘플데이터여부 = request.샘플데이터여부,
            샘플데이터코드 = string.IsNullOrWhiteSpace(request.샘플데이터코드) ? null : request.샘플데이터코드.Trim(),
            이미지Url = null,
            이미지생성상태 = 판매상품이미지생성상태.미생성,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.판매상품.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new 판매상품항목응답
        {
            Id = entity.Id,
            입고상품Id = entity.입고상품Id,
            대표상품명 = entity.대표상품명,
            판매SKU = entity.판매SKU,
            판매가 = entity.판매가,
            상태 = entity.상태,
            샘플데이터여부 = entity.샘플데이터여부,
            샘플데이터코드 = entity.샘플데이터코드,
            Image_Url = entity.이미지Url,
            이미지생성상태 = entity.이미지생성상태
        };
    }

    public async Task<판매상품항목응답> UpdateProductAsync(
        long productId,
        판매상품저장요청 request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = await _db.판매상품
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("판매상품을 찾을 수 없습니다.");
        if (!IsServerAdmin() && entity.소유자UserId != userId)
        {
            throw new InvalidOperationException("판매상품을 수정할 권한이 없습니다.");
        }

        if (request.입고상품Id != entity.입고상품Id)
        {
            var inboundItem = await _db.입고상품
                .FirstOrDefaultAsync(x => x.Id == request.입고상품Id, cancellationToken)
                ?? throw new InvalidOperationException("입고상품을 찾을 수 없습니다.");
            if (!IsServerAdmin() && inboundItem.소유자UserId != userId && inboundItem.판매자UserId != userId)
            {
                throw new InvalidOperationException("입고상품을 판매상품에 연결할 권한이 없습니다.");
            }

            entity.입고상품Id = inboundItem.Id;
        }

        entity.대표상품명 = request.대표상품명.Trim();
        entity.판매SKU = request.판매SKU.Trim();
        entity.판매가 = request.판매가;
        entity.샘플데이터여부 = request.샘플데이터여부;
        entity.샘플데이터코드 = string.IsNullOrWhiteSpace(request.샘플데이터코드)
            ? null
            : request.샘플데이터코드.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return ToProductResponse(entity);
    }

    public async Task DeleteProductAsync(long productId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = await _db.판매상품
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("판매상품을 찾을 수 없습니다.");
        if (!IsServerAdmin() && entity.소유자UserId != userId)
        {
            throw new InvalidOperationException("판매상품을 삭제할 권한이 없습니다.");
        }

        if (await _db.채널출품.AnyAsync(x => x.판매상품Id == productId, cancellationToken))
        {
            throw new InvalidOperationException("판매상품에 연결된 출품을 먼저 삭제해 주세요.");
        }

        _db.판매상품.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<판매상품목록응답> SeedSampleProductsAsync(판매상품샘플시드요청 request, CancellationToken cancellationToken)
    {
        return _productSampleSeedService.SeedSampleProductsAsync(request.최대건수, cancellationToken);
    }

    public async Task<채널출품목록응답> GetListingsAsync(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var query = from listing in _db.채널출품.AsNoTracking()
                    join product in _db.판매상품.AsNoTracking() on listing.판매상품Id equals product.Id
                    select new { listing, product };

        if (!IsServerAdmin())
        {
            query = query.Where(x => x.product.소유자UserId == userId);
        }

        var items = await query
            .OrderByDescending(x => x.listing.UpdatedAt)
            .Select(x => new 채널출품항목응답
            {
                Id = x.listing.Id,
                판매상품Id = x.listing.판매상품Id,
                판매채널계정Id = x.listing.판매채널계정Id,
                채널상품번호 = x.listing.채널상품번호,
                출품상태 = x.listing.출품상태,
                동기화상태 = x.listing.동기화상태,
                에러메시지 = x.listing.에러메시지
            })
            .ToArrayAsync(cancellationToken);

        return new 채널출품목록응답 { Items = items };
    }

    public async Task<채널출품항목응답> CreateListingAsync(채널출품저장요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var product = await _db.판매상품.FirstOrDefaultAsync(x => x.Id == request.판매상품Id, cancellationToken)
            ?? throw new InvalidOperationException("판매상품을 찾을 수 없습니다.");
        var account = await _db.판매채널계정.FirstOrDefaultAsync(x => x.Id == request.판매채널계정Id, cancellationToken)
            ?? throw new InvalidOperationException("판매채널 계정을 찾을 수 없습니다.");

        if (!IsServerAdmin())
        {
            if (product.소유자UserId != userId)
            {
                throw new InvalidOperationException("출품할 권한이 없습니다.");
            }

            if (account.UserId != userId)
            {
                throw new InvalidOperationException("채널 계정에 접근할 권한이 없습니다.");
            }
        }

        var entity = new 채널출품
        {
            판매상품Id = product.Id,
            판매채널계정Id = account.Id,
            채널상품번호 = $"LIST-{Guid.NewGuid():N}"[..17],
            출품상태 = "출품준비",
            동기화상태 = "대기",
            에러메시지 = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.상태 = "출품대기";
        product.UpdatedAt = DateTime.UtcNow;
        account.마지막동기화일시 = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;

        _db.채널출품.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new 채널출품항목응답
        {
            Id = entity.Id,
            판매상품Id = entity.판매상품Id,
            판매채널계정Id = entity.판매채널계정Id,
            채널상품번호 = entity.채널상품번호,
            출품상태 = entity.출품상태,
            동기화상태 = entity.동기화상태,
            에러메시지 = entity.에러메시지
        };
    }

    public async Task<채널출품항목응답> UpdateListingAsync(
        long listingId,
        채널출품저장요청 request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = await _db.채널출품
            .FirstOrDefaultAsync(x => x.Id == listingId, cancellationToken)
            ?? throw new InvalidOperationException("채널 출품을 찾을 수 없습니다.");
        var previousProduct = await _db.판매상품
            .FirstOrDefaultAsync(x => x.Id == entity.판매상품Id, cancellationToken)
            ?? throw new InvalidOperationException("기존 판매상품을 찾을 수 없습니다.");
        var product = await _db.판매상품
            .FirstOrDefaultAsync(x => x.Id == request.판매상품Id, cancellationToken)
            ?? throw new InvalidOperationException("판매상품을 찾을 수 없습니다.");
        var account = await _db.판매채널계정
            .FirstOrDefaultAsync(x => x.Id == request.판매채널계정Id, cancellationToken)
            ?? throw new InvalidOperationException("판매채널 계정을 찾을 수 없습니다.");

        if (!IsServerAdmin())
        {
            if (previousProduct.소유자UserId != userId || product.소유자UserId != userId || account.UserId != userId)
            {
                throw new InvalidOperationException("채널 출품을 수정할 권한이 없습니다.");
            }
        }

        entity.판매상품Id = product.Id;
        entity.판매채널계정Id = account.Id;
        entity.출품상태 = "출품준비";
        entity.동기화상태 = "대기";
        entity.에러메시지 = string.Empty;
        entity.UpdatedAt = DateTime.UtcNow;
        product.상태 = "출품대기";
        product.UpdatedAt = DateTime.UtcNow;
        account.마지막동기화일시 = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        if (previousProduct.Id != product.Id
            && !await _db.채널출품.AsNoTracking()
                .AnyAsync(x => x.Id != listingId && x.판매상품Id == previousProduct.Id, cancellationToken))
        {
            previousProduct.상태 = "준비";
            previousProduct.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(cancellationToken);

        return ToListingResponse(entity);
    }

    public async Task DeleteListingAsync(long listingId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = await _db.채널출품
            .FirstOrDefaultAsync(x => x.Id == listingId, cancellationToken)
            ?? throw new InvalidOperationException("채널 출품을 찾을 수 없습니다.");
        var product = await _db.판매상품
            .FirstOrDefaultAsync(x => x.Id == entity.판매상품Id, cancellationToken)
            ?? throw new InvalidOperationException("판매상품을 찾을 수 없습니다.");
        if (!IsServerAdmin() && product.소유자UserId != userId)
        {
            throw new InvalidOperationException("채널 출품을 삭제할 권한이 없습니다.");
        }

        _db.채널출품.Remove(entity);
        if (!await _db.채널출품.AsNoTracking()
                .AnyAsync(x => x.Id != listingId && x.판매상품Id == product.Id, cancellationToken))
        {
            product.상태 = "준비";
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private 판매채널계정항목응답 ToAccountResponse(판매채널계정 entity)
    {
        var credentials = ReadCredentials(entity.토큰암호화저장값);
        var state = BuildCredentialState(entity.채널종류, credentials);
        return new()
        {
            Id = entity.Id,
            채널종류 = entity.채널종류,
            상점명 = entity.상점명,
            연결상태 = entity.연결상태,
            마지막동기화일시 = entity.마지막동기화일시,
            등록일시 = entity.CreatedAt,
            수정일시 = entity.UpdatedAt,
            인증정보설정됨 = state.AllRequiredConfigured,
            인증필드상태 = state.Fields
        };
    }

    private string ProtectCredentials(IReadOnlyDictionary<string, string> credentials)
    {
        var protectedValue = _credentialEncryption.Protect(
            JsonSerializer.Serialize(credentials));
        if (protectedValue.Length > 2000)
        {
            throw new InvalidOperationException(
                "판매채널 자격증명 전체 길이가 보안 저장 한도를 넘었습니다. 입력값을 확인해 주세요.");
        }

        return protectedValue;
    }

    private Dictionary<string, string> ReadCredentials(string? protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue)
            || !_credentialEncryption.IsProtected(protectedValue))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var json = _credentialEncryption.Unprotect(protectedValue);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
               ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string> NormalizeCredentials(
        string channelType,
        IReadOnlyDictionary<string, string>? input)
        => MergeCredentials(
            channelType,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            input);

    private static Dictionary<string, string> MergeCredentials(
        string channelType,
        IReadOnlyDictionary<string, string> existing,
        IReadOnlyDictionary<string, string>? input)
    {
        var schema = 판매채널인증SchemaCatalog.찾기(channelType)
            ?? throw new InvalidOperationException("지원하는 판매채널을 선택해 주세요.");
        var allowedKeys = schema.Fields
            .Select(field => field.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, string>(existing, StringComparer.OrdinalIgnoreCase);

        foreach (var item in input ?? new Dictionary<string, string>())
        {
            if (!allowedKeys.Contains(item.Key))
            {
                throw new InvalidOperationException(
                    $"{schema.표시명}에서 지원하지 않는 인증 필드입니다: {item.Key}");
            }

            var value = item.Value?.Trim() ?? string.Empty;
            if (value.Length > 2000)
            {
                throw new InvalidOperationException($"{item.Key} 값은 2,000자를 넘을 수 없습니다.");
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                result[item.Key] = value;
            }
        }

        return result;
    }

    private static (bool AllRequiredConfigured, IReadOnlyList<판매채널인증필드상태> Fields)
        BuildCredentialState(
            string channelType,
            IReadOnlyDictionary<string, string> credentials)
    {
        var schema = 판매채널인증SchemaCatalog.찾기(channelType);
        if (schema is null)
        {
            return (false, []);
        }

        var fields = schema.Fields
            .Select(field =>
            {
                var configured = credentials.TryGetValue(field.Key, out var value)
                                 && !string.IsNullOrWhiteSpace(value);
                return new 판매채널인증필드상태
                {
                    Key = field.Key,
                    표시명 = field.표시명,
                    필수 = field.필수,
                    비밀값 = field.비밀값,
                    설정됨 = configured,
                    마스킹값 = configured ? Mask(value!) : string.Empty
                };
            })
            .ToArray();

        return (
            fields.Where(field => field.필수).All(field => field.설정됨),
            fields);
    }

    private static string Mask(string value)
        => value.Length <= 4
            ? "••••"
            : $"••••{value[^4..]}";

    private static 판매상품항목응답 ToProductResponse(판매상품 entity)
        => new()
        {
            Id = entity.Id,
            입고상품Id = entity.입고상품Id,
            대표상품명 = entity.대표상품명,
            판매SKU = entity.판매SKU,
            판매가 = entity.판매가,
            상태 = entity.상태,
            샘플데이터여부 = entity.샘플데이터여부,
            샘플데이터코드 = entity.샘플데이터코드,
            Image_Url = entity.이미지Url,
            이미지생성상태 = entity.이미지생성상태
        };

    private static 채널출품항목응답 ToListingResponse(채널출품 entity)
        => new()
        {
            Id = entity.Id,
            판매상품Id = entity.판매상품Id,
            판매채널계정Id = entity.판매채널계정Id,
            채널상품번호 = entity.채널상품번호,
            출품상태 = entity.출품상태,
            동기화상태 = entity.동기화상태,
            에러메시지 = entity.에러메시지
        };

    private string RequireUserId()
    {
        var userId = _currentUserAccessor.UserId?.Trim();
        return !string.IsNullOrWhiteSpace(userId)
            ? userId
            : throw new InvalidOperationException("로그인 사용자를 확인할 수 없습니다.");
    }

    private bool IsServerAdmin()
    {
        return string.Equals(_currentUserAccessor.Role, 역할명.서버관리자, StringComparison.OrdinalIgnoreCase);
    }
}
