using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace Times.Server.Utils
{
    using Client;

    static class Cryptography
    {
        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random((int)DateTime.Now.Ticks);
            char ch;

            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string randomKey()
        {
            return String.Format("{0}-{1}${2}", RandomString(4), RandomString(5), RandomString(3));
        }

        public static string GenerateLoginHash(string pword, string randomKey)
        {
            string hash = pword.swap(16);
            hash += randomKey;
            hash += "a1ebe00441f5aecb185d0ec178ca2305Y(02.>'H}t\":E1_root";

            return hash.md5().swap(16);
        }
    }

    internal static class Tools
    {
        public static string join<T>(this List<T> @list, string gum)
        {
            string joinedString = "";

            for (var i = 0; i < @list.Count; i++)
            {
                if (@list[i] == null) joinedString += "null";
                else joinedString += @list[i].ToString();
                if (i < @list.Count - 1) joinedString += gum;
            }

            return joinedString;
        }

        public static string preetify(this string @string)
        {
            string[] Strings = @string.Split(char.Parse(" "));
            string str = "";

            for (var i = 0; i < Strings.Length; i++)
            {
                string S = Strings[i];
                str += S[0].ToString().ToUpper();
                str += S.Substring(1).ToLower();
                if (i < Strings.Length - 1) str += " ";
            }

            return str;
        }

        public static Stream GetStream(this string @str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(@str);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        public static void Call(this object @obj, string @str, object[] @args)
        {
            try
            {
                Type type = @obj.GetType();
                type.GetMethod(@str).Invoke(@obj, args);
            }
            catch { }
        }
        /// <summary>
        /// Creates a Delegate callback for given handler, and adds it to Received-XML Event
        /// </summary>
        /// <param name="@obj">Current class, or Event class</param>
        /// <param name="@handle">Callback handler</param>
        /// <param name="@handler">Callback function name</param>
        public static void AddXMLHandler(this object @obj, string @handle, string @handler)
        {
            Delegate dlg = Events.EventDelegate.create(@obj, @handler);

            net.Airtower @o = net.Airtower.getCurrentAirtower();
            @o.addEventListener(String.Format("#xml{0}/", @handle), dlg, null);
        }

        public static void AddXTHandler(this object @obj, string handle, string handle1, string @handler)
        {
            Delegate dlg = Events.EventDelegate.create(@obj, @handler);

            net.Airtower @o = net.Airtower.getCurrentAirtower();
            @o.addEventListener(String.Format("#xt{0}-{1}/", handle, handle1), dlg, null);
        }

        public static void AddEventHandler (this object @obj, string tohadle, string handler)
        {
            Delegate dlg = Events.EventDelegate.create(@obj, handler);

            net.Airtower @o = net.Airtower.getCurrentAirtower();
            @o.addEventListener(tohadle, dlg, null);
        }

        public static Penguin GetPenguinById(this Dictionary<Socket, Penguin> @o, string id)
        {
            foreach(var i in @o)
            {
                if (i.Value.id == id)
                    return i.Value;
            }

            return null;
        }

        public static String GetURLContents(this string @url)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();

            return webClient.DownloadString(@url);
        }

        public static Dictionary<string, string> GetDictFromReader(this MySqlDataReader @reader)
        {
            Dictionary<string, string> details = new Dictionary<string, string> { };
            if (@reader.Read())
            {
                details = Enumerable.Range(0, @reader.FieldCount).ToDictionary
                    (i => @reader.GetName(i), i => @reader.GetValue(i).ToString());
            }

            return details;
        }

        public static List<Dictionary<string, string>> GetListFromDict(this MySqlDataReader @reader)
        {
            List<Dictionary<string, string>> details = new List<Dictionary<string, string>> { };

            while (@reader.Read())
            {
                details.Add(Enumerable.Range(0, @reader.FieldCount).ToDictionary
                    (i => @reader.GetName(i), i => @reader.GetValue(i).ToString()));                
            }

            return details;
        }

        public static string swap(this string @string, int index)
        {
            string x = @string;

            if (x.Length <= index)
            {
                return x;
            }
            else
            {
                return x.Substring(index) + x.Substring(0, index);
            }

        }

        public static string md5(this string @string)
        {
            MD5 hash = new MD5CryptoServiceProvider();
            hash.Initialize();

            byte[] buffer = Encoding.ASCII.GetBytes(@string);
            byte[] md5 = hash.ComputeHash(buffer);

            return BitConverter.ToString(md5).Replace("-", "").ToLower();

        }

        public static bool userInServer(this string id, int port)
        {
            var users = Server.net.Airtower.Clients.Values.ToList();
            for (var i = 0; i < users.Count; i++)
            {
                if (users[i].id == id)
                {
                    return true;
                }
            }

            return false;
        }

        /*public static Dynamic ToDynamic<T>(this List<T> @list)
        {
            Dynamic _loc1_ = new Dynamic();

            int _loc3_ = @list.Count;
            int _loc2_ = 0;
            while (_loc2_ < _loc3_)
            {
                _loc1_.push(@list[_loc2_]);
                _loc2_ = _loc2_ + 1;
            }

            return _loc1_;
        }*/
    }
}
