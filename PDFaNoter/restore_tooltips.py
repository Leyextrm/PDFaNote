import os
import re

def restore_tooltips():
    # PdfDocumentView.xaml mapping
    pdf_tooltips = {
        r'(<Button Content="&#xE7A7;" FontFamily="Segoe Fluent Icons")': r'\1 ToolTipService.ToolTip="Undo (Ctrl+Z)"',
        r'(<Button Content="&#xE7A6;" FontFamily="Segoe Fluent Icons")': r'\1 ToolTipService.ToolTip="Redo (Ctrl+Shift+Z / Ctrl+Y)"',
        r'(<ToggleButton x:Name="BtnPen0" Tag="0")': r'\1 ToolTipService.ToolTip="Pen 1 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnPen1" Tag="1")': r'\1 ToolTipService.ToolTip="Pen 2 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnPen2" Tag="2")': r'\1 ToolTipService.ToolTip="Pen 3 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnPen3" Tag="3")': r'\1 ToolTipService.ToolTip="Pen 4 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnPen4" Tag="4")': r'\1 ToolTipService.ToolTip="Pen 5 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnHighlighter0" Tag="0")': r'\1 ToolTipService.ToolTip="Highlighter 1 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnHighlighter1" Tag="1")': r'\1 ToolTipService.ToolTip="Highlighter 2 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnHighlighter2" Tag="2")': r'\1 ToolTipService.ToolTip="Highlighter 3 (Double-click to edit)"',
        r'(<ToggleButton x:Name="BtnEraser")': r'\1 ToolTipService.ToolTip="Eraser (Double-click for options)"',
        r'(<ToggleButton x:Name="BtnStraightLine" Content="&#xE108;" FontFamily="Segoe Fluent Icons")': r'\1 ToolTipService.ToolTip="선 (Straight Line)"',
        r'(<ToggleButton x:Name="BtnSnapToText" Content="&#xE8AC;" FontFamily="Segoe Fluent Icons")': r'\1 ToolTipService.ToolTip="텍스트에 스냅 (Snap to Text)"',
        r'(<ToggleButton x:Name="BtnText")': r'\1 ToolTipService.ToolTip="Text Mode"',
        r'(<ToggleButton x:Name="BtnLasso")': r'\1 ToolTipService.ToolTip="Lasso Select"',
        r'(<Button x:Name="BtnFitWidth")': r'\1 ToolTipService.ToolTip="Fit to Page Width"',
        r'(<Button>[\r\n\s]+<StackPanel><FontIcon Glyph="&#xE712;" />)': r'<Button ToolTipService.ToolTip="Settings &amp; Layout">\n                    <StackPanel><FontIcon Glyph="&#xE712;" />',
    }

    # MainPage.xaml mapping
    main_tooltips = {
        r'(<AppBarButton Icon="OpenFile" Label="Open" Click="BtnOpen_Click")': r'\1 ToolTipService.ToolTip="Open (Ctrl+O)"',
        r'(<AppBarButton Icon="Save" Label="Save" Click="BtnSave_Click")': r'\1 ToolTipService.ToolTip="Save (Ctrl+S)"',
        r'(<AppBarButton Icon="SaveLocal" Label="Save As" Click="BtnSaveAs_Click")': r'\1 ToolTipService.ToolTip="Save As (Ctrl+Shift+S)"',
    }

    def process_file(filename, mappings):
        with open(filename, 'r', encoding='utf-8') as f:
            content = f.read()
        
        for pattern, replacement in mappings.items():
            if filename == 'PdfDocumentView.xaml' and "Settings &amp; Layout" in replacement:
                 # special case since we match multiple lines
                 content = re.sub(r'<Button>\s*<StackPanel><FontIcon Glyph="&#xE712;" />', r'<Button ToolTipService.ToolTip="Settings &amp; Layout">\n                    <StackPanel><FontIcon Glyph="&#xE712;" />', content, count=1)
            else:
                 content = re.sub(pattern, replacement, content, count=1)
        
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(content)

    process_file('PdfDocumentView.xaml', pdf_tooltips)
    process_file('MainPage.xaml', main_tooltips)

restore_tooltips()
print("Tooltips restored.")
