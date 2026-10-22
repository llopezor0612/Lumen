# Lumen

Aplicación conversacional local para una persona. Utiliza Python 3.10 o posterior y SQLite de la biblioteca estándar; no requiere npm ni dependencias adicionales. El nombre puede cambiarse con `APP_NAME` en `.env`; también puedes editar `system_prompt.txt`.

## Inicio
1. Abre una terminal en esta carpeta.
2. Instala Ollama para Windows desde [ollama.com/download/windows](https://ollama.com/download/windows).
3. Descarga un modelo local: `ollama pull qwen3:4b`. Ollama mantiene su API en `http://127.0.0.1:11434`.
4. Copia `.env.example` a `.env` (en PowerShell: `Copy-Item .env.example .env`). El archivo ya está preparado para `PROVIDER=ollama` y `qwen3:4b`.
5. Ejecuta `python server.py` y abre http://localhost:8765. Cierra la terminal para detenerlo.

El equipo de desarrollo actual ya tiene Ollama, `qwen3:1.7b` y el modo local configurados. La primera consulta puede tardar mientras el modelo se carga en memoria. Para volver a usar OpenAI, cambia `PROVIDER=openai` y completa sus dos variables.

El modo local no necesita claves ni genera costes por consulta; requiere espacio, memoria y tiempo de cómputo del equipo. El estado «configurado» indica que el modelo está seleccionado, no que haya terminado de cargarse.

## Arquitectura y datos
El frontend HTML/CSS/JavaScript se comunica con un servidor Python ligado a 127.0.0.1. El servidor valida solicitudes locales, guarda conversaciones y preferencias en `lumen.db`, construye el contexto y llama a OpenAI Responses. El prompt de sistema está separado en `system_prompt.txt`; el proveedor se encapsula en la ruta de envío de `server.py`, que puede sustituirse por un adaptador. Sin cuentas ni aislamiento multiusuario: no exponer a una red ni utilizar en un equipo compartido sin controles adicionales. No hay TLS, autenticación ni cifrado de datos en reposo.

Cada respuesta envía el mensaje, el texto de los adjuntos, las preferencias y los mensajes recientes al proveedor. Los adjuntos se incorporan al mensaje guardado en SQLite y permanecen hasta borrar la conversación. Los originales no se guardan como archivos separados. Acepta texto UTF-8 y código hasta 100 KB por archivo y 100.000 caracteres por mensaje total. El usuario puede exportar JSON o borrar cada conversación; borrar `lumen.db` con el servidor detenido elimina todos los datos locales. Consulta la política del proveedor para su tratamiento de datos.

El historial es por conversación; las preferencias básicas son globales. No existe memoria automática entre conversaciones. Cuando el contexto anterior supera un presupuesto aproximado de caracteres se excluyen mensajes antiguos y se avisa; no se realiza un resumen. Este presupuesto no garantiza ajustarse a todos los límites de tokens de todos los modelos.

## Funcionalidades y límites
Incluye creación, recuperación, renombrado y eliminación con confirmación, búsqueda por título, exportación, texto y código adjuntos, copia de respuestas y bloques, ajustes, tema adaptable, historial persistente y avisos de error. La llamada al proveedor es real y se realiza en servidor, pero no transmite progresivamente; el mensaje se conserva en el editor cuando falla. La respuesta y el mensaje se guardan juntos solo tras una respuesta correcta. El texto de los archivos se trata como datos, aunque ningún prompt garantiza inmunidad frente a instrucciones maliciosas.

Pendiente: búsqueda por contenido, streaming y cancelación, regeneración, edición de mensajes, resaltado de sintaxis, diferencias de archivos, PDF e imágenes, búsqueda web, memoria avanzada, traducción de la interfaz al inglés, pruebas de interfaz automatizadas, límites de uso y adaptadores adicionales. No se ejecuta código ni se accede a archivos locales fuera de los seleccionados. Las preferencias son configuraciones explícitas, no memoria de información sensible.

Para desplegar, incorpora antes autenticación y separación de usuarios, protección CSRF y límites de uso adecuados al entorno, almacenamiento persistente seguro, TLS y un gestor de secretos. La versión actual es exclusivamente local. No hay garantías de cumplimiento normativo.

## Búsqueda web opcional

El archivo `.env` del equipo está configurado con `SEARCH_ENABLED=true` y Bing como proveedor HTML. Lumen decide buscar para consultas que parecen actuales (por ejemplo, noticias, precios o información de hoy) y también acepta la casilla de búsqueda en la interfaz. Las fuentes se devuelven junto a la respuesta y se incluyen como enlaces. Para uso sin internet cambia `SEARCH_ENABLED=false`. La búsqueda depende de la disponibilidad y formato del buscador; para producción sustituye este adaptador por una API oficial con clave, límites y condiciones de uso adecuadas.

## Aplicación de escritorio

El instalador crea un acceso directo a `LumenDesktop.exe`. La ventana nativa inicia el servidor local, espera a que responda y muestra la interfaz integrada. Al cerrarla, detiene el servidor que ella misma inició. Ollama y los modelos se mantienen fuera del instalador porque pueden ocupar varios gigabytes; si faltan, la interfaz sigue abriendo y muestra el estado de configuración para poder repararlo.

## Instalador de Windows

El instalador actual está en `installer-output/Lumen-Setup-1.3.0.exe`. Es por usuario, no requiere permisos de administrador, usa un identificador estable y actualiza la instalación existente conservando `lumen.db` y `.env`. También crea accesos directos y una entrada de desinstalación. Para generar otra versión, recompila el servidor, publica `LumenDesktop` para `win-x64`, incrementa `MyAppVersion` en `installer.iss` y ejecuta Inno Setup.

La búsqueda web se ejecuta automáticamente cuando está habilitada. Si no hay red o proveedor disponible, Lumen continúa en modo local con Ollama.

## Actualizaciones automáticas

La carcasa nativa consulta `LUMEN_UPDATE_URL` al iniciar. El feed debe ser HTTPS y devolver `version`, `url` y `sha256`, por ejemplo: `{ "version": "1.4.0", "url": "https://tu-dominio/Lumen-Setup-1.4.0.exe", "sha256": "..." }`. Lumen descarga el instalador, valida su hash y ofrece instalarlo. Para activar el flujo en una instalación distribuida, publica el EXE y ese JSON en un servidor HTTPS y define la URL en `.env`; no se incluye una URL inventada ni se exponen secretos.
