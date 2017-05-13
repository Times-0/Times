using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Times.Client.Base
{
    using Server;
    using Server.Utils.MySQL;
    using Server.Utils;
    using Dependencies;

    class Inventory
    {
        public List<Item> items;
        public Penguin penguin;

        public Inventory()
        {
            this.items = new List<Item> { };
            this.penguin = null;
        }


        ///<summary>
        ///  Returns `Item` instance of the item-id provided. Returns `null` is not present.
        ///</summary>
        ///<param name="id">Item id of required object.</param>
        public Item this[int id]
        {
            get
            {
                for (int i = 0; i < this.items.Count; i++)
                    if (int.Equals(this.items[i].id, id))
                        return this.items[i];

                return null;
            }
        }

        public void Add(Item item)
        {
            if (!this.ItemExists(item.id))
                this.items.Add(item);
        }

        public void Add(int item)
        {
            var Cache = (CacheHandler)Shell.getCurrentShell().DEPENDENCIES[CacheHandler.CLASS_LINKAGE_NAME];

            if (!this.ItemExists(item))
                this.items.Add(Cache.GetItem(item)[0]);

            if (this.penguin == null) return;

            string Items = this.Join("%");
            MySQLStatement statement = new MySQLStatement("UPDATE `penguins` SET `Inventory` = @inv WHERE `ID` = @id");
            
            statement.parameters["@inv"] = Items;
            statement.parameters["@id"] = this.penguin.id;

            MySQL.getCurrentMySQLObject().MySQLCallback(statement);
        }

        public void Add(string item)
        {
            int id;
            if (!int.TryParse(item, out id))
                return;

            this.Add(id);
        }

        public string Join(string delimiter)
        {
            var items = this.items.Select(x => x.id).ToList();

            return items.join(delimiter);
        }

        public string Join<T> (T del)
        {
            return this.Join(del.ToString());
        }

        public int[] Items()
        {
            return this.items.Select(x => x.id).ToArray();
        }
        
        public bool ItemExists(int id)
        {
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
                if (int.Equals(this.items[i].id, id))
                    return true;

            return false;
        }

        public bool ItemExists(string id)
        {
            int _out;

            if (!int.TryParse(id, out _out))
                return false;

            return this.ItemExists(_out);
        }

        public Inventory(Penguin penguin) : this()
        {
            this.penguin = penguin;
        }

        public Inventory(List<int> items) : this(null, items.ToArray()) { }
        public Inventory(List<string> items) : this(null, items.ToArray()) { }

        public Inventory(params int[] items) : this(null, items) { }
        public Inventory(params string[] items) : this(null, items) { }

        public Inventory(Penguin penguin = null, params int[] items) : this(penguin)
        {
            int count = items.Length;
            var Cache = (CacheHandler)Shell.getCurrentShell().DEPENDENCIES[CacheHandler.CLASS_LINKAGE_NAME];

            for (int i = 0; i < count; i++)
            {
                int _out = items[i];
                Item item_;

                if (!Cache.ItemExists(_out))
                    throw new KeyNotFoundException(String.Format("Item {0} doesn't exists or not found!", i));

                if (this.items.Contains(item_ = Cache.GetItem(_out)[0]))
                    continue;

                this.items.Add(item_);
            }
        }

        public Inventory(Penguin penguin = null, params string[] items) : this(penguin)
        {
            int count = items.Length;
            var Cache = (CacheHandler)Shell.getCurrentShell().DEPENDENCIES[CacheHandler.CLASS_LINKAGE_NAME];

            for (int i = 0; i < count; i ++)
            {
                int _out;
                Item item_;

                if (!int.TryParse(items[i], out _out))
                    throw new InvalidCastException(String.Format("Item {0} on given array is not an integer!", i));
                
                if (!Cache.ItemExists(_out))
                    throw new KeyNotFoundException(String.Format("Item {0} doesn't exists or not found!", i));

                if (this.items.Contains(item_ = Cache.GetItem(_out)[0]))
                    continue;

                this.items.Add(item_);
            }
        }

        public Inventory(string inventory_string, string delimiter = ",", Penguin penguin = null) : this(penguin, inventory_string.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries))
        { }

    }
}
