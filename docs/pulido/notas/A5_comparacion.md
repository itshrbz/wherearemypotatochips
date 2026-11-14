# A5 · Antes y después del look

Fecha: 2026-11-14 · Rama: `polish/look` · Capturas en Play directo desde cada escena, 1280×720, look al 100 %.

Antes: [`../capturas/antes/`](../capturas/antes/) · Después: [`../capturas/despues/`](../capturas/despues/) · Informe de partida: [A1_auditoria_visual.md](A1_auditoria_visual.md)

## Comparación

| Escena | Antes | Después |
| --- | --- | --- |
| Tienda | ![](../capturas/antes/02_Store.png) | ![](../capturas/despues/02_Store.png) |
| Almacén | ![](../capturas/antes/10_Warehouse.png) | ![](../capturas/despues/10_Warehouse.png) |
| Piscinas | ![](../capturas/antes/11_Pools.png) | ![](../capturas/despues/11_Pools.png) |
| Parque | ![](../capturas/antes/12_Playground.png) | ![](../capturas/despues/12_Playground.png) |
| Menú | ![](../capturas/antes/01_Menu.png) | ![](../capturas/despues/01_Menu.png) |

`90_Ending` no se usa en el flujo (los créditos los dibuja `SceneLoader`) y queda igual. El menú es UI en overlay: el post-proceso no lo afecta y su fondo 3D es tarea de E1.

## Qué cambió

- **El post-proceso ahora se ve en juego.** La cámara del jugador lo traía apagado; `RetroLook` lo enciende en las cámaras activas.
- **Look de cámara vieja** en todo el juego, ajustable de 0 a 1 con `GameSettings.RetroLook` (F4 para probar hasta que exista el control en opciones, E2).
- **Cada espacio tiene su color**: tienda cálida, almacén amarillo enfermo, piscinas cian, parque violeta con un ámbar rasante. La réplica es casi neutra y un poco fría.
- **Luces**: fluorescentes con tres ritmos (`LightFlicker`: zumbido, parpadeo, tubo que falla), luz que sale del agua en las piscinas, sol rasante y faroles en el parque.
- **Rendimiento**: luces con sombra en tiempo real, de 5 a 1 (solo el sol del parque, sombra dura). Ninguna luz puntual con sombra.

## Valores finales

### Capas de post-proceso

| Capa | Asset | Prioridad | Contenido |
| --- | --- | --- | --- |
| Base | `SampleSceneProfile` | 0 | Tonemapping Neutral. Bloom en 0 y viñeta apagada. |
| Espacio | `Ambiente_<escena>` | 1 | Balance de blancos, filtro de color, exposición, split toning |
| Réplica | `Ambiente_Replica` (Volume local en `ReplicaStore`, mezcla de 2 m) | 2 | Igual que el espacio, casi neutro y frío |
| Look | `Look_Global` (creado por `RetroLook`, persistente) | 50, peso = `RetroLook` | Ver abajo |

### `Look_Global`

| Efecto | Valor |
| --- | --- |
| Film Grain | Thin2, intensidad 0.40, respuesta 0.75 |
| Chromatic Aberration | 0.15 (apagada en WebGL: `GameFlowSettings.lighterLookOnWeb`) |
| Vignette | negra, 0.30, suavidad 0.45 |
| Lens Distortion | 0.14 (barril), escala 1.0 |
| Lift / Gamma / Gain | lift +0.02 (negros levantados), gain −0.02 |
| Color Adjustments | saturación −10, contraste +4 |

### Presets de `AmbienteTools` (`Tools/Kit/Ambiente`)

