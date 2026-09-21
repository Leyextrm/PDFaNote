import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

start_idx = text.find('public async Task DeletePageAsync')
end_idx = text.find('private System.Collections.Generic.List<Windows.Foundation.Point> GetSmoothedPoints', start_idx)

new_delete = '''public async Task DeletePageAsync(int pageIndex)
        {
            if (_workingFile == null) return;
            
            var dialog = new ContentDialog {
                Title = "Delete Page",
                Content = "Are you sure you want to delete this page?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var tempStateFile = await tempFolder.CreateFileAsync("temp_state.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            
            await SaveAsync(tempStateFile, updateSourceFile: false);

            var tempFile = await tempFolder.CreateFileAsync("temp_delete.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);

            using (var inStream = await tempStateFile.OpenStreamForReadAsync())
            using (var outStream = await tempFile.OpenStreamForWriteAsync())
            {
                var reader = new iText.Kernel.Pdf.PdfReader(inStream);
                var writer = new iText.Kernel.Pdf.PdfWriter(outStream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(reader, writer);
                
                pdf.RemovePage(pageIndex + 1);
                
                pdf.Close();
            }

            _workingFile = tempFile;
            History.HasUnsavedChanges = true;

            var newWinPdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(_workingFile);
            _pages.RemoveAt(pageIndex);

            for (int i = 0; i < _pages.Count; i++) {
                _pages[i].PageData.Document = newWinPdf;
                if (i >= pageIndex) {
                    _pages[i].PageData.PageIndex = (uint)(i);
                }
            }
        }
        '''

if start_idx != -1 and end_idx != -1:
    text = text[:start_idx] + new_delete + text[end_idx:]
    with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
        f.write(text)
    print("Fixed DeletePageAsync")
else:
    print("Not found")
