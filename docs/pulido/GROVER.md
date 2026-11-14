# Grover · Claude Code

Bloques: **C** (jefe y tensión) y **B** (niveles y arte). Tienes menos horas de uso con el plan Pro: guarda las sesiones para lo que pide razonar y haz los ajustes de números a mano en el Inspector.

Para cada tarea, en la raíz del repo:

```text
Usa la skill potato-tarea y haz la tarea <ID> de docs/pulido/GROVER.md.
```

---

## Fase 0

### S2 · Instalar

Sigue [01_INSTALACION.md](01_INSTALACION.md), secciones 1 y 2. Comprueba que `unity status` dice `ready` y que `/unity:` y `/potato` muestran las skills.

---

## Fase 1 · Bloque C: jefe y tensión · rama `polish/jefe`

Skills útiles: `unity:initialize-ai-navigation`, `unity:fix-3d-physics-collisions`, `potato-convenciones`.

### C1 · Auditoría del jefe (sin cambiar nada)

- Leer todo `01_Scripts/Enemy/` (sobre todo `BossAI` y `EnemyStateMachine`), `HidingSpot`, `HidingSpotGenerator` y `LevelContext`.
- Documentar en `docs/pulido/notas/C1_jefe.md`: estados y transiciones (diagrama Mermaid), qué lo hace ver u oír al jugador, valores actuales de velocidad y percepción por espacio, y los problemas conocidos o probables (atascos, ver a través de paredes, persecuciones imposibles de escapar).
- Proponer los cambios de C2 a C6 con prioridad.
- **Terminado cuando:** el informe está en el repo y Grover aprobó las prioridades.

### C2 · Dificultad por espacio en datos

- Crear un ScriptableObject `BossTuning` (velocidad, alcance de vista y oído, tiempo de búsqueda, enfriamiento entre persecuciones) con un asset por espacio en `Assets/07_data/Boss/`.
- Reemplazar los valores fijos actuales (por ejemplo `LevelContext.BossSpeedMultiplier`) por esos assets. `LevelContext` es de Rafael: cambia solo esas líneas y anótalo en `PROGRESO.md`.
- Curva: el almacén enseña, las piscinas presionan, el parque aprieta.
- **Terminado cuando:** se ajusta la dificultad de cada espacio solo desde el Inspector.

### C3 · Avisos antes del ataque

- Señales claras de que el jefe está cerca y de que te vio: luces cercanas que fallan (`LightFlicker`), sonido de aviso (pídele a Alan un id en el `SfxBank`, por ejemplo `boss_warn`), viñeta que ya existe en `ScreenEffects`.
- Que el jugador siempre tenga un momento para reaccionar antes de ser atrapado.
- **Terminado cuando:** nadie es atrapado sin haber tenido un aviso que se ve o se oye.

### C4 · Escondites justos

- Al esconderse: feedback claro (encuadre, respiración, indicador sutil). Reglas entendibles: el jefe solo revisa un escondite si te vio entrar o hiciste ruido.
- Revisar que `HidingSpotGenerator` deje escondites en lugares útiles de cada espacio.
- **Terminado cuando:** esconderse funciona siempre que el jugador lo hace a tiempo, y él entiende por qué falló cuando falla.

### C5 · Muerte y reinicio rápido

- Mantener el susto de captura (`CameraEffects.Screamer`), pero que el reinicio tarde menos de 3 segundos y no repita textos largos.
- Mensajes de reintento variados y cortos.
- **Terminado cuando:** morir da ganas de reintentar, no de cerrar el juego.

### C6 · Navegación y atascos

- Revisar la NavMesh y los saltos del jefe en los tres espacios (`Tools/Kit/Generar saltos del jefe`).
- Detectar cuando el jefe se queda atascado y recolocarlo fuera de la vista del jugador.
- **Terminado cuando:** en 10 minutos de juego por espacio el jefe no se queda trabado ni atraviesa paredes.

---

## Fase 2 · Bloque B: niveles y arte · rama `polish/niveles`

Skills útiles: `potato-direccion-arte`, `potato-escenas`, `unity:craft-game-floors`, `unity:unity-cli`.

Antes de empezar, haz `git merge main` para tener el look de Rafael (A2 a A4) en tu rama.

### B1 · Kit de materiales

- Grover descarga texturas CC0 de ambientCG o Poly Haven (azulejo, alfombra, concreto, cerámica de piscina, pasto seco, metal pintado). Claude las importa con tamaños aptos para WebGL y crea los materiales URP en `Assets/03_Materials/Kit/`.
- Calcomanías: manchas de humedad, carteles, señales de salida, números de pasillo.
- Anotar cada textura en `CREDITS.md`.
- **Terminado cuando:** hay un kit de al menos 12 materiales y 8 calcomanías listos para usar.

### B2 · Vestir el almacén

- Reemplazar el gris con el kit, agregar objetos (estantes, cajas, extintores, enchufes, carteles) y de 3 a 5 detalles que "están mal" según `potato-direccion-arte`.
- Extender `AlmacenBuilder` para repartir decoración de forma automática donde tenga sentido.
- Re-hornear NavMesh y revisar saltos del jefe.
- **Terminado cuando:** desde la vista del jugador no queda ninguna pared gris lisa y el jefe sigue navegando bien.

### B3 · Vestir las piscinas

- Igual que B2 para `11_Pools`: azulejo, escaleras, toboganes, vestuarios, agua, y sus detalles que están mal.
- **Terminado cuando:** igual que B2.

### B4 · Vestir el parque

- Igual que B2 para `12_Playground` (extender `ParqueBuilder`): juegos, bancos, faroles, cercas, pasto, y sus detalles que están mal.
- **Terminado cuando:** igual que B2.

### B5 · Rendimiento de los niveles

- Marcar objetos estáticos, revisar oclusión, combinar mallas o materiales repetidos, y medir en la build WebGL.
- **Terminado cuando:** los tres espacios van fluidos en WebGL en la laptop más débil del equipo.

---

## Fase 3

Arreglar los bugs de los bloques C y B que salgan del playtest externo (`BUGS.md`, columna Bloque). Para pedirlo:

```text
Usa la skill potato-tarea. Arregla los bugs abiertos del bloque C en docs/pulido/BUGS.md, de mayor a menor gravedad.
```
