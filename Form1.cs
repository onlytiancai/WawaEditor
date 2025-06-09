using System.Text;

namespace WawaEditor;

public partial class Form1 : Form
{
    private FindReplaceDialog _findReplaceDialog;

    public Form1()
    {
        InitializeComponent();
        this.Text = "WawaEditor";
        
        // Add a new tab on startup
        AddNewTab();
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
        string title = string.IsNullOrEmpty(filePath) ? "Untitled" : Path.GetFileName(filePath);
        TextEditorTabPage tab = new TextEditorTabPage(title);
        
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            tab.FilePath = filePath;
            tab.TextBox.Text = File.ReadAllText(filePath);
            tab.IsModified = false;
        }
        
        tabControl.TabPages.Add(tab);
        tabControl.SelectedTab = tab;
        tab.TextBox.Focus();
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
                
                // Check if file is already open
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    if (tabPage is TextEditorTabPage tab && tab.FilePath == filePath)
                    {
                        tabControl.SelectedTab = tabPage;
                        return;
                    }
                }
                
                AddNewTab(filePath);
            }
        }
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
            File.WriteAllText(currentTab.FilePath, currentTab.TextBox.Text);
            currentTab.IsModified = false;
            currentTab.Text = Path.GetFileName(currentTab.FilePath);
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
                File.WriteAllText(filePath, currentTab.TextBox.Text);
                
                currentTab.FilePath = filePath;
                currentTab.IsModified = false;
                currentTab.Text = Path.GetFileName(filePath);
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
}