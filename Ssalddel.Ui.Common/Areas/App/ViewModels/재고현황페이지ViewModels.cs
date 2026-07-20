using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>재고 검색·상태 필터·서버 페이징 목록과 집계만 관리합니다.</summary>
public sealed partial class 재고현황목록ViewModel(
    I재고현황페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 조회상태 { get; set; } = 창고재고조회상태코드.전체;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(항목목록))]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    [NotifyPropertyChangedFor(nameof(이전페이지있음))]
    [NotifyPropertyChangedFor(nameof(다음페이지있음))]
    public partial 창고재고현황목록페이지응답 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<창고재고현황목록항목응답> 항목목록 => 응답.Items;
    public IReadOnlyList<string> 조회상태목록 => 창고재고조회상태코드.전체목록;
    public bool 비어있음 => 초기화됨 && !오류발생 && 항목목록.Count == 0;
    public bool 이전페이지있음 => 응답.Page > 0;
    public bool 다음페이지있음 => 응답.HasNextPage;

    public async Task<bool> 조회Async(int? page = null, CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        var targetPage = Math.Max(0, page ?? 응답.Page);
        var succeeded = await 작업실행Async(
            async token =>
            {
                응답 = await service.목록조회Async(new 창고재고현황목록조회요청
                {
                    Search = string.IsNullOrWhiteSpace(검색어) ? null : 검색어.Trim(),
                    Status = 창고재고조회상태코드.Normalize(조회상태),
                    Page = targetPage,
                    PageSize = 12
                }, token);
            },
            "재고 현황 목록을 조회했습니다.",
            cancellationToken,
            ex => $"재고 현황 목록을 조회하지 못했습니다. {ex.Message}");
        초기화됨 = true;
        OnPropertyChanged(nameof(비어있음));
        return succeeded;
    }

    public Task<bool> 이전페이지Async(CancellationToken cancellationToken = default)
        => 이전페이지있음 ? 조회Async(응답.Page - 1, cancellationToken) : Task.FromResult(false);

    public Task<bool> 다음페이지Async(CancellationToken cancellationToken = default)
        => 다음페이지있음 ? 조회Async(응답.Page + 1, cancellationToken) : Task.FromResult(false);
}

/// <summary>명시한 입고상품 ID 한 건의 재고 근거만 다시 조회합니다.</summary>
public sealed partial class 재고현황상세ViewModel(
    I재고현황페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 조회대상Id { get; private set; }

    [ObservableProperty]
    public partial 창고재고현황상세응답? 항목 { get; private set; }

    [ObservableProperty]
    public partial bool 대상없음 { get; private set; }

    public void 조회대상설정(long? inboundItemId)
    {
        var normalized = inboundItemId is > 0 ? inboundItemId : null;
        if (조회대상Id == normalized)
        {
            return;
        }

        조회대상Id = normalized;
        항목 = null;
        대상없음 = false;
        작업상태초기화();
    }

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
    {
        if (조회대상Id is not > 0)
        {
            return Task.FromResult(유효성실패("조회할 재고를 선택해 주세요."));
        }

        항목 = null;
        대상없음 = false;
        return 작업실행Async(
            async token =>
            {
                항목 = await service.상세조회Async(조회대상Id.Value, token);
                대상없음 = 항목 is null;
            },
            "선택한 재고를 같은 입고상품 ID로 다시 조회했습니다.",
            cancellationToken,
            ex => $"재고 상세를 조회하지 못했습니다. {ex.Message}");
    }
}

/// <summary>재고 목록과 정확한 상세의 선택·해제 순서만 조정합니다.</summary>
public sealed partial class 재고현황PageViewModel : 조립ViewModelBase
{
    public 재고현황PageViewModel(재고현황목록ViewModel list, 재고현황상세ViewModel detail)
    {
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 재고현황목록ViewModel 목록 { get; }
    public 재고현황상세ViewModel 상세 { get; }

    [ObservableProperty]
    public partial bool 초기화됨 { get; private set; }

    public bool 처리중 => 목록.처리중 || 상세.처리중;

    public async Task<bool> 초기화Async(long? inboundItemId = null, CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        var listLoaded = await 목록.조회Async(0, cancellationToken);
        if (inboundItemId is > 0)
        {
            await 대상선택Async(inboundItemId.Value, cancellationToken);
        }
        else
        {
            상세.조회대상설정(null);
        }

        초기화됨 = true;
        return listLoaded && (inboundItemId is not > 0 || 상세.항목 is not null);
    }

    public Task<bool> 검색Async(CancellationToken cancellationToken = default)
        => 목록.조회Async(0, cancellationToken);

    public async Task<bool> 대상선택Async(long inboundItemId, CancellationToken cancellationToken = default)
    {
        상세.조회대상설정(inboundItemId);
        var loaded = await 상세.조회Async(cancellationToken);
        return loaded && !상세.대상없음 && 상세.항목?.InboundItemId == inboundItemId;
    }

    public void 선택해제() => 상세.조회대상설정(null);
}
