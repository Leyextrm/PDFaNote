$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$content = $content.Replace('PdfNumber(stroke.StrokeThickness)', 'PdfNumber(stroke.StrokeThickness * 0.75f)')
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
