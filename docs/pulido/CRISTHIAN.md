# Cristhian · Tester

No le pides trabajo a ninguna IA. Tu trabajo es que nada llegue a `main` sin probarse, y que el juego guste a alguien que nunca lo vio. Tienes la última palabra en cada puerta.

---

## Fase 0

### T0 · Instalar

Sigue [01_INSTALACION.md](01_INSTALACION.md), secciones 1 y 4. Practica cambiar de rama y abrir el juego desde `00_Boot`.

### T1 · Playtest base

- Juega la versión actual de `main` completa tres veces: una normal, una rápida y una buscando romper cosas (paredes, escondites, caerse del mapa, pausar en momentos raros).
- Anota cada bug en [BUGS.md](BUGS.md) con el formato de la tabla: dónde pasa, qué pasa y los pasos para repetirlo.
- Al final de `BUGS.md` escribe qué te aburrió, qué te confundió y qué te gustó. Eso guía las prioridades.

---

## Fases 1 y 2

### T2 · Probar cada tarea

Cada vez que una tarea queda "Para probar" en [PROGRESO.md](PROGRESO.md):

1. `git fetch` y `git switch` a la rama que dice la tarea.
2. Abre Unity, espera que importe y sigue la checklist que dejó la IA, punto por punto.
3. Si todo pasa: marca la tarea como "Hecha" en `PROGRESO.md` y avisa al dueño para el merge.
4. Si algo falla: agrega el bug a `BUGS.md` con el ID de la tarea en "Dónde" y deja la tarea en "Para probar". Avisa al dueño.

Prueba también lo que la checklist dice en "Riesgos": muchas veces el bug está en otra parte.

### T3 · Puerta 1 (fin de la Fase 1)

- Cuando A, C y D estén en `main`, juega una build completa (WebGL, en el navegador).
- Pasa si no hay bugs bloqueantes y si el look y el audio nuevos se notan en todo el juego. Escribe el veredicto en `PROGRESO.md`.

---

## Fase 2 y 3

### T4 · Playtest con 5 personas de afuera (Puerta 2)

- Personas que nunca jugaron el juego, ideal si les gusta el terror. Una a la vez, con audífonos.
- Reglas: no las ayudes ni expliques nada. Si preguntan, responde "haz lo que creas". Mira y anota.
- Anota por persona: cuánto tardó en entender qué hacer, dónde se perdió, cuándo se asustó (y si se rió o se quejó), en qué momento quiso dejar de jugar, si terminó.
- Al final, cinco preguntas:
  1. Del 1 al 10, ¿cuánto te gustó?
  2. ¿Qué fue lo que más te asustó?
  3. ¿Hubo algún momento en que no sabías qué hacer?
  4. ¿Qué cambiarías?
  5. ¿Se lo recomendarías a un amigo?
- Pasa tus notas a `docs/pulido/notas/T4_playtest.md`. Para convertirlas en bugs puedes pedirle a cualquiera del equipo que use la skill `potato-playtest`.
- Pasa la Puerta 2 si al menos 4 de 5 terminan el juego y el promedio de la pregunta 1 es 7 o más.

### T5 · Prueba final (Puerta 3)

- Build WebGL en Chrome, Firefox y Edge, y la build de Windows.
- Juego completo en cada una, opciones, cambio de idioma, pausa, reiniciar, volver al menú.
- Pasa si no hay bugs bloqueantes ni altos abiertos. Entonces Rafael publica la 1.0.
