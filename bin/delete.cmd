@echo off

rem Chequeo si especificó la version
if "%1"=="" goto noparameter

dotnet nuget delete EnterpriseLibrary.Data.NetCore %1 --source "https://nuget.aseinfo.net/nuget" --api-key 5E110749-F450-4D47-870B-77708804A3C4
if errorlevel 1 exit /b %errorlevel%

dotnet nuget delete EnterpriseLibrary.Data.SqlServer.NetCore %1 --source "https://nuget.aseinfo.net/nuget" --api-key 5E110749-F450-4D47-870B-77708804A3C4
if errorlevel 1 exit /b %errorlevel%

dotnet nuget delete EnterpriseLibrary.Data.Oracle.NetCore %1 --source "https://nuget.aseinfo.net/nuget" --api-key 5E110749-F450-4D47-870B-77708804A3C4
if errorlevel 1 exit /b %errorlevel%

nuget list EnterpriseLibrary.Data -source "https://nuget.aseinfo.net/nuget" -prerelease -allversions
goto fin

:noparameter

echo Debe especificar la version como parámetro para eliminar
echo (por ejemplo: 1.0.0.0)
echo Uso: delete version
exit /b 1

:fin
