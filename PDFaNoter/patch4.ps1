$content = Get-Content 'MainPage.xaml' -Raw
$content = $content -replace '<GridView x:Name="PenColorPicker".*?</GridView>', '<ColorPicker x:Name="PenColorPicker" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="False" IsHexInputVisible="False" IsAlphaEnabled="False" />'

$content = $content -replace '<GridView x:Name="HighlighterColorPicker".*?</GridView>', '<ColorPicker x:Name="HighlighterColorPicker" ColorChanged="HighlighterColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="False" IsHexInputVisible="False" IsAlphaEnabled="False" />'

Set-Content -Path 'MainPage.xaml' -Value $content -Encoding UTF8
