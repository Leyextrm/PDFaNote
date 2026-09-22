import re

with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

start_idx = text.find('<StackPanel x:Name="ToolbarStack"')
end_idx = text.find('</Border>', start_idx)

desired_toolbar = '''<StackPanel x:Name="ToolbarStack" Orientation="Horizontal" VerticalAlignment="Center" Spacing="5" Padding="5">
                <Button Content="&#xE7BF;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Close" Click="BtnClose_Click" />
                <Button Content="&#xE74E;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Save" Click="BtnSave_Click" />
                <Button Content="&#xE792;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Save As" Click="BtnSaveAs_Click" />
                
                <AppBarSeparator />
                
                <ToggleButton x:Name="BtnPen0" Tag="0" ToolTipService.ToolTip="Pen 1 (Double-click to edit)" Click="Pen_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (1-20)" Minimum="1" Maximum="20" Value="2" Tag="0" ValueChanged="PenThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="0" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE70F;" /><Border x:Name="IndPen0" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Black"/></StackPanel>
                </ToggleButton>
                <ToggleButton x:Name="BtnPen1" Tag="1" ToolTipService.ToolTip="Pen 2 (Double-click to edit)" Click="Pen_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (1-20)" Minimum="1" Maximum="20" Value="2" Tag="1" ValueChanged="PenThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="1" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE70F;" /><Border x:Name="IndPen1" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Red"/></StackPanel>
                </ToggleButton>
                <ToggleButton x:Name="BtnPen2" Tag="2" ToolTipService.ToolTip="Pen 3 (Double-click to edit)" Click="Pen_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (1-20)" Minimum="1" Maximum="20" Value="2" Tag="2" ValueChanged="PenThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="2" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE70F;" /><Border x:Name="IndPen2" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Blue"/></StackPanel>
                </ToggleButton>
                <ToggleButton x:Name="BtnPen3" Tag="3" ToolTipService.ToolTip="Pen 4 (Double-click to edit)" Click="Pen_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (1-20)" Minimum="1" Maximum="20" Value="2" Tag="3" ValueChanged="PenThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="3" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE70F;" /><Border x:Name="IndPen3" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Green"/></StackPanel>
                </ToggleButton>
                <ToggleButton x:Name="BtnPen4" Tag="4" ToolTipService.ToolTip="Pen 5 (Double-click to edit)" Click="Pen_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (1-20)" Minimum="1" Maximum="20" Value="2" Tag="4" ValueChanged="PenThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="4" ColorChanged="PenColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE70F;" /><Border x:Name="IndPen4" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Purple"/></StackPanel>
                </ToggleButton>

                <AppBarSeparator />
                
                <ToggleButton x:Name="BtnHighlighter0" Tag="0" ToolTipService.ToolTip="Highlighter 1 (Double-click to edit)" Click="Highlighter_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (5-50)" Minimum="5" Maximum="50" Value="15" Tag="0" ValueChanged="HighlighterThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="0" ColorChanged="HighlighterColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE7E6;" /><Border x:Name="IndHighlighter0" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Yellow"/></StackPanel>
                </ToggleButton>
                <ToggleButton x:Name="BtnHighlighter1" Tag="1" ToolTipService.ToolTip="Highlighter 2 (Double-click to edit)" Click="Highlighter_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout><StackPanel Spacing="10"><NumberBox Header="Thickness (5-50)" Minimum="5" Maximum="50" Value="15" Tag="1" ValueChanged="HighlighterThickness_Changed" SpinButtonPlacementMode="Inline" /><ColorPicker Tag="1" ColorChanged="HighlighterColor_Changed" IsMoreButtonVisible="False" IsColorSpectrumVisible="True" IsColorChannelTextInputVisible="True" IsHexInputVisible="True" IsAlphaEnabled="True" /></StackPanel></Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE7E6;" /><Border x:Name="IndHighlighter1" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="LightGreen"/></StackPanel>
                </ToggleButton>
                <ToggleButton x:Name="BtnHighlighter2" Tag="2" ToolTipService.ToolTip="Highlighter 3 (Double-click to edit)" Click="Highlighter_Click">
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
                    <StackPanel><FontIcon Glyph="&#xE75C;" /><Border Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Transparent"/></StackPanel>
                </ToggleButton>

                <AppBarSeparator />
                
                <ToggleButton x:Name="BtnStraightLine" Content="&#xE108;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Straight Line Mode" Click="ToggleOption_Click" />
                
                <AppBarSeparator />
                
                <ToggleButton x:Name="BtnText" ToolTipService.ToolTip="Text Mode" Click="Text_Click">
                    <StackPanel><FontIcon Glyph="&#xE8D2;" /><Border x:Name="IndText" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Black"/></StackPanel>
                </ToggleButton>
                
                <ToggleButton x:Name="BtnTextHighlighter" ToolTipService.ToolTip="Text Highlighter" Click="TextHighlighter_Click">
                    <ToggleButton.ContextFlyout>
                        <Flyout>
                            <StackPanel Spacing="10">
                                <ColorPicker x:Name="TextHighlighterColorPicker" ColorChanged="TextHighlighterColor_Changed" />
                            </StackPanel>
                        </Flyout>
                    </ToggleButton.ContextFlyout>
                    <StackPanel><FontIcon Glyph="&#xE7E6;" /><Border x:Name="IndTextHighlighter" Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Yellow"/></StackPanel>
                </ToggleButton>

                <AppBarSeparator />

                <ToggleButton x:Name="BtnLasso" ToolTipService.ToolTip="Lasso Select" Click="Lasso_Click">
                    <StackPanel><FontIcon Glyph="&#xEF20;" /><Border Height="4" CornerRadius="2" Margin="0,2,0,0" Background="Transparent"/></StackPanel>
                </ToggleButton>

                <AppBarSeparator />
                
                <Button x:Name="BtnFitWidth" ToolTipService.ToolTip="Fit to Width" Click="BtnFitWidth_Click">
                    <FontIcon Glyph="&#xE8A3;" />
                </Button>

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

                <!-- Hidden components for logic binding -->
                <ComboBox x:Name="TextFontFamilyCombo" Visibility="Collapsed" />
            </StackPanel>\n        '''

text = text[:start_idx] + desired_toolbar + text[end_idx:]

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Rebuilt PdfDocumentView.xaml properly")
