---
name: potato-escenas
description: Cómo modificar escenas, prefabs e iluminación de Where Are My Potato Chips sin romper el trabajo de otros. Usar antes de abrir o cambiar cualquier .unity o .prefab.
---

# Tocar escenas sin romper nada

## Regla de dueños

Cada escena tiene un dueño (tabla en `CLAUDE.md`). Solo el dueño la guarda.

| Escena | Dueño |
| --- | --- |
| `00_Boot`, `01_Menu`, `02_Store`, `90_Ending` | Rafael |
| `10_Warehouse`, `11_Pools`, `12_Playground` | Grover |

Si tu tarea necesita algo en una escena ajena:

1. Hazlo en un **prefab** que el dueño solo tenga que arrastrar, o en un componente que se agregue en tiempo de ejecución (por ejemplo desde `LevelContext` o `AudioDirector` según el nombre de la escena).
2. Anota en `docs/pulido/PROGRESO.md`: "Pedido para <dueño>: poner el prefab X en la escena Y".

Rafael (bloque A) puede ajustar luces y Volumes de las escenas de Grover solo en tareas acordadas, y nunca al mismo tiempo que Grover tiene esa escena abierta en su rama.

## Con el editor abierto

- `unity status` debe estar en `ready`. Busca el comando antes de inventarlo: `unity command --query <palabra>`.
- Abre la escena, haz el cambio, guarda la escena, y revisa con `git diff --stat` que solo cambió lo esperado.
- Si el diff de una escena tiene miles de líneas por un cambio pequeño, probablemente se re-serializó entera: avisa antes de hacer commit.
- Las herramientas del menú `Tools/Kit/` (ambiente, spawns, NavMesh, generadores) se pueden ejecutar desde el CLI. Úsalas en vez de repetir su trabajo.

## Prefabs antes que escenas

- Cambios de luz, audio o UI que se repiten en varios espacios van en prefabs o ScriptableObjects.
- Usa variantes de prefab para diferencias por espacio, no copias.
- No rompas conexiones de prefab en las escenas (nada de "Unpack" salvo que la tarea lo diga).

## Iluminación y NavMesh

- Después de mover geometría: re-hornear la NavMesh del espacio (`Tools/Kit/Nivel/5. Agregar NavMesh Surface` o el botón Bake del NavMeshSurface) y revisar los saltos del jefe (`Tools/Kit/Generar saltos del jefe`).
- Las luces horneadas generan archivos pesados en `00_Scenes/<escena>/`. Hornea solo cuando la tarea lo pida y en la rama del dueño de la escena.

## Conflictos

Si git marca conflicto en una escena, no lo resuelvas a mano. Para y avisa: se resuelve con UnityYAMLMerge (ver `docs/pulido/01_INSTALACION.md`) o rehaciendo el cambio más pequeño de los dos.
