import re

with open('PdfPageView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'<StackPanel x:Name="ButtonStackPanel".*?</StackPanel>', '', text, flags=re.DOTALL)

with open('PdfPageView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)

with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'public event EventHandler InsertPageRequested;.*?DeletePageRequested\?\.Invoke\(this, EventArgs\.Empty\);\s*\}', '', text, flags=re.DOTALL)
text = re.sub(r'public void SetLayoutMode\(bool isHorizontal\).*?\}\s*\}', '', text, flags=re.DOTALL)

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'pageView\.InsertPageRequested \+= PageView_InsertPageRequested;\s*pageView\.DeletePageRequested \+= PageView_DeletePageRequested;\s*pageView\.SetLayoutMode\(ToolState\.ScrollMode == "Horizontal"\);', '', text)
text = re.sub(r'page\.SetLayoutMode\(isHoriz\);', '', text)
text = re.sub(r'private async void PageView_InsertPageRequested.*?\}\s*\}\s*\}', '', text, flags=re.DOTALL)
text = re.sub(r'public async Task InsertBlankPageAfterAsync.*?LoadPdfAsync\(_workingFile, isReload: true\);\s*\}', '', text, flags=re.DOTALL)
text = re.sub(r'public async Task DeletePageAsync.*?LoadPdfAsync\(_workingFile, isReload: true\);\s*\}', '', text, flags=re.DOTALL)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Removed Insert/Delete Page features.")
