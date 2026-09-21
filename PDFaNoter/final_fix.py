with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# 1. Add SetLayoutMode
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

# 2. Right click eraser
# We just need to find the place where it checks for eraser mode and add right click
old_eraser = 'bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser;'
new_eraser = 'bool isEraser = pt.Properties.IsEraser || ToolState.CurrentMode == ToolMode.Eraser || pt.Properties.IsRightButtonPressed;'
text = text.replace(old_eraser, new_eraser)
# And to bypass text mode if right click is pressed:
old_text = 'if (ToolState.CurrentMode == ToolMode.Text)'
new_text = 'if (ToolState.CurrentMode == ToolMode.Text && !pt.Properties.IsRightButtonPressed)'
text = text.replace(old_text, new_text)

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text2 = f.read()

# 3. ApplyScrollMode changes
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

# 4. Add SetLayoutMode to page creation
idx_create = text2.find('pageView.DeletePageRequested += PageView_DeletePageRequested;\n                pageView.LoadPage(pageData, History);')
if idx_create != -1:
    text2 = text2[:idx_create] + 'pageView.DeletePageRequested += PageView_DeletePageRequested;\n                pageView.SetLayoutMode(ToolState.ScrollMode == "Horizontal");\n                pageView.LoadPage(pageData, History);' + text2[idx_create + len('pageView.DeletePageRequested += PageView_DeletePageRequested;\n                pageView.LoadPage(pageData, History);'):]

# 5. Fix XAML handler mapping
# The XAML has BtnFitWidth_Click and ToolButton_Click but PdfDocumentView.xaml.cs has BtnFitPage_Click and Text_Click
# So I will just rename them in XAML instead to avoid touching CS too much, OR I can replace in CS.
# Let's replace in CS because it's easier.
text2 = text2.replace('private void BtnFitPage_Click', 'private void BtnFitWidth_Click')

# Now add BtnFitWidth logic:
old_fit = '''        private void BtnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            if (_pages == null || _pages.Count == 0) return;
            var pd = _pages[0].PageData;
            var w = pd.Size.Width;
            var h = pd.Size.Height;
            float z = 1.0f;
            if (PdfScrollViewer.ViewportWidth / w < PdfScrollViewer.ViewportHeight / h)
            {
                z = (float)(PdfScrollViewer.ViewportWidth / w);
            }
            else
            {
                z = (float)(PdfScrollViewer.ViewportHeight / h);
            }
            PdfScrollViewer.ChangeView(null, null, z);
        }'''
new_fit = '''        private void BtnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            if (_pages == null || _pages.Count == 0) return;
            double targetWidth = _pages[0].ActualWidth;
            if (targetWidth <= 0) targetWidth = _pages[0].PageData.Size.Width;
            if (targetWidth <= 0) return;
            
            float newZoom = (float)(PdfScrollViewer.ViewportWidth / targetWidth);
            PdfScrollViewer.ChangeView(null, null, newZoom);
        }'''
text2 = text2.replace(old_fit, new_fit)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text2)

print("Applied fix!")
