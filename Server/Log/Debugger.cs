using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Times.Server.Log
{
    using Utils.Events;

    class Debugger
    {

        /// <summary>
        /// Stores the logging-process available on basis of Events called!
        /// </summary>
        static Dictionary<string, List<int>> Loggers = new Dictionary<string, List<int>> { }; // int = index in Logs.
        static List<Delegate> Logs = new List<Delegate> { };

        public Debugger()
        {
            
        }

        public static void CallEvent(string Event, params dynamic[] args)
        {
            object[] Params = args;

            if (!Loggers.ContainsKey(Event)) return;
            if (Loggers[Event].Count < 1) return;

            int _loc1_ = 0;
            int _loc2_ = Loggers[Event].Count;

            while (_loc1_ < _loc2_)
            {
                int _loc3_ = Loggers[Event][_loc1_];
                Delegate _loc4_ = Logs[_loc3_];

                _loc4_.DynamicInvoke(Params);

                _loc1_ = _loc1_ + 1;
            }
        }

        public static Boolean removeLogEvent(string Event, Delegate method)
        {
            if (!Loggers.ContainsKey(Event) || Logs.IndexOf(method) < 0)
            {
                return false;
            }

            int index = Logs.IndexOf(method);

            if (!Loggers[Event].Contains(index))
            {
                return false;
            }

            Loggers[Event].Remove(index);

            return true;
        }

        public static Boolean addLogEvent(string Event, Delegate method)
        {
            if (!Loggers.ContainsKey(Event))
            {
                Loggers[Event] = new List<int> { };
            }

            int index;

            if (!Logs.Contains(method))
            {
                index = Logs.Count;
                Logs.Add(method);
            } else
            {
                index = Logs.IndexOf(method);
            }

            if (!Loggers[Event].Contains(index))
            {
                Loggers[Event].Add(index);

                return true;
            }

            return false;
        }

    }
}
