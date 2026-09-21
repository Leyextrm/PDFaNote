using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Data.Pdf;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.IO;
using System;

namespace PDFaNoter
{
    public sealed partial class PdfDocumentView : UserControl
    {
        private Windows.Storage.StorageFile _sourceFile;
        public Windows.Storage.StorageFile SourceFile => _sourceFile;
        private Windows.Storage.StorageFile _workingFile;
        private PdfDocument _pdfDocument;
        private ObservableCollection<PdfPageView> _pages = new ObservableCollection<PdfPageView>();
        public CommandHistory History { get; private set; } = new CommandHistory();
        
        private List<ToggleButton> _penButtons;
        private List<ToggleButton> _highlighterButtons;
        private int _currentPageNumber = 1;
        private DispatcherTimer? _pageToastTimer;

        public PdfDocumentView()
        {
            this.InitializeComponent();
            PdfPagesControl.ItemsSource = _pages;
            
            _penButtons = new List<ToggleButton> { BtnPen0, BtnPen1, BtnPen2, BtnPen3, BtnPen4 };
            _highlighterButtons = new List<ToggleButton> { BtnHighlighter0, BtnHighlighter1, BtnHighlighter2 };
            
            IndPen0.Background = ToolState.PenColors[0];
            IndPen1.Background = ToolState.PenColors[1];
            IndPen2.Background = ToolState.PenColors[2];
            IndPen3.Background = ToolState.PenColors[3];
            IndPen4.Background = ToolState.PenColors[4];

            IndHighlighter0.Background = ToolState.HighlighterColors[0];
            IndHighlighter1.Background = ToolState.HighlighterColors[1];
            IndHighlighter2.Background = ToolState.HighlighterColors[2];

            IndText.Background = ToolState.TextColor;
            TextFontFamilyCombo.ItemsSource = ToolState.SupportedFonts;
            TextFontFamilyCombo.SelectedItem = ToolState.TextFontFamily;

            ApplyToolbarDockPosition(ToolState.ToolbarDockPosition);
            ApplyScrollMode(ToolState.ScrollMode);
            MenuMouseDraw.IsChecked = ToolState.MouseDrawEnabled;

            UpdateToolSelection();
        }

