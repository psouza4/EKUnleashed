namespace EKUnleashed
{
    partial class frmSelectRune
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSelectRune));
            this.btnRefresh = new System.Windows.Forms.Button();
            this.chk5Star = new System.Windows.Forms.CheckBox();
            this.chk4Star = new System.Windows.Forms.CheckBox();
            this.chk3Star = new System.Windows.Forms.CheckBox();
            this.chk2Star = new System.Windows.Forms.CheckBox();
            this.chk1Star = new System.Windows.Forms.CheckBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.enchantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label53 = new System.Windows.Forms.Label();
            this.mnuRuneRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.colSkill = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colElement = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStars = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRuneName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstRunes = new System.Windows.Forms.ListView();
            this.sellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuRuneRightClick.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnRefresh.Location = new System.Drawing.Point(550, 450);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 36;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // chk5Star
            // 
            this.chk5Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk5Star.AutoSize = true;
            this.chk5Star.Checked = true;
            this.chk5Star.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk5Star.ForeColor = System.Drawing.Color.White;
            this.chk5Star.Location = new System.Drawing.Point(213, 454);
            this.chk5Star.Name = "chk5Star";
            this.chk5Star.Size = new System.Drawing.Size(44, 17);
            this.chk5Star.TabIndex = 34;
            this.chk5Star.Text = "5★";
            this.chk5Star.UseVisualStyleBackColor = true;
            this.chk5Star.CheckedChanged += new System.EventHandler(this.chk5Star_CheckedChanged);
            // 
            // chk4Star
            // 
            this.chk4Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk4Star.AutoSize = true;
            this.chk4Star.ForeColor = System.Drawing.Color.White;
            this.chk4Star.Location = new System.Drawing.Point(162, 454);
            this.chk4Star.Name = "chk4Star";
            this.chk4Star.Size = new System.Drawing.Size(44, 17);
            this.chk4Star.TabIndex = 33;
            this.chk4Star.Text = "4★";
            this.chk4Star.UseVisualStyleBackColor = true;
            this.chk4Star.CheckedChanged += new System.EventHandler(this.chk4Star_CheckedChanged);
            // 
            // chk3Star
            // 
            this.chk3Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk3Star.AutoSize = true;
            this.chk3Star.ForeColor = System.Drawing.Color.White;
            this.chk3Star.Location = new System.Drawing.Point(111, 454);
            this.chk3Star.Name = "chk3Star";
            this.chk3Star.Size = new System.Drawing.Size(44, 17);
            this.chk3Star.TabIndex = 32;
            this.chk3Star.Text = "3★";
            this.chk3Star.UseVisualStyleBackColor = true;
            this.chk3Star.CheckedChanged += new System.EventHandler(this.chk3Star_CheckedChanged);
            // 
            // chk2Star
            // 
            this.chk2Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk2Star.AutoSize = true;
            this.chk2Star.ForeColor = System.Drawing.Color.White;
            this.chk2Star.Location = new System.Drawing.Point(60, 454);
            this.chk2Star.Name = "chk2Star";
            this.chk2Star.Size = new System.Drawing.Size(44, 17);
            this.chk2Star.TabIndex = 31;
            this.chk2Star.Text = "2★";
            this.chk2Star.UseVisualStyleBackColor = true;
            this.chk2Star.CheckedChanged += new System.EventHandler(this.chk2Star_CheckedChanged);
            // 
            // chk1Star
            // 
            this.chk1Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk1Star.AutoSize = true;
            this.chk1Star.ForeColor = System.Drawing.Color.White;
            this.chk1Star.Location = new System.Drawing.Point(9, 454);
            this.chk1Star.Name = "chk1Star";
            this.chk1Star.Size = new System.Drawing.Size(44, 17);
            this.chk1Star.TabIndex = 30;
            this.chk1Star.Text = "1★";
            this.chk1Star.UseVisualStyleBackColor = true;
            this.chk1Star.CheckedChanged += new System.EventHandler(this.chk1Star_CheckedChanged);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSelect.Location = new System.Drawing.Point(706, 450);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 28;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(628, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 29;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // enchantToolStripMenuItem
            // 
            this.enchantToolStripMenuItem.Name = "enchantToolStripMenuItem";
            this.enchantToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.enchantToolStripMenuItem.Text = "&Enchant";
            this.enchantToolStripMenuItem.Click += new System.EventHandler(this.enchantToolStripMenuItem_Click);
            // 
            // previewToolStripMenuItem
            // 
            this.previewToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.previewToolStripMenuItem.Name = "previewToolStripMenuItem";
            this.previewToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.previewToolStripMenuItem.Text = "&Preview";
            this.previewToolStripMenuItem.Click += new System.EventHandler(this.previewToolStripMenuItem_Click);
            // 
            // label53
            // 
            this.label53.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label53.ForeColor = System.Drawing.Color.White;
            this.label53.Location = new System.Drawing.Point(260, 452);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(354, 18);
            this.label53.TabIndex = 35;
            this.label53.Text = "(right-click a rune for options)";
            this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mnuRuneRightClick
            // 
            this.mnuRuneRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previewToolStripMenuItem,
            this.enchantToolStripMenuItem,
            this.sellToolStripMenuItem});
            this.mnuRuneRightClick.Name = "mnuCardRightClick";
            this.mnuRuneRightClick.Size = new System.Drawing.Size(121, 70);
            this.mnuRuneRightClick.Opening += new System.ComponentModel.CancelEventHandler(this.mnuRuneRightClick_Opening);
            // 
            // colSkill
            // 
            this.colSkill.Text = "Skill";
            this.colSkill.Width = 132;
            // 
            // colElement
            // 
            this.colElement.Text = "Element";
            this.colElement.Width = 75;
            // 
            // colStars
            // 
            this.colStars.Text = "Stars";
            this.colStars.Width = 88;
            // 
            // colLevel
            // 
            this.colLevel.Text = "Level";
            this.colLevel.Width = 42;
            // 
            // colRuneName
            // 
            this.colRuneName.Text = "Rune";
            this.colRuneName.Width = 299;
            // 
            // lstRunes
            // 
            this.lstRunes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstRunes.CheckBoxes = true;
            this.lstRunes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRuneName,
            this.colLevel,
            this.colStars,
            this.colElement,
            this.colSkill,
            this.colValue});
            this.lstRunes.ContextMenuStrip = this.mnuRuneRightClick;
            this.lstRunes.FullRowSelect = true;
            this.lstRunes.GridLines = true;
            this.lstRunes.Location = new System.Drawing.Point(9, 13);
            this.lstRunes.Name = "lstRunes";
            this.lstRunes.Size = new System.Drawing.Size(771, 431);
            this.lstRunes.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstRunes.TabIndex = 27;
            this.lstRunes.UseCompatibleStateImageBehavior = false;
            this.lstRunes.View = System.Windows.Forms.View.Details;
            this.lstRunes.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstRunes_ColumnClick);
            this.lstRunes.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstRunes_MouseDoubleClick);
            // 
            // sellToolStripMenuItem
            // 
            this.sellToolStripMenuItem.Name = "sellToolStripMenuItem";
            this.sellToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.sellToolStripMenuItem.Text = "&Sell";
            this.sellToolStripMenuItem.Click += new System.EventHandler(this.sellToolStripMenuItem_Click);
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            this.colValue.Width = 104;
            // 
            // frmSelectRune
            // 
            this.AcceptButton = this.btnSelect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(29)))), ((int)(((byte)(33)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(790, 479);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.chk5Star);
            this.Controls.Add(this.chk4Star);
            this.Controls.Add(this.chk3Star);
            this.Controls.Add(this.chk2Star);
            this.Controls.Add(this.chk1Star);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label53);
            this.Controls.Add(this.lstRunes);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(763, 510);
            this.Name = "frmSelectRune";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EK Unleashed :: Select a rune...";
            this.Shown += new System.EventHandler(this.frmSelectRune_Shown);
            this.mnuRuneRightClick.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox chk5Star;
        private System.Windows.Forms.CheckBox chk4Star;
        private System.Windows.Forms.CheckBox chk3Star;
        private System.Windows.Forms.CheckBox chk2Star;
        private System.Windows.Forms.CheckBox chk1Star;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolStripMenuItem enchantToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previewToolStripMenuItem;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.ContextMenuStrip mnuRuneRightClick;
        private System.Windows.Forms.ColumnHeader colSkill;
        private System.Windows.Forms.ColumnHeader colElement;
        private System.Windows.Forms.ColumnHeader colStars;
        private System.Windows.Forms.ColumnHeader colLevel;
        private System.Windows.Forms.ColumnHeader colRuneName;
        private System.Windows.Forms.ListView lstRunes;
        private System.Windows.Forms.ToolStripMenuItem sellToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader colValue;
    }
}