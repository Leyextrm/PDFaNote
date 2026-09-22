$content = Get-Content 'PdfPageView.xaml.cs' -Raw
$content = $content -replace 'public async void LoadPage\(PdfPageData data\)', 'public async void LoadPage(PdfPageData data, CommandHistory history)'
$content = $content -replace '_pageData = data;', '_pageData = data;
            _history = history;'
Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
