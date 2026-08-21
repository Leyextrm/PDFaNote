$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw

$content = $content -replace '(?s)public async Task LoadPdfAsync\(Windows.Storage.StorageFile file\)\s*\{\s*_sourceFile = file;\s*_pdfDocument = await PdfDocument.LoadFromFileAsync\(file\);', 
'public async Task LoadPdfAsync(Windows.Storage.StorageFile file)
        {
            _sourceFile = file;
            
            using (var stream = await file.OpenReadAsync())
            {
                var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                await Windows.Storage.Streams.RandomAccessStream.CopyAsync(stream, memStream);
                _pdfDocument = await PdfDocument.LoadFromStreamAsync(memStream);
            }'

Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
