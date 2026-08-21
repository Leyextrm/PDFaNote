using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PDFaNoter
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "About PDFaNote",
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock { Text = "PDFaNote", FontSize = 20 },
                        new TextBlock { Text = "License: AGPL-3.0-or-later" },
                        new HyperlinkButton { Content = "Source code on GitHub", NavigateUri = new Uri("https://github.com/Leyextrm/PDFaNote") },
                        new HyperlinkButton { Content = "Third-Party Notices", NavigateUri = new Uri("https://github.com/Leyextrm/PDFaNote/blob/main/THIRD_PARTY_NOTICES.md") },
                        new TextBlock { Text = "This software uses iText 9.7.0 (AGPL-3.0), PdfPig, and other open-source libraries. For full details, please refer to the Third-Party Notices.", TextWrapping = TextWrapping.Wrap }
                    }
                },
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            picker.FileTypeFilter.Add(".pdf");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var docView = new PdfDocumentView();
                await docView.LoadPdfAsync(file);

                var newTab = new TabViewItem
                {
                    Header = file.Name,
                    Content = docView
                };
                MainTabView.TabItems.Add(newTab);
                MainTabView.SelectedItem = newTab;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                if (docView.SourceFile != null)
                {
                    await docView.SaveAsync(docView.SourceFile);
                }
            }
        }

        private void MainTabView_AddTabButtonClick(TabView sender, object args)
        {
            BtnOpen_Click(this, null);
        }

        private async void MainTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Tab.Content is PdfDocumentView docView && docView.History.HasUnsavedChanges)
            {
                var dialog = new ContentDialog
                {
                    Title = "Unsaved Changes",
                    Content = "Do you want to save your changes to " + args.Tab.Header + "?",
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Don't Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await docView.SaveAsync(docView.SourceFile);
                    sender.TabItems.Remove(args.Tab);
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    sender.TabItems.Remove(args.Tab);
                }
            }
            else
            {
                sender.TabItems.Remove(args.Tab);
            }
        }

        public bool HasUnsavedChanges()
        {
            foreach (TabViewItem tab in MainTabView.TabItems)
            {
                if (tab.Content is PdfDocumentView docView && docView.History.HasUnsavedChanges)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task PromptSaveAndCloseAsync()
        {
            foreach (TabViewItem tab in new List<object>(MainTabView.TabItems))
            {
                if (tab.Content is PdfDocumentView docView && docView.History.HasUnsavedChanges)
                {
                    MainTabView.SelectedItem = tab;
                    var dialog = new ContentDialog
                    {
                        Title = "Unsaved Changes",
                        Content = "Do you want to save your changes to " + tab.Header + "?",
                        PrimaryButtonText = "Save",
                        SecondaryButtonText = "Don't Save",
                        CloseButtonText = "Cancel",
                        XamlRoot = this.XamlRoot
                    };
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        await docView.SaveAsync(docView.SourceFile);
                    }
                    else if (result == ContentDialogResult.None)
                    {
                        return; // Cancelled
                    }
                }
            }
            App.MainWindow.Close();
        }
    }
}

