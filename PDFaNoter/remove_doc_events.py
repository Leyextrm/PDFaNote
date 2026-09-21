import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'private async void PageView_InsertPageRequested.*?}\s*}\s*}', '', text, flags=re.DOTALL)
text = re.sub(r'public async Task InsertBlankPageAfterAsync.*?(?=\s*public async Task DeletePageAsync)', '', text, flags=re.DOTALL)
text = re.sub(r'public async Task DeletePageAsync.*?(?=\s*private System.Collections.Generic.List)', '', text, flags=re.DOTALL)

text = re.sub(r'newPageView\.InsertPageRequested \+= PageView_InsertPageRequested;\s*newPageView\.DeletePageRequested \+= PageView_DeletePageRequested;\s*', '', text)
text = re.sub(r'pageView\.InsertPageRequested \+= PageView_InsertPageRequested;\s*pageView\.DeletePageRequested \+= PageView_DeletePageRequested;\s*', '', text)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed from PdfDocumentView.xaml.cs")
