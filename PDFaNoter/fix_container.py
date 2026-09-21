import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

update_code = """
        private void UpdateContainerSize()
        {
            if (PdfPagesContainer != null && PdfScrollViewer.ViewportWidth > 0 && PdfScrollViewer.ViewportHeight > 0)
            {
                double zoom = PdfScrollViewer.ZoomFactor;
                if (zoom <= 0) zoom = 1.0;
                PdfPagesContainer.MinWidth = PdfScrollViewer.ViewportWidth / zoom;
                PdfPagesContainer.MinHeight = PdfScrollViewer.ViewportHeight / zoom;
            }
        }

        private void PdfScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)"""

content = content.replace("private void PdfScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)", update_code)

size_changed = """        private void PdfScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateContainerSize();
            if (_isFitToWidth && e.NewSize.Width > 0)
            {
                ApplyFitToWidth();
            }
        }"""

content = re.sub(r"private void PdfScrollViewer_SizeChanged\(object sender, SizeChangedEventArgs e\)\s*\{\s*if \(_isFitToWidth && e.NewSize.Width > 0\)\s*\{\s*ApplyFitToWidth\(\);\s*\}\s*\}", size_changed, content)

view_changed = """        private void PdfScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateContainerSize();
            UpdateCurrentPageNumber();
        }"""

content = re.sub(r"private void PdfScrollViewer_ViewChanged\(object\? sender, ScrollViewerViewChangedEventArgs e\)\s*\{\s*UpdateCurrentPageNumber\(\);\s*\}", view_changed, content)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
