using System;
using System.Collections.Generic;
using System.Data;

namespace EKUnleashed
{
    public class RAMCache
    {
        public Dictionary<string, Dictionary<string, RAMCacheObject>> Listings = new Dictionary<string, Dictionary<string, RAMCacheObject>>();

        public static RAMCache GlobalCache = new RAMCache();

        public class RAMCacheObject
        {
            public DateTime Expires = DateTime.Now;
            public object Data = null;

            public RAMCacheObject(DateTime _Expires, object _Data)
            {
                this.Expires = _Expires;
                this.Data = _Data;
            }

            ~RAMCacheObject()
            {
                Utils.DisposeObject(this.Data);
                this.Data = null;
            }
        }

        // does the listing itself exist?
        private bool ListingsContain(string _Name)
        {
            return (this.GetListing(_Name) != null);
        }

        // does the item exist int he listing?
        private bool ListingContains(string _ListingName, string _ValueName)
        {
            return (this.GetListingValue(_ListingName, _ValueName) != null);
        }

        private Dictionary<string, RAMCacheObject> GetListing(string _Name)
        {
            foreach (KeyValuePair<string, Dictionary<string, RAMCacheObject>> rc in this.Listings)
                if (rc.Key == _Name)
                    return rc.Value;

            return null;
        }

        private object GetListingValue(string _ListingName, string _ValueName)
        {
            try
            {
                foreach (KeyValuePair<string, Dictionary<string, RAMCacheObject>> rc in this.Listings)
                {
                    if (rc.Key == _ListingName)
                    {
                        lock (this.Listings)
                        {
                            foreach (KeyValuePair<string, RAMCacheObject> value in rc.Value)
                            {
                                if (value.Key == _ValueName)
                                {
                                    return value.Value.Data;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        public void AddListing(string _Name)
        {
            if (!this.ListingsContain(_Name))
                this.Listings.Add(_Name, new Dictionary<string, RAMCacheObject>());
        }

        public object Get(string _ListingName, string _ValueName)
        {
            try
            {
                foreach (KeyValuePair<string, Dictionary<string, RAMCacheObject>> rc in this.Listings)
                {
                    if (rc.Key == _ListingName)
                    {
                        lock (this.Listings)
                        {
                            if (rc.Value.ContainsKey(_ValueName))
                            {
                                if (rc.Value[_ValueName].Expires.CompareTo(DateTime.Now) > 0)
                                    return rc.Value[_ValueName].Data;

                                Utils.DisposeObject(rc.Value[_ValueName].Data);
                                rc.Value[_ValueName].Data = null;
                                rc.Value.Remove(_ValueName);
                                return null;
                            }
                        }

                        return null;
                    }
                }
            }
            catch { }

            return null;
        }

        public void Set(string _ListingName, string _ValueName, object _Data)
        {
            this.Set(_ListingName, _ValueName, _Data, DateTime.Now.AddSeconds(15));
            return;
        }
        public void Set(string _ListingName, string _ValueName, object _Data, DateTime _Expires)
        {
            lock (this.Listings)
            {
                foreach (KeyValuePair<string, Dictionary<string, RAMCacheObject>> rc in this.Listings)
                {
                    if (rc.Key == _ListingName)
                    {
                        if (_Data == null)
                        {
                            if (rc.Value.ContainsKey(_ValueName))
                                rc.Value.Remove(_ValueName);

                            return;
                        }

                        if (rc.Value.ContainsKey(_ValueName))
                        {
                            rc.Value[_ValueName].Expires = _Expires;
                            rc.Value[_ValueName].Data = _Data;
                            return;
                        }

                        rc.Value.Add(_ValueName, new RAMCacheObject(_Expires, _Data));
                        return;
                    }
                }

                this.AddListing(_ListingName);
                this.GetListing(_ListingName).Add(_ValueName, new RAMCacheObject(_Expires, _Data));
            }

            return;
        }

        public void DestroyListing(string _Name)
        {
            string rc_to_remove = null;

            lock (this.Listings)
            {
                foreach (KeyValuePair<string, Dictionary<string, RAMCacheObject>> rc in this.Listings)
                {
                    if (rc.Key == _Name)
                    {
                        rc_to_remove = rc.Key;
                        break;
                    }
                }

                if (rc_to_remove != null)
                {
                    try
                    {
                        foreach (KeyValuePair<string, Dictionary<string, RAMCacheObject>> rc in this.Listings)
                        {
                            if (rc.Key == rc_to_remove)
                            {
                                while (rc.Value.Count > 0)
                                {
                                    string sGetAKey = string.Empty;

                                    foreach (string x in rc.Value.Keys)
                                    {
                                        sGetAKey = x;
                                        break;
                                    }

                                    Utils.DisposeObject(rc.Value[sGetAKey].Data);
                                    rc.Value[sGetAKey].Data = null;
                                    rc.Value.Remove(sGetAKey);
                                }
                            }
                        }
                    }
                    catch { }

                    this.Listings.Remove(rc_to_remove);
                }
            }

            return;
        }

        public static void ClearAll()
        {
            try
            {
                while (RAMCache.GlobalCache.Listings.Count > 0)
                {
                    string ListingName = string.Empty;

                    foreach (KeyValuePair<string, Dictionary<string, RAMCache.RAMCacheObject>> rc in RAMCache.GlobalCache.Listings)
                    {
                        ListingName = rc.Key;
                        break;
                    }

                    RAMCache.GlobalCache.DestroyListing(ListingName);
                }
            }
            catch { }
        }
    }
}
