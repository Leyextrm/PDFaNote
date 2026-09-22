import re
with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

text = text.replace('private Microsoft.UI.Xaml.Shapes.Polygon _lassoPolygon;', 'private Microsoft.UI.Xaml.Shapes.Rectangle _lassoRect;')

old_press = '''            else if (isLasso)
            {
                ClearSelection();
                _isLassoing = true;
                _lassoPolygon = new Microsoft.UI.Xaml.Shapes.Polygon
                {
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.Blue),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
                    Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(30, 0, 0, 255))
                };
                _lassoPolygon.Points.Add(pt.Position);
                InkCanvas.Children.Add(_lassoPolygon);
            }'''
new_press = '''            else if (isLasso)
            {
                ClearSelection();
                _isLassoing = true;
                _lastDragPoint = pt.Position;
                _lassoRect = new Microsoft.UI.Xaml.Shapes.Rectangle
                {
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.Blue),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
                    Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(30, 0, 0, 255)),
                    Width = 0,
                    Height = 0
                };
                Canvas.SetLeft(_lassoRect, pt.Position.X);
                Canvas.SetTop(_lassoRect, pt.Position.Y);
                InkCanvas.Children.Add(_lassoRect);
            }'''
text = text.replace(old_press, new_press)

old_move = '''            if (_isLassoing)
            {
                _lassoPolygon.Points.Add(pt.Position);
            }'''
new_move = '''            if (_isLassoing)
            {
                double x = Math.Min(_lastDragPoint.X, pt.Position.X);
                double y = Math.Min(_lastDragPoint.Y, pt.Position.Y);
                double w = Math.Abs(pt.Position.X - _lastDragPoint.X);
                double h = Math.Abs(pt.Position.Y - _lastDragPoint.Y);
                Canvas.SetLeft(_lassoRect, x);
                Canvas.SetTop(_lassoRect, y);
                _lassoRect.Width = w;
                _lassoRect.Height = h;
            }'''
text = text.replace(old_move, new_move)

old_rel = '''            if (_isLassoing)
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
            }'''
new_rel = '''            if (_isLassoing)
            {
                _isLassoing = false;
                if (_lassoRect != null)
                {
                    var rect = new Windows.Foundation.Rect(Canvas.GetLeft(_lassoRect), Canvas.GetTop(_lassoRect), _lassoRect.Width, _lassoRect.Height);
                    FindSelectedElements(rect);
                    InkCanvas.Children.Remove(_lassoRect);
                    _lassoRect = null;
                }
                e.Handled = true;
                return;
            }'''
text = text.replace(old_rel, new_rel)

text = text.replace('private void FindSelectedElements(Microsoft.UI.Xaml.Shapes.Polygon polygon)', 'private void FindSelectedElements(Windows.Foundation.Rect rect)')
text = text.replace('if (IsPointInPolygon(pt, polygon.Points))', 'if (rect.Contains(pt))')
# Also for texts:
text = text.replace('if (IsPointInPolygon(new Windows.Foundation.Point(tbLeft, tbTop), polygon.Points) ||', 'if (rect.Contains(new Windows.Foundation.Point(tbLeft, tbTop)) ||')
text = text.replace('IsPointInPolygon(new Windows.Foundation.Point(tbLeft + tb.ActualWidth, tbTop + tb.ActualHeight), polygon.Points))', 'rect.Contains(new Windows.Foundation.Point(tbLeft + tb.ActualWidth, tbTop + tb.ActualHeight)))')


with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Updated lasso!")
