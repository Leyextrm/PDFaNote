$content = Get-Content 'PdfDocumentView.xaml' -Raw
$content = $content -replace '<Slider Header="Thickness" Minimum="1" Maximum="20" Value="3" Tag="(.*?)" ValueChanged="PenThickness_Changed" />', '<NumberBox Header="Thickness (1-20)" Minimum="1" Maximum="20" Value="3" Tag="$1" ValueChanged="PenThickness_Changed" SpinButtonPlacementMode="Inline" />'
$content = $content -replace '<Slider Header="Thickness" Minimum="5" Maximum="50" Value="15" Tag="(.*?)" ValueChanged="HighlighterThickness_Changed" />', '<NumberBox Header="Thickness (5-50)" Minimum="5" Maximum="50" Value="15" Tag="$1" ValueChanged="HighlighterThickness_Changed" SpinButtonPlacementMode="Inline" />'
Set-Content -Path 'PdfDocumentView.xaml' -Value $content -Encoding UTF8
