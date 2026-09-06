# Headless baseline — M0.2

Fecha de medición: 2026-09-06.

Configuración: 250 partidas bot-vs-bot, board 8×8, match 75 s, tick 0,5 s, tres Bank Crates, un Arrow, un Speed y un Missile con respawns escalonados.

## Resultado

- invalid states: **0**
- winner score promedio: **84,58**
- margen ganador promedio: **12,40**
- finales con margen ≤5: **32,4 %**
- intentos de movimiento bloqueados: **4,7 %**
- cambios de líder por partida: **8,93**
- robos de casilla por partida: **343,39**
- banks por partida: **26,28**
- arrows por partida: **9,26**
- speeds por partida: **8,00**
- missiles por partida: **7,76**

## Lectura de dirección

El objetivo de este laboratorio no es declarar que el juego ya es divertido: eso requiere humanos y Unity. Sirve para eliminar estados rotos y detectar señales de ritmo.

La primera versión sin cooldowns producía más de 20 activaciones de cada special y 63 banks por partida. La escasez redujo fuerte ese spam. El 4,7 % de bloqueos reales indica que el tablero no está generando fricción estructural exagerada. Hay casi nueve cambios de líder por partida, suficiente para mantener volatilidad competitiva en esta etapa.

El margen promedio todavía merece observación. No vamos a fabricar empates artificiales ni rubber-banding oculto: el balance se ajustará mediante bots, distribución de recursos, objetivos y duración, sin falsear el resultado del jugador.
