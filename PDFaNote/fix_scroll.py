import re
with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Instead of passing it around, we can just save it inside InsertBlankPageAfterAsync and DeletePageAsync
# Actually, inside LoadPdfAsync it clears _pages.
old_load = 'await LoadPdfAsync(_workingFile, isReload: true);'
new_load = '''double hOffset = PdfScrollViewer.HorizontalOffset;
            double vOffset = PdfScrollViewer.VerticalOffset;
            float zoom = PdfScrollViewer.ZoomFactor;
            
            await LoadPdfAsync(_workingFile, isReload: true);
            
            await Task.Delay(100);
            PdfScrollViewer.ChangeView(hOffset, vOffset, zoom, disableAnimation: true);'''
text = text.replace(old_load, new_load)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Updated scroll restore!")
