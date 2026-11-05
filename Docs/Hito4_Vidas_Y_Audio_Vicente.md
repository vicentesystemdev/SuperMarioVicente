# Hito 4 — Pantalla de vidas y audio de fin de nivel (Vicente Claros)

**Rama de trabajo:** `feature/vicente-hud-puntaje-tiempo` (rama actual; no se ha cambiado de rama) · **Unity:** 6000.3.16f1 · **Responsable:** Vicente Claros ([vicentesystemdev](https://github.com/vicentesystemdev))

> Este trabajo amplía el HUD documentado en [Hito4_HUD_Vicente.md](Hito4_HUD_Vicente.md). Los créditos del proyecto base siguen en el [README](../README.md). Toda la música nueva es **original** y se sintetiza con `ChiptuneSynth`; no copia melodías comerciales.

## 1. Auditoría previa

| Archivo | Hallazgo |
|---|---|
| `GameManager.cs` | `ResetLevel(float)` evita reinicios dobles con `resetting`. Hace un fundido a negro y después `ResetLevel()` descuenta la vida y recarga el nivel o va a GameOver. No había pantalla intermedia. |
| `FlagPole.cs` | **No tenía protección contra activaciones repetidas.** Si el trigger se volvía a disparar mientras Mario bajaba por el mástil, se repetían el sonido, las corrutinas y la carga de nivel. No había fanfarria y la espera de `2f` era fija, sin relación con el audio. |
| `LevelMusic.cs` | Tiene `Stop()`, que ya usaban `FlagPole` y `VictoryGoal`. 1-1 no tiene `LevelMusic` (la búsqueda devuelve `null` y no pasa nada). |
| `ChiptuneSynth.cs` | Ya tenía `Flag` (barrido descendente de 0,9 s) y `Fanfare` (la del tesoro de 8-4). No había fanfarria de nivel completado. |
| `Sfx.cs` | Un `AudioSource` persistente con `PlayOneShot`. Los sonidos siguen sonando aunque se destruya el objeto que los lanzó o cambie la escena. |
| Escenas con bandera | El prefab `Flag Pole` está en **1-1, 1-2 y 2-2**. Basta con cambiar `FlagPole.cs`; **no hace falta tocar ninguna escena**. |

## 2. Mejora 1 — Pantalla intermedia de vidas

**Diseño:** la clase nueva `LivesScreen` es una interfaz temporal que crea en tiempo de ejecución el `GameManager` persistente, igual que el HUD y el `ScreenFader`. No hay escenas nuevas y solo existe una pantalla en todo el juego.

**Secuencia** (`GameManager.ResetLevelRoutine`):
1. Mario muere y se ejecuta la animación de muerte de siempre (3 s) con el jingle. Mientras tanto, `resetting = true` y el reloj está parado.
2. Fundido a negro.
3. Si después de descontar la vida **quedan vidas**:
   - Se oculta el HUD.
   - Se congela el nivel antiguo (`Time.timeScale = 0`) para que no se oigan enemigos ni cañones detrás.
   - Se muestra la pantalla: `MUNDO X-Y`, el sprite de Mario pequeño y `× 02`.
   - Se quita el fundido, la pantalla se queda visible `livesScreenDuration` segundos (2,5 por defecto, configurable en el Inspector del `GameManager`) y se vuelve a fundir a negro.
   - Se oculta la pantalla y se restaura `timeScale = 1`.
4. `ResetLevel()`, **sin cambios**, descuenta exactamente una vida y recarga el nivel. Si no quedan vidas, carga GameOver **directamente**, sin pasar por la pantalla.
5. Al cargar la escena: se quita el fundido, el HUD vuelve a verse y el tiempo se reinicia.

**Garantías:**
- **Una sola vida por muerte:** cualquier otra muerte notificada mientras la pantalla está visible se ignora porque `resetting` sigue a `true`.
- **El reloj no avanza:** `Update` sale en cuanto `resetting` está activo.
- **Puntos, monedas y checkpoint se conservan:** `ResetLevel()` y `LoadLevel()` funcionan igual que antes.
- **Sin solapamientos:** orden de canvas HUD = 100 < pantalla de vidas = 200 < fader = `short.MaxValue`, y además el HUD se oculta mientras la pantalla está visible.
- **Sin `GameManager` duplicados:** la pantalla es hija del único `GameManager` persistente.
- **Seguridad:** `NewGame()`/`ResetGameState()` ocultan la pantalla y restauran `timeScale`.
- **Sprite de Mario:** se guarda al empezar cada nivel (`player.smallRenderer.idle`), porque al morir Mario puede estar inactivo (foso) o mostrando el sprite de muerte. Si no hay sprite, solo se muestra el texto.

## 3. Mejora 2 — Audio de bandera y fin de nivel

**Clips nuevos en `ChiptuneSynth`** (composición original):
- `FlagTouch`: "blip" agudo de 0,12 s cuando Mario agarra el mástil.
- `LevelClear`: fanfarria de **3,0 s** con arpegios ascendentes sobre I – ii – V – I (Do mayor) y un acorde final sostenido. Tiene tres voces: melodía en onda cuadrada, armonía y bajo en onda triangular.

**Secuencia nueva en `FlagPole`:**
1. Primer contacto de un Mario vivo, con protección `reached` para que todo ocurra una sola vez.
2. `LevelComplete()`: se para el reloj y se suma el bonus de tiempo.
3. **Se detiene la música ambiental.**
4. Mario y la bandera bajan juntos. Suena `FlagTouch` y, 0,08 s después, el barrido de deslizamiento `Flag` que ya existía.
5. **Cuando la bandera termina de bajar** empieza `LevelClear`, mientras Mario salta del mástil y camina hacia el castillo.
6. Mario entra en el castillo y se espera **lo que falte de la fanfarria + `exitDelay` (0,4 s)**. Antes había una espera fija de 2 s.
7. Se carga el siguiente nivel, salvo que se haya notificado una muerte durante la secuencia.

**8-4 no se ve afectado:** el castillo no usa `FlagPole`. `VictoryGoal`, `Fanfare`, `VictoryTheme` y la música del jefe no se han modificado.

## 4. Archivos modificados

| Archivo | Tipo | Cambio |
|---|---|---|
| `Assets/Scripts/LivesScreen.cs` (+ `.meta`) | **Nuevo** | Pantalla de vidas (UI en tiempo de ejecución con `PixelText`) |
| `Assets/Scripts/GameManager.cs` | Compartido | Campo `livesScreenDuration`. Crea `LivesScreen` en `Awake`. Muestra la pantalla dentro de `ResetLevelRoutine`. Guarda el sprite de Mario en `StartLevel`. Restaura el estado en `ResetGameState`. **`ResetLevel()`, `LoadLevel()` y `GameOver()` no cambian.** |
| `Assets/Scripts/FlagPole.cs` | Base (prefab de 1-1, 1-2 y 2-2) | Protección `reached`, comprobación de Mario muerto, secuencia de audio sincronizada y campo `exitDelay`. Los campos públicos anteriores se mantienen, así que el prefab y las escenas no cambian |
| `Assets/Scripts/ChiptuneSynth.cs` | Compartido | Solo añade los clips `LevelClear` y `FlagTouch` |

**No se ha modificado:** ninguna escena ni prefab, los builders de Editor, el menú de Víctor, `WaterEnemy`/2-2, `Boss`/`VictoryGoal`/8-4, `GameOverScreen` ni `VictoryScreen`.

## 5. Verificación realizada

- Compilación de los 51 scripts de juego con el Roslyn de Unity 6000.3.16f1 y el `Assembly-CSharp.rsp` del proyecto: **0 errores** (solo los avisos `CS0618 isKinematic` que ya existían).
- El Editor abierto recompiló con `Tundra build success` y sin errores en `Editor.log`.
- **No se ha ejecutado ninguna prueba en Play Mode.** Ninguna de las pruebas siguientes está aprobada todavía.

## 6. Pruebas pendientes (Play Mode)

| # | Prueba | Resultado esperado |
|---|---|---|
| 1 | 1-1, morir contra un Goomba con 3 vidas | Animación y jingle → fundido → pantalla negra `MUNDO 1-1`, Mario, `× 02` durante unos 2,5 s → 1-1 se reinicia, HUD visible, `TIEMPO 400`, puntos y monedas intactos |
| 2 | Morir cayendo a un foso | Igual que la prueba 1, y el sprite de Mario aparece aunque Mario estuviera inactivo |
| 3 | Mientras se ve la pantalla | El HUD no se ve, no hay sonidos de enemigos y el tiempo no baja (comprobar el valor al volver) |
| 4 | Morir con 1 vida | No aparece la pantalla de vidas; se carga GameOver directamente |
| 5 | Morir después de un checkpoint | Pantalla de vidas → reaparición en el checkpoint |
| 6 | Tiempo agotado (`LevelSettings` con `Time Limit = 5`, revertir después) | Muerte → pantalla de vidas → reinicio |
| 7 | Cambiar `Lives Screen Duration` en el `GameManager` de 1-1 (por ejemplo, 4) | La pantalla dura más. Con 0 no aparece |
| 8 | Hierarchy → DontDestroyOnLoad → GameManager | Hijos: `Screen Fader`, `HUD` y `Lives Screen`, uno de cada, aunque se muera varias veces |
| 9 | Tocar la bandera en 1-1 | Blip y barrido; la fanfarria empieza cuando la bandera llega abajo; Mario camina al castillo durante la fanfarria; 1-2 se carga unos 0,4 s después de que termine |
| 10 | Tocar la bandera en 1-2 y 2-2 | Además, la música ambiental se corta al tocar el mástil |
| 11 | Tocar la bandera con un salto que roce el mástil varias veces | Un solo blip, una sola fanfarria, un solo bonus de tiempo y una sola carga |
| 12 | 8-4 completo | La victoria final, la fanfarria del tesoro y la música no cambian |
| 13 | Escucha subjetiva | La fanfarria suena original y equilibrada en volumen respecto a los demás SFX |

## 7. Limitaciones conocidas

- La sincronización depende de la altura del mástil. Si Mario llega al castillo antes de que acabe la fanfarria (3 s), se espera el resto; si tarda más, la fanfarria termina antes de que entre. Se ha elegido una duración parecida a la de la animación, pero falta comprobarla jugando.
- La pantalla de vidas se superpone a la escena antigua congelada, no a una escena nueva. Durante esos 2,5 s la escena antigua sigue cargada en memoria.
- Mientras se ve la pantalla, `Time.timeScale` está a 0. Si en el futuro se añade un menú de pausa que también use `timeScale`, habrá que coordinarlo.
- No se ha implementado la animación de "contar el tiempo restante" al estilo NES; el bonus se suma de golpe al tocar la bandera, como en la entrega del HUD.
- Si Mario muere durante la secuencia de la bandera (caso raro), la carga del siguiente nivel se cancela y se aplica la lógica de muerte.
