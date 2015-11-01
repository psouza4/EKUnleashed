using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace EKUnleashed
{
    public partial class frmSelectDeck : Form
    {
        public frmSelectDeck()
        {
            InitializeComponent();
        }

        delegate void Delegate__Void_String_String_String_String(string o1, string o2, string o3, string o4);

        public void AddDeck(string ordinal, string group_id, string cards, string runes)
        {
            if (this.lstDecks.InvokeRequired)
            {
                Delegate__Void_String_String_String_String d = AddDeck;
                this.lstDecks.Invoke(d, new object[] { ordinal, group_id, cards, runes });
                return;
            }

            try
            {
                ListViewItem lvi;
                if (Utils.CInt(group_id) == 0)
                    lvi = new ListViewItem(new string[] { ordinal, group_id, cards, runes });
                else
                    lvi = new ListViewItem(new string[] { (ordinal == "KW") ? "Kingdom War" : "Deck " + ordinal, group_id, cards, runes });
                lvi.Tag = group_id;
                this.lstDecks.Items.Add(lvi);
            }
            catch { }
        }

        public void RefreshDecks()
        {
            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.lstDecks.Items.Clear();

            if (!this.InManageMode)
                this.AddDeck("None", "", "", "");

            //Utils.StartMethodMultithreadedAndWait(() =>
            //{
                string json = GameClient.Current.GetGameData(ref GameClient.Current.opts, "card", "GetCardGroup", false);
                GameClient.Current.GetUsersCards();
                GameClient.Current.GetUsersRunes();

                JObject decks = JObject.Parse(json);

                for (int iPass = 0; iPass <= 1; iPass++)
                {
                    int iAbsoluteDeckOrdinal = 0;
                    int iAdjustedDeckNumber = 0;

                    foreach (JObject deck in decks["data"]["Groups"])
                    {
                        iAbsoluteDeckOrdinal++;
                        if (Utils.CInt(decks["data"]["legionWarGroupId"]) != Utils.CInt(deck["GroupId"]))
                            iAdjustedDeckNumber++;
                        string deck_EKUnleashed_ID = iAdjustedDeckNumber.ToString();
                        string deck_name = iAdjustedDeckNumber.ToString() + ". Deck";
                        if (Utils.CInt(decks["data"]["legionWarGroupId"]) == Utils.CInt(deck["GroupId"]))
                        {
                            deck_EKUnleashed_ID = "KW";
                            deck_name = "Kingdom War Deck";
                        }

                        if ((iPass == 0 && deck_EKUnleashed_ID == "KW") || (iPass == 1 && deck_EKUnleashed_ID != "KW"))
                        {
                            string pretty_cards_used = "";
                            //foreach (string unique_user_card_id in Utils.SubStrings(deck["UserCardIds"].ToString(), "_"))
                            foreach (JObject card in deck["UserCardInfo"])
                            {
                                try
                                {
                                    if (Utils.CInt(card["CardId"]) > 0)
                                        pretty_cards_used += ", " + GameClient.Current.ShortCardInfo(Utils.CInt(card["CardId"]), Utils.CInt(card["Level"]), true);
                                }
                                catch { }
                            }
                            pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                            string pretty_runes_used = "";
                            try
                            {
                                foreach (JObject rune in deck["UserRuneInfo"])
                                {
                                    try
                                    {
                                        if (Utils.CInt(rune["RuneId"]) > 0)
                                            pretty_runes_used += ", " + GameClient.Current.ShortRuneInfo(Utils.CInt(rune["RuneId"]), Utils.CInt(rune["Level"]), true);
                                    }
                                    catch { }
                                }
                            }
                            catch { }

                            //foreach (string unique_user_rune_id in Utils.SubStrings(deck["UserRuneIds"].ToString(), "_"))
                            //    pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id, true);
                            pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                            this.AddDeck((iPass == 0) ? "KW" : iAdjustedDeckNumber.ToString(), deck["GroupId"].ToString(), pretty_cards_used, pretty_runes_used);
                        }
                    }
                }
            //}, 15); // wait up to 15 seconds

            this.Cursor = cur;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void lstDecks_DoubleClick(object sender, EventArgs e)
        {
            if (this.InManageMode)
                return;

            if (this.lstDecks.SelectedItems.Count <= 0)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public string SelectedDeckName
        {
            get
            {
                if (this.lstDecks.SelectedItems.Count > 0)
                    return this.lstDecks.SelectedItems[0].SubItems[0].Text;

                return "";
            }
        }

        public string SelectedDeckID
        {
            get
            {
                if (this.lstDecks.SelectedItems.Count > 0)
                    return this.lstDecks.SelectedItems[0].Tag.ToString();

                return "0";
            }
        }
        
        private bool InManageMode = false;

        public void ManageMode()
        {
            this.btnCancel.Visible = false;
            this.btnSelect.Text = "Done";
            this.Text = "EK Unleashed :: Manage your decks";
            this.InManageMode = true;
        }

        private string DeckCopy_GroupID = string.Empty;
        private string DeckSwap_GroupID = string.Empty;

        private void mnuDeckRightClick_Opening(object sender, CancelEventArgs e)
        {
            if (this.lstDecks.SelectedItems.Count < 1)
            {
                e.Cancel = true;
                return;
            }

            if (Utils.CInt(this.lstDecks.SelectedItems[0].Tag) == 0)
            {
                e.Cancel = true;
                return;
            }

            this.pasteToolStripMenuItem.Visible = !string.IsNullOrEmpty(this.DeckCopy_GroupID) && (this.DeckCopy_GroupID != this.lstDecks.SelectedItems[0].Tag.ToString());
            this.finishSwapToolStripMenuItem.Visible = !string.IsNullOrEmpty(this.DeckSwap_GroupID) && (this.DeckSwap_GroupID != this.lstDecks.SelectedItems[0].Tag.ToString());
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstDecks.SelectedItems.Count < 1)
                return;

            this.DeckCopy_GroupID = this.lstDecks.SelectedItems[0].Tag.ToString();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstDecks.SelectedItems.Count < 1)
                return;

            if ((this.DeckCopy_GroupID != this.lstDecks.SelectedItems[0].Tag.ToString()) && (Utils.CInt(this.lstDecks.SelectedItems[0].Tag) > 0))
            {
                string GroupID_CopyTo = this.lstDecks.SelectedItems[0].Tag.ToString();

                Utils.StartMethodMultithreadedAndWait(() =>
                {
                    JObject decks = JObject.Parse(GameClient.Current.GetGameData(ref GameClient.Current.opts, "card", "GetCardGroup", false));

                    string cards_deck_from = string.Empty;
                    string runes_deck_from = string.Empty;

                    foreach (JObject deck in decks["data"]["Groups"])
                    {
                        if (Utils.CInt(deck["GroupId"]) == Utils.CInt(this.DeckCopy_GroupID))
                        {
                            try
                            {
                                cards_deck_from = deck["UserCardIds"].ToString();
                                runes_deck_from = deck["UserRuneIds"].ToString();
                                break;
                            }
                            catch { }
                        }
                    }

                    JObject.Parse(GameClient.Current.GetGameData("card", "SetCardGroup", "Cards=" + cards_deck_from + "&GroupId=" + GroupID_CopyTo + "&Runes=" + runes_deck_from));
                });

                this.RefreshDecks();
            }
        }

        private void swapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstDecks.SelectedItems.Count < 1)
                return;

            this.DeckSwap_GroupID = this.lstDecks.SelectedItems[0].Tag.ToString();
        }

        private void finishSwapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstDecks.SelectedItems.Count < 1)
                return;

            if ((this.DeckSwap_GroupID != this.lstDecks.SelectedItems[0].Tag.ToString()) && (Utils.CInt(this.lstDecks.SelectedItems[0].Tag) > 0))
            {
                string GroupID_SwapTo = this.lstDecks.SelectedItems[0].Tag.ToString();
                
                Cursor cur = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                Utils.StartMethodMultithreadedAndWait(() =>
                {
                    JObject decks = JObject.Parse(GameClient.Current.GetGameData(ref GameClient.Current.opts, "card", "GetCardGroup", false));

                    string cards_deck_from = string.Empty;
                    string runes_deck_from = string.Empty;
                    string cards_deck_to = string.Empty;
                    string runes_deck_to = string.Empty;

                    foreach (JObject deck in decks["data"]["Groups"])
                    {
                        if (Utils.CInt(deck["GroupId"]) == Utils.CInt(this.DeckSwap_GroupID))
                        {
                            try
                            {
                                cards_deck_from = deck["UserCardIds"].ToString();
                                runes_deck_from = deck["UserRuneIds"].ToString();
                            }
                            catch { }
                        }

                        if (Utils.CInt(deck["GroupId"]) == Utils.CInt(GroupID_SwapTo))
                        {
                            try
                            {
                                cards_deck_to = deck["UserCardIds"].ToString();
                                runes_deck_to = deck["UserRuneIds"].ToString();
                            }
                            catch { }
                        }
                    }

                    JObject.Parse(GameClient.Current.GetGameData("card", "SetCardGroup", "Cards=" + cards_deck_from + "&GroupId=" + GroupID_SwapTo + "&Runes=" + runes_deck_from));
                    JObject.Parse(GameClient.Current.GetGameData("card", "SetCardGroup", "Cards=" + cards_deck_to + "&GroupId=" + this.DeckSwap_GroupID + "&Runes=" + runes_deck_to));
                });

                this.Cursor = cur;

                this.RefreshDecks();

                this.DeckSwap_GroupID = string.Empty;
            }
        }

        private void fillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstDecks.SelectedItems.Count < 1)
                return;

            if (GameClient.Current.Want_Game_Login)
            {
                Cursor cur = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                bool bFilled = false;

                string DeckFill_GroupID = this.lstDecks.SelectedItems[0].Tag.ToString();

                Utils.StartMethodMultithreadedAndWait(() =>
                {
                    string temp_cards_entered = Utils.Input_Text("Update Deck", "Enter a list of cards you want to use (separated by commas):").Trim();

                    if (!string.IsNullOrEmpty(temp_cards_entered))
                    {
                        string temp_runes_entered = Utils.Input_Text("Update Deck", "Enter a list of runes you want to use (separated by commas)").Trim();

                        bFilled = GameClient.Current.FillDeckCustom_DeckID(DeckFill_GroupID, temp_cards_entered, temp_runes_entered);
                    }
                });

                this.Cursor = cur;

                if (bFilled)
                    this.RefreshDecks();
            }
        }
    }
}
