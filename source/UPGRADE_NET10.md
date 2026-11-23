# Plan de migración a .NET 10 — versión 10.0.0-rc1

## Objetivo y alcance

Migrar la solución existente `source\Data.sln` a `source\Data.slnx`, agregar un target nativo `net10.0`, conservar `net8.0` y `netstandard2.0` y sustituir los targets de .NET Framework por `net48`. Si se confirma que Oracle no soporta `netstandard2.0`, aplicar la excepción para `Data.Oracle` indicada en la fase 3. Publicar los paquetes de producción con la versión **10.0.0-rc1**. Los avances y sus límites se registran en las casillas y el registro de ejecución; las actividades pendientes no se consideran validadas.

Se eliminan los **targets explícitos** `net47`, `net472` y `net6.0`; se conserva `net8.0`. Como `netstandard2.0` es compatible con varias plataformas, NuGet todavía podría seleccionar ese asset en aplicaciones con frameworks anteriores; eso no equivale a prometer soporte, pruebas ni publicación de assets nativos para ellos. Documentar este cambio y la nueva matriz de soporte.

La unidad de trabajo incluye `Data.sln`, la migración de `Data (With Dependencies).sln` para desarrollo con Common y sus procesos relacionados. Se decidió conservar `EnterpriseLibrary.Common.NetCore` **6.3.0** del feed público de NuGet, sin publicar el fork de Common. La evidencia disponible y las validaciones aún pendientes se detallan en la sección de Common y el registro de ejecución. El fork local permanece disponible para desarrollo o si se demuestra una necesidad futura; no se prevén cambios en él.

## Decisiones de targets y proveedor — 20 de noviembre de 2026

Por instrucción del usuario, la matriz acordada es **`net48;netstandard2.0;net8.0;net10.0`**. Se retiran `net47`, `net472` y `net6.0`. La excepción Oracle para Standard sigue sujeta a evidencia según la fase 3. Las suites ejecutables usarán `net48;net8.0;net10.0`; Standard se validará forzando su artefacto en esos tres hosts.

Se integra aquí la migración de SQL Server a `Microsoft.Data.SqlClient` en los cuatro destinos de Data y Data.SqlServer. Hay implementación y pruebas realizadas, con validaciones aún pendientes según el registro. Este documento no concede por sí mismo nuevas autorizaciones de publicación, integración o cambios en otros repositorios.

- .NET 8 se conserva explícitamente por compatibilidad, aunque Microsoft terminó su soporte el 10 de noviembre de 2026. Documentar esta limitación y probarlo igualmente: soporte del paquete y mantenimiento del runtime por Microsoft son compromisos distintos.
- .NET 6 terminó su soporte el 12 de noviembre de 2024. .NET 10 es LTS hasta el 14 de noviembre de 2028.
- Framework 4.8 concentra mantenimiento y pruebas; no es un mínimo del driver 6.1, que admite Framework 4.6.2+. El soporte de Framework depende del Windows instalado.
- Conservar Standard permite que NuGet lo seleccione en Framework anteriores a 4.8; eso no implica prometer soporte para esos consumidores.
- SqlClient 6.1 es LTS hasta el 14 de agosto de 2028; el parche identificado es 6.1.8. Las líneas 5.1 y 5.2 actuales están fuera de soporte. Revalidar parches, avisos y requisitos al implementar.

## Flujo de ramas e integración

- [x] Realizar los cambios de Data en la rama `_net_10_migration`. La rama ya existe y, al preparar este plan, parte del mismo commit que `_net_8_migration`; `master` está en otro commit. Revisar la diferencia completa respecto de `master`, incluidos cambios previos a esta migración, para evitar integrar modificaciones no deseadas.
- [ ] Completar o resolver explícitamente las actividades y criterios de aceptación aplicables antes de integrar. Se aceptan las pruebas y exclusiones documentadas; las casillas restantes y la trazabilidad del commit declarado por los paquetes requieren resolución, sin presentar trabajo no realizado como completado.
- [ ] **Diferido:** integrar `_net_10_migration` en `master` cuando el usuario confirme que ya no tiene reportes de problemas con `10.0.0-rc1` y se hayan resuelto los requisitos aplicables previos a la integración. Consultar [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md); respetar las políticas y permisos del repositorio, revisar el resultado y volver a compilar y ejecutar las pruebas pertinentes en el commit integrado.

## Inventario y matriz objetivo

| Proyecto en `Data.sln` | Targets actuales | Targets propuestos | Actividades |
|---|---|---|---|
| `Src\Data\Data.csproj` | `net47;net472;netstandard2.0;net6.0;net8.0` | `net48;netstandard2.0;net8.0;net10.0` | Actualizar condiciones de dependencias, código y paquete. |
| `Src\Data.SqlServer\Data.SqlServer.csproj` | Los mismos cinco | `net48;netstandard2.0;net8.0;net10.0` | Actualizar proveedor SQL y condiciones sin cambiar implícitamente la API pública. |
| `Src\Data.Oracle\Data.Oracle.csproj` | Los mismos cinco | `net48;netstandard2.0;net8.0;net10.0`, sujeto a compatibilidad Oracle | Seleccionar ODP.NET compatible por target; retirar `netstandard2.0` solo si se confirma que Oracle no lo soporta, según la decisión documentada en la fase 3. |
| `Tests\Data.Tests\Data.Tests.VSTS.csproj` | `net47;net6.0;net8.0` | `net48;net8.0;net10.0` | Actualizar el runner, las dependencias y pruebas; validar además los assets `netstandard2.0` con hosts reales. |
| `Tests\Oracle.Tests.VSTS\Oracle.Tests.VSTS.csproj` | `net47;net6.0;net8.0` | `net48;net8.0;net10.0` | Igual, incluyendo integración Oracle y configuración x64. |
| `Tests\Data.TestSupport\Data.TestSupport.csproj` | `net47;net6.0;net8.0` | `net48;net8.0;net10.0`; agregar `netstandard2.0` solo si lo requiere el diseño de pruebas | Es biblioteca auxiliar de pruebas, no una suite independiente. Revisar referencias y sus propias dependencias. |

**Importante:** `netstandard2.0` es una especificación de API, no un runtime ejecutable. No basta con agregarlo como target de un proyecto de pruebas ni con ejecutar `dotnet test -f netstandard2.0`. Ejecutar las suites en hosts `net48`, `net8.0` y `net10.0` y, adicionalmente, probar explícitamente los binarios de producción `netstandard2.0` en los tres hosts, como se indica en la matriz de pruebas. Los grupos `netstandard2` que aparecen hoy en los tres proyectos de pruebas no se activan con sus targets actuales.

## Fase 1 — Herramientas y dependencias

- [x] Instalar y seleccionar el SDK 10.0.404; `global.json` lo fija con `rollForward: disable` y `dotnet --version` confirma su selección. Están disponibles runtimes .NET 8 y .NET 10, y se compiló y ejecutó `net48` en Windows x64.
- [x] Inventariar herramientas y dependencias locales del modo `Package`; ver el inventario de esta fase. El modo `Project` no se requiere para el cierre, por decisión del usuario.
- [x] Comprobar restauración y compilación Debug de los modos `Package` y `Project` de Common, y registrar la versión y hash de Common público; ver registro de ejecución. El modo proyecto presentó advertencias MSB3884 del repositorio Common.
- [x] Inventariar referencias directas y transitivas resueltas por target en el modo `Package` y revisar avisos con las fuentes configuradas. La selección de assets en consumidores limpios no se deduce de este inventario.
- [x] Registrar el estado de `BVT\Data.BVT`: está fuera de `Data.slnx`, apunta a `net47` y referencia proyectos ausentes.
- [x] BVT permanece en el repositorio tal como está, sin migrarse ni retirarse; ver [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md).

### Inventario local — 23 de noviembre de 2026

Entorno: Windows x64 10.0.26300; Visual Studio Enterprise 2026 18.12.12405.243; SDK .NET 10.0.404; MSBuild 18.9.15.53019; NuGet CLI 7.9.0.83; .NET Framework 4.8 (release 534296) y referencias `v4.8`; runtimes .NET 8.0.34 y 10.0.15. `global.json` selecciona SDK 10.0.404. El host también tiene instalados otros SDK y runtimes, incluidos 8.0.428, 9.0.321 y 11.0.100.

Inventario de `dotnet list source\Data.slnx package --include-transitive --format json --no-restore` tras restaurar el modo `Package`. La tabla muestra versiones resueltas directas; `NETStandard.Library` es una referencia implícita. Los conteos son paquetes transitivos por proyecto/TFM, no sumas únicas.

| Proyecto | TFM | Referencias directas resueltas | Transitivas |
|---|---|---|---:|
| Data | net48 | Common 6.3.0 | 2 |
| Data | netstandard2.0 | Common 6.3.0; SqlClient 6.1.8; NETStandard.Library 2.0.3; ConfigurationManager 10.0.0 | 47 |
| Data | net8.0, net10.0 | Common 6.3.0; SqlClient 6.1.8; ConfigurationManager 10.0.0 | 29 cada uno |
| Data.SqlServer | net48 | SqlClient 6.1.8 | 43 |
| Data.SqlServer | netstandard2.0 | SqlClient 6.1.8; NETStandard.Library 2.0.3 | 49 |
| Data.SqlServer | net8.0, net10.0 | SqlClient 6.1.8 | 31 cada uno |
| Data.Oracle | net48 | Oracle.ManagedDataAccess 18.15.1 | 3 |
| Data.Oracle | netstandard2.0 | Oracle.ManagedDataAccess.Core 2.19.310; NETStandard.Library 2.0.3 | 52 |
| Data.Oracle | net8.0 | Oracle.ManagedDataAccess.Core 23.4.0 | 34 |
| Data.Oracle | net10.0 | Oracle.ManagedDataAccess.Core 23.26.0 | 34 |
| Data.Tests | net48 | Common 6.3.0; Test SDK 15.9.0; MSTest adapter/framework 1.4.0; Moq 4.10.0 | 46 |
| Data.Tests | net8.0 | Common 6.3.0; SqlClient 6.1.8; Configuration y Json 8.0.0; Test SDK 15.9.0; MSTest 1.4.0; Newtonsoft.Json 13.0.1; Oracle Core 23.4.0 | 45 |
| Data.Tests | net10.0 | Igual a net8.0 salvo Oracle Core 23.26.0 | 45 |
| Data.TestSupport | net48 | Common 6.3.0; Test SDK 15.9.0; MSTest 1.4.0; Oracle.ManagedDataAccess 18.15.1 | 44 |
| Data.TestSupport | net8.0, net10.0 | Common 6.3.0; Test SDK 15.9.0; MSTest 1.4.0; Newtonsoft.Json 13.0.1; Oracle Core 23.4.0 / 23.26.0 respectivamente | 41 cada uno |
| Oracle.Tests | net48 | Common 6.3.0; Test SDK 15.9.0; MSTest 1.4.0; Oracle.ManagedDataAccess 18.15.1 | 44 |
| Oracle.Tests | net8.0, net10.0 | Common 6.3.0; Configuration y Json 8.0.0; Test SDK 15.9.0; MSTest 1.4.0; Newtonsoft.Json 13.0.1; Oracle Core 23.4.0 / 23.26.0 respectivamente | 46 cada uno |

