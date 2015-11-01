using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EKUnleashed
{
    public partial class frmKWPriority : Form
    {
        public frmKWPriority()
        {
            InitializeComponent();
        }

        private void frmKWPriority_Load(object sender, EventArgs e)
        {
            string sPreferedAttackTargets = Utils.GetAppSetting("KW_Targets_Attack");
            string sIgnoredTargets = Utils.GetAppSetting("KW_Targets_Ignored");

            if (!Utils.ValidText(sPreferedAttackTargets))
            {
                sPreferedAttackTargets  = "1_Fire Furnace||7_Divine Throne||19_Fountain of Life||13_Plague Burrow||";
                sPreferedAttackTargets += "3_Sea of Lava||2_Sunset Hills||8_Squall Mountains||9_Guardian Statues||20_Magic Academy||21_Moon Valley||14_Toxic Springs||15_Gray Swamp||";
                sPreferedAttackTargets += "29_Holy War Remains||";
                sPreferedAttackTargets += "5_Crater||11_King's Temple||23_Elves Altar||17_Wild Fog||";
                sPreferedAttackTargets += "4_Flame Peak||5_Black City Gate||10_Wind Setsurei||12_Barbarian Outpost||22_Daylight Crossing||24_Jade Stream||16_Nether Trail||18_Wraith Burial||";
                sPreferedAttackTargets += "28_Horror Peaks||26_Thunder Tower||30_Mystic Wetlands||32_Magic Forest||";
                sPreferedAttackTargets += "25_Mystical Fortress||27_Sunset City||31_Pebble Bay||33_Thorn Prairie||";
            }

            if (!Utils.ValidText(sIgnoredTargets))
            {
                sIgnoredTargets = "||";
            }

            foreach (string node_details in Utils.SubStrings(sPreferedAttackTargets, "||"))
            {
                if (!Utils.ValidText(node_details)) continue;
                if (!node_details.Contains('_')) continue;

                try
                {
                    string[] node_data = Utils.SubStrings(node_details, "_");

                    ListViewItem lvi = new ListViewItem(node_data[1]);
                    lvi.Tag = node_data[0];

                    this.lstKWTargets.Items.Add(lvi);
                }
                catch { }
            }

            foreach (string node_details in Utils.SubStrings(sIgnoredTargets, "||"))
            {
                if (!Utils.ValidText(node_details)) continue;
                if (!node_details.Contains('_')) continue;

                try
                {
                    string[] node_data = Utils.SubStrings(node_details, "_");

                    ListViewItem lvi = new ListViewItem(node_data[1]);
                    lvi.Tag = node_data[0];

                    this.lstKWDisabledTargets.Items.Add(lvi);
                }
                catch { }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string attack_targets = "";
            foreach (ListViewItem lvi in this.lstKWTargets.Items)
                attack_targets += ((string)lvi.Tag) + "_" + lvi.Text + "||";

            string disabled_targets = "";
            foreach (ListViewItem lvi in this.lstKWDisabledTargets.Items)
                disabled_targets += ((string)lvi.Tag) + "_" + lvi.Text + "||";

            Utils.SetAppSetting("KW_Targets_Attack", attack_targets);
            Utils.SetAppSetting("KW_Targets_Ignored", disabled_targets);

            try
            {
                GameClient.Current.KingdomWar_LoadTargets();
            }
            catch { }

            this.Close();
        }

        private void lstKWTargets_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                Point p = this.lstKWTargets.PointToClient(Cursor.Position);
                ListViewHitTestInfo hit = this.lstKWTargets.HitTest(p.X, p.Y);

                if (hit != null)
                    if (hit.Item != null)
                        this.lstKWTargets.DoDragDrop(hit.Item, DragDropEffects.Move);
            }
            catch { }
        }

        private void lstKWTargets_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void lstKWTargets_DragDrop(object sender, DragEventArgs e)
        {
            int indexDraggingTo = -1;

            try
            {
                Point p = this.lstKWTargets.PointToClient(new Point(e.X, e.Y));
                ListViewHitTestInfo hit = this.lstKWTargets.HitTest(p.X, p.Y);

                if (hit != null)
                    if (hit.Item != null)
                        indexDraggingTo = hit.Item.Index;
            }
            catch { }

            if (indexDraggingTo < 0)
                indexDraggingTo = this.lstKWTargets.Items.Count - 1;

            ListViewItem lvi = (ListViewItem)(e.Data.GetData(typeof(ListViewItem)));

            this.lstKWTargets.Items.Remove(lvi);
            this.lstKWTargets.Items.Insert(indexDraggingTo, lvi);
        }

        private void lstKWDisabledTargets_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                Point p = this.lstKWDisabledTargets.PointToClient(Cursor.Position);
                ListViewHitTestInfo hit = this.lstKWDisabledTargets.HitTest(p.X, p.Y);

                if (hit != null)
                    if (hit.Item != null)
                        this.lstKWDisabledTargets.DoDragDrop(hit.Item, DragDropEffects.Move);
            }
            catch { }
        }

        private void lstKWDisabledTargets_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void lstKWDisabledTargets_DragDrop(object sender, DragEventArgs e)
        {
            int indexDraggingTo = -1;

            try
            {
                Point p = this.lstKWDisabledTargets.PointToClient(new Point(e.X, e.Y));
                ListViewHitTestInfo hit = this.lstKWDisabledTargets.HitTest(p.X, p.Y);

                if (hit != null)
                    if (hit.Item != null)
                        indexDraggingTo = hit.Item.Index;
            }
            catch { }

            if (indexDraggingTo < 0)
                indexDraggingTo = this.lstKWDisabledTargets.Items.Count - 1;

            ListViewItem lvi = (ListViewItem)(e.Data.GetData(typeof(ListViewItem)));

            this.lstKWDisabledTargets.Items.Remove(lvi);
            this.lstKWDisabledTargets.Items.Insert(indexDraggingTo, lvi);
        }

        private void btnMoveRight_Click(object sender, EventArgs e)
        {
            if (this.lstKWTargets.SelectedItems.Count <= 0)
                return;

            ListViewItem lvi = this.lstKWTargets.SelectedItems[0];
            this.lstKWTargets.Items.Remove(lvi);
            this.lstKWDisabledTargets.Items.Add(lvi);
        }

        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            if (this.lstKWDisabledTargets.SelectedItems.Count <= 0)
                return;

            ListViewItem lvi = this.lstKWDisabledTargets.SelectedItems[0];
            this.lstKWDisabledTargets.Items.Remove(lvi);
            this.lstKWTargets.Items.Add(lvi);
        }

        private void btnViewMap_Click(object sender, EventArgs e)
        {
            Image iDefaultBackground = GameResourceManager.LoadResource_FullName("EK_SW.KWRendered.png");

            frmImagePreview test = new frmImagePreview();
            test.BackgroundImageLayout = ImageLayout.Stretch;
            test.BackgroundImage = iDefaultBackground;
            test.ClientSize = new System.Drawing.Size(iDefaultBackground.Width, iDefaultBackground.Height);
            test.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            test.Padding = new Padding(0);
            test.PreviewType = frmImagePreview.PreviewTypes.Other;
            test.Text = "EK Unleashed :: Kingdom War Reference Map";
            test.Icon = (Icon)this.Icon.Clone();
            test.Show();
        }
    }
}
