using Microsoft.Extensions.DependencyInjection;

namespace Deliver
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell()) { Title = "홍달 배달기사" };
        }
    }
}
