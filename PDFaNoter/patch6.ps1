$content = Get-Content 'PdfPageView.xaml.cs' -Raw

$content = $content -replace '(?s)if \(_currentStroke \!= null && ToolState.StraightLineSnap && _currentStroke.Points.Count > 1\).*?\r?\n\s*\}\r?\n\s*\}', 
'if (_currentStroke != null && ToolState.StraightLineSnap && _currentStroke.Points.Count > 1)
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
            // Release capture
            InkCanvas.ReleasePointerCapture(e.Pointer);'

$content = $content -replace '(?s)if \(hitStroke \!= null\)\s*\{\s*InkCanvas.Children.Remove\(hitStroke\);\s*_pageData.Strokes.Remove\(hitStroke\);\s*\}', 
'if (hitStroke != null)
            {
                InkCanvas.Children.Remove(hitStroke);
                _pageData.Strokes.Remove(hitStroke);
                if (_history != null) _history.Add(new RemoveStrokesCommand(_pageData, new List<Polyline> { hitStroke }, InkCanvas));
            }'

$content = $content -replace '(?s)if \(modified\)\s*\{\s*InkCanvas.Children.Remove\(hitStroke\);\s*_pageData.Strokes.Remove\(hitStroke\);(.*?)\}', 
'if (modified)
                {
                    InkCanvas.Children.Remove(hitStroke);
                    _pageData.Strokes.Remove(hitStroke);
                    var addedStrokes = new List<Polyline>();

                    if (_history != null) _history.Add(new ReplaceStrokesCommand(_pageData, new List<Polyline> { hitStroke }, addedStrokes, InkCanvas));
                }'

$content = $content -replace '_pageData.Strokes.Add\(newStroke\);', '_pageData.Strokes.Add(newStroke);
                            addedStrokes.Add(newStroke);'

Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
