using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using SsalddelApp.Services;
using SsalddelApp.Services.Commerce;

namespace SsalddelApp.ViewModels.Shipper;

public sealed record ProductListingSnapshot(
    IReadOnlyList<판매상품항목응답> 상품목록,
    IReadOnlyList<판매채널계정항목응답> 계정목록,
    IReadOnlyList<채널출품항목응답> 출품목록)
{
    public static ProductListingSnapshot Empty { get; } = new([], [], []);
}

public sealed record ProductListingLedgerRow(
    채널출품항목응답 출품,
    판매상품항목응답? 상품,
    판매채널계정항목응답? 계정)
{
    public long Id => 출품.Id;
    public string 상품명 => 상품?.대표상품명 ?? $"판매상품 #{출품.판매상품Id}";
    public string 판매Sku => 상품?.판매SKU ?? "상품 정보 없음";
    public string 채널종류 => 계정?.채널종류 ?? "채널 정보 없음";
    public string 상점명 => 계정?.상점명 ?? $"계정 #{출품.판매채널계정Id}";
}

/// <summary>판매상품·채널 계정·로컬 출품 원장 조회와 화면 검색 projection만 담당합니다.</summary>
public sealed partial class ProductListingReadViewModel(
    I상품등록Service productService,
    I판매채널계정읽기Service accountService,
    I채널출품Service listingService) : 업무작업ViewModelBase
{
    private ProductListingSnapshot _스냅샷 = ProductListingSnapshot.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(표시상품목록))]
    [NotifyPropertyChangedFor(nameof(상품검색결과없음))]
    public partial string 상품검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(표시출품행목록))]
    [NotifyPropertyChangedFor(nameof(출품검색결과없음))]
    public partial string 출품검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(상품없음))]
    [NotifyPropertyChangedFor(nameof(계정없음))]
    [NotifyPropertyChangedFor(nameof(출품없음))]
    [NotifyPropertyChangedFor(nameof(상품검색결과없음))]
    [NotifyPropertyChangedFor(nameof(출품검색결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public ProductListingSnapshot 스냅샷
    {
        get => _스냅샷;
        private set
        {
            if (!SetProperty(ref _스냅샷, value))
            {
                return;
            }

            NotifyProjectionChanged();
        }
    }

    public IReadOnlyList<판매상품항목응답> 표시상품목록
        => 스냅샷.상품목록
            .Where(MatchesProductSearch)
            .OrderByDescending(item => item.Id)
            .ToArray();

    public IReadOnlyList<ProductListingLedgerRow> 출품행목록
        => 스냅샷.출품목록
            .Select(listing => new ProductListingLedgerRow(
                listing,
                스냅샷.상품목록.FirstOrDefault(product => product.Id == listing.판매상품Id),
                스냅샷.계정목록.FirstOrDefault(account => account.Id == listing.판매채널계정Id)))
            .OrderByDescending(item => item.Id)
            .ToArray();

    public IReadOnlyList<ProductListingLedgerRow> 표시출품행목록
        => 출품행목록.Where(MatchesListingSearch).ToArray();

    public bool 상품없음 => 초기화됨 && 스냅샷.상품목록.Count == 0;
    public bool 계정없음 => 초기화됨 && 스냅샷.계정목록.Count == 0;
    public bool 출품없음 => 초기화됨 && 스냅샷.출품목록.Count == 0;
    public bool 상품검색결과없음 => 초기화됨 && !상품없음 && 표시상품목록.Count == 0;
    public bool 출품검색결과없음 => 초기화됨 && !출품없음 && 표시출품행목록.Count == 0;
    public int 검토필요수 => 스냅샷.출품목록.Count(item =>
        item.동기화상태 is SalesStatusCodes.SyncPending or SalesStatusCodes.SyncManual
        || !string.IsNullOrWhiteSpace(item.에러메시지));

    public 판매상품항목응답? 상품찾기(long productId)
        => 스냅샷.상품목록.FirstOrDefault(item => item.Id == productId);

    public 채널출품항목응답? 출품찾기(long listingId)
        => 스냅샷.출품목록.FirstOrDefault(item => item.Id == listingId);

    public ProductListingLedgerRow? 출품행찾기(long listingId)
        => 출품행목록.FirstOrDefault(item => item.Id == listingId);

    public 채널출품항목응답? 동일조합찾기(long productId, long accountId)
        => 스냅샷.출품목록.FirstOrDefault(item =>
            item.판매상품Id == productId && item.판매채널계정Id == accountId);

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                var productsTask = productService.상품목록조회Async(token);
                var accountsTask = accountService.계정목록조회Async(token);
                var listingsTask = listingService.출품목록조회Async(token);

                await Task.WhenAll(productsTask, accountsTask, listingsTask);
                스냅샷 = new ProductListingSnapshot(
                    await productsTask,
                    await accountsTask,
                    await listingsTask);
                초기화됨 = true;
            },
            "판매상품·채널 계정·Simulation 출품 원장을 새로고침했습니다.",
            cancellationToken,
            ex => $"출품 관리 현황을 불러오지 못했습니다. {ex.Message}");

    private bool MatchesProductSearch(판매상품항목응답 product)
    {
        var search = 상품검색어.Trim();
        return search.Length == 0
               || product.대표상품명.Contains(search, StringComparison.OrdinalIgnoreCase)
               || product.판매SKU.Contains(search, StringComparison.OrdinalIgnoreCase)
               || product.상태.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesListingSearch(ProductListingLedgerRow row)
    {
        var search = 출품검색어.Trim();
        return search.Length == 0
               || row.상품명.Contains(search, StringComparison.OrdinalIgnoreCase)
               || row.판매Sku.Contains(search, StringComparison.OrdinalIgnoreCase)
               || row.채널종류.Contains(search, StringComparison.OrdinalIgnoreCase)
               || row.상점명.Contains(search, StringComparison.OrdinalIgnoreCase)
               || row.출품.채널상품번호.Contains(search, StringComparison.OrdinalIgnoreCase)
               || row.출품.동기화상태.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private void NotifyProjectionChanged()
    {
        OnPropertyChanged(nameof(표시상품목록));
        OnPropertyChanged(nameof(출품행목록));
        OnPropertyChanged(nameof(표시출품행목록));
        OnPropertyChanged(nameof(상품없음));
        OnPropertyChanged(nameof(계정없음));
        OnPropertyChanged(nameof(출품없음));
        OnPropertyChanged(nameof(상품검색결과없음));
        OnPropertyChanged(nameof(출품검색결과없음));
        OnPropertyChanged(nameof(검토필요수));
    }
}

/// <summary>사용자가 고른 상품과 정확한 accountId 한 건의 payload 검토 초안만 관리합니다.</summary>
public sealed partial class ProductListingDraftViewModel(
    I판매채널계정읽기Service accountService,
    ICommerceChannelListingService preparationService) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 선택상품Id { get; private set; }

    [ObservableProperty]
    public partial 판매상품항목응답? 선택상품 { get; private set; }

    [ObservableProperty]
    public partial bool 상품찾을수없음 { get; private set; }

    [ObservableProperty]
    public partial long? 선택계정Id { get; private set; }

    [ObservableProperty]
    public partial 판매채널계정항목응답? 선택계정 { get; private set; }

    [ObservableProperty]
    public partial bool 계정찾을수없음 { get; private set; }

    [ObservableProperty]
    public partial CommerceChannelListingPreparation? 준비결과 { get; private set; }

    [ObservableProperty]
    public partial 채널출품항목응답? 기존출품 { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(생성가능))]
    public partial bool 외부효과없음확인 { get; set; }

    public bool 선택완료 => 선택상품 is not null && 선택계정 is not null && 준비결과 is not null;
    public bool 중복출품 => 기존출품 is not null;
    public bool 생성가능 => 선택완료 && !중복출품 && 외부효과없음확인;

    public async Task<bool> 상품선택Async(
        long? productId,
        ProductListingSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        작업상태초기화();
        선택상품Id = productId;
        선택상품 = productId is long id
            ? snapshot.상품목록.FirstOrDefault(item => item.Id == id)
            : null;
        상품찾을수없음 = productId.HasValue && 선택상품 is null;
        준비결과 = null;
        외부효과없음확인 = false;
        출품상태동기화(snapshot.출품목록);
        NotifyDraftStateChanged();

        if (상품찾을수없음)
        {
            return 유효성실패($"판매상품 #{productId}을 찾을 수 없습니다. 다른 상품으로 대체하지 않았습니다.");
        }

        return 선택상품 is not null && 선택계정 is not null
            ? await 초안준비Async(cancellationToken)
            : true;
    }

    public async Task<bool> 계정선택Async(
        long? accountId,
        ProductListingSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        작업상태초기화();
        선택계정Id = accountId;
        선택계정 = null;
        계정찾을수없음 = false;
        준비결과 = null;
        외부효과없음확인 = false;
        출품상태동기화(snapshot.출품목록);
        NotifyDraftStateChanged();

        if (accountId is null)
        {
            return true;
        }

        var succeeded = await 작업실행Async(
            async token =>
            {
                선택계정 = await accountService.계정상세조회Async(accountId.Value, token);
                계정찾을수없음 = 선택계정 is null;
                if (선택계정 is null)
                {
                    throw new KeyNotFoundException($"판매채널 계정 #{accountId}을 찾을 수 없습니다. 다른 계정으로 대체하지 않았습니다.");
                }

                if (선택상품 is not null)
                {
                    준비결과 = await preparationService.PrepareListingAsync(선택계정, 선택상품, token);
                }
            },
            "정확한 채널 계정과 payload 검토 초안을 준비했습니다.",
            cancellationToken,
            ex => ex.Message);

        출품상태동기화(snapshot.출품목록);
        NotifyDraftStateChanged();
        return succeeded;
    }

    public async Task 선택상태재검증Async(
        ProductListingSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        if (선택상품Id is long productId)
        {
            선택상품 = snapshot.상품목록.FirstOrDefault(item => item.Id == productId);
            상품찾을수없음 = 선택상품 is null;
        }

        출품상태동기화(snapshot.출품목록);
        if (선택계정Id is long accountId)
        {
            await 계정선택Async(accountId, snapshot, cancellationToken);
        }
        else
        {
            NotifyDraftStateChanged();
        }
    }

    public void 출품상태동기화(IReadOnlyList<채널출품항목응답> listings)
    {
        기존출품 = 선택상품Id is long productId && 선택계정Id is long accountId
            ? listings.FirstOrDefault(item =>
                item.판매상품Id == productId && item.판매채널계정Id == accountId)
            : null;
        NotifyDraftStateChanged();
    }

    private async Task<bool> 초안준비Async(CancellationToken cancellationToken)
    {
        if (선택상품 is null || 선택계정 is null)
        {
            준비결과 = null;
            NotifyDraftStateChanged();
            return true;
        }

        var succeeded = await 작업실행Async(
            async token => 준비결과 = await preparationService.PrepareListingAsync(선택계정, 선택상품, token),
            "선택한 상품과 정확한 계정으로 payload 검토 초안을 준비했습니다.",
            cancellationToken,
            ex => $"payload 검토 초안을 준비하지 못했습니다. {ex.Message}");
        NotifyDraftStateChanged();
        return succeeded;
    }

    private void NotifyDraftStateChanged()
    {
        OnPropertyChanged(nameof(선택완료));
        OnPropertyChanged(nameof(중복출품));
        OnPropertyChanged(nameof(생성가능));
    }
}

