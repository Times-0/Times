using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Times.Client.Dependencies
{
    using Server;
    using Server.Log;
    using Server.net;
    using Server.Utils;
    using Server.Utils.Events;
    using Server.Utils.MySQL;

    class JoinHandler
    {

        public JoinHandler()
        {
            this.AddXTHandler("s", "j#js", "JoinPenguinToServer");
        }

        public void JoinPenguinToServer(Penguin client, string[] data)
        {
            string id = data[0];
            string hash = data[1];

            if (id != client.id)
            {
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            string confirmationHash = (MySQL.getCurrentMySQLObject().ExecuteAndFetch(
                String.Format("SELECT `ConfirmationHash` FROM `penguins` WHERE `ID` = '{0}'", client.id))).GetDictFromReader()["ConfirmationHash"];

            if (hash != confirmationHash)
            {
                Airtower.disposeClient(false, client.getSocket());
                return;
            }

            AutoResetEvent get_details = new AutoResetEvent(false);
            client.LoadPenguinDetails(get_details);
            get_details.WaitOne();
            client.GenerateString();
            client.send(Airtower.JOIN_WORLD, "1", client.epf[0], (client.moderator ? 1 : 0).ToString());

            client.doXT = true;

            var puffle_data = client.PuffleData();
            client.send(Airtower.GET_MY_PLAYER_PUFFLES, puffle_data.join("%"));

            /* TO ADD STAMPS*/
            client.send(Airtower.GET_PLAYER + "s", client.epf[0], "");

            client.send(Airtower.LOAD_PLAYER,
                (new List<string> { client.ToString(),
                    client.coins.ToString(),
                    "0", // Safe-Game mode ? 0 -> no, 1 -> yes
                    "1024", // Egg timer
                    ((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds - 2 * 60 * 60).ToString(), // Penguin Standard Time
                    client.age.ToString(),
                    "0", // Banned age
                    client.age.ToString(), // Penguin's days old
                    client.membershipDays.ToString(),
                    "",
                    client.Cache["PlayerWidget"], // Has opened PlayerWidget - Cache
                    client.Cache["MapCategory"],
                    client.Cache["Igloo"],
                }).join("%"));

            client.send("glr", "");
            client.send("spts", client.id, client.avatar, client.avatarAttributes);

            EventDispatcher._dispatcher.updateListeners("JS#{JOINED}", client);

            ((GeneralRoom)(CacheHandler.Rooms["100"])).Add(client, true);
        }
    }
}
