---
name: potato-terror
description: Diseño de miedo y ritmo para Where Are My Potato Chips: curva de tensión, sustos, espacios liminales y el jefe. Usar al diseñar o revisar sustos, encuentros, el ritmo de un espacio, la intro o cualquier cosa que deba dar miedo o inquietud.
---

# Diseño de terror

El juego da miedo por lo que el jugador **espera** que pase, más que por lo que pasa. Un susto bien preparado vale más que diez sin aviso.

## La curva de cada espacio

Cada espacio sigue esta curva, y la intensidad máxima sube de un espacio al siguiente (almacén < piscinas < parque):

1. **Calma rara (30–60 s).** Nada pasa, pero algo no encaja. El jugador explora y baja la guardia.
2. **Inquietud.** Primeras señales: un sonido lejano, una luz que falla, un objeto fuera de lugar.
3. **Tensión.** El jefe está cerca y se nota (`BossAI.Awareness01`, avisos de C3). El jugador busca escondites.
4. **Pico.** Persecución o susto fuerte.
5. **Alivio.** Escapa, encuentra la réplica o vende. Un momento seguro antes de la siguiente subida.

Sin alivio, la tensión constante se vuelve ruido y deja de asustar.

## Presupuesto de sustos

- Como máximo un susto fuerte cada 3 a 4 minutos. Entre medio, inquietud y tensión.
- Primer susto antes de los 3 minutos de juego, sin el jefe (tarea E3).
- Ningún susto se repite igual. Si un evento se repite, cambia algo: el lugar, el sonido o el momento.
- Los sustos de sonido fuerte se usan poco. El silencio repentino también asusta.

## Anticipación antes que sorpresa

- Mostrar o hacer oír la amenaza **antes** del ataque: pasos, respiración, luces que fallan, la viñeta.
- La cámara y la luz guían la mirada hacia donde algo va a pasar, un segundo antes de que pase.
- Una puerta que se cierra sola, un objeto que ya no está donde estaba: la idea de que algo se movió sin que lo vieras.

## Lo liminal

- **Vacío:** lugares hechos para mucha gente, sin nadie.
- **Repetición:** pasillos y salas casi iguales, con una variación que inquieta.
- **Escala equivocada:** puertas demasiado altas, juegos de parque enormes, techos muy bajos.
- **Normalidad incómoda:** luz pareja, música de tienda, todo limpio… demasiado limpio.
- **Algo está mal:** cada espacio tiene detalles que no deberían estar ahí. Van de sutiles al principio a obvios cerca del final.

## Oscuridad que se puede jugar

- Oscuro no es negro. El camino siempre se lee: luz en el suelo, en las salidas, en los objetos interactivos.
- Lo que no se ve bien es lo de los costados y el fondo, donde la imaginación hace el trabajo.
- Revisa cada cambio de luz con la opción de brillo de la pantalla en un monitor normal, no solo en el del editor.

## El jefe es justo

- Siempre hay aviso antes de una captura (ver C3).
- El jugador entiende por qué lo vieron: estaba a la vista, corrió, hizo ruido.
- Los escondites funcionan cuando se usan a tiempo.
- Morir da ganas de reintentar: reinicio rápido, sin castigo largo (ver C5).

## Revisar un espacio

Juega el espacio completo (o pide a Cristhian que lo juegue) y anota en una línea de tiempo: minuto, qué pasó y qué sintió el jugador. Compara con la curva:

- ¿Hay calma al inicio y alivio al final?
- ¿Cuántos sustos fuertes hubo y cada cuánto?
- ¿Hubo algún momento de aburrimiento de más de un minuto?
- ¿El jugador entendió todas las capturas?

Anota los cambios propuestos en `docs/pulido/notas/<ID>_ritmo.md`.
