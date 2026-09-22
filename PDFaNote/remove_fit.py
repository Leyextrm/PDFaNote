import re

with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Remove BtnFitWidth
text = re.sub(r'<Button x:Name="BtnFitWidth".*?</Button>', '', text, flags=re.DOTALL)
# Remove extra empty lines left by removal
text = re.sub(r'\n\s*\n\s*<Button Content="&#xE712;"', '\n                <Button Content="&#xE712;"', text)

with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Removed BtnFitWidth")
