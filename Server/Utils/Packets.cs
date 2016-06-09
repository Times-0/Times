using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Times.Server.Utils
{
    static class Packets
    {
        public const int InvalidPacket = -1;
        public const int Parsed = 1;
        public const int XML_DATA = 2;
        public const int XT_DATA = 3;

        public static int parseData(string data, dynamic dynObj)
        {
            if (data.StartsWith("%") && data.EndsWith("%"))
            {
                data = data.Skip(1).Take(data.Length - 2).ToString();
                string[] packets = data.Split(char.Parse("%"));
                if (packets[0] != "xt")
                    return InvalidPacket;

                for(int i = 1; i < packets.Length; i++)
                {
                    dynObj.clientData[i - 1] = packets[i];
                }

                return XT_DATA;
            } else
            if (data.StartsWith("<"))
            {
                if (data == "<policy-file-request/>")
                    return XML_DATA;
                try
                {
                    dynObj.clientData = (Dynamic)new XmlSerializer(typeof(Dynamic)).Deserialize(data.GetStream());

                    return XML_DATA;
                } catch (Exception exc)
                {
                    return InvalidPacket;
                }
            }

            return InvalidPacket;
        }

        public static int Parse(string data, Penguin pengObj)
        {
            pengObj.clientData.Clear();
            if (data == "" || data == null)
                return InvalidPacket;

            var parsed = parseData(data, pengObj);

            return parsed;
        }

        public static void HandleXTPacket(Penguin peng)
        {
            try
            {
                if (peng.clientData == null)
                    return;
                var category = peng.clientData[0];
                var handler = peng.clientData[1];

                var vars = new List<string> { };
                for(var i = 2; i < peng.clientData.length; i++)
                {
                    vars.Add(peng.clientData[i]);
                }

                var callbackHandlerName = String.Format("{0}/{1}/", category, handler);

            }
            catch (Exception) { }
        }
    }
}
