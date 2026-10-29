# Mejora 2 — Nivel acuático (2-2)

Proyecto: Super Mario Bros (evaluación individual) · Unity 6000.3.16f1 LTS

Base del proyecto: tutorial **Super Mario Bros (2D)** de **Zigurous**.

> **Alcance honesto:** es un nivel acuático **inspirado** en el estilo de los
> niveles acuáticos de NES. La distribución, los sprites nuevos, los enemigos y
> la música son de diseño propio; no es una copia del World 2-2.

## 1. Auditoría

| Elemento | Estado previo | Decisión |
|---|---|---|
| Física de Mario | Solo terrestre (`PlayerMovement`) | Se añade un modo de natación como rama separada; el código terrestre no cambia |
| Animación de Mario | `PlayerSpriteRenderer` usa idle/jump/slide/run | La natación reutiliza los frames existentes (pose de salto al bracear, frames de carrera al planear) |
| Sprites acuáticos | No existían | Se crean 11 sprites nuevos (sección 3) |
| Enemigos acuáticos | No existían | Nuevos `Fish` y `Jellyfish` con base común `WaterEnemy` |
| Cámara | `SideScrolling` con franja subterránea, límite y color de fondo (Mejora 1) | Se reutiliza: franja `y = -9.5` para el agua y cielo al salir |
| Tuberías / bandera / castillo | Prefabs existentes | Se reutilizan para la salida |
| 1-2 → 2-2 | La bandera de 1-2 ya tenía `nextWorld 2 / nextStage 2` | Se conecta sola al registrar 2-2 |
| Colisiones | Matriz: Enemy↔Player activa, Enemy↔Enemy desactivada; `callbacksOnDisable = 1` | Los enemigos acuáticos usan triggers en la capa Enemy |

## 2. Mecánicas implementadas

### Natación (`PlayerMovement`, `WaterZone`)
- `WaterZone` es un trigger que cubre toda la zona submarina. Al entrar llama a `SetSwimming(true, superficie)` y al salir (también al entrar en una tubería) a `SetSwimming(false)`.
- Con `swimming = true`, `Update` usa `SwimMovement()`. Con `swimming = false` se ejecuta exactamente el código terrestre de siempre, así que 1-1 y 1-2 no cambian.
- Física (editable en el Inspector de Mario):

  | Parámetro | Valor |
  |---|---|
  | `swimSpeed` | 3.5 (horizontal, con inercia) |
  | `swimStroke` | 6 (impulso por brazada) |
  | `swimGravity` | -9 (gravedad reducida) |
  | `maxSinkSpeed` | -3 (hundimiento lento, flotabilidad) |

- Cada pulsación de Saltar es una brazada, también a media agua. Mario no puede subir por encima de la superficie, camina despacio por el fondo y conserva los choques de cabeza con bloques.
- Animación: pose de brazada durante 0,3 s tras cada impulso y ciclo de piernas mientras planea.
- Burbujas que salen de Mario al bracear y cada 1,5 s (`Bubble`).

### Enemigos
- **Pez (`Fish`):** cruza la pantalla en horizontal con ondulación senoidal; velocidad y amplitud configurables. Los rápidos (≥ 3,3) se tiñen de rojo para indicar el peligro.
- **Medusa (`Jellyfish`):** se hunde contraída, y cuando queda por debajo de Mario se impulsa en diagonal hacia él con los tentáculos extendidos. Se mantiene entre el fondo y la superficie.
- Ambos (`WaterEnemy`):
  - se activan al acercarse a la pantalla y se destruyen al quedar muy atrás;
  - atraviesan las rocas, como en el género;
  - **no se pueden pisar**: el contacto daña a Mario;
  - mueren con la estrella, con `DeathAnimation` y sonido.

### Audio (síntesis original)
- Música acuática: vals en 3/4 de 6 s en bucle (`LevelMusic` con `theme = Water`).
- Efecto de brazada; la medusa lo usa en volumen bajo al impulsarse.
- Se reutilizan los efectos de la Mejora 1 (moneda, golpe, power-up, tubería, bandera...).

## 3. Recursos nuevos

