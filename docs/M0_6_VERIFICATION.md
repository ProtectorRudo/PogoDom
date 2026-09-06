# M0.6 — batallas offline verificables

PogoDom quiere **cero fricción** y bots instantáneos, pero el progreso de ciudades y naciones no puede confiar ciegamente en un score enviado por el teléfono.

La solución aprovecha una propiedad que ya construimos: el battle core es determinista.

## Flujo previsto

1. El servidor emite un `MatchTicket` firmado con HMAC: match id, ruleset, seed, identidad de ciudad/nación, campaña y vencimiento.
2. El teléfono juega localmente sin esperar al servidor durante la partida.
3. Sólo guarda los cambios de dirección por tick: un log diminuto de swipes.
4. Al terminar envía ticket + input log.
5. El servidor verifica la firma y **reproduce toda la partida** con el mismo Core, seed y bots.
6. El `BattleResult` persistente se calcula en el servidor; el score declarado por el cliente no es autoridad.
7. `MetaProgressionEngine` vuelve a proteger el mismo match id contra doble contribución.

## Ventaja de producto

No necesitamos matchmaking ni una llamada de red por rebote. El juego se siente instantáneo y el mundo persistente puede seguir siendo verificable.

En producción podemos preemitir varios tickets para tolerar conectividad pobre.

## Threat model honesto

Replay verification bloquea falsificación trivial de score, ciudad, seed e inputs imposibles, pero **no es una solución total contra botting**: un cliente modificado todavía podría automatizar la elección de swipes sobre un ticket válido. Antes de una economía competitiva a gran escala faltarán rate limits, detección de anomalías, rotación de rulesets/keys, posible device attestation y políticas server-side.

Nunca se debe incrustar la clave HMAC de emisión dentro del cliente.
