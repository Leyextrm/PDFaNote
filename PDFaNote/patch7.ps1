$content = Get-Content 'PdfPageView.xaml.cs' -Raw
$content = $content -replace 'public async void LoadPage\(PdfPageData pageData\)', 'public async void LoadPage(PdfPageData pageData, CommandHistory history)'
$content = $content -replace '_pageData = pageData;', '_pageData = pageData;
            _history = history;'
Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
