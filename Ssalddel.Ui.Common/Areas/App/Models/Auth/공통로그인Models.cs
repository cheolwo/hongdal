using Ssalddel.Contracts.Common;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Models.Auth;

public sealed record 공통로그인요청(string UserNameOrEmail, string Password);

public sealed record 공통커뮤니티회원가입요청(
    string UserName,
    string Email,
    string Password,
    bool PrivacyConsentAccepted,
    string PrivacyConsentVersion);

public sealed record 공통소셜로그인요청(string ProviderId, string ProviderDisplayName);

public sealed record 소셜로그인ProviderOption(
    string ProviderId,
    string DisplayName,
    string Icon,
    Color Color,
    string Description)
{
    public static readonly IReadOnlyList<소셜로그인ProviderOption> 기본목록 =
    [
        new(
            소셜로그인ProviderIds.Kakao,
            "카카오로 로그인",
            Icons.Material.Filled.Chat,
            Color.Warning,
            "카카오 계정으로 살뜰 계정에 연결합니다."),
        new(
            소셜로그인ProviderIds.Google,
            "구글로 로그인",
            Icons.Material.Filled.AccountCircle,
            Color.Error,
            "구글 계정으로 살뜰 계정에 연결합니다."),
        new(
            소셜로그인ProviderIds.Naver,
            "네이버로 로그인",
            Icons.Material.Filled.TravelExplore,
            Color.Success,
            "네이버 계정으로 살뜰 계정에 연결합니다.")
    ];
}
