// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using OracleConnectionStringBuilder = Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests.Configuration
{
    [TestClass]
    public class DatabaseProviderFactoryFixture
    {
        [TestInitialize]
        public void Initialize()
        {
            NetCoreHelpers.RegisterDbProviderFactories();
        }

        [TestMethod]
        public void CanCreateDatabaseFromFactory()
        {
            var factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);
            Database createdObject = factory.Create("Service_Dflt");

            Assert.IsNotNull(createdObject);
            Assert.IsInstanceOfType(createdObject, typeof(SqlDatabase));
            Assert.AreEqual(ExpectedConnetionString(),
                            new SqlConnectionStringBuilder(createdObject.ConnectionStringWithoutCredentials).ConnectionString.ToLowerInvariant());
        }

        [TestMethod]
        public void CanCreateDefaultDatabaseFromFactory()
        {
            var factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);
            Database createdObject = factory.CreateDefault();

            Assert.IsNotNull(createdObject);
            Assert.IsInstanceOfType(createdObject, typeof(SqlDatabase));
            Assert.AreEqual(ExpectedConnetionString(), new SqlConnectionStringBuilder(createdObject.ConnectionStringWithoutCredentials).ConnectionString.ToLowerInvariant());
        }

        [TestMethod]
        public void CanCreateSqlDatabaseFromFactory()
        {
            var factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);
            Database createdObject = factory.Create("Service_Dflt");

            Assert.IsNotNull(createdObject);
            Assert.AreEqual(ExpectedConnetionString(), new SqlConnectionStringBuilder(createdObject.ConnectionStringWithoutCredentials).ConnectionString.ToLowerInvariant());
        }
        private static string ExpectedConnetionString()
        {
            var expected = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString);
            expected.Remove("User ID");
            expected.Remove("Password");
            return expected.ConnectionString.ToLowerInvariant();
        }
        [TestMethod]
        public void SkipsConnectionStringsWithoutProviderNamesOrWithProviderNamesWhichDoNotMapToAProviderFactory()
        {
            DictionaryConfigurationSource configurationSource = new DictionaryConfigurationSource();
            ConnectionStringsSection section = new ConnectionStringsSection();
#if NET6_0_OR_GREATER
            section.ConnectionStrings.Add(new ConnectionStringSettings("cs1", "cs1", "Microsoft.Data.SqlClient"));
#else
            section.ConnectionStrings.Add(new ConnectionStringSettings("cs1", "cs1", "System.Data.SqlClient"));
#endif
            section.ConnectionStrings.Add(new ConnectionStringSettings("cs2", "cs2"));
            section.ConnectionStrings.Add(new ConnectionStringSettings("cs3", "cs3", "a bogus provider name"));
#if NET6_0_OR_GREATER
            section.ConnectionStrings.Add(new ConnectionStringSettings("cs4", "cs4", "Microsoft.Data.SqlClient"));
#else
            section.ConnectionStrings.Add(new ConnectionStringSettings("cs4", "cs4", "System.Data.SqlClient"));
#endif
            configurationSource.Add("connectionStrings", section);

            var factory = new DatabaseProviderFactory(configurationSource.GetSection);

            Assert.AreEqual("cs1", factory.Create("cs1").ConnectionString);
            Assert.AreEqual("cs4", factory.Create("cs4").ConnectionString);
            try
            {
                factory.Create("cs2");
                Assert.Fail("should have thrown");
            }
            catch (InvalidOperationException)
            {
                // expected, connection string is ignored
            }

            try
            {
                factory.Create("cs3");
                Assert.Fail("should have thrown");
            }
            catch (InvalidOperationException)
            {
                // expected, connection string is ignored
            }
        }

#pragma warning disable 612, 618
        [TestMethod]
        public void CanCreateOracleDatabaseFromFactory()
        {
            var factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);

            OracleDatabase createdObject = (OracleDatabase)factory.Create("OracleTest");
            Assert.IsNotNull(createdObject);
            //Assert.AreEqual(@"server=entlib;", new SqlConnectionStringBuilder(createdObject.ConnectionStringWithoutCredentials).ConnectionString.ToLowerInvariant());
            var expected = new OracleConnectionStringBuilder(ConfigurationManager.ConnectionStrings["OracleTest"].ConnectionString);
            var actual = new OracleConnectionStringBuilder(createdObject.ConnectionStringWithoutCredentials);
            Assert.AreEqual(expected.DataSource.ToLowerInvariant(), actual.DataSource.ToLowerInvariant());
            Assert.IsTrue(string.IsNullOrEmpty(actual.UserID));
            Assert.IsTrue(string.IsNullOrEmpty(actual.Password));

            // can do the configured package mapping?
            Assert.AreEqual(DatabaseFactoryFixture.OracleTestTranslatedStoredProcedureInPackageWithTranslation,
                            createdObject.GetStoredProcCommand(
                                DatabaseFactoryFixture.OracleTestStoredProcedureInPackageWithTranslation).CommandText);
            Assert.AreEqual(DatabaseFactoryFixture.OracleTestStoredProcedureInPackageWithoutTranslation,
                            createdObject.GetStoredProcCommand(
                                DatabaseFactoryFixture.OracleTestStoredProcedureInPackageWithoutTranslation).CommandText);
        }
    }
}
