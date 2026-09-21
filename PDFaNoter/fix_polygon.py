import re

with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('if (IsPointInPolygon(center, polygon.Points))', 'if (rect.Contains(center))')

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Fixed polygon.Points")
