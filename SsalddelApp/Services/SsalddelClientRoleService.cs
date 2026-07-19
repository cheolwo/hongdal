namespace SsalddelApp.Services;

public enum SsalddelClientRole
{
    Unspecified,
    Shipper,
    WarehouseManager
}

public sealed class SsalddelClientRoleService
{
    private const string ActiveRoleKey = "ssalddel.client.active_role";

    public SsalddelClientRoleService()
    {
        var savedRole = Preferences.Default.Get(ActiveRoleKey, nameof(SsalddelClientRole.Unspecified));
        CurrentRole = Enum.TryParse<SsalddelClientRole>(savedRole, out var parsedRole)
            ? parsedRole
            : SsalddelClientRole.Unspecified;
    }

    public event Action? Changed;

    public SsalddelClientRole CurrentRole { get; private set; }

    public bool HasRole => CurrentRole != SsalddelClientRole.Unspecified;

    public bool IsUnspecified => CurrentRole == SsalddelClientRole.Unspecified;

    public bool IsShipper => CurrentRole == SsalddelClientRole.Shipper;

    public bool IsWarehouseManager => CurrentRole == SsalddelClientRole.WarehouseManager;

    public string RoleLabel => CurrentRole switch
    {
        SsalddelClientRole.Shipper => "화주",
        SsalddelClientRole.WarehouseManager => "창고 관리자",
        _ => "역할 미설정"
    };

    public string RoleDescription => CurrentRole switch
    {
        SsalddelClientRole.Shipper => "운송 의뢰와 판매·물류 흐름을 연결하는 화주 운영",
        SsalddelClientRole.WarehouseManager => "입고부터 판매와 운송까지 이어지는 창고 운영",
        _ => "정보와 대화에서 시작하는 공통 커뮤니티 홈"
    };

    public void SetRole(SsalddelClientRole role)
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
