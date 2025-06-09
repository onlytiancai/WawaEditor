using System;
using System.Drawing;
using System.Windows.Forms;

namespace WawaEditor
{
    /// <summary>
    /// 大文本编辑器标签页，使用虚拟化文本框显示大文件内容
    /// </summary>
    public class LargeTextEditorTabPage : TabPage
    {
        public VirtualTextBox TextBox { get; private set; }
        public string FilePath { get; set; } = string.Empty;
        public bool IsModified { get; set; }
        
        // 状态栏信息委托
        public delegate void StatusUpdateHandler(string statusText);
        public event StatusUpdateHandler OnStatusUpdate;
        
        public LargeTextEditorTabPage(string title = "Untitled")
        {
            try
            {
                Text = title;
                FilePath = string.Empty;
                IsModified = false;
                
                // 创建虚拟文本框
                TextBox = new VirtualTextBox();
                TextBox.Dock = DockStyle.Fill;
                TextBox.Font = new Font("Consolas", 10);
                
                // 添加事件处理
                TextBox.SelectionChanged += TextBox_SelectionChanged;
                TextBox.TextChanged += TextBox_TextChanged;
                TextBox.VScroll += TextBox_VScroll;
                
                // 添加右键菜单
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("复制", null, (s, e) => CopySelectedText());
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("全选", null, (s, e) => SelectAll());
                TextBox.ContextMenuStrip = contextMenu;
                
                // 添加控件
                Controls.Add(TextBox);
                
                // 初始化状态栏信息
                UpdateStatusInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing large file editor: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 加载文件
        /// </summary>
        public void LoadFile(string filePath)
        {
            FilePath = filePath;
            TextBox.LoadFile(filePath);
            IsModified = false;
            UpdateStatusInfo();
        }
        
        /// <summary>
        /// 选择改变事件
        /// </summary>
        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
            UpdateStatusInfo();
        }
        
        /// <summary>
        /// 文本改变事件
        /// </summary>
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            IsModified = true;
            
            // 更新标签页标题显示修改状态
            if (IsModified && !Text.EndsWith("*"))
            {
                Text = Text + "*";
            }
            
            UpdateStatusInfo();
        }
        
        /// <summary>
        /// 滚动事件
        /// </summary>
        private void TextBox_VScroll(object sender, EventArgs e)
        {
            UpdateStatusInfo();
        }
        
        /// <summary>
        /// 更新状态栏信息
        /// </summary>
        public void UpdateStatusInfo()
        {
            try
            {
                // 获取光标位置
                var (line, column) = TextBox.GetCursorPosition();
                
                // 获取总行数
                int totalLines = TextBox.GetLineCount();
                
                // 构建状态信息
                string statusText = $"大文件模式 | 行: {line}/{totalLines} 列: {column}";
                
                // 触发状态更新事件
                OnStatusUpdate?.Invoke(statusText);
            }
            catch
            {
                // 忽略任何错误
            }
        }
        
        /// <summary>
        /// 设置自动换行
        /// </summary>
        public void SetWordWrap(bool enabled)
        {
            TextBox.SetWordWrap(enabled);
        }
        
        /// <summary>
        /// 设置字体
        /// </summary>
        public void SetFont(Font font)
        {
            TextBox.SetFont(font);
        }
        
        /// <summary>
        /// 复制选中文本
        /// </summary>
        public void CopySelectedText()
        {
            // 暂未实现选择功能
        }
        
        /// <summary>
        /// 全选
        /// </summary>
        public void SelectAll()
        {
            // 暂未实现选择功能
        }
        
        /// <summary>
        /// 保存文件
        /// </summary>
        public void SaveFile()
        {
            TextBox.SaveFile();
            IsModified = false;
            
            // 更新标签页标题
            if (Text.EndsWith("*"))
            {
                Text = Text.Substring(0, Text.Length - 1);
            }
        }
    }
}