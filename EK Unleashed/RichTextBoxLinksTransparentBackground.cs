using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace EKUnleashed
{
    public class RichTextBoxLinksTransparentBackground : RichTextBoxLinks
    {
        protected override CreateParams CreateParams
        {
            get
            {
                //This makes the control's background transparent
                CreateParams CP = base.CreateParams;
                CP.ExStyle |= 0x20;
                return CP;
            }
        }

        public bool Drawing = false;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0203) // WM_LBUTTONDBLCLK
            {
                // Do nothing:
                // This prevents embedded RichText objects from being edited by double-clicking them from a
                // RichText control.  See:  http://stackoverflow.com/questions/1149811/net-framework-how-to-make-richtextbox-true-read-only
            }
            else if (m.Msg == 0x000F) // WM_PAINT
            {
                if (!Drawing)
                {
                    base.WndProc(ref m); // if we decided to paint this control, just call the RichTextBox WndProc
                }
                else
                {
                    m.Result = IntPtr.Zero; // not painting, must set this to IntPtr.Zero if not painting otherwise serious problems.
                }
            }
            else
            {
                try
                {
                    base.WndProc(ref m);
                }
                catch { }
            }
        }
    }
}
