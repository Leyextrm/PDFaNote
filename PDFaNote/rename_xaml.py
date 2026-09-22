with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('BtnFitPage', 'BtnFitWidth').replace('Fit to Page', 'Fit to Width')

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
