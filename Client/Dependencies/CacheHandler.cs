using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Times.Client.Dependencies
{
    using Server;
    using Server.Log;
    using Server.net;
    using Server.Utils;
    using Server.Utils.Events;
    using Server.Utils.MySQL;
    using Newtonsoft.Json;
    using Client.Base;

    class CacheHandler
    {
        public static string CLASS_LINKAGE_NAME = "CACHE";
        
        public static Dictionary<string, Dictionary<string, dynamic>> AvailablePenguinItems = new Dictionary<string, Dictionary<string, dynamic>> { };
        public static Dictionary<string, BaseRoom> Rooms = new Dictionary<string, BaseRoom> { };

        public CacheHandler()
        {
            this.AddEventHandler("!Crumbs!paper_items.json", "ClothesLoaded");
            this.AddEventHandler("!Crumbs!rooms.json", "RoomsLoaded");

            this.AddXTHandler("s", "nx#gas", "GetActionStatus");
            this.AddXTHandler("s", "nx#pcos", "GetPlayerWidgetCache");
            this.AddXTHandler("s", "nx#mcs", "GetMapCategory");

            EventDispatcher._dispatcher.addListener("JS#{JOINED}", EventDelegate.create(this, "JoinedServer"));
        }

        public void JoinedServer(Penguin client)
        {
            var data = client.Cache["NX"];
            var _data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);

            var _settings = JsonConvert.SerializeObject(_data["settings"]);
            var _data_ = JsonConvert.SerializeObject(_data["data"]);

            client.send("nxquestsettings", _settings);
            client.send("nxquestdata", _data_);
        }

        public void GetMapCategory(Penguin client, string[] data)
        {
            client.send("mcs", client.Cache["MapCategory"]);
        }

        public void GetPlayerWidgetCache(Penguin client, string[] data)
        {
            client.send("pcos", client.Cache["PlayerWidget"]);
        }

        public void GetActionStatus(Penguin client, string[] data)
        {
            client.send("gas", client.Cache["GAS"]);
        }

        public void RoomsLoaded(string FilePath)
        {
            string text = System.IO.File.ReadAllText(FilePath);

            Dictionary<string, Dictionary<string, string>> RoomsDetails = 
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(text);
            var _k = RoomsDetails.Keys.ToList();

            for (var i = 0; i < RoomsDetails.Count; i++)
            {
                string name = _k[i];
                var room = RoomsDetails[name];
                string id = room["room_id"];
                string r_name = room["name"];
                string r_member = room["is_member"];
                string r_max = room["max_users"];

                if (room["room_key"] == "")
                {
                    Rooms[id] = new GameRoom(r_name, "-" + id, id, r_member, r_max);
                } else
                {
                    Rooms[id] = new GeneralRoom(r_name, "-" + id, id, r_member, r_max);
                }
            }
        }

        public void ClothesLoaded(string FilePath)
        {
            string text = System.IO.File.ReadAllText(FilePath);

            List<Dictionary<string, string>> ClothDetails = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(text);
            for (var i  = 0; i < ClothDetails.Count; i++)
            {
                Dictionary<string, string> cloth = ClothDetails[i];
                AvailablePenguinItems[cloth["paper_item_id"]] = new Dictionary<string, dynamic> { { "type", cloth["type"] } };

                AvailablePenguinItems[cloth["paper_item_id"]]["cost"] = cloth["cost"];
                AvailablePenguinItems[cloth["paper_item_id"]]["member"] = Boolean.Parse(cloth["is_member"]);
                AvailablePenguinItems[cloth["paper_item_id"]]["name"] = (cloth["label"]).ToString();
                AvailablePenguinItems[cloth["paper_item_id"]]["epf"] = false;

                if (cloth.ContainsKey("is_epf"))
                    AvailablePenguinItems[cloth["paper_item_id"]]["epf"] = cloth["is_epf"] == "1" ? true : false;
            }
        }
    }
}
