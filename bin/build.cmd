@echo off

dotnet pack "..\source\Src\Data\Data.csproj" --configuration Release -p:GeneratePackageOnBuild=false --output "Release"
if errorlevel 1 exit /b %errorlevel%

dotnet pack "..\source\Src\Data.SqlServer\Data.SqlServer.csproj" --configuration Release -p:GeneratePackageOnBuild=false --output "Release"
if errorlevel 1 exit /b %errorlevel%

dotnet pack "..\source\Src\Data.Oracle\Data.Oracle.csproj" --configuration Release -p:GeneratePackageOnBuild=false --output "Release"
if errorlevel 1 exit /b %errorlevel%

@echo on
dir .\Release\EnterpriseLibrary.Data.*.nupkg
