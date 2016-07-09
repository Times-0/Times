using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace Times.Server
{
    using Utils;
    using net;
    using Client;

    class Penguin : Dynamic
    {
        public string username = "", nickname = "", password = "", swid = "", id = "";
        public bool member = false, moderator = false, banned = false;
        public int age = 0, membershipDays = 0;

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

        /* CLIENT PACKETS RECEIVED, WHICH ARE PARSED*/
        public dynamic clientData;

        public Penguin(Socket sock)
        {
            this.socket = sock;
            this.clientData = new Dynamic();
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
                            Packets.HandleXTPacket(this);
                            Log.Debugger.CallEvent(Airtower.XT_EVENT, _loc1_);
                        } else 
                        if (parse == Packets.XML_DATA)
                        {
                            if (_loc1_ == "<policy-file-request/>")
                            {
                                this.send(String.Format("<cross-domain-policy><allow-access-from domain='*'" +
                                    " to-ports='{0}' /></cross-domain-policy>", Airtower.getCurrentAirtower().PORT));
                                return;
                            }

                            Log.Debugger.CallEvent(Airtower.XML_EVENT, _loc1_);
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

