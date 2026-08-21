$content = Get-Content 'MainPage.xaml.cs' -Raw

$content = $content -replace '(?s)private async void BtnSave_Click\(object sender, RoutedEventArgs e\)\s*\{\s*// TODO: Save PDF with ink\s*\}', 
'private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                var picker = new Windows.Storage.Pickers.FileSavePicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("PDF Document", new System.Collections.Generic.List<string>() { ".pdf" });
                picker.SuggestedFileName = "Annotated_" + tab.Header;

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    await docView.SaveAsync(file);
                    tab.Header = file.Name;
                }
            }
        }'

Set-Content -Path 'MainPage.xaml.cs' -Value $content -Encoding UTF8
