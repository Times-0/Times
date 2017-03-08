using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Times.Client
{
    using Server;
    using Server.Log;
    using Server.net;
    using Server.Utils;
    using Server.Utils.Events;
    using Server.Utils.MySQL;

    using Client;
    using Client.Base;

    class GeneralRoom : BaseRoom
    {

        public GeneralRoom(string name, string id, string ext, string isMember, string max_users) : base(name, id, ext, Place)
        {
            this.roomType = Place;
            this.isMember = int.Parse(isMember) > 0;
            this.MaxUsers = int.Parse(max_users);
        }

        override public void Added(Penguin client)
        {
            client.Room = this;
            Console.WriteLine(String.Format("{0} has joined {1}", client.username, this.name));
            // Send ap packet.
            this.send("ap", Shell.getCurrentShell().GetPlayerString(client.id));
            client.send("jr", this.ToString());
        }

        override public void Removed(Penguin client)
        {
            client.Room = null;
            Console.WriteLine(String.Format("Removed {0} from {1}", client.username, this.name));

            this.send("rp", client.id);
        }

        override public string ToString()
        {
            List<string> playerString = new List<string> { };
            for (var i = 0; i < this.users.Count; i++)
            {
                playerString.Add(Shell.getCurrentShell().GetPlayerString(this.users[i].id));
            }

            return playerString.join("%");
        }
    }
}
