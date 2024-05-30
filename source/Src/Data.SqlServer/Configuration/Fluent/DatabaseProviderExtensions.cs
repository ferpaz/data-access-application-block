// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Sql.Configuration.Fluent
{
    public static class DatabaseProviderExtensions
    {
        /// <summary>
#if NET6_0_OR_GREATER
        /// A Sql database for use with the Microsoft.Data.SqlClient namespace.
#else
        /// A Sql database for use with the System.Data.SqlClient namespace.
#endif
        /// </summary>
        /// <param name="context">Configuration context</param>
        /// <returns></returns>
        /// <seealso cref="System.Data.SqlClient"/>
        public static IDatabaseSqlDatabaseConfiguration ASqlDatabase(this IDatabaseConfigurationProviders context)
        {
            return new SqlDatabaseConfigurationExtension(context);
        }

    }
}
