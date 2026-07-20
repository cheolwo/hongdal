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

/// <summary>목록, 정확한 상세, 시작·완료와 각 Command 뒤 같은 Key 재조회 순서만 조정합니다.</summary>
public sealed partial class 피킹작업PageViewModel : 조립ViewModelBase
{
    public 피킹작업PageViewModel(
        피킹작업목록ViewModel list,
        피킹작업상세ViewModel detail,
        피킹작업처리ViewModel action)
    {
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
        처리 = 하위ViewModel등록(action);
    }

    public 피킹작업목록ViewModel 목록 { get; }
    public 피킹작업상세ViewModel 상세 { get; }
    public 피킹작업처리ViewModel 처리 { get; }

    [ObservableProperty]
    public partial bool 초기화됨 { get; private set; }

    public bool 처리중 => 목록.처리중 || 상세.처리중 || 처리.처리중;

    public async Task<bool> 초기화Async(
        string? taskKey = null,
        CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        var listLoaded = await 목록.조회Async(0, cancellationToken);
        if (!string.IsNullOrWhiteSpace(taskKey))
        {
            await 대상선택Async(taskKey, cancellationToken);
        }
        else
        {
            상세.조회대상설정(null);
            처리.대상준비(null);
        }

        초기화됨 = true;
        return listLoaded && (string.IsNullOrWhiteSpace(taskKey) || 상세.항목 is not null);
    }

    public Task<bool> 검색Async(CancellationToken cancellationToken = default)
        => 목록.조회Async(0, cancellationToken);

    public async Task<bool> 대상선택Async(
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        상세.조회대상설정(taskKey);
        var loaded = await 상세.조회Async(cancellationToken);
        처리.대상준비(상세.항목);
        return loaded
               && !상세.대상없음
               && string.Equals(상세.항목?.TaskKey, taskKey.Trim(), StringComparison.Ordinal);
    }

    public async Task<bool> 시작후재조회Async(CancellationToken cancellationToken = default)
        => await 명령후재조회Async(처리.시작Async, 피킹포장작업상태코드.진행중, cancellationToken);

    public async Task<bool> 완료후재조회Async(CancellationToken cancellationToken = default)
        => await 명령후재조회Async(처리.완료Async, 피킹포장작업상태코드.완료, cancellationToken);

    public void 선택해제()
    {
        상세.조회대상설정(null);
        처리.대상준비(null);
    }

    private async Task<bool> 명령후재조회Async(
        Func<CancellationToken, Task<bool>> command,
        string expectedStatus,
        CancellationToken cancellationToken)
    {
        var taskKey = 상세.항목?.TaskKey;
        if (string.IsNullOrWhiteSpace(taskKey) || !await command(cancellationToken))
        {
            return false;
        }

        상세.조회대상설정(taskKey);
        var detailReloaded = await 상세.조회Async(cancellationToken);
        처리.대상준비(상세.항목);
        await 목록.조회Async(목록.응답.Page, cancellationToken);
        return detailReloaded
               && 상세.항목 is { } reloaded
               && string.Equals(reloaded.TaskKey, taskKey, StringComparison.Ordinal)
               && string.Equals(reloaded.Status, expectedStatus, StringComparison.Ordinal);
    }
}

internal static class 피킹포장작업상태코드
{
    public const string 진행중 = "진행중";
    public const string 완료 = "완료";
}
