# Mejora 4 — Castillo de Bowser (8-4) y Victoria

Proyecto: Super Mario Bros (evaluación individual) · Unity 6000.3.16f1 LTS

Base: tutorial **Super Mario Bros (2D)** de **Zigurous**. El castillo es de
**diseño propio**, como exige el docente. El jefe final es un personaje
original, **Magmor, el Rey de Lava**, que cumple el papel de Bowser: el
proyecto no incluye sprites de Bowser y no se han copiado los de Nintendo.

## 1. Auditoría

| Elemento | Estado previo | Decisión |
|---|---|---|
| Daño a Mario | `Player.Hit()` sin invulnerabilidad | Se añaden 2 s de invulnerabilidad con parpadeo tras encoger; sin ella, pinchos y fuego matarían en el frame siguiente |
| Detección de suelo y paredes | `Extensions.Raycast` contra Default, y las consultas detectan triggers | **Todos los triggers nuevos van en la capa Ignore Raycast** (lección del bug de 2-2) |
| Cámara | `SideScrolling` solo horizontal y sin retroceso | Nueva `CastleCamera` (horizontal + vertical por zonas); `SideScrolling` no cambia |
| Plataformas | Mario se mueve con `MovePosition` (las plataformas no lo arrastran) | `PlayerMovement.Carry()`: una línea en `FixedUpdate` que vale cero fuera de plataformas |
| Checkpoints | No existían | `GameManager.SetCheckpoint` / `TryGetCheckpoint` |
| Victoria | No existía | `GameManager.Victory()` y escena `Victory` |
| 2-2 → 8-4 | La bandera de 2-2 ya tenía `nextWorld 8 / nextStage 4` | Se conecta al registrar 8-4 |
| Assets de castillo | No existían | 29 sprites nuevos (sección 4) |

## 2. Diseño del nivel (≈ 400 tiles de recorrido + torre de 28 de alto)

| # | Sección | x | Desafíos |
|---|---|---|---|
| 1 | Entrada al castillo | -2..55 | Puerta, antorchas, 3 Goombas, 1 Koopa, primer foso de lava con chispa, champiñón |
| 2 | Pasillo de trampas | 56..115 | Pinchos de suelo, campo de pinchos cruzado sobre **bloques que se derrumban**, pasillo bajo con **pinchos en el techo** (prohibido saltar alto), 3 **chorros de fuego** desfasados |
| 3 | Zona de lava | 116..185 | Checkpoint 1, lago de lava de 58 tiles: pilares, **2 plataformas móviles**, puente que se derrumba, 5 **chispas de lava** |
| 4 | Cámaras de fuego | 186..245 | 2 **barras de fuego giratorias**, pasillo estrecho con chorros, **cañones** a ras de suelo y a la altura de la cabeza |
| 5 | Torre interior | 246..287 | Checkpoint 2, ascenso en zigzag de 7 plataformas + **ascensor**, barra de fuego y cañón en la pared; la cámara sube con Mario |
| 6 | Antesala del jefe | 288..325 | Checkpoint 3, champiñón, 1-up, monedas, 1 Goomba |
| 7 | Sala del jefe | 326..375 | La reja se cierra tras Mario; combate contra Magmor con 3 plataformas para esquivar |
| 8 | Sala del tesoro | 377..399 | Se abre al derrotar al jefe; la copa dorada activa la victoria |

**Geometría verificada sobre el papel** con la física real de Mario (salto de 5 tiles, unos 8 de alcance horizontal, 2 de alto siendo grande):
- Los huecos entre plataformas miden de 1 a 3 tiles.
- Las subidas son de 1 a 4 tiles.
- Los pasillos bajos dejan entre 4 y 6 tiles libres.

**Duración estimada: 4-6 minutos. Esta cifra no se ha medido.**

## 3. Mecánicas

### Peligros y trampas
| Script | Comportamiento |
|---|---|
| `Hazard` | Trigger que daña (`Hit`) o mata al instante (`instantKill`, en la lava). Con contacto continuo vuelve a dañar cuando termina la invulnerabilidad |
| `LavaSpark` | Salta de la lava en parábola con un ritmo fijo; cae de cabeza; sonido al emerger |
| `FireJet` | Ciclo reposo → aviso (llama pequeña parpadeante e inofensiva) → llama de 3 tiles |
| `Firebar` | Cadena de bolas de fuego girando alrededor de un bloque |
| `FireCannon` | Dispara `Projectile` periódicamente, solo cerca de la pantalla |
| `CrumblingPlatform` | Tiembla 0,5 s, cae y reaparece a los 3 s |
| `MovingPlatform` | Movimiento suavizado (horizontal o vertical); arrastra a Mario |
| `Checkpoint` | Bandera gris → verde con sonido; tras morir, Mario reaparece allí |
| `Gate` | Reja que se cierra de golpe o se abre deslizándose |

