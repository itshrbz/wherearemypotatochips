# Rafael · Claude Code

Bloques: **A** (look), **E** (primeros minutos), **G** (builds y página). Además haces el merge de los PR que Cristhian aprobó.

Para cada tarea, en la raíz del repo:

```text
Usa la skill potato-tarea y haz la tarea <ID> de docs/pulido/RAFAEL.md.
```

---

## Fase 0

### S1 · Subir el setup del equipo

- Sin IA. Revisa los cambios locales que ya tenías sin commit (`DefaultVolumeProfile`, los RP Assets, `ProjectSettings`, `Column_Mat`, la carpeta `Data/`): haz commit de lo que sirva o descártalo.
- En la raíz del repo, copia la configuración de Claude Code a `.claude/`:

  ```powershell
  powershell -ExecutionPolicy Bypass -File docs\pulido\setup_claude\instalar.ps1
  ```

- En una rama `polish/setup`: commit de `CLAUDE.md`, `AGENTS.md`, `.claude/`, `docs/pulido/` y el `.gitignore` (ya ignora `.claude/settings.local.json`). La carpeta `docs/pulido/setup_claude/` se puede borrar después de copiarla.
- PR a `main`, merge, y avisa al grupo que ya pueden instalar (S2).

---

## Fase 1 · Bloque A: look propio · rama `polish/look`

Skills útiles: `potato-direccion-arte`, `unity:urp-postprocessing`, `unity:craft-game-floors`, `unity:unity-cli`.

### A1 · Auditoría visual (sin cambiar nada)

- Con el editor abierto, tomar una captura desde el punto de vista del jugador en cada escena (`02_Store`, `10_Warehouse`, `11_Pools`, `12_Playground`, `01_Menu`, `90_Ending`) y guardarlas en `docs/pulido/capturas/antes/`.
- Listar qué hay hoy: perfiles de Volume y sus efectos, `Ambiente_*` y `AmbienteTools`, luces por escena (cuántas en tiempo real, cuántas con sombras), ajustes de `PC_RPAsset` y `Mobile_RPAsset`, qué calidad usa WebGL.
- Informe en `docs/pulido/notas/A1_auditoria_visual.md`: estado actual, problemas y propuesta concreta por espacio según la dirección de arte.
- **Terminado cuando:** el informe y las capturas están en el repo y Rafael aprobó la propuesta.

### A2 · Look de cámara vieja (global)

- Perfil de Volume global (`Assets/Settings/Look_Global.asset`) con grano de película, aberración cromática suave, viñeta ligera, distorsión de lente leve, saturación baja y tonemapping. Debe convivir con la viñeta de persecución de `ScreenEffects`.
- Opción `GameSettings.RetroLook` (0 a 1, por defecto 1) y un componente que ajuste el peso del Volume en tiempo real. El control en el menú de opciones va en E2.
- Revisar el costo en WebGL: si un efecto pesa mucho, se baja o se desactiva en esa plataforma.
- **Terminado cuando:** con el look en 1 la captura parece video casero de los 90, con el look en 0 se ve limpio, y la build WebGL no pierde fluidez.

### A3 · Color de cada espacio

- Ajustar `Ambiente_10_Warehouse`, `Ambiente_11_Pools`, `Ambiente_12_Playground` y los presets de `AmbienteTools` según la tabla de `potato-direccion-arte`: color dominante, niebla (color y densidad), luz ambiente, balance de blancos.
- Agregar un perfil para la tienda (`02_Store`) cálido, y uno para la réplica casi igual pero un poco más frío.
- **Depende de:** A2.
- **Terminado cuando:** las capturas de los tres espacios se distinguen entre sí a primera vista y se ven del mismo juego.

### A4 · Luces y rendimiento

- Almacén: fluorescentes con `LightFlicker` en varios ritmos y alguno que falla de verdad. Piscinas: luz que parece venir del agua, sin fuente visible. Parque: luz baja de atardecer y faroles.
- Reducir luces en tiempo real y sombras a lo que WebGL soporta; usar luz horneada o probes donde se pueda.
- Las escenas de los espacios son de Grover, pero en la Fase 1 él no las toca (está en el bloque C). Coordinar con él igual antes de guardar.
- **Depende de:** A3.
- **Terminado cuando:** cada espacio tiene su ambiente de luz y la build WebGL va fluida en la laptop más débil del equipo.

### A5 · Capturas de después y ajuste final

- Repetir las capturas de A1 en `docs/pulido/capturas/despues/` y armar una comparación en `docs/pulido/notas/A5_comparacion.md`.
- Ajustar lo que no convenza y dejar los valores finales documentados.
- **Terminado cuando:** Cristhian y el equipo ven la comparación y aprueban.

---

## Fase 2 · Bloque E: primeros 5 minutos · rama `polish/inicio`

