using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;

namespace EKUnleashed
{
    public class GameClient
    {
        private bool Want_SQL_DataDump      = false;

        public const string settings_file   = "settings.ini";
        public bool Want_Game_Login         = true;

        public GameService Service = GameService.Elemental_Kingdoms;
        public string Login_Device = "ANDROID";

        public string Kingdom_War_ID = ""; // 1 = Tundra, 2 = Forest, 3 = Swamp, 4 = Mountain
        public string Clan_ID = "";
        public string Clan_Name = "";
        public string Login_NickName = "";
        public string Login_UID = "";
        public string ServerName = "";

        public Dictionary<string, Scheduler.ScheduledEvent> ScheduledEvents = new Dictionary<string, Scheduler.ScheduledEvent>();

        public object locker_gamedata = new object();

        public bool Want_Deck_Swap
        {
            get
            {
                if (Utils.GetAppSetting("DemonInvasion_Deck") == "KW")
                    return true;

                if (Utils.CInt(Utils.GetAppSetting("DemonInvasion_Deck")) > 0)
                    return true;

                return false;
            }
        }

        public GameChat Chat = null;
        public string All_Cards_JSON = "";
        public string All_Runes_JSON = "";
        public string All_Skills_JSON = "";
        public string Game_CDN_URL = "http://s1.ek.ifreeteam.com/";

        public const string TAG_EK = "&phpp=ANDROID_ARC&phpl=EN&pvc=1.7.4&pvb=2015-08-07%2018%3A55";
        public static string m_strBuildTime = "2015-08-07 18:55"; //DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        public int seq_id = 1000;
        public object locker = new object();
        public Comm.CommFetchOptions opts = null;

        public string Current_Demon_MeritCard1_ID = "";
        public string Current_Demon_MeritCard2_ID = "";

        private object data_locker = new object();

        public static GameClient Current
        {
            get
            {
                try
                {
                    return frmMain.Game;
                }
                catch { }

                return null;
            }
        }

        public string CurrentGame
        {
            get
            {
                try
                {
                    if (Current.Service == GameService.Lies_of_Astaroth) return "LoA";
                    if (Current.Service == GameService.Magic_Realms) return "MR";
                    if (Current.Service == GameService.Elves_Realm) return "ER";
                    if (Current.Service == GameService.Shikoku_Wars) return "SW";
                }
                catch { }

                return "EK";
            }
        }

        public bool AutomaticDIDeckSwapOnCooldown
        {
            get
            {
                return Utils.True("DemonInvasion_AvoidSniping");
            }
        }

        public enum GameService
        {
            Elemental_Kingdoms,
            Lies_of_Astaroth,
            Magic_Realms,
            Elves_Realm,
            Shikoku_Wars
        }

        public void StopEverything()
        {
            lock (this.locker)
            {
                try { this.StopAllEvents(); } catch { }
                try { this.ScheduledEvents.Clear(); } catch { }
                try { if (this.Chat != null) this.Chat.Logout(); } catch { }

                return;
            }
        }

