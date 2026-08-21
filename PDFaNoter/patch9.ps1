$content = Get-Content 'MainPage.xaml' -Raw

$content = $content -replace '<MenuFlyoutItem Text="Dock: (.*?)" Tag="(.*?)" Click="ToolbarPos_Click" />', '<RadioMenuFlyoutItem Text="Dock: $1" Tag="$2" GroupName="Dock" Click="ToolbarPos_Click" />'
$content = $content -replace '<MenuFlyoutItem Text="Scroll: (.*?)" Tag="(.*?)" Click="ScrollMode_Click" />', '<RadioMenuFlyoutItem Text="Scroll: $1" Tag="$2" GroupName="Scroll" Click="ScrollMode_Click" />'

# Set default checked state for RadioMenuFlyoutItem
$content = $content -replace '<RadioMenuFlyoutItem Text="Dock: Top" Tag="Top" GroupName="Dock" Click="ToolbarPos_Click" />', '<RadioMenuFlyoutItem Text="Dock: Top" Tag="Top" GroupName="Dock" Click="ToolbarPos_Click" IsChecked="True" />'
$content = $content -replace '<RadioMenuFlyoutItem Text="Scroll: Vertical" Tag="Vertical" GroupName="Scroll" Click="ScrollMode_Click" />', '<RadioMenuFlyoutItem Text="Scroll: Vertical" Tag="Vertical" GroupName="Scroll" Click="ScrollMode_Click" IsChecked="True" />'

Set-Content -Path 'MainPage.xaml' -Value $content -Encoding UTF8
