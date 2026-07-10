using FDriverApp.Models;
using FDriverApp.PageModels;
using Hongdal.Contracts.Common.Drivers;

namespace FDriverApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        private void OnMapMarkerSelected(object? sender, DriverMapMarkerItem marker)
        {
            if (BindingContext is MainPageModel model)
            {
                model.SelectTicketById(marker.RequestId);
            }
        }
    }
}
