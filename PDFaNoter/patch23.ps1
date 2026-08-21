$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw

$content = $content -replace 'private void UpdateToolSelection\(\) \{ \}', 
'private void UpdateToolSelection()
        {
            if (BtnStraightLine != null)
            {
                BtnStraightLine.IsChecked = ToolState.CurrentStraightLineSnap;
            }
        }'

$content = $content -replace 'private void ToggleOption_Click\(object sender, RoutedEventArgs e\) \{ ToolState.StraightLineSnap = BtnStraightLine.IsChecked == true; \}', 
'private void ToggleOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnStraightLine)
            {
                ToolState.CurrentStraightLineSnap = BtnStraightLine.IsChecked == true;
            }
        }'

Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
