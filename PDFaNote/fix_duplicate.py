import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# find the FIRST PageView_DeletePageRequested and remove it
first_idx = text.find('private async void PageView_DeletePageRequested')
if first_idx != -1:
    end_idx = text.find('}', first_idx)
    if end_idx != -1:
        end_idx = text.find('}', end_idx + 1)
        if end_idx != -1:
            end_idx = text.find('}', end_idx + 1)
            text = text[:first_idx] + text[end_idx + 1:]

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

