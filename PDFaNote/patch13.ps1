$content = Get-Content 'PdfDocumentView.xaml' -Raw

$content = $content -replace '(?s)<ToggleButton x:Name="BtnEraser" Content="&#xE75C;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Eraser \(Double-click for options\)" Click="Eraser_Click">(.*?)</ToggleButton>', 
'<ToggleButton x:Name="BtnEraser" ToolTipService.ToolTip="Eraser (Double-click for options)" Click="Eraser_Click">$1<StackPanel><FontIcon Glyph="&#xE75C;" /><Border Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Transparent"/></StackPanel></ToggleButton>'

Set-Content -Path 'PdfDocumentView.xaml' -Value $content -Encoding UTF8
