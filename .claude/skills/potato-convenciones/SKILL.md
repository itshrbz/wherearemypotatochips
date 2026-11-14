---
name: potato-convenciones
description: Convenciones de C#, carpetas y arquitectura de Where Are My Potato Chips. Usar antes de escribir, revisar o mover código o assets del proyecto.
---

# Convenciones del proyecto

## Estructura

Todo está en `Where_are_my_potato_chips/Assets/`:

| Carpeta | Qué va ahí |
| --- | --- |
| `00_Scenes` | `00_Boot`, `01_Menu`, `02_Store`, `10_Warehouse`, `11_Pools`, `12_Playground`, `90_Ending`. `_Sandbox` es para pruebas y no entra en la build. |
| `01_Scripts/<Área>` | `Audio`, `Core`, `Enemy`, `Flow`, `Interaction`, `Player`, `Sale`, `Shared`, `UI`. Un área nueva = carpeta nueva (por ejemplo `Anomalies`). |
| `02_Prefabs` | Jugador, HUD, kit modular, tienda réplica. |
| `03_Materials`, `04_Models`, `05_Textures` | Arte. Subcarpeta por espacio cuando es específico (`Levels/Pools`). |
| `07_data` | ScriptableObjects de datos (`SaleVariant`). |
| `08_Audio` | Clips. |
| `Editor` | Herramientas del menú `Tools/Kit/`. |
| `Resources` | `GameFlowSettings` y lo que se carga por nombre. |

## Estilo de C#

- Sin namespaces (el proyecto no los usa). Una clase pública por archivo, mismo nombre que el archivo.
- Identificadores en inglés (`BossAI`, `IsChasing`), comentarios y textos del jugador en español.
- `/// <summary>` corto en cada clase pública: qué hace y cuándo se usa.
- Campos editables con `[SerializeField]` privado; `[Header]` y `[Tooltip]` en español para lo que se ajusta en el Inspector.
- Nada de `GameObject.Find` en `Update`. Buscar una vez y guardar, o reintentar con un temporizador (ver `ScreenEffects`).
- Sin `async void` ni hilos: WebGL no los soporta bien. Usa corrutinas.
- Sin valores mágicos repartidos: los ajustes de diseño van en ScriptableObjects o campos serializados.

## Sistemas que ya existen (úsalos, no los dupliques)

| Necesitas | Usa |
| --- | --- |
| Cambiar de escena, reiniciar, ir al menú | `LevelFlowManager` (`GoToNextLevel`, `RestartLevel`, `BackToMenu`, `CurrentLevel`) |
| Datos del nivel actual (spawns, patrulla, jefe activo) | `LevelContext.Current` |
| Escenas, prefabs, mensajes del ticket, créditos | `GameFlowSettings` en `Resources` (`LevelFlowManager.Settings`) |
| Efecto de sonido | `Sfx.Play("id")` o `Sfx.Play("id", posicion)`, con los clips en el `SfxBank` |
| Música y ambiente por escena | `AudioDirector` + `AudioCatalog` |
| UI | `UiBuilder` (`CreateCanvas`, `CreateLabel`, `CreateButton`, `CreateSlider`, `CreatePanel`), color de acento `UiBuilder.Accent` |
| Opciones del jugador | `GameSettings` (PlayerPrefs). Opción nueva = propiedad nueva ahí + control en `SettingsScreens` |
| Viñeta y destellos de pantalla | `ScreenEffects.Flash(color, duracion)` |
| Balanceo de cámara y susto | `CameraEffects` (respeta `GameSettings.CameraEffects`) |
| Estado del jefe | `BossAI.IsChasing`, `BossAI.Awareness01`, `BossAI.EyePosition` |
| Interacciones con E | Implementar `IInteractable` (ver `SaleProduct`, `WarehouseDoor`) |

## Assets

- No borres ni edites a mano archivos `.meta`. Mover o renombrar assets solo desde el editor (o `unity` CLI con el editor abierto).
- Nombres de assets en PascalCase o con prefijo del espacio: `Pools_TileWall_Mat`.
- Texturas para WebGL: máximo 2048 px, compresión por defecto, sin mapas que no se usen.
- Todo asset de terceros se anota en `CREDITS.md` con autor, licencia y enlace.

## Antes de dar algo por terminado

- La consola del editor no tiene errores nuevos.
- Se probó en Play desde `00_Boot` (o con F2/F3 en el editor para saltar o reiniciar espacios).
- El cambio funciona con teclado y mouse, y no rompe el mando.
