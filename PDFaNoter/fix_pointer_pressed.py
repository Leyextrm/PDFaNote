import re
with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

start_idx = text.find('private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)')
end_idx = text.find('private void InkCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)')

replacement = r'''private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(InkCanvas);
            
            bool isEraserEarly = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser || pt.Properties.IsRightButtonPressed;

            if (!isEraserEarly)
            {
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
                        FontFamily = new FontFamily(ToolState.TextFontFamily)
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
                    return;
                }
            }

            if (pt.PointerDeviceType != PointerDeviceType.Pen && !ToolState.MouseDrawEnabled) return;
            
            _isDrawing = true;
            _activePointerId = pt.PointerId;

            bool isLasso = ToolState.CurrentMode == ToolMode.Lasso || pt.Properties.IsBarrelButtonPressed;
            bool isEraser = isEraserEarly;

            if (isEraser)
            {
                Erase(pt.Position);
                _lastPoint = pt.Position;
            }
            else if (ToolState.CurrentMode == ToolMode.TextHighlighter)
            {
                _currentTextHighlightStrokes = new List<Polyline>();
                _readingOrderLetters = PdfTextExtractor.GetReadingOrderLetters(_pageData.Words);
                
                int closestIdx = GetClosestLetterIndex(pt.Position);
                _textSelectionStartIndex = closestIdx;
                _textSelectionEndIndex = closestIdx;
                
                UpdateTextHighlight();
            }
            else if (isLasso)
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
            }
            else
            {
                _currentStroke = new Polyline
                {
                    Stroke = ToolState.CurrentColor,
                    StrokeThickness = ToolState.CurrentThickness,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                
                if (ToolState.CurrentMode == ToolMode.Highlighter)
                {
                    _currentStroke.Opacity = 0.5;
                }
                
                _currentStroke.Points.Add(pt.Position);
                InkCanvas.Children.Add(_currentStroke);
            }

            InkCanvas.CapturePointer(e.Pointer);
        }

        '''

if start_idx != -1 and end_idx != -1:
    new_text = text[:start_idx] + replacement + text[end_idx:]
    with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
        f.write(new_text)
    print("Replaced!")
