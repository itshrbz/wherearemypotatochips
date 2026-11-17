# Correcciones del playtest de tiendas y mapas

Rama: `polish/correcciones-tiendas-mapas`. Base: `5ad2da5`. Estado: **Para probar**.

Alcance autorizado: problemas de las capturas de `CONTEXTO CODEX`, cierre del hueco final de Pools, acceso al techo de Warehouse y mejoras puntuales de las tiendas. El pedido incluye archivos de los bloques B y E. Se conservan el recorrido, el orden aleatorio de espacios, el susto, los personajes y el significado de los diálogos. No se ejecutaron los generadores completos de niveles.

## Plan aplicado

1. Revisar las diez capturas, las escenas de juego, el prefab de réplica, los datos de ventas y los sistemas que colocan jugadores, compradores y geometría.
2. Corregir etiquetas de productos y comprador, y errores ortográficos del diálogo en inglés.
3. Separar los dinteles superpuestos de la puerta de la tienda.
4. Redistribuir y vestir la réplica dentro de sus 8 × 8 metros, dejando paso a la caja y un punto fijo para el comprador.
5. Conservar el perímetro de Pools al adaptar la réplica y bloquear el acceso a los techos de Warehouse.
6. Corregir transparencia y ajuste de manchas, reutilizar texturas existentes y verificar ventas, colisiones, navegación y transiciones.

## Qué cambió

| ID | Problema | Corrección |
| --- | --- | --- |
| B-003 | `Old Man` en una compradora; chocolate llamado `Gum`, caramelo y cupcake llamados `Soda` | `Customer`, `Chocolate`, `Candy Cane` y `Cupcake`; `Potatos` → `Potatoes` en diálogos y valores de respaldo. Los modelos y el significado de las frases se conservan |
| B-004 | Parpadeo sobre la puerta original | El dintel de la tienda empieza donde termina el del kit; marco metálico y hoja con el material de madera existente |
| B-005 | Réplica vacía, caja inaccesible y comprador dentro del mostrador | Mostrador desplazado, pasillo posterior amplio y `BuyerSpot` serializado. Estantes surtidos, heladera, baldosas, zócalos, reloj y letreros en el mismo espacio. Suelo a +2 cm y techo a −2,5 cm para separar superficies coincidentes |
| B-006 | Caída por pared norte de Pools y acceso a techos de Warehouse | Se conservan los tramos exteriores de las paredes que intersectaban la réplica. Se coloca una instancia del prefab en Pools. `ClearSpaceFor` solo oculta piezas completamente interiores; no apaga paredes que continúan fuera. Ocho barreras invisibles sobre los bordes altos de Warehouse, excluidas del horneado de navegación |
| B-007 | Rectángulos de humedad y manchas flotando junto a pilares | Mezcla alfa sin conservar brillo especular en los tres materiales de humedad; seis mallas pequeñas ajustadas a los cilindros y a sus límites verticales |
| B-008 | Materiales planos de tienda | Texturas de yeso, placas de techo, madera y asfalto ya presentes y acreditadas. UV por metros también en la tienda original. No se descargaron recursos nuevos |

Las escenas se editaron mediante el editor conectado y el Unity CLI. El diff grande del prefab corresponde a su decoración nueva, no a reconstruir los niveles. Los `.meta` nuevos de las seis mallas y su carpeta los generó Unity; no se regeneraron los existentes. Los NavMesh se hornearon conservando sus assets y GUID, y se mantuvieron los enlaces existentes.

## Pruebas ejecutadas en Unity 6000.5.5f1

- Compilación del proyecto completada sin errores. Comprobación final del editor: `compilationFailed=false`, `consoleErrors=0`.
- Auditoría de las siete escenas de juego: sin componentes con scripts faltantes; las tres réplicas tienen `BuyerSpot` válido y todos los datos de venta conservan su modelo de producto.
- Play comprobado mediante avance de `Time.frameCount` y `Time.time`.
- En los tres mapas: movimiento con el `CharacterController` real por la entrada y el pasillo posterior, suelo sólido, zona segura, comprador en su punto fijo, producto alcanzable y venta lista para escanear.
- En los tres mapas: malla animada del comprador sin vértices dentro del cuerpo del mostrador en la pose medida (`0/443`). La revisión visual manual de otras poses sigue pendiente.
- Pools: muestreo cada 0,5 m del perímetro norte fuera de la tienda, sin huecos.
- Warehouse: ocho barreras probadas con rayos y con cápsulas de 2 m y 1 m de alto; los 16 intentos de atravesarlas a la altura del techo quedaron bloqueados.
- Recorrido automatizado desde `00_Boot`: intro, dos compras, pedido de papas, susto original, puerta, tres ventas de réplicas y regreso a `02_Store` con `IsEnding=true`. Orden observado: Playground → Warehouse → Pools. Se verificó `Candy_Cane / Candy Cane`, `Cupcake / Cupcake` y `Potato_Chips_Pack / Potato Chips`.
- NavMesh tras hornear: Warehouse 1092 triángulos / 4 enlaces; Pools 1979 / 52; Playground 1032 / 16. No se modificó el comportamiento del jefe.
- Capturas revisadas de la puerta, interior de réplicas y humedad en pared y pilar: [puerta](../capturas/correcciones/puerta.png), [réplica](../capturas/correcciones/replica.png), [humedad Warehouse](../capturas/correcciones/humedad_warehouse.png), [pilar Pools](../capturas/correcciones/pilar_pools.png).

La automatización usa teletransporte para recorrer rápidamente las ventas; no equivale a completar los laberintos caminando ni a una prueba manual con teclado y mando. Quedan esas pruebas y una build WebGL en navegador. Se mantiene **Para probar** para Cristhian.

Se observó un aviso al crear el agente del jefe (`Failed to create agent because it is not close enough to the NavMesh`), tanto antes como después del horneado. El flujo completo continuó; el aviso queda para revisión del bloque C. No se alteró su prefab ni su IA para corregir estos problemas de tiendas y geometría.

## Herramienta de aplicación

`docs/pulido/herramientas/CorreccionesTiendasMapas.cs` está fuera de `Assets`: no se compila ni se incluye en el juego. Se utilizó con `unity command run_script --file ../docs/pulido/herramientas/CorreccionesTiendasMapas.cs --entry CorreccionesTiendasMapas.<método>` apuntando al proyecto de Unity. Métodos: `Texts`, `Door`, `Replica`, `Materials`, `Maps`, `Decals`, `Bake`.

Los cambios ya están guardados: **no hace falta ejecutar la herramienta para jugar**. `Replica` conserva la decoración existente si ya está aplicada. Las pruebas temporales se mantuvieron fuera de `Assets` y no se incorporaron al juego.
