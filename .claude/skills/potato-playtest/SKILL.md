---
name: potato-playtest
description: Escribir la checklist de prueba de una tarea para Cristhian y registrar bugs en el formato del equipo. Usar al terminar cualquier tarea del plan o al convertir notas de un playtest en bugs.
---

# Pruebas y bugs

Cristhian prueba todo lo que hacen las IAs. Él no lee código: necesita saber qué hacer, dónde y qué debería ver.

## Checklist de una tarea (va en PROGRESO.md)

```markdown
### Prueba de <ID> — <título>
Rama: polish/<bloque>  ·  Build: editor | WebGL | Windows

Preparar: <qué abrir, desde qué escena empezar, atajos útiles (F2 salta de espacio, F3 reinicia)>

- [ ] <acción concreta> → debería <resultado visible o audible>
- [ ] <acción concreta> → debería <resultado>
- [ ] Probar también con el efecto apagado en opciones (si aplica)
- [ ] No hay errores en la consola (solo en el editor)

Riesgos: <qué podría haberse roto en otra parte y conviene mirar>
```

Reglas:

- Entre 3 y 8 puntos. Cada uno es una acción y un resultado que se puede ver o escuchar.
- Nada de "verificar que funcione bien". Di qué tiene que pasar.
- Si la tarea toca rendimiento, incluye un punto en la build WebGL.

## Formato de bug (va en BUGS.md)

```markdown
| ID | Gravedad | Dónde | Qué pasa | Pasos | Bloque | Estado |
| B-012 | Alta | Piscinas | El jefe atraviesa la pared del vestuario | 1. F2 hasta Piscinas 2. Dejarse ver cerca del vestuario 3. Correr al pasillo | C | Abierto |
```

Gravedad:

- **Bloqueante:** no se puede terminar el juego o se cuelga.
- **Alta:** rompe la experiencia (atravesar paredes, perder progreso, susto que no funciona).
- **Media:** se nota pero se puede seguir.
- **Baja:** detalle visual o de texto.

## Convertir notas de playtest en bugs

Si te pasan notas sueltas (de Cristhian o de jugadores externos): agrúpalas, descarta las repetidas, asigna gravedad y bloque, y agrégalas a `BUGS.md` con el siguiente ID libre. Las opiniones que no son bugs ("el parque me aburrió") van a la sección de comentarios del playtest, no a la tabla.
