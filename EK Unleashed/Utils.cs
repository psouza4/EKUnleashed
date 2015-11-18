using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;

namespace EKUnleashed
{
    class Utils
    {
        public static string[] GetCommandLineArgs()
        {
            try
            {
                string sModName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;

                List<string> startup_parameters_mixed = new List<string>();
                startup_parameters_mixed.AddRange(Environment.GetCommandLineArgs());

                List<string> startup_parameters_lower = new List<string>();
                foreach (string s in startup_parameters_mixed)
                    startup_parameters_lower.Add(s.Trim());

                startup_parameters_mixed.Clear();

                return startup_parameters_lower.ToArray();
            }
            catch
            {
                return Environment.GetCommandLineArgs();
            }
        }
        
        /*
        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock); 
        */        

        private const int WM_SETREDRAW = 0x000B;
        private const int WM_USER = 0x400;
        //private const int EM_GETEVENTMASK = (WM_USER + 59);
        private const int EM_SETEVENTMASK = (WM_USER + 69);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        public static void DontDrawControl(System.Windows.Forms.Control c)
        {
            try
            {
                // Stop redrawing:
                SendMessage(c.Handle, WM_SETREDRAW, 0, IntPtr.Zero);
            }
            catch
            {
                try
                {
                    // turn on redrawing
                    SendMessage(c.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
                }
                catch { }
            }

            return;
        }

        public static void DrawControl(System.Windows.Forms.Control c)
        {
            try
            {
                // turn on redrawing
                SendMessage(c.Handle, EM_SETEVENTMASK, 0, IntPtr.Zero);
                SendMessage(c.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
            }
            catch { }
        }

        private static Int32 RandomSeed = 0;

        /*
        public static Int32 RandomNumber()
        {
            Random seed = new Random();
            int start_tick = DateTime.Now.Millisecond;
            Random random = new Random(start_tick + seed.Next());
            return (random.Next() / 10);
        }
        */

        public static Int32 RandomNumber(Int32 iSeed)
        {
            Random seed = new Random();
            int start_tick = DateTime.Now.Millisecond;
            Random random = new Random(start_tick + seed.Next() + iSeed);
            return (random.Next() / 10);
        }

        public static Int32 RandomDice(int x, int y, int z)
        {
            Int32 i_total = 0, n, zzz = 0;

            for (n = 0; n < x; n++)
            {
                zzz = Utils.RandomNumber(n + RandomSeed);
                i_total += (zzz % y) + 1;
            }
            i_total += z;

            RandomSeed += zzz;

            return i_total;
        }

        public static Int32 PickNumberBetween(int x, int y)
        {
            return RandomDice(1, (y - x) + 1, x - 1); // input 15,45 would be 1d31+14
        }


        public static void Trigger(string event_name, string event_data = "")
        {
            frmMain.ext().GameNotificationTrigger(event_name, event_data);
            return;
        }

        public static void Logger(string text = "")
        {
            try
            {
                if (Utils.False("Game_Debug"))
                    frmMain.ext().Logger(text);
            }
            catch { }

            return;
        }

        public static void Chatter(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatGeneral(text);
            }
            catch { }

            return;
        }

        public static void ChatterPrivate(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatGeneral(text);
                frmMain.ext().LoggerChatPrivate(text);
            }
            catch { }

            return;
        }

        public static void ChatterClan(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatGeneral(text);
                frmMain.ext().LoggerChatClan(text);
            }
            catch { }

            return;
        }

        public static void DebugLogger(string text = "")
        {
            try
            {
                frmMain.ext().LoggerDebug(DateTime.Now.ToShortTimeString() + " :: " + text);
            }
            catch { }

            return;
        }

        public static void LoggerNotifications(string text = "")
        {
            try
            {
                frmMain.ext().LoggerRewards(DateTime.Now.ToShortTimeString() + " :: " + text);
            }
            catch { }

            return;
        }

        public static void ChatterKWTundra(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatKWTundra(text);
            }
            catch { }

