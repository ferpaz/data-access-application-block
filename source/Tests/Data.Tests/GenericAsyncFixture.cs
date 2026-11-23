// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common.TestSupport.ContextBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    public abstract class Given_GenericDatabaseInstance : ArrangeActAssert
    {
        protected Database Database { get; private set; }
#if NET6_0_OR_GREATER
        private readonly string northwind = ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString;
#else
        private readonly string northwind = ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString;
#endif

        protected override void Arrange()
        {
            base.Arrange();

            Database = new GenericDatabase(northwind, SqlClientFactory.Instance);
        }
    }

    [TestClass]
    public class When_UsingGenericDatabase : Given_GenericDatabaseInstance
    {
        [TestMethod]
        public void Then_SupportsAsyncIsFalse()
        {
            Assert.IsFalse(Database.SupportsAsync);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Then_AsyncOperationThrows()
        {
            var command = Database.GetStoredProcCommand("Ten Most Popular Products");
            Database.BeginExecuteReader(command, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Then_AsyncAccessorThrows()
        {
            var accessor = Database.CreateSprocAccessor<object>("Ten Most Popular Products");
            accessor.BeginExecute(null, null);
        }
    }

}
