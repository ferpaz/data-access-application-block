#if NETFRAMEWORK
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    [TestClass]
    public class AsyncAccessorOwnershipFixture
    {
        [TestMethod]
        public void SprocCallbackCanEndAndDisposeOwnedCommand()
        {
            var db = new TrackingDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString);
            var accessor = db.CreateSprocAccessor<string>("Ten Most Expensive Products", new FirstColumnMapper());
            VerifyCallback(accessor);
            Assert.IsTrue(db.CommandDisposed, "The stored procedure command must be disposed after enumeration.");
        }

        [TestMethod]
        public void SqlStringCallbackCanEndOperation()
        {
            var db = new SqlDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString);
            var accessor = db.CreateSqlStringAccessor<string>("SELECT 'callback'", new FirstColumnMapper());
            VerifyCallback(accessor);
        }

        [TestMethod]
        public void SprocCommandIsDisposedWhenParameterMappingFails()
        {
            var db = new TrackingDatabase(ConfigurationManager.ConnectionStrings["Service_Dflt"].ConnectionString);
            var accessor = db.CreateSprocAccessor<string>("Ten Most Expensive Products", new ThrowingParameterMapper(), new FirstColumnMapper());
            try
            {
                accessor.BeginExecute(null, null);
                Assert.Fail("Parameter mapping should fail.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("mapping failed", ex.Message);
            }
            Assert.IsTrue(db.CommandDisposed, "The command must be disposed if the operation cannot start.");
        }

        [TestMethod]
        public void SynchronousCallbackCanEndBeforeBeginReturns()
        {
            var db = new SynchronousDatabase();
            var accessor = db.CreateSqlStringAccessor<string>("SELECT 'callback'", new FirstColumnMapper());
            var state = new object();
            IAsyncResult seen = null;
            int count = 0;
            var returned = accessor.BeginExecute(result =>
            {
                seen = result;
                count = accessor.EndExecute(result).Count();
            }, state);
            Assert.AreSame(returned, seen);
            Assert.AreSame(state, seen.AsyncState);
            Assert.AreEqual(1, count);
            Assert.IsTrue(db.CommandDisposed);
        }

        [TestMethod]
        public void CommandIsDisposedWhenEndExecuteFails()
        {
            var db = new SynchronousDatabase { FailEnd = true };
            var accessor = db.CreateSqlStringAccessor<string>("SELECT 'callback'", new FirstColumnMapper());
            var result = accessor.BeginExecute(null, null);
            try
            {
                accessor.EndExecute(result);
                Assert.Fail("EndExecute should fail.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("end failed", ex.Message);
            }
            Assert.IsTrue(db.CommandDisposed);
        }

        private static void VerifyCallback(DataAccessor<string> accessor)
        {
            using (var completed = new ManualResetEvent(false))
            {
                var state = new object();
                IAsyncResult callbackResult = null;
                Exception callbackError = null;
                int count = 0;
                var returned = accessor.BeginExecute(result =>
                {
                    try
                    {
                        callbackResult = result;
                        count = accessor.EndExecute(result).ToList().Count;
                    }
                    catch (Exception ex) { callbackError = ex; }
                    finally { completed.Set(); }
                }, state);
                Assert.IsTrue(completed.WaitOne(TimeSpan.FromSeconds(20)), "Callback did not complete.");
                Assert.IsNull(callbackError, callbackError?.ToString());
                Assert.AreSame(returned, callbackResult);
                Assert.AreSame(state, callbackResult.AsyncState);
                Assert.IsTrue(count > 0);
            }
        }

        private sealed class FirstColumnMapper : IRowMapper<string>
        {
            public string MapRow(IDataRecord row) => row.GetString(0);
        }

        private sealed class ThrowingParameterMapper : IParameterMapper
        {
            public void AssignParameters(DbCommand command, object[] parameterValues)
            {
                throw new InvalidOperationException("mapping failed");
            }
        }

        private sealed class SynchronousDatabase : Database
        {
            public SynchronousDatabase() : base("Data Source=unused", Microsoft.Data.SqlClient.SqlClientFactory.Instance) { }
            public bool CommandDisposed { get; private set; }
            public bool FailEnd { get; set; }
            public override bool SupportsAsync => true;
            protected override void DeriveParameters(DbCommand command) { throw new NotSupportedException(); }
            public override IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, object state)
            {
                command.Disposed += (sender, args) => CommandDisposed = true;
                var result = new CompletedResult(state);
                callback?.Invoke(result);
                return result;
            }
            public override IDataReader EndExecuteReader(IAsyncResult result)
            {
                if (FailEnd) throw new InvalidOperationException("end failed");
                var table = new DataTable();
                table.Columns.Add("value", typeof(string));
                table.Rows.Add("callback");
                return table.CreateDataReader();
            }
        }

        private sealed class CompletedResult : IAsyncResult
        {
            internal CompletedResult(object state) { AsyncState = state; }
            public object AsyncState { get; }
            public WaitHandle AsyncWaitHandle => null;
            public bool CompletedSynchronously => true;
            public bool IsCompleted => true;
        }

        private sealed class TrackingDatabase : SqlDatabase
        {
            public TrackingDatabase(string connectionString) : base(connectionString) { }
            public bool CommandDisposed { get; private set; }
            public override DbCommand GetStoredProcCommand(string storedProcedureName)
            {
                var command = base.GetStoredProcCommand(storedProcedureName);
                command.Disposed += (sender, args) => CommandDisposed = true;
                return command;
            }
        }
    }
}
#endif