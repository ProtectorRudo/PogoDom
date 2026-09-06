# M0.3 — meta de ciudades y naciones

Este bloque conecta el resultado de una batalla con el propósito persistente de PogoDom sin contaminar el gameplay.

## Regla de oro

**Los bots pueden hacer divertida una partida, pero jamás generan progreso global.**

El motor persistente acepta una contribución sólo si:

- el resultado pertenece a un humano;
- la partida es válida;
- la ciudad/nación coinciden con la identidad registrada;
- el `matchId` no fue procesado antes.

Una victoria humana aporta exactamente **+1 City Point y +1 Nation Point**.

## Campaña de ciudad

Valores iniciales configurados según el diseño de CIVIDOM/PogoDom:

- victoria atacando: **-1 HP** a la ciudad objetivo;
- victoria defendiendo: **+2 HP**, sin superar el máximo;
- conquista: HP llega a 0.

## Tiers por población

- GIGANTE ≥5M: 1000 HP / roster 30
- GRANDE 1M–4.999M: 2000 HP / roster 24
- MEDIANA 250k–999k: 3000 HP / roster 18
- PEQUEÑA 50k–249k: 4000 HP / roster 12
- MUY PEQUEÑA <50k: 5000 HP / roster 6

## Objetivos

El mismo resultado humano puede progresar objetivos de ciudad o nación por:

- partidas;
- victorias;
- score bancado;
- tiles pintados;
- tiles robados;
- Bank Crates;
- Arrows;
- Speed;
- Missiles.

Los objetivos están capados en su target y el recibo de progresión informa exactamente qué cambió.

## Idempotencia

`WorldState` conserva `matchId` procesados. La misma batalla no puede sumar dos veces aunque un cliente reintente el envío.

Resultados inválidos o de bots no consumen el identificador: una corrección válida del mismo match puede procesarse posteriormente.
