import re
with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Add missing buttons before the closing of ToolbarStack
end_of_stack = text.find('</StackPanel>\r\n        </Border>')
if end_of_stack == -1:
    end_of_stack = text.find('</StackPanel>\n        </Border>')

if end_of_stack != -1:
    addition = r'''
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

                <Button x:Name="BtnFitWidth" ToolTipService.ToolTip="Fit to Page Width" Click="BtnFitWidth_Click">
                    <FontIcon Glyph="&#xE8A3;" />
                </Button>

                <!-- Hidden components for logic binding -->
                <ComboBox x:Name="TextFontFamilyCombo" Visibility="Collapsed" />
'''
    # We must also rename the MenuFlyout to SettingsMenu
    text = text.replace('<MenuFlyout>', '<MenuFlyout x:Name="SettingsMenu">')
    text = text.replace('BtnFitPage_Click', 'BtnFitWidth_Click')
    
    new_text = text[:end_of_stack] + addition + text[end_of_stack:]
    with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
        f.write(new_text)
    print("Fixed!")
