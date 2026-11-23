@echo off

rem Chequeo si especificó la version
if "%1"=="" goto noparameter

dotnet nuget push "Release\EnterpriseLibrary.Data.NetCore.%1.nupkg" --source "https://nuget.aseinfo.net/nuget" --no-symbols --api-key 5E110749-F450-4D47-870B-77708804A3C4
if errorlevel 1 exit /b %errorlevel%

dotnet nuget push "Release\EnterpriseLibrary.Data.SqlServer.NetCore.%1.nupkg" --source "https://nuget.aseinfo.net/nuget" --no-symbols --api-key 5E110749-F450-4D47-870B-77708804A3C4
if errorlevel 1 exit /b %errorlevel%

dotnet nuget push "Release\EnterpriseLibrary.Data.Oracle.NetCore.%1.nupkg" --source "https://nuget.aseinfo.net/nuget" --no-symbols --api-key 5E110749-F450-4D47-870B-77708804A3C4
if errorlevel 1 exit /b %errorlevel%

nuget list EnterpriseLibrary.Data -source "https://nuget.aseinfo.net/nuget" -prerelease -allversions

:noparameter

echo Debe especificar la version como parametro
echo (por ejemplo: 1.0.0.0)
echo Uso: publish version
exit /b 1

:versionnosoportada
echo No existe paquete que publicar para la version especificada (%1).
exit /b 1

:fin
