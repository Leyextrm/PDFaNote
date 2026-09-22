$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$content = $content.Replace('float vx = (float)(pt.X / 2.0);', 'float vx = (float)(pt.X / 2.0) * 0.75f;')
$content = $content.Replace('float vy = (float)(pt.Y / 2.0);', 'float vy = (float)(pt.Y / 2.0) * 0.75f;')
$content = $content.Replace('extracted.Points.Add(new Windows.Foundation.Point(vx * 2.0, vy * 2.0));', 'extracted.Points.Add(new Windows.Foundation.Point(vx * 2.0 / 0.75f, vy * 2.0 / 0.75f));')
$content = $content.Replace('annot.Put(new iText.Kernel.Pdf.PdfName(""PDFaNoterThickness""), new iText.Kernel.Pdf.PdfNumber(stroke.StrokeThickness));', 'annot.Put(new iText.Kernel.Pdf.PdfName(""PDFaNoterThickness""), new iText.Kernel.Pdf.PdfNumber(stroke.StrokeThickness * 0.75f));')
$content = $content.Replace('canvas.SetLineWidth((float)(stroke.StrokeThickness / 2.0));', 'canvas.SetLineWidth((float)(stroke.StrokeThickness / 2.0 * 0.75f));')
$content = $content.Replace('extracted.Thickness = customThickness.GetValue();', 'extracted.Thickness = customThickness.GetValue() / 0.75f;')
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
