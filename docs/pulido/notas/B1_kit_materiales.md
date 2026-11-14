# B1 · Kit de materiales

Tarea de Grover que tomó Rafael para adelantarle trabajo. Estado: **texturas elegidas, descargadas y preparadas; falta importarlas y crear los materiales**, porque el editor de Unity dejó de responder.

## Materiales (14)

Todas CC0. Color y normal a 1024 en JPG. Sin mapas de rugosidad, AO ni desplazamiento, para no cargar WebGL: la suavidad va fija en cada material.

| Material (`Assets/03_Materials/Kit/`) | Para qué | Fuente | Autor | Licencia | Enlace |
| --- | --- | --- | --- | --- | --- |
| `Kit_AzulejoBlanco` | Paredes de piscinas y baños | ambientCG `Tiles107` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Tiles107 |
| `Kit_AzulejoPiscina` | Fondo y bordes de las piscinas | ambientCG `Tiles132B` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Tiles132B |
| `Kit_Alfombra` | Piso del almacén (backrooms) | ambientCG `Carpet016` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Carpet016 |
| `Kit_PapelTapiz` | Paredes del almacén, con moho | ambientCG `Wallpaper002C` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Wallpaper002C |
| `Kit_TechoPlacas` | Techo de oficina del almacén y la tienda | ambientCG `OfficeCeiling003` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=OfficeCeiling003 |
| `Kit_Concreto` | Pasillos de servicio, bordes | ambientCG `Concrete034` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Concrete034 |
| `Kit_PastoSeco` | Suelo del parque | Poly Haven `withered_grass` | Poly Haven | CC0 1.0 | https://polyhaven.com/a/withered_grass |
| `Kit_MetalVerde` | Juegos del parque, barandas | ambientCG `PaintedMetal006` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=PaintedMetal006 |
| `Kit_MetalAmarillo` | Columpios, barandas, bordes de seguridad | ambientCG `PaintedMetal016` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=PaintedMetal016 |
| `Kit_PisoTienda` | Piso de la tienda y la réplica | ambientCG `Tiles139` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Tiles139 |
| `Kit_Yeso` | Paredes viejas, vestuarios | ambientCG `Plaster007` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Plaster007 |
| `Kit_Asfalto` | Camino del parque, calle de la tienda | ambientCG `Asphalt031` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Asphalt031 |
| `Kit_MaderaPintada` | Bancas, casitas del parque | ambientCG `PaintedWood007B` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=PaintedWood007B |
| `Kit_Tierra` | Areneros y zonas sin pasto del parque | ambientCG `Ground109` | Lennart Demes (ambientCG) | CC0 1.0 | https://ambientcg.com/view?id=Ground109 |

> El autor de Poly Haven por textura se confirma en su página antes de pasarlo a `CREDITS.md`.

## Calcomanías (11)

Materiales URP Lit transparentes para poner en un quad pegado a la pared (`Assets/03_Materials/Kit/Calcomanias/`). No se usa el Decal Projector de URP: pide un renderer feature que cuesta en WebGL.

| Calcomanía | Fuente | Licencia |
| --- | --- | --- |
| `Calco_Humedad1`, `2`, `3` (manchas de goteo) | ambientCG `Leaking012A`, `Leaking013A`, `Leaking017B` (la forma sale del mapa de desplazamiento) | CC0 1.0 |
| `Calco_Salida` (EXIT con flecha) | Dibujada por el equipo con el script | Propia (CC0) |
| `Calco_Pasillo7`, `Calco_Pasillo13`, `Calco_Pasillo7B` (números de pasillo; el 7 se repite donde no debe) | Propia | Propia (CC0) |
| `Calco_PisoMojado` (CAUTION WET FLOOR) | Propia | Propia (CC0) |
| `Calco_NoCorrer` (NO RUNNING BY THE POOL) | Propia | Propia (CC0) |
| `Calco_SoloEmpleados` (EMPLOYEES ONLY) | Propia | Propia (CC0) |
| `Calco_AfichePapas` (promo de papas "AISLE 7", el producto que nunca aparece) | Propia | Propia (CC0) |

## Cómo rehacer las texturas

1. Descargar de ambientCG los zip `<ID>_1K-PNG` de la tabla (`https://ambientcg.com/get?file=<ID>_1K-PNG.zip`) y descomprimir cada uno en una carpeta con su ID. De Poly Haven, `withered_grass_diff_1k.png` y `withered_grass_nor_gl_1k.png` en una carpeta `withered_grass`.
2. `python -I B1_preparar_texturas.py <carpeta_descargas> <carpeta_salida>` (necesita Pillow). Deja `Kit/<Material>/Kit_<Material>_Color.jpg` y `_Normal.jpg`, y `Kit/Calcomanias/*.png`.

## Pasos que faltan (con el editor funcionando)

1. Copiar la salida a `Assets/05_Textures/Kit/` y dejar que Unity la importe.
2. Importación: tamaño máximo 1024, compresión por defecto, mipmaps activados; las `_Normal` como Normal map; las calcomanías con alfa transparente y Wrap Mode en Clamp.
3. Crear los materiales URP Lit en `Assets/03_Materials/Kit/` (color + normal, suavidad fija entre 0.1 y 0.5 según el material) y los de calcomanías como Transparent.
4. Escena de prueba en `_Sandbox` con un cubo por material para revisarlos con el look del juego.
5. Pasar la tabla de arriba a `CREDITS.md`.
