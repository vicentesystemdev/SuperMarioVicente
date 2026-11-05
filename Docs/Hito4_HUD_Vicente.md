# Hito 4 — HUD, puntuación y temporizador (Vicente Claros)

**Rama:** `feature/vicente-hud-puntaje-tiempo` · **Unity:** 6000.3.16f1 · **Responsable:** Vicente Claros ([vicentesystemdev](https://github.com/vicentesystemdev))

> Este trabajo se apoya en el proyecto base (tutorial de Zigurous y el repositorio proporcionado) y en lo que el equipo ya había hecho. Los créditos originales siguen en el [README](../README.md).

## 1. Auditoría previa

| Elemento | Situación encontrada |
|---|---|
| `GameManager` | Es un singleton con `DontDestroyOnLoad`. Hay **una copia en cada nivel** (1-1, 1-2, 2-2, 8-4); las copias que sobran se destruyen en `Awake`. Guarda mundo, nivel, vidas, monedas y checkpoint, y crea el `ScreenFader` en tiempo de ejecución. |
| HUD | No existía. Solo había la barra de vida del jefe en 8-4 (`Boss UI`, canvas con orden 10, centrada arriba). |
| Puntuación / tiempo | No existían. |
| Texto | `PixelText`: fuente pixelada 5x7 integrada, sin recursos externos. |
| GameOver / Victory | Escenas sin `GameManager`; leen el que viene del nivel anterior para mostrar sus datos. |
| Menú principal | `MainMenu` todavía no está en el repositorio (trabajo de Víctor). No se ha tocado. |

**Decisión de arquitectura:** el HUD lo crea **el `GameManager` persistente en tiempo de ejecución**, igual que hace con el `ScreenFader`. Así:

- No se modifica **ninguna escena** (`.unity`), así que no hay conflictos con las escenas de 2-2 y 8-4 de mis compañeros.
- Solo hay **un HUD** en todo el juego, porque es hijo del único `GameManager` que sobrevive. Al cambiar de escena no se duplica.

## 2. Funcionalidad implementada

### HUD estilo NES (`Assets/Scripts/GameHud.cs`, nuevo)
- Cuatro columnas: `MARIO 000000` · `MONEDAS ×00` · `MUNDO 1-1` · `TIEMPO 400`, con fuente pixelada y sombra.
- `ScreenSpaceOverlay` con `CanvasScaler` de referencia 1920x1080 y `match 0.5`, la misma configuración que el resto de canvas del proyecto.
- Ocupa solo una franja superior de unas 76 unidades de referencia (alrededor del 7 % de la altura) y se dibuja sobre el cielo, como en NES.
- Las columnas van ancladas a los **bordes izquierdo y derecho**. El centro queda libre para la barra de vida de MAGMOR en 8-4. Comprobado con números (no visualmente): no se solapan en 16:9, 4:3 ni 5:4.
- Orden de canvas 100: por encima de la UI del nivel y por debajo del fundido (`short.MaxValue`).
- Solo se ve en las escenas de nivel (`X-Y`). En `GameOver` y `Victory` se oculta.
- Solo redibuja una etiqueta cuando cambia su valor.
- El tiempo se pone rojo cuando quedan 100 o menos.

### Puntuación (`GameManager.AddScore`)
| Acción | Puntos | Dónde |
|---|---|---|
| Moneda (suelta o de bloque) | 200 | `GameManager.AddCoin` (centralizado) |
| Pisotón a Goomba / Koopa | 100 | `Goomba.Flatten`, `Koopa.EnterShell` |
| Enemigo eliminado con caparazón o estrella | 200 | `Goomba.Hit`, `Koopa.Hit`, `WaterEnemy.Hit` |
| Champiñón / estrella | 1000 | `PowerUp.Collect` |
| Derrotar a MAGMOR | 5000 | `Boss.Defeat` |
| Bonus de tiempo al terminar el nivel | 50 × tiempo restante | `GameManager.LevelComplete` |

- La puntuación se acumula entre niveles y tras morir. Solo vuelve a 0 al empezar una partida nueva. El máximo es 999999.

**Protección contra puntos duplicados:**
- `Goomba`, `Koopa`: flag `defeated`. Antes, un Goomba podía recibir `Hit()` varias veces de un caparazón o de un pisotón seguido de un caparazón.
- `WaterEnemy.Hit()`: ahora no hace nada si el enemigo ya está muerto. Antes era público y no lo comprobaba.
- `PowerUp`: flag `collected`. `Destroy` no surte efecto hasta el final del frame, así que un segundo trigger en el mismo frame daba el objeto dos veces.
- `LevelComplete()`: el bonus de tiempo solo se suma una vez por nivel (si `FlagPole` se activa varias veces o si se llama a `Victory()`).

### Temporizador
- Es descendente y vuelve al valor completo cada vez que se carga un nivel, también al reaparecer tras morir (como en NES).
- **Configuración por nivel:** componente opcional `LevelSettings` (`Assets/Scripts/LevelSettings.cs`, nuevo) con `timeLimit` y `secondsPerTick`. Si el nivel no lo tiene, se usa el valor por defecto: **400 unidades a 0,5 s = 200 s reales**.
- **Se pausa** cuando Mario no tiene el control (`PlayerMovement` desactivado: tuberías, mástil, meta del castillo). Así el final de un nivel nunca se queda sin tiempo.
- **Al llegar a 0:** `Player.Death()` hace la animación de muerte, suena el jingle, se pierde una vida y el nivel se reinicia (o salta a Game Over si no quedan vidas). Si Mario está inactivo, se llama directamente a `ResetLevel(3f)`. Una sola muerte nunca quita más de una vida (lo controla el `resetting` que ya existía).
- **Al completar el nivel:** `FlagPole` (1-1) y `Victory()` (8-4) paran el reloj y convierten el tiempo restante en puntos.

### GameOver y Victory
- `GameOverScreen`: la etiqueta pasa a mostrar `MUNDO X-Y  PUNTOS 000000`.
- `VictoryScreen`: las estadísticas pasan a mostrar `PUNTOS 000000  MONEDAS N  VIDAS N`.
- El HUD se oculta en ambas escenas. Al pulsar REINTENTAR o NUEVA PARTIDA se llama a `NewGame()`, que pone la puntuación a 0 y vuelve a mostrar el HUD en 1-1.

## 3. Revisión de SFX

Revisión **estática del código**. Falta comprobar los sonidos jugando (ver sección 6).

| SFX | Dónde suena | Estado |
|---|---|---|
| Salto | `PlayerMovement.GroundedMovement` → `ChiptuneSynth.Jump` (en el agua: `Swim`) | Existe |
| Pisotón | `Goomba.Flatten`, `Koopa.EnterShell` → `Stomp` | Existe |
| Power-up | `PowerUp.Collect` → `PowerUpCollect` (champiñón/estrella), `OneUp`, `Coin` | Existe |
| Daño | `Player.Shrink` → `PipeWarp` (Mario grande que recibe un golpe) | Existe (reutiliza el sonido de tubería, como en NES) |
| Muerte | `GameManager.ResetLevel` → `DeathJingle` (enemigo, foso, peligros, tiempo agotado) | Existe |
| **Enemigo eliminado con caparazón o estrella** | `Goomba.Hit`, `Koopa.Hit` | **Corregido:** antes no sonaba nada. Ahora suena `Kick`, igual que en `WaterEnemy` |

Observación sin cambios: cuando sale un objeto de un bloque (`BlockItem`) no suena nada. En NES sí hay sonido. Queda como mejora opcional.

## 4. Archivos modificados

| Archivo | Tipo | Cambio | Impacto |
|---|---|---|---|
| `Assets/Scripts/GameHud.cs` (+ `.meta`) | **Nuevo** | HUD | — |
| `Assets/Scripts/LevelSettings.cs` (+ `.meta`) | **Nuevo** | Configuración opcional por nivel | — |
| `Assets/Scripts/GameManager.cs` | Compartido | Añade `score`, `time`, `timerRunning`, `hurryUp`, `AddScore`, `LevelComplete`, `StartLevel`, `Update`, `TimeUp`, y crea el HUD en `Awake`. Llamadas añadidas en `Start`, `ResetGameState`, `ResetLevel(float)`, `OnSceneLoaded`, `Victory` y `AddCoin` | Solo añade cosas: no cambia la API pública que ya existía. Vidas, monedas, checkpoints y transiciones funcionan igual |
| `Assets/Scripts/Goomba.cs` | Base | Flag `defeated`, puntos y SFX `Kick` en `Hit` | Un Goomba ya derrotado no vuelve a reaccionar |
| `Assets/Scripts/Koopa.cs` | Base | Flag `defeated`, puntos y SFX `Kick` en `Hit` | Igual |
| `Assets/Scripts/PowerUp.cs` | Base | Flag `collected` y puntos de power-up | Ninguno visible |
| `Assets/Scripts/FlagPole.cs` | Base | Llama a `GameManager.Instance.LevelComplete()` | Una línea |
| `Assets/Scripts/WaterEnemy.cs` | **Nivel acuático (compañera)** | Guard `if (dead) return;` y puntos | 5 líneas. No cambia el diseño ni el comportamiento del nivel |
| `Assets/Scripts/Boss.cs` | **Castillo (Marcelo)** | `AddScore(BossPoints)` en `Defeat()` | 4 líneas. No cambia el combate. Avisar a Marcelo por si hay conflicto de merge |
| `Assets/Scripts/GameOverScreen.cs` | Compartido | Texto de la etiqueta con puntos | Una línea |
| `Assets/Scripts/VictoryScreen.cs` | Compartido | Texto de estadísticas con puntos | Una línea |

**No se modificaron** las escenas (`.unity`), los prefabs, los builders de Editor, el menú principal, el diseño de 2-2 ni el de 8-4.

## 5. Configuración en Unity

1. Abre el proyecto y espera a que compile. La consola no debe mostrar errores rojos.
2. **No hace falta configurar nada más:** el HUD aparece solo en cualquier nivel que tenga `GameManager`.
3. *(Opcional)* Para cambiar el tiempo de un nivel: crea un GameObject vacío `LevelSettings` en esa escena, añade el componente **LevelSettings** y ajusta `Time Limit` y `Seconds Per Tick`. En 2-2 y 8-4, pídeselo antes a quien se encarga de esa escena.
4. Asegúrate de que en **Build Profiles** están 1-1, 1-2, 2-2, 8-4, GameOver y Victory. Ya están en `EditorBuildSettings.asset`.

## 6. Verificación y pruebas

**Ya verificado:** compilación de todos los scripts de juego con el compilador Roslyn de Unity 6000.3.16f1 y el archivo de respuesta del proyecto (`Assembly-CSharp.rsp`): **0 errores**. Los únicos avisos son los `CS0618 isKinematic` que ya existían. El Editor abierto también recompiló con `Tundra build success`.

**Pendiente (pruebas manuales en Play Mode):**

| # | Prueba | Resultado esperado |
|---|---|---|
| 1 | Play en 1-1 | El HUD arriba muestra `MARIO 000000`, `MONEDAS ×00`, `MUNDO 1-1`, `TIEMPO 400`, y el tiempo baja 1 cada 0,5 s |
| 2 | Coger una moneda / golpear un bloque de moneda | Monedas +1, puntos +200, suena la moneda |
| 3 | Pisar un Goomba | +100, suena el pisotón |
| 4 | Pisar un Koopa y patear el caparazón contra otros enemigos | +100 por el pisotón y +200 por cada enemigo, **una sola vez** por enemigo, suena `Kick` |
| 5 | Coger un champiñón | +1000, Mario crece, suena el power-up |
| 6 | Recibir un golpe siendo grande | Mario se encoge, suena el daño y parpadea |
| 7 | Saltar | Suena el salto |
| 8 | Morir (enemigo o foso) | Suena el jingle, se pierde 1 vida, el tiempo vuelve a 400, y los puntos y monedas se conservan |
| 9 | Tiempo agotado (poner `Time Limit = 5` con `LevelSettings` en una copia de la escena o en 1-1, y **revertirlo después**) | Al llegar a 000: muerte, jingle, −1 vida, reinicio. Por debajo de 100 el tiempo se ve rojo |
| 10 | Tocar el mástil en 1-1 | El reloj se para, se suman 50 × tiempo restante, se carga 1-2 y el HUD muestra `MUNDO 1-2` con el tiempo a 400 |
| 11 | Recorrer 1-2 → 2-2 → 8-4 | Un solo `HUD` (Hierarchy → DontDestroyOnLoad → GameManager → HUD), sin duplicados; en 2-2 se suman puntos al matar peces o medusas con la estrella |
| 12 | En 8-4 iniciar el jefe | La barra de MAGMOR (centro) y el HUD (lados) no se solapan. Al derrotarlo, +5000 |
| 13 | Tocar el tesoro final | Bonus de tiempo y escena Victory con `PUNTOS ... MONEDAS ... VIDAS ...`, sin HUD |
| 14 | Perder todas las vidas | GameOver muestra `MUNDO X-Y  PUNTOS ...`, sin HUD. REINTENTAR → 1-1 con puntos 000000 y 3 vidas |
| 15 | Resoluciones (Game view: 1920x1080, 1280x720, 4:3, 16:10, Free Aspect estrecho) | El HUD se escala, las columnas no se salen de la pantalla y la franja superior no tapa plataformas ni enemigos importantes |
| 16 | Tiempo por defecto en 2-2 y 8-4 | Comprobar jugando que 200 s reales alcanzan para terminar. Si no, añadir `LevelSettings` en esa escena (coordinar con su responsable) |

## 7. Limitaciones conocidas

- La duración de 200 s se ha elegido sin probar los niveles. Hay que confirmarla con la prueba 16.
- No hay aviso sonoro de "hurry up" a los 100 segundos, solo el color rojo.
- No hay números flotantes de puntos encima de los enemigos.
- Si Víctor incluye un `GameManager` en `MainMenu`, su `Start()` llama a `NewGame()` y carga 1-1 nada más arrancar. Eso ya pasaba antes y no lo introduce este cambio. Lo recomendable es que el menú **no** lleve `GameManager` y que cargue `1-1` con `SceneManager.LoadScene`.
