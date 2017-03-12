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
    using Server.Utils.MySQL;

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

            client.nickname = client.username = (String)body.login.nick.ToString();
            client.password = body.login.pword.ToString();

            if (Airtower.ServerType.ContainsKey(client.PORT))
            {
                MySQLStatement statement = new MySQLStatement();

                if ((body.login.pword.ToString()).Contains("#") && Airtower.ServerType[client.PORT] == 0)
                {
                    // World login 
                    statement.statement = "SELECT `ID`, `Password`, `Email`, `SWID` FROM `Penguins` WHERE `ID` = @val";
                    statement.parameters = new Dictionary<string, dynamic> { { "@val", client.username.Split(char.Parse("|"))[0] } };
                    Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(statement, Server.Utils.Events.EventDelegate.create(this, "ContinueWorldLogin"), client, body);
                }
                else
                if (Airtower.ServerType[client.PORT] == -1)
                {
                    // Primary login
                    statement.statement = "SELECT `ID`, `Password`, `Email`, `SWID` FROM `Penguins` WHERE `Username` = @val";
                    statement.parameters = new Dictionary<string, dynamic> { { "@val", client.username} };
                    Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(statement, Server.Utils.Events.EventDelegate.create(this, "ContinuePrimaryLogin"), client, body);
                }

                statement = null;
            }
        }

        public void ContinueWorldLogin(MySqlDataReader reader, Penguin client, dynamic body)
        {
            if (!reader.HasRows || GetPenguinRandomKey(client) == null)
            {
                client.send("e", "101", "#1");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            if (body == null) return;
            if (body.login == null) return;
            if (body.login.pword == null) return;
            if (body.login.nick == null) return;
            if (GetPenguinRandomKey(client) == null) return;

            string[] userInfo = body.login.nick.Value.Split(char.Parse("|"));
            if (userInfo[0].userInServer(9875))
            {
                client.send("e", "3");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            client.id = userInfo[0];

            MySQLStatement stmt = new MySQLStatement("");

            AutoResetEvent banEvent = new AutoResetEvent(false);

            stmt.statement = "SELECT `till` FROM `banned` WHERE `ID` = @id";
            stmt.parameters = new Dictionary<string, dynamic> { { "@id", client.id } };
            Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(stmt, Server.Utils.Events.EventDelegate.create(this, "HandlePenguinBanned"), client, banEvent);

            banEvent.WaitOne();

            if (client.banned)
                return;

            stmt.statement = "SELECT `ID`, `Username`, `Password`, `Email`, `SWID`, `LoginKey`, `ConfirmationHash` FROM `Penguins` WHERE `ID` = @id";
            Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(stmt, Server.Utils.Events.EventDelegate.create(this, "CompleteLogin"), client, body);
            stmt = null;
        }

        public void CompleteLogin(MySqlDataReader reader, Penguin client, dynamic body)
        {
            var details = reader.GetDictFromReader();
            //"101|OP1-{AB-ERTS-972H}-09-K|valid22|f54fb87b5b1e6120e28c5bbf1a6ca862|NULL|45|2"
            var nick = ((string)body.login.nick.Value).Split(char.Parse("|"));
            client.swid = nick[1];
            client.username = client.nickname = ((string)nick[2]).preetify();
            string password = nick[3];

            if (client.username.ToLower() != details["Username"].ToLower() || password != details["ConfirmationHash"] || 
                client.swid != details["SWID"] || password == "")
            {
                client.send("e", "101", "#2");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            var pword = body.login.pword.Value.Split(char.Parse("#"));
           
            if (pword[0] != String.Format("{0}{1}", (password + GetPenguinRandomKey(client)).md5().swap(16), password) || pword[1] != details["LoginKey"] || pword[0] == "" || 
                pword[1] == "")
            {
                client.send("e", "101", "#3");
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            client.send("l", "$loggedin");
            return;
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
            MySQLStatement stmt = new MySQLStatement("");
            stmt.parameters = new Dictionary<string, dynamic> { { "@id", client.id } };
            AutoResetEvent evnt = new AutoResetEvent(false);

            stmt.statement = "SELECT `till` FROM `banned` WHERE `ID` = @id";
            Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(stmt, Server.Utils.Events.EventDelegate.create(this, "HandlePenguinBanned"), client, evnt);

            evnt.WaitOne();

            if (client.banned)
                return;

            string ConfHash = GetPenguinRandomKey(client).md5() + "-" + penguin_details["Password"].GetHashCode().ToString().md5();
            Shell.getCurrentShell().getCurmbs("Login")["HashKey"][client][1] = ConfHash;

            stmt.statement = "UPDATE `penguins` SET `LoginKey` = @lk, `ConfirmationHash` = @ch WHERE `ID` = @id";
            stmt.parameters["@lk"] = ConfHash;
            stmt.parameters["@ch"] = password;

            Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(stmt);
            
            string users_bar = Math.Floor((double)(Airtower.Clients.Values.Where(i => i.PORT == 9875).ToList().Count * 5 / 550)).ToString();

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
                MySQLStatement stmt = new MySQLStatement("UPDATE `banned` SET `till` = '0' AND `b_by`='' AND `reason`='' WHERE `ID` = @id", new Dictionary<string, dynamic> { { "@id", client.id } });
                Server.Utils.MySQL.MySQL.getCurrentMySQLObject().MySQLCallback(stmt);
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
