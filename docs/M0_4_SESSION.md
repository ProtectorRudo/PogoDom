# M0.4 — sesión, onboarding y viralidad

Este bloque convierte los pilares de producto en contratos de código que pueden probarse sin Unity.

## <20 segundos

`FirstRunCoach` jamás bloquea input. Empieza enseñando un solo gesto: swipe. Luego expone pintura/robo y Bank. Si el jugador no completa esos hitos, el coach desaparece igualmente a los **20 s**.

No hay tutorial modal, video obligatorio ni pantalla de instrucciones.

## Rematch primero

`SessionDirector` garantiza que el resultado tenga **Rematch** como acción primaria y permite saltar de Results → Playing con un nuevo match id. El metajuego y el progreso se muestran como información, no como una barrera entre partidas.

## Un objetivo visible

`ObjectiveSpotlightSelector` elige un único objetivo relevante e incompleto para mostrar durante/entre partidas. Prioriza objetivos ya empezados y próximos a completarse. El sistema puede tener muchos objetivos; la interfaz no tiene por qué mostrarlos todos a la vez.

## Viralidad medible

`MatchHighlightTracker` detecta señales candidatas a clip/share:

- Bank grande;
- Missile al líder;
- cambio de líder en los últimos 15 s;
- photo finish;
- victoria remontando en la ventana final.

No publica nada automáticamente. El objetivo es que el juego sepa cuándo acaba de ocurrir un momento que vale la pena convertir en replay, recap o share card cuando exista la capa visual.
