using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using System.Net;
using System.Windows.Forms;

namespace Times.Server
{
    using Utils;
    using Utils.MySQL;
    using net;
    using Client;
    using Client.Base;

    class Penguin : Dynamic
    {
        public string username = "", nickname = "", password = "", swid = "", id = "";
        public bool member = false, moderator = false, banned = false;
        public string frame = "", color = "", head = "", face = "", neck = "", body = "", hand = "", feet = "", bg = "", pin = "";
        public string avatar = "", avatarAttributes = "";
        public int x = 0, y = 0;
        public int age = 0, membershipDays = 0, coins = 0, RegistrationDate = 0;
        public List<string> epf = new List<string> { };
        public List<string> stamps = new List<string> { };
        public Inventory inventory;

        public Dictionary<string, string> Cache = new Dictionary<string, string> { };

        private bool __listen__on = false;
        private AutoResetEvent RecvDataEvent = new AutoResetEvent(false);

        protected AutoResetEvent _listen__Event = new AutoResetEvent(false);

        public Shell shell = Shell.getCurrentShell();

        public bool canListen
        {
            get
            {
                return __listen__on;
            } set
            {
                if (!__listen__on && value)
                {
                    this._listen__Event.Set();
                }

                __listen__on = value;
            }
        }

        public bool canSend = false;
        public int taskTimeout = 3000; // in microseconds

        protected List<string> receiveQueue = new List<string> { };
        protected StringBuilder partialBuffer = new StringBuilder("");

        protected Socket socket;
        byte[] buffer = new byte[1024 * 2]; // 2KB
        public int PORT;

        public BaseRoom Room;
        public List<string> XTExcemption = new List<string> { "s$p#getdigcooldown", "s$j#js" };
        public bool doXT = false;

        /* CLIENT PACKETS RECEIVED, WHICH ARE PARSED*/
        public dynamic clientData;

        public Penguin(Socket sock, int p)
        {
            this.PORT = p;
            this.socket = sock;
            this.clientData = new Dynamic();
        }

        public void LoadPenguinDetails(AutoResetEvent DetailsEvent)
        {
            AutoResetEvent fetchEvent = new AutoResetEvent(false);
            MySQL.getCurrentMySQLObject().MySQLCallback(String.Format(
            "SELECT `Color`, `Head`, `Face`, `Neck`, `Body`, `Hand`, `Feet`, `Photo`, `Flag` FROM `Penguins` WHERE `ID` = '{0}'"
            , this.id), Utils.Events.EventDelegate.create(this, "FetchClothings"), fetchEvent);

            fetchEvent.WaitOne();

            MySQL.getCurrentMySQLObject().MySQLCallback(String.Format(
           "SELECT `Avatar`, `AvatarAttributes`, `RegistrationDate`, `Moderator`, `Coins`, `Stamps`, `EPF`, `Inventory` FROM `Penguins` WHERE `ID` = '{0}'"
           , this.id), Utils.Events.EventDelegate.create(this, "FetchPenguinDetails"), fetchEvent);

            fetchEvent.WaitOne();

            MySQL.getCurrentMySQLObject().MySQLCallback(String.Format(
                "SELECT `Cache`, `nx` FROM `Cache` WHERE `ID` = '{0}'", this.id), 
                Utils.Events.EventDelegate.create(this, "FetchPenguinCache"), fetchEvent);

            fetchEvent.WaitOne();

            DetailsEvent.Set();
        }

        protected string ListValue(List<string> L, int i)
        {
            if (i > L.Count - 1)
            {
                return "";
            } else
            {
                return L[i];
            }
        }

        public void FetchPenguinCache(MySqlDataReader reader, AutoResetEvent Evnt)
        {
            var _dict = reader.GetDictFromReader();
            List<String> details = (_dict["Cache"]).Split(new char[] { ';' }).ToList();

            this.Cache["PlayerWidget"] = ListValue(details, 0);
            this.Cache["MapCategory"] = ListValue(details, 1);
            this.Cache["Igloo"] = ListValue(details, 2);
            this.Cache["GAS"] = ListValue(details, 3);

            this.Cache["NX"] = _dict["nx"];

            Evnt.Set();
        }

