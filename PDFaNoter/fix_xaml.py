import re
with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

target = r'''                <ToggleButton x:Name="BtnHighlighter2".*?                    <StackPanel><FontIcon Glyph="&#xE7E6;" /><Border x:Name="IndHighlighter2" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Cyan"/></StackPanel>
                </ToggleButton>'''

replacement = r'''                <ToggleButton x:Name="BtnHighlighter2" Tag="2" ToolTipService.ToolTip="Highlighter 3 (Double-click to edit)" Click="Highlighter_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (5-50)" Minimum="5" Maximum="50" Value="15" Tag="2" ValueChanged="HighlighterThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="2" ColorChanged="HighlighterColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE7E6;" /><Border x:Name="IndHighlighter2" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Cyan"/></StackPanel>
                </ToggleButton>

                <AppBarSeparator />
                
                <!-- Eraser -->
                <ToggleButton x:Name="BtnEraser" ToolTipService.ToolTip="Eraser (Double-click for options)" Click="Eraser_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout>
                            <StackPanel Spacing="10">
                                <ComboBox x:Name="EraserTypeCombo" Header="Eraser Mode" SelectedIndex="0" SelectionChanged="EraserType_Changed">
                                    <ComboBoxItem Content="Stroke Eraser" />
                                    <ComboBoxItem Content="Pixel Eraser" />
                                </ComboBox>
                                <Slider Header="Size (Pixel Mode)" Minimum="5" Maximum="100" Value="10" ValueChanged="EraserThickness_Changed" />
                            </StackPanel>
                        </Flyout>
                    </ToggleButton.ContextFlyout>
                <StackPanel><FontIcon Glyph="&#xE75C;" /><Border Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Transparent"/></StackPanel></ToggleButton>

                <AppBarSeparator />
                
                <ToggleButton x:Name="BtnText" ToolTipService.ToolTip="Text Mode" Click="ToolButton_Click">
                    <StackPanel><FontIcon Glyph="&#xE8D2;" /><Border x:Name="IndText" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Black"/></StackPanel>
                </ToggleButton>
                
                <ToggleButton x:Name="BtnTextHighlighter" ToolTipService.ToolTip="Text Highlighter" Click="ToolButton_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout>
                            <StackPanel Spacing="10">
                                <ColorPicker x:Name="TextHighlighterColorPicker" ColorChanged="TextHighlighterColor_Changed" />
                            </StackPanel>
                        </Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE7E6;" /><Border x:Name="IndTextHighlighter" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Yellow"/></StackPanel>
                </ToggleButton>

                <ToggleButton x:Name="BtnLasso" ToolTipService.ToolTip="Lasso Select" Click="ToolButton_Click">
                    <StackPanel><FontIcon Glyph="&#xEF20;" /><Border Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Transparent"/></StackPanel>
                </ToggleButton>

                <AppBarSeparator />
                
                <ToggleButton x:Name="BtnStraightLine" Content="&#xE108;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Straight Line Mode" Click="ToggleOption_Click" />
                <ToggleButton x:Name="BtnTextSnapping" Content="&#xE8D2;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Text Snapping (Coming Soon)" Click="ToggleOption_Click" IsEnabled="False" />
                
                <AppBarSeparator />
                <Button Content="&#xE712;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Settings &amp; Layout">
                    <Button.Flyout>
                        <MenuFlyout x:Name="SettingsMenu">
                            <RadioMenuFlyoutItem Text="Dock: Top" Tag="Top" GroupName="Dock" Click="ToolbarPos_Click" IsChecked="True" />
                            <RadioMenuFlyoutItem Text="Dock: Bottom" Tag="Bottom" GroupName="Dock" Click="ToolbarPos_Click" />
                            <RadioMenuFlyoutItem Text="Dock: Left" Tag="Left" GroupName="Dock" Click="ToolbarPos_Click" />
                            <RadioMenuFlyoutItem Text="Dock: Right" Tag="Right" GroupName="Dock" Click="ToolbarPos_Click" />
                            <MenuFlyoutSeparator />
                            <RadioMenuFlyoutItem Text="Scroll: Vertical" Tag="Vertical" GroupName="Scroll" Click="ScrollMode_Click" IsChecked="True" />
                            <RadioMenuFlyoutItem Text="Scroll: Horizontal" Tag="Horizontal" GroupName="Scroll" Click="ScrollMode_Click" />
                            <MenuFlyoutSeparator />
                            <ToggleMenuFlyoutItem x:Name="MenuMouseDraw" Text="Enable Mouse Drawing" Click="MenuMouseDraw_Click" />
                        </MenuFlyout>
                    </Button.Flyout>
                </Button>
                
                <Button x:Name="BtnFitWidth" ToolTipService.ToolTip="Fit to Page Width" Click="BtnFitWidth_Click">
                    <FontIcon Glyph="&#xE8A3;" />
                </Button>

                <!-- Hidden components for logic binding -->
                <ComboBox x:Name="TextFontFamilyCombo" Visibility="Collapsed" />'''

# we need to replace everything from BtnHighlighter2 to the end of ToolbarStack
start_idx = text.find('<ToggleButton x:Name="BtnHighlighter2"')
end_idx = text.find('</StackPanel>', start_idx)

if start_idx != -1 and end_idx != -1:
    new_text = text[:start_idx] + replacement + text[end_idx:]
    with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
        f.write(new_text)
    print("Replaced!")
else:
    print("Not found")

