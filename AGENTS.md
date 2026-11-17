# Where Are My Potato Chips? — reglas para Codex

Este archivo es para Codex. Las reglas son las mismas que usa Claude Code en `CLAUDE.md`: **léelo completo antes de empezar**, junto con `docs/pulido/00_LEEME.md` y tu tarea en `docs/pulido/ALAN.md`.

Codex no carga automáticamente las skills de `.claude/skills/`, así que léelas como documentación cuando la tarea lo pida:

- `.claude/skills/potato-convenciones/SKILL.md` — estilo de C# y estructura del proyecto. Léela siempre antes de escribir código.
- `.claude/skills/potato-tarea/SKILL.md` — cómo ejecutar una tarea del plan (rama, plan, commits, registro).
- `.claude/skills/potato-direccion-arte/SKILL.md` — tono visual y sonoro de cada espacio.
- `.claude/skills/potato-playtest/SKILL.md` — cómo escribir la checklist de prueba al terminar.
- `.claude/skills/potato-escenas/SKILL.md` — cómo tocar escenas sin romper el trabajo de otros.

## Resumen de lo obligatorio

- Unity 6.5 URP, proyecto en `Where_are_my_potato_chips/`. Debe funcionar en WebGL.
- Rama `polish/<bloque>`. Nunca commit en `main` ni `push --force`.
- Primero un plan corto y espera aprobación. Luego una tarea por commit, mensaje en español con el ID (`D1: mixer con grupos y snapshots`).
- Solo toca los archivos de tu bloque (tabla de dueños en `CLAUDE.md`). Alan es dueño de D (audio) y F (anomalías).
- Identificadores en inglés, comentarios en español. No borres ni inventes archivos `.meta`: si creas un asset o script nuevo, Unity genera el `.meta` al abrir el proyecto; deja anotado en `PROGRESO.md` que hay que abrir Unity para generarlo.
- Si trabajas en la nube sin Unity, no puedes probar en el editor: deja instrucciones exactas de prueba en `PROGRESO.md` y marca la tarea como "Para probar", no como "Hecha".
- Si tienes el plugin de Unity y el editor abierto (Codex CLI en la máquina de Alan), usa `unity status` y el Unity CLI para probar.
