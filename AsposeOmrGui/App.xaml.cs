using System.Windows;
using System.Windows.Threading;

namespace AsposeOmrGui;

public partial class App : Application
{
    public App()
    {
        // Never let an unexpected error crash the app silently; show it and keep running.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "An unexpected error occurred:\n\n" + e.Exception.Message,
            "Aspose OMR",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
