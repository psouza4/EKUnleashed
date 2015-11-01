using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace EKUnleashed
{
    public class RTF
    {
        private RTF() { } // CA1053, http://msdn2.microsoft.com/library/ms182169(VS.90).aspx

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage_RECT(IntPtr hwndLock, UInt32 wMsg, IntPtr wParam, ref Point pt);

        private const int EM_GETSCROLLPOS = 0x0400 + 221;
        private const int EM_SETSCROLLPOS = 0x0400 + 222;

        public static void TextParser(ref RichTextBox rtxLog, string sText)
        {
            int x, z;
            int RRR, GGG, BBB;
            bool bIsBold, bIsItalic, bIsUnderline;
            string sThisChar;

            int selStart = rtxLog.SelectionStart;
            int selLen = rtxLog.SelectionLength;

            if (sText.Length > 2000)
                sText = sText.Substring(0, 1997) + "...";

            sText = sText.Replace("\r", string.Empty);

            Point ptOriginalScrollPosition = new Point();
            RTF.SendMessage_RECT(rtxLog.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref ptOriginalScrollPosition);

            rtxLog.Select(rtxLog.TextLength, 0);
            rtxLog.ScrollToCaret();
            Point ptBottomOfScrollBoundaries_temp = new Point();
            RTF.SendMessage_RECT(rtxLog.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref ptBottomOfScrollBoundaries_temp);

            bool bResetScroll = (ptBottomOfScrollBoundaries_temp.Y - ptOriginalScrollPosition.Y) < 600;

            #region Formatting
            rtxLog.Select(rtxLog.TextLength, 0);

            Font f = rtxLog.SelectionFont;

            List<Color> fore_color = new List<Color>();
            List<Color> back_color = new List<Color>();

            fore_color.Add(rtxLog.SelectionColor);
            back_color.Add(rtxLog.SelectionBackColor);

            for (x = 0; x < sText.Length; x++)
            {
                bIsBold = rtxLog.SelectionFont.Bold;
                bIsItalic = rtxLog.SelectionFont.Italic;
                bIsUnderline = rtxLog.SelectionFont.Underline;

                sThisChar = sText.Substring(x, 1);

                if (sThisChar == "<")
                {
                    try
                    {
                        if (sText.Substring(x, 5).ToLower() == "<fs+>")
                        {
                            rtxLog.SelectionFont = new Font(rtxLog.SelectionFont.Name, rtxLog.SelectionFont.Size + 1.0F, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 4;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 5).ToLower() == "<fs->")
                        {
                            rtxLog.SelectionFont = new Font(rtxLog.SelectionFont.Name, rtxLog.SelectionFont.Size - 1.0F, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 4;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "<fa>")
                        {
                            rtxLog.SelectionFont = new Font("Arial", rtxLog.SelectionFont.Size, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "<fl>")
                        {
                            rtxLog.SelectionFont = new Font("Lucida Console", rtxLog.SelectionFont.Size, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "<fv>")
                        {
                            rtxLog.SelectionFont = new Font("Verdana", rtxLog.SelectionFont.Size, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "<ft>")
                        {
                            rtxLog.SelectionFont = new Font("Tahoma", rtxLog.SelectionFont.Size, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "<fw>")
                        {
                            rtxLog.SelectionFont = new Font("Wingdings", rtxLog.SelectionFont.Size, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "<fx>")
                        {
                            rtxLog.SelectionFont = f;
                            x += 3;
                            continue;
                        }
                    }
                    catch { }


                    try
                    {
                        if (sText.Substring(x, 3).ToLower() == "<b>")
                        {
                            if (bIsBold == false)
                                rtxLog.SelectionFont = new Font(rtxLog.SelectionFont, FontStyle.Bold | ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 2;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "</b>")
                        {
                            rtxLog.SelectionFont = new Font(rtxLog.SelectionFont, ((bIsItalic == true) ? FontStyle.Italic : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 3).ToLower() == "<u>")
                        {
                            if (bIsUnderline == false)
                                rtxLog.SelectionFont = new Font(rtxLog.SelectionFont, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0) | FontStyle.Underline);
                            x += 2;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "</u>")
                        {
                            rtxLog.SelectionFont = new Font(rtxLog.SelectionFont, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsItalic == true) ? FontStyle.Italic : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 3).ToLower() == "<i>")
                        {
                            if (bIsItalic == false)
                                rtxLog.SelectionFont = new Font(rtxLog.SelectionFont, ((bIsBold == true) ? FontStyle.Bold : 0) | FontStyle.Italic | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 2;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 4).ToLower() == "</i>")
                        {
                            rtxLog.SelectionFont = new Font(rtxLog.SelectionFont, ((bIsBold == true) ? FontStyle.Bold : 0) | ((bIsUnderline == true) ? FontStyle.Underline : 0));
                            x += 3;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 7).ToLower() == "<color=")
                        {
                            /*
                            if (sText.Substring(x + 7, 5) == "RESET")
                            {
                                rtxLog.SelectionColor = rtxLog.ForeColor;
                                x += 12;
                                continue;
                            }
                            */
                            if (sText.Substring(x + 7, 1) == "#")
                            {
                                RRR = Convert.ToInt32(sText.Substring(x + 8, 2), 16);
                                GGG = Convert.ToInt32(sText.Substring(x + 10, 2), 16);
                                BBB = Convert.ToInt32(sText.Substring(x + 12, 2), 16);
                            }
                            else
                            {
                                RRR = Convert.ToInt32(sText.Substring(x + 7, 2), 16);
                                GGG = Convert.ToInt32(sText.Substring(x + 9, 2), 16);
                                BBB = Convert.ToInt32(sText.Substring(x + 11, 2), 16);
                            }
                            fore_color.Add(rtxLog.SelectionColor);
                            rtxLog.SelectionColor = Color.FromArgb(RRR, GGG, BBB);
                            if (sText.Substring(x + 7, 1) == "#")
                                x++;
                            x += 13;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 8).ToLower() == "</color>")
                        {
                            if (fore_color.Count > 0)
                            {
                                rtxLog.SelectionColor = fore_color[fore_color.Count - 1];
                                fore_color.RemoveAt(fore_color.Count - 1);
                            }
                            else
                                rtxLog.SelectionColor = rtxLog.ForeColor;
                            x += 7;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 11).ToLower() == "<backcolor=")
                        {
                            if (sText.Substring(x + 11, 1) == "#")
                            {
                                RRR = Convert.ToInt32(sText.Substring(x + 12, 2), 16);
                                GGG = Convert.ToInt32(sText.Substring(x + 14, 2), 16);
                                BBB = Convert.ToInt32(sText.Substring(x + 16, 2), 16);
                            }
                            else
                            {
                                RRR = Convert.ToInt32(sText.Substring(x + 11, 2), 16);
                                GGG = Convert.ToInt32(sText.Substring(x + 13, 2), 16);
                                BBB = Convert.ToInt32(sText.Substring(x + 15, 2), 16);
                            }
                            back_color.Add(rtxLog.SelectionBackColor);
                            rtxLog.SelectionBackColor = Color.FromArgb(RRR, GGG, BBB);
                            if (sText.Substring(x + 11, 1) == "#")
                                x++;
                            x += 17;
                            continue;
                        }
                    }
                    catch { }

                    try
                    {
                        if (sText.Substring(x, 12).ToLower() == "</backcolor>")
                        {
                            if (back_color.Count > 0)
                            {
                                rtxLog.SelectionBackColor = back_color[back_color.Count - 1];
                                back_color.RemoveAt(back_color.Count - 1);
                            }
                            else
                                rtxLog.SelectionBackColor = rtxLog.BackColor;
                            x += 11;
                            continue;
                        }
                    }
                    catch { }
                }
                else
                {
                    z = sText.Substring(x).IndexOf("<");
                    if (z > 0)
                    {
                        rtxLog.AppendText(sText.Substring(x, z));
                        x += z - 1;
                        continue;
                    }
                }

                rtxLog.AppendText(sText.Substring(x, 1));
            }
            #endregion

            #region Removing old text
            int iMaxTries = 10;
            try
            {
                while (rtxLog.Lines.Length > 1000)
                {
                    if (--iMaxTries < 0)
                        break;

                    if (rtxLog.Text.Contains("\n"))
                    {
                        rtxLog.Select(0, rtxLog.Text.IndexOf("\n") + 1);
                        selStart -= rtxLog.SelectionLength;
                        ptOriginalScrollPosition.Y -= 22;
                        if (ptOriginalScrollPosition.Y < 0)
                            ptOriginalScrollPosition.Y = 0;
                        rtxLog.SelectedText = string.Empty;
                    }
                }
            }
            catch { }
            #endregion

            #region Resetting scroll position

            if (bResetScroll || selStart < 1)
            {
                rtxLog.Select(rtxLog.TextLength, 0);
                rtxLog.ScrollToCaret();
            }
            else
            {
                try
                {
                    if (selStart >= rtxLog.TextLength)
                        rtxLog.Select(rtxLog.TextLength, 0);
                    else if (selStart + selLen >= rtxLog.TextLength)
                        rtxLog.Select(selStart, 0);
                    else
                        rtxLog.Select(selStart, selLen);
                }
                catch { }

                RTF.SendMessage_RECT(rtxLog.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ref ptOriginalScrollPosition);
            }

            #endregion

            return;
        }

        public static void log(ref RichTextBox rtxLog, string sText)
        {
            RTF.log(ref rtxLog, sText, false);
            return;
        }

        public static void log(ref RichTextBoxLinks rtxLog, string sText)
        {
            RichTextBox x = (RichTextBox)rtxLog;
            RTF.log(ref x, sText, false);
            return;
        }

        public static void log(ref RichTextBox rtxLog, string sText, bool bWantDateTime)
        {
            string sAdd = "";
            if (bWantDateTime == true)
                sAdd = DateTime.Now.ToShortTimeString() + " :: ";
            RTF.TextParser(ref rtxLog, sAdd + sText + "\r\n");
            return;
        }

        public static void log_start_no_CR(ref RichTextBox rtxLog, string sText)
        {
            RTF.TextParser(ref rtxLog, DateTime.Now.ToShortTimeString() + " :: " + sText);
            return;
        }

        public static void log_no_CR(ref RichTextBox rtxLog, string sText)
        {
            RTF.TextParser(ref rtxLog, sText);
            return;
        }

        public static void log_no_CR(ref RichTextBoxLinks rtxLog, string sText)
        {
            RichTextBox x = (RichTextBox)rtxLog;
            RTF.TextParser(ref x, sText);
            return;
        }

        public static void log_with_links(ref RichTextBoxLinks w, string s)
        {
            s = s.Replace("{{{", "<");
            s = s.Replace("}}}", ">");

            while (s.Contains("<link>") && s.Contains("</link>"))
            {
                string sAllTextUpToLink = Utils.Chopper(s, string.Empty, "<link>");

                string sRawLinkInfo = Utils.Chopper(s, "<link>", "</link>");

                string sLinkText = Utils.Chopper(sRawLinkInfo, "<text>", "</text>");
                string sLinkURL = Utils.Chopper(sRawLinkInfo, "<url>", "</url>");

                // this replaces all instances of the link -- we don't want that
                //s = s.Replace(sAllTextUpToLink + "<link>" + sRawLinkInfo + "</link>", string.Empty);

                // this is a single replacement
                s = Utils.StringReplace(s, sAllTextUpToLink + "<link>" + sRawLinkInfo + "</link>", string.Empty, true, Utils.StringReplacePosition.Anywhere);

                if (sLinkText.Trim().Length == 0)
                    sLinkText = sLinkURL;
                if (sLinkText.Trim().Length != 0)
                {
                    RTF.log_no_CR(ref w, sAllTextUpToLink);

                    Color cBackup = w.SelectionColor;

                    w.InsertLink(sLinkText, sLinkURL);

                    w.SelectionColor = cBackup;
                }
            }

            RTF.log(ref w, s);
        }

        public static IntPtr DontDrawRichEditControl(System.Windows.Forms.RichTextBox c)
        {
            IntPtr eventMask = IntPtr.Zero;

            try
            {
                // Stop redrawing:
                RTF.SendMessage_generic(c.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                // Stop sending of events:
                eventMask = RTF.SendMessage_generic(c.Handle, EM_GETEVENTMASK, IntPtr.Zero, IntPtr.Zero);

                // change colors and stuff in the RichTextBox
            }
            catch
            {
                try
                {
                    // turn on events
                    RTF.SendMessage_generic(c.Handle, EM_SETEVENTMASK, IntPtr.Zero, eventMask);
                    // turn on redrawing
                    RTF.SendMessage_generic(c.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                }
                catch { }
            }

            return eventMask;
        }

        public static void DrawRichEditControl(System.Windows.Forms.RichTextBox c, IntPtr eventMask)
        {
            try
            {
                // turn on events
                //RTF.SendMessage_generic(c.Handle, EM_SETEVENTMASK, IntPtr.Zero, eventMask);

                // turn on redrawing
                RTF.SendMessage_generic(c.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);

                // perform the actual redraw
                //RTF.InvalidateRect(c.Handle, IntPtr.Zero, FALSE);
                // invalidation is done with Control.Refresh() now
            }
            catch { }
        }

        public const int WM_SETREDRAW = 0x000B;
        public const int WM_USER = 0x400;
        public const int EM_GETEVENTMASK = (WM_USER + 59);
        public const int EM_SETEVENTMASK = (WM_USER + 69);
        public const int FALSE = 0;
        public const int TRUE = 1;

        [DllImport("user32", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private extern static IntPtr SendMessage_generic(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static IntPtr InvalidateRect(IntPtr hWnd, IntPtr rect, int bErase);

    }
}