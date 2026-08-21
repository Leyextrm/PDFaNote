$content = Get-Content 'CommandHistory.cs' -Raw
$macroCode = "
    public class MacroCommand : IUndoableCommand
    {
        private List<IUndoableCommand> _commands;
        public MacroCommand(List<IUndoableCommand> commands) { _commands = commands; }
        public void Undo() { for (int i = _commands.Count - 1; i >= 0; i--) _commands[i].Undo(); }
        public void Redo() { foreach (var cmd in _commands) cmd.Redo(); }
    }
"
$content = $content.Replace('public class CommandHistory', $macroCode + '    public class CommandHistory')
Set-Content -Path 'CommandHistory.cs' -Value $content -Encoding UTF8
