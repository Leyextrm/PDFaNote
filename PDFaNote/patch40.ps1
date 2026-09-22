$content = Get-Content 'ToolState.cs' -Raw
$content = [System.Text.RegularExpressions.Regex]::Replace($content, 'using Microsoft\.UI\.Xaml\.Media;\r?\nusing System\.Collections\.Generic;\r?\n', '')
Set-Content -Path 'ToolState.cs' -Value $content -Encoding UTF8
