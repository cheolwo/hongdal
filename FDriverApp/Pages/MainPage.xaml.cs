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
                await model.StartMonitoringAsync();
                var focus = _entryFocus;
                model.ApplyEntryFocus(focus);
                _entryFocus = null;
                await ScrollToEntryFocusAsync(focus);
            }
        }

        protected override async void OnDisappearing()
        {
            if (BindingContext is MainPageModel model)
            {
                await model.StopMonitoringAsync();
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

        private async void OnProfileClicked(object? sender, EventArgs args)
            => await ScrollToEntryFocusAsync("profile");

        private async void OnCurrentDeliveryClicked(object? sender, EventArgs args)
            => await ScrollToEntryFocusAsync("delivery");

        private Task ScrollToEntryFocusAsync(string? focus)
        {
            if (!WorkspaceScroll.IsVisible)
            {
                return Task.CompletedTask;
            }

            VisualElement? target = focus?.Trim().ToLowerInvariant() switch
            {
                "dispatch" or "bundle" => RecommendationSection,
                "delivery" => ActiveDeliverySection,
                "settlement" or "profile" or "workspace" => WorkspaceSummarySection,
                _ => null
            };

            return target is null
                ? Task.CompletedTask
                : WorkspaceScroll.ScrollToAsync(target, ScrollToPosition.Start, true);
        }
    }
}