### Jefe final — `Boss` (Magmor)
| Requisito | Implementación |
|---|---|
| Barra de vida | `BossHealthBar` (UI superior "MAGMOR"); aparece al empezar el combate |
| Idle | Animación de 2 frames con respiración |
| Ataque | Sprite con boca abierta y una pausa de preparación antes de cada fuego |
| Fuego | Fase 1: bola apuntada a Mario (inclinación limitada). Fase 2: abanico de 3 |
| Movimiento | Recorre la arena hacia puntos aleatorios; pequeños saltos en la fase 1 |
| Daño | Pisotón en la cabeza (Mario rebota) o estrella; el contacto lateral daña a Mario |
| Invulnerabilidad | 1,2 s con parpadeo tras cada golpe |
| 2 fases | 6 de vida. Con 3 o menos: rugido, más velocidad, triple fuego y **salto con onda de fuego** en ambas direcciones (hay que saltarla) y temblor de cámara |
| Derrota | Destellos, sprite derrotado, voltereta y caída fuera de la arena; sonido propio |
| Victoria | Se abre la reja de salida → copa → `GameManager.Victory()` |

`BossArena` cierra la reja de entrada, cambia a la música del jefe y arranca el combate; al derrotar al jefe abre la salida y recupera la música del castillo.

### Victoria (`Victory.unity` + `VictoryScreen`)
- Título "FELICIDADES" que cae y cambia de color, "EL REY MAGMOR HA CAIDO" y estadísticas (monedas y vidas).
- Confeti, Mario saltando junto a la copa y música de victoria en bucle.
- Botones **NUEVA PARTIDA** (`GameManager.NewGame()`: 3 vidas, 0 monedas, 1-1) y **SALIR**, con teclado y ratón.
- No hay menú principal en el proyecto: "volver al menú" equivale a NUEVA PARTIDA.

### Cámara — `CastleCamera`
- **Horizontal:** sigue a Mario al instante, dentro de [-2,5 ; 402,5].
- **Vertical:** suavizada, limitada por zonas:

  | Zona | x | Altura visible |
  |---|---|---|
  | A (planta baja) | < 246 | -0,5 a 13,5 |
  | B (torre) | 246..288 | -0,5 a 41,5 |
  | C (planta alta) | > 288 | 27,5 a 41,5 |

- Hay mampostería sólida rellenando los bordes visibles para que no aparezcan zonas vacías.
- Temblor de cámara en los ataques del jefe.
- `SnapTo` al reaparecer en un checkpoint.

### Audio (síntesis original, `ChiptuneSynth`)
- **Músicas en bucle:** castillo (bajo con tritono), jefe (bajo rápido con acentos) y victoria.
- **Efectos:** fanfarria, fuego, chispa de lava, derrumbe, checkpoint, reja, rugido, daño al jefe, fuego del jefe, golpe en el suelo y derrota del jefe.
- `LevelMusic` admite cambiar de tema en caliente con `Play(Theme)`.

## 4. Recursos

### Nuevos (pixel art propio, `Tools/generate_castle_sprites.py` → `Assets/Sprites/Castle/`)
| Grupo | Sprites |
|---|---|
| Bloques | `Castle_Stone`, `Castle_Brick`, `Castle_BrickBack`, `Castle_Crumble`, `Castle_Platform` |
| Lava y fuego | `Lava1/2`, `Fireball1/2`, `Flame1/2`, `Torch1/2`, `Shockwave1/2` |
| Trampas | `Spikes`, `Spikes_Down`, `Cannon` |
| Escenario | `Door`, `Gate`, `Checkpoint_Off/On`, `Trophy` |
| Jefe | `Boss_Idle1/2`, `Boss_Attack`, `Boss_Defeated`, `Boss_Fireball1/2` |

Prefabs nuevos (`Assets/Prefabs/Castle/`, **los crea el menú**):
- Bloques y decoración: Stone, Brick, BrickBack, Torch, Door.
- Trampas y peligros: Lava, Spikes, Spikes_Down, Crumble, MovingPlatform, LavaSpark, FireJet, Cannon.
- Proyectiles: Fireball, BossFireball, Shockwave.
- Escenario y jefe: Gate, Checkpoint, Trophy, Boss.

### Reutilizados
- Prefabs: Mario, Goomba, Koopa, MysteryBlock, Coin, MagicMushroom y 1upMushroom.
- Sistemas: `DeathBarrier`, `GameManager`, `PixelText`, `SelectOnHover`, `Sfx`, `LevelMusic`, `BuildSceneList`.
- Sprites: `Coin` (cursor) y `Mario_Big_Jump` (pantalla de victoria).

## 5. Archivos

### Creados
- Scripts: `Hazard`, `Projectile`, `Firebar`, `FireJet`, `FireCannon`, `LavaSpark`, `CrumblingPlatform`, `MovingPlatform`, `Checkpoint`, `Gate`, `CastleCamera`, `CameraUtil`, `Boss`, `BossArena`, `BossHealthBar`, `VictoryGoal`, `VictoryScreen`
- Editor: `CastleLevelBuilder`, `VictorySceneBuilder`, `SpriteImport`
- `Tools/generate_castle_sprites.py`, `Assets/Sprites/Castle/*.png` (29)
- `Docs/Mejora4_CastilloBowser.md`
- Generados por el menú: `Assets/Prefabs/Castle/*.prefab`, `Assets/Scenes/8-4.unity`, `Assets/Scenes/Victory.unity`

