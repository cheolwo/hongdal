using CommunityToolkit.Mvvm.ComponentModel;

namespace SsalddelRestaurantDesktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string greeting = "환영합니다. 메뉴, 주문, 매출 관리 화면을 여기에 연결할 수 있습니다.";
}
