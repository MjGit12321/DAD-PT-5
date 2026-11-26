using Avalonia.Controls;
using HubLink.ViewModels;

namespace HubLink.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Opened += async (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm && vm.RefreshShipmentsCommand.CanExecute(null))
            {
                await vm.RefreshShipmentsCommand.ExecuteAsync(null);
            }
        };
    }
}
