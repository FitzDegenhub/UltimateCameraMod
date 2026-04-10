[English](README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | **Español** | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 ya esta disponible!** Sobrecargas de Sacred God Mode, opcion para desactivar la rotacion automatica del lock-on y todas las correcciones de errores. Descarga desde **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** o **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Kit de herramientas de camara independiente para Crimson Desert. Interfaz grafica completa, vista previa en vivo, tres niveles de edicion, presets basados en archivos, **exportacion JSON para [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** y **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), y soporte de HUD para ultrawide.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
</p>

<p align="center">

[![Download v3.1.2](https://img.shields.io/badge/Download-v3.1.2-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![Wiki](https://img.shields.io/badge/Wiki-Documentation-8B5CF6?style=for-the-badge&logo=bookstack&logoColor=white)](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)

[![VirusTotal v3.1.2](https://img.shields.io/badge/VirusTotal_v3.1.2-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021)
[![Reddit Discussion](https://img.shields.io/badge/Reddit-Discussion-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/CrimsonDesert/comments/1sfou61/ucm_ultimate_camera_mod_v3_crimson_desert_full/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

</p>

> Necesitas ayuda? Consulta la **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** para guias de configuracion, explicacion de los ajustes de camara, gestion de presets, solucion de problemas y documentacion para desarrolladores.

---

<details>
<summary><strong>Capturas de pantalla (v3.x)</strong> -- haz clic para expandir</summary>
<br>

**UCM Quick** -- distancia, altura, desplazamiento, FoV, zoom de lock-on, steadycam, vistas previas en vivo
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- ajuste profundo con tarjetas con bordes y busqueda
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** -- editor XML completo con comparacion vanilla
![God Mode](screenshots/v3.x/godmode.png)

**Exportar JSON** -- exportacion para JSON Mod Manager / CDUMM
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**Importar** -- importar desde .ucmpreset, XML, PAZ o paquetes de gestores de mods
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## Resumen de ramas

| Rama | Estado | Descripcion |
|------|--------|-------------|
| **`main`** | v3.1.2 Release | Kit de herramientas de camara independiente con editor de tres niveles (UCM Quick / Fine Tune / God Mode), presets basados en archivos, catalogo comunitario, exportacion multiformato e instalacion directa de PAZ |
| **`development`** | Desarrollo | Rama de desarrollo de la proxima version |

v3 incluye todas las funciones de camara de v2 mas una interfaz rediseñada, presets basados en archivos, un editor de tres niveles y exportacion multiformato. La instalacion directa de PAZ sigue disponible en v3 como opcion secundaria.

---

## Caracteristicas

### Controles de camara

| Caracteristica | Detalles |
|----------------|----------|
| **8 presets integrados** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - con vista previa en vivo |
| **Camara personalizada** | Controles deslizantes para distancia (1.5-12), altura (-1.6-1.5) y desplazamiento horizontal (-3-3). El escalado proporcional mantiene al personaje en la misma posicion en pantalla en todos los niveles de zoom |
| **Campo de vision** | Vanilla 40 grados hasta 80 grados. Consistencia universal del FoV en los estados de guardia, apuntado, montura, planeo y cinematicas |
| **Camara centrada** | Personaje centrado en mas de 150 estados de camara, eliminando el desplazamiento del hombro hacia la izquierda |
| **Zoom de lock-on** | Control deslizante de -60% (acercar al objetivo) a +60% (alejar). Afecta todos los estados de lock-on, guardia y carga. Funciona independientemente del Steadycam |
| **Rotacion automatica de lock-on** | Desactiva el giro automatico de la camara hacia el objetivo al fijar. Evita que la camara gire bruscamente hacia enemigos detras de ti. Creditos a [@sillib1980](https://github.com/sillib1980) |
| **Sincronizacion de camara en montura** | Las camaras de montura se ajustan a la altura de camara elegida para el jugador |
| **Desplazamiento horizontal en todas las monturas** | Caballo, elefante, guiverno, canoa, maquina de guerra y escoba respetan tu configuracion de desplazamiento con escalado proporcional |
| **Consistencia en apuntado de habilidades** | Linterna, Destello Cegador, Arco y todas las habilidades de apuntado/zoom/interaccion respetan el desplazamiento horizontal. Sin saltos de camara al activar habilidades |
| **Suavizado Steadycam** | Tiempos de mezcla y vaiven de velocidad normalizados en mas de 30 estados de camara: reposo, caminar, correr, sprint, combate, guardia, carga, caida libre, super salto, tiron/balanceo de cuerda, retroceso, todas las variantes de lock-on, lock-on en montura, lock-on de reanimacion, agresion/busqueda, maquina de guerra y todos los estados de montura. Cada valor es ajustable por la comunidad a traves del editor Fine Tune |
| **Sacred God Mode** | Los valores que editas en God Mode quedan permanentemente protegidos de las reconstrucciones de Quick/Fine Tune. Indicadores verdes muestran que valores son sagrados. Almacenamiento por preset |

> **Filosofia de diseño de v3: solo edicion de valores, sin inyeccion estructural.**
>
> Las versiones anteriores inyectaban nuevas lineas XML en el archivo de camara (niveles de zoom extra, modo primera persona en caballo, renovacion de la camara del caballo con niveles de zoom adicionales). v3 elimina estas funciones intencionalmente. La inyeccion de estructura tiene una probabilidad mucho mayor de fallar despues de actualizaciones del juego, y las preferencias personales para modos de camara especializados se sirven mejor con mods dedicados distribuidos a traves de gestores de mods. UCM ahora solo modifica valores existentes: el mismo numero de lineas, la misma estructura de elementos, los mismos atributos. Esto hace que los presets sean mas seguros para compartir y mas resistentes a los parches del juego.

### Editor de tres niveles (v3)

v3 organiza la edicion en tres pestañas para que puedas profundizar tanto como quieras:

| Nivel | Pestaña | Que hace |
|-------|---------|----------|
| 1 | **UCM Quick** | La capa rapida: controles de distancia/altura/desplazamiento, FoV, camara centrada, zoom de lock-on (-60% a +60%), rotacion automatica de lock-on, sincronizacion de montura, steadycam, vistas previas de camara y FoV en vivo |
| 2 | **Fine Tune** | Ajuste profundo. Secciones con busqueda para niveles de zoom a pie, zoom de caballo/montura, FoV global, monturas especiales y travesia, combate y lock-on, suavizado de camara, y posicion de apuntado y punto de mira. Se construye sobre UCM Quick |
| 3 | **God Mode** | Editor XML completo: cada parametro en un DataGrid con busqueda y filtros, agrupado por estado de camara. Columna de comparacion vanilla. Sobrecargas sagradas (verde) protegidas de reconstrucciones. Filtro "Solo sagrados". 54 tooltips de atributos |

### Sistema de presets basado en archivos (v3)

- **Formato de archivo `.ucmpreset`** - formato compartible dedicado para presets de camara de UCM. Arrastra a cualquier carpeta de presets y funciona directamente
- **Gestor en la barra lateral** con secciones agrupadas y colapsables: Game Default, UCM Presets, Community Presets, My Presets, Imported
- **Nuevo / Duplicar / Renombrar / Eliminar** desde la barra lateral
- **Bloqueo** de presets para evitar ediciones accidentales: los presets de UCM estan permanentemente bloqueados; los presets de usuario se pueden alternar con el icono de candado
- **Preset True Vanilla** - `playercamerapreset` decodificado directamente de tu backup del juego sin modificaciones aplicadas. Los controles de Quick se sincronizan con los valores reales de referencia del juego
- **Importar** desde `.ucmpreset`, XML, archivos PAZ o paquetes de gestores de mods. Las importaciones de `.ucmpreset` obtienen control total de los deslizadores de UCM; las importaciones de XML/PAZ/gestor de mods son presets independientes (solo edicion en God Mode, sin reglas de UCM aplicadas) para preservar los valores del autor original del mod
- **Auto-guardado** - los cambios en presets desbloqueados se escriben automaticamente en el archivo del preset (con retardo)
- Migracion automatica de presets legados `.json` a `.ucmpreset` en el primer inicio

### Catalogos de presets (v3)

Explora y descarga presets directamente desde UCM. Descarga con un clic, sin necesidad de cuentas.

- **UCM Presets** - 7 estilos de camara oficiales (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definiciones alojadas en GitHub, XML de sesion generado localmente a partir de tus archivos del juego y las reglas de camara actuales. Se regenera automaticamente cuando se actualizan las reglas de camara
- **[Community presets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** - presets contribuidos por la comunidad en el repositorio principal, catalogo generado automaticamente por GitHub Actions
- **Boton Explorar** en cada encabezado de grupo de la barra lateral abre el explorador del catalogo
- Cada preset muestra nombre, autor, descripcion, etiquetas y un enlace a la pagina de Nexus del creador
- **Deteccion de actualizaciones** - icono de actualizacion pulsante cuando hay una version mas reciente disponible en el catalogo. Haz clic para descargar la actualizacion con copia de seguridad opcional en My Presets
- Los presets descargados aparecen en la barra lateral (bloqueados por defecto - duplica para editar)
- **Limite de tamaño de 2MB** y validacion JSON por seguridad

**Quieres compartir tu preset con la comunidad?** Exporta como `.ucmpreset` desde UCM y luego:
- Envia un [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) añadiendo tu preset a la carpeta `community_presets/`
- O envia tu archivo `.ucmpreset` a 0xFitz en Discord/Nexus y lo añadiremos por ti

### Exportacion multiformato (v3)

El dialogo **Exportar para compartir** genera tu sesion en cuatro formatos:

| Formato | Caso de uso |
|---------|-------------|
| **JSON** (gestores de mods) | Parches de bytes + `modinfo` para **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) o **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Exporta en UCM, importa en el gestor que uses; los destinatarios no necesitan UCM. **Preparar** solo se ofrece cuando la entrada `playercamerapreset` en vivo aun coincide con el backup vanilla de UCM (verifica los archivos del juego si ya aplicaste mods de camara). |
| **XML** | `playercamerapreset.xml` sin procesar para otras herramientas o edicion manual |
| **0.paz** | Archivo parcheado listo para colocar en la carpeta `0010` del juego |
| **.ucmpreset** | Preset completo de UCM para otros usuarios de UCM |

Incluye campos de titulo, version, autor, URL de Nexus y descripcion para JSON/XML. Muestra el conteo de regiones parcheadas y bytes cambiados antes de guardar el `.json`.

### Calidad de vida

- **Deteccion automatica del juego** - Steam, Epic Games, Xbox / Game Pass
- **Copia de seguridad automatica** - backup vanilla antes de cualquier modificacion; restauracion con un clic. Compatible con versiones y limpieza automatica al actualizar
- **Banner de configuracion de instalacion** - muestra tu configuracion activa completa (FoV, distancia, altura, desplazamiento, ajustes)
- **Deteccion de parches del juego** - rastrea los metadatos de instalacion despues de aplicar; avisa cuando el juego puede haberse actualizado para que puedas re-exportar
- **Vista previa en vivo de camara y FoV** - vista superior con reconocimiento de distancia, desplazamiento horizontal y cono de campo de vision
- **Notificaciones de actualizacion** - comprueba las versiones de GitHub al iniciar
- **Acceso directo a la carpeta del juego** - abre el directorio del juego desde el encabezado
- **Identidad en la barra de tareas de Windows** - agrupacion correcta de iconos y icono en la barra de titulo mediante shell property store
- **Persistencia de configuracion** - todas las selecciones se recuerdan entre sesiones
- **Ventana redimensionable** - el tamaño persiste entre sesiones
- **Portable** - un solo `.exe`, no requiere instalador

### Filosofia

> **Nadie ha perfeccionado la camara de Crimson Desert todavia -- y ese es el punto.**
>
> El juego vanilla tiene mas de 150 estados de camara, cada uno con docenas de parametros. Ningun desarrollador individual puede ajustar todo eso para cada estilo de juego y pantalla. Por eso existe UCM -- no para decirte cual es la camara perfecta, sino para darte las herramientas para encontrarla tu mismo y compartirla con otros.
>
> Cada ajuste que modifiques se puede exportar y compartir. La correccion de rotacion automatica del lock-on que elimino los saltos de camara durante el combate fue descubierta por un solo miembro de la comunidad experimentando en God Mode. Ese tipo de ajuste fino impulsado por la comunidad es exactamente para lo que esta herramienta fue creada.

### Compartir presets

Exporta tu configuracion de camara como archivo `.ucmpreset` y compartelo con otros. Importa presets del catalogo comunitario, Nexus Mods u otros jugadores. UCM tambien exporta a JSON (para [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) y [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), XML sin procesar e instalacion directa de PAZ.

---

## Como funciona

1. Localiza el archivo PAZ del juego que contiene `playercamerapreset.xml`
2. Crea una copia de seguridad del archivo original (solo una vez - nunca sobrescribe un backup limpio)
3. Descifra la entrada del archivo (ChaCha20 + derivacion de clave Jenkins hash)
4. Descomprime via LZ4
5. Analiza y modifica los parametros XML de la camara segun tus selecciones
6. Re-comprime, re-cifra y escribe la entrada modificada de vuelta al archivo

Sin inyeccion de DLL, sin modificacion de memoria, sin conexion a internet requerida -- modificacion pura de archivos de datos.

---

## Compilar desde el codigo fuente

Requiere [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (o posterior). Windows x64.

### v3 (recomendado)

Cierra cualquier instancia en ejecucion antes de compilar - el paso de copia del exe falla si el archivo esta bloqueado.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Dependencias (NuGet - se restauran automaticamente)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - Compresion/descompresion de bloques LZ4

---

## Estructura del proyecto

```
src/UltimateCameraMod/              Biblioteca compartida + aplicacion WPF v2.x
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, modelos de datos
├── Paz/                            ArchiveWriter, CompressionUtils, E/S de PAZ
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 Interfaz v2.x
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           Aplicacion WPF v3 con prioridad en exportacion (referencia el codigo compartido)
├── Controls/                       CameraPreview, FovPreview (variantes v3)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Presets comunitarios integrados desplegados en el primer inicio
├── MainWindow.xaml                 Shell de dos paneles: barra lateral + editor con pestañas
├── ExportJsonDialog.xaml           Asistente de exportacion multiformato (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Importar desde .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Entrada de metadatos del preset (nombre, autor, descripcion, URL)
├── CommunityBrowserDialog.xaml     Explorar y descargar presets comunitarios desde GitHub
├── NewPresetDialog.xaml            Crear / nombrar nuevos presets
├── ShellTaskbarPropertyStore.cs    Icono de barra de tareas de Windows via shell property store
├── ApplicationIdentity.cs          App User Model ID compartido
└── UltimateCameraMod.V3.csproj

community_presets/                  Presets de camara contribuidos por la comunidad
ucm_presets/                        Definiciones oficiales de presets de estilo UCM
```

---

## Compatibilidad

- **Plataformas:** Steam, Epic Games, Xbox / Game Pass
- **Sistema operativo:** Windows 10/11 (x64)
- **Pantalla:** Cualquier relacion de aspecto - 16:9, 21:9, 32:9

---

## Preguntas frecuentes

**Me pueden banear por esto?**
UCM solo modifica archivos de datos offline. No toca la memoria del juego, no inyecta codigo ni interactua con procesos en ejecucion. Usa bajo tu propia responsabilidad en modos online/multijugador.

**El juego se actualizo y mi camara volvio a vanilla.**
Normal - las actualizaciones del juego sobrescriben los archivos modificados. Vuelve a abrir UCM y haz clic en Instalar (o re-exporta JSON para JSON Mod Manager / CDUMM). Tus configuraciones se guardan automaticamente.

**Mi antivirus detecto el exe.**
Falso positivo conocido con aplicaciones .NET autocontenidas. El escaneo de VirusTotal esta limpio: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). El codigo fuente completo esta disponible aqui para revisar y compilar tu mismo.

**Que significa desplazamiento horizontal 0?**
0 = posicion de camara vanilla (personaje ligeramente a la izquierda). 0.5 = personaje centrado en pantalla. Los valores negativos mueven mas a la izquierda, los valores positivos mueven mas a la derecha.

**Actualizando desde una version anterior?**
Usuarios de v3.x: simplemente reemplaza el exe, todos los presets y configuraciones se conservan. Usuarios de v2.x: elimina la carpeta antigua de UCM, verifica los archivos del juego en Steam, y luego ejecuta v3.1 desde una carpeta nueva. Consulta las [notas de la version](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) para instrucciones detalladas.

---

## Historial de versiones

- **v3.1.2** - Correccion de valores sagrados faltantes en Instalar/exportaciones en la pestaña God Mode. Ver [notas de la version](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2).
- **v3.1.1** - Correccion de deteccion falsa positiva de backup contaminado en archivos de juego limpios.
- **v3.1** - Sobrecargas de Sacred God Mode (ediciones de usuario protegidas permanentemente de reconstrucciones). Opcion de rotacion automatica de lock-on (creditos a [sillib1980](https://github.com/sillib1980)). Indicadores verdes de valores sagrados. Correccion de instalacion de Full Manual Control. Overlay de actualizacion compatible con versiones. Ver [notas de la version](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** - Todos los dialogos convertidos a sistema de overlay dentro de la aplicacion. Las sobrecargas de God Mode persisten entre cambios de pestaña. Seleccion de tipo de preset (UCM Managed vs Full Manual Control). Catalogo de presets comunitarios movido al repositorio principal. 54 tooltips de atributos de God Mode. Correcciones de crashes del juego. Validacion vanilla actualizada para el parche del juego de junio 2026. Wiki de 21 paginas.
- **v3.0.1** - Rediseño con prioridad en exportacion. Editor de tres niveles (UCM Quick / Fine Tune / God Mode). Formato de archivo `.ucmpreset`. Sistema de presets basado en archivos. Catalogos de presets UCM y comunitarios. Exportacion multiformato. Steadycam expandido a mas de 30 estados de camara. Control deslizante de zoom de lock-on.
- **v2.5** - Ultima version de v2.x.
- **v2.4** - Desplazamiento horizontal proporcional, desplazamiento en todas las monturas y habilidades de apuntado, renovacion de la camara del caballo, copias de seguridad con reconocimiento de version, vista previa de FoV, ventana redimensionable.
- **v2.3** - Correccion del desplazamiento horizontal para 16:9, control deslizante basado en delta, banner completo de configuracion de instalacion.
- **v2.2** - Steadycam, niveles de zoom extra, primera persona en caballo, desplazamiento horizontal, FoV universal, consistencia en apuntado de habilidades, importar XML, compartir presets, notificaciones de actualizacion.
- **v2.1** - Correccion de los controles deslizantes de presets personalizados que no escribian en todos los niveles de zoom.
- **v2.0** - Reescritura completa de Python a C# / .NET 6 / WPF. Editor XML avanzado, gestion de presets, deteccion automatica del juego.
- **v1.5** - Version Python con interfaz customtkinter.

---

## Creditos y agradecimientos

- **0xFitz** - Desarrollo de UCM, ajuste de camara, editor avanzado
- **[@sillib1980](https://github.com/sillib1980)** - Descubrio los campos de camara de rotacion automatica de lock-on

### Reescritura en C# (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - Analizador C# de PAZ/PAMT, cifrado ChaCha20, compresion LZ4, PaChecksum, reempaquetador de archivos (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - Implementacion pura en C# del cifrado de flujo ChaCha20 (BSD)
- **[MrIkso en Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - Guia de reempaquetado PAZ: alineacion de 16 bytes, checksum PAMT, parcheo del indice raiz PAPGT

### Version original en Python (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - Herramientas de archivos PAZ, investigacion de descifrado
- **Maszradine** - CDCamera - Reglas de camara, sistema steadycam, presets de estilo
- **manymanecki** - CrimsonCamera - Arquitectura de modificacion dinamica de PAZ

## Apoyo

Si encuentras esto util, considera apoyar el desarrollo:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licencia

MIT
