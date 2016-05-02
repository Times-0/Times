using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

namespace Times.Server.Utils.Events
{
    class EventDelegate
    {

        public EventDelegate()
        {

        }

        public static Delegate create(object target, string method)
        {
            Type _loc1_;
            var _loc3_ = target;
            var _loc5_ = method;

            var _loc6_ = _loc3_.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            List<Type> args = _loc6_.GetParameters().Select(x => x.ParameterType).ToList();
            
            if (_loc6_.ReturnType == typeof(void))
            {
                _loc1_ = Expression.GetActionType(args.ToArray());
            }
            else
            {
                args.Add(_loc6_.ReturnType);
                _loc1_ = Expression.GetFuncType(args.ToArray());
            }
            Delegate _loc7_ = Delegate.CreateDelegate(_loc1_, _loc3_, _loc6_);
            return _loc7_;
        }

    }
}
