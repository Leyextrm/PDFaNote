$content = Get-Content 'MainPage.xaml' -Raw
$content = $content -replace '<ToggleButton x:Name="BtnStraightLine".*?/>', '<ToggleButton x:Name="BtnStraightLine" Content="&#xE108;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="직선 모드" Click="ToggleOption_Click" />
                <ToggleButton x:Name="BtnTextSnapping" Content="&#xE8D2;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="텍스트 스냅" Click="ToggleOption_Click" IsEnabled="False" />'

$content = $content -replace '<Button Content="&#xE14A;"', '<Button Content="&#xE712;"'

$content = $content -replace '<GridView x:Name="PenColorPicker".*?</GridView>', '<ColorPicker x:Name="PenColorPicker" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="False" IsHexInputVisible="False" IsAlphaEnabled="False" />'

$content = $content -replace '<GridView x:Name="HighlighterColorPicker".*?</GridView>', '<ColorPicker x:Name="HighlighterColorPicker" ColorChanged="HighlighterColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="False" IsHexInputVisible="False" IsAlphaEnabled="False" />'

Set-Content -Path 'MainPage.xaml' -Value $content -Encoding UTF8
