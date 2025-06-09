using System.Text.RegularExpressions;

namespace WawaEditor
{
    public class TextEditorTabPage : TabPage
    {
        public FastTextBox? TextBox { get; private set; }
        public Panel? LineNumberPanel { get; private set; }
        public string? FilePath { get; set; } = string.Empty;
        public bool IsModified { get; set; }
        public bool ShowLineNumbers { get; private set; } = false;
        
        private int _lastLineCount = 0;
        private Stack<UndoRedoAction> _undoStack = new Stack<UndoRedoAction>();
        private Stack<UndoRedoAction> _redoStack = new Stack<UndoRedoAction>();
        private bool _isUndoRedo = false;
        private System.Windows.Forms.Timer? _lineNumberUpdateTimer;
        private int _lineNumberWidth = 40;
        
        // 状态栏信息委托
        public delegate void StatusUpdateHandler(string statusText);
        public event StatusUpdateHandler? OnStatusUpdate;

        public TextEditorTabPage(string title = "Untitled")
        {
            try
            {
                Text = title;
                FilePath = string.Empty;
                IsModified = false;
                ShowLineNumbers = false; // 默认不显示行号

                // 先创建标准RichTextBox
                TextBox = new FastTextBox();
                TextBox.Dock = DockStyle.Fill;
                TextBox.AcceptsTab = true;
                TextBox.Font = new Font("Consolas", 10);
                TextBox.WordWrap = false;
                TextBox.ScrollBars = RichTextBoxScrollBars.Both;
                TextBox.Text = "";
                
                // 添加右键菜单
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("剪切", null, (s, e) => TextBox.Cut());
                contextMenu.Items.Add("复制", null, (s, e) => TextBox.Copy());
                contextMenu.Items.Add("粘贴", null, (s, e) => TextBox.Paste());
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("全选", null, (s, e) => TextBox.SelectAll());
                TextBox.ContextMenuStrip = contextMenu;

                // 创建行号面板，但默认不显示
                LineNumberPanel = new Panel();
                LineNumberPanel.Dock = DockStyle.Left;
                LineNumberPanel.Width = _lineNumberWidth;
                LineNumberPanel.BackColor = Color.LightGray;
                LineNumberPanel.Visible = false; // 默认不显示
                LineNumberPanel.Paint += LineNumberPanel_Paint;

                // 创建简单计时器
                _lineNumberUpdateTimer = new System.Windows.Forms.Timer();
                _lineNumberUpdateTimer.Interval = 200;
                _lineNumberUpdateTimer.Tick += (s, e) => {
                    _lineNumberUpdateTimer.Stop();
                    if (ShowLineNumbers && LineNumberPanel != null)
                        LineNumberPanel.Invalidate();
                };

                // 添加事件处理程序
                TextBox.TextChanged += TextBox_TextChanged;
                TextBox.VScroll += TextBox_VScroll;
                TextBox.SelectionChanged += TextBox_SelectionChanged;

                // 创建容器面板
                Panel containerPanel = new Panel();
                containerPanel.Dock = DockStyle.Fill;
                
                // 按正确顺序添加控件
                containerPanel.Controls.Add(TextBox);
                containerPanel.Controls.Add(LineNumberPanel);
                Controls.Add(containerPanel);
                
                // 保存初始状态用于撤销
                _undoStack.Push(new UndoRedoAction("", 0));
                
                // 初始化状态栏信息
                UpdateStatusInfo();
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
            
            // 更新状态栏信息
            UpdateStatusInfo();
        }

        private void TextBox_VScroll(object? sender, EventArgs e)
        {
            if (ShowLineNumbers)
            {
                // Use timer to throttle updates
                if (_lineNumberUpdateTimer != null)
                {
                    _lineNumberUpdateTimer.Stop();
                    _lineNumberUpdateTimer.Start();
                }
            }
            
            // 更新状态栏信息
            UpdateStatusInfo();
        }

        private void TextBox_SelectionChanged(object? sender, EventArgs e)
        {
            // 更新状态栏信息
            UpdateStatusInfo();
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
                    if (TextBox != null)
                        firstVisibleLine = TextBox.GetLineFromCharIndex(TextBox.GetCharIndexFromPosition(new Point(0, 0)));
                }
                catch
                {
                    // Fallback if getting line index fails
                    firstVisibleLine = 0;
                }
                
                // Get total visible lines
                int lineHeight = TextBox?.Font.Height ?? 15;
                int visibleLines = ((TextBox?.ClientSize.Height ?? 0) / lineHeight) + 1;
                
                // Draw line numbers with simple approach
                using (Font font = new Font(TextBox?.Font.FontFamily ?? new FontFamily("Consolas"), TextBox?.Font.Size ?? 10))
                {
                    // Limit the number of lines to draw to avoid performance issues
                    int lastLine = Math.Min(firstVisibleLine + visibleLines, TextBox?.Lines.Length ?? 0);
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

            int lineCount = TextBox?.Lines.Length ?? 0;
            if (lineCount != _lastLineCount)
            {
                _lastLineCount = lineCount;
                
                // Update line number panel width based on number of digits
                int digits = lineCount > 0 ? (int)Math.Log10(lineCount) + 1 : 1;
                int requiredWidth = (digits * 10) + 10; // Approximate width based on digits
                
                if (requiredWidth != _lineNumberWidth)
                {
                    _lineNumberWidth = requiredWidth;
                    if (LineNumberPanel != null)
                        LineNumberPanel.Width = _lineNumberWidth;
                }
                
                // Use timer to throttle updates
                if (_lineNumberUpdateTimer != null)
                {
                    _lineNumberUpdateTimer.Stop();
                    _lineNumberUpdateTimer.Start();
                }
            }
        }

        public void ToggleLineNumbers(bool show)
        {
            ShowLineNumbers = show;
            System.Diagnostics.Debug.WriteLine($"切换行号显示: {show}");
            
            if (LineNumberPanel != null)
            {
                LineNumberPanel.Visible = show;
                
                // 确保面板在正确的位置
                if (LineNumberPanel.Parent != null && LineNumberPanel.Parent.Controls.Count > 0)
                {
                    // 确保行号面板在最上层
                    LineNumberPanel.BringToFront();
                }
                
                System.Diagnostics.Debug.WriteLine($"行号面板可见性: {LineNumberPanel.Visible}");
            }
            
            if (show)
            {
                UpdateLineNumbers();
                LineNumberPanel?.Invalidate();
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
                try { if (TextBox != null) selectionStart = TextBox.SelectionStart; } catch { }
                
                _undoStack.Push(new UndoRedoAction(TextBox?.Text ?? "", selectionStart));
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
            if (CanUndo() && TextBox != null)
            {
                try
                {
                    // 暂停布局和绘制以减少闪烁
                    TextBox.SuspendLayout();
                    
                    // 保存当前状态到重做栈
                    _redoStack.Push(_undoStack.Pop());
                    
                    // 应用前一个状态
                    _isUndoRedo = true;
                    UndoRedoAction action = _undoStack.Peek();
                    
                    // 使用BeginUpdate/EndUpdate减少重绘
                    TextBox.BeginUpdate();
                    TextBox.Text = action.Text;
                    TextBox.SelectionStart = action.CursorPosition;
                    TextBox.EndUpdate();
                    
                    _isUndoRedo = false;
                }
                finally
                {
                    // 恢复布局
                    if (TextBox != null)
                        TextBox.ResumeLayout();
                }
                
                // 更新状态栏信息
                UpdateStatusInfo();
            }
        }

        public void Redo()
        {
            if (CanRedo() && TextBox != null)
            {
                try
                {
                    // 暂停布局和绘制以减少闪烁
                    TextBox.SuspendLayout();
                    
                    // 从重做栈获取状态
                    UndoRedoAction action = _redoStack.Pop();
                    
                    // 保存当前状态到撤销栈
                    _undoStack.Push(action);
                    
                    // 应用状态
                    _isUndoRedo = true;
                    
                    // 使用BeginUpdate/EndUpdate减少重绘
                    TextBox.BeginUpdate();
                    TextBox.Text = action.Text;
                    TextBox.SelectionStart = action.CursorPosition;
                    TextBox.EndUpdate();
                    
                    _isUndoRedo = false;
                }
                finally
                {
                    // 恢复布局
                    if (TextBox != null)
                        TextBox.ResumeLayout();
                }
                
                // 更新状态栏信息
                UpdateStatusInfo();
            }
        }

        public void SetWordWrap(bool enabled)
        {
            if (TextBox != null)
            {
                // 先设置滚动条，再设置自动换行
                if (enabled)
                    TextBox.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
                else
                    TextBox.ScrollBars = RichTextBoxScrollBars.Both;
                
                TextBox.WordWrap = enabled;
                
                // 输出调试信息
                System.Diagnostics.Debug.WriteLine($"设置自动换行: {enabled}, 实际状态: {TextBox.WordWrap}");
            }
        }

        public void SetFont(Font font)
        {
            if (TextBox != null)
                TextBox.Font = font;
            
            if (ShowLineNumbers)
                LineNumberPanel?.Invalidate();
        }

        public void Find(string searchText, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(searchText) || TextBox == null)
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
            
            // 更新状态栏信息
            UpdateStatusInfo();
        }

        public void ReplaceSelected(string replaceText)
        {
            if (TextBox != null && TextBox.SelectionLength > 0)
            {
                TextBox.SelectedText = replaceText;
                UpdateStatusInfo();
            }
        }

        public void ReplaceAll(string searchText, string replaceText, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(searchText) || TextBox == null)
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
            UpdateStatusInfo();
        }
        
        // 更新状态栏信息
        public void UpdateStatusInfo()
        {
            if (TextBox == null) return;
            
            try
            {
                // 获取当前行号和列号
                int pos = TextBox.SelectionStart;
                int line = TextBox.GetLineFromCharIndex(pos);
                int column = pos - TextBox.GetFirstCharIndexFromLine(line);
                
                // 获取总行数和字符数
                int totalLines = TextBox.Lines.Length;
                int totalChars = TextBox.Text.Length;
                
                // 获取选中文本的字符数
                int selectedChars = TextBox.SelectionLength;
                
                // 构建状态信息
                string statusText = $"行: {line + 1}/{totalLines}  列: {column + 1}  字符: {totalChars}";
                
                // 如果有选中文本，显示选中的字符数
                if (selectedChars > 0)
                {
                    statusText += $"  选中: {selectedChars}";
                }
                
                // 触发状态更新事件
                OnStatusUpdate?.Invoke(statusText);
            }
            catch
            {
                // 忽略任何错误
            }
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
        
        // 添加BeginUpdate和EndUpdate方法减少重绘
        public void BeginUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }
        
        public void EndUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            this.Invalidate();
        }
        
        // Win32 API
        private const int WM_SETREDRAW = 0x0B;
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
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