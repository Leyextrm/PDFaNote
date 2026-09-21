import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# fix duplicate PageView_DeletePageRequested
first_del_idx = text.find('private async void PageView_DeletePageRequested')
if first_del_idx != -1:
    end_idx = text.find('}', first_del_idx)
    if end_idx != -1:
        end_idx = text.find('}', end_idx + 1)
        if end_idx != -1:
            end_idx = text.find('}', end_idx + 1)
            text = text[:first_del_idx] + text[end_idx + 1:]

# fix duplicate BtnFitWidth_Click
first_fit_idx = text.find('private void BtnFitWidth_Click')
if first_fit_idx != -1:
    end_idx = text.find('}', first_fit_idx)
    if end_idx != -1:
        end_idx = text.find('}', end_idx + 1)
        if end_idx != -1:
            end_idx = text.find('}', end_idx + 1)
            text = text[:first_fit_idx] + text[end_idx + 1:]

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed duplicates")
