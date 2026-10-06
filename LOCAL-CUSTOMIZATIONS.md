# DeskBox personalizado

Base: DeskBox 1.5.5 (`v1.5.5`).

Fork: [Pictus08/DeskBox](https://github.com/Pictus08/DeskBox).
Proyecto original: [Tianyu199509/DeskBox](https://github.com/Tianyu199509/DeskBox),
creado por Tianyu Zhu. Se conserva la licencia [GPL-3.0](LICENSE).

## Color por tarjeta

En el menú del título de una tarjeta (`…` o clic derecho), o haciendo clic
derecho en una zona vacía de su contenido:

**Color de fondo → Color personalizado → Guardar**.

El color es independiente de las demás tarjetas. El selector también permite
ajustar **Transparencia del fondo**: 0 % es opaco y 100 % es transparente.
Solo cambia el fondo; los iconos, los controles y el texto mantienen su opacidad.
Se conserva en los metadatos del widget al reiniciar. **Usar ajuste global** recupera el material
original. Si el texto sigue el tema, su contraste se ajusta automáticamente;
la opción existente **Texto y legibilidad** permite elegirlo manualmente.

**Colores recientes** guarda los últimos 10 colores distintos utilizados,
compartidos entre todas las tarjetas y conservados entre sesiones. Reutilizar
un color lo mueve al principio. Están disponibles en el menú y como muestras
dentro del selector; elegir una muestra en el selector requiere pulsar
**Guardar**, mientras que elegir un color desde el menú lo aplica directamente.
**Cancelar** no altera el historial ni la apariencia.

## Escritorio limpio

**F7** (o el atajo global configurado), el clic izquierdo en el icono de DeskBox
y la opción **Escritorio limpio: ocultar todo** de su menú ocultan tarjetas e
iconos de Windows. Repetir la acción restaura las tarjetas y la visibilidad
anterior de los iconos. También se puede activar con `DeskBox.exe --toggle-widgets`.

Solo se oculta la lista visual de iconos de Explorer; no se mueven archivos ni
se modifica la opción de Windows «Mostrar iconos del escritorio». Los iconos
se restauran al cerrar DeskBox. Un marcador de recuperación permite restaurarlos
en el siguiente inicio si la aplicación fue terminada de forma inesperada.

## Compilación local

Requiere el SDK .NET indicado en `global.json`, las herramientas C++ de Visual
Studio y Rust. Los comandos se ejecutan desde la raíz del repositorio.

```powershell
dotnet restore .\DeskBox.sln -p:Platform=x64 --source https://api.nuget.org/v3/index.json
dotnet test .\tests\DeskBox.Tests\DeskBox.Tests.csproj --no-restore --verbosity:minimal -p:Platform=x64 -p:RuntimeIdentifier=win-x64 -p:WindowsAppSDKSelfContained=true --filter "FullyQualifiedName~WidgetBackgroundSettingsTests|FullyQualifiedName~SaveAndReload_PreservesRecentBackground|FullyQualifiedName~SettingsSlice|FullyQualifiedName~FileSurfaceParityContractTests|FullyQualifiedName~Localization"
# Cerrar primero la instancia de este repositorio.
dotnet build .\src\DeskBox\DeskBox.csproj --configuration Debug --no-restore -p:Platform=x64 -p:OutputPath=bin\Debug\net10.0-windows10.0.22621.0\ -p:WindowsAppSDKSelfContained=true -v:minimal
.\scripts\complete-local-runtime.ps1
```

Ejecutable local:
`src\DeskBox\bin\Debug\net10.0-windows10.0.22621.0\DeskBox.exe`.

La versión local usa los datos habituales de `%LocalAppData%\DeskBox`.
Una actualización oficial no incorpora estos cambios: deben conservarse y
recompilarse sobre la nueva versión antes de sustituir esta compilación.
