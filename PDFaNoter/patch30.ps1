$content = Get-Content 'PDFaNoter.csproj' -Raw
$content = $content -replace '<PublishTrimmed Condition="''\$\(Configuration\)'' \!= ''Debug''">True</PublishTrimmed>', '<PublishTrimmed Condition="''$(Configuration)'' != ''Debug''">False</PublishTrimmed>'
Set-Content -Path 'PDFaNoter.csproj' -Value $content -Encoding UTF8
