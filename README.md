# PogoDom

PogoDom es un juego móvil de dominio territorial pensado para **una sola mano**: el personaje rebota automáticamente y el jugador cambia de dirección con swipes cardinales. Cada partida debe entenderse en menos de 20 segundos, durar poco, generar revancha inmediata y alimentar una guerra persistente de ciudades y naciones.

## Estado actual — M0.2 Core

El gameplay principal ya vive en C# puro y puede compilarse/probarse sin Unity.

### Implementado

- tablero 8×8;
- 1 humano + 3 bots;
- movimiento cardinal persistente;
- resolución determinista de colisiones y swaps;
- pintura y robo de casillas;
- Bank Crates: aseguran territorio temporal como score;
- Arrow: pinta hasta el borde;
- Speed: 8 s iniciales de doble rebote real;
- Missile: disparo automático al rival líder + stun inicial de 2 s;
- power-ups con respawn escalonado para evitar spam;
- bots Easy y Medium;
- personalidades Balanced / Greedy / Aggressive / Banker / Chaotic;
- standings deterministas;
- RNG por seed;
- input Unity de teclado + swipe;
- greybox Unity automático;
- tests EditMode compartidos con un harness .NET 8;
- laboratorio headless que ejecuta partidas completas de bots en CI.

### Hipótesis de tuning actuales

- match: **75 s**;
- tick base: **0,5 s**;
- Speed: **8 s**;
- Missile stun: **2 s**;
- 3 Bank Crates iniciales;
- 1 Arrow, 1 Speed y 1 Missile iniciales;
- replacements con cooldown para crear escasez y hotspots.

No son números cerrados: se calibrarán con simulación y luego con Unity/teléfonos.

## Constitución del producto

Está en [`design/PILLARS.md`](design/PILLARS.md). Resumen:

**<20 s para entender · una mano · rematch irresistible · viral · personajes memorables con skills · ciudades/naciones · bots competitivos · cero fricción.**

Una feature que no fortalece esos pilares no entra por defecto.

## Arquitectura

```text
Assets/PogoDom/Core      reglas puras, sin UnityEngine
Assets/PogoDom/Runtime   input y presentación Unity
Assets/PogoDom/Editor    herramientas de escena
Assets/PogoDom/Tests     tests compartidos
headless/                compilación, tests y simulación .NET
```

La separación permite probar miles de partidas sin renderizar, entrenar/evaluar bots y conectar después el metajuego de ciudades sin contaminar las reglas de batalla.

## Certificación sin Unity

Cada PR a `main` ejecuta `.github/workflows/headless-core.yml`:

1. compila el Core con .NET 8;
2. corre la suite NUnit;
3. incluye stress multi-seed;
4. ejecuta 250 partidas completas del laboratorio headless;
5. falla ante estados inválidos o si desaparecen loops esenciales.

Unity sigue siendo un gate posterior para presentación, input real, rendimiento y game feel visual.

## Unity

Cuando haya acceso:

1. abrir/copiar el repo en un proyecto Unity 3D compatible;
2. esperar compilación;
3. ejecutar **PogoDom → Create M0 Prototype Scene**;
4. abrir `Assets/PogoDom/Scenes/PogoDom_M0.unity`;
5. Play + Test Runner.

## Provenance

PogoDom es una implementación propia basada en comportamiento y arquitectura auditados de repos con permiso. No incorpora modelos, personajes, música, texturas, animaciones ni binarios propietarios de Crash Bash. Ver [`docs/PROVENANCE.md`](docs/PROVENANCE.md).
