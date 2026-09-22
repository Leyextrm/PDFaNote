import re

with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Replace Polygon with Rectangle
text = re.sub(r'if \(isLasso\)\s*\{\s*_lassoPolygon = new Microsoft\.UI\.Xaml\.Shapes\.Polygon\s*\{\s*Stroke = new Microsoft\.UI\.Xaml\.Media\.SolidColorBrush\(Microsoft\.UI\.Colors\.Blue\),\s*StrokeThickness = 1,\s*StrokeDashArray = new Microsoft\.UI\.Xaml\.Media\.DoubleCollection \{ 4, 4 \},\s*Fill = new Microsoft\.UI\.Xaml\.Media\.SolidColorBrush\(Windows\.UI\.Color\.FromArgb\(50, 0, 0, 255\)\)\s*\};\s*_lassoPolygon\.Points\.Add\(pt\.Position\);\s*InkCanvas\.Children\.Add\(_lassoPolygon\);\s*_isLassoing = true;\s*\}', 
r'''if (isLasso)
            {
                ClearSelection();
                _isLassoing = true;
                _lastDragPoint = pt.Position;
                _lassoRect = new Microsoft.UI.Xaml.Shapes.Rectangle
                {
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                    StrokeThickness = 1,
                    StrokeDashArray = new Microsoft.UI.Xaml.Media.DoubleCollection { 4, 4 },
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(50, 0, 0, 255)),
                    Width = 0,
                    Height = 0
                };
                Canvas.SetLeft(_lassoRect, pt.Position.X);
                Canvas.SetTop(_lassoRect, pt.Position.Y);
                InkCanvas.Children.Add(_lassoRect);
            }''', text)

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Fixed lasso polygon in pointerpressed")
