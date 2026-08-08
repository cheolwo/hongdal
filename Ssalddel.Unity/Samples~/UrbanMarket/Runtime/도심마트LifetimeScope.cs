using Ssalddel.Unity.UrbanMarket;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.UrbanMarket
{
    public sealed class 도심마트LifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Simulated도심마트조회UseCase>(Lifetime.Scoped)
                .As<I도심마트조회UseCase>();
            builder.Register<도심마트ScreenModelValidator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<도심마트View>();
            builder.RegisterComponentInHierarchy<도심마트SceneController>();
        }
    }
}
