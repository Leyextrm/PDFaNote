Add-Type -Path "C:\Users\leyex\.nuget\packages\itext7\8.0.5\lib\netstandard2.0\itext.kernel.dll"
Add-Type -Path "C:\Users\leyex\.nuget\packages\itext7\8.0.5\lib\netstandard2.0\itext.io.dll"
Add-Type -Path "C:\Users\leyex\.nuget\packages\itext7\8.0.5\lib\netstandard2.0\itext.commons.dll"

$writer = New-Object iText.Kernel.Pdf.PdfWriter("dummy.pdf")
$pdfDoc = New-Object iText.Kernel.Pdf.PdfDocument($writer)
$pageSize = New-Object iText.Kernel.Geom.PageSize(600, 800)
$page = $pdfDoc.AddNewPage($pageSize)
$rect = New-Object iText.Kernel.Geom.Rectangle(0, 100, 600, 700)
$page.SetCropBox($rect)
$pdfDoc.Close()
