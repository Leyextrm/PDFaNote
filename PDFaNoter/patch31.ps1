$content = Get-Content 'PDFaNoter.csproj' -Raw
$content = $content -replace '(?s)<EnableMsixTooling>true</EnableMsixTooling>\s*<WindowsPackageType>None</WindowsPackageType>', '<EnableMsixTooling>true</EnableMsixTooling>'
Set-Content -Path 'PDFaNoter.csproj' -Value $content -Encoding UTF8
