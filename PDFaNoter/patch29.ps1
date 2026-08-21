$content = Get-Content 'PDFaNoter.csproj' -Raw
$content = $content -replace '<EnableMsixTooling>true</EnableMsixTooling>', '<EnableMsixTooling>true</EnableMsixTooling>
    <WindowsPackageType>None</WindowsPackageType>'
Set-Content -Path 'PDFaNoter.csproj' -Value $content -Encoding UTF8
