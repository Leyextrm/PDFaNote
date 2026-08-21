$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$content = $content.Replace('var writer = new iText.Kernel.Pdf.PdfWriter(ms);', 'var writer = new iText.Kernel.Pdf.PdfWriter(ms); writer.SetCloseStream(false);')
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
