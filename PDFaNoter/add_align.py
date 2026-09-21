with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('<ItemsControl x:Name="PdfPagesControl">', '<ItemsControl x:Name="PdfPagesControl" HorizontalAlignment="Center">')

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