| Espacio | Niebla (color, densidad) | Luz ambiente | Temperatura / tinte | Filtro | Exposición | Split toning (sombras / luces) | Cielo |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Tienda | sin niebla | `(0.24, 0.20, 0.16)` | +18 / +2 | `(1, 0.93, 0.82)` | +0.05 | `(0.50, 0.47, 0.45)` / `(0.62, 0.53, 0.42)` | — |
| Réplica | — | — | +2 / −4 | `(0.95, 0.95, 0.93)` | −0.10 | `(0.46, 0.49, 0.52)` / `(0.55, 0.52, 0.48)` | — |
| Almacén | `(0.44, 0.41, 0.24)`, 0.032 | `(0.40, 0.36, 0.19)` | +28 / −10 | `(1, 0.94, 0.72)` | +0.10 | `(0.45, 0.52, 0.40)` / `(0.64, 0.58, 0.36)` | — |
| Piscinas | `(0.52, 0.72, 0.76)`, 0.018 | `(0.33, 0.46, 0.50)` | −14 / −2 | `(0.90, 1, 0.98)` | +0.05 | `(0.40, 0.50, 0.56)` / `(0.50, 0.56, 0.56)` | — |
| Parque | `(0.24, 0.21, 0.28)`, 0.028 | `(0.18, 0.15, 0.21)` | +8 / +10 | `(0.95, 0.88, 1)` | −0.15 | `(0.47, 0.42, 0.60)` / `(0.62, 0.52, 0.42)` | `Cielo_Parque` |

`Cielo_Parque.mat`: Skybox/Procedural sin disco de sol, tinte `(0.42, 0.34, 0.56)`, suelo `(0.16, 0.14, 0.18)`, atmósfera 1.25, exposición 0.45.

### Luces

| Escena | Activas | Con sombra | Con `LightFlicker` | Detalle |
| --- | --- | --- | --- | --- |
| Tienda | 11 | 0 (antes 2) | 6 | Fluorescentes `(1, 0.89, 0.72)` con zumbido; la que ya parpadeaba sigue igual |
| Almacén | 84 | 0 (antes 1) | 37 | Fluorescentes `(1, 0.93, 0.68)`, 2.2. 23 con zumbido, 7 con parpadeo (3–17 s), 7 que fallan. Direccional apagado |
| Piscinas | 14 | 0 (antes 1) | 9 | 9 `LuzAgua` cian `(0.55, 0.9, 1)`, 2.6, 0.5 m bajo la superficie, con zumbido. Paneles `(0.85, 0.97, 1)` a 1.1. Direccional apagado |
| Parque | 11 | 1 | 1 | Sol `(1, 0.62, 0.38)`, 0.55, rotación (7°, 115°), sombra dura 0.6. 6 `LuzFarol` `(1, 0.72, 0.42)`, 2.4, rango 9; una falla |

## Decisiones tomadas sin aprobación (para revisar)

1. **Rama desde `origin/polish/setup` y no desde `origin/main`**: `main` todavía no tiene el setup de S1 (`PROGRESO.md`, skills, `CLAUDE.md`). Cuando se haga merge de `polish/setup`, el PR de `polish/look` solo muestra los commits de A1–A5.
2. **Post-proceso encendido por código** (`RetroLook`) en vez de editar el prefab `Player`, para que funcione con cualquier cámara.
3. **El grano y la viñeta salen de los perfiles de cada espacio** y quedan solo en `Look_Global`, para que el look en 0 deje la imagen limpia.
4. **Se tocó `AmbienteTools.cs`** (archivo de Grover) porque A3 pide ajustar sus presets. Ahora actualiza el perfil sin borrarlo, así no cambia su GUID.
5. **Almacén iluminado por el ambiente**: el generador deja prendida 1 de cada 3 luces; en vez de prender más (caro en WebGL) se subió la luz ambiente amarilla.
6. **Luces de farol sin poste**: A4 pide no tocar geometría; los postes quedan pedidos a Grover para B4.
7. **No se tocaron `PC_RPAsset` ni `Mobile_RPAsset`** por los cambios locales sin commit. Se recomienda un nivel de calidad propio para WebGL en G1.
8. **Aberración cromática apagada en WebGL** por precaución; se puede volver a encender desde `GameFlowSettings` si la build va bien.

## Pendiente

- **Build WebGL y prueba de fluidez** en la laptop más débil. No se hizo: cambiar la plataforma a WebGL reimporta todo el proyecto y modifica ProjectSettings, que tiene cambios locales.
- Probar F4 a mano: el teclado simulado del CLI no llega al Input System con el editor sin foco.
- Texturas y variación de muros (el almacén sigue con material gris liso): B2.
