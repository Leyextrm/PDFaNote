$content = Get-Content 'CommandHistory.cs' -Raw
$content = $content -replace '(?s)public void Add\(IUndoableCommand command\)\s*\{\s*_undoStack.Push\(command\);\s*_redoStack.Clear\(\);\s*\}', 
'public void Add(IUndoableCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
            HasUnsavedChanges = true;
        }'
$content = $content -replace '(?s)public void Undo\(\)\s*\{.*?\}', 
'public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var cmd = _undoStack.Pop();
                cmd.Undo();
                _redoStack.Push(cmd);
                HasUnsavedChanges = true;
            }
        }'
$content = $content -replace '(?s)public void Redo\(\)\s*\{.*?\}', 
'public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var cmd = _redoStack.Pop();
                cmd.Redo();
                _undoStack.Push(cmd);
                HasUnsavedChanges = true;
            }
        }'
Set-Content -Path 'CommandHistory.cs' -Value $content -Encoding UTF8