/// <summary>명시적으로 확인된 조합의 로컬 Simulation 출품 원장 생성과 같은 ID 재조회 결과만 담당합니다.</summary>
public sealed partial class ProductListingCreateViewModel(
    I채널출품Service listingService) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial 채널출품항목응답? 명령결과 { get; private set; }

    [ObservableProperty]
    public partial 채널출품항목응답? 재조회결과 { get; private set; }

    public async Task<bool> 생성Async(
        ProductListingDraftViewModel draft,
        CancellationToken cancellationToken = default)
    {
        if (!draft.외부효과없음확인)
        {
            return 유효성실패("외부 API를 호출하지 않는 Simulation 원장 생성임을 확인해 주세요.");
        }

        if (draft.선택상품Id is not long productId || draft.선택계정Id is not long accountId || !draft.선택완료)
        {
            return 유효성실패("판매상품과 정확한 판매채널 계정의 payload 초안을 먼저 준비해 주세요.");
        }

        if (draft.기존출품 is not null)
        {
            return 유효성실패($"같은 상품·계정 조합의 Simulation 출품 #{draft.기존출품.Id}이 이미 있습니다.");
        }

        명령결과 = null;
        재조회결과 = null;
        return await 작업실행Async(
            async token =>
            {
                명령결과 = await listingService.출품생성Async(new 채널출품저장요청
                {
                    판매상품Id = productId,
                    판매채널계정Id = accountId
                }, token) ?? throw new InvalidOperationException("Simulation 출품 원장 생성 응답이 비어 있습니다.");
            },
            "로컬 Simulation 출품 원장을 생성했습니다. 외부 상품 API는 호출하지 않았습니다.",
            cancellationToken,
            ex => $"Simulation 출품 원장을 생성하지 못했습니다. {ex.Message}");
    }

    public bool 재조회결과적용(IReadOnlyList<채널출품항목응답> listings)
    {
        if (명령결과 is null)
        {
            return 유효성실패("재조회할 Simulation 출품 ID가 없습니다.");
        }

        재조회결과 = listings.FirstOrDefault(item => item.Id == 명령결과.Id);
        return 재조회결과 is not null
            || 유효성실패($"생성한 Simulation 출품 #{명령결과.Id}을 같은 ID로 다시 찾지 못했습니다.");
    }
}

