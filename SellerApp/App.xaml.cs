namespace SellerApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new MainPage()) { Title = "살뜰 판매자" };
}
