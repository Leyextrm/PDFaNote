$content = Get-Content 'PdfPageView.xaml.cs' -Raw
$content = $content -replace '(?s)var first = _currentStroke.Points\[0\];.*?_currentStroke.Points.Add\(last\);', 
'var first = _currentStroke.Points[0];
                var last = _currentStroke.Points[_currentStroke.Points.Count - 1];
                _currentStroke.Points.Clear();
                
                double dx = last.X - first.X;
                double dy = last.Y - first.Y;
                double dist = Math.Sqrt(dx*dx + dy*dy);
                int steps = Math.Max(1, (int)(dist / 5.0));
                
                for (int i = 0; i <= steps; i++)
                {
                    _currentStroke.Points.Add(new Point(first.X + dx * i / steps, first.Y + dy * i / steps));
                }'
Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
