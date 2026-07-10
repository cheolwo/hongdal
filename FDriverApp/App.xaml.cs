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
            return new Window(new AppShell()) { Title = "홍달 F 드라이버" };
        }
    }
}