En esta tabla Common es `EnterpriseLibrary.Common.NetCore`, SqlClient es `Microsoft.Data.SqlClient`, Configuration/Json son `Microsoft.Extensions.Configuration` y `Microsoft.Extensions.Configuration.Json`, Test SDK es `Microsoft.NET.Test.Sdk` y MSTest abarca `MSTest.TestAdapter` y `MSTest.TestFramework`. El conjunto consolidado de paquetes directos y transitivos resueltos, con todas las versiones observadas en los TFMs, es:

- Azure.Core 1.50.0; Azure.Identity 1.17.1; Castle.Core 4.3.1; EnterpriseLibrary.Common.NetCore 6.3.0; Microsoft.Bcl.AsyncInterfaces 8.0.0; Microsoft.CodeCoverage 15.9.0; Microsoft.CSharp 4.5.0; Microsoft.Data.SqlClient 6.1.8; Microsoft.Data.SqlClient.SNI y SNI.runtime 6.0.3; Microsoft.DotNet.PlatformAbstractions 1.0.3.
- Microsoft.Extensions.Caching.Abstractions 8.0.0/9.0.21; Caching.Memory 8.0.1/9.0.21; Configuration, Configuration.Abstractions, Configuration.FileExtensions y Configuration.Json 8.0.0; DependencyInjection.Abstractions 8.0.2/9.0.21; DependencyModel 1.0.3; FileProviders.Abstractions, FileProviders.Physical y FileSystemGlobbing 8.0.0; Logging.Abstractions 8.0.3/9.0.21; Options 8.0.2/9.0.21; Primitives 8.0.0/9.0.21. Los nombres abreviados de esta línea llevan el prefijo `Microsoft.Extensions.`.
- Microsoft.Identity.Client 4.84.2; Client.Broker 4.84.2; Client.Extensions.Msal 4.78.0; Client.NativeInterop 0.20.6; Microsoft.IdentityModel.Abstractions 8.14.0; JsonWebTokens, Logging, Protocols, Protocols.OpenIdConnect y Tokens 7.7.1. Los nombres abreviados llevan respectivamente los prefijos `Microsoft.Identity.` y `Microsoft.IdentityModel.`.
- Microsoft.NET.Test.Sdk 15.9.0; Microsoft.NETCore.Platforms 1.1.0; Microsoft.SqlServer.Server 1.0.0; Microsoft.TestPlatform.ObjectModel y TestHost 15.9.0; Microsoft.Win32.Registry 4.5.0; Moq 4.10.0; MSTest.TestAdapter y TestFramework 1.4.0; NETStandard.Library 1.6.1/2.0.3; Newtonsoft.Json 13.0.1; Oracle.ManagedDataAccess 18.15.1; Oracle.ManagedDataAccess.Core 2.19.310/23.4.0/23.26.0.
- System.Buffers 4.6.1; ClientModel 1.8.0; ComponentModel.Annotations 5.0.0; Configuration.ConfigurationManager 10.0.0; Data.Common 4.3.0; Diagnostics.DiagnosticSource 8.0.1; Diagnostics.EventLog 10.0.0; Diagnostics.PerformanceCounter 6.0.2/8.0.0; DirectoryServices.Protocols 6.0.2/8.0.0/8.0.2; Formats.Asn1 8.0.1/8.0.2; IdentityModel.Tokens.Jwt 7.7.1; IO.FileSystem.AccessControl 5.0.0; Memory 4.6.3; Memory.Data 8.0.1; Numerics.Vectors 4.6.1; Reflection.Emit e Emit.ILGeneration 4.3.0; Runtime.CompilerServices.Unsafe 6.1.2; Security.AccessControl 5.0.0; Security.Cryptography.Cng 5.0.0; Security.Cryptography.Pkcs 8.0.2/9.0.21; Security.Cryptography.ProtectedData 4.5.0/10.0.0; Security.Principal.Windows 5.0.0; Text.Encodings.Web 8.0.0; Text.Json 8.0.6; Text.RegularExpressions 4.3.1; Threading.Tasks.Extensions 4.6.0; ValueTuple 4.5.0; Xml.XPath.XmlDocument 4.0.1. Los nombres abreviados llevan el prefijo `System.`.
- Unity.Abstractions 3.3.1; Unity.Interception 5.5.6.

El detalle exacto de cada paquete transitivo por proyecto y TFM puede reproducirse con el comando indicado; no se ha inventariado el modo `Project`. Con `dotnet list source\Data.slnx package --vulnerable --include-transitive --no-restore`, las fuentes configuradas (NuGet público y paquetes offline) no notificaron vulnerabilidades tras actualizar Newtonsoft. Este resultado depende de los avisos disponibles y no garantiza ausencia de vulnerabilidades desconocidas.

