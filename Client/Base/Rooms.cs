using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Times.Client.Base
{
    using Server;
    using Server.Log;
    using Server.net;
    using Server.Utils;
    using Server.Utils.Events;
    using Server.Utils.MySQL;

    using Client;
    using Client.Base;

    abstract class IROOM
    {
        public const int Game = -1;
        public const int Place = 0;
        public const int Igloo = 1;
        public const int Others = 2;
    }

    abstract class BaseRoom : IROOM, IRoom
    {
        protected string roomName;
        protected string roomId;
        protected string roomExtId;

        protected int roomType;

        public bool isMember;
        public int MaxUsers;

        protected List<Penguin> users = new List<Penguin> { };

        public BaseRoom(string name, string id, string ext, int roomType)
        {
            this.roomName = name;
            this.roomId = id;
            this.roomExtId = ext;
            this.roomType = roomType;
        }

        public void Add(Penguin peng, bool ov = false)
        {
            if (!ov)
            {
                if (this.users.Count + 1 > this.MaxUsers)
                {
                    peng.send(Airtower.HANDLE_ERROR, "210");
                    return;
                }
                else
                {
                    if (this.isMember && !peng.member)
                    {
                        peng.send(Airtower.HANDLE_ERROR, "");
                        return;
                    }
                }
            }

            this.users.Add(peng);
            this.Added(peng);
        }

        public virtual void Added(Penguin peng)
        {
            Console.WriteLine("No!");
        }

        public void Remove(Penguin peng)
        {
            this.users.Remove(peng);

            this.Removed(peng);
        }

        public virtual void Removed(Penguin peng) { }

        public string GetRoomName()
        {
            return this.roomName;
        }

        public string GetRoomId()
        {
            return this.roomId;
        }

        public string getRoomExtId()
        {
            return this.roomExtId;
        }

        public int GetRoomType()
        {
            return this.roomType;
        }

        public void send(params string[] args)
        {
            for (var i = 0; i < this.users.Count; i++)
            {
                this.users[i].send(args);
            }
        }
    }
}
