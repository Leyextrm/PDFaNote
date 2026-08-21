using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        private PdfDocument _pdfDocument;
        private ObservableCollection<PdfPageView> _pages = new ObservableCollection<PdfPageView>();
        public CommandHistory History { get; private set; } = new CommandHistory();
        
        private List<ToggleButton> _penButtons;
        private List<ToggleButton> _highlighterButtons;

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

            UpdateToolSelection();
        }

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
                var writer = new iText.Kernel.Pdf.PdfWriter(ms); writer.SetCloseStream(false);
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
        }

        public async Task SaveAsync(Windows.Storage.StorageFile destFile)
        {
            if (_sourceFile == null) return;
            
            bool isSameFile = (destFile.Path == _sourceFile.Path);
            Windows.Storage.StorageFile actualDestFile = destFile;
            
            if (isSameFile)
            {
                actualDestFile = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync("temp_save.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            }

            using (var sourceStream = await _sourceFile.OpenStreamForReadAsync())
            using (var destStream = await actualDestFile.OpenStreamForWriteAsync())
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

                        foreach (var pt in stroke.Points)
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
                        
                        annot.SetNormalAppearance(appearance.GetPdfObject());
                        pdfPage.AddAnnotation(annot);
                    }
                }
                pdfDoc.Close();
            }
            
            if (isSameFile)
            {
                await actualDestFile.MoveAndReplaceAsync(_sourceFile);
            }
            
            History.HasUnsavedChanges = false;
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e) => History.Undo();
        private void BtnRedo_Click(object sender, RoutedEventArgs e) => History.Redo();

        private void UncheckAllExcept(ToggleButton activeBtn)
        {
            foreach (var btn in _penButtons) if (btn != activeBtn) btn.IsChecked = false;
            foreach (var btn in _highlighterButtons) if (btn != activeBtn) btn.IsChecked = false;
            if (BtnEraser != activeBtn) BtnEraser.IsChecked = false;
        }

        private void Pen_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            int index = int.Parse(btn.Tag.ToString());
            if (ToolState.CurrentMode == ToolMode.Pen && ToolState.CurrentPenSlot == index) { btn.ContextFlyout?.ShowAt(btn); }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Pen; ToolState.CurrentPenSlot = index; UpdateToolSelection(); }
        }

        private void Highlighter_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            int index = int.Parse(btn.Tag.ToString());
            if (ToolState.CurrentMode == ToolMode.Highlighter && ToolState.CurrentHighlighterSlot == index) { btn.ContextFlyout?.ShowAt(btn); }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Highlighter; ToolState.CurrentHighlighterSlot = index; UpdateToolSelection(); }
        }

        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (ToolState.CurrentMode == ToolMode.Eraser) { btn.ContextFlyout?.ShowAt(btn); }
            else { UncheckAllExcept(btn); btn.IsChecked = true; ToolState.CurrentMode = ToolMode.Eraser; UpdateToolSelection(); }
        }

        private void UpdateToolSelection()
        {
            if (BtnStraightLine != null)
            {
                BtnStraightLine.IsChecked = ToolState.CurrentStraightLineSnap;
            }
        }

        private void ToggleOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnStraightLine)
            {
                ToolState.CurrentStraightLineSnap = BtnStraightLine.IsChecked == true;
            }
        }
        
        private void MenuMouseDraw_Click(object sender, RoutedEventArgs e) { ToolState.MouseDrawEnabled = MenuMouseDraw.IsChecked; }

        private void ToolbarPos_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string pos = item.Tag.ToString();
            ToolbarBorder.ClearValue(Grid.RowProperty); ToolbarBorder.ClearValue(Grid.ColumnProperty);
            if (pos == "Top") { Grid.SetRow(ToolbarBorder, 0); Grid.SetColumn(ToolbarBorder, 1); ToolbarStack.Orientation = Orientation.Horizontal; }
            else if (pos == "Bottom") { Grid.SetRow(ToolbarBorder, 2); Grid.SetColumn(ToolbarBorder, 1); ToolbarStack.Orientation = Orientation.Horizontal; }
            else if (pos == "Left") { Grid.SetRow(ToolbarBorder, 1); Grid.SetColumn(ToolbarBorder, 0); ToolbarStack.Orientation = Orientation.Vertical; }
            else if (pos == "Right") { Grid.SetRow(ToolbarBorder, 1); Grid.SetColumn(ToolbarBorder, 2); ToolbarStack.Orientation = Orientation.Vertical; }
        }

        private void ScrollMode_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            bool isHoriz = (item.Tag.ToString() == "Horizontal");
            if (isHoriz) { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Orientation='Horizontal' Spacing='20' VerticalAlignment='Center' /></ItemsPanelTemplate>"); }
            else { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Orientation='Vertical' Spacing='20' HorizontalAlignment='Center' /></ItemsPanelTemplate>"); }
        }

        private void PenThickness_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Tag != null && !double.IsNaN(args.NewValue))
                ToolState.PenThicknesses[int.Parse(sender.Tag.ToString())] = args.NewValue;
        }

        private void HighlighterThickness_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Tag != null && !double.IsNaN(args.NewValue))
                ToolState.HighlighterThicknesses[int.Parse(sender.Tag.ToString())] = args.NewValue;
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
            }
        }

        private void HighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (sender.Tag != null)
            {
                int index = int.Parse(sender.Tag.ToString());
                var c = args.NewColor;
                ToolState.HighlighterColors[index] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, c.R, c.G, c.B));
                var indicator = (Border)this.FindName("IndHighlighter" + index);
                if (indicator != null) indicator.Background = ToolState.HighlighterColors[index];
            }
        }
    }
}














