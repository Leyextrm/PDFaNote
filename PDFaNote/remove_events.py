import re

with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'public event EventHandler InsertPageRequested;.*?DeletePageRequested\?\.Invoke\(this, EventArgs\.Empty\);\s*}', '', text, flags=re.DOTALL)
text = re.sub(r'private void BtnInsertBlank_Click.*?}', '', text, flags=re.DOTALL)
text = re.sub(r'private void BtnDeletePage_Click.*?}', '', text, flags=re.DOTALL)

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed events from CS")
