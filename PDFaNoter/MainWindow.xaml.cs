using Microsoft.UI.Xaml;

namespace PDFaNoter
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            AppWindow.SetIcon("Assets/AppIcon.ico");

            RootFrame.Navigate(typeof(MainPage));
            AppWindow.Closing += AppWindow_Closing;
        }

        private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            if (RootFrame.Content is MainPage mainPage && mainPage.HasUnsavedChanges())
            {
                args.Cancel = true; // Prevent immediate close
                await mainPage.PromptSaveAndCloseAsync();
            }
        }
    }
}
