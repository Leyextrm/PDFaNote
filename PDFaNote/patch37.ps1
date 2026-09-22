$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$pattern = '(?s)float minX = float\.MaxValue.*?canvas\.Stroke\(\);'

$newCode = @'
                        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                        var inkList = new iText.Kernel.Pdf.PdfArray();
                        var strokeArray = new iText.Kernel.Pdf.PdfArray();
                        
                        var cropBox = pdfPage.GetCropBox();
                        int rotation = pdfPage.GetRotation();
                        float cropW = cropBox.GetWidth();
                        float cropH = cropBox.GetHeight();

                        foreach (var pt in stroke.Points)
                        {
                            float vx = (float)(pt.X / 2.0);
                            float vy = (float)(pt.Y / 2.0);
                            float px = 0, py = 0;
                            
                            if (rotation == 0) {
                                px = vx; py = cropH - vy;
                            } else if (rotation == 90) {
                                px = vy; py = vx;
                            } else if (rotation == 180) {
                                px = cropW - vx; py = vy;
                            } else if (rotation == 270) {
                                px = cropW - vy; py = cropH - vx;
                            }
                            
                            px += cropBox.GetLeft();
                            py += cropBox.GetBottom();
                            
                            strokeArray.Add(new iText.Kernel.Pdf.PdfNumber(px));
                            strokeArray.Add(new iText.Kernel.Pdf.PdfNumber(py));
                            
                            if (px < minX) minX = px;
                            if (px > maxX) maxX = px;
                            if (py < minY) minY = py;
                            if (py > maxY) maxY = py;
                        }
                        inkList.Add(strokeArray);
                        
                        float padding = (float)(stroke.StrokeThickness / 2.0) + 2f;
                        minX -= padding; minY -= padding; maxX += padding; maxY += padding;
                        var rect = new iText.Kernel.Geom.Rectangle(minX, minY, maxX - minX, maxY - minY);
                        
                        var annot = new iText.Kernel.Pdf.Annot.PdfInkAnnotation(rect);
                        annot.SetFlags(iText.Kernel.Pdf.Annot.PdfAnnotation.PRINT);
                        
                        annot.Put(iText.Kernel.Pdf.PdfName.InkList, inkList);
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterThickness"), new iText.Kernel.Pdf.PdfNumber(stroke.StrokeThickness));
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterColor"), new iText.Kernel.Pdf.PdfArray(new float[] { color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f }));
                        
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
                        for (int j = 0; j < strokeArray.Size(); j += 2)
                        {
                            float px = strokeArray.GetAsNumber(j).FloatValue();
                            float py = strokeArray.GetAsNumber(j + 1).FloatValue();
                            
                            if (first) { canvas.MoveTo(px, py); first = false; }
                            else { canvas.LineTo(px, py); }
                        }
                        canvas.Stroke();
'@
$content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, $newCode)
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
