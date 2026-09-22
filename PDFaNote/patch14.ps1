$content = Get-Content 'PdfDocumentView.xaml' -Raw
$content = $content -replace 'IsColorChannelTextInputVisible="False"', 'IsColorChannelTextInputVisible="True"'
$content = $content -replace 'IsHexInputVisible="False"', 'IsHexInputVisible="True"'
Set-Content -Path 'PdfDocumentView.xaml' -Value $content -Encoding UTF8
