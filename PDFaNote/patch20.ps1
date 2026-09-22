$content = Get-Content 'MainPage.xaml.cs' -Raw

$content = $content -replace '(?s)private async void BtnSave_Click\(object sender, RoutedEventArgs e\).*?\}\s*\}', 
'private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabView.SelectedItem is TabViewItem tab && tab.Content is PdfDocumentView docView)
            {
                if (docView.SourceFile != null)
                {
                    await docView.SaveAsync(docView.SourceFile);
                    // Provide some visual feedback (e.g. change title briefly) could be done here
                }
            }
        }'

$content = $content -replace '(?s)private async void MainTabView_TabCloseRequested\(TabView sender, TabViewTabCloseRequestedEventArgs args\)\s*\{.*?(?=\s*\})', 
'private async void MainTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
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
            foreach (TabViewItem tab in new System.Collections.Generic.List<object>(MainTabView.TabItems))
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
        }'

Set-Content -Path 'MainPage.xaml.cs' -Value $content -Encoding UTF8