### Modificados
| Archivo | Cambio |
|---|---|
| `Player.cs` | Invulnerabilidad de 2 s con parpadeo tras recibir daño (afecta positivamente a todos los niveles) |
| `PlayerMovement.cs` | `Carry()` y una línea en `FixedUpdate` para las plataformas móviles |
| `GameManager.cs` | Checkpoints por nivel y `Victory()` |
| `ChiptuneSynth.cs` | 3 músicas, fanfarria y 11 efectos nuevos |
| `LevelMusic.cs` | Temas Castle, Boss y Victory; `Play(Theme)` |

Ninguna escena, prefab ni sprite existente se modificó. El orden del enum de `LevelMusic` se conserva, así que 1-2 y 2-2 no cambian.

## 6. Generar las escenas (Unity 6000.3.16f1)

1. Vuelve al Editor y espera a que compile. Solo deben quedar los avisos `isKinematic is obsolete`, que ya existían.
2. **Tools › Super Mario › Crear Castillo de Bowser 8-4**. El menú:
   - configura la importación de `Assets/Sprites/Castle`;
   - crea los prefabs de `Assets/Prefabs/Castle`;
   - genera y guarda `8-4.unity`;
   - crea `Victory.unity` (si ya existe, pregunta antes de reemplazarla);
   - registra 8-4 y Victory en Build Profiles sin quitar ni reordenar escenas;
   - vuelve a la escena en la que estabas.
3. Si 8-4 ya existe, pide confirmación antes de reemplazarla.
4. Comprueba en **File › Build Profiles**: `1-1`, `GameOver`, `1-2`, `2-2`, `8-4` y `Victory`.

También existe **Tools › Super Mario › Crear escena de Victoria** para regenerar solo esa pantalla.

## 7. Cómo probar

- **Castillo completo:** abre `8-4.unity` y pulsa Play (empieza con 3 vidas).
- **Combate directo:** juega hasta la antesala (checkpoint 3) o mueve temporalmente a Mario en la escena a x = 300, y = 30 y pulsa Play. Recuerda deshacer el cambio.
- **Flujo completo:** 1-1 → bandera → 1-2 → bandera → 2-2 → tubería y bandera → 8-4 → jefe → copa → Victoria → NUEVA PARTIDA → 1-1 con 3 vidas.

## 8. Pruebas

**Ejecutadas:** solo la compilación. Unity compiló todos los scripts de runtime y de Editor sin errores (log del Editor, 16:29:47).

**Pendientes (no ejecutadas):**

| # | Caso |
|---|---|
| 1 | El menú genera prefabs, 8-4 y Victory sin errores; 6 escenas en Build Profiles |
| 2 | Recorrido completo sin plataformas imposibles (especialmente la torre y el lago de lava) |
| 3 | Pinchos dañan con invulnerabilidad; la lava mata; los bloques se derrumban y reaparecen |
| 4 | Chorros, barras de fuego, cañones y chispas siguen su ciclo y dañan |
| 5 | Las plataformas móviles y el ascensor transportan a Mario |
| 6 | Los checkpoints guardan la posición; al morir se reaparece allí con la cámara correcta |
| 7 | Combate: barra de vida, fase 2 al 50 %, onda de fuego, invulnerabilidad, derrota |
| 8 | La reja se abre, la copa lleva a Victoria, NUEVA PARTIDA reinicia a 3 vidas y 0 monedas |
| 9 | 0 vidas en el castillo → GameOver |
| 10 | 2-2 → 8-4 conserva vidas y monedas |
| 11 | Regresión de 1-1, 1-2 y 2-2 (movimiento, natación, cámara, sonido) |
| 12 | Cámara: nunca muestra fuera del mapa en ninguna relación de aspecto |
| 13 | Duración real y equilibrio del jefe |

## 9. Problemas conocidos y limitaciones

- No hay menú principal: la opción de volver es NUEVA PARTIDA.
- Las monedas, los bloques "?" y los Goombas y Koopas usan los sprites del exterior.
- La pantalla de Victoria muestra solo monedas y vidas (no hay puntuación ni tiempo en el proyecto).
- El jefe es un diseño original, no Bowser; tampoco hay hacha ni puente que se derrumbe como en el original.
- La invulnerabilidad nueva de Mario también se aplica en 1-1, 1-2 y 2-2 (en el original de NES también existe).
- Si se reemplaza 8-4 con el menú, se pierden los cambios manuales hechos en esa escena (el menú lo avisa).

## 10. Créditos

- Código base, sprites de Mario, enemigos, bloques y monedas: tutorial de **Zigurous**.
- Sprites del castillo, el jefe Magmor, la copa, todo el audio y el diseño del nivel: elaboración propia.
- Super Mario Bros y Bowser son marcas de Nintendo; proyecto académico no comercial.
