using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>피킹 작업의 검색·상태 필터·서버 페이징 목록만 관리합니다.</summary>
public sealed partial class 피킹작업목록ViewModel(
    I피킹작업페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 조회상태 { get; set; } = 피킹작업조회상태코드.대기;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(항목목록))]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    [NotifyPropertyChangedFor(nameof(이전페이지있음))]
    [NotifyPropertyChangedFor(nameof(다음페이지있음))]
    public partial 피킹작업목록페이지응답 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<피킹작업목록항목응답> 항목목록 => 응답.Items;
    public IReadOnlyList<string> 조회상태목록 => 피킹작업조회상태코드.전체목록;
    public bool 비어있음 => 초기화됨 && !오류발생 && 항목목록.Count == 0;
    public bool 이전페이지있음 => 응답.Page > 0;
    public bool 다음페이지있음 => 응답.HasNextPage;

    public async Task<bool> 조회Async(
        int? page = null,
        CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        var targetPage = Math.Max(0, page ?? 응답.Page);
        var succeeded = await 작업실행Async(
            async token =>
            {
                응답 = await service.목록조회Async(new 피킹작업목록조회요청
                {
                    Search = string.IsNullOrWhiteSpace(검색어) ? null : 검색어.Trim(),
                    Status = 피킹작업조회상태코드.Normalize(조회상태),
                    Page = targetPage,
                    PageSize = 12
                }, token);
            },
            "피킹 작업 목록을 조회했습니다.",
            cancellationToken,
            ex => $"피킹 작업 목록을 조회하지 못했습니다. {ex.Message}");
        초기화됨 = true;
        OnPropertyChanged(nameof(비어있음));
        return succeeded;
    }

    public Task<bool> 이전페이지Async(CancellationToken cancellationToken = default)
        => 이전페이지있음 ? 조회Async(응답.Page - 1, cancellationToken) : Task.FromResult(false);

    public Task<bool> 다음페이지Async(CancellationToken cancellationToken = default)
        => 다음페이지있음 ? 조회Async(응답.Page + 1, cancellationToken) : Task.FromResult(false);
}

/// <summary>명시한 피킹 작업 Key 한 건의 상세 재조회만 관리합니다.</summary>
public sealed partial class 피킹작업상세ViewModel(
    I피킹작업페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string? 조회대상Key { get; private set; }

    [ObservableProperty]
    public partial 피킹작업상세응답? 항목 { get; private set; }

    [ObservableProperty]
    public partial bool 대상없음 { get; private set; }

    public void 조회대상설정(string? taskKey)
    {
        var normalized = string.IsNullOrWhiteSpace(taskKey) ? null : taskKey.Trim();
        if (string.Equals(조회대상Key, normalized, StringComparison.Ordinal))
        {
            return;
        }

        조회대상Key = normalized;
        항목 = null;
        대상없음 = false;
        작업상태초기화();
    }

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
    {
        if (조회대상Key is not { Length: > 0 } taskKey)
        {
            return Task.FromResult(유효성실패("조회할 피킹 작업을 선택해 주세요."));
        }

        항목 = null;
        대상없음 = false;
        return 작업실행Async(
            async token =>
            {
                항목 = await service.상세조회Async(taskKey, token);
                대상없음 = 항목 is null;
            },
            "선택한 피킹 작업을 같은 Key로 다시 조회했습니다.",
            cancellationToken,
            ex => $"피킹 작업 상세를 조회하지 못했습니다. {ex.Message}");
    }
}

