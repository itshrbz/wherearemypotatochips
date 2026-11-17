# Página de itch

Todo lo necesario para rehacer la página de **Where Are My Potato Chips?** en itch.io (tarea G3).

| Archivo | Contenido |
| --- | --- |
| [pagina_es.md](pagina_es.md) | Textos en español y los datos del formulario de itch (género, etiquetas, embed, colores) |
| [page_en.md](page_en.md) | Los mismos textos en inglés |
| [capturas_y_gifs.md](capturas_y_gifs.md) | Qué capturas y GIF grabar, en qué escena y en qué momento |

## Cómo armar la página

1. En itch: *Edit game*. Pegar el título, la descripción corta y la descripción larga. La página principal va en inglés, con la versión en español debajo de una línea separadora (`---`) y el título "Español".
2. Completar el formulario con la tabla "Datos del formulario de itch" de `pagina_es.md`.
3. Subir la build WebGL (zip con `index.html` en la raíz) marcada "This file will be played in the browser", y la de Windows como descarga.
4. Subir la portada y las capturas en el orden de `capturas_y_gifs.md`.
5. En *Edit theme*, aplicar los colores de la tabla.

## Antes de publicar, confirmar

- [ ] Duración real de una partida (cronometrar en T4) y reemplazar `[15–25]` en los dos idiomas.
- [ ] Si el juego ya está en español e inglés (E6). Si no, quitar esa línea de características y de "Languages".
- [ ] Mando: hoy no mueve ni mira en juego (bug en `BUGS.md`) y saltar no tiene botón. Cuando se arregle, completar las celdas `[...]` de la tabla de controles y agregar "Xbox controller" en Inputs.
- [ ] Si hay subtítulos (E5) para marcarlos en accesibilidad.
- [ ] Las texturas de terceros de B1 en los créditos, y que coincidan con `CREDITS.md`.
- [ ] La declaración de uso de IA que pide itch: decidirla con el equipo.
- [ ] Los roles de cada persona, con cada uno.
