using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Shapes;
using Windows.Data.Pdf;

namespace PDFaNoter
{
    public enum ToolMode { Pen, Highlighter, Eraser, Text, Lasso, TextHighlighter }
    public enum EraserType { Stroke, Pixel }

    public static class ToolState
    {
        public static ToolMode CurrentMode { get; set; } = ToolMode.Pen;
        public static int CurrentPenSlot { get; set; } = 0;
        public static int CurrentHighlighterSlot { get; set; } = 0;

        // SolidColorBrush is a WinUI object and therefore has thread affinity. Do
        // not create one from a static field initializer: this class is also used
        // by the background font-discovery task.
        public static SolidColorBrush[] PenColors = new SolidColorBrush[5];
        public static double[] PenThicknesses = new double[5] { 3, 3, 3, 3, 3 };

        public static SolidColorBrush[] HighlighterColors = new SolidColorBrush[3];
        public static double[] HighlighterThicknesses = new double[3] { 15, 15, 15 };

        public static EraserType EraserMode { get; set; } = EraserType.Stroke;
        public static double EraserThickness { get; set; } = 10;
        
        public static bool[] PenStraightLine = new bool[5];
        public static bool[] HighlighterStraightLine = new bool[3];
        
        public static bool[] HighlighterSnapToText = new bool[3];
        public static bool CurrentSnapToText 
        { 
            get 
            {
                if (CurrentMode == ToolMode.Highlighter || CurrentMode == ToolMode.TextHighlighter)
                    return HighlighterSnapToText[CurrentHighlighterSlot];
                return false;
            }
            set
            {
                if (CurrentMode == ToolMode.Highlighter || CurrentMode == ToolMode.TextHighlighter)
                    HighlighterSnapToText[CurrentHighlighterSlot] = value;
            }
        }
        
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
        }
        public static bool MouseDrawEnabled { get; set; } = false;
        public static string ToolbarDockPosition { get; set; } = "Top";
        public static string ScrollMode { get; set; } = "Vertical";

        public static SolidColorBrush PenColor => PenColors[CurrentPenSlot];
        public static double PenThickness => PenThicknesses[CurrentPenSlot];
        
        public static SolidColorBrush HighlighterColor => HighlighterColors[CurrentHighlighterSlot];
        public static double HighlighterThickness => HighlighterThicknesses[CurrentHighlighterSlot];
        
        public static SolidColorBrush TextHighlighterColor { get; set; } = null!;

        public static SolidColorBrush TextColor { get; set; } = null!;
        public static double TextFontSize { get; set; } = 20;
        public static string TextFontFamily { get; set; } = "Malgun Gothic";
        
        public static Dictionary<string, string> AvailableFonts { get; } = new Dictionary<string, string>();
        public static System.Collections.ObjectModel.ObservableCollection<string> SupportedFonts { get; } = new System.Collections.ObjectModel.ObservableCollection<string>();

