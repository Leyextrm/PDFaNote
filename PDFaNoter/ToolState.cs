using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Shapes;
using Windows.Data.Pdf;

namespace PDFaNoter
{
    public enum ToolMode { Pen, Highlighter, Eraser }
    public enum EraserType { Stroke, Pixel }

    public static class ToolState
    {
        public static ToolMode CurrentMode { get; set; } = ToolMode.Pen;
        public static int CurrentPenSlot { get; set; } = 0;
        public static int CurrentHighlighterSlot { get; set; } = 0;

        public static SolidColorBrush[] PenColors = new SolidColorBrush[5] 
        {
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Blue),
            new SolidColorBrush(Colors.Black),
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Purple)
        };
        public static double[] PenThicknesses = new double[5] { 3, 3, 3, 3, 3 };

        public static SolidColorBrush[] HighlighterColors = new SolidColorBrush[3]
        {
            new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, 255, 255, 0)),
            new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, 0, 255, 0)),
            new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, 0, 255, 255))
        };
        public static double[] HighlighterThicknesses = new double[3] { 15, 15, 15 };

        public static EraserType EraserMode { get; set; } = EraserType.Stroke;
        public static double EraserThickness { get; set; } = 10;
        
        public static bool[] PenStraightLine = new bool[5];
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
        }
        public static bool MouseDrawEnabled { get; set; } = false;

        public static SolidColorBrush PenColor => PenColors[CurrentPenSlot];
        public static double PenThickness => PenThicknesses[CurrentPenSlot];
        
        public static SolidColorBrush HighlighterColor => HighlighterColors[CurrentHighlighterSlot];
        public static double HighlighterThickness => HighlighterThicknesses[CurrentHighlighterSlot];
    }

    public class PdfPageData
    {
        public uint PageIndex { get; set; }
        public PdfDocument Document { get; set; }
        public List<Polyline> Strokes { get; set; } = new List<Polyline>();
        public List<ExtractedStroke> ExtractedStrokes { get; set; } = new List<ExtractedStroke>();
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
}


