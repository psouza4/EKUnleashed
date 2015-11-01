using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EKUnleashed
{
    public class Scheduler
    {
        public enum ScheduleType
        {
            None,
            Once,
            Interval
        }

        public class ScheduledEvent
        {
            private Action _Target = null;
            private DateTime _Start = DateTime.MinValue;
            private ScheduleType _Type = ScheduleType.None;
            private DateTime _LastFired = DateTime.MinValue;
            private bool _Enabled = true;
            private TimeSpan _Interval = new TimeSpan(0, 0, 0);
            private Thread trdEvent = null;
            private object locker = new object();

            public DateTime NextScheduled
            {
                get
                {
                    return this._Start;
                }
            }

            private void Go()
            {
                // if this is an interval event and the start time has already elapsed, then set it to the next scheduled start time
                if (this._Type == ScheduleType.Interval)
                {
                    while (this._Start.CompareTo(GameClient.DateTimeNow) < 0)
                    {
                        //Utils.Chatter("Event already started (" + this._Start.ToString() + ") so fast forwarding by " + this._Interval.ToString() + "...");
                        this._Start += this._Interval;
                        //Utils.Chatter("... event will now start at " + this._Start.ToString() + ".");
                    }
                }

                ThreadStart tsGenericMethod = new ThreadStart(() =>
                {
                    try
                    {
                        for (; ; Thread.Sleep(200))
                        {
                            if (!this._Enabled)
                                return;

                            if (this._Type == ScheduleType.Once)
                            {
                                if (this._Start.CompareTo(GameClient.DateTimeNow) < 0)
                                {
                                    this._LastFired = GameClient.DateTimeNow;
                                    try
                                    {
                                        if (EventAllowed(this.EventID))
                                            this._Target();
                                    }
                                    catch (ThreadAbortException)
                                    {
                                        throw;
                                    }
                                    catch (ThreadInterruptedException)
                                    {
                                        throw;
                                    }
                                    catch { }
                                    return;
                                }
                                else
                                {
                                    TimeSpan remaining_to_first_start = this._Start - GameClient.DateTimeNow;

                                    if (remaining_to_first_start.TotalSeconds > 1.0)
                                        Thread.Sleep(remaining_to_first_start.Subtract(new TimeSpan(0, 0, 0, 0, 600)));
                                }
                            }
                            else if (this._Type == ScheduleType.Interval)
                            {
                                if (this._Start.CompareTo(GameClient.DateTimeNow) < 0)
                                {
                                    if (this._LastFired == DateTime.MinValue)
                                    {
                                        //Utils.Chatter("Event firing at " + EKClient.DateTimeNow.ToString() + "...");

                                        try
                                        {
                                            if (EventAllowed(this.EventID))
                                                this._Target();
                                        }
                                        catch (ThreadAbortException)
                                        {
                                            throw;
                                        }
                                        catch (ThreadInterruptedException)
                                        {
                                            throw;
                                        }
                                        catch { }

                                        this._LastFired = this._Start;

                                        //Utils.Chatter("... event will now start at " + (this._LastFired + this._Interval).ToString() + " (+" + this._Interval.ToString() + ").");

                                        if (this._Interval.TotalSeconds > 1.0)
                                            Thread.Sleep(this._Interval.Subtract(new TimeSpan(0, 0, 0, 0, 600)));
                                    }
                                    else if (this._LastFired.CompareTo(GameClient.DateTimeNow - this._Interval) < 0)
                                    {
                                        //Utils.Chatter("Event firing at " + EKClient.DateTimeNow.ToString() + "...");

                                        try
                                        {
                                            if (EventAllowed(this.EventID))
                                                this._Target();
                                        }
                                        catch (ThreadAbortException)
                                        {
                                            throw;
                                        }
                                        catch (ThreadInterruptedException)
                                        {
                                            throw;
                                        }
                                        catch { }

                                        if (this._Interval.TotalMinutes == 15.0)
                                        {
                                            // hack: let arena battles be variable
                                            this._LastFired += new TimeSpan(0, Utils.PickNumberBetween(12, 19), Utils.PickNumberBetween(0, 59));
                                        }
                                        else
                                            this._LastFired += this._Interval;

                                        //Utils.Chatter("... event will now start at " + (this._LastFired + this._Interval).ToString() + " (+" + this._Interval.ToString() + ").");

                                        if (this._Interval.TotalSeconds > 1.0)
                                            Thread.Sleep(this._Interval.Subtract(new TimeSpan(0, 0, 0, 0, 600)));
                                    }
                                }
                                else
                                {
                                    TimeSpan remaining_to_first_start = this._Start - GameClient.DateTimeNow;

                                    if (remaining_to_first_start.TotalSeconds > 1.0)
                                        Thread.Sleep(remaining_to_first_start.Subtract(new TimeSpan(0, 0, 0, 0, 600)));
                                }
                            }
                        }
                    }
                    catch { }
                });

                this.trdEvent = new Thread(tsGenericMethod);
                this.trdEvent.IsBackground = true;
                this.trdEvent.Start();
            }

            public void Start()
            {
                this.Enabled = true;
            }

            public void End()
            {
                try
                {
                    if (this.trdEvent != null)
                        if (this.trdEvent.ThreadState == ThreadState.Running)
                            this.trdEvent.Abort();
                }
                catch { }

                this._Enabled = false;
            }

            private static List<string> _Allowed_EventIDs = new List<string>();

            public static List<string> Allowed_EventIDs
            {
                get
                {
                    return _Allowed_EventIDs;
                }
            }

            public static void AddAllowedEvent(string AllowedEventID)
            {
                try
                {
                    RemoveAllowedEvent("all");

                    if (!_Allowed_EventIDs.Contains(AllowedEventID))
                        _Allowed_EventIDs.Add(AllowedEventID);
                }
                catch { }

                return;
            }

            public static void RemoveAllowedEvent(string AllowedEventID)
            {
                try
                {
                    if (_Allowed_EventIDs.Contains(AllowedEventID))
                        _Allowed_EventIDs.Remove(AllowedEventID);
                }
                catch { }

                return;
            }

            public static void AllowAllEvents()
            {
                _Allowed_EventIDs.Clear();
                _Allowed_EventIDs.Add("all");
                return;
            }

            public static void DisallowAnyEvents()
            {
                _Allowed_EventIDs.Clear();
                return;
            }

            public static bool AllEventsAllowed
            {
                get
                {
                    return _Allowed_EventIDs.Contains("all");
                }
                set
                {
                    if (value == true)
                        AllowAllEvents();
                    else
                        DisallowAnyEvents();
                }
            }

            public static bool EventAllowed(string EventID)
            {
                if (AllEventsAllowed)
                    return true;

                return _Allowed_EventIDs.Contains(EventID);
            }

            public string EventID = "unnamed";

            public ScheduledEvent(Action __Target, DateTime __FireOn, bool AutoStart = true)
            {
                this._Target = __Target;
                this._Start = __FireOn;
                this._Type = ScheduleType.Once;

                if (AutoStart)
                    this.Go();
                else
                    this._Enabled = false;
            }

            public ScheduledEvent(string __EventID, Action __Target, DateTime __FireOn, bool AutoStart = true)
            {
                this.EventID = __EventID;
                this._Target = __Target;
                this._Start = __FireOn;
                this._Type = ScheduleType.Once;

                if (AutoStart)
                    this.Go();
                else
                    this._Enabled = false;
            }

            public ScheduledEvent(Action __Target, TimeSpan __Interval, bool AutoStart = true)
            {
                this._Target = __Target;
                this._Start = GameClient.DateTimeNow + __Interval;
                this._Interval = __Interval;
                this._Type = ScheduleType.Interval;

                if (AutoStart)
                    this.Go();
                else
                    this._Enabled = false;
            }

            public ScheduledEvent(string __EventID, Action __Target, TimeSpan __Interval, bool AutoStart = true)
            {
                this.EventID = __EventID;
                this._Target = __Target;
                this._Start = GameClient.DateTimeNow + __Interval;
                this._Interval = __Interval;
                this._Type = ScheduleType.Interval;

                if (AutoStart)
                    this.Go();
                else
                    this._Enabled = false;
            }

            public ScheduledEvent(Action __Target, DateTime __FireOn, TimeSpan __Interval, bool AutoStart = true)
            {
                this._Target = __Target;
                this._Start = __FireOn;
                this._Interval = __Interval;
                this._Type = ScheduleType.Interval;

                if (AutoStart)
                    this.Go();
                else
                    this._Enabled = false;
            }

            public ScheduledEvent(string __EventID, Action __Target, DateTime __FireOn, TimeSpan __Interval, bool AutoStart = true)
            {
                this.EventID = __EventID;
                this._Target = __Target;
                this._Start = __FireOn;
                this._Interval = __Interval;
                this._Type = ScheduleType.Interval;

                if (AutoStart)
                    this.Go();
                else
                    this._Enabled = false;
            }

            public bool Enabled
            {
                get
                {
                    return this._Enabled;
                }
                set
                {
                    this._Enabled = value;

                    if (this._Enabled)
                        this.Go();
                }
            }
        }
    }
}
