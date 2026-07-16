using Hongdal.Contracts.Common.Warehouse;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public sealed class 창고목록조회ViewModel(창고기준정보ViewModel 원본)
    : 위임업무조각ViewModelBase<창고기준정보ViewModel>(
        원본,
        "warehouse-list",
        "창고 목록 조회",
        업무조각유형.목록조회), I목록조회ViewModel<창고요약응답>
{
    public IReadOnlyList<창고요약응답> 항목목록 => 원본.창고목록;
    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 원본.목록조회Async(cancellationToken);

    public bool 선택(long warehouseId) => 원본.창고선택(warehouseId);
}

public sealed class 창고등록ViewModel(창고기준정보ViewModel 원본)
    : 위임업무조각ViewModelBase<창고기준정보ViewModel>(
        원본,
        "warehouse-create",
        "창고 등록",
        업무조각유형.등록), I명령ViewModel<창고저장요청>
{
    public 창고저장요청 초안 => 원본.창고초안;
    public Task<bool> 실행Async(CancellationToken cancellationToken = default)
        => 원본.창고생성Async(cancellationToken);

    public void 입력변경알림() => 원본.입력변경알림();
}

public sealed class 창고사용자조회ViewModel(창고기준정보ViewModel 원본)
    : 위임업무조각ViewModelBase<창고기준정보ViewModel>(
        원본,
        "warehouse-user-list",
        "창고 담당자 조회",
        업무조각유형.목록조회), I목록조회ViewModel<창고사용자항목응답>
{
    public IReadOnlyList<창고사용자항목응답> 항목목록 => 원본.사용자목록;
    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 원본.사용자목록조회Async(cancellationToken);
}

public sealed class 창고사용자등록ViewModel(창고기준정보ViewModel 원본)
    : 위임업무조각ViewModelBase<창고기준정보ViewModel>(
        원본,
        "warehouse-user-create",
        "창고 담당자 등록",
        업무조각유형.등록), I명령ViewModel<창고사용자저장요청>
{
    public 창고사용자저장요청 초안 => 원본.사용자초안;
    public Task<bool> 실행Async(CancellationToken cancellationToken = default)
        => 원본.사용자추가Async(cancellationToken);

    public void 입력변경알림() => 원본.입력변경알림();
}
