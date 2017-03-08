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

    class GameRoom : BaseRoom
    {

        public GameRoom(string name, string id, string ext, string isMember, string max_users) : base(name, id, ext, Game)
        {
            this.roomType = Game;
            this.isMember = int.Parse(isMember) > 0;
            this.MaxUsers = int.Parse(max_users);
        }

        override public void Added(Penguin client)
        {
            client.Room = this;

            List<string> MultiplayerGames = new List<string> { "998", "999" };
            if (!MultiplayerGames.Contains(this.roomExtId))
            {
                List<string> nbg = new List<string> { "121", "900", "909", "950", "956", "963" };
                if (nbg.Contains(this.roomExtId))
                {
                    client.send("jnbhg", this.roomExtId);
                } else
                {
                    client.send("jg", this.roomExtId);
                }
            }
        }

        override public void Removed(Penguin client)
        {
            client.Room = null;
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
