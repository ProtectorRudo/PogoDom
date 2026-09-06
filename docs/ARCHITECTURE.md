# Arquitectura M0.1

## Flujo de un tick

El orden está elegido para conservar la semántica observada en el clon de referencia de Pogo Painter:

1. Asegurar población de items.
2. Aplicar el item que esté bajo el jugador al comenzar el tick.
3. Pintar la casilla actual de cada jugador.
4. Resolver inputs/bots.
5. Calcular candidatos de movimiento.
6. Resolver colisiones simultáneas.
7. Mover jugadores.
8. Rotar arrows cuando corresponde.
9. Reponer items consumidos.
10. Avanzar tick/timer.

La consecuencia importante es que al caer sobre un Bank Crate, en el siguiente tick se banca el territorio anterior y **después** se pinta la casilla actual, que pasa a ser el comienzo de una nueva cadena territorial.

## Colisiones

Reglas M0.1:

- Si un jugador permanece en una casilla, tiene prioridad sobre un jugador que quiere entrar en ella.
- Si dos o más jugadores móviles quieren la misma casilla libre, gana uno mediante RNG determinista.
- Si dos jugadores intentan intercambiar posiciones en el mismo tick, ambos quedan en su lugar.
- La resolución se estabiliza iterativamente para evitar que un jugador termine entrando en la casilla de otro que fue bloqueado por una colisión en cadena.

La última regla es una mejora deliberada sobre implementaciones de referencia que pueden dejar casos encadenados ambiguos.

## Estado temporal vs score

Una casilla pintada no equivale a score confirmado.

- `BoardState.OwnerAt(pos)` = territorio temporal actual.
- `PlayerState.Score` = puntos ya bancados.
- `BankingResolver.Bank(...)` convierte todas las casillas actuales del jugador en score y las vuelve neutrales.

## Determinismo

Todo azar de gameplay pasa por `IRandomSource`.

Misma seed + mismo estado + mismos inputs => mismo resultado.

Esto es requisito para:

- golden tests;
- reproducción de bugs;
- bots;
- balance automatizado;
- eventual replay/spectator.
