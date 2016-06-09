using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Dynamic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Times.Server.Utils
{
    using Client;

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
        
        public static Dynamic ToDynamic<T>(this List<T> @list)
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
        }
    }
}