Generados por `Tools/generate_water_sprites.py` (pixel art propio, 16 px/unidad):

| Sprite (`Assets/Sprites/Water/`) | Uso |
|---|---|
| `Water_Seafloor` | Fondo marino (bloque tileable) |
| `Water_Rock` | Rocas, paredes, ruinas |
| `Water_Coral` | Columnas de coral (sólidas) |
| `Water_Surface` | Oleaje de la superficie (decorativo) |
| `Water_Seaweed1/2` | Algas animadas (decorativas) |
| `Fish_Swim1/2` | Pez, 2 frames |
| `Jellyfish_Contracted/Extended` | Medusa, 2 estados |
| `Water_Bubble` | Burbujas |

Prefabs nuevos (`Assets/Prefabs/Water/`, **los crea el menú**): `Seafloor`, `Rock`, `Coral` (mismo patrón que `UndergroundBlock`: Tiled y BoxCollider2D con autoTiling), `Fish` y `Jellyfish` (capa Enemy, Rigidbody2D cinemático, collider trigger, `DeathAnimation`).

## 4. Diseño del nivel (8 secciones, 56 tiles cada una)

| # | Sección | Contenido | Dificultad |
|---|---|---|---|
| 1 | Arrecife de entrada | Fondo plano, corales bajos, bloque "?", peces lentos | Introducción a la natación |
| 2 | Bosque de coral | Columnas desde el fondo y rocas colgantes alternadas, primera medusa | Esquivar en vertical |
| 3 | La fosa | Dos fosas mortales, roca flotante con champiñón encima | Controlar el hundimiento |
| 4 | Corredor estrecho | Banda de roca que divide el agua en dos pasillos estrechos, bloques en el inferior | Precisión |
| 5 | Laberinto de cavernas | Muros en zig-zag (huecos arriba / abajo), medusas en las cámaras, estrella | Navegación |
| 6 | Corriente de peces | Banco de 8 peces a distintas alturas y velocidades | Reflejos |
| 7 | Ruinas sumergidas | Arco con monedas, bloques con 1-up, escalera rota | Recompensa |
| 8 | Salida | Dos medusas, peces rápidos y tubería lateral | Prueba final |

- **Salida:** tubería lateral (pulsar **D**) → superficie con cielo → escalera → bandera y castillo.
- **Bandera:** apunta a `8-4`, nombre previsto para el Castillo de Bowser. Mientras esa escena no exista, `GameManager.LoadLevel` registra un aviso y vuelve a 1-1 conservando vidas y monedas. Si el castillo tendrá otro nombre, basta con cambiar `nextWorld/nextStage` en el Inspector de la bandera.
- **Cámara:** seguimiento lateral; altura fija en el agua (`-9.5`) y `6.5` en la superficie; límite derecho en 480; fondo azul agua (`#2154C7`) bajo el agua y cielo arriba.
- **Longitud:** unas 440 unidades bajo el agua. A 3,5 u/s el recorrido en línea recta lleva unos 2 minutos; con laberinto, enemigos y monedas, **se estima entre 3 y 5 minutos. Esta cifra no se ha medido.** Si hace falta, se ajusta con `SectionWidth`, `swimSpeed` o añadiendo secciones.

## 5. Archivos

### Creados
- Scripts: `WaterZone.cs`, `Bubble.cs`, `WaterEnemy.cs`, `Fish.cs`, `Jellyfish.cs`
- Editor: `WaterLevelBuilder.cs`, `BuildSceneList.cs`
- Sprites: `Assets/Sprites/Water/*.png` (11) y su generador `Tools/generate_water_sprites.py`
- Generados por el menú: `Assets/Prefabs/Water/*.prefab` (5) y `Assets/Scenes/2-2.unity`
- `Docs/Mejora2_NivelAcuatico.md`

### Modificados
| Archivo | Cambio |
|---|---|
| `PlayerMovement.cs` | Parámetros de natación, `SetSwimming()`, `SwimMovement()` y la bifurcación al inicio de `Update` |
| `PlayerSpriteRenderer.cs` | Animación de natación al inicio de `LateUpdate` (solo si `swimming`) |
| `ChiptuneSynth.cs` | `WaterTheme` y `Swim` |
| `LevelMusic.cs` | Enum `Theme` (Underground por defecto, así que 1-2 no cambia) |

