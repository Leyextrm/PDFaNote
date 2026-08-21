$content = Get-Content 'MainPage.xaml.cs' -Raw
$content = $content -replace 'private void PenColor_Changed\(object sender, SelectionChangedEventArgs e\)[\s\S]*?\}', 
'private void PenColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            ToolState.PenColor = new SolidColorBrush(args.NewColor);
        }'

$content = $content -replace 'private void HighlighterColor_Changed\(object sender, SelectionChangedEventArgs e\)[\s\S]*?\}', 
'private void HighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            var c = args.NewColor;
            ToolState.HighlighterColor = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, c.R, c.G, c.B));
        }'

$content = $content -replace 'ToolState.MouseDrawEnabled = BtnMouseDraw.IsChecked == true;', 
'ToolState.MouseDrawEnabled = BtnMouseDraw.IsChecked == true;
            if (BtnTextSnapping != null)
            {
                // Toggle text snapping state in ToolState if needed
            }'

Set-Content -Path 'MainPage.xaml.cs' -Value $content -Encoding UTF8
