namespace EKUnleashed
{
    partial class frmSelectCard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSelectCard));
            this.lstCards = new System.Windows.Forms.ListView();
            this.colCardName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStars = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colElement = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWait = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSkill1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSkill2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSkill3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSkill4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuCardRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.previewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enchantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chk1Star = new System.Windows.Forms.CheckBox();
            this.chk2Star = new System.Windows.Forms.CheckBox();
            this.chk4Star = new System.Windows.Forms.CheckBox();
            this.chk3Star = new System.Windows.Forms.CheckBox();
            this.chk5Star = new System.Windows.Forms.CheckBox();
            this.label53 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.evolveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.chkEvoChoices = new System.Windows.Forms.CheckBox();
            this.showEvolutionSkillsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCardRightClick.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstCards
            // 
            this.lstCards.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCards.CheckBoxes = true;
            this.lstCards.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCardName,
            this.colLevel,
            this.colStars,
            this.colElement,
            this.colCost,
            this.colWait,
            this.colSkill1,
            this.colSkill2,
            this.colSkill3,
            this.colSkill4,
            this.colValue});
            this.lstCards.ContextMenuStrip = this.mnuCardRightClick;
            this.lstCards.FullRowSelect = true;
            this.lstCards.GridLines = true;
            this.lstCards.Location = new System.Drawing.Point(9, 13);
            this.lstCards.Name = "lstCards";
            this.lstCards.Size = new System.Drawing.Size(981, 431);
            this.lstCards.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstCards.TabIndex = 1;
            this.lstCards.UseCompatibleStateImageBehavior = false;
            this.lstCards.View = System.Windows.Forms.View.Details;
            this.lstCards.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstCards_ColumnClick);
            this.lstCards.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstCards_MouseDoubleClick);
            // 
            // colCardName
            // 
            this.colCardName.Text = "Card";
            this.colCardName.Width = 202;
            // 
            // colLevel
            // 
            this.colLevel.Text = "Level";
            this.colLevel.Width = 42;
            // 
            // colStars
            // 
            this.colStars.Text = "Stars";
            this.colStars.Width = 88;
            // 
            // colElement
            // 
            this.colElement.Text = "Element";
            this.colElement.Width = 75;
            // 
            // colCost
            // 
            this.colCost.Text = "Cost";
            this.colCost.Width = 42;
            // 
            // colWait
            // 
            this.colWait.Text = "Wait";
            this.colWait.Width = 42;
            // 
            // colSkill1
            // 
            this.colSkill1.Text = "Lv. 0 Skill";
            this.colSkill1.Width = 100;
            // 
            // colSkill2
            // 
            this.colSkill2.Text = "Lv. 5 Skill";
            this.colSkill2.Width = 100;
            // 
            // colSkill3
            // 
            this.colSkill3.Text = "Lv. 10 Skill";
            this.colSkill3.Width = 100;
            // 
            // colSkill4
            // 
            this.colSkill4.Text = "Evolved Skill";
            this.colSkill4.Width = 100;
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            // 
            // mnuCardRightClick
            // 
            this.mnuCardRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previewToolStripMenuItem,
            this.toolStripMenuItem2,
            this.enchantToolStripMenuItem,
            this.showEvolutionSkillsToolStripMenuItem,
            this.evolveToolStripMenuItem,
            this.toolStripMenuItem1,
            this.sellToolStripMenuItem});
            this.mnuCardRightClick.Name = "mnuCardRightClick";
            this.mnuCardRightClick.Size = new System.Drawing.Size(200, 148);
            this.mnuCardRightClick.Opening += new System.ComponentModel.CancelEventHandler(this.mnuCardRightClick_Opening);
            // 
            // previewToolStripMenuItem
            // 
            this.previewToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.previewToolStripMenuItem.Name = "previewToolStripMenuItem";
            this.previewToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.previewToolStripMenuItem.Text = "&Preview";
            this.previewToolStripMenuItem.Click += new System.EventHandler(this.previewToolStripMenuItem_Click);
            // 
            // enchantToolStripMenuItem
            // 
            this.enchantToolStripMenuItem.Name = "enchantToolStripMenuItem";
            this.enchantToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.enchantToolStripMenuItem.Text = "&Enchant";
            this.enchantToolStripMenuItem.Click += new System.EventHandler(this.enchantToolStripMenuItem_Click);
            // 
            // sellToolStripMenuItem
            // 
            this.sellToolStripMenuItem.Name = "sellToolStripMenuItem";
            this.sellToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.sellToolStripMenuItem.Text = "&Sell";
            this.sellToolStripMenuItem.Click += new System.EventHandler(this.sellToolStripMenuItem_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSelect.Location = new System.Drawing.Point(915, 450);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(837, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chk1Star
            // 
            this.chk1Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk1Star.AutoSize = true;
            this.chk1Star.ForeColor = System.Drawing.Color.White;
            this.chk1Star.Location = new System.Drawing.Point(9, 454);
            this.chk1Star.Name = "chk1Star";
            this.chk1Star.Size = new System.Drawing.Size(41, 17);
            this.chk1Star.TabIndex = 7;
            this.chk1Star.Text = "1★";
            this.chk1Star.UseVisualStyleBackColor = true;
            this.chk1Star.CheckedChanged += new System.EventHandler(this.chk1Star_CheckedChanged);
            // 
            // chk2Star
            // 
            this.chk2Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk2Star.AutoSize = true;
            this.chk2Star.ForeColor = System.Drawing.Color.White;
            this.chk2Star.Location = new System.Drawing.Point(60, 454);
            this.chk2Star.Name = "chk2Star";
            this.chk2Star.Size = new System.Drawing.Size(41, 17);
            this.chk2Star.TabIndex = 8;
            this.chk2Star.Text = "2★";
            this.chk2Star.UseVisualStyleBackColor = true;
            this.chk2Star.CheckedChanged += new System.EventHandler(this.chk2Star_CheckedChanged);
            // 
            // chk4Star
            // 
            this.chk4Star.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk4Star.AutoSize = true;
            this.chk4Star.ForeColor = System.Drawing.Color.White;
            this.chk4Star.Location = new System.Drawing.Point(162, 454);
            this.chk4Star.Name = "chk4Star";
            this.chk4Star.Size = new System.Drawing.Size(41, 17);
            this.chk4Star.TabIndex = 10;
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
            this.chk3Star.Size = new System.Drawing.Size(41, 17);
            this.chk3Star.TabIndex = 9;
            this.chk3Star.Text = "3★";
            this.chk3Star.UseVisualStyleBackColor = true;
            this.chk3Star.CheckedChanged += new System.EventHandler(this.chk3Star_CheckedChanged);
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
            this.chk5Star.Size = new System.Drawing.Size(41, 17);
            this.chk5Star.TabIndex = 11;
            this.chk5Star.Text = "5★";
            this.chk5Star.UseVisualStyleBackColor = true;
            this.chk5Star.CheckedChanged += new System.EventHandler(this.chk5Star_CheckedChanged);
            // 
            // label53
            // 
            this.label53.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label53.ForeColor = System.Drawing.Color.White;
            this.label53.Location = new System.Drawing.Point(550, 452);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(203, 18);
            this.label53.TabIndex = 25;
            this.label53.Text = "(right-click a card for options)";
            this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnRefresh.Location = new System.Drawing.Point(759, 450);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 26;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // evolveToolStripMenuItem
            // 
            this.evolveToolStripMenuItem.Name = "evolveToolStripMenuItem";
            this.evolveToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.evolveToolStripMenuItem.Text = "E&volve";
            this.evolveToolStripMenuItem.Click += new System.EventHandler(this.evolveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(196, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(196, 6);
            // 
            // chkEvoChoices
            // 
            this.chkEvoChoices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkEvoChoices.AutoSize = true;
            this.chkEvoChoices.ForeColor = System.Drawing.Color.White;
            this.chkEvoChoices.Location = new System.Drawing.Point(297, 454);
            this.chkEvoChoices.Name = "chkEvoChoices";
            this.chkEvoChoices.Size = new System.Drawing.Size(155, 17);
            this.chkEvoChoices.TabIndex = 27;
            this.chkEvoChoices.Text = "Show All Evolution Choices";
            this.chkEvoChoices.UseVisualStyleBackColor = true;
            this.chkEvoChoices.CheckedChanged += new System.EventHandler(this.chkEvoChoices_CheckedChanged);
            // 
            // showEvolutionSkillsToolStripMenuItem
            // 
            this.showEvolutionSkillsToolStripMenuItem.Name = "showEvolutionSkillsToolStripMenuItem";
            this.showEvolutionSkillsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.showEvolutionSkillsToolStripMenuItem.Text = "Show evolution choices";
            this.showEvolutionSkillsToolStripMenuItem.Click += new System.EventHandler(this.showEvolutionSkillsToolStripMenuItem_Click);
            // 
            // frmSelectCard
            // 
            this.AcceptButton = this.btnSelect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(29)))), ((int)(((byte)(33)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(999, 479);
            this.Controls.Add(this.chkEvoChoices);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.label53);
            this.Controls.Add(this.chk5Star);
            this.Controls.Add(this.chk4Star);
            this.Controls.Add(this.chk3Star);
            this.Controls.Add(this.chk2Star);
            this.Controls.Add(this.chk1Star);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lstCards);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(670, 300);
            this.Name = "frmSelectCard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EK Unleashed :: Select a card...";
            this.Shown += new System.EventHandler(this.frmSelectCard_Shown);
            this.mnuCardRightClick.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstCards;
        private System.Windows.Forms.ColumnHeader colCardName;
        private System.Windows.Forms.ColumnHeader colLevel;
        private System.Windows.Forms.ColumnHeader colSkill1;
        private System.Windows.Forms.ColumnHeader colSkill2;
        private System.Windows.Forms.ColumnHeader colSkill3;
        private System.Windows.Forms.ColumnHeader colCost;
        private System.Windows.Forms.ColumnHeader colWait;
        private System.Windows.Forms.ColumnHeader colElement;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ColumnHeader colStars;
        private System.Windows.Forms.CheckBox chk1Star;
        private System.Windows.Forms.CheckBox chk2Star;
        private System.Windows.Forms.CheckBox chk4Star;
        private System.Windows.Forms.CheckBox chk3Star;
        private System.Windows.Forms.CheckBox chk5Star;
        private System.Windows.Forms.ContextMenuStrip mnuCardRightClick;
        private System.Windows.Forms.ToolStripMenuItem previewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enchantToolStripMenuItem;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.ColumnHeader colSkill4;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ToolStripMenuItem sellToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem evolveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.CheckBox chkEvoChoices;
        private System.Windows.Forms.ToolStripMenuItem showEvolutionSkillsToolStripMenuItem;
    }
}