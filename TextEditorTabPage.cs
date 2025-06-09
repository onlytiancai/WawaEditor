using System.Text.RegularExpressions;

namespace WawaEditor
{
    public class TextEditorTabPage : TabPage
    {
        public FastTextBox? TextBox { get; private set; }
        public Panel? LineNumberPanel { get; private set; }
        public string? FilePath { get; set; } = string.Empty;
        public bool IsModified { get; set; }
        public bool ShowLineNumbers { get; private set; } = true;
        
        private int _lastLineCount = 0;
        private Stack<UndoRedoAction> _undoStack = new Stack<UndoRedoAction>();
        private Stack<UndoRedoAction> _redoStack = new Stack<UndoRedoAction>();
        private bool _isUndoRedo = false;
        private System.Windows.Forms.Timer? _lineNumberUpdateTimer;
        private int _lineNumberWidth = 40;

        public TextEditorTabPage(string title = "Untitled")
        {
            try
            {
                Text = title;
                FilePath = string.Empty;
                IsModified = false;

                // Create standard RichTextBox first
                TextBox = new FastTextBox();
                TextBox.Dock = DockStyle.Fill;
                TextBox.AcceptsTab = true;
                TextBox.Font = new Font("Consolas", 10);
                TextBox.WordWrap = false;
                TextBox.ScrollBars = RichTextBoxScrollBars.Both;
                TextBox.Text = "";

                // Create line number panel
                LineNumberPanel = new Panel();
                LineNumberPanel.Dock = DockStyle.Left;
                LineNumberPanel.Width = _lineNumberWidth;
                LineNumberPanel.BackColor = Color.LightGray;
                LineNumberPanel.Visible = ShowLineNumbers;
                LineNumberPanel.Paint += LineNumberPanel_Paint;

                // Create a simple timer
                _lineNumberUpdateTimer = new System.Windows.Forms.Timer();
                _lineNumberUpdateTimer.Interval = 200;
                _lineNumberUpdateTimer.Tick += (s, e) => {
                    _lineNumberUpdateTimer.Stop();
                    if (ShowLineNumbers && LineNumberPanel != null)
                        LineNumberPanel.Invalidate();
                };

                // Add event handlers
                TextBox.TextChanged += TextBox_TextChanged;
                TextBox.VScroll += TextBox_VScroll;
                TextBox.SelectionChanged += TextBox_SelectionChanged;

                // Create container panel
                Panel containerPanel = new Panel();
                containerPanel.Dock = DockStyle.Fill;
                
                // Add controls in correct order
                containerPanel.Controls.Add(TextBox);
                containerPanel.Controls.Add(LineNumberPanel);
                Controls.Add(containerPanel);
                
                // Save initial state for undo
                _undoStack.Push(new UndoRedoAction("", 0));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing editor: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (ShowLineNumbers)
            {
                // Use timer to throttle updates
                _lineNumberUpdateTimer.Stop();
                _lineNumberUpdateTimer.Start();
            }
        }

        private void TextBox_SelectionChanged(object? sender, EventArgs e)
        {
            // No need to update line numbers on selection change
        }

        private void LineNumberPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (!ShowLineNumbers) return;

            try
            {
                // Use simple rendering settings
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

                // Get the first visible line safely
                int firstVisibleLine = 0;
                try
                {
                    firstVisibleLine = TextBox.GetLineFromCharIndex(TextBox.GetCharIndexFromPosition(new Point(0, 0)));
                }
                catch
                {
                    // Fallback if getting line index fails
                    firstVisibleLine = 0;
                }
                
                // Get total visible lines
                int lineHeight = TextBox.Font.Height;
                int visibleLines = (TextBox.ClientSize.Height / lineHeight) + 1;
                
                // Draw line numbers with simple approach
                using (Font font = new Font(TextBox.Font.FontFamily, TextBox.Font.Size))
                {
                    // Limit the number of lines to draw to avoid performance issues
                    int lastLine = Math.Min(firstVisibleLine + visibleLines, TextBox.Lines.Length);
                    lastLine = Math.Min(lastLine, firstVisibleLine + 1000); // Cap at 1000 visible lines
                    
                    for (int i = firstVisibleLine; i < lastLine; i++)
                    {
                        int lineY = (i - firstVisibleLine) * lineHeight + 2;
                        string lineNumber = (i + 1).ToString();
                        e.Graphics.DrawString(lineNumber, font, Brushes.DarkBlue, 2, lineY);
                    }
                }
            }
            catch
            {
                // Ignore any rendering errors
            }
        }

        private void UpdateLineNumbers()
        {
            if (!ShowLineNumbers) return;

            int lineCount = TextBox.Lines.Length;
            if (lineCount != _lastLineCount)
            {
                _lastLineCount = lineCount;
                
                // Update line number panel width based on number of digits
                int digits = lineCount > 0 ? (int)Math.Log10(lineCount) + 1 : 1;
                int requiredWidth = (digits * 10) + 10; // Approximate width based on digits
                
                if (requiredWidth != _lineNumberWidth)
                {
                    _lineNumberWidth = requiredWidth;
                    LineNumberPanel.Width = _lineNumberWidth;
                }
                
                // Use timer to throttle updates
                _lineNumberUpdateTimer.Stop();
                _lineNumberUpdateTimer.Start();
            }
        }

        public void ToggleLineNumbers(bool show)
        {
            ShowLineNumbers = show;
            LineNumberPanel.Visible = show;
            
            if (show)
            {
                UpdateLineNumbers();
                LineNumberPanel.Invalidate();
            }
        }

        public void SaveState()
        {
            try
            {
                // Limit undo stack size to prevent memory issues with large files
                if (_undoStack.Count > 100)
                {
                    var tempStack = new Stack<UndoRedoAction>();
                    for (int i = 0; i < 50; i++)
                    {
                        if (_undoStack.Count > 1) // Keep at least one state
                            tempStack.Push(_undoStack.Pop());
                        else
                            break;
                    }
                    _undoStack = new Stack<UndoRedoAction>();
                    while (tempStack.Count > 0)
                        _undoStack.Push(tempStack.Pop());
                }
                
                int selectionStart = 0;
                try { selectionStart = TextBox.SelectionStart; } catch { }
                
                _undoStack.Push(new UndoRedoAction(TextBox.Text, selectionStart));
            }
            catch
            {
                // Ignore errors during state saving
                if (_undoStack.Count == 0)
                    _undoStack.Push(new UndoRedoAction("", 0));
            }
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
            if (ShowLineNumbers)
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
                    replaceText.Replace("$", "$"), 
                    matchCase ? RegexOptions.None : RegexOptions.IgnoreCase
                );
            }
            
            TextBox.Text = text;
        }
    }

    // Standard RichTextBox without custom overrides to ensure stability
    public class FastTextBox : RichTextBox
    {
        public FastTextBox()
        {
            // Use standard settings for maximum compatibility
            this.MaxLength = int.MaxValue;
            this.DetectUrls = false;
            this.HideSelection = false;
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