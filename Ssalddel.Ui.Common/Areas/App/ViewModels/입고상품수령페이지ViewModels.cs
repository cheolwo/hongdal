using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>접근 가능한 창고 목록의 조회와 명시적인 한 창고 선택만 관리합니다.</summary>
public sealed partial class 입고상품수령창고ViewModel(
    I입출고작업Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial IReadOnlyList<창고요약응답> 항목목록 { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(선택됨))]
    public partial long? 선택된창고Id { get; private set; }

    [ObservableProperty]
    public partial bool 초기화됨 { get; private set; }

    public bool 비어있음 => 초기화됨 && !오류발생 && 항목목록.Count == 0;
    public bool 선택됨 => 선택된창고Id.HasValue;
    public 창고요약응답? 선택된창고
        => 선택된창고Id is { } warehouseId
            ? 항목목록.FirstOrDefault(item => item.Id == warehouseId)
            : null;

    public async Task<bool> 초기화Async(
        long? initialWarehouseId = null,
        CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        var succeeded = await 작업실행Async(
            async token =>
            {
                항목목록 = (await service.창고목록조회Async(token))
                    .Where(item => item.IsActive)
                    .OrderByDescending(item => item.기본창고여부)
                    .ThenBy(item => item.창고명, StringComparer.Ordinal)
                    .ToArray();

                선택된창고Id = initialWarehouseId is > 0
                              && 항목목록.Any(item => item.Id == initialWarehouseId.Value)
                    ? initialWarehouseId
                    : 항목목록.Count == 1
                        ? 항목목록[0].Id
                        : null;
            },
            "입고 작업 창고를 조회했습니다.",
            cancellationToken,
            ex => $"입고 작업 창고를 조회하지 못했습니다. {ex.Message}");
        초기화됨 = true;
        OnPropertyChanged(nameof(비어있음));
        OnPropertyChanged(nameof(선택된창고));
        return succeeded;
    }

    public bool 선택(long? warehouseId)
    {
        if (warehouseId is not > 0 || 항목목록.All(item => item.Id != warehouseId.Value))
        {
            return 유효성실패("조회 가능한 창고를 선택해 주세요.");
        }

        선택된창고Id = warehouseId;
        작업상태초기화();
        OnPropertyChanged(nameof(선택된창고));
        return true;
    }
}

