using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public sealed class CommunityVowVersionViewModel : ObservableObject
{
    private string _selectedCode = CommunityVowVersionCatalog.CurrentVersionCode;

    public IReadOnlyList<CommunityVowVersionDefinition> Versions { get; } =
        CommunityVowVersionCatalog.All;

    public string SelectedCode
    {
        get => _selectedCode;
        set
        {
            var normalized = CommunityVowVersionCatalog.Find(value).Code;
            if (SetProperty(ref _selectedCode, normalized))
            {
                OnPropertyChanged(nameof(Selected));
            }
        }
    }

    public CommunityVowVersionDefinition Selected
        => CommunityVowVersionCatalog.Find(SelectedCode);

    public void RestoreFromWorkflowTag(string? workflowTag)
    {
        var version = CommunityVowVersionCatalog.FindByWorkflowTag(workflowTag);
        if (version is not null)
        {
            SelectedCode = version.Code;
        }
    }

    public string BuildTitle()
        => $"[서원][{Selected.DisplayName}] ";

    public string BuildBody()
        => string.Join(
            Environment.NewLine,
            [
                $"{Selected.DisplayName}을 향한 서원",
                string.Empty,
                "이번 버전에서 이루고 싶은 일",
                string.Empty,
                $"- {Selected.Focus}",
                "- 제가 바라는 구체적인 모습을 이어서 적습니다.",
                string.Empty,
                "앞선 기반에서 이어받는 것",
                string.Empty,
                $"- {Selected.InheritedFoundation}",
                string.Empty,
                "함께 알아차리고 싶은 사람·업체",
                string.Empty,
                "- 이 일을 알거나 함께 할 사람",
                "- 필요한 상품·서비스를 제공할 업체",
                string.Empty,
                "아직 정하지 않은 것",
                string.Empty,
                "- 수량·가격·일정·역할은 의견을 들은 뒤 함께 정합니다.",
                string.Empty,
                "운영 경계",
                string.Empty,
                $"- {Selected.OperationalBoundary}",
                "- 주문·계약·결제·배차는 별도 확인 없이 확정하지 않습니다.",
                "- 이 글은 마음과 정보를 모으는 비구속적 서원이며 실행 기능의 활성화나 계약 체결을 뜻하지 않습니다."
            ]);
}
