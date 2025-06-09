using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WawaEditor
{
    /// <summary>
    /// 虚拟化文本框，用于高效显示大文件内容
    /// </summary>
    public class VirtualTextBox : Control
    {
        private LargeFileHandler _fileHandler;
        private int _firstVisibleLine = 0;
        private int _visibleLineCount = 0;
        private int _totalLines = 0;
        private int _cursorLine = 0;
        private int _cursorColumn = 0;
        private Font _font = new Font("Consolas", 10);
        private bool _wordWrap = false;
        private VScrollBar _vScrollBar;
        private List<string> _visibleLines = new List<string>();
        private int _lineHeight = 16;
        private int _charWidth = 8;
        private int _selectionStart = -1;
        private int _selectionEnd = -1;
        
        // 事件
        public event EventHandler SelectionChanged;
        public event EventHandler TextChanged;
        public event EventHandler VScroll;
        
        public VirtualTextBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.Selectable, true);
            
            BackColor = Color.White;
            ForeColor = Color.Black;
            
            // 创建滚动条
            _vScrollBar = new VScrollBar();
            _vScrollBar.Dock = DockStyle.Right;
            _vScrollBar.ValueChanged += VScrollBar_ValueChanged;
            Controls.Add(_vScrollBar);
            
            // 设置焦点和键盘事件
            TabStop = true;
            KeyDown += VirtualTextBox_KeyDown;
            MouseDown += VirtualTextBox_MouseDown;
            MouseWheel += VirtualTextBox_MouseWheel;
            
            // 计算行高和字符宽度
            using (Graphics g = CreateGraphics())
            {
                _lineHeight = (int)Math.Ceiling(_font.GetHeight(g));
                _charWidth = (int)Math.Ceiling(g.MeasureString("W", _font).Width);
            }
        }
        
        /// <summary>
        /// 加载文件
        /// </summary>
        public void LoadFile(string filePath)
        {
            // 关闭之前的文件
            if (_fileHandler != null)
            {
                _fileHandler.Close();
            }
            
            // 创建新的文件处理器
            _fileHandler = new LargeFileHandler(filePath);
            _totalLines = _fileHandler.TotalLines;
            
            // 重置滚动条
            _firstVisibleLine = 0;
            UpdateScrollBar();
            
            // 加载可见行
            LoadVisibleLines();
            
            // 重绘
            Invalidate();
        }
        
        /// <summary>
        /// 更新滚动条
        /// </summary>
        private void UpdateScrollBar()
        {
            _vScrollBar.Minimum = 0;
            _vScrollBar.Maximum = Math.Max(0, _totalLines - 1);
            _vScrollBar.Value = _firstVisibleLine;
            _vScrollBar.LargeChange = _visibleLineCount;
            _vScrollBar.SmallChange = 1;
        }
        
        /// <summary>
        /// 加载可见行
        /// </summary>
        private void LoadVisibleLines()
        {
            if (_fileHandler == null) return;
            
            // 计算可见行数
            _visibleLineCount = Height / _lineHeight;
            
            // 获取可见行
            _visibleLines = _fileHandler.GetVisibleLines(_firstVisibleLine, _visibleLineCount + 1);
        }
        
        /// <summary>
        /// 滚动条值改变事件
        /// </summary>
        private void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _firstVisibleLine = _vScrollBar.Value;
            LoadVisibleLines();
            Invalidate();
            
            // 触发滚动事件
            VScroll?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// 键盘事件
        /// </summary>
        private void VirtualTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (_cursorLine > 0)
                    {
                        _cursorLine--;
                        EnsureCursorVisible();
                    }
                    break;
                    
                case Keys.Down:
                    if (_cursorLine < _totalLines - 1)
                    {
                        _cursorLine++;
                        EnsureCursorVisible();
                    }
                    break;
                    
                case Keys.PageUp:
                    _cursorLine = Math.Max(0, _cursorLine - _visibleLineCount);
                    _firstVisibleLine = Math.Max(0, _firstVisibleLine - _visibleLineCount);
                    UpdateScrollBar();
                    LoadVisibleLines();
                    break;
                    
                case Keys.PageDown:
                    _cursorLine = Math.Min(_totalLines - 1, _cursorLine + _visibleLineCount);
                    _firstVisibleLine = Math.Min(_totalLines - _visibleLineCount, _firstVisibleLine + _visibleLineCount);
                    UpdateScrollBar();
                    LoadVisibleLines();
                    break;
                    
                case Keys.Home:
                    if (e.Control)
                    {
                        _cursorLine = 0;
                        _firstVisibleLine = 0;
                        UpdateScrollBar();
                        LoadVisibleLines();
                    }
                    else
                    {
                        _cursorColumn = 0;
                    }
                    break;
                    
                case Keys.End:
                    if (e.Control)
                    {
                        _cursorLine = _totalLines - 1;
                        _firstVisibleLine = Math.Max(0, _totalLines - _visibleLineCount);
                        UpdateScrollBar();
                        LoadVisibleLines();
                    }
                    else
                    {
                        string line = GetLine(_cursorLine);
                        _cursorColumn = line.Length;
                    }
                    break;
            }
            
            Invalidate();
        }
        
        /// <summary>
        /// 确保光标可见
        /// </summary>
        private void EnsureCursorVisible()
        {
            if (_cursorLine < _firstVisibleLine)
            {
                _firstVisibleLine = _cursorLine;
                UpdateScrollBar();
                LoadVisibleLines();
            }
            else if (_cursorLine >= _firstVisibleLine + _visibleLineCount)
            {
                _firstVisibleLine = _cursorLine - _visibleLineCount + 1;
                UpdateScrollBar();
                LoadVisibleLines();
            }
        }
        
        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        private void VirtualTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            // 获取点击位置对应的行和列
            int line = _firstVisibleLine + (e.Y / _lineHeight);
            int column = e.X / _charWidth;
            
            // 确保在有效范围内
            line = Math.Min(Math.Max(0, line), _totalLines - 1);
            string lineText = GetLine(line);
            column = Math.Min(Math.Max(0, column), lineText.Length);
            
            // 设置光标位置
            _cursorLine = line;
            _cursorColumn = column;
            
            // 开始选择
            if (e.Button == MouseButtons.Left)
            {
                _selectionStart = GetCharIndex(_cursorLine, _cursorColumn);
                _selectionEnd = _selectionStart;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            
            // 获取焦点并重绘
            Focus();
            Invalidate();
        }
        
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        private void VirtualTextBox_MouseWheel(object sender, MouseEventArgs e)
        {
            int linesToScroll = e.Delta > 0 ? -3 : 3;
            _firstVisibleLine = Math.Max(0, Math.Min(_totalLines - _visibleLineCount, _firstVisibleLine + linesToScroll));
            UpdateScrollBar();
            LoadVisibleLines();
            Invalidate();
            
            // 触发滚动事件
            VScroll?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// 获取指定行的文本
        /// </summary>
        private string GetLine(int lineNumber)
        {
            if (_fileHandler == null || lineNumber < 0 || lineNumber >= _totalLines)
                return string.Empty;
                
            int relativeIndex = lineNumber - _firstVisibleLine;
            if (relativeIndex >= 0 && relativeIndex < _visibleLines.Count)
                return _visibleLines[relativeIndex];
                
            return _fileHandler.GetLine(lineNumber);
        }
        
        /// <summary>
        /// 获取字符索引
        /// </summary>
        private int GetCharIndex(int line, int column)
        {
            if (line < 0 || line >= _totalLines)
                return -1;
                
            int index = 0;
            for (int i = 0; i < line; i++)
            {
                index += GetLine(i).Length + 1; // +1 for newline
            }
            
            return index + Math.Min(column, GetLine(line).Length);
        }
        
        /// <summary>
        /// 设置字体
        /// </summary>
        public void SetFont(Font font)
        {
            _font = font;
            
            // 重新计算行高和字符宽度
            using (Graphics g = CreateGraphics())
            {
                _lineHeight = (int)Math.Ceiling(_font.GetHeight(g));
                _charWidth = (int)Math.Ceiling(g.MeasureString("W", _font).Width);
            }
            
            // 更新可见行数并重绘
            LoadVisibleLines();
            Invalidate();
        }
        
        /// <summary>
        /// 设置自动换行
        /// </summary>
        public void SetWordWrap(bool enabled)
        {
            _wordWrap = enabled;
            Invalidate();
        }
        
        /// <summary>
        /// 重写绘制方法
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // 绘制背景
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            
            // 如果没有文件，直接返回
            if (_fileHandler == null) return;
            
            // 绘制可见行
            for (int i = 0; i < _visibleLines.Count && i < _visibleLineCount; i++)
            {
                int lineNumber = _firstVisibleLine + i;
                string line = _visibleLines[i];
                
                // 计算行的Y坐标
                int y = i * _lineHeight;
                
                // 绘制行号背景
                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), 0, y, 50, _lineHeight);
                
                // 绘制行号
                e.Graphics.DrawString((lineNumber + 1).ToString(), _font, new SolidBrush(Color.DarkGray), 5, y);
                
                // 绘制文本
                e.Graphics.DrawString(line, _font, new SolidBrush(ForeColor), 55, y);
                
                // 如果是当前行，绘制光标
                if (lineNumber == _cursorLine)
                {
                    int cursorX = 55 + _cursorColumn * _charWidth;
                    e.Graphics.DrawLine(Pens.Black, cursorX, y, cursorX, y + _lineHeight);
                }
            }
        }
        
        /// <summary>
        /// 获取当前行号和列号
        /// </summary>
        public (int Line, int Column) GetCursorPosition()
        {
            return (_cursorLine + 1, _cursorColumn + 1);
        }
        
        /// <summary>
        /// 获取总行数
        /// </summary>
        public int GetLineCount()
        {
            return _totalLines;
        }
        
        /// <summary>
        /// 保存文件
        /// </summary>
        public void SaveFile()
        {
            if (_fileHandler != null)
            {
                _fileHandler.SaveChanges();
            }
        }
    }
}