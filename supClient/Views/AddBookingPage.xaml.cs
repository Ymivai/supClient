using supClient.ViewModels;

namespace supClient.Views;

public partial class AddBookingPage : ContentPage
{
    public AddBookingPage(AddBookingPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
