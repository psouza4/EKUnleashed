using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Web;
using System.Collections;

namespace EKUnleashed
{
    public class Comm
    {
        private static bool WantToSeePageFetches = false;

        public static bool SetAllowUnsafeHeaderParsing20(bool bNewSetting)
        {
            try
            {
                // Get the assembly that contains the internal class
                Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));

                if (aNetAssembly != null)
                {
                    // Get the internal type for the internal class.
                    Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");

                    if (aSettingsType != null)
                    {
                        // Use the internal static property to get an instance of the internal settings class.
                        // If the static instance isn't created already, the property will create it for us.
                        object anInstance = aSettingsType.InvokeMember("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                        if (anInstance != null)
                        {
                            // Locate the private bool field that tells the framework if unsafe header parsing should be allowed or not.
                            FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);

                            if (aUseUnsafeHeaderParsing != null)
                            {
                                aUseUnsafeHeaderParsing.SetValue(anInstance, bNewSetting);
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        public class CommFetchOptions
        {
            public bool DataType_JSON = false;
            public string Auth_Username = string.Empty;
            public string Auth_Password = string.Empty;
            public bool XMLHttpRequest = false;

            public string Accept = string.Empty;

            public bool WantErrors = true;

            public Exception LastException = null;

            /// <summary>
            /// Timeout, in seconds (default is 30).
            /// </summary>
            public int TimeOut = 30;

            public enum HTTP_Versions
            {
                /// <summary>
                /// HTTP/1.0
                /// </summary>
                Version_1p0,

                /// <summary>
                /// HTTP/1.1
                /// </summary>
                Version_1p1
            }

            /// <summary>
            /// Which version of HTTP to use (1.0 or 1.1).
            /// </summary>
            public HTTP_Versions HTTP_Version = HTTP_Versions.Version_1p1;

            public bool SkipHEAD = true;

            public enum Referers
            {
                /// <summary>
                /// No referer data will be sent.
                /// </summary>
                None,

                /// <summary>
                /// The full URL to fetch will be sent as the URL
                /// </summary>
                Full_URL,

                /// <summary>
                /// The host part of the URL to fetch will be sent as the referring URL.
                /// </summary>
                Host_Only,

                /// <summary>
                /// Custom URL (see CustomRefererURL string property)
                /// </summary>
                Custom
            }

            public NetworkCredential Credentials = null;

            /// <summary>
            /// How to send the referer.
            /// </summary>
            public Referers Referer = Referers.Full_URL;

            /// <summary>
            /// Custom URL to use for the referer (Referer must be set to Referers.Custom).
            /// </summary>
            public string CustomRefererURL = string.Empty;

            /// <summary>
            /// Whether or not to automatically decompress gzip-compressed data.
            /// </summary>
            public bool AutomaticDecompress = true;

            /// <summary>
            /// If set to true, the return will be a byte array representing an ASCII string of how big the resulting
            /// data would be (though it may be larger after decompression).
            /// </summary>
            public bool SizeOnlyMode = false;

            public enum UserAgentModes
            {
                /// <summary>
                /// Normal user agent (Google Chrome v27 / Windows 7 x64).  This is the default.
                /// </summary>
                Chrome,

                /// <summary>
                /// Internet Explorer user agent (Internet Explorer v9 / Windows 7 x64).
                /// </summary>
                IE9,

                /// <summary>
                /// Adobe Air (mobile)
                /// </summary>
                AIR,

                Dalvik
            }

            /// <summary>
            /// Which user agent (browser name/version) to use.
            /// </summary>
            public UserAgentModes UserAgent = UserAgentModes.Chrome;

            /// <summary>
            /// Cookies tracking.  Only allowed when WantCookies property is set to true.
            /// </summary>
            public HttpCookieCollection Cookies = new HttpCookieCollection();

            public CookieContainer CookieContainerGet()
            {
                CookieContainer cc = new CookieContainer();
                if (this.Cookies.Count > 0)
                {
                    for (int i = 0; i < this.Cookies.Count; i++)
                    {
                        Cookie c = new Cookie();
                        try { c.Name = this.Cookies[i].Name.ToString(); } catch { }
                        try { c.Value = this.Cookies[i].Value.ToString(); } catch { }
                        try { c.Path = this.Cookies[i].Path.ToString(); } catch { }
                        try { c.Domain = this.Cookies[i].Domain.ToString(); } catch { }
                        try { c.Expires = this.Cookies[i].Expires; } catch { } 

                        try { cc.Add(c); } catch { }
                    }
                }
                return cc;
            }

            public static CookieCollection GetAllCookies(CookieContainer cookieJar)
            {
                CookieCollection cookieCollection = new CookieCollection();

                Hashtable table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable",
                                                                                BindingFlags.NonPublic |
                                                                                BindingFlags.GetField |
                                                                                BindingFlags.Instance,
                                                                                null,
                                                                                cookieJar,
                                                                                new object[] { });

                foreach (var tableKey in table.Keys)
                {
                    String str_tableKey = (string)tableKey;

                    if (str_tableKey[0] == '.')
                    {
                        str_tableKey = str_tableKey.Substring(1);
                    }

                    SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                                BindingFlags.NonPublic |
                                                                                BindingFlags.GetField |
                                                                                BindingFlags.Instance,
                                                                                null,
                                                                                table[tableKey],
                                                                                new object[] { });

                    foreach (var listKey in list.Keys)
                    {
                        String url = "https://" + str_tableKey + (string)listKey;
                        cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
                    }
                }

                return cookieCollection;
            }
            public void CookieContainerSet(CookieContainer cc)
            {
                this.Cookies.Clear();

                foreach (Cookie cookie in GetAllCookies(cc))
                    CookieContainerAdd(cookie);
                return;
            }

            public void CookieContainerAdd(Cookie cookie)
            {
                try
                {
                    this.Cookies.Add(new HttpCookie(cookie.Name, cookie.Value) { Expires = cookie.Expires, Path = cookie.Path, Domain = cookie.Domain });
                }
                catch
                {
                    try
                    {
                        this.Cookies[cookie.Name].Value = cookie.Value;
                        this.Cookies[cookie.Name].Expires = cookie.Expires;
                        this.Cookies[cookie.Name].Domain = cookie.Domain; // this may not be right, we may need to check if the domain matches, etc.
                        this.Cookies[cookie.Name].Path = cookie.Path;
                    }
                    catch { }
                } return;
            }

            /// <summary>
            /// Whether or not to allow cookie collection.
            /// </summary>
            public bool WantCookies = false;


            public enum Methods
            {
                GET,
                POST
            }

            public Methods Method = Methods.GET;

            public string POST_Data = string.Empty;

        }

        // WebClient download method
        private static byte[] Download_WC(string sURL, ref CommFetchOptions options)
        {
            return (new DownloadThread(sURL, ref options)).data;
        }

        public class DownloadThread
        {
            public CommFetchOptions opts = new CommFetchOptions();
            private string URL = "";
            public byte[] data = null;

            public DownloadThread(string URL_in, ref CommFetchOptions opts_in)
            {
                this.URL = URL_in;
                this.opts = opts_in;

                try
                {
                    ThreadStart tsGenericMethod = new ThreadStart(this.Go);
                    Thread trdGenericThread = new Thread(tsGenericMethod);
                    trdGenericThread.IsBackground = true;
                    trdGenericThread.Start();

                    DateTime dtStartTime = DateTime.Now;

                    for (; ; )
                    {
                        if (trdGenericThread.ThreadState == System.Threading.ThreadState.Stopped) break;
                        if (trdGenericThread.ThreadState == System.Threading.ThreadState.StopRequested) break;
                        if (trdGenericThread.ThreadState == System.Threading.ThreadState.Aborted) break;
                        if (trdGenericThread.ThreadState == System.Threading.ThreadState.AbortRequested) break;

                        Thread.Sleep(15);
                        frmMain.DoEvents();
                    }
                }
                catch { }

                opts_in = this.opts;
            }

            public void Go()
            {
                try
                {
                    this.data = Comm.Download_WC_real(this.URL, ref this.opts);
                }
                catch (Exception ex)
                {
                    if (this.opts.WantErrors)
                        this.opts.LastException = ex;
                    this.data = Utils.CByteASCII("[" + Errors.GetShortErrorDetails(ex) + "]");
                }
            }
        }

        private static string _RandomDalvikString = "";
        private static string RandomDalvikString
        {
            get
            {
                if (Utils.ValidText(Comm._RandomDalvikString))
                    return Comm._RandomDalvikString;

                // Updated Oct 17, 2015
                Comm._RandomDalvikString = "Dalvik/2." + Utils.RandomDice(1, 2, 0).ToString() + ".0 (Linux; U; Android 5." + Utils.RandomDice(1, 2, -1).ToString() + "; SM-N" + Utils.RandomDice(1, 900, 99).ToString() + "V Build/LRX" + Utils.RandomDice(1, 90, 9).ToString() + "V)";
                return Comm._RandomDalvikString;
            }
        }

        private static byte[] Download_WC_real(string sURL, ref CommFetchOptions options)
        {
            WebRequest oReq;
            HttpWebRequest ohReq;
            Uri url = new Uri(sURL);
            byte[] bContent = null;
            bool bReceivedUnicodeEncoding = false;

            if (options.DataType_JSON)
                Comm.SetAllowUnsafeHeaderParsing20(true);
            System.Net.ServicePointManager.Expect100Continue = false;

            try
            {
                oReq = WebRequest.Create(sURL);
                ohReq = (HttpWebRequest)oReq;

                if (string.IsNullOrEmpty(options.Accept))
                    ohReq.Accept = "*/*";
                else
                    ohReq.Accept = options.Accept;

                if (options.Method == CommFetchOptions.Methods.POST)
                    oReq.Method = "POST";
                else
                    oReq.Method = "GET";

                oReq.Headers.Add("Accept-Language: en-US,en;q=0.8");
                oReq.Headers.Add("Accept-Encoding: gzip,deflate");
                oReq.Headers.Add("Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3");
                if (options.HTTP_Version == CommFetchOptions.HTTP_Versions.Version_1p0)
                    ohReq.ProtocolVersion = System.Net.HttpVersion.Version10;
                else
                    ohReq.ProtocolVersion = System.Net.HttpVersion.Version11;
                if (options.UserAgent == CommFetchOptions.UserAgentModes.IE9)
                    ohReq.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                else if (options.UserAgent == CommFetchOptions.UserAgentModes.AIR)
                    ohReq.UserAgent = "Mozilla/5.0 (Android; U; en-US) AppleWebKit/533.19.4 (KHTML, like Gecko) AdobeAIR/4.0";
                else if (options.UserAgent == CommFetchOptions.UserAgentModes.Dalvik)
                    ohReq.UserAgent = Comm.RandomDalvikString;
                else
                    ohReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36";

                for (int i = 0; i < options.Cookies.Count; i++)
                    Utils.Logger("Cookie sent: " + options.Cookies[i].Name + "=" + options.Cookies[i].Value.ToString());

                for (int i = 0; i < options.Cookies.Count; i++)
                {
                    if (options.Cookies[i].Name.ToLower() == "csrftoken")
                    {
                        Utils.Logger("Request header added: X-Requested-With: XMLHttpRequest");
                        oReq.Headers.Add("X-Requested-With: XMLHttpRequest");

                        Utils.Logger("Request header added: X-CSRFToken=" + options.Cookies[i].Value.ToString());
                        ohReq.Headers.Add("X-CSRFToken", options.Cookies[i].Value.ToString());
                        break;
                    }
                }

                if (options.DataType_JSON)
                {
                    ohReq.Accept = "application/json, text/javascript, */*; q=0.01";
                    Utils.Logger("Request data type switched to JSON");
                }

                if (options.XMLHttpRequest)
                    oReq.Headers.Add("X-Requested-With: XMLHttpRequest");

                if (options.UserAgent == CommFetchOptions.UserAgentModes.Dalvik)
                    oReq.Headers.Add("X-Unity-Version: 4.5.4f1");


                ohReq.KeepAlive = true;

                oReq.Proxy = System.Net.WebRequest.DefaultWebProxy;

                if (!string.IsNullOrEmpty(options.Auth_Username))
                    oReq.Credentials = new NetworkCredential(options.Auth_Username, options.Auth_Password);

                if (options.Credentials != null)
                    oReq.Credentials = options.Credentials;

                if (options.Referer == CommFetchOptions.Referers.Full_URL)
                    ohReq.Referer = sURL;
                else if (options.Referer == CommFetchOptions.Referers.Host_Only)
                    ohReq.Referer = "http://" + url.Host + "/";
                else if (options.Referer == CommFetchOptions.Referers.Custom)
                    ohReq.Referer = options.CustomRefererURL;
                //else if (options.Referer == CommFetchOptions.Referers.None)
                //    ; // no referer

                if (options.Method == CommFetchOptions.Methods.POST)
                {
                    ohReq.ContentLength = (long)(Utils.CByteUTF8(options.POST_Data).Length);

                    if (options.DataType_JSON)
                    {
                        ohReq.ContentType = "application/json";
                        //ohReq.ContentType = "application/json, text/javascript, */*; q=0.01";
                    }
                    else
                        ohReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                }
                else
                {
                    oReq.Headers.Add("Pragma: no-cache");
                    oReq.Headers.Add("Cache-Control: max-age=0");
                }

                ohReq.CookieContainer = options.CookieContainerGet();

                oReq.Timeout = (options.TimeOut * 1000);
                try
                {
                    ohReq.Host = url.Host;
                }
                catch { }
                ohReq.MaximumAutomaticRedirections = 3;

                ohReq.AutomaticDecompression = (options.AutomaticDecompress) ? DecompressionMethods.GZip | DecompressionMethods.Deflate : DecompressionMethods.None;

                if (options.Method == CommFetchOptions.Methods.POST)
                {
                    Stream oDataStream = oReq.GetRequestStream();
                    oDataStream.Write(Utils.CByteUTF8(options.POST_Data), 0, Utils.CByteUTF8(options.POST_Data).Length);
                    oDataStream.Close();

                    if (Comm.WantToSeePageFetches)
                        if (options.DataType_JSON)
                            Utils.Logger("<fs-><color=#565656>[" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + "." + DateTime.Now.Millisecond.ToString("000") + "]</color><fx>" +
                                " <fs-><color=#5656aa><b>Posted data:</b><fs+>  " + options.POST_Data);
                }

                WebResponse oResp = oReq.GetResponse();
                HttpWebResponse ohResp = (HttpWebResponse)oResp;

                foreach (Cookie cookie in ohResp.Cookies)
                    options.CookieContainerAdd(cookie);

                for (int i = 0; i < oResp.Headers.Count; i++)
                {
                    string header = oResp.Headers.Keys[i];
                    string value = oResp.Headers.Get(i);

                    if (header == "Content-Type")
                    {
                        value = value.ToLower();

                        if (value.StartsWith("text/html"))
                        {
                            // assume unicode
                            bReceivedUnicodeEncoding = true;
                        }
                        else
                        {
                            if (value.ToLower().Contains("charset=utf-8"))
                                bReceivedUnicodeEncoding = true;
                        }
                    }
                }

                if (bReceivedUnicodeEncoding)
                {
                    string sContent = "";

                    using (StreamReader oSRead = new StreamReader(oResp.GetResponseStream(), Encoding.UTF8, true))
                    {
                        sContent = oSRead.ReadToEnd();

                        oSRead.Close();
                    }

                    bContent = Utils.CByteUTF8(sContent);
                }
                else
                {
                    BinaryReader oSRead = new BinaryReader(oResp.GetResponseStream());
                    List<byte> data = new List<byte>();

                    for (; ; ) { try { data.Add(oSRead.ReadByte()); } catch { break; } }
                    oSRead.Close();

                    bContent = data.ToArray();

                    data.Clear();
                }

                oResp.Close();
            }
            catch (TimeoutException ex)
            {
                options.LastException = ex;

                if (options.WantErrors)
                    return Utils.CByteUTF8("[timeout]");

                return null;
            }
            catch (Exception ex)
            {
                options.LastException = ex;

                if (options.WantErrors)
                    return Utils.CByteUTF8("[exception: " + ex.Message + "]");

                return null;
            }

            if (!options.AutomaticDecompress)
            {
                return bContent;
            }

            else
            {
                byte[] b = Comm.DecompressBytes(bContent);

                if (Comm.WantToSeePageFetches)
                    if (options.DataType_JSON)
                        Utils.Logger("<fs-><color=#565656>[" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + "." + DateTime.Now.Millisecond.ToString("000") + "]</color><fx>" +
                            " <fs-><color=#5656aa><b>Received data:</b><fs+>  " + Utils.CStr(b));


                //Utils.Logger("Notice: server returned UTF-8 (we requested UTF-8)");
                return b;
            }
        }

        public static byte[] Download(string sURL)
        {
            return Download(sURL, new CommFetchOptions());
        }
        public static byte[] Download(string sURL, CommFetchOptions options)
        {
            return Download_WC(sURL, ref options);
        }

        public static byte[] Download(string sURL, ref CommFetchOptions options)
        {
            return Download_WC(sURL, ref options);
        }

        public static string PageFetch(string sURL, Comm.CommFetchOptions options)
        {
            options.WantCookies = true;
            return Utils.CStr(Comm.Download(sURL, options));
        }

        public static string PageFetch(string sURL)
        {
            return Utils.CStr(Comm.Download(sURL, new Comm.CommFetchOptions() { WantCookies = true }));
        }

        public static string PrettyTimeLeft(Int64 iTotalSeconds)
        {
            if (iTotalSeconds < 0)
                return string.Empty;

            if (iTotalSeconds >= (30 * 24 * 60 * 60)) // more than a month?
                return string.Empty;

            if (iTotalSeconds >= (24 * 60 * 60))
            {
                Int64 iDays = iTotalSeconds / (24 * 60 * 60);
                Int64 iHours = (iTotalSeconds / (60 * 60)) - (iDays * 24);
                return iDays.ToString("#,##0") + " day" + ((iDays == 1) ? string.Empty : "s") + ", " +
                    iHours.ToString("#,##0") + " hour" + ((iHours == 1) ? string.Empty : "s");
            }

            if (iTotalSeconds >= (60 * 60))
            {
                Int64 iHours = iTotalSeconds / (60 * 60);
                Int64 iMinutes = (iTotalSeconds / (60)) - (iHours * 60);
                return iHours.ToString("#,##0") + " hour" + ((iHours == 1) ? string.Empty : "s") + ", " +
                    iMinutes.ToString("#,##0") + " minute" + ((iMinutes == 1) ? string.Empty : "s");
            }

            if (iTotalSeconds >= 60)
            {
                Int64 iMinutes = iTotalSeconds / 60;
                Int64 iSeconds = (iTotalSeconds) - (iMinutes * 60);
                return iMinutes.ToString("#,##0") + " minute" + ((iMinutes == 1) ? string.Empty : "s") + ", " +
                    iSeconds.ToString("#,##0") + " second" + ((iSeconds == 1) ? string.Empty : "s");
            }

            return iTotalSeconds.ToString() + " second" + ((iTotalSeconds == 1) ? string.Empty : "s");
        }

        public static string PrettyTimeLeftShort(Int64 iTotalSeconds)
        {
            if (iTotalSeconds < 0)
                return string.Empty;

            if (iTotalSeconds >= (30 * 24 * 60 * 60)) // more than a month?
                return string.Empty;

            if (iTotalSeconds >= (24 * 60 * 60))
            {
                Int64 iDays = iTotalSeconds / (24 * 60 * 60);
                Int64 iHours = (iTotalSeconds / (60 * 60)) - (iDays * 24);
                return iDays.ToString("#,##0") + "d, " +
                    iHours.ToString("#,##0") + "h";
            }

            if (iTotalSeconds >= (60 * 60))
            {
                Int64 iHours = iTotalSeconds / (60 * 60);
                Int64 iMinutes = (iTotalSeconds / (60)) - (iHours * 60);
                return iHours.ToString("#,##0") + "h, " +
                    iMinutes.ToString("#,##0") + "m";
            }

            if (iTotalSeconds >= 60)
            {
                Int64 iMinutes = iTotalSeconds / 60;
                Int64 iSeconds = (iTotalSeconds) - (iMinutes * 60);
                return iMinutes.ToString("#,##0") + "m, " +
                    iSeconds.ToString("#,##0") + "s";
            }

            return iTotalSeconds.ToString() + "s";
        }


        private static byte[] ToByteArray(Stream stream)
        {
            List<byte> result = new List<byte>();

            byte[] buffer = new byte[0x20000];
            int bytes = 0;

            while ((bytes = stream.Read(buffer, 0, 0x20000)) > 0)
                for (int i = 0; i < bytes; i++)
                    result.Add(buffer[i]);

            buffer = result.ToArray();

            result.Clear();

            return buffer;
        }

        // thank you: http://www.dotnetperls.com/decompress
        public static byte[] DecompressBytes(byte[] gzip)
        {
            try
            {
                byte[] result;

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(gzip, 0, gzip.Length);
                    ms.Position = 0L;

                    using (System.IO.Compression.GZipStream stream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress, true))
                    {
                        result = ToByteArray(stream);
                    }
                }

                if (result == null)
                    throw new Exception("Result was null in GZip decompress.");

                return result;
            }
            catch (InvalidDataException /* ex */)
            {
                //Utils.Logger(ex.GetType().ToString() + ": " + ex.Message);
                return gzip;
            }
            catch // (Exception ex)
            {
                //Utils.Logger(ex.GetType().ToString() + ": " + ex.Message);

                try
                {
                    using (System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(new MemoryStream(gzip), System.IO.Compression.CompressionMode.Decompress))
                    {
                        const int size = 4096;
                        byte[] buffer = new byte[size];
                        using (MemoryStream memory = new MemoryStream())
                        {
                            int count = 0;

                            do
                            {
                                count = stream.Read(buffer, 0, size);

                                if (count > 0)
                                    memory.Write(buffer, 0, count);
                            }
                            while (count > 0);

                            return memory.ToArray();
                        }
                    }
                }
                catch
                {
                    return gzip;
                }
            }
        }
    }
}