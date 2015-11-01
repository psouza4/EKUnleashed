using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EKUnleashed.GameObjs
{
    class Rune
    {
        private JToken raw_rune = null;
        public JToken raw_rune_details = null;

        public bool Valid
        {
            get
            {
                if (this.raw_rune_details == null) return false;
                return true;
            }
        }

        public Rune(JToken PlayerRune)
        {
            try
            {
                this.raw_rune = PlayerRune;
                this.raw_rune_details = GameClient.Current.GetRuneByID(this.ID_Generic);
            }
            catch { }
        }

        public Rune(int iGenericRuneID)
        {
            try
            {
                this.raw_rune_details = GameClient.Current.GetRuneByID(iGenericRuneID);
            }
            catch { }
        }

        private int GetValueInt(string property)
        {
            if (!this.Valid)
                return -1;

            try
            {
                return int.Parse(this.raw_rune_details[property].ToString().Replace("\"", "").Trim());
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
                return this.raw_rune_details[property].ToString().Trim();
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
                    if (this.raw_rune != null)
                        return int.Parse(this.raw_rune["RuneId"].ToString().Replace("\"", "").Trim());
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
                    this._Level = int.Parse(this.raw_rune["Level"].ToString().Replace("\"", "").Trim());
                }
                catch { }

                return this._Level;
            }
        }

        public int MaxLevel
        {
            get
            {
                return 4;
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
                    return int.Parse(this.raw_rune["UserRuneId"].ToString().Replace("\"", "").Trim());
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

                this._Name = GetValueString("RuneName");

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
                    return Utils.CInt(this.raw_rune["Exp"]);
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

                this._Element = GameClient.ConvertRuneElementToText(this.ElementValue);

                return this._Element;
            }
        }

        public enum ElementTypes
        {
            Earth,
            Water,
            Air,
            Fire,
            Unknown
        }

        private int _ElementValue = -1;
        public int ElementValue
        {
            get
            {
                if (this._ElementValue != -1)
                    return this._ElementValue;

                this._ElementValue = this.GetValueInt("Property");

                return this._ElementValue;
            }
        }
        public ElementTypes ElementType
        {
            get
            {
                int iElement = this.ElementValue;

                if (iElement == 1) return ElementTypes.Earth;
                if (iElement == 2) return ElementTypes.Water;
                if (iElement == 3) return ElementTypes.Air;
                if (iElement == 4) return ElementTypes.Fire;

                return ElementTypes.Unknown;
            }
        }

        private string _Skill = null;
        public string Skill
        {
            get
            {
                if (this._Skill != null)
                    return this._Skill;

                try
                {
                    int skill = this.GetValueInt("LockSkill" + (this.Level + 1).ToString());
                    if (skill == 0)
                    {
                        this._Skill = string.Empty;
                        return this._Skill;
                    }

                    JObject Jskill = GameClient.Current.GetSkillByID(skill);
                    if (Jskill == null)
                    {
                        this._Skill = string.Empty;
                        return this._Skill;
                    }

                    this._Skill = Jskill["Name"].ToString();
                    return this._Skill;
                }
                catch { }

                this._Skill = "(error)";
                return this._Skill;
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
                return GetValueInt("BaseExp");
            }
        }

        public int EnchantToMaxCostXP
        {
            get
            {
                return this.EnchantToLevelCostXP(4);
            }
        }

        public int EnchantToLevelCostXP(int level)
        {
            if (!this.Valid)
                return 0;

            try
            {
                string sXPArray = Utils.CondenseSpacing(this.raw_rune_details["ExpArray"].ToString().Replace("\r", " ").Replace("\n", " ").Replace("[", " ").Replace("]", " ").Trim());

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
                return this.EnchantToLevelCostGold(4);
            }
        }
        public int EnchantToLevelCostGold(int level)
        {
            if (!this.Valid)
                return 0;

            return this.getGoldNeeded(this.Stars, this.EnchantToLevelCostXP(level));
        }

        private int getGoldNeeded(int stars, int calculated_exp_cost)
        {
	        int[] cost = new int[] { 20, 30, 60, 80, 100 };

	        return calculated_exp_cost * cost[stars - 1];	
        }

        private int getExpNeeded(int stars, int level)
        {
            return level * 100;
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

        public bool OkayToConsume
        {
            get
            {
                if (!Valid)
                    return false;

                if ((this.Level == 0) && (this.CurrentXP == 0))
                {
                    foreach (string rune_excluded in Rune.RunesExcludedFromEnchantingWith)
                        if (Utils.CInt(rune_excluded) == this.ID_Generic)
                            return false;

                    bool allowed = false;
                    foreach (string star_allowed in Rune.StarsAllowedToEnchantWith)
                        if (Utils.CInt(star_allowed) == this.Stars)
                            allowed = true;
                    if (!allowed)
                        return false;

                    return true;
                }

                return false;
            }
        }

        public static string[] StarsAllowedToEnchantWith = null;
        public static string[] RunesExcludedFromEnchantingWith = null;
    }
}
