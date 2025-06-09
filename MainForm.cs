using System.Text;
using System.Text.Json;

namespace WawaEditor;

public partial class MainForm : Form
{
    private FindReplaceDialog _findReplaceDialog;
    private ToolStripStatusLabel statusLabel;

    public MainForm()
    {
        // 确保配置已加载
        LoadAndValidateConfig();
        
        InitializeComponent();
        
        // 恢复正常窗体样式
        this.FormBorderStyle = FormBorderStyle.Sizable;
        
        // 应用配置到UI
        ApplyConfigToUI();

        // 初始化状态栏
        InitializeStatusBar();

        // 初始化最近文件菜单
        InitializeRecentFilesMenu();

        // 恢复上次打开的标签页或添加新标签页
        RestoreLastOpenedTabs();
        
        // 再次应用字体设置，确保所有标签页都使用正确的字体
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            if (tabPage is TextEditorTabPage tab)
            {
                ApplyFontToTab(tab);
            }
        }
    }
    
    // 加载并验证配置
    private void LoadAndValidateConfig()
    {
        // 输出配置信息
        Logger.Log($"配置加载: WordWrap={AppConfig.Instance.WordWrap}, FontFamily={AppConfig.Instance.FontFamily}, FontSize={AppConfig.Instance.FontSize}");
        
        // 验证字体设置
        try
        {
            if (!string.IsNullOrEmpty(AppConfig.Instance.FontFamily) && AppConfig.Instance.FontSize > 0)
            {
                // 尝试创建字体对象，验证字体设置是否有效
                using (Font font = new Font(AppConfig.Instance.FontFamily, AppConfig.Instance.FontSize))
                {
                    Logger.Log($"字体验证成功: {font.Name}, {font.Size}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"字体验证失败: {ex.Message}，使用默认字体");
            // 字体无效，使用默认字体
            AppConfig.Instance.FontFamily = "Consolas";
            AppConfig.Instance.FontSize = 10;
        }
        
        // 强制保存配置，确保配置文件存在
        AppConfig.Instance.Save();
    }
    
    // 应用配置到UI
    private void ApplyConfigToUI()
    {
        // 输出配置信息
        System.Diagnostics.Debug.WriteLine($"应用配置: WordWrap={AppConfig.Instance.WordWrap}, FontFamily={AppConfig.Instance.FontFamily}, FontSize={AppConfig.Instance.FontSize}");
        
        // 应用配置到菜单项
        wordWrapToolStripMenuItem.Checked = AppConfig.Instance.WordWrap;
        
        // 应用字体设置到已打开的标签页
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            if (tabPage is TextEditorTabPage tab)
            {
                ApplyFontToTab(tab);
            }
        }
    }

    // 窗口关闭前保存配置
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存当前打开的标签页
        SaveOpenTabs();
        
        // 保存当前字体设置
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null && currentTab.TextBox != null)
        {
            AppConfig.Instance.FontFamily = currentTab.TextBox.Font.FontFamily.Name;
            AppConfig.Instance.FontSize = currentTab.TextBox.Font.Size;
            AppConfig.Instance.Save();
        }
    }

    private void InitializeStatusBar()
    {
        // 创建状态栏标签
        statusLabel = new ToolStripStatusLabel();
        statusLabel.Text = "准备就绪";
        statusLabel.Spring = true; // 自动调整大小
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;

        // 添加到状态栏
        statusStrip.Items.Add(statusLabel);
    }

    private TextEditorTabPage GetCurrentTab()
    {
        if (tabControl.SelectedTab is TextEditorTabPage tab)
        {
            return tab;
        }
        return null;
    }

    private void AddNewTab(string filePath = "")
    {
        try
        {
            // Create tab first
            string title = string.IsNullOrEmpty(filePath) ? "Untitled" : Path.GetFileName(filePath);
            TextEditorTabPage tab = new TextEditorTabPage(title);

            // 注册状态栏更新事件
            tab.OnStatusUpdate += UpdateStatusBar;

            // Add tab to control before loading content
            tabControl.TabPages.Add(tab);
            tabControl.SelectedTab = tab;

            // 先应用字体设置，再加载内容
            ApplyFontToTab(tab);
            
            // Load file content if needed
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                tab.FilePath = filePath;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Application.DoEvents();

                    // Simple file loading approach
                    using (var reader = new StreamReader(filePath))
                    {
                        tab.TextBox.Text = reader.ReadToEnd();
                    }

                    tab.IsModified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }

            // Update UI settings
            if (tab != null && tab.TextBox != null)
            {
                // 应用全局设置
                tab.SetWordWrap(AppConfig.Instance.WordWrap);
                
                // 更新菜单状态
                wordWrapToolStripMenuItem.Checked = tab.TextBox.WordWrap;
                
                tab.TextBox.Focus();
                
                // 初始化状态栏信息
                tab.UpdateStatusInfo();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating new tab: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    // 应用字体设置到标签页
    private void ApplyFontToTab(TextEditorTabPage tab)
    {
        if (tab?.TextBox == null) return;
        
        try
        {
            string fontFamily = AppConfig.Instance.FontFamily;
            float fontSize = AppConfig.Instance.FontSize;
            
            Logger.Log($"应用字体设置: {fontFamily}, {fontSize}");
            
            // 确保字体设置有效
            if (!string.IsNullOrEmpty(fontFamily) && fontSize > 0)
            {
                // 强制创建新的字体对象
                Font font = new Font(fontFamily, fontSize);
                
                // 直接设置TextBox的字体，而不是通过SetFont方法
                if (tab.TextBox != null)
                {
                    // 先保存当前文本和选择位置
                    string text = tab.TextBox.Text;
                    int selectionStart = tab.TextBox.SelectionStart;
                    int selectionLength = tab.TextBox.SelectionLength;
                    
                    // 设置字体
                    tab.TextBox.Font = font;
                    
                    // 恢复文本和选择位置（有时设置字体会清空文本）
                    if (string.IsNullOrEmpty(tab.TextBox.Text) && !string.IsNullOrEmpty(text))
                    {
                        tab.TextBox.Text = text;
                        tab.TextBox.Select(selectionStart, selectionLength);
                    }
                    
                    Logger.Log($"字体已应用: {tab.TextBox.Font.Name}, {tab.TextBox.Font.Size}");
                }
            }
        }
        catch (Exception ex) 
        { 
            Logger.Log($"字体设置错误: {ex.Message}");
        }
    }

    private void newToolStripMenuItem_Click(object sender, EventArgs e)
    {
        AddNewTab();
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                OpenFile(filePath);
            }
        }
    }

    private void OpenFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;

        // Check if file is already open
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            if (tabPage is TextEditorTabPage tab && tab.FilePath == filePath)
            {
                tabControl.SelectedTab = tabPage;
                return;
            }
        }

        // 添加到最近文件列表
        AppConfig.Instance.AddRecentFile(filePath);

        // 更新最近文件菜单
        UpdateRecentFilesMenu();

        // 打开文件
        AddNewTab(filePath);
    }

    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab == null) return;

        if (string.IsNullOrEmpty(currentTab.FilePath))
        {
            saveAsToolStripMenuItem_Click(sender, e);
        }
        else
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                File.WriteAllText(currentTab.FilePath, currentTab.TextBox.Text);
                currentTab.IsModified = false;
                currentTab.Text = Path.GetFileName(currentTab.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }

    private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab == null) return;

        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    File.WriteAllText(filePath, currentTab.TextBox.Text);

                    currentTab.FilePath = filePath;
                    currentTab.IsModified = false;
                    currentTab.Text = Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void fontToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab == null) return;

        using (FontDialog fontDialog = new FontDialog())
        {
            // 使用配置中的字体初始化对话框
            try
            {
                string fontFamily = AppConfig.Instance.FontFamily;
                float fontSize = AppConfig.Instance.FontSize;
                
                if (!string.IsNullOrEmpty(fontFamily) && fontSize > 0)
                {
                    fontDialog.Font = new Font(fontFamily, fontSize);
                }
                else
                {
                    fontDialog.Font = currentTab.TextBox.Font;
                }
            }
            catch
            {
                fontDialog.Font = currentTab.TextBox.Font;
            }
            
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                // 保存字体设置到配置
                AppConfig.Instance.FontFamily = fontDialog.Font.FontFamily.Name;
                AppConfig.Instance.FontSize = fontDialog.Font.Size;
                AppConfig.Instance.Save();
                
                Logger.Log($"保存字体设置: {fontDialog.Font.FontFamily.Name}, {fontDialog.Font.Size}");
                
                // 应用到所有标签页
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    if (tabPage is TextEditorTabPage tab)
                    {
                        ApplyFontToTab(tab);
                    }
                }
            }
        }
    }

    private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
    {
        bool newState = !wordWrapToolStripMenuItem.Checked;
        wordWrapToolStripMenuItem.Checked = newState;
        
        Logger.Log($"切换自动换行: {newState}");
        
        // 保存设置到配置
        AppConfig.Instance.WordWrap = newState;
        AppConfig.Instance.Save();

        // 应用到所有标签页
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            if (tabPage is TextEditorTabPage tab)
            {
                tab.SetWordWrap(newState);
            }
        }
    }

    private void undoToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null && currentTab.CanUndo())
        {
            currentTab.Undo();
        }
    }

    private void redoToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null && currentTab.CanRedo())
        {
            currentTab.Redo();
        }
    }

    private void findToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (_findReplaceDialog == null || _findReplaceDialog.IsDisposed)
        {
            _findReplaceDialog = new FindReplaceDialog(this);
        }
        _findReplaceDialog.Show();
        _findReplaceDialog.Focus();
    }

    private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (_findReplaceDialog == null || _findReplaceDialog.IsDisposed)
        {
            _findReplaceDialog = new FindReplaceDialog(this, true);
        }
        _findReplaceDialog.Show();
        _findReplaceDialog.Focus();
    }

    private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab == null) return;

        if (currentTab.IsModified)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to save changes to " + currentTab.Text + "?",
                "Save Changes",
                MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                saveToolStripMenuItem_Click(sender, e);
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        tabControl.TabPages.Remove(currentTab);

        // If no tabs left, add a new one
        if (tabControl.TabPages.Count == 0)
        {
            AddNewTab();
        }
    }

    private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            // Update UI based on current tab settings
            wordWrapToolStripMenuItem.Checked = currentTab.TextBox.WordWrap;

            // 更新状态栏信息
            currentTab.UpdateStatusInfo();
        }
    }
    
    // 添加标签页右键菜单
    private void tabControl_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            // 获取点击的标签页
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                Rectangle rect = tabControl.GetTabRect(i);
                if (rect.Contains(e.Location))
                {
                    // 创建右键菜单
                    ContextMenuStrip contextMenu = new ContextMenuStrip();
                    
                    // 保存菜单项
                    ToolStripMenuItem saveMenuItem = new ToolStripMenuItem("保存");
                    saveMenuItem.Click += (s, args) => {
                        tabControl.SelectedIndex = i;
                        saveToolStripMenuItem_Click(s, args);
                    };
                    contextMenu.Items.Add(saveMenuItem);
                    
                    // 关闭菜单项
                    ToolStripMenuItem closeMenuItem = new ToolStripMenuItem("关闭");
                    closeMenuItem.Click += (s, args) => {
                        tabControl.SelectedIndex = i;
                        closeTabToolStripMenuItem_Click(s, args);
                    };
                    contextMenu.Items.Add(closeMenuItem);
                    
                    // 显示菜单
                    contextMenu.Show(tabControl, e.Location);
                    break;
                }
            }
        }
    }

    // Methods for FindReplaceDialog
    public void FindText(string searchText, bool matchCase, bool wholeWord)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            currentTab.Find(searchText, matchCase, wholeWord);
        }
    }

    public void ReplaceText(string searchText, string replaceText, bool matchCase, bool wholeWord)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            // First find the text
            currentTab.Find(searchText, matchCase, wholeWord);
            // Then replace it
            currentTab.ReplaceSelected(replaceText);
        }
    }

    public void ReplaceAllText(string searchText, string replaceText, bool matchCase, bool wholeWord)
    {
        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            currentTab.ReplaceAll(searchText, replaceText, matchCase, wholeWord);
        }
    }

    // 更新状态栏信息
    private void UpdateStatusBar(string statusText)
    {
        if (statusLabel != null && !string.IsNullOrEmpty(statusText))
        {
            statusLabel.Text = statusText;
        }
    }
    
    // 初始化最近文件菜单
    private void InitializeRecentFilesMenu()
    {
        UpdateRecentFilesMenu();
    }
    
    // 更新最近文件菜单
    private void UpdateRecentFilesMenu()
    {
        // 清空现有菜单项
        recentFilesToolStripMenuItem.DropDownItems.Clear();
        
        // 如果没有最近文件，禁用菜单
        if (AppConfig.Instance.RecentFiles.Count == 0)
        {
            recentFilesToolStripMenuItem.Enabled = false;
            return;
        }
        
        // 启用菜单
        recentFilesToolStripMenuItem.Enabled = true;
        
        // 添加最近文件菜单项
        foreach (string filePath in AppConfig.Instance.RecentFiles)
        {
            if (File.Exists(filePath))
            {
                var menuItem = new ToolStripMenuItem(Path.GetFileName(filePath))
                {
                    ToolTipText = filePath,
                    Tag = filePath
                };
                menuItem.Click += RecentFileMenuItem_Click;
                recentFilesToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }
        
        // 添加分隔线和清除菜单项
        if (recentFilesToolStripMenuItem.DropDownItems.Count > 0)
        {
            recentFilesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            var clearMenuItem = new ToolStripMenuItem("清除最近文件列表");
            clearMenuItem.Click += (s, e) => 
            {
                AppConfig.Instance.RecentFiles.Clear();
                AppConfig.Instance.Save();
                UpdateRecentFilesMenu();
            };
            recentFilesToolStripMenuItem.DropDownItems.Add(clearMenuItem);
        }
    }
    
    // 最近文件菜单项点击事件
    private void RecentFileMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string filePath)
        {
            OpenFile(filePath);
        }
    }
    
    // 保存当前打开的标签页
    private void SaveOpenTabs()
    {
        // 确保LastOpenedTabs已初始化
        if (AppConfig.Instance.LastOpenedTabs == null)
            AppConfig.Instance.LastOpenedTabs = new List<string>();
        else
            AppConfig.Instance.LastOpenedTabs.Clear();
        
        // 保存当前打开的标签页
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            if (tabPage is TextEditorTabPage tab && !string.IsNullOrEmpty(tab.FilePath) && File.Exists(tab.FilePath))
            {
                AppConfig.Instance.LastOpenedTabs.Add(tab.FilePath);
                Logger.Log($"保存标签页: {tab.FilePath}");
            }
        }
        
        // 保存配置
        try
        {
            AppConfig.Instance.Save();
            Logger.Log($"配置保存成功，共保存了 {AppConfig.Instance.LastOpenedTabs.Count} 个标签页");
        }
        catch (Exception ex)
        {
            Logger.Log($"配置保存失败: {ex.Message}");
        }
    }
    
    // 恢复上次打开的标签页
    private void RestoreLastOpenedTabs()
    {
        bool tabsRestored = false;
        
        // 确保配置中有上次打开的标签页
        if (AppConfig.Instance.LastOpenedTabs != null && AppConfig.Instance.LastOpenedTabs.Count > 0)
        {
            // 尝试恢复上次打开的标签页
            foreach (string filePath in AppConfig.Instance.LastOpenedTabs)
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    AddNewTab(filePath);
                    tabsRestored = true;
                }
            }
        }
        
        // 如果没有恢复任何标签页，则添加一个新标签页
        if (!tabsRestored)
        {
            AddNewTab();
        }
    }
}