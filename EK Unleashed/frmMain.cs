using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Threading;
using System.Text.RegularExpressions;

namespace EKUnleashed
{
    public partial class frmMain : Form
    {
        public static GameClient Game = new GameClient();

        private Thread trdUIResponder = null;

        public frmMain()
        {
            InitializeComponent();
            this.LastTab = this.tabGeneral;
            System.Net.WebRequest.DefaultWebProxy = null;
            System.Net.ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            frmMain.Game.ParentForm = this;

            #region custom controls
            this.SuspendLayout();

            this.tabsGameInfo.BackColor = Color.White;

            this.lblApplicationMenu = new System.Windows.Forms.Label();
            this.lblGameMenu = new System.Windows.Forms.Label();
            this.lblDebugMenu = new System.Windows.Forms.Label();
            this.lblBuyMenu = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblResizeCorner = new System.Windows.Forms.Label();

            // 
            // lblApplicationMenu
            //
            this.lblApplicationMenu.BackColor = System.Drawing.Color.Transparent;
            this.lblApplicationMenu.ForeColor = Color.White;
            this.lblApplicationMenu.Location = new System.Drawing.Point(8, 27);
            this.lblApplicationMenu.Name = "lblApplicationMenu";
            this.lblApplicationMenu.Size = new System.Drawing.Size(72, 19);
            this.lblApplicationMenu.TabIndex = 0;
            this.lblApplicationMenu.Text = "Application";
            this.lblApplicationMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblApplicationMenu.MouseEnter += new System.EventHandler(this.lblApplicationMenu_MouseEnter);
            this.lblApplicationMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblApplicationMenu_MouseDown);
            this.lblApplicationMenu.MouseLeave += new System.EventHandler(this.lblApplicationMenu_MouseLeave);

            // 
            // lblGameMenu
            //
            this.lblGameMenu.BackColor = System.Drawing.Color.Transparent;
            this.lblGameMenu.ForeColor = Color.White;
            this.lblGameMenu.Location = new System.Drawing.Point(94, 27);
            this.lblGameMenu.Name = "lblGameMenu";
            this.lblGameMenu.Size = new System.Drawing.Size(42, 19);
            this.lblGameMenu.TabIndex = 1;
            this.lblGameMenu.Text = "Game";
            this.lblGameMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblGameMenu.MouseEnter += new System.EventHandler(this.lblGameMenu_MouseEnter);
            this.lblGameMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblGameMenu_MouseDown);
            this.lblGameMenu.MouseLeave += new System.EventHandler(this.lblGameMenu_MouseLeave);

