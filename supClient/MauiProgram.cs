using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using supClient.Services;
using supClient.Storage;
using supClient.ViewModels;
using supClient.Views;

namespace supClient;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IBookingRepository, JsonBookingRepository>();
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IBookingService, BookingService>();
        services.AddSingleton<IBookingAvailabilityService, BookingAvailabilityService>();
        services.AddSingleton<IDataResetService, DataResetService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<BookingsPageViewModel>();
        services.AddTransient<AddBookingPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();

        services.AddTransient<BookingsPage>();
        services.AddTransient<AddBookingPage>();
        services.AddTransient<SettingsPage>();

        services.AddSingleton<AppShell>();
    }
}
