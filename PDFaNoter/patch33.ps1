$content = Get-Content 'PDFaNoter.csproj' -Raw
$content = $content.Replace('<RuntimeIdentifier Condition="''$(RuntimeIdentifier)'' == ''''">win-$([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant())</RuntimeIdentifier>', '<RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>')
Set-Content -Path 'PDFaNoter.csproj' -Value $content -Encoding UTF8