/// <summary>선택된 피킹 작업의 시작·현장 확인·완료 Command만 관리합니다.</summary>
public sealed partial class 피킹작업처리ViewModel(
    I피킹작업페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(시작가능))]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial string? 대상Key { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(시작가능))]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial 피킹작업상세응답? 대상 { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial string 적재대확인코드 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial bool 상품확인 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(완료가능))]
    public partial bool 전체수량확인 { get; set; }

    [ObservableProperty]
    public partial 피킹작업결과응답? 결과 { get; private set; }

    public bool 시작가능 => !처리중 && 대상 is { CanStart: true } && 대상Key == 대상.TaskKey;

    public bool 완료가능
        => !처리중
           && 대상 is { CanComplete: true }
           && 대상Key == 대상.TaskKey
           && 적재대확인코드.Trim().Length is > 0 and <= 120
           && 상품확인
           && 전체수량확인;

    public void 대상준비(피킹작업상세응답? item)
    {
        대상 = item;
        대상Key = item?.TaskKey;
        적재대확인코드 = string.Empty;
        상품확인 = false;
        전체수량확인 = false;
        결과 = null;
        작업상태초기화();
        OnPropertyChanged(nameof(시작가능));
        OnPropertyChanged(nameof(완료가능));
    }

    public Task<bool> 시작Async(CancellationToken cancellationToken = default)
    {
        if (!시작가능 || 대상Key is not { Length: > 0 } taskKey)
        {
            return Task.FromResult(유효성실패("대기 상태의 피킹 작업을 다시 선택해 주세요."));
        }

        return 작업실행Async(
            async token =>
            {
                결과 = await service.시작Async(taskKey, token)
                    ?? throw new InvalidOperationException("피킹 작업 시작 응답이 비어 있습니다.");
            },
            "피킹 작업을 시작했습니다.",
            cancellationToken,
            ex => $"피킹 작업을 시작하지 못했습니다. {ex.Message}");
    }

    public Task<bool> 완료Async(CancellationToken cancellationToken = default)
    {
        if (!완료가능 || 대상Key is not { Length: > 0 } taskKey)
        {
            return Task.FromResult(유효성실패("적재대 코드와 상품·전체 수량 확인을 완료해 주세요."));
        }

        return 작업실행Async(
            async token =>
            {
                결과 = await service.완료Async(taskKey, new 피킹작업완료요청
                {
                    RackCode = 적재대확인코드.Trim(),
                    ProductConfirmed = 상품확인,
                    QuantityConfirmed = 전체수량확인
                }, token) ?? throw new InvalidOperationException("피킹 작업 완료 응답이 비어 있습니다.");
            },
            "피킹 작업을 완료했습니다.",
            cancellationToken,
            ex => $"피킹 작업을 완료하지 못했습니다. {ex.Message}");
    }
}

/// <summary>한 피킹 작업의 시작·완료 Command와 성공 뒤 같은 Key 재조회 순서만 조정합니다.</summary>
public sealed class 피킹작업실행ViewModel : PageViewModelBase
{
    private string _taskKey = string.Empty;

    public 피킹작업실행ViewModel(
        피킹작업상세ViewModel detail,
        피킹작업처리ViewModel action)
    {
        상세 = 하위ViewModel등록(detail);
        처리 = 하위ViewModel등록(action);
    }

    public 피킹작업상세ViewModel 상세 { get; }
    public 피킹작업처리ViewModel 처리 { get; }

    protected override bool 하위ViewModel처리중
        => 상세.처리중 || 처리.처리중;

    public Task<bool> 초기화Async(
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        _taskKey = NormalizeTaskKey(taskKey);
        return base.초기화Async(cancellationToken);
    }

    public Task<bool> 경로대상변경Async(
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        _taskKey = NormalizeTaskKey(taskKey);
        return base.새로고침Async(cancellationToken);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_taskKey))
        {
            throw new InvalidOperationException("실행할 피킹 작업 Key가 필요합니다.");
        }

        상세.조회대상설정(_taskKey);
        var loaded = await 상세.조회Async(cancellationToken);
        if (!loaded || !string.Equals(상세.항목?.TaskKey, _taskKey, StringComparison.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException(
                상세.오류메시지
                ?? (상세.대상없음
                    ? "선택한 피킹 작업을 찾을 수 없거나 조회 범위에 없습니다."
                    : "선택한 피킹 작업을 조회하지 못했습니다."));
        }

        처리.대상준비(상세.항목);
    }

    public Task<bool> 시작후재조회Async(CancellationToken cancellationToken = default)
        => 명령후재조회Async(처리.시작Async, 피킹포장작업상태코드.진행중, cancellationToken);

    public Task<bool> 완료후재조회Async(CancellationToken cancellationToken = default)
        => 명령후재조회Async(처리.완료Async, 피킹포장작업상태코드.완료, cancellationToken);

    private async Task<bool> 명령후재조회Async(
        Func<CancellationToken, Task<bool>> command,
        string expectedStatus,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(상세.항목?.TaskKey, _taskKey, StringComparison.Ordinal)
            || !await command(cancellationToken))
        {
            return false;
        }

        var detailReloaded = await 상세.조회Async(cancellationToken);
        처리.대상준비(상세.항목);
        return detailReloaded
               && 상세.항목 is { } reloaded
               && string.Equals(reloaded.TaskKey, _taskKey, StringComparison.Ordinal)
               && string.Equals(reloaded.Status, expectedStatus, StringComparison.Ordinal);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            상세.작업취소();
            처리.작업취소();
        }

        base.Dispose(disposing);
    }

    private static string NormalizeTaskKey(string taskKey)
        => string.IsNullOrWhiteSpace(taskKey) ? string.Empty : taskKey.Trim();
}

internal static class 피킹포장작업상태코드
{
    public const string 진행중 = "진행중";
    public const string 완료 = "완료";
}
