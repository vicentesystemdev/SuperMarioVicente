# Super Mario Bros 2D — Proyecto grupal Hito 4 | Unity 6

> **Proyecto académico de Ingeniería de Sistemas — UNIFRANZ (2026).** Adaptación y ampliación de un proyecto de plataformas 2D existente. Se conservan los créditos de las fuentes originales y se diferencian las contribuciones del equipo.

**Repositorio del equipo (fork):** [vicentesystemdev/SuperMarioVicente](https://github.com/vicentesystemdev/SuperMarioVicente)  
**Repositorio proporcionado para el proyecto:** [Jesusbautistah/SUPER-MArio](https://github.com/Jesusbautistah/SUPER-MArio)  
**Tutorial técnico de origen:** [Zigurous — Unity Super Mario Tutorial](https://github.com/zigurous/unity-super-mario-tutorial)  
**Versión de trabajo:** Unity **6000.3.16f1 LTS** · **Plataforma objetivo:** Windows x64

## 1. Presentación

Este proyecto comenzó como una **adaptación individual** del juego base facilitado para la evaluación de videojuegos: migración desde Unity 2021.3 a Unity 6, ampliación de escenas y mecánicas, y pruebas manuales. En el **trabajo grupal de Hito 4 — Super Mario Manual**, el equipo continúa sobre este fork para añadir un menú inicial, música de ambiente, efectos de sonido y una interfaz de indicadores (**HUD**), además de corregir y mejorar los niveles existentes.

El código, escenas y recursos del proyecto base **no fueron creados íntegramente por este equipo**. Los cambios originales y las contribuciones posteriores pueden consultarse en el historial de Git y la carpeta [`Docs/`](Docs/).

## 2. Equipo y responsabilidades actuales

| Integrante | Responsabilidad asignada en la etapa grupal | Estado |
|---|---|---|
| **Compañera del equipo (nombre por confirmar)** | Mejora del nivel acuático `2-2`; coordinación inicial de la rama `develop`. | Asignado / en desarrollo |
| **Víctor** | Diseño e implementación de la escena inicial **MainMenu**, con opciones interactivas para comenzar el juego. | Asignado / en desarrollo |
| **Marcelo** | Mejoras del **Castillo de Bowser** (`8-4`), jugabilidad y enfrentamiento final. | Asignado / en desarrollo |
| **Vicente Claros** ([vicentesystemdev](https://github.com/vicentesystemdev)) | HUD: **tiempo restante, puntuación, monedas y mundo**; revisión de errores, compatibilidad e integración entre todos los niveles. | Asignado / en desarrollo |

> **Nota de atribución:** esta tabla describe la **distribución actual del trabajo**, no acredita por sí sola la autoría de todos los commits anteriores. El desarrollo previo del fork, las aportaciones grupales y los cambios efectivamente integrados deben distinguirse mediante el historial de Git. Se actualizará el nombre pendiente cuando se confirme.

## 3. Requisitos del trabajo grupal (Hito 4)

| Requisito | Situación al organizar el trabajo | Responsable principal |
|---|---|---|
| Fork del proyecto proporcionado | Fork público existente; se utiliza como repositorio de trabajo del equipo. | Equipo |
| Menú principal interactivo | **V1 integrada.** JUGAR inicia la partida; CONTROLES muestra la ayuda y SALIR cierra el juego. Validación final del ejecutable pendiente. | Víctor |
| Sonido ambiente | Ya existen sistemas de música sintetizada en varios niveles; verificar y completar especialmente `1-1`. | Revisión conjunta |
| SFX: salto, pisotón, power-up, daño y muerte | El proyecto contiene llamadas a efectos sintetizados; se deben probar todos en juego y corregir faltantes. | Revisión conjunta |
| HUD: tiempo restante, puntaje y monedas | `GameManager` ya almacena monedas; falta comprobar/implementar interfaz visible, tiempo y puntaje por acciones. | Vicente |
| Correcciones y mejoras del escenario acuático | Base implementada; ajustes en progreso. | Compañera del equipo |
| Mejoras del castillo y enfrentamiento final | Escenas y scripts base presentes; ajustes en progreso. | Marcelo |
| Capturas y publicación en el foro | **Entrega individual**: características, dificultades, mejora favorita, captura y enlace público del fork. | Cada integrante |

**Criterios funcionales para el HUD:** tiempo descendente que responda a reinicios/transiciones, puntaje por monedas y enemigos eliminados, monedas actualizadas en pantalla y etiqueta del mundo actual. Los cambios deben evitar puntuación duplicada y conservar la lógica existente de vidas.

## 4. Escenas disponibles y recorrido

Los archivos presentes en el repositorio son:

```text
Assets/Scenes/
├── MainMenu.unity  # Menú principal: Jugar, Controles y Salir
├── 1-1.unity       # Nivel inicial del proyecto base
├── 1-2.unity       # Nivel subterráneo añadido
├── 2-2.unity       # Nivel acuático añadido
├── 8-4.unity       # Castillo de Bowser añadido
├── GameOver.unity  # Pantalla al agotar las vidas
└── Victory.unity   # Pantalla de victoria
```

**Menú inicial integrado:** `MainMenu.unity` ocupa el índice `0` en **Build Profiles**. Incluye **JUGAR**, **CONTROLES** y **SALIR**; JUGAR inicia `1-1` con tres vidas. La validación final del ejecutable sigue pendiente.

La ruta de juego prevista es:

`MainMenu → 1-1 → 1-2 → 2-2 → 8-4 → Victory`

Si se agotan las vidas, se muestra `GameOver`. La ruta completa debe validarse en el Editor y en la compilación de Windows antes de la entrega.

**Alcance de los niveles:** `1-2` y `2-2` están inspirados visual y mecánicamente en niveles clásicos de NES, pero su distribución actual es propia; no se presentan como copias exactas de los mapas originales. El castillo es una propuesta de escenario propio sobre el proyecto base.

## 5. Características implementadas previamente

- Migración del proyecto a **Unity 6 (6000.3.16f1 LTS)**.
- Sistema de **tres vidas**, reinicio de nivel y escena **GameOver**.
- Nivel **subterráneo 1-2**: plataformas, tuberías, enemigos, monedas y música sintetizada.
- Nivel **acuático 2-2**: brazadas, flotabilidad, enemigos acuáticos y ambientación.
- Nivel **castillo 8-4** y escena **Victory**, sujetos a las mejoras y pruebas grupales.
- Audio importado del proyecto Mario Victor para las pistas y efectos correspondientes; los efectos sin una grabación equivalente conservan la síntesis por código.

Para evitar confusión, **las funcionalidades grupales pendientes no se declaran terminadas** hasta que el equipo las pruebe y las fusione.

## 6. Organización de Git y colaboración

- [`main`](https://github.com/vicentesystemdev/SuperMarioVicente/tree/main): versión estable o de entrega, después de las pruebas y la integración.
- [`develop`](https://github.com/vicentesystemdev/SuperMarioVicente/tree/develop): integración del trabajo del equipo.
- `feature/*`: ramas por integrante o mejora, creadas desde `develop` cuando corresponda.

**Flujo recomendado:** crear rama `feature/*` → desarrollar y probar → commits significativos → Pull Request hacia `develop` → revisión/integración → pruebas de conjunto → integrar `develop` a `main`.

No hacer `push --force` ni sobrescribir el trabajo de otros integrantes. Cada integrante identifica sus commits y conserva los archivos `.meta` de Unity. Las operaciones Git y los commits se realizan manualmente por los estudiantes.

## 7. Requisitos y puesta en marcha

1. Tener instalado **Unity Hub** y **Unity Editor 6000.3.16f1 LTS** con soporte de Windows.
2. Clonar el repositorio:

   ```bash
   git clone https://github.com/vicentesystemdev/SuperMarioVicente.git
   ```

3. Abrir la carpeta desde Unity Hub con **Add project from disk**.
4. Para revisar el trabajo integrado del grupo, usar la rama `develop` y confirmar que Unity no esté en ejecución antes de cambiar de rama.
5. Revisar **File → Build Profiles → Scene List**. `MainMenu` debe ocupar el índice `0`.
6. Abrir `Assets/Scenes/MainMenu.unity`, ejecutar **Play** y elegir **JUGAR** para iniciar la partida. Confirmar **cero errores de compilación** en Console.

El proyecto utiliza componentes **uGUI / Unity UI** en las interfaces. Si aparecen errores por `Button`, `RawImage` o `EventSystems`, verificar la resolución de `com.unity.ugui` mediante Package Manager antes de modificar scripts.

### Controles actuales

| Acción | Control |
|---|---|
| Mover a Mario | `A`/`D` o flechas izquierda/derecha |
| Saltar | `Espacio` |
| Nadar (en `2-2`) | Pulsar `Espacio` repetidamente para dar brazadas |
| Entrar en tubería vertical | `S`, cuando corresponda |
| Entrar en tubería lateral | `D`, cuando corresponda |
| Seleccionar/confirmar en pantallas de opciones | Teclado o ratón, según la escena |
| Navegar en el menú principal | Flechas arriba/abajo, `W`/`S` o ratón |
| Confirmar en el menú principal | `Enter`, `Espacio` o clic |
| Volver desde Controles al menú | `Esc` o botón VOLVER |

## 8. Compilación para Windows

En Unity 6 ir a **File → Build Profiles → Windows**, seleccionar **Intel 64-bit**, comprobar que `MainMenu` ocupe el índice `0` de la lista de escenas y pulsar **Build**. Probar el `.exe` fuera del Editor y comprimir **toda la carpeta generada** (ejecutable, carpeta `*_Data` y bibliotecas) en `.zip` para su entrega cuando se solicite.

La compilación final solo se considera terminada después de verificar menú, HUD, audio, transiciones, GameOver y Victoria en el ejecutable.

## 9. Evidencias de la actividad grupal

Cada integrante debe publicar **de manera individual** en el foro del Hito 4:

1. Descripción breve de las características que se agregaron al fork.
2. Dificultad concreta encontrada durante la implementación.
3. Característica favorita y explicación del motivo.
4. Al menos una captura propia, relacionada con lo explicado.
5. Enlace público al fork trabajado.

La publicación debe describir **resultados efectivamente implementados y comprobados**, no tareas todavía pendientes.

## 10. Documentación técnica

- [Mejora 1 — Nivel subterráneo](Docs/Mejora1_NivelSubterraneo.md)
- [Mejora 2 — Nivel acuático](Docs/Mejora2_NivelAcuatico.md)
- [Mejora 3 — Sistema de vidas y Game Over](Docs/Mejora3_GameOver.md)
- [Mejora 4 — Castillo de Bowser](Docs/Mejora4_CastilloBowser.md)

La documentación de menú, HUD y mejoras grupales se añadirá cuando esté integrada.

## 11. Autoría, atribuciones y agradecimientos

### Nintendo — propiedad intelectual original

*Super Mario Bros*, Mario, Bowser y los personajes, marcas, diseños y recursos asociados pertenecen a **Nintendo** y sus respectivos titulares de derechos. Este es un trabajo **académico no oficial y no comercial**, sin afiliación ni autorización declarada de Nintendo. Los créditos no constituyen una licencia para reutilizar o redistribuir materiales de terceros.

### Zigurous — tutorial y proyecto técnico de origen

La base técnica del juego proviene del proyecto público [Unity Super Mario Tutorial — Zigurous](https://github.com/zigurous/unity-super-mario-tutorial), incluidos scripts, organización, prefabs y recursos de partida. Véase su [lista de videos](https://youtube.com/playlist?list=PLqlFiJjSZ2x1mrMpSQgYdRm8PyWRTg6He).

### Jesusbautistah — repositorio utilizado como punto de partida

El fork del equipo parte del repositorio [Jesusbautistah/SUPER-MArio](https://github.com/Jesusbautistah/SUPER-MArio) compartido como referencia de la actividad. Se reconoce expresamente su procedencia, sin asumir una autoría adicional sobre el tutorial original.

### Equipo de estudiantes — ampliaciones académicas

La migración inicial, integración de nuevas escenas y mecánicas, y las correcciones posteriores pertenecen a distintas **etapas de trabajo**. Las responsabilidades actuales de **Víctor, Marcelo, Vicente Claros y la compañera responsable del acuático** se muestran en la sección 2; la autoría de cada cambio concreto se acredita con los commits y Pull Requests correspondientes, no solamente por la asignación de una tarea.

### Herramientas de asistencia

Se ha utilizado **Claude Code** como apoyo para analizar, desarrollar y revisar scripts, generar configuraciones de escenas y preparar documentación. El equipo conserva la responsabilidad de probar, integrar, documentar y presentar el trabajo.

## 12. Licencias y alcance de uso

En los repositorios de referencia revisados no se identificó un archivo `LICENSE` en la raíz. **Que un repositorio sea público no implica permiso general de redistribución.** Este README informa los créditos y no concede derechos sobre materiales de terceros. Antes de distribuir los recursos fuera del contexto académico deben comprobarse sus permisos.

---

**UNIFRANZ · Proyecto grupal de Videojuegos / Hito 4 · 2026**