        /// <summary>
        /// Creates UI-thread-affine brush instances. Call only from the app's UI
        /// thread before reading or writing one of the brush properties.
        /// </summary>
        public static void InitializeUiResources()
        {
            var defaultPenColors = new[] { Colors.Red, Colors.Blue, Colors.Black, Colors.Green, Colors.Purple };
            for (int i = 0; i < PenColors.Length; i++)
            {
                PenColors[i] ??= new SolidColorBrush(defaultPenColors[i]);
            }

            var defaultHighlighterColors = new[]
            {
                Microsoft.UI.ColorHelper.FromArgb(100, 255, 255, 0),
                Microsoft.UI.ColorHelper.FromArgb(100, 0, 255, 0),
                Microsoft.UI.ColorHelper.FromArgb(100, 0, 255, 255)
            };
            for (int i = 0; i < HighlighterColors.Length; i++)
            {
                HighlighterColors[i] ??= new SolidColorBrush(defaultHighlighterColors[i]);
            }

            TextColor ??= new SolidColorBrush(Colors.Black);
            TextHighlighterColor ??= new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, 255, 255, 0));
        }
        
        public static void LoadAvailableFonts() {
            var tempFonts = new Dictionary<string, string>();
            try {
                using (var fontsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts")) {
                    if (fontsKey != null) {
                        foreach (string fontName in fontsKey.GetValueNames()) {
                            string fontFile = fontsKey.GetValue(fontName).ToString();
                            if (!fontFile.Contains("\\")) {
                                fontFile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), fontFile);
                            }
                            
                            if (fontFile.EndsWith(".ttf", System.StringComparison.OrdinalIgnoreCase) || 
                                fontFile.EndsWith(".ttc", System.StringComparison.OrdinalIgnoreCase)) {
                                
                                string cleanName = fontName.Replace(" (TrueType)", "").Replace(" (OpenType)", "");
                                if (fontFile.EndsWith(".ttc", System.StringComparison.OrdinalIgnoreCase)) {
                                    fontFile += ",0";
                                }
                                
                                if (!tempFonts.ContainsKey(cleanName)) {
                                    tempFonts.Add(cleanName, fontFile);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font discovery failed: {ex}");
            }
            
            if (tempFonts.Count == 0) {
                tempFonts["Malgun Gothic"] = "c:\\windows\\fonts\\malgun.ttf";
                tempFonts["Batang"] = "c:\\windows\\fonts\\batang.ttc,0";
                tempFonts["Gulim"] = "c:\\windows\\fonts\\gulim.ttc,0";
                tempFonts["Arial"] = "c:\\windows\\fonts\\arial.ttf";
                tempFonts["Times New Roman"] = "c:\\windows\\fonts\\times.ttf";
                tempFonts["Consolas"] = "c:\\windows\\fonts\\consola.ttf";
            }
            
            var sortedKeys = new List<string>(tempFonts.Keys);
            sortedKeys.Sort();
            
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcher == null && App.MainWindow != null) dispatcher = App.MainWindow.DispatcherQueue;
            
            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => {
                    foreach (var kvp in tempFonts) {
                        AvailableFonts[kvp.Key] = kvp.Value;
                    }
                    SupportedFonts.Clear();
                    foreach (var k in sortedKeys) {
                        SupportedFonts.Add(k);
                    }
                });
            }
            else
            {
                foreach (var kvp in tempFonts) {
                    AvailableFonts[kvp.Key] = kvp.Value;
                }
                SupportedFonts.Clear();
                foreach (var k in sortedKeys) {
                    SupportedFonts.Add(k);
                }
            }
        }

        public static string GetFontPath(string fontFamily) {
            if (string.IsNullOrEmpty(fontFamily)) return "c:\\windows\\fonts\\malgun.ttf";
            
            if (AvailableFonts.TryGetValue(fontFamily, out string path)) {
                return path;
            }
            
            foreach (var kvp in AvailableFonts) {
                if (kvp.Key.StartsWith(fontFamily, StringComparison.OrdinalIgnoreCase) ||
                    fontFamily.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase)) {
                    return kvp.Value;
                }
            }
            
            return "c:\\windows\\fonts\\malgun.ttf";
        }

        public static void Load()
        {
            InitializeUiResources();
            System.Threading.Tasks.Task.Run(() => LoadAvailableFonts());
            
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("MouseDrawEnabled")) MouseDrawEnabled = (bool)settings.Values["MouseDrawEnabled"];
            if (settings.Values.ContainsKey("ToolbarDockPosition")) ToolbarDockPosition = (string)settings.Values["ToolbarDockPosition"];
            if (settings.Values.ContainsKey("ScrollMode")) ScrollMode = (string)settings.Values["ScrollMode"];
            if (settings.Values.ContainsKey("EraserMode")) EraserMode = (EraserType)(int)settings.Values["EraserMode"];
            if (settings.Values.ContainsKey("EraserThickness")) EraserThickness = (double)settings.Values["EraserThickness"];
            
            for (int i = 0; i < 5; i++)
            {
                if (settings.Values.ContainsKey($"PenColor_{i}"))
                    PenColors[i] = new SolidColorBrush(ColorFromString((string)settings.Values[$"PenColor_{i}"]));
                if (settings.Values.ContainsKey($"PenThickness_{i}"))
                    PenThicknesses[i] = (double)settings.Values[$"PenThickness_{i}"];
                if (settings.Values.ContainsKey($"PenStraightLine_{i}"))
                    PenStraightLine[i] = (bool)settings.Values[$"PenStraightLine_{i}"];
            }
            for (int i = 0; i < 3; i++)
            {
                if (settings.Values.ContainsKey($"HighlighterColor_{i}"))
                    HighlighterColors[i] = new SolidColorBrush(ColorFromString((string)settings.Values[$"HighlighterColor_{i}"]));
                if (settings.Values.ContainsKey($"HighlighterThickness_{i}"))
                    HighlighterThicknesses[i] = (double)settings.Values[$"HighlighterThickness_{i}"];
                if (settings.Values.ContainsKey($"HighlighterStraightLine_{i}"))
                    HighlighterStraightLine[i] = (bool)settings.Values[$"HighlighterStraightLine_{i}"];
                if (settings.Values.ContainsKey($"HighlighterSnapToText_{i}"))
                    HighlighterSnapToText[i] = (bool)settings.Values[$"HighlighterSnapToText_{i}"];
            }
            if (settings.Values.ContainsKey("TextColor"))
                TextColor = new SolidColorBrush(ColorFromString((string)settings.Values["TextColor"]));
            if (settings.Values.ContainsKey("TextFontSize"))
                TextFontSize = (double)settings.Values["TextFontSize"];
            if (settings.Values.ContainsKey("TextFontFamily"))
                TextFontFamily = (string)settings.Values["TextFontFamily"];
            if (settings.Values.ContainsKey("TextHighlighterColor"))
                TextHighlighterColor = new SolidColorBrush(ColorFromString((string)settings.Values["TextHighlighterColor"]));
        }

        public static void Save()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["MouseDrawEnabled"] = MouseDrawEnabled;
            settings.Values["ToolbarDockPosition"] = ToolbarDockPosition;
            settings.Values["ScrollMode"] = ScrollMode;
            settings.Values["EraserMode"] = (int)EraserMode;
            settings.Values["EraserThickness"] = EraserThickness;
            for (int i = 0; i < 5; i++)
            {
                settings.Values[$"PenColor_{i}"] = PenColors[i].Color.ToString();
                settings.Values[$"PenThickness_{i}"] = PenThicknesses[i];
                settings.Values[$"PenStraightLine_{i}"] = PenStraightLine[i];
            }
            for (int i = 0; i < 3; i++)
            {
                settings.Values[$"HighlighterColor_{i}"] = HighlighterColors[i].Color.ToString();
                settings.Values[$"HighlighterThickness_{i}"] = HighlighterThicknesses[i];
                settings.Values[$"HighlighterStraightLine_{i}"] = HighlighterStraightLine[i];
                settings.Values[$"HighlighterSnapToText_{i}"] = HighlighterSnapToText[i];
            }
            settings.Values["TextColor"] = TextColor.Color.ToString();
            settings.Values["TextFontSize"] = TextFontSize;
            settings.Values["TextFontFamily"] = TextFontFamily;
            settings.Values["TextHighlighterColor"] = TextHighlighterColor.Color.ToString();
        }

        private static Windows.UI.Color ColorFromString(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)System.Convert.ToUInt32(hex.Substring(0, 2), 16);
            byte r = (byte)System.Convert.ToUInt32(hex.Substring(2, 2), 16);
            byte g = (byte)System.Convert.ToUInt32(hex.Substring(4, 2), 16);
            byte b = (byte)System.Convert.ToUInt32(hex.Substring(6, 2), 16);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }
    }

    public class PdfPageData
    {
        public uint PageIndex { get; set; }
        public PdfDocument Document { get; set; }
        public List<Polyline> Strokes { get; set; } = new List<Polyline>();
        public List<ExtractedStroke> ExtractedStrokes { get; set; } = new List<ExtractedStroke>();
        public List<Microsoft.UI.Xaml.Controls.TextBox> Texts { get; set; } = new List<Microsoft.UI.Xaml.Controls.TextBox>();
        public List<PdfTextWord> Words { get; set; } = new List<PdfTextWord>();
        public List<ExtractedTextAnnotation> ExtractedTexts { get; set; } = new List<ExtractedTextAnnotation>();

    }
}


namespace PDFaNoter
{
    public class ExtractedStroke
    {
        public Windows.UI.Color Color { get; set; }
        public double Thickness { get; set; }
        public List<Windows.Foundation.Point> Points { get; set; } = new List<Windows.Foundation.Point>();
    }

    public class PdfTextWord
    {
        public string Text { get; set; }
        public Windows.Foundation.Rect Bounds { get; set; }
    }
    public class ExtractedTextAnnotation
    {
        public string Text { get; set; }
        public double CanvasLeft { get; set; }
        public double CanvasTop { get; set; }
        public Windows.UI.Color Color { get; set; }
        public double FontSize { get; set; }
        public string FontFamily { get; set; }
    }
}
