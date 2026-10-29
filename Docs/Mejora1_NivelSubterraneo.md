# Mejora 1 — Nivel subterráneo (1-2)

Proyecto: Super Mario Bros (evaluación individual) · Unity 6000.3.16f1 LTS

Base del proyecto: tutorial **Super Mario Bros (2D)** de **Zigurous**
(https://github.com/zigurous/unity-super-mario-tutorial), proporcionado por el
docente. Se reutilizan sus sprites, prefabs y scripts; no se añadieron assets
gráficos nuevos.

> **Alcance honesto:** el nivel usa la estética, los elementos y las mecánicas
> clásicas de un nivel subterráneo de Super Mario Bros, pero **su distribución
> es un diseño propio. No es una copia tramo a tramo del World 1-2 de NES.**
> Las diferencias se detallan en la sección 6.

## 1. Auditoría

| Recurso | Uso en 1-2 |
|---|---|
| `UndergroundBlock.prefab` (Tiled, BoxCollider2D) | Suelo, paredes, escaleras, plataformas, techo bajo |
| `UndergroundBrick.prefab` / `.png` | Techo (sin interacción) y fondo decorativo atenuado |
| `Brick.prefab` + sprite `UndergroundBrick.png` | Ladrillos interactivos (rompibles) |
| `MysteryBlock.prefab` | Bloques "?" con moneda, champiñón, 1-up y estrella |
| `Coin.prefab` + sprite `Coin_Underground.png` | Monedas subterráneas (este sprite no lo usaba ningún prefab) |
| `Pipe.prefab`, `PipeConnection.prefab` | Tubería de entrada, tuberías obstáculo, salida a la superficie |
| `Goomba.prefab`, `Koopa.prefab` | Enemigos sin cambios |
| `Mario.prefab` | Jugador sin cambios (física, controles, animaciones) |
| `Ground`, `HardBlock`, `Flag Pole`, `Castle` | Tramo final en superficie |
| `SideScrolling` | Ya tenía modo subterráneo (`undergroundHeight = -9.5`), reutilizado |

Hallazgos:
- Los ladrillos originales no se rompen (`maxHits = -1`, solo rebotan).
- El proyecto no tenía ningún efecto de sonido de gameplay.
- No existe un sprite de "bloque vacío" en versión subterránea.
- La bandera de 1-1 apuntaba a `1-1`.
- `ProjectSettings/EditorBuildSettings.asset` en disco solo contenía 1-1: GameOver estaba registrado en memoria, pero no guardado. Ahora ambos builders llaman a `AssetDatabase.SaveAssets()`.

## 2. Funcionalidades implementadas

- **Escena `Assets/Scenes/1-2.unity`** generada con el menú del Editor. La jerarquía queda así:
  ```
  Game Manager · Music · Mario · Main Camera
  Level 1-2
  ├── Underground
  │   ├── Terrain     (suelo con fosos, paredes, techo, escaleras, plataformas)
  │   ├── Blocks      (ladrillos rompibles y bloques ?)
  │   ├── Coins
  │   ├── Pipes       (entrada colgando del techo + 3 tuberías)
  │   ├── Enemies     (Goombas y Koopas)
  │   ├── Decoration  (muros de fondo sin colisión, orden -5)
  │   └── Exit        (tubería lateral de salida)
  ├── Surface
  │   ├── Terrain     (suelo y escalera de bloques duros)
  │   ├── Pipes       (tubería por la que sale Mario)
  │   └── Exit        (bandera y castillo)
  └── Death Barrier
  ```
- **Recorrido en 5 tramos de dificultad creciente:**
  - A: entrada y primeros bloques.
  - B: escalera con plataforma y tuberías de altura creciente.
  - C: puente de ladrillos, 1-up oculto y pasillo de techo bajo con enemigos.
  - D: foso ancho con plataformas flotantes y muro de ladrillos.
  - E: subida final y tubería de salida.
- **Salida:** pulsando `D` en la tubería lateral, Mario sale a la superficie. La cámara cambia de altura y el fondo pasa de negro a azul cielo. Después vienen la bandera y el castillo.
- **Bandera de 1-2 → 2-2:** mientras 2-2 no exista o no esté en Build Profiles, `GameManager.LoadLevel` registra un aviso y vuelve a 1-1 conservando vidas y monedas, en lugar de fallar. Cuando se cree `2-2.unity` y se registre, la bandera llevará allí sin cambios de código.
- **1-1 → 1-2:** el builder pone en la bandera de 1-1 un override `nextStage = 2`.
- **Vidas y Game Over:** sin cambios. Al morir se reinicia 1-2 (desde el inicio) y con 0 vidas se abre GameOver. Vidas y monedas se conservan entre niveles.
- **Cámara:** seguimiento lateral original, con dos opciones nuevas en `SideScrolling`: límite derecho (`limitRight`/`rightLimit = 196`, para que no pase del castillo) y cambio del color de fondo al entrar o salir del subsuelo (`changeBackground`). En 1-1 ambas están desactivadas.
- **Ladrillos rompibles:** nuevo campo `BlockHit.breakable`. Mario grande rompe los ladrillos vacíos, que saltan en 4 fragmentos animados (`BrickDebris`); Mario pequeño solo los golpea. En 1-1 está desactivado.
- **Audio (síntesis original, sin archivos externos):**
  - Música de fondo en bucle del subterráneo (`LevelMusic`), que se detiene al morir o al tocar la bandera.
  - Efectos de salto, moneda, golpe a bloque, ladrillo roto, pisotón, patada al caparazón, tubería/daño, power-up, 1-up y bandera.
  - Los efectos también suenan en 1-1.

## 3. Archivos

### Creados
| Archivo | Función |
|---|---|
| `Assets/Editor/UndergroundLevelBuilder.cs` | Menús `Crear nivel subterráneo 1-2` y `Enlazar bandera de 1-1 con 1-2` |
| `Assets/Scripts/Sfx.cs` | Reproductor persistente de efectos de sonido |
| `Assets/Scripts/LevelMusic.cs` | Música de nivel en bucle |
| `Assets/Scripts/BrickDebris.cs` | Fragmentos de ladrillo roto |
| `Assets/Scenes/1-2.unity` | **La genera el menú** (no se escribió el YAML a mano) |
| `Docs/Mejora1_NivelSubterraneo.md` | Este documento |

### Modificados
| Archivo | Cambio |
|---|---|
| `ChiptuneSynth.cs` | Onda de ruido, barridos de tono, caché; tema subterráneo y 10 efectos. Los jingles de muerte y Game Over no cambian |
| `GameManager.cs` | `LoadLevel` comprueba que la escena existe (si no, vuelve a 1-1 conservando el estado) |
| `SideScrolling.cs` | Límite derecho opcional y cambio opcional del color de fondo |
| `BlockHit.cs` | Campo `breakable`, `Break()` con fragmentos y sonido de golpe |
| `PlayerMovement.cs` | 1 línea: sonido de salto |
| `PowerUp.cs`, `BlockCoin.cs`, `Goomba.cs`, `Koopa.cs`, `Pipe.cs`, `Player.cs`, `FlagPole.cs` | Llamadas a `Sfx.Play(...)`; `FlagPole` además detiene la música |
| `Assets/Editor/GameOverSceneBuilder.cs` | Guarda la lista de escenas en disco (`AssetDatabase.SaveAssets`) |
| `Assets/Scenes/1-1.unity` | **Lo modifica el menú:** override `nextStage = 2` en la bandera |

Ningún prefab, sprite ni `.meta` existente se modificó.

## 4. Pasos en Unity 6 (6000.3.16f1)

1. Vuelve al Editor y espera a que compile. La consola debe quedar sin errores rojos; los avisos `isKinematic is obsolete` ya existían.
2. **Tools › Super Mario › Crear nivel subterráneo 1-2**. Si hay cambios sin guardar, Unity preguntará si guardarlos. El menú:
   - crea y guarda `Assets/Scenes/1-2.unity`;
   - añade a la lista de escenas las que falten (1-1, GameOver, 1-2) sin quitar ninguna y la guarda en disco;
   - abre 1-1, cambia su bandera a 1-2, la guarda y vuelve a la escena en la que estabas.
3. Comprueba en **File › Build Profiles › Scene List**: `Scenes/1-1` (índice 0), `Scenes/GameOver` y `Scenes/1-2`. Si un perfil de plataforma usa una lista propia (override), añade ahí también GameOver y 1-2.
4. Abre `Assets/Scenes/1-2.unity` y pulsa Play para probarlo directamente (la partida empieza en 1-2 con 3 vidas). Para el flujo completo, abre 1-1 y termina el nivel.

## 5. Controles

| Acción | Tecla |
|---|---|
| Mover / saltar | A/D o ←/→, Espacio |
| Entrar en tubería vertical | S (comportamiento original) |
| Entrar en tubería lateral (salida de 1-2) | D (comportamiento original del prefab `PipeConnection`) |

## 6. Diferencias con el World 1-2 original

- **Distribución:** diseño propio con los elementos del género. No reproduce la disposición, longitud ni secuencia exactas del nivel de NES.
- **Inicio:** no hay escena previa en el exterior. Mario cae desde una tubería colgada del techo.
- **Zonas de warp:** no existen (no hay niveles 2-1, 3-1 ni 4-1).
- **Plataformas móviles (ascensores):** el proyecto no tiene ese prefab ni ese script, así que no se incluyen.
- **Bloque vacío:** al vaciarse, un bloque usa el sprite `EmptyBlock` marrón del exterior (no hay versión azul).
- **Ladrillos de fondo:** no hay sprites de muro subterráneo de fondo; se usan los ladrillos subterráneos atenuados.
- **Monedas que salen de los bloques:** usan el sprite de moneda del exterior (`BlockCoin.prefab`).
- **Destino de la bandera:** apunta a 2-2 (el siguiente nivel de esta evaluación), no a 1-3.
- **Música y efectos:** son composiciones sintetizadas propias, no el audio de Nintendo.
- **Reinicio tras morir:** se vuelve al inicio de 1-2; no hay punto de control a mitad del nivel.

## 7. Pruebas

Estado: **pendientes de ejecutar en Unity por el autor**. La escena aún no se ha generado ni jugado. Los scripts de runtime compilan sin errores (log del Editor); el builder de Editor no se había compilado aún al cierre de este documento.

| # | Caso | Resultado esperado |
|---|---|---|
| 1 | Menú de generación | Escena creada, 3 escenas en Build Profiles, consola sin errores |
| 2 | Play en 1-2 | Fondo negro, música en bucle, Mario cae de la tubería, la cámara sigue a la derecha |
| 3 | Colisiones | Suelo, techo, escaleras, plataformas y tuberías sólidas; los fosos matan |
| 4 | Bloques | "?" dan moneda, champiñón, 1-up o estrella; Mario grande rompe ladrillos, el pequeño solo los golpea |
| 5 | Enemigos | Goombas y Koopas caminan, se pisan y el caparazón se puede patear |
| 6 | Salida | `D` en la tubería lateral → superficie con cielo azul → bandera y castillo |
| 7 | Bandera de 1-2 | Aviso de que 2-2 no existe en consola, vuelta a 1-1 con las mismas vidas y monedas |
| 8 | 1-1 → 1-2 | Al terminar 1-1 se carga 1-2 conservando vidas y monedas |
| 9 | Muerte en 1-2 | Se reinicia 1-2 con una vida menos; con 0 vidas aparece GameOver; REINTENTAR → 1-1 con 3 vidas |
| 10 | Regresión en 1-1 | Movimiento, enemigos, tubería subterránea y cámara como antes, ahora con sonidos |
| 11 | GameManager | Un solo `Game Manager` en DontDestroyOnLoad tras cambiar de nivel |

## 8. Créditos

- Código base, sprites y prefabs: tutorial *Super Mario Bros (2D)* de **Zigurous**.
- Super Mario Bros es una marca de Nintendo; proyecto académico no comercial.
- Audio sintetizado, distribución del nivel y scripts nuevos: elaboración propia.
