# Skills y plugins

Qué instalamos, para qué, y qué se descartó. La instalación está en [01_INSTALACION.md](01_INSTALACION.md).

## Instalados

| Nombre | Para | De dónde | Qué aporta |
| --- | --- | --- | --- |
| **Plugin oficial de Unity** (`unity@unity-agent-plugin`) | Claude Code y Codex | Unity Technologies | Más de 30 skills de Unity. La clave es `unity-cli`: controla el editor abierto (crear objetos, editar escenas, ejecutar C#, builds, logs) usando el paquete `com.unity.pipeline` que ya tiene el proyecto. |
| **commit-commands** | Claude Code | Anthropic (oficial) | `/commit` arma el commit y el mensaje; también push y PR cuando se lo pidas. |
| **code-review** | Claude Code | Anthropic (oficial) | `/code-review` revisa un PR con varios agentes y respeta las reglas de `CLAUDE.md`. |
| **pr-review-toolkit** | Claude Code | Anthropic (oficial) | Agentes de revisión especializados (errores silenciosos, comentarios, simplificación). |
| **ralph-loop** | Claude Code | Anthropic (oficial) | Deja a Claude repitiendo una tarea larga hasta cumplir su criterio de terminado, sin supervisión. |
| **csharp-lsp** (opcional) | Claude Code | Anthropic (oficial) | Le da a Claude el analizador de C#: ve errores y referencias sin compilar en Unity. Necesita .NET SDK y `csharp-ls`. |

## Skills del equipo (en `.claude/skills/`)

Escritas para este proyecto. Claude Code las carga solo; Codex las lee porque `AGENTS.md` se lo indica.

| Skill | Cuándo se usa |
| --- | --- |
| `potato-convenciones` | Antes de escribir código: estilo, carpetas y qué sistemas ya existen. |
| `potato-tarea` | Para ejecutar cualquier tarea del plan: rama, plan, commits, registro en `PROGRESO.md`. |
| `potato-escenas` | Antes de tocar escenas o prefabs: dueños, prefabs primero, conflictos. |
| `potato-direccion-arte` | Antes de tocar luz, color, materiales o sonido: el tono de cada espacio. |
| `potato-playtest` | Al terminar una tarea (checklist para Cristhian) y al registrar bugs. |

Las skills de Unity más útiles para este plan: `unity-cli`, `urp-postprocessing`, `craft-game-floors`, `audio-setup-mixers`, `optimize-audio`, `initialize-ai-navigation`, `fix-3d-physics-collisions`, `ui-ugui`, `optimize-text-mesh-pro`, `localize-unity-project`, `optimize-web`.

## Alternativa, por si el Unity CLI falla

**MCP for Unity** de Coplay es un puente gratuito (licencia MIT) entre el editor y la IA, con herramientas para escenas, prefabs, scripts, consola y tests. No lo instalamos porque el plugin oficial ya hace lo mismo con el Unity CLI, y la skill `unity-cli` recomienda el CLI cuando hay terminal. Úsalo solo si `unity status` no logra conectar en alguna computadora.

## Descartados

En los registros públicos de skills hay varias "unity-developer" o "unity-development" hechas por la comunidad. Las descartamos: son genéricas, repiten lo que ya cubre el plugin oficial y no conocen nuestro proyecto. Además, una skill es un conjunto de instrucciones que la IA sigue al pie de la letra, así que solo conviene instalarlas de fuentes de confianza.

## Fuentes

- [Plugin oficial de Unity para Claude Code (blog de Unity)](https://unity.com/blog/unity-plugin-for-claude-code)
- [Documentación del plugin de Unity para Claude Code](https://docs.unity.com/en-us/ai/unity-plugin/claude-code)
- [Documentación del plugin de Unity para Codex](https://docs.unity.com/en-us/ai/unity-plugin/codex)
- [Repositorio unity-agent-plugin](https://github.com/Unity-Technologies/unity-agent-plugin)
- [Instalar el Unity CLI](https://docs.unity.com/en-us/unity-cli/use-unity-cli)
- [Plugins oficiales de Anthropic para Claude Code](https://github.com/anthropics/claude-plugins-official)
- [MCP for Unity de Coplay](https://github.com/CoplayDev/unity-mcp)
- [Ejemplo de skill genérica de la comunidad (descartada)](https://tessl.io/registry/skills/github/popey/claude-code-skills/unity-developer)
