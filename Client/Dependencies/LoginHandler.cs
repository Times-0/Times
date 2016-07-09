using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Times.Client.Dependencies
{
    using Server;
    using Server.Log;
    using Server.net;
    using Server.Utils;

    class LoginHandler : Dynamic
    {

        public LoginHandler()
        {
            this.AddXMLHandler("verChk", "ClientAPIVersionCheck");
            this.AddXMLHandler("rndK", "HandleRandomKeyRequest");
            this.AddXMLHandler("login", "HandleLogin");
        }

        public void ClientAPIVersionCheck(Penguin client, dynamic body)
        {
            string ver = body.ver["v"];
            if (ver == "153")
            {
                client.send("<msg t='sys'><body action='apiOK' r='0'></body></msg>");
                return;
            } else
            {
                client.send("<msg t='sys'><body action='apiKO' r='0'></body></msg>");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }
        }

        public void HandleRandomKeyRequest(Penguin client, dynamic body)
        {
            string randomKey = "";
            if (GetPenguinRandomKey(client) == null)
            {
                randomKey = Cryptography.randomKey();
                Shell.getCurrentShell().getCurmbs("Login")["HashKey"][client] = new List<string> { randomKey, "" };
            } else
            {
                randomKey = GetPenguinRandomKey(client);
            }

            client.send(String.Format("<msg t='sys'><body action='rndK' r='-1'><k><![CDATA[{0}]]></k></body></msg>", randomKey));
        }

        public void HandleLogin(Penguin client, dynamic body)
        {
            if (body == null) return;
            if (body.login == null) return;
            if (body.login.pword == null) return;
            if (body.login.nick == null) return;
            if (GetPenguinRandomKey(client) == null) return;

            client.nickname = client.username = ((String)body.login.nick.ToString()).preetify();
            client.password = body.login.pword.ToString();

            if ((body.login.pword.ToString()).Contains("#") && Airtower.ServerType == 0)
            {
                // World login 

            } else 
            if (Airtower.ServerType == -1)
            {
                // Primary login
                Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(
                    String.Format("SELECT `ID`, `Password`, `Email`, `SWID` FROM `Penguins` WHERE `Username` = '{0}'", client.username), 
                    Server.Utils.Events.EventDelegate.create(this, "ContinuePrimaryLogin"), client, body);
            }
        }

        public void ContinuePrimaryLogin(MySqlDataReader reader, Penguin client, dynamic data)
        {
            if (!reader.HasRows || GetPenguinRandomKey(client) == null)
            {
                client.send("e", "101");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            var penguin_details = reader.GetDictFromReader();

            string password = Cryptography.GenerateLoginHash(penguin_details["Password"], GetPenguinRandomKey(client));
            if (password != client.password)
            {
                client.send("e", "101");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            client.id = penguin_details["ID"];
            // Handle Banned
            AutoResetEvent evnt = new AutoResetEvent(false);
            Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(
                    String.Format("SELECT `till` FROM `banned` WHERE `ID` = '{0}'", client.id),
                    Server.Utils.Events.EventDelegate.create(this, "HandlePenguinBanned"), client, evnt);

            evnt.WaitOne();

            if (client.banned)
                return;

            string ConfHash = GetPenguinRandomKey(client).md5() + "-" + penguin_details["Password"];
            Shell.getCurrentShell().getCurmbs("Login")["HashKey"][client][1] = ConfHash;
            
            string users_bar = Math.Floor((double)(Airtower.Clients.Count * 5 / 500)).ToString();

            client.send("l", 
                string.Format("{0}|{1}|{2}|{3}|NULL|45|2", new object[] { client.id, penguin_details["SWID"],
                client.username, password }), ConfHash, ConfHash.md5(),
                string.Format("100,{0}", users_bar), penguin_details["Email"]);

            Airtower.disposeClient(false, client.getSocket());
        }

        public void HandlePenguinBanned(MySqlDataReader reader, Penguin client, AutoResetEvent evnt)
        {
            if (!reader.HasRows)
            {
                client.banned = false;
                evnt.Set();
                return;
            }

            var details = reader.GetDictFromReader();
            long timestamp = long.Parse(details["till"]);
            long currentTmsp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            if (timestamp > currentTmsp)
            {
                client.banned = true;
                string hoursban = (Math.Round((timestamp - currentTmsp)/3600.0)).ToString();
                client.send("e", "601", hoursban);
                Airtower.disposeClient(false, client.getSocket());
            } else
            {
                client.banned = false;
                Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(
                    String.Format("UPDATE `banned` SET `till` = '0' AND `b_by`='' AND `reason`='' WHERE `ID` = {0}", client.id));
            }

            evnt.Set();
        }

        public static string GetPenguinRandomKey(Penguin client)
        {
            if (client == null) return null;

            string RandomKey = null;
            Dictionary<Penguin, dynamic> hashes = Shell.getCurrentShell().getCurmbs("Login")["HashKey"];
            if (!hashes.ContainsKey(client))
                return null;

            RandomKey = hashes[client][0];

            return RandomKey;
        }
    }
}
