$content = Get-Content 'MainPage.xaml.cs' -Raw
$content = $content -replace '(?s)private void HighlighterColor_Changed.*?$', 
'private void HighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (sender.Tag != null)
            {
                int index = int.Parse(sender.Tag.ToString());
                var c = args.NewColor;
                ToolState.HighlighterColors[index] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, c.R, c.G, c.B));
                if (_highlighterButtons != null && _highlighterButtons.Count > index) _highlighterButtons[index].Foreground = ToolState.HighlighterColors[index];
            }
        }
    }
}'

Set-Content -Path 'MainPage.xaml.cs' -Value $content -Encoding UTF8
