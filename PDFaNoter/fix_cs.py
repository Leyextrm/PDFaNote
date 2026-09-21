with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Add SetLayoutMode
idx = text.find('private void BtnDeletePage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)')
text = text[:idx] + '''public void SetLayoutMode(bool isHorizontal)
        {
            if (isHorizontal)
            {
                MainStackPanel.Orientation = Orientation.Vertical;
                ButtonStackPanel.Margin = new Thickness(0, 10, 0, 10);
                PageContainer.Margin = new Thickness(0);
            }
            else
            {
                MainStackPanel.Orientation = Orientation.Horizontal;
                ButtonStackPanel.Orientation = Orientation.Vertical;
                ButtonStackPanel.Margin = new Thickness(20, 0, 20, 0);
                PageContainer.Margin = new Thickness(84, 0, 0, 0); // Equal width on left as the right margin+button
            }
        }

        ''' + text[idx:]

# Add right click eraser
idx_pressed = text.find('private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)\n        {\n            var pt = e.GetCurrentPoint(InkCanvas);')
if idx_pressed != -1:
    idx_insert = idx_pressed + len('private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)\n        {\n            var pt = e.GetCurrentPoint(InkCanvas);')
    text = text[:idx_insert] + '''
            bool isEraserEarly = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser || pt.Properties.IsRightButtonPressed;
            if (!isEraserEarly)
            {''' + text[idx_insert:]
    
    # We must close the if block before if (pt.PointerDeviceType != PointerDeviceType.Pen && !ToolState.MouseDrawEnabled) return;
    idx_pen_check = text.find('if (pt.PointerDeviceType != PointerDeviceType.Pen && !ToolState.MouseDrawEnabled) return;', idx_insert)
    text = text[:idx_pen_check] + '}\n            ' + text[idx_pen_check:]
    
    # Also replace ool isEraser = ... with ool isEraser = isEraserEarly;
    text = text.replace('bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser;', 'bool isEraser = isEraserEarly;')

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text2 = f.read()

# Add ApplyScrollMode changes
idx_apply = text2.find('private void ApplyScrollMode(string mode)\n        {\n            bool isHoriz = (mode == "Horizontal");\n            if (isHoriz) { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns=\'http://schemas.microsoft.com/winfx/2006/xaml/presentation\'><StackPanel Orientation=\'Horizontal\' Spacing=\'20\' VerticalAlignment=\'Center\' /></ItemsPanelTemplate>"); }\n            else { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns=\'http://schemas.microsoft.com/winfx/2006/xaml/presentation\'><StackPanel Orientation=\'Vertical\' Spacing=\'20\' HorizontalAlignment=\'Center\' /></ItemsPanelTemplate>"); }')
if idx_apply != -1:
    idx_apply_end = idx_apply + len('private void ApplyScrollMode(string mode)\n        {\n            bool isHoriz = (mode == "Horizontal");\n            if (isHoriz) { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns=\'http://schemas.microsoft.com/winfx/2006/xaml/presentation\'><StackPanel Orientation=\'Horizontal\' Spacing=\'20\' VerticalAlignment=\'Center\' /></ItemsPanelTemplate>"); }\n            else { PdfPagesControl.ItemsPanel = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"<ItemsPanelTemplate xmlns=\'http://schemas.microsoft.com/winfx/2006/xaml/presentation\'><StackPanel Orientation=\'Vertical\' Spacing=\'20\' HorizontalAlignment=\'Center\' /></ItemsPanelTemplate>"); }')
    text2 = text2[:idx_apply_end] + '''
            if (_pages != null)
            {
                foreach (var page in _pages)
                {
                    page.SetLayoutMode(isHoriz);
                }
            }''' + text2[idx_apply_end:]

# Add SetLayoutMode to page creation
idx_create = text2.find('pageView.DeletePageRequested += PageView_DeletePageRequested;\n                pageView.LoadPage(pageData, History);')
if idx_create != -1:
    text2 = text2[:idx_create] + 'pageView.DeletePageRequested += PageView_DeletePageRequested;\n                pageView.SetLayoutMode(ToolState.ScrollMode == "Horizontal");\n                pageView.LoadPage(pageData, History);' + text2[idx_create + len('pageView.DeletePageRequested += PageView_DeletePageRequested;\n                pageView.LoadPage(pageData, History);'):]

# Replace BtnFitPage with BtnFitWidth in XAML and CS
text2 = text2.replace('BtnFitPage_Click', 'BtnFitWidth_Click')

# Add BtnFitWidth_Click
idx_class_end = text2.rfind('    }\n}')
text2 = text2[:idx_class_end] + '''        private void BtnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            if (_pages == null || _pages.Count == 0) return;
            double targetWidth = _pages[0].PageContainer.Width;
            if (ToolState.ScrollMode != "Horizontal")
            {
                targetWidth += 168; // Account for margins in vertical mode
            }
            if (targetWidth <= 0) targetWidth = _pages[0].ActualWidth;
            if (targetWidth <= 0) return;
            
            float newZoom = (float)(PdfScrollViewer.ViewportWidth / targetWidth);
            PdfScrollViewer.ChangeView(null, null, newZoom);
        }
''' + text2[idx_class_end:]

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text2)

print("Applied CS fixes!")
