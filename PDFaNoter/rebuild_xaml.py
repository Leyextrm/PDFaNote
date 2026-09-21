import re

with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# 1. Rename FitPage to FitWidth
text = text.replace('BtnFitPage', 'BtnFitWidth').replace('Fit to Page', 'Fit to Width')
text = text.replace('BtnFitPage_Click', 'BtnFitWidth_Click')

# 2. Add missing tools and hidden components before the closing of ToolbarStack
end_of_stack = text.rfind('</StackPanel>')
if end_of_stack != -1:
    addition = r'''
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

                <!-- Hidden components for logic binding -->
                <ComboBox x:Name="TextFontFamilyCombo" Visibility="Collapsed" />
'''
    text = text[:end_of_stack] + addition + text[end_of_stack:]

# 3. Rename MenuFlyout to SettingsMenu
text = text.replace('<MenuFlyout>', '<MenuFlyout x:Name="SettingsMenu">')

# 4. Move Settings button and FitWidth button to the far right
# We find Settings button block
settings_start = text.find('<Button Content="&#xE712;"')
settings_end = text.find('</Button>', settings_start) + 9

# We find FitWidth button block
fitwidth_start = text.find('<Button x:Name="BtnFitWidth"')
if fitwidth_start == -1:
    fitwidth_start = text.find('<Button x:Name="BtnFitPage"')

fitwidth_end = text.find('</Button>', fitwidth_start) + 9

if settings_start != -1 and fitwidth_start != -1:
    # remove them from original position
    settings_block = text[settings_start:settings_end]
    fitwidth_block = text[fitwidth_start:fitwidth_end]
    
    text = text[:settings_start] + text[settings_end:]
    
    # Wait, fitwidth_start will change after removing settings! So we re-find
    fitwidth_start = text.find('<Button x:Name="BtnFitWidth"')
    if fitwidth_start != -1:
        fitwidth_end = text.find('</Button>', fitwidth_start) + 9
        fitwidth_block = text[fitwidth_start:fitwidth_end]
        text = text[:fitwidth_start] + text[fitwidth_end:]
    
    # insert them back right before the addition
    end_of_stack = text.rfind('</StackPanel>')
    text = text[:end_of_stack] + '\n' + fitwidth_block + '\n' + settings_block + '\n' + text[end_of_stack:]

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Fixed PdfDocumentView.xaml")
