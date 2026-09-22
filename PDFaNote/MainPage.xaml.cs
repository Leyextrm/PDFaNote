using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

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

        public async System.Threading.Tasks.Task OpenFileAsync(Windows.Storage.StorageFile file)
        {
            try
            {
                var docView = new PdfDocumentView();
                await docView.LoadPdfAsync(file);
                var newTab = new TabViewItem { Header = file.Name, Content = docView };
                MainTabView.TabItems.Add(newTab);
                MainTabView.SelectedItem = newTab;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await new ContentDialog
                {
                    Title = "PDF open failed",
                    Content = new ScrollViewer { Content = new TextBlock { Text = ex.ToString(), TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true }, MaxHeight = 400 },
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
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
                await OpenFileAsync(file);
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                if (docView.SourceFile != null)
                {
                    try
                    {
                        await docView.SaveAsync(docView.SourceFile);
                        ShowSaveNotification();
                    }
                    catch (Exception ex)
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "저장 실패",
                            Content = $"파일을 저장하는 중 오류가 발생했습니다:\n{ex.Message}",
                            CloseButtonText = "확인",
                            XamlRoot = this.XamlRoot
                        };
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    BtnSaveAs_Click(sender, e);
                }
            }
        }

        private async void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                var picker = new Windows.Storage.Pickers.FileSavePicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("PDF Document", new System.Collections.Generic.List<string>() { ".pdf" });
                picker.SuggestedFileName = docView.SourceFile?.Name ?? "New Note.pdf";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    try
                    {
                        await docView.SaveAsync(file, updateSourceFile: true);
                        tab.Header = file.Name;
                        ShowSaveNotification();
                    }
                    catch (Exception ex)
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "저장 실패",
                            Content = $"파일을 다른 이름으로 저장하는 중 오류가 발생했습니다:\n{ex.Message}",
                            CloseButtonText = "확인",
                            XamlRoot = this.XamlRoot
                        };
                        await dialog.ShowAsync();
                    }
                }
            }
        }

        private async void ShowSaveNotification()
        {
            SaveNotification.IsOpen = true;
            await Task.Delay(3000);
            SaveNotification.IsOpen = false;
        }

        private void MainTabView_AddTabButtonClick(TabView sender, object args)
        {
            BtnOpen_Click(this, null);
        }

        private async void MainTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            await CloseTabAsync(args.Tab);
        }

        private async Task CloseTabAsync(TabViewItem tab)
        {
            if (tab.Content is PdfDocumentView docView && docView.History.HasUnsavedChanges)
            {
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
                    try
                    {
                        if (docView.SourceFile != null)
                        {
                            await docView.SaveAsync(docView.SourceFile);
                        }
                        MainTabView.TabItems.Remove(tab);
                    }
                    catch (Exception ex)
                    {
                        var errDialog = new ContentDialog
                        {
                            Title = "저장 실패",
                            Content = $"저장 중 오류가 발생하여 탭을 닫지 않았습니다:\n{ex.Message}",
                            CloseButtonText = "확인",
                            XamlRoot = this.XamlRoot
                        };
                        await errDialog.ShowAsync();
                    }
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    MainTabView.TabItems.Remove(tab);
                }
            }
            else
            {
                MainTabView.TabItems.Remove(tab);
            }
        }

        private void Undo_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(this.XamlRoot);
            if (focused is TextBox or RichEditBox or PasswordBox)
            {
                return;
            }

            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                docView.History.Undo();
                args.Handled = true;
            }
        }

        private void Redo_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(this.XamlRoot);
            if (focused is TextBox or RichEditBox or PasswordBox)
            {
                return;
            }

            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                docView.History.Redo();
                args.Handled = true;
            }
        }

        private async void CloseTab_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            if (MainTabView.SelectedItem is TabViewItem tab)
            {
                args.Handled = true;
                await CloseTabAsync(tab);
            }
        }

        private void Open_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            BtnOpen_Click(this, null);
        }

        private void Save_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            BtnSave_Click(this, null);
        }

        private void SaveAs_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            BtnSaveAs_Click(this, null);
        }

        private void NextTab_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            if (MainTabView.TabItems.Count > 1)
            {
                args.Handled = true;
                MainTabView.SelectedIndex = (MainTabView.SelectedIndex + 1) % MainTabView.TabItems.Count;
            }
        }

        private void PrevTab_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            if (MainTabView.TabItems.Count > 1)
            {
                args.Handled = true;
                MainTabView.SelectedIndex = (MainTabView.SelectedIndex - 1 + MainTabView.TabItems.Count) % MainTabView.TabItems.Count;
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