Skills útiles: `unity:ui-ugui`, `unity:optimize-text-mesh-pro`, `unity:localize-unity-project`, `potato-convenciones`.

### E1 · Menú principal

- Fondo 3D en movimiento lento (un recorrido de cámara por la tienda o por un pasillo del almacén) con el look de A2, título con presencia y música de menú.
- Botones: Jugar, Opciones, Créditos, Salir (Salir no aparece en WebGL). Navegable con teclado, mouse y mando.
- **Terminado cuando:** el menú se ve como el de un juego terminado y funciona con los tres controles.

### E2 · Opciones completas

- Pantalla de opciones con: sensibilidad, invertir Y, campo de visión, volumen por grupo (música, ambiente, efectos; usa la API de audio que deja Alan en D2), efectos de cámara, intensidad del look (A2), subtítulos.
- Todo se guarda en `GameSettings` y se aplica sin reiniciar. Disponible desde el menú y desde la pausa.
- **Depende de:** A2 y D2.
- **Terminado cuando:** cada opción cambia algo que se nota al instante y queda guardada al cerrar el juego.

### E3 · Intro, tienda y primer susto

- Revisar el ritmo de la historia inicial y el tutorial: que se entienda qué hacer en menos de un minuto, con textos cortos y contextuales en vez de pantallas.
- Tienda más viva: sonidos, algún cliente o movimiento en la calle.
- Un primer susto antes de los 3 minutos (por ejemplo, la tienda parpadea y un cliente queda mirando fijo), sin el jefe.
- **Terminado cuando:** alguien que nunca jugó llega al almacén sin preguntar qué hacer y se sobresalta al menos una vez.

### E4 · Juice de ventas y feedback

- Ticket animado al cobrar, sonido de caja, leve temblor de cámara, transición más cuidada al cambiar de espacio.
- Indicaciones de interacción (E) más claras y consistentes en todos los objetos.
- **Terminado cuando:** cobrar se siente satisfactorio y nunca hay duda de con qué se puede interactuar.

### E5 · Subtítulos

- Subtítulos para diálogos y sonidos importantes (por ejemplo "[pasos cercanos]"), usando `DialogueBox` o un componente nuevo de UI. Se apagan desde opciones.
- **Depende de:** E2.
- **Terminado cuando:** con el sonido apagado se puede jugar y entender qué pasa.

### E6 · Español e inglés

- Usar el paquete Localization (skill `unity:localize-unity-project`) para todos los textos: UI, `GameFlowSettings` (mensajes del ticket y créditos), diálogos, tutorial y subtítulos.
- Selector de idioma en opciones. Por defecto, el idioma del sistema.
- **Depende de:** E2, E3, E5.
- **Terminado cuando:** todo el juego se puede jugar en inglés sin un solo texto en español.

### E7 · Final y créditos

- La tienda real reaparece al final con un detalle cambiado. Créditos con los nombres y roles del equipo y los assets de terceros.
- **Terminado cuando:** el final cierra la historia y los créditos coinciden con `CREDITS.md`.

---

## Fase 3 · Bloque G: builds y página · rama `polish/release`

Skills útiles: `unity:optimize-web`, `unity:unity-cli`.

### G1 · WebGL liviano

- Medir tamaño de descarga y tiempo de carga actuales. Aplicar la skill de optimización web: compresión, texturas, audio, código no usado, ajustes de calidad.
- **Terminado cuando:** la build pesa notablemente menos, carga más rápido y la mejora está anotada (antes y después) en `docs/pulido/notas/G1_webgl.md`.

### G2 · Build de Windows y script de builds

- Build de Windows además de WebGL, generadas con un comando del Unity CLI o un script de editor, para no hacerlas a mano cada vez.
- **Terminado cuando:** un solo comando deja las dos builds listas en `Builds/`.

### G3 · Página de itch

- Textos en español e inglés en `docs/itch/`: descripción corta y larga, controles, características, créditos, etiquetas.
- Lista de capturas y GIF a grabar, con la escena y el momento exacto de cada uno.
- **Terminado cuando:** con esos textos y capturas se puede rehacer la página entera.

### G4 · Tráiler y clips

- Modo de grabación para builds de desarrollo: ocultar HUD y cámara libre lenta.
- Guion del tráiler de 60 segundos con lista de tomas, y tres ideas de clips verticales cortos para redes.
- La edición del video la hace Rafael.
- **Terminado cuando:** el tráiler está editado y los clips listos para subir.

### G5 · Versión 1.0

- Build final de las dos plataformas con todo lo de `main`, después de que Cristhian pasó T5.
- Devlog en `docs/itch/devlog_1_0.md` explicando qué cambió desde la jam.
- Publicar en itch (a mano).
- **Terminado cuando:** la 1.0 está publicada con su devlog.
