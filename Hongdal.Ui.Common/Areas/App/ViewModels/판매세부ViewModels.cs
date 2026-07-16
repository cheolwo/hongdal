using Hongdal.Contracts.Common.Sales;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public abstract class 판매업무조각ViewModelBase(
    판매업무상태ViewModel 상태,
    string 업무코드,
    string 업무명,
    업무조각유형 업무유형) : 판매업무ViewModelBase(상태), I업무조각ViewModel
{
    public string 업무코드 { get; } = 업무코드;
    public string 업무명 { get; } = 업무명;
    public 업무조각유형 업무유형 { get; } = 업무유형;
}

public sealed class 판매채널계정조회ViewModel(
    I판매채널계정Service service,
    판매업무상태ViewModel 상태)
    : 판매업무조각ViewModelBase(
        상태,
        "sales-channel-account-query",
        "판매채널 계정 조회",
        업무조각유형.목록조회), I목록조회ViewModel<판매채널계정항목응답>
{
    private IReadOnlyList<string> _지원채널목록 = [];

    public IReadOnlyList<판매채널계정항목응답> 항목목록
        => _지원채널목록.Count == 0
            ? 판매상태.계정목록
            : 판매상태.계정목록.Where(item => _지원채널목록.Contains(
                item.채널종류,
                StringComparer.OrdinalIgnoreCase)).ToArray();

    public 판매채널계정항목응답? 선택된항목 => 판매상태.선택된계정;

    public void 지원채널설정(IEnumerable<string>? channelTypes)
    {
        _지원채널목록 = channelTypes?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
        OnPropertyChanged(nameof(항목목록));
    }

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token => 판매상태.계정목록적용(await service.계정목록조회Async(token)),
            "판매채널 계정을 조회했습니다.",
            cancellationToken);

    public bool 선택(long accountId)
        => 판매상태.계정선택(accountId)
           || 유효성실패("목록에 있는 판매채널 계정을 선택해 주세요.");
}

public sealed class 판매채널계정등록ViewModel(
    I판매채널계정Service service,
    판매업무상태ViewModel 상태)
    : 판매업무조각ViewModelBase(
        상태,
        "sales-channel-account-create",
        "판매채널 계정 등록",
        업무조각유형.등록), I명령ViewModel<판매채널계정저장요청>
{
    private IReadOnlyList<string> _지원채널목록 = [];
    private 판매채널계정저장요청 _초안 = new();

    public 판매채널계정저장요청 초안
    {
        get => _초안;
        private set => SetProperty(ref _초안, value);
    }

    public void 지원채널설정(IEnumerable<string>? channelTypes)
        => _지원채널목록 = channelTypes?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

    public async Task<bool> 실행Async(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(초안.채널종류) || string.IsNullOrWhiteSpace(초안.상점명))
        {
            return 유효성실패("판매채널 종류와 상점명을 입력해 주세요.");
        }

        if (_지원채널목록.Count > 0
            && !_지원채널목록.Contains(초안.채널종류, StringComparer.OrdinalIgnoreCase))
        {
            return 유효성실패("현재 판매 경로에서 지원하는 채널을 선택해 주세요.");
        }

        return await 작업실행Async(
            async token =>
            {
                var result = await service.계정생성Async(초안, token)
                    ?? throw new InvalidOperationException("판매채널 계정 생성 응답이 비어 있습니다.");
                판매상태.계정저장적용(result);
                초안 = new 판매채널계정저장요청 { 채널종류 = result.채널종류 };
            },
            "판매채널 계정을 등록했습니다.",
            cancellationToken);
    }

    public void 입력변경알림() => OnPropertyChanged(nameof(초안));
}

public sealed class 판매상품조회ViewModel(
    I상품등록Service service,
    판매업무상태ViewModel 상태)
    : 판매업무조각ViewModelBase(
        상태,
        "sales-product-query",
        "판매상품 조회",
        업무조각유형.목록조회), I목록조회ViewModel<판매상품항목응답>
{
    public IReadOnlyList<판매상품항목응답> 항목목록 => 판매상태.상품목록;
    public 판매상품항목응답? 선택된항목 => 판매상태.선택된상품;

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token => 판매상태.상품목록적용(await service.상품목록조회Async(token)),
            "판매상품을 조회했습니다.",
            cancellationToken);

    public bool 선택(long productId)
        => 판매상태.상품선택(productId)
           || 유효성실패("목록에 있는 판매상품을 선택해 주세요.");
}

