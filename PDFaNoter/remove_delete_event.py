import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'private async void PageView_DeletePageRequested.*?(?=\s*private System.Collections.Generic.List)', '', text, flags=re.DOTALL)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed PageView_DeletePageRequested")
