$content = Get-Content 'PDFaNoter.csproj' -Raw
$content = $content -replace '<RuntimeIdentifier Condition=''.*?''.*?</RuntimeIdentifier>', '<RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>'
Set-Content -Path 'PDFaNoter.csproj' -Value $content -Encoding UTF8
