using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

namespace PDFaNoter;

public partial class App : Application
{
    public static Window MainWindow { get; private set; }
    
    public App()
    {
        InitializeComponent();
        this.UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception.ToString());
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        LogCrash(e.ExceptionObject?.ToString() ?? "Unknown exception");
    }

    private void LogCrash(string message)
    {
        try
        {
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var file = System.IO.Path.Combine(folder, "crash_log.txt");
            System.IO.File.AppendAllText(file, $"[{System.DateTime.Now}] {message}\n\n");
        }
        catch { }
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        ToolState.Load();
        MainWindow = new MainWindow();
        MainWindow.Activate();

        var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
        if (activatedArgs.Kind == Microsoft.Windows.AppLifecycle.ExtendedActivationKind.File)
        {
            if (activatedArgs.Data is Windows.ApplicationModel.Activation.IFileActivatedEventArgs fileArgs && fileArgs.Files.Count > 0)
            {
                if (fileArgs.Files[0] is Windows.Storage.StorageFile file)
                {
                    var mainWindow = MainWindow as MainWindow;
                    if (mainWindow?.CurrentMainPage != null)
                    {
                        // Fire and forget
                        _ = mainWindow.CurrentMainPage.OpenFileAsync(file);
                    }
                }
            }
        }
    }
}
