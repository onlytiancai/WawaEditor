using System.Text.RegularExpressions;

namespace WawaEditor
{
    public class TextEditorTabPage : TabPage
    {
        public FastTextBox? TextBox { get; private set; }
        public string? FilePath { get; set; } = string.Empty;
        public bool IsModified { get; set; }
        
        private Stack<UndoRedoAction> _undoStack = new Stack<UndoRedoAction>();
        private Stack<UndoRedoAction> _redoStack = new Stack<UndoRedoAction>();
        private bool _isUndoRedo = false;
        
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

                // 创建RichTextBox
                TextBox = new FastTextBox();
                TextBox.Dock = DockStyle.Fill;
                TextBox.AcceptsTab = true;
                
                // 使用默认字体，稍后会由MainForm应用配置中的字体
                TextBox.Font = new Font("Consolas", 10);
                
                // 根据配置设置自动换行
                bool wordWrap = AppConfig.Instance.WordWrap;
                TextBox.WordWrap = wordWrap;
                TextBox.ScrollBars = wordWrap ? RichTextBoxScrollBars.ForcedVertical : RichTextBoxScrollBars.Both;
                TextBox.Text = "";
                
                // 添加右键菜单
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("剪切", null, (s, e) => TextBox.Cut());
                contextMenu.Items.Add("复制", null, (s, e) => TextBox.Copy());
                contextMenu.Items.Add("粘贴", null, (s, e) => TextBox.Paste());
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("全选", null, (s, e) => TextBox.SelectAll());
                TextBox.ContextMenuStrip = contextMenu;

                // 添加事件处理程序
                TextBox.TextChanged += TextBox_TextChanged;
                TextBox.VScroll += TextBox_VScroll;
                TextBox.SelectionChanged += TextBox_SelectionChanged;

                // 添加控件
                Controls.Add(TextBox);
                
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
            // 更新状态栏信息
            UpdateStatusInfo();
        }

        private void TextBox_SelectionChanged(object? sender, EventArgs e)
        {
            // 更新状态栏信息
            UpdateStatusInfo();
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
                    
                    TextBox.Text = action.Text;
                    TextBox.SelectionStart = action.CursorPosition;
                    
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
                    
                    TextBox.Text = action.Text;
                    TextBox.SelectionStart = action.CursorPosition;
                    
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
                // 先暂停布局
                TextBox.SuspendLayout();
                
                try
                {
                    // 设置自动换行
                    TextBox.WordWrap = enabled;
                    
                    // 然后设置滚动条
                    if (enabled)
                        TextBox.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
                    else
                        TextBox.ScrollBars = RichTextBoxScrollBars.Both;
                    
                    // 输出调试信息
                    Logger.Log($"设置自动换行: {enabled}, 实际状态: {TextBox.WordWrap}");
                }
                finally
                {
                    // 恢复布局
                    TextBox.ResumeLayout();
                }
            }
        }

        public void SetFont(Font font)
        {
            if (TextBox != null)
                TextBox.Font = font;
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
            
            // 禁用自动调整大小，避免自动换行问题
            this.AutoSize = false;
            
            // 启用双缓冲，减少闪烁
            this.DoubleBuffered = true;
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