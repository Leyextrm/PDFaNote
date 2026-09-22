$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$content = $content.Replace('px = cropH - vy; py = cropW - px;', 'px = cropW - vy; py = cropH - vx;')
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
