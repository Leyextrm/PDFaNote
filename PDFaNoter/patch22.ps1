$content = Get-Content 'ToolState.cs' -Raw

$content = $content -replace 'public static bool StraightLineSnap \{ get; set; \} = false;', 'public static bool[] PenStraightLine = new bool[5];
        public static bool[] HighlighterStraightLine = new bool[3];
        
        public static bool CurrentStraightLineSnap 
        { 
            get 
            {
                if (CurrentMode == ToolMode.Pen) return PenStraightLine[CurrentPenSlot];
                if (CurrentMode == ToolMode.Highlighter) return HighlighterStraightLine[CurrentHighlighterSlot];
                return false;
            }
            set
            {
                if (CurrentMode == ToolMode.Pen) PenStraightLine[CurrentPenSlot] = value;
                else if (CurrentMode == ToolMode.Highlighter) HighlighterStraightLine[CurrentHighlighterSlot] = value;
            }
        }'

Set-Content -Path 'ToolState.cs' -Value $content -Encoding UTF8
