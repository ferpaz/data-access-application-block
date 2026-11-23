// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data.Common;
using Microsoft.Data.SqlClient;
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
            var settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            var data = settings.GetConnectionStringSettings("DbWithSqlServerAuthn");
            var expected = new SqlConnectionStringBuilder(data.ConnectionString);
            expected.Remove("User ID");
            expected.Remove("Password");
            var sqlDatabase = new SqlDatabase(data.ConnectionString);
            var actual = new SqlConnectionStringBuilder(sqlDatabase.ConnectionStringWithoutCredentials);
            Assert.AreEqual(expected.ConnectionString.ToLowerInvariant(), actual.ConnectionString.ToLowerInvariant());
        }
        [TestMethod]
        public void CanGetConnectionForStringWithNoCredentials()
        {
            var settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            var data = settings.GetConnectionStringSettings("NewDatabase");
            var expected = new SqlConnectionStringBuilder(data.ConnectionString);
            expected.Remove("User ID");
            expected.Remove("Password");
            expected.IntegratedSecurity = true;
            var sqlDatabase = new SqlDatabase(expected.ConnectionString);
            var actual = new SqlConnectionStringBuilder(sqlDatabase.ConnectionStringWithoutCredentials);
            Assert.AreEqual(expected.ConnectionString.ToLowerInvariant(), actual.ConnectionString.ToLowerInvariant());
        }
        [TestMethod]
        public void CheckNoPasswordInConnectionStringWithPersistInfoEqualsFalse()
        {
            var settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            var data = settings.GetConnectionStringSettings("NorthwindPersistFalse");
            var sqlDatabase = new SqlDatabase(data.ConnectionString);
            using (var connection = sqlDatabase.CreateConnection())
            {
                connection.Open();
                connection.Close();
                Assert.IsFalse(connection.ConnectionString.ToLowerInvariant().Contains("pwd"));
                Assert.IsFalse(connection.ConnectionString.ToLowerInvariant().Contains("password"));
            }
        }

        [TestMethod]
        public void CheckNoPasswordWithPersistInfoEqualsFalseForDynamicConnectionString()
        {
            var settings = new DatabaseSyntheticConfigSettings(TestConfigurationSource.CreateConfigurationSource());
            var data = settings.GetConnectionStringSettings("NorthwindPersistFalse");
            var testString = new ConnectionString(data.ConnectionString, "UserId,UId", "Password,Pwd");
            var sqlDatabase = new SqlDatabase(testString.ToString());
            using (var connection = sqlDatabase.CreateConnection())
            {
                connection.Open();
                connection.Close();
                Assert.IsFalse(connection.ConnectionString.ToLowerInvariant().Contains("pwd"));
                Assert.IsFalse(connection.ConnectionString.ToLowerInvariant().Contains("password"));
            }
        }
    }
}