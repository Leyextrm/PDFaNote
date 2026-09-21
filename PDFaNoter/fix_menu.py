import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

old_code = '''            TextFontFamilyCombo.ItemsSource = ToolState.SupportedFonts;
            TextFontFamilyCombo.SelectedItem = ToolState.TextFontFamily;

            ApplyToolbarDockPosition(ToolState.ToolbarDockPosition);'''

new_code = '''            TextFontFamilyCombo.ItemsSource = ToolState.SupportedFonts;
            TextFontFamilyCombo.SelectedItem = ToolState.TextFontFamily;
            
            if (MenuMouseDraw != null) MenuMouseDraw.IsChecked = ToolState.MouseDrawEnabled;

            ApplyToolbarDockPosition(ToolState.ToolbarDockPosition);'''

text = text.replace(old_code, new_code)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Updated MenuMouseDraw.IsChecked")
