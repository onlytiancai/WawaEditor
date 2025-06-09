namespace WawaEditor;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        newToolStripMenuItem = new ToolStripMenuItem();
        openToolStripMenuItem = new ToolStripMenuItem();
        recentFilesToolStripMenuItem = new ToolStripMenuItem();
        saveToolStripMenuItem = new ToolStripMenuItem();
        saveAsToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator1 = new ToolStripSeparator();
        closeTabToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator2 = new ToolStripSeparator();
        exitToolStripMenuItem = new ToolStripMenuItem();
        editToolStripMenuItem = new ToolStripMenuItem();
        undoToolStripMenuItem = new ToolStripMenuItem();
        redoToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator3 = new ToolStripSeparator();
        findToolStripMenuItem = new ToolStripMenuItem();
        replaceToolStripMenuItem = new ToolStripMenuItem();
        formatToolStripMenuItem = new ToolStripMenuItem();
        fontToolStripMenuItem = new ToolStripMenuItem();
        wordWrapToolStripMenuItem = new ToolStripMenuItem();
        lineNumbersToolStripMenuItem = new ToolStripMenuItem();
        tabControl = new TabControl();
        statusStrip = new StatusStrip();
        menuStrip.SuspendLayout();
        SuspendLayout();
        
        // menuStrip
        menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, formatToolStripMenuItem });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(800, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "menuStrip1";
        menuStrip.Dock = DockStyle.Top;
        
        // fileToolStripMenuItem
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, recentFilesToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, closeTabToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "&File";
        
        // recentFilesToolStripMenuItem
        recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
        recentFilesToolStripMenuItem.Size = new Size(186, 22);
        recentFilesToolStripMenuItem.Text = "Recent Files";
        
        // newToolStripMenuItem
        newToolStripMenuItem.Name = "newToolStripMenuItem";
        newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
        newToolStripMenuItem.Size = new Size(186, 22);
        newToolStripMenuItem.Text = "&New";
        newToolStripMenuItem.Click += new EventHandler(newToolStripMenuItem_Click);
        
        // openToolStripMenuItem
        openToolStripMenuItem.Name = "openToolStripMenuItem";
        openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
        openToolStripMenuItem.Size = new Size(186, 22);
        openToolStripMenuItem.Text = "&Open...";
        openToolStripMenuItem.Click += new EventHandler(openToolStripMenuItem_Click);
        
        // saveToolStripMenuItem
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        saveToolStripMenuItem.Size = new Size(186, 22);
        saveToolStripMenuItem.Text = "&Save";
        saveToolStripMenuItem.Click += new EventHandler(saveToolStripMenuItem_Click);
        
        // saveAsToolStripMenuItem
        saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
        saveAsToolStripMenuItem.Size = new Size(186, 22);
        saveAsToolStripMenuItem.Text = "Save &As...";
        saveAsToolStripMenuItem.Click += new EventHandler(saveAsToolStripMenuItem_Click);
        
        // toolStripSeparator1
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(183, 6);
        
        // closeTabToolStripMenuItem
        closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
        closeTabToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.W;
        closeTabToolStripMenuItem.Size = new Size(186, 22);
        closeTabToolStripMenuItem.Text = "&Close Tab";
        closeTabToolStripMenuItem.Click += new EventHandler(closeTabToolStripMenuItem_Click);
        
        // toolStripSeparator2
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(183, 6);
        
        // exitToolStripMenuItem
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
        exitToolStripMenuItem.Size = new Size(186, 22);
        exitToolStripMenuItem.Text = "E&xit";
        exitToolStripMenuItem.Click += new EventHandler(exitToolStripMenuItem_Click);
        
        // editToolStripMenuItem
        editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator3, findToolStripMenuItem, replaceToolStripMenuItem });
        editToolStripMenuItem.Name = "editToolStripMenuItem";
        editToolStripMenuItem.Size = new Size(39, 20);
        editToolStripMenuItem.Text = "&Edit";
        
        // undoToolStripMenuItem
        undoToolStripMenuItem.Name = "undoToolStripMenuItem";
        undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
        undoToolStripMenuItem.Size = new Size(164, 22);
        undoToolStripMenuItem.Text = "&Undo";
        undoToolStripMenuItem.Click += new EventHandler(undoToolStripMenuItem_Click);
        
        // redoToolStripMenuItem
        redoToolStripMenuItem.Name = "redoToolStripMenuItem";
        redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
        redoToolStripMenuItem.Size = new Size(164, 22);
        redoToolStripMenuItem.Text = "&Redo";
        redoToolStripMenuItem.Click += new EventHandler(redoToolStripMenuItem_Click);
        
        // toolStripSeparator3
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new Size(161, 6);
        
        // findToolStripMenuItem
        findToolStripMenuItem.Name = "findToolStripMenuItem";
        findToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
        findToolStripMenuItem.Size = new Size(164, 22);
        findToolStripMenuItem.Text = "&Find...";
        findToolStripMenuItem.Click += new EventHandler(findToolStripMenuItem_Click);
        
        // replaceToolStripMenuItem
        replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
        replaceToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.H;
        replaceToolStripMenuItem.Size = new Size(164, 22);
        replaceToolStripMenuItem.Text = "&Replace...";
        replaceToolStripMenuItem.Click += new EventHandler(replaceToolStripMenuItem_Click);
        
        // formatToolStripMenuItem
        formatToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fontToolStripMenuItem, wordWrapToolStripMenuItem, lineNumbersToolStripMenuItem });
        formatToolStripMenuItem.Name = "formatToolStripMenuItem";
        formatToolStripMenuItem.Size = new Size(57, 20);
        formatToolStripMenuItem.Text = "F&ormat";
        
        // fontToolStripMenuItem
        fontToolStripMenuItem.Name = "fontToolStripMenuItem";
        fontToolStripMenuItem.Size = new Size(152, 22);
        fontToolStripMenuItem.Text = "&Font...";
        fontToolStripMenuItem.Click += new EventHandler(fontToolStripMenuItem_Click);
        
        // wordWrapToolStripMenuItem
        wordWrapToolStripMenuItem.CheckOnClick = true;
        wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
        wordWrapToolStripMenuItem.Size = new Size(152, 22);
        wordWrapToolStripMenuItem.Text = "&Word Wrap";
        wordWrapToolStripMenuItem.Click += new EventHandler(wordWrapToolStripMenuItem_Click);
        
        // lineNumbersToolStripMenuItem
        lineNumbersToolStripMenuItem.CheckOnClick = true;
        lineNumbersToolStripMenuItem.Name = "lineNumbersToolStripMenuItem";
        lineNumbersToolStripMenuItem.Size = new Size(152, 22);
        lineNumbersToolStripMenuItem.Text = "&Line Numbers";
        lineNumbersToolStripMenuItem.Click += new EventHandler(lineNumbersToolStripMenuItem_Click);
        
        // tabControl
        tabControl.Dock = DockStyle.Fill;
        tabControl.Location = new Point(0, 24);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(800, 404);
        tabControl.TabIndex = 1;
        tabControl.SelectedIndexChanged += new EventHandler(tabControl_SelectedIndexChanged);
        tabControl.MouseClick += new MouseEventHandler(tabControl_MouseClick);
        
        // statusStrip
        statusStrip.Location = new Point(0, 428);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(800, 22);
        statusStrip.TabIndex = 2;
        statusStrip.Text = "statusStrip1";
        
        // Form1
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(tabControl);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        Name = "Form1";
        Text = "WawaEditor";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem newToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripMenuItem recentFilesToolStripMenuItem;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem saveAsToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem closeTabToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem editToolStripMenuItem;
    private ToolStripMenuItem undoToolStripMenuItem;
    private ToolStripMenuItem redoToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem findToolStripMenuItem;
    private ToolStripMenuItem replaceToolStripMenuItem;
    private ToolStripMenuItem formatToolStripMenuItem;
    private ToolStripMenuItem fontToolStripMenuItem;
    private ToolStripMenuItem wordWrapToolStripMenuItem;
    private ToolStripMenuItem lineNumbersToolStripMenuItem;
    private TabControl tabControl;
    private StatusStrip statusStrip;
}