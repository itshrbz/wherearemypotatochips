---
name: potato-direccion-arte
description: Dirección de arte y sonido de Where Are My Potato Chips: look general, paleta y ambiente de cada espacio. Usar antes de tocar luz, post-proceso, materiales, texturas, decoración o audio.
---

# Dirección de arte y sonido

## La idea en una frase

Un lugar cotidiano que está **casi** bien. El miedo no viene de monstruos en cada esquina, sino de espacios vacíos, demasiado limpios o demasiado repetidos, donde un detalle no encaja. El tema de la jam fue "Something's Off": cada decisión visual y sonora debería reforzar esa sensación.

## Look general: cámara de video vieja

- Grano de película visible pero fino. Aberración cromática suave en los bordes. Viñeta ligera siempre y más fuerte cuando el jefe persigue (eso ya lo hace `ScreenEffects`).
- Un poco de distorsión de lente y negros levantados, como un video casero de los 90.
- El look tiene que poder apagarse o bajarse en opciones (accesibilidad y rendimiento).
- Nada de bloom exagerado ni colores saturados: la saturación general va baja y cada espacio tiene **un** color que domina.

## Los espacios

| Espacio | Escena | Color que domina | Luz | Sonido | Qué está mal |
| --- | --- | --- | --- | --- | --- |
| Tienda | `02_Store` | Cálido, naranja de foco | Fluorescentes y focos, se siente seguro | Música de tienda bajita, refrigeradores, calle lejana | Al volver al final, algo cambió |
| Almacén (backrooms) | `10_Warehouse` | Amarillo enfermo | Fluorescentes parejos que zumban y a veces fallan | Zumbido eléctrico, alfombra húmeda, golpes lejanos | Pasillos que no deberían caber, carteles sin sentido |
| Piscinas | `11_Pools` | Cian frío, azulejo blanco | Luz difusa que viene del agua, sin fuente clara | Agua, goteo, eco largo | Escaleras que no llevan a nada, toboganes que entran a la pared |
| Parque | `12_Playground` | Atardecer apagado, violeta y gris | Luz baja de un sol que no se ve, faroles | Viento, columpios que se mueven solos, risas lejanas cortadas | Juegos fuera de escala, ningún niño |

La tienda réplica que termina cada espacio se ve **casi** igual a la real: mismo modelo, un detalle distinto.

## Materiales y texturas

- Texturas realistas de baja resolución (512 a 1024) mejor que estilizadas: lo liminal funciona con lo cotidiano.
- Fuentes libres recomendadas: ambientCG y Poly Haven (CC0). Anotar en `CREDITS.md`.
- Variación: ninguna pared lisa del mismo color de punta a punta. Manchas, zócalos, enchufes, carteles, calcomanías.
- Desgaste moderado. No es un lugar abandonado y roto: es un lugar normal donde no hay nadie.

## Sonido

- Silencio relativo, nunca silencio total: siempre hay un tono de sala.
- La persecución se oye antes de verse (pasos del jefe, respiración, música de tensión que entra por capas).
- Reverb distinta por espacio: almacén seco y cercano, piscinas eco largo, parque abierto.
- Sustos de sonido con moderación: uno fuerte cada varios minutos vale más que muchos chicos.
- Fuentes libres recomendadas: Freesound (filtrar por CC0) y el paquete gratuito de Sonniss GDC. Anotar en `CREDITS.md`.

## Cómo saber si quedó bien

- Una captura de cada espacio se distingue de las otras a primera vista y las tres se reconocen como el mismo juego.
- Con los ojos cerrados y audífonos se adivina en qué espacio estás.
- Nada se ve gris de prototipo desde el punto de vista del jugador.
