using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EKUnleashed.GameObjs
{
    class Hero
    {
        public JToken raw_data = null;

        public bool Valid
        {
            get
            {
                if (this.raw_data == null) return false;
                return true;
            }
        }

        public Hero()
        {
            try
            {
                this.raw_data = JObject.Parse(GameClient.Current.GetGameData("user", "GetUserInfo"));
            }
            catch { }
        }

        public Hero(JToken PlayerHero)
        {
            try
            {
                this.raw_data = PlayerHero;
            }
            catch { }
        }

        private int GetValueInt(string property)
        {
            if (!this.Valid)
                return -1;

            try
            {
                return int.Parse(this.raw_data["data"][property].ToString().Replace("\"", "").Trim());
            }
            catch { }

            return -1;
        }

        private long GetValueLong(string property)
        {
            if (!this.Valid)
                return -1;

            try
            {
                return long.Parse(this.raw_data["data"][property].ToString().Replace("\"", "").Trim());
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
                return this.raw_data["data"][property].ToString().Trim();
            }
            catch { }

            return string.Empty;
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

                this._Level = this.GetValueInt("Level");

                return this._Level;
            }
        }

        private int _Energy = -1;
        public int Energy
        {
            get
            {
                if (this._Energy != -1)
                    return this._Energy;

                if (!this.Valid)
                    return -1;

                this._Energy = this.GetValueInt("Energy");

                return this._Energy;
            }
        }
        
        private long _XP = -1;
        public long XP
        {
            get
            {
                if (this._XP != -1)
                    return this._XP;

                if (!this.Valid)
                    return -1;

                this._XP = this.GetValueLong("Exp");

                return this._XP;
            }
        }

        private double _ProgressTowardLevel = -1.0;
        public double ProgressTowardLevel
        {
            get
            {
                try
                {
                    if (this._ProgressTowardLevel != -1)
                        return this._ProgressTowardLevel;

                    if (!this.Valid)
                        return -1;

                    long current_XP = this.XP;
                    long last_level_XP = this.GetValueLong("PrevExp");
                    long next_level_XP = this.GetValueLong("NextExp");

                    long XP_progress = current_XP - last_level_XP;

                    this._ProgressTowardLevel = ((double)XP_progress) / ((double)(next_level_XP - last_level_XP)) * 100.0;

                    return this._ProgressTowardLevel;
                }
                catch { }

                return 0.0;
            }
        }

        private long _PrevExp = -1;
        public long PrevExp
        {
            get
            {
                try
                {
                    if (this._PrevExp != -1)
                        return this._PrevExp;

                    if (!this.Valid)
                        return -1;

                    this._PrevExp = this.GetValueLong("PrevExp");

                    return this._PrevExp;
                }
                catch { }

                return 0;
            }
        }

        private long _NextExp = -1;
        public long NextExp
        {
            get
            {
                try
                {
                    if (this._NextExp != -1)
                        return this._NextExp;

                    if (!this.Valid)
                        return -1;

                    this._NextExp = this.GetValueLong("NextExp");

                    return this._NextExp;
                }
                catch { }

                return 0;
            }
        }
        
        private int _Gems = -1;
        public int Gems
        {
            get
            {
                if (this._Gems != -1)
                    return this._Gems;

                if (!this.Valid)
                    return -1;

                this._Gems = this.GetValueInt("Cash");

                return this._Gems;
            }
        }
        
        private int _FireTokens = -1;
        public int FireTokens
        {
            get
            {
                if (this._FireTokens != -1)
                    return this._FireTokens;

                if (!this.Valid)
                    return -1;

                this._FireTokens = this.GetValueInt("Ticket");

                return this._FireTokens;
            }
        }
        
        private int _Gold = -1;
        public int Gold
        {
            get
            {
                if (this._Gold != -1)
                    return this._Gold;

                if (!this.Valid)
                    return -1;

                this._Gold = this.GetValueInt("Coins");

                return this._Gold;
            }
        }
        
        private int _MaxCost = -1;
        public int MaxCost
        {
            get
            {
                if (this._MaxCost != -1)
                    return this._MaxCost;

                if (!this.Valid)
                    return -1;

                this._MaxCost = this.GetValueInt("LeaderShip");

                return this._MaxCost;
            }
        }

        private int _BlueShards = -1;
        public int BlueShards
        {
            get
            {
                if (this._BlueShards != -1)
                    return this._BlueShards;

                if (!this.Valid)
                    return -1;

                this._BlueShards = this.GetValueInt("Fragment_3");

                return this._BlueShards;
            }
        }


        private int _PurpleShards = -1;
        public int PurpleShards
        {
            get
            {
                if (this._PurpleShards != -1)
                    return this._PurpleShards;

                if (!this.Valid)
                    return -1;

                this._PurpleShards = this.GetValueInt("Fragment_4");

                return this._PurpleShards;
            }
        }

        private int _GoldShards = -1;
        public int GoldShards
        {
            get
            {
                if (this._GoldShards != -1)
                    return this._GoldShards;

                if (!this.Valid)
                    return -1;

                this._GoldShards = this.GetValueInt("Fragment_5");

                return this._GoldShards;
            }
        }
        
        private string _Name = null;
        public string Name
        {
            get
            {
                if (this._Name != null)
                    return this._Name;

                if (!this.Valid)
                    return string.Empty;

                this._Name = this.GetValueString("NickName");

                return this._Name;
            }
        }
        
        private string _InviteCode = null;
        public string InviteCode
        {
            get
            {
                if (this._InviteCode != null)
                    return this._InviteCode;

                if (!this.Valid)
                    return string.Empty;

                this._InviteCode = this.GetValueString("InviteCode");

                return this._InviteCode;
            }
        }

        public int CardSlots
        {
            get
            {
                int card_slots = 3;

                if (this.Level >=  3) card_slots++;
                if (this.Level >=  5) card_slots++;
                if (this.Level >= 10) card_slots++;
                if (this.Level >= 20) card_slots++;
                if (this.Level >= 30) card_slots++;
                if (this.Level >= 35) card_slots++;
                if (this.Level >= 40) card_slots++;

                return card_slots;
            }
        }

        public int RuneSlots
        {
            get
            {
                int rune_slots = 0; 

                if (this.Level >=  6) rune_slots++;
                if (this.Level >= 20) rune_slots++;
                if (this.Level >= 30) rune_slots++;
                if (this.Level >= 40) rune_slots++;

                return rune_slots;
            }
        }
        
        private int _ArenaFightsLeft = -1;
        public int ArenaFightsLeft
        {
            get
            {
                if (this._ArenaFightsLeft != -1)
                    return this._ArenaFightsLeft;

                if (!this.Valid)
                    return -1;

                this._ArenaFightsLeft = this.GetValueInt("RankTimes");

                return this._ArenaFightsLeft;
            }
        }
        
        private int _ThiefFightCooldown = -1;
        public int ThiefFightCooldown
        {
            get
            {
                if (this._ThiefFightCooldown != -1)
                    return this._ThiefFightCooldown;

                if (!this.Valid)
                    return -1;

                this._ThiefFightCooldown = this.GetValueInt("ThievesTimes");

                return this._ThiefFightCooldown;
            }
        }

    }
}
