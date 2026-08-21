$content = Get-Content 'PdfDocumentView.xaml.cs' -Raw
$content = $content -replace 'var pdfPage = pdfDoc.GetPage\(i \+ 1\);', 'var pdfPage = pdfDoc.GetPage(i + 1);
                    var pageSize = pdfPage.GetPageSize();'
Set-Content -Path 'PdfDocumentView.xaml.cs' -Value $content -Encoding UTF8
