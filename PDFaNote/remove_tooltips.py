import os, re

def remove_tooltips(filename):
    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()
    content = re.sub(r'\s*ToolTipService\.ToolTip="[^"]*"', '', content)
    with open(filename, 'w', encoding='utf-8') as f:
        f.write(content)

remove_tooltips('MainPage.xaml')
remove_tooltips('PdfDocumentView.xaml')
print("Tooltips removed.")
