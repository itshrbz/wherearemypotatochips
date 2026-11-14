# Where Are My Potato Chips? — reglas para la IA

Juego de terror liminal en primera persona. Unity 6.5 (6000.5.5f1), URP, Input System, AI Navigation, Animation Rigging, ProBuilder, TextMesh Pro. El proyecto de Unity está en `Where_are_my_potato_chips/`. Estamos en la etapa de pulido post-jam: el objetivo es que el juego se vea, suene y se sienta terminado, no agregar contenido.

Antes de cualquier tarea lee `docs/pulido/00_LEEME.md` y la tarea que te toca en el archivo de la persona (`RAFAEL.md`, `GROVER.md`, `ALAN.md`).

## Cómo se trabaja

- Cada tarea del plan tiene un ID (por ejemplo `A2`). Usa la skill `potato-tarea` para ejecutarla: rama, plan, commits y registro en `docs/pulido/PROGRESO.md`.
- Una rama por bloque: `polish/<bloque>` (por ejemplo `polish/look`). Nunca hagas commit en `main` ni hagas `git push --force`.
- Primero propón un plan corto y espera el visto bueno. Después, una tarea pequeña por commit.
- Al terminar cada tarea, escribe en `PROGRESO.md` qué cambió y cómo probarlo, para que Cristhian lo pruebe.
- Si algo no está en tu bloque, no lo toques: anótalo en `docs/pulido/BUGS.md` o en `PROGRESO.md` como pendiente para su dueño.

## Dueños de archivos

| Bloque | Dueño | Puede tocar |
| --- | --- | --- |
| A. Look | Rafael | `Assets/Settings/`, perfiles de Volume, luces y post-proceso de las escenas, `Assets/01_Scripts/Shared/LightFlicker.cs` |
| B. Niveles y arte | Grover | Escenas `10_Warehouse`, `11_Pools`, `12_Playground`, `Assets/03_Materials`, `04_Models`, `05_Textures`, `Assets/Editor/*Builder.cs`, `NivelTools.cs`, `AmbienteTools.cs` |
| C. Jefe | Grover | `Assets/01_Scripts/Enemy/`, prefab del jefe, `HidingSpot.cs`, `HidingSpotGenerator.cs`, `SaltosJefeTool.cs` |
| D. Audio | Alan | `Assets/08_Audio/`, `Assets/01_Scripts/Audio/`, el AudioMixer, prefabs de audio |
| E. Primeros minutos | Rafael | `01_Menu`, `02_Store`, `90_Ending`, `Assets/01_Scripts/UI/`, `Sale/`, `Flow/`, `Player/`, `Shared/GameSettings.cs` |
| F. Anomalías | Alan | `Assets/01_Scripts/Anomalies/` (nuevo), `Assets/07_data/Anomalies/` (nuevo) |
| G. Builds y página | Rafael | `ProjectSettings/`, `Builds/`, `docs/itch/` |

Las escenas se fusionan mal en git. Si necesitas algo en una escena que no es tuya, hazlo en un prefab o pídeselo al dueño en `PROGRESO.md`.

## Unity

- Si el editor está abierto, contrólalo con el Unity CLI (skill `unity:unity-cli`): `unity status` primero. No edites a mano archivos `.unity`, `.prefab` o `.asset` si puedes hacerlo desde el editor.
- Nunca borres ni regeneres archivos `.meta`. Si mueves un asset, hazlo desde el editor.
- Después de cambiar código, revisa la consola del editor: cero errores nuevos.
- No toques `Library/`, `Temp/`, `Logs/`, `UserSettings/` ni `Builds/` (salvo G).
- Las herramientas del equipo están en el menú `Tools/Kit/` (generadores de niveles, ambiente, spawns, NavMesh). Úsalas en vez de rehacerlas.

## Código

Lee la skill `potato-convenciones` antes de escribir C#. Lo más importante:

- Identificadores en inglés, comentarios y textos para el jugador en español.
- Comentarios cortos que expliquen el porqué, no el qué. `/// <summary>` en las clases públicas.
- La UI se arma por código con `UiBuilder`. Las opciones del jugador van en `GameSettings` (PlayerPrefs).
- Datos editables en ScriptableObjects (`GameFlowSettings`, `SaleVariant`, `AudioCatalog`, `SfxBank`), no valores mágicos en el código.
- Todo tiene que funcionar en WebGL: nada de hilos, archivos locales ni APIs de escritorio.

## Dirección de arte

Lee la skill `potato-direccion-arte` antes de tocar luz, color, materiales o sonido.

## Cuando termines

1. Consola sin errores y la escena afectada probada en Play.
2. Commit con mensaje en español: `A2: look de cámara vieja en el perfil global`.
3. Entrada en `PROGRESO.md` con la checklist de prueba (skill `potato-playtest`).
4. Avisa qué falta o qué quedó a medias. No digas que algo funciona si no lo probaste.
