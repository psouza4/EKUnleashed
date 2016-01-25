namespace EKUnleashed
{
    partial class frmSelectDeck
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSelectDeck));
            this.lstDecks = new System.Windows.Forms.ListView();
            this.colActive = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSlot = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCards = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRunes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuDeckRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fillToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finishSwapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.setAsCurrentDeckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label53 = new System.Windows.Forms.Label();
            this.mnuDeckRightClick.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstDecks
            // 
            this.lstDecks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDecks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colActive,
            this.colSlot,
            this.colID,
            this.colCards,
            this.colRunes});
            this.lstDecks.ContextMenuStrip = this.mnuDeckRightClick;
            this.lstDecks.FullRowSelect = true;
            this.lstDecks.GridLines = true;
            this.lstDecks.Location = new System.Drawing.Point(8, 10);
            this.lstDecks.MultiSelect = false;
            this.lstDecks.Name = "lstDecks";
            this.lstDecks.Size = new System.Drawing.Size(813, 284);
            this.lstDecks.TabIndex = 0;
            this.lstDecks.UseCompatibleStateImageBehavior = false;
            this.lstDecks.View = System.Windows.Forms.View.Details;
            this.lstDecks.DoubleClick += new System.EventHandler(this.lstDecks_DoubleClick);
            // 
            // colActive
            // 
            this.colActive.Text = "Active";
            this.colActive.Width = 110;
            // 
            // colSlot
            // 
            this.colSlot.Text = "Deck Slot";
            this.colSlot.Width = 80;
            // 
            // colID
            // 
            this.colID.Text = "ID #";
            this.colID.Width = 55;
            // 
            // colCards
            // 
            this.colCards.Text = "Cards";
            this.colCards.Width = 379;
            // 
            // colRunes
            // 
            this.colRunes.Text = "Runes";
            this.colRunes.Width = 184;
            // 
            // mnuDeckRightClick
            // 
            this.mnuDeckRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fillToolStripMenuItem,
            this.toolStripMenuItem1,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.swapToolStripMenuItem,
            this.finishSwapToolStripMenuItem,
            this.toolStripMenuItem2,
            this.setAsCurrentDeckToolStripMenuItem});
            this.mnuDeckRightClick.Name = "mnuDeckRightClick";
            this.mnuDeckRightClick.Size = new System.Drawing.Size(174, 148);
            this.mnuDeckRightClick.Opening += new System.ComponentModel.CancelEventHandler(this.mnuDeckRightClick_Opening);
            // 
            // fillToolStripMenuItem
            // 
            this.fillToolStripMenuItem.Name = "fillToolStripMenuItem";
            this.fillToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.fillToolStripMenuItem.Text = "Fill...";
            this.fillToolStripMenuItem.Click += new System.EventHandler(this.fillToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(170, 6);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // swapToolStripMenuItem
            // 
            this.swapToolStripMenuItem.Name = "swapToolStripMenuItem";
            this.swapToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.swapToolStripMenuItem.Text = "Swap";
            this.swapToolStripMenuItem.Click += new System.EventHandler(this.swapToolStripMenuItem_Click);
            // 
            // finishSwapToolStripMenuItem
            // 
            this.finishSwapToolStripMenuItem.Name = "finishSwapToolStripMenuItem";
            this.finishSwapToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.finishSwapToolStripMenuItem.Text = "Finish Swap";
            this.finishSwapToolStripMenuItem.Click += new System.EventHandler(this.finishSwapToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(170, 6);
            // 
            // setAsCurrentDeckToolStripMenuItem
            // 
            this.setAsCurrentDeckToolStripMenuItem.Name = "setAsCurrentDeckToolStripMenuItem";
            this.setAsCurrentDeckToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.setAsCurrentDeckToolStripMenuItem.Text = "Set as current deck";
            this.setAsCurrentDeckToolStripMenuItem.Click += new System.EventHandler(this.setAsCurrentDeckToolStripMenuItem_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSelect.Location = new System.Drawing.Point(747, 300);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 3;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(669, 300);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label53
            // 
            this.label53.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label53.ForeColor = System.Drawing.Color.White;
            this.label53.Location = new System.Drawing.Point(5, 303);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(339, 18);
            this.label53.TabIndex = 26;
            this.label53.Text = "(right-click a deck for options)";
            this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmSelectDeck
            // 
            this.AcceptButton = this.btnSelect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(29)))), ((int)(((byte)(33)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(829, 330);
            this.Controls.Add(this.label53);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lstDecks);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(601, 277);
            this.Name = "frmSelectDeck";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EK Unleashed :: Select a deck...";
            this.mnuDeckRightClick.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstDecks;
        private System.Windows.Forms.ColumnHeader colSlot;
        private System.Windows.Forms.ColumnHeader colID;
        private System.Windows.Forms.ColumnHeader colCards;
        private System.Windows.Forms.ColumnHeader colRunes;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenuStrip mnuDeckRightClick;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem swapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem finishSwapToolStripMenuItem;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.ToolStripMenuItem fillToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem setAsCurrentDeckToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader colActive;
    }
}