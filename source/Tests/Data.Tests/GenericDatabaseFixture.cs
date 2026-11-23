// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    [TestClass]
    public class GenericDatabaseFixture
    {
        const string connectionString = "notexpectedtowork;";

        [TestInitialize]
        public void Setup()
        {
            NetCoreHelpers.RegisterDbProviderFactories();
        }

        [TestMethod]
        public void CanCreateGenericDatabaseFromSysConfiguration()
        {
            Database database =
                new DatabaseProviderFactory(new SystemConfigurationSource(false)).Create("mapping2");

            Assert.IsNotNull(database);
            Assert.AreEqual(typeof(GenericDatabase), database.GetType());
            Assert.AreEqual(typeof(SqlClientFactory), database.DbProviderFactory.GetType());
            Assert.AreEqual(connectionString, database.ConnectionStringWithoutCredentials);
        }

        [TestMethod]
        public void CanDoExecuteDataReaderForGenericDatabaseForSqlProviderBug1836()
        {
#if NET6_0_OR_GREATER
            Database db = new GenericDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString, SqlClientFactory.Instance);
#else
            Database db = new GenericDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString, SqlClientFactory.Instance);
#endif


            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();

                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "select * from [Order Details]")) { }
                    transaction.Commit();
                }
            }
        }

        [TestMethod]
        public void DSDBWrapperTransSqlNoCommitTestBug1857()
        {
#if NET6_0_OR_GREATER
            Database db = new GenericDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString, SqlClientFactory.Instance);
#else
            Database db = new GenericDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString, SqlClientFactory.Instance);
#endif

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    string sqlCommand = "select * from [Order Details]";
                    DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

                    DataSet dsActualResult = db.ExecuteDataSet(dbCommand, transaction);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void GenericDatabaseThrowsWhenAskedToDeriveParameters()
        {
#if NET6_0_OR_GREATER
            Database db = new GenericDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString, SqlClientFactory.Instance);
#else
            Database db = new GenericDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString, SqlClientFactory.Instance);
#endif
            DbCommand storedProcedure = db.GetStoredProcCommand("CustOrdersOrders");
            db.DiscoverParameters(storedProcedure);
        }
    }
}
