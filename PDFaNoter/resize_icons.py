with open('PdfPageView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('Width="32" Height="32" CornerRadius="16"', 'Width="44" Height="44" CornerRadius="22"')
text = text.replace('FontSize="16"', 'FontSize="18"')

with open('PdfPageView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
