// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;


namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests
{
#pragma warning disable 612, 618
    [TestClass]
    public class OracleDatabaseFixture
    {
        IConfigurationSource configurationSource;

        [TestInitialize]
        public void SetUp()
        {
            NetCoreHelpers.RegisterDbProviderFactories();
            EnvironmentHelper.AssertOracleClientIsInstalled();
            configurationSource = OracleTestConfigurationSource.CreateConfigurationSource();
        }

        [TestMethod]
        public void CanConnectToOracleAndExecuteAReader()
        {
            var oracleDatabase = new DatabaseSyntheticConfigSettings(this.configurationSource).GetDatabase("OracleTest").BuildDatabase();

            DbConnection connection = oracleDatabase.CreateConnection();
            Assert.IsNotNull(connection);
            Assert.IsTrue(connection is OracleConnection);
            connection.Open();
            DbCommand cmd = oracleDatabase.GetSqlStringCommand("Select * from Region");
            cmd.CommandTimeout = 0;
        }

        [TestMethod]
        public void CanExecuteCommandWithEmptyPackages()
        {
            ConnectionStringSettings data = OracleTestConfigurationSource.GetConnectionStringSettings("OracleTest");
            OracleDatabase oracleDatabase = new OracleDatabase(data.ConnectionString);
            DbConnection connection = oracleDatabase.CreateConnection();
            Assert.IsNotNull(connection);
            Assert.IsTrue(connection is OracleConnection);
            connection.Open();
            DbCommand cmd = oracleDatabase.GetSqlStringCommand("Select * from Region");
            cmd.CommandTimeout = 0;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructingAnOracleDatabaseWithNullPackageListThrows()
        {
            ConnectionStringSettings data = OracleTestConfigurationSource.GetConnectionStringSettings("OracleTest");

            new OracleDatabase(data.ConnectionString, null);
        }
    }
#pragma warning restore 612, 618
}
