using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Input;

namespace PDFaNoter
{
    public sealed partial class PdfPageView : UserControl
    {
        private PdfPageData _pageData;
        public PdfPageData PageData => _pageData;
        private bool _isDrawing = false;
        private Polyline _currentStroke;
        private CommandHistory _history;
        private uint _activePointerId;
        private Point _lastPoint;
        private List<IUndoableCommand> _currentEraserSession = new List<IUndoableCommand>();
        
        private Microsoft.UI.Xaml.Shapes.Polygon _lassoPolygon;
        private bool _isLassoing;
        private List<UIElement> _selectedElements = new List<UIElement>();
        private Microsoft.UI.Xaml.Shapes.Rectangle _selectionBox;
        private List<PdfTextWord> _readingOrderLetters = null;
        private int _textSelectionStartIndex = -1;
        private int _textSelectionEndIndex = -1;
        private List<Polyline> _currentTextHighlightStrokes = null;
        private bool _isDraggingSelection;
        private Point _lastDragPoint;
        private double _totalDragX;
        private double _totalDragY;
        private double _currentDisplayWidth = 0;
        public double BaseWidth { get; private set; } = 0;
        public double BaseHeight { get; private set; } = 0;
        public double AspectRatio => (BaseWidth > 0 && BaseHeight > 0) ? (BaseHeight / BaseWidth) : 1.414;

        public void SetDisplayWidth(double width)
        {
            if (width <= 0) return;
            _currentDisplayWidth = width;
            double h = width * AspectRatio;
            this.Width = width;
            this.Height = h;
            if (PageViewbox != null)
            {
                PageViewbox.Width = width;
                PageViewbox.Height = h;
            }
        }

        public void SetDisplayHeight(double height)
        {
            if (height <= 0) return;
            double w = height / AspectRatio;
            this.Width = w;
            this.Height = height;
            if (PageViewbox != null)
            {
                PageViewbox.Width = w;
                PageViewbox.Height = height;
            }
        }

        public PdfPageView()
        {
            this.InitializeComponent();
        }

        

        public async void LoadPage(PdfPageData data, CommandHistory history)
        {
            _pageData = data;
            _history = history;
            
            // Clear canvas
                        InkCanvas.Children.Clear();
            
            if (_pageData.ExtractedStrokes != null && _pageData.ExtractedStrokes.Count > 0)
            {
                foreach (var extracted in _pageData.ExtractedStrokes)
                {
                    if (extracted.Points.Count < 2) continue;
                    var polyline = new Microsoft.UI.Xaml.Shapes.Polyline
                    {
                        Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(extracted.Color),
                        StrokeThickness = extracted.Thickness,
                        StrokeLineJoin = Microsoft.UI.Xaml.Media.PenLineJoin.Round,
                        StrokeStartLineCap = Microsoft.UI.Xaml.Media.PenLineCap.Round,
                        StrokeEndLineCap = Microsoft.UI.Xaml.Media.PenLineCap.Round,
                        Tag = "IsSaved"
                    };
                    var points = new Microsoft.UI.Xaml.Media.PointCollection();
                    foreach (var pt in extracted.Points)
                    {
                        points.Add(pt);
                    }
                    polyline.Points = points;
                    _pageData.Strokes.Add(polyline);
                }
                _pageData.ExtractedStrokes.Clear();
            }

            foreach (var stroke in _pageData.Strokes)
            {
                InkCanvas.Children.Add(stroke);
            }

            foreach (var tb in _pageData.Texts)
            {
                if (tb.Parent != null)
                {
                    ((Panel)tb.Parent).Children.Remove(tb);
                }
                InkCanvas.Children.Add(tb);
            }

            // Restore saved text annotations as editable TextBoxes
            if (_pageData.ExtractedTexts != null && _pageData.ExtractedTexts.Count > 0)
            {
                foreach (var extracted in _pageData.ExtractedTexts)
                {
                    var textBox = new TextBox
                    {
                        MinWidth = 50,
                        MinHeight = 40,
                        Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                        BorderThickness = new Thickness(0),
                        BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                        Foreground = new SolidColorBrush(extracted.Color),
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.NoWrap,
                        FontSize = extracted.FontSize,
                        FontFamily = new FontFamily(extracted.FontFamily ?? "Malgun Gothic"),
                        IsSpellCheckEnabled = false,
                        Text = extracted.Text,
                    };
                    Canvas.SetLeft(textBox, extracted.CanvasLeft);
                    Canvas.SetTop(textBox, extracted.CanvasTop);
                    textBox.LostFocus += (s, args) =>
                    {
                        if (string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            InkCanvas.Children.Remove(textBox);
                            _pageData.Texts.Remove(textBox);
                        }
                        else
                        {
                            textBox.BorderThickness = new Thickness(0);
                        }
                    };
                    textBox.GotFocus += (s, args) =>
                    {
                        textBox.BorderThickness = new Thickness(1);
                    };
                    InkCanvas.Children.Add(textBox);
                    _pageData.Texts.Add(textBox);
                }
                _pageData.ExtractedTexts.Clear();
            }

            using (var pdfPage = _pageData.Document.GetPage(_pageData.PageIndex))
            {
                BaseWidth = pdfPage.Size.Width * 2;
                BaseHeight = pdfPage.Size.Height * 2;

                using (var stream = new InMemoryRandomAccessStream())
                {
                    var renderOptions = new PdfPageRenderOptions();
                    renderOptions.DestinationWidth = (uint)BaseWidth;
                    renderOptions.DestinationHeight = (uint)BaseHeight;
                    
                    await pdfPage.RenderToStreamAsync(stream, renderOptions);
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    PdfImage.Source = bitmap;
                    
                    InkCanvas.Width = BaseWidth;
                    InkCanvas.Height = BaseHeight;
                    PdfImage.Width = BaseWidth;
                    PdfImage.Height = BaseHeight;
                    PageContainer.Width = BaseWidth;
                    PageContainer.Height = BaseHeight;
                }

                if (_currentDisplayWidth > 0)
                {
                    SetDisplayWidth(_currentDisplayWidth);
                }
                else
                {
                    SetDisplayWidth(BaseWidth);
                }
            }
        }

