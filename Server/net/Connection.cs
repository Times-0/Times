using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Dynamic;
using System.Xml.Serialization;

namespace Times.Server.net
{
    using Utils;

    class Connection : Socket
    {

        public bool ServerOpen = false;
        private ManualResetEvent Server = new ManualResetEvent(false);
        public List<Socket> Sockets = new List<Socket> { };

        public string SERVER_MESSAGE_DELIMITER = "%";
        public Dictionary<string, dynamic> MessageHandlers = new Dictionary<string, object> { };

        public dynamic onExtensionMessage
        {
            get; set;
        }

        public Connection(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) :
            base(addressFamily, socketType, protocolType)
        {
            this.SendTimeout = 30000;
            this.ReceiveTimeout = 30000;

            this.initialize();
        }

        new public void Connect(string ipAddr, int port)
        {
            IPAddress Addr = IPAddress.Parse(ipAddr);
            IPEndPoint IPE = new IPEndPoint(Addr, port);

            base.Bind(IPE);
            base.Listen(5);

            this.ServerOpen = true;
            this.BeginServer();
        }

        public void BeginServer()
        {
            while (this.ServerOpen)
            {
                this.TryAccept();
            }
        }

        public void TryAccept()
        {
            var AvailableSockets = new List<Socket> { this }.Concat(this.Sockets).ToList();
            Select(AvailableSockets, null, null, 1000);

            if (AvailableSockets.Contains(this))
            {
                Socket accepted = this.Accept();
                accepted.Blocking = false;

                this.beginAccept(accepted);
                AvailableSockets.Remove(this);
            }

            int _loc1_ = AvailableSockets.Count;
            int _loc2_ = 0;

            while (_loc2_ < _loc1_)
            {
                Socket _loc3_ = AvailableSockets[_loc2_];
                byte[] buffers = new byte[] { };


                var _loc4_ = _loc3_.Receive(buffers, 9532, 0);
                if (_loc4_ <= 0)
                {
                    Utils.Events.EventDispatcher._dispatcher.dispatchEvent(new
                    {
                        type = "onDisconnectSocket",
                        socket = _loc3_
                    });

                    try { _loc3_.Close(); } catch (Exception) { };

                    if (this.Sockets.Contains(_loc3_)) this.Sockets.Remove(_loc3_);

                }

                List<string> _loc5_ = ASCIIEncoding.ASCII.GetString(buffers).
                    Split(char.Parse("\x00")).ToList();

                int _loc7_ = _loc5_.Count, _loc6_ = 0;
                while(_loc6_ < _loc7_)
                {
                    this.OnDataReceived(_loc3_, _loc5_[_loc6_]);
                    _loc6_ = _loc6_ + 1;
                }

                _loc2_ = _loc2_ + 1;
            }

        }

        public void beginAccept(Socket Result)
        {
            Socket Client_socket = Result;
            this.Sockets.Add(Client_socket);

            dynamic EventHandler = new { type = "onClientAccept", socket = Client_socket };

            Utils.Events.EventDispatcher._dispatcher.dispatchEvent(EventHandler);

        }

        public void OnDataReceived(Socket Penguin, string Data)
        {
            if (Data.ElementAt(0).ToString() == this.SERVER_MESSAGE_DELIMITER)
            {
                try { this.StrReceived(Penguin, Data); } catch { };

            }
            else if (Data.ElementAt(0).ToString() == "<")
            {
                try { this.XMLReceived(Penguin, Data); } catch { };
            }
        }

        public void StrReceived(Socket Penguin, string Data)
        {
            Data = Data.Remove(0, 1);
            Data = Data.Remove(Data.Length - 2, 2);

            List<string> packet = Data.Split(char.Parse(this.SERVER_MESSAGE_DELIMITER)).ToList();
            MessageBox.Show(packet.join(", "));

            string _loc1_ = packet[1];
            string _loc3_ = packet[2];

            List<dynamic> _loc2_ = new List<dynamic> { }.Concat( packet.Skip(2).Take(Data.Length - 1).ToList()).ToList();
            this.MessageHandlers[packet[0]].handleMessage(Penguin, _loc2_, this, "str");
        }

        public dynamic XMLReceived
        {
            get; set;
        }

        public void handleExtensionMessages(Socket penguin, dynamic data, dynamic target, string type = null)
        {
            if (type == null)
            {
                type = "xml";
            }

            if (type == "xml")
            {
                String _loc2_ = data.action;
                String _loc3_ = data.r;
                if (_loc2_ == "xtRes")
                {
                    String _loc4_ = data.value;
                    dynamic _loc5_ = (Dynamic)new XmlSerializer(typeof(Dynamic)).Deserialize(_loc4_.GetStream());

                    target.onExtensionMessage(penguin, _loc5_, type);
                }
            }
            else if (type == "str")
            {
                target.onExtensionMessage(penguin, data, type);
            }
        }

        public void setupMessageHandlers()
        {
            this.addMessageHandler(new Dictionary<string, dynamic>
            {
                { "xt", Utils.Events.EventDelegate.create(this, "handleExtensionMessages")}
            });
        }

        public void addMessageHandler(Dictionary<string, dynamic> handlers)
        {
            var _loc1_ = handlers.Keys.ToList();
            int _loc2_ = _loc1_.Count;
            int _loc3_ = 0;

            while (_loc3_ < _loc2_)
            {
                var _loc4_ = _loc1_[_loc3_];
                var _loc5_ = handlers[_loc4_];

                this.MessageHandlers[_loc4_] = new Dynamic();
                this.MessageHandlers[_loc4_].handleMessage = _loc5_;

                _loc3_ = _loc3_ + 1;
            }

        }

        public void initialize()
        {
            this.setupMessageHandlers();
            Utils.Events.EventDispatcher._dispatcher.dispatchEvent(new { type = "initialize" });
        }

    }
}