            // 
            // lblBuyMenu
            //
            this.lblBuyMenu.BackColor = System.Drawing.Color.Transparent;
            this.lblBuyMenu.ForeColor = Color.White;
            this.lblBuyMenu.Location = new System.Drawing.Point(146, 27);
            this.lblBuyMenu.Name = "lblBuyMenu";
            this.lblBuyMenu.Size = new System.Drawing.Size(54, 19);
            this.lblBuyMenu.TabIndex = 1;
            this.lblBuyMenu.Text = "Buy";
            this.lblBuyMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBuyMenu.MouseEnter += new System.EventHandler(this.lblBuyMenu_MouseEnter);
            this.lblBuyMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblBuyMenu_MouseDown);
            this.lblBuyMenu.MouseLeave += new System.EventHandler(this.lblBuyMenu_MouseLeave);

            // 
            // lblDebugMenu
            //
            this.lblDebugMenu.BackColor = System.Drawing.Color.Transparent;
            this.lblDebugMenu.ForeColor = Color.White;
            this.lblDebugMenu.Location = new System.Drawing.Point(200, 27);
            this.lblDebugMenu.Name = "lblDebugMenu";
            this.lblDebugMenu.Size = new System.Drawing.Size(54, 19);
            this.lblDebugMenu.TabIndex = 1;
            this.lblDebugMenu.Text = "Debug";
            this.lblDebugMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDebugMenu.MouseEnter += new System.EventHandler(this.lblDebugMenu_MouseEnter);
            this.lblDebugMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblDebugMenu_MouseDown);
            this.lblDebugMenu.MouseLeave += new System.EventHandler(this.lblDebugMenu_MouseLeave);

            // 
            // lblResizeCorner
            //
            this.lblResizeCorner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResizeCorner.AutoSize = true;
            this.lblResizeCorner.BackColor = System.Drawing.Color.Transparent;
            this.lblResizeCorner.ForeColor = System.Drawing.Color.White;
            this.lblResizeCorner.Font = new System.Drawing.Font("Marlett", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
            this.lblResizeCorner.Location = new System.Drawing.Point(this.ClientSize.Width - 16, this.ClientSize.Height - 14);
            this.lblResizeCorner.Name = "lblResizeCorner";
            this.lblResizeCorner.Size = new System.Drawing.Size(16, 14);
            this.lblResizeCorner.TabIndex = 2;
            this.lblResizeCorner.Text = "p";
            this.lblResizeCorner.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelResize_MouseUp);
            this.lblResizeCorner.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelResize_MouseMove);
            this.lblResizeCorner.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelResize_MouseDown);
            this.lblResizeCorner.Cursor = Cursors.SizeNWSE;
            // 
            // MainForm
            //
            this.Controls.Add(this.lblResizeCorner);
            this.Controls.Add(this.lblApplicationMenu);
            this.Controls.Add(this.lblGameMenu);
            this.Controls.Add(this.lblBuyMenu);
            if (GameClient.Want_Debug)
                this.Controls.Add(this.lblDebugMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            this.ResumeLayout(false);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            #endregion

            ThreadStart ts = new ThreadStart(UIThreadResponder);
            this.trdUIResponder = new Thread(ts);
            this.trdUIResponder.IsBackground = true;
            this.trdUIResponder.Start();
        }

        private static object _DoEvents_locker = new object();
        private static bool _DoEvents_engaged = false;
        public static void DoEvents()
        {
            try
            {
                bool local__DoEventsEngaged = false;
                lock (_DoEvents_locker)
                {
                    local__DoEventsEngaged = _DoEvents_engaged;

                    if (!local__DoEventsEngaged)
                        _DoEvents_engaged = true;
                }

                if (!local__DoEventsEngaged)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                lock (_DoEvents_locker)
                {
                    _DoEvents_engaged = false;
                }
            }
            catch { }
        }

        private void UIThreadResponder()
        {
            try
            {
                for (; ; )
                {
                    frmMain.DoEvents();
                    Thread.Sleep(100);
                }
            }
            catch { }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // mostly handled by assigning a shortcut to the right-click menu
            bool bControl = ((keyData & Keys.Control) == Keys.Control);
            bool bShift = ((keyData & Keys.Shift) == Keys.Shift);
            bool bAlt = ((keyData & Keys.Alt) == Keys.Alt);
            bool bR = ((keyData & Keys.KeyCode) == Keys.R);
            bool bA = ((keyData & Keys.KeyCode) == Keys.A);
            bool bD = ((keyData & Keys.KeyCode) == Keys.D);
            bool bI = ((keyData & Keys.KeyCode) == Keys.I);
            //int ms_delay = 50;

            if ((keyData & Keys.KeyCode) == Keys.Enter)
            {
                if (Utils.ValidText(this.txtChatMessage.Text))
                {
                    string msg = this.txtChatMessage.Text;

                    if (frmMain.ChatChannel[0] == "W")
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Chat.MessageToGeneral(Game.FriendlyReplacerOutbound(msg)); });
                        this.txtChatMessage.Text = "";
                    }
                    else if ((frmMain.ChatChannel[0] == "C") && (Utils.ValidNumber(Game.Clan_ID)))
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Chat.MessageToClan(Game.Clan_ID, Game.FriendlyReplacerOutbound(msg)); });
                        this.txtChatMessage.Text = "";
                    }
                    else if ((frmMain.ChatChannel[0] == "P") && (Utils.ValidNumber(frmMain.ChatChannel[1])))
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Chat.PrivateMessage(frmMain.ChatChannel[1], Game.FriendlyReplacerOutbound(msg)); });
                        this.txtChatMessage.Text = "";
                    }
                    else if ((frmMain.ChatChannel[0] == "KW") && (Utils.CInt(Game.Kingdom_War_ID) > 0))
                    {
                        if (Game.KW_Ongoing)
                        {
                            Utils.StartMethodMultithreaded(() => { Game.Chat.MessageToChannel("Country_" + Game.Kingdom_War_ID, Game.FriendlyReplacerOutbound(msg)); });
                            this.txtChatMessage.Text = "";
                        }
                    }
                }
                return true;
            }

            // Normal modifiers
            if ((!bControl) && (!bShift) && (!bAlt))
            {
                if (Game.Want_Game_Login)
                {
                    // F2
                    if ((keyData & Keys.KeyCode) == Keys.F2)
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Play_SendFriendListEnergy(); Game.Play_ReceiveFriendListEnergy(); });
                        return true;
                    }

                    // F3
                    if ((keyData & Keys.KeyCode) == Keys.F3)
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Play_SpendEnergy(); });
                        return true;
                    }

                    // F4
                    if ((keyData & Keys.KeyCode) == Keys.F4)
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Play_Explore(); });
                        return true;
                    }

                    // F5
                    if ((keyData & Keys.KeyCode) == Keys.F5)
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Play_ArenaFight(); });
                        return true;
                    }

                    // F6
                    if ((keyData & Keys.KeyCode) == Keys.F6)
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Play_FightThieves(); });
                        return true;
                    }

                    // F7
                    if ((keyData & Keys.KeyCode) == Keys.F7)
                    {
                        Utils.StartMethodMultithreaded(Game.Play_FightMapInvasions);
                        return true;
                    }

                    // F8
                    if ((keyData & Keys.KeyCode) == Keys.F8)
                    {
                        Game.DoingDemonInvasion = !Game.DoingDemonInvasion;
                        return true;
                    }

                    // F9
                    if ((keyData & Keys.KeyCode) == Keys.F9)
                    {
                        Utils.StartMethodMultithreaded(() => { Game.Play_WorldTree(true); });
                        return true;
                    }

                    // F10
                    if ((keyData & Keys.KeyCode) == Keys.F10)
                    {
                        string card_name = Utils.Input_Text("Card Search", "Search for a card name in your collection:");
                        if (!string.IsNullOrEmpty(card_name))
                        {
                            this.tabsChatChannels.SelectedTab = this.tabNotifications;
                            Utils.StartMethodMultithreaded(() => { Game.CardSearch(card_name); }); 
                        }
                        return true;
                    }
                    // F11
                    if ((keyData & Keys.KeyCode) == Keys.F11)
                    {
                        string rune_name = Utils.Input_Text("Rune Search", "Search for a rune name in your collection:");
                        if (!string.IsNullOrEmpty(rune_name))
                        {
                            this.tabsChatChannels.SelectedTab = this.tabNotifications;
                            Utils.StartMethodMultithreaded(() => { Game.RuneSearch(rune_name); }); 
                        }
                        return true;
                    }
                }
            }

            // Control
            if ((bControl) && (!bShift) && (!bAlt))
            {
                // Ctrl+C, Ctrl+X, Ctrl+V, Ctrl+Z
                if ((keyData & Keys.KeyCode) == Keys.C) return base.ProcessDialogKey(keyData); // copy
                if ((keyData & Keys.KeyCode) == Keys.X) return base.ProcessDialogKey(keyData); // cut
                if ((keyData & Keys.KeyCode) == Keys.V) return base.ProcessDialogKey(keyData); // paste
                if ((keyData & Keys.KeyCode) == Keys.Z) return base.ProcessDialogKey(keyData); // undo
            }


            // Shift
            if ((!bControl) && (bShift) && (!bAlt))
            {
                // Shift+F3
                if ((keyData & Keys.KeyCode) == Keys.F3)
                {
                    this.fightMazeTowersonlyToolStripMenuItem_Click(null, null);
                    return true;
                }

                if ((keyData & Keys.KeyCode) == Keys.F9)
                {
                    this.fightRaidersToolStripMenuItem_Click(null, null);
                    return true;
                }
            }
            
            // Alt
            if ((!bControl) && (!bShift) && (bAlt))
            {               
                // Alt+D
                if (((keyData & Keys.KeyCode) == Keys.D) && Game.Want_Game_Login)
                {
                    Utils.StartMethodMultithreaded(() =>
                    {
                        string temp_deck_ordinal = Utils.Input_Text("Update Deck", "Enter deck ID to set these cards & runes to (\"KW\" or 1-10):").Trim().ToUpper();

                        if (((Utils.CInt(temp_deck_ordinal) > 0) && (Utils.CInt(temp_deck_ordinal) <= 10)) || temp_deck_ordinal.Trim().ToUpper() == "KW")
                        {
                            string temp_cards_entered = Utils.Input_Text("Update Deck", "Enter a list of cards you want to use (separated by commas):").Trim();

                            if (!string.IsNullOrEmpty(temp_cards_entered))
                            {
                                string temp_runes_entered = Utils.Input_Text("Update Deck", "Enter a list of runes you want to use (separated by commas)").Trim();

                                if (!string.IsNullOrEmpty(temp_runes_entered))
                                    Game.FillDeckCustom(temp_deck_ordinal, temp_cards_entered, temp_runes_entered);
                            }
                        }

                    });

                    return true;
                }

                // Alt+F1
                if (((keyData & Keys.KeyCode) == Keys.F1) && Game.Want_Game_Login)
                {
                    Utils.StartMethodMultithreaded(() =>
                    {
                        if (Game.Chat != null)
                            Game.Chat.Logout();

                        Game.MasterLogin();
                    });

                    return true;
                }

                // Alt+F2
                if ((keyData & Keys.KeyCode) == Keys.F2)
                {
                    try
                    {
                        Game.StopAllEvents();
                        Game.ScheduledEvents.Clear();
                    }
                    catch { }

                    try
                    {
                        if (Game.Chat != null)
                            Game.Chat.Logout();
                    }
                    catch { }

                    Utils.Chatter();
                    Utils.Chatter("<color=#40c0ff>Logged out from the game!</color>");
                    return true;
                }
            }

            return base.ProcessDialogKey(keyData);
        }

        private object locker = new object();

        delegate void VOID__STRING(string s);
        delegate void VOID__STRING_STRING(string s1, string s2);

        public void GameNotificationTrigger(string notification, string data)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING_STRING de = GameNotificationTrigger;
                this.Invoke(de, new object[] { notification, data});
                return;
            }

            switch (notification)
            {
                case "notification_bossarrive":
                    if (Game.DoingDemonInvasion)
                        break;

                    Utils.StartMethodMultithreaded(() =>
                    {
                        if (!Game.ChatIsConnected)
                            Game.MasterLogin();

                        if (!Game.DoingDemonInvasion)
                            if (Utils.False("Game_FightDemonInvasions"))
                                Game.DoingDemonInvasion = true;
                    });
                    break;

                case "boss_incoming":
                    string[] demon_info = Utils.SubStringsDups(data, "_");

                    if (demon_info.Length == 3)
                    {
                        if ((Utils.CInt(demon_info[0]) < 10) && (Utils.CInt(demon_info[0]) > 0))
                        {
                            Game.UserCards_CachedData = null;
                            Game.GetUsersCards();
                            Game.UserRunes_CachedData = null;
                            Game.GetUsersRunes();
                        }

                        Game.Current_Demon_MeritCard1_ID = demon_info[1];
                        Game.Current_Demon_MeritCard2_ID = demon_info[2];
                    }
                    break;

                case "boss_hp_update":
                    lock (this.locker)
                        this.lblDIHP.Text = Utils.CInt(data).ToString("#,##0");

                    if (!Game.DoingDemonInvasion)
                    {
                        Utils.StartMethodMultithreaded(() =>
                        {
                            if (!Game.ChatIsConnected)
                            {
                                Game.CheckLogin();
                                if (Game.opts == null)
                                    return;
                            }

                            if (!Game.DoingDemonInvasion)
                                if (Utils.False("Game_FightDemonInvasions"))
                                    Game.DoingDemonInvasion = true;

                        });
                    }
                    break;

                case "notification_bossdie":
                case "notification_award":
                    lock (this.locker)
                        Game._invasion_ended = true;
                    break;
            }
        }

        #region Logger

        public static frmMain ext()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType() == typeof(frmMain))
                    return (frmMain)form;
            }

            return null;
        }

        public void Logger(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = Logger;
                this.Invoke(d, sText);
                return;
            }

            RTF.log_with_links(ref this.rchOutput, sText);
        }

        public void LoggerChatGeneral(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatGeneral;
                this.Invoke(d, sText);
                return;
            }

            if (sText.Contains("[Clan #"))
                sText = sText.Replace("[Clan #" + Game.Clan_ID + "]", "[" + Game.Clan_Name + "]");

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchChatGeneral, sText);
        }

        public void LoggerChatPrivate(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatPrivate;
                this.Invoke(d, sText);
                return;
            }

            if (sText.Contains("[Clan #"))
                sText = sText.Replace("[Clan #" + Game.Clan_ID + "]", "[" + Game.Clan_Name + "]");

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchChatPrivate, sText);
        }

        public void LoggerChatClan(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatClan;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            if (sText != sText.Replace("[Clan #" + Game.Clan_ID + "]", "[" + Game.Clan_Name + "]"))
            {
                sText = sText.Replace("[Clan #" + Game.Clan_ID + "]", "[" + Game.Clan_Name + "]");

                RTF.log_with_links(ref this.rchChatClan, sText);
            }
            else
            {
                LoggerChatClansOther(sText);
            }
        }

        public void LoggerChatClansOther(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatClansOther;
                this.Invoke(d, sText);
                return;
            }

            RTF.log_with_links(ref this.rchChatClansOther, sText);
        }

        public void LoggerChatKWTundra(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatKWTundra;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchKWTundra, sText);
        }

        public void LoggerChatKWMountain(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatKWMountain;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchKWMountain, sText);
        }

        public void LoggerChatKWForest(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatKWForest;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchKWForest, sText);
        }

        public void LoggerChatKWSwamp(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerChatKWSwamp;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchKWSwamp, sText);
        }

        public void LoggerDebug(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerDebug;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchDebug, sText);
        }

        public void LoggerRewards(string sText)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING d = LoggerRewards;
                this.Invoke(d, sText);
                return;
            }

            sText = Game.FriendlyReplacerInbound(sText);

            RTF.log_with_links(ref this.rchNotifications, sText);

            if (sText.Contains("You have new rewards available"))
                if (!Game.DoingDemonInvasion)
                    Utils.StartMethodMultithreaded(() => { Game.Play_ClaimAllRewards(); });
        }
        #endregion

        #region Tray icon

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.Hide();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        #endregion

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (Game.ChatIsConnected)
                    Game.Chat.Logout();
            }
            catch { }

            try
            {
                this.trdUIResponder.Abort();
            }
            catch { }
        }

        public delegate void Delegate__Void();
        public delegate void Delegate__Void_Int(int i);

        public static string Common_AA = "320"; // magic number from Unity resources
        public static string Common_BB = "";
        public static string Common_CC = "";
        public static string Common_DD = "Ù„Ø´Ø§Ø´";
        public static string Common_EE = "";
        public static string Common_FF = "";
        public static int AuthType = 0;
        public static long AuthSerial = 0L;

        public static string CalculateEncryptedPacketData()
        {
            return string.Concat(new object[]
			{
				"&phpp=", "ANDROID_ARC",
				"&phpl=", "EN",
				"&pvc=",  "1.7.4",
				"&phpk=", frmMain.CalculateMD5SumForGamePacket(),
				"&phps=", frmMain.AuthSerial,
				"&pvb=", System.Web.HttpUtility.UrlEncode(GameClient.m_strBuildTime)
			});
        }

        private static string CalculateMD5SumForGamePacket()
        {
            if (!Utils.ValidNumber(GameClient.Current.Login_UID)) return "0";
            if (!Utils.ValidText(Common_AA)) return "0";
            //if (!Utils.ValidText(Common_BB)) return "0";
            //if (!Utils.ValidText(Common_CC)) return "0";
            //if (!Utils.ValidText(Common_DD)) return "0";
            //if (!Utils.ValidText(Common_EE)) return "0";
            //if (!Utils.ValidText(Common_FF)) return "0";

            string text = string.Concat(new object[]
			{
                GameClient.Current.Login_UID,
				AuthType.ToString(),
				"nj3dy&hf3h#jui$5lyf!s54", // hardcoded string
				AuthSerial.ToString()
			});

            //Utils.Chatter(GameClient.Current.Login_UID.ToString());
            //Utils.Chatter(AuthType.ToString());
            //Utils.Chatter(AuthSerial.ToString());
            //Utils.Chatter("All: " + text);

            switch (AuthType)
            {
                case 1:
                    // Done
                    text += Utils.GetMD5(Utils.CInt(Common_AA).ToString()).Substring(25);
                    break;

                case 2:
                    // Can't do: needs Unity
                    break;

                case 3:
                    // Done

                    if (GameClient.Current.Cards_JSON_Parsed == null)
                        return "0";

                    if (!Utils.ValidText(Common_CC))
                    {
                        try
                        {
                            Common_CC = GameClient.Current.GetCardByID(11)["MasterPacket"].ToString();
                            byte[] bs = System.Security.Cryptography.MD5.Create().ComputeHash(new UTF8Encoding().GetBytes(Common_CC));
                            string Common_CC_temp = string.Empty;
                            for (int i = 0; i < bs.Length; i++)
                                Common_CC_temp += Convert.ToString(bs[i], 16).PadLeft(2, '0');
                            Common_CC = Common_CC_temp.Substring(23);
                        }
                        catch
                        {
                            Common_CC = "";
                            return "0";
                        }
                    }

                    text += Common_CC;
                    break;

                case 4:
                    // Done, but doesn't appear to be working ?  needs more testing

                    text += Common_DD;
                    break;

                case 5:
                    // Can't do: needs Unity
                    break;

                case 6:
                    // Can't do: needs Unity
                    break;
            }

			byte[] bytes = new UTF8Encoding().GetBytes(text);
            byte[] array = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);

			string text2 = string.Empty;
			for (int i = 0; i < array.Length; i++)
				text2 += Convert.ToString(array[i], 16).PadLeft(2, '0');

            return text2.PadLeft(32, '0');
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            string version = "(unknown)";

            try
            {
                System.Diagnostics.FileVersionInfo fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
                version = fv.FileMajorPart + "." + Utils.CInt(fv.FileMinorPart).ToString("00") + "." + fv.FileBuildPart + "." + fv.FilePrivatePart;
            }
            catch { }

            Utils.Chatter();
            Utils.Chatter("<fs+><fs+><fs+><b><u><i>EK Unleashed</i></u></b> version " + version);

            /*
            //some quick code to scan for the magic number (brute force)
            for (int i = 0; i < 10000; i++)
            {
                string y = i.ToString();
                string x = "132933" + "1" + "nj3dy&hf3h#jui$5lyf!s54" + "909152908" + Utils.GetMD5(y).ToLower().Substring(25);

                string md5 = "e48021308c7bfc8c2f2b8e5ced178508";
                string md5_calc = Utils.GetMD5(x).ToLower();

                if (md5 == md5_calc)
                {
                    Utils.Chatter("Magic number is: " + i.ToString());
                    break;
                }

                //Utils.Chatter(Utils.GetMD5(x));
            }

            Utils.Chatter("Done.");
            return;
            */

            Scheduler.ScheduledEvent.AllowAllEvents();

            if (string.IsNullOrEmpty(Utils.GetAppSetting("Login_Account").Trim()) || Utils.GetAppSetting("Login_Account").Trim().StartsWith("["))
                return;
            if (string.IsNullOrEmpty(Utils.GetAppSetting("Login_Password").Trim()) || Utils.GetAppSetting("Login_Password").Trim().StartsWith("["))
                return;

            this.txtChatMessage.Focus();

            Utils.Chatter();
            Utils.Chatter("Getting the current server time...");

            uint time_check = 0;
            try
            {
                string result = Utils.CStr(Comm.Download("http://www.ekunleashed.com/time.php"));
                Utils.DebugLogger("Time result: " + result);

                time_check = 0;
                uint.TryParse(result, out time_check);
            }
            catch (Exception ex)
            {
                Utils.Chatter("... <color=#ff4040>FAILED!  Are you running an outdated version of the .NET Framework?</color>");
                Utils.Chatter();
                Utils.Chatter("<color=#ff4040>Check Windows Updates for all .NET updates.</color>");
                Utils.Chatter();
                Utils.Chatter("<color=#ff4040>Full error: " + Errors.GetAllErrorDetails(ex) + "</color>");
                return;
            }

            if (time_check <= 0)
            {
                Utils.Chatter("... <color=#ff4040>FAILED!  Are you firewalled or is the server down?</color>");
                Utils.Chatter();
                Utils.Chatter("<color=#ff4040>This utility requires data from the EKUnleashed.com website to schedule automated game events.</color>");
                return;
            }

            DateTime dtBaseServerTime = DateTime.MinValue;

            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                dtBaseServerTime = Utils.time_val(time_check);

                // game service is set in Form_Load, so it's safe to depend on here
                if (Game.Service == GameClient.GameService.Lies_of_Astaroth) 
                    dtBaseServerTime = dtBaseServerTime.AddHours(-5.0); // three hours ahead of EK, US/EST
                else if (Game.Service == GameClient.GameService.Shikoku_Wars)
                    dtBaseServerTime = dtBaseServerTime.AddHours(7.0); // fifteen hours ahead of EK
                else
                    dtBaseServerTime = dtBaseServerTime.AddHours(-8.0); // EK and MR are the same time zone, US/PST

                GameClient.ServerTime = dtBaseServerTime.AddHours(tz.IsDaylightSavingTime(DateTime.Now) ? 1.0 : 0.0);

                GameClient.ServerTimeChecked = DateTime.Now;
            }
            catch
            {
                Utils.Chatter("... WARNING: unable to determine daylight savings info using standard method (backup method used instead)");

                dtBaseServerTime = Utils.time_val(time_check);

                // game service is set in Form_Load, so it's safe to depend on here
                if (Game.Service == GameClient.GameService.Lies_of_Astaroth) 
                    dtBaseServerTime = dtBaseServerTime.AddHours(-5.0); // three hours ahead of EK, US/EST
                else if (Game.Service == GameClient.GameService.Shikoku_Wars)
                    dtBaseServerTime = dtBaseServerTime.AddHours(7.0); // fifteen hours ahead of EK
                else
                    dtBaseServerTime = dtBaseServerTime.AddHours(-8.0); // EK and MR are the same time zone, US/PST

                bool DaylightSavingsInEffect = bool.Parse(Utils.CStr(Comm.Download("http://www.ekunleashed.com/time_dst.php")));

                GameClient.ServerTime = dtBaseServerTime.AddHours(DaylightSavingsInEffect ? 1.0 : 0.0);

                GameClient.ServerTimeChecked = DateTime.Now;
            }

            TimeSpan server_time_diff = GameClient.ServerTimeChecked - GameClient.DateTimeNow;

            Utils.Chatter("... base server time: " + dtBaseServerTime.ToString());
            Utils.Chatter("... actual server time after daylight savings adjustment: " + GameClient.ServerTime.ToString());
            Utils.Chatter();

            Utils.StartMethodMultithreaded(() =>
            {
                Utils.Chatter("Logging you into the game...");
                Utils.Chatter();

                Game.GameVitalsUpdate();
                Game.MasterLogin();
            });
        }

        private string[] PersonClicked = null;

        private void SharedLinkClickedEvent(object sender, LinkClickedEventArgs e)
        {
            string[] sData = e.LinkText.ToString().Split(new[] { "||EKU||" }, StringSplitOptions.None);

            if (sData[1] == "URL")
            {
                try
                {
                    System.Diagnostics.Process.Start(sData[2].Trim());
                }
                catch { }
            }
            else if (sData[1] == "PM")
            {
                this.PersonClicked = sData;
                this.nameClicked.Show(Cursor.Position);
            }
            else if (sData[1] == "CARD")
            {
                try
                {
                    int evolved_skill = 0;
                    int evolved_times = 0;

                    try { evolved_skill = Utils.CInt(sData[4]); } catch { }
                    try { evolved_times = Utils.CInt(sData[5]); } catch { }

                    this.PopupCardPreviewWindow(Utils.CInt(sData[2]), Utils.CInt(sData[3]), Cursor.Position, "", 0, 0, 0, 0, null, null, null, null, evolved_times, evolved_skill);
                }
                catch (Exception ex)
                {
                    Utils.Chatter(Errors.GetAllErrorDetails(ex));
                }
            }
            else if (sData[1] == "RUNE")
            {
                try
                {
                    this.PopupRunePreviewWindow(Utils.CInt(sData[2]), Utils.CInt(sData[3]), Cursor.Position);
                }
                catch (Exception ex)
                {
                    Utils.Chatter(Errors.GetAllErrorDetails(ex));
                }
            }
        }

        private void nameClicked_Opening(object sender, CancelEventArgs e)
        {
            if (this.PersonClicked == null) { e.Cancel = true; return; }

            this.mnuNameClicked_Name.Text = this.PersonClicked[3].Trim();
        }

        private void mnuNameClicked_PM_Click(object sender, EventArgs e)
        {
            if (this.PersonClicked == null) { return; }

            string uid = this.PersonClicked[2].Trim();
                
            if (!string.IsNullOrEmpty(uid) && Utils.CInt(uid) > 0)
            {
                frmMain.ChatChannel = new string[] { "P", uid };
                this.btnChatChannel.Text = this.PersonClicked[3].Trim();
                this.RefocusInput();
            }
        }

        private void mnuNameClicked_ShowInfo_Click(object sender, EventArgs e)
        {
            if (this.PersonClicked == null) { return; }

            Utils.StartMethodMultithreaded(() =>
            {
                lock (Game.locker_gamedata)
                {
                    Game.CheckLogin();
                    if (Game.opts == null)
                        return;

                    JObject user_info = JObject.Parse(Game.GetGameData(ref Game.opts, "friend", "Search", "NickName=" + this.PersonClicked[3].Trim(), false));

                    foreach (var friend in user_info["data"]["Friends"])
                    {
                        if (friend["Uid"].ToString().ToLower().Trim() == this.PersonClicked[2].ToLower().Trim())
                        {
                            Utils.Chatter("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
                            Utils.Chatter("<color=#00efff><b>Player Information:</b>  " + friend["NickName"].ToString() + "</color>");
                            Utils.Chatter("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
                            Utils.Chatter("<color=#00efff><b>Level:</b>\t\t" + Utils.CInt(friend["Level"].ToString()).ToString("#,##0") + "</color>");
                            Utils.Chatter("<color=#00efff><b>Gender:</b>\t" + ((Utils.CInt(friend["Sex"].ToString()) == 0 ? "Male" : "Female")) + "</color>");
                            double wins = (double)Utils.CInt(friend["Win"].ToString());
                            double losses = (double)Utils.CInt(friend["Lose"].ToString());
                            if (wins + losses > 0)
                                Utils.Chatter("<color=#00efff><b>Arena:</b>\t\t" + wins.ToString("#,##0") + "-" + losses.ToString("#,##0") + " (" + (wins * 100.0 / (wins + losses)).ToString("0") + "%)</color>");
                            else
                                Utils.Chatter("<color=#00efff><b>Arena:</b>\t\t" + wins.ToString("#,##0") + "-" + losses.ToString("#,##0") + "</color>");
                            Utils.Chatter("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
                        }
                    }
                }
            });
        }

        private Region TitleBar
        {
            get { return new Region(new Rectangle(0, 0, Width, 26)); }
        }

        private GraphicsPath CloseButton
        {
            get
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(new Rectangle(Width - 26, 3, 18, 18));
                return gp;
            }
        }

        private GraphicsPath MaxButton
        {
            get
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(new Rectangle(Width - 49, 3, 18, 18));
                return gp;
            }
        }

        private GraphicsPath MinButton
        {
            get
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(new Rectangle(Width - 72, 3, 18, 18));
                return gp;
            }
        }
        
        private GraphicsPath FormShape
        {
            get
            {
                GraphicsPath gp = new GraphicsPath();
                Rectangle r = ClientRectangle;
                int radius = 12;

                gp.AddArc(r.Left, r.Top + 24, radius, radius, 180, 90);
                gp.AddArc(r.Left + 80 - radius, r.Top + 24 - radius, radius, radius, -270, -90);
                gp.AddArc(r.Left + 80, r.Top, radius, radius, 180, 90);
                gp.AddArc(r.Right - radius, r.Top, radius, radius, 270, 90);
                gp.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, 0, 90);
                gp.AddArc(r.Left, r.Bottom - radius, radius, radius, 90, 90);
                gp.CloseFigure();

                return gp;
            }
        }

        private bool ClosePress = false;
        private bool MinPress = false;
        private bool MaxPress = false;
        private bool FormDrag = false;
        private bool ApplicationMenuActive = false;
        private bool GameMenuActive = false;
        private bool BuyMenuActive = false;
        private bool DebugMenuActive = false;
        private bool ApplicationMenuHover = false;
        private bool GameMenuHover = false;
        private bool BuyMenuHover = false;
        private bool DebugMenuHover = false;

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private System.Windows.Forms.Label lblApplicationMenu;
        private System.Windows.Forms.Label lblGameMenu;
        private System.Windows.Forms.Label lblBuyMenu;
        private System.Windows.Forms.Label lblDebugMenu;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblResizeCorner;
        private const int HT_CAPTION = 0x2;

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int WS_CLIPCHILDREN = 0x2000000;
                const int WS_MINIMIZEBOX = 0x20000;
                const int WS_MAXIMIZEBOX = 0x10000;
                const int WS_SYSMENU = 0x80000;

                const int CS_DBLCLKS = 0x8;
                const int CS_DROPSHADOW = 0x20000;

                int ClassFlags = CS_DBLCLKS;
                int OSVER = Environment.OSVersion.Version.Major * 10;
                OSVER += Environment.OSVersion.Version.Minor;
                if (OSVER >= 51) ClassFlags = CS_DBLCLKS | CS_DROPSHADOW;

                cp.Style = WS_CLIPCHILDREN | WS_MINIMIZEBOX | WS_SYSMENU | WS_MAXIMIZEBOX;
                cp.ClassStyle = ClassFlags;

                return cp;
            }
        }

        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            Brush brushLightRed = new SolidBrush(Color.FromArgb(255, 200, 200));
            Brush brushLightBlue = new SolidBrush(Color.FromArgb(200, 235, 255));
            Brush brushLightYellow = new SolidBrush(Color.FromArgb(255, 255, 200));

            Brush brushDarkBlue = new SolidBrush(Color.FromArgb(18, 29, 33));
            e.Graphics.FillRegion(brushDarkBlue, TitleBar);
            e.Graphics.FillRectangle(brushDarkBlue, new Rectangle(0, 26, Width, 19));

            //Brush brushDarkGray = new SolidBrush(Color.FromArgb(100, 100, 100));

            if (ApplicationMenuHover) e.Graphics.FillRectangle(Brushes.White, lblApplicationMenu.Bounds);
            if (GameMenuHover) e.Graphics.FillRectangle(Brushes.White, lblGameMenu.Bounds);
            if (BuyMenuHover) e.Graphics.FillRectangle(Brushes.White, lblBuyMenu.Bounds);
            if (DebugMenuHover) e.Graphics.FillRectangle(Brushes.White, lblDebugMenu.Bounds);

            Pen BorderPen = new Pen(Color.FromArgb(48, 59, 73), 2);
            BorderPen.Alignment = PenAlignment.Inset;
            e.Graphics.DrawPath(BorderPen, FormShape);
            BorderPen.Dispose();

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
            sf.Trimming = StringTrimming.EllipsisCharacter;
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(this._TitleText, Control.DefaultFont, Brushes.White, RectangleF.FromLTRB(84, 0, MinButton.GetBounds().X, 24), sf);

            if (ClosePress)
                e.Graphics.FillPath(Brushes.Red, CloseButton);
            else
                e.Graphics.FillPath(brushLightRed, CloseButton);

            if (MinPress)
                e.Graphics.FillPath(brushLightYellow, MinButton);
            else
                e.Graphics.FillPath(brushLightBlue, MinButton);

            if (MaxPress)
                e.Graphics.FillPath(brushLightYellow, MaxButton);
            else
                e.Graphics.FillPath(brushLightBlue, MaxButton);

            Font GlyphFont = new Font("Marlett", Font.SizeInPoints, FontStyle.Regular, GraphicsUnit.Point);

            RectangleF r = MinButton.GetBounds();
            //r.Y -= 2;
            r.X += 2;
            e.Graphics.DrawString("0", GlyphFont, Brushes.Black, r, sf);

            r = MaxButton.GetBounds();
            r.Y += 1;
            r.X += 2;
            e.Graphics.DrawString((this.WindowState == FormWindowState.Maximized) ? "2" : "1", GlyphFont, Brushes.Black, r, sf);

            r = CloseButton.GetBounds();
            r.Y += 1;
            r.X += 2;
            e.Graphics.DrawString("r", GlyphFont, Brushes.Black, r, sf);
            GlyphFont.Dispose();

            brushDarkBlue.Dispose();
            //brushDarkGray.Dispose();

            brushLightRed.Dispose();
            brushLightBlue.Dispose();
            brushLightYellow.Dispose();
        }

        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            bool OverClose, OverMin, OverMax;
            OverClose = CloseButton.IsVisible(e.X, e.Y);
            OverMin = MinButton.IsVisible(e.X, e.Y);
            OverMax = MaxButton.IsVisible(e.X, e.Y);
            ClosePress = OverClose && e.Button == MouseButtons.Left;
            MinPress = OverMin && e.Button == MouseButtons.Left;
            MaxPress = OverMax && e.Button == MouseButtons.Left;

            if (OverClose && !ClosePress)
            {
                if (toolTip1.GetToolTip(this) != "Close")
                {
                    toolTip1.SetToolTip(this, "Close");
                    Invalidate();
                }
            }
            else if (OverMin && !MinPress)
            {
                if (toolTip1.GetToolTip(this) != "Minimize to Tray")
                {
                    toolTip1.SetToolTip(this, "Minimize to Tray");
                    Invalidate();
                }
            }
            else if (OverMax && !MaxPress)
            {
                if (toolTip1.GetToolTip(this) != "Maximize")
                {
                    toolTip1.SetToolTip(this, "Maximize");
                    Invalidate();
                }
            }
            else
            {
                if (toolTip1.GetToolTip(this) != "")
                {
                    toolTip1.SetToolTip(this, "");
                    Invalidate();
                }
            }
        }

        private void frmMain_MouseDown(object sender, MouseEventArgs e)
        {
            ClosePress = CloseButton.IsVisible(e.X, e.Y) && e.Button == MouseButtons.Left;
            MinPress = MinButton.IsVisible(e.X, e.Y) && e.Button == MouseButtons.Left;
            MaxPress = MaxButton.IsVisible(e.X, e.Y) && e.Button == MouseButtons.Left;
            FormDrag = TitleBar.IsVisible(e.X, e.Y) && e.Button == MouseButtons.Left && !ClosePress && !MinPress && !MaxPress;

            if (FormDrag)
            {
                this.Capture = false;
                Message msg = Message.Create(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
                WndProc(ref msg);
            }

            Invalidate();
        }

        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            bool OverClose, OverMin, OverMax;
            OverClose = CloseButton.IsVisible(e.X, e.Y);
            OverMin = MinButton.IsVisible(e.X, e.Y);
            OverMax = MaxButton.IsVisible(e.X, e.Y);

            if (OverClose && ClosePress && e.Button == MouseButtons.Left)
                this.Close();
            if (OverMin && MinPress && e.Button == MouseButtons.Left)
                this.WindowState = FormWindowState.Minimized;
            if (OverMax && MaxPress && e.Button == MouseButtons.Left)
                this.WindowState = (this.WindowState == FormWindowState.Maximized) ? FormWindowState.Normal : FormWindowState.Maximized;

            if (e.Button == MouseButtons.Right && TitleBar.IsVisible(e.X, e.Y))
            {
                if (OverClose == false && OverMin == false && OverMax == false)
                {
                    const int WM_GETSYSMENU = 0x313;
                    if (e.Button == MouseButtons.Right)
                    {
                        Point pos = this.PointToScreen(new Point(e.X, e.Y));
                        IntPtr hPos = (IntPtr)((int)((pos.Y * 0x10000) | (pos.X & 0xFFFF)));
                        Message msg = Message.Create(this.Handle, WM_GETSYSMENU, IntPtr.Zero, hPos);
                        WndProc(ref msg);
                    }
                }
            }

            ClosePress = false;
            MinPress = false;
            MaxPress = false;
            FormDrag = false;

            Invalidate();
        }

        private bool Sizing = false;
        private Point SizeOffset = Point.Empty;

        private void frmMain_Resize(object sender, EventArgs e)
        {
            this.Region = new Region(FormShape);
        }

        private void lblGameMenu_MouseEnter(object sender, System.EventArgs e)
        {
            GameMenuHover = true;
            lblGameMenu.ForeColor = Color.Black;
            lblGameMenu.Invalidate();
        }

        private void lblGameMenu_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.GameMenuActive)
            {
                this.GameMenuActive = true;
                this.mnuGame.Show(lblGameMenu, new Point(0, lblGameMenu.Height));
                this.lblGameMenu.Invalidate();
            }
            else
            {
                this.mnuGame.Hide();
            }
        }

        private void lblGameMenu_MouseLeave(object sender, System.EventArgs e)
        {
            if (!this.GameMenuActive)
            {
                GameMenuHover = false;
                lblGameMenu.ForeColor = Color.White;
            }
            lblGameMenu.Invalidate();
        }


        private void lblBuyMenu_MouseEnter(object sender, System.EventArgs e)
        {
            BuyMenuHover = true;
            lblBuyMenu.ForeColor = Color.Black;
            lblBuyMenu.Invalidate();
        }

        private void lblBuyMenu_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.BuyMenuActive)
            {
                this.BuyMenuActive = true;
                this.mnuBuy.Show(lblBuyMenu, new Point(0, lblBuyMenu.Height));
                this.lblBuyMenu.Invalidate();
            }
            else
            {
                this.mnuBuy.Hide();
            }
        }

        private void lblBuyMenu_MouseLeave(object sender, System.EventArgs e)
        {
            if (!this.BuyMenuActive)
            {
                BuyMenuHover = false;
                lblBuyMenu.ForeColor = Color.White;
            }
            lblBuyMenu.Invalidate();
        }

        private void lblDebugMenu_MouseEnter(object sender, System.EventArgs e)
        {
            DebugMenuHover = true;
            lblDebugMenu.ForeColor = Color.Black;
            lblDebugMenu.Invalidate();
        }

        private void lblDebugMenu_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.DebugMenuActive)
            {
                this.DebugMenuActive = true;
                this.mnuDebug.Show(lblDebugMenu, new Point(0, lblDebugMenu.Height));
                this.lblDebugMenu.Invalidate();
            }
            else
            {
                this.mnuDebug.Hide();
            }
        }

        private void lblDebugMenu_MouseLeave(object sender, System.EventArgs e)
        {
            if (!this.DebugMenuActive)
            {
                DebugMenuHover = false;
                lblDebugMenu.ForeColor = Color.White;
            }
            lblDebugMenu.Invalidate();
        }

        private void lblApplicationMenu_MouseEnter(object sender, System.EventArgs e)
        {
            ApplicationMenuHover = true;
            lblApplicationMenu.ForeColor = Color.Black;
            lblApplicationMenu.Invalidate();
        }

        private void lblApplicationMenu_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.ApplicationMenuActive)
            {
                this.ApplicationMenuActive = true;
                this.mnuApplication.Show(lblApplicationMenu, new Point(0, lblApplicationMenu.Height));
                this.lblApplicationMenu.Invalidate();
            }
            else
            {
                this.mnuApplication.Hide();
            }
        }

        private void lblApplicationMenu_MouseLeave(object sender, System.EventArgs e)
        {
            if (!this.ApplicationMenuActive)
            {
                ApplicationMenuHover = false;
                lblApplicationMenu.ForeColor = Color.White;
            }
            lblApplicationMenu.Invalidate();
        }

        private void labelResize_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Sizing = true;
            SizeOffset = new Point(this.Right - Cursor.Position.X, this.Bottom - Cursor.Position.Y);
            this.SuspendLayout();
        }

        private void labelResize_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Sizing)
            {
                // Clip cursor to disallow sizing of form below 250x100
                Rectangle ClipRectangle = RectangleToScreen(new Rectangle(250, 100, Width, Height));
                ClipRectangle.Offset(SizeOffset);
                Cursor.Clip = ClipRectangle;
                this.Size = new Size(Cursor.Position.X + SizeOffset.X - Location.X, Cursor.Position.Y + SizeOffset.Y - Location.Y);
            }
        }

        private void labelResize_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Sizing)
            {
                this.ResumeLayout(true);
                Sizing = false;
            }

            Cursor.Clip = Rectangle.Empty;
        }

        private void mnuApplication_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ApplicationMenuHover = false;
            ApplicationMenuActive = false;
            lblApplicationMenu.ForeColor = Color.White;
            lblApplicationMenu.Invalidate();
        }

        private void mnuGame_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            GameMenuHover = false;
            GameMenuActive = false;
            lblGameMenu.ForeColor = Color.White;
            lblGameMenu.Invalidate();
        }

        private void mnuBuy_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            BuyMenuHover = false;
            BuyMenuActive = false;
            lblBuyMenu.ForeColor = Color.White;
            lblBuyMenu.Invalidate();
        }
        private void mnuDebug_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            DebugMenuHover = false;
            DebugMenuActive = false;
            lblDebugMenu.ForeColor = Color.White;
            lblDebugMenu.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private PrivateFontCollection pfc = new PrivateFontCollection();
        public static Font FONT__141CAI978 = null;

        private void frmMain_Load(object sender, EventArgs e)
        {
            // convert settings.ini to registry settings
            if (string.IsNullOrEmpty(Utils.GetAppSetting("Login_Service")))
            {
                if (System.IO.File.Exists(Utils.AppFolder + "\\" + GameClient.settings_file))
                {
                    //Utils.SetAppSetting("License_Email", SettingFromFile.GetData(GameClient.settings_file, "License", "Email"));
                    //Utils.SetAppSetting("License_Code", SettingFromFile.GetData(GameClient.settings_file, "License", "Code"));

                    Utils.SetAppSetting("Login_Service", SettingFromFile.GetData(GameClient.settings_file, "Login", "Service"));
                    Utils.SetAppSetting("Login_Account", SettingFromFile.GetData(GameClient.settings_file, "Login", "Account"));
                    if (string.IsNullOrEmpty(Utils.GetAppSetting("Login_Account")))
                        Utils.SetAppSetting("Login_Account", SettingFromFile.GetData(GameClient.settings_file, "Login", "Email"));
                    Utils.SetAppSetting("Login_Service", SettingFromFile.GetData(GameClient.settings_file, "Login", "Service"));
                    Utils.SetAppSetting("Login_Password", SettingFromFile.GetData(GameClient.settings_file, "Login", "Password"));
                    Utils.SetAppSetting("Login_Device", SettingFromFile.GetData(GameClient.settings_file, "Login", "Device"));
                    Utils.SetAppSetting("Login_Chat", SettingFromFile.True(GameClient.settings_file, "Chat", "Login").ToString());
                    Utils.SetAppSetting("Login_ChatReconnect", SettingFromFile.GetData(GameClient.settings_file, "Chat", "AutoReconnect"));

                    Utils.SetAppSetting("Game_Debug", SettingFromFile.GetData(GameClient.settings_file, "Game", "Debug"));
                    Utils.SetAppSetting("Game_Events", SettingFromFile.GetData(GameClient.settings_file, "Game", "Events"));
                    Utils.SetAppSetting("Game_DefaultDeck", SettingFromFile.GetData(GameClient.settings_file, "Game", "DefaultDeck"));
                    Utils.SetAppSetting("Game_SendFriendEnergy", SettingFromFile.GetData(GameClient.settings_file, "Game", "SendFriendEnergy"));
                    Utils.SetAppSetting("Game_ReceiveFriendEnergy", SettingFromFile.GetData(GameClient.settings_file, "Game", "ReceiveFriendEnergy"));
                    Utils.SetAppSetting("Game_FightMapInvasions", SettingFromFile.GetData(GameClient.settings_file, "Game", "FightMapInvasions"));
                    Utils.SetAppSetting("Game_FightThieves", SettingFromFile.GetData(GameClient.settings_file, "Game", "FightThieves"));
                    Utils.SetAppSetting("Game_FightMazeTowers", SettingFromFile.GetData(GameClient.settings_file, "Game", "FightMazeTowers"));
                    Utils.SetAppSetting("Game_MazeTowers", SettingFromFile.GetData(GameClient.settings_file, "Game", "MazeTowers"));
                    Utils.SetAppSetting("Game_MazeTowerMonsters", SettingFromFile.GetData(GameClient.settings_file, "Game", "MazeTowerMonsters"));
                    Utils.SetAppSetting("Game_MazeTowerChests", SettingFromFile.GetData(GameClient.settings_file, "Game", "MazeTowerChests"));
                    Utils.SetAppSetting("Game_Explore", SettingFromFile.GetData(GameClient.settings_file, "Game", "ExploreAfterMazes"));
                    Utils.SetAppSetting("Game_FightArena", SettingFromFile.GetData(GameClient.settings_file, "Game", "FightArena"));
                    Utils.SetAppSetting("Game_FightDemonInvasions", SettingFromFile.GetData(GameClient.settings_file, "Game", "FightDemonInvasions"));
                    Utils.SetAppSetting("Game_FightKW", SettingFromFile.GetData(GameClient.settings_file, "Game", "KWFight"));
                    Utils.SetAppSetting("Game_FightWorldTree", SettingFromFile.GetData(GameClient.settings_file, "Game", "WorldTree"));
                    Utils.SetAppSetting("Game_FightHydra", SettingFromFile.GetData(GameClient.settings_file, "Game", "Hydra"));
                    Utils.SetAppSetting("Game_ClanMemberReport", SettingFromFile.GetData(GameClient.settings_file, "Game", "ClanMemberReport"));

                    Utils.SetAppSetting("Enchant_Cards_WithStars", SettingFromFile.GetData(GameClient.settings_file, "Game", "EnchantWithStars"));
                    Utils.SetAppSetting("Enchant_Cards_Excluded", SettingFromFile.GetData(GameClient.settings_file, "Game", "ExcludeFromEnchant"));

                    Utils.SetAppSetting("Dev_ImageScalingSize", SettingFromFile.GetData(GameClient.settings_file, "Game", "ImageScalingSize"));
                    Utils.SetAppSetting("Dev_WantImageGeneration", SettingFromFile.GetData(GameClient.settings_file, "Game", "WantImageGeneration"));
                    Utils.SetAppSetting("Dev_ImageSaveFormat", SettingFromFile.GetData(GameClient.settings_file, "Game", "ImageSaveFormat"));
                    Utils.SetAppSetting("Dev_ImageSaveQuality", SettingFromFile.GetData(GameClient.settings_file, "Game", "ImageSaveQuality"));

                    Utils.SetAppSetting("Arena_DontAttack", SettingFromFile.GetData(GameClient.settings_file, "Arena", "DontAttack"));

                    Utils.SetAppSetting("Thief_Deck", SettingFromFile.GetData(GameClient.settings_file, "Thief", "Deck"));
                    Utils.SetAppSetting("Thief_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Thief", "DeckCards"));
                    Utils.SetAppSetting("Thief_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Thief", "DeckRunes"));
                    Utils.SetAppSetting("Thief_AlwaysFill", SettingFromFile.GetData(GameClient.settings_file, "Thief", "AlwaysFill"));
                    Utils.SetAppSetting("Thief_IgnoreFrom", SettingFromFile.GetData(GameClient.settings_file, "Thief", "IgnoreFrom"));

                    Utils.SetAppSetting("Hydra_Deck", SettingFromFile.GetData(GameClient.settings_file, "Hydra", "Deck"));
                    Utils.SetAppSetting("Hydra_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Hydra", "DeckCards"));
                    Utils.SetAppSetting("Hydra_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Hydra", "DeckRunes"));
                    Utils.SetAppSetting("Hydra_AlwaysFill", SettingFromFile.GetData(GameClient.settings_file, "Hydra", "AlwaysFill"));
                    Utils.SetAppSetting("Hydra_Frequency", SettingFromFile.GetData(GameClient.settings_file, "Hydra", "Frequency"));

                    //Utils.SetAppSetting("DemonInvasion_DeckSwap", SettingFromFile.GetData(GameClient.settings_file, "Demon Invasion", "DeckSwap"));
                    Utils.SetAppSetting("DemonInvasion_Deck", SettingFromFile.GetData(GameClient.settings_file, "Demon Invasion", "Deck"));
                    Utils.SetAppSetting("DemonInvasion_Deucalion_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Deucalion", "DeckCards"));
                    Utils.SetAppSetting("DemonInvasion_Deucalion_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Deucalion", "DeckRunes"));
                    Utils.SetAppSetting("DemonInvasion_Mars_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Mars", "DeckCards"));
                    Utils.SetAppSetting("DemonInvasion_Mars_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Mars", "DeckRunes"));
                    Utils.SetAppSetting("DemonInvasion_PlagueOgryn_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Plague Ogryn", "DeckCards"));
                    Utils.SetAppSetting("DemonInvasion_PlagueOgryn_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Plague Ogryn", "DeckRunes"));
                    Utils.SetAppSetting("DemonInvasion_DarkTitan_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Dark Titan", "DeckCards"));
                    Utils.SetAppSetting("DemonInvasion_DarkTitan_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Dark Titan", "DeckRunes"));
                    Utils.SetAppSetting("DemonInvasion_SeaKing_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Sea King", "DeckCards"));
                    Utils.SetAppSetting("DemonInvasion_SeaKing_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Sea King", "DeckRunes"));
                    Utils.SetAppSetting("DemonInvasion_Pandarus_DeckCards", SettingFromFile.GetData(GameClient.settings_file, "Pandarus", "DeckCards"));
                    Utils.SetAppSetting("DemonInvasion_Pandarus_DeckRunes", SettingFromFile.GetData(GameClient.settings_file, "Pandarus", "DeckRunes"));
                }
            }

            // settings conversion
            if (Utils.GetAppSetting("Hydra_SkipFighting").Trim().Length > 0)
            {
                if (Utils.False("Hydra_SkipFighting"))
                    Utils.SetAppSetting("Hydra_AutomationMode", "claim only");
                Utils.SetAppSetting("Hydra_SkipFighting", null);
            }

            // first time load (not application restart)
            if (FONT__141CAI978 == null)
            {
                #region Load up the custom font

                System.IO.Stream fontStream = this.GetType().Assembly.GetManifestResourceStream("EKUnleashed.Resources.common.141-CAI978.ttf");

                byte[] fontdata = new byte[fontStream.Length];
                fontStream.Read(fontdata, 0, (int)fontStream.Length);
                fontStream.Close();
                unsafe
                {
                    fixed (byte* pFontData = fontdata)
                    {
                        pfc.AddMemoryFont((System.IntPtr)pFontData, fontdata.Length);
                    }
                }

                foreach (FontFamily ff in pfc.Families)
                {
                    FONT__141CAI978 = new Font(ff, 28f, FontStyle.Regular);
                    break;
                }

                #endregion

                this.MinimumSize = this.Size;
                this.Region = new Region(FormShape);
            }

            string game_service = Utils.GetAppSetting("Login_Service").Trim().ToUpper();
            switch (game_service)
            {
                case "LOA":
                    Game.Service = GameClient.GameService.Lies_of_Astaroth;
                    break;

                case "MR":
                    Game.Service = GameClient.GameService.Magic_Realms;
                    break;

                case "ER":
                    Game.Service = GameClient.GameService.Elves_Realm;
                    break;

                case "SW":
                    Game.Service = GameClient.GameService.Shikoku_Wars;
                    break;

                default:
                    Game.Service = GameClient.GameService.Elemental_Kingdoms;
                    break;
            }

            Game.Login_Device = Utils.GetAppSetting("Login_Device").ToUpper().Trim();
            if (Game.Login_Device.StartsWith("IOS")) Game.Login_Device = "IPHONE";
            if (Game.Login_Device == "IPAD") Game.Login_Device = "IPHONE";
            if (Game.Login_Device.StartsWith("APPLE")) Game.Login_Device = "IPHONE";
            if (Game.Login_Device.StartsWith("ITUNE")) Game.Login_Device = "IPHONE";                    
            if (Game.Login_Device != "IPHONE") Game.Login_Device = "ANDROID";

            if (Game.Service == GameClient.GameService.Lies_of_Astaroth || Game.Service == GameClient.GameService.Elves_Realm)
                this.fireTokensPackToolStripMenuItem.Text = "Magic coupons pack...";
            else
                this.fireTokensPackToolStripMenuItem.Text = "Fire tokens pack...";

            //Game.Want_Deck_Swap = Utils.False("DemonInvasion_DeckSwap");

            this.prgXPToNext.Visible = false;

            try
            {
                Game.Kingdom_War_ID = "0";
                if (this.tabsChatChannels.TabPages.Contains(this.tabKWTundra)) this.tabsChatChannels.TabPages.Remove(this.tabKWTundra);
                if (this.tabsChatChannels.TabPages.Contains(this.tabKWForest)) this.tabsChatChannels.TabPages.Remove(this.tabKWForest);
                if (this.tabsChatChannels.TabPages.Contains(this.tabKWSwamp)) this.tabsChatChannels.TabPages.Remove(this.tabKWSwamp);
                if (this.tabsChatChannels.TabPages.Contains(this.tabKWMountain)) this.tabsChatChannels.TabPages.Remove(this.tabKWMountain);

                if (!GameClient.Want_Debug)
                {
                    if (this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                        this.tabsChatChannels.TabPages.Remove(this.tabDebug);
                }
                else
                    if (!this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                        this.tabsChatChannels.TabPages.Add(this.tabDebug);

                if (this.tabsChatChannels.TabPages.Contains(this.tabOtherClans))
                    this.tabsChatChannels.TabPages.Remove(this.tabOtherClans);

                if (this.tabsGameInfo.TabPages.Contains(this.tabInfoDemonInvasion))
                    this.tabsGameInfo.TabPages.Remove(this.tabInfoDemonInvasion);
            }
            catch { }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.FileVersionInfo fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            string sDateText = string.Empty;
            try
            {
                double dYear = 2000.0 + (double)((int)(fv.FileBuildPart % 100));
                double dDays = (double)((int)((fv.FileBuildPart / 100) - 1));
                double dMins = (double)fv.FilePrivatePart;
                DateTime dtDate = DateTime.Parse(dYear.ToString() + "-01-01 00:00:00.000");
                dtDate = dtDate.AddDays(dDays);
                dtDate = dtDate.AddMinutes(dMins);

                sDateText =
                    "\r\n" +
                    "Released: " + dtDate.ToString() + " GMT-7\r\n";
            }
            catch { }

            MessageBox.Show
            (
                "Version " + fv.FileMajorPart + "." + Utils.CInt(fv.FileMinorPart).ToString("00") + ", build " + fv.FileBuildPart + "." + fv.FilePrivatePart + "\r\n" +
                sDateText + 
                "\r\n" +
                "Copyright © " + ((DateTime.Now.Year <= 2014) ? "2014" : "2014-" + DateTime.Now.Year.ToString()) + " Souza Software, Inc.\r\n" +
                "All Rights Reserved.\r\n" +
                "\r\n" +
                "www.EKUnleashed.com",
                "About " + Application.ProductName,
                MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }

        private void quickbuildADeckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Game.Want_Game_Login)
            {
                Utils.StartMethodMultithreaded(() =>
                {
                    string temp_deck_ordinal = Utils.Input_Text("Update Deck", "Enter deck ID to set these cards & runes to (\"KW\" or 1-10):").Trim().ToUpper();

                    if (((Utils.CInt(temp_deck_ordinal) > 0) && (Utils.CInt(temp_deck_ordinal) <= 10)) || temp_deck_ordinal.Trim().ToUpper() == "KW")
                    {
                        string temp_cards_entered = Utils.Input_Text("Update Deck", "Enter a list of cards you want to use (separated by commas):").Trim();

                        if (!string.IsNullOrEmpty(temp_cards_entered))
                        {
                            string temp_runes_entered = Utils.Input_Text("Update Deck", "Enter a list of runes you want to use (separated by commas)").Trim();

                            Game.FillDeckCustom(temp_deck_ordinal, temp_cards_entered, temp_runes_entered);
                        }
                    }

                });
            }
        }

        private void sendEnergyToAllFriendsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_SendFriendListEnergy);
        }

        private void receiveEnergyFromAllFriendsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_ReceiveFriendListEnergy);
        }

        private void sendReceiveEnergyTofromAllFriendsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() => { Game.Play_SendFriendListEnergy(); Game.Play_ReceiveFriendListEnergy(); });
        }

        private void fightMazeTowersonlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() => { Game.Play_SpendEnergy(1); });
        }

        private void exploreBestExperiencerewardingMapStageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_Explore);
        }

        private void fightMazeTowersAndThenExploreWithRemainingEnergyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() => { Game.Play_SpendEnergy(2); });
        }

        private void fightAnAutomaticallyselectedCompetitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_ArenaFight);
        }

        private void generateAClanMemberReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.ClanMemberReport);
        }

        private void sendAnEmailToTheClanclanLeadersOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                lock (Game.locker_gamedata)
                {
                    Game.CheckLogin();
                    if (Game.opts == null)
                        return;

                    string clan_message_title = Utils.Input_Text("Send Clan E-Mail", "Enter the clan e-mail subject:");

                    if (!string.IsNullOrEmpty(clan_message_title))
                    {
                        string clan_message_body = Utils.Input_Text("Send Clan E-Mail", "Enter the clan e-mail content:");

                        if (!string.IsNullOrEmpty(clan_message_body))
                            Game.GetGameData(ref Game.opts, "legion", "SendMail", "Title=" + System.Web.HttpUtility.UrlEncode(clan_message_title.Trim()) + "&Content=" + System.Web.HttpUtility.UrlEncode(clan_message_body.Trim()), false);
                    }
                }
            });

        }

        private void meditateInTheTempleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                string meditate_gold_threshold = Utils.Input_Text("Temple Meditation", "How much gold do you want to spend meditating in the temple?", "0");
                if (!string.IsNullOrEmpty(meditate_gold_threshold) && Utils.CInt(meditate_gold_threshold) > 0)
                    Game.Play_TempleMeditation(Utils.CInt(meditate_gold_threshold));
            });
        }

        private void fightAnAutomaticallyselectedThiefToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_FightThieves);
        }

        private void fightTheCurrentDemonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.DoingDemonInvasion = !Game.DoingDemonInvasion;
        }

        private void searchForACardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string card_name = Utils.Input_Text("Card Search", "Search for a card name:");
            if (!string.IsNullOrEmpty(card_name))
            {
                this.tabsChatChannels.SelectedTab = this.tabNotifications;
                Utils.StartMethodMultithreaded(() => { Game.CardSearch(card_name); });
            }
        }

        private void searchForARuneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string rune_name = Utils.Input_Text("Rune Search", "Search for a rune name:");
            if (!string.IsNullOrEmpty(rune_name))
            {
                this.tabsChatChannels.SelectedTab = this.tabNotifications;
                Utils.StartMethodMultithreaded(() => { Game.RuneSearch(rune_name); });
            }
        }

        private void logInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                if (Game.Chat != null)
                    Game.Chat.Logout();

                Game.MasterLogin();
            });
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Game.StopAllEvents();
                Game.ScheduledEvents.Clear();
            }
            catch { }

            try
            {
                if (Game.Chat != null)
                    Game.Chat.Logout();
            }
            catch { }

            Utils.Chatter();
            Utils.Chatter("<color=#40c0ff>Logged out from the game!</color>");
        }

        private void mnuTools_Opening(object sender, CancelEventArgs e)
        {
            this.fightTheCurrentDemonToolStripMenuItem.Checked = Game.DoingDemonInvasion;

            try
            {
                if (Game.Chat != null)
                {
                    if (Game.ChatIsConnected)
                    {
                        this.logInToolStripMenuItem.Text = "Disconnect and log back in";
                        this.logOutToolStripMenuItem.Visible = true;
                        this.stopAllAutomaticEventsButRemainLoggedInToolStripMenuItem.Visible = Game.ScheduledEvents.Count > 5;
                        if (Game.ScheduledEvents.Count > 5)
                            this.logOutToolStripMenuItem.Text = "Log out and stop all events";
                        else
                            this.logOutToolStripMenuItem.Text = "Log out";
                    }
                    else
                    {
                        this.logInToolStripMenuItem.Text = "Log back in (currently disconnected)";
                        this.stopAllAutomaticEventsButRemainLoggedInToolStripMenuItem.Visible = false;
                        if (Game.ScheduledEvents.Count > 5)
                        {
                            this.logOutToolStripMenuItem.Visible = true;
                            this.logOutToolStripMenuItem.Text = "Stop all events";
                        }
                        else
                            this.logOutToolStripMenuItem.Visible = false;
                    }
                }
                else
                {
                    this.logInToolStripMenuItem.Text = "Log in";
                    this.stopAllAutomaticEventsButRemainLoggedInToolStripMenuItem.Visible = false;
                    if (Game.ScheduledEvents.Count > 5)
                    {
                        this.logOutToolStripMenuItem.Visible = true;
                        this.logOutToolStripMenuItem.Text = "Stop all events";
                    }
                    else
                        this.logOutToolStripMenuItem.Visible = false;
                }
            }
            catch { }
        }
        
        private void stopAllAutomaticEventsButRemainLoggedInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Game.StopAllEvents();
                Game.ScheduledEvents.Clear();
            }
            catch { }

            Utils.Chatter();
            Utils.Chatter("<color=#40c0ff>Automatic events disabled!</color>");
        }

        delegate void DELEGATE__VOID();


        public void DemonInvasion_Start()
        {
            if (this.InvokeRequired)
            {
                DELEGATE__VOID de = DemonInvasion_Start;
                this.Invoke(de);
                return;
            }

            this.lblDICooldown.Text = "";
            this.lblDICooldownLabel.Text = "";
            this.lblDICooldownLabel.ForeColor = Color.Black;
            lock (this.locker)
                this.lblDIHP.Text = "";
            this.lblDIMerit.Text = "";
            this.lblDIName.Text = "";
            this.lblDIPrize.Text = "";
            this.lblDIRank.Text = "";
            this.lblDIPlaceName1.Text = "";
            this.lblDIPlaceMerit1.Text = "";
            this.lblDIPlaceName2.Text = "";
            this.lblDIPlaceMerit2.Text = "";
            this.lblDIPlaceName3.Text = "";
            this.lblDIPlaceMerit3.Text = "";
            this.lblDIPlaceName4.Text = "";
            this.lblDIPlaceMerit4.Text = "";
            this.lblDIPlaceName5.Text = "";
            this.lblDIPlaceMerit5.Text = "";
            this.lblDIPlaceName6.Text = "";
            this.lblDIPlaceMerit6.Text = "";
            this.lblDIPlaceName7.Text = "";
            this.lblDIPlaceMerit7.Text = "";
            this.lblDIPlaceName8.Text = "";
            this.lblDIPlaceMerit8.Text = "";
            this.lblDIPlaceName9.Text = "";
            this.lblDIPlaceMerit9.Text = "";
            this.lblDIPlaceName10.Text = "";
            this.lblDIPlaceMerit10.Text = "";

            this.btnDIEndCooldown.Visible = true;

            if (!this.tabsGameInfo.TabPages.Contains(this.tabInfoDemonInvasion))
                this.tabsGameInfo.TabPages.Add(this.tabInfoDemonInvasion);
            this.tabsGameInfo.SelectedTab = this.tabInfoDemonInvasion;
        }

        public void DemonInvasion_End()
        {
            Utils.StartMethodMultithreaded(() =>
            {
                lock (Game.locker_gamedata)
                {
                    if (Utils.CInt(Game.DefaultDeck) > 0)
                    {
                        Game.GetGameData(ref Game.opts, "card", "SetDefalutGroup", "GroupId=" + Game.DefaultDeck, false); // switch to primary deck | default deck
                        Utils.LoggerNotifications("<color=#a07000>Switched to default deck</color>");
                    }
                }
            });

            if (this.InvokeRequired)
            {
                DELEGATE__VOID de = DemonInvasion_End;
                this.Invoke(de);
                return;
            }

            try
            {
                this.tabsGameInfo.SelectedTab = this.tabInfoMain;
                this.btnDIEndCooldown.Visible = false;
            }
            catch { }

            Scheduler.ScheduledEvent.AllowAllEvents();
        }

        delegate void VOID__JOBJECT_JOBJECT(JObject j1, JObject j2);

        public void DemonInvasion_UpdateData(JObject DI_boss, JObject DI_rank)
        {
            if (this.InvokeRequired)
            {
                VOID__JOBJECT_JOBJECT de = DemonInvasion_UpdateData;
                this.Invoke(de, new object[] { DI_boss, DI_rank });
                return;
            }

            lock (this.locker)
            {
                try
                {
                    int current_cooldown = Utils.CInt(DI_boss["data"]["CanFightTime"].ToString());
                    int boss_health = Utils.CInt(DI_boss["data"]["Boss"]["BossCurrentHp"].ToString());

                    if (boss_health <= 0)
                    {
                        this.lblDICooldownLabel.Text = "";
                        this.lblDICooldownLabel.ForeColor = Color.Black;
                    }
                    else if (current_cooldown > 0)
                    {
                        int minutes = current_cooldown / 60;
                        int seconds = current_cooldown % 60;

                        this.lblDICooldown.Text = minutes.ToString() + ":" + seconds.ToString("00");
                        this.lblDICooldownLabel.Text = "Cooldown:";
                        this.lblDICooldownLabel.ForeColor = Color.Black;
                    }
                    else
                    {
                        this.lblDICooldown.Text = "";
                        this.lblDICooldownLabel.Text = "No cooldown!";
                        this.lblDICooldownLabel.ForeColor = Color.Red;
                    }

                    lock (this.locker)
                        this.lblDIHP.Text = boss_health.ToString("#,##0");
                    this.lblDIMerit.Text = Utils.CInt(DI_boss["data"]["MyHonor"].ToString()).ToString("#,##0");
                    this.lblDIRank.Text = Utils.CInt(DI_rank["data"]["Rank"].ToString()).ToString("#,##0");

                    this.lblDIPlaceName1.Text = DI_rank["data"]["RankUsers"][0]["NickName"].ToString();
                    this.lblDIPlaceMerit1.Text = Utils.CInt(DI_rank["data"]["RankUsers"][0]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName2.Text = DI_rank["data"]["RankUsers"][1]["NickName"].ToString();
                    this.lblDIPlaceMerit2.Text = Utils.CInt(DI_rank["data"]["RankUsers"][1]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName3.Text = DI_rank["data"]["RankUsers"][2]["NickName"].ToString();
                    this.lblDIPlaceMerit3.Text = Utils.CInt(DI_rank["data"]["RankUsers"][2]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName4.Text = DI_rank["data"]["RankUsers"][3]["NickName"].ToString();
                    this.lblDIPlaceMerit4.Text = Utils.CInt(DI_rank["data"]["RankUsers"][3]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName5.Text = DI_rank["data"]["RankUsers"][4]["NickName"].ToString();
                    this.lblDIPlaceMerit5.Text = Utils.CInt(DI_rank["data"]["RankUsers"][4]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName6.Text = DI_rank["data"]["RankUsers"][5]["NickName"].ToString();
                    this.lblDIPlaceMerit6.Text = Utils.CInt(DI_rank["data"]["RankUsers"][5]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName7.Text = DI_rank["data"]["RankUsers"][6]["NickName"].ToString();
                    this.lblDIPlaceMerit7.Text = Utils.CInt(DI_rank["data"]["RankUsers"][6]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName8.Text = DI_rank["data"]["RankUsers"][7]["NickName"].ToString();
                    this.lblDIPlaceMerit8.Text = Utils.CInt(DI_rank["data"]["RankUsers"][7]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName9.Text = DI_rank["data"]["RankUsers"][8]["NickName"].ToString();
                    this.lblDIPlaceMerit9.Text = Utils.CInt(DI_rank["data"]["RankUsers"][8]["Honor"].ToString()).ToString("#,##0");
                    this.lblDIPlaceName10.Text = DI_rank["data"]["RankUsers"][9]["NickName"].ToString();
                    this.lblDIPlaceMerit10.Text = Utils.CInt(DI_rank["data"]["RankUsers"][9]["Honor"].ToString()).ToString("#,##0");
                }
                catch { }
            }
        }

        delegate void VOID__VOID_JOBJECT_JOBJECT_JOBJECT(JObject j1, JObject j2, JObject j3);

        public void GameVitalsUpdateUI(JObject user_info_data, JObject arena_data, JObject thief_data)
        {
            if (this.InvokeRequired)
            {
                VOID__VOID_JOBJECT_JOBJECT_JOBJECT de = GameVitalsUpdateUI;
                this.Invoke(de, new object[] { user_info_data, arena_data, thief_data });
                return;
            }

            try
            {
                if (user_info_data != null)
                {
                    GameObjs.Hero hero = new GameObjs.Hero(user_info_data);

                    this.lblLogin.Text = Utils.GetAppSetting("Login_Account").Trim();

                    this.lblIndicators.Text = "";
                    this.lblIndicators.Text += "[" + GameClient.GameAbbreviation(Game.Service).Replace("SW", "四国战记") + "] ";
                    if (Utils.True("Login_Chat")) this.lblIndicators.Text += "[Chat] ";
                    if (Utils.True("Game_Events")) this.lblIndicators.Text += "[Events] ";
                    if (Utils.False("Chat_AutoReconnect")) this.lblIndicators.Text += "[Reconnect] ";
                    if (GameClient.Want_Debug) this.lblIndicators.Text += "[Debug] ";
                    
                    this.trayIcon.Text =
                        (
                            "EK Unleashed\r\n" +
                            (Utils.ValidText(hero.Name) ? hero.Name + "\r\n" : "") +
                            (Utils.ValidText(Game.ServerName) ? Game.ServerName + "\r\n" : "") +
                            Game.Service.ToString().Replace("_", " ") + "\r\n" + 
                        "").Trim();

                    this.lblNickName.Text = hero.Name;
                    this.lblLastUpdated.Text = DateTime.Now.ToString();
                    this.lblLevel.Text = hero.Level.ToString();

                    this.prgXPToNext.Value = (int)hero.ProgressTowardLevel;
                    if (!this.prgXPToNext.Visible)
                        this.prgXPToNext.Visible = true;

                    // Causes the progress bar to be yellow, but stops the animation effect
                    Utils.SendMessage
                    (
                        this.prgXPToNext.Handle,
                        0x400 + 16, // WM_USER + PBM_SETSTATE
                        0x0003, // PBST_PAUSED
                        IntPtr.Zero
                    );

                    Game.User_Gems = hero.Gems;
                    Game.User_Level = hero.Level;
                    lock (this.locker)
                    {
                        this.lblGems.Text = Game.User_Gems.ToString("#,##0");
                    }
                    this.lblGold.Text = hero.Gold.ToString("#,##0");
                    this.lblFireTokens.Text = hero.FireTokens.ToString("#,##0");
                    this.lblEnergy.Text = hero.Energy.ToString() + " / " + user_info_data["data"]["EnergyMax"].ToString();
                    this.lblArena.Text = (hero.ArenaFightsLeft == 1) ? "1 fight remaining" : hero.ArenaFightsLeft.ToString() + " fights remaining";

                    this.lblDIAvailable.Text = "";
                    this.lblFriendRequest.Text = "";
                    this.lblEmail.Text = "";

                    if ((Utils.CInt(user_info_data["data"]["Boss"].ToString()) != 0) || (Game.DoingDemonInvasion))
                    {
                        this.lblEmail.Text = "A demon invasion is in progress.";
                        if (!Game.DoingDemonInvasion)
                            if (Utils.False("Game_FightDemonInvasions"))
                                Game.DoingDemonInvasion = true;
                    }
                    if (Utils.CInt(user_info_data["data"]["NewEmail"].ToString()) > 0)
                        this.lblEmail.Text = "You have " + Utils.CInt(user_info_data["data"]["NewEmail"].ToString()).ToString() + " new messages.";
                    if (Utils.CInt(user_info_data["data"]["FriendApplyNum"].ToString()) > 0)
                        this.lblFriendRequest.Text = "You have " + Utils.CInt(user_info_data["data"]["FriendApplyNum"].ToString()).ToString() + " new friend applicants.";
                }

                if (arena_data != null)
                {
                    this.lblArenaRank.Text = "rank " + Utils.CInt(arena_data["data"]["Rank"].ToString()).ToString("#,##0");
                }

                if (thief_data != null)
                {
                    this.lblThiefAvailable.Text = "";

                    int thieves_legendary = 0;
                    int thieves_regular = 0;

                    foreach (var thief in thief_data["data"]["Thieves"])
                    {
                        int tuid = Utils.CInt(thief["UserThievesId"].ToString());
                        int level = Utils.CInt(thief["ThievesId"].ToString());
                        int hp = Utils.CInt(thief["HPCurrent"].ToString());
                        int hpmax = Utils.CInt(thief["HPCount"].ToString());
                        bool legendary = Utils.CInt(thief["Type"].ToString()) == 2;
                        string discovered_by = thief["NickName"].ToString().Trim();
                        int flee_seconds_remaining = Utils.CInt(thief["FleeTime"].ToString());

                        if (flee_seconds_remaining > 0)
                        {
                            if (legendary)
                                thieves_legendary++;
                            else
                                thieves_regular++;
                        }
                    }

                    if (thieves_regular + thieves_legendary > 0)
                    {
                        if (thieves_legendary == 0)
                            this.lblThiefAvailable.Text = thieves_regular.ToString() + " " + Utils.PluralWord(thieves_regular, "thief", "thieves");
                        else if (thieves_regular == 0)
                            this.lblThiefAvailable.Text = thieves_legendary.ToString() + " legendary " + Utils.PluralWord(thieves_legendary, "thief", "thieves");
                        else
                            this.lblThiefAvailable.Text = thieves_regular.ToString() + " " + Utils.PluralWord(thieves_regular, "thief", "thieves") + " and " + thieves_legendary.ToString() + " legendary " + Utils.PluralWord(thieves_legendary, "thief", "thieves");
                    }
                }
            }
            catch { }

            return;
        }

        public void btnDIEndCooldown_Click(object sender, EventArgs e)
        {
            this.btnDIEndCooldown.Enabled = false;

            Utils.StartMethodMultithreaded(() =>
            {
                Game.GetGameData("boss", "BuyTime", false);

                lock (Game.locker)
                    Game.DI_BoughtCooldown = true;

                Utils.StartMethodMultithreaded(() =>
                {
                    JObject game_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));

                    lock (Game.locker)
                        Game.User_Gems = Utils.CInt(game_data["data"]["Cash"].ToString());

                    lock (this.locker)
                    {
                        this.lblGems.Text = Utils.CInt(game_data["data"]["Cash"].ToString()).ToString("#,##0");
                    }
                });
            });
        }

        delegate void VOID__BOOL_BOOL_STRING(bool b1, bool b2, string s);

        public void UpdateDIButton(bool visible = false, bool enabled = false, string text = "")
        {
            if (this.btnDIEndCooldown.InvokeRequired)
            {
                VOID__BOOL_BOOL_STRING de = UpdateDIButton;
                this.btnDIEndCooldown.Invoke(de, new object[] { visible, enabled, text });
                return;
            }

            this.btnDIEndCooldown.Visible = visible;
            this.btnDIEndCooldown.Enabled = enabled;
            this.btnDIEndCooldown.Text = text;
            return;
        }

        delegate void VOID__STRING_STRING_STRING_STRING(string s1, string s2, string s3, string s4);

        public void UpdateDIVitals(string cooldown = null, string HP = null, string name = null, string prize = null)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING_STRING_STRING_STRING de = UpdateDIVitals;
                this.Invoke(de, new object[] { cooldown, HP, name, prize });
                return;
            }

            if (cooldown != null)
            {
                this.lblDICooldown.Text = cooldown;

                if (cooldown == "")
                {
                    string temp_HP = HP;

                    if (temp_HP == null)
                    {
                        lock (this.locker)
                            temp_HP = Utils.CInt(this.lblDIHP.Text.Trim().Replace(",", "")).ToString();
                    }

                    if (temp_HP == "0")
                    {
                        this.lblDICooldownLabel.Text = "";
                    }
                    else
                    {
                        this.lblDICooldownLabel.Text = "No cooldown!";
                        this.lblDICooldownLabel.ForeColor = Color.Red;
                    }
                }
                else
                {
                    this.lblDICooldownLabel.Text = "Cooldown:";
                    this.lblDICooldownLabel.ForeColor = Color.Black;
                }
            }
            lock (this.locker)
            {
                if (HP != null)
                {
                    int current_hp = Utils.CInt(this.lblDIHP.Text.Replace(",", ""));
                    if (current_hp != 0 && current_hp > Utils.CInt(HP.Replace(",", "")))
                        this.lblDIHP.Text = HP;
                }
            }
            if (name != null) this.lblDIName.Text = name;
            if (prize != null) this.lblDIPrize.Text = prize;
            return;
        }

        private void showAllDecksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.StartMethodMultithreaded(() => { Game.DeckReport(); });
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.StartMethodMultithreaded(() => { Game.HelpText(); });
        }

        private void newCardsReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.StartMethodMultithreaded(() => { Game.NewestCardReport(); });
        }

        public void PopupCardPreviewWindow(int card_id, int card_level, Point popup_location, string card_name = "", int element = 0, int stars = 0, int cost = 0, int wait = 0, List<JObject> skills = null, List<int> attack_progression = null, List<int> hp_progression = null, Image iBaseImage = null, int evolved_times = 0, int skill_evolved = 0)
        {
            if (element == 0)
            {
                try
                {
                    JObject card = Game.GetCardByID(card_id);
                    if (card == null)
                        return;

                    card_name = card["CardName"].ToString();
                    element = Utils.CInt(card["Race"]);
                    stars = Utils.CInt(card["Color"]);
                    cost = Utils.CInt(card["Cost"]);
                    wait = Utils.CInt(card["Wait"]);
                    skills = new List<JObject>();
                    try
                    {
                        JObject skill = Game.GetSkillByID(Utils.CInt(card["Skill"]));
                        if (skill == null)
                            skill = JObject.Parse("{ \"empty\": \"1\" }");
                        skills.Add(skill);
                    }
                    catch
                    {
                        skills.Add(JObject.Parse("{ \"empty\": \"1\" }"));
                    }
                    try
                    {
                        JObject skill = Game.GetSkillByID(Utils.CInt(card["LockSkill1"]));
                        if (skill == null)
                            skill = JObject.Parse("{ \"empty\": \"1\" }");
                        skills.Add(skill);
                    }
                    catch
                    {
                        skills.Add(JObject.Parse("{ \"empty\": \"1\" }"));
                    }
                    try
                    {
                        string[] sub_skills = Utils.SubStringsDups(card["LockSkill2"].ToString(), "_");

                        foreach (string sub_skill in sub_skills)
                        {
                            JObject skill = Game.GetSkillByID(Utils.CInt(sub_skill));
                            if (skill == null)
                                skill = JObject.Parse("{ \"empty\": \"1\" }");
                            skills.Add(skill);
                        }
                    }
                    catch
                    {
                        skills.Add(JObject.Parse("{ \"empty\": \"1\" }"));
                    }
                    attack_progression = new List<int>();
                    foreach (var attack in card["AttackArray"])
                        attack_progression.Add(Utils.CInt(attack.ToString()));
                    hp_progression = new List<int>();
                    foreach (var attack in card["HpArray"])
                        hp_progression.Add(Utils.CInt(attack.ToString()));
                }
                catch (Exception ex)
                {
                    Utils.Chatter(Errors.GetAllErrorDetails(ex));
                    return;
                }
            }

            if (skill_evolved != 0)
            {
                try
                {
                    JObject skill = Game.GetSkillByID(skill_evolved);
                    if (skill != null)
                        skills.Add(skill);
                }
                catch { }
            }

            Image iDefaultBackground = GameResourceManager.LoadResource_CardBlank();
            if (iDefaultBackground == null)
            {
                Utils.DebugLogger("Card image is empty!");
                return;
            }
            iDefaultBackground = Utils.ImageResizer(iDefaultBackground, 289, 435);

            frmImagePreview preview = new frmImagePreview();
            preview.BackgroundImageLayout = ImageLayout.Stretch;
            preview.BackgroundImage = iDefaultBackground;
            preview.ClientSize = new System.Drawing.Size(iDefaultBackground.Width, iDefaultBackground.Height);
            //preview.MouseClick += new MouseEventHandler((sender, e) => { if (!preview.IsDragging) preview.Close(); });
            preview.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            preview.Padding = new Padding(0);
            //preview.AllowTransparency = true;
            //preview.StartPosition = FormStartPosition.CenterParent;

            if (card_level >= 15) card_level = 15;
            if (card_level <= 0) card_level = 0;

            if (element == 100)
            {
                card_level = 10;
                preview.Text = "EK Unleashed :: " + card_name;
            }
            else
            {
                preview.Text = "EK Unleashed :: " + card_name + ((card_level > 0) ? " at level " + card_level.ToString() : "");
            }

            preview.StartPosition = FormStartPosition.Manual;
            preview.Location = popup_location;

            preview.Icon = (Icon)this.Icon.Clone();

            System.Windows.Forms.PictureBox pb = new PictureBox();
            pb.Location = new Point(0, 0);
            pb.BorderStyle = BorderStyle.None;

            try
            {
                if ((Utils.CInt(card_id) == 0) && (iBaseImage != null))
                    pb.Image = GameResourceManager.GenerateFakeCardImage(iBaseImage, card_level, card_name, element, stars, cost, wait, skills, attack_progression, hp_progression);
                else
                    pb.Image = GameResourceManager.GenerateCardImage(card_id, card_level, card_name, element, stars, cost, wait, skills, attack_progression, hp_progression, null, evolved_times);

                pb.Size = new System.Drawing.Size(pb.Image.Width, pb.Image.Height);
            }
            catch { }
            //pb.SizeMode = PictureBoxSizeMode.AutoSize;
            pb.Padding = new Padding(0);
            //pb.MouseClick += new MouseEventHandler((sender, e) => { if (!preview.IsDragging) preview.Close(); });
            pb.MouseDown += new MouseEventHandler(preview.parent_MouseDown);
            pb.MouseUp += new MouseEventHandler(preview.parent_MouseUp);
            pb.MouseMove += new MouseEventHandler(preview.parent_MouseMove);
            preview.Controls.Add(pb);

            try { preview.ClientSize = new Size(pb.Image.Width, pb.Image.Height + 2); } catch { }
            try { preview.Size = new System.Drawing.Size(pb.Image.Width, pb.Image.Height); } catch { }

            preview.IsExpanded = true;
            preview.PreviewType = frmImagePreview.PreviewTypes.Card;

            preview.Show();
        }

        public void PopupRunePreviewWindow(int rune_id, int rune_level, Point popup_location)
        {
            JObject rune = Game.GetRuneByID(rune_id);
            if (rune == null)
                return;

            Image iDefaultBackground = GameResourceManager.LoadResource_RuneBlank();
            iDefaultBackground = Utils.ImageResizer(iDefaultBackground, 291, 410);

            frmImagePreview preview = new frmImagePreview();
            preview.BackgroundImageLayout = ImageLayout.Stretch;
            preview.BackgroundImage = iDefaultBackground;
            preview.ClientSize = new System.Drawing.Size(iDefaultBackground.Width, iDefaultBackground.Height);
            preview.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            if (rune_level >= 4) rune_level = 4;
            if (rune_level <= 0) rune_level = 0;

            preview.Text = "EK Unleashed :: " + rune["RuneName"].ToString() + ((rune_level > 0) ? " at level " + rune_level.ToString() : "");

            preview.StartPosition = FormStartPosition.Manual;
            preview.Location = popup_location;

            preview.Icon = (Icon)this.Icon.Clone();

            System.Windows.Forms.PictureBox pb = new PictureBox();
            pb.Location = new Point(0, 0);
            pb.BorderStyle = BorderStyle.None;
            try
            {
                pb.Image = GameResourceManager.GenerateRuneImage(rune_id, rune_level, rune["RuneName"].ToString().Trim(), Utils.CInt(rune["Property"]), Utils.CInt(rune["Color"]), Game.GetSkillByID(Utils.CInt(rune["LockSkill" + (rune_level + 1).ToString()])), rune["Condition"].ToString(), rune["SkillTimes"].ToString());
                pb.Size = new System.Drawing.Size(pb.Image.Width, pb.Image.Height);
            }
            catch { }
            //pb.SizeMode = PictureBoxSizeMode.AutoSize;
            pb.Padding = new System.Windows.Forms.Padding(0);
            pb.MouseDown += new MouseEventHandler(preview.parent_MouseDown);
            pb.MouseUp += new MouseEventHandler(preview.parent_MouseUp);
            pb.MouseMove += new MouseEventHandler(preview.parent_MouseMove);
            preview.Controls.Add(pb);

            try { preview.ClientSize = new Size(pb.Image.Width, pb.Image.Height + 2); } catch { }
            try { preview.Size = new System.Drawing.Size(pb.Image.Width, pb.Image.Height); } catch { }

            preview.IsExpanded = true;
            preview.PreviewType = frmImagePreview.PreviewTypes.Rune;

            preview.Show();
        }

        private void dumpDataTojsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                Game.DumpData();

                if (Utils.False("Dev_WantImageGeneration"))
                {
                    double dImageSaveQuality = Utils.CDbl(Utils.GetAppSetting("Dev_ImageSaveQuality"));
                    if (dImageSaveQuality == 0.0)
                        dImageSaveQuality = 95.0;

                    string sImageSaveFormat = Utils.GetAppSetting("Dev_ImageSaveFormat");
                    if (sImageSaveFormat.Trim().ToUpper() != "JPG")
                        sImageSaveFormat = "PNG";

                    bool bEnglishize = true;

                    foreach (JObject card in Game.Cards_JSON_Parsed["data"]["Cards"])
                    {
                        try
                        {
                            int card_level = 10;
                            string card_name = card["CardName"].ToString();

                            if (bEnglishize)
                            {
                                JObject temp_card = Game.GetCardByID_English(Utils.CInt(card["CardId"]));
                                if (temp_card != null)
                                    card_name = temp_card["CardName"].ToString();
                                card_name = Game.TranslateCardName(card_name);
                            }

                            int element = Utils.CInt(card["Race"]);
                            int stars = Utils.CInt(card["Color"]);
                            int cost = Utils.CInt(card["Cost"]);
                            int wait = Utils.CInt(card["Wait"]);
                            List<JObject> skills = new List<JObject>();
                            try
                            {
                                JObject skill = (bEnglishize) ? Game.GetSkillByID_English(Utils.CInt(card["Skill"])) : Game.GetSkillByID(Utils.CInt(card["Skill"]));
                                if (skill == null)
                                    skill = JObject.Parse("{ \"empty\": \"1\" }");
                                skills.Add(skill);
                            }
                            catch
                            {
                                skills.Add(JObject.Parse("{ \"empty\": \"1\" }"));
                            }
                            try
                            {
                                JObject skill = (bEnglishize) ? Game.GetSkillByID_English(Utils.CInt(card["LockSkill1"])) : Game.GetSkillByID(Utils.CInt(card["LockSkill1"]));
                                if (skill == null)
                                    skill = JObject.Parse("{ \"empty\": \"1\" }");
                                skills.Add(skill);
                            }
                            catch
                            {
                                skills.Add(JObject.Parse("{ \"empty\": \"1\" }"));
                            }
                            try
                            {
                                string[] sub_skills = Utils.SubStringsDups(card["LockSkill2"].ToString(), "_");

                                foreach (string sub_skill in sub_skills)
                                {
                                    JObject skill = (bEnglishize) ? Game.GetSkillByID_English(Utils.CInt(sub_skill)) : Game.GetSkillByID(Utils.CInt(sub_skill));
                                    if (skill == null)
                                        skill = JObject.Parse("{ \"empty\": \"1\" }");
                                    skills.Add(skill);
                                }
                            }
                            catch
                            {
                                skills.Add(JObject.Parse("{ \"empty\": \"1\" }"));
                            }
                            List<int> attack_progression = new List<int>();
                            foreach (var attack in card["AttackArray"])
                                attack_progression.Add(Utils.CInt(attack.ToString()));
                            List<int> hp_progression = new List<int>();
                            foreach (var attack in card["HpArray"])
                                hp_progression.Add(Utils.CInt(attack.ToString()));

                            Image i = GameResourceManager.GenerateCardImage(Utils.CInt(card["CardId"]), card_level, card_name, element, stars, cost, wait, skills, attack_progression, hp_progression);

                            if (i != null)
                            {
                                Utils.Chatter(GameClient.Current.CurrentGame + "_GeneratedCard_" + Utils.CInt(card["CardId"]).ToString("0000") + "_" + card_name.Trim().Replace(" ", "_") + "_level_" + card_level.ToString("00") + ".png");

                                string image_filename = "";

                                if (GameClient.Current.Service != GameClient.GameService.Shikoku_Wars)
                                    image_filename = "ResourceCache\\" + GameClient.Current.CurrentGame + "_GeneratedCard_" + Utils.CInt(card["CardId"]).ToString("0000") + "_" + card_name.Trim().Replace(" ", "_") + "_level_" + card_level.ToString("00");
                                else
                                    image_filename = "ResourceCache\\" + GameClient.Current.CurrentGame + "_GeneratedCard_" + Utils.CInt(card["CardId"]).ToString("0000") + "_" + "_level_" + card_level.ToString("00");

                                if (sImageSaveFormat == "JPG")
                                {
                                    ImageCodecInfo codec = Utils.GetEncoderInfo("image/jpeg");

                                    System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                                    EncoderParameter ratio = new EncoderParameter(qualityEncoder, (long)dImageSaveQuality);
                                    EncoderParameters codecParams = new EncoderParameters(1);
                                    codecParams.Param[0] = ratio;

                                    i.Save(image_filename + ".jpg", codec, codecParams);
                                }
                                else
                                    i.Save(image_filename + ".png", System.Drawing.Imaging.ImageFormat.Png);

                                i.Dispose();
                            }
                        }
                        catch { }
                    }

                    foreach (JObject rune in Game.Runes_JSON_Parsed["data"]["Runes"])
                    {
                        try
                        {
                            int rune_level = 4;
                            string rune_name = rune["RuneName"].ToString().Trim();

                            Image i = GameResourceManager.GenerateRuneImage(Utils.CInt(rune["RuneId"]), 4, rune_name, Utils.CInt(rune["Property"]), Utils.CInt(rune["Color"]), Game.GetSkillByID(Utils.CInt(rune["LockSkill" + (rune_level + 1).ToString()])), rune["Condition"].ToString(), rune["SkillTimes"].ToString());

                            if (i != null)
                            {
                                Utils.Chatter(GameClient.Current.CurrentGame + "_GeneratedRune_" + Utils.CInt(rune["RuneId"]).ToString("0000") + "_" + rune_name.Trim().Replace(" ", "_") + "_level_" + rune_level.ToString("00") + ".png");

                                string image_filename = "";

                                if (GameClient.Current.Service != GameClient.GameService.Shikoku_Wars)
                                    image_filename = "ResourceCache\\" + GameClient.Current.CurrentGame + "_GeneratedRune_" + Utils.CInt(rune["RuneId"]).ToString("0000") + "_" + rune_name.Trim().Replace(" ", "_") + "_level_" + rune_level.ToString("00");
                                else
                                    image_filename = "ResourceCache\\" + GameClient.Current.CurrentGame + "_GeneratedRune_" + Utils.CInt(rune["RuneId"]).ToString("0000") + "_" + "_level_" + rune_level.ToString("00");

                                if (sImageSaveFormat == "JPG")
                                {
                                    ImageCodecInfo codec = Utils.GetEncoderInfo("image/jpeg");

                                    System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                                    EncoderParameter ratio = new EncoderParameter(qualityEncoder, (long)dImageSaveQuality);
                                    EncoderParameters codecParams = new EncoderParameters(1);
                                    codecParams.Param[0] = ratio;

                                    i.Save(image_filename + ".jpg", codec, codecParams);
                                }
                                else
                                    i.Save(image_filename + ".png", System.Drawing.Imaging.ImageFormat.Png);


                                i.Dispose();
                            }
                        }
                        catch { }
                    }
                }
            });
        }

        private void moreEnergyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGems = Utils.CInt(user_data["data"]["Cash"]);
                int iCost = Utils.CInt(user_data["data"]["BuyEnergyCost"]);

                if (MessageBox.Show("You have " + iGems.ToString("#,##0") + " gems and it will cost " + iCost.ToString() + " to buy +20 energy.\r\n\r\nContinue?", "Really Spend " + iCost.ToString() + " Gems?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    Game.GetGameData("user", "EditEnergy", false);
                    user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                    this.GameVitalsUpdateUI(user_data, null, null);
                }
            }
            catch { }
        }

        private void map4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(4);
        }

        private void map5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(5);
        }

        private void map6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(6);
        }

        private void map7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(7);
        }

        private void map8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(8);
        }

        private void map9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(9);
        }

        private void map10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ResetMazeTower(10);
        }

        public enum StoreTypes
        {
            Gold,
            Gems_30,
            Gems_300,
            Gems_50,
            Gems_480,
            FireTokens,
        }

        public static JObject GetStoreType(JObject shop_data, StoreTypes st)
        {
            JObject shop = null;

            try
            {
                foreach (var shop_temp in (JArray)(shop_data["data"]))
                {
                    int iGoldCost = Utils.CInt(shop_temp["Coins"]);
                    int iGemsCost = Utils.CInt(shop_temp["Cash"]);
                    int iFTCost = Utils.CInt(shop_temp["Ticket"]);

                    if (st == StoreTypes.Gold)
                    {
                        if (iGoldCost > 0)
                        {
                            shop = (JObject)shop_temp;
                            break;
                        }
                    }
                    else if (st == StoreTypes.Gems_30)
                    {
                        if (iGemsCost == 30)
                        {
                            shop = (JObject)shop_temp;
                            break;
                        }
                    }
                    else if (st == StoreTypes.Gems_300)
                    {
                        if (iGemsCost == 300)
                        {
                            shop = (JObject)shop_temp;
                            break;
                        }
                    }
                    else if (st == StoreTypes.Gems_50)
                    {
                        if (iGemsCost == 50)
                        {
                            shop = (JObject)shop_temp;
                            break;
                        }
                    }
                    else if (st == StoreTypes.Gems_480)
                    {
                        if (iGemsCost == 480)
                        {
                            shop = (JObject)shop_temp;
                            break;
                        }
                    }
                    else if (st == StoreTypes.FireTokens)
                    {
                        if (iFTCost == 1)
                        {
                            shop = (JObject)shop_temp;
                            break;
                        }
                    }
                }
            }
            catch { }                

            if (shop == null)
            {
                for (int i = 0; i <= 100; i++)
                {
                    try
                    {
                        JObject shop_temp = (JObject)shop_data["data"][i.ToString()];

                        int iGoldCost = Utils.CInt(shop_temp["Coins"]);
                        int iGemsCost = Utils.CInt(shop_temp["Cash"]);
                        int iFTCost = Utils.CInt(shop_temp["Ticket"]);

                        if (st == StoreTypes.Gold)
                        {
                            if (iGoldCost > 0)
                            {
                                shop = (JObject)shop_temp;
                                break;
                            }
                        }
                        else if (st == StoreTypes.Gems_30)
                        {
                            if (iGemsCost == 30)
                            {
                                shop = (JObject)shop_temp;
                                break;
                            }
                        }
                        else if (st == StoreTypes.Gems_300)
                        {
                            if (iGemsCost == 300)
                            {
                                shop = (JObject)shop_temp;
                                break;
                            }
                        }
                        else if (st == StoreTypes.Gems_50)
                        {
                            if (iGemsCost == 50)
                            {
                                shop = (JObject)shop_temp;
                                break;
                            }
                        }
                        else if (st == StoreTypes.Gems_480)
                        {
                            if (iGemsCost == 480)
                            {
                                shop = (JObject)shop_temp;
                                break;
                            }
                        }
                        else if (st == StoreTypes.FireTokens)
                        {
                            if (iFTCost == 1)
                            {
                                shop = (JObject)shop_temp;
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }

            return shop;
        }

        private void goldPacksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGold = Utils.CInt(user_data["data"]["Coins"]);

                JObject shop_data = JObject.Parse(Game.GetGameData("shop", "GetGoods", false));

                int iConsecutiveErrors = 0;

                JObject shop = frmMain.GetStoreType(shop_data, StoreTypes.Gold);
                if (shop != null)
                {
                    try
                    {
                        int iGoldCost = Utils.CInt(shop["Coins"]);

                        if (iGoldCost > 0)
                        {
                            string pack_name = shop["Name"].ToString();

                            if (pack_name.ToLower().EndsWith("ard")) pack_name = pack_name + "s";
                            else if (pack_name.ToLower().EndsWith("ack")) pack_name = pack_name + "s";
                            else pack_name = pack_name + "card packs";

                            int iHowMany = Utils.CInt(Utils.Input_Text("Buy How Many", "How many " + pack_name + " would you like to buy?"));
                            if (iHowMany < 1)
                                return;

                            if (MessageBox.Show("You have " + iGold.ToString("#,##0") + " gold and it will cost " + (iGoldCost * iHowMany).ToString("#,##0") + " to buy " + iHowMany.ToString("#,##0") + " x " + shop["Name"].ToString() + ".\r\n\r\nContinue?", "Really Spend " + (iGoldCost * iHowMany).ToString("#,##0") + " Gold?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            {
                                for (int i = 0; i < iHowMany; i++)
                                {
                                    try
                                    {
                                        JObject cards_received = JObject.Parse(Game.GetGameData("shop", "Buy", "GoodsId=" + shop["GoodsId"].ToString(), false));

                                        string[] card_ids = Utils.SubStringsDups(cards_received["data"].ToString(), "_");
                                        string all_cards = "";
                                        bool bFiveStar = false;
                                        bool bEventCard = false;
                                        bool bTreasureCard = false;
                                        bool bFoodCard = false;

                                        foreach (string card_id in card_ids)
                                        {
                                            all_cards += ", [Card #" + card_id + "]";

                                            GameObjs.Card card = new GameObjs.Card(Utils.CInt(card_id));

                                            if (card.Stars == 5)
                                                bFiveStar = true;
                                            if (card.EventCard)
                                                bEventCard = true;
                                            if (card.TreasureCard)
                                                bTreasureCard = true;
                                            if (card.FoodCard)
                                                bFoodCard = true;
                                        }

                                        all_cards = all_cards.Trim(new char[] { ' ', ',' });

                                        Utils.LoggerNotifications("<color=#ffa000>You bought a " + shop["Name"].ToString() + " pack and received...</color>");
                                        Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                        if (bFiveStar)
                                            Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                        if (bEventCard)
                                            Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                        if (bTreasureCard)
                                            Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                        if (bFoodCard)
                                            Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");

                                        iConsecutiveErrors = 0;
                                    }
                                    catch
                                    {
                                        i--;
                                        iConsecutiveErrors++;

                                        if (iConsecutiveErrors > 5)
                                            break;
                                    }
                                }

                                user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                                this.GameVitalsUpdateUI(user_data, null, null);
                            }
                        }
                    }
                    catch { }
                }
            });
        }

        private void gemSingleCardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGems = Utils.CInt(user_data["data"]["Cash"]);

                JObject shop_data = JObject.Parse(Game.GetGameData("shop", "GetGoods", false));

                int iConsecutiveErrors = 0;

                JObject shop = frmMain.GetStoreType(shop_data, StoreTypes.Gems_30);
                if (shop != null)
                {
                    try
                    {
                        int iGemsCost = Utils.CInt(shop["Cash"]);
                        string pack_name = shop["Name"].ToString();

                        if (pack_name.ToLower().EndsWith("ard")) pack_name = pack_name + "s";
                        else if (pack_name.ToLower().EndsWith("ack")) pack_name = pack_name + "s";
                        else pack_name = pack_name + "card packs";

                        int iHowMany = Utils.CInt(Utils.Input_Text("Buy How Many", "How many " + pack_name + " would you like to buy?"));
                        if (iHowMany < 1)
                            return;

                        if (MessageBox.Show("You have " + iGems.ToString("#,##0") + " " + Utils.Pluralize("gem", iGems) + " and it will cost " + (iGemsCost * iHowMany).ToString("#,##0") + " to buy " + iHowMany.ToString("#,##0") + " x " + shop["Name"].ToString() + " " + Utils.Pluralize("pack", iHowMany) + ".\r\n\r\nContinue?", "Really Spend " + (iGemsCost * iHowMany).ToString("#,##0") + " " + Utils.Pluralize("Gem", iGemsCost * iHowMany) + "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            for (int i = 0; i < iHowMany; i++)
                            {
                                try
                                {
                                    JObject cards_received = JObject.Parse(Game.GetGameData("shop", "Buy", "GoodsId=" + shop["GoodsId"].ToString(), false));

                                    string[] card_ids = Utils.SubStringsDups(cards_received["data"].ToString(), "_");
                                    string all_cards = "";
                                    bool bFiveStar = false;
                                    bool bEventCard = false;
                                    bool bTreasureCard = false;
                                    bool bFoodCard = false;

                                    foreach (string card_id in card_ids)
                                    {
                                        all_cards += ", [Card #" + card_id + "]";

                                        GameObjs.Card card = new GameObjs.Card(Utils.CInt(card_id));

                                        if (card.Stars == 5)
                                            bFiveStar = true;
                                        if (card.EventCard)
                                            bEventCard = true;
                                        if (card.TreasureCard)
                                            bTreasureCard = true;
                                        if (card.FoodCard)
                                            bFoodCard = true;
                                    }

                                    all_cards = all_cards.Trim(new char[] { ' ', ',' });

                                    Utils.LoggerNotifications("<color=#ffa000>You bought a " + shop["Name"].ToString() + " pack and received...</color>");
                                    Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                    if (bFiveStar)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                    if (bEventCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                    if (bTreasureCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                    if (bFoodCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");

                                    iConsecutiveErrors = 0;
                                }
                                catch
                                {
                                    i--;
                                    iConsecutiveErrors++;

                                    if (iConsecutiveErrors > 5)
                                        break;
                                }
                            }

                            user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                            this.GameVitalsUpdateUI(user_data, null, null);
                        }

                    }
                    catch { }
                }
            });
        }

        private void gemMulticardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGems = Utils.CInt(user_data["data"]["Cash"]);

                JObject shop_data = JObject.Parse(Game.GetGameData("shop", "GetGoods", false));

                int iConsecutiveErrors = 0;

                JObject shop = frmMain.GetStoreType(shop_data, StoreTypes.Gems_300);
                if (shop != null)
                {
                    try
                    {
                        int iGemsCost = Utils.CInt(shop["Cash"]);

                        string pack_name = shop["Name"].ToString();

                        if (pack_name.ToLower().EndsWith("ard")) pack_name = pack_name + "s";
                        else if (pack_name.ToLower().EndsWith("ack")) pack_name = pack_name + "s";
                        else pack_name = pack_name + "card packs";

                        int iHowMany = Utils.CInt(Utils.Input_Text("Buy How Many", "How many " + pack_name + " would you like to buy?"));
                        if (iHowMany < 1)
                            return;

                        if (MessageBox.Show("You have " + iGems.ToString("#,##0") + " " + Utils.Pluralize("gem", iGems) + " and it will cost " + (iGemsCost * iHowMany).ToString("#,##0") + " to buy " + iHowMany.ToString("#,##0") + " x " + shop["Name"].ToString() + " " + Utils.Pluralize("pack", iHowMany) + ".\r\n\r\nContinue?", "Really Spend " + (iGemsCost * iHowMany).ToString("#,##0") + " " + Utils.Pluralize("Gem", iGemsCost * iHowMany) + "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            for (int i = 0; i < iHowMany; i++)
                            {
                                try
                                {
                                    JObject cards_received = JObject.Parse(Game.GetGameData("shop", "Buy", "GoodsId=" + shop["GoodsId"].ToString(), false));

                                    string[] card_ids = Utils.SubStringsDups(cards_received["data"].ToString(), "_");
                                    string all_cards = "";
                                    bool bFiveStar = false;
                                    bool bEventCard = false;
                                    bool bTreasureCard = false;
                                    bool bFoodCard = false;

                                    foreach (string card_id in card_ids)
                                    {
                                        all_cards += ", [Card #" + card_id + "]";

                                        GameObjs.Card card = new GameObjs.Card(Utils.CInt(card_id));

                                        if (card.Stars == 5)
                                            bFiveStar = true;
                                        if (card.EventCard)
                                            bEventCard = true;
                                        if (card.TreasureCard)
                                            bTreasureCard = true;
                                        if (card.FoodCard)
                                            bFoodCard = true;
                                    }

                                    all_cards = all_cards.Trim(new char[] { ' ', ',' });

                                    Utils.LoggerNotifications("<color=#ffa000>You bought a " + shop["Name"].ToString() + " pack and received...</color>");
                                    Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                    if (bFiveStar)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                    if (bEventCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                    if (bTreasureCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                    if (bFoodCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");

                                    iConsecutiveErrors = 0;
                                }
                                catch
                                {
                                    i--;
                                    iConsecutiveErrors++;

                                    if (iConsecutiveErrors > 5)
                                        break;
                                }
                            }

                            user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                            this.GameVitalsUpdateUI(user_data, null, null);
                        }
                    }
                    catch { }
                }
            });
        }

        private void gemPromoCardsifAvailableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGems = Utils.CInt(user_data["data"]["Cash"]);

                JObject shop_data = JObject.Parse(Game.GetGameData("shop", "GetGoods", false));

                int iConsecutiveErrors = 0;

                JObject shop = frmMain.GetStoreType(shop_data, StoreTypes.Gems_50);
                if (shop != null)
                {
                    try
                    {
                        int iGemsCost = Utils.CInt(shop["Cash"]);

                        string pack_name = shop["Name"].ToString();

                        if (pack_name.ToLower().EndsWith("ard")) pack_name = pack_name + "s";
                        else if (pack_name.ToLower().EndsWith("ack")) pack_name = pack_name + "s";
                        else pack_name = pack_name + "card packs";

                        int iHowMany = Utils.CInt(Utils.Input_Text("Buy How Many", "How many " + pack_name + " would you like to buy?"));
                        if (iHowMany < 1)
                            return;

                        if (MessageBox.Show("You have " + iGems.ToString("#,##0") + " " + Utils.Pluralize("gem", iGems) + " and it will cost " + (iGemsCost * iHowMany).ToString("#,##0") + " to buy " + iHowMany.ToString("#,##0") + " x " + shop["Name"].ToString() + " " + Utils.Pluralize("pack", iHowMany) + ".\r\n\r\nContinue?", "Really Spend " + (iGemsCost * iHowMany).ToString("#,##0") + " " + Utils.Pluralize("Gem", iGemsCost * iHowMany) + "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            for (int i = 0; i < iHowMany; i++)
                            {
                                try
                                {
                                    JObject cards_received = JObject.Parse(Game.GetGameData("shop", "Buy", "GoodsId=" + shop["GoodsId"].ToString(), false));

                                    string[] card_ids = Utils.SubStringsDups(cards_received["data"].ToString(), "_");
                                    string all_cards = "";
                                    bool bFiveStar = false;
                                    bool bEventCard = false;
                                    bool bTreasureCard = false;
                                    bool bFoodCard = false;

                                    foreach (string card_id in card_ids)
                                    {
                                        all_cards += ", [Card #" + card_id + "]";

                                        GameObjs.Card card = new GameObjs.Card(Utils.CInt(card_id));

                                        if (card.Stars == 5)
                                            bFiveStar = true;
                                        if (card.EventCard)
                                            bEventCard = true;
                                        if (card.TreasureCard)
                                            bTreasureCard = true;
                                        if (card.FoodCard)
                                            bFoodCard = true;
                                    }

                                    all_cards = all_cards.Trim(new char[] { ' ', ',' });

                                    Utils.LoggerNotifications("<color=#ffa000>You bought a " + shop["Name"].ToString() + " pack and received...</color>");
                                    Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                    if (bFiveStar)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                    if (bEventCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                    if (bTreasureCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                    if (bFoodCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");

                                    iConsecutiveErrors = 0;
                                }
                                catch
                                {
                                    i--;
                                    iConsecutiveErrors++;

                                    if (iConsecutiveErrors > 5)
                                        break;
                                }
                            }

                            user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                            this.GameVitalsUpdateUI(user_data, null, null);
                        }
                    }
                    catch { }
                }
            });
        }

        private void gemPromoCardsifAvailableToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGems = Utils.CInt(user_data["data"]["Cash"]);

                JObject shop_data = JObject.Parse(Game.GetGameData("shop", "GetGoods", false));

                int iConsecutiveErrors = 0;

                JObject shop = frmMain.GetStoreType(shop_data, StoreTypes.Gems_480);
                if (shop != null)
                {
                    try
                    {
                        int iGemsCost = Utils.CInt(shop["Cash"]);

                        string pack_name = shop["Name"].ToString();

                        if (pack_name.ToLower().EndsWith("ard")) pack_name = pack_name + "s";
                        else if (pack_name.ToLower().EndsWith("ack")) pack_name = pack_name + "s";
                        else pack_name = pack_name + "card packs";

                        int iHowMany = Utils.CInt(Utils.Input_Text("Buy How Many", "How many " + pack_name + " would you like to buy?"));
                        if (iHowMany < 1)
                            return;

                        if (MessageBox.Show("You have " + iGems.ToString("#,##0") + " " + Utils.Pluralize("gem", iGems) + " and it will cost " + (iGemsCost * iHowMany).ToString("#,##0") + " to buy " + iHowMany.ToString("#,##0") + " x " + shop["Name"].ToString() + " " + Utils.Pluralize("pack", iHowMany) + ".\r\n\r\nContinue?", "Really Spend " + (iGemsCost * iHowMany).ToString("#,##0") + " " + Utils.Pluralize("Gem", iGemsCost * iHowMany) + "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            for (int i = 0; i < iHowMany; i++)
                            {
                                try
                                {
                                    JObject cards_received = JObject.Parse(Game.GetGameData("shop", "Buy", "GoodsId=" + shop["GoodsId"].ToString(), false));

                                    string[] card_ids = Utils.SubStringsDups(cards_received["data"].ToString(), "_");
                                    string all_cards = "";
                                    bool bFiveStar = false;
                                    bool bEventCard = false;
                                    bool bTreasureCard = false;
                                    bool bFoodCard = false;

                                    foreach (string card_id in card_ids)
                                    {
                                        all_cards += ", [Card #" + card_id + "]";

                                        GameObjs.Card card = new GameObjs.Card(Utils.CInt(card_id));

                                        if (card.Stars == 5)
                                            bFiveStar = true;
                                        if (card.EventCard)
                                            bEventCard = true;
                                        if (card.TreasureCard)
                                            bTreasureCard = true;
                                        if (card.FoodCard)
                                            bFoodCard = true;
                                    }

                                    all_cards = all_cards.Trim(new char[] { ' ', ',' });

                                    Utils.LoggerNotifications("<color=#ffa000>You bought a " + shop["Name"].ToString() + " pack and received...</color>");
                                    Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                    if (bFiveStar)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                    if (bEventCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                    if (bTreasureCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                    if (bFoodCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");

                                    iConsecutiveErrors = 0;
                                }
                                catch
                                {
                                    i--;
                                    iConsecutiveErrors++;

                                    if (iConsecutiveErrors > 5)
                                        break;
                                }
                            }

                            user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                            this.GameVitalsUpdateUI(user_data, null, null);
                        }
                    }
                    catch { }
                }
            });
        }

        private void fireTokensPackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iTickets = Utils.CInt(user_data["data"]["Ticket"]);

                JObject shop_data = JObject.Parse(Game.GetGameData("shop", "GetGoods", false));

                int iConsecutiveErrors = 0;

                JObject shop = frmMain.GetStoreType(shop_data, StoreTypes.FireTokens);
                if (shop != null)
                {
                    try
                    {
                        int iTicketCost = Utils.CInt(shop["Ticket"]);

                        string pack_name = shop["Name"].ToString();

                        if (pack_name.ToLower().EndsWith("ard")) pack_name = pack_name + "s";
                        else if (pack_name.ToLower().EndsWith("ack")) pack_name = pack_name + "s";
                        else if (pack_name.ToLower().EndsWith("oken")) pack_name = pack_name + " cards";
                        else if (pack_name.ToLower().EndsWith("oupon")) pack_name = pack_name + " cards";
                        else pack_name = pack_name + " card packs";

                        string fire_token = "Fire Token";
                        if (Game.Service == GameClient.GameService.Lies_of_Astaroth || Game.Service == GameClient.GameService.Elves_Realm)
                                fire_token = "Magic Coupon";

                        int iHowMany = Utils.CInt(Utils.Input_Text("Buy How Many", "How many " + pack_name + " would you like to buy?"));
                        if (iHowMany < 1)
                            return;

                        if (MessageBox.Show("You have " + iTickets.ToString("#,##0") + " " + Utils.Pluralize(fire_token.ToLower(), iTickets) + " and it will cost " + (iTicketCost * iHowMany).ToString("#,##0") + " to buy " + iHowMany.ToString("#,##0") + " x " + shop["Name"].ToString() + " " + Utils.Pluralize("pack", iHowMany) + ".\r\n\r\nContinue?", "Really Spend " + (iTicketCost * iHowMany).ToString("#,##0") + " " + Utils.Pluralize(fire_token, iTicketCost * iHowMany) + "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            for (int i = 0; i < iHowMany; i++)
                            {
                                try
                                {
                                    JObject cards_received = JObject.Parse(Game.GetGameData("shop", "Buy", "GoodsId=" + shop["GoodsId"].ToString(), false));

                                    string[] card_ids = Utils.SubStringsDups(cards_received["data"].ToString(), "_");
                                    string all_cards = "";
                                    bool bFiveStar = false;
                                    bool bEventCard = false;
                                    bool bTreasureCard = false;
                                    bool bFoodCard = false;

                                    foreach (string card_id in card_ids)
                                    {
                                        all_cards += ", [Card #" + card_id + "]";

                                        GameObjs.Card card = new GameObjs.Card(Utils.CInt(card_id));

                                        if (card.Stars == 5)
                                            bFiveStar = true;
                                        if (card.EventCard)
                                            bEventCard = true;
                                        if (card.TreasureCard)
                                            bTreasureCard = true;
                                        if (card.FoodCard)
                                            bFoodCard = true;
                                    }

                                    all_cards = all_cards.Trim(new char[] { ' ', ',' });

                                    Utils.LoggerNotifications("<color=#ffa000>You bought a " + shop["Name"].ToString() + " pack and received...</color>");
                                    Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                    if (bFiveStar)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                    if (bEventCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                    if (bTreasureCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                    if (bFoodCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");

                                    iConsecutiveErrors = 0;
                                }
                                catch
                                {
                                    i--;
                                    iConsecutiveErrors++;

                                    if (iConsecutiveErrors > 5)
                                        break;
                                }
                            }

                            user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                            this.GameVitalsUpdateUI(user_data, null, null);
                        }
                    }
                    catch { }
                }
            });
        }

        private void thiefToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                JObject user_data = JObject.Parse(Game.GetGameData("user", "GetUserInfo", false));
                int iGems = Utils.CInt(user_data["data"]["Cash"]);

                JObject user_thieves = JObject.Parse(Game.GetGameData("arena", "GetThieves", true, "thief_info"));
                int iThiefTimer = Utils.CInt(user_data["user_thieves"]["Countdown"]);
            }
            catch { }
        }

        private void addAnAttemptToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void cooldownToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void useUpWorldTreeBattlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                Game.Play_WorldTree(true);
            });
        }

        private void enchantACardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string EnchantCard = Utils.Input_Text("Enchant Which Card?", "Enter the name of a card you want to enchant:");
            if (string.IsNullOrEmpty(EnchantCard))
                return;

            int EnchantLevel = Utils.CInt(Utils.Input_Text("To What Level?", "Enchant this card to what level (1 - 10 or 1 - 15):"));
            if ((EnchantLevel < 1) || (EnchantLevel > 15))
                return;

            Utils.StartMethodMultithreaded(() =>
            {
                Game.EnchantCard(EnchantCard, EnchantLevel);
            });
        }

        public static string[] ChatChannel = new string[] { "W", "" };

        private void chatchannelWorld_Click(object sender, EventArgs e)
        {
            frmMain.ChatChannel = new string[] { "W", "" };
            this.btnChatChannel.Text = "World";
            this.RefocusInput();
        }

        private void chatchannelClan_Click(object sender, EventArgs e)
        {
            if (Utils.ValidNumber(Game.Clan_ID))
            {
                frmMain.ChatChannel = new string[] { "C", "" };
                this.btnChatChannel.Text = "Clan";
                this.RefocusInput();
            }
            else
            {
                this.tabsChatChannels.SelectedTab = this.tabGeneral;
            }
        }

        private void btnChatChannel_Click(object sender, EventArgs e)
        {
            this.mnuChatChannels.Show(frmMain.MousePosition);
        }

        private void tabGeneral_Enter(object sender, EventArgs e)
        {
            if (this.LastTab != this.tabGeneral)
            {
                this.LastTab = this.tabGeneral;
                this.chatchannelWorld_Click(null, null);
            }
        }

        private void tabClan_Enter(object sender, EventArgs e)
        {
            if (this.LastTab != this.tabClan)
            {
                this.LastTab = this.tabClan;
                this.chatchannelClan_Click(null, null);
            }
        }

        private void tabPrivate_Enter(object sender, EventArgs e)
        {
            if ((Game == null) || (Game.Chat == null))
                return;

            if (frmMain.ChatChannel[0] == "P")
                return;

            if (Utils.ValidNumber(Game.Chat.LastPMFromUID))
            {
                frmMain.ChatChannel = new string[] { "P", Game.Chat.LastPMFromUID };
                this.btnChatChannel.Text = GameChat.GetUserName(Utils.CInt(Game.Chat.LastPMFromUID));
                this.RefocusInput();
            }
            else if (Utils.ValidNumber(Game.Chat.LastPMToUID))
            {
                frmMain.ChatChannel = new string[] { "P", Game.Chat.LastPMToUID };
                this.btnChatChannel.Text = GameChat.GetUserName(Utils.CInt(Game.Chat.LastPMToUID));
                this.RefocusInput();
            }
            else
            {
                if (frmMain.ChatChannel[0] == "C") this.tabsChatChannels.SelectedTab = this.tabClan;
                else this.tabsChatChannels.SelectedTab = this.tabGeneral;
            }
        }

        private void privateMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = Utils.Input_Text("Private Message", "Enter a name to send a private message to:");
            if (!Utils.ValidText(name))
                return;

            string uid = GameChat.GetUserID(name).ToString();

            if (Utils.CInt(uid) == 0)
                uid = Game.GetUserID(name.Trim());

            if (!string.IsNullOrEmpty(uid) && Utils.CInt(uid) > 0)
            {
                GameChat.SaveUserName(Utils.CInt(uid), name);

                frmMain.ChatChannel = new string[] { "P", uid };
                this.btnChatChannel.Text = name;
                this.RefocusInput();
            }
            else
                Utils.Chatter("<color=#ff4040>Error: couldn't find any players by that name.  Remember that names are CaSe-SeNsItIVe.</color>");
        }

        private TabPage LastTab = null;

        private void tabsChatChannels_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            //this.LastTab = e.TabPage;
        }

        public void RefreshWindow()
        {
            if (this.InvokeRequired)
            {
                Delegate__Void d = this.RefreshWindow;
                this.Invoke(d);
                return;
            }

            this.Refresh();
        }

        private string _TitleText = "";
        public void SetText(string real_title, string display_title)
        {
            if (this.InvokeRequired)
            {
                VOID__STRING_STRING d = this.SetText;
                this.Invoke(d, new object[] { real_title, display_title } );
                return;
            }

            this.Text = real_title;
            this._TitleText = display_title;
        }

        public void SetUpChatMode()
        {
            if (this.InvokeRequired)
            {
                Delegate__Void d = this.SetUpChatMode;
                this.Invoke(d);
                return;
            }

            this.SetText("EK Unleashed Chatter", "Chat Mode Only");
            this.RefreshWindow(); // required to re-paint the title bar

            this.MinimumSize = new Size(0, 0);

            this.tabsGameInfo.Visible = false;
            this.tabsChatChannels.Left = this.tabsGameInfo.Left;
            this.Width -= this.tabsGameInfo.Width + 6;
            this.tabsChatChannels.Width += this.tabsGameInfo.Width + 6;

            this.MinimumSize = this.Size;

            if (this.tabsChatChannels.TabPages.Contains(this.tabNotifications))
                this.tabsChatChannels.TabPages.Remove(this.tabNotifications);
            if (this.tabsChatChannels.TabPages.Contains(this.tabClan))
                this.tabsChatChannels.TabPages.Remove(this.tabClan);
            if (this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                    this.tabsChatChannels.TabPages.Remove(this.tabDebug);

            this.mnuChatChannels.Items.Remove(this.privateMessageToolStripMenuItem);
            this.mnuChatChannels.Items.Remove(this.chatchannelClan);

            this.btnChatChannel.Visible = false;

            this.mnuApplication.Items.RemoveAt(0);

            this.lblGameMenu.Visible = false;
            this.lblDebugMenu.Visible = false;
            this.lblBuyMenu.Visible = false;

            this.nameClicked.Items.Remove(this.mnuNameClicked_ShowInfo);
        }

        public void KWChatActivator(int force)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    Delegate__Void_Int d = this.KWChatActivator;
                    this.Invoke(d, new object[] { force });
                    return;
                }

                try
                {
                    // crashes if chat connection is lost
                    Game.Chat.LeaveChannel("Country_0");
                    Game.Chat.LeaveChannel("Country_1");
                    Game.Chat.LeaveChannel("Country_2");
                    Game.Chat.LeaveChannel("Country_3");
                    Game.Chat.LeaveChannel("Country_4");
                }
                catch { }

                try
                {
                    if (this.tabsChatChannels.TabPages.Contains(this.tabKWTundra)) this.tabsChatChannels.TabPages.Remove(this.tabKWTundra);
                    if (this.tabsChatChannels.TabPages.Contains(this.tabKWForest)) this.tabsChatChannels.TabPages.Remove(this.tabKWForest);
                    if (this.tabsChatChannels.TabPages.Contains(this.tabKWSwamp)) this.tabsChatChannels.TabPages.Remove(this.tabKWSwamp);
                    if (this.tabsChatChannels.TabPages.Contains(this.tabKWMountain)) this.tabsChatChannels.TabPages.Remove(this.tabKWMountain);
                }
                catch { }

                try
                {
                    if (force > 0)
                    {
                        if (force == 1) this.tabsChatChannels.TabPages.Insert(2, this.tabKWTundra);
                        if (force == 2) this.tabsChatChannels.TabPages.Insert(2, this.tabKWForest);
                        if (force == 3) this.tabsChatChannels.TabPages.Insert(2, this.tabKWSwamp);
                        if (force == 4) this.tabsChatChannels.TabPages.Insert(2, this.tabKWMountain);

                        // crashes if chat connection is lost
                        Game.Chat.JoinChannel("Country_" + force.ToString());
                    }
                }
                catch { }

                Game.Kingdom_War_ID = force.ToString();
            }
            catch { }
        }

        private void tabKWSwamp_Enter(object sender, EventArgs e)
        {
            if (this.LastTab != this.tabKWSwamp)
            {
                this.LastTab = this.tabKWSwamp;
                this.kingdomWarToolStripMenuItem_Click(null, null);
            }
        }

        private void kingdomWarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utils.CInt(Game.Kingdom_War_ID) > 0)
            {
                frmMain.ChatChannel = new string[] { "KW", "" };
                this.btnChatChannel.Text = GameClient.ConvertCardElementToText(Game.Kingdom_War_ID);
                this.RefocusInput();
            }
            else
            {
                this.tabsChatChannels.SelectedTab = this.tabGeneral;
            }
        }

        private void RefocusInput()
        {
            if (this.InvokeRequired)
            {
                Delegate__Void d = this.RefocusInput;
                this.Invoke(d);
                return;
            }

            try
            {
                this.txtChatMessage.Focus();
                this.txtChatMessage.Select(this.txtChatMessage.Text.Length, 0);
            }
            catch { }
        }

        private void tabsChatChannels_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.RefocusInput();
        }

        private void tabsChatChannels_Selected(object sender, TabControlEventArgs e)
        {
            this.RefocusInput();
        }

        private void btnRefreshStats_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(() =>
            {
                string s = Game.GetGameData("user", "GetUserInfo", "", false);
                Game.GameVitalsUpdate(s);
                Game.GameVitalsUpdate(s, Game.GetGameData("arena", "GetRankCompetitors", "", false), Game.GetGameData("arena", "GetThieves", "", false));
            });
        }

        private void reloadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;

            Game.ReloadSettings();

            if (GameClient.Want_Debug && !this.Controls.Contains(this.lblDebugMenu))
                this.Controls.Add(this.lblDebugMenu);
            if (!GameClient.Want_Debug && this.Controls.Contains(this.lblDebugMenu))
                this.Controls.Remove(this.lblDebugMenu);
            if (!GameClient.Want_Debug)
            {
                if (this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                    this.tabsChatChannels.TabPages.Remove(this.tabDebug);
            }
            else
                if (!this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                    this.tabsChatChannels.TabPages.Add(this.tabDebug);
        }
        
        public void RestartEKUnleashed()
        {
            if (this.InvokeRequired)
            {
                Delegate__Void d = RestartEKUnleashed;
                this.Invoke(d);
                return;
            }

            frmMain.Game.StopEverything();
            frmMain.Game.ReloadSettings();

            frmMain.Game = new GameClient();
            frmMain.Game.ParentForm = this;

            this.trayIcon.Text = "EK Unleashed";
            this.Text = "EK Unleashed";

            this.PersonClicked = null;
            frmMain.ChatChannel = new string[] { "W", "" };
            this.btnChatChannel.Text = "World";

            this.rchChatClan.Clear();
            this.rchChatClansOther.Clear();
            this.rchChatGeneral.Clear();
            this.rchChatPrivate.Clear();
            this.rchDebug.Clear();
            this.rchKWForest.Clear();
            this.rchKWMountain.Clear();
            this.rchKWSwamp.Clear();
            this.rchKWTundra.Clear();
            this.rchNotifications.Clear();
            this.rchOutput.Clear();

            this.lblArena.Text = "";
            this.lblArenaRank.Text = "";
            this.lblEmail.Text = "";
            this.lblEnergy.Text = "";
            this.lblFireTokens.Text = "";
            this.lblFriendRequest.Text = "";
            this.lblGems.Text = "";
            this.lblGold.Text = "";
            this.lblIndicators.Text = "";
            this.lblLastUpdated.Text = "";
            this.lblLevel.Text = "";
            this.lblLogin.Text = "";
            this.lblNickName.Text = "";
            this.lblThiefAvailable.Text = "";

            this.tabsGameInfo.SelectedTab = this.tabInfoMain;
            this.tabsChatChannels.SelectedTab = this.tabGeneral;

            if (this.tabsGameInfo.TabPages.Contains(this.tabInfoDemonInvasion))
                this.tabsGameInfo.TabPages.Remove(this.tabInfoDemonInvasion);

            this.LastTab = this.tabGeneral;

            if (GameClient.Want_Debug && !this.Controls.Contains(this.lblDebugMenu))
                this.Controls.Add(this.lblDebugMenu);
            if (!GameClient.Want_Debug && this.Controls.Contains(this.lblDebugMenu))
                this.Controls.Remove(this.lblDebugMenu);

            this.frmMain_Load(null, null);
            this.frmMain_Shown(null, null);
            return;
        }

        public void ShutdownEKUnleashed()
        {
            if (this.InvokeRequired)
            {
                Delegate__Void d = ShutdownEKUnleashed;
                this.Invoke(d);
                return;
            }

            frmMain.Game.StopEverything();
            try { this.Close(); } catch { }
            try { Application.ExitThread(); } catch { }
            try { Application.Exit(); } catch { }
            return;
        }

        private void restartEKUnleashedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.RestartEKUnleashed();
        }

        private void settingsPreferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSettings settings = new frmSettings();
            if (settings.ShowDialog() == DialogResult.OK)
            {
                this.tabsChatChannels.SelectedTab = this.tabNotifications;

                Game.ReloadSettings();

                if (GameClient.Want_Debug && !this.Controls.Contains(this.lblDebugMenu))
                    this.Controls.Add(this.lblDebugMenu);
                if (!GameClient.Want_Debug && this.Controls.Contains(this.lblDebugMenu))
                    this.Controls.Remove(this.lblDebugMenu);
                if (!GameClient.Want_Debug)
                {
                    if (this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                        this.tabsChatChannels.TabPages.Remove(this.tabDebug);
                }
                else
                    if (!this.tabsChatChannels.TabPages.Contains(this.tabDebug))
                        this.tabsChatChannels.TabPages.Add(this.tabDebug);
            }
        }

        private void fightRaidersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_FightRaider_Hydra);
        }

        private void fightDailyMapInvadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_FightMapInvasions);
        }

        private void viewAllCardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSelectCard card_selector = new frmSelectCard();
            card_selector.ManageMode();
            card_selector.StartPosition = FormStartPosition.Manual;
            card_selector.Location = new Point(this.Location.X + (this.Width - card_selector.Width) / 2, this.Location.Y + (this.Height - card_selector.Height) / 2);
            card_selector.Show();
        }

        private void manageDecksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSelectDeck deck_selector = new frmSelectDeck();
            deck_selector.ManageMode();
            deck_selector.StartPosition = FormStartPosition.Manual;
            deck_selector.Location = new Point(this.Location.X + (this.Width - deck_selector.Width) / 2, this.Location.Y + (this.Height - deck_selector.Height) / 2);
            deck_selector.Show();
            deck_selector.RefreshDecks();
        }

        private void useAllOfMyFreeMazeTowerResetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.Play_ResetAllTowersFree);
        }

        private void startFightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utils.StartMethodMultithreaded(Game.KingdomWar_WarBegins);
        }

        private void testKWLoginConditionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.LoggerNotifications("<color=#33aaff><b><u>Kingdom Wars auto-login test:</u></b></color>");
            Utils.LoggerNotifications("<color=#33aaff>... game service check: " + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "passed" : "failed") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... events automation setting: " + ((Utils.True("Game_Events")) ? "enabled" : "disabled") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... chat connected: " + ((GameClient.Current.ChatIsConnected) ? "connected" : "disconnected") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... Fri/Sat/Sun check: " + (((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday)) ? "passed" : "failed") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... Kingdom War automation setting: " + ((Utils.False("Game_FightKW")) ? "enabled" : "disabled") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>....... if tests above passed, settings are enabled, and chat is disconnected, then EKU should reconnect (not in test mode, though)</color>");
        }

        private void testKWAutostartConditionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.LoggerNotifications("<color=#33aaff><b><u>Kingdom Wars auto-start test:</u></b></color>");
            Utils.LoggerNotifications("<color=#33aaff>... game service check: " + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "passed" : "failed") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... events automation setting: " + ((Utils.True("Game_Events")) ? "enabled" : "disabled") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... Fri/Sat/Sun check: " + (((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday)) ? "passed" : "failed") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... Kingdom War automation setting: " + ((Utils.False("Game_FightKW")) ? "enabled" : "disabled") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>... Kingdom War already ongoing check: " + ((!GameClient.Current.KW_Ongoing) ? "passed" : "failed") + "</color>");
            Utils.LoggerNotifications("<color=#33aaff>....... if tests above passed and settings are enabled, then EKU should begin Kingdom War automation (not in test mode, though)</color>");
        }

        private void listEventsAllowedToTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;

            Dictionary<string,Scheduler.ScheduledEvent> all_events = new Dictionary<string,Scheduler.ScheduledEvent>(Game.ScheduledEvents);
            Utils.LoggerNotifications("<color=#33aaff><b><u>Events allowed to trigger:</u></b></color>");
            if (all_events.Count == 0)
                Utils.LoggerNotifications("<color=#33aaff>... none!</color>");
            else
            {
                foreach (KeyValuePair<string,Scheduler.ScheduledEvent> ev in all_events)
                    Utils.LoggerNotifications("<color=#33aaff>... <b>" + ev.Key + "</b> scheduled for <b>" + ev.Value.NextScheduled.ToString() + "</b></color>");
            }

            Utils.LoggerNotifications();

            List<string> allowed_events = new List<string>(Scheduler.ScheduledEvent.Allowed_EventIDs);
            Utils.LoggerNotifications("<color=#33aaff><b><u>Events allowed to trigger:</u></b></color>");
            if (allowed_events.Count == 0)
                Utils.LoggerNotifications("<color=#33aaff>... <u>none</u></color>");
            else if (Scheduler.ScheduledEvent.AllEventsAllowed)
                Utils.LoggerNotifications("<color=#33aaff>... <u>all</u></color>");
            else
            {
                foreach (string ev in allowed_events)
                    Utils.LoggerNotifications("<color=#33aaff>... " + ev + "</color>");
            }
        }

        private void enableAllEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.LoggerNotifications("<color=#33aaff>You manually enabled all events</color>");
            Scheduler.ScheduledEvent.AllEventsAllowed = true;
        }

        private void disableAllEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tabsChatChannels.SelectedTab = this.tabNotifications;
            Utils.LoggerNotifications("<color=#33aaff>You manually disabled all events</color>");
            Scheduler.ScheduledEvent.AllEventsAllowed = false;
        }

        private void openLogsAndDataFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("explorer.exe", "/e,\"" + Utils.AppFolder + "\",\"" + Utils.AppFolder + "\"");
                System.Diagnostics.Process.Start(psi);
            }
            catch { }
        }

        private void manageRunesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSelectRune rune_selector = new frmSelectRune();
            rune_selector.ManageMode();
            rune_selector.StartPosition = FormStartPosition.Manual;
            rune_selector.Location = new Point(this.Location.X + (this.Width - rune_selector.Width) / 2, this.Location.Y + (this.Height - rune_selector.Height) / 2);
            rune_selector.Show();
        }

        private void enchantARuneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string EnchantRune = Utils.Input_Text("Enchant Which Rune?", "Enter the name of a rune you want to enchant:");
            if (string.IsNullOrEmpty(EnchantRune))
                return;

            int EnchantLevel = Utils.CInt(Utils.Input_Text("To What Level?", "Enchant this rune to what level (1 - 4):"));
            if ((EnchantLevel < 1) || (EnchantLevel > 4))
                return;

            Utils.StartMethodMultithreaded(() =>
            {
                Game.EnchantRune(EnchantRune, EnchantLevel);
            });
        }

        private void claimAGiftCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string giftcode = Utils.Input_Text("Enter Gift Code", "Enter the gift code:");
            if (Utils.ValidText(giftcode))
            {
                this.tabsChatChannels.SelectedTab = this.tabNotifications;
                Utils.StartMethodMultithreaded(() => { Game.ClaimGiftCode(giftcode); }); 
            }
            return;
        }

        private void setTargetPrioritiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmKWPriority().Show();
        }

        private void fieldOfHonorSpinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.Play_FieldOfHonorSpins();
        }

        private void doDailyTasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.Play_DailyFreeCards();
            Game.Play_DailyTasks();
        }

    } // end: public partial class frmMain : Form

} // end: namespace EK Unleashed