public sealed class 판매상품등록ViewModel(
    I상품등록Service service,
    판매업무상태ViewModel 상태)
    : 판매업무조각ViewModelBase(
        상태,
        "sales-product-create",
        "판매상품 등록",
        업무조각유형.등록), I명령ViewModel<판매상품저장요청>
{
    private 판매상품저장요청 _초안 = new();

    public 판매상품저장요청 초안
    {
        get => _초안;
        private set => SetProperty(ref _초안, value);
    }

    public void 입고상품연결(long inboundProductId, string? productName = null, string? sku = null)
    {
        초안.입고상품Id = inboundProductId;
        if (!string.IsNullOrWhiteSpace(productName))
        {
            초안.대표상품명 = productName;
        }

        if (string.IsNullOrWhiteSpace(초안.판매SKU) && !string.IsNullOrWhiteSpace(sku))
        {
            초안.판매SKU = sku;
        }

        OnPropertyChanged(nameof(초안));
    }

    public async Task<bool> 실행Async(CancellationToken cancellationToken = default)
    {
        if (초안.입고상품Id <= 0)
        {
            return 유효성실패("연결할 입고상품을 선택해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(초안.대표상품명)
            || string.IsNullOrWhiteSpace(초안.판매SKU)
            || 초안.판매가 <= 0)
        {
            return 유효성실패("대표상품명, 판매 SKU와 0원보다 큰 판매가를 입력해 주세요.");
        }

        return await 작업실행Async(
            async token =>
            {
                var result = await service.상품생성Async(초안, token)
                    ?? throw new InvalidOperationException("판매상품 생성 응답이 비어 있습니다.");
                판매상태.상품저장적용(result);
                초안 = new 판매상품저장요청
                {
                    입고상품Id = result.입고상품Id,
                    대표상품명 = result.대표상품명,
                    판매SKU = result.판매SKU,
                    판매가 = result.판매가
                };
            },
            "판매상품을 등록했습니다.",
            cancellationToken);
    }

    public void 입력변경알림() => OnPropertyChanged(nameof(초안));
}

public sealed class 채널출품조회ViewModel(
    I채널출품Service service,
    판매업무상태ViewModel 상태)
    : 판매업무조각ViewModelBase(
        상태,
        "sales-listing-query",
        "채널 출품 조회",
        업무조각유형.목록조회), I목록조회ViewModel<채널출품항목응답>
{
    public IReadOnlyList<채널출품항목응답> 항목목록 => 판매상태.출품목록;
    public 채널출품항목응답? 선택된항목 => 판매상태.선택된출품;

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token => 판매상태.출품목록적용(await service.출품목록조회Async(token)),
            "채널 출품을 조회했습니다.",
            cancellationToken);

    public bool 선택(long listingId)
        => 판매상태.출품선택(listingId)
           || 유효성실패("목록에 있는 채널 출품을 선택해 주세요.");
}

public sealed class 채널출품등록ViewModel(
    I채널출품Service service,
    판매업무상태ViewModel 상태)
    : 판매업무조각ViewModelBase(
        상태,
        "sales-listing-create",
        "채널 출품 등록",
        업무조각유형.등록), I명령ViewModel<채널출품저장요청>
{
    private 채널출품저장요청 _초안 = new();

    public 채널출품저장요청 초안
    {
        get => _초안;
        private set => SetProperty(ref _초안, value);
    }

    public bool 계정선택(long accountId)
    {
        if (!판매상태.계정선택(accountId))
        {
            return 유효성실패("출품할 판매채널 계정을 선택해 주세요.");
        }

        초안.판매채널계정Id = accountId;
        OnPropertyChanged(nameof(초안));
        return true;
    }

    public bool 상품선택(long productId)
    {
        if (!판매상태.상품선택(productId))
        {
            return 유효성실패("출품할 판매상품을 선택해 주세요.");
        }

        초안.판매상품Id = productId;
        OnPropertyChanged(nameof(초안));
        return true;
    }

    public async Task<bool> 실행Async(CancellationToken cancellationToken = default)
    {
        if (초안.판매상품Id <= 0 || 초안.판매채널계정Id <= 0)
        {
            return 유효성실패("출품할 판매상품과 판매채널 계정을 선택해 주세요.");
        }

        return await 작업실행Async(
            async token =>
            {
                var result = await service.출품생성Async(초안, token)
                    ?? throw new InvalidOperationException("채널 출품 응답이 비어 있습니다.");
                판매상태.출품저장적용(result);
            },
            "채널 출품을 등록했습니다.",
            cancellationToken);
    }

    public void 입력변경알림() => OnPropertyChanged(nameof(초안));
}
