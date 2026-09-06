# Siguiente tanda recomendada

## M0.2 — cerrar Pogo Painter

1. Implementar Speed Boost con duración configurable (objetivo inicial: 8 s).
2. Implementar Missile + Stun.
3. Agregar `PowerUpInventory` separado del item de tablero.
4. Reemplazar EasyBot por tres niveles:
   - Easy: objetivo simple + error.
   - Medium: scoring heurístico.
   - Hard: interfaz preparada para policy entrenada.
5. Golden tests de 1000 seeds para garantizar:
   - nunca dos jugadores en misma casilla;
   - nunca item encima de otro;
   - nunca item aparece sobre jugador;
   - score nunca decrece;
   - partida siempre termina.
6. Medir `game feel`:
   - tick 0.40 / 0.45 / 0.50 / 0.55 s;
   - altura de salto;
   - ventana de swipe;
   - cantidad de banks/arrows;
   - agresividad de bots.

## M0.3 — presentación propia

- avatar 3D original;
- pogo original;
- squash/stretch;
- landing FX;
- trail;
- VFX de bank;
- VFX de arrow;
- HUD móvil real.
