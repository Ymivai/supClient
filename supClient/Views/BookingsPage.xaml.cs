using supClient.ViewModels;

namespace supClient.Views;

public partial class BookingsPage : ContentPage
{
    public BookingsPage(BookingsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BookingsPageViewModel viewModel)
            await viewModel.OnNavigatedTo();
    }
}
