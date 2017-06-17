﻿using System;
using MySql.Data.MySqlClient;

namespace Spikemoss.DataAccessLayer
{
    class MySQL : IDataAccessLayer
    {
        private string _connectionString;
        private SimpleAES _aes;
        private static MySQL _instance;
        private const string DATABASE_NAME = "spikemossv2";

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
    }
}
