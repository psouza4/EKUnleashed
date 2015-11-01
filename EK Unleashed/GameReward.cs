using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EKUnleashed
{
    class GameReward
    {
        private JObject json = null;

        public GameReward(string js)
        {
            try
            {
                this.json = JObject.Parse(js);
            }
            catch { }

            if (this.json != null)
                this.Parse();
        }

        public GameReward(JObject jo)
        {
            this.json = jo;

            if (this.json != null)
                this.Parse();
        }

        private void Parse()
        {
            this.CardIDs.Clear();
            this.RuneIDs.Clear();
            this.Fragments_CardIDs.Clear();
            this.Fragments_Qty.Clear();

            try { this.XP = Utils.CInt(this.json["data"]["ExtData"]["Award"]["Exp"]); } catch { }
            try { this.Gold = Utils.CInt(this.json["data"]["ExtData"]["Award"]["Gold"]); } catch { }
            try { this.Gold += Utils.CInt(this.json["data"]["ExtData"]["Award"]["Coins"]); } catch { }
            try { this.ClanContribution = Utils.CInt(this.json["data"]["ExtData"]["Award"]["Resources"]); } catch { }
            try { this.RaiderPoints = Utils.CInt(this.json["data"]["ExtData"]["Award"]["attackPoint"]); } catch { }
            try { this.DIMerit = Utils.CInt(this.json["data"]["ExtData"]["Award"]["Honor"]); } catch { }
            try { this.WTPoints = Utils.CInt(this.json["data"]["ExtData"]["Score"]); } catch { }
            try { this.WTPointsBonus = Utils.CInt(this.json["data"]["ExtData"]["ComboScore"]); } catch { }
            try { this.WTAchievement = (Utils.CInt(this.json["data"]["ExtData"]["Achievement"]) == 1); } catch { }
            try { this.WTAchievementText = this.json["data"]["ExtData"]["AchievementDesc"].ToString(); } catch { }

            try { this.Gems = Utils.CInt(this.json["data"]["Award"]["Cash"]); } catch { }
            try { this.Glory = Utils.CInt(this.json["data"]["Award"]["LeaguePoint"]); } catch { }
            try { this.FieldOfHonorSpins = Utils.CInt(this.json["data"]["Award"]["Refresh"]); } catch { }
       
            int first_card_id = 0;

            try
            {
                first_card_id = Utils.CInt(this.json["data"]["ExtData"]["Award"]["CardId"]);

                if (first_card_id > 0)
                    this.CardIDs.Add(first_card_id);
            }
            catch
            {
                first_card_id = -1;
            }

            try
            {
                JArray cards = (JArray)this.json["data"]["ExtData"]["Award"]["SecondDrop"];

                for (int cardIndex = (first_card_id == -1) ? 0 : 1; cardIndex < cards.Count; cardIndex++)
                    this.CardIDs.Add(Utils.CInt(cards[cardIndex].ToString().Replace("\"", string.Empty)));
            }
            catch { }

            int first_rune_id = 0;

            try
            {
                first_rune_id = Utils.CInt(this.json["data"]["ExtData"]["Award"]["RuneId"]);

                if (first_rune_id > 0)
                    this.RuneIDs.Add(first_rune_id);
            }
            catch
            {
                first_rune_id = -1;
            }

            // There is no 'SecondDrop' equivalent for runes.

            try
            {
                int fragment_count = Utils.CInt(this.json["data"]["ExtData"]["Award"]["ChipNum"]);

                if (fragment_count > 0)
                {
                    int fragment_id = Utils.CInt(this.json["data"]["ExtData"]["Award"]["ChipId"]);
                    if (fragment_id > 0)
                    {
                        this.Fragments_CardIDs.Add(fragment_id);
                        this.Fragments_Qty.Add(fragment_count);
                    }
                }
            }
            catch { }

            try
            {
                int fragment_count = Utils.CInt(this.json["data"]["ExtData"]["Award"]["Chips"]["ChipNum"]);

                if (fragment_count > 0)
                {
                    int fragment_id = Utils.CInt(this.json["data"]["ExtData"]["Award"]["Chips"]["ChipId"]);
                    if (fragment_id > 0)
                    {
                        this.Fragments_CardIDs.Add(fragment_id);
                        this.Fragments_Qty.Add(fragment_count);
                    }
                }
            }
            catch { }
     
            // Exploration and Daily Map Invasion rewards are in an array instead of properties
            for (int i = 1; i <= 2; i++)
            {
                try
                {
                    JArray bonuses = null;
                    
                    if (i == 1)
                        bonuses = (JArray)this.json["data"]["Bonus"];
                    else
                        bonuses = (JArray)this.json["data"]["ExtData"]["Bonus"];

                    for (int iBonusIndex = 0; iBonusIndex < bonuses.Count; iBonusIndex++)
                    {
                        try
                        {
                            string bonus_raw = bonuses[iBonusIndex].ToString();

                            if (bonus_raw.StartsWith("Exp_"))
                                this.XP += Utils.CInt(Utils.ChopperBlank(bonus_raw, "Exp_", null));
                            else if (bonus_raw.StartsWith("Coins_"))
                                this.Gold += Utils.CInt(Utils.ChopperBlank(bonus_raw, "Coins_", null));
                            else if (bonus_raw.StartsWith("Card_"))
                                this.CardIDs.Add(Utils.CInt(Utils.ChopperBlank(bonus_raw, "Card_", null)));
                            else if (bonus_raw.StartsWith("Rune_"))
                                this.RuneIDs.Add(Utils.CInt(Utils.ChopperBlank(bonus_raw, "Rune_", null)));
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        public string XPAward
        {
            get
            {
                if (this.XP <= 0)
                    return "";

                return this.XP.ToString("#,##0") + " EXP";
            }
        }

        public string GoldAward
        {
            get
            {
                if (this.Gold <= 0)
                    return "";

                return this.Gold.ToString("#,##0") + " gold";
            }
        }

        public string GloryAward
        {
            get
            {
                if (this.Glory <= 0)
                    return "";

                return this.Glory.ToString("#,##0") + " glory";
            }
        }

        public string FieldOfHonorSpinsAward
        {
            get
            {
                if (this.FieldOfHonorSpins <= 0)
                    return "";

                return this.FieldOfHonorSpins.ToString("#,##0") + " field of honor " + Utils.Pluralize("spin", this.FieldOfHonorSpins);
            }
        }

        public string GemsAward
        {
            get
            {
                if (this.Gems <= 0)
                    return "";

                string gem_name = "gems";
                if (GameClient.Current.Service == GameClient.GameService.Lies_of_Astaroth || GameClient.Current.Service == GameClient.GameService.Elves_Realm)
                        gem_name = "gems";

                return this.Gems.ToString("#,##0") + " " + gem_name;
            }
        }

        public string ClanContributionAward
        {
            get
            {
                if (this.ClanContribution <= 0)
                    return "";

                return this.ClanContribution.ToString("#,##0") + " clan contribution";
            }
        }

        public string RaiderPointsAward
        {
            get
            {
                if (this.RaiderPoints <= 0)
                    return "";

                return this.RaiderPoints.ToString("#,##0") + " DP";
            }
        }

        public string DIMeritAward
        {
            get
            {
                if (this.DIMerit <= 0)
                    return "";

                return this.DIMerit.ToString("#,##0") + " merit";
            }
        }

        public string WTPointsAward
        {
            get
            {
                if (this.WTPoints <= 0)
                    return "";

                if (this.WTPointsBonus <= 0)
                    return this.WTPoints.ToString("#,##0") + " world tree points";

                return (this.WTPoints + this.WTPointsBonus).ToString("#,##0") + " world tree points (including " + this.WTPointsBonus.ToString("#,##0") + " bonus points for " + this.WTAchievementText.ToLower() + ")";
            }
        }

        public string CardsAward
        {
            get
            {
                if (this.CardIDs.Count == 0)
                    return "";

                if (this.CardIDs.Count == 1)
                    return GameClient.Current.FriendlyReplacerInbound("[Card #" + this.CardIDs[0].ToString() + "]");

                string card_rewards = "";
                foreach (int card in this.CardIDs)
                    card_rewards += ", " + GameClient.Current.FriendlyReplacerInbound("[Card #" + card.ToString() + "]");

                return card_rewards.Trim(new char[] { ' ', ',' });
            }
        }

        public string RunesAward
        {
            get
            {
                if (this.RuneIDs.Count == 0)
                    return "";

                if (this.RuneIDs.Count == 1)
                    return GameClient.Current.FriendlyReplacerInbound("[Rune #" + this.RuneIDs[0].ToString() + "]");

                string rune_rewards = "";
                foreach (int rune in this.RuneIDs)
                    rune_rewards += ", " + GameClient.Current.FriendlyReplacerInbound("[Card #" + rune.ToString() + "]");

                return rune_rewards.Trim(new char[] { ' ', ',' });
            }
        }

        public string AllAwards
        {
            get
            {
                string all = "";

                try
                {
                    if (this.Gems > 0) all += ", " + this.GemsAward;
                    if (this.CardsAward.Length > 0) all += ", " + this.CardsAward;
                    if (this.RunesAward.Length > 0) all += ", " + this.RunesAward;
                    if (this.XPAward.Length > 0) all += ", " + this.XPAward;
                    if (this.GoldAward.Length > 0) all += ", " + this.GoldAward;
                    if (this.ClanContributionAward.Length > 0) all += ", " + this.ClanContributionAward;
                    if (this.RaiderPointsAward.Length > 0) all += ", " + this.RaiderPointsAward;
                    if (this.DIMeritAward.Length > 0) all += ", " + this.DIMeritAward;
                    if (this.WTPointsAward.Length > 0) all += ", " + this.WTPointsAward;
                    if (this.Glory > 0) all += ", " + this.GloryAward;
                    if (this.FieldOfHonorSpins > 0) all += ", " + this.FieldOfHonorSpinsAward;

                    all = all.Trim(new char[] { ' ', ',' });
                }
                catch { }

                if (all.Length > 0)
                    return all;

                return "no rewards";
            }
        }

        public bool HasRewards
        {
            get
            {
                return (this.AllAwards != "no rewards");
            }
        }

        public int XP = 0;
        public int Gold = 0;
        public int ClanContribution = 0;
        public int RaiderPoints = 0;
        public int DIMerit = 0;
        public int WTPoints = 0;
        public int WTPointsBonus = 0;
        public bool WTAchievement = false;
        public string WTAchievementText = "";
        public List<int> CardIDs = new List<int>();
        public List<int> RuneIDs = new List<int>();
        public List<int> Fragments_Qty = new List<int>();
        public List<int> Fragments_CardIDs = new List<int>();
        public int Glory = 0;
        public int Gems = 0;
        public int FieldOfHonorSpins = 0;
    }
}
