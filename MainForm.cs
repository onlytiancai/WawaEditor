using System.Text;
using System.Text.Json;

namespace WawaEditor;

public partial class MainForm : Form
{
    private FindReplaceDialog _findReplaceDialog;
    private ToolStripStatusLabel statusLabel;

    public MainForm()
    {
        InitializeComponent();
        this.Text = "WawaEditor";

        // 初始化状态栏
        InitializeStatusBar();

        // 初始化最近文件菜单
        InitializeRecentFilesMenu();

        // 恢复上次打开的标签页或添加新标签页
        RestoreLastOpenedTabs();
    }

    // 窗口关闭前保存配置
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存当前打开的标签页
        SaveOpenTabs();
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
                tab.TextBox.Focus();
                lineNumbersToolStripMenuItem.Checked = tab.ShowLineNumbers;
                wordWrapToolStripMenuItem.Checked = tab.TextBox.WordWrap;

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
            fontDialog.Font = currentTab.TextBox.Font;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                currentTab.SetFont(fontDialog.Font);
            }
        }
    }

    private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
    {
        wordWrapToolStripMenuItem.Checked = !wordWrapToolStripMenuItem.Checked;

        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            currentTab.SetWordWrap(wordWrapToolStripMenuItem.Checked);
        }
    }

    private void lineNumbersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        lineNumbersToolStripMenuItem.Checked = !lineNumbersToolStripMenuItem.Checked;

        TextEditorTabPage currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            currentTab.ToggleLineNumbers(lineNumbersToolStripMenuItem.Checked);
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
            lineNumbersToolStripMenuItem.Checked = currentTab.ShowLineNumbers;

            // 更新状态栏信息
            currentTab.UpdateStatusInfo();
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
        AppConfig.Instance.LastOpenedTabs.Clear();
        
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            if (tabPage is TextEditorTabPage tab && !string.IsNullOrEmpty(tab.FilePath) && File.Exists(tab.FilePath))
            {
                AppConfig.Instance.LastOpenedTabs.Add(tab.FilePath);
            }
        }
        
        AppConfig.Instance.Save();
    }
    
    // 恢复上次打开的标签页
    private void RestoreLastOpenedTabs()
    {
        bool tabsRestored = false;
        
        // 尝试恢复上次打开的标签页
        foreach (string filePath in AppConfig.Instance.LastOpenedTabs)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                AddNewTab(filePath);
                tabsRestored = true;
            }
        }
        
        // 如果没有恢复任何标签页，则添加一个新标签页
        if (!tabsRestored)
        {
            AddNewTab();
        }
    }    
}
