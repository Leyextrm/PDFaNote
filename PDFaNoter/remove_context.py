import re

with open('PdfPageView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = re.sub(r'<Grid.ContextFlyout>.*?</Grid.ContextFlyout>', '', text, flags=re.DOTALL)
text = text.replace('<Grid.Shadow>', '<!-- Adding a slight shadow to make pages distinct -->\n            <Grid.Shadow>')

with open('PdfPageView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed ContextFlyout")
