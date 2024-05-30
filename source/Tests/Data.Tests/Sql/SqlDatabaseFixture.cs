// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data.Common;
#if NET6_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using Microsoft.Practices.EnterpriseLibrary.Data.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Sql.Tests
{
    [TestClass]
    public class SqlDatabaseFixture
    {
        [TestMethod]
        public void ConnectionTest()
        {
            DatabaseSyntheticConfigSettings settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            ConnectionStringSettings data = settings.GetConnectionStringSettings("NewDatabase");
            SqlDatabase sqlDatabase = new SqlDatabase(data.ConnectionString);

            DbConnection connection = sqlDatabase.CreateConnection();
            Assert.IsNotNull(connection);
            Assert.IsTrue(connection is SqlConnection);
            connection.Open();
            DbCommand cmd = sqlDatabase.GetSqlStringCommand("Select * from Region");
            cmd.CommandTimeout = 60;
            Assert.AreEqual(cmd.CommandTimeout, 60);
        }

        [TestMethod]
        public void CanGetConnectionWithoutCredentials()
        {
            DatabaseSyntheticConfigSettings settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            ConnectionStringSettings data = settings.GetConnectionStringSettings("DbWithSqlServerAuthn");
            SqlDatabase sqlDatabase = new SqlDatabase(data.ConnectionString);

#if NET6_0_OR_GREATER
            Assert.AreEqual(String.Format(@"server={0};initial catalog=northwind;integrated security=false;trust server certificate=true;", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), sqlDatabase.ConnectionStringWithoutCredentials);
#else
            Assert.AreEqual(String.Format(@"server={0};initial catalog=northwind;integrated security=false;", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), sqlDatabase.ConnectionStringWithoutCredentials);
#endif
        }

        [TestMethod]
        public void CanGetConnectionForStringWithNoCredentials()
        {
            DatabaseSyntheticConfigSettings settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            ConnectionStringSettings data = settings.GetConnectionStringSettings("NewDatabase");
            SqlDatabase sqlDatabase = new SqlDatabase(data.ConnectionString);

#if NET6_0_OR_GREATER
            Assert.AreEqual(String.Format(@"server={0};initial catalog=northwind;integrated security=true;trust server certificate=true;", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), sqlDatabase.ConnectionStringWithoutCredentials);
#else
            Assert.AreEqual(String.Format(@"server={0};initial catalog=northwind;integrated security=true;", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), sqlDatabase.ConnectionStringWithoutCredentials);
#endif
        }

        [TestMethod]
        public void CheckNoPasswordInConnectionStringWithPersistInfoEqualsFalse()
        {
            try
            {
                CreateUser();
                DatabaseSyntheticConfigSettings settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
                ConnectionStringSettings data = settings.GetConnectionStringSettings("NorthwindPersistFalse");
                SqlDatabase sqlDatabase = new SqlDatabase(data.ConnectionString);
                DbConnection dbConnection = sqlDatabase.CreateConnection();
                dbConnection.Open();
                dbConnection.Close();
                string connectionString = dbConnection.ConnectionString;
                if (connectionString.ToLower().Contains("pwd") || connectionString.ToLower().Contains("password"))
                {
                    Assert.Fail();
                }
            }
            finally
            {
                DeleteUser();
            }
        }

        void CreateUser()
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory(TestConfigurationSource.CreateConfigurationSource());
            Database adminDb = factory.CreateDefault();
            using (DbConnection connection = adminDb.CreateConnection())
            {
                connection.Open();
                string query;
                DbCommand addUser;
                try
                {
                    query = string.Format("exec sp_addlogin '{0}', '{1}', 'Northwind'", TestConfigurationSource.NorthwindDummyUser, TestConfigurationSource.NorthwindDummyPassword);
                    addUser = adminDb.GetSqlStringCommand(query);
                    adminDb.ExecuteNonQuery(addUser);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                try
                {
                    query = string.Format("exec sp_grantdbaccess '{0}', '{0}'", TestConfigurationSource.NorthwindDummyUser);
                    addUser = adminDb.GetSqlStringCommand(query);
                    adminDb.ExecuteNonQuery(addUser);
                }
                catch {}
                try
                {
                    query = string.Format("exec sp_addrolemember N'db_owner', N'{0}'", TestConfigurationSource.NorthwindDummyUser);
                    addUser = adminDb.GetSqlStringCommand(query);
                    adminDb.ExecuteNonQuery(addUser);
                }
                catch {}
            }
        }

        void DeleteUser()
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory(TestConfigurationSource.CreateConfigurationSource());
            Database adminDb = factory.CreateDefault();
            using (DbConnection connection = adminDb.CreateConnection())
            {
                connection.Open();
                string query;
                DbCommand dropUser;
                try
                {
                    query = string.Format("exec sp_revokedbaccess '{0}'", TestConfigurationSource.NorthwindDummyUser);
                    dropUser = adminDb.GetSqlStringCommand(query);
                    adminDb.ExecuteNonQuery(dropUser);
                }
                catch {}
                try
                {
                    query = string.Format("exec sp_droplogin '{0}'", TestConfigurationSource.NorthwindDummyUser);
                    dropUser = adminDb.GetSqlStringCommand(query);
                    adminDb.ExecuteNonQuery(dropUser);
                }
                catch {}
            }
        }

        [TestMethod]
        public void CheckNoPasswordWithPersistInfoEqualsFalseForDynamicConnectionString()
        {
            try
            {
                CreateUser();
                ConnectionString testString =
#if NET6_0_OR_GREATER
                    new ConnectionString(String.Format(@"server={0};initial catalog=Northwind;uid=entlib;pwd=hdf7&834k(*KA;Persist Security Info=false;trust server certificate=true;", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), "UserId,UId", "Password,Pwd");
#else
                    new ConnectionString(String.Format(@"server={0};initial catalog=Northwind;uid=entlib;pwd=hdf7&834k(*KA;Persist Security Info=false;", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), "UserId,UId", "Password,Pwd");
#endif
                SqlDatabase sqlDatabase = new SqlDatabase(testString.ToString());
                DbConnection dbConnection = sqlDatabase.CreateConnection();
                dbConnection.Open();
                dbConnection.Close();
                string connectionString = dbConnection.ConnectionString;
                if (connectionString.ToLower().Contains("pwd") || connectionString.ToLower().Contains("password"))
                {
                    Assert.Fail();
                }
            }
            finally
            {
                DeleteUser();
            }
        }
    }
}
