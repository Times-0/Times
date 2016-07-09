using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Times.Server.Utils.MySQL
{
    using Server;
    using Server.Utils;
    using Server.Log;
    using Server.net;

    using MySql.Data.MySqlClient;

    class MySQLHandler
    {
        public bool _available = true;

        public bool available
        {
            get { return _available; }
            set
            {
                this.lastInsertID = 0;
                _available = value;
                if (_available)
                {
                    MySQL.getCurrentMySQLObject().PushToAvailable(this);
                } else
                {
                    MySQL.getCurrentMySQLObject().PushToBusyList(this);
                }
            }
        }

        public MySqlConnection conn = null;
        public string query = null;
        public long lastInsertID;

        public MySQLHandler(MySqlConnection connection)
        {
            this.conn = connection;
        }

        public void start(string query)
        {
            this.query = query;
            this.available = false;
        }

        public async void Execute()
        {
            this.conn.Close();

            MySqlCommand command = new MySqlCommand(this.query, this.conn);
            await this.conn.OpenAsync();

            var reader = await command.ExecuteNonQueryAsync();

            this.lastInsertID = command.LastInsertedId;
        }

        public async void ExecuteAndFetch(Delegate callback, params object[] args)
        {
            this.conn.Close();

            MySqlCommand command = new MySqlCommand(this.query, this.conn);
            this.conn.Open();

            var reader = await command.ExecuteReaderAsync();

            args = new object[] { reader }.Concat(args).ToArray();
            callback.DynamicInvoke(args);
            this.available = true;
        }
    }

    class MySQL
    {
        private static MySQL __mysqlObject = null;

        public static int max_conn = 100;

        public List<MySqlConnection> connections = new List<MySqlConnection> { };
        protected List<MySQLHandler> AvailableConnections = new List<MySQLHandler> { };
        public List<dynamic> waitingCallback = new List<dynamic> { };

        public MySQL(string dbAddr, string dbName, string dbUser, string dbPass)
        {
            __mysqlObject = this;
            string MySQLConnectionString = 
                string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};", dbAddr, dbName, dbUser, dbPass);

            Log.Debugger.CallEvent(Airtower.INFO_EVENT, String.Format("MySQL Stating connection - {0} : {1} -> {2} @ YES", dbAddr, dbName, dbUser));
            try
            {
                this.connections = new List<MySqlConnection> { };
                this.connections.Add(new MySqlConnection(MySQLConnectionString));
                for(var i = 1; i < max_conn; i ++)
                {
                    MySqlConnection connection = new MySqlConnection(MySQLConnectionString);
                    this.connections.Add(connection);
                    this.AvailableConnections.Add(new MySQLHandler(connection));
                }
            } catch (Exception e)
            {
                Log.Debugger.CallEvent(Airtower.ERROR_EVENT, String.Format("Unable to start MySQL Connection : {0}", e));
            }
        }

        public void PushToAvailable(MySQLHandler handler)
        {
            if (!this.AvailableConnections.Contains(handler))
                this.AvailableConnections.Add(handler);

            this.NextCallback();
        }

        public void PushToBusyList(MySQLHandler handler)
        {
            if (this.AvailableConnections.Contains(handler))
            {
                this.AvailableConnections.Remove(handler);
            }
        }

        public MySqlDataReader ExecuteAndFetch(string query)
        {
            AutoResetEvent aev = new AutoResetEvent(false);
            MySqlDataReader toreturn = null;

            var _loc1_ = new Action<MySqlDataReader>((MySqlDataReader reader) =>
            {
                toreturn = reader;
                aev.Set();
            });

            this.MySQLCallback(query, _loc1_);
            while(true)
            {
                aev.WaitOne();
                break;
            }

            return toreturn;            
        }

        public void MySQLCallback(string query, Delegate delg = null, params object[] args)
        {
            if (query == "" || query == null) return;

            this.waitingCallback.Add(new List<dynamic> { query, delg, args });
            this.NextCallback();
        }

        public void NextCallback()
        {
            while (this.AvailableConnections.Count > 0 && this.waitingCallback.Count > 0)
            {
                MySQLHandler handler = this.AvailableConnections[0];
                this.AvailableConnections.RemoveAt(0);
                var callback = this.waitingCallback[0];
                this.waitingCallback.RemoveAt(0);

                if (callback[1] == null)
                {
                    handler.start(callback[0]);
                    handler.Execute();
                } else
                {
                    handler.start(callback[0]);
                    handler.ExecuteAndFetch(callback[1], callback[2]);
                }
            }
            
        }

        public static MySQL getCurrentMySQLObject()
        {
            return __mysqlObject;
        }
    }
}
