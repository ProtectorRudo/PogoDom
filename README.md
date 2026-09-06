# PogoDom — M0.1 Core Prototype

Primer bloque ejecutable de PogoDom, construido como **clean-room reimplementation** en C# para Unity.

## Qué ya está implementado

- Tablero lógico configurable, por defecto **8×8**.
- **4 jugadores** en las esquinas: 1 humano + 3 bots.
- Movimiento cardinal persistente.
- Resolución determinista de colisiones simultáneas.
- Pintado de casillas.
- **Bank Crate**: convierte territorio temporal en puntos, limpia ese territorio y deja la casilla actual como inicio de una nueva cadena.
- **Arrow**: pinta desde la posición del jugador hasta un borde y rota periódicamente.
- Spawner determinista de 3 Bank Crates + 3 Arrows.
- RNG determinista por seed.
- Bot Easy orientado a objetivos.
- Timer de partida (90 s por defecto).
- Eventos de dominio para desacoplar reglas y presentación.
- Greybox 3D automático con cubos/cápsulas, cámara, luz, UI debug, teclado y swipe.
- Tests EditMode para banking, choques, swaps y determinismo básico.
- Herramienta de Unity: **PogoDom → Create M0 Prototype Scene**.

## Qué NO está todavía

- Speed boost.
- Misiles / stun.
- Pogo-a-Gogo enclosure.
- El Pogo Loco / TNT.
- Padlock.
- Arte final, avatares, pogos, trails, festejos.
- Meta de ciudades/naciones.
- Online.

Es deliberado: primero cerramos el `game feel` del núcleo y recién después agregamos superficie.

## Cómo probarlo en Unity

1. Crear un proyecto Unity 3D vacío.
2. Copiar la carpeta `Assets/PogoDom` dentro del proyecto.
3. Esperar a que Unity compile.
4. Ir a **PogoDom → Create M0 Prototype Scene**.
5. Abrir/usar la escena creada en `Assets/PogoDom/Scenes/PogoDom_M0.unity`.
6. Presionar Play.

### Controles

- PC: flechas o WASD.
- Mobile: swipe horizontal/vertical.
- La última dirección elegida se mantiene, como en el Pogo clásico.

## Parámetros de calibración

En `MatchConfig.cs`:

- `BoardWidth = 8`
- `BoardHeight = 8`
- `TickSeconds = 0.5f`
- `MatchSeconds = 90f`
- `TargetBankCrates = 3`
- `TargetArrows = 3`
- `BankThresholdForBots = 4`

Estos valores son configurables y todavía deben pasar por fase de calibración de feel.

## Principio de arquitectura

`Assets/PogoDom/Core` **no depende de UnityEngine**. Todo el gameplay importante vive allí.

Unity queda en `Assets/PogoDom/Runtime` como presentación/input. Esto permite:

- tests rápidos;
- replays reproducibles;
- entrenamiento de bots;
- reemplazar visuales sin tocar reglas;
- conectar después el metajuego sin contaminar la partida.

## Origen técnico de las decisiones

Se tomó comportamiento observable/arquitectónico como referencia de proyectos auditados, pero **no se copiaron assets ni código propietario del Crash Bash original**. Ver `docs/PROVENANCE.md`.
