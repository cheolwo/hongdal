namespace DriverApp;

public partial class App : Application
{
	private readonly NativeDriverHomePage _homePage;

	public App(NativeDriverHomePage homePage)
	{
		InitializeComponent();
		_homePage = homePage;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new NavigationPage(_homePage)) { Title = "홍달 용달기사" };
	}
}
