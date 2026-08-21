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
                        StrokeEndLineCap = Microsoft.UI.Xaml.Media.PenLineCap.Round
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

            using (var pdfPage = _pageData.Document.GetPage(_pageData.PageIndex))
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    var renderOptions = new PdfPageRenderOptions();
                    renderOptions.DestinationWidth = (uint)(pdfPage.Size.Width * 2);
                    
                    await pdfPage.RenderToStreamAsync(stream, renderOptions);
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    PdfImage.Source = bitmap;
                    
                    InkCanvas.Width = pdfPage.Size.Width * 2;
                    InkCanvas.Height = pdfPage.Size.Height * 2;
                    PdfImage.Width = pdfPage.Size.Width * 2;
                    PdfImage.Height = pdfPage.Size.Height * 2;
                    PageContainer.Width = pdfPage.Size.Width * 2;
                    PageContainer.Height = pdfPage.Size.Height * 2;
                }
            }
        }

        private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(InkCanvas);
            if (pt.PointerDeviceType != PointerDeviceType.Pen && !ToolState.MouseDrawEnabled) return;

            _isDrawing = true;
            _activePointerId = pt.PointerId;
            _lastPoint = pt.Position;
            InkCanvas.CapturePointer(e.Pointer);

            bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser;

            if (isEraser)
            {
                _currentEraserSession.Clear();
                Erase(pt.Position);
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
            if (!_isDrawing) return;
            var pt = e.GetCurrentPoint(InkCanvas);
            if (pt.PointerId != _activePointerId) return;

            bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser;

            if (isEraser)
            {
                // Interpolate for fast eraser movement
                Erase(pt.Position);
                _lastPoint = pt.Position;
            }
            else if (_currentStroke != null)
            {
                _currentStroke.Points.Add(pt.Position);
            }
            e.Handled = true;
        }

        private void InkCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDrawing) return;
            var pt = e.GetCurrentPoint(InkCanvas);
            if (pt.PointerId != _activePointerId) return;

            InkCanvas.ReleasePointerCapture(e.Pointer);
            _isDrawing = false;
            
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
    }
}