        public string MeditateReward(string json)
        {
            try
            {
                JObject json_data = JObject.Parse(json);

                if (Utils.CInt(json_data["status"].ToString()) == 0)
                {
                    if (json_data["message"].ToString().Contains("clear some space"))
                        return "full";
                    return "error: " + json_data["message"].ToString();
                }

                int reward_type = Utils.CInt(json_data["data"]["AwardItem"]["Type"].ToString());
                int which_reward = Utils.CInt(json_data["data"]["AwardItem"]["Value"].ToString());

                string[] FRAGMENTS = null;
                string[] GRAY_RUNES = null;

                if (this.Service == GameService.Lies_of_Astaroth || this.Service == GameClient.GameService.Elves_Realm)
                {
                    FRAGMENTS = new string[] { "0", "1", "2", "<color=#5050ff><b>blue scrap</b></color>", "<color=#B030FF><b>purple scrap</b></color>", "<color=#FFB730><b>golden scrap</b></color>" };
                    GRAY_RUNES = new string[] { "0", "<color=808080>broken scrap</color>", "<color=808080>lost scrap</color>", "<color=808080>odd scrap</color>", "<color=808080>broken scrap</color>", "<color=808080>child's drawing</color>" };
                }
                else
                {
                    FRAGMENTS = new string[] { "0", "1", "2", "<color=#5050ff><b>blue fragment</b></color>", "<color=#B030FF><b>purple fragment</b></color>", "<color=#FFB730><b>golden fragment</b></color>" };
                    GRAY_RUNES = new string[] { "0", "<color=808080>broken shard</color>", "<color=808080>lost shard</color>", "<color=808080>odd shard</color>", "<color=808080>broken shard</color>", "<color=808080>child's drawing</color>" };
                }

                switch (reward_type)
                {
                    case 1: // fragments
                        return FRAGMENTS[which_reward];

                    case 2: // regular runes
                        JObject rune_details = this.GetRuneByID(which_reward);
                        return "<b>" + rune_details["RuneName"].ToString() + "</b> (" + rune_details["Color"].ToString() + "★ " + rune_details["Property"].ToString().Replace("1", "Earth").Replace("2", "Water").Replace("3", "Air").Replace("4", "Fire") + " rune)";

                    case 3: // gray runes
                        return GRAY_RUNES[which_reward];
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.GetType().ToString() + ": " + ex.Message;
            }

            return "<color=#ff4040>unknown</color>";
        }

        public void HelpText()
        {
            Utils.Chatter("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
            Utils.Chatter("<b><fs+><fs+><fs+>Elemental Kingdoms Tools Help<fx></b></color>");
            Utils.Chatter("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
            Utils.Chatter("<color=#00efff>F1\t\tThis help text</color>");
            Utils.Chatter("<color=#00efff>F2\t\tSend & receive friend energy</color>");

            string towers_to_fight = Utils.GetAppSetting("Game_MazeTowers").Trim().Replace(" ", string.Empty);

            if (!Utils.ValidText(towers_to_fight))
                towers_to_fight = "10, 9, 8, 7, 6";

            Utils.Chatter("<color=#00efff>F3\t\tFight tower mazes from maps " + towers_to_fight + ", then explore</color>");
            Utils.Chatter("<color=#00efff>F4\t\tExplore (auto-selects the best unlocked map stage)</color>");
            Utils.Chatter("<color=#00efff>F5\t\tFight a ranked arena battle</color>");
            Utils.Chatter("<color=#00efff>F6\t\tFight a thief</color>");
            Utils.Chatter("<color=#00efff>F7\t\tFight daily map invasions</color>");
            Utils.Chatter("<color=#00efff>F8\t\tFight the demon invasion boss</color>");
            Utils.Chatter("<color=#00efff>F9\t\tFight world tree battles</color>");
            Utils.Chatter("<color=#00efff>F10\t\tSearch for cards</color>");
            Utils.Chatter("<color=#00efff>F11\t\tSearch for runes</color>");
            Utils.Chatter("<color=#00efff>P\t\tSend a private message to somebody (PM)</color>");
            Utils.Chatter("<color=#00efff>C\t\tSend your clan a message</color>");
            Utils.Chatter("<color=#00efff>T\t\tSend a private message to the same person as last time (re-tell)</color>");
            Utils.Chatter("<color=#00efff>R\t\tSend a private message to the last person to message you (reply)</color>");
            Utils.Chatter("<color=#00efff>Alt+D\t\tQuick-build a deck</color>");
            Utils.Chatter("<color=#00efff>Alt+F1\t\tLogin (in case of disconnection)</color>");
            Utils.Chatter("<color=#00efff>Alt+F2\t\tLogout and stop all events</color>");
            Utils.Chatter("<color=#00efff>Enter\t\tSend a message to general chat</color>");
            Utils.Chatter("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
        }        

        public void Play_FieldOfHonorSpins()
        {
            lock (this.locker_gamedata)
            {
                //this.GetGameData("league", "lotteryInfo");

                for (int i = 0; i < 100; i++)
                {
                    JObject lottery_result = JObject.Parse(this.GetGameData("league", "lottery"));

                    if (Utils.CInt(lottery_result["status"]) != 1)
                        break;

                    Utils.LoggerNotifications("<color=#ffa000>Field of Honor 'happy hour' spin:</color>");
                    Utils.LoggerNotifications("<color=#ffa000>    " + new GameReward(lottery_result).AllAwards + "</color>");
                }
            }

            return;
        }

        public void Play_DailyTasks()
        {
            try
            {
                int iPoints = 0;

                JObject all_task_data = JObject.Parse(this.GetGameData("task", "GetDailyTaskInfo"));

                for (int iMajorTaskID = 0; iMajorTaskID <= 10; iMajorTaskID++)
                {
                    for (int iMinorTaskID = 0; iMinorTaskID <= 10; iMinorTaskID++)
                    {
                        string sTaskID = ((iMajorTaskID * 100) + iMinorTaskID).ToString();

                        try
                        {
                            JToken task_data = all_task_data["data"]["ActionRecord"][sTaskID];

                            //Utils.Chatter("Task: " + task_data["Desc"].ToString());
                            //Utils.Chatter("    finished: " + ((Utils.CInt(task_data["Done"]) == 1) ? "yes" : "no"));

                            if (Utils.CInt(task_data["Done"]) == 0)
                            {
                                string task_desc = task_data["Desc"].ToString().ToLower();

                                if (task_desc.Contains("gold pack"))
                                {
                                    // get the gold packs for free from the 5-per-day free gold packs

                                    this.Play_DailyFreeCards();
                                }
                                else if (task_desc.Contains("meditate"))
                                {
                                    this.Play_DoDailyTemple();
                                }
                                else if (task_desc.Contains("enchant"))
                                {
                                    this.Play_DoDailyEnchantAndSell();
                                }
                            }
                            else
                            {
                                iPoints += Utils.CInt(task_data["Point"]);
                            }
                        }
                        catch { }
                    }
                }

                List<int> AlreadyAwarded = new List<int>();
                foreach (JToken task_reward_point in ((JArray)all_task_data["data"]["Rewarded"]))
                    AlreadyAwarded.Add(Utils.CInt(task_reward_point));

                if ((iPoints >= 40) && (!AlreadyAwarded.Contains(40))) this.Play_DailyClaimTaskReward(40);
                if ((iPoints >= 60) && (!AlreadyAwarded.Contains(60))) this.Play_DailyClaimTaskReward(60);
                if ((iPoints >= 80) && (!AlreadyAwarded.Contains(80))) this.Play_DailyClaimTaskReward(80);
                if ((iPoints >= 100) && (!AlreadyAwarded.Contains(100))) this.Play_DailyClaimTaskReward(100);

                AlreadyAwarded.Clear();
            }
            catch { }
        }

        public void Play_DailyClaimTaskReward(int iPoint)
        {
            lock (this.locker_gamedata)
            {
                try
                {
                    string s = this.GetGameData("task", "GetDailyTaskReward", "point=" + iPoint.ToString());

                    Utils.Chatter(s);

                    JObject task_reward = JObject.Parse(s);

                    if (Utils.CInt(task_reward["status"]) == 1)
                    {
                        // nothing to do here.. rewards will be sent to the reward chest (queue), which should then be picked up by
                        // the chat notification as a claimable reward
                    }
                }
                catch { }
            }
        }

        public void Play_DoDailyTemple()
        {
            // meditate 5 times

            this.Play_TempleMeditation(100000, 5);
        }

        public void Play_DoDailyEnchantAndSell()
        {
            lock (this.locker_gamedata)
            {
                // buy a gold pack
                // (assume this has already been done by buying a free gold pack)

                // grab a fresh user card list
                this.UserCards_CachedData = null;
                JObject cards = GameClient.Current.GetUsersCards();

                // find a 1* card                
                GameObjs.Card card_A = null;
                foreach (var jCard in cards["data"]["Cards"])
                {
                    GameObjs.Card temp_card = new GameObjs.Card(jCard);
                    if ((temp_card.CurrentXP == 0) && (temp_card.Stars == 1))
                    {
                        card_A = temp_card;
                        break;
                    }
                }

                // couldn't find a destination card to enchant
                if (card_A == null)
                    return;

                // find a different 1* card
                GameObjs.Card card_B = null;
                foreach (var jCard in cards["data"]["Cards"])
                {
                    GameObjs.Card temp_card = new GameObjs.Card(jCard);
                    if ((temp_card.ID_User != card_A.ID_User) && (temp_card.CurrentXP == 0) && (temp_card.Stars == 1))
                    {
                        card_B = temp_card;
                        break;
                    }
                }

                // couldn't find a source card to enchant with
                if (card_B == null)
                    return;

                // enchant the first 1* card with the second 1* card
                 this.GetGameData("streng", "Card", "UserCardId1=" + card_A.ID_User.ToString() + "&UserCardId2=" + card_B.ID_User.ToString(), false);

                // sell the first 1* card
                GameClient.Current.GetGameData("card", "SaleCardRunes", "Cards=" + card_A.ID_User.ToString());

                // clear the user card list cache
                this.UserCards_CachedData = null;
            }
        }

        public void Play_DailyFreeCards()
        {
            lock (this.locker_gamedata)
            {
                JObject shop_data = JObject.Parse(this.GetGameData("shopnew", "GetGoods", true));

                try
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        try
                        {
                            JToken shop_item = shop_data["data"]["oldgood"][i.ToString()];

                            if (Utils.CInt(shop_item["MaxFreeTimes"]) > 0)
                            {
                                // something they can get for free

                                if ((Utils.CInt(shop_item["RemainTime"]) == 0) && (Utils.CInt(shop_item["FreeTimes"]) < Utils.CInt(shop_item["MaxFreeTimes"])))
                                {
                                    // ... and cooldown is up and they have remaining attempts

                                    int goods_id = Utils.CInt(shop_item["GoodsId"]);

                                    JObject cards_received = JObject.Parse(this.GetGameData("shopnew", "FreeBuy", "GoodsId=" + goods_id.ToString()));

                                    string[] card_ids = Utils.SubStringsDups(cards_received["data"]["CardIds"].ToString(), "_");
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

                                    Utils.LoggerNotifications("<color=#ffa000>You bought a <b><u>free</u></b> " + shop_item["Name"].ToString() + " and received...</color>");
                                    Utils.LoggerNotifications("<color=#ffa000>      " + all_cards + "</color>");
                                    if (bFiveStar)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new 5★ card!</color>");
                                    if (bEventCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new event card!</color>");
                                    if (bTreasureCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new treasure card!</color>");
                                    if (bFoodCard)
                                        Utils.LoggerNotifications("<color=#ffff40>      <b>CONGRATULATIONS</b> on your new food card!</color>");
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        // This doesn't work (never has)
        public void Play_AutomaticallyCraftCards()
        {
            lock (this.locker_gamedata)
            {
                string js_CardCraft = this.GetGameData("cardchip", "GetCardChip");

                try
                {
                    JObject user_data = JObject.Parse(this.GetGameData("user", "GetUserInfo"));
                    int gold_have = Utils.CInt(user_data["data"]["Coins"]);

                    foreach (JToken jtCraftable in JObject.Parse(js_CardCraft)["data"]["CardChips"])
                    {
                        //GameObjs.Card card = new GameObjs.Card(Utils.CInt(jtCraftable["CardId"]));
                        int chips_have = Utils.CInt(jtCraftable["ChipNum"]);
                        int chips_needed = Utils.CInt(jtCraftable["ChipAmount"]);
                        int gold_needed = Utils.CInt(jtCraftable["Coins"]);

                        //Utils.Chatter("You have " + chips_have.ToString("#,##0") + " of " + chips_needed.ToString("#,##0") + " fragments needed to craft [Card #" + jtCraftable["CardId"].ToString() + "].");

                        if ((chips_have >= chips_needed) && (gold_needed >= gold_have))
                        {
                            this.GetGameData("cardchip", "ComposeChip", "ChipId=" + jtCraftable["CardId"].ToString() + "&IsSuper=0");

                            Utils.Chatter("<color=#ffa000>Automatically crafted a [Card #" + jtCraftable["CardId"].ToString() + "] using <b>" + chips_have.ToString("#,##0") + "</b> fragments.</color>");
                        }
                    }
                }
                catch { }
            }
        }


        public void Play_FightRaider_Hydra()
        {
            lock (this.locker_gamedata)
            {
                this.Play_FightRaider_Hydra__real();
            }
        }

        private bool bAnnouncedHydraCD = false;
        private void Play_FightRaider_Hydra__real()
        {
            string hydraListTest = this.GetGameData("hydra", "GetHydraInfo", false);
            if (!hydraListTest.StartsWith("{"))
                return;

            List<string> hydra_clanmate_blacklist = new List<string>();
            try
            {
                hydra_clanmate_blacklist.AddRange(Utils.CondenseSpacing(Utils.GetAppSetting("Hydra_IgnoreFrom")).Replace(", ", ",").Replace("\t", "").Split(new char[] { ',' }));
            }
            catch { }

            JObject hydraListTestJSON = JObject.Parse(hydraListTest);

            // If Hydra event isn't ongoing, abort
            if (Utils.CInt(hydraListTestJSON["status"]) == 0)
                return;

            // find out how many hydras we can attack (for quicker/less-spammy deck swapping)
            int hydra_attack_count = 0;
            int iTempHydraID = 0;
            foreach (JObject hydraThief in hydraListTestJSON["data"]["hydraList"])
            {
                int hp = Utils.CInt(hydraThief["HPCurrent"]);
                int flee_seconds_remaining = Utils.CInt(hydraThief["FleeTime"].ToString());

                if ((flee_seconds_remaining > 0) && (hp > 0))
                {
                    if (iTempHydraID == 0)
                        iTempHydraID = Utils.CInt(hydraThief["UserHydraId"]);

                    hydra_attack_count++;
                }
            }

            int hydra_deck_to_use = 0;
            bool bSkipFight = Utils.GetAppSetting("Hydra_AutomationMode").ToLower().Trim() == "claim only";
            bool bSkipClaim = Utils.GetAppSetting("Hydra_AutomationMode").ToLower().Trim() == "fight only";

            if (hydra_attack_count > 0)
            {
                string hydraUserInfo = this.GetGameData("hydra", "GetUserHydraInfo", "userHydraId=" + iTempHydraID.ToString(), false);
                JObject hydraUserInfoJSON = JObject.Parse(hydraUserInfo);
                int current_cooldown = Utils.CInt(hydraUserInfoJSON["data"]["userHydraInfo"]["CDTime"]);
                int cd_time_status = Utils.CInt(hydraUserInfoJSON["data"]["userHydraInfo"]["CDTimeStatus"]);
                if ((current_cooldown > 0) && (cd_time_status == 0))
                {
                    bSkipFight = true;

                    if (!bAnnouncedHydraCD)
                    {
                        Utils.LoggerNotifications("<color=#a07000>Raider on cooldown for " + Comm.PrettyTimeLeftShort((long)current_cooldown) + " (ends at " + DateTime.Now.AddSeconds(current_cooldown).ToShortTimeString() + ")</color>");
                        bAnnouncedHydraCD = true;
                    }
                }

                if (!bSkipFight)
                {
                    bAnnouncedHydraCD = false;
                    hydra_deck_to_use = Utils.CInt(this.DeckToUseForRaiders());

                    if (hydra_deck_to_use > 0)
                    {
                        this.GetGameData("card", "SetDefalutGroup", "GroupId=" + hydra_deck_to_use.ToString(), false); // switch to hydra deck
                        Utils.LoggerNotifications("<color=#a07000>Switched to raider deck</color>");
                    }
                    else if (Utils.CInt(this.DefaultDeck) > 0)
                    {
                        this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                        Utils.LoggerNotifications("<color=#a07000>Switched to default deck to attack raiders</color>");
                    }
                }
            }

        restart_fights__get_list:

            bool bHitAnyHydraAtAll = false;

            string hydraList = hydraListTest;
            JObject hydraListJSON = hydraListTestJSON;

            if (hydraList == string.Empty)
            {
                hydraList = this.GetGameData("hydra", "GetHydraInfo", false);
                hydraListJSON = JObject.Parse(hydraList);
            }
            else
            {
                hydraListTest = string.Empty;
                hydraListTestJSON = null;
            }

            // If Hydra event isn't ongoing, abort
            if (Utils.CInt(hydraListJSON["status"]) == 0)
                return;

            if (!bSkipClaim)
            {
                foreach (JObject hydraThief in hydraListJSON["data"]["hydraList"])
                {
                    int hydraID = Utils.CInt(hydraThief["UserHydraId"]);
                    int hp = Utils.CInt(hydraThief["HPCurrent"].ToString());
                    int flee_seconds_remaining = Utils.CInt(hydraThief["FleeTime"].ToString());

                    if (Utils.CInt(hydraThief["enableAward"]) != 0)
                    {
                        string hydraPointReward = this.GetGameData("hydra", "GetHydraPointReward", "userHydraId=" + hydraID.ToString(), false);
                        JObject hydraPointRewardJSON = JObject.Parse(hydraPointReward);

                        if (Utils.CInt(hydraPointRewardJSON["status"]) == 1)
                        {
                            string reward_DP_kill = "0"; try { reward_DP_kill = Utils.CInt(hydraPointRewardJSON["data"]["pointAward"]["kill"]).ToString(); } catch { }
                            string reward_DP_finder = "0"; try { reward_DP_finder = Utils.CInt(hydraPointRewardJSON["data"]["pointAward"]["finder"]).ToString(); } catch { }
                            string reward_DP_mvp = "0"; try { reward_DP_mvp = Utils.CInt(hydraPointRewardJSON["data"]["pointAward"]["mvp"]).ToString(); } catch { }
                            string reward_DP_lastAttack = "0"; try { reward_DP_lastAttack = Utils.CInt(hydraPointRewardJSON["data"]["pointAward"]["lastAttack"]).ToString(); } catch { }
                            string reward_DP_card = "0"; try { reward_DP_card = Utils.CInt(hydraPointRewardJSON["data"]["pointAward"]["card"]).ToString(); } catch { }

                            string output_rewards = "";
                            if (Utils.CInt(reward_DP_kill) > 0) output_rewards += ", " + Utils.CInt(reward_DP_kill).ToString("#,##0") + " DP from kill";
                            if (Utils.CInt(reward_DP_finder) > 0) output_rewards += ", " + Utils.CInt(reward_DP_finder).ToString("#,##0") + " DP from finding raider";
                            if (Utils.CInt(reward_DP_mvp) > 0) output_rewards += ", " + Utils.CInt(reward_DP_mvp).ToString("#,##0") + " DP as MVP";
                            if (Utils.CInt(reward_DP_lastAttack) > 0) output_rewards += ", " + Utils.CInt(reward_DP_lastAttack).ToString("#,##0") + " DP from last hit";
                            if (Utils.CInt(reward_DP_card) > 0) output_rewards += ", " + Utils.CInt(reward_DP_card).ToString("#,##0") + " DP bonuses from event cards";

                            int reward_DP_all = Utils.CInt(reward_DP_kill) + Utils.CInt(reward_DP_finder) + Utils.CInt(reward_DP_mvp) + Utils.CInt(reward_DP_lastAttack) + Utils.CInt(reward_DP_card);

                            if (output_rewards.Length > 0)
                            {
                                string hydraCardLink = "[<link><text>" + hydraThief["HydraName"].ToString() + "</text><url>||EKU||CARD||EKU||" + hydraThief["HydraAvatar"].ToString() + "||EKU||10</url></link>]";

                                Utils.LoggerNotifications("<color=#ffa000><b>" + hydraCardLink + "</b> final reward:  <b>" + reward_DP_all.ToString("#,##0") + " DP</b> (" + output_rewards.Substring(2) + ")</color>");
                            }
                        }
                    }
                }
            }

        restart_fights:

            bool bHitAnyHydraThisRound = false;

            if (!bSkipFight)
            {
                foreach (JObject hydraThief in hydraListJSON["data"]["hydraList"])
                {
                    int hydraID = Utils.CInt(hydraThief["UserHydraId"]);
                    int hp = Utils.CInt(hydraThief["HPCurrent"].ToString());
                    int flee_seconds_remaining = Utils.CInt(hydraThief["FleeTime"].ToString());

                    if ((flee_seconds_remaining > 0) && (hp > 0))
                    {
                        // Redundant: this info mirrors what we already have in 'hydraThief' (from 'GetHydraInfo')
                        //string hydraDetails = this.GetGameData("hydra", "GetUserHydraInfo", "userHydraId=" + hydraID.ToString(), false);
                        //JObject hydraDetailsJSON = JObject.Parse(hydraDetails);

                        if ((!Utils.False("Hydra_OnlyFightMine")) || ((Utils.CInt(this.Login_UID) == Utils.CInt(hydraThief["Uid"]))))
                        {
                            bool skip_this_fight = false;

                            string discovered_by = hydraThief["NickName"].ToString();
                            foreach (string blacklisted_clanmate in hydra_clanmate_blacklist)
                            {
                                if ((TextComparison.IsExactMatch(blacklisted_clanmate, discovered_by)) || (discovered_by.Trim().ToLower() == blacklisted_clanmate.Trim().ToLower()))
                                {
                                    skip_this_fight = true;
                                    break;
                                }
                            }

                            if (!skip_this_fight)
                            {
                                string hydraFight = this.GetGameData("hydra", "HydraFight", "userHydraId=" + hydraID.ToString(), false);
                                JObject hydraFightJSON = JObject.Parse(hydraFight);

                                if (Utils.CInt(hydraFightJSON["status"]) == 1)
                                {
                                    string hydraCardLink = "[<link><text>" + hydraThief["HydraName"].ToString() + "</text><url>||EKU||CARD||EKU||" + hydraThief["HydraAvatar"].ToString() + "||EKU||10</url></link>]";

                                    string reward_gold = "0"; try { reward_gold = Utils.CInt(hydraFightJSON["data"]["ExtData"]["Award"]["Coins"]).ToString(); } catch { }
                                    string reward_DP = "0"; try { reward_DP = Utils.CInt(hydraFightJSON["data"]["ExtData"]["Award"]["attackPoint"]).ToString(); } catch { }

                                    Utils.LoggerNotifications("<color=#ffa000><b>" + hydraCardLink + "</b> discovered by " + ((Utils.CInt(this.Login_UID) == Utils.CInt(hydraThief["Uid"])) ? "you" : hydraThief["NickName"].ToString()) + " battle " + ((Utils.CInt(reward_gold) > 0) ? "win" : "loss") + ": " + new GameReward(hydraFightJSON).AllAwards + "</color>");

                                    bHitAnyHydraAtAll = true;
                                    bHitAnyHydraThisRound = true;
                                }
                            }
                        }
                    }
                }
            }

            if (bHitAnyHydraThisRound) goto restart_fights;
            if (bHitAnyHydraAtAll)     goto restart_fights__get_list;

            if (!bSkipClaim)
            {
                string hydraFriendContributedList = this.GetGameData("hydra", "GetFriendContributeList", false);
                JObject hydraFriendContributedListJSON = JObject.Parse(hydraFriendContributedList);

                foreach (JObject hydraFriend in hydraFriendContributedListJSON["data"]["friendPointsList"])
                {
                    string friendName = hydraFriend["nickName"].ToString();
                    int friendID = Utils.CInt(hydraFriend["friendId"]);
                    int contributionPoints = Utils.CInt(hydraFriend["contributePoint"]);

                    string hydraClaimFriendContribution = this.GetGameData("hydra", "GetContributePoints", "friendUid=" + friendID.ToString(), false);
                    JObject hydraClaimFriendContributionJSON = JObject.Parse(hydraClaimFriendContribution);

                    int contributionPoints2 = Utils.CInt(hydraClaimFriendContributionJSON["data"]["point"]);

                    Utils.LoggerNotifications("<color=#ffa000>Raider clanmate contribution: <b>" + contributionPoints2.ToString("#,##0") + "</b> DP from " + friendName + " (their total is " + Utils.CInt(hydraFriend["totalPoint"]).ToString("#,##0") + " DP)</color>");
                }
            }

            if (Utils.CInt(this.DefaultDeck) > 0 && (hydra_deck_to_use > 0))
            {
                this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                Utils.LoggerNotifications("<color=#a07000>Switched back to default deck</color>");
            }
        }

        private bool KW_InQueue = false;
        private DateTime KW_InQueueSince = DateTime.MinValue;
        private DateTime KW_CooldownExpires = DateTime.MinValue;
        private bool KW_TeamHasBeenEliminated = false;
        public bool KW_Ongoing = false;
        private int KW_LastBattlesWon = 0;
        private int KW_LastBattlesLost = 0;
        private int KW_ForceID = 0;
        private int KW_LastHonor = 0;
        private object KW_locker_callback = new object();
        private object KW_locker_fight = new object();

        public void KingdomWar_NotificationCallback(int msgid, string msg)
        {
            lock (this.KW_locker_callback)
            {
                try
                {
                    if (msgid == -1) // point the player was attacking/defending switched forces during queue
                    {
                        Utils.Logger("<color=#ffa000>... Kingdom War queue failed because the point's kingdom ownership changed</color>");
                        Utils.StartMethodMultithreaded(this.KingdomWar_Fight);
                    }

                    if (msgid == -2) // war has ended for today
                        Utils.StartMethodMultithreaded(this.KingdomWar_WarEnds);

                    if (msgid == -3) // war has started
                    {
                        if (!this.KW_Ongoing)
                        {
                            Utils.StartMethodMultithreaded(() =>
                            {
                                try
                                {
                                    string kwMap = this.GetGameData("forcefight", "ShowForceMap", false);
                                    JObject kwMapJSON = JObject.Parse(kwMap);

                                    if (Utils.CInt(kwMapJSON["status"]) == 0)
                                    {
                                        // KW has ended or hasn't started yet
                                        this.KW_Ongoing = false;
                                        this.KingdomWar_Reset();
                                        return;
                                    }

                                    this.KW_ForceID = Utils.CInt(kwMapJSON["data"]["force_id"]);
                                    this.Kingdom_War_ID = kwMapJSON["data"]["force_id"].ToString();
                                }
                                catch { }

                                this.KingdomWar_WarBegins();
                            });
                        }
                    }

                    if (msgid == 203) // attack or defense combat ended
                    {
                        KW_InQueue = false;

                        JObject msgResultJSON = JObject.Parse(msg);

                        int new_honor_total = Utils.CInt(msgResultJSON["user_medal"]);
                        int battles_won = Utils.CInt(msgResultJSON["battle_win"]);
                        int battles_lost = Utils.CInt(msgResultJSON["battle_dead"]);
                        int user_life = Utils.CInt(msgResultJSON["user_life"]);
                        int cooldown = Utils.CInt(msgResultJSON["user_cd"]);

                        if (cooldown > 0)
                            this.KW_CooldownExpires = DateTime.Now.AddSeconds(cooldown);

                        bool won_this_fight = false;
                        if (this.KW_LastBattlesWon < battles_won)
                            won_this_fight = true;

                        int honor_gained = new_honor_total - this.KW_LastHonor;

                        if (this.KW_LastBattlesWon < battles_won)
                            this.KW_LastBattlesWon = battles_won;
                        if (this.KW_LastBattlesLost < battles_lost)
                            this.KW_LastBattlesLost = battles_lost;
                        if (this.KW_LastHonor < new_honor_total)
                            this.KW_LastHonor = new_honor_total;

                        Utils.LoggerNotifications("<color=#ffa000>Kingdom War battle " + ((won_this_fight) ? "won" : "lost") + ": you gain " + honor_gained.ToString("#,##0") + " honor!</color>");
                        Utils.LoggerNotifications("<color=#ffa000>... win/lose: " + this.KW_LastBattlesWon.ToString() + "-" + this.KW_LastBattlesLost.ToString() + "</color>");

                        if (cooldown > 0)
                        {
                            if (honor_gained > 0)
                                Utils.LoggerNotifications("<color=#ffa000>...... on cooldown for " + Comm.PrettyTimeLeftShort((long)cooldown) + " (ends at " + this.KW_CooldownExpires.ToShortTimeString() + ")</color>");
                            else
                                Utils.LoggerNotifications("<color=#ffa000>Kingdom War on cooldown for " + Comm.PrettyTimeLeftShort((long)cooldown) + " (ends at " + this.KW_CooldownExpires.ToShortTimeString() + ")</color>");

                            this.KingdomWar_CooldownNotified = true;

                            try { this.ScheduledEvents.Add("KW Fight", new Scheduler.ScheduledEvent("KW Fight", this.KingdomWar_Fight, GameClient.DateTimeNow.AddSeconds(cooldown))); }
                            catch { }
                        }
                        else
                        {
                            this.KingdomWar_CooldownNotified = false;

                            Utils.StartMethodMultithreaded(this.KingdomWar_Fight);
                        }
                    }

                    if (msgid == 204) // KW map point update (just in case we logged into EKU in the middle of a KW and missed the started signal)
                    {
                        if (!this.KW_Ongoing)
                        {
                            try
                            {
                                string kwMap = this.GetGameData("forcefight", "ShowForceMap", false);
                                JObject kwMapJSON = JObject.Parse(kwMap);

                                if (Utils.CInt(kwMapJSON["status"]) == 0)
                                {
                                    // KW has ended or hasn't started yet
                                    this.KW_Ongoing = false;
                                    this.KingdomWar_Reset();
                                    return;
                                }

                                this.KW_ForceID = Utils.CInt(kwMapJSON["data"]["force_id"]);
                                this.Kingdom_War_ID = kwMapJSON["data"]["force_id"].ToString();
                            }
                            catch { }

                            Utils.StartMethodMultithreaded(this.KingdomWar_WarBegins);
                        }
                    }

                    if (msgid == 205) // KW score update
                    {
                        JObject msgResultJSON = JObject.Parse(msg);

                        int tundra_score = Utils.CInt(msgResultJSON["1"]);
                        int forest_score = Utils.CInt(msgResultJSON["2"]);
                        int swamp_score = Utils.CInt(msgResultJSON["3"]);
                        int mountain_score = Utils.CInt(msgResultJSON["4"]);

                        Utils.LoggerNotifications("<color=#ffa000>Kingdom War score:</color>");
                        Utils.LoggerNotifications("<color=#ffa000>... " + ConvertCardElementToText(1) + " has " + tundra_score.ToString("#,##0") + " points</color>");
                        Utils.LoggerNotifications("<color=#ffa000>... " + ConvertCardElementToText(2) + " has " + forest_score.ToString("#,##0") + " points</color>");
                        Utils.LoggerNotifications("<color=#ffa000>... " + ConvertCardElementToText(3) + " has " + swamp_score.ToString("#,##0") + " points</color>");
                        Utils.LoggerNotifications("<color=#ffa000>... " + ConvertCardElementToText(4) + " has " + mountain_score.ToString("#,##0") + " points</color>");
                    }

                }
                catch { }
            }
        }

        public static string ConvertCardElementToText(object element)
        {
            int iRace = Utils.CInt(element);

            if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
            {
                if (iRace == 1) return "Tundra";
                if (iRace == 2) return "Forest";
                if (iRace == 3) return "Swamp";
                if (iRace == 4) return "Mountain";
                if (iRace == 95) return "Treasure";
                if (iRace == 96) return "Food";
                if (iRace == 97) return "Raider";
                if (iRace == 99) return "Activity";
                if (iRace == 100) return "Demon";
            }
            else
            {
                if (iRace == 1) return "Kingdom";
                if (iRace == 2) return "Forest";
                if (iRace == 3) return "Wilderness";
                if (iRace == 4) return "Hell";
                if (iRace == 95) return "Treasure";
                if (iRace == 96) return "Food";
                if (iRace == 97) return "Raider";
                if (iRace == 99) return "Resource";
                if (iRace == 100) return "Demon";
            }

            return "Unknown " + element;
        }

        public static string ConvertRuneElementToText(object element)
        {
            int iRace = Utils.CInt(element);

            if (iRace == 1) return "Earth";
            if (iRace == 2) return "Water";
            if (iRace == 3) return "Air";
            if (iRace == 4) return "Fire";

            return "Unknown " + element;
        }

        public void KingdomWar_LoadTargets()
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

            List<KW_Point> tmpPoints = new List<KW_Point>();

            foreach (string node_details in Utils.SubStrings(sPreferedAttackTargets, "||"))
            {
                if (!Utils.ValidText(node_details)) continue;
                if (!node_details.Contains('_')) continue;

                tmpPoints.Add(this.ConvertSettingPOI(node_details));
            }

            GameClient.KW_AttackTargets = tmpPoints.ToArray();
            tmpPoints.Clear();
        }

        public void KingdomWar_Reset()
        {
            this.KW_InQueue = false;
            this.KW_InQueueSince = DateTime.MinValue;
            this.KW_CooldownExpires = DateTime.MinValue;
            this.KW_TeamHasBeenEliminated = false;
            this.KW_LastBattlesWon = 0;
            this.KW_LastBattlesLost = 0;
            this.KW_LastHonor = 0;

            this.KingdomWar_LoadTargets();
        }

        public void KingdomWar_WarBegins()
        {
            this.KingdomWar_Reset();

            this.KW_Ongoing = true;
            frmMain.ext().KWChatActivator(this.KW_ForceID);

            this.KingdomWar_Fight();
        }

        public void KingdomWar_WarEnds()
        {
            this.KW_Ongoing = false;
        }

        public bool KingdomWar_InCooldown
        {
            get
            {
                if (this.KW_CooldownExpires == DateTime.MinValue)
                    return false;

                if (DateTime.Now.CompareTo(this.KW_CooldownExpires) < 0)
                    return false;

                return false;
            }
        }

        public static KW_Point[] KW_AttackTargets = new KW_Point[]
        {
            KW_Point.Fire_Furnace, KW_Point.Divine_Throne, KW_Point.Fountain_of_Life, KW_Point.Plague_Burrow,
            KW_Point.Sea_of_Lava, KW_Point.Sunset_Hills, KW_Point.Squall_Mountains, KW_Point.Guardian_Statues, KW_Point.Magic_Academy, KW_Point.Moon_Valley, KW_Point.Toxic_Springs, KW_Point.Gray_Swamp,
            KW_Point.Holy_War_Remains,
            KW_Point.Crater, KW_Point.Kings_Temple, KW_Point.Elves_Altar, KW_Point.Wild_Fog,
            KW_Point.Flame_Peak, KW_Point.Black_City_Gate, KW_Point.Wind_Setsurei, KW_Point.Barbarian_Outpost, KW_Point.Daylight_Crossing, KW_Point.Jade_Stream, KW_Point.Nether_Trail, KW_Point.Wraith_Burial,
            KW_Point.Horror_Peaks, KW_Point.Thunder_Tower, KW_Point.Mystic_Wetlands, KW_Point.Magic_Forest,
            KW_Point.Mystical_Fortress, KW_Point.Sunset_City, KW_Point.Pebble_Bay, KW_Point.Thorn_Prairie
        };

        public static KW_Point[] KW_DefendTargets = new KW_Point[]
        {
            KW_Point.Fire_Furnace, KW_Point.Divine_Throne, KW_Point.Fountain_of_Life, KW_Point.Plague_Burrow,
            KW_Point.Sea_of_Lava, KW_Point.Sunset_Hills, KW_Point.Squall_Mountains, KW_Point.Guardian_Statues, KW_Point.Magic_Academy, KW_Point.Moon_Valley, KW_Point.Toxic_Springs, KW_Point.Gray_Swamp,
            KW_Point.Holy_War_Remains,
            KW_Point.Crater, KW_Point.Kings_Temple, KW_Point.Elves_Altar, KW_Point.Wild_Fog,
            KW_Point.Flame_Peak, KW_Point.Black_City_Gate, KW_Point.Wind_Setsurei, KW_Point.Barbarian_Outpost, KW_Point.Daylight_Crossing, KW_Point.Jade_Stream, KW_Point.Nether_Trail, KW_Point.Wraith_Burial,
            KW_Point.Horror_Peaks, KW_Point.Thunder_Tower, KW_Point.Mystic_Wetlands, KW_Point.Magic_Forest,
            KW_Point.Mystical_Fortress, KW_Point.Sunset_City, KW_Point.Pebble_Bay, KW_Point.Thorn_Prairie
        };

        private KW_Point ConvertSettingPOI(string setting)
        {
            if (!Utils.ValidText(setting))
                return KW_Point.Unknown;
            if (setting.Contains("_"))
                setting = Utils.ChopperBlank(setting, null, "_").Trim();

            return (KW_Point)Utils.CInt(setting);
        }

        public enum KW_Point : int
        {
            Fire_Furnace = 1,
            Sunset_Hills = 2,
            Sea_of_Lava = 3,
            Flame_Peak = 4,
            Crater = 5,
            Black_City_Gate = 6,
            Divine_Throne = 7,
            Squall_Mountains = 8,
            Guardian_Statues = 9,
            Wind_Setsurei = 10,
            Kings_Temple = 11,
            Barbarian_Outpost = 12,
            Plague_Burrow = 13,
            Toxic_Springs = 14,
            Gray_Swamp = 15,
            Nether_Trail = 16,
            Wild_Fog = 17,
            Wraith_Burial = 18,
            Fountain_of_Life = 19,
            Magic_Academy = 20,
            Moon_Valley = 21,
            Daylight_Crossing = 22,
            Elves_Altar = 23,
            Jade_Stream = 24,
            Mystical_Fortress = 25,
            Thunder_Tower = 26,
            Sunset_City = 27,
            Horror_Peaks = 28,
            Holy_War_Remains = 29,
            Mystic_Wetlands = 30,
            Pebble_Bay = 31,
            Magic_Forest = 32,
            Thorn_Prairie = 33,

            Unknown = 0,
        }

        private bool KingdomWar_CooldownNotified = false;

        public void KingdomWar_Fight()
        {
            lock (this.KW_locker_fight)
            {
                if (!Utils.False("Game_FightKW") || (!Utils.True("Game_Events")))
                    return;

                if (this.KingdomWar_InCooldown)
                {
                    if (!this.KingdomWar_CooldownNotified)
                    {
                        Utils.LoggerNotifications("<color=#ffa000>Kingdom War in cooldown (ends at " + this.KW_CooldownExpires.ToShortTimeString() + ")</color>");
                        this.KingdomWar_CooldownNotified = true;
                    }
                    return;
                }

                if (this.KW_TeamHasBeenEliminated)
                {
                    Utils.LoggerNotifications("<color=#ffa000>Kingdom War notice:  your kingdom has been eliminated!");
                    return;
                }

                if (this.KW_InQueue)
                {
                    if ((DateTime.Now - this.KW_InQueueSince).TotalSeconds < 15)
                        return;

                    this.KW_InQueue = false;
                }

                string kwMap = this.GetGameData("forcefight", "ShowForceMap", false);
                //Utils.Chatter(kwMap);
                //Utils.Chatter();

                try
                {
                    JObject kwMapJSON = JObject.Parse(kwMap);

                    if (Utils.CInt(kwMapJSON["status"]) == 0)
                    {
                        // KW has ended or hasn't started yet
                        this.KW_Ongoing = false;
                        this.KingdomWar_Reset();
                        return;
                    }

                    if (this.KW_LastHonor == 0)
                        this.KW_LastHonor = Utils.CInt(kwMapJSON["KW_LastHonor"]);

                    this.KW_ForceID = Utils.CInt(kwMapJSON["data"]["force_id"]);
                    this.Kingdom_War_ID = kwMapJSON["data"]["force_id"].ToString();

                    if (!this.KW_Ongoing)
                        this.KingdomWar_WarBegins();

                    int cooldown_check = Utils.CInt(kwMapJSON["data"]["cd_time"]);
                    if (cooldown_check > 0)
                    {
                        this.KW_CooldownExpires = DateTime.Now.AddSeconds(cooldown_check);

                        if (!KingdomWar_CooldownNotified)
                        {
                            Utils.LoggerNotifications("<color=#ffa000>Kingdom War in cooldown (ends at " + this.KW_CooldownExpires.ToShortTimeString() + ")</color>");
                            this.KingdomWar_CooldownNotified = true;
                        }

                        try { this.ScheduledEvents.Add("KW Fight", new Scheduler.ScheduledEvent("KW Fight", this.KingdomWar_Fight, GameClient.DateTimeNow.AddSeconds(cooldown_check))); }
                        catch { }
                        return;
                    }

                    this.KingdomWar_CooldownNotified = false;

                    //Utils.LoggerNotifications("KW fight status: " + kwMapJSON["data"]["fight_status"].ToString()); // 0 = nothing, -1 or -2 = in queue to defend, 1 or 2 = in queue to attack
                    //Utils.LoggerNotifications("KW current target point ID: " + kwMapJSON["data"]["current_id"].ToString()); // point ID we're attacking or defending

                    foreach (KW_Point p in GameClient.KW_AttackTargets)
                    {
                        JObject point = (JObject)kwMapJSON["data"]["map"][((int)p).ToString()];
                        /*
                        Utils.Chatter(i.ToString());
                        Utils.Chatter("    " + point["point_id"].ToString());
                        Utils.Chatter("    " + point["point_name"].ToString());
                        Utils.Chatter("    " + point["force_id"].ToString());
                        Utils.Chatter("    " + point["value"].ToString());
                        Utils.Chatter("    " + point["resource"].ToString());
                        Utils.Chatter("    " + point["ishome"].ToString());
                        Utils.Chatter("    " + point["link"].ToString());
                        Utils.Chatter("    " + point["around"].ToString());
                        */

                        if (Utils.CInt(point["force_id"]) != this.KW_ForceID)
                        {
                            // attackStatus:  -1 defend, 1 attack
                            string kwMapAttack = this.GetGameData("forcefight", "JoinFight", "point_id=" + point["point_id"].ToString() + "&attackStatus=1&NewVersion=1", true);

                            if (kwMapAttack.Contains("Your Kingdom has already been eliminated from the Kingdom War"))
                            {
                                this.KW_TeamHasBeenEliminated = true;
                                break;
                            }

                            //Utils.LoggerNotifications(kwMapAttack);

                            JObject kwMapAttackJSON = JObject.Parse(kwMapAttack);

                            if ((Utils.CInt(kwMapAttackJSON["status"]) == 1) || (kwMapAttack.Contains("Please wait, searching for an opponent")))
                            {
                                // attack success or already in queue to attack
                                this.KW_InQueue = true;
                                this.KW_InQueueSince = DateTime.Now;

                                if (Utils.CInt(kwMapAttackJSON["status"]) == 1)
                                    Utils.LoggerNotifications("<color=#ffa000>Kingdom War in queue to attack <b>" + point["point_name"].ToString() + "</b> worth <b>" + Utils.CInt(point["value"]).ToString("#,##0") + "</b> points...</color>");

                                try { this.ScheduledEvents.Add("KW Fight Followup", new Scheduler.ScheduledEvent("KW Fight Followup", this.KingdomWar_Fight, GameClient.DateTimeNow.AddSeconds(15))); }
                                catch { } break;
                            }
                        }
                    }

                    if (this.KW_InQueue || this.KW_TeamHasBeenEliminated)
                        return;

                    // Don't try to defend, for now... there's no logic in place to determine if a point is likely to be attacked, so it could waste
                    // a lot of time defending a point that nobody would/could attack.
                    /*
                    foreach (KW_Point p in GameClient.KW_DefendTargets)
                    {
                        JObject point = (JObject)kwMapJSON["data"]["map"][((int)p).ToString()];
                    
                        if (Utils.CInt(point["force_id"]) == this.KW_ForceID)
                        {
                            // attackStatus:  -1 defend, 1 attack
                            string kwMapDefend= this.GetGameData("forcefight", "JoinFight", "point_id=" + point["point_id"].ToString() + "&attackStatus=-1&NewVersion=1", true);
                            //Utils.LoggerNotifications(kwMapDefend);
                            JObject kwMapDefendJSON = JObject.Parse(kwMapDefend);
                            if (Utils.CInt(kwMapDefendJSON["status"]) == 1)
                            {
                                // defend success
                                break;
                            }
                        }
                    }
                    */

                    // couldn't find a point to attack or defend
                    try { this.ScheduledEvents.Add("KW Fight Followup", new Scheduler.ScheduledEvent("KW Fight Followup", this.KingdomWar_Fight, GameClient.DateTimeNow.AddSeconds(2))); }
                    catch { }
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    // Ignore and try again in a second
                    try { this.ScheduledEvents.Add("KW Fight", new Scheduler.ScheduledEvent("KW Fight", this.KingdomWar_Fight, GameClient.DateTimeNow.AddSeconds(1))); } catch { }
                }
                catch (Exception ex)
                {
                    Utils.LoggerNotifications(Errors.GetShortErrorDetails(ex));
                }
            }

            return;
        }

        public void ClanMemberReport()
        {
            try
            {
                lock (this.locker_gamedata)
                {
                    this.CheckLogin();
                    if (this.opts == null)
                        return;

                    JObject clan_members = JObject.Parse(this.GetGameData(ref this.opts, "legion", "GetMember", false));

                    string ReportText = "";

                    ReportText += "Member list report:  " + DateTime.Now.ToString() + "\r\n";

                    foreach (JObject member in clan_members["data"]["Members"])
                    {
                        ReportText += "\t" + member["NickName"].ToString() + "\r\n";
                        ReportText += "\t\tcontribution:\t" + Utils.CInt(member["Contribute"].ToString()).ToString("#,##0") + "\r\n";
                        ReportText += "\t\tlast login:\t" + DateTime.Parse(member["LastLoginDate"].ToString()).ToShortDateString() + "\r\n";
                        ReportText += "\t\thero level:\t" + member["Level"].ToString() + "\r\n";
                        ReportText += "\t\tarena ranking:\t" + member["Rank"].ToString() + "\r\n";
                    }

                    System.IO.File.AppendAllText(Utils.AppFolder + @"\Clan member report, " + this.Clan_Name + ", " + DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + ".txt", ReportText);
                }
            }
            catch { }
        }

        #region Login

        #region ARC

        public bool login_done_arc = false;

        public string Login_EK_GetARCID()
        {
            if (this.login_done_arc && !string.IsNullOrEmpty(this.Login_ARC_UIN))
                return this.Login_ARC_UIN;

            if (string.IsNullOrEmpty(Utils.GetAppSetting("Login_Password").Trim()))
            {
                Utils.Chatter("Your game login password is blank.");
                return null;
            }

            bool captcha_required_forced = false;

            this.opts = new Comm.CommFetchOptions();

        captcha_resetter:

            this.opts.WantCookies = true;
            this.opts.Method = Comm.CommFetchOptions.Methods.GET;
            this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.Chrome;
            string ARC_Login_Form = Utils.CStr(Comm.Download("http://mobile.arcgames.com/user/login?gameid=51&sdkvcode=1.3.6&platform=android&androidos=18", ref this.opts));

            string user_field_name = Utils.ChopperBlank(ARC_Login_Form, " id=\"un\" name=\"", "\"");
            if (!Utils.ValidText(user_field_name))
                user_field_name = Utils.ChopperBlank(Utils.ChopperBlank(ARC_Login_Form, "Username", "</div>"), "name=\"", "\"");
            string pass_field_name = Utils.ChopperBlank(ARC_Login_Form, " id=\"pw\" name=\"", "\"");
            if (!Utils.ValidText(pass_field_name))
                pass_field_name = Utils.ChopperBlank(Utils.ChopperBlank(ARC_Login_Form, "Password", "</div>"), "name=\"", "\"");

            bool captcha_required = !ARC_Login_Form.Contains("class=\"js-captcha-wrap\" style=\"display:none") || captcha_required_forced;
            string captcha_field_name = Utils.ChopperBlank(ARC_Login_Form, " id=\"captcha_login\" name=\"", "\"");
            string captcha_answer = "";

            if (captcha_required)
            {
                string captcha_source = Utils.ChopperBlank(Utils.ChopperBlank(Utils.ChopperBlank(ARC_Login_Form, "<div class=\"js-captcha-wrap\"", "</div>"), "<img", "/>"), "src='", "'").Trim();
                if (!captcha_source.StartsWith("http"))
                {
                    if (!captcha_source.StartsWith("/"))
                        captcha_source = "http://mobile.arcgames.com/" + captcha_source;
                    else
                        captcha_source = "http://mobile.arcgames.com" + captcha_source;
                }

                using (frmEKARCCAPTCHA captcha_solve = new frmEKARCCAPTCHA())
                {
                    captcha_solve.cc = this.opts.CookieContainerGet();

                    captcha_solve.SetImage(captcha_source);

                    if (captcha_solve.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        captcha_answer = captcha_solve.Answer;

                    this.opts.CookieContainerSet(captcha_solve.cc);
                }

                if (!Utils.ValidText(captcha_answer))
                    return null;
            }

            this.opts.Method = Comm.CommFetchOptions.Methods.POST;
            this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
            this.opts.CustomRefererURL = "http://mobile.arcgames.com/user/login?gameid=51&sdkvcode=1.3.6&platform=android&androidos=18";

            if (captcha_required)
            {
                this.opts.POST_Data =
                    user_field_name + "=" + System.Web.HttpUtility.UrlEncode(Utils.GetAppSetting("Login_Account").Trim()) + "&" +
                    pass_field_name + "=" + System.Web.HttpUtility.UrlEncode(Utils.GetAppSetting("Login_Password").Trim()) + "&" +
                    captcha_field_name + "=" + System.Web.HttpUtility.UrlEncode(captcha_answer.Trim()) + "";                
            }
            else
            {
                this.opts.POST_Data =
                    user_field_name + "=" + System.Web.HttpUtility.UrlEncode(Utils.GetAppSetting("Login_Account").Trim()) + "&" +
                    pass_field_name + "=" + System.Web.HttpUtility.UrlEncode(Utils.GetAppSetting("Login_Password").Trim()) + "";
            }

            string result = Utils.CStr(Comm.Download("http://mobile.arcgames.com/user/login/?gameid=51", ref this.opts));

            Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
            result = regex.Replace(result, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());

            Utils.Logger("<b>ARC login result:</b> " + result);
            Utils.Logger();

            if (!result.Replace(" ", "").Contains("\"result\":true"))
            {
                string msg = "";
                if (result.Contains("\"msg\":\""))
                    msg = Utils.ChopperBlank(result, "\"msg\":\"", "\"");

                if (!captcha_required_forced && msg.ToLower().Contains("captcha"))
                {
                    captcha_required_forced = true;
                    goto captcha_resetter;
                }

                Utils.Chatter("The ARC gateway login failed.  Check your username and password.");
                Utils.Chatter();
                Utils.Chatter("If you continue to see this message and are sure your login details are correct, then Perfect World may have changed how the login gateway works, which will require an update from EK Unleashed.");

                if (Utils.ValidText(msg))
                {
                    Utils.Chatter();
                    Utils.Chatter("Server responded with error: " + msg.Trim());
                }

                return null;
            }

            try
            {
                JObject ARC_Login_Data = JObject.Parse(result);

                this.Login_ARC_UIN = ARC_Login_Data["loginstatus"].ToString().Split(new char[] { ':' })[1];
                this.Login_ARC_DeviceToken = ARC_Login_Data["loginstatus"].ToString().Split(new char[] { ':' })[3];
            }
            catch { }

            //this.Login_ARC_UIN = Utils.ChopperBlank(result, "user:", ":");

            if (Utils.ValidText(this.Login_ARC_UIN))
            {
                this.login_done_arc = true;
                return this.Login_ARC_UIN;
            }

            return null;
        }

        public string Login_ARC_DeviceToken = "";

        public string Login_EK_GetARCID(string user_override, string pass_override)
        {
            if (this.login_done_arc && !string.IsNullOrEmpty(this.Login_ARC_UIN))
                return this.Login_ARC_UIN;

            if (string.IsNullOrEmpty(Utils.GetAppSetting("Login_Password").Trim()))
            {
                Utils.Chatter("Your game login password is blank.");
                return null;
            }

            bool captcha_required_forced = false;

            this.opts = new Comm.CommFetchOptions();

        captcha_resetter:

            this.opts.WantCookies = true;
            this.opts.Method = Comm.CommFetchOptions.Methods.GET;
            this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.Chrome;
            string ARC_Login_Form = Utils.CStr(Comm.Download("http://mobile.arcgames.com/user/login?gameid=51&sdkvcode=1.3.6&platform=android&androidos=18", ref this.opts));

            string user_field_name = Utils.ChopperBlank(ARC_Login_Form, " id=\"un\" name=\"", "\"");
            if (!Utils.ValidText(user_field_name))
                user_field_name = Utils.ChopperBlank(Utils.ChopperBlank(ARC_Login_Form, "Username", "</div>"), "name=\"", "\"");
            string pass_field_name = Utils.ChopperBlank(ARC_Login_Form, " id=\"pw\" name=\"", "\"");
            if (!Utils.ValidText(pass_field_name))
                pass_field_name = Utils.ChopperBlank(Utils.ChopperBlank(ARC_Login_Form, "Password", "</div>"), "name=\"", "\"");

            bool captcha_required = !ARC_Login_Form.Contains("class=\"js-captcha-wrap\" style=\"display:none") || captcha_required_forced;
            string captcha_field_name = Utils.ChopperBlank(ARC_Login_Form, " id=\"captcha_login\" name=\"", "\"");
            string captcha_answer = "";

            if (captcha_required)
            {
                string captcha_source = Utils.ChopperBlank(Utils.ChopperBlank(Utils.ChopperBlank(ARC_Login_Form, "<div class=\"js-captcha-wrap\"", "</div>"), "<img", "/>"), "src='", "'").Trim();
                if (!captcha_source.StartsWith("http"))
                {
                    if (!captcha_source.StartsWith("/"))
                        captcha_source = "http://mobile.arcgames.com/" + captcha_source;
                    else
                        captcha_source = "http://mobile.arcgames.com" + captcha_source;
                }

                using (frmEKARCCAPTCHA captcha_solve = new frmEKARCCAPTCHA())
                {
                    captcha_solve.cc = this.opts.CookieContainerGet();

                    captcha_solve.SetImage(captcha_source);

                    if (captcha_solve.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        captcha_answer = captcha_solve.Answer;

                    this.opts.CookieContainerSet(captcha_solve.cc);
                }

                if (!Utils.ValidText(captcha_answer))
                    return null;
            }

            this.opts.Method = Comm.CommFetchOptions.Methods.POST;
            this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
            this.opts.CustomRefererURL = "http://mobile.arcgames.com/user/login?gameid=51&sdkvcode=1.3.6&platform=android&androidos=18";

            if (captcha_required)
            {
                this.opts.POST_Data =
                    user_field_name + "=" + System.Web.HttpUtility.UrlEncode(user_override.Trim()) + "&" +
                    pass_field_name + "=" + System.Web.HttpUtility.UrlEncode(pass_override.Trim()) + "&" +
                    captcha_field_name + "=" + System.Web.HttpUtility.UrlEncode(captcha_answer.Trim()) + "";
            }
            else
            {
                this.opts.POST_Data =
                    user_field_name + "=" + System.Web.HttpUtility.UrlEncode(user_override.Trim()) + "&" +
                    pass_field_name + "=" + System.Web.HttpUtility.UrlEncode(pass_override.Trim()) + "";
            }

            string result = Utils.CStr(Comm.Download("http://mobile.arcgames.com/user/login/?gameid=51", ref this.opts));

            Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
            result = regex.Replace(result, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());

            Utils.Logger("<b>ARC login result:</b> " + result);
            Utils.Logger();

            if (!result.Replace(" ", "").Contains("\"result\":true"))
            {
                string msg = "";
                if (result.Contains("\"msg\":\""))
                    msg = Utils.ChopperBlank(result, "\"msg\":\"", "\"");

                if (!captcha_required_forced && msg.ToLower().Contains("captcha"))
                {
                    captcha_required_forced = true;
                    goto captcha_resetter;
                }

                Utils.Chatter("The ARC gateway login failed.  Check your username and password.");
                Utils.Chatter();
                Utils.Chatter("If you continue to see this message and are sure your login details are correct, then Perfect World may have changed how the login gateway works, which will require an update from EK Unleashed.");

                if (Utils.ValidText(msg))
                {
                    Utils.Chatter();
                    Utils.Chatter("Server responded with error: " + msg.Trim());
                }

                return null;
            }

            try
            {
                Utils.Chatter("ARC login: " + result);
                Utils.Chatter();

                JObject ARC_Login_Data = JObject.Parse(result);

                this.Login_ARC_UIN = ARC_Login_Data["loginstatus"].ToString().Split(new char[] { ':' })[1];
                this.Login_ARC_DeviceToken = ARC_Login_Data["loginstatus"].ToString().Split(new char[] { ':' })[3];
            }
            catch { }

            //this.Login_ARC_UIN = Utils.ChopperBlank(result, "user:", ":");

            if (Utils.ValidText(this.Login_ARC_UIN))
            {
                this.login_done_arc = true;
                return this.Login_ARC_UIN;
            }

            return null;
        }

        public static string FixUnicode(string s)
        {
            Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
            return regex.Replace(s, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
        }

        #endregion

        public string Login_ARC_UIN = "";
        public string PassportLoginJSON = "";
        public string GAME_URL = "http://s1.ek.ifreeteam.com/";

        public Comm.CommFetchOptions CheckLogin()
        {
            if (!Want_Game_Login)
                return null;

            switch (this.Service)
            {
                case GameService.Lies_of_Astaroth:
                    return this.Login_LOA();

                case GameService.Magic_Realms:
                    return this.Login_MR();

                case GameService.Elves_Realm:
                    return this.Login_ER();

                case GameService.Shikoku_Wars:
                    return this.Login_SW_New();
            }

            return Login_EK();
        }

        private Comm.CommFetchOptions Login_EK()
        {
            if (this.opts != null)
            {
                string data = this.GetGameData(ref this.opts, "user", "GetUserInfo", false);

                if (data.Contains("{\"status\":1"))
                    return this.opts;

                this.GameVitalsUpdate(data);
            }

            string Udid = ""; // unique device ID (MAC address)
            this.Login_ARC_UIN = Login_EK_GetARCID();

            this.opts = new Comm.CommFetchOptions();
            this.opts.WantCookies = true;
            this.opts.Method = Comm.CommFetchOptions.Methods.POST;
            this.opts.DataType_JSON = false;
            this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
            this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
            this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

            this.opts.POST_Data =
                "plat=pwe&" +
                "Udid=" + Udid + "&" +
                "uin=" + Login_ARC_UIN + "&" +
                "nickName=" + Login_ARC_UIN;

            Utils.Logger("<b>POST'ing:</b> " + this.opts.POST_Data);
            string result = Utils.CStr(Comm.Download("http://master.ek.ifreeteam.com/mpassport.php?do=plogin&v=" + this.seq_id.ToString() + TAG_EK, ref this.opts));
            this.seq_id++;
            frmMain.AuthSerial++;

            Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
            result = regex.Replace(result, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
            Utils.Logger(result);
            this.PassportLoginJSON = result;
            Utils.Logger();
            if (!result.Contains("\"status\":1,"))
                return null;

            this.GAME_URL = "http://s1.ek.ifreeteam.com/";
            try { GAME_URL = JObject.Parse(this.PassportLoginJSON)["data"]["current"]["GS_IP"].ToString(); }
            catch { }

            string MUid = Utils.ChopperBlank(result, "\"MUid\":\"", "\"").Trim();
            string time = Utils.ChopperBlank(result, "\"time\":", ",").Trim();
            string sign = Utils.ChopperBlank(result, "\"sign\":\"", "\"").Trim();
            string ppsign = Utils.ChopperBlank(result, "\"ppsign\":\"", "\"").Trim();
            string access_token = Utils.ChopperBlank(result, "\"access_token\":\"", "\"").Trim();

            // The server always returns AuthType 1 now (used to be random AuthType 1-6), so this loop may not be required anymore.
            // We were using the loop to keep cycling until we found an AuthType we could handle.
            for (int i = 0; i < 10; i++)
            {
                this.opts = new Comm.CommFetchOptions();
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";
                this.opts.POST_Data =
                    "Devicetoken=&" +
                    "time=" + time + "&" +
                    "plat=pwe&" +
                    "access%5Ftoken=" + access_token + "&" +
                    "MUid=" + MUid + "&" +
                    "ppsign=" + ppsign + "&" +
                    "nick=" + Login_ARC_UIN + "&" +
                    "Udid=" + Udid + "&" +
                    "sign=" + sign + "&" +
                    "uin=" + Login_ARC_UIN + "&" +
                    "Origin=%5FARC";
                Utils.Logger("<b>POST'ing:</b> " + this.opts.POST_Data);
                result = Utils.CStr(Comm.Download(this.GAME_URL + "login.php?do=mpLogin&v=" + this.seq_id.ToString() + TAG_EK, ref this.opts));
                this.seq_id++;
                frmMain.AuthSerial++;
                regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                result = regex.Replace(result, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(result);
                Utils.Logger();
                if (!result.Contains("\"status\":1,"))
                    return null;

                this.Game_CDN_URL = "http://s1.ek.ifreeteam.com/";
                try { this.Game_CDN_URL = JObject.Parse(result)["data"]["cdnurl"].ToString(); } catch { }
                try { frmMain.AuthType = Utils.CInt(JObject.Parse(result)["data"]["AuthType"].ToString()); } catch { }
                try { frmMain.AuthSerial = Utils.CLng(JObject.Parse(result)["data"]["AuthSerial"].ToString()); } catch { }

                //Utils.DebugLogger("AuthType: " + frmMain.AuthType.ToString());
                //Utils.DebugLogger("AuthSerial: " + frmMain.AuthSerial.ToString());

                if (frmMain.AuthType == 1 || frmMain.AuthType == 3 /* || frmMain.AuthType == 4 */)
                    break;
            }

            return this.opts;
        }

        #endregion

        #region Data

        public string GetGameData(string page, string action)
        {
            return this.GetGameData(ref this.opts, page, action, "", false);
        }

        public string GetGameData(string page, string action, string post_data)
        {
            return this.GetGameData(ref this.opts, page, action, post_data, false);
        }

        public string GetGameData(string page, string action, string post_data, bool log = false, string log_to = "", bool only_if_valid = true)
        {
            return this.GetGameData(ref this.opts, page, action, post_data, log, log_to, only_if_valid);
        }

        public string GetGameData(string page, string action, bool log = false, string log_to = "", bool only_if_valid = true)
        {
            return this.GetGameData(ref this.opts, page, action, "", log, log_to, only_if_valid);
        }

        public string GetGameData(ref Comm.CommFetchOptions opts, string page, string action, bool log = false, string log_to = "", bool only_if_valid = true)
        {
            return this.GetGameData(ref opts, page, action, "", log, log_to, only_if_valid);
        }

        public string GetGameData(ref Comm.CommFetchOptions opts, string page, string action, string post_data = "", bool log = false, string log_to = "", bool only_if_valid = true)
        {
            string result = string.Empty;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    result = this.GetGameData__internal(ref opts, page, action, post_data, log, log_to, only_if_valid);

                    if (!string.IsNullOrEmpty(result) && !result.StartsWith("[")) // automatic retries on connection failures
                        break;
                }
                catch { }
            }

            return result;
        }

        public string GameTagToUse
        {
            get
            {
                string calc = frmMain.CalculateEncryptedPacketData();

                if (Utils.ValidText(calc))
                    return calc;

                if (this.Service == GameService.Elemental_Kingdoms) return TAG_EK;
                if (this.Service == GameService.Lies_of_Astaroth) return TAG_LOA;
                if (this.Service == GameService.Magic_Realms) return TAG_MR;
                if (this.Service == GameService.Elves_Realm) return TAG_ER;
                if (this.Service == GameService.Shikoku_Wars) return TAG_SW;
                return "";
            }
        }

        private Regex regex_unicode_replacer = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);

        public string GetGameData__internal(ref Comm.CommFetchOptions opts, string page, string action, string post_data = "", bool log = false, string log_to = "", bool only_if_valid = true)
        {
            opts.POST_Data = post_data;

            //log = true;

            Utils.Logger();

            string result = Utils.CStr(Comm.Download(this.GAME_URL + page + ".php?do=" + action + "&v=" + this.seq_id.ToString() + GameTagToUse, opts));
            Utils.Logger("<b>HTTP request:</b> " + this.GAME_URL + page + ".php?do=" + action + "&v=" + this.seq_id.ToString() + GameTagToUse);
            if (post_data.Trim().Length > 0)
                Utils.Logger("<b>POST'ing:</b> " + opts.POST_Data);
            this.seq_id++;
            frmMain.AuthSerial++;

            try
            {
                // converts \u0123 into actual unicode characters
                if (result.Contains("\\u"))
                    result = this.regex_unicode_replacer.Replace(result, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
            }
            catch { }

            if (Want_Debug && log /* && (!only_if_valid || result.Contains("\"status\":1,")) */)
            {
                try
                {
                    if (!System.IO.Directory.Exists(Utils.AppFolder + "\\Game Data"))
                        System.IO.Directory.CreateDirectory(Utils.AppFolder + "\\Game Data");

                    string request_info = string.Empty;

                    request_info += "Data requested:  " + DateTime.Now.ToString() + "  (local time)\r\n";
                    request_info += "Data from server page:   " + page + ".php?do=" + action + "&v=" + this.seq_id.ToString() + GameTagToUse + "\r\n";
                    if (Utils.ValidText(post_data))
                        request_info += "Data POST'ed to this page:  " + post_data + "\r\n";

                    Utils.FileOverwrite(Utils.AppFolder + "\\Game Data\\" + GameAbbreviation(Service) + ", " + RequestMeaning(page, action, log_to) + " (" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".").Replace(":", ".") + "), request.txt", request_info);
                    Utils.FileOverwrite(Utils.AppFolder + "\\Game Data\\" + GameAbbreviation(Service) + ", " + RequestMeaning(page, action, log_to) + " (" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".").Replace(":", ".") + "), data.json", JSBeautifyLib.JSBeautify.BeautifyMe(result));
                }
                catch (Exception ex)
                {
                    Utils.Chatter(Errors.GetShortErrorDetails(ex));
                }
            }

            Utils.Logger(result);
            Utils.Logger();

            return result;
        }

        public static bool Want_Debug
        {
            get
            {
                try
                {
                    return Utils.False("Game_Debug");
                }
                catch { }

                return false;
            }
        }

        private static string RequestMeaning(string page, string action, string log_to)
        {
            //if ((page == "login") && (action == "cdnurl")) return "CDN URL";
            if ((page == "card") && (action == "GetAllCard")) return "Master Card List";
            if ((page == "rune") && (action == "GetAllRune")) return "Master Rune List";
            if ((page == "card") && (action == "GetAllSkill")) return "Master Skill List";
            if ((page == "activity") && (action == "ActivityInfo")) return "Game Events";
            if ((page == "mapstage") && (action == "GetMapStageALL")) return "Map Stage List";
            if ((page == "mapstage") && (action == "GetUserMapStages")) return "User Map Stage Completion";
            if ((page == "shop") && (action == "GetGoods")) return "Card Shop List";
            if ((page == "boss") && (action == "GetBoss")) return "Latest Boss Info";
            if ((page == "forceshop") && (action == "GetAuctionGoods")) return "Kingdom War Auction Info";
            if ((page == "forceshop") && (action == "GetExchangeGoods")) return "Kingdom War Exchange List";
            if ((page == "user") && (action == "GetUserInfo")) return "User Info (SENSITIVE DATA)";
            if ((page == "card") && (action == "GetUserCards")) return "User Card List";
            if ((page == "rune") && (action == "GetUserRunes")) return "User Rune List";
            if ((page == "card") && (action == "GetCardGroup")) return "User Deck List";
            if ((page == "friend") && (action == "GetFriends")) return "User Friend List";
            if ((page == "legion") && (action == "GetUserLegion")) return "User Clan Info";
            if ((page == "legion") && (action == "GetMember")) return "User Clan Roster";
            if ((page == "arena") && (action == "GetThieves")) return "Thieves List";
            if ((page == "hydra") && (action == "GetHydraInfo")) return "Hydra List";
            if ((page == "league") && (action == "lotteryInfo")) return "User Field of Honor Info";

            if (!Utils.ValidText(log_to))
                log_to = page + " - " + action;
            return log_to;
        }

        #endregion

        private object _LockedCardsLocker = new object();
        private List<string> _LockedCards = new List<string>();

        public List<string> LockedCards
        {
            get
            {
                List<string> temp = new List<string>();

                lock (this._LockedCardsLocker)
                {
                    temp.AddRange(this._LockedCards);
                }

                return temp;
            }
        }

        public void EnchantCard(string card_name, int card_level, int unique_card_id_to_enchant = 0)
        {
            JObject EnchantThisCard = this.GetCardByName(card_name);
            
            if (EnchantThisCard == null)
            {
                Utils.LoggerNotifications("<color=#ffa000>Couldn't find a matching card to enchant.</color>");
                return;
            }

            JObject user_data = JObject.Parse(this.GetGameData("user", "GetUserInfo", false));
            int UserGold = Utils.CInt(user_data["data"]["Coins"]);

            lock (this._LockedCardsLocker)
            {
                this._LockedCards.Clear();
                try
                {
                    JArray locked_cards = (JArray)user_data["data"]["LockedUserCardIds"];

                    foreach (var locked_card in locked_cards)
                    {
                        this._LockedCards.Add(locked_card.ToString().Replace("\"", "").Trim());
                    }
                }
                catch { }
            }

            this.UserCards_CachedData = null;
            this.GetUsersCards();
            
            GameObjs.Card CardToEnchant = null;
            List<GameObjs.Card> CardsToEat = new List<GameObjs.Card>();

            if (unique_card_id_to_enchant == 0)
            {
                for (int iLevelCap = card_level - 1; iLevelCap >= 0; iLevelCap--)
                {
                    foreach (var jCard in this.UserCards_CachedData["data"]["Cards"])
                    {
                        GameObjs.Card TempCard = new GameObjs.Card(jCard);

                        if ((TempCard.Level == iLevelCap) && (TempCard.ID_Generic == Utils.CInt(EnchantThisCard["CardId"])))
                        {
                            if (TempCard.MaxLevel >= card_level)
                            {
                                Utils.LoggerNotifications("<color=#ffa000>Found a level " + TempCard.Level.ToString() + " " + TempCard.Name + " to enchant to level " + card_level.ToString() + "!</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempCard.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP to enchant</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempCard.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold to enchant</color>");

                                CardToEnchant = TempCard;
                                break;
                            }
                        }
                    }

                    if (CardToEnchant != null)
                        break;
                }
            }
            else
            {
                foreach (var jCard in this.UserCards_CachedData["data"]["Cards"])
                {
                    GameObjs.Card TempCard = new GameObjs.Card(jCard);

                    if (TempCard.Level < TempCard.MaxLevel)
                    {
                        if (TempCard.MaxLevel >= card_level)
                        {
                            if (TempCard.ID_User == unique_card_id_to_enchant)
                            {
                                Utils.LoggerNotifications("<color=#ffa000>Enchanting your level " + TempCard.Level.ToString() + " " + TempCard.Name + " to level " + card_level.ToString() + "!</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempCard.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP to enchant</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempCard.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold to enchant</color>");

                                CardToEnchant = TempCard;
                                break;
                            }
                        }
                    }
                }
            }

            if (CardToEnchant == null)
            {
                Utils.LoggerNotifications("<color=#ffa000>You don't have an eligible " + EnchantThisCard["CardName"].ToString().Trim() + " to enchant.</color>");
            }
            else if (((double)UserGold) < (((double)CardToEnchant.EnchantToLevelCostGold(card_level)) * 1.05))
            {
                Utils.LoggerNotifications("<color=#ffa000>You need more gold to enchant this card.</color>");
            }
            else
            {
                GameObjs.Card.StarsAllowedToEnchantWith = Utils.SubStrings(Utils.GetAppSetting("Enchant_Cards_WithStars").Replace(" ", ""), ",");
                GameObjs.Card.CardsExcludedFromEnchantingWith = Utils.SubStrings(Utils.GetAppSetting("Enchant_Cards_Excluded").Replace(" ", ""), ",");
                List<string> temp = new List<string>();
                foreach (string s in GameObjs.Card.CardsExcludedFromEnchantingWith)
                {
                    JObject card = this.GetCardByName(s);
                    if (card != null)
                        temp.Add(Utils.CInt(card["CardId"]).ToString());
                }
                GameObjs.Card.CardsExcludedFromEnchantingWith = temp.ToArray();


                int XPRunningTotal = 0;

                Dictionary<int,int> cardCount = new Dictionary<int,int>();

                foreach (var jCard in this.UserCards_CachedData["data"]["Cards"])
                {
                    GameObjs.Card TempCard = new GameObjs.Card(jCard);

                    if (!cardCount.ContainsKey(TempCard.ID_Generic))
                        cardCount.Add(TempCard.ID_Generic, 1);
                    else
                        cardCount[TempCard.ID_Generic]++;
                }

                Dictionary<int,int> cardUsed = new Dictionary<int,int>();
                int iDontUseMoreThanThreshold = Utils.GetAppValue("Enchant_Cards_ReserveThreshold", 5);

                GameObjs.Card.RefreshCardsInDeckCache();

                for (int iPassNum = 0; iPassNum <= 1; iPassNum++)
                {
                    foreach (var jCard in this.UserCards_CachedData["data"]["Cards"])
                    {
                        GameObjs.Card TempCard = new GameObjs.Card(jCard);

                        if ((iPassNum == 0) && (!TempCard.FoodCard)) continue;

                        if (TempCard.OkayToConsume)
                        {
                            if (TempCard.FoodCard)
                            {
                                try
                                {

                                    int remaining_XP = CardToEnchant.EnchantToLevelCostXP(card_level) - XPRunningTotal;
                                    if (remaining_XP > 0)
                                    {
                                        double food_card_waste_percent = ((double)TempCard.EnchantingWorth) * 100.0 / ((double)remaining_XP);

                                        // using a 2,000 XP card for less than 1,000 XP need, etc.
                                        if (food_card_waste_percent >= 100.0)
                                            continue;
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                int iUsedSoFar = 0;
                                if (cardUsed.ContainsKey(TempCard.ID_Generic))
                                    iUsedSoFar = cardUsed[TempCard.ID_Generic];

                                if (cardCount[TempCard.ID_Generic] - iUsedSoFar <= iDontUseMoreThanThreshold)
                                    continue;
                            }

                            //Utils.LoggerNotifications("Found a level " + TempCard.Level.ToString() + " " + TempCard.Name + " to enchant with for " + TempCard.EnchantingWorth.ToString("#,##0") + " XP");

                            XPRunningTotal += TempCard.EnchantingWorth;
                            CardsToEat.Add(TempCard);

                            if (!cardUsed.ContainsKey(TempCard.ID_Generic))
                                cardUsed.Add(TempCard.ID_Generic, 1);
                            else
                                cardUsed[TempCard.ID_Generic]++;

                            if (XPRunningTotal >= CardToEnchant.EnchantToLevelCostXP(card_level))
                                break;
                        }
                    }
                }

                bool bEnchantAnyway = false;

                if ((CardsToEat.Count > 0) && (XPRunningTotal < CardToEnchant.EnchantToLevelCostXP(card_level)))
                    if (MessageBox.Show("Enchanting needs consume at least " + CardToEnchant.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP and " + CardToEnchant.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold in costs.  The " + CardsToEat.Count.ToString("#,##0") + " cards available to enchant with only add up to " + XPRunningTotal.ToString("#,##0") + " XP.\r\n\r\nWould you like to enchant as much as you can with the available cards?", "Partially Enchant?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                        bEnchantAnyway = true;

                if ((bEnchantAnyway) || ((XPRunningTotal >= CardToEnchant.EnchantToLevelCostXP(card_level)) && (CardsToEat.Count > 0)))
                {
                    Utils.LoggerNotifications("<color=#ffa000>Enchanting will consume " + CardsToEat.Count.ToString("#,##0") + " cards, totalling at least " + CardToEnchant.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP and " + CardToEnchant.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold in costs.</color>");

                    string cards_to_eat_list = "";
                    foreach (GameObjs.Card TempCard in CardsToEat)
                        cards_to_eat_list += "_" + TempCard.ID_User.ToString();

                    if (cards_to_eat_list.Length > 0)
                    {
                        cards_to_eat_list = cards_to_eat_list.Substring(1);

                        string upgrade_preview_json = this.GetGameData("streng", "CardPreview", "UserCardId1=" + CardToEnchant.ID_User.ToString() + "&UserCardId2=" + cards_to_eat_list, false);
                                
                        Utils.Logger(upgrade_preview_json);

                        JObject upgrade_preview = JObject.Parse(upgrade_preview_json);
                        
                        string message = "";
                        try
                        {
                            if (Utils.CInt(upgrade_preview["status"]) == 0)
                                message = upgrade_preview["message"].ToString();
                        }
                        catch { }

                        if (Utils.ValidText(message))
                        {
                            Utils.LoggerNotifications("<color=#ffa000>Card enchantment preview failure!</color>");
                            Utils.LoggerNotifications("<color=#ffa000>... " + message + "</color>");
                        }
                        else if ((Utils.CInt(upgrade_preview["data"]["CardLevel"]) != card_level) && (!bEnchantAnyway))
                        {
                            Utils.LoggerNotifications("<color=#ffa000>Card enchantment preview failure!</color>");
                            Utils.LoggerNotifications("<color=#ffa000>... is one of the enchantment cards in use?</color>");
                        }
                        else
                        {
                            Utils.LoggerNotifications("<color=#ffa000>... actual preview cost is " + Utils.CInt(upgrade_preview["data"]["Exp"]).ToString("#,##0") + " XP and " + Utils.CInt(upgrade_preview["data"]["Coins"]).ToString("#,##0") + " gold in costs.</color>");

                            if (Utils.CInt(upgrade_preview["data"]["Coins"]) > UserGold)
                            {
                                Utils.LoggerNotifications("<color=#ffa000>You need more gold to enchant this card.</color>");
                            }
                            else
                            {
                                string enchant_result_json = this.GetGameData("streng", "Card", "UserCardId1=" + CardToEnchant.ID_User.ToString() + "&UserCardId2=" + cards_to_eat_list, false);
                                
                                Utils.Logger(enchant_result_json);
                                
                                JObject enchant_result = JObject.Parse(enchant_result_json);

                                Utils.LoggerNotifications("<color=#ffa000>Card has been enchanted to level " + upgrade_preview["data"]["CardLevel"].ToString() + "!</color>");
                            }
                        }
                    }
                    else
                    {
                        // logic error: shouldn't be able to get here

                        Utils.LoggerNotifications("<color=#ffa000>Enchanting needs consume at least " + CardToEnchant.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP and " + CardToEnchant.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold in costs.  The " + CardsToEat.Count.ToString("#,##0") + " cards available to enchant with only add up to " + XPRunningTotal.ToString("#,##0") + " XP.</color>");
                    }
                }
                else if (CardsToEat.Count > 0)
                {
                    Utils.LoggerNotifications("<color=#ffa000>Enchanting needs consume at least " + CardToEnchant.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP and " + CardToEnchant.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold in costs.  The " + CardsToEat.Count.ToString("#,##0") + " cards available to enchant with only add up to " + XPRunningTotal.ToString("#,##0") + " XP.</color>");
                }
                else
                {
                    Utils.LoggerNotifications("<color=#ffa000>Enchanting needs consume at least " + CardToEnchant.EnchantToLevelCostXP(card_level).ToString("#,##0") + " XP and " + CardToEnchant.EnchantToLevelCostGold(card_level).ToString("#,##0") + " gold in costs.  There are not enough available cards to enchant with in your collection.  Check your collection and/or your settings to make sure EK Unleashed is allowed to enchant with 1* and 2* cards, for example.</color>");
                }
            }

            Utils.LoggerNotifications("<color=#ffa000>Auto-enchant finished.</color>");
        }

        public void EnchantRune(string rune_name, int rune_level, int unique_rune_id_to_enchant = 0)
        {
            JObject EnchantThisRune = this.GetRuneByName(rune_name);
            
            if (EnchantThisRune == null)
            {
                Utils.LoggerNotifications("<color=#ffa000>Couldn't find a matching rune to enchant.</color>");
                return;
            }

            JObject user_data = JObject.Parse(this.GetGameData("user", "GetUserInfo", false));
            int UserGold = Utils.CInt(user_data["data"]["Coins"]);

            this.UserRunes_CachedData = null;
            this.GetUsersRunes();
            
            GameObjs.Rune RuneToEnchant = null;
            List<GameObjs.Rune> RunesToEat = new List<GameObjs.Rune>();

            if (unique_rune_id_to_enchant == 0)
            {
                for (int iLevelCap = rune_level - 1; iLevelCap >= 0; iLevelCap--)
                {
                    foreach (var jRune in this.UserRunes_CachedData["data"]["Runes"])
                    {
                        GameObjs.Rune TempRune = new GameObjs.Rune(jRune);

                        if ((TempRune.Level == iLevelCap) && (TempRune.ID_Generic == Utils.CInt(EnchantThisRune["RuneId"])))
                        {
                            if (TempRune.MaxLevel >= rune_level)
                            {
                                Utils.LoggerNotifications("<color=#ffa000>Found a level " + TempRune.Level.ToString() + " " + TempRune.Name + " to enchant to level " + rune_level.ToString() + "!</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempRune.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP to enchant</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempRune.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold to enchant</color>");

                                RuneToEnchant = TempRune;
                                break;
                            }
                        }
                    }

                    if (RuneToEnchant != null)
                        break;
                }
            }
            else
            {
                foreach (var jRune in this.UserRunes_CachedData["data"]["Runes"])
                {
                    GameObjs.Rune TempRune = new GameObjs.Rune(jRune);

                    if (TempRune.Level < TempRune.MaxLevel)
                    {
                        if (TempRune.MaxLevel >= rune_level)
                        {
                            if (TempRune.ID_User == unique_rune_id_to_enchant)
                            {
                                Utils.LoggerNotifications("<color=#ffa000>Enchanting your level " + TempRune.Level.ToString() + " " + TempRune.Name + " to level " + rune_level.ToString() + "!</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempRune.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP to enchant</color>");
                                Utils.LoggerNotifications("<color=#ffa000>... it will cost " + TempRune.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold to enchant</color>");
                                
                                RuneToEnchant = TempRune;
                                break;
                            }
                        }
                    }
                }
            }

            if (RuneToEnchant == null)
            {
                Utils.LoggerNotifications("<color=#ffa000>You don't have an eligible " + EnchantThisRune["RuneName"].ToString().Trim() + " to enchant.</color>");
            }
            else if (((double)UserGold) < (((double)RuneToEnchant.EnchantToLevelCostGold(rune_level)) * 1.05))
            {
                Utils.LoggerNotifications("<color=#ffa000>You need more gold to enchant this rune.</color>");
            }
            else
            {
                GameObjs.Rune.StarsAllowedToEnchantWith = Utils.SubStrings(Utils.GetAppSetting("Enchant_Runes_WithStars").Replace(" ", ""), ",");
                GameObjs.Rune.RunesExcludedFromEnchantingWith = Utils.SubStrings(Utils.GetAppSetting("Enchant_Runes_Excluded").Replace(" ", ""), ",");
                List<string> temp = new List<string>();
                foreach (string s in GameObjs.Rune.RunesExcludedFromEnchantingWith)
                {
                    JObject rune = this.GetRuneByName(s);
                    if (rune != null)
                        temp.Add(Utils.CInt(rune["RuneId"]).ToString());
                }
                GameObjs.Rune.RunesExcludedFromEnchantingWith = temp.ToArray();


                int XPRunningTotal = 0;

                Dictionary<int,int> runeCount = new Dictionary<int,int>();

                foreach (var jRune in this.UserRunes_CachedData["data"]["Runes"])
                {
                    GameObjs.Rune TempRune = new GameObjs.Rune(jRune);

                    if (!runeCount.ContainsKey(TempRune.ID_Generic))
                        runeCount.Add(TempRune.ID_Generic, 1);
                    else
                        runeCount[TempRune.ID_Generic]++;
                }

                Dictionary<int,int> runeUsed = new Dictionary<int,int>();
                int iDontUseMoreThanThreshold = Utils.GetAppValue("Enchant_Runes_ReserveThreshold", 1);

                List<int> runesInDecks = new List<int>();
                string json = this.GetGameData(ref GameClient.Current.opts, "card", "GetCardGroup", false);
                JObject decks = JObject.Parse(json);
                foreach (JObject deck in decks["data"]["Groups"])
                    foreach (JObject rune in deck["UserRuneInfo"])
                        if (Utils.CInt(rune["UserRuneId"]) > 0)
                            if (!runesInDecks.Contains(Utils.CInt(rune["UserRuneId"])))
                                runesInDecks.Add(Utils.CInt(rune["UserRuneId"]));

                foreach (var jRune in this.UserRunes_CachedData["data"]["Runes"])
                {
                    GameObjs.Rune TempRune = new GameObjs.Rune(jRune);

                    if (TempRune.OkayToConsume && !runesInDecks.Contains(TempRune.ID_User))
                    {
                        int iUsedSoFar = 0;
                        if (runeUsed.ContainsKey(TempRune.ID_Generic))
                            iUsedSoFar = runeUsed[TempRune.ID_Generic];

                        if (runeCount[TempRune.ID_Generic] - iUsedSoFar <= iDontUseMoreThanThreshold)
                            continue;

                        //Utils.LoggerNotifications("Found a level " + TempRune.Level.ToString() + " " + TempRune.Name + " to enchant with for " + TempRune.EnchantingWorth.ToString("#,##0") + " XP");

                        XPRunningTotal += TempRune.EnchantingWorth;
                        RunesToEat.Add(TempRune);

                        if (!runeUsed.ContainsKey(TempRune.ID_Generic))
                            runeUsed.Add(TempRune.ID_Generic, 1);
                        else
                            runeUsed[TempRune.ID_Generic]++;

                        if (XPRunningTotal >= RuneToEnchant.EnchantToLevelCostXP(rune_level))
                            break;
                    }
                }

                bool bEnchantAnyway = false;

                if ((RunesToEat.Count > 0) && (XPRunningTotal < RuneToEnchant.EnchantToLevelCostXP(rune_level)))
                    if (MessageBox.Show("Enchanting needs consume at least " + RuneToEnchant.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP and " + RuneToEnchant.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold in costs.  The " + RunesToEat.Count.ToString("#,##0") + " runes available to enchant with only add up to " + XPRunningTotal.ToString("#,##0") + " XP.\r\n\r\nWould you like to enchant as much as you can with the available runes?", "Partially Enchant?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                        bEnchantAnyway = true;

                if ((bEnchantAnyway) || ((XPRunningTotal >= RuneToEnchant.EnchantToLevelCostXP(rune_level)) && (RunesToEat.Count > 0)))
                {
                    Utils.LoggerNotifications("<color=#ffa000>Enchanting will consume " + RunesToEat.Count.ToString("#,##0") + " runes, totalling at least " + RuneToEnchant.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP and " + RuneToEnchant.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold in costs.</color>");

                    string runes_to_eat_list = "";
                    foreach (GameObjs.Rune TempRune in RunesToEat)
                        runes_to_eat_list += "_" + TempRune.ID_User.ToString();

                    if (runes_to_eat_list.Length > 0)
                    {
                        runes_to_eat_list = runes_to_eat_list.Substring(1);

                        string upgrade_preview_json = this.GetGameData("streng", "RunePreview", "UserRuneId1=" + RuneToEnchant.ID_User.ToString() + "&UserRuneId2=" + runes_to_eat_list, false);
                                
                        Utils.Logger(upgrade_preview_json);

                        JObject upgrade_preview = JObject.Parse(upgrade_preview_json);
                        
                        string message = "";
                        try
                        {
                            if (Utils.CInt(upgrade_preview["status"]) == 0)
                                message = upgrade_preview["message"].ToString();
                        }
                        catch { }

                        if (Utils.ValidText(message))
                        {
                            Utils.LoggerNotifications("<color=#ffa000>Rune enchantment preview failure!</color>");
                            Utils.LoggerNotifications("<color=#ffa000>... " + message + "</color>");
                        }
                        else if ((Utils.CInt(upgrade_preview["data"]["RuneLevel"]) != rune_level) && (!bEnchantAnyway))
                        {
                            Utils.LoggerNotifications("<color=#ffa000>Rune enchantment preview failure!</color>");
                            Utils.LoggerNotifications("<color=#ffa000>... is one of the enchantment runes in use?</color>");
                        }
                        else
                        {
                            Utils.LoggerNotifications("<color=#ffa000>... actual preview cost is " + Utils.CInt(upgrade_preview["data"]["Exp"]).ToString("#,##0") + " XP and " + Utils.CInt(upgrade_preview["data"]["Coins"]).ToString("#,##0") + " gold in costs.</color>");

                            if (Utils.CInt(upgrade_preview["data"]["Coins"]) > UserGold)
                            {
                                Utils.LoggerNotifications("<color=#ffa000>You need more gold to enchant this rune.</color>");
                            }
                            else
                            {
                                //todo: remove (if enabled/debugging)
                                //Utils.LoggerNotifications("--- disabled --- debug mode --- disabled ---");
                                //Utils.LoggerNotifications(upgrade_preview_json);

                                string enchant_result_json = this.GetGameData("streng", "Rune", "UserRuneId1=" + RuneToEnchant.ID_User.ToString() + "&UserRuneId2=" + runes_to_eat_list, false);
                                
                                Utils.Logger(enchant_result_json);
                                
                                JObject enchant_result = JObject.Parse(enchant_result_json);

                                Utils.LoggerNotifications("<color=#ffa000>Rune has been enchanted to level " + upgrade_preview["data"]["RuneLevel"].ToString() + "!</color>");
                            }
                        }
                    }
                    else
                    {
                        // logic error: shouldn't be able to get here

                        Utils.LoggerNotifications("<color=#ffa000>Enchanting needs consume at least " + RuneToEnchant.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP and " + RuneToEnchant.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold in costs.  The " + RunesToEat.Count.ToString("#,##0") + " runes available to enchant with only add up to " + XPRunningTotal.ToString("#,##0") + " XP.</color>");
                    }
                }
                else if (RunesToEat.Count > 0)
                {
                    Utils.LoggerNotifications("<color=#ffa000>Enchanting needs consume at least " + RuneToEnchant.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP and " + RuneToEnchant.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold in costs.  The " + RunesToEat.Count.ToString("#,##0") + " runes available to enchant with only add up to " + XPRunningTotal.ToString("#,##0") + " XP.</color>");
                }
                else
                {
                    Utils.LoggerNotifications("<color=#ffa000>Enchanting needs consume at least " + RuneToEnchant.EnchantToLevelCostXP(rune_level).ToString("#,##0") + " XP and " + RuneToEnchant.EnchantToLevelCostGold(rune_level).ToString("#,##0") + " gold in costs.  There are not enough available runes to enchant with in your collection.  Check your collection and/or your settings to make sure EK Unleashed is allowed to enchant with 1* and 2* runes, for example.</color>");
                }
            }

            Utils.LoggerNotifications("<color=#ffa000>Auto-enchant finished.</color>");
        }

        public bool _doing_invasion = false;
        public bool _invasion_ended = false;
        public DateTime _DIStarted = DateTime.MinValue;

        public bool DoingDemonInvasion
        {
            get
            {
                bool local_is_doing_demon_invasion = false;

                lock (this.locker)
                    local_is_doing_demon_invasion = this._doing_invasion;

                return local_is_doing_demon_invasion;
            }
            set
            {
                if (value)
                {
                    if (!this.DoingDemonInvasion)
                    {
                        this._DIStarted = DateTime.Now;
                        Utils.StartMethodMultithreaded(this.Play_DemonInvasion);
                    }
                }
                else
                {
                    lock (this.locker)
                    {
                        this._invasion_ended = true;
                        this._DIStarted = DateTime.MinValue;
                        this._DILastInvasionEnded = DateTime.Now;
                    }
                }
            }
        }

        public DateTime DIStarted
        {
            get
            {
                DateTime local_DIStarted = DateTime.MinValue;

                lock (this.locker)
                {
                    local_DIStarted = this._DIStarted;
                }

                return local_DIStarted;
            }
        }

        public bool DemonInvasion_ShouldEnd
        {
            get
            {
                if (!this.DoingDemonInvasion)
                    return true;

                if (this.DIStarted != DateTime.MinValue)
                    if ((DateTime.Now - this.DIStarted).TotalMinutes >= 120.0)
                        return true;

                return false;
            }
        }

        private DateTime _DILastInvasionEnded = DateTime.MinValue;

        public void Play_StopDemonInvasion()
        {
            lock (this.locker)
            {
                this._invasion_ended = true;
                this._DIStarted = DateTime.MinValue;
                this._DILastInvasionEnded = DateTime.Now;
            }

            Scheduler.ScheduledEvent.AllowAllEvents();
        }

        public bool DemonInvasion_CanStart
        {
            get
            {
                bool can = true;

                lock (this.locker)
                {
                    if (this._DILastInvasionEnded != DateTime.MinValue)
                        if ((DateTime.Now - this._DILastInvasionEnded).TotalMinutes < 60.0)
                            can = false;
                }

                return can;
            }
        }

        public EKUnleashed.frmMain ParentForm = null;

        public bool DI_BoughtCooldown = false;

        public int User_Gems = 0;
        public int User_Level = 0;

        public void Play_DemonInvasion_CooldownWait(string BossName, string CardReward, int current_cooldown, int waited_for_fight_data = 0, JObject initial_DI_boss = null, JObject initial_DI_rank = null)
        {
            if (current_cooldown - waited_for_fight_data > 4)
            {
                DateTime dtStart = GameClient.DateTimeNow;

                int iPasses = 0;
                for (DateTime dtStartLoop = DateTime.Now; (DateTime.Now - dtStartLoop).TotalMinutes <= 15; ) // failsafe: wait up to 15 minutes before bailing
                {
                    iPasses++;

                    lock (this.locker)
                    {
                        if (this._invasion_ended)
                            break;

                        if (this.DI_BoughtCooldown)
                            break;
                    }

                    if ((GameClient.DateTimeNow - dtStart).TotalSeconds >= current_cooldown - (waited_for_fight_data + 4))
                        break;

                    try
                    {
                        JObject DI_boss = null;
                        if (iPasses == 0 && initial_DI_boss != null)
                            DI_boss = initial_DI_boss;
                        else
                            DI_boss = JObject.Parse(this.GetGameData("boss", "GetBoss", false));

                        int remaining_cooldown__live = Utils.CInt(DI_boss["data"]["CanFightTime"].ToString());

                        if (remaining_cooldown__live <= 0)
                            break;

                        JObject DI_rank = null;
                        if (iPasses == 0 && initial_DI_rank != null)
                            DI_rank = initial_DI_rank;
                        else
                            DI_rank = JObject.Parse(this.GetGameData("boss", "GetRanks", "Amount=10&StartRank=1", false));

                        this.ParentForm.DemonInvasion_UpdateData(DI_boss, DI_rank);

                        int gem_cost = ((remaining_cooldown__live + 59) / 60) * 10;

                        this.ParentForm.UpdateDIButton(true, (this.User_Gems >= gem_cost) && (!this.DI_BoughtCooldown), "End Cooldown\r\nCosts: " + gem_cost.ToString("#,##0") + " gems\r\n(you have: " + this.User_Gems.ToString("#,##0") + " gems)");

                        int DI_current_ranking = Utils.CInt(DI_rank["data"]["Rank"].ToString());

                        int minutes = remaining_cooldown__live / 60;
                        int seconds = remaining_cooldown__live % 60;

                        this.ParentForm.SetText("EK Unleashed", this.Login_NickName + "  ::  " + this.ServerName + "  ::  Fighting: " + BossName + "  ::  Rank #" + DI_current_ranking.ToString("#,##0") + " ::  CD " + minutes.ToString() + ":" + seconds.ToString("00"));
                        this.ParentForm.RefreshWindow(); // required to re-paint the title bar
                    }
                    catch { }

                    for (int i = 0; i < 5; i++) // wait a quarter second
                    {
                        lock (this.locker)
                        {
                            if (this._invasion_ended)
                                break;

                            if (this.DI_BoughtCooldown)
                                break;
                        }

                        System.Threading.Thread.Sleep(50);
                    }
                }
            }

            lock (this.locker)
                this.DI_BoughtCooldown = false;

            this.ParentForm.UpdateDIButton();

            this.ParentForm.UpdateDIVitals("");
        }

        private object locker_di_entrance = new object();

        public void Play_DemonInvasion()
        {
            string result = string.Empty;

            lock (this.locker_di_entrance)
            {
                if (!this.DemonInvasion_CanStart)
                    return;

                lock (this.locker)
                {
                    this._doing_invasion = true;
                    this._invasion_ended = false;
                }
            }

            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                Utils.StartMethodMultithreaded(this.ParentForm.DemonInvasion_Start);

                bool ShownBossInfo = false;
                int DI_current_ranking = 0;
                int DI_deck_to_use = 0;
                int DI_total_merit_earned = 0;
                string CurrentBossName = "";
                string CardReward = "";
                //bool first_attack = true;
                bool currently_on_special_DI_deck = false;

                for (DateTime dtStartLoop = DateTime.Now; (DateTime.Now - dtStartLoop).TotalMinutes < 210; ) // failsafe: do not run Demon Invasion for longer than 2 hours, 30 minutes
                {
                    try
                    {
                        lock (this.locker)
                            if (this._invasion_ended)
                                break;

                        bool DeckSwitched = false;
                        JObject boss_info = JObject.Parse(this.GetGameData(ref this.opts, "boss", "GetBoss", false));
                        string boss_card_id = boss_info["data"]["Boss"]["BossCardId"].ToString().Replace("\"", "").Trim();
                        int boss_current_HP = Utils.CInt(boss_info["data"]["Boss"]["BossCurrentHp"].ToString());
                        int current_cooldown = Utils.CInt(boss_info["data"]["CanFightTime"].ToString());
                        DI_total_merit_earned = Utils.CInt(boss_info["data"]["MyHonor"].ToString());
                        int minutes = 0, seconds = 0;

                        if (!ShownBossInfo)
                        {
                            ShownBossInfo = true;
                            Utils.Logger();
                            Utils.LoggerNotifications("<color=#ffa000>Now fighting the demon [Card #" + boss_card_id + "] who has " + boss_current_HP.ToString("#,##0") + " HP...</color>");

                            Utils.Logger("<b>The demon " + this.FriendlyReplacerInbound("[Card #" + boss_card_id + "]") + " is here with " + boss_current_HP.ToString("#,##0") + " HP");

                            CurrentBossName = this.NameOfDemon(boss_card_id).Trim();
                            DI_deck_to_use = Utils.CInt(this.DeckToUseForDI(boss_card_id));

                            try
                            {
                                foreach (JObject rank_award in boss_info["data"]["Boss"]["RankAwards"])
                                {
                                    if (Utils.CInt(rank_award["StartRank"].ToString()) == 1)
                                    {
                                        CardReward = this.GetCardName(rank_award["AwardValue"].ToString().Replace("\"", "").Trim()).Trim();
                                        break;
                                    }
                                }
                            }
                            catch { }

                            this.ParentForm.UpdateDIVitals(null, boss_current_HP.ToString("#,##0"), CurrentBossName, CardReward);

                            if (boss_current_HP == 0)
                                Utils.LoggerNotifications("<color=#ffa000>... this demon invasion has ended already!</color>");
                        }

                        if (Utils.CInt(boss_info["data"]["BossFleeTime"].ToString()) <= 0) // boss escaped -- we're done here
                        {
                            this.Play_StopDemonInvasion();
                            continue;
                        }

                        if (this.DemonInvasion_ShouldEnd) // internal limit to avoid a problem at the server that causes infinite DI combat
                        {
                            this.Play_StopDemonInvasion();
                            continue;
                        }

                        if (boss_current_HP <= 0) // boss died -- we're done here
                        {
                            this.Play_StopDemonInvasion();
                            continue;
                        }

                        Utils.Logger();

                        if (current_cooldown > 0)
                        {
                            minutes = current_cooldown / 60;
                            seconds = current_cooldown % 60;
                            Utils.Logger("<b>Boss fight now on cooldown for " + minutes.ToString() + ":" + seconds.ToString("00") + "</b>");
                            Utils.Logger();

                            this.Play_DemonInvasion_CooldownWait(CurrentBossName, CardReward, current_cooldown, 0, boss_info, null);
                            continue;
                        }

                        Utils.Logger();

                        if ((this.AutomaticDIDeckSwapOnCooldown || !currently_on_special_DI_deck) && !DeckSwitched && (DI_deck_to_use > 0))
                        {
                            this.GetGameData("card", "SetDefalutGroup", "GroupId=" + DI_deck_to_use.ToString(), false); // switch to DI deck
                            Utils.LoggerNotifications("<color=#a07000>Switched to demon invasion deck</color>");
                            DeckSwitched = true;
                            currently_on_special_DI_deck = true;
                        }

                        Utils.Logger();

                        JObject fight = JObject.Parse(this.GetGameData(ref this.opts, "boss", "Fight", false));

                        if (Utils.CInt(fight["status"].ToString()) == 0)
                        {
                            if (fight["message"].ToString().Contains("Someone has already eliminated the boss."))
                                this.Play_StopDemonInvasion();

                            continue;
                        }

                        //first_attack = false;

                        JObject fight_data = null;

                        int waited_for_fight_data = 0;
                        if (Utils.CInt(fight["status"].ToString()) == 1) // if the battle was successful
                        {
                            for (DateTime dtStartLoop_FightQueueWait = DateTime.Now; (DateTime.Now - dtStartLoop_FightQueueWait).TotalSeconds <= 30; ) // wait up to 30 seconds
                            {
                                try
                                {
                                    fight_data = JObject.Parse(this.GetGameData(ref this.opts, "boss", "GetFightData", false));

                                    if (Utils.CInt(fight_data["status"].ToString()) == 0) // for the fight data to be valid
                                    {
                                        waited_for_fight_data = (int)((DateTime.Now - dtStartLoop_FightQueueWait).TotalSeconds);
                                        System.Threading.Thread.Sleep(250); // still waiting in the queue 
                                    }
                                    else
                                        break;
                                }
                                catch
                                {
                                    System.Threading.Thread.Sleep(50); // in case of HTTP error (which will break the fight_data JObject)
                                }
                            }
                        }

                        int DI_top_rank_merit = 0;
                        string DI_top_rank_merit_name = "(nobody)";
                        JObject DI_rank = null;
                        try
                        {
                            DI_rank = JObject.Parse(this.GetGameData("boss", "GetRanks", "Amount=10&StartRank=1", false));

                            DI_top_rank_merit = Utils.CInt(DI_rank["data"]["RankUsers"][0]["Honor"].ToString());
                            DI_top_rank_merit_name = DI_rank["data"]["RankUsers"][0]["NickName"].ToString();
                            DI_current_ranking = Utils.CInt(DI_rank["data"]["Rank"].ToString());
                        }
                        catch { }

                        this.ParentForm.SetText("EK Unleashed", this.Login_NickName + "  ::  " + this.ServerName + "  ::  DI rank #" + DI_current_ranking.ToString("#,##0") + "  ::  " + CurrentBossName + "  ::  prize is " + CardReward);
                        this.ParentForm.RefreshWindow(); // required to re-paint the title bar

                        current_cooldown = Utils.CInt(fight["data"]["CanFightTime"].ToString());

                        string reward_gold = "0"; try { reward_gold = Utils.CInt(fight_data["data"]["ExtData"]["Award"]["Coins"]).ToString(); } catch { }
                        string reward_EXP = "0"; try { reward_EXP = Utils.CInt(fight_data["data"]["ExtData"]["Award"]["Exp"]).ToString(); } catch { }
                        string reward_merit = "0"; try { reward_merit = Utils.CInt(fight_data["data"]["ExtData"]["Award"]["Honor"]).ToString(); } catch { }

                        if (boss_current_HP <= 0) // boss died -- we're done here
                        {
                            this.Play_StopDemonInvasion();
                            continue;
                        }

                        if (current_cooldown == 0) // error during the fight
                        {
                            System.Threading.Thread.Sleep(100);
                            continue;
                        }

                        minutes = current_cooldown / 60;
                        seconds = current_cooldown % 60;

                        Utils.Logger();

                        if (Utils.CInt(reward_merit) > 0)
                        {
                            Utils.Logger("<b>Merit earned from demon invasion battle:</b> " + Utils.CInt(reward_merit).ToString("#,##0"));

                            Utils.LoggerNotifications("<color=#ffa000>Demon invasion battle result: " + new GameReward(fight_data).AllAwards + "</color>");
                        }
                        if (Utils.CInt(reward_gold) > 0) Utils.Logger("<b>Gold earned from demon invasion battle:</b> " + Utils.CInt(reward_gold).ToString("#,##0"));
                        if (Utils.CInt(reward_EXP) > 0) Utils.Logger("<b>Experience earned from demon invasion battle:</b> " + Utils.CInt(reward_EXP).ToString("#,##0"));
                        Utils.Logger("<b>Boss fight now on cooldown for " + minutes.ToString() + ":" + seconds.ToString("00") + "</b>");
                        Utils.Logger("<b>Current ranking is: " + DI_current_ranking.ToString("#,##0") + "</b>");
                        Utils.Logger("<b>" + DI_top_rank_merit_name + " in first place has " + DI_top_rank_merit.ToString("#,##0") + " merit</b>");
                        Utils.Logger();

                        if (this.AutomaticDIDeckSwapOnCooldown && DeckSwitched && (Utils.CInt(this.DefaultDeck) > 0))
                        {
                            this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                            Utils.LoggerNotifications("<color=#a07000>Switched to default deck to avoid sniping</color>");
                            currently_on_special_DI_deck = false;
                        }

                        this.Play_DemonInvasion_CooldownWait(CurrentBossName, CardReward, current_cooldown, waited_for_fight_data, boss_info, DI_rank);
                    }
                    catch { }
                }

                JObject DI_rank_final = null;
                try
                {
                    DI_rank_final = JObject.Parse(this.GetGameData("boss", "GetRanks", "Amount=10&StartRank=1", false));

                    int temp_DI_current_ranking = Utils.CInt(DI_rank_final["data"]["Rank"].ToString());
                    if (temp_DI_current_ranking <= 0)
                        temp_DI_current_ranking = Utils.CInt(DI_rank_final["data"]["MyRank"].ToString());
                    if (temp_DI_current_ranking >= 0)
                        DI_current_ranking = temp_DI_current_ranking;


                    int temp_DI_total_merit_earned = Utils.CInt(DI_rank_final["data"]["Honor"].ToString());
                    if (temp_DI_total_merit_earned <= 0)
                        temp_DI_total_merit_earned = Utils.CInt(DI_rank_final["data"]["MyHonor"].ToString());
                    if (temp_DI_total_merit_earned >= DI_total_merit_earned)
                        DI_total_merit_earned = temp_DI_total_merit_earned;
                }
                catch { }

                JObject DI_boss_final = null;
                try
                {
                    DI_boss_final = JObject.Parse(this.GetGameData("boss", "GetBoss", false));

                    int temp_DI_current_ranking = Utils.CInt(DI_boss_final["data"]["Rank"].ToString());
                    if (temp_DI_current_ranking <= 0)
                        temp_DI_current_ranking = Utils.CInt(DI_boss_final["data"]["MyRank"].ToString());
                    if (temp_DI_current_ranking >= 0)
                        DI_current_ranking = temp_DI_current_ranking;

                    int temp_DI_total_merit_earned = Utils.CInt(DI_boss_final["data"]["Honor"].ToString());
                    if (temp_DI_total_merit_earned <= 0)
                        temp_DI_total_merit_earned = Utils.CInt(DI_boss_final["data"]["MyHonor"].ToString());
                    if (temp_DI_total_merit_earned >= DI_total_merit_earned)
                        DI_total_merit_earned = temp_DI_total_merit_earned;
                }
                catch { }

                this.ParentForm.DemonInvasion_UpdateData(DI_boss_final, DI_rank_final);

                Utils.Logger("<b>Finished demon invasion; gained a total of " + DI_total_merit_earned.ToString("#,##0") + " merit!</b>");
                Utils.LoggerNotifications("<color=#ffa000>... finished demon invasion; gained a total of " + DI_total_merit_earned.ToString("#,##0") + " merit and finishing #" + DI_current_ranking.ToString("#,##0") + "!</color>");

                if (Utils.CInt(this.DefaultDeck) > 0)
                {
                    this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                    Utils.LoggerNotifications("<color=#a07000>Switched back to default deck</color>");
                }

                this.ParentForm.SetText("EK Unleashed", this.Login_NickName + "  ::  " + this.ServerName + "  ::  " + this.Service.ToString().Replace("_", " "));
                this.ParentForm.RefreshWindow(); // required to re-paint the title bar

                lock (this.locker)
                {
                    this._doing_invasion = false;
                    this._invasion_ended = false;
                }

                System.Threading.Thread.Sleep(1000);
                this.Play_ClaimAllRewards();
            }

            this.ParentForm.DemonInvasion_End();
            return;
        }

        public string NameOfDemon(string which_demon_invasion_cardID)
        {
            try
            {
                JObject DemonCard = this.GetCardByID(Utils.CInt(which_demon_invasion_cardID));
                if (DemonCard != null)
                    return DemonCard["CardName"].ToString();
            }
            catch { }

            if (this.Service == GameService.Elemental_Kingdoms || this.Service == GameService.Shikoku_Wars || this.Service == GameService.Magic_Realms)
            {
                if (which_demon_invasion_cardID == "9001") return "Deucalion";
                if (which_demon_invasion_cardID == "9002") return "Mars";
                if (which_demon_invasion_cardID == "9003") return "Plague Ogryn";
                if (which_demon_invasion_cardID == "9004") return "Dark Titan";
                if (which_demon_invasion_cardID == "9005") return "Sea King";
                if (which_demon_invasion_cardID == "9006") return "Pandarus";
                //if (which_demon_invasion_cardID == "9007") return "?"; // Azathoth ?
                if (which_demon_invasion_cardID == "9008") return "Pazuzu";
                if (which_demon_invasion_cardID == "9009") return "Bahamut";
            }
            else if (this.Service == GameService.Lies_of_Astaroth || this.Service == GameService.Elves_Realm)
            {
                if (which_demon_invasion_cardID == "9001") return "Mahr";
                if (which_demon_invasion_cardID == "9002") return "Lord Shiva";
                if (which_demon_invasion_cardID == "9003") return "SpiderQueen";
                if (which_demon_invasion_cardID == "9004") return "Onaga";
                if (which_demon_invasion_cardID == "9005") return "Nemesis";
                if (which_demon_invasion_cardID == "9006") return "Demon Fiend";
            }

            return "DI General";
        }

        public string ShortCardInfo(int card_ID, int level, bool compact_view = false)
        {
            try
            {
                JObject users_cards = this.GetUsersCards();
                JObject generic_card_details = this.GetCardByID(card_ID);

                if (compact_view)
                    return "L." + level.ToString() + " " + generic_card_details["CardName"].ToString();
                return "L." + level.ToString() + " " + generic_card_details["CardName"].ToString() + " (" + generic_card_details["Color"].ToString() + "★ " + ConvertCardElementToText(generic_card_details["Race"]) + ")";
            }
            catch (Exception ex) { Utils.DebugLogger(Errors.GetAllErrorDetails(ex)); }

            return string.Empty;
        }

        public string ShortCardInfo(string unique_user_card_ID, bool compact_view = false)
        {
            try
            {
                JObject users_cards = this.GetUsersCards();
                JObject matching_card = null;

                foreach (JObject card in users_cards["data"]["Cards"])
                {
                    if (card["UserCardId"].ToString() == unique_user_card_ID)
                    {
                        matching_card = card;
                        break;
                    }
                }

                if (matching_card != null)
                {
                    JObject generic_card_details = this.GetCardByID(Utils.CInt(matching_card["CardId"].ToString()));

                    if (compact_view)
                        return "L." + matching_card["Level"].ToString() + " " + generic_card_details["CardName"].ToString();
                    return "L." + matching_card["Level"].ToString() + " " + generic_card_details["CardName"].ToString() + " (" + generic_card_details["Color"].ToString() + "★ " + ConvertCardElementToText(generic_card_details["Race"]) + ")";
                }
                else
                    return "[ERROR CARD: " + unique_user_card_ID + "]";
            }
            catch (Exception ex) { Utils.DebugLogger(Errors.GetAllErrorDetails(ex)); }

            return string.Empty;
        }

        public string ShortRuneInfo(int rune_ID, int level, bool compact_view = false)
        {
            try
            {
                JObject users_runes = this.GetUsersRunes();
                JObject generic_rune_details = this.GetRuneByID(rune_ID);

                if (compact_view)
                    return "L." + level.ToString() + " " + generic_rune_details["RuneName"].ToString();
                return "L." + level.ToString() + " " + generic_rune_details["RuneName"].ToString() + " (" + generic_rune_details["Color"].ToString() + "★ " + generic_rune_details["Property"].ToString().Replace("1", "Earth").Replace("2", "Water").Replace("3", "Air").Replace("4", "Fire") + ")";
            }
            catch (Exception ex) { Utils.DebugLogger(Errors.GetAllErrorDetails(ex)); }

            return string.Empty;
        }

        public string ShortRuneInfo(string unique_user_rune_ID, bool compact_view = false)
        {
            try
            {
                JObject users_runes = this.GetUsersRunes();
                JObject matching_rune = null;

                foreach (JObject rune in users_runes["data"]["Runes"])
                {
                    if (rune["UserRuneId"].ToString() == unique_user_rune_ID)
                    {
                        matching_rune = rune;
                        break;
                    }
                }

                if (matching_rune != null)
                {
                    JObject generic_rune_details = this.GetRuneByID(Utils.CInt(matching_rune["RuneId"].ToString()));

                    if (compact_view)
                        return "L." + matching_rune["Level"].ToString() + " " + generic_rune_details["RuneName"].ToString();
                    return "L." + matching_rune["Level"].ToString() + " " + generic_rune_details["RuneName"].ToString() + " (" + generic_rune_details["Color"].ToString() + "★ " + generic_rune_details["Property"].ToString().Replace("1", "Earth").Replace("2", "Water").Replace("3", "Air").Replace("4", "Fire") + ")";
                }
                else
                    return "[ERROR RUNE: " + unique_user_rune_ID + "]";
            }
            catch (Exception ex) { Utils.DebugLogger(Errors.GetAllErrorDetails(ex)); }

            return string.Empty;
        }

        public void Play_ClaimAllRewards(JObject joUserInfo = null)
        {
            string result = "";
            JObject joResult;

            try
            {
                if (joUserInfo == null)
                    joUserInfo = JObject.Parse(this.GetGameData("user", "GetUserInfo"));

                for (;;)
                {
                    bool awarded_something = false;
                    
                    #region Rewards chest
                    if (Utils.True("Game_ClaimRewards"))
                    {
                        this.ClaimChestRewards();
                    }
                    #endregion

                    #region [Events] button reward (leveling up, mostly)
                    if (Utils.True("Game_ClaimLevelingRewards"))
                    {
                        result = this.GetGameData("shop", "GetWelfare", true);
                        joResult = JObject.Parse(result);

                        //Utils.Chatter("Welfare (events/leveling rewards): " + result);
                        //Utils.Chatter();

                        foreach (JObject welfare in joResult["data"]["LevelWelfare"])
                        {
                            if (Utils.CInt(welfare["Level"]) <= Utils.CInt(joUserInfo["data"]["Level"]))
                            {
                                result = this.GetGameData("shop", "RecieveAward", "id=" + welfare["Id"].ToString() + "&type=2", true);
                                JObject joResultTemp = JObject.Parse(result);

                                //Utils.Chatter("Received welfare reward: " + result);
                                //Utils.Chatter();

                                awarded_something = (Utils.CInt(joResultTemp["status"]) == 1);
                            }
                        }
                    }
                    #endregion;

                    #region Achievements
                    if (Utils.True("Game_ClaimAchievementRewards"))
                    {
                        try
                        {
                            result = this.GetGameData("achievement", "GetAchievement", "PageNum=1&achievementType=3", true);
                            joResult = JObject.Parse(result);

                            //Utils.Chatter("Achievements: " + result);
                            //Utils.Chatter();

                            foreach (JObject achievement in joResult["data"]["Achievement"])
                            {
                                result = this.GetGameData("achievement", "GetUserAchievementReward", "Type=1&Id=" + achievement["AchievementId"].ToString(), true);
                                JObject joResultTemp = JObject.Parse(result);

                                //Utils.Chatter("Received achievement reward: " + result);
                                //Utils.Chatter();

                                awarded_something = (Utils.CInt(joResultTemp["status"]) == 1);
                            }
                        }
                        catch { }
                    }
                    #endregion


                    #region World Tree
                    if (Utils.True("Game_ClaimWorldTreeRewards"))
                    {
                        try
                        {
                            result = this.GetGameData("tree", "GetAwardInfo");
                            joResult = JObject.Parse(result);

                            //Utils.Chatter(JSBeautifyLib.JSBeautify.BeautifyMe(result));

                            for (int i = 0; i < 100; i++)
                            {
                                try
                                {
                                    JToken jtTemp = joResult["data"]["TreeAwardSegment"][i.ToString()];

                                    int iAwardableStatus = Utils.CInt(jtTemp["CanAward"]);

                                    if (iAwardableStatus == -1) // already claimed
                                        ;
                                    else if (iAwardableStatus == 0) // can't claim yet
                                        ;
                                    else if (iAwardableStatus == 1) // ready to claim!
                                    {
                                        Utils.LoggerNotifications("<color=#ffa000>Accepted <b>" + jtTemp["AwardName"].ToString() + "</b> world tree reward for reaching " + Utils.CInt(jtTemp["Score"]).ToString("#,##0") + " points!</color>");

                                        string s_rewards_claimed = this.GetGameData("tree", "GetReward", "SegmentId=" + i.ToString());
                                        JObject rewards_claimed = JObject.Parse(s_rewards_claimed);

                                        try
                                        {
                                            foreach (JToken fragments_received in rewards_claimed["data"]["Chips"])
                                            {
                                                int card_id = Utils.CInt(fragments_received["ChipId"]);
                                                int qty = Utils.CInt(fragments_received["ChipNum"]);

                                                Utils.LoggerNotifications("<color=#ffa000>... reward received:  " + qty.ToString("#,##0") + " " +  this.FriendlyReplacerInbound("[Card #" + card_id.ToString() + "]") + " fragment" + ((qty == 1) ? "" : "s") + "</color>");
                                            }
                                        }
                                        catch
                                        {
                                            Utils.LoggerNotifications("<color=#ffa000>... reward received:  " + s_rewards_claimed + "</color>");
                                        }

                                        awarded_something = (Utils.CInt(rewards_claimed["status"]) == 1);
                                    }
                                    else // unknown status
                                        ;
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                    #endregion

                    if (!awarded_something)
                        break;
                }
            }
            catch { }
        }

        public int ClaimAllRewards_SkipSettingsCheck(JObject joUserInfo = null)
        {
            string result = "";
            JObject joResult;
            int total_claimed_rewards = 0;

            try
            {
                if (joUserInfo == null)
                    joUserInfo = JObject.Parse(this.GetGameData("user", "GetUserInfo"));

                for (;;)
                {
                    bool awarded_something = false;
                    
                    #region Rewards chest
                    int chest_rewards_claimed = this.ClaimChestRewards();

                    if (chest_rewards_claimed > 0)
                    {
                        total_claimed_rewards += chest_rewards_claimed;
                        awarded_something = true;
                    }
                    #endregion

                    #region [Events] button reward (leveling up, mostly)
                    result = this.GetGameData("shop", "GetWelfare", true);
                    joResult = JObject.Parse(result);

                    //Utils.Chatter("Welfare (events/leveling rewards): " + result);
                    //Utils.Chatter();

                    foreach (JObject welfare in joResult["data"]["LevelWelfare"])
                    {
                        if (Utils.CInt(welfare["Level"]) <= Utils.CInt(joUserInfo["data"]["Level"]))
                        {
                            result = this.GetGameData("shop", "RecieveAward", "id=" + welfare["Id"].ToString() + "&type=2", true);
                            JObject joResultTemp = JObject.Parse(result);
                            
                            //Utils.Chatter("Received welfare reward: " + result);
                            //Utils.Chatter();

                            if (Utils.CInt(joResultTemp["status"]) == 1)
                            {
                                total_claimed_rewards++;
                                awarded_something = true;
                            }
                        }
                    }

                    #endregion;

                    #region Achievements
                    try
                    {
                        result = this.GetGameData("achievement", "GetAchievement", "PageNum=1&achievementType=3", true);
                        joResult = JObject.Parse(result);
                        
                        //Utils.Chatter("Achievements: " + result);
                        //Utils.Chatter();

                        foreach (JObject achievement in joResult["data"]["Achievement"])
                        {
                            result = this.GetGameData("achievement", "GetUserAchievementReward", "Type=1&Id="+ achievement["AchievementId"].ToString(), true);
                            JObject joResultTemp = JObject.Parse(result);
                            
                            //Utils.Chatter("Received achievement reward: " + result);
                            //Utils.Chatter();

                            if (Utils.CInt(joResultTemp["status"]) == 1)
                            {
                                total_claimed_rewards++;
                                awarded_something = true;
                            }

                        }
                    }
                    catch { }
                    #endregion

                    if (!awarded_something)
                        break;
                }
            }
            catch { }

            return total_claimed_rewards;
        }

        public string FillDeckCustom(string deck_to_use, string deck_cards, string deck_runes)
        {
            if (deck_to_use.ToUpper().Contains("KW")) deck_to_use = "KW";
            if (Utils.CInt(deck_to_use) > 0) deck_to_use = Utils.CInt(deck_to_use).ToString();

            if (deck_to_use != "KW" && deck_to_use != "1" && deck_to_use != "2" && deck_to_use != "3" && deck_to_use != "4" && deck_to_use != "5" && deck_to_use != "6" && deck_to_use != "7" && deck_to_use != "8" && deck_to_use != "9" && deck_to_use != "10") return this.DefaultDeck;

            deck_cards = Utils.CondenseSpacing(deck_cards).Replace(", ", ",");
            deck_runes = Utils.CondenseSpacing(deck_runes).Replace(", ", ",");

            List<string> card_ids_to_use = this.BuildDeckCards(new List<string>(Utils.SubStringsDups(deck_cards, ",")));
            List<string> rune_ids_to_use = this.BuildDeckRunes(new List<string>(Utils.SubStringsDups(deck_runes, ",")));

            string pretty_cards_used = "";
            foreach (string unique_user_card_id in card_ids_to_use)
                pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id);
            pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

            this.UserRunes_CachedData = null;

            string pretty_runes_used = "";
            foreach (string unique_user_rune_id in rune_ids_to_use)
                pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id);
            pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

            Utils.LoggerNotifications("<color=#a07000>Filling deck slot " + deck_to_use + " with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

            string[] fill_results = this.SetDeckInfo
            (
                deck_to_use,
                card_ids_to_use,
                rune_ids_to_use
            );

            if (fill_results == null)
            {
                Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck!</color>");
                return "0";
            }

            if (fill_results.Length == 1)
            {
                Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck, server said: " + fill_results[0] + "</color>");
                return "0";
            }

            pretty_cards_used = "";
            foreach (string unique_user_card_id in Utils.SubStringsDups(fill_results[0], "%5F"))
                pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id, true);
            pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

            this.UserRunes_CachedData = null;

            pretty_runes_used = "";
            foreach (string unique_user_rune_id in Utils.SubStringsDups(fill_results[1], "%5F"))
                pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id, true);
            pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

            Utils.LoggerNotifications("<color=#a07000>... filled deck " + deck_to_use + " with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

            return this.GetDeckIDForOrdinal(deck_to_use);
        }

        public bool FillDeckCustom_DeckID(string group_id, string deck_cards, string deck_runes)
        {
            deck_cards = Utils.CondenseSpacing(deck_cards).Replace(", ", ",");
            deck_runes = Utils.CondenseSpacing(deck_runes).Replace(", ", ",");

            List<string> card_ids_to_use = this.BuildDeckCards(new List<string>(Utils.SubStringsDups(deck_cards, ",")));
            List<string> rune_ids_to_use = this.BuildDeckRunes(new List<string>(Utils.SubStringsDups(deck_runes, ",")));

            string pretty_cards_used = "";
            foreach (string unique_user_card_id in card_ids_to_use)
                pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id);
            pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

            this.UserRunes_CachedData = null;

            string pretty_runes_used = "";
            foreach (string unique_user_rune_id in rune_ids_to_use)
                pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id);
            pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

            string[] fill_results = this.SetDeckInfo_DeckID
            (
                group_id,
                card_ids_to_use,
                rune_ids_to_use
            );

            if (fill_results == null)
                return false;

            if (fill_results.Length == 1)
                return false;

            return true;
        }

        public string DeckToUseForDI(string which_demon_invasion_cardID)
        {
            if (this.Want_Deck_Swap)
            {
                string deck_to_use = Utils.GetAppSetting("DemonInvasion_Deck");
                string deck_cards = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_" + this.NameOfDemon(which_demon_invasion_cardID).Replace(" ", "") + "_DeckCards")).Replace(", ", ",");
                string deck_runes = Utils.CondenseSpacing(Utils.GetAppSetting("DemonInvasion_" + this.NameOfDemon(which_demon_invasion_cardID).Replace(" ", "") + "_DeckRunes")).Replace(", ", ",");

                if (deck_to_use.ToUpper().Contains("KW")) deck_to_use = "KW";
                if (Utils.CInt(deck_to_use) > 0) deck_to_use = Utils.CInt(deck_to_use).ToString();

                if ((deck_to_use != "KW") && (Utils.CInt(deck_to_use) <= 0)) return "0";

                List<string> card_ids_to_use = this.BuildDeckCards(new List<string>(Utils.SubStringsDups(deck_cards, ",")));
                List<string> rune_ids_to_use = this.BuildDeckRunes(new List<string>(Utils.SubStringsDups(deck_runes, ",")));

                string pretty_cards_used = "";
                foreach (string unique_user_card_id in card_ids_to_use)
                    pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id);
                pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                this.UserRunes_CachedData = null;

                string pretty_runes_used = "";
                foreach (string unique_user_rune_id in rune_ids_to_use)
                    pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id);
                pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                Utils.LoggerNotifications("<color=#a07000>Filled Demon Invasion deck (deck slot " + deck_to_use + ") with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

                string[] fill_results = this.SetDeckInfo
                (
                    deck_to_use,
                    card_ids_to_use,
                    rune_ids_to_use
                );

                if (fill_results == null)
                {
                    Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck!</color>");
                    return "0";
                }

                if (fill_results.Length == 1)
                {
                    Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck, server said: " + fill_results[0] + "</color>");
                    return "0";
                }

                pretty_cards_used = "";
                foreach (string unique_user_card_id in Utils.SubStringsDups(fill_results[0], "%5F"))
                    pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id, true);
                pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                this.UserRunes_CachedData = null;

                pretty_runes_used = "";
                foreach (string unique_user_rune_id in Utils.SubStringsDups(fill_results[1], "%5F"))
                    pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id, true);
                pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                Utils.LoggerNotifications("<color=#a07000>... filled deck " + deck_to_use + " with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

                return this.GetDeckIDForOrdinal(deck_to_use);
            }

            return "0";
        }

        private bool ThiefFilled = false;
        public string DeckToUseForThief()
        {
            if (this.Want_Deck_Swap)
            {
                string deck_to_use = Utils.GetAppSetting("Thief_Deck");
                string deck_cards = Utils.CondenseSpacing(Utils.GetAppSetting("Thief_DeckCards")).Replace(", ", ",");
                string deck_runes = Utils.CondenseSpacing(Utils.GetAppSetting("Thief_DeckRunes")).Replace(", ", ",");

                if (deck_to_use.ToUpper().Contains("KW")) deck_to_use = "KW";
                if (Utils.CInt(deck_to_use) > 0) deck_to_use = Utils.CInt(deck_to_use).ToString();

                if ((deck_to_use != "KW") && (Utils.CInt(deck_to_use) <= 0)) return "0";

                if ((!this.ThiefFilled) || (Utils.False("Thief_AlwaysFill")))
                {
                    List<string> card_ids_to_use = this.BuildDeckCards(new List<string>(Utils.SubStringsDups(deck_cards, ",")));
                    List<string> rune_ids_to_use = this.BuildDeckRunes(new List<string>(Utils.SubStringsDups(deck_runes, ",")));

                    if (deck_cards.Length > 0)
                    {
                        string pretty_cards_used = "";
                        foreach (string unique_user_card_id in card_ids_to_use)
                            pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id);
                        pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                        this.UserRunes_CachedData = null;

                        string pretty_runes_used = "";
                        foreach (string unique_user_rune_id in rune_ids_to_use)
                            pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id);
                        pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                        Utils.LoggerNotifications("<color=#a07000>Filled Thief deck (deck slot " + deck_to_use + ") with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

                        string[] fill_results = this.SetDeckInfo
                        (
                            deck_to_use,
                            card_ids_to_use,
                            rune_ids_to_use
                        );

                        if (fill_results == null)
                        {
                            Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck!</color>");
                            return "0";
                        }

                        if (fill_results.Length == 1)
                        {
                            Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck, server said: " + fill_results[0] + "</color>");
                            return "0";
                        }

                        pretty_cards_used = "";
                        foreach (string unique_user_card_id in Utils.SubStringsDups(fill_results[0], "%5F"))
                            pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id, true);
                        pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                        this.UserRunes_CachedData = null;

                        pretty_runes_used = "";
                        foreach (string unique_user_rune_id in Utils.SubStringsDups(fill_results[1], "%5F"))
                            pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id, true);
                        pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                        Utils.LoggerNotifications("<color=#a07000>... filled deck " + deck_to_use + " with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

                        this.ThiefFilled = true;
                    }
                }

                return this.GetDeckIDForOrdinal(deck_to_use);
            }

            return "0";
        }
        
        private bool RaiderFilled = false;
        public string DeckToUseForRaiders()
        {
            if (this.Want_Deck_Swap)
            {
                string deck_to_use = Utils.GetAppSetting("Hydra_Deck");
                string deck_cards = Utils.CondenseSpacing(Utils.GetAppSetting("Hydra_DeckCards")).Replace(", ", ",");
                string deck_runes = Utils.CondenseSpacing(Utils.GetAppSetting("Hydra_DeckRunes")).Replace(", ", ",");

                if (deck_to_use.ToUpper().Contains("KW")) deck_to_use = "KW";
                if (Utils.CInt(deck_to_use) > 0) deck_to_use = Utils.CInt(deck_to_use).ToString();

                if ((deck_to_use != "KW") && (Utils.CInt(deck_to_use) <= 0)) return "0";

                if ((!this.RaiderFilled) || (Utils.False("Hydra_AlwaysFill")))
                {
                    List<string> card_ids_to_use = this.BuildDeckCards(new List<string>(Utils.SubStringsDups(deck_cards, ",")));
                    List<string> rune_ids_to_use = this.BuildDeckRunes(new List<string>(Utils.SubStringsDups(deck_runes, ",")));

                    if (deck_cards.Length > 0)
                    {
                        string pretty_cards_used = "";
                        foreach (string unique_user_card_id in card_ids_to_use)
                            pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id);
                        pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                        this.UserRunes_CachedData = null;

                        string pretty_runes_used = "";
                        foreach (string unique_user_rune_id in rune_ids_to_use)
                            pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id);
                        pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                        Utils.LoggerNotifications("<color=#a07000>Filled raider deck (deck slot " + deck_to_use + ") with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

                        string[] fill_results = this.SetDeckInfo
                        (
                            deck_to_use,
                            card_ids_to_use,
                            rune_ids_to_use
                        );

                        if (fill_results == null)
                        {
                            Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck!</color>");
                            return "0";
                        }

                        if (fill_results.Length == 1)
                        {
                            Utils.LoggerNotifications("<color=#ff4000>... could not fill this deck, server said: " + fill_results[0] + "</color>");
                            return "0";
                        }

                        pretty_cards_used = "";
                        foreach (string unique_user_card_id in Utils.SubStringsDups(fill_results[0], "%5F"))
                            pretty_cards_used += ", " + this.ShortCardInfo(unique_user_card_id, true);
                        pretty_cards_used = pretty_cards_used.TrimStart(new char[] { ',', ' ' });

                        this.UserRunes_CachedData = null;

                        pretty_runes_used = "";
                        foreach (string unique_user_rune_id in Utils.SubStringsDups(fill_results[1], "%5F"))
                            pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id, true);
                        pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                        Utils.LoggerNotifications("<color=#a07000>... filled deck " + deck_to_use + " with: " + (pretty_cards_used + ", " + pretty_runes_used) + "</color>");

                        this.RaiderFilled = true;
                    }
                }

                return this.GetDeckIDForOrdinal(deck_to_use);
            }

            return "0";
        }

        public string _DefaultDeck = "";
        public string DefaultDeck
        {
            get
            {
                if (this.Want_Deck_Swap)
                {
                    if (Utils.CInt(this._DefaultDeck) > 0)
                        return this._DefaultDeck;

                    string deck_to_use = Utils.GetAppSetting("Game_DefaultDeck");

                    if (deck_to_use.ToUpper().Contains("KW")) deck_to_use = "KW";
                    if (Utils.CInt(deck_to_use) > 0) deck_to_use = Utils.CInt(deck_to_use).ToString();

                    if ((deck_to_use != "KW") && (Utils.CInt(deck_to_use) <= 0)) return "0";

                    this._DefaultDeck = this.GetDeckIDForOrdinal(deck_to_use);

                    return this._DefaultDeck;
                }

                return "0";
            }
        }

        public bool ClaimGiftCode(string giftcode, bool supress_badcode = false)
        {
            this.CheckLogin();
            if (this.opts == null)
                return false;

            string result = this.GetGameData("activity", "GiftCodeReward", "code=" + giftcode.Trim());

            if (result.Contains("\"status\":1"))
            {
                Utils.LoggerNotifications("<color=#ffa000>Gift code <b>" + giftcode + "</b> accepted!</color>");
                return true;
            }

            if (supress_badcode)
                if (result.Contains("\"Gift Code Error"))
                    return false;

            if (result.Contains("\"message\":"))
                Utils.LoggerNotifications("<color=#ffa000>Gift code <b>" + giftcode + "</b> rejected:  " + Utils.ChopperBlank(result, "\"message\":\"", "\"") + "</color>");
            else
                Utils.LoggerNotifications("<color=#ffa000>Gift code <b>" + giftcode + "</b> rejected!</color>");

            return false;
        }


        public string GetUserID(string name)
        {
            this.CheckLogin();
            if (this.opts == null)
                return null;

            string result = this.GetGameData(ref this.opts, "friend", "Search", "NickName=" + name.Trim(), false);
            foreach (string s in Utils.SubStrings(Utils.ChopperBlank(result, "{\"Friends\":[{", null), "},{"))
            {
                string nickname = Utils.ChopperBlank(s, "\"NickName\":\"", "\"");
                string uid = Utils.ChopperBlank(s, "\"Uid\":", ",").Replace("\"", "").Trim();

                if (nickname.ToLower() == name.ToLower())
                    return uid;
            }

            return null;
        }

        public void Play_FightThieves()
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                this.Play_FightThieves__real();
            }
        }

        private class ThiefInfo
        {
            public int tuid = 0;
            public int level = 0;
            public bool legendary = false;
            public int remaining_hp;
            public int remaining_time = 0;
        }

        private void Play_FightThieves__real()
        {
            JObject user_thieves = JObject.Parse(this.GetGameData(ref this.opts, "arena", "GetThieves", false));
            
            try { if (Utils.CInt(user_thieves["data"]["Cooldown"])  > 0) return; } catch { } 
            try { if (Utils.CInt(user_thieves["data"]["Countdown"]) > 0) return; } catch { }

            List<ThiefInfo> thieves = new List<ThiefInfo>();
            List<string> thief_friend_blacklist = new List<string>();
            try
            {
                thief_friend_blacklist.AddRange(Utils.CondenseSpacing(Utils.GetAppSetting("Thief_IgnoreFrom")).Replace(", ", ",").Replace("\t", "").Split(new char[] { ',' }));
            }
            catch { }

            try
            {
                foreach (var thief in user_thieves["data"]["Thieves"])
                {
                    try
                    {
                        int tuid = Utils.CInt(thief["UserThievesId"].ToString());
                        int level = Utils.CInt(thief["ThievesId"].ToString());
                        int hp = Utils.CInt(thief["HPCurrent"].ToString());
                        int hpmax = Utils.CInt(thief["HPCount"].ToString());
                        bool legendary = Utils.CInt(thief["Type"].ToString()) == 2;
                        string discovered_by = thief["NickName"].ToString().Trim();
                        int flee_seconds_remaining = Utils.CInt(thief["FleeTime"].ToString());

                        bool skip_adding = false;
                        foreach (string blacklisted_friend in thief_friend_blacklist)
                        {
                            if ((TextComparison.IsExactMatch(blacklisted_friend, discovered_by)) || (discovered_by.Trim().ToLower() == blacklisted_friend.Trim().ToLower()))
                            {
                                skip_adding = true;
                                break;
                            }
                        }

                        if (flee_seconds_remaining < 1)
                            skip_adding = true;

                        if (hp < 1)
                            skip_adding = true;

                        if (!skip_adding)
                        {
                            ThiefInfo ti = new ThiefInfo();

                            ti.tuid = tuid;
                            ti.level = level;
                            ti.remaining_hp = hp;
                            ti.legendary = legendary;
                            ti.remaining_time = flee_seconds_remaining;

                            thieves.Add(ti);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                thieves.Sort((thief1, thief2) =>
                {
                    //Comparer<string>.Default.Compare(combo1, combo2));

                    // legendary over non-legendary
                    if (thief1.legendary != thief2.legendary)
                        return thief1.legendary.CompareTo(thief2.legendary);

                    // high level over low level
                    if (thief1.level != thief2.level)
                        return thief2.level.CompareTo(thief1.level);

                    // low HP over high HP
                    if (thief1.remaining_hp != thief2.remaining_hp)
                        return thief1.remaining_hp.CompareTo(thief2.remaining_hp);

                    // low remaining time over high remaining time
                    return thief1.remaining_time.CompareTo(thief2.remaining_time);
                });
            }
            catch { }

            if (thieves.Count > 0)
            {
                Utils.LoggerNotifications("<color=#a07000>Looking for a thief to fight...</color>");

                int thief_deck_to_use = Utils.CInt(this.DeckToUseForThief());

                try
                {
                    if (thief_deck_to_use > 0)
                    {
                        this.GetGameData("card", "SetDefalutGroup", "GroupId=" + thief_deck_to_use.ToString(), false); // switch to thief deck
                        Utils.LoggerNotifications("<color=#a07000>Switched to thief deck</color>");
                    }
                    else if (Utils.CInt(this.DefaultDeck) > 0)
                    {
                        this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                        Utils.LoggerNotifications("<color=#a07000>Switched to default deck to fight thieves</color>");
                    }
                }
                catch { }

                //bool fought_a_thief = false;

                //for (int i = 1; i <= 2; i++)
                {
                    //if (fought_a_thief)
                    //    break;

                    //foreach (var thief in user_thieves["data"]["Thieves"])
                    {
                        //if (fought_a_thief)
                        //    break;

                        //int tuid = Utils.CInt(thief["UserThievesId"].ToString());
                        //int level = Utils.CInt(thief["ThievesId"].ToString());
                        //int hp = Utils.CInt(thief["HPCurrent"].ToString());
                        //int hpmax = Utils.CInt(thief["HPCount"].ToString());
                        //bool legendary = Utils.CInt(thief["Type"].ToString()) == 2;
                        //string discovered_by = thief["NickName"].ToString().Trim();
                        //int flee_seconds_remaining = Utils.CInt(thief["FleeTime"].ToString());

                        int tuid = thieves[0].tuid;
                        int level = thieves[0].level;
                        int hp = thieves[0].remaining_hp;
                        bool legendary = thieves[0].legendary;
                        int flee_seconds_remaining = thieves[0].remaining_time;

                        int flee_minutes = flee_seconds_remaining / 60;
                        int flee_seconds = flee_seconds_remaining % 60;

                        if ((flee_seconds_remaining > 0) && (hp > 0))
                        {
                            //if (legendary || i == 2)
                            {
                                //Utils.Logger(discovered_by + " has discovered a" + ((legendary) ? " <b>legendary</b>" : "") + " level " + level.ToString() + " thief!  (#" + tuid.ToString() + ")");
                                //Utils.Logger("... HP: " + hp.ToString("#,##0") + "/" + hpmax.ToString("#,##0"));
                                //Utils.Logger("... Time remaining: " + flee_minutes.ToString() + ":" + flee_seconds.ToString("00"));

                                //fought_a_thief = true;

                                JObject thief_fight = JObject.Parse(this.GetGameData(ref this.opts, "arena", "ThievesFight", "UserThievesId=" + tuid.ToString(), false));

                                string reward_gold = "0"; try { reward_gold = Utils.CInt(thief_fight["data"]["ExtData"]["Award"]["Coins"]).ToString(); } catch { }
                                string reward_EXP = "0"; try { reward_EXP = Utils.CInt(thief_fight["data"]["ExtData"]["Award"]["Exp"]).ToString(); } catch { }

                                string reward_card = "";
                                try
                                {
                                    foreach (var this_reward_card in thief_fight["data"]["AwardCards"])
                                        reward_card += ", [Card #" + this_reward_card.ToString() + "]";
                                    reward_card = reward_card.Trim(new char[] { ',', ' ' });
                                }
                                catch { }

                                //Utils.Logger();

                                if (Utils.CInt(reward_gold) > 0) Utils.Logger("<b>Gold earned through thief battle:</b> " + Utils.CInt(reward_gold).ToString("#,##0"));
                                if (Utils.CInt(reward_EXP) > 0) Utils.Logger("<b>Experience earned through thief battle:</b> " + Utils.CInt(reward_EXP).ToString("#,##0"));
                                if (reward_card.Length > 0) Utils.Logger("<b>Card earned through thief battle:</b> " + this.FriendlyReplacerInbound(reward_card));

                                Utils.LoggerNotifications("<color=#ffa000>" + ((legendary) ? "<b>Legendary</b> thief" : "Thief") + " battle " + ((Utils.CInt(thief_fight["data"]["Win"]) == 1) ? "won" : "lost") + ": " + new GameReward(thief_fight).AllAwards + "</color>");

                                Utils.Logger();
                            }
                        }
                    }
                }

                if (Utils.CInt(this.DefaultDeck) > 0 && (thief_deck_to_use > 0))
                {
                    this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                    Utils.LoggerNotifications("<color=#a07000>Switched back to default deck</color>");
                }
            }
        }

        public void Play_TempleMeditation(int threshold = 0, int absolute_count = 0)
        {
            this.CheckLogin();
            if (this.opts == null)
                return;

            int current_gold = 0;

            this.GetGameData(ref this.opts, "meditation", "Deal", false); // sell/claim runes

            string available_npcs_json = this.GetGameData(ref this.opts, "meditation", "Info", false); // list of stuff in the temple that hasn't been sold/claimed yet
            //Utils.Chatter(available_npcs_json);
            //Utils.Chatter();

            JObject available_npcs = JObject.Parse(available_npcs_json);
            int starting_gold = Utils.CInt(available_npcs["data"]["Coins"].ToString());
            int iMeditateCounter = 0;
            bool QuickProcessed = false;

            for (int counter = 0; ; counter++)
            {
                iMeditateCounter++; // this gets reset every 10 go's

                if (absolute_count > 0)
                    if (counter >= absolute_count)
                        break;

                current_gold = Utils.CInt(available_npcs["data"]["Coins"].ToString());

                if (starting_gold - current_gold >= threshold)
                    break;

                int highest_level_NPC = 0;
                foreach (var npc in (JArray)(available_npcs["data"]["NpcList"]))
                    if (Utils.CInt(npc.ToString()) > highest_level_NPC)
                        highest_level_NPC = Utils.CInt(npc.ToString());

                if (highest_level_NPC <= 0)
                    break;

                string npc_name = "";

                if (this.Service == GameService.Lies_of_Astaroth || this.Service == GameClient.GameService.Elves_Realm)
                {
                    if (highest_level_NPC == 4) npc_name = "Diviner";
                    else if (highest_level_NPC == 3) npc_name = "Sophisticate";
                    else if (highest_level_NPC == 2) npc_name = "Archon";
                    else npc_name = "Guard";
                }
                else
                {
                    if (highest_level_NPC == 4) npc_name = "Oracle";
                    else if (highest_level_NPC == 3) npc_name = "Priest";
                    else if (highest_level_NPC == 2) npc_name = "Archon";
                    else npc_name = "Pastor";
                }

                //Utils.Chatter();
                //Utils.Chatter("Mediting at NPC #" + highest_level_NPC.ToString());

                string meditate_json = "";

                for (int i = 0; i < 3; i++)
                {
                    meditate_json = this.GetGameData(ref this.opts, "meditation", "Npc", "NpcId=" + highest_level_NPC.ToString(), false); // meditate for runes
                    if (!meditate_json.Contains(" sleeping"))
                        break;

                    available_npcs_json = this.GetGameData(ref this.opts, "meditation", "Info", false); // list of stuff in the temple that hasn't been sold/claimed yet
                    available_npcs = JObject.Parse(available_npcs_json);

                    foreach (var npc in (JArray)(available_npcs["data"]["NpcList"]))
                        if (Utils.CInt(npc.ToString()) > highest_level_NPC)
                            highest_level_NPC = Utils.CInt(npc.ToString());
                }

                if (meditate_json.Contains(" sleeping"))
                    break;

                string meditate_reward = this.MeditateReward(meditate_json);
                QuickProcessed = false;

                //Utils.Chatter(meditate_json);
                //Utils.Chatter();

                if (meditate_reward == "full")
                    Utils.LoggerNotifications("Couldn't meditate in the temple: out of room!");
                else if (meditate_reward.StartsWith("error:"))
                    Utils.LoggerNotifications("Couldn't meditate in the temple: " + meditate_reward.Substring(7));
                else if (meditate_reward == "none")
                    Utils.LoggerNotifications("Couldn't meditate in the temple: out of money or room?");
                else
                    Utils.LoggerNotifications("<color=#ffa000>Meditated in the temple with " + npc_name + " for:</color> " + meditate_reward);

                available_npcs = JObject.Parse(meditate_json);

                if ((iMeditateCounter % 10) == 0)
                {
                    this.GetGameData(ref this.opts, "meditation", "Deal", false); // sell/claim runes
                    Utils.LoggerNotifications("<color=#a07000>Quick-processed the stuff in the temple</color>");
                    QuickProcessed = true;
                }
            }

            if (!QuickProcessed)
            {
                this.GetGameData(ref this.opts, "meditation", "Deal", false); // sell/claim runes
                Utils.LoggerNotifications("<color=#a07000>Quick-processed the stuff in the temple</color>");
                QuickProcessed = true;
            }
        }

        public void Play_FightMapInvasions()
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                Utils.LoggerNotifications("<color=#a07000>Looking for map invaders to fight off...</color>");

                if (Utils.CInt(this.DefaultDeck) > 0)
                {
                    this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                    Utils.LoggerNotifications("<color=#a07000>Switched to default deck to fight map invaders</color>");
                }

                JObject user_map_completion = JObject.Parse(this.GetGameData(ref this.opts, "mapstage", "GetUserMapStages", false));
                for (int i = 0; i < 1000; i++)
                {
                    try
                    {
                        if (Utils.CInt(user_map_completion["data"][i.ToString()]["CounterAttackTime"].ToString()) > 0)
                        {
                            int map_id = Utils.CInt(user_map_completion["data"][i.ToString()]["MapStageDetailId"].ToString());

                            if (map_id > 0)
                            {
                                Utils.Logger("Map stage #" + map_id.ToString() + " has been invaded!");

                                JObject battle_info = JObject.Parse(this.GetGameData(ref this.opts, "mapstage", "EditUserMapStages", "isManual=1&MapStageDetailId=" + map_id.ToString(), false));

                                string battle_result = this.GetGameData(ref this.opts, "mapstage", "EditUserMapStages", "isManual=0&Stages&MapStageDetailId=" + map_id.ToString() + "&battleid=" + battle_info["data"]["BattleId"].ToString(), false);


                                string reward_gold = Utils.ChopperBlank(battle_result, "\"Coins_", "\"");
                                string reward_EXP = Utils.ChopperBlank(battle_result, "\"Exp_", "\"");
                                string reward_card = Utils.ChopperBlank(battle_result, "\"Card_", "\"");

                                Utils.Logger();

                                if (Utils.CInt(reward_gold) > 0) Utils.Logger("<b>Gold earned through map invasion battle:</b> " + Utils.CInt(reward_gold).ToString("#,##0"));
                                if (Utils.CInt(reward_EXP) > 0) Utils.Logger("<b>Experience earned through map invasion battle:</b> " + Utils.CInt(reward_EXP).ToString("#,##0"));
                                if (Utils.CInt(reward_card) > 0) Utils.Logger("<b>Card earned through map invasion battle:</b> " + this.FriendlyReplacerInbound("[Card #" + reward_card + "]"));

                                Utils.LoggerNotifications("<color=#ffa000>Map invasion battle: " + new GameReward(battle_result).AllAwards + "</color>");

                                Utils.Logger();

                            }
                        }
                    }
                    catch { }
                }
            }
        }

        public void Play_Explore()
        {
            this.Play_Explore(true);
            return;
        }

        public void Play_Explore(bool use_all_energy)
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                Utils.LoggerNotifications("<color=#a07000>Exploring...</color>");

                string user_data = this.GetGameData(ref this.opts, "user", "GetUserInfo", false);
                this.GameVitalsUpdate(user_data);
                int user_energy = Utils.CInt(Utils.ChopperBlank(user_data, "\"Energy\":", ",").Replace("\"", "").Trim());

                if (user_energy >= 2)
                {
                    string map_to_explore = this.Play_PickMapToExplore();

                    if (Utils.CInt(map_to_explore) == 0)
                    {
                        Utils.Logger();
                        Utils.Logger("<b>CAN'T FIND A MAP STAGE TO EXPLORE</b>");
                        Utils.Logger();
                        return;
                    }

                    while (user_energy >= 2)
                    {
                        string result = this.GetGameData(ref this.opts, "mapstage", "Explore", "MapStageDetailId=" + map_to_explore);

                        string reward_gold = Utils.ChopperBlank(result, "\"Coins_", "\"");
                        string reward_EXP = Utils.ChopperBlank(result, "\"Exp_", "\"");
                        string reward_card = Utils.ChopperBlank(result, "\"Card_", "\"");

                        Utils.Logger();

                        if (Utils.CInt(reward_gold) > 0) Utils.Logger("<b>Gold earned through exploration:</b> " + Utils.CInt(reward_gold).ToString("#,##0"));
                        if (Utils.CInt(reward_EXP) > 0) Utils.Logger("<b>Experience earned through exploration:</b> " + Utils.CInt(reward_EXP).ToString("#,##0"));
                        if (Utils.CInt(reward_card) > 0) Utils.Logger("<b>Card earned through exploration:</b> " + this.FriendlyReplacerInbound("[Card #" + reward_card + "]"));

                        Utils.LoggerNotifications("<color=#ffa000>Exploration: " + new GameReward(result).AllAwards + "</color>");

                        Utils.Logger();

                        user_energy -= 2;

                        if (Utils.False("Game_FightThieves"))
                            this.Play_FightThieves__real(); // avoid locking and logged in check

                        if (Utils.False("Game_FightHydra"))
                            this.Play_FightRaider_Hydra__real(); // avoid locking and logged in check

                        if (!use_all_energy)
                            break;
                    }
                }

                Utils.Logger();
                Utils.Logger("<b>OUT OF ENERGY .. TRY AGAIN IN A LITTLE WHILE</b>");
                Utils.Logger();
            }
        }

        public void Play_WorldTree(bool manually_started)
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                string result = this.GetGameData("user", "GetUserInfo");
                this.GameVitalsUpdate(result);

                for (int iIteration = 0; iIteration < 10; iIteration++)
                {
                    JObject tree_JSON = JObject.Parse(this.GetGameData("tree", "GetTreeInfo"));

                    if (Utils.CInt(tree_JSON["status"]) == 0)
                        return;

                    int iLastMapUnlocked = 1;
                    string LastMapUnlockedName = "";
                    int iLastPointOnMapUnlocked = 1;
                    string LastPointOnMapUnlockedName = "";

                    int ExploreCD = Utils.CInt(tree_JSON["data"]["ExploreCD"]);
                    int WorldTreePoints = Utils.CInt(tree_JSON["data"]["Score"]);

                    if (ExploreCD <= 0)
                    {
                        if (iIteration == 0)
                        {
                            if (Utils.CInt(this.DefaultDeck) > 0)
                            {
                                this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                                Utils.LoggerNotifications("<color=#a07000>Switched to default deck to fight in the World Tree</color>");
                            }
                        }

                        foreach (var tree_location in tree_JSON["data"]["TreeMap"])
                        {
                            //Utils.LoggerNotifications(tree_location["MapName"].ToString());

                            if (WorldTreePoints >= Utils.CInt(tree_location["UnlockScore"]))
                            {
                                LastMapUnlockedName = tree_location["MapName"].ToString().Trim();
                                iLastMapUnlocked = Utils.CInt(tree_location["MapId"]);
                                iLastPointOnMapUnlocked = 1;
                            }

                            int iCurPoint = 0;
                            foreach (var location_point in tree_location["Points"])
                            {
                                iCurPoint++;

                                //Utils.LoggerNotifications(".... " + location_point["Name"].ToString());

                                if ((WorldTreePoints >= Utils.CInt(tree_location["UnlockScore"])) && (WorldTreePoints >= Utils.CInt(location_point["UnlockScore"])))
                                {
                                    LastPointOnMapUnlockedName = location_point["Name"].ToString().Trim();
                                    iLastPointOnMapUnlocked = iCurPoint;
                                }
                            }
                        }

                        //JObject treemap_JSON = JObject.Parse(Game.GetGameData("tree", "GetMapInfo", true, "world tree, map info"));

                        if (iIteration == 0)
                            Utils.LoggerNotifications("<color=#ffa000>Most recent World Tree map location to explore is <b>" + LastPointOnMapUnlockedName + "</b>, in <b>" + LastMapUnlockedName + "</b> (" + iLastMapUnlocked.ToString() + "-" + iLastPointOnMapUnlocked.ToString() + "):</color>");

                        JObject find_someone_to_fight = JObject.Parse(this.GetGameData("tree", "Explore", true, "world tree, explore"));
                        JObject battle_data = JObject.Parse(this.GetGameData("tree", "Battle", "Uid=" + Utils.CInt(find_someone_to_fight["Uid"]).ToString() + "&isManual=1", true, "world tree, battle"));
                        JObject battle_result = JObject.Parse(this.GetGameData("tree", "ManualBattle", "stage=&isManual=0&battleid=" + battle_data["data"]["BattleId"].ToString(), true, "world tree, manual battle"));

                        Utils.LoggerNotifications("<color=#ffa000>World Tree battle vs. " + find_someone_to_fight["data"]["EnemyInfo"]["NickName"].ToString() + " at rank #" + Utils.CInt(find_someone_to_fight["data"]["EnemyInfo"]["Rank"]).ToString("#,##0") + " for " + ((Utils.CInt(battle_result["data"]["Win"]) == 1) ? "winning" : "losing") + ": " + new GameReward(battle_result).AllAwards + "</color>");
                    }
                    else
                    {
                        if (manually_started)
                            Utils.LoggerNotifications("<color=#ffa000>Can't battle in the World Tree during cooldown.  You will need to wait:  " + Comm.PrettyTimeLeftShort((long)ExploreCD) + "</color>");
                        break;
                    }
                }
            }
        }

        private class ArenaCompetitor
        {
            public string name = "";
            public int uid = 0;
            public int level = 0;
            public int rank = 0;
            public int wins = 0;
            public int losses = 0;

            private double _ct_win_vs = 0.0;
            private double _ct_lose_vs = 0.0;
            private int _win_percentage = 0;

            public int win_percentage
            {
                get
                {
                    if (this._ct_lose_vs + this._ct_win_vs == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            JObject fight_JSON = JObject.Parse(GameClient.Current.GetGameData("arena", "FreeFight", "Competitor=" + this.uid.ToString() + "&isManual=0", false));

                            if (Utils.CInt(fight_JSON["data"]["Win"]) == 1) this._ct_win_vs  += 1.0;
                            else                                            this._ct_lose_vs += 1.0;
                        }

                        this._win_percentage = (int)(this._ct_win_vs * 100.0 / (this._ct_win_vs + this._ct_lose_vs));
                    }

                    return this._win_percentage;
                }
            }
        }

        public void Play_ArenaFight()
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                string result = this.GetGameData(ref this.opts, "user", "GetUserInfo", false);
                JObject result_JSON = JObject.Parse(result);

                int iPlayerUID = Utils.CInt(result_JSON["data"]["Uid"]);

                this.GameVitalsUpdate(result);

                // No ranked matches left
                if (Utils.CInt(result_JSON["data"]["RankTimes"]) < 1)
                    return;

                result = this.GetGameData(ref this.opts, "arena", "GetRankCompetitors", false);
                result_JSON = JObject.Parse(result);

                // Still in cooldown
                if (Utils.CInt(result_JSON["data"]["Countdown"]) > 0)
                    return;

                if (Utils.CInt(this.DefaultDeck) > 0)
                {
                    this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                    Utils.LoggerNotifications("<color=#a07000>Switched to default deck to fight a ranked match in the arena</color>");
                }

                Utils.LoggerNotifications("<color=#a07000>Finding an arena competitor to battle...</color>");

                List<string> arena_blacklist = new List<string>();
                try
                {
                    arena_blacklist.AddRange(Utils.CondenseSpacing(Utils.GetAppSetting("Arena_DontAttack")).Replace(", ", ",").Replace("\t", "").Split(new char[] { ',' }));
                }
                catch { }

                try
                {
                    if (Utils.False("Arena_SkipClan"))
                    {
                        JObject jsClanMates = JObject.Parse(this.GetGameData("legion", "GetMember"));
                        foreach (JToken jtClanMate in jsClanMates["data"]["Members"])
                            arena_blacklist.Add(jtClanMate["NickName"].ToString());
                    }

                    if (Utils.False("Arena_SkipFriends"))
                    {
                        JObject jsFriends = JObject.Parse(this.GetGameData("friend", "GetFriends"));
                        foreach (JToken jtFriend in jsFriends["data"]["Friends"])
                            arena_blacklist.Add(jtFriend["NickName"].ToString());
                    }
                }
                catch { }

                List<ArenaCompetitor> competitors = new List<ArenaCompetitor>();
                try
                {
                    foreach (JObject competitor in result_JSON["data"]["Competitors"])
                    {
                        try
                        {
                            ArenaCompetitor ac = new ArenaCompetitor();

                            ac.name = competitor["NickName"].ToString();
                            ac.uid = Utils.CInt(competitor["Uid"]);
                            ac.level = Utils.CInt(competitor["Level"]);
                            ac.wins = Utils.CInt(competitor["Win"]);
                            ac.losses = Utils.CInt(competitor["Lose"]);
                            ac.rank = Utils.CInt(competitor["Rank"]);

                            bool blacklisted = false;
                            foreach (string blacklisted_friend in arena_blacklist)
                            {
                                if ((ac.name.Trim().ToLower() == blacklisted_friend.Trim().ToLower()) || (TextComparison.IsExactMatch(blacklisted_friend, ac.name)))
                                {
                                    blacklisted = true;
                                    break;
                                }
                            }

                            if (!blacklisted)
                                competitors.Add(ac);
                        }
                        catch { }
                    }
                }
                catch { }

                try
                {
                    // if there is at least one competitor and the player isn't the top-ranked player, then...
                    if ((competitors.Count > 0) && (competitors[0].uid != iPlayerUID))
                    {
                        // if we want arena stealth mode, then skip this part entirely
                        if (!Utils.False("Arena_Stealth"))
                        {
                            // if the player isn't in the top 5, then find out which one we're most likely to defeat...
                            bool player_is_in_top_5 = false;

                            for (int i = 0; i < competitors.Count; i++)
                            {
                                if (competitors[i].uid == iPlayerUID)
                                {
                                    player_is_in_top_5 = true;
                                    break;
                                }
                            }

                            if (!player_is_in_top_5)
                            {
                                try
                                {
                                    competitors.Sort((competitor1, competitor2) =>
                                    {
                                        if (competitor1.win_percentage != competitor2.win_percentage)
                                            return competitor2.win_percentage.CompareTo(competitor1.win_percentage);

                                        return competitor1.level.CompareTo(competitor2.level);
                                    });
                                }
                                catch { }
                            }
                        }

                        // (otherwise, if the player is in the top 5 -- or we want arena stealth mode -- then just fight the top-ranked player)

                        string easiest_rank = competitors[0].rank.ToString();
                        string easiest_name = competitors[0].name.ToString();

                        result = this.GetGameData(ref this.opts, "arena", "RankFight", "CompetitorRank=" + easiest_rank);

                        string battle_result = Utils.ChopperBlank(result, "\"Award\":", "}") + ",";
                        string reward_contribution = Utils.ChopperBlank(battle_result, "\"Resources\":", ",").Replace("\"", "").Trim();
                        string reward_gold = Utils.ChopperBlank(battle_result, "\"Coins\":", ",").Replace("\"", "").Trim();
                        string reward_EXP = Utils.ChopperBlank(battle_result, "\"Exp\":", ",").Replace("\"", "").Trim();

                        GameReward gb = new GameReward(result);
                        if (gb.HasRewards)
                            Utils.LoggerNotifications("<color=#ffa000>Arena battle vs. " + easiest_name + " at rank #" + easiest_rank + " for " + ((result.Replace("\"", "").Replace(" ", "").Contains(",Win:1,")) ? "winning" : "losing") + ": " + gb.AllAwards + "</color>");
                        else
                            Utils.LoggerNotifications("<color=#ffa000>Skipped: arena is blacked out during this hour</color>");
                    }
                    else
                        Utils.LoggerNotifications("<color=#a07000>... nobody eligible to fight!</color>");
                }
                catch { }
            }
        }

        public void StopAllEvents()
        {
            foreach (KeyValuePair<string, Scheduler.ScheduledEvent> val in this.ScheduledEvents)
                val.Value.End();
        }

        public void RestartAllEvents()
        {
            foreach (KeyValuePair<string, Scheduler.ScheduledEvent> val in this.ScheduledEvents)
                val.Value.Start();
        }

        public bool ChatIsConnected
        {
            get
            {
                bool chat_connected = false;

                try
                {
                    if (this.Chat != null)
                        chat_connected = this.Chat.Connected;
                }
                catch { }

                return chat_connected;
            }
        }

        public void MasterLogin()
        {
            this.seq_id = Utils.RandomDice(1, 9000, 999);

            this.StopAllEvents();
            this.ScheduledEvents.Clear();

            if (!Want_Game_Login)
                return;

            this.CheckLogin();
            if (this.opts == null)
            {
                Utils.Chatter("<b><color=#ff0000><fs+><fs+><i>COULDN'T LOG IN TO THE GAME!!</i><fx></color></b>");
                Utils.Chatter();
                return;
            }

            lock (this.locker)
            {
                this._doing_invasion = false;
                this._invasion_ended = false;
            }

            //Utils.Chatter("AuthType: " + frmMain.AuthType.ToString());
            //Utils.Chatter("AuthSerial: " + frmMain.AuthSerial.ToString());

            string UserLogin_JSON = this.GetGameData(ref this.opts, "user", "GetUserInfo", false);
            this.GameVitalsUpdate(UserLogin_JSON);

            //Utils.Chatter(UserLogin_JSON);

            JObject user_login_data = JObject.Parse(UserLogin_JSON);

            this.Login_NickName = user_login_data["data"]["NickName"].ToString().Trim();
            this.Login_UID = user_login_data["data"]["Uid"].ToString().Trim();
            bool has_rewards = false;
 
            if (Utils.CInt(this.Login_UID) > 0)
            {
                string sServerName = Utils.ChopperBlank(this.PassportLoginJSON, "\"GS_NAME\":", ",").Replace("\"", "").Trim();
                string sServerChatHost = Utils.ChopperBlank(this.PassportLoginJSON, "\"GS_CHAT_IP\":", ",").Replace("\"", "").Trim();
                string sServerChatPort = Utils.ChopperBlank(this.PassportLoginJSON, "\"GS_CHAT_PORT\":", ",").Replace("\"", "").Trim();
                
                this.ServerName = sServerName;
                this.ParentForm.SetText("EK Unleashed", this.Login_NickName + "  ::  " + this.ServerName + "  ::  " + this.Service.ToString().Replace("_", " "));
                this.ParentForm.RefreshWindow(); // required to re-paint the title bar

                if (Utils.CInt(sServerChatPort) == 0)
                {
                    try
                    {
                        sServerName = "(unknown)";
                        sServerChatHost = Utils.ChopperBlank(this.PassportLoginJSON, "\"GS_CHAT_IP\":", ",").Replace("\"", "").Trim();
                        sServerChatPort = Utils.ChopperBlank(this.PassportLoginJSON, "\"ipport\":", "}").Replace("\"", "").Trim();
                    }
                    catch { }
                }

                if (sServerChatHost.Contains("}"))
                    sServerChatHost = Utils.ChopperBlank(sServerChatHost, null, "}");

                string UserLegion_JSON = this.GetGameData(ref this.opts, "legion", "GetUserLegion", false);
                this.Clan_ID = Utils.ChopperBlank(UserLegion_JSON, "\"LegionId\":", ",").Replace("\"", "").Trim();
                this.Clan_Name = Utils.ChopperBlank(UserLegion_JSON, "\"LegionName\":", ",").Replace("\"", "").Trim();

                if (Utils.True("Login_Chat"))
                {
                    try
                    {
                        if (this.Chat != null)
                            this.Chat.Logout();
                    }
                    catch { }

                    this.Chat = new GameChat();

                    if (this.Chat.Login(new GameChat.ChatServer() { Name = sServerName, Host = sServerChatHost, Port = Utils.CInt(sServerChatPort) }, this.Login_NickName, this.Login_UID))
                    {
                        this.Chat.JoinChannel("Legion_" + this.Clan_ID);

                        if (Utils.CInt(Kingdom_War_ID) > 0)
                            this.Chat.JoinChannel("Country_" + Kingdom_War_ID);

                        this.Chat.AutoReconnect = Utils.False("Chat_AutoReconnect");
                    }
                    else
                    {
                        Utils.Chatter("... could not log into chat system: some features, such as Demon Invasions, will not work correctly!");
                        Utils.Chatter();
                    }
                }

                this.Cards_JSON_Parsed = null;
                this.Runes_JSON_Parsed = null;

                this.All_Cards_JSON = this.GetGameData(ref this.opts, "card", "GetAllCard", false);
                this.All_Runes_JSON = this.GetGameData(ref this.opts, "rune", "GetAllRune", false);
                this.All_Skills_JSON = this.GetGameData(ref this.opts, "card", "GetAllSkill", false);

                this.Cards_JSON_Parsed = JObject.Parse(this.All_Cards_JSON);
                this.Runes_JSON_Parsed = JObject.Parse(this.All_Runes_JSON);
                this.Skills_JSON_Parsed = JObject.Parse(this.All_Skills_JSON);

                Utils.Chatter("<b><i><fs+><fs+>You are now logged into the game.<fx></i></b>");
                Utils.Chatter("");

                if (has_rewards)
                {
                    Utils.Chatter("<color=#ffa000>[Notification]  You have new rewards available!</color>");
                    Utils.LoggerNotifications("<color=#ffa000>[Notification]  You have new rewards available!</color>");
                }

                this.GameVitalsUpdate(this.GetGameData(ref this.opts, "user", "GetUserInfo", false), this.GetGameData(ref this.opts, "arena", "GetRankCompetitors", false), this.GetGameData(ref this.opts, "arena", "GetThieves", false));

                this.ScheduleAllEvents();
            }

            return;
        }

        public void ScheduleAllEvents()
        {
            // every 2m: check for program expiration and refresh user info
            this.ScheduledEvents.Add
            (
                "GameDataUpdate",
                new Scheduler.ScheduledEvent
                (
                    "GameDataUpdate",
                    () =>
                    {
                        lock (this.locker_gamedata)
                        {
                            this.CheckLogin();
                            if (this.opts == null)
                                return;

                            string User_JSON = this.GetGameData(ref this.opts, "user", "GetUserInfo", false);
                            string Arena_JSON = this.GetGameData(ref this.opts, "arena", "GetRankCompetitors", false);
                            string Thief_JSON = this.GetGameData(ref this.opts, "arena", "GetThieves", false);

                            this.GameVitalsUpdate(User_JSON, Arena_JSON, Thief_JSON);
                        }
                    },
                    new TimeSpan(0, 0, 2, 0)
                )
            );

            // every 1m: check for chat server disconnect state and reconnect if desired
            this.ScheduledEvents.Add
            (
                "ChatReconnector",
                new Scheduler.ScheduledEvent
                (
                    "ChatReconnector",
                    () =>
                    {
                        try
                        {
                            if (this.Chat != null)
                            {
                                this.Chat.AutoReconnect = Utils.False("Chat_AutoReconnect");

                                if (this.Chat.AutoReconnect)
                                    if (!this.ChatIsConnected)
                                        this.Chat.Login(this.Chat.CurrentServer, this.Chat.CurrentUserAlias, this.Chat.CurrentUserID);
                            }
                        }
                        catch { }
                    },
                    new TimeSpan(0, 0, 1, 0)
                )
            );

            // every 15m: do field of honor spins, do daily events, and claim rewards
            this.ScheduledEvents.Add
            (
                "DailiesAndRewards",
                new Scheduler.ScheduledEvent
                (
                    "DailiesAndRewards",
                    () =>
                    {
                        if (Utils.True("Game_FOHHappyHour"))
                            this.Play_FieldOfHonorSpins();

                        if (Utils.True("Game_DailyTasks"))
                        {
                            this.Play_DailyFreeCards();
                            this.Play_DailyTasks();
                        }

                        this.Play_ClaimAllRewards();

                        if (Utils.True("Game_CardCrafting"))
                            this.Play_AutomaticallyCraftCards();
                    },
                    new TimeSpan(0, 0, 15, 0)
                )
            );

            // 16:30: run a clan member report
            this.ScheduledEvents.Add
            (
                "Clan Member Report",
                new Scheduler.ScheduledEvent
                (
                    "Clan Member Report",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_ClanMemberReport")) this.ClanMemberReport(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "8:30:00 PM"),
                    new TimeSpan(1, 0, 0, 0)
                )
            );

            #region log in (needed to react to Demon Invasion and Kingdom War events)

            // LoA only has 2 demons
            if (this.Service != GameClient.GameService.Lies_of_Astaroth &&  this.Service != GameClient.GameService.Elves_Realm)
            {
                this.ScheduledEvents.Add
                (
                    "DI_Login1",
                    new Scheduler.ScheduledEvent
                    (
                        "DI_Login1",
                        () =>
                        {
                            if (Utils.True("Game_Events"))
                            {
                                if (Utils.False("Game_FightDemonInvasions"))
                                {
                                    this.GetUsersCards();
                                    this.GetUsersRunes();

                                    if (!GameClient.Current.ChatIsConnected)
                                    {
                                        Scheduler.ScheduledEvent.DisallowAnyEvents();
                                        Scheduler.ScheduledEvent.AddAllowedEvent("GameDataUpdate");
                                        Scheduler.ScheduledEvent.AddAllowedEvent("ChatReconnector");
                                        Scheduler.ScheduledEvent.AddAllowedEvent("TWGCPrz");
                                        
                                        this.MasterLogin();
                                    }
                                }
                            }
                        },
                        Utils.GameEvent(GameClient.DateTimeNow, "4:55:00 AM"),
                        new TimeSpan(1, 0, 0, 0)
                    )
                );
            }

            this.ScheduledEvents.Add
            (
                "DI_Login2",
                new Scheduler.ScheduledEvent
                (
                    "DI_Login2",
                    () =>
                    {
                        if (Utils.True("Game_Events"))
                        {
                            if (!GameClient.Current.ChatIsConnected)
                            {
                                if (Utils.False("Game_FightDemonInvasions"))
                                {
                                    Scheduler.ScheduledEvent.DisallowAnyEvents();
                                    Scheduler.ScheduledEvent.AddAllowedEvent("GameDataUpdate");
                                    Scheduler.ScheduledEvent.AddAllowedEvent("ChatReconnector");
                                    Scheduler.ScheduledEvent.AddAllowedEvent("TWGCPrz");

                                    this.MasterLogin();
                                }
                            }
                        }
                    },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:55:00 PM"),
                    new TimeSpan(1, 0, 0, 0)
                )
            );

            this.ScheduledEvents.Add
            (
                "DI_Login3",
                new Scheduler.ScheduledEvent
                (
                    "DI_Login3",
                    () =>
                    {
                        if (Utils.True("Game_Events"))
                        {
                            if (!GameClient.Current.ChatIsConnected)
                            {
                                if (Utils.False("Game_FightDemonInvasions"))
                                {
                                    Scheduler.ScheduledEvent.DisallowAnyEvents();
                                    Scheduler.ScheduledEvent.AddAllowedEvent("GameDataUpdate");
                                    Scheduler.ScheduledEvent.AddAllowedEvent("ChatReconnector");
                                    Scheduler.ScheduledEvent.AddAllowedEvent("TWGCPrz");

                                    this.MasterLogin();
                                }
                            }
                        }
                    },
                    Utils.GameEvent(GameClient.DateTimeNow, "8:55:00 PM"),
                    new TimeSpan(1, 0, 0, 0)
                )
            );

            this.ScheduledEvents.Add
            (
                "KW_Login",
                new Scheduler.ScheduledEvent
                (
                    "KW_Login",
                    () =>
                    {
                        if (GameClient.Want_Debug)
                        {
                            Utils.LoggerNotifications("<color=#33aaff><b><u>Kingdom Wars auto-login test:</u></b></color>");
                            Utils.LoggerNotifications("<color=#33aaff>... game service check: " + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "passed" : "failed") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... events automation setting: " + ((Utils.True("Game_Events")) ? "enabled" : "disabled") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... chat connected: " + ((GameClient.Current.ChatIsConnected) ? "connected" : "disconnected") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... Fri/Sat/Sun check: " + (((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday)) ? "passed" : "failed") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... Kingdom War automation setting: " + ((Utils.False("Game_FightKW")) ? "enabled" : "disabled") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>....... if tests above passed, settings are enabled, and chat is disconnected, then EKU should reconnect right now</color>");
                        }

                        // Lies of Astaroth doesn't have Kingdom War
                        if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                        {
                            if (Utils.True("Game_Events"))
                            {
                                if (!GameClient.Current.ChatIsConnected)
                                {
                                    if ((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday))
                                    {
                                        if (Utils.False("Game_FightKW"))
                                        {
                                            Scheduler.ScheduledEvent.DisallowAnyEvents();
                                            Scheduler.ScheduledEvent.AddAllowedEvent("GameDataUpdate");
                                            Scheduler.ScheduledEvent.AddAllowedEvent("ChatReconnector");
                                            Scheduler.ScheduledEvent.AddAllowedEvent("TWGCPrz");
                                            Scheduler.ScheduledEvent.AddAllowedEvent("KW_BattleStarter");
                                            Scheduler.ScheduledEvent.AddAllowedEvent("KW_CleanUp");
                                            Scheduler.ScheduledEvent.AddAllowedEvent("KW Fight");
                                            Scheduler.ScheduledEvent.AddAllowedEvent("KW Fight Followup");
                                                
                                            this.MasterLogin();
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Utils.GameEvent(GameClient.DateTimeNow, (this.Service == GameService.Magic_Realms) ? "7:55:00 PM" : "10:55:00 AM"),
                    new TimeSpan(1, 0, 0, 0)
                )
            );

            #endregion

            #region Kingdom War fighting and cleanup
                
            this.ScheduledEvents.Add
            (
                "KW_BattleStarter",
                new Scheduler.ScheduledEvent
                (
                    "KW_BattleStarter",
                    () =>
                    {
                        if (GameClient.Want_Debug)
                        {
                            Utils.LoggerNotifications("<color=#33aaff><b><u>Kingdom Wars auto-start test:</u></b></color>");
                            Utils.LoggerNotifications("<color=#33aaff>... game service check: " + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "passed" : "failed") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... events automation setting: " + ((Utils.True("Game_Events")) ? "enabled" : "disabled") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... Fri/Sat/Sun check: " + (((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday)) ? "passed" : "failed") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... Kingdom War automation setting: " + ((Utils.False("Game_FightKW")) ? "enabled" : "disabled") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>... Kingdom War already ongoing check: " + ((!GameClient.Current.KW_Ongoing) ? "passed" : "failed") + "</color>");
                            Utils.LoggerNotifications("<color=#33aaff>....... if tests above passed and settings are enabled, then EKU should begin Kingdom War automation now</color>");
                        }

                        // Lies of Astaroth doesn't have Kingdom War
                        if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                        {
                            if (Utils.True("Game_Events"))
                                if ((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday))
                                    if (Utils.False("Game_FightKW"))
                                        if (!GameClient.Current.KW_Ongoing) // KW time scheduled, but not fighting -- missed the beginning signal
                                            GameClient.Current.KingdomWar_WarBegins();
                        }
                    },
                    Utils.GameEvent(GameClient.DateTimeNow, (this.Service == GameService.Magic_Realms) ? "8:01:30 PM" : "11:01:30 AM"), // 90 seconds after KW starts
                    new TimeSpan(1, 0, 0, 0)
                )
            );

                
            this.ScheduledEvents.Add
            (
                "KW_CleanUp",
                new Scheduler.ScheduledEvent
                (
                    "KW_CleanUp",
                    () =>
                    {
                        // Lies of Astaroth doesn't have Kingdom War
                        if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                            if (Utils.True("Game_Events"))
                                if ((GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Friday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Saturday) || (GameClient.DateTimeNow.DayOfWeek == DayOfWeek.Sunday))
                                    if (Utils.False("Game_FightKW"))
                                        Scheduler.ScheduledEvent.AllowAllEvents();
                    },
                    Utils.GameEvent(GameClient.DateTimeNow, (this.Service == GameService.Magic_Realms) ? "8:01:30 PM" : "11:01:30 AM").AddHours(1.0), // 90 seconds after KW ends
                    new TimeSpan(1, 0, 0, 0)
                )
            );

            #endregion
                
            #region maze tower resets
                
            this.ScheduledEvents.Add
            (
                "MazeTower_Resets",
                new Scheduler.ScheduledEvent
                (
                    "MazeTower_Resets",
                    () =>
                    {
                        if (Utils.True("Game_Events"))
                            if (Utils.False("Game_FreeMazeResetDaily"))
                                GameClient.Current.Play_ResetAllTowersFree();
                    },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:01:30 AM"), // 90 seconds after daily reset
                    new TimeSpan(1, 0, 0, 0)
                )
            );

            #endregion

            #region map invasions

            // 12:45am / 8:45am / 4:45 pm
            // every 8h: fight map invasions
            this.ScheduledEvents.Add
            (
                "Map invasions",
                new Scheduler.ScheduledEvent
                (
                    "Map invasions",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_FightMapInvasions")) this.Play_FightMapInvasions(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:45:00 AM"),
                    new TimeSpan(0, 8, 0, 0)
                )
            );

            #endregion
            #region send/receive friend energy

            // 10 minutes past the hour, every hour
            this.ScheduledEvents.Add
            (
                "Send/Receive Energy",
                new Scheduler.ScheduledEvent
                (
                    "Send/Receive Energy",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_SendFriendEnergy")) this.Play_SendFriendListEnergy(); if (Utils.True("Game_Events"))if (Utils.False("Game_ReceiveFriendEnergy")) this.Play_ReceiveFriendListEnergy(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:10:00 AM"),
                    new TimeSpan(0, 1, 0, 0)
                )
            );

            #endregion
            #region fight thieves/thief

            // every 5m: fight thieves
            this.ScheduledEvents.Add
            (
                "Fight Thieves",
                new Scheduler.ScheduledEvent
                (
                    "Fight Thieves",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_FightThieves")) this.Play_FightThieves(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:35:00 AM"),
                    new TimeSpan(0, 0, 5, 0)
                )
            );

            #endregion
            #region use energy

            // every 1h: use energy
            this.ScheduledEvents.Add
            (
                "Use Energy",
                new Scheduler.ScheduledEvent
                (
                    "Use Energy",
                    () => { if (Utils.True("Game_Events")) this.Play_SpendEnergy(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:05:00 AM"),
                    new TimeSpan(0, 1, 0, 0)
                )
            );

            #endregion
            #region arena fights

            // every 15m

            this.ScheduledEvents.Add
            (
                "Arena Fight",
                new Scheduler.ScheduledEvent
                (
                    "Arena Fight",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_FightArena")) this.Play_ArenaFight(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:15:00 AM"),
                    new TimeSpan(0, 0, 15, 0)
                )
            );

            #endregion
            #region world tree

            // every 5m

            this.ScheduledEvents.Add
            (
                "World Tree Fight",
                new Scheduler.ScheduledEvent
                (
                    "World Tree Fight",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_FightWorldTree"))this.Play_WorldTree(false); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:05:00 AM"),
                    new TimeSpan(0, 0, 5, 0)
                )
            );

            #endregion
            #region raiders (hydra)

            int raiderInterval = Utils.CInt(Utils.GetAppSetting("Hydra_Frequency"));
            if (raiderInterval < 1)
                raiderInterval = 60; // every 1m by default

            this.ScheduledEvents.Add
            (
                "Raider Fight",
                new Scheduler.ScheduledEvent
                (
                    "Raider Fight",
                    () => { if (Utils.True("Game_Events")) if (Utils.False("Game_FightHydra")) this.Play_FightRaider_Hydra(); },
                    Utils.GameEvent(GameClient.DateTimeNow, "12:00:00 AM"), // optional
                    new TimeSpan(0, 0, 0, raiderInterval)
                )
            );

            #endregion
        }

        public static string GameAbbreviation(GameService svc)
        {
            if (svc == GameService.Elemental_Kingdoms) return "EK";
            if (svc == GameService.Lies_of_Astaroth) return "LoA";
            if (svc == GameService.Magic_Realms) return "MR";
            if (svc == GameService.Elves_Realm) return "ER";
            if (svc == GameService.Shikoku_Wars) return "SW";

            return "?";
        }

        public void SwitchDecks(string deck_ordinal, string deck_JSON = null)
        {
            deck_JSON = (deck_JSON == null) ? this.GetGameData(ref this.opts, "card", "GetCardGroup", false) : deck_JSON;

            string deck_id = this.GetDeckIDForOrdinal(deck_ordinal, deck_JSON);

            if (Utils.CInt(deck_id) > 0)
            {
                Utils.LoggerNotifications("Switched to deck " + deck_ordinal.ToUpper());
                this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + deck_id, false);
            }

            return;
        }
        
        public string[] SetDeckInfo(string deck_ordinal, List<string> card_id_list, List<string> rune_id_list, string deck_JSON = null)
        {
            deck_JSON = (deck_JSON == null) ? this.GetGameData(ref this.opts, "card", "GetCardGroup", false) : deck_JSON;

            this.UserCards_CachedData = null;
            this.UserRunes_CachedData = null;

            string deck_id = this.GetDeckIDForOrdinal(deck_ordinal, deck_JSON);

            while (card_id_list.Count > 10) card_id_list.RemoveAt(card_id_list.Count - 1);
            while (rune_id_list.Count > 4) rune_id_list.RemoveAt(rune_id_list.Count - 1);

            string all_cards = "";
            foreach (string card in card_id_list)
                all_cards += "_" + card;
            all_cards = all_cards.Trim(new char[] { '_' }).Replace("_", "%5F");

            string all_runes = "";
            foreach (string rune in rune_id_list)
                all_runes += "_" + rune;
            all_runes = all_runes.Trim(new char[] { '_' }).Replace("_", "%5F");

            Utils.Logger("Cards=" + all_cards + "&GroupId=" + deck_id + "&Runes=" + all_runes);

            try
            {
                string error = "unknown error";

                int iMaxUnknownErrorAttempts = 5;

                for (DateTime dtStartLoop = DateTime.Now; (DateTime.Now - dtStartLoop).TotalMinutes < 2; ) // failsafe: wait up to 2 minutes for this to complete successfully
                {
                    JObject result = JObject.Parse(this.GetGameData(ref this.opts, "card", "SetCardGroup", "Cards=" + all_cards + "&GroupId=" + deck_id + "&Runes=" + all_runes, false));

                    if (Utils.CInt(result["status"].ToString()) == 0)
                    {
                        error = result["message"].ToString();

                        if (error.Contains("COST")) // too much cost
                        {
                            Utils.LoggerNotifications("<color=#ff4000>Cards were removed from this deck configuration because the game server said the COST was too high!</color>");
                            card_id_list.RemoveAt(card_id_list.Count - 1); // take out a card

                            all_cards = "";
                            foreach (string card in card_id_list)
                                all_cards += "_" + card;
                            all_cards = all_cards.Trim(new char[] { '_' }).Replace("_", "%5F");

                            continue;
                        }
                        else
                            iMaxUnknownErrorAttempts--;
                    }
                    else
                        return new string[] { all_cards, all_runes };

                    if (iMaxUnknownErrorAttempts <= 0)
                        break;
                }

                return new string[] { error };
            }
            catch { }

            return null;
        }

        public string[] SetDeckInfo_DeckID(string group_id, List<string> card_id_list, List<string> rune_id_list, string deck_JSON = null)
        {
            this.UserCards_CachedData = null;
            this.UserRunes_CachedData = null;

            while (card_id_list.Count > 10) card_id_list.RemoveAt(card_id_list.Count - 1);
            while (rune_id_list.Count > 4) rune_id_list.RemoveAt(rune_id_list.Count - 1);

            string all_cards = "";
            foreach (string card in card_id_list)
                all_cards += "_" + card;
            all_cards = all_cards.Trim(new char[] { '_' }).Replace("_", "%5F");

            string all_runes = "";
            foreach (string rune in rune_id_list)
                all_runes += "_" + rune;
            all_runes = all_runes.Trim(new char[] { '_' }).Replace("_", "%5F");

            try
            {
                string error = "unknown error";

                int iMaxUnknownErrorAttempts = 5;

                for (DateTime dtStartLoop = DateTime.Now; (DateTime.Now - dtStartLoop).TotalMinutes < 2; ) // failsafe: wait up to 2 minutes for this to complete successfully
                {
                    JObject result = JObject.Parse(this.GetGameData(ref this.opts, "card", "SetCardGroup", "Cards=" + all_cards + "&GroupId=" + group_id + "&Runes=" + all_runes, false));

                    if (Utils.CInt(result["status"].ToString()) == 0)
                    {
                        error = result["message"].ToString();

                        if (error.Contains("COST")) // too much cost
                        {
                            card_id_list.RemoveAt(card_id_list.Count - 1); // take out a card

                            all_cards = "";
                            foreach (string card in card_id_list)
                                all_cards += "_" + card;
                            all_cards = all_cards.Trim(new char[] { '_' }).Replace("_", "%5F");

                            continue;
                        }
                        else
                            iMaxUnknownErrorAttempts--;
                    }
                    else
                        return new string[] { all_cards, all_runes };

                    if (iMaxUnknownErrorAttempts <= 0)
                        break;
                }

                return new string[] { error };
            }
            catch { }

            return null;
        }

        public string GetDeckIDForOrdinal(string deck_ordinal, string deck_JSON = null)
        {
            try
            {
                deck_JSON = (deck_JSON == null) ? this.GetGameData(ref this.opts, "card", "GetCardGroup", false) : deck_JSON;

                string[] deck_info = Utils.SubStringsDups(this.GetDeckInfo(deck_ordinal, deck_JSON), "||");

                if (deck_info[0] == "*") return "";

                return deck_info[0];
            }
            catch { }

            return "0";
        }

        public string GetDeckInfo(string deck_ordinal, string deck_JSON = null)
        {
            try
            {
                JObject decks = JObject.Parse((deck_JSON == null) ? this.GetGameData(ref this.opts, "card", "GetCardGroup", false) : deck_JSON);

                if (deck_ordinal.ToUpper() == "KW")
                {
                    foreach (var deck in decks["data"]["Groups"])
                    {
                        try
                        {
                            if (Utils.CInt(decks["data"]["legionWarGroupId"]) == Utils.CInt(deck["GroupId"]))
                                return deck["GroupId"].ToString() + "||" + deck["UserCardIds"].ToString() + "||" + deck["UserRuneIds"].ToString();
                        }
                        catch { }
                    }
                }


                int deck_number = 0;

                foreach (var deck in decks["data"]["Groups"])
                {
                    try
                    {
                        if (Utils.CInt(decks["data"]["legionWarGroupId"]) != Utils.CInt(deck["GroupId"]))
                        {
                            deck_number++;

                            if (Utils.CInt(deck_ordinal) == deck_number)
                                return deck["GroupId"].ToString() + "||" + deck["UserCardIds"].ToString() + "||" + deck["UserRuneIds"].ToString();
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Utils.Logger("<color=#ff0000>" + Errors.GetAllErrorDetails(ex) + "</color>");
            }

            return "*||*||*";
        }

        private static string TranslateRewardID_EK(int id)
        {
            if (id == 1) return "gold";
            if (id == 2) return "gems";
            if (id == 3) return "fire tokens";
            if (id == 4) return "card";
            if (id == 5) return "rune";
            // dragon points ?
            if (id == 7) return "honor";
            if (id == 8) return "card fragment";
            return "unknown";
        }

        private static string TranslateRewardID_LoA(int id)
        {
            if (id == 1) return "gold";
            if (id == 2) return "gems";
            if (id == 3) return "magic tickets";
            if (id == 4) return "card";
            if (id == 5) return "rune";
            // dragon points ?
            if (id == 7) return "honor";
            if (id == 8) return "card fragment";
            return "unknown";
        }

        private object reward_locker = new object();
        private int ClaimChestRewards()
        {
            int total_claimed_rewards = 0;

            try
            {
                lock (reward_locker)
                {
                    string reward_data = this.GetGameData("user", "GetUserSalary");

                    JObject rewards = JObject.Parse(reward_data);

                    foreach (var reward in rewards["data"]["SalaryInfos"])
                    {
                        try
                        {
                            total_claimed_rewards++;

                            Utils.LoggerNotifications("<color=#ffa000>Accepted <b>" + reward["TypeName"].ToString() + "</b>!</color>");
                            Utils.LoggerNotifications("<color=#ffa000>... reward received:  " + Utils.time_val((uint)Utils.CInt(reward["Time"].ToString())).ToLocalTime().ToString() + "</color>");

                            int award_quantity = 1;
                            try
                            {
                                award_quantity = Utils.CInt(reward["AwardNum"]);
                                if (award_quantity < 1)
                                    award_quantity = 1;
                            }
                            catch { }

                            string reward_name = (this.Service != GameService.Lies_of_Astaroth && this.Service != GameClient.GameService.Elves_Realm) ? TranslateRewardID_EK(Utils.CInt(reward["AwardType"])) : TranslateRewardID_LoA(Utils.CInt(reward["AwardType"]));

                            // de-pluralize if the reward is just 1 of something (applies to gold, gems, fire tokens/magic tickets, and honor)
                            if ((Utils.CInt(reward["AwardValue"]) == 1) && (reward_name.EndsWith("s")))
                                reward_name = reward_name.Substring(0, reward_name.Length - 1);

                            switch (reward_name)
                            {
                                case "gold":
                                case "gems":
                                case "fire tokens":
                                case "magic tickets":
                                case "honor":
                                default:
                                    Utils.LoggerNotifications("<color=#ffa000>... reward is: " + Utils.CInt(reward["AwardValue"]).ToString("#,##0") + " " + reward_name + "</color>");
                                    break;

                                case "card":
                                case "rune":
                                    Utils.LoggerNotifications("<color=#ffa000>... reward is: " + this.FriendlyReplacerInbound("[Card #" + reward["AwardValue"].ToString() + "]") + "</color>");
                                    break;

                                case "card fragment":
                                    Utils.LoggerNotifications("<color=#ffa000>... reward is: " + award_quantity.ToString("#,##0") + " " + this.FriendlyReplacerInbound("[Card #" + reward["AwardValue"].ToString() + "]") + " " + Utils.PluralWord(award_quantity, "fragment", "fragments") + "</color>");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Utils.DebugLogger("<color=#ff0000>" + Errors.GetAllErrorDetails(ex) + "</color>");
                        }
                    }

                    this.GetGameData(ref this.opts, "user", "AwardSalary", false);
                }
            }
            catch (Exception ex)
            {
                Utils.DebugLogger("<color=#ff0000>" + Errors.GetAllErrorDetails(ex) + "</color>");
            }

            return total_claimed_rewards;
        }

        public JObject Cards_JSON_Parsed = null;
        public JObject Runes_JSON_Parsed = null;
        public JObject Skills_JSON_Parsed = null;

        public JObject GetSkillByID(int id)
        {
            if (this.Skills_JSON_Parsed == null)
                this.Skills_JSON_Parsed = JObject.Parse(this.All_Skills_JSON);

            foreach (JObject skill in this.Skills_JSON_Parsed["data"]["Skills"])
                if (Utils.CInt(skill["SkillId"]) == id)
                    return skill;

            return null;
        }

        public JObject GetSkillByName(string name)
        {
            if (this.Skills_JSON_Parsed == null)
                this.Skills_JSON_Parsed = JObject.Parse(this.All_Skills_JSON);

            foreach (JObject skill in this.Skills_JSON_Parsed["data"]["Skills"])
                if (skill["Name"].ToString().Trim().ToLower() == name.Trim().ToLower())
                    return skill;

            return null;
        }

        public JObject Skills_English_JSON_Parsed = null;

        public JObject GetSkillByID_English(int id)
        {
            /*
            try
            {
                if (this.Skills_English_JSON_Parsed == null)
                    this.Skills_English_JSON_Parsed = JObject.Parse(System.IO.File.ReadAllText(@"\path\to\english\master skill list.json"));

                foreach (JObject skill in this.Skills_English_JSON_Parsed["data"]["Skills"])
                    if (Utils.CInt(skill["SkillId"].ToString().Trim()) == id)
                        return skill;
            }
            catch { }
            */

            return GetSkillByID(id);
        }

        public JObject Cards_English_JSON_Parsed = null;

        public JObject GetCardByID_English(int id)
        {
            /*
            try
            {
                if (this.Cards_English_JSON_Parsed == null)
                    this.Cards_English_JSON_Parsed = JObject.Parse(System.IO.File.ReadAllText(@"\path\to\english\master card list.json"));

                foreach (JObject card in this.Cards_English_JSON_Parsed["data"]["Cards"])
                    if (Utils.CInt(card["CardId"].ToString().Trim()) == id)
                        return card;
            }
            catch { }
            */

            return GetCardByID(id);
        }

        public string TranslateCardName(string name)
        {
            if (name == "彩蝶妖精") return "Butterfly Fairy";
            if (name == "秘林王子") return "Forest Prince";
            if (name == "战争仲裁者") return "Arbiter of War";
            if (name == "光之九头蛇") return "Vicious Hydra";
            if (name == "冰原驯兽师") return "Arctic Beastmaster";
            if (name == "圣光之刃") return "Light Paladin";
            if (name == "水晶魔女") return "Crystal Witch";
            if (name == "流浪剑客") return "Boreal Ranger";
            if (name == "北国执政官") return "Northern Archon";
            if (name == "杀戮战神") return "God of War";
            if (name == "噬魂冰魔") return "Arctic Ravager";
            if (name == "邪灵皇后") return "Demon Queen";
            if (name == "灵蛇巫女") return "Serpent Shamanka";
            if (name == "兽族统领") return "Orc Veteran";
            if (name == "食人花之祖") return "Ancestral Maneater";
            if (name == "哥布林发明家") return "Goblin Engineer";
            if (name == "火山盾灵") return "Volcanic Defender";
            if (name == "死灵皇后") return "Queen of the Dead";
            if (name == "骷髅元帅") return "Skeletal Battlemage";
            if (name == "烈焰之翼") return "Fiery Angel";
            if (name == "灵能使者") return "Psionic Angel";
            if (name == "放逐之翼") return "Exiled Angel";
            if (name == "熔岩龙龟") return "Lava Dragon Turtle";
            if (name == "火炎女皇") return "Flame Queen";

            if (name == "生命之源") return "Life Source";
            if (name == "魔法核心") return "Magic Core";
            if (name == "邪恶之灵") return "Evil Spirit";
            if (name == "火焰之力") return "Firepower";
            if (name == "古龙之骸") return "Ancient Dragon Skull";
            
            if (name == "远古陨石") return "Ancient Meteorite";
            if (name == "圣诞精灵") return "Christmas Elf";
            if (name == "圣诞魔法") return "Christmas Magic";
            if (name == "圣诞气息") return "Christmas Spirit";
            if (name == "圣诞帽") return "Santa Hat";
            if (name == "圣诞铃铛") return "Jingle Bells";
            if (name == "传说古木") return "Legendary Treant";
            if (name == "定情玫瑰") return "Token of Love";
            if (name == "元宵节灯笼") return "Festive Lantern";

            return name;
        }

        public JObject GetCardByName(string name)
        {
            if (this.Cards_JSON_Parsed == null)
                this.Cards_JSON_Parsed = JObject.Parse(this.All_Cards_JSON);

            if (!Utils.ValidText(name))
                return null;

            foreach (JObject card in this.Cards_JSON_Parsed["data"]["Cards"])
                if (card["CardName"].ToString().Trim().ToLower() == name.Trim().ToLower())
                    return card;

            for (int iTextComparisonClosenessMatchMode = 1; iTextComparisonClosenessMatchMode <= 5; iTextComparisonClosenessMatchMode++)
            {
                foreach (JObject card in this.Cards_JSON_Parsed["data"]["Cards"])
                {
                    string actual_card_name = card["CardName"].ToString();

                    if      (iTextComparisonClosenessMatchMode == 1) { if (TextComparison.IsExactMatch(name, actual_card_name)) return card; }
                    else if (iTextComparisonClosenessMatchMode == 2) { if (TextComparison.IsReallyCloseMatch(name, actual_card_name)) return card; }
                    else if (iTextComparisonClosenessMatchMode == 3) { if (TextComparison.IsCloseMatch(name, actual_card_name)) return card; }
                    else if (iTextComparisonClosenessMatchMode == 4) { if (TextComparison.IsPossibleMatch(name, actual_card_name)) return card; }
                    else if (iTextComparisonClosenessMatchMode == 5) { if (TextComparison.ProbabilityForMatch(name, actual_card_name) >= 40.0) return card; }
                }
            }

            return null;
        }

        private static string TrimSkillName(string skill_name)
        {
            try
            {
                if (Utils.ValidText(skill_name))
                {
                    if (skill_name.ToUpper().StartsWith("QS "))
                        skill_name = "Quick Strike: " + skill_name.Substring(3).Trim();
                    else if (skill_name.ToUpper().StartsWith("Q "))
                        skill_name = "Quick Strike: " + skill_name.Substring(2).Trim();
                    else if (skill_name.ToUpper().StartsWith("D "))
                        skill_name = "Desperation: " + skill_name.Substring(2).Trim();

                    if (skill_name.EndsWith(" 1")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 2")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 3")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 4")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 5")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 6")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 7")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 8")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 9")) return skill_name.Substring(0, skill_name.Length - 2);
                    if (skill_name.EndsWith(" 10")) return skill_name.Substring(0, skill_name.Length - 3);
                }
            }
            catch { }

            return skill_name;
        }

        public System.Collections.Generic.IEnumerable<int> GetSkillsByName(string name)
        {
            if (this.Skills_JSON_Parsed == null)
                this.Skills_JSON_Parsed = JObject.Parse(this.All_Skills_JSON);

            if (!Utils.ValidText(name))
                yield break;

            string trimmed_name = GameClient.TrimSkillName(name);

            foreach (JObject skill in this.Skills_JSON_Parsed["data"]["Skills"])
            {
                if (skill["Name"].ToString().Trim().ToLower() == name.Trim().ToLower())
                {
                    yield return Utils.CInt(skill["SkillId"]);
                    yield break;
                }
            }

            foreach (JObject skill in this.Skills_JSON_Parsed["data"]["Skills"])
                if (GameClient.TrimSkillName(skill["Name"].ToString().Trim().ToLower()) == trimmed_name.Trim().ToLower())
                    yield return Utils.CInt(skill["SkillId"]);
        }

        public JObject GetCardByID(int id)
        {
            while (string.IsNullOrEmpty(this.All_Cards_JSON))
                System.Threading.Thread.Sleep(50);

            if (this.Cards_JSON_Parsed == null)
                this.Cards_JSON_Parsed = JObject.Parse(this.All_Cards_JSON);

            foreach (JObject card in this.Cards_JSON_Parsed["data"]["Cards"])
                if (Utils.CInt(card["CardId"].ToString().Trim()) == id)
                    return card;

            return null;
        }

        public JObject GetRuneByName(string name)
        {
            if (this.Runes_JSON_Parsed == null)
                this.Runes_JSON_Parsed = JObject.Parse(this.All_Runes_JSON);

            if (!Utils.ValidText(name))
                return null;

            foreach (JObject rune in this.Runes_JSON_Parsed["data"]["Runes"])
                if (rune["RuneName"].ToString().Trim().ToLower() == name.Trim().ToLower())
                    return rune;
            
            for (int iTextComparisonClosenessMatchMode = 1; iTextComparisonClosenessMatchMode <= 5; iTextComparisonClosenessMatchMode++)
            {
                foreach (JObject rune in this.Runes_JSON_Parsed["data"]["Runes"])
                {
                    string actual_rune_name = rune["RuneName"].ToString();

                    if      (iTextComparisonClosenessMatchMode == 1) { if (TextComparison.IsExactMatch(name, actual_rune_name)) return rune; }
                    else if (iTextComparisonClosenessMatchMode == 2) { if (TextComparison.IsReallyCloseMatch(name, actual_rune_name)) return rune; }
                    else if (iTextComparisonClosenessMatchMode == 3) { if (TextComparison.IsCloseMatch(name, actual_rune_name)) return rune; }
                    else if (iTextComparisonClosenessMatchMode == 4) { if (TextComparison.IsPossibleMatch(name, actual_rune_name)) return rune; }
                    else if (iTextComparisonClosenessMatchMode == 5) { if (TextComparison.ProbabilityForMatch(name, actual_rune_name) >= 40.0) return rune; }
                }
            }

            return null;
        }

        public JObject GetRuneByID(int id)
        {
            if (this.Runes_JSON_Parsed == null)
                this.Runes_JSON_Parsed = JObject.Parse(this.All_Runes_JSON);

            foreach (JObject rune in this.Runes_JSON_Parsed["data"]["Runes"])
                if (Utils.CInt(rune["RuneId"].ToString().Trim()) == id)
                    return rune;

            return null;
        }

        private DateTime UserCards_CachedTime = DateTime.MinValue;
        public JObject UserCards_CachedData = null;
        public JObject GetUsersCards()
        {
            if ((this.UserCards_CachedData == null) || (UserCards_CachedTime == DateTime.MinValue) || ((DateTime.Now - this.UserCards_CachedTime).TotalMinutes > 7)) // cache for 8 minutes
            {
                this.UserCards_CachedTime = DateTime.Now;
                this.UserCards_CachedData = JObject.Parse(this.GetGameData(ref this.opts, "card", "GetUserCards", false));
            }

            return this.UserCards_CachedData;
        }

        private DateTime UserRunes_CachedTime = DateTime.MinValue;
        public JObject UserRunes_CachedData = null;
        public JObject GetUsersRunes()
        {
            if (this.UserRunes_CachedData == null || ((DateTime.Now - this.UserRunes_CachedTime).TotalMinutes > 7)) // cache for 8 minutes
            {
                this.UserRunes_CachedTime = DateTime.Now;
                this.UserRunes_CachedData = JObject.Parse(this.GetGameData(ref this.opts, "rune", "GetUserRunes", false));
            }

            return this.UserRunes_CachedData;
        }

        public void NewestCardReport()
        {
            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
            Utils.LoggerNotifications("<color=#00efff><fs+><b>New Card Report</b> (the thirty most recent 4★ and 5★ cards and resources)<fs-></color>");
            Utils.LoggerNotifications("<color=#00a0d8>only unleveled cards will appear in this list :: newest cards on top</color>");
            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");

            try
            {
                JObject users_cards = this.GetUsersCards();

                int iCardStop = 0;
                JArray cards = (JArray)users_cards["data"]["Cards"];

                for (int cardIndex = cards.Count - 1; cardIndex >= 0; cardIndex--)
                {
                    JObject this_card = (JObject)cards[cardIndex];
                    JObject generic_card_details = this.GetCardByID(Utils.CInt(this_card["CardId"]));

                    GameObjs.Card card = new GameObjs.Card(this_card);

                    if (card.CurrentXP == 0)
                    {
                        if ((card.Stars >= 4) || (card.Cost == 99) || (card.ElementType == GameObjs.Card.ElementTypes.Activity) || (card.ElementType == GameObjs.Card.ElementTypes.Treasure) || (card.ElementType == GameObjs.Card.ElementTypes.Food))
                        {
                            string card_pretty_details = "";
                            string card_color_start = "<color=#c0c0c0>";
                            string card_color_end = "</color>";

                            if ((card.Cost == 99) || (card.ElementType == GameObjs.Card.ElementTypes.Activity) || (card.ElementType == GameObjs.Card.ElementTypes.Treasure) || (card.ElementType == GameObjs.Card.ElementTypes.Food))
                            {
                                if (card.FoodCard)
                                {
                                    card_pretty_details = card.Stars.ToString() + "★ food worth " + card.EnchantingWorth.ToString("#,##0") + " EXP when enchanting another card";

                                    card_color_start = "<color=#40ff40>";
                                }
                                else if (card.TreasureCard)
                                {
                                    card_pretty_details = card.Stars.ToString() + "★ treasure worth " + Utils.CInt(generic_card_details["Price"]).ToString("#,##0") + " gold when selling";

                                    card_color_start = "<color=#ffa000>";
                                }
                                else
                                {
                                    card_pretty_details = card.Stars.ToString() + "★ activity useful during a special event";

                                    card_color_start = "<color=#ff40d0>";
                                }
                            }
                            else
                            {
                                card_pretty_details = card.Stars.ToString() + "★ " + card.Element + " with a " + card.Wait.ToString() + "-turn wait and " + card.Cost.ToString() + " COST";

                                if (card.Stars >= 5)
                                    card_color_start = "<color=#ffffff>";
                            }

                            if ((card.Cost != 99 && card.Stars >= 5) || (card.Cost == 99 && card.Stars >= 4))
                            {
                                card_color_start = "<i><b><fs+>" + card_color_start;
                                card_color_end = card_color_end + "<fs-></b></i>";
                            }

                            Utils.LoggerNotifications("<color=#00efff>" + card_color_start + card.Name + card_color_end + "  " + card_pretty_details + "</color>");

                            if (++iCardStop >= 30)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LoggerNotifications(Errors.GetAllErrorDetails(ex));
            }

            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
        }

        public void CardSearch(string card_name, JObject users_cards = null)
        {
            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
            Utils.LoggerNotifications("<color=#00efff><b>Card Search:</b>  " + card_name + "</color>");
            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");

            try
            {
                JObject card = this.GetCardByName(card_name);

                if (users_cards == null)
                    users_cards = this.GetUsersCards();

                int CardId = Utils.CInt(card["CardId"].ToString());
                int CardsInDeck = 0;
                int TotalCardsFound = 0;

                Utils.LoggerNotifications("<color=#00efff><b>Card:</b>\t\t" + card["CardName"].ToString() + "  (" + card["Color"].ToString() + "★)</color>");
                Utils.LoggerNotifications("<color=#00efff><b>Element:</b>\t\t" + ConvertCardElementToText(card["Race"]) + "</color>");
                Utils.LoggerNotifications("<color=#00efff><b>Cost:</b>\t\t" + card["Cost"].ToString() + "</color>");
                Utils.LoggerNotifications("<color=#00efff><b>View:</b>\t\t<link><text>L.0</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||0</url></link>  <link><text>L.1</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||1</url></link>  <link><text>L.2</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||2</url></link>  <link><text>L.3</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||3</url></link>  <link><text>L.4</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||4</url></link>  <link><text>L.5</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||5</url></link>  <link><text>L.6</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||6</url></link>  <link><text>L.7</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||7</url></link>  <link><text>L.8</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||8</url></link>  <link><text>L.9</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||9</url></link>  <link><text>L.10</text><url>||EKU||CARD||EKU||" + card["CardId"].ToString() + "||EKU||10</url></link></color>");
                Utils.LoggerNotifications("<color=#00efff><b>More:</b>\t\t</color><link><text>Google Docs Spreadsheet</text><url>||EKU||URL||EKU||https://docs.google.com/spreadsheet/lv?key=0Aus5D4IokB-1dHJFYzVqM2sxWlplM1pRV2tSZmtyeGc</url></link> <color=#00efff>/</color> <link><text>Arcannis' Website</text><url>||EKU||URL||EKU||http://ek.arcannis.com/elemental-kingdoms/card-list/</url></link>");
                Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");

                for (int Level = 15; Level > -1; Level--)
                {
                    if (Level >= 10)
                    {
                        for (int Evolutions = 0; Evolutions <= 100; Evolutions++)
                        {
                            int CardsThisLevel = 0;

                            foreach (var this_card in users_cards["data"]["Cards"])
                            {
                                if (Level == 0)
                                    CardsInDeck++;

                                int evolved_times = Utils.CInt(this_card["Evolution"]);

                                if (evolved_times == Evolutions)
                                {
                                    if (Utils.CInt(this_card["CardId"].ToString()) == CardId)
                                    {
                                        if (Utils.CInt(this_card["Level"].ToString()) == Level)
                                        {
                                            CardsThisLevel++;
                                            TotalCardsFound++;
                                        }
                                    }
                                }
                            }

                            if (CardsThisLevel > 0)
                            {
                                int level_cap = Evolutions + 10;
                                if (level_cap > 15)
                                    level_cap = 15;

                                if (Level == level_cap)
                                {
                                    if (Evolutions == 0)
                                        Utils.LoggerNotifications("<color=#00efff><b>" + CardsThisLevel.ToString() + "</b>x at <b>MAXIMUM</b> level " + level_cap.ToString() + "</color>");
                                    else
                                        Utils.LoggerNotifications("<color=#00efff><b>" + CardsThisLevel.ToString() + "</b>x at <b>MAXIMUM</b> level " + level_cap.ToString() + ", <b>EVOLVED " + Evolutions.ToString() + "x</b></color>");
                                }
                                else
                                {
                                    if (Evolutions == 0)
                                        Utils.LoggerNotifications("<color=#00efff><b>" + CardsThisLevel.ToString() + "</b>x at level <b>" + level_cap.ToString() + "</b></color>");
                                    else
                                        Utils.LoggerNotifications("<color=#00efff><b>" + CardsThisLevel.ToString() + "</b>x at level <b>" + level_cap.ToString() + "</b>, <b>EVOLVED " + Evolutions.ToString() + "x</b></color>");
                                }
                            }
                        }
                    }
                    else
                    {
                        int CardsThisLevel = 0;

                        foreach (var this_card in users_cards["data"]["Cards"])
                        {
                            if (Level == 0)
                                CardsInDeck++;

                            if (Utils.CInt(this_card["CardId"].ToString()) == CardId)
                            {
                                if (Utils.CInt(this_card["Level"].ToString()) == Level)
                                {
                                    CardsThisLevel++;
                                    TotalCardsFound++;
                                }
                            }
                        }

                        if (CardsThisLevel > 0)
                        {
                            if (Level == 0)
                                Utils.LoggerNotifications("<color=#00efff><b>" + CardsThisLevel.ToString() + "</b>x unleveled</b></color>");
                            else
                                Utils.LoggerNotifications("<color=#00efff><b>" + CardsThisLevel.ToString() + "</b>x at level <b>" + Level.ToString() + "</b></color>");
                        }
                    }
                }

                if (TotalCardsFound == 0)
                    Utils.LoggerNotifications("<color=#00efff>No <b>" + card["CardName"].ToString() + "</b> in your collection.</color>");
            }
            catch
            {
                Utils.LoggerNotifications("<color=#00efff>There doesn't seem to be any card by that name.</color>");
            }

            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
        }

        public void RuneSearch(string rune_name, JObject users_runes = null)
        {
            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
            Utils.LoggerNotifications("<color=#00efff><b>Rune Search:</b>  " + rune_name + "</color>");
            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");

            try
            {
                JObject rune = this.GetRuneByName(rune_name);

                if (users_runes == null)
                    users_runes = this.GetUsersRunes();

                int RuneId = Utils.CInt(rune["RuneId"].ToString());
                int RunesInDeck = 0;
                int TotalRunesFound = 0;

                Utils.LoggerNotifications("<color=#00efff><b>Rune:</b>\t\t" + rune["RuneName"].ToString() + "  (" + rune["Color"].ToString() + "★)</color>");
                Utils.LoggerNotifications("<color=#00efff><b>Element:</b>\t\t" + rune["Property"].ToString().Replace("1", "Earth").Replace("2", "Water").Replace("3", "Air").Replace("4", "Fire") + "</color>");
                Utils.LoggerNotifications("<color=#00efff><b>View:</b>\t\t<link><text>L.0</text><url>||EKU||RUNE||EKU||" + rune["RuneId"].ToString() + "||EKU||0</url></link>  <link><text>L.1</text><url>||EKU||RUNE||EKU||" + rune["RuneId"].ToString() + "||EKU||1</url></link>  <link><text>L.2</text><url>||EKU||RUNE||EKU||" + rune["RuneId"].ToString() + "||EKU||2</url></link>  <link><text>L.3</text><url>||EKU||RUNE||EKU||" + rune["RuneId"].ToString() + "||EKU||3</url></link>  <link><text>L.4</text><url>||EKU||RUNE||EKU||" + rune["RuneId"].ToString() + "||EKU||4</url></link></color>");
                Utils.LoggerNotifications("<color=#00efff><b>More:</b>\t\t</color><link><text>Arcannis' Website</text><url>||EKU||URL||EKU||http://ek.arcannis.com/elemental-kingdoms/runes/</url></link>");
                Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");

                for (int Level = 4; Level > -1; Level--)
                {
                    int RunesThisLevel = 0;

                    foreach (var this_rune in users_runes["data"]["Runes"])
                    {
                        if (Level == 0)
                            RunesInDeck++;

                        if (Utils.CInt(this_rune["RuneId"].ToString()) == RuneId)
                        {
                            if (Utils.CInt(this_rune["Level"].ToString()) == Level)
                            {
                                RunesThisLevel++;
                                TotalRunesFound++;
                            }
                        }
                    }

                    if (RunesThisLevel > 0)
                    {
                        if (Level == 0)
                            Utils.LoggerNotifications("<color=#00efff><b>" + RunesThisLevel.ToString() + "</b>x unleveled</b></color>");
                        else if (Level == 4)
                            Utils.LoggerNotifications("<color=#00efff><b>" + RunesThisLevel.ToString() + "</b>x at <b>MAXIMUM level (" + Level.ToString() + ")</b></color>");
                        else
                            Utils.LoggerNotifications("<color=#00efff><b>" + RunesThisLevel.ToString() + "</b>x at level <b>" + Level.ToString() + "</b></color>");
                    }
                }

                if (TotalRunesFound == 0)
                    Utils.LoggerNotifications("<color=#00efff>No <b>" + rune["RuneName"].ToString() + "</b> in your collection.</color>");
            }
            catch
            {
                Utils.LoggerNotifications("<color=#00efff>There doesn't seem to be any rune by that name.</color>");
            }

            Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
        }

        private static string DropLevelFromName(string card_or_rune_name)
        {
            try
            {
                if (Utils.ValidText(card_or_rune_name))
                {
                    card_or_rune_name = card_or_rune_name.Trim();

                    if (!card_or_rune_name.ToUpper().Replace('.', ' ').StartsWith("LEVEL ") && !card_or_rune_name.ToUpper().Replace('.', ' ').StartsWith("L "))
                        return card_or_rune_name;

                    if (card_or_rune_name.ToUpper().StartsWith("L.1 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.2 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.3 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.4 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.5 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.6 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.7 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.8 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.9 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L.10 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L.11 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L.12 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L.13 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L.14 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L.15 ")) card_or_rune_name = card_or_rune_name.Substring(5);

                    if (card_or_rune_name.ToUpper().StartsWith("L 1 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 2 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 3 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 4 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 5 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 6 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 7 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 8 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 9 ")) card_or_rune_name = card_or_rune_name.Substring(4);
                    if (card_or_rune_name.ToUpper().StartsWith("L 10 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L 11 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L 12 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L 13 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L 14 ")) card_or_rune_name = card_or_rune_name.Substring(5);
                    if (card_or_rune_name.ToUpper().StartsWith("L 15 ")) card_or_rune_name = card_or_rune_name.Substring(5);

                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.1 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.2 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.3 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.4 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.5 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.6 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.7 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.8 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.9 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.10 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.11 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.12 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.13 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.14 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL.15 ")) card_or_rune_name = card_or_rune_name.Substring(9);

                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 1 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 2 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 3 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 4 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 5 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 6 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 7 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 8 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 9 ")) card_or_rune_name = card_or_rune_name.Substring(8);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 10 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 11 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 12 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 13 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 14 ")) card_or_rune_name = card_or_rune_name.Substring(9);
                    if (card_or_rune_name.ToUpper().StartsWith("LEVEL 15 ")) card_or_rune_name = card_or_rune_name.Substring(9);

                    card_or_rune_name = card_or_rune_name.Trim();
                }
            }
            catch { }

            return card_or_rune_name;
        }

        private int FindBestUserCardByName_IndexID(string name, JObject users_cards = null)
        {
            try
            {
                if (users_cards == null)
                    users_cards = this.GetUsersCards();

                string card_required_name = name;
                int card_required_level = -1;
                string card_required_evolved_skill_name = "";
                List<int> card_required_evolved_skill_IDs = null;

                if (name.Contains(":"))
                {
                    try
                    {
                        string[] card_search_parts = Utils.SubStringsDups(name, ":");

                        card_required_name = card_search_parts[0].Trim();
                        
                        if (card_search_parts.Length == 2)
                        {
                            if (Utils.ValidNumber(card_search_parts[1]))
                            {
                                // name and level
                                card_required_level = Utils.CInt(card_search_parts[1]);
                            }
                            else
                            {
                                // name and skill
                                card_required_evolved_skill_name = card_search_parts[1].Trim();
                            }
                        }
                        else if (card_search_parts.Length >= 3)
                        {
                            if (Utils.ValidNumber(card_search_parts[1]))
                            {
                                // name, level, and skill
                                card_required_level = Utils.CInt(card_search_parts[1]);
                                card_required_evolved_skill_name = card_search_parts[2].Trim();
                            }
                            else
                            {
                                // name and skill
                                card_required_evolved_skill_name = card_search_parts[1].Trim();

                                if (Utils.ValidNumber(card_search_parts[2]))
                                {
                                    // name, skill, and level
                                    card_required_level = Utils.CInt(card_search_parts[2]);
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Utils.DebugLogger("Couldn't parse: " + name);
                        Utils.DebugLogger(Errors.GetShortErrorDetails(ex));
                    }

                    if (Utils.ValidText(card_required_evolved_skill_name))
                    {
                        if ((card_required_evolved_skill_name.ToLower() == "none") || (card_required_evolved_skill_name.ToLower() == "n/a"))
                            card_required_evolved_skill_IDs = new List<int>(new int[] { 0 });
                        else
                            card_required_evolved_skill_IDs = new List<int>(this.GetSkillsByName(card_required_evolved_skill_name));
                    }
                }

                card_required_name = GameClient.DropLevelFromName(card_required_name);

                /*
                if (evolved_skill_IDs != null)
                {
                    Utils.LoggerNotifications("Skills matching \"" + evolved_skill_name + "\":");
                    foreach (int skill_id in evolved_skill_IDs)
                    {
                        JObject skill = this.GetSkillByID(skill_id);
                        Utils.LoggerNotifications("... " + skill["Name"].ToString());
                    }
                }
                */

                if (card_required_level == -1)
                {
                    if (!Utils.ValidText(card_required_evolved_skill_name))
                        Utils.DebugLogger("FILLING DECK: looking for card <b>" + card_required_name + "</b> at <b>best</b> level");
                    else
                        Utils.DebugLogger("FILLING DECK: looking for card <b>" + card_required_name + "</b> at <b>best</b> level with evolved skill <b>" + card_required_evolved_skill_name + "</b>");
                }
                else
                {
                    if (!Utils.ValidText(card_required_evolved_skill_name))
                        Utils.DebugLogger("FILLING DECK: looking for card <b>" + card_required_name + "</b> only at level <b>" + card_required_level.ToString() + "</b>");
                    else
                        Utils.DebugLogger("FILLING DECK: looking for card <b>" + card_required_name + "</b> only at level <b>" + card_required_level.ToString() + "</b> with evolved skill <b>" + card_required_evolved_skill_name + "</b>");
                }

                int card_required_GID = Utils.CInt(this.GetCardByName(card_required_name)["CardId"]);

                for (int Level = 15; Level > -1; Level--)
                {
                    int CurrentCardIndexID = -1;

                    if (card_required_level != -1)
                        if (Level != card_required_level)
                            continue;

                    foreach (var card in users_cards["data"]["Cards"])
                    {
                        CurrentCardIndexID++;

                        try
                        {
                            if (Utils.CInt(card["CardId"]) == card_required_GID)
                            {
                                if (Utils.CInt(card["Level"]) == Level)
                                {
                                    int iEvolvedSkillID = 0; try { if (Level >= 10) iEvolvedSkillID = Utils.CInt(card["SkillNew"]); } catch { }

                                    if ((card_required_evolved_skill_IDs == null) || (card_required_evolved_skill_IDs.Contains(iEvolvedSkillID)))
                                    {
                                        GameObjs.Card this_card = new GameObjs.Card(card);

                                        if (this_card.EvolvedSkillID == 0)
                                            Utils.DebugLogger("FILLING DECK: found card <b>" + this_card.Name + "</b> at level <b>" + this_card.Level.ToString() + "</b>");
                                        else
                                            Utils.DebugLogger("FILLING DECK: found card <b>" + this_card.Name + "</b> at level <b>" + this_card.Level.ToString() + "</b> with evolved skill <b>" + this_card.EvolvedSkill + "</b>");

                                        return CurrentCardIndexID;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }

                return -1;
            }
            catch { }

            return -1;
        }

        private int FindBestUserRuneByName_IndexID(string name, JObject users_runes = null)
        {
            if (!Utils.ValidText(name))
                return -1;

            string required_rune_name = name.Trim();
            int rune_required_level = -1;

            if (required_rune_name.Contains(":"))
            {
                try
                {
                    string[] rune_search_parts = Utils.SubStringsDups(required_rune_name, ":");

                    required_rune_name = rune_search_parts[0].Trim();
                        
                    if (rune_search_parts.Length >= 2)
                    {
                        if (Utils.ValidNumber(rune_search_parts[1]))
                        {
                            // name and level
                            rune_required_level = Utils.CInt(rune_search_parts[1]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.DebugLogger("Couldn't parse: " + name);
                    Utils.DebugLogger(Errors.GetShortErrorDetails(ex));
                }
            }


            required_rune_name = GameClient.DropLevelFromName(required_rune_name);

            if (rune_required_level == -1)
                Utils.DebugLogger("FILLING DECK: looking for rune <b>" + required_rune_name + "</b> at <b>best</b> level");
            else
                Utils.DebugLogger("FILLING DECK: looking for rune <b>" + required_rune_name + "</b> only at level <b>" + rune_required_level.ToString() + "</b>");

            try
            {
                if (users_runes == null)
                    users_runes = this.GetUsersRunes();

                int rune_required_GID = Utils.CInt(this.GetRuneByName(required_rune_name)["RuneId"]);
                int MatchingRuneIndex = -1;

                for (int Level = 10; Level > -1; Level--)
                {
                    int RunesInDeck = 0;

                    if (rune_required_level != -1)
                        if (Level != rune_required_level)
                            continue;

                    if (MatchingRuneIndex == -1)
                    {
                        foreach (var rune in users_runes["data"]["Runes"])
                        {
                            if (Utils.CInt(rune["RuneId"]) == rune_required_GID)
                            {
                                if (Utils.CInt(rune["Level"]) == Level)
                                {
                                    GameObjs.Rune this_rune = new GameObjs.Rune(rune);

                                    Utils.DebugLogger("FILLING DECK: found rune <b>" + this_rune.Name + "</b> at level <b>" + this_rune.Level.ToString() + "</b>");

                                    MatchingRuneIndex = RunesInDeck;
                                    break;
                                }
                            }

                            RunesInDeck++;
                        }
                    }
                }

                return MatchingRuneIndex;
            }
            catch { }

            return -1;
        }

        public List<string> BuildDeckCards(List<string> cards)
        {
            JObject users_cards = this.GetUsersCards();

            Utils.Logger();

            List<string> new_deck_ids = new List<string>();

            this.UserCards_CachedData = null;
            this.UserRunes_CachedData = null;

            foreach (string card_name in cards)
            {
                int card_index = this.FindBestUserCardByName_IndexID(card_name, users_cards);
                if (card_index != -1)
                {
                    new_deck_ids.Add(users_cards["data"]["Cards"][card_index]["UserCardId"].ToString());
                    users_cards["data"]["Cards"][card_index].Remove();
                }
            }

            this.UserCards_CachedData = null;
            this.UserRunes_CachedData = null;

            return new_deck_ids;
        }

        public List<string> BuildDeckRunes(List<string> runes)
        {
            JObject users_runes = this.GetUsersRunes();

            Utils.Logger();

            List<string> new_deck_ids = new List<string>();

            foreach (string rune_name in runes)
            {
                int rune_index = this.FindBestUserRuneByName_IndexID(rune_name, users_runes);
                if (rune_index != -1)
                {
                    Utils.Logger("Best " + GetRuneByName(rune_name)["RuneName"].ToString() + ":  #" + users_runes["data"]["Runes"][rune_index]["UserRuneId"].ToString() + ", level " + users_runes["data"]["Runes"][rune_index]["Level"].ToString());
                    new_deck_ids.Add(users_runes["data"]["Runes"][rune_index]["UserRuneId"].ToString());
                    users_runes["data"]["Runes"][rune_index].Remove();
                }
            }

            Utils.Logger();

            foreach (string rune_id in new_deck_ids)
                Utils.Logger(rune_id);

            users_runes = this.GetUsersRunes();

            return new_deck_ids;
        }

        public string Play_PickMapToExplore()
        {
            try
            {
                JObject all_maps = JObject.Parse(this.GetGameData(ref this.opts, "mapstage", "GetMapStageALL", false));
                JObject user_map_completion = JObject.Parse(this.GetGameData(ref this.opts, "mapstage", "GetUserMapStages", false));

                Dictionary<string, long> Levels = new Dictionary<string, long>();

                foreach (var map_stage in all_maps["data"])
                {
                    foreach (var map_level in map_stage["MapStageDetails"])
                    {
                        string friendly_map = "?";

                        try
                        {
                            friendly_map = Utils.SubStringsDups(map_level["Dialogue"][0]["Did"].ToString(), "_")[0] + "-" + Utils.SubStringsDups(map_level["Dialogue"][0]["Did"].ToString(), "_")[1];
                        }
                        catch { }

                        if (map_level["Type"].ToString() == "0") // Hidden three-stage map location
                        {
                            continue;
                        }
                        else if (map_level["Type"].ToString() == "1") // Normal three-stage map location
                        {
                            foreach (var map_level_details in map_level["Levels"])
                            {
                                try
                                {
                                    if (user_map_completion["data"][map_level["MapStageDetailId"].ToString()]["FinishedStage"].ToString() == map_level_details["Level"].ToString())
                                    {
                                        long EXP_value = (long)Utils.CInt(Utils.Chopper(Utils.Chopper(map_level_details["BonusExplore"].ToString(), "Exp_", ""), "", ","));
                                        long Gold_value = (long)Utils.CInt(Utils.Chopper(Utils.Chopper(map_level_details["BonusExplore"].ToString(), "Coins_", ""), "", ","));
                                        Levels.Add(map_level["MapStageDetailId"].ToString() + "_" + map_level_details["Level"].ToString() + "_" + map_level["Dialogue"][0]["Did"].ToString(), (EXP_value * 10000000L) + Gold_value);
                                    }
                                }
                                catch { }
                            }
                        }
                        else if (map_level["Type"].ToString() == "2") // Boss three-stage map location
                        {
                            foreach (var map_level_details in map_level["Levels"])
                            {
                                try
                                {
                                    if (user_map_completion["data"][map_level["MapStageDetailId"].ToString()]["FinishedStage"].ToString() == map_level_details["Level"].ToString())
                                    {
                                        long EXP_value = (long)Utils.CInt(Utils.Chopper(Utils.Chopper(map_level_details["BonusExplore"].ToString(), "Exp_", ""), "", ","));
                                        long Gold_value = (long)Utils.CInt(Utils.Chopper(Utils.Chopper(map_level_details["BonusExplore"].ToString(), "Coins_", ""), "", ","));
                                        Levels.Add(map_level["MapStageDetailId"].ToString() + "_" + map_level_details["Level"].ToString() + "_" + map_level["Dialogue"][0]["Did"].ToString(), (EXP_value * 10000000L) + Gold_value);
                                    }
                                }
                                catch { }
                            }
                        }
                        else if (map_level["Type"].ToString() == "3") // Maze tower map location
                        {
                        }
                        else if (map_level["Type"].ToString() == "4") // Unreleased map
                        {
                        }
                        else
                        {
                        }
                    }
                }

                var sortedDict = from entry in Levels orderby entry.Value descending select entry;

                string map_id = "0";
                string map_friendly = "0-0";
                string exp_value = "0";

                foreach (var level in sortedDict)
                {
                    map_id = Utils.SubStrings(level.Key, "_")[0];
                    map_friendly = Utils.SubStrings(level.Key, "_")[2] + "-" + Utils.SubStrings(level.Key, "_")[3];
                    exp_value = (level.Value / 10000000L).ToString("0");
                    break;
                }

                Utils.Logger();
                Utils.Logger("Explore map " + map_friendly + " (#" + map_id + ") for " + Utils.CInt(exp_value).ToString("#,##0") + " EXP");
                Utils.Logger();

                if ((Utils.CInt(map_id) > 0) && (Utils.CInt(exp_value) > 0))
                    return Utils.CInt(map_id).ToString();
            }
            catch (Exception ex)
            {
                Utils.Chatter(Errors.GetAllErrorDetails(ex));
            }

            return "0";
        }

        public void Play_SendFriendListEnergy()
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                Utils.LoggerNotifications("<color=#a07000>Sending energy to friends...</color>");

                string friend_data = JSBeautifyLib.JSBeautify.BeautifyMe(this.GetGameData(ref this.opts, "friend", "GetFriends", false));

                foreach (string friend in Utils.SubStrings(Utils.ChopperBlank(friend_data, "\"Friends\": [{", "}]"), "},"))
                {
                    string friend_id = Utils.ChopperBlank(friend, "\"Uid\": \"", "\"");
                    string friend_name = Utils.ChopperBlank(friend, "\"NickName\": \"", "\"");

                    if (Utils.CInt(friend_id) == 0)
                        continue;

                    bool can_send_energy = friend.Contains("\"FEnergySend\": 1");

                    if (can_send_energy)
                    {
                        this.GetGameData(ref this.opts, "fenergy", "SendFEnergy", "Fid=" + friend_id.ToString(), false);
                        Utils.Logger("Sent energy to <b>" + friend_name + "</b>");

                        Utils.LoggerNotifications("<color=#a07000>... sent energy to <b>" + friend_name + "</b></color>");
                    }
                }
            }
        }

        public void Play_ReceiveFriendListEnergy()
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                Utils.LoggerNotifications("<color=#a07000>Receiving energy from friends...</color>");

                string friend_data = JSBeautifyLib.JSBeautify.BeautifyMe(this.GetGameData(ref this.opts, "friend", "GetFriends", false));
                bool stop_energy = false;

                foreach (string friend in Utils.SubStrings(Utils.ChopperBlank(friend_data, "\"Friends\": [{", "}]"), "},"))
                {
                    string friend_id = Utils.ChopperBlank(friend, "\"Uid\": \"", "\"");
                    string friend_name = Utils.ChopperBlank(friend, "\"NickName\": \"", "\"");

                    if (Utils.CInt(friend_id) == 0)
                        continue;

                    if (!stop_energy)
                    {
                        bool can_receive_energy = friend.Contains("\"FEnergySurplus\": 1");

                        if (can_receive_energy)
                        {
                            string result = this.GetGameData(ref this.opts, "fenergy", "GetFEnergy", "Fid=" + friend_id.ToString(), false);

                            if (result.Contains("\"message\":"))
                            {
                                stop_energy = true;
                                Utils.Logger("Couldn't receive energy from <b>" + friend_name + "</b>, server said: " + Utils.ChopperBlank(result, "\"message\":\"", "\""));
                                Utils.LoggerNotifications("<color=#a07000>... couldn't receive energy from <b>" + friend_name + "</b>: " + Utils.ChopperBlank(result, "\"message\":\"", "\"") + "</color>");
                            }
                            else
                            {
                                Utils.Logger("Received energy from <b>" + friend_name + "</b>");
                                Utils.LoggerNotifications("<color=#a07000>... received energy from <b>" + friend_name + "</b></color>");
                            }
                        }
                    }
                }
            }
        }

        public void Play_ResetAllTowersFree()
        {
            Utils.LoggerNotifications("<color=#ffa000>Resetting maze towers (only using free resets)...</color>");

            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                for (int map_stage = 2; map_stage <= 15; map_stage++)
                {
                    string maze_tower_basic_data = this.GetGameData("maze", "Show", "MapStageId=" + map_stage.ToString());

                    bool free_reset = maze_tower_basic_data.Replace(" ", string.Empty).Replace("\"", string.Empty).Contains("FreeReset:1");

                    if (free_reset)
                    {
                        Utils.LoggerNotifications("<color=#ffa000>... map stage " + map_stage.ToString() + " maze towers reset!</color>");
                        this.GetGameData("maze", "Reset", "MapStageId=" + map_stage.ToString());
                    }
                    else
                        Utils.LoggerNotifications("<color=#ffa000>... map stage " + map_stage.ToString() + " maze towers not reset (no free resets available)</color>");
                }
            }
        }

        public void Play_SpendEnergy(int override_mode = 0)
        {
            Utils.LoggerNotifications("<color=#a07000>Spending energy...</color>");

            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                if (Utils.CInt(this.DefaultDeck) > 0)
                {
                    this.GetGameData(ref this.opts, "card", "SetDefalutGroup", "GroupId=" + this.DefaultDeck, false); // switch to primary deck
                    Utils.LoggerNotifications("<color=#a07000>Switched to default deck to spend energy</color>");
                }
            }

            lock (this.locker_gamedata)
            {
                if ((override_mode != 3) && (override_mode == 1 || override_mode == 2 || Utils.False("Game_FightMazeTowers")))
                {
                    Utils.LoggerNotifications("<color=#a07000>Looking for maze towers to complete (or reset for free)...</color>");

                    try
                    {
                        string towers_to_fight = Utils.GetAppSetting("Game_MazeTowers").Trim().Replace(" ", string.Empty);

                        if (!Utils.ValidText(towers_to_fight))
                            towers_to_fight = "8, 7, 6";

                        foreach (string tower_to_fight in Utils.SubStrings(towers_to_fight, ","))
                        {
                            try
                            {
                                if (Utils.CInt(tower_to_fight) > 0)
                                {
                                    MazeStatus mzStatus = this.Play_DoTowerMaze(ref this.opts, Utils.CInt(tower_to_fight));
                                    if (mzStatus != MazeStatus.Completed && mzStatus != MazeStatus.Skipped_No_Reset_Available) return;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            if (override_mode != 2)
                if (!Utils.False("Game_Explore"))
                    return;

            Utils.Logger();
            Utils.Logger("<b>Now exploring...</b>");
            Utils.Logger();

            this.Play_Explore();
        }

        private enum MazeStatus
        {
            Completed,
            Skipped_No_Reset_Available,
            Out_Of_Energy,
            Error,
            Unknown
        }

        private MazeStatus Play_DoTowerMaze(ref Comm.CommFetchOptions opts, int map_stage)
        {
            Utils.LoggerNotifications("<color=#a07000>Running through maze tower " + map_stage.ToString() + "...</color>");

            string maze_tower_basic_data = this.GetGameData(ref this.opts, "maze", "Show", "MapStageId=" + map_stage.ToString(), false);
            JObject maze_tower_basic_data_JSON = JObject.Parse(maze_tower_basic_data);

            bool free_reset = maze_tower_basic_data.Replace(" ", string.Empty).Replace("\"", string.Empty).Contains("FreeReset:1");

            bool tower_is_cleared = true;
            for (int layer_in_mazetower_check = 1; layer_in_mazetower_check <= 10; layer_in_mazetower_check++)
            {
                string current_layer = this.GetGameData(ref this.opts, "maze", "Info", "Layer=" + layer_in_mazetower_check.ToString() + "&MapStageId=" + map_stage.ToString());
                JObject current_layer_JSON = JObject.Parse(current_layer);

                if (Utils.CInt(current_layer_JSON["status"]) > 0)
                {
                    try
                    {
                        if (!current_layer.Replace("\"", "").Replace(" ", "").Contains("IsFinish:true")) tower_is_cleared = false;
                        if ((Utils.True("Game_MazeTowerChests")) && (Utils.CInt(current_layer_JSON["data"]["RemainBoxNum"]) > 0)) tower_is_cleared = false;
                        if ((Utils.True("Game_MazeTowerMonsters")) && (Utils.CInt(current_layer_JSON["data"]["RemainMonsterNum"]) > 0)) tower_is_cleared = false;
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                    break;

                if (!tower_is_cleared)
                    break;
            }

            if (tower_is_cleared)
            {
                if (free_reset)
                    this.GetGameData(ref this.opts, "maze", "Reset", "MapStageId=" + map_stage.ToString(), false);
                else
                {
                    Utils.Logger("<b><color=#ff0000>THIS TOWER MAZE HAS BEEN CLEARED AND NO FREE RESETS ARE AVAILABLE FOR IT!</color></b>");
                    return MazeStatus.Skipped_No_Reset_Available;
                }
            }
            else
                Utils.Logger("<b>TOWER MAZE HAS BEEN STARTED IN A PREVIOUS SESSION -- RESUMING!</b>");

            int max_floor = 100;
            for (int layer_in_mazetower = 1; layer_in_mazetower <= max_floor; layer_in_mazetower++)
            {
                string current_layer_JSON = this.GetGameData(ref this.opts, "maze", "Info", "Layer=" + layer_in_mazetower.ToString() + "&MapStageId=" + map_stage.ToString());
                JObject current_layer_data = JObject.Parse(current_layer_JSON);

                if (Utils.CInt(current_layer_data["status"]) == 0)
                    break;

                max_floor = Utils.CInt(current_layer_data["data"]["TotalLayer"]);

                string[] current_layer_items = Utils.SubStringsDups(Utils.ChopperBlank(current_layer_JSON.Replace(" ", ""), "\"Items\":[", "]"), ",");

                for (int current_item_index = 0; current_item_index < current_layer_items.Length; current_item_index++)
                {
                    int current_item_type = Utils.CInt(current_layer_items[current_item_index]);

                    if ((current_item_type == 2) || (current_item_type == 3))
                    {
                        Utils.Logger("Layer #" + layer_in_mazetower.ToString() + ", slot #" + (current_item_index + 1).ToString() + ", type " + MapType(current_item_type));

                        bool want_to_fight_this_type = true;
                        if ((current_item_type == 2) && (!Utils.True("Game_MazeTowerChests"))) want_to_fight_this_type = false;
                        if ((current_item_type == 3) && (!Utils.True("Game_MazeTowerMonsters"))) want_to_fight_this_type = false;

                        if (want_to_fight_this_type)
                        {
                            string fight_info = this.GetGameData(ref this.opts, "maze", "Battle", "Layer=" + layer_in_mazetower.ToString() + "&ItemIndex=" + current_item_index.ToString() + "&MapStageId=" + map_stage.ToString() + "&manual=1", false);
                            JObject fight_JSON = JObject.Parse(fight_info);

                            string battle_id = "";
                            if (fight_info.Contains("\"BattleId\""))
                                battle_id = fight_JSON["data"]["BattleId"].ToString();

                            if (!Utils.ValidText(battle_id))
                            {
                                if (fight_info.Contains("\"message\":"))
                                {
                                    if (fight_info.ToLower().Contains("not enough energy"))
                                    {
                                        Utils.Logger("<b>RAN OUT OF ENERGY -- TRY AGAIN LATER!</b>");
                                        return MazeStatus.Out_Of_Energy;
                                    }

                                    Utils.LoggerNotifications("<color=#aa0000><b>Problem picking a maze tower fight: " + fight_JSON["message"].ToString() + "</b></color>");
                                    Utils.LoggerNotifications("<color=#aa0000><b>In map " + map_stage.ToString() + ", floor " + layer_in_mazetower.ToString() + ", position " + (current_item_index + 1).ToString() + " (" + MapType(current_item_type) + ")</b></color>");
                                    continue;
                                }

                                Utils.LoggerNotifications("<color=#aa0000><b>Problem picking a maze tower fight: odd server response</b>");
                                Utils.LoggerNotifications("<color=#aa0000><b>" + fight_info + "</b>");
                                Utils.LoggerNotifications("<color=#aa0000><b>In map " + map_stage.ToString() + ", floor " + layer_in_mazetower.ToString() + ", position " + (current_item_index + 1).ToString() + " (" + MapType(current_item_type) + ")</b></color>");
                                //return MazeStatus.Error;
                                continue;
                            }

                            string battle_result_original = this.GetGameData(ref this.opts, "maze", "ManualBattle", "battleid=" + battle_id + "&stage=&manual=0", false, "user_mapfight" + map_stage.ToString() + "_level" + layer_in_mazetower.ToString() + "_" + MapType(current_item_type) + "_at" + current_item_index.ToString());
                            string battle_result = Utils.ChopperBlank(battle_result_original, "\"Award\":", "}") + ",";

                            string reward_gold = Utils.ChopperBlank(battle_result, "\"Coins\":", ",").Replace("\"", "").Trim();
                            string reward_EXP = Utils.ChopperBlank(battle_result, "\"Exp\":", ",").Replace("\"", "").Trim();
                            string reward_card = Utils.ChopperBlank(battle_result, "\"CardId\":", ",").Replace("\"", "").Trim();

                            Utils.Logger();
                            if (Utils.CInt(reward_gold) > 0) Utils.Logger("<b>Gold earned from maze tower fight:</b> " + Utils.CInt(reward_gold).ToString("#,##0"));
                            if (Utils.CInt(reward_EXP) > 0) Utils.Logger("<b>Experience earned from maze tower fight:</b> " + Utils.CInt(reward_EXP).ToString("#,##0"));
                            if (Utils.CInt(reward_card) > 0) Utils.Logger("<b>Card earned from maze tower fight:</b> " + this.FriendlyReplacerInbound("[Card #" + reward_card + "]"));
                            Utils.Logger();

                            Utils.LoggerNotifications("<color=#ffa000>Maze tower " + MapType(current_item_type) + " fight on map " + map_stage.ToString() + ", floor " + layer_in_mazetower.ToString() + ": " + new GameReward(battle_result_original).AllAwards + "</color>");
                            
                            if (battle_result_original.Replace(" ", "").Replace("\"", "").Contains("{IsClear:1,"))
                            {
                                string maze_tower_completed_result = Utils.ChopperBlank(battle_result_original, "{\"IsClear\"", "}") + ",";

                                string completion_reward_gold = Utils.ChopperBlank(maze_tower_completed_result, "\"Coins\":", ",").Replace("\"", "").Trim();
                                string completion_reward_EXP = Utils.ChopperBlank(maze_tower_completed_result, "\"Exp\":", ",").Replace("\"", "").Trim();
                                string completion_reward_card = Utils.ChopperBlank(maze_tower_completed_result, "\"CardId\":", ",").Replace("\"", "").Trim();

                                if (Utils.CInt(completion_reward_gold) > 0) Utils.Logger("<b>Gold earned from maze tower completion:</b> " + Utils.CInt(completion_reward_gold).ToString("#,##0"));
                                if (Utils.CInt(completion_reward_EXP) > 0) Utils.Logger("<b>Experience earned from maze tower completion:</b> " + Utils.CInt(completion_reward_EXP).ToString("#,##0"));
                                if (Utils.CInt(completion_reward_card) > 0) Utils.Logger("<b>Card earned from maze tower completion:</b> " + this.FriendlyReplacerInbound("[Card #" + completion_reward_card + "]"));
                                Utils.Logger();

                                string output_rewards = "";
                                if (Utils.CInt(completion_reward_gold) > 0) output_rewards += ", " + Utils.CInt(completion_reward_gold).ToString("#,##0") + " gold";
                                if (Utils.CInt(completion_reward_EXP) > 0) output_rewards += ", " + Utils.CInt(completion_reward_EXP).ToString("#,##0") + " EXP";
                                if (Utils.CInt(completion_reward_card) > 0) output_rewards += ", " + this.FriendlyReplacerInbound("[Card #" + completion_reward_card + "]");
                                if (output_rewards.Length > 0)
                                    Utils.LoggerNotifications("<color=#ffa000>Maze tower completion rewards for map " + map_stage.ToString() + ": " + output_rewards.Substring(2) + "</color>");
                            }
                        }
                    }
                }

                if (current_layer_JSON.Replace("\"", "").Replace(" ", "").ToLower().Contains("isfinish:true")) continue;

                for (int current_item_index = 0; current_item_index < current_layer_items.Length; current_item_index++)
                {
                    int current_item_type = Utils.CInt(current_layer_items[current_item_index]);

                    if (current_item_type == 5)
                    {
                        string fight_info = this.GetGameData(ref this.opts, "maze", "Battle", "Layer=" + layer_in_mazetower.ToString() + "&ItemIndex=" + current_item_index.ToString() + "&MapStageId=" + map_stage.ToString() + "&manual=1", false);
                        JObject fight_JSON = JObject.Parse(fight_info);

                        string battle_id = "";
                        if (fight_info.Contains("\"BattleId\""))
                            battle_id = fight_JSON["data"]["BattleId"].ToString();

                        if (string.IsNullOrEmpty(battle_id))
                        {
                            if (fight_info.Contains("\"message\":"))
                            {
                                if (fight_info.ToLower().Contains("not enough energy"))
                                {
                                    Utils.Logger("<b>RAN OUT OF ENERGY -- TRY AGAIN LATER!</b>");
                                    return MazeStatus.Out_Of_Energy;
                                }

                                Utils.LoggerNotifications("<color=#aa0000><b>Problem picking a maze tower fight: " + fight_JSON["message"].ToString() + "</b></color>");
                                Utils.LoggerNotifications("<color=#aa0000><b>In map " + map_stage.ToString() + ", floor " + layer_in_mazetower.ToString() + ", position " + (current_item_index + 1).ToString() + " (" + MapType(current_item_type) + ")</b></color>");
                                continue;
                            }

                            Utils.LoggerNotifications("<color=#aa0000><b>Problem picking a maze tower fight: odd server response</b></color>");
                            Utils.LoggerNotifications("<color=#aa0000><b>" + fight_info + "</b></color>");
                            Utils.LoggerNotifications("<color=#aa0000><b>In map " + map_stage.ToString() + ", floor " + layer_in_mazetower.ToString() + ", position " + (current_item_index + 1).ToString() + " (" + MapType(current_item_type) + ")</b></color>");
                            continue;
                            //return MazeStatus.Error;
                        }

                        string battle_result_original = this.GetGameData(ref this.opts, "maze", "ManualBattle", "battleid=" + battle_id + "&stage=&manual=0", false, "user_mapfight" + map_stage.ToString() + "_level" + layer_in_mazetower.ToString() + "_" + MapType(current_item_type) + "_at" + current_item_index.ToString());

                        if (battle_result_original.Replace(" ", "").Replace("\"", "").Contains("{IsClear:1,"))
                        {
                            string maze_tower_completed_result = Utils.ChopperBlank(battle_result_original, "{\"IsClear\"", "}") + ",";

                            string completion_reward_gold = Utils.ChopperBlank(maze_tower_completed_result, "\"Coins\":", ",").Replace("\"", "").Trim();
                            string completion_reward_EXP = Utils.ChopperBlank(maze_tower_completed_result, "\"Exp\":", ",").Replace("\"", "").Trim();
                            string completion_reward_card = Utils.ChopperBlank(maze_tower_completed_result, "\"CardId\":", ",").Replace("\"", "").Trim();

                            Utils.Logger();
                            if (Utils.CInt(completion_reward_gold) > 0) Utils.Logger("<b>Gold earned from maze tower completion:</b> " + Utils.CInt(completion_reward_gold).ToString("#,##0"));
                            if (Utils.CInt(completion_reward_EXP) > 0) Utils.Logger("<b>Experience earned from maze tower completion:</b> " + Utils.CInt(completion_reward_EXP).ToString("#,##0"));
                            if (Utils.CInt(completion_reward_card) > 0) Utils.Logger("<b>Card earned from maze tower completion:</b> " + this.FriendlyReplacerInbound("[Card #" + completion_reward_card + "]"));
                            Utils.Logger();

                            string output_rewards = "";
                            if (Utils.CInt(completion_reward_gold) > 0) output_rewards += ", " + Utils.CInt(completion_reward_gold).ToString("#,##0") + " gold";
                            if (Utils.CInt(completion_reward_EXP) > 0) output_rewards += ", " + Utils.CInt(completion_reward_EXP).ToString("#,##0") + " EXP";
                            if (Utils.CInt(completion_reward_card) > 0) output_rewards += ", " + this.FriendlyReplacerInbound("[Card #" + completion_reward_card + "]");
                            if (output_rewards.Length > 0)
                                Utils.LoggerNotifications("<color=#ffa000>Maze tower completion rewards for map " + map_stage.ToString() + ": " + output_rewards.Substring(2) + "</color>");
                        }

                    }
                }

            }

            return MazeStatus.Completed;
        }

        private static string MapType(int type)
        {
            if (type == 1) return "nothing";
            if (type == 2) return "chest";
            if (type == 3) return "monster";
            if (type == 4) return "entrance";
            if (type == 5) return "exit";
            if (type == 6) return "nothing";
            return "unknown (#" + type.ToString() + ")";
        }

        public void ResetMazeTower(int tower)
        {
            JObject user_data = JObject.Parse(this.GetGameData("user", "GetUserInfo", false));
            JObject map_data = JObject.Parse(this.GetGameData("maze", "Show", "MapStageId=" + tower.ToString(), false));

            int iGems = Utils.CInt(user_data["data"]["Cash"]);
            int iCost = Utils.CInt(map_data["data"]["ResetCash"]);

            if (Utils.CInt(map_data["data"]["FreeReset"]) == 1)
            {
                MessageBox.Show("You already have a free reset for this tower.\r\n\r\nYou can't buy a free reset on a tower you can reset for free.", "Can't Buy Maze Tower Reset", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (Utils.CInt(map_data["data"]["Clear"]) == 0)
                if (MessageBox.Show("This maze tower is still in progress.  Are you sure you want to reset it early?", "Reset Early?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                    return;

            if (MessageBox.Show("You have " + iGems.ToString("#,##0") + " gems and it will cost " + iCost.ToString() + " to reset maze tower " + tower.ToString() + ".\r\n\r\nContinue?", "Really Spend " + iCost.ToString() + " Gems?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                this.GetGameData("maze", "Reset", "MapStageId=" + tower.ToString(), false);
                user_data = JObject.Parse(this.GetGameData("user", "GetUserInfo", false));
                this.ParentForm.GameVitalsUpdateUI(user_data, null, null);
            }
        }

        public void DumpData()
        {
            this.CheckLogin();
            if (this.opts == null)
                return;

            this.GetGameData("login", "cdnurl", true, "cdn_url");
            this.GetGameData("user", "GetUserInfo", true, "user_info");
            this.GetGameData("user", "AwardSalary", true, "user_award_salary");
            this.GetGameData("arena", "GetRankCompetitors", true, "arena_competitors");
            this.GetGameData("card", "GetAllCard", true, "card_list");
            this.GetGameData("card", "GetAllSkill", true, "card_skill_list");
            this.GetGameData("rune", "GetAllRune", true, "rune_list");
            this.GetGameData("shop", "GetWelfare", true, "event_rewards");
            this.GetGameData("card", "GetUserCards", true, "user_cards");
            this.GetGameData("rune", "GetUserRunes", true, "user_runes");
            this.GetGameData("card", "GetCardGroup", true, "user_decks");
            this.GetGameData("user", "GetBackground", true, "game_background");
            this.GetGameData("friend", "GetFriends", true, "user_friends");
            this.GetGameData("legion", "GetUserLegion", true, "user_clan");
            this.GetGameData("legion", "GetMember", true, "user_clan_members");
            this.GetGameData("legion", "GetSetting", true, "user_clan_settings");
            this.GetGameData("activity", "ActivityInfo", true, "game_events");
            this.GetGameData("mapstage", "GetMapStageALL", true, "map_stage_list");
            this.GetGameData("mapstage", "GetUserMapStages", true, "user_map_stages");
            this.GetGameData("shop", "GetGoods", true, "shop_goods");
            this.GetGameData("boss", "GetBoss", true, "game_current_boss");
            this.GetGameData("forcefight", "GetForceStatus", true, "user_force_status");
            this.GetGameData("forceshop", "GetExchangeGoods", true, "kingdomwar_shop_exchange");
            this.GetGameData("forceshop", "GetAuctionGoods", true, "kingdomwar_shop_auction");
            this.GetGameData("arena", "GetThieves", true, "thieves_list");
            this.GetGameData("hydra", "GetHydraInfo", true, "hydra_list");
            this.GetGameData("tree", "GetTreeInfo", true, "World Tree, event info");
            this.GetGameData("league", "lotteryInfo", true, "league_lottery_info");

            this.SQL_Creator_NonUserData();
            this.SQL_Creator_UserData();

            return;
        }

        #region SQL dump stuff
        public void SQL_Creator_UserData()
        {
            if (!this.Want_SQL_DataDump)
                return;

            int ct = 0;
            string output = string.Empty;

            ct = 0;
            output = "DELETE FROM `EK`.`Cards` WHERE (`owner` = \"" + this.Login_ARC_UIN + "\");\r\n\r\n";
            foreach (string s in Utils.SubStringsDups(Utils.ChopperBlank(JSBeautifyLib.JSBeautify.BeautifyMe(this.GetGameData("card", "GetUserCards", false)), "\"Cards\": [{", "}]"), "},"))
            {
                ct++;

                string card__ID = Utils.CInt(Utils.ChopperBlank(s, "\"CardId\": ", ",").Replace("\"", string.Empty).Trim()).ToString();
                string card__XP = Utils.CInt(Utils.ChopperBlank(s, "\"Exp\": ", ",").Replace("\"", string.Empty).Trim()).ToString();
                string card__LV = Utils.CInt(Utils.ChopperBlank(s, "\"Level\": ", ",").Replace("\"", string.Empty).Trim()).ToString();
                string card__EV = Utils.CInt(Utils.ChopperBlank(s, "\"Evolution\": ", ",").Replace("\"", string.Empty).Trim()).ToString();
                string card__SK = Utils.CInt(Utils.ChopperBlank(s, "\"SkillNew\": ", ",").Replace("\"", string.Empty).Trim()).ToString();

                output += "INSERT INTO `EK`.`Cards` VALUES (\"" + this.Login_ARC_UIN + "\", " + card__ID + ", " + card__LV + ", " + card__XP + ", " + card__EV + ", " + card__SK + ");\r\n";

                if (ct <= 10)
                    Utils.Logger("Card ID: " + card__ID);
                else if (ct == 11)
                    Utils.Logger("...");
            }
            Utils.FileOverwrite(Utils.AppFolder + @"\\Game Data\" + GameAbbreviation(Service) + "__user_cards_(" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".") + ").sql", output);
        }

        public void SQL_Creator_NonUserData()
        {
            if (!this.Want_SQL_DataDump)
                return;

            int ct = 0;
            string output = string.Empty;

            ct = 0;
            output = "DELETE FROM `EK`.`CardDefs`;\r\n\r\n";
            foreach (string s in Utils.SubStringsDups(Utils.ChopperBlank(JSBeautifyLib.JSBeautify.BeautifyMe(this.GetGameData("card", "GetAllCard", false)), "\"Cards\": [{", "}]"), "},"))
            {
                ct++;

                string card__ID = Utils.ChopperBlank(s, "\"CardId\": ", ",").Replace("\"", string.Empty);
                string card__Name = Utils.ChopperBlank(s, "\"CardName\": ", ",").Replace("\"", string.Empty);
                string card__Cost = Utils.ChopperBlank(s, "\"Cost\": ", ",").Replace("\"", string.Empty);
                string card__Stars = Utils.ChopperBlank(s, "\"Color\": ", ",").Replace("\"", string.Empty);
                string card__Element = Utils.ChopperBlank(s, "\"Race\": ", ",").Replace("\"", string.Empty);
                string card__Wait = Utils.ChopperBlank(s, "\"Wait\": ", ",").Replace("\"", string.Empty);

                string card__Skill1 = Utils.ChopperBlank(s, "\"Skill\": ", ",").Replace("\"", string.Empty);
                string card__Skill2 = Utils.ChopperBlank(s, "\"LockSkill1\": ", ",").Replace("\"", string.Empty);
                string card__Skill3 = Utils.ChopperBlank(s, "\"LockSkill2\": ", ",").Replace("\"", string.Empty);
                string card__GoldWorth = Utils.ChopperBlank(s, "\"Price\": ", ",").Replace("\"", string.Empty);
                string card__XPWorth = Utils.ChopperBlank(s, "\"BaseExp\": ", ",").Replace("\"", string.Empty);

                string card__DropMazeTower = s.Contains("\"Maze\": \"1\"") ? "1" : "0";
                string card__DropThief = s.Contains("\"Robber\": \"1\"") ? "1" : "0";
                string card__DropFireToken = s.Contains("\"MagicCard\": \"1\"") ? "1" : "0";
                string card__DropDemonInvasion = s.Contains("\"Boss\": \"1\"") ? "1" : "0";
                string card__DropGoldPack = s.Contains("\"SeniorPacket\": \"1\"") ? "1" : "0"; // seems to also have FightMPacket
                string card__DropGemPack = s.Contains("\"MasterPacket\": \"1\"") ? "1" : "0"; // MasterPacket/StandardPacket/GuruPacket, seems to also have FightMPacket
                string card__DropPromoGemPack = s.Contains("\"BigYearPacket\": \"1\"") ? "1" : "0";

                string card__KWAuctionPrice = Utils.ChopperBlank(s, "\"ForceAuctionInitPrice\": ", ",").Replace("\"", string.Empty);
                string card__KWExchangePrice = s.Contains("\"ForceFightExchange\": \"1\"") ? Utils.ChopperBlank(s, "\"ForceExchangeInitPrice\": ", ",").Replace("\"", string.Empty) : "0";

                string card__HPProgression = Utils.ChopperBlank(s, "\"HpArray\": ", ",\r\n").Replace("\"", string.Empty);
                string card__ATKProgression = Utils.ChopperBlank(s, "\"AttackArray\": ", ",\r\n").Replace("\"", string.Empty);
                string card__XPProgression = Utils.Chopper(s, "\"ExpArray\": ", "\r\n").Replace("\"", string.Empty);

                string card__JSON = s;

                if (!card__JSON.Trim().StartsWith("{")) card__JSON = "{\r\n\t\t" + card__JSON;
                if (!card__JSON.Trim().EndsWith("}")) card__JSON += "}";

                int card__Adjusted_XPWorth = (Utils.CInt(card__XPWorth.Replace("\"", "")) * 100) *
                    ((100 - ((5 - Utils.CInt(card__Stars.Replace("\"", ""))) * 10))) / 10000;

                output +=
                    "INSERT INTO `EK`.`CardDefs` VALUES (" +
                    Utils.CInt(card__ID.Replace("\"", "")).ToString() + ", " +
                    "\"" + card__Name.Trim().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").Trim() + "\", " +
                    Utils.CInt(card__Cost.Replace("\"", "")).ToString() + ", " +
                    Utils.CInt(card__Stars.Replace("\"", "")).ToString() + ", " +
                    Utils.CInt(card__Element.Replace("\"", "")).ToString() + ", " +
                    Utils.CInt(card__Wait.Replace("\"", "")).ToString() + ", " +

                    Utils.CInt(card__Skill1.Replace("\"", "")).ToString() + ", " +
                    Utils.CInt(card__Skill2.Replace("\"", "")).ToString() + ", " +
                    Utils.CInt(card__Skill3.Replace("\"", "")).ToString() + ", " +

                    Utils.CInt(card__GoldWorth.Replace("\"", "")).ToString() + ", " +
                    card__Adjusted_XPWorth.ToString() + ", " +
                    card__DropMazeTower + ", " +
                    card__DropThief + ", " +
                    card__DropFireToken + ", " +
                    card__DropDemonInvasion + ", " +
                    card__DropGoldPack + ", " +
                    card__DropGemPack + ", " +
                    card__DropPromoGemPack + ", " +

                    Utils.CInt(card__KWAuctionPrice.Replace("\"", "")).ToString() + ", " +
                    Utils.CInt(card__KWExchangePrice.Replace("\"", "")).ToString() + ", " +

                    "\"" + card__HPProgression + "\", " +
                    "\"" + card__ATKProgression + "\", " +
                    "\"" + card__XPProgression + "\", " +

                    "\"" + card__JSON.Trim().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\t", "\\t").Replace("\n", "\\n").Trim() + "\"" +
                    ");\r\n";

                if (ct <= 10)
                    Utils.Logger("Card: " + card__ID + ", \"" + card__Name + "\"");
                else if (ct == 11)
                    Utils.Logger("...");
            }
            Utils.FileOverwrite(Utils.AppFolder + @"\\Game Data\" + GameAbbreviation(Service) + "__card_list_(" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".") + ").sql", output);


            ct = 0;
            output = "DELETE FROM `EK`.`RuneDefs`;\r\n\r\n";

            JObject runes = JObject.Parse(this.GetGameData("rune", "GetAllRune", false));

            foreach (JObject rune in runes["data"]["Runes"])
            {
                output += "INSERT INTO `EK`.`RuneDefs` VALUES (" +
                    rune["RuneId"].ToString() + ", " +
                    "\"" + rune["RuneName"].ToString() + "\", " +
                    rune["Color"].ToString() + ", " +
                    rune["Property"].ToString() + ", " +
                    rune["LockSkill1"].ToString() + ", " +
                    rune["LockSkill2"].ToString() + ", " +
                    rune["LockSkill3"].ToString() + ", " +
                    rune["LockSkill4"].ToString() + ", " +
                    rune["LockSkill5"].ToString() + ", " +
                    rune["Price"].ToString() + ", " +
                    rune["SkillTimes"].ToString() + ", " +
                    "\"" + rune["Condition"].ToString() + "\", " +
                    rune["ThinkGet"].ToString() + ", " +
                    rune["Fragment"].ToString() + ", " +
                    rune["SkillConditionSlide"].ToString() + ", " +
                    rune["SkillConditionType"].ToString() + ", " +
                    rune["SkillConditionRace"].ToString() + ", " +
                    rune["SkillConditionColor"].ToString() + ", " +
                    rune["SkillConditionCompare"].ToString() + ", " +
                    rune["SkillConditionValue"].ToString() + ", " +
                    rune["BaseExp"].ToString() +
                    ");\r\n";
            }
            Utils.FileOverwrite(Utils.AppFolder + @"\\Game Data\" + GameAbbreviation(Service) + "__rune_list_(" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".") + ").sql", output);



            ct = 0;
            output = "DELETE FROM `EK`.`SkillDefs`;\r\n\r\n";
            foreach (string s in Utils.SubStringsDups(Utils.ChopperBlank(JSBeautifyLib.JSBeautify.BeautifyMe(this.GetGameData("card", "GetAllSkill", false)), "\"Skills\": [{", "}]"), "},"))
            {
                ct++;

                string skill__ID = Utils.ChopperBlank(s, "\"SkillId\": ", ",").Replace("\"", string.Empty);
                string skill__Name = Utils.ChopperBlank(s, "\"Name\": ", ",").Replace("\"", string.Empty);
                string skill__SkillType = Utils.ChopperBlank(s, "\"Type\": ", ",").Replace("\"", string.Empty);
                string skill__LaunchType = Utils.ChopperBlank(s, "\"LanchType\": ", ",").Replace("\"", string.Empty);
                string skill__LaunchCondition = Utils.ChopperBlank(s, "\"LanchCondition\": ", ",").Replace("\"", string.Empty);
                string skill__LaunchConditionValue = Utils.ChopperBlank(s, "\"LanchConditionValue\": ", ",").Replace("\"", string.Empty);
                string skill__AffectType = Utils.ChopperBlank(s, "\"AffectType\": ", ",").Replace("\"", string.Empty);
                string skill__AffectValue = Utils.ChopperBlank(s, "\"AffectValue\": ", ",").Replace("\"", string.Empty);
                string skill__AffectValue2 = Utils.ChopperBlank(s, "\"AffectValue2\": ", ",").Replace("\"", string.Empty);
                string skill__SkillCategory = Utils.ChopperBlank(s, "\"SkillCategory\": ", ",").Replace("\"", string.Empty);
                string skill__EvoRank = Utils.CInt(Utils.ChopperBlank(s, "\"EvoRank\": ", "\r\n").Replace("\"", string.Empty)).ToString();
                string skill__Desc = Utils.ChopperBlank(s, "\"Desc\": ", "\r\n").Replace("\"", string.Empty).TrimEnd(new char[] { ',', ' ' });

                if (skill__AffectValue2.Trim().Length == 0)
                {
                    if (skill__AffectValue.Contains("_"))
                    {
                        skill__AffectValue2 = Utils.ChopperBlank(skill__AffectValue, "_", null);
                        skill__AffectValue = Utils.ChopperBlank(skill__AffectValue, null, "_");
                    }
                }

                output +=
                    "INSERT INTO `EK`.`SkillDefs` VALUES (" +
                    skill__ID + ", " +
                    "\"" + skill__Name.Trim().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").Trim() + "\", " +
                    skill__SkillType + ", " +
                    skill__LaunchType + ", " +
                    skill__LaunchCondition + ", " +
                    skill__LaunchConditionValue + ", " +
                    skill__AffectType + ", " +
                    skill__AffectValue + ", " +
                    skill__AffectValue2 + ", " +
                    skill__SkillCategory + ", " +
                    "\"" + skill__Desc.Trim().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").Trim() + "\", " +
                    skill__EvoRank + 
                    ");\r\n";

                if (ct <= 10)
                    Utils.Logger("Skill: " + skill__ID + ", \"" + skill__Name + "\"");
                else if (ct == 11)
                    Utils.Logger("...");
            }
            Utils.FileOverwrite(Utils.AppFolder + @"\\Game Data\" + GameAbbreviation(Service) + "__card_skill_list_(" + Utils.GetAppSetting("Login_Account").Trim().Replace("@", ".") + ").sql", output);
        }
        #endregion

        public void GameVitalsUpdate(string game_data = null, string arena_data = null, string thief_data = null)
        {
            JObject j_game_data = null;
            JObject j_arena_data = null;
            JObject j_thief_data = null;

            try
            {
                if (game_data != null) j_game_data = JObject.Parse(game_data);
                if (arena_data != null) j_arena_data = JObject.Parse(arena_data);
                if (thief_data != null) j_thief_data = JObject.Parse(thief_data);
            }
            catch { }

            this.ParentForm.GameVitalsUpdateUI(j_game_data, j_arena_data, j_thief_data);
            return;
        }


        public string GetCardName(string id)
        {
            foreach (string card_JSON in Utils.SubStringsDups(this.All_Cards_JSON, "},{"))
            {
                string card_id = Utils.ChopperBlank(card_JSON, "\"CardId\":", ",").Replace("\"", "").Trim();
                if (Utils.CInt(card_id) > 0)
                {
                    string card_name = Utils.ChopperBlank(card_JSON, "\"CardName\":", ",").Replace("\"", "").Trim();

                    if (card_id == id)
                        return card_name;
                }
            }

            return "Card #" + id;
        }

        public string FriendlyReplacerInbound(string text)
        {
            try
            {
                if (!Utils.ValidText(this.All_Cards_JSON))
                    return text;

                if (this.Cards_JSON_Parsed == null)
                    this.Cards_JSON_Parsed = JObject.Parse(this.All_Cards_JSON);

                foreach (JObject card in this.Cards_JSON_Parsed["data"]["Cards"])
                {
                    string card_name = card["CardName"].ToString().Replace("\"", string.Empty);
                    string card_id = Utils.CInt(card["CardId"].ToString()).ToString();

                    text = text.Replace("[Card #" + card_id + "]", "[<link><text>" + card_name + "</text><url>||EKU||CARD||EKU||" + card_id + "||EKU||" + 0 + "||EKU||</url></link>]");
                }

                //text = text.Replace("<", "((").Replace(">", "))");
            }
            catch
            {
                // note: do not use Utils.Chatter() or any logger in here, since it will just call FriendlyReplacerInbound() again recursively and cause a stack overflow

                foreach (string card_JSON in Utils.SubStringsDups(this.All_Cards_JSON, "},{"))
                {
                    string card_id = Utils.ChopperBlank(card_JSON, "\"CardId\":", ",").Replace("\"", "").Trim();
                    if (Utils.CInt(card_id) > 0)
                    {
                        string card_name = Utils.ChopperBlank(card_JSON, "\"CardName\":", ",").Replace("\"", "").Trim();

                        //text = text.Replace("[Card #" + card_id + "]", "<u>[" + card_name + "]</u>");

                        text = text.Replace("[Card #" + card_id + "]", "[<link><text>" + card_name + "</text><url>||EKU||CARD||EKU||" + card_id + "||EKU||" + 0 + "||EKU||</url></link>]");
                    }
                }
            }

            foreach (string rune_JSON in Utils.SubStringsDups(this.All_Runes_JSON, "},{"))
            {
                string rune_id = Utils.ChopperBlank(rune_JSON, "\"RuneId\":", ",").Replace("\"", "").Trim();
                if (Utils.CInt(rune_id) > 0)
                {
                    string rune_name = Utils.ChopperBlank(rune_JSON, "\"RuneName\":", ",").Replace("\"", "").Trim();

                    //text = text.Replace("[Rune #" + rune_id + "]", "<u>[" + rune_name + "]</u>");

                    text = text.Replace("[Rune #" + rune_id + "]", "[<link><text>" + rune_name + "</text><url>||EKU||RUNE||EKU||" + rune_id + "||EKU||" + 0 + "||EKU||</url></link>]");
                }
            }

            return text;
        }

        public string FriendlyReplacerOutbound(string text)
        {
            bool skip_HTML_formatted_text = false;

            text = text.Replace("&", "&amp;");
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");

            if (!skip_HTML_formatted_text)
            {
                if (this.Cards_JSON_Parsed == null) this.Cards_JSON_Parsed = JObject.Parse(this.All_Cards_JSON);
                if (this.Runes_JSON_Parsed == null) this.Runes_JSON_Parsed = JObject.Parse(this.All_Runes_JSON);

                foreach (string card_JSON in Utils.SubStringsDups(this.All_Cards_JSON, "},{"))
                {
                    string card_id = Utils.ChopperBlank(card_JSON, "\"CardId\":", ",").Replace("\"", "").Trim();
                    if (Utils.CInt(card_id) > 0)
                    {
                        string card_name_spaces = Utils.ChopperBlank(card_JSON, "\"CardName\":", ",").Replace("\"", "");
                        string card_name = Utils.ChopperBlank(card_JSON, "\"CardName\":", ",").Replace("\"", "").Trim();

                        text = Utils.StringReplace(text, "[" + card_name_spaces + "]", "<a href=\"event:card_" + card_id + "\"><u>[" + card_name_spaces + "]</u></a>");
                        text = Utils.StringReplace(text, "[" + card_name + "]", "<a href=\"event:card_" + card_id + "\"><u>[" + card_name_spaces + "]</u></a>");

                        for (int Level = 0; Level <= 10; Level++)
                        {
                            text = Utils.StringReplace(text, "[" + card_name_spaces + "@" + Level.ToString() + "]", "<a href=\"event:card_" + card_id + "_" + Level.ToString() + "\"><u>[" + card_name_spaces + "]</u></a>");
                            text = Utils.StringReplace(text, "[" + card_name + "@" + Level.ToString() + "]", "<a href=\"event:card_" + card_id + "_" + Level.ToString() + "\"><u>[" + card_name_spaces + "]</u></a>");
                        }
                    }
                }

                foreach (string rune_JSON in Utils.SubStringsDups(this.All_Runes_JSON, "},{"))
                {
                    string rune_id = Utils.ChopperBlank(rune_JSON, "\"RuneId\":", ",").Replace("\"", "").Trim();
                    if (Utils.CInt(rune_id) > 0)
                    {
                        string rune_name_spaces = Utils.ChopperBlank(rune_JSON, "\"RuneName\":", ",").Replace("\"", "");
                        string rune_name = Utils.ChopperBlank(rune_JSON, "\"RuneName\":", ",").Replace("\"", "").Trim();

                        text = Utils.StringReplace(text, "[" + rune_name_spaces + "]", "<a href=\"event:rune_" + rune_id + "\"><u>[" + rune_name_spaces + "]</u></a>");
                        text = Utils.StringReplace(text, "[" + rune_name + "]", "<a href=\"event:rune_" + rune_id + "\"><u>[" + rune_name_spaces + "]</u></a>");

                        for (int Level = 0; Level <= 4; Level++)
                        {
                            text = Utils.StringReplace(text, "[" + rune_name_spaces + "@" + Level.ToString() + "]", "<a href=\"event:rune_" + rune_id + "_" + Level.ToString() + "\"><u>[" + rune_name_spaces + "]</u></a>");
                            text = Utils.StringReplace(text, "[" + rune_name + "@" + Level.ToString() + "]", "<a href=\"event:rune_" + rune_id + "_" + Level.ToString() + "\"><u>[" + rune_name_spaces + "]</u></a>");
                        }
                    }
                }

                for (int iPos = 0; iPos < text.Length; )
                {
                    bool match = false;

                    for (int iTextComparisonClosenessMatchMode = 1; iTextComparisonClosenessMatchMode <= 5; iTextComparisonClosenessMatchMode++)
                    {
                        try
                        {
                            string current_text = text.Substring(iPos);

                            if (current_text.StartsWith("<a") && current_text.Contains("</a>"))
                            {
                                iPos += current_text.IndexOf("</a>") + 3;
                                match = true;
                                break;
                            }

                            if (!current_text.StartsWith("[")) break;
                            if (current_text.Length < 3) break;
                            if (!current_text.Contains("]")) break;

                            string Level = "";
                            string original_card_link_from_user = Utils.ChopperBlank(current_text, "[", "]");
                            string card_name_to_search = original_card_link_from_user;

                            if (original_card_link_from_user.Contains("@"))
                            {
                                Level = Utils.ChopperBlank(original_card_link_from_user, "@", null);
                                card_name_to_search = Utils.ChopperBlank(original_card_link_from_user, null, "@");
                            }

                            if (!string.IsNullOrEmpty(card_name_to_search))
                            {
                                foreach (JObject card in this.Cards_JSON_Parsed["data"]["Cards"])
                                {
                                    string actual_card_name = card["CardName"].ToString();

                                    if (iTextComparisonClosenessMatchMode == 1) match = TextComparison.IsExactMatch(card_name_to_search, actual_card_name);
                                    else if (iTextComparisonClosenessMatchMode == 2) match = TextComparison.IsReallyCloseMatch(card_name_to_search, actual_card_name);
                                    else if (iTextComparisonClosenessMatchMode == 3) match = TextComparison.IsCloseMatch(card_name_to_search, actual_card_name);
                                    else if (iTextComparisonClosenessMatchMode == 4) match = TextComparison.IsPossibleMatch(card_name_to_search, actual_card_name);
                                    else if (iTextComparisonClosenessMatchMode == 5) match = TextComparison.ProbabilityForMatch(card_name_to_search, actual_card_name) >= 40.0;

                                    if (match)
                                    {
                                        if (Utils.CInt(Level) >= 1 && Utils.CInt(Level) <= 10)
                                            text = text.Replace("[" + original_card_link_from_user + "]", "<a href=\"event:card_" + card["CardId"].ToString() + "_" + Utils.CInt(Level).ToString() + "\"><u>[" + actual_card_name + "]</u></a>");
                                        else
                                            text = text.Replace("[" + original_card_link_from_user + "]", "<a href=\"event:card_" + card["CardId"].ToString() + "\"><u>[" + actual_card_name + "]</u></a>");
                                        iPos = 0;
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            break;
                        }

                        if (match)
                            break;
                    }

                    if (!match)
                        iPos++;
                }

                for (int iPos = 0; iPos < text.Length; )
                {
                    bool match = false;

                    for (int iTextComparisonClosenessMatchMode = 1; iTextComparisonClosenessMatchMode <= 5; iTextComparisonClosenessMatchMode++)
                    {
                        try
                        {
                            string current_text = text.Substring(iPos);

                            if (current_text.StartsWith("<a") && current_text.Contains("</a>"))
                            {
                                iPos += current_text.IndexOf("</a>") + 3;
                                match = true;
                                break;
                            }

                            if (!current_text.StartsWith("[")) break;
                            if (current_text.Length < 3) break;
                            if (!current_text.Contains("]")) break;

                            string Level = "";
                            string original_rune_link_from_user = Utils.ChopperBlank(current_text, "[", "]");
                            string rune_name_to_search = original_rune_link_from_user;

                            if (original_rune_link_from_user.Contains("@"))
                            {
                                Level = Utils.ChopperBlank(original_rune_link_from_user, "@", null);
                                rune_name_to_search = Utils.ChopperBlank(original_rune_link_from_user, null, "@");
                            }

                            if (!string.IsNullOrEmpty(rune_name_to_search))
                            {
                                foreach (JObject rune in this.Runes_JSON_Parsed["data"]["Runes"])
                                {
                                    string actual_rune_name = rune["RuneName"].ToString();

                                    if (iTextComparisonClosenessMatchMode == 1) match = TextComparison.IsExactMatch(rune_name_to_search, actual_rune_name);
                                    else if (iTextComparisonClosenessMatchMode == 2) match = TextComparison.IsReallyCloseMatch(rune_name_to_search, actual_rune_name);
                                    else if (iTextComparisonClosenessMatchMode == 3) match = TextComparison.IsCloseMatch(rune_name_to_search, actual_rune_name);
                                    else if (iTextComparisonClosenessMatchMode == 4) match = TextComparison.IsPossibleMatch(rune_name_to_search, actual_rune_name);
                                    else if (iTextComparisonClosenessMatchMode == 5) match = TextComparison.ProbabilityForMatch(rune_name_to_search, actual_rune_name) >= 40.0;


                                    if (match)
                                    {
                                        if (Utils.CInt(Level) >= 1 && Utils.CInt(Level) <= 4)
                                            text = text.Replace("[" + original_rune_link_from_user + "]", "<a href=\"event:rune_" + rune["RuneId"].ToString() + "_" + Utils.CInt(Level).ToString() + "\"><u>[" + actual_rune_name + "]</u></a>");
                                        else
                                            text = text.Replace("[" + original_rune_link_from_user + "]", "<a href=\"event:rune_" + rune["RuneId"].ToString() + "\"><u>[" + actual_rune_name + "]</u></a>");
                                        iPos = 0;
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            break;
                        }

                        if (match)
                            break;
                    }

                    if (!match)
                        iPos++;
                }
            }

            return text;
        }
        
        #region Chinese server stuff

        private const string TAG_SW = "&phpp=ANDROID&phpl=ZH_CN&pvc=1.7.4&pvb=2015-08-07%2018%3A55";

        private Comm.CommFetchOptions Login_SW(string language_to_use = "ZH_CN")
        {
            try
            {
                if (this.opts != null)
                {
                    string data = this.GetGameData("user", "GetUserInfo", false);
                    this.GameVitalsUpdate(data);

                    if (data.Contains("{\"status\":1"))
                        return this.opts;
                }

                this.opts = new Comm.CommFetchOptions();
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

                this.opts.POST_Data =
                    "{\"callPara\":{\"userId\":null,\"udid\":\"" + Utils.GetAppSetting("Login_Account").Trim().ToUpper() + "\",\"accessToken\":null,\"loginType\":null,\"idfa\":\"\",\"gameName\":\"SGZJ-ANDROID-CHS\"},\"serviceName\":\"startGameJson\"}";

                this.PassportLoginJSON = Utils.CStr(Comm.Download("http://pp.fantasytoyou.com/pp/httpService.do", ref this.opts));
                Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                this.PassportLoginJSON = regex.Replace(this.PassportLoginJSON, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(this.PassportLoginJSON);
                Utils.Logger();


                JObject login = JObject.Parse(this.PassportLoginJSON);

                this.GAME_URL = login["returnObjs"]["GS_IP"].ToString();
                string time = login["returnObjs"]["timestamp"].ToString();
                string key = login["returnObjs"]["key"].ToString();
                string uid = login["returnObjs"]["U_ID"].ToString();



                this.opts = new Comm.CommFetchOptions();
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

                string UserName = Utils.GetAppSetting("Login_Account").Trim().ToLower();

                this.opts.POST_Data =
                    "Password=" + uid + "&" +
                    "Devicetoken=&" +
                    "UserName=" + UserName + "&" +
                    "Origin=&" +
                    "Udid=" + UserName.Trim().Replace(":", "%3A").ToUpper() + "&" +
                    "time=" + time + "&" +
                    "key=" + key;

                Utils.Logger("<b>POST'ing:</b> " + this.opts.POST_Data);
                string login2 = Utils.CStr(Comm.Download(GAME_URL + "login.php?do=PassportLogin&v=" + this.seq_id.ToString() + TAG_SW, ref this.opts));
                regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                login2 = regex.Replace(login2, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(login2);
                Utils.Logger();

                this.PassportLoginJSON += login2;

                this.seq_id++;
                frmMain.AuthSerial++;
                return this.opts;
            }
            catch (Exception ex)
            {
                Utils.Logger(Errors.GetAllErrorDetails(ex));
            }

            return null;
        }
        
        private string TAG_LOA
        {
            get
            {
                return "&phpp=" + this.Login_Device + "&phpl=EN&pvc=1.3.5&pvb=2014-01-20%2010%3A11%3A25";
            }
        }

        private Comm.CommFetchOptions Login_LOA()
        {
            try
            {
                if (this.opts != null)
                {
                    string data = this.GetGameData("user", "GetUserInfo", false);
                    this.GameVitalsUpdate(data);

                    if (data.Contains("{\"status\":1"))
                        return this.opts;
                }

                string DataInfo = "{\"userName\":\"" + Utils.GetAppSetting("Login_Account").Trim() + "\",\"userPassword\":\"" + Utils.GetAppSetting("Login_Password").Trim() + "\",\"gameName\":\"CARD-" + this.Login_Device + "-EN\",\"udid\":\"01:23:45:67:89:0a\",\"idfa\":\"\",\"clientType\":\"" + this.Login_Device.ToLower() + "\",\"releaseChannel\":\"\",\"locale\":\"en\"}";
                string EncodedDataInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(DataInfo));
                //Utils.Chatter(EncodedDataInfo);

                this.opts = new Comm.CommFetchOptions();
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.GET;
                Comm.Download("http://pp.fantasytoyou.com/pp/start.do?udid=01:23:45:67:89:0a&0idfa=&locale=EN&gameName=CARD-" + this.Login_Device + "-EN&client-flash", ref this.opts);

                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.XMLHttpRequest = true;
                this.opts.POST_Data = "{\"serviceName\":\"checkUserActivedBase64Json\",\"callPara\":\"" + EncodedDataInfo + "\"}";

                this.PassportLoginJSON = Utils.CStr(Comm.Download("http://pp.fantasytoyou.com/pp/httpService.do", ref this.opts));

                Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                this.PassportLoginJSON = regex.Replace(this.PassportLoginJSON, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(this.PassportLoginJSON);
                //Utils.Chatter(this.PassportLoginJSON);
                Utils.Logger();

                JObject login = JObject.Parse(this.PassportLoginJSON);

                this.GAME_URL = login["returnObjs"]["GS_IP"].ToString();


                this.opts = new Comm.CommFetchOptions();
                this.opts.XMLHttpRequest = false;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

                this.opts.POST_Data = "";
                this.opts.POST_Data += "Origin=googleplayer&";
                this.opts.POST_Data += "PP_source=" + (login["returnObjs"]["source"].ToString()) + "&";
                this.opts.POST_Data += "PP_friendCode=" + (login["returnObjs"]["friendCode"].ToString()) + "&";
                this.opts.POST_Data += "PP_G_TYPE=" + (login["returnObjs"]["G_TYPE"].ToString()) + "&";
                this.opts.POST_Data += "PP_LOGIN_TYPE=" + (login["returnObjs"]["LOGIN_TYPE"].ToString()) + "&";
                this.opts.POST_Data += "PP_initialUName=" + (login["returnObjs"]["initialUName"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_NAME=" + (login["returnObjs"]["GS_NAME"].ToString()) + "&";
                this.opts.POST_Data += "Password=" + (login["returnObjs"]["U_ID"].ToString()) + "&";
                this.opts.POST_Data += "PP_key=" + (login["returnObjs"]["key"].ToString()) + "&";
                this.opts.POST_Data += "PP_timestamp=" + (login["returnObjs"]["timestamp"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_DESC=" + (login["returnObjs"]["GS_DESC"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_CHAT_IP=" + (login["returnObjs"]["GS_CHAT_IP"].ToString()) + "&";
                this.opts.POST_Data += "Udid=" + ("01:23:45:67:89:0a").ToUpper() + "&";
                this.opts.POST_Data += "newguide=1&";
                this.opts.POST_Data += "time=" + (login["returnObjs"]["timestamp"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_PORT=" + (login["returnObjs"]["GS_PORT"].ToString()) + "&";
                this.opts.POST_Data += "key=" + (login["returnObjs"]["key"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_IP=" + (login["returnObjs"]["GS_IP"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_ID=" + (login["returnObjs"]["GS_ID"].ToString()) + "&";
                this.opts.POST_Data += "DeviceToken=&";
                this.opts.POST_Data += "PP_U_ID=" + (login["returnObjs"]["U_ID"].ToString()) + "&";
                this.opts.POST_Data += "PP_userName=" + (login["returnObjs"]["userName"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_CHAT_PORT=" + (login["returnObjs"]["GS_CHAT_PORT"].ToString()) + "&";
                this.opts.POST_Data += "IDFA=&";
                this.opts.POST_Data += "UserName=" + (login["returnObjs"]["source"].ToString());

                this.opts.POST_Data = this.opts.POST_Data.Replace("_", "%5F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(":", "%3A");
                this.opts.POST_Data = this.opts.POST_Data.Replace("/", "%2F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(".", "%2E");
                this.opts.POST_Data = this.opts.POST_Data.Replace("-", "%2D");
                this.opts.POST_Data = this.opts.POST_Data.Replace("[", "%5B");
                this.opts.POST_Data = this.opts.POST_Data.Replace("]", "%5D");

                Utils.Logger("<b>POST'ing:</b> " + this.opts.POST_Data);
                string login2 = Utils.CStr(Comm.Download(GAME_URL + "login.php?do=PassportLogin&v=" + this.seq_id.ToString() + TAG_LOA, ref this.opts));
                regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                login2 = regex.Replace(login2, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(login2);

                Utils.Logger();

                this.PassportLoginJSON += login2;

                this.seq_id++;
                frmMain.AuthSerial++;

                this.Game_CDN_URL = JObject.Parse(this.GetGameData(ref this.opts, "login", "cdnurl", false))["data"]["cdnurl"].ToString().Replace("\\/", "/");


                return this.opts;
            }
            catch (Exception ex)
            {
                Utils.Logger(Errors.GetAllErrorDetails(ex));
            }

            return null;
        }


        private Comm.CommFetchOptions Login_SW_New()
        {
            try
            {
                if (this.opts != null)
                {
                    string data = this.GetGameData("user", "GetUserInfo", false);
                    this.GameVitalsUpdate(data);

                    if (data.Contains("{\"status\":1"))
                        return this.opts;
                }

                string DataInfo = "{\"userName\":\"" + Utils.GetAppSetting("Login_Account").Trim() + "\",\"userPassword\":\"" + Utils.GetAppSetting("Login_Password").Trim() + "\",\"gameName\":\"SGZJ-ANDROID-CHS\",\"udid\":\"01:23:45:67:89:0a\",\"idfa\":\"\",\"clientType\":\"" + this.Login_Device.ToLower() + "\",\"releaseChannel\":\"\",\"locale\":\"en\"}";
                string EncodedDataInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(DataInfo));
                //Utils.Chatter(EncodedDataInfo);

                this.opts = new Comm.CommFetchOptions();
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.GET;
                Comm.Download("http://pp.fantasytoyou.com/pp/start.do?locale=EN&gameName=SGZJ-ANDROID-CHS", ref this.opts);

                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.XMLHttpRequest = true;
                this.opts.POST_Data = "{\"serviceName\":\"checkUserActivedBase64Json\",\"callPara\":\"" + EncodedDataInfo + "\"}";

                this.PassportLoginJSON = Utils.CStr(Comm.Download("http://pp.fantasytoyou.com/pp/httpService.do", ref this.opts));

                Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                this.PassportLoginJSON = regex.Replace(this.PassportLoginJSON, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(this.PassportLoginJSON);
                //Utils.Chatter(this.PassportLoginJSON);
                Utils.Logger();

                JObject login = JObject.Parse(this.PassportLoginJSON);

                this.GAME_URL = login["returnObjs"]["GS_IP"].ToString();


                this.opts = new Comm.CommFetchOptions();
                this.opts.XMLHttpRequest = false;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

                this.opts.POST_Data = "";
                this.opts.POST_Data += "Origin=googleplayer&";
                this.opts.POST_Data += "PP_source=" + (login["returnObjs"]["source"].ToString()) + "&";
                this.opts.POST_Data += "PP_friendCode=" + (login["returnObjs"]["friendCode"].ToString()) + "&";
                this.opts.POST_Data += "PP_G_TYPE=" + (login["returnObjs"]["G_TYPE"].ToString()) + "&";
                this.opts.POST_Data += "PP_LOGIN_TYPE=" + (login["returnObjs"]["LOGIN_TYPE"].ToString()) + "&";
                this.opts.POST_Data += "PP_initialUName=" + (login["returnObjs"]["initialUName"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_NAME=" + (login["returnObjs"]["GS_NAME"].ToString()) + "&";
                this.opts.POST_Data += "Password=" + (login["returnObjs"]["U_ID"].ToString()) + "&";
                this.opts.POST_Data += "PP_key=" + (login["returnObjs"]["key"].ToString()) + "&";
                this.opts.POST_Data += "PP_timestamp=" + (login["returnObjs"]["timestamp"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_DESC=" + (login["returnObjs"]["GS_DESC"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_CHAT_IP=" + (login["returnObjs"]["GS_CHAT_IP"].ToString()) + "&";
                this.opts.POST_Data += "Udid=" + ("01:23:45:67:89:0a").ToUpper() + "&";
                this.opts.POST_Data += "newguide=1&";
                this.opts.POST_Data += "time=" + (login["returnObjs"]["timestamp"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_PORT=" + (login["returnObjs"]["GS_PORT"].ToString()) + "&";
                this.opts.POST_Data += "key=" + (login["returnObjs"]["key"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_IP=" + (login["returnObjs"]["GS_IP"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_ID=" + (login["returnObjs"]["GS_ID"].ToString()) + "&";
                this.opts.POST_Data += "DeviceToken=&";
                this.opts.POST_Data += "PP_U_ID=" + (login["returnObjs"]["U_ID"].ToString()) + "&";
                this.opts.POST_Data += "PP_userName=" + (login["returnObjs"]["userName"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_CHAT_PORT=" + (login["returnObjs"]["GS_CHAT_PORT"].ToString()) + "&";
                this.opts.POST_Data += "IDFA=&";
                this.opts.POST_Data += "UserName=" + (login["returnObjs"]["source"].ToString());

                this.opts.POST_Data = this.opts.POST_Data.Replace("_", "%5F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(":", "%3A");
                this.opts.POST_Data = this.opts.POST_Data.Replace("/", "%2F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(".", "%2E");
                this.opts.POST_Data = this.opts.POST_Data.Replace("-", "%2D");
                this.opts.POST_Data = this.opts.POST_Data.Replace("[", "%5B");
                this.opts.POST_Data = this.opts.POST_Data.Replace("]", "%5D");

                Utils.Logger("<b>POST'ing:</b> " + this.opts.POST_Data);
                string login2 = Utils.CStr(Comm.Download(GAME_URL + "login.php?do=PassportLogin&v=" + this.seq_id.ToString() + TAG_SW, ref this.opts));
                regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                login2 = regex.Replace(login2, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(login2);

                Utils.Logger();

                this.PassportLoginJSON += login2;

                this.seq_id++;
                frmMain.AuthSerial++;

                this.Game_CDN_URL = JObject.Parse(this.GetGameData(ref this.opts, "login", "cdnurl", false))["data"]["cdnurl"].ToString().Replace("\\/", "/");


                return this.opts;
            }
            catch (Exception ex)
            {
                Utils.Logger(Errors.GetAllErrorDetails(ex));
            }

            return null;
        }

        private string TAG_MR
        {
            get
            {
                return "&phpp=" + this.Login_Device + "&phpl=EN&pvc=1.7.4&pvb=2015-08-07%2018%3A55";
            }
        }

        private Comm.CommFetchOptions Login_MR()
        {
            try
            {
                if (this.opts != null)
                {
                    string data = this.GetGameData("user", "GetUserInfo", false);
                    this.GameVitalsUpdate(data);

                    if (data.Contains("{\"status\":1"))
                        return this.opts;
                }

                string DataInfo = "{\"userName\":\"" + Utils.GetAppSetting("Login_Account").Trim() + "\",\"userPassword\":\"" + Utils.GetAppSetting("Login_Password").Trim() + "\",\"gameName\":\"SGZJ-BLACKBERRY-ENG\",\"udid\":\"01:23:45:67:89:0a\",\"idfa\":\"\",\"clientType\":\"" + this.Login_Device.ToLower() + "\",\"releaseChannel\":\"\",\"locale\":\"en\"}";
                string EncodedDataInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(DataInfo));
                //Utils.Chatter(EncodedDataInfo);

                this.opts = new Comm.CommFetchOptions();
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.GET;
                Comm.Download("http://im.fantasytoyou.com/pp/start.do?udid=01:23:45:67:89:0a&0idfa=&locale=EN&gameName=SGZJ-BLACKBERRY-ENG", ref this.opts);

                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.XMLHttpRequest = true;
                this.opts.POST_Data = "{\"serviceName\":\"checkUserActivedBase64Json\",\"callPara\":\"" + EncodedDataInfo + "\"}";

                this.PassportLoginJSON = Utils.CStr(Comm.Download("http://im.fantasytoyou.com/pp/httpService.do", ref this.opts));

                Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                this.PassportLoginJSON = regex.Replace(this.PassportLoginJSON, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(this.PassportLoginJSON);
                //Utils.Chatter(this.PassportLoginJSON);
                Utils.Logger();

                JObject login = JObject.Parse(this.PassportLoginJSON);

                this.GAME_URL = login["returnObjs"]["GS_IP"].ToString();


                this.opts = new Comm.CommFetchOptions();
                this.opts.XMLHttpRequest = false;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

                this.opts.POST_Data = "";
                this.opts.POST_Data += "Origin=googleplayer&";
                this.opts.POST_Data += "PP_source=" + (login["returnObjs"]["source"].ToString()) + "&";
                this.opts.POST_Data += "PP_friendCode=" + (login["returnObjs"]["friendCode"].ToString()) + "&";
                this.opts.POST_Data += "PP_G_TYPE=" + (login["returnObjs"]["G_TYPE"].ToString()) + "&";
                //this.opts.POST_Data += "PP_LOGIN_TYPE=" + (login["returnObjs"]["LOGIN_TYPE"].ToString()) + "&";
                //this.opts.POST_Data += "PP_initialUName=" + (login["returnObjs"]["initialUName"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_NAME=" + (login["returnObjs"]["GS_NAME"].ToString()) + "&";
                this.opts.POST_Data += "Password=" + (login["returnObjs"]["U_ID"].ToString()) + "&";
                this.opts.POST_Data += "PP_key=" + (login["returnObjs"]["key"].ToString()) + "&";
                this.opts.POST_Data += "PP_timestamp=" + (login["returnObjs"]["timestamp"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_DESC=" + (login["returnObjs"]["GS_DESC"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_CHAT_IP=" + (login["returnObjs"]["GS_CHAT_IP"].ToString()) + "&";
                this.opts.POST_Data += "Udid=" + ("01:23:45:67:89:0a").ToUpper() + "&";
                this.opts.POST_Data += "newguide=1&";
                this.opts.POST_Data += "time=" + (login["returnObjs"]["timestamp"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_PORT=" + (login["returnObjs"]["GS_PORT"].ToString()) + "&";
                this.opts.POST_Data += "key=" + (login["returnObjs"]["key"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_IP=" + (login["returnObjs"]["GS_IP"].ToString()) + "&";
                //this.opts.POST_Data += "PP_GS_ID=" + (login["returnObjs"]["GS_ID"].ToString()) + "&";
                this.opts.POST_Data += "DeviceToken=&";
                this.opts.POST_Data += "PP_U_ID=" + (login["returnObjs"]["U_ID"].ToString()) + "&";
                this.opts.POST_Data += "PP_userName=" + (login["returnObjs"]["userName"].ToString()) + "&";
                this.opts.POST_Data += "PP_GS_CHAT_PORT=" + (login["returnObjs"]["GS_CHAT_PORT"].ToString()) + "&";
                this.opts.POST_Data += "IDFA=&";
                this.opts.POST_Data += "UserName=" + (login["returnObjs"]["source"].ToString());

                this.opts.POST_Data = this.opts.POST_Data.Replace("_", "%5F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(":", "%3A");
                this.opts.POST_Data = this.opts.POST_Data.Replace("/", "%2F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(".", "%2E");
                this.opts.POST_Data = this.opts.POST_Data.Replace("-", "%2D");
                this.opts.POST_Data = this.opts.POST_Data.Replace("[", "%5B");
                this.opts.POST_Data = this.opts.POST_Data.Replace("]", "%5D");

                Utils.Logger("<b>POST'ing:</b> " + this.opts.POST_Data);
                string login2 = Utils.CStr(Comm.Download(GAME_URL + "login.php?do=PassportLogin&v=" + this.seq_id.ToString() + TAG_MR, ref this.opts));
                regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                login2 = regex.Replace(login2, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                Utils.Logger(login2);

                Utils.Logger();

                this.PassportLoginJSON += login2;

                this.seq_id++;
                frmMain.AuthSerial++;

                this.Game_CDN_URL = JObject.Parse(this.GetGameData(ref this.opts, "login", "cdnurl", false))["data"]["cdnurl"].ToString().Replace("\\/", "/");


                return this.opts;
            }
            catch (Exception ex)
            {
                Utils.Logger(Errors.GetAllErrorDetails(ex));
            }

            return null;
        }

        private string TAG_ER
        {
            get
            {
                return "&phpp=ANDROID_EFUNEN&phpl=EN&pvc=1.4.2&pvb=2014-05-23%2016%3A00%3A28";
            }
        }

        private Comm.CommFetchOptions Login_ER()
        {
            try
            {
                if (this.opts != null)
                {
                    string data = this.GetGameData("user", "GetUserInfo", false);
                    this.GameVitalsUpdate(data);

                    if (data.Contains("{\"status\":1"))
                        return this.opts;
                }

                this.opts = new Comm.CommFetchOptions();
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.POST_Data = "mac=00:00:00:00&imte=0000000000000&androidid=0000000000000000&advertiser=sohu%24%24sohu_mken&params=efun&referrer=&loginName=" + System.Web.HttpUtility.UrlEncode(Utils.GetAppSetting("Login_Account").Trim()) + "&loginPwd=" + System.Web.HttpUtility.UrlEncode(Utils.GetAppSetting("Login_Password").Trim()) + "&timestamp=140595581630&gameCode=mken&systemVersion=4.4.2&deviceType=samsung%20%20SM-N900V&appPlatform=e00009&languge=en_US&signature=";
                string slogin = Utils.CStr(Comm.Download("http://login.efun.com/standard_login.shtml", ref this.opts));

                Utils.Logger("<b>HTTP request:</b>  " + "http://login.efun.com/standard_login.shtml");
                Utils.Logger("<b>POST'ing:</b>  " + this.opts.POST_Data);
                Utils.Logger(slogin);
                Utils.Logger();

                JObject jlogin = JObject.Parse(slogin);

                this.opts = new Comm.CommFetchOptions();
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.POST_Data = "";
                this.opts.POST_Data += "timestamp=" + jlogin["timestamp"].ToString();
                this.opts.POST_Data += "&sign=" + jlogin["sign"].ToString();
                this.opts.POST_Data += "&Udid=00%3A00%3A00%3A00%3A00%3A00";
                this.opts.POST_Data += "&plat=EFUN%5FEN";
                this.opts.POST_Data += "&newguide=1";
                this.opts.POST_Data += "&IDFA=";
                this.opts.POST_Data += "&userid=" + jlogin["userid"].ToString();
                this.PassportLoginJSON = Utils.CStr(Comm.Download("http://ermaster.vsplay.com/mpassport.php?do=plogin&v=12345" + TAG_ER, ref this.opts));

                Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                this.PassportLoginJSON = regex.Replace(this.PassportLoginJSON, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());

                Utils.Logger("<b>HTTP request:</b>  " + "http://ermaster.vsplay.com/mpassport.php?do=plogin&v=12345" + TAG_ER);
                Utils.Logger("<b>POST'ing:</b>  " + this.opts.POST_Data);
                Utils.Logger(this.PassportLoginJSON);
                Utils.Logger();

                JObject login = JObject.Parse(this.PassportLoginJSON);

                this.GAME_URL = login["data"]["current"]["GS_IP"].ToString();


                this.opts = new Comm.CommFetchOptions();
                this.opts.XMLHttpRequest = false;
                this.opts.WantCookies = true;
                this.opts.Method = Comm.CommFetchOptions.Methods.POST;
                this.opts.DataType_JSON = false;
                this.opts.UserAgent = Comm.CommFetchOptions.UserAgentModes.AIR;
                this.opts.Referer = Comm.CommFetchOptions.Referers.Custom;
                this.opts.CustomRefererURL = "app:/assets/CardMain.swf";

                this.opts.POST_Data = "";
                this.opts.POST_Data += "time=" + Utils.CInt(login["data"]["uinfo"]["time"]).ToString();
                this.opts.POST_Data += "&uin=" + Utils.CInt(login["data"]["uinfo"]["uin"]).ToString();
                this.opts.POST_Data += "&plat=EFUN%5FEN";
                this.opts.POST_Data += "&IDFA=";
                this.opts.POST_Data += "&nick=" + login["data"]["uinfo"]["nick"].ToString();
                this.opts.POST_Data += "&sign=" + login["data"]["uinfo"]["sign"].ToString();
                this.opts.POST_Data += "&Devicetoken=";
                this.opts.POST_Data += "&access%5Ftoken=" + login["data"]["uinfo"]["access_token"].ToString();
                this.opts.POST_Data += "&Udid=00%3A00%3A00%3A00%3A00%3A00";
                this.opts.POST_Data += "&Origin=efun%5Fen";
                this.opts.POST_Data += "&newguide=1";
                this.opts.POST_Data += "&MUid=" + Utils.CInt(login["data"]["uinfo"]["MUid"]).ToString();
                this.opts.POST_Data += "&ppsign=" + login["data"]["uinfo"]["ppsign"].ToString();

                this.opts.POST_Data = this.opts.POST_Data.Replace("_", "%5F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(":", "%3A");
                this.opts.POST_Data = this.opts.POST_Data.Replace("/", "%2F");
                this.opts.POST_Data = this.opts.POST_Data.Replace(".", "%2E");
                this.opts.POST_Data = this.opts.POST_Data.Replace("-", "%2D");
                this.opts.POST_Data = this.opts.POST_Data.Replace("[", "%5B");
                this.opts.POST_Data = this.opts.POST_Data.Replace("]", "%5D");

                string login2 = Utils.CStr(Comm.Download(GAME_URL + "login.php?do=mpLogin&v=" + this.seq_id.ToString() + TAG_ER, ref this.opts));
                regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
                login2 = regex.Replace(login2, match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString());

                Utils.Logger("<b>HTTP request:</b>  " + GAME_URL + "login.php?do=mpLogin&v=" + this.seq_id.ToString() + TAG_ER);
                Utils.Logger("<b>POST'ing:</b>  " + this.opts.POST_Data);
                Utils.Logger(login2);
                Utils.Logger();

                if (login2.Contains("\"status\":0,"))
                    return null;

                this.PassportLoginJSON += login2;
                this.seq_id++;
                frmMain.AuthSerial++;

                this.Game_CDN_URL = JObject.Parse(this.GetGameData(ref this.opts, "login", "cdnurl", false))["data"]["cdnurl"].ToString().Replace("\\/", "/");

                return this.opts;
            }
            catch (Exception ex)
            {
                Utils.Logger(Errors.GetAllErrorDetails(ex));
            }

            return null;
        }

        #endregion



        public static DateTime ServerTime = DateTime.MaxValue;
        public static DateTime ServerTimeChecked = DateTime.MaxValue;
        public static DateTime DateTimeNow
        {
            get
            {
                try
                {
                    TimeSpan time_diff = DateTime.Now - GameClient.ServerTimeChecked;
                    return GameClient.ServerTime + time_diff;
                }
                catch { }

                return DateTime.MaxValue;
            }
        }

        delegate bool DELEGATE__BOOL_VOID();
        delegate void DELEGATE__VOID();

        public void DeckReport()
        {
            lock (this.locker_gamedata)
            {
                this.CheckLogin();
                if (this.opts == null)
                    return;

                string json = this.GetGameData(ref this.opts, "card", "GetCardGroup", false);
                this.UserCards_CachedData = null;
                this.GetUsersCards();
                this.UserRunes_CachedData = null;
                this.GetUsersRunes();

                //json = JSBeautifyLib.JSBeautify.BeautifyMe(json);
                //Utils.Chatter(json);

                JObject decks = JObject.Parse(json);

                Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
                Utils.LoggerNotifications("<b><fs+><fs+><fs+>Current Deck Report<fx></b></color>");
                Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
                
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
                            Utils.LoggerNotifications("<color=#5fffff>Deck slot <b><u><color=#ffffff>" + deck_EKUnleashed_ID + "</color></u></b>:</color>  \"<i>" + deck_name + "</i>\"  <color=808080>(a.k.a. \"" + "Deck #" + iAbsoluteDeckOrdinal.ToString() + "\")</color>");
                            string pretty_cards_used = "";
                            //foreach (string unique_user_card_id in Utils.SubStrings(deck["UserCardIds"].ToString(), "_"))
                            foreach (JObject card in deck["UserCardInfo"])
                            {
                                try
                                {
                                    if (Utils.CInt(card["CardId"]) > 0)
                                    {
                                       pretty_cards_used += ", " + this.ShortCardInfo(Utils.CInt(card["CardId"]), Utils.CInt(card["Level"]), true);
                                    }
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
                                        {
                                            pretty_runes_used += ", " + this.ShortRuneInfo(Utils.CInt(rune["RuneId"]), Utils.CInt(rune["Level"]), true);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            //foreach (string unique_user_rune_id in Utils.SubStrings(deck["UserRuneIds"].ToString(), "_"))
                            //    pretty_runes_used += ", " + this.ShortRuneInfo(unique_user_rune_id, true);
                            pretty_runes_used = pretty_runes_used.TrimStart(new char[] { ',', ' ' });

                            Utils.LoggerNotifications("<color=#00afcf>\tcards: </color>" + pretty_cards_used);
                            Utils.LoggerNotifications("<color=#00afcf>\trunes: </color>" + pretty_runes_used);

                            if (iAbsoluteDeckOrdinal < 10)
                                Utils.LoggerNotifications();
                        }
                    }
                }

                Utils.LoggerNotifications("<color=#005f8f>------------------------------------------------------------------------------------------------------------------------</color>");
            }
        }

        public void ReloadSettings()
        {
            this.ThiefFilled = false;
            this.RaiderFilled = false;

            RAMCache.ClearAll();

            if (this.Chat != null)
            {
                this.Chat.AutoReconnect = Utils.False("Chat_AutoReconnect");

                if (this.Chat.AutoReconnect)
                    if (!this.ChatIsConnected)
                        Utils.StartMethodMultithreaded(() => { this.Chat.Login(this.Chat.CurrentServer, this.Chat.CurrentUserAlias, this.Chat.CurrentUserID); });
            }

            Utils.LoggerNotifications("<color=#a0ffa0>Settings have been reloaded!</color>");
        }
    }
}
