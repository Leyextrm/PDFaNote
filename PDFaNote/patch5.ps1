$content = Get-Content 'PdfPageView.xaml.cs' -Raw
$content = $content -replace 'private Polyline _currentStroke;', 'private Polyline _currentStroke;
        private CommandHistory _history;'
$content = $content -replace '_pageData = pageData;', '_pageData = pageData;
            _history = history;'
Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
