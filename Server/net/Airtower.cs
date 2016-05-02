using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Dynamic;
using System.Xml.Linq;

namespace Times.Server.net
{
    using Utils;
    using XmlToDynamic;

    class Airtower : Dynamic
    {

        public string SERVER_MESSAGE_DELIMITER = "%";
        public string STRING_TYPE = "str";
        public string XML_TYPE = "xml";
        public string PLAY_EXT = "s";
        public string GAME_EXT = "z";
        public string NAVIGATION = "j";
        public string PLAYER_HANDLER = "u";
        public string ITEM_HANDLER = "i";
        public string IGNORE_HANDLER = "n";
        public string BUDDY_HANDLER = "b";
        public string TOY_HANDLER = "t";
        public string TABLE_HANDLER = "a";
        public string IGLOO_HANDLER = "g";
        public string PET_HANDLER = "p";
        public string MESSAGE_HANDLER = "m";
        public string MAIL_HANDLER = "l";
        public string SURVEY_HANDLER = "e";
        public string WADDLE = "w";
        public string SETTING_HANDLER = "s";
        public string MODERATION_HANDLER = "o";
        public string NINJA_HANDLER = "ni";
        public string CARD_HANDLER = "cd";
        public string ROOM_HANDLER = "r";
        public string NEW_USER_EXPERIENCE_HANDLER = "nx";
        public string PLAYER_TRANSFORMATION_HANDLER = "pt";
        public string GHOST_BUSTER_HANDLER = "gb";
        public string PLAYER_TICKET_HANDLER = "tic";
        public string COOKIE_BAKERY_HANDLER = "ba";
        public string BATTLE_ROOM_COUNTDOWN_UPDATE = "brcu";
        public string BATTLE_ROOM_STATUS_UPDATE = "brsu";
        public string BATTLE_ROOM_HIT_SNOWBALL = "bhs";
        public string BATTLE_ROOM_THROW_SNOWBALL = "brts";
        public string REDEMPTION = "red";
        public string REDEMPTION_JOIN_WORLD = "rjs";
        public string HANDLE_LOGIN = "l";
        public string HANDLE_LOGIN_EXPIRED = "loginMustActivate";
        public string HANDLE_ALERT = "a";
        public string HANDLE_ERROR = "e";
        public string GET_BUDDY_LIST = "gb";
        public string GET_IGNORE_LIST = "gn";
        public string GET_PLAYER = "gp";
        public string GET_ROOM_LIST = "gr";
        public string GET_TABLE = "gt";
        public string JOIN_WORLD = "js";
        public string JOIN_ROOM = "jr";
        public string CLIENT_ROOM_LOADED = "crl";
        public string REFRESH_ROOM = "grs";
        public string LOAD_PLAYER = "lp";
        public string ADD_PLAYER = "ap";
        public string REMOVE_PLAYER = "rp";
        public string UPDATE_PLAYER = "up";
        public string PLAYER_MOVE = "sp";
        public string PLAYER_TELEPORT = "tp";
        public string REFRESH_PLAYER_FRIEND_INFORMATION = "rpfi";
        public string PLAYER_FRAME = "sf";
        public string PLAYER_ACTION = "sa";
        public string OPEN_BOOK = "at";
        public string CLOSE_BOOK = "rt";
        public string THROW_BALL = "sb";
        public string JOIN_GAME = "jg";
        public string JOIN_NON_BLACK_HOLE_GAME = "jnbhg";
        public string LEAVE_NON_BLACK_HOLE_GAME = "lnbhg";
        public string SEND_MESSAGE = "sm";
        public string SEND_PHRASECHAT_MESSAGE = "sc";
        public string SEND_BLOCKED_MESSAGE = "mm";
        public string SEND_EMOTE = "se";
        public string SEND_JOKE = "sj";
        public string SEND_SAFE_MESSAGE = "ss";
        public string SEND_LINE_MESSAGE = "sl";
        public string SEND_QUICK_MESSAGE = "sq";
        public string SEND_TOUR_GUIDE_MESSAGE = "sg";
        public string COIN_DIG_UPDATE = "cdu";
        public string GET_INVENTORY_LIST = "gi";
        public string GET_CURRENT_TOTAL_COIN = "gtc";
        public string NINJA_GET_INVENTORY_LIST = "ngi";
        public string GET_CURRENCIES = "currencies";
        public string MAIL_START_ENGINE = "mst";
        public string GET_MAIL = "mg";
        public string SEND_MAIL = "ms";
        public string RECEIVE_MAIL = "mr";
        public string DELETE_MAIL = "md";
        public string DELETE_MAIL_FROM_PLAYER = "mdp";
        public string GET_MAIL_DETAILS = "mgd";
        public string MAIL_CHECKED = "mc";
        public string GAME_OVER = "zo";
        public string BUY_INVENTORY = "ai";
        public string CHECK_INVENTORY = "qi";
        public string ADD_IGNORE = "an";
        public string REMOVE_IGNORE = "rn";
        public string REMOVE_BUDDY = "rb";
        public string REQUEST_BUDDY = "br";
        public string ACCEPT_BUDDY = "ba";
        public string BUDDY_ONLINE = "bon";
        public string BUDDY_OFFLINE = "bof";
        public string FIND_BUDDY = "bf";
        public string GET_PLAYER_OBJECT = "gp";
        public string GET_MASCOT_OBJECT = "gmo";
        public string REPORT_PLAYER = "r";
        public string GET_ACTION_STATUS = "gas";
        public string UPDATE_PLAYER_COLOUR = "upc";
        public string UPDATE_PLAYER_HEAD = "uph";
        public string UPDATE_PLAYER_FACE = "upf";
        public string UPDATE_PLAYER_NECK = "upn";
        public string UPDATE_PLAYER_BODY = "upb";
        public string UPDATE_PLAYER_HAND = "upa";
        public string UPDATE_PLAYER_FEET = "upe";
        public string UPDATE_PLAYER_FLAG = "upl";
        public string UPDATE_PLAYER_PHOTO = "upp";
        public string UPDATE_PLAYER_REMOVE = "upr";
        public string SEND_ACTION_DANCE = "sdance";
        public string SEND_ACTION_WAVE = "swave";
        public string SEND_ACTION_SNOWBALL = "ssnowball";
        public string GET_FURNITURE_LIST = "gii";
        public string UPDATE_ROOM = "up";
        public string UPDATE_FLOOR = "ag";
        public string UPDATE_IGLOO_TYPE = "au";
        public string BUY_IGLOO_LOCATION = "aloc";
        public string UNLOCK_IGLOO = "or";
        public string LOCK_IGLOO = "cr";
        public string UPDATE_IGLOO_MUSIC = "um";
        public string GET_IGLOO_DETAILS = "gm";
        public string JOIN_PLAYER_ROOM = "jp";
        public string SAVE_IGLOO_FURNITURE = "ur";
        public string JUMP_TO_IGLOO = "ji";
        public string GET_IGLOO_LIST = "gr";
        public string GET_IGLOO_LIST_ITEM = "gri";
        public string PLAYER_IGLOO_OPEN = "pio";
        public string BUY_FURNITURE = "af";
        public string BUY_MULTIPLE_FURNITURE = "buy_multiple_furniture";
        public string SEND_IGLOO = "sig";
        public string GET_OWNED_IGLOOS = "go";
        public string ACTIVATE_IGLOO = "ao";
        public string GET_MY_PLAYER_PUFFLES = "pgu";
        public string RETURN_PUFFLE = "prp";
        public string GET_PLAYER_PUFFLES = "pg";
        public string PUFFLE_FRAME = "ps";
        public string PUFFLE_MOVE = "pm";
        public string PUFFLE_VISITOR_HAT_UPDATE = "puphi";
        public string ADD_A_PUFFLE = "addpuffle";
        public string REST_PUFFLE = "pr";
        public string BATH_PUFFLE = "pb";
        public string PLAY_PUFFLE = "pp";
        public string BUBBLE_GUM_PUFFLE = "pbg";
        public string FEED_PUFFLE = "pf";
        public string WALK_PUFFLE = "pw";
        public string TREAT_PUFFLE = "pt";
        public string SWAP_PUFFLE = "puffleswap";
        public string WALK_SWAP_PUFFLE = "pufflewalkswap";
        public string INTERACTION_PLAY = "ip";
        public string INTERACTION_REST = "ir";
        public string INTERACTION_FEED = "if";
        public string PUFFLE_INIT_INTERACTION_PLAY = "pip";
        public string PUFFLE_INIT_INTERACTION_REST = "pir";
        public string ADOPT_PUFFLE = "pn";
        public string PUFFLE_QUIZ_STATUS = "pgas";
        public string ADD_PUFFLE_CARE_ITEM = "papi";
        public string UPDATE_TABLE = "ut";
        public string GET_TABLE_POPULATION = "gt";
        public string JOIN_TABLE = "jt";
        public string LEAVE_TABLE = "lt";
        public string UPDATE_WADDLE = "uw";
        public string GET_WADDLE_POPULATION = "gw";
        public string GET_WADDLE_CJ = "gwcj";
        public string JOIN_WADDLE = "jw";
        public string LEAVE_WADDLE = "lw";
        public string START_WADDLE = "sw";
        public string SEND_WADDLE = "jx";
        public string CARD_JITSU_MATCH_SUCCESSFUL = "cjms";
        public string SPY_PHONE_REQUEST = "spy";
        public string HEARTBEAT = "h";
        public string TIMEOUT = "t";
        public string MODERATOR_ACTION = "ma";
        public string KICK = "k";
        public string MUTE = "m";
        public string BAN = "b";
        public string INT_BAN = "initban";
        public string SEND_MASCOT_MESSAGE = "sma";
        public string DONATE = "dc";
        public string POLL = "spl";
        public string CONNECTION_LOST = "con";
        public string GET_CARDS = "gcd";
        public string GET_NINJA_LEVEL = "gnl";
        public string GET_FIRE_LEVEL = "gfl";
        public string GET_WATER_LEVEL = "gwl";
        public string GET_SNOW_LEVEL = "gsl";
        public string GET_NINJA_RANKS = "gnr";
        public string BUY_POWER_CARDS = "bpc";
        public string SET_SAVED_MAP_CATEGORY = "mcs";
        public string SET_PLAYER_CARD_OPENED = "pcos";
        public string SET_VISITED_CONTENT_FLAGS = "vcfs";
        public string GET_VISITED_CONTENT_FLAGS = "vcf";
        public string PLAYER_TRANSFORMATION = "spts";
        public string GET_LAST_REVISION = "glr";
        public string MOBILE_ACCOUNT_ACTIVATION_REQUIRED = "maar";
        public string GET_PLAYER_ID_AND_NAME_BY_SWID = "pbs";
        public string GET_PLAYER_INFO_BY_NAME = "pbn";
        public string GET_PLAYER_INFO_BY_ID = "pbi";
        public string GET_PLAYER_IDS_BY_SWIDS = "pbsm";
        public string PBSM_START = "pbsms";
        public string PBSM_FINISHED = "pbsmf";
        public string SCAVENGER_HUNT_NOTIFICATION = "shn";
        public string GET_SCAVENGER_HUNT_TICKETS = "gptc";
        public string INC_SCAVENGER_HUNT_TICKETS = "iptc";
        public string DEC_SCAVENGER_HUNT_TICKETS = "dptc";
        public string COINS_AWARDED = "$";
        public string PLAYER_DIRECTOR_POINTS = "pdp";
        public string GET_COINS_FOR_CHANGE_TOTALS = "gcfct";
        public string CAN_PURCHASE_COOKIE = "cac";
        public string PURCHASE_COOKIE = "ac";
        public string COOKIES_READY = "cr";
        public string GET_BAKERY_ROOM_STATE = "barsu";
        public string SEND_SNOWBALL_ENTER_HOPPER = "seh";
        public string GET_COOKIE_STOCK = "ctc";
        public string CANCEL_COOKIE_RESERVATION = "cc";
        public string GET_PARTY_COOKIE = "qpc";
        public string SET_PARTY_COOKIE = "spd";
        public string SET_PLAYER_TEST_GROUP_ID = "pigt";
        public string UPDATE_EGG_TIMER = "uet";
        public string SNOWBALL_HIT = "sh";
        public string GET_AB_TEST_DATA = "gabcms";
        public string GET_ACTIVE_FEATURES = "activefeatures";