            return;
        }

        public static void ChatterKWMountain(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatKWMountain(text);
            }
            catch { }

            return;
        }

        public static void ChatterKWForest(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatKWForest(text);
            }
            catch { }

            return;
        }

        public static void ChatterKWSwamp(string text = "")
        {
            try
            {
                frmMain.ext().LoggerChatKWSwamp(text);
            }
            catch { }

            return;
        }

        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo t in encoders)
                if (t.MimeType == mimeType)
                    return t;
            return null;
        }

        public static void StartMethodMultithreadedAndWait(Action target)
        {
            StartMethodMultithreadedAndWait(target, 0);
        }

        public static void StartMethodMultithreadedAndWait(Action target, int iHowLongToWait)
        {
            try
            {
                ThreadStart tsGenericMethod = new ThreadStart(() => { try { target(); } catch (Exception ex) { Utils.DebugLogger(Errors.GetAllErrorDetails(ex)); } });
                Thread trdGenericThread = new Thread(tsGenericMethod);
                trdGenericThread.IsBackground = true;
                trdGenericThread.Start();

                DateTime dtStartTime = DateTime.Now;

                for (; ; )
                {
                    if (iHowLongToWait > 0)
                    {
                        if ((DateTime.Now - dtStartTime).TotalSeconds > iHowLongToWait)
                        {
                            try { trdGenericThread.Abort(); } catch { }
                            break;
                        }
                    }

                    if (trdGenericThread.ThreadState == System.Threading.ThreadState.Stopped) break;
                    if (trdGenericThread.ThreadState == System.Threading.ThreadState.StopRequested) break;
                    if (trdGenericThread.ThreadState == System.Threading.ThreadState.Aborted) break;
                    if (trdGenericThread.ThreadState == System.Threading.ThreadState.AbortRequested) break;

                    Thread.Sleep(15);
                    frmMain.DoEvents();
                }
            }
            catch { }
        }

        public static void StartMethodMultithreaded(Action target)
        {
            ThreadStart tsGenericMethod = new ThreadStart(() => { try { target(); } catch (Exception ex) { Utils.DebugLogger(Errors.GetAllErrorDetails(ex)); } });
            Thread trdGenericThread = new Thread(tsGenericMethod);
            trdGenericThread.IsBackground = true;
            trdGenericThread.Start();
        }

        public static string Input_Text(string sTitle, string sInstructions)
        {
            return Input_Text(sTitle, sInstructions, string.Empty);
        }

        public static string Input_Text(string sTitle, string sInstructions, string sDefaultValue)
        {
            try
            {
                using (frmInputText inputForm = new frmInputText())
                {
                    inputForm.Title = sTitle;
                    inputForm.Instructions = sInstructions;
                    inputForm.Input = sDefaultValue;

                    if (inputForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        return inputForm.Input;

                    return sDefaultValue;
                }
            }
            catch { }

            return string.Empty;
        }

        public static int CInt(Object oData)
        {
            int iRetVal = 0;

            if (oData == null)
                return iRetVal;
            if (oData.GetType() == typeof(string))
                if (((string)oData) == string.Empty)
                    return iRetVal;
            try
            {
                if (!int.TryParse(Utils.Chopper(oData.ToString().Trim(), "\"", "\"").Replace(",", "").Trim(), out iRetVal))
                    return 0;
            }
            catch { }

            return iRetVal;
        }

        public static long CLng(Object oData)
        {
            long lRetVal = 0L;

            if (oData == null)
                return -1;

            try
            {
                string sTemp = oData.ToString().Trim();

                sTemp = sTemp.Replace(",", string.Empty);
                sTemp = sTemp.Replace(".", string.Empty);
                sTemp = sTemp.Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator, string.Empty);

                if (!long.TryParse(sTemp, out lRetVal))
                    return 0L;
            }
            catch { }

            return lRetVal;
        }

        public static double CDbl(Object oData)
        {
            double iRetVal = 0.0;

            if (oData == null)
                return -1.0;

            try
            {
                string sTemp = oData.ToString().Trim();

                sTemp = sTemp.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);

                if (!double.TryParse(sTemp, out iRetVal))
                    return 0.0;
            }
            catch { }

            return iRetVal;
        }

        public static string AppFolder
        {
            get
            {
                try
                {
                    // No version!
                    return System.Environment.GetEnvironmentVariable("AppData").Trim() + "\\" + System.Windows.Forms.Application.CompanyName + "\\" + System.Windows.Forms.Application.ProductName;
                }
                catch { }


                try
                {
                    // Version, but chopped out
                    return System.Windows.Forms.Application.UserAppDataPath.Substring(0, System.Windows.Forms.Application.UserAppDataPath.LastIndexOf("\\"));
                }
                catch
                {
                    try
                    {
                        // App launch folder
                        return System.Windows.Forms.Application.ExecutablePath.Substring(0, System.Windows.Forms.Application.ExecutablePath.LastIndexOf("\\"));
                    }
                    catch
                    {
                        try
                        {
                            // Current working folder
                            return System.Environment.CurrentDirectory;
                        }
                        catch
                        {
                            try
                            {
                                // Desktop
                                return System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            }
                            catch
                            {
                                // Also current working folder
                                return ".";
                            }
                        }
                    }
                }
            }
        }
        
        public static string[] SubStrings(string sText, string sSplitter)
        {
            return sText.Split(new[] { sSplitter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SubStringsDups(string sText, string sSplitter)
        {
            return sText.Split(new[] { sSplitter }, StringSplitOptions.None);
        }

        public static string Chopper(string sText, string sSearch, string sEnd, int offset)
        {
            string sIntermediate = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sSearch))
                {
                    sIntermediate = sText.Substring(offset);
                }
                else
                {
                    if (sText.Contains(sSearch) == false)
                        return sText;

                    sIntermediate = sText.Substring(sText.IndexOf(sSearch) + sSearch.Length + offset);
                }

                if (string.IsNullOrEmpty(sEnd))
                    return sIntermediate;

                return sIntermediate.Contains(sEnd) == false ? sIntermediate : sIntermediate.Substring(0, sIntermediate.IndexOf(sEnd));
            }
            catch { }

            return sIntermediate == string.Empty ? sText : sIntermediate;
        }

        public static string Chopper(string sText, string sSearch, string sEnd)
        {
            return Utils.Chopper(sText, sSearch, sEnd, 0);
        }

        public static string ChopperBlank(string sText, string sSearch, string sEnd, int offset)
        {
            string sIntermediate = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sSearch))
                {
                    sIntermediate = sText.Substring(offset);
                }
                else
                {
                    int iIndexStart = sText.IndexOf(sSearch);
                    if (iIndexStart == -1)
                        return string.Empty;

                    sIntermediate = sText.Substring(iIndexStart + sSearch.Length + offset);
                }

                if (string.IsNullOrEmpty(sEnd))
                    return sIntermediate;

                int iIndexEnd = sIntermediate.IndexOf(sEnd);

                return (iIndexEnd == -1) ? sIntermediate : sIntermediate.Substring(0, iIndexEnd);
            }
            catch { }

            return sIntermediate == string.Empty ? string.Empty : sIntermediate;
        }

        public static string ChopperBlank(string sText, string sSearch, string sEnd)
        {
            return Utils.ChopperBlank(sText, sSearch, sEnd, 0);
        }

        public static byte[] CByteASCII(string sData)
        {
            byte[] nuller = null;

            try
            {
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                byte[] byter = new byte[encoding.GetByteCount(sData)];
                encoding.GetBytes(sData, 0, sData.Length, byter, 0);
                return byter;
            }
            catch { }

            return nuller;
        }

        public static byte[] CByteUTF8(string sData)
        {
            byte[] nuller = null;

            try
            {
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] byter = new byte[encoding.GetByteCount(sData)];
                encoding.GetBytes(sData, 0, sData.Length, byter, 0);
                return byter;
            }
            catch { }

            return nuller;
        }

        public static string CStr(byte[] bData)
        {
            string s = "";

            try
            {
                s = System.Text.Encoding.UTF8.GetString(bData, 0, bData.Length);
            }
            catch { }

            return s;
        }

        public static string CStr(byte[] bData, int iCount)
        {
            string s = "";

            try
            {
                s = System.Text.Encoding.UTF8.GetString(bData, 0, iCount);
            }
            catch { }

            return s;
        }

        public static byte[] HexToBytes(string input)
        {
            byte[] bOut = new byte[input.Length / 2];

            for (int i = 0; i < input.Length / 2; i++)
            {
                string sCurrent = input[(i * 2)].ToString() + input[(i * 2) + 1];
                bOut[i] = byte.Parse(sCurrent, System.Globalization.NumberStyles.HexNumber);
            }

            return bOut;
        }

        public static string StripHTML(string sText)
        {
            sText = sText.Replace("<b>", string.Empty).Replace("</b>", string.Empty).Replace("<i>", string.Empty).Replace("</i>", string.Empty).Replace("<u>", string.Empty).Replace("</u>", string.Empty);
            sText = sText.Replace("<B>", string.Empty).Replace("</B>", string.Empty).Replace("<I>", string.Empty).Replace("</I>", string.Empty).Replace("<U>", string.Empty).Replace("</U>", string.Empty);
            return System.Text.RegularExpressions.Regex.Replace(sText, @"<(.|\n)*?>", string.Empty).Trim();
        }

        public static string UnHTML(string sIn)
        {
            sIn = sIn.Replace("&amp;", "&");
            sIn = sIn.Replace("&apos;", "'");
            sIn = sIn.Replace("&quot;", "\"");
            sIn = sIn.Replace("&lt;", "<");
            sIn = sIn.Replace("&gt;", ">");

            while (sIn.Contains("&#x"))
            {
                int iPos = sIn.IndexOf("&#x");
                if (sIn.Substring(iPos + 5, 1) != ";")
                    break;
                string sCode = sIn.Substring(iPos + 3, 2);
                byte[] bByteVersion = Utils.HexToBytes(sCode);
                if (bByteVersion[0] == 0)
                    break;
                string sCharToReplaceWith = ((Char)bByteVersion[0]).ToString();
                sIn = sIn.Replace("&#x" + sCode + ";", sCharToReplaceWith);
            }

            try
            {
                while (sIn.Contains("&#x"))
                {
                    int iPos = sIn.IndexOf("&#x");
                    if (sIn.Substring(iPos + 6, 1) != ";")
                        break;
                    string sCode = sIn.Substring(iPos + 3, 3);

                    Int16 iUnicodeCharacter = Int16.Parse(sCode, System.Globalization.NumberStyles.HexNumber); // two-byte integer
                    System.Text.Encoding encoder = new System.Text.UnicodeEncoding(); // two-byte encoder
                    string sCharToReplaceWith = encoder.GetString(System.BitConverter.GetBytes(iUnicodeCharacter));

                    sIn = sIn.Replace("&#x" + sCode + ";", sCharToReplaceWith);
                }
            }
            catch { }

            try
            {
                while (sIn.Contains("&#x"))
                {
                    int iPos = sIn.IndexOf("&#x");
                    if (sIn.Substring(iPos + 7, 1) != ";")
                        break;
                    string sCode = sIn.Substring(iPos + 3, 4);

                    Int16 iUnicodeCharacter = Int16.Parse(sCode, System.Globalization.NumberStyles.HexNumber); // two-byte integer
                    System.Text.Encoding encoder = new System.Text.UnicodeEncoding(); // two-byte encoder
                    string sCharToReplaceWith = encoder.GetString(System.BitConverter.GetBytes(iUnicodeCharacter));

                    sIn = sIn.Replace("&#x" + sCode + ";", sCharToReplaceWith);
                }
            }
            catch { }

            while (sIn.Contains("&#"))
            {
                string sCode = Utils.Chopper(sIn, "&#", ";");

                if (Utils.CInt(sCode) == 0)
                    break;

                sIn = sIn.Replace("&#" + sCode + ";", ((Char)Utils.CInt(sCode)).ToString());
            }

            sIn = sIn.Replace("\"", "");
            sIn = sIn.Replace("&amp;", "&");
            sIn = sIn.Replace("&apos;", "'");
            sIn = sIn.Replace("&quot;", "\"");
            sIn = sIn.Replace("&lt;", "<");
            sIn = sIn.Replace("&gt;", ">");
            sIn = sIn.Replace("&nbsp;", " ");

            return sIn;
        }

        public static DateTime time_val(uint utime_in)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(utime_in);
        }

        public static uint time_val(DateTime dttime_in)
        {
            return (uint)((dttime_in - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }

        public enum StringReplacePosition
        {
            Anywhere,
            Beginning,
            Middle,
            End
        }

        public static string StringReplace(string haystack, string needle, string replacement = "", bool case_sensitive = false, StringReplacePosition position = StringReplacePosition.Anywhere)
        {
            string haystack_caseshift = (case_sensitive) ? haystack : haystack.ToLower();
            string needle_caseshift = (case_sensitive) ? needle : needle.ToLower();

            int located = haystack_caseshift.IndexOf(needle_caseshift);

            if (located < 0) return haystack;

            bool at_end = (located == (haystack.Length - needle.Length));

            if (located == 0 && (position == StringReplacePosition.Middle || position == StringReplacePosition.End)) return haystack;
            if (at_end && (position == StringReplacePosition.Beginning || position == StringReplacePosition.Middle)) return haystack;
            if (located != 0 && !at_end && (position == StringReplacePosition.Beginning || position == StringReplacePosition.End)) return haystack;

            string new_haystack = string.Empty;

            if (located == 0) new_haystack = replacement + haystack.Substring(located + needle.Length);
            else if (located != 0 && !at_end) new_haystack = haystack.Substring(0, located) + replacement + haystack.Substring(located + needle.Length);
            else if (at_end) new_haystack = haystack.Substring(0, located) + replacement;
            else
            {
                // todo: error ?
            }

            return new_haystack;
        }

        public static void FileOverwrite(string filename, string contents)
        {
            try
            {
                System.IO.File.WriteAllText(filename, contents);
            }
            catch { }

            return;
        }

        public static string FileRead(string filename)
        {
            try
            {
                return System.IO.File.ReadAllText(filename);
            }
            catch { }

            return string.Empty;
        }

        public static bool DisposeObject(object someObject)
        {
            if (someObject == null)
                return true;

            try
            {
                Type reflectedType = someObject.GetType(); // get object type

                if (new List<Type>(reflectedType.GetInterfaces()).Contains(typeof(IDisposable))) //if IDisposable
                {
                    MethodInfo disposeMethod = reflectedType.GetMethod("Dispose"); // get method
                    disposeMethod.Invoke(someObject, null);                        // call it 
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static string GetAppValue(string sAppKey, string sDefault)
        {
            try
            {
                string s = Utils.GetAppSetting(sAppKey);

                if (string.IsNullOrEmpty(s))
                    return sDefault;

                return s;
            }
            catch { }

            return sDefault;
        }

        public static int GetAppValue(string sAppKey, int iDefault)
        {
            try
            {
                string s = Utils.GetAppSetting(sAppKey);

                if (string.IsNullOrEmpty(s))
                    return iDefault;

                return Utils.CInt(s);
            }
            catch { }

            return iDefault;
        }

        public static long GetAppValueL(string sAppKey, long lDefault)
        {
            try
            {
                string s = Utils.GetAppSetting(sAppKey);

                if (string.IsNullOrEmpty(s))
                    return lDefault;

                return Utils.CLng(s);
            }
            catch { }

            return lDefault;
        }

        public static bool GetAppValue(string sAppKey, bool bDefault)
        {
            try
            {
                string s = Utils.GetAppSetting(sAppKey);

                if (string.IsNullOrEmpty(s))
                    return bDefault;

                bool bRet = false;

                if (bool.TryParse(Utils.GetAppSetting(sAppKey.Trim()).Trim().ToLower(), out bRet))
                    return bRet;
            }
            catch { }

            return bDefault;
        }

        public static bool True(string sAppKey)
        {
            try
            {
                string val = Utils.GetAppSetting(sAppKey.Trim()).Trim().ToLower();
                return (val != "false") && (val != "no") && (val != "0") && (val != "off") && (val != "disabled");
            }
            catch { }

            return true;
        }

        public static bool False(string sAppKey)
        {
            try
            {
                string val = Utils.GetAppSetting(sAppKey.Trim()).Trim().ToLower();
                return !((val != "true") && (val != "yes") && (val != "1") && (val != "on") && (val != "enabled"));
            }
            catch { }

            return false;
        }

        public static string SettingsProfile = "Default";

        public static Microsoft.Win32.RegistryKey AppSettingKey
        {
            get
            {
                try
                {
                    return Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + System.Windows.Forms.Application.CompanyName + "\\" + System.Windows.Forms.Application.ProductName + "\\" + SettingsProfile, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                catch
                {
                    Microsoft.Win32.RegistryKey key = System.Windows.Forms.Application.UserAppDataRegistry;
                    string sKeyToUse = key.ToString().Replace("HKEY_CURRENT_USER\\", "");
                    sKeyToUse = sKeyToUse.Substring(0, sKeyToUse.LastIndexOf("\\")) + "\\" + SettingsProfile;
                    key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sKeyToUse, true);

                    return key;
                }
            }
        }

        public static Microsoft.Win32.RegistryKey AppSettingKey_NoProfile
        {
            get
            {
                try
                {
                    return Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + System.Windows.Forms.Application.CompanyName + "\\" + System.Windows.Forms.Application.ProductName, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                catch
                {
                    Microsoft.Win32.RegistryKey key = System.Windows.Forms.Application.UserAppDataRegistry;
                    string sKeyToUse = key.ToString().Replace("HKEY_CURRENT_USER\\", "");
                    sKeyToUse = sKeyToUse.Substring(0, sKeyToUse.LastIndexOf("\\"));
                    key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sKeyToUse, true);

                    return key;
                }
            }
        }

        /// <summary>
        /// Reads the appSetting section of .config file for a key name and returns
        /// the value.  Will return "" if the .config file doesn't exist, the key
        /// could not be found, or an exception was raised.
        /// </summary>
        /// <param name="sKeyName">The key in the appSetting section of .config to return a value for.</param>
        public static string GetAppSetting(string sKeyName)
        {
            string sVal = null;

            object o = RAMCache.GlobalCache.Get("Settings", sKeyName);
            if (o != null)
                if (o.GetType() == typeof(string))
                    sVal = (string)o;

            if (sVal != null)
                return sVal;

            try
            {
                sVal = Utils.AppSettingKey.GetValue(sKeyName, string.Empty).ToString();

                RAMCache.GlobalCache.Set("Settings", sKeyName, sVal, DateTime.MaxValue);
            }
            catch { }

            return (string.IsNullOrEmpty(sVal) ? string.Empty : sVal);
        }

        public static void SetAppSetting(string sKeyName, string sKeyValue)
        {
            RAMCache.GlobalCache.Set("Settings", sKeyName, sKeyValue, DateTime.MaxValue);

            try
            {
                if (string.IsNullOrEmpty(sKeyValue))
                {
                    Utils.AppSettingKey.SetValue(sKeyName, string.Empty, Microsoft.Win32.RegistryValueKind.String);
                    Utils.AppSettingKey.DeleteValue(sKeyName);
                }
                else
                    Utils.AppSettingKey.SetValue(sKeyName, sKeyValue);
                return;
            }
            catch { }

            return;
        }

        public static string PluralWord(object val, string word1, string word2)
        {
            return (Utils.CInt(val) == 1) ? word1 : word2;
        }

        public static bool ValidNumber(object oNum)
        {
            if (oNum == null)
                return false;

            string sNum = "";

            try
            {
                sNum = oNum.ToString();
                sNum = sNum.Replace("$", "").Replace("\"", "").Replace(".", "").Replace(",", "");
                sNum = sNum.Trim();
            }
            catch { }

            if (sNum.Length == 0)
                return false;

            try
            {
                if (!char.IsNumber(sNum, 0))
                    return false;

                if ((sNum[0] == '0') && (long.Parse(sNum) == 0))
                    return true;

                long l = 0;
                bool successful = long.TryParse(sNum, out l);
                if ((successful) && (l != 0))
                    return true;
            }
            catch { }

            return false;
        }

        public static bool ValidText(object oText)
        {
            if (oText == null) return false;
            if (oText.GetType() == typeof(string))
            {
                if (((string)oText).Trim().Length < 1)
                    return false;
                return true;
            }
            if (oText.ToString().Trim().Length < 1)
                return false;
            return true;
        }


        public static bool ValidDate(object oDate)
        {
            if (!ValidText(oDate)) return false;
            DateTime dt = SuperDateTime.Parse(oDate.ToString(), DateTime.MinValue);
            return ((dt != DateTime.MinValue) && (dt != DateTime.MaxValue) && (dt.Year < 3000));
        }

        public static bool GoodYear(Object oYear)
        {
            return Utils.CInt(Utils.SafeYear(oYear)) > 1900;
        }

        public static bool BadYear(Object oYear)
        {
            return !GoodYear(oYear);
        }

        public static string SafeYear(Object oYear)
        {
            try
            {
                if (oYear.GetType() == typeof(int))
                {
                    if (((int)oYear) < 100)
                        return (1900 + ((int)oYear)).ToString();
                    return ((int)oYear).ToString();
                }

                if (oYear.GetType() == typeof(string))
                {
                    if (((string)oYear).Length <= 4)
                        return (string)oYear;

                    if (((string)oYear).Contains("/"))
                        return ((string)oYear).Substring(0, 4);

                    try
                    {
                        return DateTime.Parse((string)oYear).Year.ToString();
                    }
                    catch
                    {
                        return Utils.CInt(((string)oYear).Substring(0, 4)).ToString();
                    }
                }
            }
            catch { }

            return "1900";
        }

        // Quick, but lame conversion of accented characters
        public static string MapInternationals(string sText)
        {
            sText = sText.Replace("À", "A");
            sText = sText.Replace("Â", "A");
            sText = sText.Replace("Ã", "A");
            sText = sText.Replace("Ä", "A");
            sText = sText.Replace("Å", "A");
            sText = sText.Replace("Æ", "Ae");
            sText = sText.Replace("Ç", "C");
            sText = sText.Replace("È", "E");
            sText = sText.Replace("É", "E");
            sText = sText.Replace("Ê", "E");
            sText = sText.Replace("Ë", "E");
            sText = sText.Replace("Ì", "I");
            sText = sText.Replace("Í", "I");
            sText = sText.Replace("Î", "I");
            sText = sText.Replace("Ï", "I");
            sText = sText.Replace("Ð", "D");
            sText = sText.Replace("Ñ", "N");
            sText = sText.Replace("Ò", "O");
            sText = sText.Replace("Ó", "O");
            sText = sText.Replace("Ô", "O");
            sText = sText.Replace("Õ", "O");
            sText = sText.Replace("Ö", "O");
            sText = sText.Replace("×", "x");
            sText = sText.Replace("Ø", "O");
            sText = sText.Replace("Ù", "U");
            sText = sText.Replace("Ú", "U");
            sText = sText.Replace("Û", "U");
            sText = sText.Replace("Ü", "U");
            sText = sText.Replace("Ý", "Y");
            sText = sText.Replace("ß", "B");
            sText = sText.Replace("à", "a");
            sText = sText.Replace("á", "a");
            sText = sText.Replace("â", "a");
            sText = sText.Replace("ã", "a");
            sText = sText.Replace("ä", "a");
            sText = sText.Replace("å", "a");
            sText = sText.Replace("æ", "ae");
            sText = sText.Replace("ç", "c");
            sText = sText.Replace("è", "e");
            sText = sText.Replace("é", "e");
            sText = sText.Replace("ê", "e");
            sText = sText.Replace("ë", "e");
            sText = sText.Replace("ì", "i");
            sText = sText.Replace("í", "i");
            sText = sText.Replace("î", "i");
            sText = sText.Replace("ï", "i");
            sText = sText.Replace("ð", "s");
            sText = sText.Replace("ñ", "n");
            sText = sText.Replace("ò", "o");
            sText = sText.Replace("ó", "o");
            sText = sText.Replace("ô", "o");
            sText = sText.Replace("õ", "o");
            sText = sText.Replace("ö", "o");
            sText = sText.Replace("ø", "o");
            sText = sText.Replace("ù", "u");
            sText = sText.Replace("ú", "u");
            sText = sText.Replace("û", "u");
            sText = sText.Replace("ü", "u");
            sText = sText.Replace("ý", "y");
            sText = sText.Replace("ÿ", "y");

            return sText;
        }

        public static string CondenseSpacing(string s)
        {
            if (s == null)
                return string.Empty;

            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s;
        }

        public static string PrepTextForComparison(string sTitle)
        {
            sTitle = Utils.MapInternationals(sTitle).ToLower();
            sTitle = System.Text.RegularExpressions.Regex.Replace(sTitle, @"\(*?\)", "").Trim();
            sTitle = sTitle + " ";
            sTitle = sTitle.Replace("_", " ");
            sTitle = sTitle.Replace(".", " ");
            sTitle = sTitle.Replace("  ", " ");
            sTitle = sTitle.Replace("  ", " ");
            sTitle = sTitle.Replace("  ", " ");
            sTitle = sTitle.Replace("  ", " ");
            if (sTitle.StartsWith("the ")) sTitle = sTitle.Substring(4);
            if (sTitle.StartsWith("a ")) sTitle = sTitle.Substring(2);
            if (sTitle.StartsWith("an ")) sTitle = sTitle.Substring(3);
            if (sTitle.StartsWith("and ")) sTitle = sTitle.Substring(4);
            if (sTitle.EndsWith(", the ")) sTitle = sTitle.Substring(0, sTitle.Length - 6) + " ";
            if (sTitle.EndsWith(", a ")) sTitle = sTitle.Substring(0, sTitle.Length - 4) + " ";
            if (sTitle.EndsWith(", an ")) sTitle = sTitle.Substring(0, sTitle.Length - 5) + " ";
            sTitle = sTitle.Replace(" the ", " ");
            sTitle = sTitle.Replace(" a ", " ");
            sTitle = sTitle.Replace(" an ", " ");
            sTitle = sTitle.Replace(" and ", " ");
            sTitle = sTitle.Replace(" of ", " ");
            sTitle = sTitle.Replace("!", " ");
            sTitle = sTitle.Replace("\"", " ");
            sTitle = sTitle.Replace("&", " ");
            sTitle = sTitle.Replace("²", " 2");
            sTitle = sTitle.Replace("³", " 3");
            sTitle = sTitle.Replace("·", " ");
            sTitle = sTitle.Replace("+", " ");
            sTitle = sTitle.Replace("'", "");
            sTitle = sTitle.Replace("?", " ");
            sTitle = sTitle.Replace("!", " ");
            sTitle = sTitle.Replace("#", " ");
            sTitle = sTitle.Replace("/", " ");
            sTitle = sTitle.Replace("(", "");
            sTitle = sTitle.Replace(")", "");
            sTitle = sTitle.Replace("[", "");
            sTitle = sTitle.Replace("]", "");
            sTitle = sTitle.Replace(":", " ");
            sTitle = sTitle.Replace(",", " ");
            sTitle = sTitle.Replace("  ", " ");
            sTitle = sTitle.Replace("  ", " ");
            sTitle = sTitle.Replace("  ", " ");
            sTitle = sTitle.Replace("  ", " ");

            return sTitle.Trim();
        }

        public static System.Drawing.Image GetImageFromBytes(byte[] bImage)
        {
            if (bImage == null) return null;
            if (bImage.Length == 0) return null;

            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(bImage, 0, bImage.Length);
                ms.Seek(0, SeekOrigin.Begin);
                Image i = Image.FromStream(ms);
                return i;
            }
            catch (Exception ex)
            {
                Utils.DebugLogger(Errors.GetAllErrorDetails(ex));
            }

            return null;
        }

        public static System.Drawing.Image LoadImageFromDisk(string sFileName)
        {
            if (!System.IO.File.Exists(sFileName))
                return new Bitmap(1, 1);

            if (new FileInfo(sFileName).Length <= 100)
                return new Bitmap(1, 1);

            FileStream fs = null;

            try
            {
                fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Image iTemp = System.Drawing.Image.FromStream(fs);
                Image iNew = (Image)iTemp.Clone();
                iTemp.Dispose();
                fs.Close();
                fs.Dispose();
                return iNew;
            }
            catch
            {
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                    catch { }
                }
            }

            return new Bitmap(1, 1);
        }

        public static void FitImageNicely(ref System.Windows.Forms.PictureBox picControl, System.Drawing.Image picture = null)
        {
            try
            {
                picControl.Image = Utils.ImageResizer(picture == null ? picControl.Image : picture, picControl.Width - 2, picControl.Height - 2);
            }
            catch { }
        }

        public static Font GetFontThatWillFit(Graphics gfxDrawingArea, string sText, int width, int height, string sFontName = "Arial", FontStyle fsStyle = FontStyle.Regular, float fStartingPointSize = 16.0f)
        {
            for (float new_size = fStartingPointSize; new_size >= 6.0f; new_size -= 0.1f)
            {
                Font temp_font = new Font(sFontName, new_size, fsStyle);
                
                SizeF bounds = SizeOfDrawnText(gfxDrawingArea, temp_font, sText);

                //Utils.Chatter("Bounds " + new_size.ToString("#.0") + "pt: " + bounds.Width.ToString("#,##0") + "x" + bounds.Height.ToString("#,##0"));

                if (bounds.Width <= (float)width)
                {
                    if (bounds.Height <= (float)height)
                    {
                        return temp_font;
                    }
                }
                
                temp_font.Dispose();
            }

            return new Font(sFontName, 6.0f);
        }

        public static SizeF SizeOfDrawnText(Graphics gfxDrawingArea, Font fntFont, string sText)
        {
            return gfxDrawingArea.MeasureString(sText, fntFont);
        }

        public static System.Drawing.Image ImageResizer(System.Drawing.Image image_in, double percentage)
        {
            try
            {
                int width = (int)(((double)image_in.Width) * percentage);
                int height = (int)(((double)image_in.Height) * percentage);
                Image iTemp = (Image)image_in.Clone();

                System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(width, height);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpResized);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                g.DrawImage(
                    iTemp,
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmpResized.Size),
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, iTemp.Size),
                    System.Drawing.GraphicsUnit.Pixel);

                iTemp.Dispose();
                g.Dispose();

                return bmpResized;
            }
            catch { }

            return image_in;
        }

        public static System.Drawing.Image ImageResizer(System.Drawing.Image image_in, int width, int height)
        {
            try
            {
                Image iTemp = (Image)image_in.Clone();

                System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(width, height);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpResized);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                g.DrawImage(
                    iTemp,
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmpResized.Size),
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, iTemp.Size),
                    System.Drawing.GraphicsUnit.Pixel);

                iTemp.Dispose();
                g.Dispose();

                return bmpResized;
            }
            catch { }

            return image_in;
        }

        public static System.Drawing.Image ImageOverlayer(System.Drawing.Image image1, System.Drawing.Image image2, System.Drawing.Point pImage2)
        {
            System.Drawing.Bitmap bmpRedrawn = new System.Drawing.Bitmap(image1.Width, image1.Height);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpRedrawn))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                g.DrawImage(
                    image1,
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, image1.Size), // Destination Rectangle
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, image1.Size), // Source Rectangle
                    System.Drawing.GraphicsUnit.Pixel);

                g.DrawImage(
                    image2,
                    new System.Drawing.Rectangle(pImage2.X, pImage2.Y, image2.Width, image2.Height), // Destination Rectangle
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, image2.Size), // Source Rectangle
                    System.Drawing.GraphicsUnit.Pixel);
            }
            return bmpRedrawn;
        }

        public static Image DownloadImage(string sURL)
        {
            byte[] b = new byte[] { };
            return DownloadImage(sURL, ref b);
        }

        public static Image DownloadImage(string sURL, ref byte[] b_output)
        {
            Image i = new Bitmap(1, 1);

            try
            {
                bool success = false;
                byte[] b = null;

                Comm.CommFetchOptions options = new Comm.CommFetchOptions() { WantCookies = true, TimeOut = 15 };

                for (int retries = 0; retries < 3; retries++)
                {
                    options.LastException = null;
                    b = Comm.Download(sURL, ref options);

                    if ((b.Length > 100) && (options.LastException == null))
                        break;
                }

                if (options.LastException != null)
                    throw options.LastException;

                #region Test that the image data is valid
                try
                {
                    using (MemoryStream ms_download_data = new MemoryStream(b))
                    {
                        ms_download_data.Position = 0L;

                        using (Bitmap bmp = new Bitmap(ms_download_data))
                        {
                            string message = string.Empty;

                            if ((bmp.Width > 10) && (bmp.Height > 10))
                            {
                                i = (Bitmap)(bmp.Clone());

                                success = true;
                            }
                        }
                    }

                    b_output = b;
                }
                catch
                {
                    if (!success) // ignore any GDI+ issues with disposed objects if we did manage to load the image correctly
                        throw;
                }
                #endregion
            }
            catch { }

            return i;
        }

        public static System.Drawing.Image LoadImageFromResource(string sResource)
        {
            System.Drawing.Image i = null;

            try
            {
                string strNameSpace = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                System.IO.Stream str = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(strNameSpace.Replace(" ", "") + "." + sResource);

                bool success = false;

                #region Test that the image data is valid
                try
                {
                    str.Position = 0L;

                    using (Bitmap bmp = new Bitmap(str))
                    {
                        string message = string.Empty;

                        if ((bmp.Width > 10) && (bmp.Height > 10))
                        {
                            i = (Bitmap)(bmp.Clone());

                            success = true;
                        }
                    }
                }
                catch
                {
                    if (!success) // ignore any GDI+ issues with disposed objects if we did manage to load the image correctly
                        throw;
                }
                #endregion
            }
            catch { }

            return i;
        }

        public static bool JObjectValid(JObject j)
        {
            try
            {
                if (j == null)
                    return false;

                if (!j.HasValues)
                    return false;

                try
                {
                    if (Utils.CInt(j["empty"]) == 1)
                        return false;
                }
                catch { }

                return true;
            }
            catch { }

            return false;
        }

        public static string Pluralize(string s, int count)
        {
            if (s.EndsWith("s")) return s;
            if (count == 1) return s;
            return s + "s";
        }

        public static DateTime Today(DateTime dtRef)
        {
            return new DateTime(dtRef.Year, dtRef.Month, dtRef.Day);
        }

        public static double ConvertTimeToMinutes(string sTime)
        {
            double t = 0.0;

            sTime = sTime.ToLower();

            try
            {
                sTime = sTime.Replace("a", " a");
                sTime = sTime.Replace("p", " p");
                sTime = Utils.CondenseSpacing(sTime).Trim();
                sTime = sTime.Replace(" ", ":");
                string[] sParts = Utils.SubStringsDups(sTime, ":");

                t += Utils.CDbl(sParts[0]) * 60.0;
                t += Utils.CDbl(sParts[1]);

                bool bIsAM = false;
                bool bIsPM = false;

                if (sParts.Length == 3)
                {
                    if (sParts[2].StartsWith("p")) bIsPM = true;
                    if (sParts[2].StartsWith("a")) bIsAM = true;
                }

                if (sParts.Length == 4)
                {
                    if (sParts[3].StartsWith("p")) bIsPM = true;
                    if (sParts[3].StartsWith("a")) bIsAM = true;
                }

                if (Utils.CInt(sParts[0]) == 12 && bIsAM) t -= 12.0 * 60.0;
                if (Utils.CInt(sParts[0]) != 12 && bIsPM) t += 12.0 * 60.0;
            }
            catch { }
            
            return t;
        }

        public static DateTime GameEvent(DateTime dtRef, string sTime)
        {
            return Utils.Today(dtRef).AddMinutes(Utils.ConvertTimeToMinutes(sTime));
        }

        public static int CountListOccurance(IEnumerable<int> list, int val)
        {
            int ct = 0;

            foreach (int x in list)
                if (x == val)
                    ct++;

            return ct;
        }

        public static int CountListOccurance(IEnumerable<string> list, string val)
        {
            int ct = 0;

            foreach (string x in list)
                if (x == val)
                    ct++;

            return ct;
        }

        public static string GetMD5(string text)
        {
            return GetMD5(new UTF8Encoding().GetBytes(text));
        }

        public static string GetMD5(byte[] arr)
        {
            byte[] md5_array = System.Security.Cryptography.MD5.Create().ComputeHash(arr);

			string md5 = string.Empty;
			for (int i = 0; i < md5_array.Length; i++)
				md5 += Convert.ToString(md5_array[i], 16).PadLeft(2, '0');

            return md5.ToLower();
        }
    }
}
