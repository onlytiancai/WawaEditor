using System.Text.RegularExpressions;

namespace WawaEditor
{
    public class TextEditorTabPage : TabPage
    {
        public RichTextBox TextBox { get; private set; }
        public Panel LineNumberPanel { get; private set; }
        public string FilePath { get; set; }
        public bool IsModified { get; set; }
        private int _lastLineCount = 0;
        private Stack<UndoRedoAction> _undoStack = new Stack<UndoRedoAction>();
        private Stack<UndoRedoAction> _redoStack = new Stack<UndoRedoAction>();
        private bool _isUndoRedo = false;

        public TextEditorTabPage(string title = "Untitled")
        {
            Text = title;
            FilePath = string.Empty;
            IsModified = false;

            // Create line number panel
            LineNumberPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 40,
                BackColor = Color.LightGray
            };
            LineNumberPanel.Paint += LineNumberPanel_Paint;

            // Create text box
            TextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                AcceptsTab = true,
                Font = new Font("Consolas", 10),
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both,
                DetectUrls = false
            };

            TextBox.TextChanged += TextBox_TextChanged;
            TextBox.VScroll += TextBox_VScroll;
            TextBox.SelectionChanged += TextBox_SelectionChanged;

            // Create a container panel to hold both controls
            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            containerPanel.Controls.Add(TextBox);
            containerPanel.Controls.Add(LineNumberPanel);
            Controls.Add(containerPanel);

            // Save initial state for undo
            SaveState();
        }

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            if (!_isUndoRedo)
            {
                SaveState();
                _redoStack.Clear();
            }

            IsModified = true;
            UpdateLineNumbers();
            
            // Update tab text to show modified status
            if (IsModified && !Text.EndsWith("*"))
            {
                Text = Text + "*";
            }
        }

        private void TextBox_VScroll(object? sender, EventArgs e)
        {
            LineNumberPanel.Invalidate();
        }

        private void TextBox_SelectionChanged(object? sender, EventArgs e)
        {
            LineNumberPanel.Invalidate();
        }

        private void LineNumberPanel_Paint(object? sender, PaintEventArgs e)
        {
            // Get the first visible line
            int firstVisibleLine = TextBox.GetLineFromCharIndex(TextBox.GetCharIndexFromPosition(new Point(0, 0)));
            
            // Get total visible lines
            int visibleLines = TextBox.Height / TextBox.Font.Height;
            
            // Draw line numbers
            using (Font font = new Font(TextBox.Font.FontFamily, TextBox.Font.Size))
            {
                for (int i = firstVisibleLine; i <= firstVisibleLine + visibleLines && i < TextBox.Lines.Length; i++)
                {
                    int lineY = (i - firstVisibleLine) * TextBox.Font.Height + 2;
                    e.Graphics.DrawString((i + 1).ToString(), font, Brushes.DarkBlue, LineNumberPanel.Width - 25, lineY);
                }
            }
        }

        private void UpdateLineNumbers()
        {
            int lineCount = TextBox.Lines.Length;
            if (lineCount != _lastLineCount)
            {
                _lastLineCount = lineCount;
                LineNumberPanel.Invalidate();
            }
        }

        public void SaveState()
        {
            _undoStack.Push(new UndoRedoAction(TextBox.Text, TextBox.SelectionStart));
        }

        public bool CanUndo()
        {
            return _undoStack.Count > 1; // Keep at least one state
        }

        public bool CanRedo()
        {
            return _redoStack.Count > 0;
        }

        public void Undo()
        {
            if (CanUndo())
            {
                // Save current state to redo stack
                _redoStack.Push(_undoStack.Pop());
                
                // Apply previous state
                _isUndoRedo = true;
                UndoRedoAction action = _undoStack.Peek();
                TextBox.Text = action.Text;
                TextBox.SelectionStart = action.CursorPosition;
                _isUndoRedo = false;
            }
        }

        public void Redo()
        {
            if (CanRedo())
            {
                // Get state from redo stack
                UndoRedoAction action = _redoStack.Pop();
                
                // Save current state to undo stack
                _undoStack.Push(action);
                
                // Apply state
                _isUndoRedo = true;
                TextBox.Text = action.Text;
                TextBox.SelectionStart = action.CursorPosition;
                _isUndoRedo = false;
            }
        }

        public void SetWordWrap(bool enabled)
        {
            TextBox.WordWrap = enabled;
        }

        public void SetFont(Font font)
        {
            TextBox.Font = font;
            LineNumberPanel.Invalidate();
        }

        public void Find(string searchText, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            int startIndex = TextBox.SelectionStart + TextBox.SelectionLength;
            if (startIndex >= TextBox.Text.Length)
                startIndex = 0;

            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            int index;
            if (wholeWord)
            {
                string pattern = $"\\b{Regex.Escape(searchText)}\\b";
                RegexOptions options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                Match match = Regex.Match(TextBox.Text.Substring(startIndex), pattern, options);
                index = match.Success ? startIndex + match.Index : -1;
            }
            else
            {
                index = TextBox.Text.IndexOf(searchText, startIndex, comparison);
            }

            if (index >= 0)
            {
                TextBox.Select(index, searchText.Length);
                TextBox.Focus();
            }
            else if (startIndex > 0)
            {
                // Wrap around to beginning
                TextBox.SelectionStart = 0;
                TextBox.SelectionLength = 0;
                Find(searchText, matchCase, wholeWord);
            }
        }

        public void ReplaceSelected(string replaceText)
        {
            if (TextBox.SelectionLength > 0)
            {
                TextBox.SelectedText = replaceText;
            }
        }

        public void ReplaceAll(string searchText, string replaceText, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            string text = TextBox.Text;
            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            if (wholeWord)
            {
                string pattern = $"\\b{Regex.Escape(searchText)}\\b";
                RegexOptions options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                text = Regex.Replace(text, pattern, replaceText);
            }
            else
            {
                text = Regex.Replace(
                    text, 
                    Regex.Escape(searchText), 
                    replaceText.Replace("$", "$$"), 
                    matchCase ? RegexOptions.None : RegexOptions.IgnoreCase
                );
            }
            
            TextBox.Text = text;
        }
    }

    public class UndoRedoAction
    {
        public string Text { get; }
        public int CursorPosition { get; }

        public UndoRedoAction(string text, int cursorPosition)
        {
            Text = text;
            CursorPosition = cursorPosition;
        }
    }
}