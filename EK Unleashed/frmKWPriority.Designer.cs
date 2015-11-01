namespace EKUnleashed
{
    partial class frmKWPriority
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmKWPriority));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnMoveLeft = new System.Windows.Forms.Button();
            this.btnMoveRight = new System.Windows.Forms.Button();
            this.lstKWTargets = new System.Windows.Forms.ListView();
            this.colTargets = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstKWDisabledTargets = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnViewMap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(11, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(257, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Attack These Targets In Order:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(407, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(234, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Don\'t Attack These Targets:";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSave.Location = new System.Drawing.Point(652, 459);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(574, 459);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnMoveLeft
            // 
            this.btnMoveLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveLeft.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnMoveLeft.Location = new System.Drawing.Point(333, 72);
            this.btnMoveLeft.Name = "btnMoveLeft";
            this.btnMoveLeft.Size = new System.Drawing.Size(75, 23);
            this.btnMoveLeft.TabIndex = 9;
            this.btnMoveLeft.Text = "<<";
            this.btnMoveLeft.UseVisualStyleBackColor = false;
            this.btnMoveLeft.Click += new System.EventHandler(this.btnMoveLeft_Click);
            // 
            // btnMoveRight
            // 
            this.btnMoveRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveRight.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnMoveRight.Location = new System.Drawing.Point(333, 101);
            this.btnMoveRight.Name = "btnMoveRight";
            this.btnMoveRight.Size = new System.Drawing.Size(75, 23);
            this.btnMoveRight.TabIndex = 10;
            this.btnMoveRight.Text = ">>";
            this.btnMoveRight.UseVisualStyleBackColor = false;
            this.btnMoveRight.Click += new System.EventHandler(this.btnMoveRight_Click);
            // 
            // lstKWTargets
            // 
            this.lstKWTargets.AllowDrop = true;
            this.lstKWTargets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTargets});
            this.lstKWTargets.FullRowSelect = true;
            this.lstKWTargets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstKWTargets.Location = new System.Drawing.Point(15, 39);
            this.lstKWTargets.MultiSelect = false;
            this.lstKWTargets.Name = "lstKWTargets";
            this.lstKWTargets.Size = new System.Drawing.Size(316, 407);
            this.lstKWTargets.TabIndex = 11;
            this.lstKWTargets.UseCompatibleStateImageBehavior = false;
            this.lstKWTargets.View = System.Windows.Forms.View.Details;
            this.lstKWTargets.DragDrop += new System.Windows.Forms.DragEventHandler(this.lstKWTargets_DragDrop);
            this.lstKWTargets.DragOver += new System.Windows.Forms.DragEventHandler(this.lstKWTargets_DragOver);
            this.lstKWTargets.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstKWTargets_MouseDown);
            // 
            // colTargets
            // 
            this.colTargets.Width = 290;
            // 
            // lstKWDisabledTargets
            // 
            this.lstKWDisabledTargets.AllowDrop = true;
            this.lstKWDisabledTargets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstKWDisabledTargets.FullRowSelect = true;
            this.lstKWDisabledTargets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstKWDisabledTargets.Location = new System.Drawing.Point(411, 39);
            this.lstKWDisabledTargets.MultiSelect = false;
            this.lstKWDisabledTargets.Name = "lstKWDisabledTargets";
            this.lstKWDisabledTargets.Size = new System.Drawing.Size(316, 407);
            this.lstKWDisabledTargets.TabIndex = 12;
            this.lstKWDisabledTargets.UseCompatibleStateImageBehavior = false;
            this.lstKWDisabledTargets.View = System.Windows.Forms.View.Details;
            this.lstKWDisabledTargets.DragDrop += new System.Windows.Forms.DragEventHandler(this.lstKWDisabledTargets_DragDrop);
            this.lstKWDisabledTargets.DragOver += new System.Windows.Forms.DragEventHandler(this.lstKWDisabledTargets_DragOver);
            this.lstKWDisabledTargets.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstKWDisabledTargets_MouseDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 290;
            // 
            // btnViewMap
            // 
            this.btnViewMap.Location = new System.Drawing.Point(15, 459);
            this.btnViewMap.Name = "btnViewMap";
            this.btnViewMap.Size = new System.Drawing.Size(171, 23);
            this.btnViewMap.TabIndex = 13;
            this.btnViewMap.Text = "View Kingdom War Map";
            this.btnViewMap.UseVisualStyleBackColor = true;
            this.btnViewMap.Click += new System.EventHandler(this.btnViewMap_Click);
            // 
            // frmKWPriority
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(29)))), ((int)(((byte)(33)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(741, 494);
            this.Controls.Add(this.btnViewMap);
            this.Controls.Add(this.lstKWDisabledTargets);
            this.Controls.Add(this.lstKWTargets);
            this.Controls.Add(this.btnMoveRight);
            this.Controls.Add(this.btnMoveLeft);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmKWPriority";
            this.Text = "EK Unleashed :: Set Kingdom War Attack Priority";
            this.Load += new System.EventHandler(this.frmKWPriority_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnMoveLeft;
        private System.Windows.Forms.Button btnMoveRight;
        private System.Windows.Forms.ListView lstKWTargets;
        private System.Windows.Forms.ColumnHeader colTargets;
        private System.Windows.Forms.ListView lstKWDisabledTargets;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button btnViewMap;
    }
}