$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$pattern = '(?s)public async Task LoadPdfAsync\(Windows\.Storage\.StorageFile file\).*?_pages\.Add\(pageView\);\s*await Task\.Delay\(10\);\s*\}'

$newCode = @'
public async Task LoadPdfAsync(Windows.Storage.StorageFile file)
        {
            _sourceFile = file;
            var extractedStrokesDict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<ExtractedStroke>>();
            var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            
            using (var stream = await file.OpenReadAsync())
            {
                var dotNetStream = stream.AsStreamForRead();
                var reader = new iText.Kernel.Pdf.PdfReader(dotNetStream);
                var ms = new System.IO.MemoryStream();
                var writer = new iText.Kernel.Pdf.PdfWriter(ms);
                var pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader, writer);
                
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var annots = page.GetAnnotations();
                    var extractedStrokes = new System.Collections.Generic.List<ExtractedStroke>();
                    var annotsToRemove = new System.Collections.Generic.List<iText.Kernel.Pdf.Annot.PdfAnnotation>();
                    
                    var cropBox = page.GetCropBox();
                    int rotation = page.GetRotation();
                    float cropW = cropBox.GetWidth();
                    float cropH = cropBox.GetHeight();
                    
                    if (annots != null)
                    {
                        foreach (var annot in annots)
                        {
                            var pdfObject = annot.GetPdfObject();
                            var inkList = pdfObject.GetAsArray(iText.Kernel.Pdf.PdfName.InkList);
                            var customColor = pdfObject.GetAsArray(new iText.Kernel.Pdf.PdfName("PDFaNoterColor"));
                            var customThickness = pdfObject.GetAsNumber(new iText.Kernel.Pdf.PdfName("PDFaNoterThickness"));
                            
                            if (inkList != null && customColor != null && customThickness != null)
                            {
                                var extracted = new ExtractedStroke();
                                extracted.Thickness = customThickness.GetValue();
                                extracted.Color = Windows.UI.Color.FromArgb(
                                    (byte)(customColor.GetAsNumber(3).GetValue() * 255),
                                    (byte)(customColor.GetAsNumber(0).GetValue() * 255),
                                    (byte)(customColor.GetAsNumber(1).GetValue() * 255),
                                    (byte)(customColor.GetAsNumber(2).GetValue() * 255)
                                );
                                
                                for (int j = 0; j < inkList.Size(); j++)
                                {
                                    var strokeArray = inkList.GetAsArray(j);
                                    if (strokeArray == null) continue;
                                    
                                    for (int k = 0; k < strokeArray.Size(); k += 2)
                                    {
                                        float px = strokeArray.GetAsNumber(k).FloatValue();
                                        float py = strokeArray.GetAsNumber(k + 1).FloatValue();
                                        
                                        px -= cropBox.GetLeft();
                                        py -= cropBox.GetBottom();
                                        float vx = 0, vy = 0;
                                        
                                        if (rotation == 0) {
                                            vx = px; vy = cropH - py;
                                        } else if (rotation == 90) {
                                            vx = py; vy = px;
                                        } else if (rotation == 180) {
                                            vx = cropW - px; vy = py;
                                        } else if (rotation == 270) {
                                            vx = cropH - py; vy = cropW - px;
                                        }
                                        
                                        extracted.Points.Add(new Windows.Foundation.Point(vx * 2.0, vy * 2.0));
                                    }
                                }
                                extractedStrokes.Add(extracted);
                                annotsToRemove.Add(annot);
                            }
                        }
                        
                        foreach (var annot in annotsToRemove)
                        {
                            page.RemoveAnnotation(annot);
                        }
                    }
                    extractedStrokesDict[i - 1] = extractedStrokes;
                }
                
                pdfDoc.Close();
                
                ms.Position = 0;
                var randomAccessStream = ms.AsRandomAccessStream();
                await Windows.Storage.Streams.RandomAccessStream.CopyAsync(randomAccessStream, memStream);
            }
            
            _pdfDocument = await Windows.Data.Pdf.PdfDocument.LoadFromStreamAsync(memStream);
            _pages.Clear();
            if (_pdfDocument == null) return;

            for (uint i = 0; i < _pdfDocument.PageCount; i++)
            {
                var pageData = new PdfPageData { PageIndex = i, Document = _pdfDocument };
                if (extractedStrokesDict.ContainsKey((int)i))
                {
                    pageData.ExtractedStrokes = extractedStrokesDict[(int)i];
                }
                var pageView = new PdfPageView();
                pageView.LoadPage(pageData, History);
                _pages.Add(pageView);
                await Task.Delay(10);
            }
'@
$content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, $newCode)
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
