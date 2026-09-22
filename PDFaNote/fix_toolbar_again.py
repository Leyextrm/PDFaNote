import re

with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

bad_buttons = '''<Button Content="&#xE7BF;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Close" Click="BtnClose_Click" />
                <Button Content="&#xE74E;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Save" Click="BtnSave_Click" />
                <Button Content="&#xE792;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Save As" Click="BtnSaveAs_Click" />'''

good_buttons = '''<Button Content="&#xE7A7;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Undo (Ctrl+Z)" Click="BtnUndo_Click" />
                <Button Content="&#xE7A6;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Redo (Ctrl+Y)" Click="BtnRedo_Click" />'''

text = text.replace(bad_buttons, good_buttons)

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Fixed Toolbar buttons")
