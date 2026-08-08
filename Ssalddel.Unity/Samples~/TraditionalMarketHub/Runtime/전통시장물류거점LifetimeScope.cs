using Ssalddel.Unity.TraditionalMarkets;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub
{
    public sealed class 전통시장물류거점LifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Simulated전통시장물류거점조회UseCase>(Lifetime.Scoped)
                .As<I전통시장물류거점조회UseCase>();
            builder.Register<전통시장물류거점ScreenModelValidator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<전통시장물류거점View>();
            builder.RegisterComponentInHierarchy<전통시장물류거점SceneController>();
        }
    }
}
