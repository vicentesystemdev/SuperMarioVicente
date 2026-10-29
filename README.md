# Super Mario Bros 2D — Evaluación individual en Unity 6

**Adaptación académica de un proyecto existente, ampliada con nuevas mecánicas y escenarios.**

[Repositorio de esta evaluación](https://github.com/vicentesystemdev/SuperMarioVicente) · [Proyecto base proporcionado](https://github.com/Jesusbautistah/SUPER-MArio) · [Tutorial de origen — Zigurous](https://github.com/zigurous/unity-super-mario-tutorial)

## Descripción

Este repositorio contiene un proyecto de plataformas 2D de *Super Mario Bros* utilizado como base para una **evaluación individual de Programación Multimedia / Videojuegos**. Se partió del repositorio indicado por el docente, se migró a **Unity 6 (6000.3.16f1 LTS)** y se incorporaron escenas, mecánicas y mejoras técnicas.

**No es un juego desarrollado íntegramente desde cero.** Se preserva y reconoce la procedencia del código, los prefabs y los recursos originales. Las contribuciones de esta evaluación se describen por separado.

## Estado de las mejoras

| Requisito de la evaluación | Escena | Estado | Alcance |
|---|---|---|---|
| Proyecto base de Mario | `1-1` | Funcional | Escena inicial del proyecto de partida, migrada a Unity 6. |
| Escena Game Over (25 %) | `GameOver` | Implementada y probada | Sistema de 3 vidas, reinicio de nivel, transición al agotar vidas, opciones REINTENTAR y SALIR. |
| Nivel subterráneo (25 %) | `1-2` | Implementado y probado | Escenario subterráneo con bloques, tuberías, enemigos, monedas, audio y transición. **Distribución propia inspirada en el clásico, no reproducción exacta del World 1-2.** |
| Nivel acuático (25 %) | `2-2` | Implementado; validación final según pruebas del proyecto | Natación, gravedad reducida, enemigos acuáticos, ocho secciones, sonidos y salida a superficie. **Distribución propia inspirada en el clásico, no reproducción exacta del World 2-2.** |
| Castillo de Bowser (25 %) | `8-4` (previsto) | **En desarrollo** | Nivel original con trampas, lava, enfrentamiento final y condición de victoria. |

> La tabla refleja el estado documentado durante el desarrollo. Actualizarla al completar las pruebas finales y registrar el castillo en el repositorio.

## Características y cambios realizados

- **Migración a Unity 6:** actualización del proyecto desde Unity 2021.3.24f1 a 6000.3.16f1, con revisión de compatibilidad de scripts y configuración.
- **Vidas y Game Over:** tres vidas iniciales, protección frente al descuento duplicado, reinicio de la escena actual, pantalla Game Over con botones, animaciones y efectos.
- **Subterráneo:** nueva escena 1-2 construida reutilizando prefabs y sprites existentes, con obstáculos, enemigos, audio y conexión desde 1-1.
- **Acuático:** sistema de natación separado del movimiento terrestre, nuevos enemigos y recursos de pixel art, ambientación sonora y escenario de varias secciones.
- **Integración:** transiciones entre escenas, configuración de Build Profiles, reutilización de `GameManager` y generación de niveles mediante herramientas del Editor.
- **Documentación técnica:** detalles de cada mejora en la carpeta [`Docs/`](Docs/).

La música y los efectos añadidos para las mejoras utilizan **síntesis de audio por código**, sin incorporar grabaciones de la banda sonora original de Nintendo.

## Requisitos

- **Unity Editor:** `6000.3.16f1` (Unity 6 LTS).
- **Plataforma objetivo de entrega:** Windows, arquitectura Intel 64-bit.
- Git para clonar el repositorio.

## Abrir el proyecto

1. Clonar este repositorio:

   ```bash
   git clone https://github.com/vicentesystemdev/SuperMarioVicente.git
   ```

2. Abrir **Unity Hub** y seleccionar **Add project from disk**.
3. Elegir la carpeta clonada y abrirla con **Unity 6000.3.16f1**.
4. Abrir `Assets/Scenes/1-1.unity` y ejecutar **Play**.
5. Verificar en **File → Build Profiles → Scene List** las escenas registradas y habilitadas.

### Escenas disponibles

```text
Assets/Scenes/
├── 1-1.unity       # Nivel inicial del proyecto base
├── 1-2.unity       # Nivel subterráneo
├── 2-2.unity       # Nivel acuático
└── GameOver.unity  # Fin de partida
```

La escena `8-4` y la pantalla de victoria se incorporarán cuando termine y se verifique esa mejora.

## Controles

| Acción | Control |
|---|---|
| Mover a Mario | `A` / `D` o flechas izquierda / derecha |
| Saltar en tierra | `Espacio` |
| Nadar hacia arriba | Pulsaciones sucesivas de `Espacio` dentro del agua |
| Entrar a una tubería vertical | `S` cuando corresponda |
| Entrar a la tubería lateral de salida | `D` cuando corresponda |
| Navegar en Game Over | Flechas arriba / abajo o ratón |
| Confirmar en Game Over | `Enter` o clic |

## Compilar el ejecutable Windows

1. Abrir **File → Build Profiles**.
2. Seleccionar **Windows** e **Intel 64-bit**.
3. Comprobar que las escenas que se necesitan estén habilitadas en **Scene List** y que `1-1` ocupe el índice `0`.
4. Generar el ejecutable con **Build**.
5. Probar el ejecutable fuera del Editor.
6. Para la evaluación, comprimir **todos los archivos generados por Unity**, incluida la carpeta `*_Data`, y entregar el `.zip`, `.rar` o `.7z` junto con el enlace a este repositorio.

## Documentación técnica

- [Mejora 1 — Nivel subterráneo](Docs/Mejora1_NivelSubterraneo.md)
- [Mejora 2 — Nivel acuático](Docs/Mejora2_NivelAcuatico.md)
- [Mejora 3 — Game Over](Docs/Mejora3_GameOver.md)

## Autoría, atribución y agradecimientos

### Nintendo — derechos sobre la obra original

*Super Mario Bros*, Mario, Bowser y los personajes, diseños, elementos visuales y marcas asociadas pertenecen a **Nintendo** y a sus respectivos titulares de derechos. Este trabajo es un **ejercicio académico no oficial y no comercial**; no está afiliado, patrocinado ni autorizado por Nintendo. Reconocer la titularidad no equivale a obtener una licencia de uso o redistribución.

### Zigurous — tutorial y proyecto técnico de origen

La estructura y gran parte del código, escenas, prefabs y recursos de partida provienen del tutorial público **[Unity Super Mario Tutorial — Zigurous](https://github.com/zigurous/unity-super-mario-tutorial)**, cuya referencia se mantiene expresamente. La presencia de recursos relacionados con Mario en ese tutorial no implica que Zigurous sea titular de los derechos de los personajes o gráficos originales de Nintendo.

- [Repositorio del tutorial](https://github.com/zigurous/unity-super-mario-tutorial)
- [Lista de videos del tutorial](https://youtube.com/playlist?list=PLqlFiJjSZ2x1mrMpSQgYdRm8PyWRTg6He)

### Jesusbautistah — repositorio utilizado como punto de partida

El trabajo académico tomó como base el repositorio **[Jesusbautistah/SUPER-MArio](https://github.com/Jesusbautistah/SUPER-MArio)**, proporcionado como referencia para la evaluación. Este repositorio de Vicente es un **fork** de esa fuente y conserva el historial y la atribución de procedencia. No se atribuye a Jesusbautistah una autoría adicional del tutorial que no esté documentada.

### Vicente Claros — integración y mejoras individuales

**[Vicente Claros — vicentesystemdev](https://github.com/vicentesystemdev)** realizó el trabajo de adaptación académica, migración de versión, integración de funcionalidades, configuración de escenas, pruebas manuales y control de versiones de esta evaluación. Las contribuciones nuevas se distinguen de los componentes preexistentes mediante el historial de Git y los documentos de la carpeta `Docs/`.

### Asistencia técnica de IA

Se utilizó **Claude Code** como herramienta de apoyo para analizar scripts, proponer e implementar código, generar configuraciones de escenas y redactar documentación técnica. La integración, revisión en Unity, validación de funcionamiento y commits de la evaluación corresponden al estudiante. No se presenta el código generado con asistencia como si todo el proyecto base fuera de autoría propia.

## Nota sobre licencias y uso

En la revisión de los repositorios de referencia no se identificó un archivo `LICENSE` en la raíz. **Un repositorio público no significa que su código, personajes, imágenes y música estén disponibles para cualquier uso.** Este README informa la procedencia y no concede licencias sobre materiales de terceros. Antes de redistribuir el proyecto fuera del contexto académico o publicarlo con recursos protegidos, deben revisarse los permisos correspondientes.

---

**Proyecto académico de Ingeniería de Sistemas — UNIFRANZ · 2026**
