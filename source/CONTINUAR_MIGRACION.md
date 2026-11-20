# Para retomar la migración

El plan completo está en [UPGRADE_NET10.md](UPGRADE_NET10.md). Leerlo y verificar el estado del repositorio antes de ejecutar actividades. El plan fue guardado en el commit `8a1ea0a` de la rama `_net_10_migration`; su existencia no significa que la migración se haya ejecutado.

- Los destinos acordados son `net48`, `netstandard2.0`, `net8.0` y `net10.0`. Se propone usar `Microsoft.Data.SqlClient` en todos ellos. El plan documenta las pruebas y los límites de soporte, incluido .NET 8.
- Probar primero el paquete público de Common, empezando por la versión actualmente referenciada si corresponde. Si las pruebas pasan, no es necesario publicar el fork. La prueba aún está pendiente.
- Si se necesita el fork de Common, está en `E:\vh4Apps\Tools\common-infrastructure`; según el usuario, la rama actual ya soporta .NET 10 y .NET Standard 2.0. El feed privado es `https://nuget.aseinfo.net`. El plan detalla la publicación como alternativa condicionada.
- El usuario pidió guardar el plan y ejecutar la migración otro día. Retomar su ejecución cuando lo solicite; no interpretar esta nota como evidencia de compilación, pruebas, publicación o integración.
- Al guardar el plan había un cambio previo en `source/Data.sln`, ajeno al commit. Revisar el estado actual y preservar los cambios existentes.
