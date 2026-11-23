# Data Access Application Block Release Notes   {#ReleaseNotes}
## Version 10.0.0-rc1
* The Data, SQL Server and Oracle packages target `net48`, `netstandard2.0`, `net8.0` and `net10.0`. Explicit `net47`, `net472` and `net6.0` assets were removed; `netstandard2.0` compatibility does not imply support for older runtimes.
* SQL Server uses `Microsoft.Data.SqlClient` on all targets. Provider configuration and code using `System.Data.SqlClient` types may need changes and recompilation. See the [migration guide](@ref UpgradeNet10).
* Packages are available from the private feed described in the README and use `EnterpriseLibrary.Common.NetCore` 6.3.0 from NuGet.org. This release does not build provider-specific ODBC, OLE DB or SQL Server CE packages.
* Standard assets passed the documented limited SQL and Oracle tests; this is not exhaustive validation or a statement of official Oracle support for every runtime.

## Version 7.0 RC1
* The block was split to several NuGet packages, one for each database provider. See [Upgrade](@ref Upgrade) and
  [User Guide](@ref Overview) for details.
* Support for some older .NET frameworks was dropped, in favor of Long Term Support versions:
  * Dropped support for .NET Framework 4.0, 4.5, .NET Core 2.0, 3.0.
  * Added support for .NET Framework 4.5.2, .NET Core 2.1, 3.1
* OLE DB and ODBC now have their own database providers, [OleDbDatabase][5] and [OdbcDatabase][6], respectively,
  instead of the GenericDatabase used for these providers earlier.
* Fixed [bug #23 Oracle RAW data type is not cast to byte[] correctly][1]
* Fixed [bug #31 SprocAccessor<TResult>.Execute(params object[]) throws ObjectDisposedException under Oracle][2]
* Fixed [bug #32 DatabaseProviderFactory.Create(String) fails for an ODBC provider under .NET Core][3]
* Add support for `IN OUT` parameters in Oracle database provider ([issue #14][4])


[1]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/23
[2]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/31
[3]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/32
[4]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/14
[5]: @ref Microsoft.Practices.EnterpriseLibrary.Data.OleDb.OleDbDatabase
[6]: @ref Microsoft.Practices.EnterpriseLibrary.Data.Odbc.OdbcDatabase