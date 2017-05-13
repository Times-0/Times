using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Times.Client.Base
{
    static class ItemDetails
    {
        private static Func<String, Boolean, dynamic, Tuple<string, bool, dynamic>> ProduceTuple = (a, b, c) =>
        {
            return new Tuple<string, bool, dynamic>(a, b, c);
            //       identifer^ required^ default^
        };

        public static Dictionary<string, Tuple<string, Boolean, dynamic>> details = new Dictionary<string, Tuple<string, Boolean, dynamic>>
        {
            {"paper_item_id", ProduceTuple("id", true, null)},
            {"type", ProduceTuple("type", true, null)},
            {"cost", ProduceTuple("cost", false, 0)},
            {"is_member", ProduceTuple("member", false, false)},
            {"label", ProduceTuple("name", true, null)},
            {"prompt", ProduceTuple("display", false, null) },
            {"layer", ProduceTuple("layer", false, 0) },
            {"is_epf", ProduceTuple("epf", false, false) },
            {"is_bait", ProduceTuple("bait", false, false) }
        };
    }

    class Item
    {

        private Dictionary<string, dynamic> _ItemDetails;

        public int id
        { get { return int.Parse(_ItemDetails["id"]); } }

        public int type
        { get { return int.Parse(_ItemDetails["type"]); } }

        public int cost
        { get { return int.Parse(_ItemDetails["cost"]); } }

        public bool member
        { get { return Convert.ToBoolean(_ItemDetails["member"]); } }

        public string name
        { get { return Convert.ToString(_ItemDetails["name"]); } }

        public string display
        { get { return Convert.ToString(_ItemDetails["display"] != null ? _ItemDetails["name"] : name); } }

        public int layer
        { get { return int.Parse(_ItemDetails["layer"]); } }

        public bool epf
        { get { return Convert.ToBoolean(_ItemDetails["epf"]); } }

        public bool bait
        { get { return Convert.ToBoolean(_ItemDetails["bait"]); } }

        public Item(Dictionary<string, string> data)
        {
            var keys = ItemDetails.details.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                var value = ItemDetails.details[key];

                if (!data.ContainsKey(key))
                    if (value.Item2)
                        throw new KeyNotFoundException(key);
                    else
                        this._ItemDetails[value.Item1] = value.Item3;

                var type = ((object)value.Item3).GetType();

                this._ItemDetails[value.Item1] = data[key];
            }
        }
    }
}
