namespace RestaurantDeskApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage())
        {
            Title = "살뜰 식당",
        };

#if WINDOWS && DEBUG
        window.Width = 430;
        window.Height = 860;
#endif

        return window;
    }
}
