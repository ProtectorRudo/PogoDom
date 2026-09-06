# M0.5 — personajes, skills visuales y monetización justa

PogoDom necesita personajes que la gente quiera tener, mostrar y compartir. Este bloque prepara esa economía sin permitir pay-to-win.

## Slots monetizables

- personaje;
- pogo;
- trail;
- landing FX;
- festejo/emote de victoria;
- skill visual.

## Qué significa “skill visual”

Una skill visual escucha un evento que **ya ocurrió** en las reglas: robo de tile, Bank, Arrow, Speed, Missile, victoria, etc. Sólo cambia la presentación asociada.

Ejemplo futuro: dos jugadores recogen exactamente el mismo Missile. Mecánicamente sucede lo mismo; uno puede verlo como misil base y otro como un dragón de energía. El Core competitivo no recibe ningún modificador.

## Integridad competitiva

El assembly `PogoDom.Cosmetics` no posee referencias mutables a `MatchConfig` o `PlayerState` ni campos de damage/speed/score/stun bonus. CI incluye un guard por reflexión para evitar que aparezcan ventajas de gameplay accidentalmente dentro de definiciones cosméticas.

## Inventario y loadout

- no se puede equipar algo no poseído;
- los pogos pueden declarar una familia de rig compatible;
- cambiar de personaje corrige al pogo default si el actual es incompatible;
- el catálogo valida defaults y signature skill visuals.

## Adquisición

El dominio admite Starter, Coins, Gems, Event, Achievement y Founder. Todos los offers apuntan exclusivamente a cosméticos.

La política de precios reales, probabilidades o bundles no se fija aquí. Primero se protege la competencia y después se diseña la tienda.
