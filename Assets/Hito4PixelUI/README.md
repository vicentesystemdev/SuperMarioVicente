# Menu Hito 4 integrado

Graficos y distribucion recibidos en `SuperMario_Hito4_Menu_Profesional_Unity6.zip`.

- La escena de inicio sigue siendo `Assets/Scenes/MainMenu.unity`.
- Pantallas: principal, niveles, controles, creditos, configuracion y referencia de SFX.
- Las tarjetas inician partidas nuevas en 1-1, 1-2, 2-2 y 8-4 usando GameManager.
- La musica y los efectos conservan los archivos de Mario Victor del proyecto.
- Configuracion persistente de musica, efectos y volumen mediante PlayerPrefs.
- Esc o P abre la pausa de los cuatro niveles: continuar, reiniciar o volver al menu.
- Se conservan los HUD, puntajes, temporizadores, vidas, enemigos y escenas de juego existentes.
- El marco 16:9 mantiene las posiciones de los controles y agrega bandas al cambiar de proporcion.

Para regenerar la escena y el prefab de pausa: `Tools > Super Mario > Hito 4 > Crear menu del paquete`.
Los graficos se importan como Sprite, filtro Point y sin compresion. Los scripts del paquete se adaptaron a las reglas actuales del juego.
