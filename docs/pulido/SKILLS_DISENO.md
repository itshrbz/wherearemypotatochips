# Skills de diseño

Estas 14 skills le enseñan a Claude Code a diseñar, no solo a programar: arte, luz, niveles, sensación de juego, sonido, interfaz, miedo y ritmo. Doce vienen de repositorios de código abierto que revisamos uno por uno, y dos (`potato-revision-visual` y `potato-terror`) están hechas para este juego.

## Instalar

Desde la raíz del repo, en PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File _skills_diseno\instalar.ps1
```

El script copia las skills a `.claude/skills/` y este archivo a `docs/pulido/SKILLS_DISENO.md`. Después se puede borrar la carpeta `_skills_diseno`. Reinicia Claude Code para que las cargue.

Hazlo cuando no haya otra sesión de Claude Code trabajando, y súbelo en su propia rama:

```powershell
git switch main
git pull
git switch -c polish/skills-diseno
powershell -ExecutionPolicy Bypass -File _skills_diseno\instalar.ps1
git add .claude/skills docs/pulido/SKILLS_DISENO.md
git commit -m "Skills de diseño: arte, niveles, game feel, audio, UI, terror y revisión con capturas"
git push -u origin polish/skills-diseno
```

## Qué hay

| Skill | Para qué | Origen |
| --- | --- | --- |
| `potato-revision-visual` | Ciclo de rediseño: captura, crítica con rúbrica, 3 cambios, captura de nuevo y comparación lado a lado | Propia |
| `potato-terror` | Curva de tensión, presupuesto de sustos, lo liminal y un jefe justo | Propia |
| `game-feel` | Juice: temblor de cámara, pausas de impacto, easing, flashes, con opciones de accesibilidad | gamedev-skills (Apache 2.0) |
| `level-design` | Recorridos, ritmo, cómo guiar al jugador, blockout antes de decorar | gamedev-skills (Apache 2.0) |
| `audio-design` | Mezcla, grupos, música adaptativa, variación de sonidos | gamedev-skills (Apache 2.0) |
| `game-ui-ux` | Menús, HUD, foco con teclado y mando, accesibilidad | gamedev-skills (Apache 2.0) |
| `camera-systems` | Cámara en primera persona, balanceo, encuadre | gamedev-skills (Apache 2.0) |
| `create-game-assets` | Guía de estilo, texturas coherentes, control de licencias | gamedev-skills (Apache 2.0) |
| `itch-publish` | Publicar en itch, página y builds (sirve para G3 a G5) | gamedev-skills (Apache 2.0) |
| `unity-lighting-vfx` | Luz horneada, probes, APV, partículas y post-proceso en Unity 6 | Nice-Wolf-Studio (MIT) |
| `aesthetic-direction-framework` | Identidad visual, sonora y de sensación de juego | claude-game-design-suite (MIT) |
| `player-experience-modeler` | Qué debe sentir el jugador en cada momento | claude-game-design-suite (MIT) |
| `level-encounter-planner` | Planificar encuentros y su ritmo dentro de un nivel | claude-game-design-suite (MIT) |
| `playtest-protocol-designer` | Diseñar playtests que den datos útiles (para T4) | claude-game-design-suite (MIT) |

Cada skill de terceros lleva su `LICENSE` (y `NOTICE` en las de Apache) dentro de su carpeta, como piden sus licencias. No se modificó su contenido.

## Cómo pedir un rediseño

```text
Usa las skills potato-revision-visual, potato-direccion-arte y potato-terror. Rediseña <escena o pantalla>: captura el antes, critícalo con la rúbrica, aplica los 3 cambios de más impacto, captura el después y muéstrame la comparación. Respeta los dueños de escenas de CLAUDE.md.
```

Para la sensación de juego:

```text
Usa las skills game-feel y potato-convenciones. Revisa las ventas, saltos, aterrizajes y capturas del jefe, y agrega feedback proporcional a cada evento, con opción para reducir temblor y destellos.
```

## Qué descartamos y por qué

- **Claude Code Game Studios** (78 skills): es un estudio completo con sprints, épicas y agentes por rol. Es demasiado proceso para cuatro personas y choca con nuestro plan.
- **Skills genéricas de Unity** de varios repos: repiten lo que ya cubre el plugin oficial de Unity.
- **Skills de géneros que no son el nuestro** (plataformas, RPG, cartas) y de otros motores.

## Fuentes

- [gamedev-skills/awesome-gamedev-agent-skills](https://github.com/gamedev-skills/awesome-gamedev-agent-skills)
- [baxatron-git/claude-game-design-suite](https://github.com/baxatron-git/claude-game-design-suite)
- [Nice-Wolf-Studio/unity-claude-skills](https://github.com/Nice-Wolf-Studio/unity-claude-skills)
- [Donchitos/Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) (descartado)
