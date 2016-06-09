using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Times.Server.net
{
    using Server.Utils;
    using Server;
    
    class Airtower : IAirtower
    {
        /* LOG CONSTANTS */

        public const string XT_EVENT = "onXTReceivedE";
        public const string XML_EVENT = "onXMLReceivedE";
        public const string ERROR_EVENT = "onErroredE";
        public const string INFO_EVENT = "onInfoE";
        public const string WARN_EVENT = "onWarnE";
        public const string ON_SERVER_END = "onServerFatalStop";

        /* OTHER SERVER STUFF */
        public string IP, NAME;
        public int PORT;

        private bool _serverRun = false;
        public bool isServerRunning
        {
            get
            {
                return _serverRun;
            } set
            {
                _serverRun = value;
                if (!value)
                {
                    Log.Debugger.CallEvent(ON_SERVER_END, "The host machine/user has raised token to stop the server.");
                }
            }
        }

        protected Socket socket = null;

        public AutoResetEvent Server_event = new AutoResetEvent(false);
        public static Dictionary<Socket, Penguin> Clients = new Dictionary<Socket, Penguin> { };

        private static Airtower __airtower__O = null;

        public Airtower(string ip, int port, string name = "Times Server")
        {
            var signature =
                @"
                 --------------------------------------------------
                 |            TIMES CPPS EMULATOR                 |
                 |       SUPPORTED PROTOCOL : AS3                 |
                 |       Powered by : Dote                        |
               ---[;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;]---
                         NAME OF THE CPPS: {0}
                         IP : {1}, PORT : {2}";
            signature = String.Format(signature, name, ip, port);
            this.IP = ip;
            this.PORT = port;
            this.NAME = name;

            Log.Debugger.CallEvent(INFO_EVENT, signature);

            __airtower__O = this;
        }

        public void startConnection()
        {
            IPAddress ip = IPAddress.Parse(this.IP);
            IPEndPoint IPe = new IPEndPoint(ip, this.PORT);

            Log.Debugger.CallEvent(INFO_EVENT, "Airtower started!");
            
            this.startServer(IPe);
        }

        protected Boolean startServer(IPEndPoint IPEnd)
        {
            int max_tries = 10;
            int tries = 0;

            try
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Bind(IPEnd);
                this.socket.Listen(15);
                this.isServerRunning = true;
            } catch(Exception except)
            {
                MessageBox.Show("Unable to create server. ERROR : " + except.Message.ToString(), "Error");
                Log.Debugger.CallEvent(ERROR_EVENT, except.InnerException.ToString());
            }

            while (this.isServerRunning)
            {
                try
                {
                    this.socket.BeginAccept(new AsyncCallback(NewPenguinConnection), this.socket);
                    this.Server_event.WaitOne();
                } catch (Exception except)
                {
                    // Try again until max_times.
                    if (tries < max_tries)
                    {

                        Log.Debugger.CallEvent(WARN_EVENT, 
                            String.Format("Server unfortunately stopped listening, trying again. {0} tries remaining. " + 
                            "Will start to listen again after 3 seconds.\nERROR : {1}", max_tries - tries, except.ToString()));
                        // Sleep sometime and try again.
                        Task.Delay(3000);
                        tries++;
                    }
                    else
                        break;
                }
            }

            // Server stopped!
            this.isServerRunning = false;

            return false;
        }

        protected void NewPenguinConnection(IAsyncResult newClient)
        {
            Socket client = (Socket)newClient.AsyncState;
            client = client.EndAccept(newClient);

            Log.Debugger.CallEvent(INFO_EVENT, String.Format("New penguin : #{0}", Airtower.Clients.Count + 1));

            Penguin objPeng = new Penguin(client);

            Airtower.Clients[client] = objPeng;

            objPeng.startConnection();

            this.Server_event.Set();
        }

        public static void disposeClient(bool all = true, Socket sock = null)
        {
            if (!all && sock == null)
                return;

            if (all)
            {
                Socket[] clients = Airtower.Clients.Keys.ToArray();
                for (int i = 0; i < clients.Length; i++)
                {
                    Socket client = clients[i];
                    Penguin clientOj = Airtower.Clients[client];

                    Airtower.Clients.Remove(client);
                    try
                    {
                        clientOj.Dispose();
                    }
                    catch (Exception) { };
                }
            } else
            {
                if (Airtower.Clients.ContainsKey(sock))
                {
                    Penguin objClient = Airtower.Clients[sock];
                    Airtower.Clients.Remove(sock);

                    objClient.Dispose();
                }
            }

        }

        public static Airtower getCurrentAirtower()
        {
            return __airtower__O;
        }
    }
}
