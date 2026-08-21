$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw

$content = $content -replace '(?s)var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas\(pdfPage\);.*?canvas.Stroke\(\);\s*\}', 
'foreach (var stroke in pageData.Strokes)
                    {
                        if (stroke.Points.Count < 2) continue;
                        
                        var color = (stroke.Stroke as SolidColorBrush).Color;
                        var pdfColor = new iText.Kernel.Colors.DeviceRgb(color.R, color.G, color.B);
                        
                        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                        foreach (var pt in stroke.Points)
                        {
                            float pdfX = (float)(pt.X / 2.0);
                            float pdfY = pageSize.GetHeight() - (float)(pt.Y / 2.0);
                            if (pdfX < minX) minX = pdfX;
                            if (pdfX > maxX) maxX = pdfX;
                            if (pdfY < minY) minY = pdfY;
                            if (pdfY > maxY) maxY = pdfY;
                        }
                        
                        float padding = (float)(stroke.StrokeThickness / 2.0) + 2f;
                        minX -= padding; minY -= padding; maxX += padding; maxY += padding;
                        var rect = new iText.Kernel.Geom.Rectangle(minX, minY, maxX - minX, maxY - minY);
                        
                        var annot = new iText.Kernel.Pdf.Annot.PdfInkAnnotation(rect);
                        annot.SetFlags(iText.Kernel.Pdf.Annot.PdfAnnotation.PRINT);
                        
                        var appearance = new iText.Kernel.Pdf.Xobject.PdfFormXObject(rect);
                        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(appearance, pdfDoc);
                        
                        canvas.SetStrokeColor(pdfColor);
                        canvas.SetLineWidth((float)(stroke.StrokeThickness / 2.0));
                        canvas.SetLineJoinStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineJoinStyle.ROUND);
                        canvas.SetLineCapStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineCapStyle.ROUND);
                        
                        var gs = new iText.Kernel.Pdf.Extgstate.PdfExtGState();
                        gs.SetStrokeOpacity((float)(color.A / 255.0));
                        if (color.A < 255)
                        {
                            gs.SetBlendMode(iText.Kernel.Pdf.Extgstate.PdfExtGState.BM_MULTIPLY);
                        }
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
                        
                        annot.SetNormalAppearance(appearance.GetPdfObject());
                        pdfPage.AddAnnotation(annot);
                    }'

Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
