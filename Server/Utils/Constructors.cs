using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using XmlToDynamic;
using System.Windows.Forms;

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

        // remove all variables from this instance
        public void Clear()
        {
            this.length = 0;
            var _loc1_ = this._indexed.Keys.ToList();
            int _loc2_ = _loc1_.Count - 1;

            while (_loc2_ > -1)
            {
                this._indexed.Remove(_loc1_[_loc2_]);
                _loc2_--;
            }

            var _loc3_ = this._properties.Keys.ToList();
            _loc2_ = _loc3_.Count - 1;

            while (_loc2_ > -1)
            {
                this._properties.Remove(_loc3_[_loc2_]);
                _loc2_--;
            }

        }
        
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

        /* DISPOSABLE AREA*/
        
        public void disconnected()
        {
            
        }
    }

    abstract class IAirtower : Dynamic
    {
        /* AIRTOWER PACKET */
        public static string SERVER_MESSAGE_DELIMITER = "%";
        public static string STRING_TYPE = "str";
        public static string XML_TYPE = "xml";
        public static string PLAY_EXT = "s";
        public static string GAME_EXT = "z";
        public static string NAVIGATION = "j";
        public static string PLAYER_HANDLER = "u";
        public static string ITEM_HANDLER = "i";
        public static string IGNORE_HANDLER = "n";
        public static string BUDDY_HANDLER = "b";
        public static string TOY_HANDLER = "t";
        public static string TABLE_HANDLER = "a";
        public static string IGLOO_HANDLER = "g";
        public static string PET_HANDLER = "p";
        public static string MESSAGE_HANDLER = "m";
        public static string MAIL_HANDLER = "l";
        public static string SURVEY_HANDLER = "e";
        public static string WADDLE = "w";
        public static string SETTING_HANDLER = "s";
        public static string MODERATION_HANDLER = "o";
        public static string NINJA_HANDLER = "ni";
        public static string CARD_HANDLER = "cd";
        public static string ROOM_HANDLER = "r";
        public static string NEW_USER_EXPERIENCE_HANDLER = "nx";
        public static string PLAYER_TRANSFORMATION_HANDLER = "pt";
        public static string GHOST_BUSTER_HANDLER = "gb";
        public static string PLAYER_TICKET_HANDLER = "tic";
        public static string COOKIE_BAKERY_HANDLER = "ba";
        public static string BATTLE_ROOM_COUNTDOWN_UPDATE = "brcu";
        public static string BATTLE_ROOM_STATUS_UPDATE = "brsu";
        public static string BATTLE_ROOM_HIT_SNOWBALL = "bhs";
        public static string BATTLE_ROOM_THROW_SNOWBALL = "brts";
        public static string REDEMPTION = "red";
        public static string REDEMPTION_JOIN_WORLD = "rjs";
        public static string HANDLE_LOGIN = "l";
        public static string HANDLE_LOGIN_EXPIRED = "loginMustActivate";
        public static string HANDLE_ALERT = "a";
        public static string HANDLE_ERROR = "e";
        public static string GET_BUDDY_LIST = "gb";
        public static string GET_IGNORE_LIST = "gn";
        public static string GET_PLAYER = "gp";
        public static string GET_ROOM_LIST = "gr";
        public static string GET_TABLE = "gt";
        public static string JOIN_WORLD = "js";
        public static string JOIN_ROOM = "jr";
        public static string CLIENT_ROOM_LOADED = "crl";
        public static string REFRESH_ROOM = "grs";
        public static string LOAD_PLAYER = "lp";
        public static string ADD_PLAYER = "ap";
        public static string REMOVE_PLAYER = "rp";
        public static string UPDATE_PLAYER = "up";
        public static string PLAYER_MOVE = "sp";
        public static string PLAYER_TELEPORT = "tp";
        public static string REFRESH_PLAYER_FRIEND_INFORMATION = "rpfi";
        public static string PLAYER_FRAME = "sf";
        public static string PLAYER_ACTION = "sa";
        public static string OPEN_BOOK = "at";
        public static string CLOSE_BOOK = "rt";
        public static string THROW_BALL = "sb";
        public static string JOIN_GAME = "jg";
        public static string JOIN_NON_BLACK_HOLE_GAME = "jnbhg";
        public static string LEAVE_NON_BLACK_HOLE_GAME = "lnbhg";
        public static string SEND_MESSAGE = "sm";
        public static string SEND_PHRASECHAT_MESSAGE = "sc";
        public static string SEND_BLOCKED_MESSAGE = "mm";
        public static string SEND_EMOTE = "se";
        public static string SEND_JOKE = "sj";
        public static string SEND_SAFE_MESSAGE = "ss";
        public static string SEND_LINE_MESSAGE = "sl";
        public static string SEND_QUICK_MESSAGE = "sq";
        public static string SEND_TOUR_GUIDE_MESSAGE = "sg";
        public static string COIN_DIG_UPDATE = "cdu";
        public static string GET_INVENTORY_LIST = "gi";
        public static string GET_CURRENT_TOTAL_COIN = "gtc";
        public static string NINJA_GET_INVENTORY_LIST = "ngi";
        public static string GET_CURRENCIES = "currencies";
        public static string MAIL_START_ENGINE = "mst";
        public static string GET_MAIL = "mg";
        public static string SEND_MAIL = "ms";
        public static string RECEIVE_MAIL = "mr";
        public static string DELETE_MAIL = "md";
        public static string DELETE_MAIL_FROM_PLAYER = "mdp";
        public static string GET_MAIL_DETAILS = "mgd";
        public static string MAIL_CHECKED = "mc";
        public static string GAME_OVER = "zo";
        public static string BUY_INVENTORY = "ai";
        public static string CHECK_INVENTORY = "qi";
        public static string ADD_IGNORE = "an";
        public static string REMOVE_IGNORE = "rn";
        public static string REMOVE_BUDDY = "rb";
        public static string REQUEST_BUDDY = "br";
        public static string ACCEPT_BUDDY = "ba";
        public static string BUDDY_ONLINE = "bon";
        public static string BUDDY_OFFLINE = "bof";
        public static string FIND_BUDDY = "bf";
        public static string GET_PLAYER_OBJECT = "gp";
        public static string GET_MASCOT_OBJECT = "gmo";
        public static string REPORT_PLAYER = "r";
        public static string GET_ACTION_STATUS = "gas";
        public static string UPDATE_PLAYER_COLOUR = "upc";
        public static string UPDATE_PLAYER_HEAD = "uph";
        public static string UPDATE_PLAYER_FACE = "upf";
        public static string UPDATE_PLAYER_NECK = "upn";
        public static string UPDATE_PLAYER_BODY = "upb";
        public static string UPDATE_PLAYER_HAND = "upa";
        public static string UPDATE_PLAYER_FEET = "upe";
        public static string UPDATE_PLAYER_FLAG = "upl";
        public static string UPDATE_PLAYER_PHOTO = "upp";
        public static string UPDATE_PLAYER_REMOVE = "upr";
        public static string SEND_ACTION_DANCE = "sdance";
        public static string SEND_ACTION_WAVE = "swave";
        public static string SEND_ACTION_SNOWBALL = "ssnowball";
        public static string GET_FURNITURE_LIST = "gii";
        public static string UPDATE_ROOM = "up";
        public static string UPDATE_FLOOR = "ag";
        public static string UPDATE_IGLOO_TYPE = "au";
        public static string BUY_IGLOO_LOCATION = "aloc";
        public static string UNLOCK_IGLOO = "or";
        public static string LOCK_IGLOO = "cr";
        public static string UPDATE_IGLOO_MUSIC = "um";
        public static string GET_IGLOO_DETAILS = "gm";
        public static string JOIN_PLAYER_ROOM = "jp";
        public static string SAVE_IGLOO_FURNITURE = "ur";
        public static string JUMP_TO_IGLOO = "ji";
        public static string GET_IGLOO_LIST = "gr";
        public static string GET_IGLOO_LIST_ITEM = "gri";
        public static string PLAYER_IGLOO_OPEN = "pio";
        public static string BUY_FURNITURE = "af";
        public static string BUY_MULTIPLE_FURNITURE = "buy_multiple_furniture";
        public static string SEND_IGLOO = "sig";
        public static string GET_OWNED_IGLOOS = "go";
        public static string ACTIVATE_IGLOO = "ao";
        public static string GET_MY_PLAYER_PUFFLES = "pgu";
        public static string RETURN_PUFFLE = "prp";
        public static string GET_PLAYER_PUFFLES = "pg";
        public static string PUFFLE_FRAME = "ps";
        public static string PUFFLE_MOVE = "pm";
        public static string PUFFLE_VISITOR_HAT_UPDATE = "puphi";
        public static string ADD_A_PUFFLE = "addpuffle";
        public static string REST_PUFFLE = "pr";
        public static string BATH_PUFFLE = "pb";
        public static string PLAY_PUFFLE = "pp";
        public static string BUBBLE_GUM_PUFFLE = "pbg";
        public static string FEED_PUFFLE = "pf";
        public static string WALK_PUFFLE = "pw";
        public static string TREAT_PUFFLE = "pt";
        public static string SWAP_PUFFLE = "puffleswap";
        public static string WALK_SWAP_PUFFLE = "pufflewalkswap";
        public static string INTERACTION_PLAY = "ip";
        public static string INTERACTION_REST = "ir";
        public static string INTERACTION_FEED = "if";
        public static string PUFFLE_INIT_INTERACTION_PLAY = "pip";
        public static string PUFFLE_INIT_INTERACTION_REST = "pir";
        public static string ADOPT_PUFFLE = "pn";
        public static string PUFFLE_QUIZ_STATUS = "pgas";
        public static string ADD_PUFFLE_CARE_ITEM = "papi";
        public static string UPDATE_TABLE = "ut";
        public static string GET_TABLE_POPULATION = "gt";
        public static string JOIN_TABLE = "jt";
        public static string LEAVE_TABLE = "lt";
        public static string UPDATE_WADDLE = "uw";
        public static string GET_WADDLE_POPULATION = "gw";
        public static string GET_WADDLE_CJ = "gwcj";
        public static string JOIN_WADDLE = "jw";
        public static string LEAVE_WADDLE = "lw";
        public static string START_WADDLE = "sw";
        public static string SEND_WADDLE = "jx";
        public static string CARD_JITSU_MATCH_SUCCESSFUL = "cjms";
        public static string SPY_PHONE_REQUEST = "spy";
        public static string HEARTBEAT = "h";
        public static string TIMEOUT = "t";
        public static string MODERATOR_ACTION = "ma";
        public static string KICK = "k";
        public static string MUTE = "m";
        public static string BAN = "b";
        public static string INT_BAN = "initban";
        public static string SEND_MASCOT_MESSAGE = "sma";
        public static string DONATE = "dc";
        public static string POLL = "spl";
        public static string CONNECTION_LOST = "con";
        public static string GET_CARDS = "gcd";
        public static string GET_NINJA_LEVEL = "gnl";
        public static string GET_FIRE_LEVEL = "gfl";
        public static string GET_WATER_LEVEL = "gwl";
        public static string GET_SNOW_LEVEL = "gsl";
        public static string GET_NINJA_RANKS = "gnr";
        public static string BUY_POWER_CARDS = "bpc";
        public static string SET_SAVED_MAP_CATEGORY = "mcs";
        public static string SET_PLAYER_CARD_OPENED = "pcos";
        public static string SET_VISITED_CONTENT_FLAGS = "vcfs";
        public static string GET_VISITED_CONTENT_FLAGS = "vcf";
        public static string PLAYER_TRANSFORMATION = "spts";
        public static string GET_LAST_REVISION = "glr";
        public static string MOBILE_ACCOUNT_ACTIVATION_REQUIRED = "maar";
        public static string GET_PLAYER_ID_AND_NAME_BY_SWID = "pbs";
        public static string GET_PLAYER_INFO_BY_NAME = "pbn";
        public static string GET_PLAYER_INFO_BY_ID = "pbi";
        public static string GET_PLAYER_IDS_BY_SWIDS = "pbsm";
        public static string PBSM_START = "pbsms";
        public static string PBSM_FINISHED = "pbsmf";
        public static string SCAVENGER_HUNT_NOTIFICATION = "shn";
        public static string GET_SCAVENGER_HUNT_TICKETS = "gptc";
        public static string INC_SCAVENGER_HUNT_TICKETS = "iptc";
        public static string DEC_SCAVENGER_HUNT_TICKETS = "dptc";
        public static string COINS_AWARDED = "$";
        public static string PLAYER_DIRECTOR_POINTS = "pdp";
        public static string GET_COINS_FOR_CHANGE_TOTALS = "gcfct";
        public static string CAN_PURCHASE_COOKIE = "cac";
        public static string PURCHASE_COOKIE = "ac";
        public static string COOKIES_READY = "cr";
        public static string GET_BAKERY_ROOM_STATE = "barsu";
        public static string SEND_SNOWBALL_ENTER_HOPPER = "seh";
        public static string GET_COOKIE_STOCK = "ctc";
        public static string CANCEL_COOKIE_RESERVATION = "cc";
        public static string GET_PARTY_COOKIE = "qpc";
        public static string SET_PARTY_COOKIE = "spd";
        public static string SET_PLAYER_TEST_GROUP_ID = "pigt";
        public static string UPDATE_EGG_TIMER = "uet";
        public static string SNOWBALL_HIT = "sh";
        public static string GET_AB_TEST_DATA = "gabcms";
        public static string GET_ACTIVE_FEATURES = "activefeatures";
    }
}
