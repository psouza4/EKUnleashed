using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace EKUnleashed
{
    public partial class frmImagePreview : Form
    {
        public enum PreviewTypes
        {
            Card,
            Rune,
            Other
        }

        public PreviewTypes PreviewType = PreviewTypes.Other;

        public bool IsExpanded = true;

        public frmImagePreview()
        {
            InitializeComponent();

            this.AllowDragging = true;
            this.MouseDown +=new MouseEventHandler(parent_MouseDown);
            this.MouseUp += new MouseEventHandler(parent_MouseUp);
            this.MouseMove +=new MouseEventHandler(parent_MouseMove);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        public bool AllowDragging = true;

        private bool isDragging = false;
        private bool isMouseDown = false;
        private Point OriginalLocationBeforeDrag;

        public bool IsDragging
        {
            get
            {
                return this.isDragging;
            }
        }

        public void parent_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isMouseDown = true;

                OriginalLocationBeforeDrag = e.Location;
            }
        }

        public void parent_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isMouseDown = false;

                if (isDragging)
                    isDragging = false;
                else
                    this.Close();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (this.PreviewType == PreviewTypes.Card || this.PreviewType == PreviewTypes.Rune)
                {
                    if (this.IsExpanded)
                        this.Width -= 262;
                    else
                        this.Width += 262;

                    this.IsExpanded = !this.IsExpanded;
                }

                this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 6, 6));
            }
        }

        public void parent_MouseMove(object sender, MouseEventArgs e)
        {
            Point NewCursorLocation = new Point(Cursor.Position.X - OriginalLocationBeforeDrag.X, Cursor.Position.Y - OriginalLocationBeforeDrag.Y);

            if (isMouseDown)
            {
                if (this.Location.X != NewCursorLocation.X || this.Location.Y != NewCursorLocation.Y)
                    isDragging = true;

                if (isDragging && AllowDragging)
                    this.Location = NewCursorLocation;
            }
            else
                isDragging = false;
        }

        private void frmImagePreview_Shown(object sender, EventArgs e)
        {
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 6, 6));
        }
    }
}
