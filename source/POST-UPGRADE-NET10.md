# Temas posteriores y fuera del alcance de la migración .NET 10

Este documento conserva los temas diferidos y las exclusiones acordadas para `10.0.0-rc1`. Su inclusión no acredita pruebas adicionales ni constituye una promesa de implementación. Las fechas y detalles de ejecución están en [UPGRADE_NET10.md](UPGRADE_NET10.md).

## Diferidos

### Integración en `master`

La integración de `_net_10_migration` en `master` queda diferida hasta que el usuario confirme que ya no tiene reportes de problemas con el paquete `10.0.0-rc1`. No se ha establecido un plazo de observación ni se ha autorizado integrar automáticamente.

Antes de integrar, resolver las comprobaciones aplicables del plan, incluida la discrepancia entre el commit declarado por los paquetes y la rama de entrega, y decidir qué hacer con `UPGRADE_NET10.md`. Revisar los cambios a integrar y, después de la integración autorizada, compilar y ejecutar las pruebas pertinentes sobre el commit resultante. La información de trazabilidad y los pendientes están en [UPGRADE_NET10.md](UPGRADE_NET10.md).

### Consumidores limpios `net8.0` y `net10.0`

En otro requerimiento, restaurar y ejecutar `10.0.0-rc1` desde los orígenes configurados en consumidores limpios de ambos runtimes; comprobar assets, dependencias, configuración y operaciones relevantes. Las pruebas de `7.0.0-rc2` en esos targets no prueban la versión 10. La restauración aislada `net48` verificó orígenes, no ejecución funcional; el usuario informó además una prueba intensiva real `net48`, sin detallar todas sus operaciones.

### Símbolos de depuración

El feed privado actualmente no admite publicar archivos de símbolos. Cuando exista una opción apropiada, decidir cómo distribuirlos y comprobar su disponibilidad para consumidores y la correspondencia PDB/DLL. Los `.snupkg` locales contienen PDB para los cuatro targets, pero no se verificó esa correspondencia ni la publicación de símbolos. La firma NuGet del `.nupkg` es un asunto distinto: el usuario aceptó no exigirla para los paquetes de este proyecto en el feed privado.

## Fuera del alcance acordado

### Common alternativo y BVT

Se utiliza `EnterpriseLibrary.Common.NetCore` 6.3.0 del feed público. No se requiere actualizar ni publicar el fork de Common, ni completar la validación del modo `Project` para este cierre. Si se demuestra posteriormente una incompatibilidad que requiera el fork, identificar rama y commit, validar el paquete y sus dependencias y decidir su publicación sin guardar credenciales.

La alternativa anteriormente contemplada era usar el fork hermano `common-infrastructure`, cuya rama local se informó compatible con `net10.0` y `netstandard2.0`, sin exigir una nueva migración de su código. Antes de usarla habría que comprobar la rama, commit, proceso de versionado, ID, assets, metadatos, dependencias y pruebas del paquete; verificar qué asset selecciona cada consumidor y cualquier diferencia de API. Solo si se decide publicarlo, confirmar previamente la versión existente, el endpoint y los permisos, y verificar restauración sin incorporar secretos. Common público 6.3.0 puede seleccionar assets compatibles distintos de los targets nativos de Data; no se debe inferir que incluye un asset nativo `net48` o `net8.0`.

`BVT\Data.BVT` permanece en el repositorio tal como está, sin migrarse ni retirarse; está fuera de `Data.slnx`, apunta a `net47` y referencia proyectos ausentes. Cualquier cambio futuro requiere un alcance separado.

### Certificados y compatibilidad antigua

La prueba con un certificado confiable no se exigió para este requerimiento. Las excepciones de confianza y `Encrypt=False` usadas en pruebas locales no son una recomendación de producción; cada consumidor debe validar su configuración de seguridad.

La revisión adicional de configuración antigua, aliases y compatibilidad binaria se cerró por decisión del usuario, sin probar todas las combinaciones. Como referencia para una revisión futura, en las pruebas Oracle `App.config` registra en `net48` aliases antiguos para `System.Data.SqlClient`, mientras que `NetCoreHelpers` registra el nombre antiguo como alias de `Microsoft.Data.SqlClient` en `net8.0` y `net10.0`. Son registros de pruebas y no garantizan compatibilidad para los consumidores. Evaluar por separado si deben limpiarse o conservarse.

## Límites aceptados y posibles ampliaciones

- Las suites completas Release y la revisión individual de todas las omisiones no se exigieron para este cierre. Las pruebas omitidas no se cuentan como aprobadas. En Debug se registraron para Data `386/0/6` en net48 y `311/0/6` en net8 y net10; para Oracle `102/0/3` en net48 y `101/0/3` en net8 y net10 (aprobadas/fallidas/omitidas).
- Se aceptaron las pruebas Standard parciales en `net48`, `net8.0` y `net10.0`. Las DLL Standard publicadas se forzaron explícitamente y superaron operaciones básicas SQL y Oracle; no se ejecutaron las suites completas ni se comprobó la selección automática de NuGet. La prueba no acredita soporte oficial de Oracle para todas las combinaciones. Una futura prueba en un consumidor real debe verificar las DLL cargadas y describir las operaciones y runtime cubiertos.
- La versión 7.0.0-rc2 se había probado en `net8.0` y `net10.0`, pero esa evidencia no se atribuye a `10.0.0-rc1`. .NET 8 se conserva como target aunque su soporte por Microsoft terminó el 10 de noviembre de 2026.
- La prueba local usó Windows x64 y autenticación SQL. Autenticación integrada, Entra ID, otras arquitecturas y sistemas deben validarse si un consumidor los requiere; no se consideran verificados por esta entrega.