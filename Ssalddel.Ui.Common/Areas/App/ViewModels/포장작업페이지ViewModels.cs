using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed partial class 포장작업목록ViewModel(I포장작업페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty] public partial string 검색어 { get; set; } = string.Empty;
    [ObservableProperty] public partial string 조회상태 { get; set; } = 포장작업조회상태코드.대기;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(항목목록))] [NotifyPropertyChangedFor(nameof(비어있음))] [NotifyPropertyChangedFor(nameof(이전페이지있음))] [NotifyPropertyChangedFor(nameof(다음페이지있음))]
    public partial 포장작업목록페이지응답 응답 { get; private set; } = new();
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(비어있음))] public partial bool 초기화됨 { get; private set; }
    public IReadOnlyList<포장작업목록항목응답> 항목목록 => 응답.Items;
    public IReadOnlyList<string> 조회상태목록 => 포장작업조회상태코드.전체목록;
    public bool 비어있음 => 초기화됨 && !오류발생 && 항목목록.Count == 0;
    public bool 이전페이지있음 => 응답.Page > 0; public bool 다음페이지있음 => 응답.HasNextPage;
    public async Task<bool> 조회Async(int? page = null, CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        var succeeded = await 작업실행Async(async token => 응답 = await service.목록조회Async(new 포장작업목록조회요청
        {
            Search = string.IsNullOrWhiteSpace(검색어) ? null : 검색어.Trim(), Status = 포장작업조회상태코드.Normalize(조회상태),
            Page = Math.Max(0, page ?? 응답.Page), PageSize = 12
        }, token), "포장 작업 목록을 조회했습니다.", cancellationToken, ex => $"포장 작업 목록을 조회하지 못했습니다. {ex.Message}");
        초기화됨 = true; OnPropertyChanged(nameof(비어있음)); return succeeded;
    }
    public Task<bool> 이전페이지Async(CancellationToken cancellationToken = default) => 이전페이지있음 ? 조회Async(응답.Page - 1, cancellationToken) : Task.FromResult(false);
    public Task<bool> 다음페이지Async(CancellationToken cancellationToken = default) => 다음페이지있음 ? 조회Async(응답.Page + 1, cancellationToken) : Task.FromResult(false);
}

public sealed partial class 포장작업상세ViewModel(I포장작업페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty] public partial long? 조회대상Id { get; private set; }
    [ObservableProperty] public partial 포장작업상세응답? 항목 { get; private set; }
    [ObservableProperty] public partial bool 대상없음 { get; private set; }
    public void 조회대상설정(long? id) { var normalized = id is > 0 ? id : null; if (조회대상Id == normalized) return; 조회대상Id = normalized; 항목 = null; 대상없음 = false; 작업상태초기화(); }
    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
    {
        if (조회대상Id is not > 0) return Task.FromResult(유효성실패("조회할 포장 작업을 선택해 주세요."));
        항목 = null; 대상없음 = false;
        return 작업실행Async(async token => { 항목 = await service.상세조회Async(조회대상Id.Value, token); 대상없음 = 항목 is null; },
            "선택한 포장 작업을 같은 입고상품 ID로 다시 조회했습니다.", cancellationToken, ex => $"포장 작업 상세를 조회하지 못했습니다. {ex.Message}");
    }
}

