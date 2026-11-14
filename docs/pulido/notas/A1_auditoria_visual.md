# A1 · Auditoría visual

Fecha: 2026-11-14 · Rama: `polish/look` · Editor 6000.5.5f1, URP, color space lineal.

Capturas desde el punto de vista del jugador (Play directo en cada escena, 1280×720) en [`../capturas/antes/`](../capturas/antes/).

| Menú | Tienda | Almacén |
| --- | --- | --- |
| ![](../capturas/antes/01_Menu.png) | ![](../capturas/antes/02_Store.png) | ![](../capturas/antes/10_Warehouse.png) |
| **Piscinas** | **Parque** | **Final (90_Ending)** |
| ![](../capturas/antes/11_Pools.png) | ![](../capturas/antes/12_Playground.png) | ![](../capturas/antes/90_Ending.png) |

## 1. Estado actual

### Post-proceso

| Volume | Dónde | Efectos |
| --- | --- | --- |
| `Global Volume` → `SampleSceneProfile` | En **todas** las escenas | Tonemapping Neutral, Bloom (intensidad 0), Motion Blur (apagado), Viñeta 0.2 |
| `Volumen_Ambiente` → `Ambiente_10_Warehouse` | Almacén | Viñeta 0.4, Grano Thin1 0.45, Exposición −0.3, Saturación −25 |
| `Volumen_Ambiente` → `Ambiente_11_Pools` | Piscinas | Viñeta 0.25, Grano 0.35, Exposición +0.25, Saturación −15 |
| `Volumen_Ambiente` → `Ambiente_12_Playground` | Parque | Viñeta 0.45, Grano 0.4, Exposición −0.6, Saturación −20 |
| `DefaultVolumeProfile` | Global del proyecto (URP) | Todo en neutro (sin efecto visible) |

- **El problema más grande:** la cámara del prefab `Player` tiene **Post Processing apagado** (`m_RenderPostProcessing: 0`). Como en la tienda y en los espacios el juego desactiva las demás cámaras y usa la del jugador, **ningún Volume se ve en juego**: ni el grano, ni la viñeta, ni la saturación de `AmbienteTools`. Solo el menú (que usa `Main Camera`) tiene post-proceso.
- Los dos Volumes globales de cada espacio tienen prioridad 0 y los dos definen viñeta: el resultado depende del orden en que se cargan.
- `ScreenEffects` dibuja su viñeta de persecución como imagen de UI encima de todo, así que no choca con el post-proceso.

### Niebla y luz ambiente (`RenderSettings`, aplicadas con `Tools/Kit/Ambiente`)

| Escena | Niebla | Luz ambiente | Skybox |
| --- | --- | --- | --- |
| 02_Store | apagada | Flat `#292B30` | ninguno (pero la cámara limpia con skybox) |
| 10_Warehouse | Exp. `#2E3329`, 0.045 | Flat `#0D0E0D` | Default |
| 11_Pools | Exp. `#80A8AD`, 0.020 | Flat `#4C6166` | Default |
| 12_Playground | Exp. `#14141F`, 0.032 | Flat `#09090F` | **Default (cielo azul de día visible)** |
| 01_Menu, 90_Ending, 00_Boot | apagada | Skybox | Default |

### Luces (todas en tiempo real, sin lightmaps, sin probes, sin reflection probes)

| Escena | Activas | Con sombra | Con `LightFlicker` | Detalle |
| --- | --- | --- | --- | --- |
| 02_Store | 11 | 2 | 1 | 6 fluorescentes (point, 1.6, r 7.5; **una con sombra suave**, que es la más cara: 6 caras de cubemap), 3 de heladera, 1 farol spot, direccional 0.08 con sombra |
| 10_Warehouse | **85** | 1 | **0** | 82 `LuzFluorescente` del `Kit_Techo` (point, 2, r 7, blanco verdoso `#EDFFE6`), 2 de parkour, direccional con intensidad 0 **pero con sombra suave** |
| 11_Pools | 6 | 1 | 0 | 5 `PanelLight` (point, 1.5, r 12, `#D9F7FF`), direccional en 0 con sombra suave |
| 12_Playground | 5 | 1 | 0 | 4 `LuzFluorescente` del kit (blanco verdoso, de interior), direccional 0.06 con sombra; **no hay faroles** |

### Calidad y RP Assets

| | PC (`PC_RPAsset`) | Mobile (`Mobile_RPAsset`) |
| --- | --- | --- |
| Lo usan | Standalone, **WebGL** | Android, iOS |
| Renderer | Forward+ | Forward |
| HDR | sí (32 bits) | sí |
| Escala de render | 1.0 | 0.8 |
| Sombras principales | 2048, 4 cascadas, suaves, 50 m | 1024, 1 cascada, duras |
| Sombras de luces extra | sí | no |
| Luces extra | por píxel, 8 por objeto | por píxel, 8 por objeto |
| Opaque/Depth texture | sí / sí | no / no |

WebGL usa el nivel **PC**: cuatro cascadas, sombras suaves y sombras de luces puntuales, todo lo caro, en la plataforma más débil.

