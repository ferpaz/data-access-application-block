using System.Data.Common;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    public static class NetCoreHelpers
    {
        // DbProviderFactories are not automatically registered in .NET Core
        public static void RegisterDbProviderFactories()
        {
#if NET6_0
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Data.Tests.SqlAlias1", SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Data.Tests.SqlAlias2", SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);
#endif
        }
    }
}
