using System;
using System.Collections;
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
    public partial class frmSelectCard : Form
    {
        private static int iEvoMaxChoices = 8;

        public frmSelectCard()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        delegate void Void__Void();
        delegate void Void__ListCardData(List<GameObjs.Card> c, bool b);

        private void ClearList()
        {
            if (this.InvokeRequired)
            {
                Void__Void d = this.ClearList;
                this.Invoke(d);
                return;
            }

            this.lstCards.Items.Clear();
            return;
        }

        private void AddCards(List<GameObjs.Card> cards, bool want_evo_cards)
        {
            if (this.InvokeRequired)
            {
                Void__ListCardData d = this.AddCards;
                this.Invoke(d, new object[] { cards, want_evo_cards });
                return;
            }

            foreach (var card in cards)
            {
                ListViewItem lvi;

                if (!want_evo_cards)
                {
                    lvi = new ListViewItem(new string[] 
                    {
                        card.Name.ToString(),
                        (card.EvolvedTimes > 0) ? (card.Level.ToString() + "+" + card.EvolvedTimes.ToString()) : card.Level.ToString(),
                        (string.Empty).PadLeft(card.Stars, '★'),
                        card.Element,
                        card.Cost.ToString(),
                        card.Wait.ToString(),
                        card.Skill1,
                        card.Skill2,
                        card.Skill3,
                        card.EvolvedSkill,
                        card.SellWorth.ToString("#,##0"),
                    });
                }
                else
                {
                    lvi = new ListViewItem(new string[] 
                    {
                        card.Name.ToString(),
                        (card.EvolvedTimes > 0) ? (card.Level.ToString() + "+" + card.EvolvedTimes.ToString()) : card.Level.ToString(),
                        (string.Empty).PadLeft(card.Stars, '★'),
                        card.Element,
                        card.Cost.ToString(),
                        card.Wait.ToString(),
                        card.Skill1,
                        card.Skill2,
                        card.Skill3,
                        card.EvolvedSkill,
                        card.SellWorth.ToString("#,##0"),
                        card.EvoSkillChoice(1),
                        card.EvoSkillChoice(2),
                        card.EvoSkillChoice(3),
                        card.EvoSkillChoice(4),
                        card.EvoSkillChoice(5),
                        card.EvoSkillChoice(6),
                        card.EvoSkillChoice(7),
                        card.EvoSkillChoice(8),
                    });
                }

                lvi.Tag = card;

                this.lstCards.Items.Add(lvi);
            }
            return;
        }

        private void InitCards()
        {
            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            Utils.DontDrawControl(this.lstCards);

            bool want_evo_choices = this.chkEvoChoices.Checked;

            if ((this.lstCards.Columns.Count == 11) && (want_evo_choices))
            {
                for (int i = 1; i <= iEvoMaxChoices; i++)
                    this.lstCards.Columns.Add("Evo Choice #" + i.ToString(), 100);
            }
            else if ((this.lstCards.Columns.Count == 11 + iEvoMaxChoices) && (!want_evo_choices))
            {
                for (int i = 1; i <= iEvoMaxChoices; i++)
                    this.lstCards.Columns.RemoveAt(11);
            }

            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameObjs.Card.RefreshCardsInDeckCache();

                JObject cards = GameClient.Current.GetUsersCards();

                this.ClearList();

                List<GameObjs.Card> cardobjs = new List<GameObjs.Card>();

                foreach (var jCard in cards["data"]["Cards"])
                {
                    try
                    {
                        GameObjs.Card TempCard = new GameObjs.Card(jCard);

                        if (!TempCard.Valid) continue;
                        if (!Utils.ValidText(TempCard.Name)) continue;

                        if ((TempCard.Stars == 1) && (!this.chk1Star.Checked)) continue;
                        if ((TempCard.Stars == 2) && (!this.chk2Star.Checked)) continue;
                        if ((TempCard.Stars == 3) && (!this.chk3Star.Checked)) continue;
                        if ((TempCard.Stars == 4) && (!this.chk4Star.Checked)) continue;
                        if ((TempCard.Stars == 5) && (!this.chk5Star.Checked)) continue;

                        cardobjs.Add(TempCard);
                    }
                    catch (Exception ex)
                    {
                        Utils.Chatter(Errors.GetShortErrorDetails(ex));
                    }
                }

                this.AddCards(cardobjs, want_evo_choices);
            });

            this.lstCards.Sort();

            Utils.DrawControl(this.lstCards);

            this.Cursor = cur;
        }

        private void chk1Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitCards();
        }

        private void chk2Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitCards();
        }

        private void chk3Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitCards();
        }

        private void chk4Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitCards();
        }

        private void chk5Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitCards();
        }

        private void chkEvoChoices_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkEvoChoices.Checked)
            {
                if (MessageBox.Show("This could take a very long time.\r\n\r\nAre you sure you want evolution skill details checked for all of these cards?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                {
                    this.chkEvoChoices.Checked = false;
                    return;
                }
            }

            this.InitCards();
        }

        private void frmSelectCard_Shown(object sender, EventArgs e)
        {
            this.lstCards.ListViewItemSorter = new SpecialListViewItemComparer(0, this.lstCards.Sorting);

            this.InitCards();
        }

        private void lstCards_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SpecialListViewItemComparer current_sort = (SpecialListViewItemComparer)this.lstCards.ListViewItemSorter;

            if ((current_sort.SortedColumn != e.Column) || (current_sort.SortOrder == SortOrder.Descending))
                this.lstCards.ListViewItemSorter = new SpecialListViewItemComparer(e.Column, SortOrder.Ascending);
            else
                this.lstCards.ListViewItemSorter = new SpecialListViewItemComparer(e.Column, SortOrder.Descending);

            if (e.Column == this.colLevel.Index)
                ((SpecialListViewItemComparer)this.lstCards.ListViewItemSorter).ComparerType = SpecialListViewItemComparer.ComparerTypes.EK_Card_Level;
            else if ((e.Column == this.colCost.Index) || (e.Column == this.colWait.Index) || (e.Column == this.colValue.Index))
                ((SpecialListViewItemComparer)this.lstCards.ListViewItemSorter).ComparerType = SpecialListViewItemComparer.ComparerTypes.Standard_Numeric;

            this.lstCards.Sort();
        }

        class SpecialListViewItemComparer : IComparer
        {
            private int _SortedColumn;
            private SortOrder _SortOrder = SortOrder.None;
            private ComparerTypes _ComparerType = ComparerTypes.Standard_Text;

            public enum ComparerTypes
            {
                Standard_Text,
                Standard_Numeric,
                EK_Card_Level
            }

            public int SortedColumn
            {
                get
                {
                    return this._SortedColumn;
                }
                set
                {
                    this._SortedColumn = value;
                }
            }

            public SortOrder SortOrder
            {
                get
                {
                    return this._SortOrder;
                }
                set
                {
                    this._SortOrder = value;
                }
            }

            public ComparerTypes ComparerType
            {
                get
                {
                    return this._ComparerType;
                }
                set
                {
                    this._ComparerType = value;

                }
            }

            public SpecialListViewItemComparer(int c = 0, SortOrder so = System.Windows.Forms.SortOrder.Ascending, ComparerTypes ct = ComparerTypes.Standard_Text)
            {
                this._SortedColumn = c;
                this._SortOrder = so;
                this._ComparerType = ct;
            }

            public int Compare(Object o1, Object o2)
            {
                if ((o1.GetType() == typeof(ListViewItem)) && (o2.GetType() == typeof(ListViewItem)))
                {
                    ListViewItem lvi1 = (ListViewItem)o1;
                    ListViewItem lvi2 = (ListViewItem)o2;

                    return this.CompareLVIs(lvi1, lvi2);
                }

                return 0;
            }

            public int CompareLVIs(ListViewItem lvi1, ListViewItem lvi2)
            {
                if (this._SortOrder == System.Windows.Forms.SortOrder.None) return 0;

                int sort_result = -1;
                GameObjs.Card card1 = (GameObjs.Card)lvi1.Tag;
                GameObjs.Card card2 = (GameObjs.Card)lvi2.Tag;

                if (this._ComparerType == ComparerTypes.EK_Card_Level)
                {
                    if (card1.Level == card2.Level)
                        sort_result = card1.EvolvedTimes.CompareTo(card2.EvolvedTimes);
                    else
                        sort_result = card1.Level.CompareTo(card2.Level);
                }
                else if (this._ComparerType == ComparerTypes.Standard_Numeric)
                    sort_result = Utils.CInt(lvi1.SubItems[_SortedColumn].Text).CompareTo(Utils.CInt(lvi2.SubItems[_SortedColumn].Text));
                else
                    sort_result = string.Compare(lvi1.SubItems[_SortedColumn].Text, lvi2.SubItems[_SortedColumn].Text);

                return (this._SortOrder == System.Windows.Forms.SortOrder.Ascending) ? sort_result : -sort_result;
            }
        }

        public void ManageMode()
        {
            this.btnCancel.Visible = false;
            this.btnSelect.Text = "Done";
            this.Text = "EK Unleashed :: Manage your card collection";
        }

        class ListViewItemComparer : IComparer
        {
            public int SortedColumn = 0;
            public SortOrder SortOrder = SortOrder.Ascending;
            
            public ListViewItemComparer(int column) 
            {
                this.SortedColumn = column;
            }
            
            public ListViewItemComparer(int column, SortOrder s) 
            {
                this.SortedColumn = column;
                this.SortOrder = s;
            }

            public int Compare(object x, object y) 
            {
                int returnVal = -1;

                try
                {
                    string x_text = ((ListViewItem)x).SubItems[this.SortedColumn].Text;
                    string y_text = ((ListViewItem)y).SubItems[this.SortedColumn].Text;

                    if (x_text != "" && char.IsDigit(x_text[0]) && y_text != "" && char.IsDigit(y_text[0]))
                    {
                        int x_num = Utils.CInt(x_text);
                        int y_num = Utils.CInt(y_text);
                        returnVal = (x_num == y_num) ? 0 : (x_num > y_num) ? 1 : -1;
                    }
                    else
                        returnVal = string.Compare(x_text, y_text);

                    if ((returnVal != 0) && (this.SortOrder == SortOrder.Descending))
                        returnVal = -returnVal;
                }
                catch { }

                return returnVal;
            }
        }

        private void lstCards_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.lstCards.SelectedItems.Count <= 0)
                return;

            try
            {
                GameObjs.Card card = (GameObjs.Card)this.lstCards.SelectedItems[0].Tag;

                frmMain.ext().PopupCardPreviewWindow(card.ID_Generic, card.Level, Cursor.Position, "", 0, 0, 0, 0, null, null, null, null, card.EvolvedTimes, card.EvolvedSkillID);
            }
            catch { }
        }

        private void mnuCardRightClick_Opening(object sender, CancelEventArgs e)
        {
            if (this.AllSelectedItems.Count <= 0)
            {
                e.Cancel = true;
                return;
            }

            if (this.AllSelectedItems.Count == 1)
            {
                this.previewToolStripMenuItem.Visible = true;
                this.toolStripMenuItem2.Visible = true;
                this.enchantToolStripMenuItem.Visible = true;
                return;
            }

            this.previewToolStripMenuItem.Visible = false;
            this.toolStripMenuItem2.Visible = false;
            this.enchantToolStripMenuItem.Visible = false;
        }

        private List<ListViewItem> AllSelectedItems
        {
            get
            {
                List<ListViewItem> items = new List<ListViewItem>();

                foreach (ListViewItem lvi in this.lstCards.SelectedItems)
                    items.Add(lvi);
                foreach (ListViewItem lvi in this.lstCards.CheckedItems)
                    if (!items.Contains(lvi))
                        items.Add(lvi);

                return items;
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstCards.SelectedItems.Count <= 0)
                return;

            try
            {
                GameObjs.Card card = (GameObjs.Card)this.lstCards.SelectedItems[0].Tag;

                frmMain.ext().PopupCardPreviewWindow(card.ID_Generic, card.Level, Cursor.Position, "", 0, 0, 0, 0, null, null, null, null, card.EvolvedTimes, card.EvolvedSkillID);
            }
            catch { }
        }

        private void enchantToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            if (this.lstCards.SelectedItems.Count <= 0)
                return;

            int EnchantLevel = Utils.CInt(Utils.Input_Text("To What Level?", "Enchant this card to what level (1 - 10 or 1 - 15):"));
            if ((EnchantLevel < 1) || (EnchantLevel > 15))
                return;

            try
            {
                GameObjs.Card card = (GameObjs.Card)this.lstCards.SelectedItems[0].Tag;

                Utils.StartMethodMultithreaded(() =>
                {
                    GameClient.Current.EnchantCard(card.Name, EnchantLevel, card.ID_User);
                });
            }
            catch { }
        }
        
        private void showEvolutionSkillsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = this.AllSelectedItems;

            if (items.Count <= 0)
                return;

            try
            {
                foreach (var lvi in items)
                {
                    GameObjs.Card card = (GameObjs.Card)lvi.Tag;

                    if (this.lstCards.Columns.Count == 11)
                    {
                        for (int i = 1; i <= iEvoMaxChoices; i++)
                        {
                            this.lstCards.Columns.Add("Evo Choice #" + i.ToString(), 100);
                            this.lstCards.Items[lvi.Index].SubItems.Add(card.EvoSkillChoice(i));
                        }
                    }
                    else
                    {
                        if (this.lstCards.Items[lvi.Index].SubItems.Count == 11)
                        {
                            for (int i = 1; i <= iEvoMaxChoices; i++)
                                this.lstCards.Items[lvi.Index].SubItems.Add(card.EvoSkillChoice(i));
                        }
                        else
                        {
                            for (int i = 1; i <= iEvoMaxChoices; i++)
                                this.lstCards.Items[lvi.Index].SubItems[10 + i].Text = card.EvoSkillChoice(i);
                        }
                    }
                }
            }
            catch { }
        }

        private void evolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstCards.SelectedItems.Count <= 0)
                return;


            try
            {

            }
            catch { }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() => { GameClient.Current.UserCards_CachedData = null; });
            this.Cursor = cur;
            this.InitCards();
        }

        private void sellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = this.AllSelectedItems;

            if (items.Count <= 0)
                return;

            if (MessageBox.Show("Do you really want to sell " + items.Count.ToString("#,##0") + " " + Utils.PluralWord(items.Count, "card", "cards") + "?", "Confirm Card Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                return;

            string cards_to_sell = "";
            int gold_amount = 0;
            int gold_cards_sold = items.Count;

            foreach (ListViewItem lvi in items)
            {
                GameObjs.Card card = (GameObjs.Card)lvi.Tag;

                cards_to_sell += card.ID_User.ToString() + "_";
                gold_amount += card.SellWorth;
            }

            cards_to_sell = cards_to_sell.Trim(new char[] { '_' });

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Cards=" + cards_to_sell);
                GameClient.Current.GameVitalsUpdate(GameClient.Current.GetGameData("user", "GetUserInfo"));
                GameClient.Current.UserCards_CachedData = null;
                Utils.LoggerNotifications("<color=#ffa000>Sold " + gold_cards_sold.ToString("#,##0") + " " + Utils.PluralWord(gold_cards_sold, "card", "cards") + " for " + gold_amount.ToString("#,##0") + " gold.</color>");
            });
            this.Cursor = cur;
            this.InitCards();
        }

        private void exportAllCardsToTextFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string output_txt = "";
            string output_csv = "";

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameObjs.Card.RefreshCardsInDeckCache();

                JObject cards = GameClient.Current.GetUsersCards();

                output_txt += String.Format("{0, -25}  {1, -5}  {2, -12}  {3, -5}  {4, -20}  {5, -14}  {6, -14}\r\n", "Card Name", "Stars", "Element", "Level", "Evolved Skill", "Card ID", "Unique Card ID");
                output_txt += String.Format("{0, -25}  {1, -5}  {2, -12}  {3, -5}  {4, -20}  {5, -14}  {6, -14}\r\n", "-------------------------", "-----", "------------", "-----", "--------------------", "--------------", "--------------");
                output_csv += String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"\r\n", "Card Name", "Stars", "Element", "Level", "Evolved Skill", "Card ID", "Unique Card ID");

                foreach (var jCard in cards["data"]["Cards"])
                {
                    try
                    {
                        GameObjs.Card TempCard = new GameObjs.Card(jCard);

                        if (!TempCard.Valid) continue;
                        if (!Utils.ValidText(TempCard.Name)) continue;

                        output_txt += String.Format("{0, -25}  {1, -5}  {2, -12}  {3, -5}  {4, -20}  {5, -14}  {6, -14}\r\n", TempCard.Name, TempCard.Stars.ToString(), TempCard.ElementType.ToString(), TempCard.Level.ToString(), TempCard.EvolvedSkill, TempCard.ID_Generic.ToString(), TempCard.ID_User.ToString());
                        output_csv += String.Format("\"{0}\",{1},\"{2}\",{3},\"{4}\",{5},{6}\r\n", TempCard.Name, TempCard.Stars.ToString(), TempCard.ElementType.ToString(), TempCard.Level.ToString(), TempCard.EvolvedSkill, TempCard.ID_Generic.ToString(), TempCard.ID_User.ToString());

                    }
                    catch { }
                }

                Utils.FileOverwrite(Utils.AppFolder + @"\\Game Data\" + GameClient.GameAbbreviation(GameClient.Current.Service) + " card collection (" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".") + ").txt", output_txt);
                Utils.FileOverwrite(Utils.AppFolder + @"\\Game Data\" + GameClient.GameAbbreviation(GameClient.Current.Service) + " card collection (" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".") + ").csv", output_csv);
            });

            this.Cursor = cur;

            MessageBox.Show("Done exporting!", "Finished", MessageBoxButtons.OK);
        }

        private void sellAll1CardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameObjs.Card.RefreshCardsInDeckCache();

            JObject cards = GameClient.Current.GetUsersCards();

            List<GameObjs.Card> items = new List<GameObjs.Card>();

            foreach (var jCard in cards["data"]["Cards"])
            {
                try
                {
                    GameObjs.Card TempCard = new GameObjs.Card(jCard);

                    if (!TempCard.Valid) continue;
                    if (!Utils.ValidText(TempCard.Name)) continue;

                    if (TempCard.Stars != 1) continue;
                    if (TempCard.Level != 0) continue;
                    if (TempCard.InAnyDeck) continue;
                    if (TempCard.Locked) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Food) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Activity) continue;

                    items.Add(TempCard);
                }
                catch { }
            }

            if (items.Count == 0)
            {
                MessageBox.Show("You don't have any 1★ cards to sell.", "No 1★ Cards", MessageBoxButtons.OK);
                return;
            }

            if (MessageBox.Show("Do you really want to sell " + items.Count.ToString("#,##0") + " " + Utils.PluralWord(items.Count, "card", "cards") + "?", "Confirm Card Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                return;

            string cards_to_sell = "";
            int gold_amount = 0;
            int gold_cards_sold = items.Count;

            foreach (GameObjs.Card card in items)
            {
                cards_to_sell += card.ID_User.ToString() + "_";
                gold_amount += card.SellWorth;
            }

            cards_to_sell = cards_to_sell.Trim(new char[] { '_' });

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Cards=" + cards_to_sell);
                GameClient.Current.GameVitalsUpdate(GameClient.Current.GetGameData("user", "GetUserInfo"));
                GameClient.Current.UserCards_CachedData = null;
                Utils.LoggerNotifications("<color=#ffa000>Sold " + gold_cards_sold.ToString("#,##0") + " " + Utils.PluralWord(gold_cards_sold, "card", "cards") + " for " + gold_amount.ToString("#,##0") + " gold.</color>");
            });

            this.Cursor = cur;
        }

        private void sellAll2CardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameObjs.Card.RefreshCardsInDeckCache();

            JObject cards = GameClient.Current.GetUsersCards();

            List<GameObjs.Card> items = new List<GameObjs.Card>();

            foreach (var jCard in cards["data"]["Cards"])
            {
                try
                {
                    GameObjs.Card TempCard = new GameObjs.Card(jCard);

                    if (!TempCard.Valid) continue;
                    if (!Utils.ValidText(TempCard.Name)) continue;

                    if (TempCard.Stars != 2) continue;
                    if (TempCard.Level != 0) continue;
                    if (TempCard.InAnyDeck) continue;
                    if (TempCard.Locked) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Food) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Activity) continue;

                    items.Add(TempCard);
                }
                catch { }
            }

            if (items.Count == 0)
            {
                MessageBox.Show("You don't have any 2★ cards to sell.", "No 2★ Cards", MessageBoxButtons.OK);
                return;
            }

            if (MessageBox.Show("Do you really want to sell " + items.Count.ToString("#,##0") + " " + Utils.PluralWord(items.Count, "card", "cards") + "?", "Confirm Card Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                return;

            string cards_to_sell = "";
            int gold_amount = 0;
            int gold_cards_sold = items.Count;

            foreach (GameObjs.Card card in items)
            {
                cards_to_sell += card.ID_User.ToString() + "_";
                gold_amount += card.SellWorth;
            }

            cards_to_sell = cards_to_sell.Trim(new char[] { '_' });

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Cards=" + cards_to_sell);
                GameClient.Current.GameVitalsUpdate(GameClient.Current.GetGameData("user", "GetUserInfo"));
                GameClient.Current.UserCards_CachedData = null;
                Utils.LoggerNotifications("<color=#ffa000>Sold " + gold_cards_sold.ToString("#,##0") + " " + Utils.PluralWord(gold_cards_sold, "card", "cards") + " for " + gold_amount.ToString("#,##0") + " gold.</color>");
            });

            this.Cursor = cur;
        }

        private void sellAll3CardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameObjs.Card.RefreshCardsInDeckCache();

            JObject cards = GameClient.Current.GetUsersCards();

            List<GameObjs.Card> items = new List<GameObjs.Card>();

            foreach (var jCard in cards["data"]["Cards"])
            {
                try
                {
                    GameObjs.Card TempCard = new GameObjs.Card(jCard);

                    if (!TempCard.Valid) continue;
                    if (!Utils.ValidText(TempCard.Name)) continue;

                    if (TempCard.Stars != 3) continue;
                    if (TempCard.Level != 0) continue;
                    if (TempCard.InAnyDeck) continue;
                    if (TempCard.Locked) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Food) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Activity) continue;

                    items.Add(TempCard);
                }
                catch { }
            }

            if (items.Count == 0)
            {
                MessageBox.Show("You don't have any 3★ cards to sell.", "No 3★ Cards", MessageBoxButtons.OK);
                return;
            }

            if (MessageBox.Show("Do you really want to sell " + items.Count.ToString("#,##0") + " " + Utils.PluralWord(items.Count, "card", "cards") + "?", "Confirm Card Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                return;

            string cards_to_sell = "";
            int gold_amount = 0;
            int gold_cards_sold = items.Count;

            foreach (GameObjs.Card card in items)
            {
                cards_to_sell += card.ID_User.ToString() + "_";
                gold_amount += card.SellWorth;
            }

            cards_to_sell = cards_to_sell.Trim(new char[] { '_' });

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Cards=" + cards_to_sell);
                GameClient.Current.GameVitalsUpdate(GameClient.Current.GetGameData("user", "GetUserInfo"));
                GameClient.Current.UserCards_CachedData = null;
                Utils.LoggerNotifications("<color=#ffa000>Sold " + gold_cards_sold.ToString("#,##0") + " " + Utils.PluralWord(gold_cards_sold, "card", "cards") + " for " + gold_amount.ToString("#,##0") + " gold.</color>");
            });

            this.Cursor = cur;
        }

        private void sellAllTreasureCardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameObjs.Card.RefreshCardsInDeckCache();

            JObject cards = GameClient.Current.GetUsersCards();

            List<GameObjs.Card> items = new List<GameObjs.Card>();

            foreach (var jCard in cards["data"]["Cards"])
            {
                try
                {
                    GameObjs.Card TempCard = new GameObjs.Card(jCard);

                    if (!TempCard.Valid) continue;
                    if (!Utils.ValidText(TempCard.Name)) continue;

                    if (TempCard.ElementType != GameObjs.Card.ElementTypes.Treasure) continue;
                    if (TempCard.Level != 0) continue;
                    if (TempCard.InAnyDeck) continue;
                    if (TempCard.Locked) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Food) continue;
                    if (TempCard.ElementType == GameObjs.Card.ElementTypes.Activity) continue;

                    items.Add(TempCard);
                }
                catch { }
            }

            if (items.Count == 0)
            {
                MessageBox.Show("You don't have any treasure cards to sell.", "No Treasure Cards", MessageBoxButtons.OK);
                return;
            }

            if (MessageBox.Show("Do you really want to sell " + items.Count.ToString("#,##0") + " " + Utils.PluralWord(items.Count, "card", "cards") + "?", "Confirm Card Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                return;

            string cards_to_sell = "";
            int gold_amount = 0;
            int gold_cards_sold = items.Count;

            foreach (GameObjs.Card card in items)
            {
                cards_to_sell += card.ID_User.ToString() + "_";
                gold_amount += card.SellWorth;
            }

            cards_to_sell = cards_to_sell.Trim(new char[] { '_' });

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Cards=" + cards_to_sell);
                GameClient.Current.GameVitalsUpdate(GameClient.Current.GetGameData("user", "GetUserInfo"));
                GameClient.Current.UserCards_CachedData = null;
                Utils.LoggerNotifications("<color=#ffa000>Sold " + gold_cards_sold.ToString("#,##0") + " " + Utils.PluralWord(gold_cards_sold, "card", "cards") + " for " + gold_amount.ToString("#,##0") + " gold.</color>");
            });

            this.Cursor = cur;
        }
    }
}
