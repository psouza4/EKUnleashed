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
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();
            
            // Allow scroll with wheel mouse
            foreach (TabPage tab in this.tabctrlSettings.TabPages)
            {
                //tab.Click += (s, e) => tab.Focus();
                tab.MouseEnter += (s, e) => tab.Focus();
            }

            this.tabctrlSettings.TabPages.Remove(this.tabDemonInvasions_LoA);

            this.chkAutomation_ClanMemberReport.Paint += ChkPaintOverride;
            this.chkAutomation_MazeTowerDailyFreeResets.Paint += ChkPaintOverride;
            this.chkAutomation_Explore.Paint += ChkPaintOverride;
            this.chkAutomation_FightArena.Paint += ChkPaintOverride;
            this.chkAutomation_FightDemonInvasions.Paint += ChkPaintOverride;
            this.chkAutomation_FightHydra.Paint += ChkPaintOverride;
            this.chkAutomation_FightKW.Paint += ChkPaintOverride;
            this.chkAutomation_FightMapInvasions.Paint += ChkPaintOverride;
            this.chkAutomation_FightMazeTowers.Paint += ChkPaintOverride;
            this.chkAutomation_FightThieves.Paint += ChkPaintOverride;
            this.chkAutomation_FightWorldTree.Paint += ChkPaintOverride;
            this.chkAutomation_ReceiveFriendEnergy.Paint += ChkPaintOverride;
            this.chkAutomation_SendFriendEnergy.Paint += ChkPaintOverride;
            this.chkAutomation_DailyTasks.Paint += ChkPaintOverride;
            this.chkAutomation_FieldOfHonorSpins.Paint += ChkPaintOverride;
            this.chkAutomation_CardCrafting.Paint += ChkPaintOverride;
        }

        protected void ChkPaintOverride(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (sender == null) return;
            if (sender.GetType() != typeof(CheckBox)) return;

            CheckBox thisCheckBox = (CheckBox)sender;

            if (thisCheckBox.Enabled)
            {
                base.OnPaint(e);
                return;
            }

            using (Brush aBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                Rectangle r = thisCheckBox.ClientRectangle;
                r.Offset(17, 1);
                e.Graphics.DrawString(thisCheckBox.Text, thisCheckBox.Font, aBrush, r);
            }
        }

        private int _LoggedIntoGame = 0;
        private bool LoggedIntoGame
        {
            get
            {
                if (this._LoggedIntoGame == 0)
                {
                    bool LoggedIn = false;

                    try
                    {
                        if (GameClient.Current != null)
                            if (GameClient.Current.CheckLogin() != null)
                                LoggedIn = true;
                    }
                    catch { }

                    if (LoggedIn)
                        this._LoggedIntoGame = 1;
                    else
                        this._LoggedIntoGame = -1;
                }

                return (this._LoggedIntoGame == 1);
            }
        }

        private void LoadSettings()
        {
            if (!this.LoggedIntoGame)
                while (this.tabctrlSettings.TabPages.Count > 1)
                    this.tabctrlSettings.TabPages.RemoveAt(1);

            /*
            if (GameClient.Current != null)
                if (!GameClient.Current.Want_Developer)
                    if (this.tabsSettings.TabPages.Contains(this.tabDeveloper))
                        this.tabsSettings.TabPages.Remove(this.tabDeveloper));
            */

            // Account tab
            this.txtAccount_GameAccount.Text = Utils.GetAppSetting("Login_Account").Trim();
            this.txtAccount_GamePassword.Text = Utils.GetAppSetting("Login_Password").Trim();
            if (Utils.GetAppSetting("Login_Service").ToUpper().Trim() == "LOA")
                this.ddlAccount_GameService.SelectedIndex = 1;
            else if (Utils.GetAppSetting("Login_Service").ToUpper().Trim() == "MR")
                this.ddlAccount_GameService.SelectedIndex = 2;
            else if (Utils.GetAppSetting("Login_Service").ToUpper().Trim() == "ER")
                this.ddlAccount_GameService.SelectedIndex = 3;
            else if (Utils.GetAppSetting("Login_Service").ToUpper().Trim() == "SW")
                this.ddlAccount_GameService.SelectedIndex = 4;
            else
                this.ddlAccount_GameService.SelectedIndex = 0;
            this.ddlAccount_GameService_SelectedIndexChanged(null, null);
            if (Utils.GetAppSetting("Login_Device").ToUpper().Trim().Contains("ANDROID"))
                this.ddlAccount_DeviceType.SelectedIndex = 1;
            else
                this.ddlAccount_DeviceType.SelectedIndex = 0;
            this.chkAccount_ChatLogin.Checked = Utils.True("Login_Chat");
            this.chkAccount_ChatAutoReconnect.Checked = Utils.False("Chat_AutoReconnect");

            if (this.LoggedIntoGame)
            {
                // General tab
                string default_deck = Utils.GetAppSetting("Game_DefaultDeck").Trim().ToUpper();
                if (default_deck == "KW")
                    this.btnDefaultDeck.Text = "Kingdom War";
                else if (default_deck == "DF")
                    this.btnDefaultDeck.Text = "Defense";
                else if (Utils.CInt(default_deck) > 0)
                    this.btnDefaultDeck.Text = "Deck " + default_deck;
                this.btnDefaultDeck.Tag = default_deck;
                this.chkGeneral_EnchantCardWith1Star.Checked = (Utils.GetAppSetting("Enchant_Cards_WithStars").Contains("1"));
                this.chkGeneral_EnchantCardWith2Star.Checked = (Utils.GetAppSetting("Enchant_Cards_WithStars").Contains("2"));
                this.chkGeneral_EnchantCardWith3Star.Checked = (Utils.GetAppSetting("Enchant_Cards_WithStars").Contains("3"));
                this.txtGeneral_EnchantCardReserveThreshold.Text = Utils.GetAppValue("Enchant_Cards_ReserveThreshold", 5).ToString();
                this.txtGeneral_SellCardReserveThreshold.Text = Utils.GetAppValue("Sell_Cards_ReserveThreshold", 10).ToString();
                this.txtGeneral_EnchantCardExclude.Text = Utils.GetAppSetting("Enchant_Cards_Excluded");
                this.chkGeneral_EnchantCardFoodCards.Checked = Utils.True("Enchant_Cards_AllowFood");
                this.chkGeneral_EnchantRuneWith1Star.Checked = (Utils.GetAppSetting("Enchant_Runes_WithStars").Contains("1"));
                this.chkGeneral_EnchantRuneWith2Stars.Checked = (Utils.GetAppSetting("Enchant_Runes_WithStars").Contains("2"));
                this.chkGeneral_EnchantRuneWith3Stars.Checked = (Utils.GetAppSetting("Enchant_Runes_WithStars").Contains("3"));
                this.txtGeneral_EnchantRuneReserveThreshold.Text = Utils.GetAppValue("Enchant_Runes_ReserveThreshold", 1).ToString();
                this.txtGeneral_EnchantRuneExclude.Text = Utils.GetAppSetting("Enchant_Runes_Excluded");
                this.chkGeneral_MazeTowerChests.Checked = Utils.True("Game_MazeTowerChests");
                this.chkGeneral_MazeTowerMonsters.Checked = Utils.True("Game_MazeTowerMonsters");
                this.txtGeneral_MazeTowers.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Game_MazeTowers").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                if (string.IsNullOrEmpty(this.txtGeneral_MazeTowers.Text))
                    this.txtGeneral_MazeTowers.Text = "8, 7, 6";
                this.chkGeneral_ClaimChestRewards.Checked = Utils.True("Game_ClaimRewards");
                this.chkGeneral_ClaimAchievementRewards.Checked = Utils.True("Game_ClaimAchievementRewards");
                this.chkGeneral_ClaimLevelingRewards.Checked = Utils.True("Game_ClaimLevelingRewards");
                this.chkGeneral_ClaimWorldTreeRewards.Checked = Utils.True("Game_ClaimWorldTreeRewards");
                this.chkGeneral_Debug.Checked = Utils.False("Game_Debug");
                this.chkGeneral_AutoStoreReplays.Checked = Utils.False("Game_StoreReplays");
                this.chkGeneral_DontLinkLowStarCards.Checked = Utils.True("Game_DontLinkLowStarCards");

                // Automation tab
                this.chkAutomation_Events.Checked = Utils.True("Game_Events");
                this.chkAutomation_ClanMemberReport.Checked = Utils.False("Game_ClanMemberReport");
                this.chkAutomation_Explore.Checked = Utils.False("Game_Explore");
                this.chkAutomation_FightArena.Checked = Utils.False("Game_FightArena");
                this.chkAutomation_FightDemonInvasions.Checked = Utils.False("Game_FightDemonInvasions");
                this.chkAutomation_FightHydra.Checked = Utils.False("Game_FightHydra");
                this.chkAutomation_FightKW.Checked = Utils.False("Game_FightKW");
                this.chkAutomation_FightMapInvasions.Checked = Utils.False("Game_FightMapInvasions");
                this.chkAutomation_FightMazeTowers.Checked = Utils.False("Game_FightMazeTowers");
                this.chkAutomation_FightThieves.Checked = Utils.False("Game_FightThieves");
                this.chkAutomation_FightWorldTree.Checked = Utils.False("Game_FightWorldTree");
                this.chkAutomation_ReceiveFriendEnergy.Checked = Utils.False("Game_ReceiveFriendEnergy");
                this.chkAutomation_SendFriendEnergy.Checked = Utils.False("Game_SendFriendEnergy");
                this.chkAutomation_MazeTowerDailyFreeResets.Checked = Utils.False("Game_FreeMazeResetDaily");
                this.chkAutomation_DailyTasks.Checked = Utils.False("Game_DailyTasks");
                this.chkAutomation_FieldOfHonorSpins.Checked = Utils.False("Game_FOHHappyHour");
                this.chkAutomation_CardCrafting.Checked = Utils.False("Game_CardCrafting");
                this.chkAutomation_Events_CheckedChanged(null, null);
                this.chkAutomation_EnableConnectionThrottling.Checked = Utils.True("Game_ThrottleConnectionSpeed");
                this.txtAutomation_ConnectionThrottleAmount.Text = Utils.GetAppValueL("Game_ThrottleAmount", 750).ToString();

                // Elemental Kingdoms/Magic Realms Demon Invasion tab
                this.chkDI_AvoidSniping_EK.Checked = Utils.True("DemonInvasion_AvoidSniping");

                this.btn_Azathoth_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Azathoth_Deck");
                this.btn_Azathoth_Deck.Text = DescribeDeck(this.btn_Azathoth_Deck.Tag);
                this.txtDI_Azathoth_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Azathoth_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Azathoth_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Azathoth_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Bahamut_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Bahamut_Deck");
                this.btn_Bahamut_Deck.Text = DescribeDeck(this.btn_Bahamut_Deck.Tag);
                this.txtDI_Bahamut_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Bahamut_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Bahamut_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Bahamut_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_DarkTitan_Deck.Tag = Utils.GetAppSetting("DemonInvasion_DarkTitan_Deck");
                this.btn_DarkTitan_Deck.Text = DescribeDeck(this.btn_DarkTitan_Deck.Tag);
                this.txtDI_DarkTitan_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_DarkTitan_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_DarkTitan_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_DarkTitan_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Deucalion_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Deucalion_Deck");
                this.btn_Deucalion_Deck.Text = DescribeDeck(this.btn_Deucalion_Deck.Tag);
                this.txtDI_Deucalion_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Deucalion_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Deucalion_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Deucalion_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");            

                this.btn_Mars_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Mars_Deck");
                this.btn_Mars_Deck.Text = DescribeDeck(this.btn_Mars_Deck.Tag);
                this.txtDI_Mars_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Mars_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Mars_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Mars_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Pandarus_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Pandarus_Deck");
                this.btn_Pandarus_Deck.Text = DescribeDeck(this.btn_Pandarus_Deck.Tag);
                this.txtDI_Pandarus_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Pandarus_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Pandarus_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Pandarus_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Pazuzu_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Pazuzu_Deck");
                this.btn_Pazuzu_Deck.Text = DescribeDeck(this.btn_Pazuzu_Deck.Tag);
                this.txtDI_Pazuzu_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Pazuzu_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Pazuzu_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Pazuzu_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_PlagueOgryn_Deck.Tag = Utils.GetAppSetting("DemonInvasion_PlagueOgryn_Deck");
                this.btn_PlagueOgryn_Deck.Text = DescribeDeck(this.btn_PlagueOgryn_Deck.Tag);
                this.txtDI_PlagueOgryn_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_PlagueOgryn_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_PlagueOgryn_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_PlagueOgryn_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_SeaKing_Deck.Tag = Utils.GetAppSetting("DemonInvasion_SeaKing_Deck");
                this.btn_SeaKing_Deck.Text = DescribeDeck(this.btn_SeaKing_Deck.Tag);
                this.txtDI_SeaKing_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_SeaKing_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_SeaKing_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_SeaKing_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                // Lies of Astaroth Demon Invasions tab
                this.chkDI_AvoidSniping_LoA.Checked = Utils.True("DemonInvasion_AvoidSniping");

                this.btn_Mahr_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Mahr_Deck");
                this.btn_Mahr_Deck.Text = DescribeDeck(this.btn_Mahr_Deck.Tag);
                this.txtDI_Mahr_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Mahr_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Mahr_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Mahr_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Destroyer_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Destroyer_Deck");
                this.btn_Destroyer_Deck.Text = DescribeDeck(this.btn_Destroyer_Deck.Tag);
                this.txtDI_Destroyer_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Destroyer_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Destroyer_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Destroyer_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_SpiderQueen_Deck.Tag = Utils.GetAppSetting("DemonInvasion_SpiderQueen_Deck");
                this.btn_SpiderQueen_Deck.Text = DescribeDeck(this.btn_SpiderQueen_Deck.Tag);
                this.txtDI_SpiderQueen_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_SpiderQueen_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_SpiderQueen_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_SpiderQueen_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Onaga_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Onaga_Deck");
                this.btn_Onaga_Deck.Text = DescribeDeck(this.btn_Onaga_Deck.Tag);
                this.txtDI_Onaga_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Onaga_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Onaga_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Onaga_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_Nemesis_Deck.Tag = Utils.GetAppSetting("DemonInvasion_Nemesis_Deck");
                this.btn_Nemesis_Deck.Text = DescribeDeck(this.btn_Nemesis_Deck.Tag);
                this.txtDI_Nemesis_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Nemesis_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_Nemesis_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_Nemesis_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                this.btn_DemonFiend_Deck.Tag = Utils.GetAppSetting("DemonInvasion_DemonFiend_Deck");
                this.btn_DemonFiend_Deck.Text = DescribeDeck(this.btn_DemonFiend_Deck.Tag);
                this.txtDI_DemonFiend_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_DemonFiend_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtDI_DemonFiend_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_DemonFiend_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                // Arena tab
                this.txtArena_DontAttack.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Arena_DontAttack").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.chkArenaSkipClan.Checked = Utils.False("Arena_SkipClan");
                this.chkArenaSkipFriends.Checked = Utils.False("Arena_SkipFriends");
                this.chkArenaStealthMethod.Checked = Utils.False("Arena_Stealth");

                // Thieves tab
                this.btnThiefDeck.Tag = Utils.GetAppSetting("Thief_Deck").Trim().ToUpper();
                this.btnThiefDeck.Text = DescribeDeck(this.btnThiefDeck.Tag);
                this.txtThieves_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Thief_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtThieves_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Thief_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.chkThieves_AlwaysFill.Checked = Utils.False("Thief_AlwaysFill");
                this.txtThieves_IgnoreFrom.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Thief_IgnoreFrom").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");

                // Raider tab
                string raider_deck = Utils.GetAppSetting("Hydra_Deck").Trim().ToUpper();
                if (raider_deck == "KW")
                    this.btnRaider_Deck.Text = "Kingdom War";
                else if (raider_deck == "DF")
                    this.btnRaider_Deck.Text = "Defense";
                else if (Utils.CInt(raider_deck) > 0)
                    this.btnRaider_Deck.Text = "Deck " + raider_deck;
                this.btnRaider_Deck.Tag = raider_deck;
                this.txtRaider_DeckCards.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Hydra_DeckCards").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.txtRaider_DeckRunes.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Hydra_DeckRunes").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                this.chkRaider_AlwaysFill.Checked = Utils.False("Hydra_AlwaysFill");
                if (Utils.GetAppSetting("Hydra_AutomationMode").Trim().ToLower() == "fight only")
                    this.ddlRaider_AutomationMode.SelectedIndex = 1;
                else if (Utils.GetAppSetting("Hydra_AutomationMode").Trim().ToLower() == "claim only")
                    this.ddlRaider_AutomationMode.SelectedIndex = 2;
                else
                    this.ddlRaider_AutomationMode.SelectedIndex = 0;
                this.chkRaider_OnlyFightMine.Checked = Utils.False("Hydra_OnlyFightMine");
                this.txtRaider_IgnoreFrom.Text = Utils.CondenseSpacing(Utils.GetAppSetting("Hydra_IgnoreFrom").Trim()).Replace(", ", ",").Replace(" ,", ",").Replace(",", ", ");
                if (Utils.CInt(Utils.GetAppSetting("Hydra_Frequency")) > 0)
                    this.txtRaider_Frequency.Text = Utils.CInt(Utils.GetAppSetting("Hydra_Frequency")).ToString();
                else
                    this.txtRaider_Frequency.Text = "60";
            }
        }

        private void SaveSettings()
        {
            // Account tab
            Utils.SetAppSetting("Login_Account", this.txtAccount_GameAccount.Text.Trim());
            Utils.SetAppSetting("Login_Password", this.txtAccount_GamePassword.Text.Trim());
            if (this.ddlAccount_GameService.SelectedIndex == 1)
                Utils.SetAppSetting("Login_Service", "LoA");
            else if (this.ddlAccount_GameService.SelectedIndex == 2)
                Utils.SetAppSetting("Login_Service", "MR");
            else if (this.ddlAccount_GameService.SelectedIndex == 3)
                Utils.SetAppSetting("Login_Service", "ER");
            else if (this.ddlAccount_GameService.SelectedIndex == 4)
                Utils.SetAppSetting("Login_Service", "SW");
            else
                Utils.SetAppSetting("Login_Service", "EK");
            if (this.ddlAccount_DeviceType.SelectedIndex == 1)
                Utils.SetAppSetting("Login_Device", "Android");
            else
                Utils.SetAppSetting("Login_Device", "iOS");
            Utils.SetAppSetting("Login_Chat", this.chkAccount_ChatLogin.Checked.ToString());
            Utils.SetAppSetting("Chat_AutoReconnect", this.chkAccount_ChatAutoReconnect.Checked.ToString());

            if (this.LoggedIntoGame)
            {
                // General tab
                Utils.SetAppSetting("Game_DefaultDeck", this.btnDefaultDeck.Tag.ToString());
                string enchant_card_stars = "";
                if (this.chkGeneral_EnchantCardWith1Star.Checked) enchant_card_stars += ", 1";
                if (this.chkGeneral_EnchantCardWith2Star.Checked) enchant_card_stars += ", 2";
                if (this.chkGeneral_EnchantCardWith3Star.Checked) enchant_card_stars += ", 3";
                Utils.SetAppSetting("Enchant_Cards_WithStars", enchant_card_stars.TrimStart(new char[] { ' ', ',' }));
                Utils.SetAppSetting("Enchant_Cards_ReserveThreshold", this.txtGeneral_EnchantCardReserveThreshold.Text.Trim());
                Utils.SetAppSetting("Sell_Cards_ReserveThreshold", this.txtGeneral_SellCardReserveThreshold.Text.Trim());
                Utils.SetAppSetting("Enchant_Cards_Excluded", this.txtGeneral_EnchantCardExclude.Text.Trim());
                Utils.SetAppSetting("Enchant_Cards_AllowFood", this.chkGeneral_EnchantCardFoodCards.Checked.ToString());
                string enchant_rune_stars = "";
                if (this.chkGeneral_EnchantRuneWith1Star.Checked) enchant_rune_stars += ", 1";
                if (this.chkGeneral_EnchantRuneWith2Stars.Checked) enchant_rune_stars += ", 2";
                if (this.chkGeneral_EnchantRuneWith3Stars.Checked) enchant_rune_stars += ", 3";
                Utils.SetAppSetting("Enchant_Runes_WithStars", enchant_rune_stars.TrimStart(new char[] { ' ', ',' }));
                Utils.SetAppSetting("Enchant_Runes_ReserveThreshold", this.txtGeneral_EnchantRuneReserveThreshold.Text.Trim());
                Utils.SetAppSetting("Enchant_Runes_Excluded", this.txtGeneral_EnchantRuneExclude.Text.Trim());
                Utils.SetAppSetting("Game_MazeTowerChests", this.chkGeneral_MazeTowerChests.Checked.ToString());
                Utils.SetAppSetting("Game_MazeTowerMonsters", this.chkGeneral_MazeTowerMonsters.Checked.ToString());
                if (string.IsNullOrEmpty(this.txtGeneral_MazeTowers.Text))
                    this.txtGeneral_MazeTowers.Text = "10, 9, 8, 7, 6";
                Utils.SetAppSetting("Game_MazeTowers", this.txtGeneral_MazeTowers.Text.Trim());
                Utils.SetAppSetting("Game_ClaimRewards", this.chkGeneral_ClaimChestRewards.Checked.ToString());
                Utils.SetAppSetting("Game_ClaimAchievementRewards", this.chkGeneral_ClaimAchievementRewards.Checked.ToString());
                Utils.SetAppSetting("Game_ClaimLevelingRewards", this.chkGeneral_ClaimLevelingRewards.Checked.ToString());
                Utils.SetAppSetting("Game_ClaimWorldTreeRewards", this.chkGeneral_ClaimWorldTreeRewards.Checked.ToString());
                Utils.SetAppSetting("Game_Debug", this.chkGeneral_Debug.Checked.ToString());
                Utils.SetAppSetting("Game_StoreReplays", this.chkGeneral_AutoStoreReplays.Checked.ToString());
                Utils.SetAppSetting("Game_DontLinkLowStarCards", this.chkGeneral_DontLinkLowStarCards.Checked.ToString());

                // Automation tab
                Utils.SetAppSetting("Game_Events", this.chkAutomation_Events.Checked.ToString());
                Utils.SetAppSetting("Game_ClanMemberReport", this.chkAutomation_ClanMemberReport.Checked.ToString());
                Utils.SetAppSetting("Game_FreeMazeResetDaily", this.chkAutomation_MazeTowerDailyFreeResets.Checked.ToString());
                Utils.SetAppSetting("Game_Explore", this.chkAutomation_Explore.Checked.ToString());
                Utils.SetAppSetting("Game_FightArena", this.chkAutomation_FightArena.Checked.ToString());
                Utils.SetAppSetting("Game_FightDemonInvasions", this.chkAutomation_FightDemonInvasions.Checked.ToString());
                Utils.SetAppSetting("Game_FightHydra", this.chkAutomation_FightHydra.Checked.ToString());
                Utils.SetAppSetting("Game_FightKW", this.chkAutomation_FightKW.Checked.ToString());
                Utils.SetAppSetting("Game_FightMapInvasions", this.chkAutomation_FightMapInvasions.Checked.ToString());
                Utils.SetAppSetting("Game_FightMazeTowers", this.chkAutomation_FightMazeTowers.Checked.ToString());
                Utils.SetAppSetting("Game_FightThieves", this.chkAutomation_FightThieves.Checked.ToString());
                Utils.SetAppSetting("Game_FightWorldTree", this.chkAutomation_FightWorldTree.Checked.ToString());
                Utils.SetAppSetting("Game_ReceiveFriendEnergy", this.chkAutomation_ReceiveFriendEnergy.Checked.ToString());
                Utils.SetAppSetting("Game_SendFriendEnergy", this.chkAutomation_SendFriendEnergy.Checked.ToString());
                Utils.SetAppSetting("Game_DailyTasks", this.chkAutomation_DailyTasks.Checked.ToString());
                Utils.SetAppSetting("Game_FOHHappyHour", this.chkAutomation_FieldOfHonorSpins.Checked.ToString());
                Utils.SetAppSetting("Game_CardCrafting", this.chkAutomation_CardCrafting.Checked.ToString());
                Utils.SetAppSetting("Game_ThrottleConnectionSpeed", this.chkAutomation_EnableConnectionThrottling.Checked.ToString());
                Utils.SetAppSetting("Game_ThrottleAmount", Utils.CLng(this.txtAutomation_ConnectionThrottleAmount.Text).ToString());

                // Demon Invasion tab
                if (this.ddlAccount_GameService.SelectedIndex == 1) // LoA
                {
                    Utils.SetAppSetting("DemonInvasion_AvoidSniping", this.chkDI_AvoidSniping_LoA.Checked.ToString());

                    Utils.SetAppSetting("DemonInvasion_Mahr_Deck", this.btn_Mahr_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Mahr_DeckCards", this.txtDI_Mahr_DeckRunes.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Mahr_DeckRunes", this.txtDI_Mahr_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Destroyer_Deck", this.btn_Destroyer_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Destroyer_DeckCards", this.txtDI_Destroyer_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Destroyer_DeckRunes", this.txtDI_Destroyer_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_SpiderQueen_Deck", this.btn_SpiderQueen_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_SpiderQueen_DeckCards", this.txtDI_SpiderQueen_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_SpiderQueen_DeckRunes", this.txtDI_SpiderQueen_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Onaga_Deck", this.btn_Onaga_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Onaga_DeckCards", this.txtDI_Onaga_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Onaga_DeckRunes", this.txtDI_Onaga_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Nemesis_Deck", this.btn_Nemesis_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Nemesis_DeckCards", this.txtDI_Nemesis_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Nemesis_DeckRunes", this.txtDI_Nemesis_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_DemonFiend_Deck", this.btn_DemonFiend_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_DemonFiend_DeckCards", this.txtDI_DemonFiend_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_DemonFiend_DeckRunes", this.txtDI_DemonFiend_DeckRunes.Text.Trim());
                }
                else
                {
                    Utils.SetAppSetting("DemonInvasion_AvoidSniping", this.chkDI_AvoidSniping_EK.Checked.ToString());

                    Utils.SetAppSetting("DemonInvasion_Azathoth_Deck", this.btn_Azathoth_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Azathoth_DeckCards", this.txtDI_Azathoth_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Azathoth_DeckRunes", this.txtDI_Azathoth_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Bahamut_Deck", this.btn_Bahamut_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Bahamut_DeckCards", this.txtDI_Bahamut_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Bahamut_DeckRunes", this.txtDI_Bahamut_DeckRunes.Text.Trim());                    

                    Utils.SetAppSetting("DemonInvasion_DarkTitan_Deck", this.btn_DarkTitan_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_DarkTitan_DeckCards", this.txtDI_DarkTitan_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_DarkTitan_DeckRunes", this.txtDI_DarkTitan_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Deucalion_Deck", this.btn_Deucalion_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Deucalion_DeckCards", this.txtDI_Deucalion_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Deucalion_DeckRunes", this.txtDI_Deucalion_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Mars_Deck", this.btn_Mars_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Mars_DeckCards", this.txtDI_Mars_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Mars_DeckRunes", this.txtDI_Mars_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Pandarus_Deck", this.btn_Pandarus_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Pandarus_DeckCards", this.txtDI_Pandarus_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Pandarus_DeckRunes", this.txtDI_Pandarus_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_Pazuzu_Deck", this.btn_Pazuzu_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_Pazuzu_DeckCards", this.txtDI_Pazuzu_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_Pazuzu_DeckRunes", this.txtDI_Pazuzu_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_PlagueOgryn_Deck", this.btn_PlagueOgryn_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_PlagueOgryn_DeckCards", this.txtDI_PlagueOgryn_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_PlagueOgryn_DeckRunes", this.txtDI_PlagueOgryn_DeckRunes.Text.Trim());

                    Utils.SetAppSetting("DemonInvasion_SeaKing_Deck", this.btn_SeaKing_Deck.Tag.ToString());
                    Utils.SetAppSetting("DemonInvasion_SeaKing_DeckCards", this.txtDI_SeaKing_DeckCards.Text.Trim());
                    Utils.SetAppSetting("DemonInvasion_SeaKing_DeckRunes", this.txtDI_SeaKing_DeckRunes.Text.Trim());                    
                }


                // Arena tab
                Utils.SetAppSetting("Arena_DontAttack", this.txtArena_DontAttack.Text.ToString());
                Utils.SetAppSetting("Arena_SkipClan", this.chkArenaSkipClan.Checked.ToString());
                Utils.SetAppSetting("Arena_SkipFriends", this.chkArenaSkipFriends.Checked.ToString());
                Utils.SetAppSetting("Arena_Stealth", this.chkArenaStealthMethod.Checked.ToString());



                // Thieves tab
                Utils.SetAppSetting("Thief_Deck", this.btnThiefDeck.Tag.ToString());
                Utils.SetAppSetting("Thief_DeckCards", this.txtThieves_DeckCards.Text.Trim());
                Utils.SetAppSetting("Thief_DeckRunes", this.txtThieves_DeckRunes.Text.Trim());
                Utils.SetAppSetting("Thief_AlwaysFill", this.chkThieves_AlwaysFill.Checked.ToString());
                Utils.SetAppSetting("Thief_IgnoreFrom", this.txtThieves_IgnoreFrom.Text.Trim());


                // Raider tab
                Utils.SetAppSetting("Hydra_Deck", this.btnRaider_Deck.Tag.ToString());
                Utils.SetAppSetting("Hydra_DeckCards", this.txtRaider_DeckCards.Text.Trim());
                Utils.SetAppSetting("Hydra_DeckRunes", this.txtRaider_DeckRunes.Text.Trim());
                Utils.SetAppSetting("Hydra_AlwaysFill", this.chkRaider_AlwaysFill.Checked.ToString());
                if (this.ddlRaider_AutomationMode.SelectedIndex == 1)
                    Utils.SetAppSetting("Hydra_AutomationMode", "fight only");
                else if (this.ddlRaider_AutomationMode.SelectedIndex == 2)
                    Utils.SetAppSetting("Hydra_AutomationMode", "claim only");
                else
                    Utils.SetAppSetting("Hydra_AutomationMode", "all");
                Utils.SetAppSetting("Hydra_OnlyFightMine", this.chkRaider_OnlyFightMine.Checked.ToString());
                Utils.SetAppSetting("Hydra_IgnoreFrom", this.txtRaider_IgnoreFrom.Text.Trim());
                int hydra_freq = Utils.CInt(this.txtRaider_Frequency.Text);
                if (hydra_freq < 1)
                    hydra_freq = 60;
                Utils.SetAppSetting("Hydra_Frequency", hydra_freq.ToString());
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveSettings();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            this.LoadSettings();
        }

        private void chkAutomation_Events_CheckedChanged(object sender, EventArgs e)
        {
            this.chkAutomation_ClanMemberReport.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_MazeTowerDailyFreeResets.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_Explore.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightArena.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightDemonInvasions.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightHydra.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightKW.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightMapInvasions.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightMazeTowers.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightThieves.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FightWorldTree.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_ReceiveFriendEnergy.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_SendFriendEnergy.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_DailyTasks.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_FieldOfHonorSpins.Enabled = this.chkAutomation_Events.Checked;
            this.chkAutomation_CardCrafting.Enabled = this.chkAutomation_Events.Checked;
        }

        private static string DescribeDeck(object deck)
        {
            string deck_eval = deck.ToString().Trim().ToUpper();

            if (deck_eval == "KW")
                return "Kingdom War";
            else if (deck_eval == "DF")
                return "Defense";
            else if (Utils.CInt(deck_eval) > 0)
                return "Deck " + Utils.CInt(deck_eval).ToString();

            return "(select deck)";
        }

        private void DeckSelector(ref Button btn)
        {
            GameClient.Current.CheckLogin();
            if (GameClient.Current.opts == null)
                return;

            Cursor cur = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            frmSelectDeck SelectDeck = new frmSelectDeck();

            JObject user_data = JObject.Parse(GameClient.Current.GetGameData("user", "GetUserInfo", false));
            string sActivelySelectedGroup = user_data["data"]["DefaultGroupId"].ToString();

            SelectDeck.AddDeck(false, "None", "", "", "");

            Utils.StartMethodMultithreadedAndWait(() =>
            {
                string json = GameClient.Current.GetGameData(ref GameClient.Current.opts, "card", "GetCardGroup", false);

                GameClient.Current.GetUsersCards();
                GameClient.Current.GetUsersRunes();

                JObject decks = JObject.Parse(json);

                for (int iPass = 0; iPass <= 2; iPass++)
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
                        else if (Utils.CLng(GameClient.Current.Defense_Deck_ID) == Utils.CLng(deck["GroupId"]))
                        {
                            deck_EKUnleashed_ID = "DF";
                            deck_name = "Defense Deck";
                        }

                        if
                        (
                            (iPass == 0 && deck_EKUnleashed_ID == "DF") ||
                            (iPass == 1 && deck_EKUnleashed_ID == "KW") ||
                            (iPass == 2 && deck_EKUnleashed_ID != "KW" && deck_EKUnleashed_ID != "DF")
                        )
                        {
                            string pretty_cards_used = "";
                            try
                            {
                                foreach (JObject card in deck["UserCardInfo"])
                                {
                                    try
                                    {
                                        if (Utils.CInt(card["CardId"]) > 0)
                                        {
                                            pretty_cards_used += ", " + GameClient.Current.ShortCardInfo(Utils.CInt(card["CardId"]), Utils.CInt(card["Level"]), true);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                            string pretty_runes_used = "";
                            try
                            {
                                foreach (JObject rune in deck["UserRuneInfo"])
                                {
                                    try
                                    {
                                        if (Utils.CInt(rune["RuneId"]) > 0)
                                        {
                                            pretty_runes_used += ", " + GameClient.Current.ShortRuneInfo(Utils.CInt(rune["RuneId"]), Utils.CInt(rune["Level"]), true);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                            SelectDeck.AddDeck
                            (
                                (Utils.CLng(sActivelySelectedGroup) == Utils.CLng(deck["GroupId"])),
                                (iPass == 1) ? "KW" : iAdjustedDeckNumber.ToString(),
                                Utils.CLng(deck["GroupId"]).ToString(),
                                pretty_cards_used,
                                pretty_runes_used
                            );
                        }
                    }
                }
            }, 60); // wait up to 60 seconds

            this.Cursor = cur;
            if (SelectDeck.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(SelectDeck.SelectedDeckName))
                {
                    btn.Text = SelectDeck.SelectedDeckName;

                    if (btn.Text.Trim().ToUpper().StartsWith("K"))
                        btn.Tag = "KW";
                    else if (btn.Text.Trim().ToUpper().StartsWith("DEF") || btn.Text.Trim().ToUpper() == "DF")
                        btn.Tag = "DF";
                    else if (Utils.CInt(Utils.ChopperBlank(btn.Text, " ", null)) > 0)
                        btn.Tag = Utils.CInt(Utils.ChopperBlank(btn.Text, " ", null)).ToString();
                    else
                    {
                        btn.Text = "(select deck)";
                        btn.Tag = "";
                    }
                }
                else
                {
                    btn.Text = "(select deck)";
                    btn.Tag = "";
                }
            }
        }

        private void btnDefaultDeck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btnDefaultDeck);
        }
       
        private void btn_Azathoth_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Azathoth_Deck);
        }

        private void btn_Bahamut_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Bahamut_Deck);
        }

        private void btn_DarkTitan_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_DarkTitan_Deck);
        }

        private void btn_Deucalion_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Deucalion_Deck);
        }

        private void btn_Mars_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Mars_Deck);
        }

        private void btn_Pandarus_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Pandarus_Deck);
        }

        private void btn_Pazuzu_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Pazuzu_Deck);
        }

        private void btn_PlagueOgryn_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_PlagueOgryn_Deck);
        }

        private void btn_SeaKing_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_SeaKing_Deck);
        }

        private void btnThiefDeck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btnThiefDeck);
        }

        private void btnRaider_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btnRaider_Deck);
        }

        private void ddlAccount_GameService_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlAccount_GameService.SelectedIndex == 1) // LoA
            {
                this.ddlAccount_DeviceType.Enabled = true;

                if (this.tabctrlSettings.TabPages.Contains(this.tabDemonInvasions_EK))
                {
                    this.tabctrlSettings.TabPages.Remove(this.tabDemonInvasions_EK);
                    this.tabctrlSettings.TabPages.Insert(3, this.tabDemonInvasions_LoA);
                }
            }
            else // EK/MR
            {
                this.ddlAccount_DeviceType.Enabled = false;

                if (this.tabctrlSettings.TabPages.Contains(this.tabDemonInvasions_LoA))
                {
                    this.tabctrlSettings.TabPages.Remove(this.tabDemonInvasions_LoA);
                    this.tabctrlSettings.TabPages.Insert(3, this.tabDemonInvasions_EK);
                }
            }
        }

        private void btn_Mahr_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Mahr_Deck);
        }

        private void btn_Destroyer_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Destroyer_Deck);
        }

        private void btn_SpiderQueen_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_SpiderQueen_Deck);
        }

        private void btn_Onaga_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Onaga_Deck);
        }

        private void btn_Nemesis_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_Nemesis_Deck);
        }

        private void btn_DemonFiend_Deck_Click(object sender, EventArgs e)
        {
            this.DeckSelector(ref this.btn_DemonFiend_Deck);
        }
                
    }
}