/// <summary>한 창고와 정확한 상품 바코드로 입고 예정 원장 후보만 조회합니다.</summary>
public sealed partial class 입고예정상품검색ViewModel(
    I입출고작업Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(검색가능))]
    public partial string 상품바코드 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial IReadOnlyList<입고요청항목응답> 후보목록 { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial bool 검색완료 { get; private set; }

    public bool 결과없음 => 검색완료 && !오류발생 && 후보목록.Count == 0;
    public bool 검색가능 => !처리중 && !string.IsNullOrWhiteSpace(상품바코드);

    public Task<bool> 검색Async(long warehouseId, CancellationToken cancellationToken = default)
    {
        if (warehouseId <= 0)
        {
            return Task.FromResult(유효성실패("입고 예정 상품을 조회할 창고를 선택해 주세요."));
        }

        var normalizedBarcode = NormalizeBarcode(상품바코드);
        if (normalizedBarcode.Length is < 1 or > 100)
        {
            return Task.FromResult(유효성실패("상품 바코드는 1자 이상 100자 이하로 입력해 주세요."));
        }

        검색완료 = false;
        후보목록 = [];
        return 작업실행Async(
            async token =>
            {
                var response = await service.입고목록조회Async(new 입고요청목록조회요청
                {
                    Page = 0,
                    PageSize = 50,
                    SortBy = nameof(입고요청항목응답.CreatedAtUtc),
                    SortDescending = true,
                    WarehouseId = warehouseId,
                    Status = 입고상태코드.예정,
                    Sku = normalizedBarcode
                }, token);
                후보목록 = response.Items
                    .Where(item => string.Equals(
                        NormalizeBarcode(item.예정SKU),
                        normalizedBarcode,
                        StringComparison.Ordinal))
                    .ToArray();
                검색완료 = true;
            },
            "정확한 상품 바코드의 입고 예정 원장을 조회했습니다.",
            cancellationToken,
            ex => $"입고 예정 상품을 조회하지 못했습니다. {ex.Message}");
    }

    public void 초기화()
    {
        상품바코드 = string.Empty;
        후보목록 = [];
        검색완료 = false;
        작업상태초기화();
    }

    public void 검색어설정(string? productBarcode)
    {
        상품바코드 = productBarcode ?? string.Empty;
        후보목록 = [];
        검색완료 = false;
        작업상태초기화();
    }

    private static string NormalizeBarcode(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}

/// <summary>계약 연결 전 현장 반입 요청의 입력, 안내 확인과 멱등 저장만 관리합니다.</summary>
public sealed partial class 현장입고요청작성ViewModel(
    I입출고작업Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial bool 폼표시 { get; private set; }

    [ObservableProperty]
    public partial Guid 클라이언트요청Id { get; private set; } = Guid.NewGuid();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial string 상품바코드 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial string 입고묶음바코드 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial string 상품명 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial string 공급처명 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial int 입고수량 { get; set; } = 1;

    [ObservableProperty]
    public partial string 보관조건 { get; set; } = 현장입고보관조건.미지정;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial string 현장입고사유 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial bool 임시입고안내확인 { get; set; }

    [ObservableProperty]
    public partial 입고요청항목응답? 등록응답 { get; private set; }

    public IReadOnlyList<string> 보관조건목록 => 현장입고보관조건.전체;
    public bool 제출가능
        => !처리중
           && !string.IsNullOrWhiteSpace(상품바코드)
           && !string.IsNullOrWhiteSpace(입고묶음바코드)
           && !string.IsNullOrWhiteSpace(상품명)
           && !string.IsNullOrWhiteSpace(공급처명)
           && 입고수량 is >= 1 and <= 100_000
           && 현장입고사유.Trim().Length >= 5
           && 임시입고안내확인;

    public void 새요청준비(string? productBarcode = null)
    {
        폼표시 = true;
        클라이언트요청Id = Guid.NewGuid();
        상품바코드 = NormalizeBarcode(productBarcode);
        입고묶음바코드 = BuildDefaultBundleBarcode(상품바코드);
        상품명 = string.Empty;
        공급처명 = string.Empty;
        입고수량 = 1;
        보관조건 = 현장입고보관조건.미지정;
        현장입고사유 = "입고 예정 또는 계약 연결을 확인하기 전 현장 반입";
        임시입고안내확인 = false;
        등록응답 = null;
        작업상태초기화();
    }

    public Task<bool> 등록Async(long warehouseId, CancellationToken cancellationToken = default)
    {
        if (warehouseId <= 0)
        {
            return Task.FromResult(유효성실패("현장 입고를 기록할 창고를 선택해 주세요."));
        }

        if (!제출가능)
        {
            return Task.FromResult(유효성실패("필수 현장 입고 정보와 안내 확인을 완료해 주세요."));
        }

        return 작업실행Async(
            async token =>
            {
                등록응답 = await service.현장입고요청생성Async(new 현장입고요청등록요청
                {
                    클라이언트요청Id = 클라이언트요청Id,
                    창고Id = warehouseId,
                    상품바코드 = NormalizeBarcode(상품바코드),
                    입고묶음바코드 = NormalizeBarcode(입고묶음바코드),
                    상품명 = 상품명.Trim(),
                    공급처명 = 공급처명.Trim(),
                    입고수량 = 입고수량,
                    보관조건 = 보관조건,
                    현장입고사유 = 현장입고사유.Trim(),
                    임시입고안내확인 = true,
                    안내버전 = 현장입고요청안내.현재버전
                }, token) ?? throw new InvalidOperationException("현장 입고 요청 저장 응답이 비어 있습니다.");
            },
            "현장 입고 요청을 입고 예정 원장에 저장했습니다.",
            cancellationToken,
            ex => $"현장 입고 요청을 저장하지 못했습니다. {ex.Message}");
    }

    public void 닫기()
    {
        if (!처리중)
        {
            폼표시 = false;
        }
    }

    private static string BuildDefaultBundleBarcode(string? seed)
    {
        var normalized = new string(NormalizeBarcode(seed)
            .Where(char.IsLetterOrDigit)
            .TakeLast(12)
            .ToArray());
        return $"BND:{(string.IsNullOrWhiteSpace(normalized) ? "UNPLANNED" : normalized)}";
    }

    private static string NormalizeBarcode(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}

/// <summary>입고상품 수령 페이지가 선택한 정확한 입고 ID 한 건의 재조회만 관리합니다.</summary>
public sealed partial class 입고상품수령상세ViewModel(
    I입출고작업Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 입고요청Id { get; private set; }

    [ObservableProperty]
    public partial 입고요청항목응답? 항목 { get; private set; }

    [ObservableProperty]
    public partial bool 대상없음 { get; private set; }

    public void 조회대상설정(long? inboundId)
    {
        var normalized = inboundId is > 0 ? inboundId : null;
        if (입고요청Id == normalized)
        {
            return;
        }

        입고요청Id = normalized;
        항목 = null;
        대상없음 = false;
        작업상태초기화();
    }

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
    {
        if (입고요청Id is not { } inboundId)
        {
            return Task.FromResult(유효성실패("조회할 입고 요청을 선택해 주세요."));
        }

        항목 = null;
        대상없음 = false;
        return 작업실행Async(
            async token =>
            {
                항목 = await service.입고상세조회Async(inboundId, token);
                대상없음 = 항목 is null;
            },
            "선택한 입고 요청을 같은 ID로 다시 조회했습니다.",
            cancellationToken,
            ex => $"입고 요청 상세를 조회하지 못했습니다. {ex.Message}");
    }
}

/// <summary>창고 선택, 바코드 검색, 현장 요청 작성과 저장 후 같은 ID 재조회만 조립합니다.</summary>
public sealed class 입고상품수령PageViewModel : PageViewModelBase
{
    private long? _초기창고Id;
    private long? _초기입고요청Id;

    public 입고상품수령PageViewModel(
        입고상품수령창고ViewModel warehouses,
        입고예정상품검색ViewModel search,
        현장입고요청작성ViewModel writer,
        입고상품수령상세ViewModel detail)
    {
        창고 = 하위ViewModel등록(warehouses);
        검색 = 하위ViewModel등록(search);
        작성 = 하위ViewModel등록(writer);
        상세 = 하위ViewModel등록(detail);
    }

    public 입고상품수령창고ViewModel 창고 { get; }
    public 입고예정상품검색ViewModel 검색 { get; }
    public 현장입고요청작성ViewModel 작성 { get; }
    public 입고상품수령상세ViewModel 상세 { get; }

    protected override bool 하위ViewModel처리중
        => 창고.처리중 || 검색.처리중 || 작성.처리중 || 상세.처리중;

    public Task<bool> 초기화Async(
        long? initialWarehouseId,
        long? inboundId = null,
        CancellationToken cancellationToken = default)
    {
        초기경로설정(initialWarehouseId, inboundId);
        return base.초기화Async(cancellationToken);
    }

    public Task<bool> 경로변경Async(
        long? initialWarehouseId,
        long? inboundId,
        CancellationToken cancellationToken = default)
    {
        초기경로설정(initialWarehouseId, inboundId);
        return base.새로고침Async(cancellationToken);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        var warehousesLoaded = await 창고.초기화Async(_초기창고Id, cancellationToken);
        if (!warehousesLoaded)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException(
                창고.오류메시지 ?? "입고 작업 창고를 조회하지 못했습니다.");
        }

        if (_초기입고요청Id is > 0)
        {
            var inboundLoaded = await 입고선택Async(_초기입고요청Id.Value, cancellationToken);
            if (!inboundLoaded)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException(
                    상세.오류메시지
                    ?? (상세.대상없음
                        ? "선택한 입고 요청을 찾을 수 없거나 조회 범위에 없습니다."
                        : "선택한 입고 요청을 조회하지 못했습니다."));
            }

            var item = 상세.항목!;
            if (!창고.선택(item.창고Id))
            {
                throw new InvalidOperationException(
                    "선택한 입고 요청의 창고가 현재 작업 범위에 없습니다.");
            }

            검색.검색어설정(item.예정SKU);
            return;
        }

        상세.조회대상설정(null);
    }

    public bool 창고선택(long? warehouseId)
    {
        if (!창고.선택(warehouseId))
        {
            return false;
        }

        검색.초기화();
        작성.닫기();
        상세.조회대상설정(null);
        return true;
    }

    public Task<bool> 검색Async(CancellationToken cancellationToken = default)
        => 검색.검색Async(창고.선택된창고Id ?? 0, cancellationToken);

    public void 상품바코드변경(string? productBarcode)
    {
        검색.검색어설정(productBarcode);
        작성.닫기();
        상세.조회대상설정(null);
    }

    public void 현장입고작성시작()
        => 작성.새요청준비(검색.상품바코드);

    public async Task<bool> 현장입고등록후조회Async(CancellationToken cancellationToken = default)
    {
        if (창고.선택된창고Id is not { } warehouseId
            || !await 작성.등록Async(warehouseId, cancellationToken)
            || 작성.등록응답 is not { } created)
        {
            return false;
        }

        var reloaded = await 입고선택Async(created.Id, cancellationToken);
        if (reloaded)
        {
            작성.닫기();
        }

        return reloaded;
    }

    public async Task<bool> 입고선택Async(long inboundId, CancellationToken cancellationToken = default)
    {
        상세.조회대상설정(inboundId);
        var queried = await 상세.조회Async(cancellationToken);
        return queried
               && !상세.대상없음
               && 상세.항목?.Id == inboundId;
    }

    public void 입고선택해제()
        => 상세.조회대상설정(null);

    private void 초기경로설정(long? initialWarehouseId, long? inboundId)
    {
        _초기창고Id = initialWarehouseId is > 0 ? initialWarehouseId : null;
        _초기입고요청Id = inboundId is > 0 ? inboundId : null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            창고.작업취소();
            검색.작업취소();
            작성.작업취소();
            상세.작업취소();
        }

        base.Dispose(disposing);
    }
}
