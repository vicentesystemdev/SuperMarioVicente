# Mejora 3 — Sistema de vidas y escena Game Over

Proyecto: Super Mario Bros (evaluación individual) · Unity 6000.3.16f1 LTS

Base del proyecto: tutorial **Super Mario Bros (2D)** de **Zigurous**
(https://github.com/zigurous/unity-super-mario-tutorial). Se conserva el código
original; esta mejora solo añade y modifica lo indicado abajo.

## 1. Auditoría del sistema original

| Aspecto | Comportamiento original |
|---|---|
| Inicialización de vidas | `GameManager.Start()` → `NewGame()` → `lives = 3`, `coins = 0`, `LoadLevel(1,1)` (recargaba 1-1 aunque ya estuviera abierta). |
| Detección de muerte | Enemigos (`Goomba`, `Koopa`, caparazón) → `Player.Hit()` → `Player.Death()`. Caída → `DeathBarrier` desactiva a Mario. |
| Descuento de vidas | Ambos casos llaman a `GameManager.ResetLevel(3f)` → `Invoke` → `ResetLevel()` → `lives--`. |
| Reinicio del nivel | `lives > 0` → `LoadLevel(world, stage)`. |
| Cero vidas | `GameOver()` llamaba a `NewGame()` de inmediato (sin pantalla). |
| Persistencia | Singleton con `DontDestroyOnLoad`; la copia que trae 1-1 al recargarse se destruye en `Awake`. |
| Riesgo de muerte duplicada | `ResetLevel(float)` solo evitaba la duplicación mediante `CancelInvoke`; `Player.Death()` no tenía protección propia; no había ningún estado que indicara "muerte en curso". |

## 2. Funcionalidades implementadas

- Una partida nueva empieza con **3 vidas** (`GameManager.StartingLives`).
- Cada muerte descuenta **exactamente una vida**: el flag `resetting` ignora los avisos repetidos de una misma muerte y se limpia al cargar la escena siguiente.
- Con vidas restantes se reinicia el nivel actual y las monedas se conservan (regla original).
- Con 0 vidas se carga la escena `GameOver`. Si la escena no está en Build Profiles, se registra un error en consola y se inicia una partida nueva, sin bloquear el juego.
- **REINTENTAR**: `GameManager.NewGame()` → 3 vidas, 0 monedas, carga 1-1.
- **SALIR**: `Application.Quit()` en el ejecutable. En el Editor escribe un mensaje en consola y detiene el modo Play.
- Al pulsar Play directamente en una escena de nivel (`W-S`), la partida empieza en ese nivel sin recargarlo.
- Transición: animación de muerte original (3 s) → fundido a negro de 0,5 s → carga de la escena → fundido de entrada.
- Pantalla Game Over: fondo negro con suelo de bloques atenuado, título **GAME OVER** en fuente pixel 5×7 que cae desde arriba con rebote, texto "MUNDO W-S", botones que aparecen con fundido y un cursor-moneda animado. Se maneja con teclado (↑/↓ + Enter) y con ratón.
- Audio: jingle de muerte (lo reproduce `GameManager`, así suena también al caer al vacío) y jingle de Game Over. Ambos son **composiciones originales sintetizadas por código** (`ChiptuneSynth`), sin archivos externos ni música de Nintendo. Si se asigna un `AudioClip` en el Inspector, se usa ese en lugar del sintetizado.

## 3. Archivos

### Creados
| Archivo | Función |
|---|---|
| `Assets/Scripts/ScreenFader.cs` | Overlay negro persistente para fundidos (lo crea el GameManager en tiempo de ejecución). |
| `Assets/Scripts/ChiptuneSynth.cs` | Genera los jingles de muerte y de Game Over (ondas cuadrada y triangular). |
| `Assets/Scripts/PixelText.cs` | Texto con fuente pixel 5×7 integrada, filtrado Point (`ExecuteAlways`). |
| `Assets/Scripts/GameOverScreen.cs` | Controlador de la escena: animaciones, audio, REINTENTAR y SALIR. |
| `Assets/Scripts/SelectOnHover.cs` | Al pasar el ratón por un botón lo selecciona, para que ratón y teclado compartan el resaltado. |
| `Assets/Editor/GameOverSceneBuilder.cs` | Menú de editor que crea `GameOver.unity` y registra las escenas en Build Profiles. |
| `Assets/Scenes/GameOver.unity` | **La genera el menú del punto 4** (no se escribió el YAML a mano). |
| `Docs/Mejora3_GameOver.md` | Este documento. |

### Modificados
| Archivo | Cambio |
|---|---|
| `Assets/Scripts/GameManager.cs` | Constantes `StartingLives` y `GameOverScene`; flag `resetting` contra muertes duplicadas; reinicio con corrutina y fundido en vez de `Invoke`; `GameOver()` carga la escena; sonido de muerte; `Start()` respeta el nivel abierto; `OnSceneLoaded` limpia el estado y hace el fundido de entrada; logs `[GameManager]`. |
| `Assets/Scripts/Player.cs` | `Death()` ignora la llamada si Mario ya está muerto. |

`DeathBarrier.cs`, `DeathAnimation.cs`, `PlayerMovement.cs`, `FlagPole.cs`, los prefabs y `1-1.unity` **no se modificaron**.

## 4. Configuración en Unity (pasos manuales)

1. Abre el proyecto y espera a que compile. La consola debe quedar sin errores; los avisos `isKinematic is obsolete` ya existían.
2. Menú **Tools › Super Mario › Crear escena Game Over**.
   - Si hay cambios sin guardar, Unity te preguntará si quieres guardarlos.
   - Crea `Assets/Scenes/GameOver.unity`, la guarda, la añade a la lista de escenas y vuelve a abrir la escena en la que estabas.
3. Comprueba en **File › Build Profiles › Scene List**: `Scenes/1-1` (índice 0) y `Scenes/GameOver`, ambas marcadas. Si un perfil de plataforma usa una lista de escenas propia (override), añade GameOver también ahí.
4. Opcional, en el Inspector:
   - `Game Manager` (escena 1-1): `Death Sound` y `Fade Duration`.
   - `Game Over Canvas` › `GameOverScreen`: `Game Over Sound`, `First Level Scene`, tiempos de animación.
5. Comprueba que los botones tienen el evento asignado: `Retry Button` › OnClick → `GameOverScreen.Retry`; `Quit Button` › OnClick → `GameOverScreen.Quit`.

## 5. Controles

| Acción | Tecla |
|---|---|
| Mover / saltar (sin cambios) | A/D o ←/→, Espacio |
| Navegar el menú Game Over | ↑ / ↓ o ratón |
| Confirmar | Enter o clic |

## 6. Pruebas

Estado: **pendientes de ejecutar en Unity por el autor**. No se ha ejecutado ninguna en el Editor, porque estaba abierto por el usuario y no permitía ejecutarlo en modo batch. La consola muestra `[GameManager] ...` con el número de vidas, lo que facilita comprobarlas.

| # | Caso | Resultado esperado | Estado |
|---|---|---|---|
| 1 | Play en 1-1 | Log "Nueva partida: 3 vidas" | Pendiente |
| 2 | Primera muerte (Goomba) | Jingle, fundido, "quedan 2 vidas", reinicia 1-1 | Pendiente |
| 3 | Segunda muerte (caída al vacío) | "quedan 1 vidas", reinicia 1-1 | Pendiente |
| 4 | Tercera muerte | "quedan 0 vidas", "Game Over", carga GameOver | Pendiente |
| 5 | REINTENTAR | Fundido, "Nueva partida: 3 vidas", 1-1, monedas a 0 | Pendiente |
| 6 | SALIR en build Windows | Se cierra el ejecutable | Pendiente |
| 7 | Hierarchy › DontDestroyOnLoad tras varias muertes | Un solo `Game Manager` | Pendiente |
| 8 | Movimiento, salto, enemigos, monedas, tubería, cámara y bandera | Sin cambios respecto al original | Pendiente |
| 9 | Audio y animaciones | Jingles audibles, título con rebote, cursor animado | Pendiente |
| Extra | Tocar un Goomba y caer al vacío en la misma muerte | Solo se descuenta 1 vida | Pendiente |

## 7. Limitaciones conocidas

- No hay HUD de vidas ni de monedas en el nivel (el original tampoco lo tenía). Las vidas se ven en la consola y se pueden consultar en `GameManager.Instance.lives`.
- El nivel no tiene música de fondo (el proyecto original no incluye audio). Los jingles se sintetizan; si se quiere audio grabado, hay que asignar clips propios o libres en el Inspector.
- `FlagPole` en 1-1 apunta a `nextWorld = 1, nextStage = 1` (comportamiento original). Se ajustará en las mejoras de los niveles siguientes.
- Los avisos `Rigidbody2D.isKinematic is obsolete` vienen de la migración a Unity 6 y quedan fuera del alcance de esta mejora.

## 8. Créditos

- Código base y sprites: tutorial *Super Mario Bros (2D)* de **Zigurous** — https://github.com/zigurous/unity-super-mario-tutorial
- Super Mario Bros es una marca de Nintendo. Este proyecto es académico y no comercial.
- Jingles y fuente pixel de esta mejora: elaboración propia.
