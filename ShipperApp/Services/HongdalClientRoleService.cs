namespace ShipperApp.Services;

public enum HongdalClientRole
{
    Shipper,
    WarehouseManager
}

public sealed class HongdalClientRoleService
{
    private const string ActiveRoleKey = "hongdal.client.active_role";

    public HongdalClientRoleService()
    {
        var savedRole = Preferences.Default.Get(ActiveRoleKey, nameof(HongdalClientRole.Shipper));
        CurrentRole = Enum.TryParse<HongdalClientRole>(savedRole, out var parsedRole)
            ? parsedRole
            : HongdalClientRole.Shipper;
    }

    public event Action? Changed;

    public HongdalClientRole CurrentRole { get; private set; }

    public bool IsShipper => CurrentRole == HongdalClientRole.Shipper;

    public bool IsWarehouseManager => CurrentRole == HongdalClientRole.WarehouseManager;

    public string RoleLabel => IsWarehouseManager ? "창고 관리자" : "화주";

    public string RoleDescription => IsWarehouseManager
        ? "입고부터 판매와 배송까지 이어지는 창고 운영"
        : "운송 의뢰와 판매·물류 흐름을 연결하는 화주 운영";

    public void SetRole(HongdalClientRole role)
    {
        if (CurrentRole == role)
        {
            return;
        }

        CurrentRole = role;
        Preferences.Default.Set(ActiveRoleKey, role.ToString());
        Changed?.Invoke();
    }
}
