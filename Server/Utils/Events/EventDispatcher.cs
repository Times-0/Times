using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Windows.Forms;

namespace Times.Server.Utils.Events
{
    class EventDispatcher : DynamicObject
    {
        private static ExpandoObject _dynamicObj = new ExpandoObject();
        public static EventDispatcher _dispatcher = new EventDispatcher();
        public Dictionary<string, dynamic> _listeners = null;

        public EventDispatcher()
        {

        }

        public bool addListener(string e_type, Delegate e_handler, DynamicObject e_scope = null)
        {
            return this.addEventListener(e_type, e_handler, e_scope);
        }
        
        public bool removeListener(string e_type, Delegate e_handler, DynamicObject e_scope = null)
        {
            return this.removeEventListener(e_type, e_handler, e_scope);
        }

        public bool addEventListener(string e_type, Delegate e_handler, DynamicObject e_scope = null)
        {
            List<dynamic> _loc1_ = EventDispatcher.getListenersList(this, e_type);
            int _loc2_ = EventDispatcher.getListenerIndex(_loc1_, e_handler, e_scope);

            if (_loc2_ == -1)
            {
                dynamic _loc3_ = new ExpandoObject();
                _loc3_.handler = e_handler;
                _loc3_.scope = e_scope;
                _loc1_.Add(_loc3_);

                return true;
            }
            
            return false;
        }

        public bool removeEventListener(string type, Delegate handler, DynamicObject scope)
        {
            var _loc1_ = EventDispatcher.getListenersList(this, type);
            int _loc2_ = EventDispatcher.getListenerIndex(_loc1_, handler, scope);

            if (_loc2_ != -1)
            {
                _loc1_.RemoveAt(_loc2_);
                return true;
            }

            return false;
        }

        public bool updateListeners(string type, dynamic Event = null, params dynamic[] args)
        {
            if (Event == null)
            {
                Event = new ExpandoObject();
            }

            Event.type = type;
            return this.dispatchEvent(Event, args);
        }

        public bool dispatchEvent(dynamic Event, params object[] args)
        {
            var _loc1_ = EventDispatcher.getListenersList(_dispatcher, Event.type);
            int _loc2_ = _loc1_.Count;

            if (_loc2_ < 1)
            {
                return false;
            }

            int _loc3_ = 0;
            while (_loc3_ < _loc2_)
            {
                var handler = _loc1_[_loc3_].handler;
                
                if (_loc1_[_loc3_].scope == null)
                {
                    handler.DynamicInvoke(new Object[] { Event }.Concat(args).ToArray());
                } else
                {
                    handler.Invoke(new Object[] { Event }.Concat(args).ToArray());
                }
                    
                _loc3_ = _loc3_ + 1;
            }

            return true;
        }

        public static int getListenerIndex(List<dynamic> listeners, Delegate e_handler, DynamicObject scope=null)
        {
            int _loc1_ = listeners.Count;
            int _loc2_ = 0;

            while (_loc2_ < _loc1_)
            {
                if (listeners[_loc2_].handler == e_handler && (listeners[_loc2_].scope == scope || scope == null))
                {
                    return _loc2_;
                }

                _loc2_ = _loc2_ + 1;
            }

            return -1;
        }

        public static List<dynamic> getListenersList(DynamicObject event_source, string e_type)
        {
            dynamic source = event_source;
            if (source._listeners == null)
            {
                source._listeners = new Dictionary<string, dynamic> { };
            }

            if (!source._listeners.ContainsKey(e_type))
            {
                source._listeners[e_type] = new List<dynamic>();
            }

            return source._listeners[e_type];

        }

        public static void initialize(dynamic obj)
        {
            
            if (_dispatcher == null)
            {
                _dispatcher = new EventDispatcher();
            }
            

            obj.addEventListener = EventDelegate.create(_dispatcher, "addEventListener");
            obj.addListener = EventDelegate.create(_dispatcher, "addEventListener");
            obj.removeEventListener = EventDelegate.create(_dispatcher, "removeEventListener");
            obj.removeListener = EventDelegate.create(_dispatcher, "removeEventListener");
            obj.dispatchEvent = EventDelegate.create(_dispatcher, "dispatchEvent");
            obj.updateListeners = EventDelegate.create(_dispatcher, "updateListeners");

        }
    }
    
}
