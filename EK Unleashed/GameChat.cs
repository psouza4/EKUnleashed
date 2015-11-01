using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace EKUnleashed
{
    public class GameChat
    {
        private List<string> ChatBuffer = new List<string>();
        public ChatServer CurrentServer = null;
        private DateTime dtLastHeartbeat = DateTime.Now;
        public int HeartbeatSpeed = 5000;
        private TcpClient chat_client = null;
        public string CurrentUserAlias = "";
        public string CurrentUserID = "0";
        public bool AutoReconnect = false;

        public static Dictionary<int,string> Users = new Dictionary<int,string>();

        public GameChat() { }

        public class ChatServer
        {
            public string Name = "";
            public string Host = "";
            public int Port = 8000;
        }

        private Thread trdHeartbeat = null;
        private Thread trdDataReceiver = null;

        public bool Login(ChatServer server, string alias = "", string uid = "")
        {
            try
            {
                Utils.DebugLogger("Chat server: " + server.Name);
                Utils.DebugLogger("... host: " + server.Host);
                Utils.DebugLogger("... port: " + server.Port.ToString());
                Utils.DebugLogger("... alias: " + alias);
                Utils.DebugLogger("... UID: " + uid);

                this.CurrentUserAlias = alias;
                this.CurrentUserID = uid;

                lock (this.locker)
                {
                    this.CurrentServer = server;

                    this.chat_client = new TcpClient();
                    this.chat_client.Connect(this.CurrentServer.Host, this.CurrentServer.Port);

                    this.Login_Packet(this.CurrentUserAlias, this.CurrentUserID);

                    if (!this.chat_client.Connected)
                        return false;
                }

                ThreadStart tsHeartbeat = new ThreadStart(Thread_DoHeartbeat);
                this.trdHeartbeat = new Thread(tsHeartbeat);
                this.trdHeartbeat.IsBackground = true;
                this.trdHeartbeat.Start();

                ThreadStart tsDataReceiver = new ThreadStart(Thread_DataReceiver);
                this.trdDataReceiver = new Thread(tsDataReceiver);
                this.trdDataReceiver.IsBackground = true;
                this.trdDataReceiver.Start();

                return true;
            }
            catch { }

            return false;
        }

        public void Logout(string message = null)
        {
            try
            {
                if (this.trdHeartbeat != null)
                    this.trdHeartbeat.Abort();
            }
            catch { }

            if (this.trdHeartbeat != null)
            {
                Utils.Chatter();

                if (message == null)
                {
                    if (this.AutoReconnect)
                        Utils.Chatter("<color=#ff4040>Disconnected from the game server.  Automatically reconnecting...</color>");
                    else
                        Utils.Chatter("<color=#ff4040>Disconnected from the game server (press <b>Alt+F1</b> to reconnect)</color>");
                }
                else
                {
                    Utils.Chatter("<color=#ff4040>Disconnected from the game server.  " + message + "</color>");
                }

                Utils.Chatter();
            }

            this.trdHeartbeat = null;

            try
            {
                if (this.trdDataReceiver != null)
                    this.trdDataReceiver.Abort();
            }
            catch { }

            this.trdDataReceiver = null;

            if (this.chat_client != null)
            {
                lock (this.locker)
                {
                    try
                    {
                        this.chat_client.Close();
                    }
                    catch { }
                }

                this.chat_client = null;
            }
        }

        private void Thread_DataReceiver()
        {
            try
            {
                for (; ; )
                {
                    if (!this.Connected)
                        return;

                    Thread.Sleep(100);

                    bool DataAvailable = false;

                    lock (this.locker)
                    {
                        DataAvailable = this.chat_client.Available > 0;
                    }

                    if (DataAvailable)
                    {
                        byte[] data = this.ReadPacket();
                        int packet_id = 0;

                        try
                        {
                            packet_id = ((int)data[4]);

                            if (packet_id != 2 && packet_id != 112 && packet_id != 108 && packet_id != 109 && packet_id != 113)
                            //if (packet_id != 2 && packet_id != 112 && packet_id != 113 && packet_id != 108 && packet_id != 101 && packet_id != 109)
                            {
                                Utils.DebugLogger("");
                                Utils.DebugLogger("<fl>Reading ID: " + packet_id.ToString() + GameChat.PacketMeaning(packet_id));
                                //Utils.DebugLogger("<fl>.... full data/dec: " + GameChat.OutputData(data));
                                Utils.DebugLogger("<fl>.... full data/hex: " + GameChat.OutputData(data, true));
                            }

                            if (packet_id == 5)   this.Packet5_Handler  (data);
                            if (packet_id == 112) this.Packet112_Handler(data);
                            if (packet_id == 113) this.Packet113_Handler(data);
                        }
                        catch (Exception ex)
                        {
                            Utils.DebugLogger("<color=#ff0000>" + ex.GetType().ToString() + ": " + ex.Message + "</color>");
                        }

                        //if (packet_id != 2 && packet_id != 112 && packet_id != 113 && packet_id != 108 && packet_id != 101 && packet_id != 109)
                        if (packet_id != 2 && packet_id != 112 && packet_id != 108)
                            Utils.DebugLogger("");
                    }
                }
            }
            catch { }

            this.Logout();
        }

        private object locker = new object();

        private void Thread_DoHeartbeat()
        {
            try
            {
                for (; ; )
                {
                    this.Heartbeat_Packet();

                    Thread.Sleep(this.HeartbeatSpeed);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch
            {
                this.Logout();

                if (this.AutoReconnect)
                    this.Login(this.CurrentServer, this.CurrentUserAlias, this.CurrentUserID);
            }
        }

        ~GameChat() // destructor, do not remove even with '0 references'
        {
            try
            {
                if (this.trdHeartbeat != null)
                    this.trdHeartbeat.Abort();
            }
            catch { }

            this.trdHeartbeat = null;


            try
            {
                if (this.trdDataReceiver != null)
                    this.trdDataReceiver.Abort();
            }
            catch { }

            this.trdDataReceiver = null;
        }

        public void JoinChannel(string channel)
        {
            try
            {
                this.JoinChannel_Packet(channel.Trim());
                this.Heartbeat_Packet();
            }
            catch
            {
                this.Logout();
            }
        }

        public void LeaveChannel(string channel)
        {
            try
            {
                this.LeaveChannel_Packet(channel.Trim());
                this.Heartbeat_Packet();
            }
            catch
            {
                this.Logout();
            }
        }

        public void MessageToGeneral(string message)
        {
            try
            {
                this.MessageToGeneral_Packet(message.Trim());
            }
            catch
            {
                this.Logout();
            }
        }

        public void PrivateMessage(string uid, string message)
        {
            try
            {
                this.LastUserMessage = message;
                this.PrivateMessage_Packet(uid.Trim(), message.Trim());
            }
            catch
            {
                this.Logout();
            }
        }

        public void MessageToClan(string clan_id, string message)
        {
            try
            {
                this.MessageToClan_Packet(clan_id.Trim(), message.Trim());
            }
            catch
            {
                this.Logout();
            }
        }

        public void MessageToChannel(string channel_name, string message)
        {
            try
            {
                this.MessageToChannel_Packet(channel_name.Trim(), message.Trim());
            }
            catch
            {
                this.Logout();
            }
        }

        public bool Connected
        {
            get
            {
                try
                {
                    lock (this.locker)
                    {
                        if (this.trdHeartbeat == null)
                        {
                            //Utils.Chatter(".. heartbeat thread does not exist");
                            return false;
                        }

                        if (this.trdDataReceiver == null)
                        {
                            //Utils.Chatter(".. receiver thread does not exist");
                            return false;
                        }

                        if (((this.trdHeartbeat.ThreadState & ThreadState.Background) != ThreadState.Background) && ((this.trdHeartbeat.ThreadState & ThreadState.Running) != ThreadState.Running))
                        {
                            //Utils.Chatter(".. heartbeat threadstate is: " + this.trdHeartbeat.ThreadState.ToString());
                            return false;
                        }

                        if (((this.trdDataReceiver.ThreadState & ThreadState.Background) != ThreadState.Background) && ((this.trdDataReceiver.ThreadState & ThreadState.Running) != ThreadState.Running))
                        {
                            //Utils.Chatter(".. receiver threadstate is: " + this.trdHeartbeat.ThreadState.ToString());
                            return false;
                        }

                        if (this.chat_client == null)
                        {
                            //Utils.Chatter(".. chat connection does not exist");
                            return false;
                        }

                        if (!this.chat_client.Connected)
                        {
                            //Utils.Chatter(".. chat connection exists, but is closed");
                            return false;
                        }

                        if (!this.chat_client.GetStream().CanWrite)
                        {
                            //Utils.Chatter(".. chat stream is not writable");
                            return false;
                        }

                        return true;
                    }
                }
                catch { }

                //Utils.Chatter(".. exception while checking gamechat connection state");
                return false;
            }
        }

        private string LastUserMessage = "";

        public static void SaveUserName(int id, string name)
        {
            try
            {
                if (id > 0)
                    if (!GameChat.Users.ContainsKey(id))
                        GameChat.Users.Add(id, name);
            }
            catch { }
        }

        public static string GetUserName(int id)
        {
            try
            {
                if (id > 0)
                    if (GameChat.Users.ContainsKey(id))
                        return GameChat.Users[id];
            }
            catch { }

            return "[user " + id.ToString() + "]";
        }

        public static int GetUserID(string name)
        {
            try
            {
                foreach (KeyValuePair<int,string> user in GameChat.Users)
                    if (user.Value.Trim().ToLower() == name.Trim().ToLower())
                        return user.Key;
            }
            catch { }

            return 0;
        }

        private void Packet5_Handler(byte[] raw_data)
        {
            int offset = 6; // skip 4 bytes (entire packet size) and then 2 more bytes (packet ID, in this case #5)
            List<byte> data = new List<byte>(raw_data);

            int message = (int)IntFieldFromPacket(ref data, ref offset);

            if (message == 1)
                this.Logout("You have been logged in from another location.  The chat server only allows you to be connected from one location at a time.");
            else if (message == 3)
                Utils.ChatterPrivate("<color=#ff4040>Error: " + GameChat.GetUserName(Utils.CInt(this.LastPMToUID)) + " is not logged into chat.  Your last message of \"" + this.LastUserMessage + "\" was not received.</color>");
        }

        private void Packet112_Handler(byte[] raw_data)
        {
            int offset = 6; // skip 4 bytes (entire packet size) and then 2 more bytes (packet ID, in this case #112)
            List<byte> data = new List<byte>(raw_data);

            int message_type = (int)IntFieldFromPacket(ref data, ref offset);
            string src_uid = TextFieldFromPacket(ref data, ref offset).Trim();
            string src_name = TextFieldFromPacket(ref data, ref offset).Trim();
            string channel_name = TextFieldFromPacket(ref data, ref offset).Trim();
            string message = TextFieldFromPacket(ref data, ref offset).Trim();

            src_name = Utils.UnHTML(src_name);

            if (Utils.CInt(src_uid) > 0)
                GameChat.SaveUserName(Utils.CInt(src_uid), src_name);

            string name_link = "<link><text>" + src_name + "</text><url>||EKU||PM||EKU||" + src_uid + "||EKU||" + src_name + "</url></link>";

            // <a href="event:user_84938##GearedforGod">GearedforGod</a> won the 5 Star Card <a href="event:card_60">[Ice Dragon]</a>!

            try
            {
                while (message.Contains("<a href=\"event:user_") && message.Substring(message.IndexOf("<a href=\"event:user_") + 1).Contains("</a>"))
                {
                    string username_link = Utils.ChopperBlank(message.Substring(message.IndexOf("<a href=\"event:user_")), null, "</a>") + "</a>";

                    string username_link_uid = Utils.ChopperBlank(username_link, "<a href=\"event:user_", "##");
                    string username_link_name = Utils.ChopperBlank(username_link, "##", "\"");

                    if (Utils.ValidText(username_link_name) && Utils.ValidNumber(username_link_uid))
                    {
                        GameChat.SaveUserName(Utils.CInt(username_link_uid), username_link_name);

                        // markup for names are only ever included in [System] messages, so we can assume the color should be yellow
                        message = message.Replace(username_link, "{{{link}}}{{{text}}}" + username_link_name + "{{{/text}}}{{{url}}}||EKU||PM||EKU||" + username_link_uid + "||EKU||" + username_link_name + "{{{/url}}}{{{/link}}}");
                    }
                    else
                        break;
                }
            }
            catch { }

            if (message_type == 100)
            {
                message = this.ReplaceMessage_Cards(message, "ffff00");
                message = this.ReplaceMessage_Runes(message, "ffff00");

                Utils.Chatter("<color=#ffff00>[System]  " + ParseChatMessage(message) + "</color>");
            }
            else if (message_type == 113)
            {
                Utils.Chatter("<color=#888888>[" + channel_name + ":" + message_type.ToString() + "]   <u>" + src_name + "</u>:  " + ParseChatMessage(message) + "</color>");
            }
            else if (channel_name != "chat_world")
            {
                string color = "";

                if (channel_name.StartsWith("Legion_"))
                {
                    channel_name = channel_name.Replace("Legion_", "Clan #");
                    color = "<color=#00ff00>";
                    message = this.ReplaceMessage_Cards(message, "00ff00");
                    message = this.ReplaceMessage_Runes(message, "00ff00");
                }
                else if (channel_name.StartsWith("Country_"))
                {
                    channel_name = channel_name.Replace("Country_1", "KW Tundra").Replace("Country_2", "KW Forest").Replace("Country_3", "KW Swamp").Replace("Country_4", "KW Mountain");
                    color = "<color=#00ffff>";
                    message = this.ReplaceMessage_Cards(message, "00ffff");
                    message = this.ReplaceMessage_Runes(message, "00ffff");
                }
                else
                {
                    color = "<color=#aa00aa>";
                    message = this.ReplaceMessage_Cards(message, "aa00aa");
                    message = this.ReplaceMessage_Runes(message, "aa00aa");
                }

                if (channel_name.StartsWith("KW "))
                {
                    if (channel_name.EndsWith("Tundra"))
                        Utils.ChatterKWTundra(color + "[Tundra]   </color>" + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                    else if (channel_name.EndsWith("Forest"))
                        Utils.ChatterKWForest(color + "[Forest]   </color>" + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                    else if (channel_name.EndsWith("Mountain"))
                        Utils.ChatterKWMountain(color + "[Mountain]   </color>" + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                    else if (channel_name.EndsWith("Swamp"))
                        Utils.ChatterKWSwamp(color + "[Swamp]   </color>" + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                    else
                        Utils.Chatter(color + "[" + channel_name + ":" + message_type.ToString() + "]   " + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                }
                else
                {
                    if (message_type == 101)
                    {
                        if (channel_name.StartsWith("Legion_") || channel_name.StartsWith("Clan #"))
                            Utils.ChatterClan(color + "[" + channel_name + "]   </color>" + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                        else
                            Utils.Chatter(color + "[" + channel_name + "]   </color>" + name_link + color + ":  " + ParseChatMessage(message) + "</color>");
                    }
                    else if (message_type == 106)
                    {
                        Utils.ChatterClan(color + "[" + channel_name + "]   " + ParseChatMessage(message) + "</color>");
                    }
                    else
                        Utils.Chatter(color + "[" + channel_name + ":" + message_type.ToString() + "]   <u>" + src_name + "</u>:  " + ParseChatMessage(message) + "</color>");
                }
            }
            else
            {
                if (message_type == 101)
                {
                    message = this.ReplaceMessage_Cards(message);
                    message = this.ReplaceMessage_Runes(message);

                    Utils.Chatter("<color=#ffffff>[World]  <link><text>" + src_name + "</text><url>||EKU||PM||EKU||" + src_uid + "||EKU||" + src_name + "</url></link>:  " + ParseChatMessage(message) + "</color>");
                }
                else if (message_type == 102)
                {
                    Utils.Chatter("<color=#ffa000>[Notification]  The demon invasion has started!</color>");
                    Utils.Trigger("notification_bossarrive", message);
                }
                else if (message_type == 103)
                {
                    string[] demon_info = Utils.SubStringsDups(message, "_");
                    if (demon_info.Length == 3)
                    {
                        Utils.Chatter("<color=#ffff00>[System]   The Demon is " + demon_info[0] + " minutes away.  The merit cards are: [Card #" + demon_info[1] + "] and [Card #" + demon_info[2] + "].</color>");
                        Utils.Trigger("boss_incoming", message);
                    }
                    else
                        Utils.Chatter("<color=#888888>[" + channel_name + ":" + message_type.ToString() + "]   " + src_name + "@" + src_uid + "  " + ParseChatMessage(message) + "</color>");
                }
                else if (message_type == 104) // demon invasion health remaining update
                {
                    Utils.Trigger("boss_hp_update", message);
                }
                else if (message_type == 105) // demon invasion results
                {
                    // test data:  {"LastAwards":{"AwardType":4,"AwardValue":8005,"Honor":21095,"NickName":"asifusje","Uid":62985},"RankAwards":[{"NickName":"Crystal Emperor","Honor":184773,"AwardType":4,"AwardValue":"88","Uid":92657},{"NickName":"CAtastr0ph3","Honor":155610,"AwardType":4,"AwardValue":8001,"Uid":126899},{"NickName":"Bearden","Honor":151339,"AwardType":4,"AwardValue":8005,"Uid":111425},{"NickName":"Murderdoll","Honor":109981,"AwardType":4,"AwardValue":8002,"Uid":35608},{"NickName":"self","Honor":96060,"AwardType":4,"AwardValue":8002,"Uid":88555}]}

                    string last_hit_JSON = Utils.ChopperBlank(message, "\"LastAwards\":{", "},");
                    string placements_JSON = Utils.ChopperBlank(message, "\"RankAwards\":[", "]}");
                    string place1_JSON = Utils.SubStringsDups(placements_JSON, "},{")[0].TrimStart(new char[] { '{' });
                    string place2_JSON = Utils.SubStringsDups(placements_JSON, "},{")[1].TrimStart(new char[] { '{' });
                    string place3_JSON = Utils.SubStringsDups(placements_JSON, "},{")[2].TrimStart(new char[] { '{' });
                    string place4_JSON = Utils.SubStringsDups(placements_JSON, "},{")[3].TrimStart(new char[] { '{' });
                    string place5_JSON = Utils.SubStringsDups(placements_JSON, "},{")[4].TrimStart(new char[] { '{' });

                    Utils.Chatter("<color=#00efff>" + "[System] The Demon invasion is over, the top 5 players were:</color>");
                    Utils.Chatter("<color=#00efff>" + "   【<fx>" + Utils.ChopperBlank(place1_JSON, "\"NickName\":", ",").Replace("\"", "").Trim() + "】<fx>Rewards [Card #" + Utils.ChopperBlank(place1_JSON, "\"AwardValue\":", ",").Replace("\"", "").Trim() + "] , Merit " + Utils.ChopperBlank(place1_JSON, "\"Honor\":", ",").Replace("\"", "").Trim() + "</color>");
                    Utils.Chatter("<color=#00efff>" + "   【<fx>" + Utils.ChopperBlank(place2_JSON, "\"NickName\":", ",").Replace("\"", "").Trim() + "】<fx>Rewards [Card #" + Utils.ChopperBlank(place2_JSON, "\"AwardValue\":", ",").Replace("\"", "").Trim() + "] , Merit " + Utils.ChopperBlank(place2_JSON, "\"Honor\":", ",").Replace("\"", "").Trim() + "</color>");
                    Utils.Chatter("<color=#00efff>" + "   【<fx>" + Utils.ChopperBlank(place3_JSON, "\"NickName\":", ",").Replace("\"", "").Trim() + "】<fx>Rewards [Card #" + Utils.ChopperBlank(place3_JSON, "\"AwardValue\":", ",").Replace("\"", "").Trim() + "] , Merit " + Utils.ChopperBlank(place3_JSON, "\"Honor\":", ",").Replace("\"", "").Trim() + "</color>");
                    Utils.Chatter("<color=#00efff>" + "   【<fx>" + Utils.ChopperBlank(place4_JSON, "\"NickName\":", ",").Replace("\"", "").Trim() + "】<fx>Rewards [Card #" + Utils.ChopperBlank(place4_JSON, "\"AwardValue\":", ",").Replace("\"", "").Trim() + "] , Merit " + Utils.ChopperBlank(place4_JSON, "\"Honor\":", ",").Replace("\"", "").Trim() + "</color>");
                    Utils.Chatter("<color=#00efff>" + "   【<fx>" + Utils.ChopperBlank(place5_JSON, "\"NickName\":", ",").Replace("\"", "").Trim() + "】<fx>Rewards [Card #" + Utils.ChopperBlank(place5_JSON, "\"AwardValue\":", ",").Replace("\"", "").Trim() + "] , Merit " + Utils.ChopperBlank(place5_JSON, "\"Honor\":", ",").Replace("\"", "").Trim() + "</color>");
                    
                    if ( Utils.ChopperBlank(last_hit_JSON, "\"NickName\":", ",").Replace("\"", "").Trim().Length > 0)
                        Utils.Chatter("<color=#00efff>" + "Lucky <u>" + Utils.ChopperBlank(last_hit_JSON, "\"NickName\":", ",").Replace("\"", "").Trim() + "</u> received [Card #" + Utils.ChopperBlank(last_hit_JSON, "\"AwardValue\":", ",").Replace("\"", "").Trim() + "] for being the last player to hit the Demon.</color>");

                    Utils.Trigger("notification_bossdie");
                }
                else if (message_type == 108) // unknown message (think it has to do with LoA/ER clan vs. clan stuff)
                {
                    // [chat_world:108]   php53f617817b951@php53f617817b951  {"Id":6,"AtkName":"Unbreakables","DefName":"Grindhouse","Date":"19:50"}
                    // [chat_world:108]   php53f617817e05d@php53f617817e05d  {"Id":8,"AtkName":"WarRiors","DefName":"Atlantis","Date":"21:50"}

                    ; // do nothing with this message type
                }
                else if (message_type == 109)
                {
                    if (Utils.CInt(message) != 0)
                    {
                        Utils.Chatter("<color=#ffff00>[System]   " + message + " minutes until the Kingdom War begins, please make sure your Kingdom War deck is ready for battle!</color>");

                        Utils.Trigger("kwstartscountdown", message);
                    }
                    else
                    {
                        Utils.Trigger("kwstarted");

                        Utils.LoggerNotifications("<color=#ffa000>Kingdom War has started</color>");

                        Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(-3, string.Empty); });
                    }
                }
                else if (message_type == 110)
                {
                    Utils.Chatter("<color=#ffff00>[System]   Kingdom War has ended for today</color>");
                    
                    Utils.Trigger("kwended");

                    Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(-2, string.Empty); });
                }
                else if (message_type == 112)
                {
                    if (Utils.CInt(message) != 0)
                    {
                        Utils.Chatter("<color=#ffff00>[System]   " + message + " minutes until the Kingdom War begins, please make sure your Kingdom War deck is ready for battle!</color>");

                        Utils.Trigger("kwstartscountdown", message);
                    }
                    else
                    {
                        Utils.Trigger("kwstarted");
                        Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(-3, string.Empty); });
                    }
                }
                else if (message_type == 118) // unknown message (no data.. think it has to do with world tree or event ranking updates)
                {
                    // [chat_world:118]   php53f617817b951@php53f617817b951
                    // [chat_world:118]   php53f617817e05d@php53f617817e05d

                    ; // do nothing with this message type
                }
                else
                    Utils.Chatter("<color=#888888>[" + channel_name + ":" + message_type.ToString() + "]   " + src_name + "@" + src_uid + "  " + ParseChatMessage(message) + "</color>");
            }
        }

        private static string ParseChatMessage(string s)
        {
            s = s.Replace("&gt;", ">");
            s = s.Replace("&lt;", "<");
            s = s.Replace("&amp;", "&");

            s = s.Replace("<u>", "[[u]]");
            s = s.Replace("</u>", "[[/u]]");
            s = s.Replace("<b>", "[[b]]");
            s = s.Replace("</b>", "[[/b]]");

            s = Utils.StripHTML(s);

            s = s.Replace("[[u]]", "<u>");
            s = s.Replace("[[/u]]", "</u>");
            s = s.Replace("[[b]]", "<b>");
            s = s.Replace("[[/b]]", "</b>");

            return s;
        }

        public string LastPMFromUID = "0";
        public string LastPMToUID = "0";

        public string ReplaceMessage_Cards(string message, string color_reset = "ffffff")
        {
            try
            {
                while (message.Contains("[<a href=\"event:card_") && message.Substring(message.IndexOf("[<a href=\"event:card_") + 1).Contains("</a>]"))
                {
                    string cardname_link = Utils.ChopperBlank(message.Substring(message.IndexOf("[<a href=\"event:card_")), null, "</a>]") + "</a>]";

                    string card_link_id = Utils.ChopperBlank(cardname_link, "[<a href=\"event:card_", "_");
                    if (!Utils.ValidNumber(card_link_id))
                        card_link_id = Utils.ChopperBlank(cardname_link, "[<a href=\"event:card_", "\"");

                    if (Utils.ValidNumber(card_link_id))
                    {
                        //string card_link_level = Utils.ChopperBlank(cardname_link, "[<a href=\"event:card_" + card_link_id + "_", "\"");

                        string[] card_link_details = Utils.SubStringsDups(Utils.ChopperBlank(cardname_link, "[<a href=\"event:card_", "\""), "_");

                        int cld__CardID = 0; try { cld__CardID = Utils.CInt(card_link_details[0]); } catch { }
                        int cld__CardLevel = 0; try { cld__CardLevel = Utils.CInt(card_link_details[1]); } catch { }
                        int cld__CardEvolvedSkill = 0; try { cld__CardEvolvedSkill = Utils.CInt(card_link_details[2]); } catch { }
                        int cld__CardEvolvedTimes = 0; try { cld__CardEvolvedTimes = Utils.CInt(card_link_details[3]); } catch { }
                        int cld__CardUnknown = 0; try { cld__CardUnknown = Utils.CInt(card_link_details[4]); } catch { }

                        // yellow text?  meh
                        //message = message.Replace(cardname_link, "[{{{link}}}{{{text}}}" + GameClient.Current.GetCardByID(Utils.CInt(card_link_id))["CardName"] + "{{{/text}}}{{{url}}}||EKU||CARD||EKU||" + card_link_id + "||EKU||" + card_link_level + "{{{/url}}}{{{/link}}}]");
                        message = message.Replace(cardname_link, "[{{{link}}}{{{text}}}" + GameClient.Current.GetCardByID(Utils.CInt(cld__CardID))["CardName"] + "{{{/text}}}{{{url}}}||EKU||CARD||EKU||" + card_link_id + "||EKU||" + cld__CardLevel.ToString() + "||EKU||" + cld__CardEvolvedSkill.ToString() + "||EKU||" + cld__CardEvolvedTimes.ToString() + "{{{/url}}}{{{/link}}}]");
                    }
                    else
                        break;
                }
            }
            catch { }

            try
            {
                while (message.Contains("<a href=\"event:card_") && message.Substring(message.IndexOf("<a href=\"event:card_") + 1).Contains("</a>"))
                {
                    string cardname_link = Utils.ChopperBlank(message.Substring(message.IndexOf("<a href=\"event:card_")), null, "</a>") + "</a>";

                    string card_link_id = Utils.ChopperBlank(cardname_link, "<a href=\"event:card_", "_");
                    if (!Utils.ValidNumber(card_link_id))
                        card_link_id = Utils.ChopperBlank(cardname_link, "<a href=\"event:card_", "\"");

                    if (Utils.ValidNumber(card_link_id))
                    {
                        //string card_link_level = Utils.ChopperBlank(cardname_link, "<a href=\"event:card_" + card_link_id + "_", "\"");

                        string[] card_link_details = Utils.SubStringsDups(Utils.ChopperBlank(cardname_link, "<a href=\"event:card_", "\""), "_");

                        int cld__CardID = 0; try { cld__CardID = Utils.CInt(card_link_details[0]); } catch { }
                        int cld__CardLevel = 0; try { cld__CardLevel = Utils.CInt(card_link_details[1]); } catch { }
                        int cld__CardEvolvedSkill = 0; try { cld__CardEvolvedSkill = Utils.CInt(card_link_details[2]); } catch { }
                        int cld__CardEvolvedTimes = 0; try { cld__CardEvolvedTimes = Utils.CInt(card_link_details[3]); } catch { }
                        int cld__CardUnknown = 0; try { cld__CardUnknown = Utils.CInt(card_link_details[4]); } catch { }

                        // yellow text?  meh
                        //message = message.Replace(cardname_link, "[{{{link}}}{{{text}}}" + GameClient.Current.GetCardByID(Utils.CInt(card_link_id))["CardName"] + "{{{/text}}}{{{url}}}||EKU||CARD||EKU||" + card_link_id + "||EKU||" + card_link_level + "{{{/url}}}{{{/link}}}]");
                        message = message.Replace(cardname_link, "[{{{link}}}{{{text}}}" + GameClient.Current.GetCardByID(Utils.CInt(cld__CardID))["CardName"] + "{{{/text}}}{{{url}}}||EKU||CARD||EKU||" + card_link_id + "||EKU||" + cld__CardLevel.ToString() + "||EKU||" + cld__CardEvolvedSkill.ToString() + "||EKU||" + cld__CardEvolvedTimes.ToString() + "{{{/url}}}{{{/link}}}]");
                    }
                    else
                        break;
                }
            }
            catch { }

            return message;
        }

        public string ReplaceMessage_Runes(string message, string color_reset = "ffffff")
        {
            try
            {
                while (message.Contains("[<a href=\"event:rune_") && message.Substring(message.IndexOf("[<a href=\"event:rune_") + 1).Contains("</a>]"))
                {
                    string runename_link = Utils.ChopperBlank(message.Substring(message.IndexOf("[<a href=\"event:rune_")), null, "</a>]") + "</a>]";

                    string rune_link_id = Utils.ChopperBlank(runename_link, "[<a href=\"event:rune_", "_");
                    if (!Utils.ValidNumber(rune_link_id))
                        rune_link_id = Utils.ChopperBlank(runename_link, "[<a href=\"event:rune_", "\"");

                    if (Utils.ValidNumber(rune_link_id))
                    {
                        string rune_link_level = Utils.ChopperBlank(runename_link, "[<a href=\"event:rune_" + rune_link_id + "_", "\"");

                        // yellow text?  meh
                        message = message.Replace(runename_link, "[{{{link}}}{{{text}}}" + GameClient.Current.GetRuneByID(Utils.CInt(rune_link_id))["RuneName"] + "{{{/text}}}{{{url}}}||EKU||RUNE||EKU||" + rune_link_id + "||EKU||" + rune_link_level + "{{{/url}}}{{{/link}}}]");
                    }
                    else
                        break;
                }
            }
            catch { }

            try
            {
                while (message.Contains("<a href=\"event:rune_") && message.Substring(message.IndexOf("<a href=\"event:rune_") + 1).Contains("</a>"))
                {
                    string runename_link = Utils.ChopperBlank(message.Substring(message.IndexOf("<a href=\"event:rune_")), null, "</a>") + "</a>";

                    string rune_link_id = Utils.ChopperBlank(runename_link, "<a href=\"event:rune_", "_");
                    if (!Utils.ValidNumber(rune_link_id))
                        rune_link_id = Utils.ChopperBlank(runename_link, "<a href=\"event:rune_", "\"");

                    if (Utils.ValidNumber(rune_link_id))
                    {
                        string rune_link_level = Utils.ChopperBlank(runename_link, "<a href=\"event:rune_" + rune_link_id + "_", "\"");

                        // yellow text?  meh
                        message = message.Replace(runename_link, "[{{{link}}}{{{text}}}" + GameClient.Current.GetRuneByID(Utils.CInt(rune_link_id))["RuneName"] + "{{{/text}}}{{{url}}}||EKU||RUNE||EKU||" + rune_link_id + "||EKU||" + rune_link_level + "{{{/url}}}{{{/link}}}]");
                    }
                    else
                        break;
                }
            }
            catch { }

            return message;
        }

        private void Packet113_Handler(byte[] raw_data)
        {
            int offset = 6; // skip 4 bytes (entire packet size) and then 2 more bytes (packet ID, in this case #112)
            List<byte> data = new List<byte>(raw_data);

            int message_type = (int)IntFieldFromPacket(ref data, ref offset);
            string src_uid = TextFieldFromPacket(ref data, ref offset).Trim();
            string src_name = TextFieldFromPacket(ref data, ref offset).Trim();
            string dst_uid = TextFieldFromPacket(ref data, ref offset).Trim();
            string message = TextFieldFromPacket(ref data, ref offset).Trim();

            if (Utils.CInt(src_uid) > 0)
                GameChat.SaveUserName(Utils.CInt(src_uid), src_name);

            string name_link = "<link><text>" + src_name + "</text><url>||EKU||PM||EKU||" + src_uid + "||EKU||" + src_name + "</url></link>";

            if (message_type == 100) // notification
            {
                if (message == "friend")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have a new friend request!</color>");
                else if (message == "box")
                {
                    Utils.Chatter("<color=#ffa000>[Notification]  You have new rewards available!</color>");
                    Utils.LoggerNotifications("<color=#ffa000>[Notification]  You have new rewards available!</color>");
                }
                else if (message == "thieve")
                    Utils.Chatter("<color=#ffa000>[Notification]  A thief has been discovered!</color>");
                else if (message == "pay")
                    Utils.Chatter("<color=#ffa000>[Notification]  Your payment was successful!</color>");
                else if (message == "boss")
                    ; // ignore this signal: it's faulty
                else if (message == "bossdie")
                    Utils.Chatter("<color=#ffa000>[Notification]  The demon invasion has ended -- the demon was slaughtered!</color>");
                else if (message.StartsWith("bossaward"))
                {
                    // sample:  bossaward {"MyAwards":{"Ranks":1,"Honor":594260}}

                    Utils.Chatter("<color=#ffa000>[Notification]  You have new rewards available from the demon invasion!</color>");
                    Utils.LoggerNotifications("<color=#ffa000>[Notification]  You have new rewards available from the demon invasion!</color>");
                }
                else if (message == "joinlegion")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have joined a new legion!</color>");
                else if (message == "outlegion")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have been kicked out of your clan!</color>");
                else if (message == "applyLegion")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have new applications to join your clan!</color>");
                else if (message == "applyLegionBoss")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have new applications to join your clan!</color>");
                else if (message == "exitlegion")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have left your clan!</color>");
                else if (message == "energy")
                    Utils.Chatter("<color=#ffa000>[Notification]  You have gained additional energy...</color>");
                else if (message == "forcefight")
                {
                    //Utils.Chatter("<color=#ffa000>[Notification]  Someone is battling you!</color>");
                }
                else if (message == "forcefightover")
                {
                    Utils.LoggerNotifications("<color=#ffa000>Kingdom War has ended for today</color>");

                    Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(-2, string.Empty); });
                }
                else if (message == "pointforcechang")
                {
                    // This means that the player is no longer attacking/defending because the control point they were on changed owner
                    //Utils.Chatter("<color=#ffa000>[Notification]  Kingdom War force point change</color>");

                    Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(-1, string.Empty); });
                }
                else if (message == "legionleader")
                    Utils.Chatter("<color=#ffa000>[Notification]  Your clan's leadership has been changed!</color>");
                else
                    Utils.Chatter("<color=#ffa000>[Notification]  Unknown '" + message + "' notification signal</color>");

                Utils.Trigger("notification_" + message);
            }
            else if (message_type == 101) // PM
            {
                message = this.ReplaceMessage_Cards(message, "bb22bb");
                message = this.ReplaceMessage_Runes(message, "bb22bb");

                if (src_uid == this.CurrentUserID)
                {
                    string dst_name = GameChat.GetUserName(Utils.CInt(dst_uid));
                    Utils.ChatterPrivate("<color=#bb22bb>[Private]  to </color><link><text>" + dst_name + "</text><url>||EKU||PM||EKU||" + dst_uid + "||EKU||" + dst_name + "</url></link><color=#bb22bb>:  " + ParseChatMessage(message) + "</color>");
                }
                else
                {
                    Utils.ChatterPrivate("<color=#ff66ff>[Private]  from </color>" + name_link + "<color=#ff66ff>:  " + ParseChatMessage(message) + "</color>");

                    this.LastPMFromUID = src_uid;
                }
            }
            else if (message_type == 203) // KW 'CountrywarDealUserInfo'
            {
                // sample:  {"user_medal":28,"battle_win":0,"battle_dead":3,"user_life":3,"user_cd":360}

                //Utils.Chatter("[" + dst_uid + ":" + message_type.ToString() + "]   " + ParseChatMessage(message));

                Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(203, message); });
            }
            else if (message_type == 204) // KW map point update
            {
                // sample:  {"point_id":15,"around":{"1":0,"2":0,"3":130,"4":0}}
                
                //Utils.Chatter("[" + dst_uid + ":" + message_type.ToString() + "]   " + ParseChatMessage(message));

                Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(204, message); });
            }
            else if (message_type == 205) // KW 'updateMainView_Resource'
            {
                // sample:  {"1":1172,"2":1614,"3":1163,"4":1106}

                //Utils.Chatter("[" + dst_uid + ":" + message_type.ToString() + "]   " + ParseChatMessage(message));

                Utils.StartMethodMultithreaded(() => { GameClient.Current.KingdomWar_NotificationCallback(205, message); });
            }
            else if (message_type == 206) // KW map point change
            {
                // sample:  [{"point_id":2,"new_force_id":1}]

                //Utils.Chatter("[" + dst_uid + ":" + message_type.ToString() + "]   " + ParseChatMessage(message));
            }
            else if (message_type == 207) // KW buff info
            {
                // if message == "1", ten minute timer on clan ATK buff
                // if message == "2", ten minute timer on clan HP buff
                // if message == "3", ten minute timer on personal ATK buff
                // if message == "4", ten minute timer on personal HP buff

                //Utils.Chatter("[" + dst_uid + ":" + message_type.ToString() + "]   " + ParseChatMessage(message));
            }
            else
            {
                Utils.Chatter("<color=#aa0000>[" + dst_uid + ":" + message_type.ToString() + "]   <u>" + src_name + "</u>:  " + ParseChatMessage(message) + "</color>");
                Utils.DebugLogger("[" + dst_uid + ":" + message_type.ToString() + "]   " + src_name + ":  " + ParseChatMessage(message));
            }
        }

        private static ushort ShortFieldFromPacket(ref List<byte> data, ref int offset)
        {
            ushort u = 0;

            u += (ushort)(((ushort)data[offset + 0]) << 0);
            u += (ushort)(((ushort)data[offset + 1]) << 8);
            offset += 2;

            return u;
        }

        private static uint IntFieldFromPacket(ref List<byte> data, ref int offset)
        {
            uint u = 0;

            u += (uint)(((uint)data[offset + 0]) << 0);
            u += (uint)(((uint)data[offset + 1]) << 8);
            u += (uint)(((uint)data[offset + 2]) << 16);
            u += (uint)(((uint)data[offset + 3]) << 24);
            offset += 4;

            return u;
        }

        private static string TextFieldFromPacket(ref List<byte> data, ref int offset)
        {
            try
            {
                List<byte> b = new List<byte>();

                uint string_length = IntFieldFromPacket(ref data, ref offset);

                for (uint i = 0; i < string_length; i++)
                    b.Add(data[offset + (int)i]);

                offset += (int)string_length;

                return Utils.CStr(b.ToArray(), b.Count);
            }
            catch { }

            return "[[error reading text from packet]]";
        }

        private static string PacketMeaning(int packet_id)
        {
            if (packet_id == 2) return " (standard heartbeat)";
            if (packet_id == 5) return " (error)";
            if (packet_id == 101) return " (chat login welcome)";
            if (packet_id == 108) return " (channel joined)";
            if (packet_id == 113) return " (notification)";

            return string.Empty;
        }

        private static string OutputData(byte[] d, bool hex_mode = false)
        {
            string s = "";

            for (int i = 0; i < d.Length; i++)
            {
                try
                {
                    if (hex_mode)
                        s = s + BytesToHex(new byte[] { d[i] }) + "   ";
                    else
                        s = s + d[i].ToString("000") + "  ";
                }
                catch { }
            }

            return s.Trim();
        }

        private static string BytesToHex(byte[] input)
        {
            string sOut = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                string sCurrent = input[i].ToString("X");
                if (sCurrent.Length == 1)
                    sCurrent = "0" + sCurrent;
                sOut += sCurrent;
            }

            return sOut;
        }

        private void Login_Packet(string nickname, string uid)
        {
            List<byte> login_packet = new List<byte>();
            Packet_AddShort(ref login_packet, 100);
            Packet_AddText(ref login_packet, "serverkey"); // "serverkey" (yes, they actually set the chat server global key to 'serverkey')
            Packet_AddText(ref login_packet, uid); // Uid
            Packet_AddText(ref login_packet, nickname); // NickName
            this.SendPacket(login_packet);

            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] logged into chat server as \"" + nickname + "\".</color>");
        }

        private void Heartbeat_Packet()
        {
            List<byte> heartbeat_packet = new List<byte>();
            Packet_AddShort(ref heartbeat_packet, 1);
            this.SendPacket(heartbeat_packet);

            dtLastHeartbeat = DateTime.Now;
        }

        private void JoinChannel_Packet(string channel)
        {
            List<byte> packet = new List<byte>();
            Packet_AddShort(ref packet, 107);
            Packet_AddText(ref packet, channel);
            this.SendPacket(packet);

            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] joined channel \"" + channel + "\".</color>");
        }

        public void Test()
        {
            List<byte> packet = new List<byte>();
            Packet_AddShort(ref packet, 117);
            Packet_AddText(ref packet, "1");
            Packet_AddText(ref packet, "1");
            this.SendPacket(packet);

            this.Heartbeat_Packet();
        }

        private void LeaveChannel_Packet(string channel)
        {
            List<byte> packet = new List<byte>();
            Packet_AddShort(ref packet, 107); // this looks the same as join, but that's how it appears in the EK app code, so it must be a toggling ID
            Packet_AddText(ref packet, channel);
            this.SendPacket(packet);

            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] left channel \"" + channel + "\".</color>");
        }

        private void MessageToGeneral_Packet(string message)
        {
            this.MessageToChannel_Packet("chat_world", message);

            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] sent message to channel \"chat_world\".</color>");
        }

        private void MessageToClan_Packet(string clan_id, string message)
        {
            this.MessageToChannel_Packet("Legion_" + clan_id, message);

            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] sent message to channel \"Legion_" + clan_id + "\".</color>");
        }

        private void MessageToKWCountry_Packet(string kwcountry_id, string message)
        {
            this.MessageToChannel_Packet("Country_" + kwcountry_id, message);

            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] sent message to channel \"Country_" + kwcountry_id + "\".</color>");
        }

        private void MessageToChannel_Packet(string channel, string message)
        {
            List<byte> packet = new List<byte>();
            Packet_AddShort(ref packet, 110);
            Packet_AddInt(ref packet, 101);
            Packet_AddText(ref packet, channel);
            Packet_AddText(ref packet, message);
            this.SendPacket(packet);
        }

        private void PrivateMessage_Packet(string recipient_id, string message)
        {
            List<byte> packet = new List<byte>();
            Packet_AddShort(ref packet, 111);
            Packet_AddInt(ref packet, 101);
            Packet_AddText(ref packet, recipient_id);
            Packet_AddText(ref packet, message);
            this.SendPacket(packet);

            this.LastPMToUID = recipient_id;


            Utils.DebugLogger("<color=#ffa000>[EK Unleashed] sent message to UID #" + recipient_id + ".</color>");
        }

        private static void Packet_AddShort(ref List<byte> data, ushort val)
        {
            byte[] val_bytes = new byte[] { ((byte)(val & 0x00ff)), ((byte)((val >> 8) & 0x00ff)) };
            data.AddRange(val_bytes);
            return;
        }

        private static void Packet_AddInt(ref List<byte> data, ulong val)
        {
            byte[] val_bytes = new byte[]
            {
                (byte)(val & 0x000000ff),
                (byte)((val >> 8) & 0x000000ff),
                (byte)((val >> 16) & 0x000000ff),
                (byte)((val >> 24) & 0x000000ff)
            };

            data.AddRange(val_bytes);
            return;
        }

        private static void Packet_AddText(ref List<byte> data, string val)
        {
            byte[] ascii_string = Utils.CByteUTF8(val);

            Packet_AddInt(ref data, (ulong)ascii_string.Length);
            data.AddRange(ascii_string);

            return;
        }

        private void SendPacket(List<byte> data)
        {
            byte[] packet_length = new byte[]
            {
                (byte)((data.Count >>  0) & 0x000000ff),
                (byte)((data.Count >>  8) & 0x000000ff),
                (byte)((data.Count >> 16) & 0x000000ff),
                (byte)((data.Count >> 24) & 0x000000ff)
            };

            try
            {
                List<byte> b = new List<byte>();
                b.AddRange(packet_length);
                b.AddRange(data);

                if (b[4] != 1 && b[4] != 107)
                //if (b[4] != 1 && b[4] != 100 && b[4] != 107 && b[4] != 110 && b[4] != 111)
                {
                    Utils.DebugLogger("");
                    Utils.DebugLogger("<fl>Writing ID: " + b[4].ToString() + PacketMeaning(b[4]));
                    Utils.DebugLogger("<fl>.... full data/hex: " + OutputData(b.ToArray(), true));
                    Utils.DebugLogger("");
                }

                lock (this.locker)
                {
                    try
                    {
                        this.chat_client.GetStream().Write(packet_length, 0, 4);
                        this.chat_client.GetStream().Write(data.ToArray(), 0, data.ToArray().Length);
                        this.chat_client.GetStream().Flush();
                    }
                    catch (Exception ex)
                    {
                        Utils.DebugLogger("<color=#ff0000>" + ex.GetType().ToString() + ": " + ex.Message + "</color>");

                        this.Logout();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.DebugLogger("<color=#ff0000>" + ex.GetType().ToString() + ": " + ex.Message + "</color>");
            }
            return;
        }

        private byte[] ReadPacket()
        {
            bool bDidReceiveYet = false;
            MemoryStream ms = new MemoryStream();
            byte[] b = null;

            lock (this.locker)
            {
            read_more:
                if (this.chat_client.Available > 0)
                {
                    bDidReceiveYet = true;

                    byte[] bBuff = new byte[16384];
                    int iAvail = this.chat_client.Available;
                    if (iAvail > 16384)
                    {
                        this.chat_client.Client.Receive(bBuff, 16384, SocketFlags.Partial);
                        ms.Write(bBuff, 0, 16384);
                        goto read_more;
                    }
                    else
                    {
                        this.chat_client.Client.Receive(bBuff, iAvail, SocketFlags.None);
                        ms.Write(bBuff, 0, iAvail);
                    }
                }

                if (!bDidReceiveYet)
                {
                    for (int i = 0; i < 3000; i++) // 30 seconds
                    {
                        System.Threading.Thread.Sleep(10);

                        if (this.chat_client.Available > 0)
                            goto read_more;

                        if (!this.chat_client.Connected)
                            this.chat_client.Close();
                    }
                }

                if (!this.chat_client.Connected)
                    this.chat_client.Close();

                b = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }

            return b;
        }

    }
}
