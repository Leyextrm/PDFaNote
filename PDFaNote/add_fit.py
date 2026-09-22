with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

idx_class_end = text.rfind('    }')
if idx_class_end != -1:
    text = text[:idx_class_end] + '''        private void BtnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            if (_pages == null || _pages.Count == 0) return;
            double targetWidth = _pages[0].PageContainer.Width;
            if (ToolState.ScrollMode != "Horizontal")
            {
                targetWidth += 168; // Account for margins in vertical mode
            }
            if (double.IsNaN(targetWidth) || targetWidth <= 0) targetWidth = _pages[0].ActualWidth;
            if (targetWidth <= 0) targetWidth = _pages[0].PageData.Size.Width;
            if (targetWidth <= 0) return;
            
            float newZoom = (float)(PdfScrollViewer.ViewportWidth / targetWidth);
            PdfScrollViewer.ChangeView(null, null, newZoom);
        }
''' + text[idx_class_end:]
    with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
        f.write(text)
    print("Added BtnFitWidth_Click!")
else:
    print("Not found end of class!")
