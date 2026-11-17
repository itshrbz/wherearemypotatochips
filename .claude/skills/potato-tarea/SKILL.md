---
name: potato-tarea
description: Flujo para ejecutar una tarea del plan de pulido (IDs como A2, C3, D1). Usar cuando el usuario pida "haz la tarea X", "sigue con mi siguiente tarea" o pegue un prompt de docs/pulido.
---

# Ejecutar una tarea del plan de pulido

## 1. Ubicar la tarea

- Lee `docs/pulido/00_LEEME.md` y el archivo de la persona (`RAFAEL.md`, `GROVER.md` o `ALAN.md`).
- Si el usuario dice "mi siguiente tarea", busca en `docs/pulido/PROGRESO.md` la primera tarea de esa persona que no esté "Hecha" y cuyas dependencias sí lo estén.
- Si una dependencia no está hecha, dilo y no empieces.

## 2. Preparar

```bash
git status            # tiene que estar limpio; si no, pregunta qué hacer con los cambios
git switch main && git pull
git switch -c polish/<bloque>   # o git switch polish/<bloque> si ya existe
unity status          # si el editor está abierto, debe decir "ready"
```

Marca la tarea como "En curso" en `PROGRESO.md`.

## 3. Planear

- Lee el código y los assets que la tarea menciona antes de proponer nada.
- Propón un plan de 3 a 8 pasos, cada uno pequeño y verificable. Di qué archivos vas a tocar.
- Si un paso necesita un archivo de otro bloque (tabla de dueños en `CLAUDE.md`), propón una alternativa (prefab, componente nuevo) o déjalo como pedido para su dueño.
- Espera el visto bueno del usuario antes de editar. Si el usuario pidió trabajar sin preguntar, sigue, pero deja el plan escrito en el primer commit.

## 4. Ejecutar

- Un paso, una prueba, un commit. Mensaje: `<ID>: <qué cambió>` en español.
- Con el editor abierto, haz los cambios de escena y prefabs desde el editor con el Unity CLI, nunca editando el YAML a mano.
- Después de cada cambio de código, espera a que Unity compile y revisa la consola.
- Si algo falla tres veces seguidas, detente y explica qué probaste.

## 5. Cerrar

- Actualiza la fila en `PROGRESO.md`: estado "Para probar", rama y la checklist para Cristhian (skill `potato-playtest`).
- Si quedó algo fuera de alcance, agrégalo como pendiente en `PROGRESO.md` o como bug en `BUGS.md`.
- No hagas `git push` sin que el usuario lo pida. Cuando lo pida: `git push -u origin polish/<bloque>` y sugiere abrir el PR.
- Resume en 3 a 5 líneas: qué cambió, cómo probarlo, qué quedó pendiente.
