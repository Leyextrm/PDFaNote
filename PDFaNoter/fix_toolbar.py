import re

with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# 1. Remove BtnTextSnapping
text = re.sub(r'\s*<ToggleButton x:Name="BtnTextSnapping"[^>]+/>', '', text)

# 2. Reduce double AppBarSeparator to single
text = re.sub(r'(<AppBarSeparator\s*/>\s*)+<ToggleButton x:Name="BtnText"', r'<AppBarSeparator />\n                <ToggleButton x:Name="BtnText"', text)

# 3. Add AppBarSeparator after BtnTextHighlighter
# We find BtnTextHighlighter end tag, then we insert the separator before BtnLasso
old_text_highlighter_end = '''</ToggleButton>

                <ToggleButton x:Name="BtnLasso"'''

new_text_highlighter_end = '''</ToggleButton>
                
                <AppBarSeparator />

                <ToggleButton x:Name="BtnLasso"'''

text = text.replace(old_text_highlighter_end, new_text_highlighter_end)

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Updated toolbar XAML")