/// <summary>조회, 정확한 선택·검토, 로컬 원장 생성과 같은 ID 재조회 순서만 조립합니다.</summary>
public sealed class ProductListingsPageViewModel : 화주PageViewModelBase
{
    public ProductListingsPageViewModel(
        ProductListingReadViewModel read,
        ProductListingDraftViewModel draft,
        ProductListingCreateViewModel create)
    {
        조회 = 하위ViewModel등록(read);
        초안 = 하위ViewModel등록(draft);
        생성 = 하위ViewModel등록(create);
    }

    public ProductListingReadViewModel 조회 { get; }
    public ProductListingDraftViewModel 초안 { get; }
    public ProductListingCreateViewModel 생성 { get; }
    protected override bool 하위ViewModel처리중
        => 조회.처리중 || 초안.처리중 || 생성.처리중;

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        if (!await 조회.조회Async(cancellationToken))
        {
            throw new InvalidOperationException(
                조회.오류메시지 ?? "상품 출품 후보를 조회하지 못했습니다.");
        }

        await 초안.선택상태재검증Async(조회.스냅샷, cancellationToken);
    }

    public Task<bool> 상품선택Async(long? productId, CancellationToken cancellationToken = default)
        => 처리중
            ? Task.FromResult(false)
            : 초안.상품선택Async(productId, 조회.스냅샷, cancellationToken);

    public Task<bool> 계정선택Async(long? accountId, CancellationToken cancellationToken = default)
        => 처리중
            ? Task.FromResult(false)
            : 초안.계정선택Async(accountId, 조회.스냅샷, cancellationToken);

    public async Task<bool> Simulation원장생성Async(CancellationToken cancellationToken = default)
    {
        if (처리중 || !await 생성.생성Async(초안, cancellationToken))
        {
            return false;
        }

        if (!await 조회.조회Async(cancellationToken))
        {
            return false;
        }

        초안.출품상태동기화(조회.스냅샷.출품목록);
        return 생성.재조회결과적용(조회.스냅샷.출품목록);
    }
}
