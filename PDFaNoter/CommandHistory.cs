using System.Collections.Generic;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Controls;

namespace PDFaNoter
{
    public interface IUndoableCommand
    {
        void Undo();
        void Redo();
    }

    public class AddStrokeCommand : IUndoableCommand
    {
        private PdfPageData _pageData;
        private Polyline _stroke;
        private Canvas _canvas;

        public AddStrokeCommand(PdfPageData pageData, Polyline stroke, Canvas canvas)
        {
            _pageData = pageData;
            _stroke = stroke;
            _canvas = canvas;
        }

        public void Undo()
        {
            _pageData.Strokes.Remove(_stroke);
            _canvas.Children.Remove(_stroke);
        }

        public void Redo()
        {
            _pageData.Strokes.Add(_stroke);
            _canvas.Children.Add(_stroke);
        }
    }

    public class RemoveStrokesCommand : IUndoableCommand
    {
        private PdfPageData _pageData;
        private List<Polyline> _removedStrokes;
        private Canvas _canvas;

        public RemoveStrokesCommand(PdfPageData pageData, List<Polyline> removedStrokes, Canvas canvas)
        {
            _pageData = pageData;
            _removedStrokes = removedStrokes;
            _canvas = canvas;
        }

        public void Undo()
        {
            foreach (var stroke in _removedStrokes)
            {
                _pageData.Strokes.Add(stroke);
                _canvas.Children.Add(stroke);
            }
        }

        public void Redo()
        {
            foreach (var stroke in _removedStrokes)
            {
                _pageData.Strokes.Remove(stroke);
                _canvas.Children.Remove(stroke);
            }
        }
    }

    public class ReplaceStrokesCommand : IUndoableCommand
    {
        private PdfPageData _pageData;
        private List<Polyline> _removedStrokes;
        private List<Polyline> _addedStrokes;
        private Canvas _canvas;

        public ReplaceStrokesCommand(PdfPageData pageData, List<Polyline> removedStrokes, List<Polyline> addedStrokes, Canvas canvas)
        {
            _pageData = pageData;
            _removedStrokes = removedStrokes;
            _addedStrokes = addedStrokes;
            _canvas = canvas;
        }

        public void Undo()
        {
            foreach (var s in _addedStrokes) { _pageData.Strokes.Remove(s); _canvas.Children.Remove(s); }
            foreach (var s in _removedStrokes) { _pageData.Strokes.Add(s); _canvas.Children.Add(s); }
        }

        public void Redo()
        {
            foreach (var s in _removedStrokes) { _pageData.Strokes.Remove(s); _canvas.Children.Remove(s); }
            foreach (var s in _addedStrokes) { _pageData.Strokes.Add(s); _canvas.Children.Add(s); }
        }
    }

    
    public class MoveElementsCommand : IUndoableCommand
    {
        private List<Microsoft.UI.Xaml.UIElement> _elements;
        private double _dx;
        private double _dy;

        public MoveElementsCommand(List<Microsoft.UI.Xaml.UIElement> elements, double dx, double dy)
        {
            _elements = new List<Microsoft.UI.Xaml.UIElement>(elements);
            _dx = dx;
            _dy = dy;
        }

        public void Undo()
        {
            foreach (var elem in _elements)
            {
                if (elem is Polyline poly)
                {
                    for (int i = 0; i < poly.Points.Count; i++)
                    {
                        var pt = poly.Points[i];
                        poly.Points[i] = new Windows.Foundation.Point(pt.X - _dx, pt.Y - _dy);
                    }
                }
                else if (elem is TextBox tb)
                {
                    Canvas.SetLeft(tb, Canvas.GetLeft(tb) - _dx);
                    Canvas.SetTop(tb, Canvas.GetTop(tb) - _dy);
                }
            }
        }

        public void Redo()
        {
            foreach (var elem in _elements)
            {
                if (elem is Polyline poly)
                {
                    for (int i = 0; i < poly.Points.Count; i++)
                    {
                        var pt = poly.Points[i];
                        poly.Points[i] = new Windows.Foundation.Point(pt.X + _dx, pt.Y + _dy);
                    }
                }
                else if (elem is TextBox tb)
                {
                    Canvas.SetLeft(tb, Canvas.GetLeft(tb) + _dx);
                    Canvas.SetTop(tb, Canvas.GetTop(tb) + _dy);
                }
            }
        }
    }

    public class MacroCommand : IUndoableCommand
    {
        private List<IUndoableCommand> _commands;
        public MacroCommand(List<IUndoableCommand> commands) { _commands = commands; }
        public void Undo() { for (int i = _commands.Count - 1; i >= 0; i--) _commands[i].Undo(); }
        public void Redo() { foreach (var cmd in _commands) cmd.Redo(); }
    }
    public class CommandHistory
    {
        private Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
        
        public bool HasUnsavedChanges { get; set; } = false;

        public void Add(IUndoableCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
            HasUnsavedChanges = true;
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var cmd = _undoStack.Pop();
                cmd.Undo();
                _redoStack.Push(cmd);
                HasUnsavedChanges = true;
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var cmd = _redoStack.Pop();
                cmd.Redo();
                _undoStack.Push(cmd);
                HasUnsavedChanges = true;
            }
        }
    }
}

