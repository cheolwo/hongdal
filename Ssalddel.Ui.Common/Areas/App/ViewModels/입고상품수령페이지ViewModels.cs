using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

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
    internal I입출고작업Service Service => service;

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

/// <summary>선택한 입고 예정 한 건의 도착·수령 기록과 정확한 입고상품 ID 확인을 관리합니다.</summary>
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WarehouseInboundVertical,
    SsalddelCodeLayer.ViewModel,
    "입고 예정 한 건의 실제 수량과 검수 대기 위치를 확인하고 운영 입고 완료 Command를 요청한다.",
    Effects = SsalddelCodeEffect.UiStateMutation,
    ContractType = typeof(입고완료요청),
    FlowOrder = 20,
    Boundary = "운영 MAUI 조작 상태이며 서버 응답 전에는 수령 완료를 확정하지 않는다.")]
public sealed partial class 입고수령완료ViewModel(
    I입출고작업Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    [NotifyPropertyChangedFor(nameof(공통상태코드))]
    public partial 입고요청항목응답? 대상 { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial string 상품명 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial string 상품Sku { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 옵션명 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial int 실제입고수량 { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial int 불량수량 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial string 검수대기위치 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial bool 도착상품수량확인 { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<입고상품항목응답> 결과목록 { get; private set; } = [];

    [ObservableProperty]
    public partial long? 완료된입고상품Id { get; private set; }

    public string 공통상태코드 => 창고입고업무상태Adapter.정규화(대상?.상태 ?? string.Empty);

    public bool 완료가능
        => !처리중
           && 대상 is { Id: > 0 }
           && string.Equals(공통상태코드, 창고입고상태코드.입고예정, StringComparison.Ordinal)
           && !string.IsNullOrWhiteSpace(상품명)
           && !string.IsNullOrWhiteSpace(상품Sku)
           && 실제입고수량 is >= 1 and <= 100_000
           && 불량수량 >= 0
           && 불량수량 <= 실제입고수량
           && 검수대기위치.Trim().Length is > 0 and <= 100
           && 도착상품수량확인;

    public void 대상준비(입고요청항목응답? item)
    {
        대상 = item;
        상품명 = item?.예정상품명?.Trim() ?? string.Empty;
        상품Sku = item?.예정SKU?.Trim() ?? string.Empty;
        옵션명 = string.Empty;
        실제입고수량 = Math.Max(1, item?.예정수량 ?? 1);
        불량수량 = 0;
        검수대기위치 = string.Empty;
        도착상품수량확인 = false;
        결과목록 = [];
        완료된입고상품Id = null;
        작업상태초기화();
        OnPropertyChanged(nameof(공통상태코드));
        OnPropertyChanged(nameof(완료가능));
    }

    public Task<bool> 수령기록Async(CancellationToken cancellationToken = default)
    {
        if (!완료가능 || 대상 is not { Id: > 0 } inbound)
            return Task.FromResult(유효성실패("실제 입고 수량, 불량 수량, 검수 대기 위치와 도착 확인을 완료해 주세요."));

        var expectedSku = NormalizeSku(상품Sku);
        return 작업실행Async(
            async token =>
            {
                var items = await service.입고완료Async(inbound.Id, new 입고완료요청
                {
                    Items =
                    [
                        new 입고상품저장요청
                        {
                            상품명 = 상품명.Trim(),
                            SKU = 상품Sku.Trim(),
                            옵션명 = 옵션명.Trim(),
                            입고수량 = 실제입고수량,
                            불량수량 = 불량수량,
                            보관위치 = 검수대기위치.Trim(),
                        },
                    ],
                }, token);
                var matched = items.Where(item =>
                        item.입고요청Id == inbound.Id
                        && string.Equals(NormalizeSku(item.SKU), expectedSku, StringComparison.Ordinal))
                    .ToArray();
                if (items.Count != 1 || matched.Length != 1)
                    throw new InvalidOperationException("입고 완료 응답이 선택한 한 상품과 정확히 일치하지 않습니다.");
                결과목록 = items;
                완료된입고상품Id = matched[0].Id;
            },
            "도착·수령 기록을 저장하고 입고상품 ID를 확인했습니다.",
            cancellationToken,
            ex => $"도착·수령 기록을 저장하지 못했습니다. {ex.Message}");
    }

    public bool 재조회결과확인(입고요청항목응답? reloaded)
    {
        if (대상 is not { } original
            || reloaded?.Id != original.Id
            || !string.Equals(
                창고입고업무상태Adapter.정규화(reloaded.상태),
                창고입고상태코드.검수대기,
                StringComparison.Ordinal))
            return 유효성실패("수령 기록 뒤 같은 입고 요청이 검수 대기 상태인지 확인하지 못했습니다.");

        대상 = reloaded;
        OnPropertyChanged(nameof(공통상태코드));
        OnPropertyChanged(nameof(완료가능));
        return 완료된입고상품Id is > 0;
    }

    private static string NormalizeSku(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}

/// <summary>창고 선택, 바코드 검색, 현장 요청 작성과 저장 후 같은 ID 재조회만 조립합니다.</summary>
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WarehouseInboundVertical,
    SsalddelCodeLayer.ViewModel,
    "창고 선택부터 수령 기록, 같은 입고 요청 재조회와 검수 인계까지 한 건의 MAUI 흐름을 조립한다.",
    Effects = SsalddelCodeEffect.UiStateMutation,
    ContractType = typeof(입고완료요청),
    FlowOrder = 10,
    Boundary = "운영 Command 성공과 같은 ID 재조회가 모두 확인되어야 다음 검수 단계로 인계한다.")]
public sealed class 입고상품수령PageViewModel : PageViewModelBase
{
    private long? _초기창고Id;
    private long? _초기입고요청Id;

    public 입고상품수령PageViewModel(
        입고상품수령창고ViewModel warehouses,
        입고예정상품검색ViewModel search,
        현장입고요청작성ViewModel writer,
        입고상품수령상세ViewModel detail,
        입고수령완료ViewModel? receiver = null)
    {
        창고 = 하위ViewModel등록(warehouses);
        검색 = 하위ViewModel등록(search);
        작성 = 하위ViewModel등록(writer);
        상세 = 하위ViewModel등록(detail);
        수령 = 하위ViewModel등록(receiver ?? new 입고수령완료ViewModel(detail.Service));
    }

    public 입고상품수령창고ViewModel 창고 { get; }
    public 입고예정상품검색ViewModel 검색 { get; }
    public 현장입고요청작성ViewModel 작성 { get; }
    public 입고상품수령상세ViewModel 상세 { get; }
    public 입고수령완료ViewModel 수령 { get; }

    protected override bool 하위ViewModel처리중
        => 창고.처리중 || 검색.처리중 || 작성.처리중 || 상세.처리중 || 수령.처리중;

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
        수령.대상준비(null);
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
        수령.대상준비(null);
        return true;
    }

    public Task<bool> 검색Async(CancellationToken cancellationToken = default)
        => 검색.검색Async(창고.선택된창고Id ?? 0, cancellationToken);

    public void 상품바코드변경(string? productBarcode)
    {
        검색.검색어설정(productBarcode);
        작성.닫기();
        상세.조회대상설정(null);
        수령.대상준비(null);
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
        var selected = queried
               && !상세.대상없음
               && 상세.항목?.Id == inboundId;
        수령.대상준비(selected ? 상세.항목 : null);
        return selected;
    }

    public async Task<bool> 수령기록후재조회Async(CancellationToken cancellationToken = default)
    {
        if (!await 수령.수령기록Async(cancellationToken)
            || 상세.입고요청Id is not { } inboundId
            || 수령.완료된입고상품Id is not > 0)
            return false;

        상세.조회대상설정(inboundId);
        return await 상세.조회Async(cancellationToken)
               && 수령.재조회결과확인(상세.항목);
    }

    public void 입고선택해제()
    {
        상세.조회대상설정(null);
        수령.대상준비(null);
    }

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
            수령.작업취소();
        }

        base.Dispose(disposing);
    }
}