        public void FetchPenguinDetails(MySqlDataReader reader, AutoResetEvent evnt)
        {
            var details = reader.GetDictFromReader();

            this.coins = int.Parse(details["Coins"]);
            this.avatar = details["Avatar"];
            this.avatarAttributes = details["AvatarAttributes"];
            this.RegistrationDate = int.Parse(details["RegistrationDate"]);
            this.moderator = bool.Parse(details["Moderator"]);
            this.epf = details["EPF"].Split(',').ToList();
            this.inventory = new Inventory(details["Inventory"], "%", this);

            long one_day = 24 * 60 * 60;
            long currentTmsp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            this.age = (int)Math.Floor((decimal)((currentTmsp - this.RegistrationDate) / one_day));
            /* Membership days:
            if age <= 3 months = 3 x 30.5 = 91.5, then membership = age / 10 * 30.5
            if age > 3 months <= 13 months, then membership = age/10 * 3
            if age > 13, then membership = floor((age - 365) * 100 / 365)
            */

            this.membershipDays = (int)(this.age <= 91.5 ? Math.Floor((decimal)(this.age * 10 / 365)) :
                this.age > 91.5 & this.age <= 396.5 ? Math.Floor((decimal)(this.age * 3 / 10)) :
                Math.Floor((decimal)((this.age - 365) * 100 / 365)));

            // ToDo : Stamps

            evnt.Set();
        }

        public void GenerateString()
        {
            if (Shell.getCurrentShell().getCurmbs("PlayerCache").ContainsKey(this.id))
            {
                Shell.getCurrentShell().getCurmbs("PlayerCache").Remove(this.id);
            }

            this.ToString();
        }

        public void FetchClothings(MySqlDataReader reader, AutoResetEvent evnt)
        {
            var details = reader.GetDictFromReader();

            this.color = details["Color"];
            this.head = details["Head"];
            this.face = details["Face"];
            this.neck = details["Neck"];
            this.body = details["Body"];
            this.hand = details["Hand"];
            this.feet = details["Feet"];
            this.bg = details["Photo"];
            this.pin = details["Flag"];

            evnt.Set();
        }

        public List<string> PuffleData()
        {
            AutoResetEvent PE = new AutoResetEvent(false);

            List<string> puffle_details = new List<string> { };
            Action<MySqlDataReader, AutoResetEvent> PD = new Action<MySqlDataReader, AutoResetEvent>((reader, evnt) =>
            {
                var details = reader.GetListFromDict();

                for(int i = 0; i < details.Count; i++)
                {
                    var detail = details[i];
                    string data = detail["ID"];
                    data += "|" + detail["Type"];
                    data += "|" + (detail["Subtype"] == "0" ? "" : detail["Subtype"]);
                    data += "|" + detail["Name"];
                    data += "|" + detail["AdoptionDate"];
                    data += "|" + (new List<string> { detail["Food"], detail["Play"], detail["Rest"], detail["Clean"] }).join("|");
                    data += "|" + detail["Hat"];

                    puffle_details.Add(data);
                }

                PE.Set();

            });

            MySQL.getCurrentMySQLObject().MySQLCallback(String.Format(
            String.Format("SELECT ID, Type, Subtype, Name, AdoptionDate, Food, Play, Rest, Clean, Hat FROM `puffles` WHERE Owner = {0}", this.id), 
            this.id), PD, PE);

            PE.WaitOne();

            return puffle_details;
        }

        new public string ToString()
        {
            if (!(Shell.getCurrentShell().getCurmbs("PlayerCache")).ContainsKey(this.id))
            {
                List<string> PCache = new List<string>
                {
                    this.id,
                    this.username,

                    "45", //language

                    this.color,
                    this.head,
                    this.face,
                    this.neck,
                    this.body,
                    this.hand,
                    this.feet,
                    this.bg,
                    this.pin,

                    this.x.ToString(),
                    this.y.ToString(),
                    this.frame,

                    this.membershipDays > 0 ? "1" : "0", // IsMember
                    this.membershipDays.ToString(),

                    this.avatar,
                    null, // Does Nothing ??
                    "", //  Party info, blank??

                    "", // Puffle id
                    ""  // Puffle head id
                };

                (Shell.getCurrentShell().getCurmbs("PlayerCache"))[this.id] = PCache.join("|");
                return Shell.getCurrentShell().GetPlayerString(this.id);
            } else
            {
                return Shell.getCurrentShell().getCurmbs("PlayerCache")[this.id];
            }
        }

        public Socket getSocket()
        {
            return this.socket;
        }

        private void endSend(string data)
        {
            data = data.Take(data.Length - 1).ToList().join("");
            Log.Debugger.CallEvent(Airtower.SEND_EVENT, data);
        }

