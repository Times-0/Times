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
        public static List<Penguin> _penguins = new List<Penguin> { };

        public static bool IsWorldServer = false;
        public static bool IsLoginServer = false;
        // TODO : Recognize Redeem server.
        public static bool IsRedeemServer = false;

        protected static Shell __shell__O;

        public Shell()
        {
            __shell__O = this;

            EventDispatcher.initialize(this);
            this.pushListeners();

            this.InitHandlers();
            Server.Log.Debugger.CallEvent(Server.net.Airtower.INFO_EVENT, "Shell initiated!");
        }

        protected void InitHandlers()
        {
            Server.Log.Debugger.CallEvent(Server.net.Airtower.INFO_EVENT, "Loading server dependencies...");
            Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
            string Namespace = "Times.Client.Dependencies"; // Change if if you mean to change the name space of the class

            // Prefer using String.Equal comparison.
            Type[] Dependencies = CurrentAssembly.GetTypes().Where(type => String.Equals(type.Namespace, Namespace, StringComparison.Ordinal)).ToArray();

            for(var _loc1_ = 0; _loc1_ < Dependencies.Length; _loc1_ ++)
            {
                try
                {
                    var Class = Activator.CreateInstance(Dependencies[_loc1_]);
                }
                catch (Exception Excep)
                {
                    Server.Log.Debugger.CallEvent(Server.net.Airtower.ERROR_EVENT, "Unable to load dependency : \n" + Excep.InnerException.ToString());
                    Server.Log.Debugger.CallEvent(Server.net.Airtower.WARN_EVENT, "Server exited!");
                    Console.Write("Press any key to exit...");
                    Console.Read();
                    Environment.Exit(0);
                }
            }

            Server.Log.Debugger.CallEvent(Server.net.Airtower.INFO_EVENT,
                String.Format("Successfully loaded {0} dependencies!", Dependencies.Length));
        }

        private void pushListeners()
        {
            //this.addListener("onDisconnectSocket", EventDelegate.create(this, "removePenguinObject"), this);
            
        }

        public static Shell getCurrentAirtower()
        {
            return __shell__O;
        }
    }
}
