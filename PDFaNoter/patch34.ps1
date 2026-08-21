$content = Get-Content 'ToolState.cs' -Raw
$content = $content -replace 'public List<Polyline> Strokes { get; set; } = new List<Polyline>\(\);', "public List<Polyline> Strokes { get; set; } = new List<Polyline>();
        public List<ExtractedStroke> ExtractedStrokes { get; set; } = new List<ExtractedStroke>();"
Set-Content -Path 'ToolState.cs' -Value $content -Encoding UTF8