`Newtonsoft.Json` 9.0.1 era transitiva en los tres proyectos de pruebas para `net8.0` y `net10.0`. Se fijó 13.0.1, la primera versión corregida para [GHSA-5crp-9r3c-p9vr](https://github.com/advisories/GHSA-5crp-9r3c-p9vr); NuGet publica assets compatibles con `net48` y `netstandard2.0`, y compatibilidad calculada para .NET 8 y .NET 10. La referencia se limita a los targets de pruebas afectados; no cambia los paquetes de producción publicados ni introduce Newtonsoft en `net48` de pruebas. Tras restaurar, los tres proyectos resolvieron 13.0.1 en ambos targets y la auditoría no reportó paquetes vulnerables.

### Decisiones de cierre — 23 de noviembre de 2026

- Suites completas Release y revisión individual de omitidas: cerradas por aceptación del usuario, sin afirmar que se ejecutaron o revisaron por completo. También se aceptan para este cierre las pruebas Standard parciales registradas, sin exigir suites Standard completas.
- Configuración antigua, aliases y compatibilidad binaria: cerradas para este requerimiento por aceptación del usuario; la evidencia existente no demuestra todas las combinaciones posibles.
- Pruebas del paquete 10.0.0-rc1 en consumidores limpios `net8.0` y `net10.0`: **diferidas a otro requerimiento**; antecedentes en [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md).
- Common: se conserva la versión pública 6.3.0; no se requiere actualizar Common ni completar el modo `Project` para este cierre. BVT permanece en el repositorio sin migrarse ni retirarse.
- Oracle: el usuario acepta el alcance verificado en el consumidor `net48` y, para este cierre, las operaciones básicas probadas con el asset `netstandard2.0` en los tres hosts. Esto no acredita soporte oficial del proveedor en todos ellos.
- Configuración del feed documentada y restauración aislada `net48` verificada; la guía de migración sigue pendiente. La comprobación de hashes de los paquetes publicados se completó.
- Firma NuGet: el usuario acepta que no se requiere para los paquetes de este proyecto en el feed privado. Los `.nupkg` publicados no están firmados; la firma strong-name de los ensamblados y la comparación de hashes no equivalen a una firma NuGet ni acreditan la autoría del paquete.
- Símbolos de depuración: la publicación y las comprobaciones pendientes se difieren; alcance y evidencia en [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md).
- Assets `netstandard2.0`: el usuario acepta las pruebas registradas como suficientes para cerrar este requisito. Un host con referencias explícitas ejecutó operaciones básicas SQL y Oracle en los tres runtimes y comprobó los binarios cargados. No se ejecutaron suites Standard completas ni se validó la selección normal de NuGet; una biblioteca intermedia con target Standard no garantiza por sí sola que el ejecutable cargue esos assets.

### Common y solución con dependencias

El repositorio Common está en `E:\vh4Apps\Tools\common-infrastructure`; su proyecto es `E:\vh4Apps\Tools\common-infrastructure\source\Src\Common\Common.csproj`. La solución secundaria `source\Data (With Dependencies).slnx` apunta a ese proyecto. Los cuatro proyectos principales que usan Common comparten la ruta configurable `EntLibCommon` definida en `package.props`; el modo predeterminado es `Package`.

Los proyectos Common referidos por `Data (With Dependencies).sln` pertenecen al fork en `E:\vh4Apps\Tools\common-infrastructure`. Según confirmación del usuario, la rama actualmente seleccionada de ese fork ya soporta `net10.0` y `netstandard2.0`. No se planifica modificar su código, migrar sus proyectos ni cambiar sus targets o suites: si se requiere el fork, el trabajo previsto es generar y validar el paquete existente y publicarlo en el feed privado [https://nuget.aseinfo.net](https://nuget.aseinfo.net). Registrar el nombre de rama y commit usados al generar el paquete. Corregir las rutas de las referencias desde este repositorio Data no implica modificar Common. Si las validaciones revelan una incompatibilidad concreta, registrar la evidencia y reevaluar únicamente la corrección necesaria.

#### Decisión: conservar Common público

Se conserva `EnterpriseLibrary.Common.NetCore` **6.3.0** de NuGet público como dependencia de Data. Su paquete fue restaurado de forma aislada y las suites nativas de Data y Oracle se ejecutaron con Common público, con las omisiones detalladas en el registro. Los paquetes Data `10.0.0-rc1` declaran esa dependencia. No se requiere publicar el fork de Common ni igualar artificialmente su versión a la de Data. Esta decisión no sustituye las pruebas pendientes de consumidores limpios ni las revisiones de dependencias y despliegue; la cobertura Standard no es exhaustiva y se acepta para este cierre.

- [x] Identificar el paquete público y verificar que contiene un asset Standard: `EnterpriseLibrary.Common.NetCore` 6.3.0, obtenido de `https://api.nuget.org/v3/index.json`; hash y pruebas iniciales en el registro. La revisión completa de dependencias, avisos y assets seleccionados por target sigue pendiente.
- [x] Restaurar Common en modo paquete desde NuGet público de forma aislada y comprobar su compilación y casos iniciales; el registro documenta el hash y los assets observados en esa comprobación. Completar la identificación de assets para la matriz final y consumidores limpios.
- [ ] Validar restauración, compilación y pruebas funcionales/integración de Data en `net48`, `net8.0` y `net10.0`, además de los hosts que fuerzan el artefacto `netstandard2.0`. Comprobar configuración, recursos, firma, compatibilidad de API, dependencias y binding redirects donde corresponda.
- [ ] Probar el NuGet resultante de Data en consumidores limpios usando Common público, con dependencias resueltas desde sus orígenes configurados.
- [x] Registrar la elección del paquete público: se conserva Common 6.3.0 y las actividades exclusivas de empaquetar y publicar el fork son **no aplicables a la decisión actual**. Reconsiderarlas solo si aparece una incompatibilidad demostrada.
- Si en el futuro se demuestra que se necesita el fork de Common, consultar la contingencia en [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md); no es un requisito de este cierre.

**Evidencia de decisión:** restauración aislada e identidad de Common, suites nativas con omisiones y pruebas Standard parciales registradas más abajo. Siguen pendientes los consumidores limpios del NuGet publicado y las revisiones de despliegue y dependencias; no se exige completar la matriz Standard para este cierre.

#### Alternativa Common

El empaquetado y la publicación del fork no aplican a la decisión actual; la información para una posible contingencia está en [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md).
- [x] Documentar en `README.md` el feed privado y NuGet público, mapeo de paquetes y autenticación sin credenciales en el repositorio. Verificar restauración aislada `net48` con cachés separadas y confirmar los orígenes efectivos de Data y Common; ver registro de ejecución.
- [x] Configurar `EntLibCommonVersion` en `6.3.0` y mantener las referencias condicionales de los cuatro proyectos; los `.nuspec` generados declaran Common público 6.3.0 directamente o a través de Data.
- [ ] Inspeccionar `project.assets.json` y los assets seleccionados por TFM en consumidores limpios; los resultados del modo `Project` no sustituyen la prueba del paquete público.
- [x] Reconciliar y convertir la solución secundaria: `Data (With Dependencies).slnx` contiene los seis proyectos principales y `Common.csproj`, sin SQL CE; `dotnet sln list` confirmó sus siete proyectos.
- [x] Centralizar la ruta configurable `EntLibCommon` en `package.props`, con ruta predeterminada para repositorios hermanos y modo `Package` predeterminado.
- [x] Configurar referencias excluyentes para los modos `Project` y `Package`, con error explícito para modo inválido o proyecto Common ausente; las comprobaciones y compilaciones Debug de ambos modos constan en el registro.
- [x] Aceptar el uso de Common 6.3.0 público para este cierre; por decisión del usuario no se requiere actualizar Common ni completar la validación del modo `Project` de la solución secundaria.

## Fase 2 — Solución y compilación

- [x] Generar `source\Data.slnx` y comprobar que contiene los seis proyectos principales; compilar la solución y descubrir y ejecutar pruebas en el entorno local.
- [ ] Completar la comparación de carpetas, elementos, configuraciones Debug/Release y Any CPU y secciones heredadas frente a la solución anterior. Verificar los cambios restantes en scripts y documentación; las soluciones `.slnx` son las actuales y se conserva la secundaria para desarrollo con Common.
- [x] Actualizar los targets declarados de los seis proyectos: producción `net48;netstandard2.0;net8.0;net10.0` y pruebas/soporte `net48;net8.0;net10.0`; compilación local y paquetes generados según el registro.
- [ ] Completar la revisión de condiciones y dependencias por TFM, incluidas las condiciones inactivas de los proyectos de pruebas.
- [ ] Revisar `#if NET6_0_OR_GREATER`, `NET8_0_OR_GREATER`, `NETSTANDARD2_0`, compilación condicional y exclusiones de archivos. Los símbolos `_OR_GREATER` también cubren .NET 10: no reemplazarlos mecánicamente si aún expresan el comportamiento correcto. En particular, revisar `AsynchronousExecutionFixture.cs`, excluido actualmente fuera de `net4`, y la rama `!NET8_0_OR_GREATER` en `OracleTransactionScopeFixture.cs`; registrar qué pruebas se ejecutan por target.
- [x] Conservar el shim `Src\Data\System.Data.Common` solo para `netstandard2.0` y comprobar resolución de la fábrica SQL en los tres hosts; ver registro de corrección del shim y sus límites.
- [ ] Completar la revisión del shim, configuración, carga de recursos, firma, documentación XML y `OutputPath`, incluidas colisiones entre compilaciones paralelas e interacción con las fábricas de los hosts.
- [x] Migrar la implementación SQL de producción a `Microsoft.Data.SqlClient` para los cuatro destinos y comprobar compilación y casos de conexión seleccionados; ver registro de ejecución.
- [x] Cerrar la revisión de compatibilidad para este requerimiento por decisión del usuario, con la evidencia de las pruebas realizadas. Esto no afirma que todas las combinaciones de configuración antigua o compatibilidad binaria se hayan probado exhaustivamente.

## Fase 3 — Análisis y actualización de NuGet

No elegir simplemente la versión más reciente. Usar versiones estables, mantenidas y compatibles con cada TFM y runtime, sin introducir deliberadamente paquetes de la línea .NET 11. En paquetes Microsoft `System.*` y `Microsoft.Extensions.*`, partir de la línea 10.0.x o de una anterior compatible; el número mayor de Oracle, SqlClient o herramientas de prueba **no indica por sí solo** que pertenezcan a .NET 11. Revisar también sus dependencias transitivas y assets reales. La tabla siguiente conserva el inventario inicial y las propuestas históricas; las versiones finalmente elegidas y su evidencia constan en los proyectos y el registro de ejecución.

| Dependencia | Situación actual | Análisis y acción propuesta |
|---|---|---|
| `EnterpriseLibrary.Common.NetCore` | `6.3.0`, o el proyecto local según `EntLibDependencyType`; `Oracle.Tests.VSTS` usa un ID explícito. | Probar primero el paquete público y su asset Standard en los destinos acordados; fijar ID y versión comprobados. Si funciona, conservarlo sin publicar el fork. Si existe una incompatibilidad o funcionalidad necesaria demostrada, usar el fork de `E:\vh4Apps\Tools\common-infrastructure` y publicar `10.0.0-rc1` en [el feed privado](https://nuget.aseinfo.net), sin planificar cambios de código o targets. Verificar API, dependencias transitivas y assets en ambos modos. |
| `System.Configuration.ConfigurationManager` | 4.6.0 en `netstandard2.0`; 6.0.1/8.0.0 en targets modernos; `net4` usa referencia de framework. | Evaluar la línea estable 10.0.x, que ofrece assets `netstandard2.0` y `net462`, para standard y .NET 10; mantener la referencia del framework en `net48` si corresponde. Comprobar configuración XML, resolución y transitive dependencies. |
| `System.Data.SqlClient` | 4.8.6 en `netstandard2.0`; `net4` usa el proveedor del framework. | Revisar estado de mantenimiento, avisos y vulnerabilidades de 4.8.6 y alternativas compatibles. Retirar esta dependencia al migrar a `Microsoft.Data.SqlClient`. Analizar API y compatibilidad binaria, migrar shim y registro de fábrica; conservar referencias antiguas solo para casos deliberados de compatibilidad o documentación histórica. |
| `Microsoft.Data.SqlClient` | 5.1.5 para `net6.0`, 5.2.0 para `net8.0`; también se usa en pruebas. | Evaluar 6.1.8 de la línea 6.1 LTS, o su parche vigente al implementar, para los cuatro destinos acordados. Alinear `Data`, `Data.SqlServer` y pruebas; comprobar cifrado, certificados, autenticación, SNI y assets nativos. Migrar también Framework y Standard; el recurso Standard del driver es de referencia, no ejecutable. |
| `Oracle.ManagedDataAccess` | 18.15.1 para `net4` en producción y soporte de pruebas. | Evaluar una versión estable compatible con `net48`, por ejemplo 23.26.0 o un parche posterior adecuado; alinear referencias y verificar servidor, x64, configuración y binding redirects. |
| `Oracle.ManagedDataAccess.Core` | 2.18.151 en `netstandard2.0`, 3.21.140 en `net6.0`, 23.4.0 en `net8.0`. | Separar versiones por TFM: evaluar 2.19.310 para `netstandard2.0` y una versión mantenida compatible para `net8.0`, y 23.26.0 o parche compatible para `net10.0`. Oracle documenta soporte de .NET 10 desde 23.26.0; ese paquete no ofrece asset `netstandard2.0`. Verificar soporte del proveedor antiguo en los hosts usados para probar el asset standard, y registrar cualquier limitación o bloqueo sin afirmar soporte solo por compatibilidad calculada de NuGet. Aplicar la decisión condicional indicada abajo si se confirma la ausencia de soporte de Oracle para `netstandard2.0`. |
| `Microsoft.Extensions.Configuration` y `.Json` | 6.0.x/8.0.0 en suites modernas. | Evaluar versiones compatibles para `net8.0` y 10.0.x para las pruebas `net10.0`, alinear versiones y comprobar lectura/copia de `appsettings.json`. Mantener la configuración de `net48` separada si corresponde. |
| `Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`, `MSTest.TestFramework` | 15.9.0, 1.4.0 y 1.4.0 en los tres proyectos de pruebas/soporte. | Evaluar una combinación estable VSTest que ejecute `net48`, `net8.0` y `net10.0`, por ejemplo Test SDK 17.14.1 con MSTest 4.x compatible. Alinear framework y adapter; verificar APIs y cambios de comportamiento, descubrimiento y ejecución en Visual Studio y CLI. No asumir que una referencia de paquete hace ejecutable `netstandard2.0`. Quitar dependencias de runner de `Data.TestSupport` solo si el análisis demuestra que no se necesitan. |
| `Moq` | 4.10.0 en pruebas `net4`. | Evaluar una versión estable compatible con `net48` y el uso real de mocks; revisar APIs y dependencias transitivas. Mantenerla condicionada si solo se usa en ese target. |

- [x] Seleccionar `EnterpriseLibrary.Common.NetCore` 6.3.0 público y `Microsoft.Data.SqlClient` 6.1.8 para la implementación SQL; generar paquetes y ejecutar los casos registrados con esas dependencias.
- [x] Actualizar Newtonsoft.Json a 13.0.1 en los proyectos de pruebas afectados y ejecutar la auditoría de vulnerabilidades de NuGet; ver inventario y limitaciones de pruebas Oracle.
- [x] Inspeccionar los grupos de dependencias y assets por TFM de los tres `.nupkg` generados; ver registro del 23 de noviembre. Los paquetes descargados coincidieron por hash con los locales; esto no cubre toda la resolución transitiva de consumidores.
- [ ] Completar la inspección de referencias reales y sus ramas, incluidas las versiones centralizadas y las fijas en cada proyecto; eliminar propiedades obsoletas solo después de comprobar usos. No forzar una única versión cuando los assets no sean compatibles con todos los targets.
- [ ] Para cada candidato consultar los metadatos NuGet, plataformas oficialmente soportadas, licencias, cambios incompatibles y avisos de seguridad. Ejecutar `dotnet list source\Data.slnx package --include-transitive`, `--outdated` y `--vulnerable --include-transitive` con el SDK seleccionado; revisar resultados por TFM y corregir incompatibilidades, downgrades y vulnerabilidades, sin ocultar advertencias. Registrar las versiones elegidas y la justificación, incluidas excepciones aprobadas.
- [ ] Validar que la selección transitiva no introduce dependencias de la línea .NET 11, que los grupos de dependencias del `.nuspec` sean correctos y que los consumidores no requieran versiones incompatibles entre sí. Resolver explícitamente cualquier conflicto, no usar supresiones como sustituto de la prueba.
- [x] Aceptar el alcance Oracle probado en el consumidor `net48` para este requerimiento, según confirmación del usuario. Esa prueba no valida el asset Oracle `netstandard2.0`; posteriormente se probaron operaciones básicas con dicho asset en los tres hosts, sin acreditar soporte oficial del proveedor.

## Fase 4 — Versión y empaquetado

- [x] Configurar `version.props` para `10.0.0-rc1`, generar los tres paquetes Release y comprobar IDs, versión NuGet, dependencias y assets incluidos. La publicación fue informada por el usuario; la descarga y comparación de artefactos constan en el registro.
- [x] Comparar los tres paquetes del feed con los generados por SHA-256 e inspeccionar símbolos y firmas; ver los resultados y la limitación de firma NuGet en el registro de ejecución.
- [x] Aceptar por decisión del usuario que estos paquetes no requieren firma NuGet en el feed privado. `dotnet nuget verify` devolvió NU3004 porque los tres `.nupkg` no están firmados; los ensamblados locales `net48` sí superaron `sn -vf`, que comprueba una firma distinta.
- [x] Actualizar las notas `PackageReleaseNotes` con los targets y el cambio de proveedor.
- [x] Documentar la migración en `docs\Pages\UpgradeNet10.md`, enlazarla desde README y la guía histórica, y añadir notas de `10.0.0-rc1` en `docs\Pages\ReleaseNotes.md`. La revisión integral de todas las referencias históricas sigue en tareas finales.
- [x] Preparar y utilizar SQL Server con Northwind y Oracle para las suites de integración locales, según el registro; la ejecución incluyó omisiones que aún deben revisarse.

## Fase 5 — Pruebas obligatorias

### Matriz de ejecución

Evidencia disponible para las suites nativas en Debug con Common público (20 de noviembre):

| Target | Data: aprobadas / fallidas / omitidas | Oracle: aprobadas / fallidas / omitidas | Estado |
|---|---:|---:|---|
| `net48` | 386 / 0 / 6 | 102 / 0 / 3 | Ejecutadas; revisar omisiones. |
| `net8.0` | 311 / 0 / 6 | 101 / 0 / 3 | Ejecutadas; revisar omisiones. |
| `net10.0` | 311 / 0 / 6 | 101 / 0 / 3 | Ejecutadas; revisar omisiones. |

El usuario informó el 23 de noviembre que todos los tests pasaron después de corregir las cadenas de net48; no se recibieron conteos ni detalle de omisiones de esa ejecución. Tras cambiar a la versión 10, se ejecutaron 12 pruebas seleccionadas de Data por cada uno de los tres targets, todas aprobadas. Estas evidencias no completan por sí solas la matriz Release ni las suites Standard forzadas.

- [x] Ejecutar las suites nativas de Data y Oracle en Debug con Common público en `net48`, `net8.0` y `net10.0`; resultados y omisiones en la tabla anterior.
- [x] Repetir pruebas seleccionadas de conexión y fábricas Data con la versión 10 en los tres targets: 12 aprobadas por target.
- [x] Probar consulta y rollback SQL con los assets `netstandard2.0` forzados de Data y Data.SqlServer en los tres hosts; es una validación parcial, no las suites Standard completas.
- [x] Probar consultas parametrizadas, lector y transacción de lectura con rollback en SQL y Oracle usando los tres assets `netstandard2.0` publicados, forzados en los tres hosts; ver registro para dependencias y límites.
- [x] Probar el paquete `10.0.0-rc1` en un consumidor real `net48`; el usuario informó que la prueba fue intensiva y funcionó como se esperaba. No se especificaron las operaciones ni los paquetes exactos utilizados.
- [x] Registrar la evidencia anterior informada por el usuario: la versión `7.0.0-rc2` ya se había probado en `net8.0` y `net10.0`. Estas pruebas corresponden a otra versión y no acreditan por sí solas el paquete `10.0.0-rc1` en esos targets.
- [x] Cerrar el requisito de ejecutar las suites completas en Release y revisar las omitidas por decisión del usuario, aceptando como evidencia las ejecuciones registradas y la prueba intensiva del consumidor. No se afirma que se hayan ejecutado las suites completas en Release ni que se hayan investigado todas las omisiones.
- [x] Aceptar por decisión del usuario las pruebas `netstandard2.0` realizadas como suficientes para este requisito; no se ejecutaron las suites Standard completas y podrían ampliarse posteriormente.

La tabla conserva la matriz inicialmente prevista; para este cierre se acepta el alcance Standard descrito en el registro, sin afirmar que se cubrieron exhaustivamente todas las validaciones de las filas.

| Host de pruebas | Assets de producción que deben cargarse | Evidencia requerida |
|---|---|---|
| Windows x64, .NET Framework 4.8 | `net48` de `Data`, `Data.SqlServer` y `Data.Oracle` | Ambas suites descubiertas y ejecutadas; pruebas unitarias e integración SQL/Oracle. |
| Windows x64, .NET 8 | `net8.0` de los tres proyectos | Ambas suites e integración SQL/Oracle; runtime conservado por compatibilidad. |
| Windows x64, .NET 10 | `net10.0` de los tres proyectos | Las mismas categorías aplicables, incluyendo integración SQL/Oracle. |
| Windows x64, .NET Framework 4.8 | `netstandard2.0` de los tres proyectos | Consumidores/hosts de compatibilidad que fuerzan esos assets y ejecutan pruebas funcionales y de integración aplicables. |
| Windows x64, .NET 8 | `netstandard2.0` de los tres proyectos | Forzar los assets Standard y verificar implementación ejecutable del driver, configuración e integración. |
| Windows x64, .NET 10 | `netstandard2.0` de los tres proyectos | Igual, verificando especialmente shim de proveedor y dependencias Oracle/SQL. |

Si se confirma la excepción Oracle de la fase 3 y se elimina su target `netstandard2.0`, las tres filas standard aplican solo a `Data` y `Data.SqlServer`; documentar y probar el comportamiento de los consumidores que intenten usar Oracle con ese TFM, sin afirmar soporte para él. Las filas `net48`, `net8.0` y `net10.0` siguen incluyendo los tres proyectos.

- [x] Implementar un host de compatibilidad que referencie explícitamente las tres DLL `netstandard2.0` publicadas y comprobar el TFM y los hashes de los binarios cargados en los tres runtimes. Las dependencias directas y el alcance ejecutado constan en el registro; no se probó la selección normal de assets por NuGet.
- [x] Cerrar por aceptación del usuario la ampliación de las suites Standard para este requerimiento. Se ejecutaron los casos descritos en el registro; no se afirma que se hayan reutilizado o ejecutado todas las pruebas de las suites.
Como referencia, los siguientes comandos permiten ejecutar las suites nativas en Release; no se exige su ejecución completa para este cierre por decisión del usuario:

```powershell
dotnet restore source\Data.slnx
dotnet build source\Data.slnx -c Debug --no-restore
dotnet build source\Data.slnx -c Release --no-restore
dotnet test source\Tests\Data.Tests\Data.Tests.VSTS.csproj -c Release -f net48 --settings source\.runsettings --logger trx
dotnet test source\Tests\Data.Tests\Data.Tests.VSTS.csproj -c Release -f net8.0 --settings source\.runsettings --logger trx
dotnet test source\Tests\Data.Tests\Data.Tests.VSTS.csproj -c Release -f net10.0 --settings source\.runsettings --logger trx
dotnet test source\Tests\Oracle.Tests.VSTS\Oracle.Tests.VSTS.csproj -c Release -f net48 --settings source\.runsettings --logger trx
dotnet test source\Tests\Oracle.Tests.VSTS\Oracle.Tests.VSTS.csproj -c Release -f net8.0 --settings source\.runsettings --logger trx
dotnet test source\Tests\Oracle.Tests.VSTS\Oracle.Tests.VSTS.csproj -c Release -f net10.0 --settings source\.runsettings --logger trx
```

Estos seis comandos prueban los assets nativos seleccionados por defecto; **no sustituyen** las pruebas realizadas con los assets Standard forzados. El usuario acepta estas últimas para el cierre, sin exigir las suites Standard completas. Usar nombres de TRX y directorios de resultados distintos para evitar sobrescrituras si se ejecutan.

### Casos funcionales y de regresión

- [x] Aceptar los resultados y omisiones registrados para este cierre por decisión del usuario; no se afirma que todas las omisiones se hayan investigado individualmente.
- [ ] Configuración XML y JSON, cadenas de conexión, registro y resolución de proveedores, fábricas, configuración fluida, errores de proveedor inexistente y carga de recursos. Comprobar las rutas distintas para `System.Data.SqlClient`, `Microsoft.Data.SqlClient` y Oracle.
- [ ] SQL Server: conexión, consultas parametrizadas y procedimientos, parámetros de entrada/salida, lectores, `DataSet`, mapeo, transacciones commit/rollback, concurrencia, errores y liberación de recursos. Verificar APM donde sigue soportado y comportamiento asíncrono disponible en .NET 8 y .NET 10; comprobar la autenticación con la versión elegida de SqlClient; la validación con certificado confiable está fuera del alcance por decisión del usuario.
- [ ] Oracle: conexión, procedimientos/paquetes, cursores y parámetros, tipos, lectura/escritura, errores, transacciones commit/rollback y comportamiento de `TransactionScope` según plataforma y proveedor. Revisar las pruebas ignoradas o excluidas existentes y las diferencias soportadas, sin habilitarlas a ciegas.
- [x] Aceptar las comprobaciones Standard con assets forzados para este cierre, con los límites del registro; no se afirma soporte oficial de Oracle en todas las combinaciones. Las pruebas en consumidores limpios `net8.0` y `net10.0` se difieren a otro requerimiento; no se presume que la prueba real net48 haya cubierto un entorno limpio o todos los binding redirects.
- [x] Aceptar las pruebas Standard registradas como suficientes para este requisito. La publicación y las comprobaciones pendientes de símbolos se difieren; la ausencia de firma NuGet fue aceptada.
- [x] Aceptar Common público como dependencia para este cierre; no se exige validar adicionalmente el modo proyecto de Common.

## Plan integrado de migración SQL Server

Esta sección complementa las fases generales y reemplaza la propuesta anterior de conservar `System.Data.SqlClient` en Framework y Standard. No cambia el proveedor Oracle.

El recurso Standard de SqlClient 6.1 es de referencia: no contiene una implementación ejecutable. La aplicación debe seleccionar una implementación compatible del driver. Probar el artefacto Standard de esta biblioteca en los tres hosts y evitar desplegar por copia manual la DLL de referencia del driver.

Los tipos de ambos proveedores son distintos. Revisar comandos, parámetros, lectores, transacciones, builders y excepciones en API y consumidores. En particular, `WithConnectionString(SqlConnectionStringBuilder)` cambia su firma concreta y `DefaultSqlProviderName` es una constante pública cuyo valor puede estar incorporado en consumidores ya compilados. Documentar recompilación y cambios necesarios.

Las pruebas locales usaron confianza explícita del certificado; las cadenas net48 de Data también especifican `Encrypt=False`. Por decisión del usuario, no se exige probar un certificado confiable. Validar autenticación, SNI, arquitecturas y binding redirects de Framework; esta configuración local no establece una recomendación general de despliegue.

### Inventario SQL y puntos de cambio

La tabla siguiente conserva el inventario inicial y las actividades propuestas; el estado actual se indica en las casillas y el registro de ejecución.

| Archivo o área | Hallazgo y trabajo esperado |
|---|---|
| `source/Src/Data.SqlServer/SqlDatabase.cs` | Usa `NET6_0_OR_GREATER` para seleccionar proveedor; convierte comandos y adaptadores a tipos concretos. Revisar también llamadas Begin/End y manejo de recursos. |
| `source/Src/Data.SqlServer/SqlDatabase.Desktop.cs` | Selección condicional del proveedor; revisar toda la clase parcial. |
| `source/Src/Data.SqlServer/Configuration/Fluent/SqlDatabaseConfigurationExtension.cs` | `WithConnectionString(SqlConnectionStringBuilder)` expone un tipo del proveedor en una firma pública. |
| `source/Src/Data/Configuration/DbProviderMapping.cs` | Nombre predeterminado condicional: `System.Data.SqlClient` o `Microsoft.Data.SqlClient`. Es constante pública; revisar consumidores compilados con el valor anterior. |
| `source/Src/Data/System.Data.Common/` | Implementación propia de fábricas utilizada solo para Standard, con referencias al proveedor anterior. Migrarla y probar la resolución del nuevo proveedor para Standard. |
| `source/Src/Data/Data.csproj` y `source/Src/Data.SqlServer/Data.SqlServer.csproj` | Targets y dependencias condicionadas; hoy usan 5.1.5 para .NET 6 y 5.2.0 para .NET 8. |
| `source/Tests/` | Usings, registro de fábricas, nombres de proveedor, configuración y referencias del driver; las suites actuales no tienen target explícito `net472`. |
| `source/Tests/Data.Tests/Data.Tests.VSTS.csproj` | Pruebas asíncronas heredadas excluidas fuera de `net4`; registrar cobertura real por runtime. |
| `package.props` | Propiedades de versiones, Common, notas de paquete y salida compartida; comprobar usos antes de eliminar propiedades. |
| `source/Src/Data.Oracle/` | Revisar coherencia de targets y dependencias compartidas sin sustituir su proveedor Oracle por SqlClient. |

### Actividades SQL y evidencias

#### 1. Cerrar alcance y consumidores

- [ ] Inventariar aplicaciones y bibliotecas consumidoras: TFM, runtime instalado, Windows/SO, arquitectura, versión del paquete, configuración y uso directo de tipos SqlClient.
- [ ] Identificar consumidores de Standard y .NET 8, que se conservan, y consumidores Framework anteriores a 4.8 que requieren una ruta de actualización. Registrar impacto y ruta de actualización.
- [ ] Aplicar la matriz acordada de cuatro destinos; validar con evidencia la excepción Oracle de Standard y mantener coherentes las secciones de este documento.
- [x] Documentar la migración explícita al nuevo nombre de proveedor y aceptar el alcance de compatibilidad antigua según la decisión del usuario; no se garantiza que todos los aliases o configuraciones anteriores funcionen.
- [ ] Registrar versión mayor, prerelease, responsables y límites del alcance.

**Evidencia:** tabla de consumidores, matriz acordada y registro de decisiones al final del documento.

#### 2. Preparar herramientas y pruebas

- [x] Fijar y verificar localmente el SDK 10.0.404; registrar las comprobaciones de Common público y Common en modo proyecto. Ver fase 1 y registro de ejecución.
- [x] Preparar SQL Server local con Northwind y autenticación SQL de prueba; ejecutar casos de lectura y rollback, conforme al registro.
- [ ] Registrar commit, rama, estado de trabajo, SDK, runtime, MSBuild y targeting pack de Framework 4.8. No sobrescribir cambios ajenos.
- [ ] Completar la verificación local de herramientas de desarrollo compatibles con SDK 10.
- [ ] Registrar compilación, descubrimiento y pruebas ejecutadas, omitidas y fallidas por target; investigar y documentar los fallos observados.
- [ ] Inventariar dependencias efectivamente resueltas y comprobar Common en modo paquete y proyecto, si ambos permanecen dentro del alcance.
- [ ] Completar la definición de autenticación, aislamiento y limpieza reproducible de datos de integración para otros consumidores.

**Evidencia:** entorno, resultados y conteos de pruebas y dependencias resueltas.

#### 3. Demostrar compatibilidad del candidato

- [x] Compilar los cuatro destinos con SqlClient 6.1.8 y comprobar los grupos de dependencias de los paquetes generados; ejecutar casos SQL en hosts nativos y Standard forzado según el registro.
- [ ] Verificar documentación, metadatos NuGet, cambios incompatibles y avisos del parche de SqlClient elegido.
- [ ] Probar restauración y compilación con el driver para cada destino aprobado. Inspeccionar assets y dependencias reales; compilar no acredita ejecución.
- [ ] Revisar el contrato público y binario antes/después, incluidos `WithConnectionString`, constantes de proveedor, tipos concretos y ensamblados firmados.
- [ ] Confirmar en consumidores mínimos `net48`, `net8.0` y `net10.0` conexión, creación de comandos y ejecución de una consulta.
- [ ] Registrar cualquier API no disponible o comportamiento diferente antes de ampliar la migración.

**Evidencia:** versión exacta elegida, dependencias por TFM, diferencia de API y resultados de los consumidores mínimos.

#### 4. Implementar destinos, proveedor y configuración

- [x] Actualizar los targets declarados de producción, pruebas y soporte según la matriz acordada; compilar y generar los paquetes.
- [ ] Comprobar consumidores internos y referencias compartidas en los escenarios pendientes.
- [ ] Alinear referencias de SqlClient en Data, Data.SqlServer y pruebas; evitar versiones fijas divergentes y retirar propiedades sin usos.
- [ ] Revisar todos los condicionales del proveedor y archivos parciales. Los símbolos `_OR_GREATER` incluyen .NET 10: no sustituirlos mecánicamente.
- [x] Cerrar la revisión de fábricas, mapeos, configuración antigua y compatibilidad para este requerimiento por decisión del usuario, con las limitaciones de pruebas registradas.
- [x] Migrar el shim Standard y comprobar la resolución de la fábrica SQL en los tres hosts con assets forzados; ver registro de ejecución.
- [x] Aceptar las pruebas registradas del shim y los casos Standard para este cierre; no se realizaron las suites Standard completas ni se garantiza que todos los registros de fábrica de los hosts sean compartidos.
- [ ] Revisar casts, parámetros, excepciones, autenticación y métodos APM/TAP. Preservar comportamiento público salvo cambios deliberados y documentados.
- [ ] Retirar referencias innecesarias a `System.Data.SqlClient`; distinguir referencias de producción vigentes de documentación histórica o casos deliberados de compatibilidad.
- [x] Registrar los aliases heredados de `source/Tests/Oracle.Tests.VSTS/App.config` y `NetCoreHelpers`: en `net48` apuntan a la fábrica de `System.Data.SqlClient`; en `net8.0` y `net10.0` el nombre antiguo se registra como alias de `Microsoft.Data.SqlClient`. La revisión adicional se cierra por decisión del usuario; estos registros de tests no se incluyen como garantía de compatibilidad para consumidores.
- [x] Aceptar las pruebas existentes para este cierre por decisión del usuario; no se afirma que exista cobertura exhaustiva de configuración antigua o compatibilidad binaria.

**Evidencia:** cambios revisables y compilación de todos los destinos, sin advertencias nuevas sin resolver o justificar.

#### 5. Validar comportamiento y despliegue

| Host | Artefacto | Validaciones mínimas |
|---|---|---|
| Windows con Framework 4.8 | `net48` | Configuración, fábrica, operaciones SQL, APM aplicable, transacciones, SNI, binding redirects y despliegue limpio. |
| .NET 8 en los SO comprometidos | `net8.0` | Configuración, fábrica, operaciones SQL, async, transacciones, publicación y dependencias. |
| .NET 10 en los SO comprometidos | `net10.0` | Configuración, fábrica, operaciones SQL, async aplicable, transacciones y publicación/carga de dependencias. |
| Framework 4.8, .NET 8 y .NET 10, forzando Standard | `netstandard2.0` forzado de la biblioteca | Evidencia del artefacto cargado y de la implementación ejecutable correcta de SqlClient. |

- [ ] Probar consultas y procedimientos, entrada/salida de parámetros, descubrimiento de parámetros, lectores, XML, DataSet y actualizaciones.
- [ ] Probar commit/rollback, `TransactionScope` donde se soporte, errores SQL y liberación de conexiones/lectores; comprobar los tipos de excepción esperados.
- [ ] Probar configuración directa y mediante fábrica/configuración fluida, nombre de proveedor y comportamiento de configuración antigua según la decisión tomada.
- [ ] Verificar las rutas asíncronas existentes por runtime, incluidas pruebas actualmente excluidas; no declarar cobertura por la sola presencia de fixtures.
- [x] Probar autenticación SQL con el usuario de pruebas en Northwind y ejecutar consulta y rollback en los tres hosts con assets nativos y Standard forzados; ver registro.
- [ ] Evaluar autenticación integrada y Entra ID si forman parte de los consumidores comprometidos. La prueba con certificado confiable es **no aplicable** por decisión del usuario.
- [ ] Validar x86/x64 si ambas arquitecturas están comprometidas; registrar otras arquitecturas y SO como probados o fuera del alcance.
- [x] Ejecutar las suites nativas de regresión de Data y Oracle con las omisiones registradas; el usuario informó que posteriormente todos los tests pasaron.
- [x] Aceptar las regresiones registradas para este cierre sin exigir suites completas Release o Standard ni la revisión individual de todas las omisiones; no se afirma que se hayan realizado esas actividades.
- [ ] Registrar los conteos de pruebas y las exclusiones. Pruebas omitidas por falta de base de datos, credenciales o infraestructura no cuentan como aprobadas.

**Evidencia:** resultados por host/artefacto, logs o TRX sin secretos, conteos, omisiones y limitaciones.

#### 6. Empaquetar y preparar entrega

- [x] Generar los tres paquetes Release `10.0.0-rc1` y examinar IDs, versión, grupos de dependencias y assets por TFM; el usuario informó su publicación privada.
- [x] Comparar los `.nupkg` publicados con los locales, inspeccionar los `.snupkg` locales y ejecutar la verificación de firma; los hashes coinciden y los `.nupkg` no tienen firma NuGet. Ver registro para límites y resultados.
- [x] Aceptar explícitamente la ausencia de firma NuGet para estos paquetes en el feed privado; no se exige una verificación de firma NuGet satisfactoria para este cierre.
- [ ] Verificar restauración y ejecución en un consumidor limpio para los escenarios que se acuerden; las pruebas específicas de `net8.0` y `net10.0` se difieren a otro requerimiento.
- [x] Forzar y verificar los tres artefactos Standard en hosts de compatibilidad, con el alcance registrado; la selección normal de NuGet podría elegir `net48` o `net10.0` y no se validó con esta prueba.
- [x] Documentar esta entrega en README, notas de versión y guía de actualización, conservando el contexto histórico en el plan.
- [x] Documentar en `docs\Pages\UpgradeNet10.md` los pasos del consumidor: targets, tipos, `providerName`, fábrica, certificados, dependencias y recompilación.
- [ ] Registrar artefactos y commits verificados; mantener la versión anterior disponible para consumidores que aún no migren.
- [ ] Preparar la entrega para revisión. Publicación e integración seguirán la autorización aplicable y el alcance aprobado, sin inferirla de esta lista.

### Criterios de aceptación SQL

- [ ] Matriz acordada y decisiones de implementación coherentes en este documento.
- [ ] Todos los destinos aprobados compilan y sus dependencias son compatibles con el runtime comprometido.
- [ ] Se utiliza `Microsoft.Data.SqlClient` en las rutas SQL acordadas, sin selección accidental del proveedor anterior.
- [ ] Cambios de API, tipos, constantes y configuración identificados y documentados como incompatibles cuando corresponda.
- [ ] Pruebas funcionales, integración y consumidores del NuGet ejecutados con evidencia; ninguna combinación pendiente se presenta como verificada.
- [ ] Autenticación, SNI y carga de dependencias funcionan en los entornos comprometidos. La validación con certificado confiable es **no aplicable** por decisión del usuario.
- [ ] Paquetes y documentación reflejan la misma versión y matriz de soporte.

### Trazabilidad y preparación de entrega — 23 de noviembre de 2026

- Rama inspeccionada: `_net_10_migration`; `HEAD` al registrar inicialmente esta información: `5c5478f7543327ca7e9f33241f8c6b3e37a779d5`. El commit `31e6b71d985564de38e424f72e5b86b975ff888a` preparó la versión `10.0.0-rc1`; el posterior `5c5478f` cambió dependencias y configuración de pruebas. El commit que contiene esta documentación no resuelve por sí solo la trazabilidad de los paquetes.
- Los tres `.nupkg` locales bajo `bin\Release` conservan los SHA-256 que se compararon con los paquetes descargados del feed privado: Data `71455714a03baf4ff90e8080d82b87fae3e6a7172b80c997fb3c782e9102cbe5`, SqlServer `3cb4561c77a76d4d8afa39e4dd75983ddda6e3f4fd8caa7935c1c801f0178429`, Oracle `a23a6450b6c868d772258d85595617652a946132a73484b3553fb4e302d7a688`. La comparación demuestra igualdad entre las copias comparadas, no el origen exacto del código compilado.
- **Limitación de trazabilidad:** los tres `.nuspec` locales declaran como commit del repositorio `478dc05d588b9612e438f75ef86375034e407aeb`, que existe localmente pero no es ancestro del `HEAD` de esta rama. Entre ese commit y `31e6b71` no hay diferencias en `source\Src`, pero sí en `package.props`, `version.props` y `source\Data.slnx`; por tanto, el commit declarado por sí solo no reproduce la configuración de esta publicación y no prueba que el estado de trabajo usado al empaquetar coincidiera con el commit de entrega. No se ha verificado una compilación reproducible ni se ha reemplazado ningún paquete.
- Common: `EnterpriseLibrary.Common.NetCore` 6.3.0, NuGet.org, SHA-256 del paquete `61a6b443bf762495628a5d854e5981ecf9a22ed8d06dcf14dc560fd25f9a37f4`. El inventario por TFM está en la fase 1; la restauración aislada confirmó los orígenes, con los límites del registro.
- La guía de migración, las notas de versión y los temas posteriores están preparados para revisión. El host Standard es un artefacto de sesión en `session-state\files\standard-check`, no parte de la solución ni de la entrega versionada. La disponibilidad futura de versiones anteriores del feed no se ha verificado.
- Antes de integrar, una persona responsable debe resolver explícitamente la discrepancia del commit del paquete y registrar el commit definitivo de entrega y de documentación. No sobrescribir ni eliminar `10.0.0-rc1` por esta discrepancia: cualquier decisión sobre nuevos artefactos, versión o publicación requiere autorización y comprobación de la política del feed.

### Conciliación de pendientes

Las casillas marcadas reflejan evidencia o una aceptación explícita, no necesariamente una ejecución exhaustiva. Las tareas exclusivas del fork Common, la publicación de símbolos y los consumidores net8/net10 diferidos constan en [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md), sin contarlos como pruebas aprobadas. Siguen abiertas las revisiones no cubiertas por dichas decisiones, incluidas condiciones y referencias por TFM, compatibilidad del proveedor y despliegue, y la trazabilidad indicada arriba. En particular, los comentarios XML de `SqlDatabase.cs` y `Configuration\Fluent\DatabaseProviderExtensions.cs` aún mencionan `System.Data.SqlClient` para Framework y Standard, aunque la implementación usa `Microsoft.Data.SqlClient`; la guía advierte esa discrepancia. Las casillas de integración quedan para el final; este registro no autoriza integrar ni publicar.

### Registro de decisiones

| Fecha | Decisión | Responsable | Motivo/evidencia |
|---|---|---|---|
| 2026-11-23 | Diferir la integración en `master` hasta que el usuario confirme que ya no tiene reportes de problemas con `10.0.0-rc1`. | Usuario | Las condiciones y actividades previas siguen en [POST-UPGRADE-NET10.md](POST-UPGRADE-NET10.md); no se fijó un plazo de espera. |
| 2026-11-23 | Conservar `EnterpriseLibrary.Common.NetCore` 6.3.0 del feed público; no publicar el fork de Common para esta migración. | Usuario | Restauración aislada, suites nativas y casos Standard parciales con Common público; los consumidores limpios siguen pendientes y se acepta para el cierre la evidencia Standard disponible. |
| 2026-11-20 | Targets acordados: net48, netstandard2.0, net8.0 y net10.0. Plan SQL integrado; implementación pendiente. | Usuario | Instrucción explícita de integrar el plan y conservar los cuatro destinos. |
| 2026-11-20 | Common ya soporta net10.0 y netstandard2.0 en la rama actual del fork local; alternativa disponible: empaquetado, validación y publicación en el feed privado solo si se necesita frente al paquete público. | Usuario | Ruta: `E:\vh4Apps\Tools\common-infrastructure`. Feed: https://nuget.aseinfo.net. |
| 2026-11-20 | Probar primero Common público compatible con Standard; conservarlo y omitir la publicación del fork si las validaciones pasan. Mantener el fork como alternativa condicionada. | Usuario | Pruebas pendientes; fijar paquete y versión a partir de evidencia. |
| 2026-11-20 | No se requiere probar con un certificado confiable; usar la excepción de confianza para las pruebas locales de desarrollo y documentar el alcance. | Usuario | Instrucción explícita durante la ejecución. |

### Registro de ejecución

Agregar una fila por actividad relevante; usar estados pendiente, en curso, validado o bloqueado. Marcar casillas solo con evidencia y anotar el motivo de cualquier actividad no aplicable.

| Fecha | Actividad | Commit/entorno | Resultado | Evidencia o bloqueo |
|---|---|---|---|---|
| 2026-11-23 | Revisión de avisos de paquetes | SDK 10.0.404; solución; dependencias restauradas | Sin avisos en las fuentes consultadas | `dotnet list source\Data.slnx package --vulnerable --include-transitive --no-restore` no reportó paquetes vulnerables para los seis proyectos usando NuGet.org y la fuente offline configurada. No sustituye una revisión completa de licencias, compatibilidad o avisos que no estén en esas fuentes. |
| 2026-11-23 | Guía y trazabilidad de entrega | Rama `_net_10_migration`; HEAD `5c5478f`; paquetes `10.0.0-rc1` | Preparado con bloqueo de trazabilidad | Se prepararon la guía y los documentos de entrega. Los hashes locales coinciden con la comparación previa del feed, pero los tres `.nuspec` declaran `478dc05d588b9612e438f75ef86375034e407aeb`, ajeno a la ascendencia actual y anterior a cambios en versión y empaquetado. Falta resolver su relación con el commit definitivo antes de integrar. |
| 2026-11-23 | Cierre de pruebas Standard | Paquetes 10.0.0-rc1; tres runtimes | Aceptado por el usuario | Se aceptan las pruebas parciales registradas como suficientes para este requisito. No se ejecutaron las suites Standard completas ni se validó la selección automática de assets; la prueba tampoco acredita soporte oficial de Oracle. |
| 2026-11-23 | Host de compatibilidad Standard | Windows; SDK 10.0.404; .NET Framework 4.8, .NET 8.0.34 y .NET 10.0.15 | Validado parcialmente | Host temporal compilado sin errores ni advertencias; referencias explícitas a las tres DLL `netstandard2.0` extraídas de los paquetes 10.0.0-rc1 restaurados del feed. En cada runtime se comprobó `TargetFrameworkAttribute` y SHA-256 de las DLL cargadas frente a las restauradas. SQL y Oracle completaron consultas parametrizadas escalares, lectores y una consulta dentro de una transacción revertida; no se modificaron datos. Dependencias directas del host: Common 6.3.0, Microsoft.Data.SqlClient 6.1.8, Oracle.ManagedDataAccess.Core 2.19.310 y ConfigurationManager 10.0.0. La prueba fuerza referencias de DLL y no demuestra selección normal de NuGet, soporte oficial de Oracle ni suites completas. |
| 2026-11-23 | Símbolos y validación Standard | Feed privado y futuro consumidor real | Símbolos diferidos; Standard pendiente en ese momento | El usuario informa que el feed no admite publicar archivos de símbolos y difiere su publicación y las comprobaciones pendientes. En ese momento no podía probar Standard en el consumidor real; ver las filas posteriores sobre el host de compatibilidad y la aceptación del alcance probado. |
| 2026-11-23 | Decisión de firma NuGet | Paquetes Data 10.0.0-rc1 en el feed privado | Aceptada | El usuario acepta que estos paquetes no requieren firma NuGet. La verificación anterior devolvió NU3004; se conserva la distinción entre firma NuGet, strong-name y comparación de hashes. |
| 2026-11-23 | Configuración y restauración aislada del feed | SDK 10.0.404; consumidor temporal net48 | Validado para restauración | `NuGet.Config` con mapeo exacto para los tres paquetes Data y NuGet público para el resto; restauración con carpeta de paquetes aislada y `--no-http-cache`. Los `.nupkg.metadata` confirmaron los tres paquetes Data 10.0.0-rc1 desde el feed privado y Common 6.3.0 desde NuGet público. No acredita ejecución funcional ni autenticación cuando el feed la exija. |
| 2026-11-23 | Suites completas tras corregir configuración | Debug; net48/net8.0/net10.0 | Validado con omisiones | Data: net48 386 aprobadas/0 fallidas/6 omitidas; net8 y net10 311/0/6 cada uno. Oracle: net48 102/0/3; net8 y net10 101/0/3 cada uno. Las pruebas omitidas no cuentan como aprobadas. |
| 2026-11-23 | Corrección de configuración de pruebas Oracle | Debug; net48/net8.0/net10.0 | Validado con omisiones | Se eliminaron fragmentos que hacían inválido `appsettings.json`. Suite Oracle completa: net48 102 aprobadas/0 fallidas/3 omitidas; net8 y net10 101/0/3 cada uno. La auditoría de paquetes no reportó vulnerabilidades con las fuentes configuradas. |
| 2026-11-23 | Actualización de Newtonsoft y revisión de dependencias | Modo Package; SDK 10.0.404 | Validado parcialmente | Fijada 13.0.1 en los tres proyectos de pruebas para net8/net10; restauración y compilación correctas, auditoría sin avisos con las fuentes configuradas. Data: 12 pruebas seleccionadas aprobadas por target. Oracle: 4 aprobadas en net48 y una prueba de serialización aprobada en net8/net10; 3 pruebas de OracleDatabaseFixture en net8 fallaron al cargar `appsettings.json` inválido, problema preexistente independiente de Newtonsoft. |
| 2026-11-23 | Comparación de paquetes publicados | Feed privado; 10.0.0-rc1 | Validado con limitaciones | Los SHA-256 de los tres `.nupkg` locales y descargados coinciden: Data `71455714a03baf4ff90e8080d82b87fae3e6a7172b80c997fb3c782e9102cbe5`; SqlServer `3cb4561c77a76d4d8afa39e4dd75983ddda6e3f4fd8caa7935c1c801f0178429`; Oracle `a23a6450b6c868d772258d85595617652a946132a73484b3553fb4e302d7a688`. Los `.snupkg` locales contienen PDB para cuatro targets; no se verificó la publicación de símbolos ni correspondencia PDB/binario. `dotnet nuget verify` devolvió NU3004: los tres paquetes no están firmados. `sn -vf` validó strong-name de los tres ensamblados locales net48; sus versiones de ensamblado y archivo son 10.0.0.0. |
| 2026-11-23 | Pruebas con consumidor real | Paquete `10.0.0-rc1`; consumidor `net48` | Validado según el usuario | El usuario informó que realizó pruebas intensivas reales y que todo funcionó como se esperaba. No se indicaron las operaciones, los paquetes exactos ni los resultados detallados; no se infiere que cubran toda la matriz o un entorno limpio. |
| 2026-11-23 | Pruebas de versión anterior | Paquete `7.0.0-rc2`; `net8.0` y `net10.0` | Validado según el usuario | El usuario informó que esa versión ya se había probado en ambos targets. No se indicó el detalle de las pruebas; esta evidencia no acredita directamente la versión `10.0.0-rc1`. |
| 2026-11-23 | Configuración SQL net48 | Debug; SQL local; `SqlDatabaseFixture` | Validado para casos seleccionados | Tras actualizar las cadenas en el archivo de configuración de Data y recompilar, las 5 pruebas del fixture pasaron en net48, net8 y net10. El usuario informó posteriormente que todos los tests pasaron, sin aportar conteos ni detalle de omitidas. |
| 2026-11-23 | Versión 10 y pruebas seleccionadas | Debug; Common público; net48/net8.0/net10.0 | Validado para casos seleccionados | Compilación de la solución correcta; 12 pruebas seleccionadas de conexión y fábricas Data aprobadas por target, 0 fallidas y 0 omitidas. No representa una nueva ejecución completa de ambas suites. |
| 2026-11-23 | Empaquetado y publicación privada | Release; 10.0.0-rc1 | En curso | Generados tres `.nupkg` y `.snupkg`; inspeccionados IDs, versiones, grupos de dependencias y assets de los cuatro targets. El usuario informó que publicó los paquetes en `nuget.aseinfo.net`; la comparación posterior, firmas y símbolos se registran en la fila de comparación de paquetes publicados. |
| 2026-11-20 | Preparación inicial | `_net_10_migration`, commit `4eac6861fde2d1b9f08a9abc6cc456549e5d9567`; SDK local 10.0.404; Windows x64 | En curso | Compilación Debug de `Data.sln`: 0 errores y 0 advertencias. Descubrimiento de ambas suites en net47, net6.0 y net8.0; no se ejecutaron pruebas. SDK y runtime 8 aislados en `.dotnet`; `global.json` fija SDK 10.0.404. `Data.slnx` generado con seis proyectos; conversión y Visual Studio pendientes de validación completa. |
| 2026-11-20 | Referencias Common y solución secundaria | Common local `master`, commit `68c913e7659716d24b5186e3f8afdefe5518830e` | En curso | Modo Package: compilación Debug correcta, 0 advertencias. Modo Project: restauración y compilación Debug correctas con ocho advertencias MSB3884 de Common por archivo de reglas no encontrado. Ruta inexistente y modo inválido fallan de forma explícita. Pendientes pruebas funcionales, validación de assets por TFM y apertura en Visual Studio. |
| 2026-11-20 | Common público, restauración aislada | `EnterpriseLibrary.Common.NetCore` 6.3.0; origen exclusivo `https://api.nuget.org/v3/index.json` | En curso | SHA-256 del nupkg: `61A6B443BF762495628A5D854E5981ECF9A22ED8D06DCF14DC560FD25F9A37F4`. Contiene `netstandard2.0`; Data selecciona `net47` para net47/net472, `netstandard2.0` para Standard y `netcoreapp2.2` para net6/net8. Compilación Debug aislada: 0 errores, 0 advertencias. `ProviderMappingFixture.WillGetMappedProviderTypeEvenIfDefaultExists`: 1 aprobada en net47 y 1 en net8. Pendientes matriz objetivo, integraciones, activos cargados y consumidores limpios. |
| 2026-11-20 | Matriz inicial y Common en .NET 10 | SDK 10.0.404; Common público 6.3.0 | En curso | Proyectos de producción: net48/netstandard2.0/net8.0/net10.0; pruebas: net48/net8.0/net10.0. Compilación Debug: 0 errores, 12 advertencias NU1903 por Newtonsoft.Json 9.0.1 transitivo de pruebas. Una prueba de configuración aprobada en cada runtime. Consumidores independientes net8/net10 cargaron Common netcoreapp2.2 y crearon DictionaryConfigurationSource. SqlClient 6.1.8 aplicado solo a net8/net10; Framework/Standard, Oracle, seguridad y suites completas siguen pendientes. |
| 2026-11-20 | Migración SQL parcial | SDK 10.0.404; Microsoft.Data.SqlClient 6.1.8 | En curso | Referencias y tipos de producción migrados a SqlClient para Framework y Standard; shim actualizado. Compilación Debug: 0 errores y 12 advertencias NU1903 de Newtonsoft.Json transitivo de pruebas. Una prueba de configuración aprobada por runtime; no acredita conexión SQL ni shim Standard. Pendientes configuración antigua, registro de fábricas, pruebas de integración, API, seguridad y documentación. |
| 2026-11-20 | Pruebas aisladas de configuración y mapeo | Debug, Common público; net48/net8.0/net10.0 | En curso | Por runtime: mapeo 20 aprobadas/0 fallidas/0 omitidas; configuración 9 aprobadas/1 fallida/0 omitidas. Falla `GivenConfigurationForAnOracleDatabase.when_creating_database_then_creates_oracle_database_with_packages`: Sequence contains no elements; investigar registro de fábrica Oracle. No se ejecutaron integraciones ni hosts Standard. Filtro inicial rechazado en net8/net10; repetido con filtro compatible. |
| 2026-11-20 | Registro de fábrica en pruebas Oracle | Debug; net48/net8.0/net10.0 | En curso | Ocho pruebas fallaban en cada runtime con colección vacía. Se agregó registro en la preparación; net8/net10: 8 aprobadas, 0 fallidas, 0 omitidas por runtime. net48: 0 aprobadas, 8 fallidas, 0 omitidas; el helper no registra fábricas en Framework y queda pendiente configurar/verificar el host. No se utilizaron bases de datos. |
| 2026-11-20 | Diagnóstico de fábrica Oracle en Framework | VSTest con testhost.net48.exe; Oracle.ManagedDataAccess 18.15.1 | En curso | Consumidor net48 independiente con App.config registró y resolvió OracleClientFactory correctamente, sin base de datos. Los ocho casos bajo VSTest siguen fallando; DisableAppDomain=false no cambió el resultado. La configuración de host de pruebas sigue pendiente de diagnóstico; no se modificó machine.config. |
| 2026-11-20 | Corrección de configuración del host Framework | VSTest, Debug, net48/net8.0/net10.0 | Validado para casos seleccionados | MSBuild generaba el archivo dll.config solo con binding redirects y omitía system.data. Se asignó AppConfig del proyecto de pruebas en net48; el archivo generado conserva la fábrica y redirects. Ocho pruebas Oracle aprobadas en net48; selección de configuración y mapeo: 37 aprobadas, 0 fallidas, 0 omitidas en cada runtime. Integraciones y hosts Standard siguen pendientes. |
| 2026-11-20 | Fábricas y consumidor Standard | Debug, net48/net8.0/net10.0 | Bloqueado por defecto en producción | 7 pruebas de fábricas y 29 de configuración fluida aprobadas en cada runtime. Consumidor temporal forzó assemblies netstandard2.0 de Data y Data.SqlServer en los tres hosts; resolución del proveedor Microsoft.Data.SqlClient falla con NullReferenceException en DbProviderFactories.cs:177, al llamar IsNull sobre entrada Odbc no inicializada. Requiere cambio de código de producción y repetir comprobación. No se accedió a bases de datos. |
| 2026-11-20 | Corrección del shim Standard | Debug; net48/net8.0/net10.0 | Validado para el caso aislado | Se omiten entradas de fábrica nulas antes de llamar IsNull. En los tres hosts un consumidor temporal confirmó assemblies Data y Data.SqlServer netstandard2.0, resolvió Microsoft.Data.SqlClient.SqlClientFactory y creó SqlCommand sin conexión. Compilación: 0 errores, 12 advertencias NU1903 de pruebas; integración SQL y demás comportamientos Standard siguen pendientes. |
| 2026-11-20 | Fábrica Oracle con assemblies Standard | Debug; net48/net8.0/net10.0; Oracle.ManagedDataAccess.Core 2.18.151 | Validado para caso aislado | Consumidores temporales verificaron TargetFramework netstandard2.0 de Data y Data.Oracle, resolvieron OracleClientFactory desde el shim y crearon OracleCommand en los tres hosts. No se abrió ninguna conexión; no acredita soporte oficial del proveedor antiguo ni integración con Oracle. |
| 2026-11-20 | Serialización Oracle y diagnóstico de integración | Debug; net48/net8.0/net10.0; SQL Server local | En curso | OracleDatabaseDataExtraInformationFixture.CanDeserializeSerializedConfiguration: 1 aprobada, 0 fallidas, 0 omitidas por runtime. Servicio MSSQLSERVER en ejecución; consulta de solo lectura con sqlcmd falló por certificado TLS no confiable. No se confirmó base aislada ni se ejecutaron pruebas de integración. |
| 2026-11-20 | Preparación SQL local y prueba de lectura | Instancia local predeterminada; Northwind nueva | En curso | Creada Northwind mediante copia temporal del script, sustituyendo la eliminación previa por una protección que falla si la base existe. Verificadas 13 tablas, 7 procedimientos, 91 clientes y 77 productos. ExecuteScalarWithIDbCommand: 1 aprobada/0 fallidas/0 omitidas en net8 y net10. Conexión local de desarrollo con confianza explícita de certificado; cadena confiable, net48, usuario entlib e integración completa pendientes. |
| 2026-11-20 | Usuario SQL de pruebas | Instancia local predeterminada; Northwind | Validado | Ejecutado addentlibuser.sql con autorización explícita. Login entlib autenticó con SQL Server, usuario de Northwind pertenece a db_owner y consulta de Region devolvió 4 filas. Certificado aceptado explícitamente solo en la conexión local; no se guardó contraseña en este registro. |
| 2026-11-20 | SQL con usuario entlib y rollback | Northwind local; net48/net8.0/net10.0 | Validado para casos aislados | Consumidores temporales verificaron usuario entlib y base Northwind, consulta de Region y una inserción revertida con rollback. Pasó con assets nativos y con Data/Data.SqlServer netstandard2.0 forzados en los tres runtimes; Standard verificado mediante TargetFrameworkAttribute. Se aceptó el certificado explícitamente solo en conexión local. Usuario solicitó eliminar Northwind y entlib al cerrar la tarea. Pendientes suite completa, certificados confiables y Oracle. |
| 2026-11-20 | Suites de integración SQL y Oracle | Northwind SQL con entlib; Oracle XEPDB1 NORTHWIND; Debug | En curso | Oracle: net48 102 aprobadas/0 fallidas/3 omitidas; net8/net10 101/0/3 por runtime. Data antes de los últimos ajustes de fixtures: net8/net10 311/0/6. Data net48 tras configurar entlib: 367 aprobadas/14 fallidas/6 omitidas; fallos incluyen EndExecuteReader tras disponer prematuramente comandos de accesores asíncronos, un reader abierto al rollback y captura del tipo SqlException anterior. Se suspenden cambios de producción según instrucción del usuario. Validación total y repetición tras cambios pendientes. |
| 2026-11-20 | Vida útil del comando asíncrono y suites nativas | Debug; Common público; SQL entlib y Oracle XEPDB1 | Validado con omisiones | Accesores conservan el comando hasta terminar la enumeración y conservan el IAsyncResult original; se comprueban callback síncrono y limpieza en error. Data: net48 386 aprobadas/0 fallidas/6 omitidas; net8 y net10 311/0/6 por runtime. Oracle: net48 102/0/3; net8 y net10 101/0/3. Rutas Standard aún no ejecutaron suites completas; Release, empaquetado y avisos siguen pendientes. |
| 2026-11-20 | Suites con assemblies Standard forzados | Copias temporales de pruebas; SDK 10.0.404 | En curso | La compilación inicial de pruebas que usan DbProviderFactories falló con CS0433 por la colisión entre el shim público y el tipo del runtime. No se ejecutaron las suites Standard; se investigó compatibilidad y se comprobó un posible workaround de pruebas con alias. No se autorizó cambiar producción para este problema. |
| 2026-11-20 | Investigación histórica del shim Standard | NuGet público Data 6.3.2; paquete privado 7.0.0-rc2; SDK 10.0.404 | Analizado | Ambos assets Standard exportan públicamente System.Data.Common.DbProviderFactories.GetFactory; assets nativos revisados no lo contienen. Data 6.3.2 con selección normal compila en net48/net8/net10; el mismo asset Standard forzado produce CS0433 en los tres, igual que el actual. Referencia Standard con extern alias compila en los tres. En net8/net10 el registro de fábrica en runtime no aparece en el shim; se deben adaptar las pruebas sin asumir registros compartidos. Mantener shim sin cambios mientras se revisa la compatibilidad. |
| 2026-11-20 | Pruebas SQL existentes con entlib | Northwind local; net48/net8.0/net10.0 | Validado para casos seleccionados | ExecuteScalarWithIDbCommand, ExecuteScalarWithIDbTransaction, ExecuteScalarWithCommandTextAndTypeInTransaction y CanExecuteScalarDoAnInsertion: 4 aprobadas/0 fallidas/0 omitidas por runtime. Configuración temporal del host restaurada al finalizar; conexión cifrada con confianza explícita del certificado local. No se utilizó sa. |

### Fuentes oficiales de la investigación SQL

- [Migración de System.Data.SqlClient a Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/migrate-system-data-sql-client-to-microsoft-data-sql-client?view=sql-server-ver17).
- [SqlClient 6.1: plataformas, dependencias y recurso Standard](https://github.com/dotnet/SqlClient/blob/main/release-notes/6.1/6.1.0.md).
- [Ciclo de soporte del driver y parches vigentes](https://learn.microsoft.com/en-us/sql/connect/ado-net/sqlclient-driver-support-lifecycle).
- [Ciclo de vida de .NET: fechas de .NET 6, 8 y 10](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core).
- [Política de soporte de .NET](https://dotnet.microsoft.com/en-us/platform/support/policy).
- [Ciclo de vida de .NET Framework y dependencia de Windows](https://learn.microsoft.com/lifecycle/faq/dotnet-framework).
- [Requisitos de sistema de .NET Framework](https://learn.microsoft.com/dotnet/framework/get-started/system-requirements).

Revalidar versiones, parches y requisitos al implementar; las páginas pueden actualizarse después de esta investigación.

## Tareas finales antes de integrar a `master`

La integración está diferida hasta que el usuario confirme que ya no tiene reportes de problemas con `10.0.0-rc1`. Las siguientes comprobaciones permanecen pendientes para ese momento; esta decisión no las da por completadas.

- [ ] Validar la publicación de los tres paquetes de producción de Data `10.0.0-rc1` en `nuget.aseinfo.net`. Para Common, comprobar restauración desde NuGet público con la versión seleccionada si esa alternativa fue validada; exigir la publicación de Common `10.0.0-rc1` en el feed privado solo si se decidió usar el fork. Verificar IDs, versiones, contenido y dependencias mediante consumidores limpios; resolver cualquier paquete faltante o incorrecto antes de integrar.
- [ ] Resolver la discrepancia entre el commit declarado en los `.nuspec` y la rama actual, y vincular con evidencia los artefactos al commit que se integrará en `master`. La identidad de Common, los hashes y el inventario disponibles constan en la sección de trazabilidad; no se ha demostrado una relación exacta entre los paquetes y el commit de entrega.
- [ ] Revisar documentación, README, notas de versión, textos de paquetes, ejemplos y demás referencias a frameworks y soluciones. Actualizarlos para comunicar el soporte de `net10.0`, la conservación explícita de `net8.0` y `netstandard2.0` y la excepción Oracle si aplica, el cambio a `net48` y la retirada del soporte explícito de `net47`, `net472` y `net6.0`. Explicar que un consumidor podría resolver el asset `netstandard2.0` sin que ello implique soporte para los frameworks retirados. Verificar que no queden indicaciones vigentes contradictorias y preservar el contexto histórico cuando corresponda.
- [ ] Al finalizar la migración y antes de integrar, preguntar a una persona responsable qué hacer con `source\UPGRADE_NET10.md`: **conservarlo en el repositorio**, **eliminarlo** o **guardarlo en una tarea de ClickUp**. No decidir por ella ni eliminar el archivo antes de recibir su respuesta. Si elige ClickUp, confirmar la tarea de destino, guardar allí el contenido y verificarlo antes de retirar el archivo del repositorio, si así lo indicó.

## Criterios de aceptación y bloqueos

- [ ] `Data.slnx` conserva los seis proyectos y configuraciones requeridas y abre en Visual Studio.
- [x] Aceptar Common 6.3.0 público para este cierre sin exigir validación adicional del modo `Project`; su compilación previa y limitaciones constan en el registro.
- [ ] Common público está validado y es restaurable con ID y versión fijados, o el fork `10.0.0-rc1` está publicado y es restaurable desde el feed privado si fue necesario. Data usa la alternativa elegida en modo paquete y existen pruebas de assets, API y dependencias por target. Publicar el fork no es un requisito si funciona la alternativa pública.
- [ ] Los proyectos de producción compilan y publican los targets acordados, incluida la excepción Oracle documentada si aplica; los dos proyectos de pruebas ejecutan en los tres runtimes y `Data.TestSupport` compila donde se necesita.
- [ ] Existe evidencia de las seis filas de la matriz, incluidas integraciones SQL y Oracle o una limitación del proveedor formalmente aprobada. Registrar conteos y omisiones; no declarar éxito con cero pruebas ni con ejecución parcial presentada como completa.
- [ ] Dependencias y transitivas revisadas por TFM, sin adopción deliberada de paquetes de la línea .NET 11; vulnerabilidades, incompatibilidades y excepciones identificadas y resueltas o aprobadas explícitamente.
- [ ] Los paquetes propios de Data verificables llevan `10.0.0-rc1`; versión, assets y documentación coinciden. Se acepta que los paquetes del feed privado no tengan firma NuGet. No publicar si faltan SDK/runtimes, bases, credenciales seguras, compatibilidad del proveedor o evidencia de ejecución.

## Referencias para validar durante la implementación

- [TFM y compatibilidad de .NET Standard](https://learn.microsoft.com/en-us/dotnet/standard/frameworks); [conversión de soluciones con `dotnet sln`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln).
- [EnterpriseLibrary.Common.NetCore 6.3.0](https://www.nuget.org/packages/EnterpriseLibrary.Common.NetCore/6.3.0); [System.Configuration.ConfigurationManager 10.0.0](https://www.nuget.org/packages/System.Configuration.ConfigurationManager/10.0.0).
- [Microsoft.Data.SqlClient 6.1.0](https://www.nuget.org/packages/Microsoft.Data.SqlClient/6.1.0); [migración desde System.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/migrate-system-data-sql-client-to-microsoft-data-sql-client).
- [Requisitos de Oracle ODP.NET](https://docs.oracle.com/en/database/oracle/oracle-database/26/odpnt/InstallSystemRequirements.html); [Oracle.ManagedDataAccess.Core 2.19.310](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/2.19.310); [Core 23.26.0](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/23.26.0); [ManagedDataAccess 23.26.0](https://www.nuget.org/packages/Oracle.ManagedDataAccess/23.26.0).
- [MSTest y plataformas](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-intro); [Microsoft.NET.Test.Sdk 17.14.1](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.1); [MSTest.TestFramework 4.0.0](https://www.nuget.org/packages/MSTest.TestFramework/4.0.0).
