using System.ComponentModel;
using Ssalddel.Ui.Common.Areas.App.Components;
using Ssalddel.Ui.Common.Areas.App.Components.Crud;
using Ssalddel.Ui.Common.Areas.App.Components.Community;
using Ssalddel.Ui.Common.Areas.App.Components.Sales;
using Microsoft.AspNetCore.Components;

namespace Ssalddel.Tests.Ui.Common;

public sealed class SsalddelCrudComponentContractTests
{
    [Fact]
    public void CrudComponents_AreReusableRazorComponents()
    {
        Type[] componentTypes =
        [
            typeof(SsalddelCrudActionBar),
            typeof(SsalddelOperationState),
            typeof(SsalddelCrudTable<>),
            typeof(SsalddelCrudCardList<>),
            typeof(SsalddelServerDataGrid<>),
            typeof(SsalddelAutocomplete<>),
            typeof(SsalddelLoadingSkeleton),
            typeof(SsalddelCommandDialog<>),
            typeof(SsalddelDeleteDialog<>),
            typeof(SsalddelSalesChannelAccountEditor),
            typeof(SsalddelSalesChannelAccountDialog),
            typeof(SsalddelSalesChannelAccountWorkspace),
            typeof(PlatformCommunityPostComposer),
            typeof(PlatformCommunityPostList)
        ];

        Assert.All(componentTypes, type =>
            Assert.True(typeof(ComponentBase).IsAssignableFrom(type), $"{type.Name} is not a Razor component."));
    }

    [Fact]
    public void ParameterViewModelComponent_TransfersAndReleasesSubscription()
    {
        var first = new TrackingViewModel();
        var second = new TrackingViewModel();
        var component = new TestParameterComponent();

        component.SetViewModel(first);
        Assert.Equal(1, first.SubscriberCount);

        component.SetViewModel(second);
        Assert.Equal(0, first.SubscriberCount);
        Assert.Equal(1, second.SubscriberCount);

        component.Dispose();
        Assert.Equal(0, second.SubscriberCount);
    }

    private sealed class TestParameterComponent : ViewModelParameterComponentBase<TrackingViewModel>
    {
        public void SetViewModel(TrackingViewModel viewModel)
        {
            ViewModel = viewModel;
            OnParametersSet();
        }
    }

    private sealed class TrackingViewModel : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler? _propertyChanged;

        public int SubscriberCount { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                _propertyChanged += value;
                SubscriberCount++;
            }
            remove
            {
                _propertyChanged -= value;
                SubscriberCount--;
            }
        }
    }
}
