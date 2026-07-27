using FDriverApp.Models;
using FDriverApp.PageModels;
using Ssalddel.Contracts.Common.Drivers;

namespace FDriverApp.Pages
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        private string? _entryFocus;

        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is MainPageModel model)
            {
                await model.InitializeAsync();
                model.StartMonitoring();
                model.ApplyEntryFocus(_entryFocus);
                _entryFocus = null;
            }
        }

        protected override void OnDisappearing()
        {
            if (BindingContext is MainPageModel model)
            {
                model.StopMonitoring();
            }

            base.OnDisappearing();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _entryFocus = query.TryGetValue(FDriverWorkspaceNavigator.FocusQueryKey, out var value)
                ? Uri.UnescapeDataString(Convert.ToString(value) ?? string.Empty)
                : null;
        }

        private async void OnMapMarkerSelected(object? sender, DriverMapMarkerItem marker)
        {
            if (BindingContext is MainPageModel model)
            {
                await model.SelectTicketByIdAsync(marker.RequestId);
            }
        }
    }
}
