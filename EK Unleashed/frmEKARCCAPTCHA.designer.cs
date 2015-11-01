namespace EKUnleashed
{
    partial class frmEKARCCAPTCHA
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
            this.picCAPTCHA = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAnswer = new System.Windows.Forms.TextBox();
            this.btnAnswer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picCAPTCHA)).BeginInit();
            this.SuspendLayout();
            // 
            // picCAPTCHA
            // 
            this.picCAPTCHA.Location = new System.Drawing.Point(117, 12);
            this.picCAPTCHA.Name = "picCAPTCHA";
            this.picCAPTCHA.Size = new System.Drawing.Size(200, 76);
            this.picCAPTCHA.TabIndex = 0;
            this.picCAPTCHA.TabStop = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(410, 57);
            this.label1.TabIndex = 1;
            this.label1.Text = "The Elemental Kingdoms ARC login server is asking for you to solve this CAPTCHA b" +
    "efore you may log in.  Usually, solving the CAPTCHA once will prevent it from re" +
    "curring.";
            // 
            // txtAnswer
            // 
            this.txtAnswer.Location = new System.Drawing.Point(127, 175);
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.Size = new System.Drawing.Size(100, 20);
            this.txtAnswer.TabIndex = 2;
            this.txtAnswer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnAnswer
            // 
            this.btnAnswer.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAnswer.Location = new System.Drawing.Point(233, 173);
            this.btnAnswer.Name = "btnAnswer";
            this.btnAnswer.Size = new System.Drawing.Size(75, 23);
            this.btnAnswer.TabIndex = 3;
            this.btnAnswer.Text = "Answer";
            this.btnAnswer.UseVisualStyleBackColor = true;
            // 
            // frmNZBMatrixCAPTCHA
            // 
            this.AcceptButton = this.btnAnswer;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 208);
            this.Controls.Add(this.btnAnswer);
            this.Controls.Add(this.txtAnswer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picCAPTCHA);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNZBMatrixCAPTCHA";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Elemental Kingdoms Login CAPTCHA";
            this.Shown += new System.EventHandler(this.frmEKARCCAPTCHA_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.picCAPTCHA)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picCAPTCHA;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAnswer;
        private System.Windows.Forms.Button btnAnswer;
    }
}