        public void send(string data)
        {
            if (this.socket.Connected)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data + "\x00");

                try { this.socket.Send(buffer); this.endSend(data + "\x00"); }
                catch { };
            }
        }

        public void send(params string[] args)
        {
            if (this.socket.Connected)
            {
                var packets = new List<string> { "", "xt", args[0] };
                var internalID = this.shell.getServerRoomIdByPenguinObject(this);

                packets.Add(internalID);
                packets = packets.Concat(args.Skip(1)).ToList();
                packets.Add("\x00");

                String data = packets.join("%");
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                try { this.socket.Send(buffer); this.endSend(data); }
                catch { };
            }
        }

        public void startConnection()
        {
            var recv_func = new Action(() =>
            {
                AsyncCallback receiveCallback = new AsyncCallback(this.receiveDataFromClient);

                while (this.socket.Connected)
                {
                    if (this.canListen)
                    {
                        // Avoid creating new AysncCallback for receive data each time.
                        try
                        {
                            this.socket.BeginReceive(this.buffer, 0, this.buffer.Length, 0, receiveCallback, this.socket);
                        } catch { break; }
                        this.RecvDataEvent.WaitOne();
                    }
                    else
                    {
                        this._listen__Event.WaitOne();
                        continue;
                    }
                }

                Airtower.disposeClient(false, this.socket);
            });

            this.canListen = true;
            this.canSend = true;

            new Thread(() => recv_func()).Start();
            Task.Run(() => recv_func());
        }

        protected void receiveDataFromClient(IAsyncResult datas)
        {
            int bytesReceived;

            try { bytesReceived = ((Socket)datas.AsyncState).EndReceive(datas); }
            catch { return; }

            if (bytesReceived > 0)
            {
                string data = ASCIIEncoding.ASCII.GetString(this.buffer, 0, bytesReceived);
                Boolean isPartial = this.checkForPartialData(data);

                if (!isPartial)
                {
                    // \x00 Null byte in last index, remove it.
                    string partialdata = this.partialBuffer.ToString();
                    string[] packet = partialdata.Split(char.Parse("\x00"));

                    this.partialBuffer.Clear();

                    for(int i = 0; i < packet.Length - 1; i++)
                    {
                        string _loc1_ = packet[i];
                        var parse = Packets.Parse(_loc1_, this);

                        if (parse == Packets.InvalidPacket)
                        {
                            // Better to remove that penguin
                            Log.Debugger.CallEvent(Airtower.ERROR_EVENT, "Incorrect Packet : " + _loc1_);
                            Airtower.disposeClient(false, this.socket);
                            break;
                        }

                        // handle packets acc to received, if xt or xml
                        if (parse == Packets.XT_DATA)
                        {
                            Log.Debugger.CallEvent(Airtower.XT_EVENT, _loc1_);
                            Packets.HandleXTPacket(this);
                        } else 
                        if (parse == Packets.XML_DATA)
                        {
                            Log.Debugger.CallEvent(Airtower.XML_EVENT, _loc1_);
                            if (_loc1_ == "<policy-file-request/>")
                            {
                                this.send(String.Format("<cross-domain-policy><allow-access-from domain='*'" +
                                    " to-ports='{0}' /></cross-domain-policy>", Airtower.getCurrentAirtower().PORT));
                                return;
                            }

                            Packets.HandleXMLPacket(this);
                        }
                    }

                }

                this.RecvDataEvent.Set();
            }
            else
            {
                Airtower.disposeClient(false, this.socket);
            }
        }

        protected bool checkForPartialData(string data)
        {
            if (this.partialBuffer.ToString() != "")
            {
                this.partialBuffer.Append(data);

                if (this.partialBuffer.ToString().EndsWith("\x00"))
                {
                    return false;
                }

                return true;
            } else
            if (!data.EndsWith("\x00"))
            {
                // More packets to receive yet.
                this.partialBuffer.Append(data);
                return true;
            }
            else
            {
                this.partialBuffer.Clear();
                this.partialBuffer.Append(data);
            }

            return false;
        }

        new public void disconnected()
        {
            // stuff to do after penguin disconnected.

            this.canListen = false;
            this.canSend = false;

            Log.Debugger.CallEvent(Airtower.INFO_EVENT, String.Format("{0} disconnected.", this.username));

            if (this.socket.Connected)
            {
                this.socket.Close();
            }
        }
    }
}
