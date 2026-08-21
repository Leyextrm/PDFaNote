$content = Get-Content 'MainPage.xaml.cs' -Raw

$content = $content -replace 'UpdateToolSelection\(\);', 'UpdateToolSelection();
            
            for (int i=0; i<5; i++) _penButtons[i].Foreground = ToolState.PenColors[i];
            for (int i=0; i<3; i++) _highlighterButtons[i].Foreground = ToolState.HighlighterColors[i];'

$content = $content -replace '(?s)private void Pen_Click\(object sender, RoutedEventArgs e\).*?UpdateToolSelection\(\);\s*\}', 
'private void Pen_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            int index = int.Parse(btn.Tag.ToString());
            
            if (ToolState.CurrentMode == ToolMode.Pen && ToolState.CurrentPenSlot == index)
            {
                btn.ContextFlyout?.ShowAt(btn);
            }
            else
            {
                UncheckAllExcept(btn);
                btn.IsChecked = true;
                ToolState.CurrentMode = ToolMode.Pen;
                ToolState.CurrentPenSlot = index;
                UpdateToolSelection();
            }
        }'

$content = $content -replace '(?s)private void Highlighter_Click\(object sender, RoutedEventArgs e\).*?UpdateToolSelection\(\);\s*\}', 
'private void Highlighter_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            int index = int.Parse(btn.Tag.ToString());
            
            if (ToolState.CurrentMode == ToolMode.Highlighter && ToolState.CurrentHighlighterSlot == index)
            {
                btn.ContextFlyout?.ShowAt(btn);
            }
            else
            {
                UncheckAllExcept(btn);
                btn.IsChecked = true;
                ToolState.CurrentMode = ToolMode.Highlighter;
                ToolState.CurrentHighlighterSlot = index;
                UpdateToolSelection();
            }
        }'

$content = $content -replace '(?s)private void PenColor_Changed.*?\}', 
'private void PenColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (sender.Tag != null)
            {
                int index = int.Parse(sender.Tag.ToString());
                ToolState.PenColors[index] = new SolidColorBrush(args.NewColor);
                if (_penButtons != null && _penButtons.Count > index) _penButtons[index].Foreground = ToolState.PenColors[index];
            }
        }'

$content = $content -replace '(?s)private void HighlighterColor_Changed.*?\}', 
'private void HighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (sender.Tag != null)
            {
                int index = int.Parse(sender.Tag.ToString());
                var c = args.NewColor;
                ToolState.HighlighterColors[index] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, c.R, c.G, c.B));
                if (_highlighterButtons != null && _highlighterButtons.Count > index) _highlighterButtons[index].Foreground = ToolState.HighlighterColors[index];
            }
        }'

Set-Content -Path 'MainPage.xaml.cs' -Value $content -Encoding UTF8