## 2. Problemas

1. **Post-proceso apagado en la cámara del jugador**: todo el trabajo de ambiente no se ve.
2. **Los espacios no tienen su color.** El almacén se ve gris azulado oscuro, no amarillo enfermo. El parque tiene un cielo azul de día con niebla nocturna, y los árboles negros contra el cielo claro rompen la ilusión. Las piscinas son las más cercanas a la dirección de arte.
3. **La tienda se ve de prototipo**: luz blanca pareja, sin post-proceso, sin calidez. Es el lugar "seguro" y debería sentirse naranja de foco.
4. **No hay look de cámara vieja** en ningún lado.
5. **Rendimiento WebGL**: sombras en luces que no aportan (direccionales en 0, una fluorescente de la tienda), 4 cascadas suaves en WebGL, 85 luces puntuales en el almacén.
6. **El almacén no parpadea**: `LightFlicker` solo está en una luz de la tienda.
7. **90_Ending** es una escena vacía con el skybox por defecto. Hoy los créditos se muestran desde `SceneLoader` y la escena no se usa en el flujo. Se deja para E7.
8. Mezcla de viñetas: `SampleSceneProfile` (0.2) contra `Ambiente_*` (0.25–0.45) con la misma prioridad.

## 3. Propuesta

> Rafael pidió no esperar aprobación: esta propuesta se aplica en A2–A5 y queda aquí para revisarla después.

### Capas de post-proceso (quién manda qué)

| Capa | Asset | Prioridad | Qué lleva | Se apaga con |
| --- | --- | --- | --- | --- |
| Base | `SampleSceneProfile` (ya está en todas las escenas) | 0 | Tonemapping Neutral. Le quito la viñeta. | nunca |
| Espacio | `Ambiente_<escena>` | 1 | Color del espacio: balance de blancos, filtro de color, exposición, contraste, saturación, split toning. Sin grano ni viñeta. | nunca |
| Réplica | `Ambiente_Replica` (Volume local en el prefab `ReplicaStore`) | 2 | Igual que la tienda, un poco más frío | nunca |
| Look | `Look_Global` (Volume global persistente creado por código) | 50 | Grano, aberración cromática, viñeta, distorsión de lente, negros levantados, un poco menos de saturación | `GameSettings.RetroLook` (peso 0–1) |

- **Decisión:** el post-proceso se enciende por código en la cámara activa (componente `RetroLook`), no editando el prefab `Player`. Así funciona con cualquier cámara (menú, tienda, espacios) y no depende de cada escena.
- **Decisión:** `Look_Global` lo crea `RetroLook` en `DontDestroyOnLoad` al arrancar, con la referencia al perfil en `GameFlowSettings`. No hay que tocar las escenas de Grover para que el look aparezca.
- **Decisión:** el grano y la viñeta pasan del perfil de cada espacio a `Look_Global`, para que con el look en 0 la imagen quede limpia de verdad y el color de cada espacio se mantenga.

### Por espacio

| Espacio | Niebla | Luz ambiente | Color (Volume) | Luces |
| --- | --- | --- | --- | --- |
| Tienda | apagada | Flat cálido `#3A322A` | Temperatura +15, filtro durazno, contraste +10 | Fluorescentes a blanco cálido, sin sombras en la fluorescente; `LightFlicker` lento en una |
| Réplica | — | — | Como la tienda pero temperatura +2 y saturación −10 | — |
| Almacén | Exp. amarillo oliva `#4A4528`, 0.035 | Flat `#2A2614` | Temperatura +25, tinte −10 (verde), filtro amarillo, saturación −10 | Fluorescentes amarillo pálido `#FFF1B8`; `LightFlicker` en tres ritmos y algunas que fallan |
| Piscinas | Exp. cian `#7FB8C0`, 0.018 | Flat `#5A7880` | Temperatura −20, filtro cian, exposición +0.2 | Paneles en cian pálido; luz desde el agua (luces puntuales bajo la superficie, sin malla visible) |
| Parque | Exp. violeta gris `#3A3346`, 0.028 | Flat `#2A2433` | Temperatura +5, split toning violeta/ámbar, exposición −0.3 | Skybox propio de atardecer apagado (sol bajo, sin disco), direccional rasante ámbar y faroles cálidos |

### Rendimiento (A4)

- Sin sombras en las direccionales que están en 0 y en las puntuales. Una sola luz con sombra por escena como máximo.
- Las luces del almacén se quedan en tiempo real (Forward+ las agrupa), pero sin sombras y con rango ajustado.
- **No toco `PC_RPAsset` ni `Mobile_RPAsset`** en esta rama: tienen cambios locales de Rafael sin commit (prefiltrado de shaders) y no los quiero mezclar. Recomendación para G1: un nivel de calidad propio para WebGL con 1 cascada, sombras duras de 1024, sin sombras de luces extra y escala de render 0.85.
- Sin lightmaps horneados: las escenas son de Grover y los archivos de luz horneada pesan y generan conflictos. Queda como opción para B5 si hace falta.
