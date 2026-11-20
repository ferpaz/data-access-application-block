# Plan de migración a .NET 10 — versión 10.0.0-rc1

## Objetivo y alcance

Migrar la solución existente `source\Data.sln` a `source\Data.slnx`, agregar un target nativo `net10.0`, conservar `net8.0` y `netstandard2.0` y sustituir los targets de .NET Framework por `net48`. Si se confirma que Oracle no soporta `netstandard2.0`, aplicar la excepción para `Data.Oracle` indicada en la fase 3. Publicar los paquetes de producción con la versión **10.0.0-rc1**. Este documento es un plan: no significa que la migración o las pruebas ya se hayan ejecutado.

Se eliminan los **targets explícitos** `net47`, `net472` y `net6.0`; se conserva `net8.0`. Como `netstandard2.0` es compatible con varias plataformas, NuGet todavía podría seleccionar ese asset en aplicaciones con frameworks anteriores; eso no equivale a prometer soporte, pruebas ni publicación de assets nativos para ellos. Documentar este cambio y la nueva matriz de soporte.

La unidad de trabajo incluye `Data.sln`, la migración de `Data (With Dependencies).sln` para desarrollo con Common y sus procesos relacionados. Primero se probará el paquete público de Common, que según lo indicado por el usuario ya soporta .NET Standard 2.0. Si supera las validaciones, se conservará esa dependencia sin publicar el fork. Como alternativa, el fork en `E:\vh4Apps\Tools\common-infrastructure`, en su rama actual, ya soporta .NET 10 y .NET Standard 2.0; podrá generarse, validarse y publicarse su paquete `10.0.0-rc1` en [el feed privado](https://nuget.aseinfo.net) si existe una necesidad demostrada. No se prevén cambios de código ni de targets en Common. Los ajustes de referencias y consumo se realizarán en Data; no incorporar automáticamente proyectos que ya no existen.

## Decisiones de targets y proveedor — 20 de noviembre de 2026

Por instrucción del usuario, la matriz acordada es **`net48;netstandard2.0;net8.0;net10.0`**. Se retiran `net47`, `net472` y `net6.0`. La excepción Oracle para Standard sigue sujeta a evidencia según la fase 3. Las suites ejecutables usarán `net48;net8.0;net10.0`; Standard se validará forzando su artefacto en esos tres hosts.

Se integra aquí la propuesta de migrar SQL Server a `Microsoft.Data.SqlClient` en los cuatro destinos de Data y Data.SqlServer. Implementación y pruebas pendientes: las casillas no acreditan trabajo ejecutado. Este documento no concede por sí mismo nuevas autorizaciones de publicación, integración o cambios en otros repositorios.

- .NET 8 se conserva explícitamente por compatibilidad, aunque Microsoft terminó su soporte el 10 de noviembre de 2026. Documentar esta limitación y probarlo igualmente: soporte del paquete y mantenimiento del runtime por Microsoft son compromisos distintos.
- .NET 6 terminó su soporte el 12 de noviembre de 2024. .NET 10 es LTS hasta el 14 de noviembre de 2028.
- Framework 4.8 concentra mantenimiento y pruebas; no es un mínimo del driver 6.1, que admite Framework 4.6.2+. El soporte de Framework depende del Windows instalado.
- Conservar Standard permite que NuGet lo seleccione en Framework anteriores a 4.8; eso no implica prometer soporte para esos consumidores.
- SqlClient 6.1 es LTS hasta el 14 de agosto de 2028; el parche identificado es 6.1.8. Las líneas 5.1 y 5.2 actuales están fuera de soporte. Revalidar parches, avisos y requisitos al implementar.

## Flujo de ramas e integración

- [ ] Realizar los cambios de Data en la rama `_net_10_migration`. La rama ya existe y, al preparar este plan, parte del mismo commit que `_net_8_migration`; `master` está en otro commit. Revisar la diferencia completa respecto de `master`, incluidos cambios previos a esta migración, para evitar integrar modificaciones no deseadas.
- [ ] Completar en la rama todas las actividades, pruebas y criterios de aceptación de este documento, incluida la validación de Common público o, si resulta necesario usar el fork, su publicación verificable y la restauración de Data con la dependencia seleccionada. Registrar resultados y resolver errores y conflictos antes de integrar.
- [ ] Una vez que todo esté verificado, integrar `_net_10_migration` en `master` del repositorio mantenido aquí. Como el proyecto original está abandonado y sin soporte, no se depende de una integración en ese proyecto original. Respetar las políticas y permisos del repositorio de destino; revisar el resultado y ejecutar nuevamente la compilación y las pruebas pertinentes en el commit integrado.

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

## Fase 1 — Línea base y herramientas

- [ ] Registrar SDK, Visual Studio, MSBuild, NuGet, runtimes y targeting pack de .NET Framework instalados, sistema operativo, arquitectura y resultados iniciales. En esta revisión solo se detectaron los SDK 8.0.428, 9.0.321 y 11.0.100: falta instalar un SDK 10 compatible antes de ejecutar el plan. Instalar también los runtimes .NET 8 y .NET 10 y el developer/targeting pack de .NET Framework 4.8 en Windows.
- [ ] Fijar mediante `global.json` una versión **10.0.x** concreta y una política de roll-forward que no seleccione el SDK 11 accidentalmente. Elegir un Visual Studio/MSBuild que soporte tanto .NET 10 como `.slnx` y verificar su soporte en la imagen de CI.
- [ ] Antes de cambiar targets, recopilar compilación, descubrimiento y resultados de pruebas de la solución actual por target y configuración, incluyendo pruebas omitidas y fallidas. Distinguir fallos preexistentes de regresiones. No se obtuvo una lista de pruebas desde Test Explorer durante esta revisión; volver a descubrirlas tras compilar.
- [ ] Inventariar referencias directas y transitivas por target, versiones efectivamente resueltas y dependencias de ambos modos `EntLibDependencyType=Package` y `Project`. La restauración del modo paquete es la línea principal actual; Common está disponible localmente para validar el modo proyecto.
- [ ] Registrar el estado de `BVT\Data.BVT`: está fuera de `Data.sln`, apunta a `net47` y referencia proyectos ausentes. Decidir su migración o retiro por separado, sin usarlo como evidencia de la matriz principal.

### Common y solución con dependencias

El repositorio Common está en `E:\vh4Apps\Tools\common-infrastructure`; su proyecto es `E:\vh4Apps\Tools\common-infrastructure\source\Src\Common\Common.csproj`. En esta disposición, desde `source\Data (With Dependencies).sln` la ruta relativa es `..\..\common-infrastructure\source\Src\Common\Common.csproj`; desde los cuatro `.csproj` que definen `EntLibCommon` es `..\..\..\..\common-infrastructure\source\Src\Common\Common.csproj`. Las rutas actuales apuntan a un directorio hermano llamado `Common`, que no existe en este entorno.

Los proyectos Common referidos por `Data (With Dependencies).sln` pertenecen al fork en `E:\vh4Apps\Tools\common-infrastructure`. Según confirmación del usuario, la rama actualmente seleccionada de ese fork ya soporta `net10.0` y `netstandard2.0`. No se planifica modificar su código, migrar sus proyectos ni cambiar sus targets o suites: si se requiere el fork, el trabajo previsto es generar y validar el paquete existente y publicarlo en el feed privado [https://nuget.aseinfo.net](https://nuget.aseinfo.net). Registrar el nombre de rama y commit usados al generar el paquete. Corregir las rutas de las referencias desde este repositorio Data no implica modificar Common. Si las validaciones revelan una incompatibilidad concreta, registrar la evidencia y reevaluar únicamente la corrección necesaria.

#### Alternativa permanente: probar Common público antes de publicar el fork

Según lo indicado por el usuario, Common disponible en NuGet público ya soporta `netstandard2.0`. Esa compatibilidad permite evaluar su consumo desde los destinos de Data; no demuestra por sí sola que todas las API y dependencias funcionen. **Siempre se mantiene la posibilidad de usar el paquete público y evitar la publicación de la versión del fork.** El hecho de que Data tenga versión `10.0.0-rc1` no obliga a que Common comparta ese número de versión ni tenga un asset nativo `net10.0`.

- [ ] Identificar el ID y versión exactos del paquete público candidato; comenzar con la dependencia actual `EnterpriseLibrary.Common.NetCore` 6.3.0 si corresponde. Verificar contenido, soporte Standard, dependencias y avisos; no dar por ejecutada esta comprobación.
- [ ] Probar en modo `EntLibDependencyType=Package`, resolviendo Common desde NuGet público, sin que un `ProjectReference` al fork o la caché de un paquete privado enmascare el resultado. Registrar origen, versión, hash y assets efectivamente cargados.
- [ ] Validar restauración, compilación y pruebas funcionales/integración de Data en `net48`, `net8.0` y `net10.0`, además de los hosts que fuerzan el artefacto `netstandard2.0`. Comprobar configuración, recursos, firma, compatibilidad de API, dependencias y binding redirects donde corresponda.
- [ ] Probar el NuGet resultante de Data en consumidores limpios usando Common público, con dependencias resueltas desde sus orígenes configurados.
- [ ] Si funciona, registrar la elección del paquete público y marcar las actividades exclusivas del fork como **no aplicables**, con referencia a los resultados. No publicar el fork ni cambiar la versión de Common para igualarla artificialmente a Data.
- [ ] Si falla por una incompatibilidad o una función ausente de Common necesaria para Data, registrar el caso reproducible y el motivo para elegir el fork; no atribuir al paquete un fallo de infraestructura ajeno. Validar el paquete del fork localmente antes de publicarlo.

**Evidencia de decisión:** candidato público, resultados por target, origen y assets cargados, limitaciones y elección final. La prueba está pendiente; documentar esta alternativa no declara que ya haya pasado.

#### Alternativa condicionada: empaquetar y publicar el fork existente

- [ ] **Solo si se requiere el fork:** usar la rama actual del fork Common sin planificar cambios de código o targets. Registrar rama, commit y dependencias resueltas; verificar que los assets `net10.0` y `netstandard2.0` estén presentes. Generar el paquete con versión `10.0.0-rc1` mediante las opciones de versión del proceso de empaquetado existente, sin exigir editar `version.props` ni otros archivos de Common. Verificar que el proceso no altere inesperadamente la versión; registrar cualquier bloqueo real antes de proponer cambios en ese repositorio.
- [ ] **Solo si se requiere el fork:** compilar, ejecutar las pruebas disponibles y generar el paquete Release de Common. Verificar el ID `EnterpriseLibrary.Common.NetCore`, la versión exacta `10.0.0-rc1`, firma, metadatos, assets y grupos de dependencias. Comprobar la selección de assets en consumidores `net48`, `netstandard2.0`, `net8.0` y `net10.0`, sin asumir que Common tiene un asset nativo net8: `net48` puede seleccionar el asset compatible `net472`, pero eso no crea un asset nativo `net48`. Ejecutar las pruebas aplicables de integración desde Data y documentar las limitaciones de las suites actuales de Common, sin requerir una nueva migración de Common salvo que aparezca una incompatibilidad real.
- [ ] **Solo si se requiere el fork:** publicar el paquete verificado `EnterpriseLibrary.Common.NetCore` **10.0.0-rc1** en el feed privado [https://nuget.aseinfo.net](https://nuget.aseinfo.net), utilizando el endpoint y la configuración de autenticación autorizados para ese feed; no asumir la URL completa del endpoint ni guardar credenciales en el repositorio. Antes de publicar, comprobar si esa versión ya existe y, si existe, verificar su identidad e integridad en lugar de sobrescribirla. Confirmar que el feed permite restaurar esa versión prerelease y que CI tiene acceso a ella.
- [ ] Documentar la configuración del origen NuGet del feed privado [https://nuget.aseinfo.net](https://nuget.aseinfo.net) para desarrollo y CI: endpoint autorizado, autenticación mediante el mecanismo seguro disponible y selección de origen o mapeo de paquetes para resolver Common y Data desde el feed previsto, sin incorporar credenciales al repositorio. Verificar la restauración desde un entorno limpio y registrar la configuración necesaria sin secretos.
- [ ] Configurar `EntLibCommonVersion` de Data con la versión seleccionada y validada: conservar `6.3.0` si es el paquete público probado y suficiente, o usar la versión pública concreta elegida; cambiar a `10.0.0-rc1` únicamente si se requiere el fork y después de confirmar su publicación. No usar alternativas silenciosas. Verificar las referencias de los cuatro proyectos, incluido el ID explícito de `Oracle.Tests.VSTS`, y restaurar en modo `Package` desde el origen correspondiente. Inspeccionar `project.assets.json` y los `.nuspec` para acreditar ID, versión, dependencias y assets seleccionados por TFM. Mantener el modo `Project` para desarrollo con la solución secundaria; sus resultados no sustituyen la prueba del paquete público.
- [ ] Reconciliar `source\Data (With Dependencies).sln` con los seis proyectos vigentes de `Data.sln` más `Common.csproj`: actualmente faltan `Data.SqlServer` y `Data.Oracle`, y sobran las referencias a `Src\SqlCe\Data.SqlCe.csproj` y `Tests\SqlCe.Tests\Data.SqlCe.Tests.VSTS.csproj`, que no existen. Corregir también la ruta de Common y las configuraciones y carpetas de solución sin reintroducir SQL CE.
- [ ] Ajustar la ruta `EntLibCommon` en `Src\Data\Data.csproj`, `Tests\Data.Tests\Data.Tests.VSTS.csproj`, `Tests\Data.TestSupport\Data.TestSupport.csproj` y `Tests\Oracle.Tests.VSTS\Oracle.Tests.VSTS.csproj`, preferiblemente mediante una propiedad compartida configurable, con una ruta relativa predeterminada documentada para repositorios hermanos. Mantener el modo paquete para instalaciones sin Common y evitar depender de la ruta absoluta local en CI.
- [ ] Revisar las condiciones de referencia: actualmente, si `EntLibDependencyType=Project` y la ruta no existe, se selecciona silenciosamente el paquete. Hacer que el modo `Project` falle con un mensaje claro si falta Common, y que seleccione únicamente `ProjectReference` cuando exista; el modo `Package` debe seleccionar únicamente `PackageReference`. Probar ambos modos y valores inválidos, y actualizar el comentario de `package.props`.
- [ ] Una vez reconciliada, convertir la solución secundaria con `dotnet sln "source\Data (With Dependencies).sln" migrate` y verificar `source\Data (With Dependencies).slnx`: siete proyectos, carpetas, configuraciones Debug/Release y referencias correctas. Comprobar la resolución real de `ProjectReference` con logs o grafo de MSBuild para los cuatro TFMs, no solo la presencia del proyecto en la solución. Abrirla en Visual Studio, restaurar, compilar y ejecutar las suites Data en los tres runtimes con el modo proyecto; probar por separado el modo paquete desde la solución principal. Registrar la revisión de Common utilizada y habilitar el checkout de ambos repositorios en CI.

## Fase 2 — Solución y compilación

- [ ] Con SDK 10 y desde la raíz del repositorio, ejecutar `dotnet sln source\Data.sln migrate` para generar `source\Data.slnx`. Comparar `dotnet sln source\Data.sln list` y `dotnet sln source\Data.slnx list`: deben contener exactamente los seis proyectos de la tabla. Comparar carpetas, elementos de solución, Debug/Release y Any CPU, dependencias y comportamiento de compilación; revisar cualquier sección heredada que no sobreviva a la conversión.
- [ ] Abrir `Data.slnx` en Visual Studio, restaurar, compilar y descubrir pruebas. Actualizar CI, scripts y documentación que refieran a `source\Data.sln`; retirar los `.sln` migrados solo después de verificar sus respectivas conversiones para evitar fuentes de verdad duplicadas. La solución secundaria se conserva para desarrollo con Common según las actividades anteriores.
- [ ] Actualizar los seis `.csproj` según la matriz. Retirar las condiciones de `net6`, conservar y revisar las de `net8`, agregar condiciones para `net10.0` y mantener las de `net4` y `netstandard2` donde correspondan. Evaluar las condiciones inactivas de pruebas en lugar de simplemente copiarlas.
- [ ] Revisar `#if NET6_0_OR_GREATER`, `NET8_0_OR_GREATER`, `NETSTANDARD2_0`, compilación condicional y exclusiones de archivos. Los símbolos `_OR_GREATER` también cubren .NET 10: no reemplazarlos mecánicamente si aún expresan el comportamiento correcto. En particular, revisar `AsynchronousExecutionFixture.cs`, excluido actualmente fuera de `net4`, y la rama `!NET8_0_OR_GREATER` en `OracleTransactionScopeFixture.cs`; registrar qué pruebas se ejecutan por target.
- [ ] Conservar y probar el shim `Src\Data\System.Data.Common` solo para `netstandard2.0`; el proyecto lo excluye en los demás targets. Revisar carga de recursos, firma con `EnterpriseLibrary.snk`, documentación XML, configuración, `DbProviderFactories` y `OutputPath` compartido en `package.props` para prevenir colisiones entre targets y entre compilaciones paralelas.
- [ ] Migrar el proveedor SQL a `Microsoft.Data.SqlClient` en todos los destinos acordados: `net48`, `netstandard2.0`, `net8.0` y `net10.0`. Revisar `DbProviderMapping`, las fábricas, los nombres invariantes, pruebas y configuración. Cambiar de proveedor puede modificar tipos públicos, cifrado, autenticación y compatibilidad binaria; tratarlo como cambio incompatible deliberado dentro de esta migración, conforme al plan SQL integrado en este archivo.

## Fase 3 — Análisis y actualización de NuGet

No elegir simplemente la versión más reciente. Usar versiones estables, mantenidas y compatibles con cada TFM y runtime, sin introducir deliberadamente paquetes de la línea .NET 11. En paquetes Microsoft `System.*` y `Microsoft.Extensions.*`, partir de la línea 10.0.x o de una anterior compatible; el número mayor de Oracle, SqlClient o herramientas de prueba **no indica por sí solo** que pertenezcan a .NET 11. Revisar también sus dependencias transitivas y assets reales. Las versiones siguientes son puntos de partida, no una aprobación sin restauración y pruebas.

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

- [ ] Inspeccionar todas las referencias reales y sus ramas, incluidas las versiones centralizadas y las fijas en cada proyecto; eliminar propiedades obsoletas solo después de comprobar usos. No forzar una única versión cuando los assets no sean compatibles con todos los targets.
- [ ] Para cada candidato consultar los metadatos NuGet, plataformas oficialmente soportadas, licencias, cambios incompatibles y avisos de seguridad. Ejecutar `dotnet list source\Data.slnx package --include-transitive`, `--outdated` y `--vulnerable --include-transitive` con el SDK seleccionado; revisar resultados por TFM y corregir incompatibilidades, downgrades y vulnerabilidades, sin ocultar advertencias. Registrar las versiones elegidas y la justificación, incluidas excepciones aprobadas.
- [ ] Validar que la selección transitiva no introduce dependencias de la línea .NET 11, que los grupos de dependencias del `.nuspec` sean correctos y que los consumidores no requieran versiones incompatibles entre sí. Resolver explícitamente cualquier conflicto, no usar supresiones como sustituto de la prueba.
- [ ] Verificar con documentación de Oracle y pruebas qué versión del proveedor soporta `netstandard2.0` y qué runtimes admite. La ausencia de ese asset en una versión concreta no demuestra que ninguna versión lo soporte. Si se confirma que Oracle no soporta `netstandard2.0`, documentar la evidencia y eliminar **solo de `Data.Oracle`** ese target; conservar `netstandard2.0` en `Data` y `Data.SqlServer`. Actualizar el paquete, sus consumidores, la matriz de pruebas y la documentación para reflejar esa excepción. Una limitación en un host específico, por sí sola, debe documentarse y evaluarse sin inferir automáticamente la ausencia de soporte del TFM completo.

## Fase 4 — Versión, empaquetado y CI

- [ ] Cambiar `version.props` a `MajorVersion=10`, `MinorVersion=0`, `ReleaseVersion=0`, `Revision=0` y `SemanticVersionSuffix=-rc1`. Verificar `PackageVersion=10.0.0-rc1`, `AssemblyVersion=10.0.0.0`, `FileVersion=10.0.0.0`, información de versión, nombres de paquetes, firma y símbolos para los tres proyectos publicables.
- [ ] Corregir `appveyor.yml`: hoy usa Visual Studio 2019, restaura/compila `Data.sln` y **suma 1 a `ReleaseVersion`**, lo que impediría producir exactamente `10.0.0-rc1`. Usar una imagen compatible con SDK 10 y `.slnx`, generar exactamente la versión deseada sin reescribirla inesperadamente y revisar el commit automático de `version.props` y la selección de artefactos.
- [ ] Actualizar `PackageReleaseNotes` en `package.props`, `README.md`, `docs\Pages\Upgrade.md` y `docs\Pages\ReleaseNotes.md`: targets y soporte, cambios incompatibles de framework y proveedor, requisitos de despliegue, versión mayor, paquetes y pasos de migración. Revisar cualquier otra referencia a la solución antigua o a los targets retirados.
- [ ] Preparar SQL Server con Northwind y sus objetos, y Oracle con sus tablas, paquetes y procedimientos. Revisar `scripts\SqlServer`, `scripts\Oracle` y los scripts de preparación existentes; `appveyor.yml` llama a `scripts\CREATE_Northwind.bat` y `scripts\DROP_Northwind.bat`, rutas que no aparecen en el inventario actual. Reparar la preparación y limpieza o reemplazarlas por pasos verificables. Usar instancias aisladas, credenciales secretas y limpieza aun ante fallos; no incluir credenciales en artefactos.
- [ ] Configurar trabajos separados para pruebas rápidas y de integración en Windows x64, con `.runsettings` y bases disponibles. Una integración omitida por infraestructura ausente no cuenta como aprobada. Publicar logs, resultados TRX y paquetes; fallar CI ante errores, cero pruebas inesperadas o resultados faltantes.

## Fase 5 — Pruebas obligatorias

### Matriz de ejecución

| Host de pruebas | Assets de producción que deben cargarse | Evidencia requerida |
|---|---|---|
| Windows x64, .NET Framework 4.8 | `net48` de `Data`, `Data.SqlServer` y `Data.Oracle` | Ambas suites descubiertas y ejecutadas; pruebas unitarias e integración SQL/Oracle. |
| Windows x64, .NET 8 | `net8.0` de los tres proyectos | Ambas suites e integración SQL/Oracle; runtime conservado por compatibilidad. |
| Windows x64, .NET 10 | `net10.0` de los tres proyectos | Las mismas categorías aplicables, incluyendo integración SQL/Oracle. |
| Windows x64, .NET Framework 4.8 | `netstandard2.0` de los tres proyectos | Consumidores/hosts de compatibilidad que fuerzan esos assets y ejecutan pruebas funcionales y de integración aplicables. |
| Windows x64, .NET 8 | `netstandard2.0` de los tres proyectos | Forzar los assets Standard y verificar implementación ejecutable del driver, configuración e integración. |
| Windows x64, .NET 10 | `netstandard2.0` de los tres proyectos | Igual, verificando especialmente shim de proveedor y dependencias Oracle/SQL. |

Si se confirma la excepción Oracle de la fase 3 y se elimina su target `netstandard2.0`, las tres filas standard aplican solo a `Data` y `Data.SqlServer`; documentar y probar el comportamiento de los consumidores que intenten usar Oracle con ese TFM, sin afirmar soporte para él. Las filas `net48`, `net8.0` y `net10.0` siguen incluyendo los tres proyectos.

- [ ] Implementar hosts o una variante de pruebas de compatibilidad que referencie **explícitamente** los assemblies `netstandard2.0` de los proyectos que conserven ese target y sus dependencias transitivas correctas, evitando que `ProjectReference` o NuGet seleccione automáticamente `net48`/`net8.0`/`net10.0`. Por ejemplo, usar un feed local temporal con paquetes de prueba que contengan únicamente los assets standard, manteniendo las dependencias por TFM; no publicar esos paquetes como release. Registrar `project.assets.json`, rutas de carga y/o hashes de assemblies y comprobar el TFM del binario cargado. No mezclar assets nativos y standard en la misma ejecución por accidente.
- [ ] Reutilizar tantas pruebas de ambas suites como sea viable en esos hosts; adaptar fixtures dependientes del proveedor y del runtime. Si una prueba no puede compartirse, crear una equivalente que verifique el mismo comportamiento. Registrar exclusiones justificadas y cobertura por comportamiento, no contar una compilación como ejecución.
- [ ] Ejecutar por target y configuración Debug/Release: restauración limpia, compilación sin errores ni nuevos warnings no justificados, descubrimiento y ejecución. Para los dos proyectos de pruebas, por ejemplo, desde la raíz con SDK 10:

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

Estos seis comandos prueban los assets nativos seleccionados por defecto; **no sustituyen** las tres ejecuciones adicionales forzando los assets standard. Usar nombres de TRX y directorios de resultados distintos para evitar sobrescrituras; ejecutar también la variante Debug y los hosts de compatibilidad definidos durante la implementación.

### Casos funcionales y de regresión

- [ ] Descubrimiento en CLI y Visual Studio, cantidad de pruebas por suite/target y comparación con la línea base; revisar `[Ignore]`, pruebas excluidas por compilación condicional y atributos/lifecycle afectados por MSTest. Ninguna omisión nueva sin justificación y aprobación.
- [ ] Configuración XML y JSON, cadenas de conexión, registro y resolución de proveedores, fábricas, configuración fluida, errores de proveedor inexistente y carga de recursos. Comprobar las rutas distintas para `System.Data.SqlClient`, `Microsoft.Data.SqlClient` y Oracle.
- [ ] SQL Server: conexión, consultas parametrizadas y procedimientos, parámetros de entrada/salida, lectores, `DataSet`, mapeo, transacciones commit/rollback, concurrencia, errores y liberación de recursos. Verificar APM donde sigue soportado y comportamiento asíncrono disponible en .NET 8 y .NET 10; comprobar TLS/certificados y autenticación con la versión elegida de SqlClient.
- [ ] Oracle: conexión, procedimientos/paquetes, cursores y parámetros, tipos, lectura/escritura, errores, transacciones commit/rollback y comportamiento de `TransactionScope` según plataforma y proveedor. Revisar las pruebas ignoradas o excluidas existentes y las diferencias soportadas, sin habilitarlas a ciegas.
- [ ] En `net48`, comprobar binding redirects, configuración de aplicación y carga de dependencias en un consumidor limpio; en `net8.0` y `net10.0`, publicación y carga con assets nativos y resolución de configuración. Para standard, repetir en los tres runtimes con los binarios forzados. Registrar explícitamente cualquier combinación Oracle que el fabricante no soporte.
- [ ] Empaquetar en Release y examinar cada `.nupkg`/`.snupkg`: ID, versión exacta, firma, símbolos, metadatos, grupos de dependencias y `lib\net48`, `lib\netstandard2.0`, `lib\net8.0`, `lib\net10.0`, con la excepción Oracle documentada si aplica; no deben quedar carpetas `lib` para los targets retirados. Probar restauración y ejecución desde un feed local en consumidores limpios `net48`, `net8.0` y `net10.0`, comprobando también selección normal de assets y compatibilidad de API con la versión anterior.
- [ ] Comprobar compilación y restauración en CI, preparación/limpieza de bases, rutas SLNX, publicación de artefactos y que el paquete generado en CI sea exactamente `10.0.0-rc1`. Verificar ambos modos de Common, con su proyecto local en la solución secundaria y con el paquete en la principal.

## Plan integrado de migración SQL Server

Esta sección complementa las fases generales y reemplaza la propuesta anterior de conservar `System.Data.SqlClient` en Framework y Standard. No cambia el proveedor Oracle.

El recurso Standard de SqlClient 6.1 es de referencia: no contiene una implementación ejecutable. La aplicación debe seleccionar una implementación compatible del driver. Probar el artefacto Standard de esta biblioteca en los tres hosts y evitar desplegar por copia manual la DLL de referencia del driver.

Los tipos de ambos proveedores son distintos. Revisar comandos, parámetros, lectores, transacciones, builders y excepciones en API y consumidores. En particular, `WithConnectionString(SqlConnectionStringBuilder)` cambia su firma concreta y `DefaultSqlProviderName` es una constante pública cuyo valor puede estar incorporado en consumidores ya compilados. Documentar recompilación y cambios necesarios.

Revisar cifrado y certificados: no usar `Encrypt=false` ni `TrustServerCertificate=true` como solución general. Validar certificados confiables, autenticación real, SNI, arquitecturas y binding redirects de Framework.

### Inventario SQL y puntos de cambio

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
- [ ] Definir el tratamiento de configuración antigua: migración explícita al nuevo nombre de proveedor, diagnóstico claro o compatibilidad deliberada y probada. No redirigir nombres silenciosamente.
- [ ] Registrar versión mayor, prerelease, responsables y límites del alcance.

**Evidencia:** tabla de consumidores, matriz acordada y registro de decisiones al final del documento.

#### 2. Obtener línea base y preparar herramientas

- [ ] Registrar commit, rama, estado de trabajo, SDK, runtime, MSBuild y targeting pack de Framework 4.8. No sobrescribir cambios ajenos.
- [ ] Verificar SDK 10 y herramientas de desarrollo/CI compatibles; fijar SDK conforme al plan general.
- [ ] Registrar compilación, descubrimiento, pruebas ejecutadas/omitidas y fallos preexistentes por target. Distinguir regresiones de problemas anteriores.
- [ ] Inventariar dependencias efectivamente resueltas y comprobar Common en modo paquete y proyecto, si ambos permanecen dentro del alcance.
- [ ] Preparar SQL Server y datos aislados de integración; definir certificados y métodos de autenticación que deben probarse.

**Evidencia:** logs de línea base, entorno, conteos de pruebas y dependencias resueltas.

#### 3. Demostrar compatibilidad del candidato

- [ ] Verificar documentación, metadatos NuGet, cambios incompatibles y avisos del parche de SqlClient elegido.
- [ ] Probar restauración y compilación con el driver para cada destino aprobado. Inspeccionar assets y dependencias reales; compilar no acredita ejecución.
- [ ] Revisar el contrato público y binario antes/después, incluidos `WithConnectionString`, constantes de proveedor, tipos concretos y ensamblados firmados.
- [ ] Confirmar en consumidores mínimos `net48`, `net8.0` y `net10.0` conexión, creación de comandos y ejecución de una consulta.
- [ ] Registrar cualquier API no disponible o comportamiento diferente antes de ampliar la migración.

**Evidencia:** versión exacta elegida, dependencias por TFM, diferencia de API y resultados de los consumidores mínimos.

#### 4. Implementar destinos, proveedor y configuración

- [ ] Actualizar targets de proyectos de producción, pruebas y soporte según la matriz acordada; comprobar consumidores internos y referencias compartidas.
- [ ] Alinear referencias de SqlClient en Data, Data.SqlServer y pruebas; evitar versiones fijas divergentes y retirar propiedades sin usos.
- [ ] Revisar todos los condicionales del proveedor y archivos parciales. Los símbolos `_OR_GREATER` incluyen .NET 10: no sustituirlos mecánicamente.
- [ ] Actualizar fábricas, mapeos, configuración fluida, archivos XML/JSON, alias y ejemplos a la estrategia aprobada.
- [ ] Conservar y migrar el shim de Standard, probando su resolución del nuevo proveedor y la interacción con las fábricas de los hosts.
- [ ] Revisar casts, parámetros, excepciones, autenticación y métodos APM/TAP. Preservar comportamiento público salvo cambios deliberados y documentados.
- [ ] Retirar referencias innecesarias a `System.Data.SqlClient`; distinguir referencias de producción vigentes de documentación histórica o casos deliberados de compatibilidad.

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
- [ ] Probar TLS con certificado confiable y errores de certificado esperados; autenticación integrada/SQL y Entra ID si forma parte de los consumidores comprometidos.
- [ ] Validar x86/x64 si ambas arquitecturas están comprometidas; registrar otras arquitecturas y SO como probados o fuera del alcance.
- [ ] Ejecutar regresiones pertinentes de Data y Oracle ante cambios compartidos; no ampliar innecesariamente esta migración a una reescritura del proveedor Oracle.
- [ ] Comparar conteos con la línea base y registrar exclusiones. Pruebas omitidas por falta de base, credenciales o infraestructura no cuentan como aprobadas.

**Evidencia:** resultados por host/artefacto, logs o TRX sin secretos, conteos, omisiones y limitaciones.

#### 6. Empaquetar y preparar entrega

- [ ] Generar paquetes Release y examinar IDs, versión, firma, símbolos, dependencias y carpetas de cada TFM; no incluir targets retirados.
- [ ] Instalar los NuGet generados desde un feed local en consumidores limpios, sin referencias al código fuente. Probar restauración, publicación y ejecución.
- [ ] Forzar y verificar el artefacto Standard en hosts de compatibilidad; la selección normal de NuGet podría elegir `net48` o `net10.0` y dejarlo sin probar.
- [ ] Actualizar CI y documentación: README, notas de paquete, guía de actualización y las secciones de este plan. Mantener registros históricos claramente identificados.
- [ ] Documentar pasos del consumidor: retarget a Framework 4.8/.NET 10, cambio de namespaces y tipos, `providerName`, fábrica, certificados, dependencias y recompilación.
- [ ] Registrar artefactos y commits verificados; mantener la versión anterior disponible para consumidores que aún no migren.
- [ ] Preparar la entrega para revisión. Publicación e integración seguirán la autorización aplicable y el alcance aprobado, sin inferirla de esta lista.

### Criterios de aceptación SQL

- [ ] Matriz acordada y decisiones de implementación coherentes en este documento.
- [ ] Todos los destinos aprobados compilan y sus dependencias son compatibles con el runtime comprometido.
- [ ] Se utiliza `Microsoft.Data.SqlClient` en las rutas SQL acordadas, sin selección accidental del proveedor anterior.
- [ ] Cambios de API, tipos, constantes y configuración identificados y documentados como incompatibles cuando corresponda.
- [ ] Pruebas funcionales, integración y consumidores del NuGet ejecutados con evidencia; ninguna combinación pendiente se presenta como verificada.
- [ ] Certificados, autenticación, SNI y carga de dependencias funcionan en los entornos comprometidos.
- [ ] Paquetes, documentación y CI reflejan la misma versión y matriz de soporte.

### Registro de decisiones

| Fecha | Decisión | Responsable | Motivo/evidencia |
|---|---|---|---|
| 2026-11-20 | Targets acordados: net48, netstandard2.0, net8.0 y net10.0. Plan SQL integrado; implementación pendiente. | Usuario | Instrucción explícita de integrar el plan y conservar los cuatro destinos. |
| 2026-11-20 | Common ya soporta net10.0 y netstandard2.0 en la rama actual del fork local; alternativa disponible: empaquetado, validación y publicación en el feed privado solo si se necesita frente al paquete público. | Usuario | Ruta: `E:\vh4Apps\Tools\common-infrastructure`. Feed: https://nuget.aseinfo.net. |
| 2026-11-20 | Probar primero Common público compatible con Standard; conservarlo y omitir la publicación del fork si las validaciones pasan. Mantener el fork como alternativa condicionada. | Usuario | Pruebas pendientes; fijar paquete y versión a partir de evidencia. |

### Registro de ejecución

Agregar una fila por actividad relevante; usar estados pendiente, en curso, validado o bloqueado. Marcar casillas solo con evidencia y anotar el motivo de cualquier actividad no aplicable.

| Fecha | Actividad | Commit/entorno | Resultado | Evidencia o bloqueo |
|---|---|---|---|---|
| — | Implementación y pruebas | — | Pendiente | Este documento no acredita cambios ni pruebas de la migración. |

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

- [ ] Validar la publicación de los tres paquetes de producción de Data `10.0.0-rc1` en `nuget.aseinfo.net`. Para Common, comprobar restauración desde NuGet público con la versión seleccionada si esa alternativa fue validada; exigir la publicación de Common `10.0.0-rc1` en el feed privado solo si se decidió usar el fork. Verificar IDs, versiones, contenido y dependencias mediante consumidores limpios; resolver cualquier paquete faltante o incorrecto antes de integrar.
- [ ] Registrar el commit de Data, la identidad del Common público utilizado (ID, versión, origen y hash del paquete) o, si se usa el fork, su rama y commit exactos. Registrar dependencias resueltas y hashes de artefactos publicados; comparar los paquetes descargados con los validados y vincular los artefactos propios al commit que se integrará en `master`, sin credenciales.
- [ ] Revisar documentación, README, notas de versión, textos de paquetes, ejemplos y demás referencias a frameworks y soluciones. Actualizarlos para comunicar el soporte de `net10.0`, la conservación explícita de `net8.0` y `netstandard2.0` y la excepción Oracle si aplica, el cambio a `net48` y la retirada del soporte explícito de `net47`, `net472` y `net6.0`. Explicar que un consumidor podría resolver el asset `netstandard2.0` sin que ello implique soporte para los frameworks retirados. Verificar que no queden indicaciones vigentes contradictorias y preservar el contexto histórico cuando corresponda.
- [ ] Al finalizar la migración y antes de integrar, preguntar a una persona responsable qué hacer con `source\UPGRADE_NET10.md`: **conservarlo en el repositorio**, **eliminarlo** o **guardarlo en una tarea de ClickUp**. No decidir por ella ni eliminar el archivo antes de recibir su respuesta. Si elige ClickUp, confirmar la tarea de destino, guardar allí el contenido y verificarlo antes de retirar el archivo del repositorio, si así lo indicó.

## Criterios de aceptación y bloqueos

- [ ] `Data.slnx` conserva los seis proyectos y configuraciones requeridas, abre en Visual Studio y es la solución principal usada por CI.
- [ ] `Data (With Dependencies).slnx` contiene esos seis proyectos y Common, sin referencias SQL CE ausentes, y restaura, compila y ejecuta las pruebas Data con `EntLibDependencyType=Project`; se verifica que realmente se usa el proyecto local y que una ruta inexistente produce un error explícito.
- [ ] Common público está validado y es restaurable con ID y versión fijados, o el fork `10.0.0-rc1` está publicado y es restaurable desde el feed privado si fue necesario. Data usa la alternativa elegida en modo paquete y existen pruebas de assets, API y dependencias por target. Publicar el fork no es un requisito si funciona la alternativa pública.
- [ ] Los proyectos de producción compilan y publican los targets acordados, incluida la excepción Oracle documentada si aplica; los dos proyectos de pruebas ejecutan en los tres runtimes y `Data.TestSupport` compila donde se necesita.
- [ ] Existe evidencia de las seis filas de la matriz, incluidas integraciones SQL y Oracle o una limitación del proveedor formalmente aprobada. Comparar conteos y omisiones con la línea base; no declarar éxito con cero pruebas ni con ejecución parcial presentada como completa.
- [ ] Dependencias y transitivas revisadas por TFM, sin adopción deliberada de paquetes de la línea .NET 11; vulnerabilidades, incompatibilidades y excepciones identificadas y resueltas o aprobadas explícitamente.
- [ ] Los paquetes propios de Data verificables llevan `10.0.0-rc1`; versión, firma, assets, documentación y CI coinciden. No publicar si faltan SDK/runtimes, bases, credenciales seguras, compatibilidad del proveedor o evidencia de ejecución.

## Referencias para validar durante la implementación

- [TFM y compatibilidad de .NET Standard](https://learn.microsoft.com/en-us/dotnet/standard/frameworks); [conversión de soluciones con `dotnet sln`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln).
- [EnterpriseLibrary.Common.NetCore 6.3.0](https://www.nuget.org/packages/EnterpriseLibrary.Common.NetCore/6.3.0); [System.Configuration.ConfigurationManager 10.0.0](https://www.nuget.org/packages/System.Configuration.ConfigurationManager/10.0.0).
- [Microsoft.Data.SqlClient 6.1.0](https://www.nuget.org/packages/Microsoft.Data.SqlClient/6.1.0); [migración desde System.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/migrate-system-data-sql-client-to-microsoft-data-sql-client).
- [Requisitos de Oracle ODP.NET](https://docs.oracle.com/en/database/oracle/oracle-database/26/odpnt/InstallSystemRequirements.html); [Oracle.ManagedDataAccess.Core 2.19.310](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/2.19.310); [Core 23.26.0](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/23.26.0); [ManagedDataAccess 23.26.0](https://www.nuget.org/packages/Oracle.ManagedDataAccess/23.26.0).
- [MSTest y plataformas](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-intro); [Microsoft.NET.Test.Sdk 17.14.1](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.1); [MSTest.TestFramework 4.0.0](https://www.nuget.org/packages/MSTest.TestFramework/4.0.0).
