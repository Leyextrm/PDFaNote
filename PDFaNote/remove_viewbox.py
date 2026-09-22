import re
with open('PdfPageView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('<Viewbox x:Name="PdfViewbox" Stretch="Uniform" HorizontalAlignment="Center" VerticalAlignment="Center">', '')
text = text.replace('</Viewbox>', '')

with open('PdfPageView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
