namespace WawaEditor
{
    public partial class FindReplaceDialog : Form
    {
        private readonly Form1 _mainForm;
        private bool _isReplace;

        public FindReplaceDialog(Form1 mainForm, bool isReplace = false)
        {
            InitializeComponent();
            _mainForm = mainForm;
            _isReplace = isReplace;
            
            if (!isReplace)
            {
                // Hide replace controls
                lblReplace.Visible = false;
                txtReplace.Visible = false;
                btnReplace.Visible = false;
                btnReplaceAll.Visible = false;
                this.Height = 150;
                this.Text = "Find";
            }
            else
            {
                this.Text = "Replace";
            }
        }

        private void InitializeComponent()
        {
            this.lblFind = new Label();
            this.txtFind = new TextBox();
            this.lblReplace = new Label();
            this.txtReplace = new TextBox();
            this.chkMatchCase = new CheckBox();
            this.chkWholeWord = new CheckBox();
            this.btnFindNext = new Button();
            this.btnReplace = new Button();
            this.btnReplaceAll = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            
            // lblFind
            this.lblFind.AutoSize = true;
            this.lblFind.Location = new Point(12, 15);
            this.lblFind.Name = "lblFind";
            this.lblFind.Size = new Size(33, 15);
            this.lblFind.Text = "Find:";
            
            // txtFind
            this.txtFind.Location = new Point(80, 12);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new Size(200, 23);
            
            // lblReplace
            this.lblReplace.AutoSize = true;
            this.lblReplace.Location = new Point(12, 44);
            this.lblReplace.Name = "lblReplace";
            this.lblReplace.Size = new Size(53, 15);
            this.lblReplace.Text = "Replace:";
            
            // txtReplace
            this.txtReplace.Location = new Point(80, 41);
            this.txtReplace.Name = "txtReplace";
            this.txtReplace.Size = new Size(200, 23);
            
            // chkMatchCase
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new Point(12, 70);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new Size(86, 19);
            this.chkMatchCase.Text = "Match case";
            
            // chkWholeWord
            this.chkWholeWord.AutoSize = true;
            this.chkWholeWord.Location = new Point(120, 70);
            this.chkWholeWord.Name = "chkWholeWord";
            this.chkWholeWord.Size = new Size(88, 19);
            this.chkWholeWord.Text = "Whole word";
            
            // btnFindNext
            this.btnFindNext.Location = new Point(290, 12);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new Size(75, 23);
            this.btnFindNext.Text = "Find Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Click += new EventHandler(btnFindNext_Click);
            
            // btnReplace
            this.btnReplace.Location = new Point(290, 41);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new Size(75, 23);
            this.btnReplace.Text = "Replace";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new EventHandler(btnReplace_Click);
            
            // btnReplaceAll
            this.btnReplaceAll.Location = new Point(290, 70);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new Size(75, 23);
            this.btnReplaceAll.Text = "Replace All";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new EventHandler(btnReplaceAll_Click);
            
            // btnCancel
            this.btnCancel.Location = new Point(290, 99);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 23);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            
            // FindReplaceDialog
            this.ClientSize = new Size(377, 134);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.chkWholeWord);
            this.Controls.Add(this.chkMatchCase);
            this.Controls.Add(this.txtReplace);
            this.Controls.Add(this.lblReplace);
            this.Controls.Add(this.txtFind);
            this.Controls.Add(this.lblFind);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindReplaceDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFind.Text))
                return;

            _mainForm.FindText(txtFind.Text, chkMatchCase.Checked, chkWholeWord.Checked);
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFind.Text))
                return;

            _mainForm.ReplaceText(txtFind.Text, txtReplace.Text, chkMatchCase.Checked, chkWholeWord.Checked);
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFind.Text))
                return;

            _mainForm.ReplaceAllText(txtFind.Text, txtReplace.Text, chkMatchCase.Checked, chkWholeWord.Checked);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Label lblFind;
        private TextBox txtFind;
        private Label lblReplace;
        private TextBox txtReplace;
        private CheckBox chkMatchCase;
        private CheckBox chkWholeWord;
        private Button btnFindNext;
        private Button btnReplace;
        private Button btnReplaceAll;
        private Button btnCancel;
    }
}