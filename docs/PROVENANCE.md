# Provenance / Clean-room notes

PogoDom M0.1 es código nuevo escrito para este proyecto.

## Repositorios usados como oráculos de comportamiento / diseño

### Alekssasho/PogoPainter
- `master`: `d283992af40276f60263ca2cae5d6540c431d19d`
- `merge_multi`: `3dc92ba1ff6bfc028c20dd04b81b4abbf56bdb18`

Referencias funcionales:
- board 8×8;
- cuatro spawns de esquina;
- tick ~0.5 s;
- dirección persistente por swipe;
- Bank/Checkpoint;
- Arrow;
- bots que persiguen objetivos.

### bitsmag/squares
- `master`: `c8ea27740158f0d0f48f2cde84e1427c4e473e55`

Referencias arquitectónicas:
- motor separado de transporte;
- resolución de colisiones simultáneas;
- RNG/clock inyectables;
- entorno de entrenamiento de bots.

### AgusCrow/pogo-pandemonium
- `master`: `c91759137b960b28232b88369c18e18291259423`

Usado sólo como catálogo funcional para futuras variantes/power-ups.

### AgusCrow/pogo-painter-web
- `main`: `a803de2de191d91695ffc9fbfd00640ee51b6228`
- `master`: `4b4368a63bc1371f786dedd5067e470a3766a091`

Usado como referencia para scoring heurístico de bots y separación entre estado lógico / interpolación visual.

### TheGuysBrushes/pogo-painter
Usado como oráculo mínimo de movimiento cardinal, pintura y score territorial.

### barisyild/bash-editor
Usado únicamente como herramienta de análisis del formato/arenas/placements del Crash Bash original. No forma parte del runtime de PogoDom.

### mateusfavarin/crashbash
Auditado como referencia de reverse engineering. A la fecha de corte no contiene un módulo Pogo decompilado.

## Qué NO se incorpora

- personajes Crash;
- logos;
- nombres de niveles del producto original;
- música;
- voces;
- texturas;
- modelos;
- animaciones;
- código binario/decompilado del juego comercial.

El objetivo es reproducir y mejorar un tipo de loop de juego con identidad visual y código propios.
