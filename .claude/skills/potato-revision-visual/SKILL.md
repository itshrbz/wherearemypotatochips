---
name: potato-revision-visual
description: Ciclo de rediseño con capturas para Where Are My Potato Chips. Captura una escena desde la vista del jugador, la critica con una rúbrica, aplica los cambios de más impacto y compara antes y después. Usar cuando pidan rediseñar, mejorar cómo se ve, pulir una escena, un menú o la UI, o "que se vea mejor".
---

# Revisión visual con capturas

Un cambio visual no está hecho hasta que se ve en una captura. Este ciclo evita cambiar cosas a ciegas: siempre se mira, se critica, se cambia poco y se vuelve a mirar desde la misma cámara.

Antes de empezar, lee `potato-direccion-arte` (el tono de cada espacio) y, si el tema es miedo o ritmo, `potato-terror`.

## 1. Capturar el antes

- Con el editor abierto y `unity status` en `ready`, entra en Play en la escena y pon la cámara del jugador en 2 o 3 puntos fijos: la entrada, un pasillo típico y el lugar más importante (la tienda réplica, un escondite, el jefe a la vista).
- Anota la posición y rotación exactas de cada punto en `docs/pulido/notas/<ID>_revision.md`, para repetir las capturas igual.
- Guarda las capturas en `docs/pulido/capturas/<ID>/antes/` a 1280×720.
- **Mira cada captura con la herramienta de lectura de imágenes.** No critiques de memoria ni por la descripción de la escena.

## 2. Criticar con la rúbrica

Puntúa cada captura de 1 a 5 en cada punto y escribe una frase de por qué:

| Criterio | Pregunta |
| --- | --- |
| Lectura | ¿Se entiende en 2 segundos hacia dónde ir y qué es interactivo? |
| Punto focal | ¿Hay una sola cosa que llama la mirada, y es la correcta? |
| Valores | En blanco y negro, ¿se separan suelo, paredes y objetos? ¿Hay zonas de oscuridad con intención? |
| Color | ¿Domina el color del espacio? ¿El acento (amarillo de la UI, luces) se usa solo para lo importante? |
| Detalle | ¿Hay detalle en tres escalas (formas grandes, objetos medianos, detalles chicos)? ¿Quedan superficies lisas de prototipo? |
| Algo está mal | ¿Hay al menos un detalle extraño que se descubre al mirar bien? |
| Tono | ¿Da la sensación del espacio según `potato-direccion-arte`, o se ve genérico? |
| UI | ¿El texto se lee sobre el fondo? ¿Tapa algo importante? |

Para revisar valores, convierte la captura a escala de grises (por ejemplo con Python y Pillow) y mírala también.

## 3. Elegir pocos cambios

- De la crítica, elige los **3 cambios** con más impacto por esfuerzo. Escríbelos antes de tocar nada.
- Preferir, en este orden: luz y color, composición (mover o agregar objetos grandes), detalle, efectos. Un efecto nuevo rara vez arregla una escena que no se lee.
- Respeta los dueños de escenas (`potato-escenas`). Si el cambio es en una escena ajena, déjalo como pedido.

## 4. Aplicar y capturar el después

- Aplica los cambios desde el editor con el Unity CLI.
- Repite las capturas desde los mismos puntos en `docs/pulido/capturas/<ID>/despues/`.
- Arma una imagen comparativa lado a lado (antes | después) con Pillow en `docs/pulido/capturas/<ID>/comparacion_<punto>.png`, y mírala.
- Vuelve a puntuar con la rúbrica. Si un cambio no subió el puntaje o empeoró otro criterio, revísalo o deshazlo.

## 5. Cerrar

- En `docs/pulido/notas/<ID>_revision.md`: puntajes antes y después, los cambios hechos, los descartados y por qué.
- Si el rendimiento puede haber cambiado (luces, sombras, partículas, post-proceso), anótalo como riesgo para la prueba en WebGL.
- Checklist de prueba para Cristhian con `potato-playtest`.

## Errores comunes

- Subir el brillo general para que "se vea". Mejor iluminar el camino y dejar oscuro lo demás.
- Saturar todo. La saturación baja es parte del look; el color fuerte se guarda para el acento.
- Llenar de efectos (bloom, partículas, niebla espesa) una escena vacía. Primero formas y objetos.
- Cambiar diez cosas a la vez: después no se sabe qué funcionó.
