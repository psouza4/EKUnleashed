using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EKUnleashed.GameObjs
{
    class Card
    {
        private JToken raw_card = null;
        public JToken raw_card_details = null;

        public bool Valid
        {
            get
            {
                if (this.raw_card_details == null) return false;
                return true;
            }
        }

        public Card(JToken PlayerCard)
        {
            try
            {
                this.raw_card = PlayerCard;
                this.raw_card_details = GameClient.Current.GetCardByID(this.ID_Generic);
            }
            catch { }
        }

        public Card(int iGenericCardID)
        {
            try
            {
                this.raw_card_details = GameClient.Current.GetCardByID(iGenericCardID);
            }
            catch { }
        }

        private int GetValueInt(string property)
        {
            if (!this.Valid)
                return -1;

            try
            {
                return int.Parse(this.raw_card_details[property].ToString().Replace("\"", "").Trim());
            }
            catch { }

            return -1;
        }

        private string GetValueString(string property)
        {
            if (!this.Valid)
                return string.Empty;

            try
            {
                return this.raw_card_details[property].ToString().Trim();
            }
            catch { }

            return string.Empty;
        }

        public int ID_Generic
        {
            get
            {
                // Don't use 'Valid' here -- this is the only property that is called before all JSON objects are set

                try
                {
                    if (this.raw_card != null)
                        return int.Parse(this.raw_card["CardId"].ToString().Replace("\"", "").Trim());
                }
                catch { }

                return -1;
            }
        }

        private int _Level = -1;
        public int Level
        {
            get
            {
                if (this._Level != -1)
                    return this._Level;

                if (!this.Valid)
                    return -1;

                try
                {
                    this._Level = int.Parse(this.raw_card["Level"].ToString().Replace("\"", "").Trim());
                }
                catch { }

                return this._Level;
            }
        }

        private int _MaxLevel = -1;
        public int MaxLevel
        {
            get
            {
                if (this._MaxLevel != -1)
                    return this._MaxLevel;

                if (!this.Valid)
                    return -1;

                this._MaxLevel = 10;
                
                if (this.EvolvedTimes > 0)
                    this._MaxLevel += this.EvolvedTimes;

                if (this._MaxLevel > 15)
                    this._MaxLevel = 15;

                return this._MaxLevel;
            }
        }

        public int ID_User
        {
            get
            {
                if (!this.Valid)
                    return -1;

                try
                {
                    return int.Parse(this.raw_card["UserCardId"].ToString().Replace("\"", "").Trim());
                }
                catch { }

                return -1;
            }
        }

        private int _Stars = -1;
        public int Stars
        {
            get
            {
                if (this._Stars != -1)
                    return this._Stars;
                
                this._Stars = GetValueInt("Color");

                return this._Stars;
            }
        }

        private string _Name = null;
        public string Name
        {
            get
            {
                if (this._Name != null)
                    return this._Name;

                this._Name = GetValueString("CardName");

                return this._Name;
            }
        }
        
        public int CurrentXP
        {
            get
            {
                if (!this.Valid)
                    return 0;

                try
                {
                    return Utils.CInt(this.raw_card["Exp"]);
                }
                catch { }

                return 0;
            }
        }

        private string _Element = null;
        public string Element
        {
            get
            {
                if (this._Element != null)
                    return this._Element;

                this._Element = GameClient.ConvertCardElementToText(this.ElementValue);

                return this._Element;
            }
        }

        public enum ElementTypes
        {
            Tundra,   // Kingdom
            Forest,   // Forest
            Swamp,    // Wilderness
            Mountain, // Hell
            Raider,
            Treasure,
            Food,
            Activity, // Special/Resource
            Demon,
            Unknown
        }

        private int _ElementValue = -1;
        public int ElementValue
        {
            get
            {
                if (this._ElementValue != -1)
                    return this._ElementValue;

                this._ElementValue = this.GetValueInt("Race");

                return this._ElementValue;
            }
        }
        public ElementTypes ElementType
        {
            get
            {
                int iElement = this.ElementValue;

                if (iElement == 1) return ElementTypes.Tundra;
                if (iElement == 2) return ElementTypes.Forest;
                if (iElement == 3) return ElementTypes.Swamp;
                if (iElement == 4) return ElementTypes.Mountain;
                if (iElement == 95) return ElementTypes.Treasure;
                if (iElement == 96) return ElementTypes.Food;
                if (iElement == 97) return ElementTypes.Raider;
                if (iElement == 99) return ElementTypes.Activity;
                if (iElement == 100) return ElementTypes.Demon;

                return ElementTypes.Unknown;
            }
        }

        private int _Cost = -1;
        public int Cost
        {
            get
            {
                if (this._Cost != -1)
                    return this._Cost;

                this._Cost = this.GetValueInt("Cost");

                return this._Cost;
            }
        }

        private int _Wait = -1;
        public int Wait
        {
            get
            {
                if (this._Wait != -1)
                    return this._Wait;

                this._Wait = this.GetValueInt("Wait");

                return this._Wait;
            }
        }

        private string _js_EvoSkillChoices = null;
        private string js_EvoSkillChoices
        {
            get
            {
                if (this._js_EvoSkillChoices == null)
                    this._js_EvoSkillChoices = GameClient.Current.GetGameData("streng", "RandomSkill", "UserCardId=" + this.ID_User.ToString(), true);

                return this._js_EvoSkillChoices;
            }
        }

        private JObject _joEvoSkillChoices = null;
        private JObject joEvoSkillChoices
        {
            get
            {
                if (this._joEvoSkillChoices == null)
                    this._joEvoSkillChoices = JObject.Parse(this.js_EvoSkillChoices);

                return this._joEvoSkillChoices;
            }
        }

        private bool CanEvo
        {
            get
            {
                if (this.Stars < 4)
                    return false;

                if (this.Level < 10)
                    return false;

                return true;
            }
        }

        public string EvoSkillChoice(int iWhich)
        {
            string skill_desc = "";

            if (this.CanEvo)
            {
                try
                {
                    JArray evo_skill_choices = (JArray)joEvoSkillChoices["data"]["TempSkills"];

                    JObject Jskill = GameClient.Current.GetSkillByID(Utils.CInt(evo_skill_choices[iWhich - 1]));
                    if (Jskill != null)
                        skill_desc = Jskill["Name"].ToString();
                }
                catch { }
            }

            return skill_desc;
        }

        private string _Skill1 = null;
        public string Skill1
        {
            get
            {
                if (this._Skill1 != null)
                    return this._Skill1;

                try
                {
                    int skill = this.GetValueInt("Skill");
                    if (skill == 0)
                    {
                        this._Skill1 = string.Empty;
                        return this._Skill1;
                    }

                    JObject Jskill = GameClient.Current.GetSkillByID(skill);
                    if (Jskill == null)
                    {
                        this._Skill1 = string.Empty;
                        return this._Skill1;
                    }

                    this._Skill1 = Jskill["Name"].ToString();
                    return this._Skill1;
                }
                catch { }

                this._Skill1 = "(error)";
                return this._Skill1;
            }
        }

        private string _Skill2 = null;
        public string Skill2
        {
            get
            {
                if (this._Skill2 != null)
                    return this._Skill2;

                try
                {
                    int skill = this.GetValueInt("LockSkill1");
                    if (skill == 0)
                    {
                        this._Skill2 = string.Empty;
                        return this._Skill2;
                    }

                    JObject Jskill = GameClient.Current.GetSkillByID(skill);
                    if (Jskill == null)
                    {
                        this._Skill2 = string.Empty;
                        return this._Skill2;
                    }

                    this._Skill2 = Jskill["Name"].ToString();
                    return this._Skill2;
                }
                catch { }

                this._Skill2 = "(error)";
                return this._Skill2;
            }
        }

        private string _Skill3 = null;
        public string Skill3
        {
            get
            {
                if (this._Skill3 != null)
                    return this._Skill3;

                try
                {
                    string skill_parts_raw = this.GetValueString("LockSkill2");
                    if (skill_parts_raw.Length > 0)
                    {
                        string[] skill_parts = Utils.SubStringsDups(skill_parts_raw, "_");

                        int skill = Utils.CInt(skill_parts[0]);
                        if (skill == 0)
                        {
                            this._Skill3 = string.Empty;
                            return this._Skill3;
                        }

                        JObject Jskill = GameClient.Current.GetSkillByID(skill);
                        if (Jskill == null)
                        {
                            this._Skill3 = string.Empty;
                            return this._Skill3;
                        }

                        this._Skill3 = Jskill["Name"].ToString();
                        return this._Skill3;
                    }

                    this._Skill3 = string.Empty;
                    return this._Skill3;
                }
                catch { }

                this._Skill3 = "(error)";
                return this._Skill3;
            }
        }

        private string _Skill4 = null;
        public string Skill4
        {
            get
            {
                if (this._Skill4 != null)
                    return this._Skill4;

                try
                {
                    string skill_parts_raw = this.GetValueString("LockSkill2");
                    if (skill_parts_raw.Length > 0)
                    {
                        string[] skill_parts = Utils.SubStringsDups(skill_parts_raw, "_");

                        if (skill_parts.Length > 1)
                        {
                            int skill = Utils.CInt(skill_parts[1]);
                            if (skill == 0)
                            {
                                this._Skill4 = string.Empty;
                                return this._Skill4;
                            }

                            JObject Jskill = GameClient.Current.GetSkillByID(skill);
                            if (Jskill == null)
                            {
                                this._Skill4 = string.Empty;
                                return this._Skill4;
                            }

                            this._Skill4 = Jskill["Name"].ToString();
                            return this._Skill4;
                        }
                    }

                    this._Skill4 = string.Empty;
                    return this._Skill4;
                }
                catch { }

                this._Skill4 = string.Empty;
                return this._Skill4;
            }
        }

        private string _EvolvedSkill = null;
        public string EvolvedSkill
        {
            get
            {
                try
                {
                    int iSkill = this.EvolvedSkillID;

                    if (iSkill > 0)
                    {
                        JObject Jskill = GameClient.Current.GetSkillByID(iSkill);
                        if (Jskill == null)
                        {
                            this._EvolvedSkill = string.Empty;
                            return this._EvolvedSkill;
                        }

                        this._EvolvedSkill = Jskill["Name"].ToString();
                        return this._EvolvedSkill;
                    }

                    this._EvolvedSkill = string.Empty;
                    return this._EvolvedSkill;
                }
                catch { }

                this._EvolvedSkill = string.Empty;
                return this._EvolvedSkill;
            }
        }

        private int _EvolvedSkillID = -1;
        public int EvolvedSkillID
        {
            get
            {
                if (this.Stars < 4)
                    return 0;

                if (this._EvolvedSkillID != -1)
                    return this._EvolvedSkillID;

                int iSkill = 0;

                try
                {
                    iSkill = int.Parse(this.raw_card["SkillNew"].ToString().Replace("\"", "").Trim());
                }
                catch { }

                this._EvolvedSkillID = iSkill;

                return this._EvolvedSkillID;
            }
        }

        private int _EvolvedTimes = -1;
        public int EvolvedTimes
        {
            get
            {
                if (this.Stars < 4)
                    return 0;

                if (this._EvolvedTimes != -1)
                    return this._EvolvedTimes;

                int iEvolved = 0;

                try
                {
                    iEvolved = int.Parse(this.raw_card["Evolution"].ToString().Replace("\"", "").Trim());
                }
                catch { }

                this._EvolvedTimes = iEvolved;

                return this._EvolvedTimes;
            }
        }

        public bool FoodCard
        {
            get
            {
                if (this.GetValueInt("Race") == 96)
                {
                    if (this.GetValueInt("BaseExp") >= 1)
                        return true;

                    return false;
                }

                if (this.GetValueInt("Race") != 99)
                    return false;

                if ((this.GetValueInt("BaseExp") >= 1) && (this.GetValueInt("Skill") == 972))
                    return true;

                return false;
            }
        }

        public bool TreasureCard
        {
            get
            {
                if (this.GetValueInt("Race") == 95)
                {
                    if (this.GetValueInt("Price") >= 1)
                        return true;

                    return false;
                }

                if (this.GetValueInt("Race") != 99)
                    return false;

                if ((this.GetValueInt("Price") >= 1) && (this.GetValueInt("Skill") == 1188))
                    return true;

                return false;
            }
        }

        public bool EventCard
        {
            get
            {
                if (this.GetValueInt("Race") != 99)
                    return false;

                if (!this.FoodCard && !this.TreasureCard)
                    return true;

                return false;
            }
        }

        private int _SellWorth = -1;
        public int SellWorth
        {
            get
            {
                if (this._SellWorth != -1)
                    return this._SellWorth;

                this._SellWorth = GetValueInt("Price");

                return this._SellWorth;
            }
        }

        public int EnchantingWorth
        {
            get
            {
                if (GetValueInt("BaseExp") <= 0)
                    return 0;
                
                double XP_Worth = (double)GetValueInt("BaseExp");
                XP_Worth *= 100.0 * ((100.0 - ((5.0 - ((double)this.Stars)) * 10.0))) / 10000.0;
                XP_Worth = Math.Floor(XP_Worth);

                return (int)XP_Worth;
            }
        }

        public int EnchantToMaxCostXP
        {
            get
            {
                return this.EnchantToLevelCostXP(10);
            }
        }

        public int EnchantToLevelCostXP(int level)
        {
            if (!this.Valid)
                return 0;

            try
            {
                string sXPArray = Utils.CondenseSpacing(this.raw_card_details["ExpArray"].ToString().Replace("\r", " ").Replace("\n", " ").Replace("[", " ").Replace("]", " ").Trim());

                int maxXP = Utils.CInt(Utils.SubStringsDups(sXPArray, ",")[level]);

                return maxXP - this.CurrentXP;
            }
            catch
            {
                int expResult = this.getTotalExpNeeded(this.Stars, level) - getTotalExpNeeded(this.Stars, this.Level);
                if (expResult < 0)
                    expResult = 0;

                return expResult;
            }
        }

        public int EnchantToMaxCostGold
        {
            get
            {
                return this.EnchantToLevelCostGold(10);
            }
        }
        public int EnchantToLevelCostGold(int level)
        {
            if (!this.Valid)
                return 0;

            return this.getGoldNeeded(this.Stars, this.EnchantToLevelCostXP(level));
        }

        private int getGoldNeeded(int stars, int exp)
        {
	        int[] cost = new int[] { 0, 5, 6, 8, 12, 25 };
	
	        return exp * cost[stars];	
        }

        private int getExpNeeded(int stars, int level)
        {
	        int[] cost = new int[] { 0, 60, 100, 250, 350, 500 };
	
	        if (level < 6)
                return level * cost[stars];

	        if (level < 10)
                return (level - 1) * cost[stars] * 2;

	        return level * cost[stars] * 3;
        }

        private int getTotalExpNeeded(int stars, int level)
        {
	        int needed = 0;
	
	        while (level > 0)
            {
		        needed += this.getExpNeeded(stars, level);
		        level--;
	        }
	
	        return needed;
        }

        private static List<int> CardsInDeckCache = new List<int>();

        public static void RefreshCardsInDeckCache()
        {
            Card.CardsInDeckCache.Clear();

            string json = GameClient.Current.GetGameData(ref GameClient.Current.opts, "card", "GetCardGroup", false);
            JObject decks = JObject.Parse(json);
            foreach (JObject deck in decks["data"]["Groups"])
                foreach (JObject card in deck["UserCardInfo"])
                    if (Utils.CInt(card["UserCardId"]) > 0)
                        if (!Card.CardsInDeckCache.Contains(Utils.CInt(card["UserCardId"])))
                            Card.CardsInDeckCache.Add(Utils.CInt(card["UserCardId"]));

            return;
        }

        public bool InAnyDeck
        {
            get
            {
                return Card.CardsInDeckCache.Contains(this.ID_User);
            }
        }

        public bool Locked
        {
            get
            {
                foreach (string locked_card in GameClient.Current.LockedCards)
                    if (locked_card == this.ID_User.ToString())
                        return true;

                return false;
            }
        }

        public bool OkayToConsume
        {
            get
            {
                if (!Valid)
                    return false;

                if ((this.Level == 0) && (this.CurrentXP == 0))
                {
                    if (this.InAnyDeck)
                        return false;

                    foreach (string card_excluded in Card.CardsExcludedFromEnchantingWith)
                        if (Utils.CInt(card_excluded) == this.ID_Generic)
                            return false;

                    if (this.Locked)
                        return false;

                    if (this.FoodCard)
                        return Utils.True("Enchant_Cards_AllowFood");

                    bool allowed = false;
                    foreach (string star_allowed in Card.StarsAllowedToEnchantWith)
                        if (Utils.CInt(star_allowed) == this.Stars)
                            allowed = true;
                    if (!allowed)
                        return false;

                    string sElement = this.Element;
                    if ((sElement != "Tundra") && (sElement != "Forest") && (sElement != "Swamp") && (sElement != "Mountain")
                            && (sElement != "Kingdom") && (sElement != "Forest") && (sElement != "Wilderness") && (sElement != "Hell"))
                        return false;

                    return true;
                }

                return false;
            }
        }

        public static string[] StarsAllowedToEnchantWith = null;
        public static string[] CardsExcludedFromEnchantingWith = null;
    }
}
