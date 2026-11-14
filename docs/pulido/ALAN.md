# Alan · Codex

Bloques: **D** (audio) y **F** (anomalías). Además revisas los PR de Rafael y Grover con Codex.

Codex lee `AGENTS.md` solo, pero no las skills. Para cada tarea, en la raíz del repo:

```text
Lee AGENTS.md y .claude/skills/potato-tarea/SKILL.md, y haz la tarea <ID> de docs/pulido/ALAN.md.
```

Trabaja con Codex CLI en tu computadora y Unity abierto, así Codex puede probar con el Unity CLI. Las tareas marcadas **(nube)** también se pueden mandar a Codex en la nube, porque son solo código; quedan "Para probar" hasta que las abras en Unity.

Codex no puede crear bien assets de Unity sin el editor (mixers, prefabs, `.meta`). Si trabaja sin editor, que escriba el código y te deje los pasos para crear el asset a mano.

---

## Fase 0

### S2 · Instalar

Sigue [01_INSTALACION.md](01_INSTALACION.md), secciones 1 y 3. Comprueba que `unity status` dice `ready` y que `codex plugin list` muestra `unity`.

---

## Fase 1 · Bloque D: audio · rama `polish/audio`

Lecturas para Codex: `.claude/skills/potato-direccion-arte/SKILL.md` (sección Sonido) y `.claude/skills/potato-convenciones/SKILL.md`. Skills de Unity útiles: `audio-setup-mixers`, `optimize-audio`.

### D1 · Lista de sonidos que faltan **(nube)**

- Revisar `08_Audio`, `AudioCatalog`, `SfxBank` y dónde se llama a `Sfx.Play` en el código.
- Escribir `docs/pulido/notas/D1_sonidos.md`: por espacio y por sistema (jefe, jugador, tienda, UI), qué sonidos faltan, con nombre de archivo propuesto, id para el `SfxBank` y términos de búsqueda en inglés para Freesound. Incluir `boss_warn`, que pide Grover para C3.
- Alan descarga los sonidos (solo CC0 o con licencia que lo permita), los importa a `08_Audio/` y los anota en `CREDITS.md`.
- **Terminado cuando:** la lista está y los sonidos más importantes ya están en el proyecto.

### D2 · Mixer y volumen por grupo

- AudioMixer `Assets/08_Audio/Mixer/Main.mixer` con grupos Música, Ambiente, Efectos, Jefe e Interfaz, y snapshots Normal, Persecución, Escondido y Pausa. El asset se crea en el editor.
- Clase estática `AudioMix` con `SetVolume(grupo, 0..1)` y `GetVolume(grupo)` que guarda en PlayerPrefs y convierte a decibeles. Rafael la usa en E2 para el menú de opciones.
- **Terminado cuando:** se puede cambiar el volumen de cada grupo por código y queda guardado.

### D3 · Todo pasa por el mixer **(nube)**

- Asignar el grupo correcto a cada `AudioSource`: `AudioDirector`, `Sfx`, `BossAudio`, `PlayerFootsteps`, `ProximityMusic` y cualquier otro. Las referencias a los grupos van en `AudioCatalog` o `SfxBank`, no buscadas por nombre.
- **Depende de:** D2.
- **Terminado cuando:** bajar un grupo en el mixer baja todos sus sonidos y ninguno queda fuera.

### D4 · Snapshots según lo que pasa

- `AudioDirector` cambia de snapshot con transiciones suaves: Persecución cuando `BossAI.IsChasing`, Escondido cuando el jugador está en un escondite, Pausa con filtro de graves cuando el juego está en pausa (detectarlo sin tocar `PauseMenu`, que es de Rafael).
- La música de tensión entra por capas, no de golpe.
- **Depende de:** D3.
- **Terminado cuando:** con audífonos se nota cuándo empieza una persecución antes de ver al jefe, y esconderse suena distinto.

### D5 · Ambiente por capas y reverb **(nube)**

- Componente `AmbientOneShots`: sonidos lejanos al azar alrededor del jugador, con intervalo y lista configurables por escena en `AudioCatalog`.
- Reverb distinta por espacio creada por código desde `AudioDirector` según el nombre de la escena (así no hay que tocar las escenas de Grover).
- **Terminado cuando:** cada espacio suena distinto con los ojos cerrados.

### D6 · Pasos según el suelo **(nube)**

- `PlayerFootsteps` detecta la superficie (material, etiqueta o capa) y usa el grupo de clips correspondiente del `SfxBank`: alfombra, azulejo, agua, pasto, concreto. Sonidos de salto y aterrizaje.
- Avisar a Grover qué etiqueta o capa tiene que llevar cada suelo cuando vista los niveles (B2 a B4).
- **Terminado cuando:** caminar por cada tipo de suelo suena distinto.

---

## Fase 2 · Bloque F: anomalías (opcional) · rama `polish/anomalias`

Solo empieza si D pasó la Puerta 1. Si no, sigue con bugs de D.

Idea: cada vez que llegas a la tienda réplica, algo está distinto. Si lo notas, ganas una pista o una ventaja. Refuerza el tema "Something's Off" y da clips para redes.

### F1 · Sistema base **(nube)**

- `Assets/01_Scripts/Anomalies/`: ScriptableObject `AnomalyDefinition` (id, descripción, peso, desde qué espacio puede salir), clase base `Anomaly` con `Apply()` y `Revert()`, y `AnomalyManager` que en cada visita elige una o dos según `LevelFlowManager.CurrentLevel`.
- El manager se agrega al prefab de la tienda réplica (`GameFlowSettings.replicaStorePrefab`) como componente nuevo, sin cambiar lo que ya hay. Avisar a Rafael en `PROGRESO.md`.
- **Terminado cuando:** el sistema elige y aplica anomalías de prueba en la réplica.

### F2 · Entre 6 y 8 anomalías concretas

- Ideas: un producto al revés o flotando, un cartel con el texto cambiado, el reloj marcando otra hora, una luz de otro color, un objeto duplicado, un cliente que no deja de mirar, una sombra sin dueño, el letrero de la tienda con una letra mal.
- Cada una es sutil pero se puede encontrar si miras bien.
- **Depende de:** F1.
- **Terminado cuando:** cada visita a la réplica tiene al menos una anomalía distinta.

### F3 · Notar la anomalía

- El jugador la "marca" mirándola de cerca e interactuando (E). Si acierta: una pista o una ventaja pequeña en el siguiente espacio (por ejemplo, el jefe tarda más en aparecer). Si se equivoca, no pasa nada.
- Mostrar al final cuántas anomalías encontró.
- **Depende de:** F2.
- **Terminado cuando:** un jugador que no sabe del sistema descubre solo que puede marcar anomalías.

---

## Fase 3

- Bugs de D y F en `BUGS.md`.
- Revisar con Codex los PR de Rafael y Grover antes de que Cristhian los pruebe: revisión de código buscando errores, cosas que no funcionan en WebGL y archivos tocados fuera del bloque.
