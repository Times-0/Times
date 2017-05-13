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
        
        public static Dictionary<int, Item> AvailablePenguinItems = new Dictionary<int, Item> { };
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
                Item item = new Item(cloth);

                AvailablePenguinItems[item.id] = item;
            }
        }

        public List<Item> GetItem(dynamic item, string key = "id")
        {
            List<Item> Items = new List<Item> { };

            key = key.ToLower();
            var Keys = AvailablePenguinItems.Keys.ToList();
            for (int i = 0; i < Keys.Count; i++)
            {
                var _item = AvailablePenguinItems[Keys[i]];
                if (key == "id" && int.Equals(_item.id, item))
                    Items.Add(_item);
                else if (key == "type" && int.Equals(_item.id, item))
                    Items.Add(_item);
                else if (key == "cost" && int.Equals(_item.id, item))
                    Items.Add(_item);
                else if (key == "member" && bool.Equals(_item.member, item))
                    Items.Add(_item);
                else if (key == "name" && string.Equals(_item.name, item))
                    Items.Add(_item);
                else if (key == "display" && string.Equals(_item.display, item))
                    Items.Add(_item);
                else if (key == "epf" && bool.Equals(_item.epf, item))
                    Items.Add(_item);
                else if (key == "layer" && int.Equals(_item.layer, item))
                    Items.Add(_item);
                else if (key == "bait" && bool.Equals(_item.bait, item))
                    Items.Add(_item);
            }
               
            return Items;
        }

        public bool ItemExists(int item)
        {
            return this.GetItem(item).Count > 0;
        }

        public bool ItemExists(string item)
        {
            int _item;

            if (!int.TryParse(item, out _item))
                return false;

            return this.ItemExists(_item);
        }

        public List<Item> FilterItemsByType(List<int> items, int type)
        {
            List<Item> FilteredItems = new List<Item> { };
            Item item;

            for (int i = 0; i < items.Count; i++)
                if ((item = this.GetItem(i)[0]).type == type)
                    FilteredItems.Add(item);

            return FilteredItems;          
        }
    }
}
