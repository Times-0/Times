using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;

namespace Times.Client
{
    using Server;
    using Server.Utils;
    using Server.Utils.Events;

    class Shell : Dynamic
    {
		public Dictionary<string, object> DEPENDENCIES;
		
        public static bool IsWorldServer = false;
        public static bool IsLoginServer = false;
        // TODO : Recognize Redeem server.
        public static bool IsRedeemServer = false;

        protected static Shell __shell__O;
        
        protected Dictionary<string, dynamic> Crumbs = new Dictionary<string, dynamic> { };

        public Shell()
        {
            __shell__O = this;

            EventDispatcher.initialize(this);
            this.pushListeners();

            this.InitHandlers();
            this.InitCrumbs();
            Server.Log.Debugger.CallEvent(Server.net.Airtower.INFO_EVENT, "Shell initiated!");
        }

        protected void InitHandlers()
        {
			this.DEPENDENCIES = new Dictionary<string, object> { };
			
            Server.Log.Debugger.CallEvent(Server.net.Airtower.INFO_EVENT, "Loading server dependencies...");
            Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
            string Namespace = "Times.Client.Dependencies"; // Change if if you mean to change the name space of the class

            // Prefer using String.Equal comparison.
            Type[] Dependencies = CurrentAssembly.GetTypes().Where(type => String.Equals(type.Namespace, Namespace, StringComparison.Ordinal) && type.IsClass 
                && !type.IsAbstract).ToArray();

            for(var _loc1_ = 0; _loc1_ < Dependencies.Length; _loc1_ ++)
            {
                try
                {
                    var Class = Activator.CreateInstance(Dependencies[_loc1_]);
					string linkage_name;

                    if (Dependencies[_loc1_].GetProperty("CLASS_LINKAGE_NAME") != null)
                        linkage_name = ((dynamic)Class).CLASS_LINKAGE_NAME;
                    else
                        linkage_name = Dependencies[_loc1_].Name;

                    this.DEPENDENCIES[linkage_name] = Class;
                }
                catch (Exception Excep)
                {
                    Server.Log.Debugger.CallEvent(Server.net.Airtower.ERROR_EVENT, "Unable to load dependency : \n" + Excep.InnerException);
                    Server.Log.Debugger.CallEvent(Server.net.Airtower.WARN_EVENT, "Server exited!");
                    Server.Log.Debugger.CallEvent(Server.net.Airtower.WARN_EVENT, Excep.ToString());
                    Console.Write("Press any key to exit...");
                    Console.Read();
                    Environment.Exit(0);
                }
            }

            Server.Log.Debugger.CallEvent(Server.net.Airtower.INFO_EVENT,
                String.Format("Successfully loaded {0} dependencies!", Dependencies.Length));
        }

        public string getServerRoomIdByPenguinObject(Penguin client)
        {
            if (client.Room != null)
                return client.Room.GetRoomId();
            return "-1";
        }

        public Client.Base.BaseRoom getServerRoomObjectByPenguinObject(Penguin client)
        {
            if (client.Room != null)
                return client.Room;
            return null;
        }

        private void pushListeners()
        {
            
        }

        private void CheckForCrumbs(List<string> Files, string directory = "\\Crumbs\\", 
            string jsonURL = "http://media1.clubpenguin.com/play/en/web_service/game_configs/")
        {
            var CurrentDir = System.IO.Directory.GetCurrentDirectory();
            var CrumbsDir = CurrentDir + directory;

            if (!System.IO.Directory.Exists(CrumbsDir))
                System.IO.Directory.CreateDirectory(CrumbsDir);
            
            foreach(String file in Files)
            {
                if (!System.IO.File.Exists(CrumbsDir + file))
                {
                    string Data = (jsonURL + file).GetURLContents();
                    System.IO.File.Create(CrumbsDir + file).Dispose();

                    new System.IO.StreamWriter(CrumbsDir + file).WriteLine(Data);
                }

                // Call all handlers, which needs that crumb cache.
                string EventHandler = String.Format("!Crumbs!{0}", file);
                EventDispatcher._dispatcher.updateListeners(EventHandler, CrumbsDir + file);
            }
        }

        protected void InitCrumbs()
        {
            this.Crumbs["Login"] = new Dictionary<string, Dictionary<Penguin, dynamic>> { };
            // Penguin -> {random Key, login hash}
            this.Crumbs["Login"]["HashKey"] = new Dictionary<Penguin, dynamic> { };

            /* TODO:
              -: Handles failed login, and the time they failed to login.
              -: Penguin -> { No of times failed, Timestamp of prev fail, banned for } 
            */
            this.Crumbs["Login"]["FailedAttempts"] = new Dictionary<Penguin, dynamic>{ };

            // General crumbs.
            CheckForCrumbs(new List<string> { "rooms.json", "mascots.json", "paper_items.json" });

            // Penguin id -> playerString
            this.Crumbs["PlayerCache"] = new Dictionary<string, string> { };
        }

        public dynamic getCurmbs(string Name)
        {
            if (!this.Crumbs.ContainsKey(Name)) return null;

            return this.Crumbs[Name];
        }

        public string GetPlayerString(string id)
        {
            if (this.Crumbs["PlayerCache"].ContainsKey(id))
            {
                return this.Crumbs["PlayerCache"][id];
            } else
            {
                return Server.net.Airtower.Clients.GetPenguinById(id).ToString();
            }
        }

        public static Shell getCurrentShell()
        {
            return __shell__O;
        }
    }
}
