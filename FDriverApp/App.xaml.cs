using Microsoft.Extensions.DependencyInjection;

namespace FDriverApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell()) { Title = "홍달 음식 배달" };
        }
    }
}
