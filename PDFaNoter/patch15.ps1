$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw

$content = $content -replace '(?s)public async Task LoadPdfAsync\(.*?\)\s*\{.*?\}', 
'public async Task LoadPdfAsync(Windows.Storage.StorageFile file)
        {
            _sourceFile = file;
            _pdfDocument = await PdfDocument.LoadFromFileAsync(file);
            _pages.Clear();
            if (_pdfDocument == null) return;

            for (uint i = 0; i < _pdfDocument.PageCount; i++)
            {
                var pageData = new PdfPageData { PageIndex = i, Document = _pdfDocument };
                var pageView = new PdfPageView();
                pageView.LoadPage(pageData, History);
                _pages.Add(pageView);
                await Task.Delay(10);
            }
        }

        public async Task SaveAsync(Windows.Storage.StorageFile destFile)
        {
            if (_sourceFile == null) return;
            
            using (var sourceStream = await _sourceFile.OpenStreamForReadAsync())
            using (var destStream = await destFile.OpenStreamForWriteAsync())
            {
                var pdfReader = new iText.Kernel.Pdf.PdfReader(sourceStream);
                var pdfWriter = new iText.Kernel.Pdf.PdfWriter(destStream);
                var pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader, pdfWriter);
                
                for (int i = 0; i < _pages.Count; i++)
                {
                    var pageView = _pages[i];
                    var pageData = pageView.PageData;
                    if (pageData == null) continue;
                    
                    var pdfPage = pdfDoc.GetPage(i + 1);
                    var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdfPage);
                    var pageSize = pdfPage.GetPageSize(); // or get cropbox
                    // The WinRT PdfDocument scales the page size in GetPage(i).Size
                    // iText uses native points. Let''s assume they match perfectly except for the *2 scaling in UI.
                    
                    foreach (var stroke in pageData.Strokes)
                    {
                        if (stroke.Points.Count < 2) continue;
                        var color = (stroke.Stroke as SolidColorBrush).Color;
                        var pdfColor = new iText.Kernel.Colors.DeviceRgb(color.R, color.G, color.B);
                        
                        canvas.SetStrokeColor(pdfColor);
                        canvas.SetLineWidth((float)(stroke.StrokeThickness / 2.0));
                        canvas.SetLineJoinStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineJoinStyle.ROUND);
                        canvas.SetLineCapStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineCapStyle.ROUND);
                        
                        var gs = new iText.Kernel.Pdf.Extgstate.PdfExtGState();
                        gs.SetStrokeOpacity((float)(color.A / 255.0));
                        canvas.SetExtGState(gs);
                        
                        bool first = true;
                        foreach (var pt in stroke.Points)
                        {
                            float pdfX = (float)(pt.X / 2.0);
                            float pdfY = pageSize.GetHeight() - (float)(pt.Y / 2.0);
                            if (first) { canvas.MoveTo(pdfX, pdfY); first = false; }
                            else { canvas.LineTo(pdfX, pdfY); }
                        }
                        canvas.Stroke();
                    }
                }
                pdfDoc.Close();
            }
            History.HasUnsavedChanges = false;
        }'

Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
