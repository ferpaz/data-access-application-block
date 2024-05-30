// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.


using System.Configuration;

#if NET6_0_OR_GREATER
using System.IO;
using Microsoft.Extensions.Configuration;
#endif

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.TestSupport;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport
{
    public static class OracleTestConfigurationSource
    {
        public static readonly string OracleConnectionString;
        public const string OracleConnectionStringName = "OracleTest";
        public const string OracleProviderName = "Oracle.ManagedDataAccess.Client";

#if NET6_0_OR_GREATER
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
#endif

        static OracleTestConfigurationSource()
        {
#if NET6_0_OR_GREATER
            OracleConnectionString = Configuration.GetConnectionString(OracleConnectionStringName);
#else
            OracleConnectionString = ConfigurationManager.ConnectionStrings[OracleConnectionStringName].ConnectionString;
#endif
        }

        public static DictionaryConfigurationSource CreateConfigurationSource()
        {
            DictionaryConfigurationSource configSource = TestConfigurationSource.CreateConfigurationSource();

            var connectionString = new ConnectionStringSettings(
                OracleConnectionStringName,
                OracleConnectionString,
                OracleProviderName);

            var connectionStrings = new ConnectionStringsSection();
            connectionStrings.ConnectionStrings.Add(connectionString);

            configSource.Add("connectionStrings", connectionStrings);

            var dbSettings = configSource.GetSection(DatabaseSettings.SectionName) as DatabaseSettings;
            DbProviderMapping mapping = new DbProviderMapping(DbProviderMapping.DefaultOracleProviderName, typeof(OracleDatabase));
            dbSettings.ProviderMappings.Add(mapping);

            return configSource;
        }

        public static ConnectionStringSettings GetConnectionStringSettings(string name)
        {
#if NET6_0_OR_GREATER
            return new("OracleTest", OracleTestConfigurationSource.Configuration.GetConnectionString(name), "Oracle.ManagedDataAccess.Client");
#else
            return ConfigurationManager.ConnectionStrings[name];
#endif
        }
    }
}
