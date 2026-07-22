using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 주문자가 재료 카드에서 한 번의 명시적 클릭으로 서버 배치 미리보기와 비구속 수요 저장을 이어서 수행합니다.
/// 결제·주문·계약·수입·운송·창고 작업은 이 경계에서 실행하지 않습니다.
/// </summary>
public sealed class OrdererIngredientCardAutoGroupingViewModel(
    I비구속공동구매수요Service demandService,
    ISsalddel현재사용자Context currentUserContext)
{
    private const int TargetParticipantCount = 5;
    private readonly Dictionary<string, OrdererIngredientCardGroupingState> _states =
        new(StringComparer.Ordinal);
    private string? _stateOwnerUserId;

    public 현재사용자Snapshot CurrentUser => currentUserContext.현재사용자;

    public 주문자집단배송권Snapshot? DeliveryScope => CurrentUser.주문자집단배송권;

    public bool CanAutoGroup
        => CurrentUser.인증됨
           && DeliveryScope is { ScopeKey.Length: > 0 };

    public string ScopeDisplayName
        => DeliveryScope?.DisplayName ?? "배송권 미설정";

    public OrdererIngredientCardGroupingState StateFor(string productId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        SynchronizeOwner();
        if (_states.TryGetValue(productId, out var state))
        {
            return state;
        }

        state = new OrdererIngredientCardGroupingState();
        _states.Add(productId, state);
        return state;
    }

    public decimal SuggestedQuantityFor(HS먹거리공동구매상품카드 product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return product.온도코드 == 공동구매온도코드.냉동 ? 20m : 5m;
    }

    public async Task<bool> JoinAsync(
        HS먹거리공동구매상품카드 product,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        var state = StateFor(product.상품카드Id);
        if (state.IsBusy)
        {
            return false;
        }

        var user = CurrentUser;
        if (!user.인증됨)
        {
            return Fail(state, "재료 집단화 참여는 로그인 후 사용할 수 있습니다.");
        }

        var scope = user.주문자집단배송권;
        if (scope is null || string.IsNullOrWhiteSpace(scope.ScopeKey))
        {
            return Fail(state, "가입 또는 온보딩에서 주문자 배송권을 한 번 설정해 주세요. 이후에는 카드 클릭만으로 집단화됩니다.");
        }

        if (state.HasActiveDemand)
        {
            state.Notice = "이 재료는 이미 비구속 집단화에 참여 중입니다.";
            state.ErrorMessage = null;
            return true;
        }

        state.IsBusy = true;
        state.ErrorMessage = null;
        state.Notice = null;
        try
        {
            var command = BuildDemand(product, user, scope, state);
            state.PlacementPreview = await demandService.수요배치미리보기Async(command, cancellationToken)
                ?? throw new InvalidOperationException("자동집단 배치 결과를 확인하지 못했습니다.");
            state.RegisteredGroup = await demandService.비구속수요저장Async(command, cancellationToken)
                ?? throw new InvalidOperationException("비구속 수요를 저장하지 못했습니다.");
            state.DemandSourceKey = command.수요출처키;
            state.WithdrawOperationNonce = Guid.NewGuid().ToString("N");

            var placement = state.PlacementPreview.배치유형 == 공동구매자동집단배치유형코드.기존집단
                ? "기존 집단에 합류했습니다"
                : "새 집단을 시작했습니다";
            state.Notice = $"{placement}. 현재 {state.RegisteredGroup.참여자수:N0}명, "
                           + $"{state.RegisteredGroup.총희망수량:N0}{state.RegisteredGroup.수량단위}의 비구속 수요가 모였습니다.";
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            state.ErrorMessage = exception.Message;
            return false;
        }
        finally
        {
            state.IsBusy = false;
        }
    }

    public async Task<bool> WithdrawAsync(
        HS먹거리공동구매상품카드 product,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        var state = StateFor(product.상품카드Id);
        if (state.IsBusy)
        {
            return false;
        }

        if (!state.HasActiveDemand)
        {
            return Fail(state, "이 카드에서 철회할 비구속 수요가 없습니다.");
        }

        state.IsBusy = true;
        state.ErrorMessage = null;
        state.Notice = null;
        try
        {
            var idempotencyKey = OperationKey(
                "demand-withdraw",
                state.WithdrawOperationNonce,
                state.DemandSourceKey);
            var response = await demandService.비구속수요철회Async(
                    state.DemandSourceKey,
                    idempotencyKey,
                    "주문자 앱 재료 카드에서 철회",
                    cancellationToken)
                ?? throw new InvalidOperationException("비구속 수요 철회 응답을 확인하지 못했습니다.");
            if (!response.철회완료)
            {
                throw new InvalidOperationException(response.안내);
            }

            state.DemandSourceKey = string.Empty;
            state.RegisteredGroup = null;
            state.PlacementPreview = null;
            state.SaveOperationNonce = Guid.NewGuid().ToString("N");
            state.WithdrawOperationNonce = string.Empty;
            state.Notice = "비구속 집단화 참여를 철회했습니다. 결제나 주문은 발생하지 않았습니다.";
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            state.ErrorMessage = exception.Message;
            return false;
        }
        finally
        {
            state.IsBusy = false;
        }
    }

    public void ClearUserState()
    {
        _states.Clear();
        _stateOwnerUserId = CurrentUser.UserId;
    }

    private 공동구매자동수요등록Command BuildDemand(
        HS먹거리공동구매상품카드 product,
        현재사용자Snapshot user,
        주문자집단배송권Snapshot scope,
        OrdererIngredientCardGroupingState state)
    {
        var demandSourceKey = DemandSourceKey(
            user.UserId!,
            product.상품카드Id,
            scope.ScopeKey,
            product.온도코드,
            product.예상물류방식);

        return new 공동구매자동수요등록Command
        {
            요청멱등키 = OperationKey("demand-save", state.SaveOperationNonce, demandSourceKey),
            수요출처키 = demandSourceKey,
            상품키 = product.상품카드Id,
            상품명 = product.상품명,
            HS코드 = product.HS코드,
            온도코드 = product.온도코드,
            물류방식 = product.예상물류방식,
            주문자키 = user.UserId!,
            주문자표시명 = user.UserName ?? "공동구매 참여자",
            배송권키 = scope.ScopeKey,
            배송권명 = scope.DisplayName,
            희망수량 = SuggestedQuantityFor(product),
            수량단위 = "kg",
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = "주문자 앱 재료 카드에서 명시적으로 클릭해 저장한 비구속 집단화 수요",
            목표참여자수 = TargetParticipantCount,
            목표수량 = product.SuggestedTargetQuantityKg
        };
    }

    private void SynchronizeOwner()
    {
        var currentUserId = CurrentUser.UserId;
        if (string.Equals(_stateOwnerUserId, currentUserId, StringComparison.Ordinal))
        {
            return;
        }

        _states.Clear();
        _stateOwnerUserId = currentUserId;
    }

    private static bool Fail(OrdererIngredientCardGroupingState state, string message)
    {
        state.ErrorMessage = message;
        state.Notice = null;
        return false;
    }

    private static string DemandSourceKey(
        string userId,
        string productId,
        string deliveryScopeKey,
        string temperatureCode,
        string logisticsMode)
        => $"orderer-card:{Hash(string.Join('|', userId, productId, deliveryScopeKey, temperatureCode, logisticsMode))[..32]}";

    private static string OperationKey(string prefix, string nonce, string material)
        => $"{prefix}:{Hash(string.Join('|', nonce, material))[..32]}";

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

public sealed class OrdererIngredientCardGroupingState
{
    public bool IsBusy { get; internal set; }
    public string? ErrorMessage { get; internal set; }
    public string? Notice { get; internal set; }
    public 공동구매자동집단배치미리보기응답? PlacementPreview { get; internal set; }
    public 공동구매자동집단사용자응답? RegisteredGroup { get; internal set; }
    public string DemandSourceKey { get; internal set; } = string.Empty;
    internal string SaveOperationNonce { get; set; } = Guid.NewGuid().ToString("N");
    internal string WithdrawOperationNonce { get; set; } = string.Empty;

    public bool HasActiveDemand
        => !string.IsNullOrWhiteSpace(DemandSourceKey)
           && RegisteredGroup is not null;
}
