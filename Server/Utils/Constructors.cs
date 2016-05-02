using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using XmlToDynamic;

namespace Times.Server.Utils
{
    public class Dynamic : DynamicObject
    {

        public dynamic addEventListener { get; set; }
        public dynamic addListener { get; set; }
        public dynamic removeEventListener { get; set; }
        public dynamic removeListener { get; set; }
        public dynamic dispatchEvent { get; set; }
        public dynamic updateListeners { get; set; }

        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        private Dictionary<int, object> _indexed = new Dictionary<int, object> { };
        public int length = 0;

        public dynamic remove(dynamic obj)
        {
            try
            {
                if (obj is int)
                {
                    if (this._indexed.ContainsKey(obj)) this._indexed.Remove(obj);
                    return obj;
                }

                var Key2 = this._properties.Values.ToList().IndexOf(obj);
                if (Key2 > -1)
                {
                    this._properties.Remove(this._properties.Keys.ToList()[Key2]);
                }

                return obj;
            }
            catch
            {
                return null;
            }
        }

        public dynamic push(dynamic obj)
        {
            try
            {
                this._indexed[this.length] = obj;
                this.length = this.length + 1;

                return obj;
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<int, object> indexed()
        {
            return this._indexed;
        }

        public dynamic this[dynamic index]
        {
            get
            {
                dynamic ret = null;

                if (index is int)
                {
                    if (this._indexed.ContainsKey(index))
                    {
                        ret = this._indexed[index];
                    }
                }
                else if (index is string)
                {
                    if (this._properties.ContainsKey(index))
                    {
                        PropertyInfo info = this.GetType().GetProperty(index);
                        if (info != null)
                            ret = info.GetValue(this, null);
                    }
                }

                return ret;
            }
            set
            {
                if (index is int)
                {
                    this._indexed[index] = value;

                }
                else if (index is string)
                {
                    this._properties[index] = value;
                }
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _properties[binder.Name] = value;
            return true;
        }
    }
}
