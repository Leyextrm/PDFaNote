with open('ToolState.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Add HighlighterSnapToText
idx = text.find('public static bool CurrentStraightLineSnap')
if idx != -1:
    text = text[:idx] + '''public static bool[] HighlighterSnapToText = new bool[3];
        public static bool CurrentSnapToText 
        { 
            get 
            {
                if (CurrentMode == ToolMode.Highlighter) return HighlighterSnapToText[CurrentHighlighterSlot];
                return false;
            }
            set
            {
                if (CurrentMode == ToolMode.Highlighter) HighlighterSnapToText[CurrentHighlighterSlot] = value;
            }
        }
        
        ''' + text[idx:]

# Add ExtractedTexts to PdfPageData
idx2 = text.find('public List<PdfTextWord> Words { get; set; } = new List<PdfTextWord>();')
if idx2 != -1:
    text = text[:idx2] + 'public List<PdfTextWord> Words { get; set; } = new List<PdfTextWord>();\n        public List<ExtractedTextAnnotation> ExtractedTexts { get; set; } = new List<ExtractedTextAnnotation>();\n' + text[idx2 + len('public List<PdfTextWord> Words { get; set; } = new List<PdfTextWord>();'):]

# Add ExtractedTextAnnotation class
idx3 = text.rfind('}')
if idx3 != -1:
    text = text[:idx3] + '''    public class ExtractedTextAnnotation
    {
        public string Text { get; set; }
        public double CanvasLeft { get; set; }
        public double CanvasTop { get; set; }
        public Windows.UI.Color Color { get; set; }
        public double FontSize { get; set; }
        public string FontFamily { get; set; }
    }
}
'''

with open('ToolState.cs', 'w', encoding='utf-8') as f:
    f.write(text)

