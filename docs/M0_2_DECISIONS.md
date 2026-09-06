# M0.2 — decisiones de dirección

## Control: una mano sigue siendo ley

No se agregó ningún botón de power-up.

- **Bank Crate**: se activa al aterrizar.
- **Arrow**: se activa al aterrizar.
- **Speed**: se activa al aterrizar y agrega un segundo rebote real por tick.
- **Missile**: se dispara automáticamente al aterrizar, priorizando al rival con mayor score; desempata por territorio temporal y distancia.

Así el jugador sigue haciendo una sola cosa: **deslizar**.

## Bots

El juego real usa por ahora tres bots Medium con personalidades diferentes:

- Greedy: disputa ítems.
- Aggressive: roba territorio y presiona al líder.
- Banker: valora más asegurar puntos.

Existe Easy para onboarding y queda reservada una entrada Hard para una policy entrenada/local. Hard nunca dependerá de una llamada HTTP por movimiento.

## Match length

M0.2 baja la hipótesis inicial de 90 s a **75 s**. No se considera definitiva. El headless lab medirá variantes antes de fijarla.

## Missile stun

La implementación web de referencia usa 3 s de stun. PogoDom arranca con **2 s** (4 ticks a 0,5 s) para reducir frustración y preservar el deseo de revancha. Es un parámetro de balance, no una regla cerrada.

## Speed

Speed dura 16 ticks = 8 s con tick de 0,5 s. En vez de multiplicar una animación, otorga un **segundo aterrizaje real**: puede pintar, robar y recoger un ítem intermedio.

## Headless certification

Como Unity no está disponible en esta etapa, GitHub compila `Assets/PogoDom/Core` como .NET 8 y ejecuta:

- tests unitarios;
- 200 partidas de stress en NUnit;
- 250 partidas completas del laboratorio headless en cada PR.

El lab falla si detecta posiciones duplicadas, partidas que no terminan o si los loops esenciales (bank, speed, missile/stun, robo) dejan de ocurrir.