        public void RefreshWords()
        {
            _readingOrderLetters = null;
        }

        private void EnsureReadingOrderLetters()
        {
            if (_readingOrderLetters != null && _readingOrderLetters.Count > 0) return;
            if (_pageData == null || _pageData.Words == null || _pageData.Words.Count == 0)
            {
                return;
            }
            
            // In most PDFs, page.Letters is in content stream order, which naturally 
            // follows reading order (including columns). Manually grouping by Y 
            // destroys column separation, so we use the raw stream order.
            _readingOrderLetters = new List<PdfTextWord>(_pageData.Words);
        }

        private int GetClosestLetterIndex(Point pt)
        {
            EnsureReadingOrderLetters();
            if (_readingOrderLetters == null || _readingOrderLetters.Count == 0) return -1;
            
            int bestIdx = -1;
            double bestDist = double.MaxValue;
            double scale = 2.0 / 0.75;
            double canvasH = InkCanvas.Height;
            if (double.IsNaN(canvasH) || canvasH <= 0) canvasH = BaseHeight;
            
            for (int i = 0; i < _readingOrderLetters.Count; i++)
            {
                var w = _readingOrderLetters[i];
                double wLeft = w.Bounds.X * scale;
                double wRight = wLeft + w.Bounds.Width * scale;
                double wordTopPdf = w.Bounds.Y + w.Bounds.Height;
                double wTop = canvasH - (wordTopPdf * scale);
                double wBottom = canvasH - (w.Bounds.Y * scale);
                
                double cx = (wLeft + wRight) / 2.0;
                double cy = (wTop + wBottom) / 2.0;
                double dx = pt.X - cx;
                double dy = pt.Y - cy;
                double dist = dx * dx + dy * dy;
                
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(InkCanvas);
            
            // Touch on touchscreen/Surface is reserved for multi-touch gestures (pinch-to-zoom & panning)
            if (pt.PointerDeviceType == PointerDeviceType.Touch)
                return;

            // Middle button: don't handle here — let ScrollViewer handle panning
            if (pt.Properties.IsMiddleButtonPressed)
                return;
            if (ToolState.CurrentMode == ToolMode.Text)
            {
                var textBox = new TextBox
                {
                    MinWidth = 50,
                    MinHeight = 40,
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    Foreground = ToolState.TextColor,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.NoWrap,
                    FontSize = ToolState.TextFontSize,
                    FontFamily = new FontFamily(ToolState.TextFontFamily),
                    IsSpellCheckEnabled = false,
                };
                
                Canvas.SetLeft(textBox, pt.Position.X);
                Canvas.SetTop(textBox, pt.Position.Y);
                
                textBox.LostFocus += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        InkCanvas.Children.Remove(textBox);
                        _pageData.Texts.Remove(textBox);
                    }
                    else
                    {
                        textBox.BorderThickness = new Thickness(0);
                    }
                };
                
                textBox.GotFocus += (s, args) =>
                {
                    textBox.BorderThickness = new Thickness(1);
                };

                InkCanvas.Children.Add(textBox);
                _pageData.Texts.Add(textBox);
                textBox.Focus(FocusState.Programmatic);
                
                e.Handled = true;
                return;
            }
            if (_selectionBox != null)
            {
                double left = Canvas.GetLeft(_selectionBox);
                double top = Canvas.GetTop(_selectionBox);
                if (pt.Position.X >= left && pt.Position.X <= left + _selectionBox.Width && 
                    pt.Position.Y >= top && pt.Position.Y <= top + _selectionBox.Height)
                {
                    InkCanvas.ManipulationMode = ManipulationModes.None;
                    _isDraggingSelection = true;
                    _isDrawing = true;
                    _activePointerId = pt.PointerId;
                    _lastDragPoint = pt.Position;
                    _totalDragX = 0;
                    _totalDragY = 0;
                    InkCanvas.CapturePointer(e.Pointer);
                    e.Handled = true;
                    return;
                }
                else
                {
                    ClearSelection();
                }
            }

