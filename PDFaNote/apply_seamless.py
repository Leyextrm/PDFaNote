import re

with open('PdfDocumentView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Replace InsertBlankPageAfterAsync body
old_insert = '''        public async Task InsertBlankPageAfterAsync(int pageIndex)
        {
            if (_workingFile == null) return;

            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var tempStateFile = await tempFolder.CreateFileAsync("temp_state.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            
            // Save current changes to temp state file without updating _sourceFile
            await SaveAsync(tempStateFile, updateSourceFile: false);

            var tempFile = await tempFolder.CreateFileAsync("temp_insert.pdf", Windows.Storage.CreationCollisionOption.GenerateUniqueName);

            using (var inStream = await tempStateFile.OpenStreamForReadAsync())
            using (var outStream = await tempFile.OpenStreamForWriteAsync())
            {
                var reader = new iText.Kernel.Pdf.PdfReader(inStream);
                var writer = new iText.Kernel.Pdf.PdfWriter(outStream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(reader, writer);
                
                var pageSize = iText.Kernel.Geom.PageSize.A4;
                if (pageIndex < pdf.GetNumberOfPages()) {
                    var rect = pdf.GetPage(pageIndex + 1).GetPageSize();
                    pageSize = new iText.Kernel.Geom.PageSize(rect);
                }

                pdf.AddNewPage(pageIndex + 2, pageSize);
                pdf.Close();
            }

            _workingFile = tempFile;
            History.HasUnsavedChanges = true;
            await LoadPdfAsync(_workingFile, isReload: true);
        }'''

new_insert = '''        public async Task InsertBlankPageAfterAsync(int pageIndex)
        {
            if (_workingFile == null) return;

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
                if (pageIndex < pdf.GetNumberOfPages()) {
                    var rect = pdf.GetPage(pageIndex + 1).GetPageSize();
                    pageSize = new iText.Kernel.Geom.PageSize(rect);
                }
                pdf.AddNewPage(pageIndex + 2, pageSize);
                pdf.Close();
            }

            _workingFile = tempFile;
            History.HasUnsavedChanges = true;

            var newWinPdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(_workingFile);
            var newWinPage = newWinPdf.GetPage((uint)(pageIndex + 1));
            
            var newPageData = new PageData {
                Document = newWinPdf,
                PageIndex = (uint)(pageIndex + 1),
                Size = newWinPage.Size
            };
            
            var newPageView = new PdfPageView();
            newPageView.InsertPageRequested += PageView_InsertPageRequested;
            newPageView.DeletePageRequested += PageView_DeletePageRequested;
            newPageView.LoadPage(newPageData, History);
            
            _pages.Insert(pageIndex + 1, newPageView);
            
            for (int i = 0; i < _pages.Count; i++) {
                _pages[i].PageData.Document = newWinPdf;
                if (i > pageIndex + 1) {
                    _pages[i].PageData.PageIndex = (uint)(i);
                }
            }
        }'''

# Replace DeletePageAsync body
old_delete = '''        public async Task DeletePageAsync(int pageIndex)
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
            await LoadPdfAsync(_workingFile, isReload: true);
        }'''

new_delete = '''        public async Task DeletePageAsync(int pageIndex)
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
        }'''

text = text.replace(old_insert, new_insert)
text = text.replace(old_delete, new_delete)

with open('PdfDocumentView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Applied seamless logic")