public sealed partial class 포장작업작성ViewModel(I포장작업페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(완료가능))] public partial long? 대상Id { get; private set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(완료가능))] public partial 포장작업상세응답? 대상 { get; private set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(완료가능))] public partial int 포장수량 { get; private set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(완료가능))] public partial string 포장유형 { get; set; } = 포장유형코드.일반포장;
    [ObservableProperty] public partial string 메모 { get; set; } = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(완료가능))] public partial bool 재고확인 { get; set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(완료가능))] public partial bool 포장표찰확인 { get; set; }
    [ObservableProperty] public partial 포장작업결과응답? 결과 { get; private set; }
    public IReadOnlyList<string> 포장유형목록 => 포장유형코드.전체목록;
    public bool 완료가능 => !처리중 && 대상 is { CanPack: true } && 대상Id == 대상.InboundItemId
        && 포장수량 == 대상.AvailableQuantity && 포장수량 > 0 && 포장유형코드.IsValid(포장유형) && 메모.Trim().Length <= 400 && 재고확인 && 포장표찰확인;
    public void 대상준비(포장작업상세응답? item)
    {
        대상 = item; 대상Id = item?.InboundItemId; 포장수량 = item?.AvailableQuantity ?? 0; 포장유형 = 포장유형코드.일반포장;
        메모 = string.Empty; 재고확인 = false; 포장표찰확인 = false; 결과 = null; 작업상태초기화(); OnPropertyChanged(nameof(완료가능));
    }
    public Task<bool> 완료Async(CancellationToken cancellationToken = default)
    {
        if (!완료가능 || 대상Id is not > 0) return Task.FromResult(유효성실패("전체 가용수량과 두 현장 확인을 완료해 주세요."));
        return 작업실행Async(async token => 결과 = await service.완료Async(대상Id.Value, new 포장작업완료요청
        {
            PackagingQuantity = 포장수량, PackagingType = 포장유형, Memo = 메모.Trim(), InventoryConfirmed = 재고확인, PackageLabelConfirmed = 포장표찰확인
        }, token) ?? throw new InvalidOperationException("포장 완료 응답이 비어 있습니다."),
            "출고 준비용 포장을 완료했습니다.", cancellationToken, ex => $"포장 작업을 완료하지 못했습니다. {ex.Message}");
    }
}

public sealed partial class 포장작업PageViewModel : 조립ViewModelBase
{
    public 포장작업PageViewModel(포장작업목록ViewModel list, 포장작업상세ViewModel detail, 포장작업작성ViewModel editor)
    { 목록 = 하위ViewModel등록(list); 상세 = 하위ViewModel등록(detail); 작성 = 하위ViewModel등록(editor); }
    public 포장작업목록ViewModel 목록 { get; } public 포장작업상세ViewModel 상세 { get; } public 포장작업작성ViewModel 작성 { get; }
    [ObservableProperty] public partial bool 초기화됨 { get; private set; }
    public bool 처리중 => 목록.처리중 || 상세.처리중 || 작성.처리중;
    public async Task<bool> 초기화Async(long? id = null, CancellationToken cancellationToken = default)
    {
        초기화됨 = false; var listLoaded = await 목록.조회Async(0, cancellationToken);
        if (id is > 0) await 대상선택Async(id.Value, cancellationToken); else { 상세.조회대상설정(null); 작성.대상준비(null); }
        초기화됨 = true; return listLoaded && (id is not > 0 || 상세.항목 is not null);
    }
    public Task<bool> 검색Async(CancellationToken cancellationToken = default) => 목록.조회Async(0, cancellationToken);
    public async Task<bool> 대상선택Async(long id, CancellationToken cancellationToken = default)
    { 상세.조회대상설정(id); var loaded = await 상세.조회Async(cancellationToken); 작성.대상준비(상세.항목); return loaded && 상세.항목?.InboundItemId == id; }
    public async Task<bool> 완료후재조회Async(CancellationToken cancellationToken = default)
    {
        var id = 상세.항목?.InboundItemId; if (id is not > 0 || !await 작성.완료Async(cancellationToken)) return false;
        상세.조회대상설정(id); var loaded = await 상세.조회Async(cancellationToken); 작성.대상준비(상세.항목);
        await 목록.조회Async(목록.응답.Page, cancellationToken); return loaded && 상세.항목?.InventoryStatus.StartsWith("포장완료-", StringComparison.Ordinal) == true;
    }
    public void 선택해제() { 상세.조회대상설정(null); 작성.대상준비(null); }
}
