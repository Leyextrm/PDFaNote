$content = Get-Content 'PdfPageView.xaml.cs' -Raw
$pattern = '(?s)InkCanvas\.Children\.Clear\(\);\s*foreach \(var stroke in _pageData\.Strokes\)\s*\{\s*InkCanvas\.Children\.Add\(stroke\);\s*\}'

$newCode = @'
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
'@
$content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, $newCode)
Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
