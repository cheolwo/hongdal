using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 로그인 주문자가 자신의 개별 원함을 조회·수량 변경·철회하고,
/// 그 원함에서 파생된 본인 참여 집단만 살펴보도록 조율합니다.
/// </summary>
public sealed class GroupPurchaseMyWishesViewModel(
    I공동구매내원함Client wishesClient,
    I비구속공동구매수요Service demandService,
    ISsalddel현재사용자Context currentUserContext)
{
    private string? _loadedOwnerUserId;
    private readonly Dictionary<string, string> _updateOperationKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _withdrawOperationKeys = new(StringComparer.Ordinal);

    public 공동구매내원함목록응답? Result { get; private set; }
    public IReadOnlyList<공동구매내원함응답> Wishes => Result?.원함목록 ?? [];
    public bool IsLoading { get; private set; }
    public bool IsOperating { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? Notice { get; private set; }

    public IReadOnlyList<GroupPurchaseOwnedGroupSummary> Groups
        => Wishes
            .Where(item => !string.IsNullOrWhiteSpace(item.자동집단Id))
            .GroupBy(item => item.자동집단Id, StringComparer.Ordinal)
            .Select(group =>
            {
                var items = group.OrderByDescending(item => item.수정시각Utc).ToArray();
                var primary = items.First();
                return new GroupPurchaseOwnedGroupSummary(
                    group.Key,
                    primary.자동집단요약,
                    items,
                    items.Select(item => item.공동수입원장Id)
                        .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? string.Empty);
            })
            .OrderBy(group => group.AllWishesClosed)
            .ThenByDescending(group => group.LastUpdatedUtc)
            .ToArray();

    public async Task<bool> LoadAsync(
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        var currentUser = currentUserContext.현재사용자;
        if (!currentUser.인증됨)
        {
            Reset();
            ErrorMessage = "내 원함을 보려면 먼저 로그인해 주세요.";
            return false;
        }

        if (!string.Equals(_loadedOwnerUserId, currentUser.UserId, StringComparison.Ordinal))
        {
            Reset();
            _loadedOwnerUserId = currentUser.UserId;
        }

        if (!force && Result is not null)
        {
            return true;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            Result = await wishesClient.내원함목록조회Async(cancellationToken)
                     ?? new 공동구매내원함목록응답();
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"내 원함을 불러오지 못했습니다. {exception.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public 공동구매내원함응답? FindWish(string? wishLedgerId)
        => string.IsNullOrWhiteSpace(wishLedgerId)
            ? null
            : Wishes.FirstOrDefault(item => string.Equals(
                item.개별원함원장Id,
                wishLedgerId,
                StringComparison.Ordinal));

    public GroupPurchaseOwnedGroupSummary? FindGroup(string? autoGroupId)
        => string.IsNullOrWhiteSpace(autoGroupId)
            ? null
            : Groups.FirstOrDefault(item => string.Equals(
                item.AutoGroupId,
                autoGroupId,
                StringComparison.Ordinal));

    public async Task<bool> WithdrawAsync(
        공동구매내원함응답 wish,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(wish);
        if (IsOperating)
        {
            return false;
        }

        if (!IsActive(wish))
        {
            return Fail("이미 닫힌 원함은 다시 철회할 수 없습니다.");
        }

        IsOperating = true;
        ErrorMessage = null;
        Notice = null;
        try
        {
            var response = await demandService.비구속수요철회Async(
                wish.수요출처키,
                OperationKey(_withdrawOperationKeys, wish.개별원함원장Id, "wish-withdraw"),
                wish.Revision,
                "주문자 앱 내 원함에서 철회",
                cancellationToken);
            if (response is null || !response.철회완료)
            {
                return Fail(response?.안내 ?? "원함 철회 결과를 확인하지 못했습니다.");
            }

            await LoadAsync(force: true, cancellationToken);
            _withdrawOperationKeys.Remove(wish.개별원함원장Id);
            Notice = $"{wish.상품명} 원함을 철회했습니다. 결제나 주문 취소가 아니라 비구속 원함만 닫았습니다.";
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            return Fail(exception.Message);
        }
        finally
        {
            IsOperating = false;
        }
    }

    public async Task<bool> UpdateQuantityAsync(
        공동구매내원함응답 wish,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(wish);
        if (IsOperating)
        {
            return false;
        }

        if (!IsActive(wish))
        {
            return Fail("닫힌 원함은 수량을 변경할 수 없습니다.");
        }

        if (quantity <= 0)
        {
            return Fail("희망 수량은 0보다 커야 합니다.");
        }

        var user = currentUserContext.현재사용자;
        if (!user.인증됨)
        {
            return Fail("원함을 변경하려면 다시 로그인해 주세요.");
        }

        IsOperating = true;
        ErrorMessage = null;
        Notice = null;
        try
        {
            var updateOperationIdentity = $"{wish.개별원함원장Id}|{quantity.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)}";
            var command = BuildUpdateCommand(
                wish,
                quantity,
                user,
                OperationKey(_updateOperationKeys, updateOperationIdentity, "wish-update"));
            var preview = await demandService.수요배치미리보기Async(command, cancellationToken);
            var saved = await demandService.비구속수요저장Async(command, cancellationToken);
            if (preview is null || saved is null)
            {
                return Fail("수량 변경 결과를 확인하지 못했습니다.");
            }

            await LoadAsync(force: true, cancellationToken);
            _updateOperationKeys.Remove(updateOperationIdentity);
            Notice = $"{wish.상품명} 희망 수량을 {quantity:N2}{wish.수량단위}으로 변경했습니다.";
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            return Fail(exception.Message);
        }
        finally
        {
            IsOperating = false;
        }
    }

    public static bool IsActive(공동구매내원함응답 wish)
        => string.Equals(
            wish.원함상태,
            공동구매내원함상태코드.활성,
            StringComparison.Ordinal);

    private static 공동구매자동수요등록Command BuildUpdateCommand(
        공동구매내원함응답 wish,
        decimal quantity,
        현재사용자Snapshot user,
        string idempotencyKey)
        => new()
        {
            요청멱등키 = idempotencyKey,
            수요출처키 = wish.수요출처키,
            개별원함기대Revision = wish.Revision,
            상품키 = wish.상품키,
            상품명 = wish.상품명,
            HS코드 = wish.HS코드,
            온도코드 = wish.온도코드,
            물류방식 = 공동구매자동수요물류방식코드.후속검토,
            거래유형 = wish.거래유형,
            가격표시기준 = wish.가격표시기준,
            구매조직참조키 = wish.구매조직참조키,
            구매조직표시명 = wish.구매조직표시명,
            세금계산서필요 = wish.세금계산서필요,
            주문자키 = user.UserId!,
            주문자표시명 = user.UserName ?? "공동구매 참여자",
            배송권키 = wish.배송권키,
            배송권명 = wish.배송권명,
            희망수량 = quantity,
            수량단위 = wish.수량단위,
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = "주문자 앱 내 원함에서 변경한 비구속 희망 수량",
            목표참여자수 = wish.목표참여자수,
            목표수량 = wish.목표수량
        };

    private bool Fail(string message)
    {
        ErrorMessage = message;
        Notice = null;
        return false;
    }

    private void Reset()
    {
        Result = null;
        _loadedOwnerUserId = null;
        _updateOperationKeys.Clear();
        _withdrawOperationKeys.Clear();
        ErrorMessage = null;
        Notice = null;
    }

    private static string OperationKey(
        IDictionary<string, string> keys,
        string ledgerId,
        string prefix)
    {
        if (keys.TryGetValue(ledgerId, out var key))
        {
            return key;
        }

        key = $"{prefix}:{Guid.NewGuid():N}";
        keys[ledgerId] = key;
        return key;
    }
}

public sealed record GroupPurchaseOwnedGroupSummary(
    string AutoGroupId,
    공동구매자동집단요약응답? Summary,
    IReadOnlyList<공동구매내원함응답> Wishes,
    string GroupImportLedgerId)
{
    public 공동구매내원함응답 PrimaryWish => Wishes[0];
    public bool AllWishesClosed => Wishes.All(wish => !GroupPurchaseMyWishesViewModel.IsActive(wish));
    public DateTime LastUpdatedUtc => Wishes.Max(wish => wish.수정시각Utc);
}
