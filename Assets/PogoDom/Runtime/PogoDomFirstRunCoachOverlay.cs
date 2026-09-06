using System.Collections.Generic;
using PogoDom.Core;
using PogoDom.Session;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Runtime expression of the product rule: the first match teaches itself in
    /// under twenty seconds without pausing, modal screens or extra buttons.
    /// </summary>
    [DefaultExecutionOrder(1400)]
    [DisallowMultipleComponent]
    public sealed class PogoDomFirstRunCoachOverlay : MonoBehaviour
    {
        private PogoDomPrototypeBootstrap _bootstrap;
        private FirstRunCoach _coach;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomFirstRunCoachOverlay>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomFirstRunCoachOverlay>();
            }
        }

        private void Awake()
        {
            _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
            _coach = new FirstRunCoach(localPlayerId: 0);
        }

        private void OnEnable()
        {
            if (_bootstrap == null) _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
            if (_coach == null) _coach = new FirstRunCoach(localPlayerId: 0);
            if (_bootstrap != null) _bootstrap.PresentationEvents += ObserveEvents;
        }

        private void OnDisable()
        {
            if (_bootstrap != null) _bootstrap.PresentationEvents -= ObserveEvents;
        }

        private void Update()
        {
            if (_coach == null || _coach.IsComplete) return;
            _coach.AdvanceWithoutTick(Time.unscaledDeltaTime);
        }

        private void ObserveEvents(IReadOnlyList<MatchEvent> events)
        {
            if (_coach == null || _coach.IsComplete || events == null) return;
            _coach.ObserveEvents(events, 0f);
        }

        private void OnGUI()
        {
            if (_coach == null || _coach.IsComplete) return;
            var directive = _coach.Current;
            if (directive.Hint == CoachHint.None) return;

            var safe = Screen.safeArea;
            var scale = Mathf.Clamp(Screen.height / 1080f, 0.70f, 1.35f);
            var cardWidth = Mathf.Min(safe.width * 0.88f, 620f * scale);
            var cardHeight = 116f * scale;
            var x = safe.x + (safe.width - cardWidth) * 0.5f;
            var y = safe.yMax - cardHeight - Mathf.Max(18f * scale, safe.height * 0.035f);
            var rect = new Rect(x, y, cardWidth, cardHeight);

            string title;
            string body;
            int step;
            Describe(directive.Hint, out title, out body, out step);

            var oldBackground = GUI.backgroundColor;
            var oldContent = GUI.contentColor;
            GUI.backgroundColor = new Color(0.07f, 0.08f, 0.12f, 0.92f);
            GUI.contentColor = Color.white;
            GUI.Box(rect, GUIContent.none);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = Mathf.RoundToInt(25f * scale)
            };
            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(16f * scale),
                wordWrap = true
            };
            var stepStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = Mathf.RoundToInt(12f * scale)
            };

            GUI.Label(new Rect(x + 12f * scale, y + 8f * scale, cardWidth - 24f * scale, 34f * scale), title, titleStyle);
            GUI.Label(new Rect(x + 18f * scale, y + 42f * scale, cardWidth - 36f * scale, 46f * scale), body, bodyStyle);
            GUI.Label(new Rect(x, y + 89f * scale, cardWidth, 20f * scale), StepDots(step), stepStyle);

            GUI.backgroundColor = oldBackground;
            GUI.contentColor = oldContent;
        }

        private static void Describe(CoachHint hint, out string title, out string body, out int step)
        {
            switch (hint)
            {
                case CoachHint.SwipeToTurn:
                    title = "DESLIZÁ";
                    body = "Un dedo. Cambiá de dirección; el salto es automático.";
                    step = 1;
                    break;
                case CoachHint.PaintAndSteal:
                    title = "PINTÁ Y ROBÁ";
                    body = "Caé sobre casillas para hacerlas tuyas y quitar territorio rival.";
                    step = 2;
                    break;
                default:
                    title = "BANKEÁ";
                    body = "Buscá la caja violeta para convertir tu territorio en puntos.";
                    step = 3;
                    break;
            }
        }

        private static string StepDots(int step)
        {
            switch (step)
            {
                case 1: return "●  ○  ○";
                case 2: return "●  ●  ○";
                default: return "●  ●  ●";
            }
        }
    }
}
