$content = Get-Content 'ToolState.cs' -Raw
$content = $content.Replace(@""using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;"", """")
Set-Content -Path 'ToolState.cs' -Value $content -Encoding UTF8