        public async Task LoadPdfAsync(Windows.Storage.StorageFile file, bool isReload = false)
        {
            if (!isReload) {
                _sourceFile = file;
                _workingFile = file;
            } else {
                _workingFile = file;
            }
            var extractedStrokesDict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<ExtractedStroke>>();
            var pigWordsDict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<PdfTextWord>>();
            
            try {
                using (var pigStream = await file.OpenReadAsync()) {
                    var pigDotNetStream = pigStream.AsStreamForRead();
                    using (var pigDoc = UglyToad.PdfPig.PdfDocument.Open(pigDotNetStream, new UglyToad.PdfPig.ParsingOptions { UseLenientParsing = true })) {
                        for (int i = 1; i <= pigDoc.NumberOfPages; i++) {
                            var page = pigDoc.GetPage(i);
                            var words = new System.Collections.Generic.List<PdfTextWord>();
                            foreach (var letter in page.Letters) {
                                words.Add(new PdfTextWord { Text = letter.Value, Bounds = new Windows.Foundation.Rect(letter.BoundingBox.Left, letter.BoundingBox.Bottom, letter.BoundingBox.Width, letter.BoundingBox.Height) });
                            }
                            pigWordsDict[i - 1] = words;
                        }
                    }
                }
            } catch { }

            var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var extractedTextsDict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<ExtractedTextAnnotation>>();
            
            using (var stream = await file.OpenReadAsync())
            {
                var dotNetStream = stream.AsStreamForRead();
                var reader = new iText.Kernel.Pdf.PdfReader(dotNetStream);
                var writerStream = memStream.AsStreamForWrite();
                var writer = new iText.Kernel.Pdf.PdfWriter(writerStream); writer.SetCloseStream(false);
                var pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader, writer);
                
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var annots = page.GetAnnotations();
                    var extractedStrokes = new System.Collections.Generic.List<ExtractedStroke>();
                    var extractedTextAnnotations = new System.Collections.Generic.List<ExtractedTextAnnotation>();
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
                                extracted.Thickness = customThickness.GetValue() / 0.75f;
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
                                        
                                        extracted.Points.Add(new Windows.Foundation.Point(vx * 2.0 / 0.75f, vy * 2.0 / 0.75f));
                                    }
                                }
                                extractedStrokes.Add(extracted);
                                annotsToRemove.Add(annot);
                            }
                            // Restore PDFaNoter text annotations as editable TextBoxes
                            else if (pdfObject.Get(new iText.Kernel.Pdf.PdfName("PDFaNoterText")) != null)
                            {
                                var textContent = pdfObject.GetAsString(iText.Kernel.Pdf.PdfName.Contents);
                                var rect = annot.GetRectangle();
                                var colorArr = pdfObject.GetAsArray(new iText.Kernel.Pdf.PdfName("PDFaNoterTextColor"));
                                var fontSizeNum = pdfObject.GetAsNumber(new iText.Kernel.Pdf.PdfName("PDFaNoterFontSize"));
                                var fontFamilyStr = pdfObject.GetAsString(new iText.Kernel.Pdf.PdfName("PDFaNoterFontFamily"));
                                
                                if (textContent != null && rect != null)
                                {
                                    float rx = rect.GetAsNumber(0).FloatValue() - cropBox.GetLeft();
                                    float ry = rect.GetAsNumber(1).FloatValue() - cropBox.GetBottom();
                                    float rtop = rect.GetAsNumber(3).FloatValue() - cropBox.GetBottom();
                                    
                                    float cvx = 0, cvy = 0;
                                    if (rotation == 0) {
                                        cvx = rx; cvy = cropH - rtop;
                                    } else if (rotation == 90) {
                                        cvx = rtop; cvy = rx;
                                    } else if (rotation == 180) {
                                        cvx = cropW - rx; cvy = ry;
                                    } else if (rotation == 270) {
                                        cvx = cropW - ry; cvy = cropH - rx;
                                    }
                                    
                                    Windows.UI.Color textColor = Microsoft.UI.Colors.Black;
                                    if (colorArr != null && colorArr.Size() >= 3)
                                    {
                                        textColor = Windows.UI.Color.FromArgb(255,
                                            (byte)(colorArr.GetAsNumber(0).GetValue() * 255),
                                            (byte)(colorArr.GetAsNumber(1).GetValue() * 255),
                                            (byte)(colorArr.GetAsNumber(2).GetValue() * 255));
                                    }
                                    double fontSize = fontSizeNum != null ? fontSizeNum.GetValue() / 0.75f * 2.0 : 20.0;
                                    string fontFamily = fontFamilyStr != null ? fontFamilyStr.GetValue() : "Malgun Gothic";
                                    
                                    extractedTextAnnotations.Add(new ExtractedTextAnnotation
                                    {
                                        Text = textContent.GetValue().Replace("\n", "\r\n"),
                                        CanvasLeft = cvx * 2.0 / 0.75f,
                                        CanvasTop = cvy * 2.0 / 0.75f,
                                        Color = textColor,
                                        FontSize = fontSize,
                                        FontFamily = fontFamily,
                                    });
                                    annotsToRemove.Add(annot);
                                }
                            }
                        }
                        
                        foreach (var annot in annotsToRemove)
                        {
                            page.RemoveAnnotation(annot);
                        }
                    }
                    extractedStrokesDict[i - 1] = extractedStrokes;
                    extractedTextsDict[i - 1] = extractedTextAnnotations;
                }
                
                pdfDoc.Close();
            }
            
            memStream.Seek(0);
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
                if (extractedTextsDict.ContainsKey((int)i))
                {
                    pageData.ExtractedTexts = extractedTextsDict[(int)i];
                }
                if (pigWordsDict.TryGetValue((int)i, out var wordsList))
                {
                    pageData.Words = wordsList;
                }
                var pageView = new PdfPageView();
                pageView.LoadPage(pageData, History);
                _pages.Add(pageView);
                await Task.Delay(10);
            }
            
            await Task.Delay(100);
            BtnFitWidth_Click(null, null);
            UpdateCurrentPageNumber(false);
        }

        public async Task SaveAsync(Windows.Storage.StorageFile destFile, bool updateSourceFile = true)
        {
            if (_workingFile == null) return;

            // 1. Read entire source file into memory and close handle immediately
            byte[] sourceBytes;
            using (var srcStream = await _workingFile.OpenStreamForReadAsync())
            using (var ms = new MemoryStream())
            {
                await srcStream.CopyToAsync(ms);
                sourceBytes = ms.ToArray();
            }

            // 2. Process with iText in-memory
            byte[] outputBytes;
            using (var inStream = new MemoryStream(sourceBytes))
            using (var outMs = new MemoryStream())
            {
                var pdfReader = new iText.Kernel.Pdf.PdfReader(inStream);
                var pdfWriter = new iText.Kernel.Pdf.PdfWriter(outMs);
                var pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader, pdfWriter);
                
                for (int i = 0; i < _pages.Count; i++)
                {
                    var pageView = _pages[i];
                    var pageData = pageView.PageData;
                    if (pageData == null) continue;
                    
                    var pdfPage = pdfDoc.GetPage(i + 1);

                    // Remove existing PDFaNoter annotations so erased/edited items don't resurrect or duplicate
                    var annots = pdfPage.GetAnnotations();
                    if (annots != null)
                    {
                        var annotsToRemove = new List<iText.Kernel.Pdf.Annot.PdfAnnotation>();
                        foreach (var annot in annots)
                        {
                            var pdfObj = annot.GetPdfObject();
                            if (pdfObj != null)
                            {
                                if (pdfObj.Get(new iText.Kernel.Pdf.PdfName("PDFaNoterColor")) != null ||
                                    pdfObj.Get(new iText.Kernel.Pdf.PdfName("PDFaNoterThickness")) != null ||
                                    pdfObj.Get(new iText.Kernel.Pdf.PdfName("PDFaNoterText")) != null)
                                {
                                    annotsToRemove.Add(annot);
                                }
                            }
                        }
                        foreach (var annot in annotsToRemove)
                        {
                            pdfPage.RemoveAnnotation(annot);
                        }
                    }

                    var pageSize = pdfPage.GetPageSize();
                    foreach (var stroke in pageData.Strokes)
                    {
                        if (stroke.Points.Count < 2) continue;
                        
                        var color = (stroke.Stroke as SolidColorBrush).Color;
                        var pdfColor = new iText.Kernel.Colors.DeviceRgb(color.R, color.G, color.B);
                        
                        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                        var inkList = new iText.Kernel.Pdf.PdfArray();
                        var strokeArray = new iText.Kernel.Pdf.PdfArray();
                        
                        var cropBox = pdfPage.GetCropBox();
                        int rotation = pdfPage.GetRotation();
                        float cropW = cropBox.GetWidth();
                        float cropH = cropBox.GetHeight();

                        IEnumerable<Windows.Foundation.Point> pointsToSave;
                        if (stroke.Tag as string == "IsSaved")
                        {
                            pointsToSave = stroke.Points;
                        }
                        else
                        {
                            pointsToSave = GetSmoothedPoints(stroke.Points);
                            stroke.Tag = "IsSaved";
                        }

                        foreach (var pt in pointsToSave)
                        {
                            float vx = (float)(pt.X / 2.0) * 0.75f;
                            float vy = (float)(pt.Y / 2.0) * 0.75f;
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
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterThickness"), new iText.Kernel.Pdf.PdfNumber(stroke.StrokeThickness * 0.75f));
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterColor"), new iText.Kernel.Pdf.PdfArray(new float[] { color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f }));
                        
                        var appearance = new iText.Kernel.Pdf.Xobject.PdfFormXObject(rect);
                        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(appearance, pdfDoc);
                        
                        canvas.SetStrokeColor(pdfColor);
                        canvas.SetLineWidth((float)(stroke.StrokeThickness / 2.0 * 0.75f));
                        canvas.SetLineJoinStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineJoinStyle.ROUND);
                        
                        if (stroke.StrokeStartLineCap == PenLineCap.Square || stroke.StrokeStartLineCap == PenLineCap.Flat)
                        {
                            canvas.SetLineCapStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineCapStyle.BUTT);
                        }
                        else
                        {
                            canvas.SetLineCapStyle(iText.Kernel.Pdf.Canvas.PdfCanvasConstants.LineCapStyle.ROUND);
                        }
                        
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
                        
                        annot.SetNormalAppearance(appearance.GetPdfObject());
                        pdfPage.AddAnnotation(annot);
                    }

                    foreach (var tb in pageData.Texts)
                    {
                        if (string.IsNullOrWhiteSpace(tb.Text)) continue;
                        
                        var color = (tb.Foreground as SolidColorBrush).Color;
                        var pdfColor = new iText.Kernel.Colors.DeviceRgb(color.R, color.G, color.B);
                        
                        double x = Canvas.GetLeft(tb);
                        double y = Canvas.GetTop(tb);
                        double w = tb.ActualWidth > 0 ? tb.ActualWidth : tb.Width;
                        double h = tb.ActualHeight > 0 ? tb.ActualHeight : (tb.MinHeight > 0 ? tb.MinHeight : 40);
                        if (double.IsNaN(w)) w = 200;
                        if (double.IsNaN(h)) h = 40;
                        
                        var fontPath = ToolState.GetFontPath(tb.FontFamily.Source);
                        iText.Kernel.Font.PdfFont font = null;
                        try {
                            font = iText.Kernel.Font.PdfFontFactory.CreateFont(fontPath, iText.IO.Font.PdfEncodings.IDENTITY_H);
                        } catch {
                            // Fallback if the font file is invalid or unsupported by iText
                            try {
                                font = iText.Kernel.Font.PdfFontFactory.CreateFont("c:\\windows\\fonts\\malgun.ttf", iText.IO.Font.PdfEncodings.IDENTITY_H);
                            } catch {
                                font = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                            }
                        }
                        
                        float fontSize = (float)(tb.FontSize / 2.0) * 0.75f;
                        string[] lines = tb.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        
                        float maxTextWidth = 0;
                        foreach(var line in lines) {
                            float lw = font.GetWidth(line, fontSize);
                            if (lw > maxTextWidth) maxTextWidth = lw;
                        }
                        
                        float cropW = pdfPage.GetCropBox().GetWidth();
                        float cropH = pdfPage.GetCropBox().GetHeight();
                        
                        float vx = (float)(x / 2.0) * 0.75f;
                        float vy = (float)(y / 2.0) * 0.75f;
                        float vw = Math.Max((float)(w / 2.0) * 0.75f, maxTextWidth + 20f);
                        float textVH = (lines.Length * fontSize * 1.2f) + fontSize;
                        float vh = Math.Max((float)(h / 2.0) * 0.75f, textVH + 20f);
                        
                        float minX = 0, minY = 0, maxX = 0, maxY = 0;
                        int rotation = pdfPage.GetRotation();
                        
                        if (rotation == 0) {
                            minX = vx; maxY = cropH - vy;
                            maxX = vx + vw; minY = cropH - (vy + vh);
                        } else if (rotation == 90) {
                            minX = vy; maxY = vx + vw;
                            maxX = vy + vh; minY = vx;
                        } else if (rotation == 180) {
                            minX = cropW - (vx + vw); maxY = vy + vh;
                            maxX = cropW - vx; minY = vy;
                        } else if (rotation == 270) {
                            minX = cropW - (vy + vh); maxY = cropH - vx;
                            maxX = cropW - vy; minY = cropH - (vx + vw);
                        }
                        
                        var cropBox = pdfPage.GetCropBox();
                        minX += cropBox.GetLeft();
                        maxX += cropBox.GetLeft();
                        minY += cropBox.GetBottom();
                        maxY += cropBox.GetBottom();
                        
                        var rect = new iText.Kernel.Geom.Rectangle(minX, minY, maxX - minX, maxY - minY);
                        var annot = new iText.Kernel.Pdf.Annot.PdfFreeTextAnnotation(rect, new iText.Kernel.Pdf.PdfString(tb.Text));
                        annot.SetFlags(iText.Kernel.Pdf.Annot.PdfAnnotation.PRINT);
                        
                        var appearance = new iText.Kernel.Pdf.Xobject.PdfFormXObject(rect);
                        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(appearance, pdfDoc);
                        
                        canvas.BeginText();
                        canvas.SetFontAndSize(font, fontSize);
                        canvas.SetFillColor(pdfColor);
                        
                        for (int l = 0; l < lines.Length; l++)
                        {
                            float linePy = maxY - fontSize - 5 - (l * fontSize * 1.2f);
                            if (rotation == 0)
                                canvas.SetTextMatrix(1, 0, 0, 1, minX + 5, linePy);
                            else if (rotation == 90)
                                canvas.SetTextMatrix(0, -1, 1, 0, minX + 5 + (l * fontSize * 1.2f) + fontSize, maxY - 5);
                            else if (rotation == 180)
                                canvas.SetTextMatrix(-1, 0, 0, -1, maxX - 5, minY + 5 + (l * fontSize * 1.2f) + fontSize);
                            else if (rotation == 270)
                                canvas.SetTextMatrix(0, 1, -1, 0, maxX - 5 - (l * fontSize * 1.2f) - fontSize, minY + 5);
                                
                            canvas.ShowText(lines[l]);
                        }
                        canvas.EndText();
                        
                        annot.SetNormalAppearance(appearance.GetPdfObject());
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterText"), iText.Kernel.Pdf.PdfBoolean.TRUE);
                        // Store metadata for round-trip editing
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterTextColor"), new iText.Kernel.Pdf.PdfArray(new float[] {
                            color.R / 255f, color.G / 255f, color.B / 255f
                        }));
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterFontSize"), new iText.Kernel.Pdf.PdfNumber((float)(tb.FontSize / 2.0) * 0.75f));
                        annot.Put(new iText.Kernel.Pdf.PdfName("PDFaNoterFontFamily"), new iText.Kernel.Pdf.PdfString(tb.FontFamily.Source));
                        
                        pdfPage.AddAnnotation(annot);
                    }
                }
                pdfDoc.Close();
                outputBytes = outMs.ToArray();
            }

            // 3. Write directly to destFile without temporary cross-volume moves
            using (var destStream = await destFile.OpenStreamForWriteAsync())
            {
                destStream.SetLength(0);
                await destStream.WriteAsync(outputBytes, 0, outputBytes.Length);
                await destStream.FlushAsync();
            }
            
            if (updateSourceFile)
            {
                _sourceFile = destFile;
                History.HasUnsavedChanges = false;
            }
            _workingFile = destFile;
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e) => History.Undo();
        private void BtnRedo_Click(object sender, RoutedEventArgs e) => History.Redo();

        private void UncheckAllExcept(ToggleButton activeBtn)
        {
            foreach (var btn in _penButtons) if (btn != activeBtn) btn.IsChecked = false;
            foreach (var btn in _highlighterButtons) if (btn != activeBtn) btn.IsChecked = false;
            if (BtnEraser != activeBtn) BtnEraser.IsChecked = false;
            if (BtnText != activeBtn) BtnText.IsChecked = false;
            if (BtnLasso != activeBtn) BtnLasso.IsChecked = false;
        }

        private void Pen_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            int index = int.Parse(btn.Tag.ToString());
            if (ToolState.CurrentMode == ToolMode.Pen && ToolState.CurrentPenSlot == index) { btn.IsChecked = true; btn.ContextFlyout?.ShowAt(btn); }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Pen; ToolState.CurrentPenSlot = index; UpdateToolSelection(); }
        }

        private void Highlighter_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            int index = int.Parse(btn.Tag.ToString());
            bool isCurrent = (ToolState.CurrentMode == ToolMode.Highlighter || ToolState.CurrentMode == ToolMode.TextHighlighter) && ToolState.CurrentHighlighterSlot == index;
            if (isCurrent)
            {
                btn.IsChecked = true;
                btn.ContextFlyout?.ShowAt(btn);
            }
            else
            {
                UncheckAllExcept(btn);
                btn.IsChecked = true;
                ToolState.CurrentHighlighterSlot = index;
                if (ToolState.HighlighterSnapToText[index])
                {
                    ToolState.CurrentMode = ToolMode.TextHighlighter;
                }
                else
                {
                    ToolState.CurrentMode = ToolMode.Highlighter;
                }
                UpdateToolSelection();
            }
        }

        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (ToolState.CurrentMode == ToolMode.Eraser) { btn.IsChecked = true; btn.ContextFlyout?.ShowAt(btn); }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Eraser; UpdateToolSelection(); }
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (ToolState.CurrentMode == ToolMode.Text) { btn.IsChecked = true; btn.ContextFlyout?.ShowAt(btn); }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Text; UpdateToolSelection(); }
        }
        
        private void Lasso_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (ToolState.CurrentMode == ToolMode.Lasso) { btn.IsChecked = true; }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Lasso; UpdateToolSelection(); }
        }

        private void UpdateToolSelection()
        {
            if (BtnStraightLine != null)
            {
                BtnStraightLine.IsChecked = ToolState.CurrentStraightLineSnap;
                BtnStraightLine.IsEnabled = ToolState.CurrentMode == ToolMode.Pen || ToolState.CurrentMode == ToolMode.Highlighter || ToolState.CurrentMode == ToolMode.TextHighlighter;
            }
            if (BtnSnapToText != null)
            {
                BtnSnapToText.IsChecked = ToolState.CurrentSnapToText;
                BtnSnapToText.IsEnabled = ToolState.CurrentMode == ToolMode.Highlighter || ToolState.CurrentMode == ToolMode.TextHighlighter;
            }
        }

        private void ToggleOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnStraightLine)
            {
                bool isChecked = BtnStraightLine.IsChecked == true;
                ToolState.CurrentStraightLineSnap = isChecked;
                if (isChecked && (ToolState.CurrentMode == ToolMode.Highlighter || ToolState.CurrentMode == ToolMode.TextHighlighter))
                {
                    ToolState.CurrentSnapToText = false;
                    ToolState.CurrentMode = ToolMode.Highlighter;
                }
                ToolState.Save();
                UpdateToolSelection();
            }
            else if (sender == BtnSnapToText)
            {
                bool isChecked = BtnSnapToText.IsChecked == true;
                ToolState.CurrentSnapToText = isChecked;
                if (isChecked)
                {
                    ToolState.CurrentStraightLineSnap = false;
                    ToolState.CurrentMode = ToolMode.TextHighlighter;
                }
                else
                {
                    ToolState.CurrentMode = ToolMode.Highlighter;
                }
                ToolState.Save();
                UpdateToolSelection();
            }
        }
        
        private void MenuMouseDraw_Click(object sender, RoutedEventArgs e) { ToolState.MouseDrawEnabled = MenuMouseDraw.IsChecked; ToolState.Save(); }

        private void ApplyToolbarDockPosition(string pos)
        {
            ToolbarBorder.ClearValue(Grid.RowProperty); ToolbarBorder.ClearValue(Grid.ColumnProperty);
            bool isVertical = (pos == "Left" || pos == "Right");
            if (pos == "Top") { Grid.SetRow(ToolbarBorder, 0); Grid.SetColumn(ToolbarBorder, 1); ToolbarStack.Orientation = Orientation.Horizontal; ToolbarStack.HorizontalAlignment = HorizontalAlignment.Center; ToolbarStack.VerticalAlignment = VerticalAlignment.Center; }
            else if (pos == "Bottom") { Grid.SetRow(ToolbarBorder, 2); Grid.SetColumn(ToolbarBorder, 1); ToolbarStack.Orientation = Orientation.Horizontal; ToolbarStack.HorizontalAlignment = HorizontalAlignment.Center; ToolbarStack.VerticalAlignment = VerticalAlignment.Center; }
            else if (pos == "Left") { Grid.SetRow(ToolbarBorder, 1); Grid.SetColumn(ToolbarBorder, 0); ToolbarStack.Orientation = Orientation.Vertical; ToolbarStack.VerticalAlignment = VerticalAlignment.Center; ToolbarStack.HorizontalAlignment = HorizontalAlignment.Center; }
            else if (pos == "Right") { Grid.SetRow(ToolbarBorder, 1); Grid.SetColumn(ToolbarBorder, 2); ToolbarStack.Orientation = Orientation.Vertical; ToolbarStack.VerticalAlignment = VerticalAlignment.Center; ToolbarStack.HorizontalAlignment = HorizontalAlignment.Center; }

            foreach (var child in ToolbarStack.Children)
            {
                if (child is FrameworkElement fe && fe.Tag as string == "ToolbarSeparator")
                {
                    if (isVertical)
                    {
                        fe.Width = 24;
                        fe.Height = 1;
                        fe.Margin = new Thickness(4, 2, 4, 2);
                    }
                    else
                    {
                        fe.Width = 1;
                        fe.Height = 20;
                        fe.Margin = new Thickness(2, 4, 2, 4);
                    }
                }
            }

            if (SettingsMenu != null) {
                foreach (var item in SettingsMenu.Items) {
                    if (item is RadioMenuFlyoutItem radio && radio.GroupName == "Dock" && radio.Tag.ToString() == pos) radio.IsChecked = true;
                }
            }
        }

        private void ToolbarPos_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string pos = item.Tag.ToString();
            ToolState.ToolbarDockPosition = pos;
            ToolState.Save();
            ApplyToolbarDockPosition(pos);
        }

        private void ApplyScrollMode(string mode)
        {
            bool isHoriz = (mode == "Horizontal");
            if (isHoriz) { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Orientation='Horizontal' Spacing='5' VerticalAlignment='Center' /></ItemsPanelTemplate>"); }
            else { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Orientation='Vertical' Spacing='5' HorizontalAlignment='Center' /></ItemsPanelTemplate>"); }

            if (SettingsMenu != null) {
                foreach (var item in SettingsMenu.Items) {
                    if (item is RadioMenuFlyoutItem radio && radio.GroupName == "Scroll" && radio.Tag.ToString() == mode) radio.IsChecked = true;
                }
            }

            ApplyFitToWidth();
        }

        private void ScrollMode_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string mode = item.Tag.ToString();
            ToolState.ScrollMode = mode;
            ToolState.Save();
            ApplyScrollMode(mode);
        }

        private void PenThicknessSlider_Changed(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (sender is Slider slider && slider.Tag != null && !double.IsNaN(args.NewValue))
            {
                int index = int.Parse(slider.Tag.ToString());
                ToolState.PenThicknesses[index] = args.NewValue;
                ToolState.Save();

                if (slider.Parent is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is NumberBox numBox && Math.Abs(numBox.Value - args.NewValue) > 0.01)
                        {
                            numBox.Value = args.NewValue;
                        }
                    }
                }
            }
        }

        private void PenThickness_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Tag != null && !double.IsNaN(args.NewValue))
            {
                int index = int.Parse(sender.Tag.ToString());
                ToolState.PenThicknesses[index] = args.NewValue;
                ToolState.Save();

                if (sender.Parent is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is Slider slider && Math.Abs(slider.Value - args.NewValue) > 0.01)
                        {
                            slider.Value = args.NewValue;
                        }
                    }
                }
            }
        }

        private void HighlighterThicknessSlider_Changed(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (sender is Slider slider && slider.Tag != null && !double.IsNaN(args.NewValue))
            {
                int index = int.Parse(slider.Tag.ToString());
                ToolState.HighlighterThicknesses[index] = args.NewValue;
                ToolState.Save();

                if (slider.Parent is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is NumberBox numBox && Math.Abs(numBox.Value - args.NewValue) > 0.01)
                        {
                            numBox.Value = args.NewValue;
                        }
                    }
                }
            }
        }

        private void HighlighterThickness_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Tag != null && !double.IsNaN(args.NewValue))
            {
                int index = int.Parse(sender.Tag.ToString());
                ToolState.HighlighterThicknesses[index] = args.NewValue;
                ToolState.Save();

                if (sender.Parent is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is Slider slider && Math.Abs(slider.Value - args.NewValue) > 0.01)
                        {
                            slider.Value = args.NewValue;
                        }
                    }
                }
            }
        }

        private void PenFlyout_Opening(object sender, object e)
        {
            if (sender is Flyout flyout && flyout.Content is StackPanel panel)
            {
                int index = -1;
                foreach (var child in panel.Children)
                {
                    if (child is Grid grid)
                    {
                        foreach (var gridChild in grid.Children)
                        {
                            if (gridChild is FrameworkElement fe && fe.Tag != null && int.TryParse(fe.Tag.ToString(), out int i))
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index >= 0 && index < ToolState.PenThicknesses.Length)
                        {
                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is Slider slider) slider.Value = ToolState.PenThicknesses[index];
                                if (gridChild is NumberBox numBox) numBox.Value = ToolState.PenThicknesses[index];
                            }
                        }
                    }
                    else if (child is Microsoft.UI.Xaml.Controls.ColorPicker picker && index >= 0 && index < ToolState.PenColors.Length)
                    {
                        picker.Color = ToolState.PenColors[index].Color;
                    }
                }
            }
        }

        private void HighlighterFlyout_Opening(object sender, object e)
        {
            if (sender is Flyout flyout && flyout.Content is StackPanel panel)
            {
                int index = -1;
                foreach (var child in panel.Children)
                {
                    if (child is Grid grid)
                    {
                        foreach (var gridChild in grid.Children)
                        {
                            if (gridChild is FrameworkElement fe && fe.Tag != null && int.TryParse(fe.Tag.ToString(), out int i))
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index >= 0 && index < ToolState.HighlighterThicknesses.Length)
                        {
                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is Slider slider) slider.Value = ToolState.HighlighterThicknesses[index];
                                if (gridChild is NumberBox numBox) numBox.Value = ToolState.HighlighterThicknesses[index];
                            }
                        }
                    }
                    else if (child is Microsoft.UI.Xaml.Controls.ColorPicker picker && index >= 0 && index < ToolState.HighlighterColors.Length)
                    {
                        picker.Color = ToolState.HighlighterColors[index].Color;
                    }
                }
            }
        }

        private void EraserFlyout_Opening(object sender, object e)
        {
            if (sender is Flyout flyout && flyout.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is ComboBox combo)
                    {
                        combo.SelectedIndex = ToolState.EraserMode == EraserType.Stroke ? 0 : 1;
                    }
                    else if (child is Slider slider)
                    {
                        slider.Value = ToolState.EraserThickness;
                    }
                }
            }
        }

                private void PdfScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateContainerSize();
            UpdateCurrentPageNumber();
        }

        private void UpdateCurrentPageNumber(bool showToast = true)
        {
            if (_pages == null || _pages.Count == 0)
            {
                UpdatePageDisplay(1, 1, false);
                return;
            }

            int totalPages = _pages.Count;
            double viewportCenterY = PdfScrollViewer.ViewportHeight / 2.0;
            double viewportCenterX = PdfScrollViewer.ViewportWidth / 2.0;

            int closestPageIndex = 0;
            double minDistance = double.MaxValue;

            for (int i = 0; i < _pages.Count; i++)
            {
                var page = _pages[i];
                try
                {
                    var transform = page.TransformToVisual(PdfScrollViewer);
                    var pt = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
                    double distance;
                    if (ToolState.ScrollMode == "Horizontal")
                    {
                        double pageW = page.ActualWidth > 0 ? page.ActualWidth : page.Width;
                        double pageCenter = pt.X + pageW / 2.0;
                        distance = Math.Abs(pageCenter - viewportCenterX);
                    }
                    else
                    {
                        double pageH = page.ActualHeight > 0 ? page.ActualHeight : page.Height;
                        double pageCenter = pt.Y + pageH / 2.0;
                        distance = Math.Abs(pageCenter - viewportCenterY);
                    }

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPageIndex = i;
                    }
                }
                catch
                {
                }
            }

            int newPage = closestPageIndex + 1;
            if (newPage != _currentPageNumber)
            {
                _currentPageNumber = newPage;
                UpdatePageDisplay(_currentPageNumber, totalPages, showToast);
            }
            else
            {
                UpdatePageDisplay(_currentPageNumber, totalPages, false);
            }
        }

        private void UpdatePageDisplay(int current, int total, bool showToast)
        {
            string pageStr = $"{current} / {total}";
            if (TxtPageNumber != null)
            {
                TxtPageNumber.Text = pageStr;
            }

            if (showToast && PageToast != null && PageToastText != null)
            {
                PageToastText.Text = pageStr;
                PageToast.Opacity = 1.0;

                if (_pageToastTimer == null)
                {
                    _pageToastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
                    _pageToastTimer.Tick += (s, e) =>
                    {
                        _pageToastTimer.Stop();
                        if (PageToast != null) PageToast.Opacity = 0.0;
                    };
                }
                else
                {
                    _pageToastTimer.Stop();
                }
                _pageToastTimer.Start();
            }
        }

        private void TextFontSize_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(args.NewValue))
            {
                ToolState.TextFontSize = args.NewValue;
                ToolState.Save();
            }
        }

        private void TextFontFamily_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (TextFontFamilyCombo.SelectedItem != null)
            {
                ToolState.TextFontFamily = TextFontFamilyCombo.SelectedItem.ToString();
                ToolState.Save();
            }
        }

        private void TextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            ToolState.TextColor = new SolidColorBrush(args.NewColor);
            if (IndText != null) IndText.Background = ToolState.TextColor;
            ToolState.Save();
        }

        private void EraserThickness_Changed(object sender, RangeBaseValueChangedEventArgs e) { ToolState.EraserThickness = e.NewValue; }
        
        private void EraserType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (EraserTypeCombo.SelectedIndex == 0) ToolState.EraserMode = EraserType.Stroke;
            else ToolState.EraserMode = EraserType.Pixel;
        }

        private void PenColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (sender.Tag != null)
            {
                int index = int.Parse(sender.Tag.ToString());
                ToolState.PenColors[index] = new SolidColorBrush(args.NewColor);
                var indicator = (Border)this.FindName("IndPen" + index);
                if (indicator != null) indicator.Background = ToolState.PenColors[index];
                ToolState.Save();
            }
        }

        private void HighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (sender.Tag != null)
            {
                int index = int.Parse(sender.Tag.ToString());
                var c = args.NewColor;
                ToolState.HighlighterColors[index] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(c.A, c.R, c.G, c.B));
                var indicator = (Border)this.FindName("IndHighlighter" + index);
                if (indicator != null) indicator.Background = ToolState.HighlighterColors[index];
                ToolState.Save();
            }
        }

        

                

        

        
        private System.Collections.Generic.List<Windows.Foundation.Point> GetSmoothedPoints(System.Collections.Generic.IList<Windows.Foundation.Point> points)
        {
            var smoothedPoints = new System.Collections.Generic.List<Windows.Foundation.Point>();
            if (points.Count < 3)
            {
                foreach (var p in points) smoothedPoints.Add(p);
                return smoothedPoints;
            }

            smoothedPoints.Add(points[0]);
            for (int k = 0; k < points.Count - 2; k++)
            {
                var p0 = (k == 0) ? points[0] : new Windows.Foundation.Point((points[k].X + points[k + 1].X) / 2.0, (points[k].Y + points[k + 1].Y) / 2.0);
                var p1 = points[k + 1];
                var p2 = (k == points.Count - 3) ? points[k + 2] : new Windows.Foundation.Point((points[k + 1].X + points[k + 2].X) / 2.0, (points[k + 1].Y + points[k + 2].Y) / 2.0);

                double dist = Math.Sqrt(Math.Pow(p2.X - p0.X, 2) + Math.Pow(p2.Y - p0.Y, 2));
                int segments = Math.Max(2, (int)(dist / 2.0));

                for (int step = 1; step <= segments; step++)
                {
                    double t = (double)step / segments;
                    double x = Math.Pow(1 - t, 2) * p0.X + 2 * (1 - t) * t * p1.X + t * t * p2.X;
                    double y = Math.Pow(1 - t, 2) * p0.Y + 2 * (1 - t) * t * p1.Y + t * t * p2.Y;
                    smoothedPoints.Add(new Windows.Foundation.Point(x, y));
                }
            }
            return smoothedPoints;
        }
        private bool _isFitToWidth = true;
        private double _currentPageWidth = 0;

        // Panning state
        private bool _isPanning = false;
        private bool _isMidButtonPanning = false;
        private uint _panPointerId;
        private Windows.Foundation.Point _panStartPointer;
        private double _panStartOffsetX;
        private double _panStartOffsetY;

        private bool IsContentOverflowingHorizontally()
        {
            return PdfScrollViewer.ScrollableWidth > 1;
        }

        private bool IsContentOverflowingVertically()
        {
            return PdfScrollViewer.ScrollableHeight > 1;
        }

        private bool IsContentOverflowing()
        {
            return IsContentOverflowingHorizontally() || IsContentOverflowingVertically();
        }

        private void BtnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            _isFitToWidth = true;
            ApplyFitToWidth();
        }

        private void ApplyFitToWidth()
        {
            if (_pages == null || _pages.Count == 0) return;
            
            if (Math.Abs(PdfScrollViewer.ZoomFactor - 1.0f) > 0.01f)
            {
                PdfScrollViewer.ChangeView(0, null, 1.0f, true);
            }

            double viewW = PdfScrollViewer.ViewportWidth;
            if (viewW <= 0) viewW = RootGrid.ColumnDefinitions[1].ActualWidth;
            if (viewW <= 0) viewW = this.ActualWidth;
            if (viewW <= 0) return;

            bool isHoriz = (ToolState.ScrollMode == "Horizontal");
            if (isHoriz)
            {
                double viewH = PdfScrollViewer.ViewportHeight;
                if (viewH <= 0) viewH = RootGrid.RowDefinitions[1].ActualHeight;
                if (viewH <= 0) viewH = this.ActualHeight;
                double targetH = Math.Max(200, viewH - 24);
                foreach (var page in _pages)
                {
                    page.SetDisplayHeight(targetH);
                }
            }
            else
            {
                double targetW = Math.Max(200, viewW - 24);
                _currentPageWidth = targetW;
                foreach (var page in _pages)
                {
                    page.SetDisplayWidth(targetW);
                }
            }
        }

        
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

                private void PdfScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateContainerSize();
            if (_isFitToWidth && e.NewSize.Width > 0)
            {
                ApplyFitToWidth();
            }
        }

        private void PdfScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var keyState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            bool isCtrl = (keyState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
            if (isCtrl)
            {
                var pt = e.GetCurrentPoint(PdfScrollViewer);
                int delta = pt.Properties.MouseWheelDelta;
                if (delta != 0)
                {
                    _isFitToWidth = false;

                    // If ZoomFactor was modified by pinch gesture on Surface, fold it into _currentPageWidth and reset ZoomFactor
                    if (Math.Abs(PdfScrollViewer.ZoomFactor - 1.0f) > 0.01f)
                    {
                        if (_currentPageWidth <= 0 && _pages.Count > 0) _currentPageWidth = _pages[0].Width;
                        _currentPageWidth *= PdfScrollViewer.ZoomFactor;
                        PdfScrollViewer.ChangeView(null, null, 1.0f, true);
                    }

                    double factor = (delta > 0) ? 1.15 : 0.87;
                    double currentW = _currentPageWidth;
                    if (currentW <= 0 && _pages.Count > 0) currentW = _pages[0].Width;
                    if (currentW <= 0) currentW = 1000;

                    double newW = Math.Clamp(currentW * factor, 250, 10000);
                    factor = newW / currentW;

                    double cursorX = pt.Position.X;
                    double cursorY = pt.Position.Y;

                    double targetOffsetX = 0;
                    double targetOffsetY = 0;

                    try
                    {
                        var transform = PdfPagesControl.TransformToVisual(PdfScrollViewer);
                        var contentOrigin = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

                        double relX = cursorX - contentOrigin.X;
                        double relY = cursorY - contentOrigin.Y;

                        targetOffsetX = Math.Max(0, relX * factor - cursorX);
                        targetOffsetY = Math.Max(0, relY * factor - cursorY);
                    }
                    catch
                    {
                        targetOffsetX = Math.Max(0, (PdfScrollViewer.HorizontalOffset + cursorX) * factor - cursorX);
                        targetOffsetY = Math.Max(0, (PdfScrollViewer.VerticalOffset + cursorY) * factor - cursorY);
                    }

                    _currentPageWidth = newW;
                    foreach (var page in _pages)
                    {
                        page.SetDisplayWidth(newW);
                    }

                    PdfPagesControl.UpdateLayout();
                    PdfScrollViewer.ChangeView(targetOffsetX, targetOffsetY, 1.0f, true);

                    DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                    {
                        PdfScrollViewer.ChangeView(targetOffsetX, targetOffsetY, 1.0f, true);
                    });

                    e.Handled = true;
                }
            }
        }

        private void PdfScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isPanning) return;

            var pt = e.GetCurrentPoint(PdfScrollViewer);
            bool isMidButton = pt.Properties.IsMiddleButtonPressed;
            bool isLeftButton = pt.Properties.IsLeftButtonPressed;

            // Middle button: pan in any mode when zoomed in
            if (isMidButton && ToolState.MouseDrawEnabled && IsContentOverflowing())
            {
                _isPanning = true;
                _isMidButtonPanning = true;
                _panPointerId = pt.PointerId;
                _panStartPointer = pt.Position;
                _panStartOffsetX = PdfScrollViewer.HorizontalOffset;
                _panStartOffsetY = PdfScrollViewer.VerticalOffset;
                PdfScrollViewer.CapturePointer(e.Pointer);
                e.Handled = true;
                return;
            }

            // Left button: pan with MOUSE when mouse mode is OFF (mouse drag acts as scroll)
            // (Note: Touch on Surface/touchscreen natively scrolls and pinches via DirectManipulation;
            // only capture when PointerDeviceType == Mouse!)
            if (isLeftButton && !ToolState.MouseDrawEnabled 
                && pt.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse
                && IsContentOverflowing())
            {
                _isPanning = true;
                _isMidButtonPanning = false;
                _panPointerId = pt.PointerId;
                _panStartPointer = pt.Position;
                _panStartOffsetX = PdfScrollViewer.HorizontalOffset;
                _panStartOffsetY = PdfScrollViewer.VerticalOffset;
                PdfScrollViewer.CapturePointer(e.Pointer);
                e.Handled = true;
                return;
            }
        }

        private void PdfScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isPanning) return;
            var pt = e.GetCurrentPoint(PdfScrollViewer);
            if (pt.PointerId != _panPointerId) return;

            double dx = pt.Position.X - _panStartPointer.X;
            double dy = pt.Position.Y - _panStartPointer.Y;
            double newX = Math.Max(0, _panStartOffsetX - dx);
            double newY = Math.Max(0, _panStartOffsetY - dy);
            PdfScrollViewer.ChangeView(newX, newY, null, true);
            e.Handled = true;
        }

        private void PdfScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isPanning) return;
            var pt = e.GetCurrentPoint(PdfScrollViewer);
            if (pt.PointerId != _panPointerId) return;
            _isPanning = false;
            PdfScrollViewer.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }
}