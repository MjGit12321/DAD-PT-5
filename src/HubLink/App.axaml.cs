using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HubLink.Services;
using HubLink.ViewModels;
using HubLink.Views;

namespace HubLink;

public partial class App : Application
{
    private static readonly HttpClient SharedHttpClient = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = new JsonSettingsService();
            var settings = settingsService.LoadSettings();
            var apiService = new ShipmentApiService(SharedHttpClient);
            var csvExportService = new CsvExportService();

            var mainWindow = new MainWindow();
            var fileDialogService = new AvaloniaFileDialogService(mainWindow);
            var viewModel = new MainWindowViewModel(apiService, csvExportService, settings, fileDialogService);

            mainWindow.DataContext = viewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
// MJ Jade Piquero - 2024300423
// John Carlo Magno - 2024301174
