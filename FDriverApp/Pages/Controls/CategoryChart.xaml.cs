using FDriverApp.Models;

namespace FDriverApp.Pages.Controls
{
    public partial class CategoryChart
    {
        public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
            nameof(IsBusy),
            typeof(bool),
            typeof(CategoryChart),
            false);

        public static readonly BindableProperty TodoCategoryDataProperty = BindableProperty.Create(
            nameof(TodoCategoryData),
            typeof(IList<CategoryChartData>),
            typeof(CategoryChart),
            defaultValueCreator: _ => new List<CategoryChartData>());

        public static readonly BindableProperty TodoCategoryColorsProperty = BindableProperty.Create(
            nameof(TodoCategoryColors),
            typeof(IList<Brush>),
            typeof(CategoryChart),
            defaultValueCreator: _ => new List<Brush>());

        public CategoryChart()
        {
            InitializeComponent();
        }

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public IList<CategoryChartData> TodoCategoryData
        {
            get => (IList<CategoryChartData>)GetValue(TodoCategoryDataProperty);
            set => SetValue(TodoCategoryDataProperty, value);
        }

        public IList<Brush> TodoCategoryColors
        {
            get => (IList<Brush>)GetValue(TodoCategoryColorsProperty);
            set => SetValue(TodoCategoryColorsProperty, value);
        }
    }
}