Ninguna escena, prefab, sprite ni `.meta` existente se modificó.

## 6. Pasos en Unity 6 (6000.3.16f1)

1. Vuelve al Editor y espera a que compile. Solo deben quedar los avisos `isKinematic is obsolete`, que ya existían.
2. **Tools › Super Mario › Crear nivel acuático 2-2**. El menú:
   - configura la importación de los sprites de `Assets/Sprites/Water`;
   - crea los prefabs de `Assets/Prefabs/Water`;
   - genera y guarda `Assets/Scenes/2-2.unity`;
   - la añade a Build Profiles (sin quitar ni reordenar escenas) y la guarda en disco;
   - vuelve a la escena en la que estabas.
3. En **File › Build Profiles › Scene List** deben aparecer `1-1`, `GameOver`, `1-2` y `2-2`.
4. **Prueba directa:** abre `2-2.unity` y pulsa Play.
5. **Flujo completo:** juega desde 1-1, termina 1-2 y debería cargar 2-2.

## 7. Controles

| Acción | Tecla |
|---|---|
| Nadar horizontal | A/D o ←/→ |
| Brazada (subir) | Espacio, cada pulsación |
| Hundirse | Soltar (flotabilidad) |
| Entrar en la tubería de salida | D |

## 8. Pruebas pendientes (no ejecutadas)

| # | Caso | Esperado |
|---|---|---|
| 1 | Menú de generación | Sin errores; prefabs y escena creados; 4 escenas en Build Profiles |
| 2 | Natación | Brazadas, hundimiento lento, límite en la superficie, caminar por el fondo |
| 3 | Animación | Pose de brazada, ciclo de piernas, burbujas |
| 4 | Colisiones | Fondo, rocas, corales y muros sólidos; las fosas matan |
| 5 | Peces | Aparecen al acercarse, ondulan, dañan al contacto, mueren con la estrella |
| 6 | Medusas | Se hunden, persiguen con impulsos, no salen del agua |
| 7 | Bloques | "?" con moneda, champiñón, estrella y 1-up se golpean nadando |
| 8 | Salida | D en la tubería → superficie con cielo → bandera → aviso de 8-4 → 1-1 con vidas y monedas |
| 9 | Vidas | Muerte → reinicia 2-2; 0 vidas → GameOver; REINTENTAR → 1-1 con 3 vidas |
| 10 | Regresión | 1-1 y 1-2 sin cambios en movimiento terrestre |
| 11 | Duración | Medir la primera partida y ajustar a 4-6 min si hace falta |
| 12 | Dificultad | Pasillos de las secciones 4 y 5 transitables con Mario grande (2 tiles de alto) |

## 9. Problemas conocidos y limitaciones

- Al morir se reinicia desde el principio del nivel (no hay puntos de control); en un nivel largo puede resultar duro.
- Los enemigos atraviesan las rocas, por diseño (se mueven por transform).
- La natación reutiliza frames terrestres de Mario; no hay sprites de nado dedicados.
- Las monedas y los bloques "?" usan los sprites del exterior.
- Los champiñones y la estrella que salen de los bloques caen con la gravedad normal también bajo el agua.
- En la sección 4, el coral del pasillo inferior deja 3 tiles de altura: sitio justo para Mario grande.

## 10. Diferencias con el World 2-2 original

- Distribución, longitud y secuencia de diseño propio.
- No hay escena exterior inicial: Mario empieza ya bajo el agua.
- Pez y medusa son diseños originales. Se inspiran en el comportamiento de los peces y calamares del género, pero no son los sprites de Nintendo.
- Música y efectos sintetizados de elaboración propia.
- La salida apunta al futuro castillo (`8-4`), no a 2-3.

## 11. Créditos

- Código base, sprites y prefabs originales: tutorial de **Zigurous**.
- Sprites acuáticos, enemigos, audio y diseño del nivel: elaboración propia.
- Super Mario Bros es una marca de Nintendo; proyecto académico no comercial.