            if (pt.PointerDeviceType != PointerDeviceType.Pen && !ToolState.MouseDrawEnabled) return;
            if (!pt.IsInContact) return;

            InkCanvas.ManipulationMode = ManipulationModes.None;
            _isDrawing = true;
            _activePointerId = pt.PointerId;
            _lastPoint = pt.Position;
            InkCanvas.CapturePointer(e.Pointer);

            bool isLasso = ToolState.CurrentMode == ToolMode.Lasso || pt.Properties.IsBarrelButtonPressed;
            bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser || (ToolState.MouseDrawEnabled && pt.Properties.IsRightButtonPressed);

            if (isLasso)
            {
                _lassoPolygon = new Microsoft.UI.Xaml.Shapes.Polygon
                {
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                    StrokeThickness = 1,
                    StrokeDashArray = new Microsoft.UI.Xaml.Media.DoubleCollection { 4, 4 },
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(50, 0, 0, 255))
                };
                _lassoPolygon.Points.Add(pt.Position);
                InkCanvas.Children.Add(_lassoPolygon);
                _isLassoing = true;
            }
            else if (isEraser)
            {
                _currentEraserSession.Clear();
                Erase(pt.Position);
            }
            else if (ToolState.CurrentMode == ToolMode.TextHighlighter)
            {
                EnsureReadingOrderLetters();
                if (_readingOrderLetters != null && _readingOrderLetters.Count > 0)
                {
                    _textSelectionStartIndex = GetClosestLetterIndex(pt.Position);
                    _textSelectionEndIndex = _textSelectionStartIndex;
                    UpdateTextSelectionPreview();
                }
                else
                {
                    _currentStroke = new Polyline();
                    _currentStroke.StrokeLineJoin = PenLineJoin.Round;
                    _currentStroke.StrokeStartLineCap = PenLineCap.Square;
                    _currentStroke.StrokeEndLineCap = PenLineCap.Square;
                    _currentStroke.Stroke = ToolState.HighlighterColor;
                    _currentStroke.StrokeThickness = ToolState.HighlighterThickness;
                    _currentStroke.Points.Add(pt.Position);
                    _pageData.Strokes.Add(_currentStroke);
                    InkCanvas.Children.Add(_currentStroke);
                }
            }
            else
            {
                _currentStroke = new Polyline();
                _currentStroke.StrokeLineJoin = PenLineJoin.Round;
                _currentStroke.StrokeStartLineCap = PenLineCap.Round;
                _currentStroke.StrokeEndLineCap = PenLineCap.Round;
                
                if (ToolState.CurrentMode == ToolMode.Pen)
                {
                    _currentStroke.Stroke = ToolState.PenColor;
                    _currentStroke.StrokeThickness = ToolState.PenThickness;
                }
                else if (ToolState.CurrentMode == ToolMode.Highlighter)
                {
                    _currentStroke.Stroke = ToolState.HighlighterColor;
                    _currentStroke.StrokeThickness = ToolState.HighlighterThickness;
                    _currentStroke.StrokeStartLineCap = PenLineCap.Square;
                    _currentStroke.StrokeEndLineCap = PenLineCap.Square;
                }

                _currentStroke.Points.Add(pt.Position);
                _pageData.Strokes.Add(_currentStroke);
                InkCanvas.Children.Add(_currentStroke);
            }
            e.Handled = true;
        }

        private void InkCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(InkCanvas);
            
            if (ToolState.CurrentMode == ToolMode.TextHighlighter)
            {
                this.ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.IBeam);
            }
            else
            {
                this.ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
            }
            
            if (_isDraggingSelection)
            {
                if (!pt.IsInContact)
                {
                    InkCanvas_PointerReleased(sender, e);
                    return;
                }

                double dx = pt.Position.X - _lastDragPoint.X;
                double dy = pt.Position.Y - _lastDragPoint.Y;
                _totalDragX += dx;
                _totalDragY += dy;
                
                Canvas.SetLeft(_selectionBox, Canvas.GetLeft(_selectionBox) + dx);
                Canvas.SetTop(_selectionBox, Canvas.GetTop(_selectionBox) + dy);
                
                foreach (var elem in _selectedElements)
                {
                    if (elem is Polyline poly)
                    {
                        var newPoints = new Microsoft.UI.Xaml.Media.PointCollection();
                        for (int i = 0; i < poly.Points.Count; i++)
                        {
                            var p = poly.Points[i];
                            newPoints.Add(new Windows.Foundation.Point(p.X + dx, p.Y + dy));
                        }
                        poly.Points = newPoints;
                    }
                    else if (elem is TextBox tb)
                    {
                        Canvas.SetLeft(tb, Canvas.GetLeft(tb) + dx);
                        Canvas.SetTop(tb, Canvas.GetTop(tb) + dy);
                    }
                }
                _lastDragPoint = pt.Position;
                e.Handled = true;
                return;
            }

            if (!_isDrawing) return;
            if (pt.PointerId != _activePointerId) return;

            if (!pt.IsInContact)
            {
                InkCanvas_PointerReleased(sender, e);
                return;
            }

            bool isLasso = _isLassoing || pt.Properties.IsBarrelButtonPressed;
            bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser || (ToolState.MouseDrawEnabled && pt.Properties.IsRightButtonPressed);

            if (_isLassoing)
            {
                _lassoPolygon.Points.Add(pt.Position);
            }
            else if (isEraser)
            {
                Erase(pt.Position);
                _lastPoint = pt.Position;
            }
            else if (ToolState.CurrentMode == ToolMode.TextHighlighter)
            {
                if (_readingOrderLetters != null && _readingOrderLetters.Count > 0)
                {
                    int newEnd = GetClosestLetterIndex(pt.Position);
                    if (newEnd != -1)
                    {
                        if (_textSelectionStartIndex == -1)
                        {
                            _textSelectionStartIndex = newEnd;
                        }
                        if (newEnd != _textSelectionEndIndex)
                        {
                            _textSelectionEndIndex = newEnd;
                            UpdateTextSelectionPreview();
                        }
                    }
                }
                else if (_currentStroke != null)
                {
                    _currentStroke.Points.Add(pt.Position);
                }
            }
            else if (_currentStroke != null)
            {
                _currentStroke.Points.Add(pt.Position);
            }
            e.Handled = true;
        }

        private void UpdateTextSelectionPreview()
        {
            if (_currentTextHighlightStrokes != null)
            {
                foreach (var s in _currentTextHighlightStrokes)
                {
                    InkCanvas.Children.Remove(s);
                }
                _currentTextHighlightStrokes.Clear();
            }
            else
            {
                _currentTextHighlightStrokes = new List<Polyline>();
            }
            
            if (_textSelectionStartIndex == -1 || _textSelectionEndIndex == -1) return;
            
            int start = Math.Min(_textSelectionStartIndex, _textSelectionEndIndex);
            int end = Math.Max(_textSelectionStartIndex, _textSelectionEndIndex);
            
            var selectedWords = new List<PdfTextWord>();
            for (int i = start; i <= end; i++)
            {
                selectedWords.Add(_readingOrderLetters[i]);
            }
            
            var lines = new List<List<PdfTextWord>>();
            foreach (var word in selectedWords)
            {
                bool added = false;
                foreach (var line in lines)
                {
                    var l0 = line[0];
                    double overlapTop = Math.Max(l0.Bounds.Y, word.Bounds.Y);
                    double overlapBottom = Math.Min(l0.Bounds.Y + l0.Bounds.Height, word.Bounds.Y + word.Bounds.Height);
                    if (overlapBottom > overlapTop)
                    {
                        double overlapHeight = overlapBottom - overlapTop;
                        double minHeight = Math.Min(l0.Bounds.Height, word.Bounds.Height);
                        if (overlapHeight > minHeight * 0.3)
                        {
                            line.Add(word);
                            added = true;
                            break;
                        }
                    }
                }
                if (!added) lines.Add(new List<PdfTextWord> { word });
            }
            
            double scale = 2.0 / 0.75;
            double canvasH = InkCanvas.Height;
            if (double.IsNaN(canvasH) || canvasH <= 0) canvasH = BaseHeight;
            foreach (var line in lines)
            {
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minTop = double.MaxValue;
                double maxBottom = double.MinValue;
                
                foreach (var w in line)
                {
                    double wLeft = w.Bounds.X * scale;
                    double wRight = wLeft + w.Bounds.Width * scale;
                    if (wLeft < minX) minX = wLeft;
                    if (wRight > maxX) maxX = wRight;
                    
                    double wordTopPdf = w.Bounds.Y + w.Bounds.Height;
                    double wTop = canvasH - (wordTopPdf * scale);
                    double wBottom = canvasH - (w.Bounds.Y * scale);
                    
                    if (wTop < minTop) minTop = wTop;
                    if (wBottom > maxBottom) maxBottom = wBottom;
                }
                
                double maxH = maxBottom - minTop;
                double centerY = (minTop + maxBottom) / 2.0;
                
                var stroke = new Polyline();
                stroke.StrokeLineJoin = PenLineJoin.Round;
                stroke.StrokeStartLineCap = PenLineCap.Flat;
                stroke.StrokeEndLineCap = PenLineCap.Flat;
                
                // Keep the color somewhat transparent for preview and saving
                stroke.Stroke = ToolState.HighlighterColor;
                stroke.StrokeThickness = maxH;
                stroke.IsHitTestVisible = false;
                
                stroke.Points.Add(new Point(minX, centerY));
                stroke.Points.Add(new Point(maxX, centerY));
                
                _currentTextHighlightStrokes.Add(stroke);
                InkCanvas.Children.Add(stroke);
            }
        }

        private void InkCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDrawing) return;
            var pt = e.GetCurrentPoint(InkCanvas);
            if (pt.PointerId != _activePointerId) return;

            InkCanvas.ManipulationMode = ManipulationModes.System;
            InkCanvas.ReleasePointerCapture(e.Pointer);
            _isDrawing = false;
            
            if (_isDraggingSelection)
            {
                _isDraggingSelection = false;
                if (_totalDragX != 0 || _totalDragY != 0)
                {
                    _history?.Add(new MoveElementsCommand(_selectedElements, _totalDragX, _totalDragY));
                }
                e.Handled = true;
                return;
            }
            
            if (_isLassoing)
            {
                _isLassoing = false;
                if (_lassoPolygon != null)
                {
                    FindSelectedElements(_lassoPolygon);
                    InkCanvas.Children.Remove(_lassoPolygon);
                    _lassoPolygon = null;
                }
                e.Handled = true;
                return;
            }
            
            if (ToolState.CurrentMode == ToolMode.TextHighlighter)
            {
                if (_currentTextHighlightStrokes != null && _currentTextHighlightStrokes.Count > 0)
                {
                    if (_history != null)
                    {
                        var commands = new List<IUndoableCommand>();
                        foreach (var s in _currentTextHighlightStrokes)
                        {
                            commands.Add(new AddStrokeCommand(_pageData, s, InkCanvas));
                            _pageData.Strokes.Add(s);
                            // It is already in InkCanvas.Children from UpdateTextSelectionPreview
                        }
                        _history.Add(new MacroCommand(commands));
                    }
                    _currentTextHighlightStrokes = null;
                    _textSelectionStartIndex = -1;
                    _textSelectionEndIndex = -1;
                    if (_currentStroke != null)
                    {
                        InkCanvas.Children.Remove(_currentStroke);
                        _pageData.Strokes.Remove(_currentStroke);
                        _currentStroke = null;
                    }
                    e.Handled = true;
                    return;
                }
                _textSelectionStartIndex = -1;
                _textSelectionEndIndex = -1;
                if (_currentStroke == null)
                {
                    e.Handled = true;
                    return;
                }
            }
            
            if (_currentStroke != null && ToolState.CurrentStraightLineSnap && _currentStroke.Points.Count > 1)
            {
                var first = _currentStroke.Points[0];
                var last = _currentStroke.Points[_currentStroke.Points.Count - 1];
                _currentStroke.Points.Clear();
                
                double dx = last.X - first.X;
                double dy = last.Y - first.Y;
                double dist = Math.Sqrt(dx*dx + dy*dy);
                int steps = Math.Max(1, (int)(dist / 5.0));
                
                for (int i = 0; i <= steps; i++)
                {
                    _currentStroke.Points.Add(new Point(first.X + dx * i / steps, first.Y + dy * i / steps));
                }
            }
            if (_currentStroke != null)
            {
                if (_history != null) _history.Add(new AddStrokeCommand(_pageData, _currentStroke, InkCanvas));
                _currentStroke = null;
            }
            else if (_currentEraserSession.Count > 0)
            {
                if (_history != null) _history.Add(new MacroCommand(new List<IUndoableCommand>(_currentEraserSession)));
                _currentEraserSession.Clear();
            }
            // Release capture
            InkCanvas.ReleasePointerCapture(e.Pointer);

            _currentStroke = null;
            e.Handled = true;
        }

        private void Erase(Point pt)
        {
            double radius = ToolState.EraserThickness / 2;
            var toAdd = new List<Polyline>();
            var toRemove = new List<Polyline>();

            foreach (var stroke in _pageData.Strokes)
            {
                // Fast bounding box check for both eraser types
                double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
                foreach(var p in stroke.Points) {
                    if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
                }
                if (pt.X < minX - radius || pt.X > maxX + radius || pt.Y < minY - radius || pt.Y > maxY + radius)
                    continue;

                if (ToolState.EraserMode == EraserType.Stroke)
                {
                    // Stroke eraser: check if any segment is hit
                    if (IsHit(stroke, pt, radius))
                    {
                        toRemove.Add(stroke);
                    }
                }
                else
                {
                    // Pixel eraser: split stroke
                    var pts = stroke.Points;
                    List<Point> currentPath = new List<Point>();
                    bool modified = false;

                    for (int i = 0; i < pts.Count; i++)
                    {
                        // Check distance to point
                        double dist = GetDistance(pts[i], pt);
                        // Also check segment to previous point
                        double segDist = (i > 0) ? DistanceToSegment(pt, pts[i-1], pts[i]) : double.MaxValue;

                        if (dist <= radius || segDist <= radius)
                        {
                            modified = true;
                            if (currentPath.Count > 1)
                            {
                                toAdd.Add(CloneStroke(stroke, currentPath));
                            }
                            currentPath = new List<Point>();
                        }
                        else
                        {
                            currentPath.Add(pts[i]);
                        }
                    }

                    if (modified)
                    {
                        if (currentPath.Count > 1) toAdd.Add(CloneStroke(stroke, currentPath));
                        toRemove.Add(stroke);
                    }
                }
            }

            foreach (var s in toRemove) { _pageData.Strokes.Remove(s); InkCanvas.Children.Remove(s); }
            foreach (var s in toAdd) { _pageData.Strokes.Add(s); InkCanvas.Children.Add(s); }
            if (toRemove.Count > 0 || toAdd.Count > 0)
            {
                if (toAdd.Count == 0) _currentEraserSession.Add(new RemoveStrokesCommand(_pageData, toRemove, InkCanvas));
                else _currentEraserSession.Add(new ReplaceStrokesCommand(_pageData, toRemove, toAdd, InkCanvas));
            }
        }

        private bool IsHit(Polyline stroke, Point p, double radius)
        {
            var pts = stroke.Points;
            for (int i = 1; i < pts.Count; i++)
            {
                if (DistanceToSegment(p, pts[i - 1], pts[i]) <= radius + stroke.StrokeThickness / 2) return true;
            }
            if (pts.Count == 1 && GetDistance(p, pts[0]) <= radius + stroke.StrokeThickness / 2) return true;
            return false;
        }

        private double GetDistance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double DistanceToSegment(Point p, Point a, Point b)
        {
            double l2 = (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
            if (l2 == 0) return GetDistance(p, a);
            double t = ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y)) / l2;
            t = Math.Max(0, Math.Min(1, t));
            Point proj = new Point(a.X + t * (b.X - a.X), a.Y + t * (b.Y - a.Y));
            return GetDistance(p, proj);
        }

        private Polyline CloneStroke(Polyline source, List<Point> points)
        {
            var p = new Polyline();
            p.StrokeLineJoin = source.StrokeLineJoin;
            p.StrokeStartLineCap = source.StrokeStartLineCap;
            p.StrokeEndLineCap = source.StrokeEndLineCap;
            p.Stroke = source.Stroke;
            p.StrokeThickness = source.StrokeThickness;
            foreach (var pt in points) p.Points.Add(pt);
            return p;
        }
        private void ClearSelection()
        {
            if (_selectionBox != null)
            {
                InkCanvas.Children.Remove(_selectionBox);
                _selectionBox = null;
            }
            _selectedElements.Clear();
        }

        private bool IsPointInPolygon(Point p, Microsoft.UI.Xaml.Media.PointCollection poly)
        {
            Point p1, p2;
            bool inside = false;
            if (poly.Count < 3) return false;

            var oldPoint = new Point(poly[poly.Count - 1].X, poly[poly.Count - 1].Y);
            for (int i = 0; i < poly.Count; i++)
            {
                var newPoint = new Point(poly[i].X, poly[i].Y);
                if (newPoint.X > oldPoint.X) { p1 = oldPoint; p2 = newPoint; }
                else { p1 = newPoint; p2 = oldPoint; }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }
            return inside;
        }

        private void FindSelectedElements(Microsoft.UI.Xaml.Shapes.Polygon polygon)
        {
            ClearSelection();
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            
            foreach (var stroke in _pageData.Strokes)
            {
                foreach (var pt in stroke.Points)
                {
                    if (IsPointInPolygon(pt, polygon.Points))
                    {
                        _selectedElements.Add(stroke);
                        foreach (var p2 in stroke.Points)
                        {
                            if (p2.X < minX) minX = p2.X;
                            if (p2.X > maxX) maxX = p2.X;
                            if (p2.Y < minY) minY = p2.Y;
                            if (p2.Y > maxY) maxY = p2.Y;
                        }
                        break;
                    }
                }
            }

            foreach (var tb in _pageData.Texts)
            {
                double left = Canvas.GetLeft(tb);
                double top = Canvas.GetTop(tb);
                double w = tb.ActualWidth;
                double h = tb.ActualHeight;
                Point center = new Point(left + w / 2, top + h / 2);
                if (IsPointInPolygon(center, polygon.Points))
                {
                    _selectedElements.Add(tb);
                    if (left < minX) minX = left;
                    if (left + w > maxX) maxX = left + w;
                    if (top < minY) minY = top;
                    if (top + h > maxY) maxY = top + h;
                }
            }

            if (_selectedElements.Count > 0)
            {
                _selectionBox = new Microsoft.UI.Xaml.Shapes.Rectangle
                {
                    Width = maxX - minX + 20,
                    Height = maxY - minY + 20,
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                    StrokeThickness = 2,
                    StrokeDashArray = new Microsoft.UI.Xaml.Media.DoubleCollection { 4, 4 },
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(20, 0, 0, 255)),
                    ManipulationMode = ManipulationModes.None
                };
                Canvas.SetLeft(_selectionBox, minX - 10);
                Canvas.SetTop(_selectionBox, minY - 10);
                InkCanvas.Children.Add(_selectionBox);
            }
        }
    }
}








