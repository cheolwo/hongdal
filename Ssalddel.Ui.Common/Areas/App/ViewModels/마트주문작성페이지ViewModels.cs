using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>한 공개 상품의 수량·안내 확인과 멱등 제출만 관리합니다.</summary>
public sealed partial class 마트주문작성ViewModel(
    I마트주문요청Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial int 수량 { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial bool 비구속주문요청확인 { get; set; }

    [ObservableProperty]
    public partial Guid 클라이언트요청Id { get; private set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial 마트주문요청응답? 등록응답 { get; private set; }

    public bool 제출가능 => 수량 is >= 1 and <= 100 && 비구속주문요청확인 && !처리중;

    public Task<bool> 등록Async(long productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return Task.FromResult(유효성실패("주문 요청할 공개 상품을 확인해 주세요."));
        }

        if (수량 is < 1 or > 100)
        {
            return Task.FromResult(유효성실패("주문 요청 수량은 1개 이상 100개 이하로 입력해 주세요."));
        }

        if (!비구속주문요청확인)
        {
            return Task.FromResult(유효성실패("비구속 주문 요청 안내를 확인해 주세요."));
        }

        return 작업실행Async(
            async token =>
            {
                등록응답 = await service.등록Async(new 마트주문요청등록요청
                {
                    클라이언트요청Id = 클라이언트요청Id,
                    공개상품Id = productId,
                    수량 = 수량,
                    비구속주문요청확인 = true,
                    안내버전 = 마트주문요청안내.현재버전
                }, token);
            },
            "마트 주문 요청을 저장했습니다.",
            cancellationToken,
            ex => $"마트 주문 요청을 저장하지 못했습니다. {ex.Message}");
    }

    public void 새요청준비()
    {
        수량 = 1;
        비구속주문요청확인 = false;
        클라이언트요청Id = Guid.NewGuid();
        등록응답 = null;
        작업상태초기화();
    }
}

/// <summary>주소나 등록 응답에서 받은 정확한 주문 요청 ID 한 건만 다시 조회합니다.</summary>
public sealed partial class 마트주문요청상세ViewModel(
    I마트주문요청Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial Guid? 요청Id { get; private set; }

    [ObservableProperty]
    public partial 마트주문요청응답? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(Guid requestId, CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
        {
            return Task.FromResult(유효성실패("조회할 마트 주문 요청 ID를 확인해 주세요."));
        }

        요청Id = requestId;
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await service.상세Async(requestId, token);
                찾을수없음 = 상세 is null;
            },
            "마트 주문 요청을 다시 조회했습니다.",
            cancellationToken,
            ex => $"마트 주문 요청을 다시 조회하지 못했습니다. {ex.Message}");
    }

    public void 선택해제()
    {
        요청Id = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }
}

/// <summary>기능 접근, 주문자 인증, 공개 상품, 작성과 저장 후 재조회를 조립합니다.</summary>
public sealed class 마트주문작성PageViewModel : 조립ViewModelBase
{
    public 마트주문작성PageViewModel(
        마트페이지접근ViewModel access,
        주문자앱인증ViewModel authentication,
        마트공개상품상세ViewModel product,
        마트주문작성ViewModel writer,
        마트주문요청상세ViewModel requestDetail)
    {
        접근 = 하위ViewModel등록(access);
        인증 = 하위ViewModel등록(authentication);
        상품 = 하위ViewModel등록(product);
        작성 = 하위ViewModel등록(writer);
        요청상세 = 하위ViewModel등록(requestDetail);
    }

    public 마트페이지접근ViewModel 접근 { get; }
    public 주문자앱인증ViewModel 인증 { get; }
    public 마트공개상품상세ViewModel 상품 { get; }
    public 마트주문작성ViewModel 작성 { get; }
    public 마트주문요청상세ViewModel 요청상세 { get; }
}
