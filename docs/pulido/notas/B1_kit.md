# B1 · Kit de materiales

Rama prevista: `polish/niveles` (hacer `git merge main` antes, como pide `GROVER.md`). Estado: **Para probar**. Las texturas están en `Assets/05_Textures/Kit/<Nombre>/` (falta `Bloque_Pintado`) y los materiales en `Assets/03_Materials/Kit/`. El Smoothness sale de `<base>_MetallicSmoothness.png`, generado desde el editor: R = Metalness (0 si no hay) y alfa = 1 − Roughness.

## 1. Calcomanías (listas)

Generadas por código en esta sesión, sin licencias de terceros. Están en `Assets/05_Textures/Kit/Decals/`, en PNG con transparencia:

| Archivo | Tamaño | Dónde va | Nota |
| --- | --- | --- | --- |
| `Decal_Exit.png` | 1024×384 | Los tres espacios | Cartel de salida iluminado, flecha a la derecha. Detalle que está mal: ponerlo apuntando a una pared |
| `Decal_AisleNumbers.png` | 1024×512 | Almacén | Hoja de 4×2: 01 a 07 y un **07 repetido** a propósito. Cada celda es 0.25 × 0.5 en tiling/offset |
| `Decal_StaffOnly.png` | 1024×512 | Almacén | |
| `Decal_NoRunning.png` | 1024×512 | Piscinas | "Pool closed after dark" |
| `Decal_WetFloor.png` | 768×768 | Piscinas, almacén | |
| `Decal_ParkCloses.png` | 1024×512 | Parque | "It is always dusk": detalle que está mal |
| `Decal_WaterStain.png` | 1024 | Techos y paredes | Mancha de humedad |
| `Decal_RustDrip.png` | 1024 | Bajo tubos, estantes, escaleras de piscina | Chorreado de óxido |
| `Decal_Grime.png` | 1024 | Pisos y zócalos | Mugre suave |

Al importar en Unity: Texture Type **Default**, **Alpha Is Transparency** activado, Max Size 1024, compresión normal. Se usan con **URP Decal Projector** o, más barato en WebGL, en un quad con material **Unlit transparente**.

## 2. Texturas para descargar (Grover)

De [ambientCG](https://ambientcg.com) o [Poly Haven](https://polyhaven.com/textures), todas CC0. En ambientCG elegir **1K-JPG** (2K solo para el piso más grande de cada espacio). Del zip solo hacen falta tres mapas: **Color**, **NormalGL** y **Roughness**.

Guardar cada una en su carpeta, con el nombre de la tabla: `Assets/05_Textures/Kit/<Nombre>/` (por ejemplo `Assets/05_Textures/Kit/Concreto_Piso/Concrete034_1K-JPG_Color.jpg`).

| # | Nombre de carpeta | Espacio | Qué buscar | Reemplaza (a confirmar mirando la escena) |
| --- | --- | --- | --- | --- |
| 1 | `Concreto_Piso` | Almacén | Concrete (piso pulido o con manchas) | `Almacen_Bloque`, `Greybox_Gris` (pisos) |
| 2 | `Bloque_Pintado` | Almacén | Painted concrete block / bricks pintados claros | `Greybox_Oscuro` (paredes) |
| 3 | `Metal_Pintado` | Almacén, parque | Painted metal (con algo de desgaste) | `Almacen_Metal`, `Parque_Cerca` |
| 4 | `Carton` | Almacén | Cardboard | `Almacen_Carton` |
| 5 | `Madera_Palet` | Almacén | Wood planks (madera clara, basta) | Palets nuevos |
| 6 | `Azulejo_Piscina` | Piscinas | Tiles chicos y cuadrados, blanco o celeste | Paredes y fondo de piscina |
| 7 | `Azulejo_Piso` | Piscinas | Tiles de piso antideslizante, grises o beige | Pisos de piscina |
| 8 | `Metal_Inox` | Piscinas | Brushed metal / stainless | Escaleras y barandas |
| 9 | `Pasto_Seco` | Parque | Grass seco o amarillento (no verde intenso) | `Parque_Verde` |
| 10 | `Tierra_Camino` | Parque | Gravel / ground de tierra compacta | `Parque_Camino` |
| 11 | `Madera_Pintada` | Parque | Painted wood / wood planks pintados | Bancos, `Parque_Marron` |
| 12 | `Goma_Juegos` | Parque | Rubber / asphalt claro (piso de goma de juegos) | Bajo los juegos |
| 13 | `Alfombra` | Pasillos y oficinas | Carpet (gastada, color apagado) | Zonas interiores de paso |
| 14 | `Yeso_Pared` | Todos | Plaster pintado claro | Paredes interiores |

Los colores vivos del parque (`Parque_Rojo`, `Azul`, `Amarillo`, `Naranja`) pueden quedar como color liso con un poco de `Metal_Pintado` como textura de detalle: el plástico de juegos es liso de verdad.

Anotar en `CREDITS.md` el nombre y el enlace de cada textura descargada (ambientCG y Poly Haven no lo exigen, pero el plan sí).

## 3. Lo que sigue (en una sesión de Claude Code con el editor)

1. Importar las texturas: Max Size 1024 (2048 solo la del piso grande), NormalGL como **Normal map**, Roughness sin sRGB.
2. Crear los materiales URP Lit en `Assets/03_Materials/Kit/` (`Kit_<Nombre>.mat`). Roughness en URP va invertido como Smoothness: usar el mapa en el canal alfa o ajustar el valor de Smoothness a mano.
3. Asignar las texturas a los materiales grises de la columna "Reemplaza". Los generadores (`AlmacenBuilder`, `ParqueBuilder`) reutilizan esos materiales por nombre, así que no los pisan, y `SurfaceTiling` repite la textura según el tamaño real de cada cubo.
4. Materiales de calcomanía (`Kit_Decal_<Nombre>.mat`) con las PNG de arriba.
5. Terminado cuando hay al menos 12 materiales y 8 calcomanías listos para usar.
