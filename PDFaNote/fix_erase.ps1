$content = Get-Content 'PdfPageView.xaml.cs' -Raw
$content = $content.Replace('private Point _lastPoint;', "private Point _lastPoint;
        private List<IUndoableCommand> _currentEraserSession = new List<IUndoableCommand>();")

$pressedErase = "if (isEraser)
            {
                _currentEraserSession.Clear();
                Erase(pt.Position);
            }"
$content = $content.Replace('if (isEraser)' + [Environment]::NewLine + '            {' + [Environment]::NewLine + '                Erase(pt.Position);' + [Environment]::NewLine + '            }', $pressedErase)

$releasedErase = "if (_currentStroke != null)
            {
                if (_history != null) _history.Add(new AddStrokeCommand(_pageData, _currentStroke, InkCanvas));
                _currentStroke = null;
            }
            else if (_currentEraserSession.Count > 0)
            {
                if (_history != null) _history.Add(new MacroCommand(new List<IUndoableCommand>(_currentEraserSession)));
                _currentEraserSession.Clear();
            }"
$content = $content.Replace('if (_currentStroke != null)' + [Environment]::NewLine + '            {' + [Environment]::NewLine + '                if (_history != null) _history.Add(new AddStrokeCommand(_pageData, _currentStroke, InkCanvas));' + [Environment]::NewLine + '                _currentStroke = null;' + [Environment]::NewLine + '            }', $releasedErase)

$eraseAddCmd = "            foreach (var s in toRemove) { _pageData.Strokes.Remove(s); InkCanvas.Children.Remove(s); }
            foreach (var s in toAdd) { _pageData.Strokes.Add(s); InkCanvas.Children.Add(s); }
            if (toRemove.Count > 0 || toAdd.Count > 0)
            {
                if (toAdd.Count == 0) _currentEraserSession.Add(new RemoveStrokesCommand(_pageData, toRemove, InkCanvas));
                else _currentEraserSession.Add(new ReplaceStrokesCommand(_pageData, toRemove, toAdd, InkCanvas));
            }"
$content = $content.Replace('foreach (var s in toRemove) { _pageData.Strokes.Remove(s); InkCanvas.Children.Remove(s); }' + [Environment]::NewLine + '            foreach (var s in toAdd) { _pageData.Strokes.Add(s); InkCanvas.Children.Add(s); }', $eraseAddCmd)

Set-Content -Path 'PdfPageView.xaml.cs' -Value $content -Encoding UTF8
