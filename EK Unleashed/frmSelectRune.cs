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
    public partial class frmSelectRune : Form
    {
        public frmSelectRune()
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
        delegate void Void__ListRuneData(List<GameObjs.Rune> r);

        private void ClearList()
        {
            if (this.InvokeRequired)
            {
                Void__Void d = this.ClearList;
                this.Invoke(d);
                return;
            }

            this.lstRunes.Items.Clear();
            return;
        }

        private void AddRunes(List<GameObjs.Rune> Runes)
        {
            if (this.InvokeRequired)
            {
                Void__ListRuneData d = this.AddRunes;
                this.Invoke(d, new object[] { Runes });
                return;
            }

            foreach (var rune in Runes)
            {
                ListViewItem lvi = new ListViewItem(new string[] 
                {
                    rune.Name.ToString(),
                    rune.Level.ToString(),
                    (string.Empty).PadLeft(rune.Stars, '★'),
                    rune.Element,
                    rune.Skill,
                    rune.SellWorth.ToString("#,##0"),
                });
                lvi.Tag = rune;

                this.lstRunes.Items.Add(lvi);
            }
            return;
        }

        private void InitRunes()
        {
            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            Utils.DontDrawControl(this.lstRunes);

            Utils.StartMethodMultithreadedAndWait(() =>
            {
                JObject Runes = GameClient.Current.GetUsersRunes();

                this.ClearList();

                List<GameObjs.Rune> runeobjs = new List<GameObjs.Rune>();

                foreach (var jRune in Runes["data"]["Runes"])
                {
                    try
                    {
                        GameObjs.Rune TempRune = new GameObjs.Rune(jRune);

                        if (!TempRune.Valid) continue;
                        if (!Utils.ValidText(TempRune.Name)) continue;

                        if ((TempRune.Stars == 1) && (!this.chk1Star.Checked)) continue;
                        if ((TempRune.Stars == 2) && (!this.chk2Star.Checked)) continue;
                        if ((TempRune.Stars == 3) && (!this.chk3Star.Checked)) continue;
                        if ((TempRune.Stars == 4) && (!this.chk4Star.Checked)) continue;
                        if ((TempRune.Stars == 5) && (!this.chk5Star.Checked)) continue;

                        runeobjs.Add(TempRune);
                    }
                    catch (Exception ex)
                    {
                        Utils.Chatter(Errors.GetShortErrorDetails(ex));
                    }
                }

                this.AddRunes(runeobjs);
            });

            this.lstRunes.Sort();

            Utils.DrawControl(this.lstRunes);

            this.Cursor = cur;
        }
        
        private void chk1Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitRunes();
        }

        private void chk2Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitRunes();
        }

        private void chk3Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitRunes();
        }

        private void chk4Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitRunes();
        }

        private void chk5Star_CheckedChanged(object sender, EventArgs e)
        {
            this.InitRunes();
        }

        private void frmSelectRune_Shown(object sender, EventArgs e)
        {
            this.lstRunes.ListViewItemSorter = new SpecialListViewItemComparer(0, this.lstRunes.Sorting);

            this.InitRunes();
        }

        private void lstRunes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SpecialListViewItemComparer current_sort = (SpecialListViewItemComparer)this.lstRunes.ListViewItemSorter;

            if ((current_sort.SortedColumn != e.Column) || (current_sort.SortOrder == SortOrder.Descending))
                this.lstRunes.ListViewItemSorter = new SpecialListViewItemComparer(e.Column, SortOrder.Ascending);
            else
                this.lstRunes.ListViewItemSorter = new SpecialListViewItemComparer(e.Column, SortOrder.Descending);

            if (e.Column == this.colLevel.Index)
                ((SpecialListViewItemComparer)this.lstRunes.ListViewItemSorter).ComparerType = SpecialListViewItemComparer.ComparerTypes.EK_Rune_Level;
            else if (e.Column == this.colValue.Index)
                ((SpecialListViewItemComparer)this.lstRunes.ListViewItemSorter).ComparerType = SpecialListViewItemComparer.ComparerTypes.Standard_Numeric;

            this.lstRunes.Sort();
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
                EK_Rune_Level
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
                GameObjs.Rune Rune1 = (GameObjs.Rune)lvi1.Tag;
                GameObjs.Rune Rune2 = (GameObjs.Rune)lvi2.Tag;

                if (this._ComparerType == ComparerTypes.EK_Rune_Level)
                    sort_result = Rune1.Level.CompareTo(Rune2.Level);
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
            this.Text = "EK Unleashed :: Manage your rune collection";
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

        private void lstRunes_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.lstRunes.SelectedItems.Count <= 0)
                return;

            try
            {
                GameObjs.Rune Rune = (GameObjs.Rune)this.lstRunes.SelectedItems[0].Tag;

                frmMain.ext().PopupRunePreviewWindow(Rune.ID_Generic, Rune.Level, Cursor.Position);
            }
            catch { }
        }

        private void mnuRuneRightClick_Opening(object sender, CancelEventArgs e)
        {
            if (this.AllSelectedItems.Count <= 0)
            {
                e.Cancel = true;
                return;
            }

            if (this.AllSelectedItems.Count == 1)
            {
                this.previewToolStripMenuItem.Visible = true;
                this.enchantToolStripMenuItem.Visible = true;
                return;
            }

            this.previewToolStripMenuItem.Visible = false;
            this.enchantToolStripMenuItem.Visible = false;
        }

        private List<ListViewItem> AllSelectedItems
        {
            get
            {
                List<ListViewItem> items = new List<ListViewItem>();

                foreach (ListViewItem lvi in this.lstRunes.SelectedItems)
                    items.Add(lvi);
                foreach (ListViewItem lvi in this.lstRunes.CheckedItems)
                    if (!items.Contains(lvi))
                        items.Add(lvi);

                return items;
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstRunes.SelectedItems.Count <= 0)
                return;

            try
            {
                GameObjs.Rune Rune = (GameObjs.Rune)this.lstRunes.SelectedItems[0].Tag;

                frmMain.ext().PopupRunePreviewWindow(Rune.ID_Generic, Rune.Level, Cursor.Position);
            }
            catch { }
        }

        private void enchantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lstRunes.SelectedItems.Count <= 0)
                return;

            int EnchantLevel = Utils.CInt(Utils.Input_Text("To What Level?", "Enchant this Rune to what level (1 - 4):"));
            if ((EnchantLevel < 1) || (EnchantLevel > 4))
                return;

            try
            {
                GameObjs.Rune Rune = (GameObjs.Rune)this.lstRunes.SelectedItems[0].Tag;

                Utils.StartMethodMultithreaded(() =>
                {
                    GameClient.Current.EnchantRune(Rune.Name, EnchantLevel, Rune.ID_User);
                });
            }
            catch { }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() => { GameClient.Current.UserRunes_CachedData = null; });
            this.Cursor = cur;
            this.InitRunes();
        }

        private void sellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = this.AllSelectedItems;

            if (items.Count <= 0)
                return;

            if (MessageBox.Show("Do you really want to sell " + items.Count.ToString("#,##0") + " " + Utils.PluralWord(items.Count, "rune", "runes") + "?", "Confirm Rune Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                return;

            string runes_to_sell = "";
            int gold_amount = 0;
            int gold_runes_sold = items.Count;

            foreach (ListViewItem lvi in items)
            {
                GameObjs.Rune rune = (GameObjs.Rune)lvi.Tag;

                runes_to_sell += rune.ID_User.ToString() + "_";
                gold_amount += rune.SellWorth;
            }

            runes_to_sell = runes_to_sell.Trim(new char[] { '_' });

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Utils.StartMethodMultithreadedAndWait(() =>
            {
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Runes=" + runes_to_sell);
                GameClient.Current.GameVitalsUpdate(GameClient.Current.GetGameData("user", "GetUserInfo"));
                GameClient.Current.UserRunes_CachedData = null;
                Utils.LoggerNotifications("<color=#ffa000>Sold " + gold_runes_sold.ToString("#,##0") + " " + Utils.PluralWord(gold_runes_sold, "rune", "runes") + " for " + gold_amount.ToString("#,##0") + " gold.</color>");
            });
            this.Cursor = cur;
            this.InitRunes();
        }
    }
}
