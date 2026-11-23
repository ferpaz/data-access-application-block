# Migración a Data Access Application Block 10.0.0-rc1 {#UpgradeNet10}

Esta guía complementa la [migración de la versión 6 a la 7](@ref Upgrade). La versión `10.0.0-rc1` es preliminar y está publicada en el feed privado; antes de adoptarla, prueba cada aplicación y conserva una alternativa de regreso a su versión anterior.

## Targets y paquetes

Los tres paquetes de esta entrega contienen assets `net48`, `netstandard2.0`, `net8.0` y `net10.0`:

| Uso | Paquete 10.0.0-rc1 |
|---|---|
| Núcleo y `GenericDatabase` | `EnterpriseLibrary.Data.NetCore` |
| SQL Server | `EnterpriseLibrary.Data.SqlServer.NetCore` |
| Oracle | `EnterpriseLibrary.Data.Oracle.NetCore` |

Los paquetes de proveedor dependen del núcleo; Data usa `EnterpriseLibrary.Common.NetCore` 6.3.0 publicado en NuGet.org. Configura los dos orígenes y su mapeo siguiendo la sección **Consumir paquetes del feed privado** del [README](../../README.md). No guardes credenciales en el repositorio.

Ya no se publican assets específicos `net47`, `net472` ni `net6.0` en estos paquetes. NuGet podría seleccionar el asset `netstandard2.0` para otros frameworks compatibles; eso no implica soporte ni pruebas para esos consumidores. `netstandard2.0` no es un runtime: el ejecutable debe tener un target compatible y sus propias dependencias. .NET 8 se conserva como target, pero Microsoft terminó su soporte el 10 de noviembre de 2026. La presente entrega no genera paquetes específicos ODBC, OLE DB ni SQL Server CE; los consumidores de esos paquetes deben evaluar su ruta de actualización por separado.

## Pasos de actualización

1. Identifica el target, el sistema operativo, la arquitectura, los proveedores y los proyectos que consumen directamente los tipos de SQL Server. En .NET Framework, actualiza a `net48` si corresponde; para aplicaciones modernas elige un runtime disponible y compatible con el despliegue.
2. Configura el feed privado y NuGet.org. Actualiza el núcleo y los paquetes SQL Server u Oracle utilizados a `10.0.0-rc1`; evita mezclar versiones de Data y sus proveedores. Restaura las dependencias con el cliente NuGet y verifica los assets seleccionados por el proyecto ejecutable, por ejemplo en `obj\project.assets.json` para proyectos `PackageReference`.
3. Para SQL Server, reemplaza referencias y usos directos de `System.Data.SqlClient` por `Microsoft.Data.SqlClient`. Revisa conexiones, comandos, parámetros, lectores, transacciones, adaptadores, builders y capturas de excepciones: los tipos de ambos proveedores no son intercambiables. Si tu código usa `WithConnectionString(SqlConnectionStringBuilder)`, debe pasar un `Microsoft.Data.SqlClient.SqlConnectionStringBuilder`. Los namespaces de DAAB siguen siendo `Microsoft.Practices.EnterpriseLibrary.Data` y `Microsoft.Practices.EnterpriseLibrary.Data.Sql`.
4. Revisa las cadenas de conexión, `providerName`, los mapeos DAAB y el registro de fábricas. El nombre SQL predeterminado es ahora `Microsoft.Data.SqlClient`. La constante pública `DbProviderMapping.DefaultSqlProviderName` puede estar incorporada en binarios compilados anteriormente: recompila también las bibliotecas consumidoras y actualiza las cadenas o configuraciones que contengan el nombre antiguo. No supongas que los aliases utilizados por las pruebas de este repositorio sean una garantía para tu aplicación.
5. Recompila la aplicación y sus bibliotecas. En `net48`, revisa los binding redirects generados, el archivo `.exe.config` o `web.config` efectivamente desplegado y las dependencias nativas de SqlClient; en hosts modernos revisa que se publiquen las dependencias adecuadas para el sistema y la arquitectura. No copies manualmente el asset de referencia Standard de SqlClient como implementación ejecutable.
6. Prueba en un entorno representativo las conexiones, consultas y procedimientos, parámetros, lectores, transacciones, errores, configuración, autenticación y escenarios asíncronos que utilice tu aplicación. Para Oracle, valida también el driver, el servidor y las operaciones usadas. Verifica los activos realmente cargados cuando sea importante distinguir Standard de un asset nativo.

