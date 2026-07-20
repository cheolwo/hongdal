using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>입고 요청 한 건의 서버 조회와 조회 결과 상태만 관리합니다.</summary>
public sealed class 입고상세조회ViewModel(I입출고작업Service service)
    : 업무조각ViewModelBase("inbound-detail-query", "입고 상세 조회", 업무조각유형.상세조회),
        I상세조회ViewModel<입고요청항목응답>
{
    private long? _입고요청Id;
    private 입고요청항목응답? _항목;
    private bool _대상없음;

    public long? 입고요청Id
    {
        get => _입고요청Id;
        private set => SetProperty(ref _입고요청Id, value);
    }

    public 입고요청항목응답? 항목
    {
        get => _항목;
        private set => SetProperty(ref _항목, value);
    }

    public bool 대상없음
    {
        get => _대상없음;
        private set => SetProperty(ref _대상없음, value);
    }

    public void 조회대상설정(long? inboundId)
    {
        var normalized = inboundId is > 0 ? inboundId : null;
        if (입고요청Id == normalized)
        {
            return;
        }

        입고요청Id = normalized;
        조회결과초기화();
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
                var item = await service.입고상세조회Async(inboundId, token);
                대상없음 = item is null;
                항목 = item;
            },
            "입고 요청 상세를 조회했습니다.",
            cancellationToken);
    }

    public void 조회결과초기화()
    {
        항목 = null;
        대상없음 = false;
        작업상태초기화();
    }
}
