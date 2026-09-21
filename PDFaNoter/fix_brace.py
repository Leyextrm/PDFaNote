import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# I will truncate text right before private async void PageView_InsertPageRequested
# and rewrite the whole block cleanly.
cut_idx = text.find('private async void PageView_InsertPageRequested')
if cut_idx != -1:
    text = text[:cut_idx]

seamless_code = '''
        private async void PageView_InsertPageRequested(object sender, EventArgs e)
        {
            if (sender is PdfPageView pageView)
            {
                int index = _pages.IndexOf(pageView);
                if (index >= 0)
                {
                    await InsertBlankPageAfterAsync(pageView);
                }
            }
        }

        private async void PageView_DeletePageRequested(object sender, EventArgs e)
        {
            if (sender is PdfPageView pageView)
            {
                await DeletePageAsync(pageView);
            }
        }

        public async Task InsertBlankPageAfterAsync(PdfPageView requestingPage)
        {
            if (_workingFile == null) return;
            int index = _pages.IndexOf(requestingPage);
            if (index < 0) return;

            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var tempStateFile = await tempFolder.CreateFileAsync("temp_state.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            await SaveAsync(tempStateFile, updateSourceFile: false);

            var tempFile = await tempFolder.CreateFileAsync("temp_insert.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);

            using (var inStream = await tempStateFile.OpenStreamForReadAsync())
            using (var outStream = await tempFile.OpenStreamForWriteAsync())
            {
                var reader = new iText.Kernel.Pdf.PdfReader(inStream);
                var writer = new iText.Kernel.Pdf.PdfWriter(outStream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(reader, writer);
                
                var pageSize = iText.Kernel.Geom.PageSize.A4;
                if (index < pdf.GetNumberOfPages()) {
                    var rect = pdf.GetPage(index + 1).GetPageSize();
                    pageSize = new iText.Kernel.Geom.PageSize(rect);
                }

                pdf.AddNewPage(index + 2, pageSize);
                pdf.Close();
            }

            _workingFile = tempFile;
            History.HasUnsavedChanges = true;

            var newWinPdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(_workingFile);
            for (int i = 0; i < _pages.Count; i++) {
                _pages[i].PageData.Document = newWinPdf;
                if (i > index) {
                    _pages[i].PageData.PageIndex = (uint)(i + 1);
                }
            }

            var newPageData = new PdfPageData {
                PageIndex = (uint)(index + 1),
                Document = newWinPdf,
                Words = new System.Collections.Generic.List<PdfTextWord>()
            };

            var newPageView = new PdfPageView();
            newPageView.InsertPageRequested += PageView_InsertPageRequested;
            newPageView.DeletePageRequested += PageView_DeletePageRequested;
            newPageView.SetLayoutMode(ToolState.ScrollMode == "Horizontal");
            
            _pages.Insert(index + 1, newPageView);
            newPageView.LoadPage(newPageData, History);
        }

        public async Task DeletePageAsync(PdfPageView requestingPage)
        {
            if (_workingFile == null) return;
            int index = _pages.IndexOf(requestingPage);
            if (index < 0) return;
            
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog {
                Title = "Delete Page",
                Content = "Are you sure you want to delete this page?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

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
                
                pdf.RemovePage(index + 1);
                
                pdf.Close();
            }

            _workingFile = tempFile;
            History.HasUnsavedChanges = true;

            var newWinPdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(_workingFile);
            _pages.RemoveAt(index);

            for (int i = 0; i < _pages.Count; i++) {
                _pages[i].PageData.Document = newWinPdf;
                if (i >= index) {
                    _pages[i].PageData.PageIndex = (uint)(i);
                }
            }
        }
    }
}
'''

text = text + seamless_code

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