        public Object _shell;
        public List<Object> Penguins;

        public string MessageDelimiter = "%";
        public bool debug = true;
        public Dictionary<Object, List<string>> SendCommandBuffers = new Dictionary<object, List<string>> { };

        protected Connection server;

        public Airtower(Object shell)
        {
            this._shell = shell;
            this.server = new Connection(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.prepare();
        }

        public void prepare()
        {
            Utils.Events.EventDispatcher.initialize(this);
            this.addListener("initialize", Utils.Events.EventDelegate.create(this, "initialize"), this);
            this.server.onExtensionMessage = Utils.Events.EventDelegate.create(this, "OnAirtowerResponse");
            this.server.XMLReceived = Utils.Events.EventDelegate.create(this, "XMLReceived");
        }
        

        public Connection GetServer()
        {
            return this.server;
        }

        public void initialize(dynamic events)
        {
            this.Penguins = new List<object> { };
        }

        public void XMLReceived(object penguin, string Data)
        {
            try
            {
                dynamic msg = XElement.Parse(Data).ToDynamic();
                var body = msg.body;

                string action = body["action"];

                this.updateListeners(action, body);
            }
            catch { }
        }

        public void OnAirtowerResponse(object penguin, List<dynamic> data, string type)
        {
            try
            {
                string _loc1_ = (String)data[0];
                data.RemoveRange(0, 2);

                Dynamic _loc2_ = data.ToDynamic();

                this.updateListeners(_loc1_, _loc2_);
            }
            catch { }

        }

    }
}
