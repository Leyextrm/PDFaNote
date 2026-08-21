$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$content = $content -replace 'RangeBaseValueChangedEventArgs', 'Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs'
$content = $content -replace 'sender is Slider', 'sender is NumberBox'
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