**Nota sobre documentación XML:** algunos comentarios de API de esta versión todavía mencionan `System.Data.SqlClient` para Framework y Standard. La implementación de `SqlDatabase` usa `Microsoft.Data.SqlClient` en todos los targets de esta entrega.

## Configuración SQL

En una configuración XML, actualiza el nombre de proveedor y el mapeo DAAB existentes. Este fragmento muestra solo los elementos relevantes; conserva las declaraciones de sección y el resto de la configuración de tu aplicación:

```xml
<connectionStrings>
  <add name="MiConexion" connectionString="CADENA_DE_CONEXION_SEGURA"
	   providerName="Microsoft.Data.SqlClient" />
</connectionStrings>
<dataConfiguration defaultDatabase="MiConexion">
  <providerMappings>
	<add name="Microsoft.Data.SqlClient"
		 databaseType="Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase, Microsoft.Practices.EnterpriseLibrary.Data.SqlServer" />
  </providerMappings>
</dataConfiguration>
```

Si la aplicación resuelve la fábrica por nombre, el proveedor debe estar registrado en el mecanismo que use su host. En `net48`, una opción es registrar `Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient` en `system.data/DbProviderFactories` del archivo de configuración efectivo. En hosts modernos que usen `System.Data.Common.DbProviderFactories`, el código puede registrar `Microsoft.Data.SqlClient.SqlClientFactory.Instance` con el nombre `Microsoft.Data.SqlClient` antes de resolverla. El asset Standard de DAAB incluye un shim propio de fábricas; no asumas que comparte el registro del runtime: prueba la ruta concreta de tu consumidor.

En `net48`, verifica cuál archivo de configuración copia realmente el proyecto al directorio de salida: modificar un `appsettings.json` no afecta automáticamente a código que consulta `ConfigurationManager.ConnectionStrings`. Los consumidores que usan JSON deben proporcionar explícitamente esa configuración a la aplicación; no se presupone que DAAB lea ese archivo por sí solo.

## Cifrado y certificados

Revisa los valores de cifrado y confianza del certificado al cambiar de proveedor. Para producción, utiliza la configuración de seguridad aprobada y un certificado válido y confiable. Las pruebas locales de esta migración usaron excepciones de confianza y, en algunas conexiones, `Encrypt=False`; **no son recomendaciones para producción**. No incluyas contraseñas ni claves en el código o en ejemplos compartidos.

## Alcance validado y límites

Se probaron las suites nativas en `net48`, `net8.0` y `net10.0` con las omisiones registradas, y el usuario informó una prueba intensiva satisfactoria de `10.0.0-rc1` en un consumidor real `net48`. También se forzaron las DLL `netstandard2.0` publicadas en los tres runtimes y pasaron operaciones básicas SQL y Oracle; no se ejecutaron las suites Standard completas ni se validó la selección normal de NuGet con ese host. Las pruebas del consumidor real no cubren automáticamente otros consumidores, runtimes o despliegues. El proveedor Oracle Standard se probó en esos casos, pero esto no acredita soporte oficial de Oracle para todas las combinaciones.

Los paquetes del feed privado no tienen firma NuGet; su uso sin esa firma fue aceptado para esta entrega. La firma strong-name de un ensamblado no equivale a una firma NuGet. Para límites y temas posteriores, consulta [POST-UPGRADE-NET10.md](../../source/POST-UPGRADE-NET10.md).