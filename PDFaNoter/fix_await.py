import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('await newPageView.LoadPage(newPageData, History);', 'newPageView.LoadPage(newPageData, History);')

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed await")
