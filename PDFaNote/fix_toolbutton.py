import re

with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('x:Name="BtnText" ToolTipService.ToolTip="Text Mode" Click="ToolButton_Click"', 'x:Name="BtnText" ToolTipService.ToolTip="Text Mode" Click="Text_Click"')
text = text.replace('x:Name="BtnTextHighlighter" ToolTipService.ToolTip="Text Highlighter" Click="ToolButton_Click"', 'x:Name="BtnTextHighlighter" ToolTipService.ToolTip="Text Highlighter" Click="TextHighlighter_Click"')
text = text.replace('x:Name="BtnLasso" ToolTipService.ToolTip="Lasso Select" Click="ToolButton_Click"', 'x:Name="BtnLasso" ToolTipService.ToolTip="Lasso Select" Click="Lasso_Click"')

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
