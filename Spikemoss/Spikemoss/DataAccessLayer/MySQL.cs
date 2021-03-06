﻿using System;
using MySql.Data.MySqlClient;
using Spikemoss.Models;
using System.Data;
using System.Collections.Generic;

namespace Spikemoss.DataAccessLayer
{
    class MySQL : IDataAccessLayer
    {
        private string _connectionString;
        private SimpleAES _aes;
        private static MySQL _instance;
        private const string DATABASE_NAME = "spikemoss";

        //Used internally to ensure thready safety by locking a private object
        private static readonly object padlock = new object();

        public static MySQL Instance
        {
            get
            {
                lock(padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new MySQL();
                    }
                    return _instance;
                }
            }
        }

        private MySQL()
        {
            _aes = new SimpleAES();
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public DataAccessLayerType IdalType
        {
            get
            {
                return DataAccessLayerType.MySQL;
            }
        }

        public void CreateDatabase()
        {
            using (var con = new MySqlConnection(_connectionString))
            {
                con.Open();
                //SQL injection vulnerability
                var cmd = new MySqlCommand("CREATE DATABASE " + DATABASE_NAME, con);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            //Edit connection string to enable connections to newly created database
            var constr = new MySqlConnectionStringBuilder();

            constr.ConnectionString = _connectionString;
            constr.Database = DATABASE_NAME;
            _connectionString = constr.ConnectionString;
        }

        public void CreateTables()
        {           
            using (var con = new MySqlConnection(_connectionString))
            {
                con.Open();
                CreateUserTable(con);
                CreateProcessorLookupTable(con);
                CreateClusterTable(con);
                CreateServerTable(con);
                CreateHardwareTable(con);
                con.Close();
            }
        }

        public List<Server> GetAllServers()
        {
            var list = new List<Server>();
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT * FROM server;", con);
                using (MySqlDataReader data = cmd.ExecuteReader())
                {
                    while (data.Read())
                    {
                        list.Add(ReadServer(data));
                    }
                }
                con.Close();
            }
            return list;
        }

        public void DeleteServer(int id)
        {
        }

        public Server GetServer(int id)
        {
            return null;
        }

        public void InsertServer(Server server)
        {
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "INSERT INTO server"
                        + "(ClusterID,"
                        + "UserID,"
                        + "VirtualHostID,"
                        + "Address,"
                        + "Hostname,"
                        + "SSHPort,"
                        + "Error,"
                        + "OperatingSystemType,"
                        + "ServerType,"
                        + "IsConfigured,"
                        + "IsVirtual)"
                        + " VALUES ("
                        + "@ClusterID,"
                        + "@UserID,"
                        + "@VirtualHostID,"
                        + "@Address,"
                        + "@Hostname,"
                        + "@SSHPort,"
                        + "@Error,"
                        + "@OperatingSystemType,"
                        + "@ServerType,"
                        + "@IsConfigured,"
                        + "@IsVirtual);"
                        + "SELECT LAST_INSERT_ID();";
                var cmd = new MySqlCommand(query, con);
                
                if (server.ClusterID == 0)
                {
                    cmd.Parameters.AddWithValue("@ClusterID", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ClusterID", server.ClusterID);
                }
                if (server.User.UserID == 0)
                {
                    cmd.Parameters.AddWithValue("@UserID", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@UserID", server.User.UserID);
                }
                if (server.VirtualHostID == 0)
                {
                    cmd.Parameters.AddWithValue("@VirtualHostID", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@VirtualHostID", server.VirtualHostID);
                }                
                cmd.Parameters.AddWithValue("@Address", server.Address);
                cmd.Parameters.AddWithValue("@Hostname", server.Hostname);
                cmd.Parameters.AddWithValue("@SSHPort", server.SSHPort);
                cmd.Parameters.AddWithValue("@Error", server.Error);
                cmd.Parameters.AddWithValue("@OperatingSystemType", server.OperatingSystem.ToString());
                cmd.Parameters.AddWithValue("@ServerType", server.ServerType.ToString());
                cmd.Parameters.AddWithValue("@IsConfigured", server.IsConfigured);
                cmd.Parameters.AddWithValue("@IsVirtual", server.IsVirtual);
                server.ServerID = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateServer(Server server)
        {
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "UPDATE server SET "
                        + "ClusterID=@ClusterID,"
                        + "UserID=@UserID,"
                        + "VirtualHostID=@VirtualHostID,"
                        + "Address=@Address,"
                        + "Hostname=@Hostname,"
                        + "SSHPort=@SSHPort,"
                        + "Error=@Error,"
                        + "OperatingSystemType=@OperatingSystemType,"
                        + "ServerType=@ServerType,"
                        + "IsConfigured=@IsConfigured,"
                        + "IsVirtual=@IsVirtual "
                        + "WHERE ServerID=@ServerID;";
                var cmd = new MySqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ServerID", server.ServerID);
                if (server.ClusterID == 0)
                {
                    cmd.Parameters.AddWithValue("@ClusterID", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ClusterID", server.ClusterID);
                }
                if (server.User == null)
                {
                    cmd.Parameters.AddWithValue("@UserID", null);
                }
                else if (server.User.UserID == 0)
                {
                    cmd.Parameters.AddWithValue("@UserID", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@UserID", server.User.UserID);
                }
                if (server.VirtualHostID == 0)
                {
                    cmd.Parameters.AddWithValue("@VirtualHostID", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@VirtualHostID", server.VirtualHostID);
                }
                cmd.Parameters.AddWithValue("@Address", server.Address);
                cmd.Parameters.AddWithValue("@Hostname", server.Hostname);
                cmd.Parameters.AddWithValue("@SSHPort", server.SSHPort);
                cmd.Parameters.AddWithValue("@Error", server.Error);
                cmd.Parameters.AddWithValue("@OperatingSystemType", server.OperatingSystem.ToString());
                cmd.Parameters.AddWithValue("@ServerType", server.ServerType.ToString());
                cmd.Parameters.AddWithValue("@IsConfigured", server.IsConfigured);
                cmd.Parameters.AddWithValue("@IsVirtual", server.IsVirtual);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Cluster> GetAllClusters()
        {
            var list = new List<Cluster>();
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT * FROM cluster;", con);
                using (MySqlDataReader data = cmd.ExecuteReader())
                {
                    if (data.HasRows)
                    {
                        while (data.Read())
                        {
                            var cluster = new Cluster();
                            cluster.ClusterID = data.GetInt32("ClusterID");
                            cluster.Name = data.GetString("Name");
                            list.Add(cluster);
                        }
                    }                    
                }
                con.Close();
            }
            return list;
        }

        public void DeleteCluster(int id)
        {
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "DELETE FROM cluster "
                        + "WHERE ClusterID=@ClusterID;";
                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClusterID", id);

                cmd.ExecuteNonQuery();
            }
        }

        public Cluster GetCluster(int id)
        {
            return null;
        }

        public List<Server> GetClusterServers(Cluster cluster)
        {
            var list = new List<Server>();
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                if (cluster.ClusterID > 0)
                {
                    var cmd = new MySqlCommand("SELECT * FROM server WHERE ClusterID=@clusterID;", con);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                list.Add(ReadServer(reader));
                            }
                        }
                    }
                }
                else if (cluster.ClusterID == 0)
                {
                    var cmd = new MySqlCommand("SELECT * FROM server WHERE ClusterID IS NULL;", con);
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        if (data.HasRows)
                        {
                            while (data.Read())
                            {                                
                                list.Add(ReadServer(data));
                            }
                        }
                    }
                }                
                con.Close();
            }
            return list;
        }

        public void InsertCluster(Cluster cluster)
        {
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "INSERT INTO cluster"
                        + "(Name)"
                        + " VALUES (@Name);"
                        + "SELECT LAST_INSERT_ID();";
                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", cluster.Name);

                cluster.ClusterID = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateCluster(Cluster cluster)
        {
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "UPDATE cluster SET "
                        + "Name=@Name WHERE ClusterID=@ClusterID;";
                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", cluster.Name);
                cmd.Parameters.AddWithValue("@ClusterID", cluster.ClusterID);

                cmd.ExecuteScalar();
            }
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT * FROM user;", con);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User();
                        user.UserID = reader.GetInt32("UserID");
                        user.Name = reader.GetString("Name");
                        user.Password = reader.GetString("Password");
                        list.Add(user);
                    }
                }
                con.Close();
            }
            return list;
        }
        
        public void DeleteUser(int id)
        {
        }

        public User GetUser(int id)
        {
            var user = new User();
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "SELECT * FROM user"
                        + " WHERE UserID=@ID;";
                var cmd = new MySqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ID", id);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {                        
                        user.Name = reader["Name"].ToString();
                        user.Password = _aes.DecryptString(reader["Password"].ToString());
                    }
                }
            }
            return user;
        }

        public void InsertUser(User user)
        {
            using (var con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                string query = "INSERT INTO user"
                        + "(Name,"
                        + "Password)"
                        + " VALUES ("
                        + "@Name,"
                        + "@Password);"
                        + "SELECT LAST_INSERT_ID();";
                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Password", _aes.EncryptToString(user.Password));

                user.UserID = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateUser(User user)
        {
        }

        private void CreateUserTable(MySqlConnection connection)
        {
            var cmd = new MySqlCommand("CREATE TABLE user (UserID INT NOT NULL AUTO_INCREMENT,"
                + " Name VARCHAR(255) NOT NULL,"
                + " Password VARCHAR(255) NOT NULL,"
                + " PRIMARY KEY (UserID));", connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateClusterTable(MySqlConnection connection)
        {
            var cmd = new MySqlCommand("CREATE TABLE cluster (ClusterID INT NOT NULL AUTO_INCREMENT,"
                + " Name VARCHAR(255) NOT NULL,"
                + " PRIMARY KEY (ClusterID));", connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateProcessorLookupTable(MySqlConnection connection)
        {
            var cmd = new MySqlCommand("CREATE TABLE processorlookup (ProcessorID INT NOT NULL AUTO_INCREMENT,"
                + " Model VARCHAR(255) NULL,"
                + " ProcessorIdentifier VARCHAR(255) NULL,"
                + " Manufacturer VARCHAR(255) NULL,"
                + " Architecture VARCHAR(255) NULL,"
                + " SpeedMhz INT NULL,"
                + " Cores INT NULL,"
                + " Factor DOUBLE NULL,"
                + " RISC TINYINT(1) NULL,"
                + " PRIMARY KEY (ProcessorID));", connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateServerTable(MySqlConnection connection)
        {
            var cmd = new MySqlCommand("CREATE TABLE server (ServerID INT NOT NULL AUTO_INCREMENT,"
                + " ClusterID INT NULL,"
                + " UserID INT NULL,"
                + " VirtualHostID INT NULL,"
                + " Address VARCHAR(255) NULL,"
                + " Hostname VARCHAR(255) NULL,"
                + " SSHPort INT NULL DEFAULT 22,"
                + " Error VARCHAR(255) NULL,"
                + " OperatingSystemType VARCHAR(255) NULL DEFAULT 'Unknown',"
                + " ServerType VARCHAR(255) NULL DEFAULT 'Unknown',"
                + " IsConfigured TINYINT(1) NULL DEFAULT 0,"
                + " IsVirtual TINYINT(1) NULL DEFAULT 0,"
                + " PRIMARY KEY (ServerID),"
                + " KEY ServerClusterFK_idx (ClusterID),"
                + " KEY ServerUserFK_idx (UserID),"
                + " KEY ServerHostFK_idx (VirtualHostID),"
                + " CONSTRAINT ServerClusterFK FOREIGN KEY (ClusterID)"
                + " REFERENCES cluster (ClusterID) ON DELETE NO ACTION ON UPDATE NO ACTION,"
                + " CONSTRAINT ServerHostFK FOREIGN KEY (VirtualHostID)"
                + " REFERENCES server (ServerID) ON DELETE NO ACTION ON UPDATE NO ACTION,"
                + " CONSTRAINT ServerUserFK FOREIGN KEY (UserID)"
                + " REFERENCES user (UserID) ON DELETE NO ACTION ON UPDATE NO ACTION);", connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateHardwareTable(MySqlConnection connection)
        {

            var cmd = new MySqlCommand("CREATE TABLE hardware (ServerID int(11) NOT NULL,"
                 + " Manufacturer varchar(255) DEFAULT NULL,"
                 + " Model varchar(255) DEFAULT NULL,"
                 + " SocketsPopulatedPhysical int(11) DEFAULT NULL,"
                 + " TotalPhysicalCores int(11) DEFAULT NULL,"
                 + " ProcessorIdentifier varchar(255) DEFAULT NULL,"
                 + " ProcessorSpeed varchar(255) DEFAULT NULL,"
                 + " SocketsCapacityPhysical int(11) DEFAULT NULL,"
                 + " TotalLogicalCores int(11) DEFAULT NULL,"
                 + " MachineName varchar(255) DEFAULT NULL,"
                 + " OperatingSystemName varchar(255) DEFAULT NULL,"
                 + " OperatingSystemVersion varchar(255) DEFAULT NULL,"
                 + " PRIMARY KEY(ServerID),"
                 + " CONSTRAINT ServerHardwareFK FOREIGN KEY(ServerID)"
                 + " REFERENCES server (ServerID) ON DELETE CASCADE ON UPDATE NO ACTION);", connection);
            cmd.ExecuteNonQuery();
        }

        private Server ReadServer(MySqlDataReader data)
        {
            var server = new Server();
            server.ServerID = data.GetInt32("ServerID");
            if (!data.IsDBNull(data.GetOrdinal("ClusterID")))
            {
                server.ClusterID = data.GetInt32("ClusterID");                
            }
            if (!data.IsDBNull(data.GetOrdinal("UserID")))
            {
                server.User.UserID = data.GetInt32("UserID");
            }
            if (!data.IsDBNull(data.GetOrdinal("VirtualHostID")))
            {
                server.VirtualHostID = data.GetInt32("VirtualHostID");
            }
            if (!data.IsDBNull(data.GetOrdinal("Address")))
            {
                server.Address = data.GetString("Address");
            }
            if (!data.IsDBNull(data.GetOrdinal("Hostname")))
            {
                server.Hostname = data.GetString("Hostname");
            }
            server.SSHPort = data.GetInt32("SSHPort");
            if (!data.IsDBNull(data.GetOrdinal("Error")))
            {
                server.Hostname = data.GetString("Error");
            }
            server.OperatingSystem = (OperatingSystemType)Enum.Parse(typeof(OperatingSystemType),
                data.GetString("OperatingSystemType"));
            server.ServerType = (ServerType)Enum.Parse(typeof(ServerType),
                data.GetString("ServerType"));
            server.IsConfigured = data.GetBoolean("IsConfigured");
            server.IsVirtual = data.GetBoolean("IsVirtual");
            return server;
        }
    }
}
