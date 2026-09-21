import re

for tf in ['PdfPageView.xaml', 'PdfPageView.xaml.cs', 'PdfDocumentView.xaml', 'PdfDocumentView.xaml.cs']:
    try:
        with open(tf, 'rb') as f:
            content = f.read().decode('utf-8-sig')
        # fix multiple \r
        content = re.sub(r'\r+', '\r', content)
        content = content.replace('\r\n', '\n').replace('\r', '\n')
        # now everything is pure \n
        with open(tf, 'wb') as f:
            f.write(content.encode('utf-8-sig'))
    except:
        pass
print("Normalized line endings to UNIX format temporarily.")
