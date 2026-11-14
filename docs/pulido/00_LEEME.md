# Plan de pulido · Where Are My Potato Chips?

El código del jam funciona. Lo que todavía se ve de jam es el arte, el audio y los primeros minutos. Este plan reparte el pulido en bloques grandes para que las IAs hagan el trabajo y las personas decidan y prueben.

## Quién hace qué

| Persona | Herramienta | Bloques | Archivo con sus tareas |
| --- | --- | --- | --- |
| Rafael | Claude Code + plugin de Unity | A (look), E (primeros minutos), G (builds y página) | [RAFAEL.md](RAFAEL.md) |
| Grover | Claude Code + plugin de Unity | C (jefe y tensión), B (niveles y arte) | [GROVER.md](GROVER.md) |
| Alan | Codex + plugin de Unity | D (audio), F (anomalías) | [ALAN.md](ALAN.md) |
| Cristhian | Tester, sin IA | Prueba todo, lleva los bugs, decide las puertas | [CRISTHIAN.md](CRISTHIAN.md) |

Instalación de todo: [01_INSTALACION.md](01_INSTALACION.md). Estado de cada tarea: [PROGRESO.md](PROGRESO.md). Bugs: [BUGS.md](BUGS.md). Por qué estas skills y plugins: [SKILLS_Y_PLUGINS.md](SKILLS_Y_PLUGINS.md).

## Orden de trabajo

Las fases se hacen en orden. Dentro de una fase, cada persona sigue su lista de arriba a abajo y las tres trabajan al mismo tiempo sin pisarse. Ninguna fase empieza sin pasar la puerta de la anterior.

| Fase | Rafael | Grover | Alan | Cristhian | Puerta para pasar |
| --- | --- | --- | --- | --- | --- |
| **0 · Preparar** (1–2 días) | S1 subir este setup | S2 instalar | S2 instalar | T0 instalar, T1 playtest base | Todos tienen Claude Code o Codex funcionando con `unity status` en `ready`, y `BUGS.md` tiene la lista inicial |
| **1 · Base** (semana 1) | A1 → A5 look | C1 → C6 jefe | D1 → D6 audio | T2 prueba cada tarea | **Puerta 1:** build jugable con el look y el audio nuevos; Cristhian la juega entera sin bugs bloqueantes |
| **2 · Experiencia** (semana 2) | E1 → E7 primeros minutos | B1 → B5 niveles y arte | F1 → F3 anomalías (si D está cerrado) | T2 prueba cada tarea, T3 puerta | **Puerta 2:** cinco personas de afuera lo juegan completo (T4) |
| **3 · Lanzar** (semana 3) | G1 → G5 builds y página | Bugs de C y B | Bugs de D y F, revisar PR | T4 playtest externo, T5 prueba final | **Puerta 3:** versión 1.0 publicada en itch |

## Cómo pedirle una tarea a la IA

Abre la terminal en la raíz del repo y ejecuta `claude` (Rafael y Grover) o `codex` (Alan). Luego:

```text
Usa la skill potato-tarea y haz la tarea A2 de docs/pulido/RAFAEL.md.
```

O, si ya vienes siguiendo el orden:

```text
Usa la skill potato-tarea y sigue con mi siguiente tarea pendiente (soy Grover).
```

Codex no carga las skills solo; para Alan:

```text
Lee AGENTS.md y .claude/skills/potato-tarea/SKILL.md, y haz la tarea D2 de docs/pulido/ALAN.md.
```

La IA primero propone un plan. Revísalo, corrígelo si hace falta y dile que siga. Cada paso termina en un commit.

### Tareas largas sin supervisión

Para una tarea grande y clara (por ejemplo vestir un nivel completo), Rafael y Grover pueden dejar a Claude Code trabajando solo con el plugin Ralph Loop, que lo hace repetir hasta cumplir el criterio:

```text
/ralph-loop "Usa la skill potato-tarea y haz la tarea B2 de docs/pulido/GROVER.md sin pedirme confirmación. Cuando se cumpla su criterio de terminado y PROGRESO.md esté actualizado, escribe <promise>LISTO</promise>." --completion-promise "LISTO" --max-iterations 20
```

Úsalo solo en tareas que ya tengan el plan aprobado, y revisa los commits al volver.

## Cómo se integra el trabajo

1. La IA trabaja en la rama del bloque (`polish/look`, `polish/jefe`, `polish/audio`, etc.) y deja la tarea "Para probar" en `PROGRESO.md` con su checklist.
2. La persona hace `git push` y abre un PR hacia `main`.
3. La otra IA revisa el PR: Claude Code con `/code-review` o el toolkit de revisión, Codex con su revisión de PR. Lo de Claude lo revisa Codex y al revés.
4. Cristhian prueba con la checklist. Si pasa, la marca "Hecha"; si no, abre bugs.
5. Rafael hace el merge. Nadie hace merge de lo que Cristhian no probó.

## Reglas que evitan perder trabajo

- Una rama por bloque, nunca commits directos en `main`.
- Cada escena tiene un dueño (tabla en `CLAUDE.md`). Lo que se necesite en una escena ajena va en un prefab o se pide al dueño.
- Antes de empezar el día: `git switch main && git pull`, y luego `git merge main` en tu rama.
- Si git marca conflicto en una escena, no se resuelve a mano: avisa en el grupo.
