using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// First real-device presentation pass. It intentionally touches presentation only:
    /// tighter portrait framing, readable URP tile materials, larger avatars and a
    /// compact phone HUD. Match rules and deterministic state remain untouched.
    /// </summary>
    [DefaultExecutionOrder(2000)]
    [DisallowMultipleComponent]
    public sealed class PogoDomMobilePresentationPolish : MonoBehaviour
    {
        private static readonly string[] PlayerNames = { "YOU", "BOT A", "BOT B", "BOT C" };
        private static readonly Color[] PlayerColors =
        {
            new Color(0.98f, 0.22f, 0.25f),
            new Color(0.18f, 0.90f, 0.42f),
            new Color(0.20f, 0.52f, 1.00f),
            new Color(1.00f, 0.80f, 0.12f)
        };

        private static readonly FieldInfo StateField = typeof(PogoDomPrototypeBootstrap).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo LastEventField = typeof(PogoDomPrototypeBootstrap).GetField("_lastEvent", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo RematchMethod = typeof(PogoDomPrototypeBootstrap).GetMethod("Rematch", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly HashSet<int> _fixedRenderers = new HashSet<int>();
        private PogoDomPrototypeBootstrap _bootstrap;
        private Shader _litShader;
        private float _rescan;
        private Texture2D _headerTexture;
        private Texture2D _panelTexture;
        private GUIStyle _titleStyle;
        private GUIStyle _timeStyle;
        private GUIStyle _scoreStyle;
        private GUIStyle _eventStyle;
        private GUIStyle _hintStyle;
        private GUIStyle _modalTitleStyle;
        private GUIStyle _modalBodyStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomMobilePresentationPolish>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomMobilePresentationPolish>();
            }
        }

        private IEnumerator Start()
        {
            _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
            _litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (_litShader == null) _litShader = Shader.Find("Standard");

            // Let the existing visual decorators finish building their first-frame
            // children before applying the device readability pass.
            yield return null;
            yield return null;

            FixPresentationMaterials();
            EnlargePlayers();
            FitPortraitCamera();
        }

        private void Update()
        {
            _rescan -= Time.unscaledDeltaTime;
            if (_rescan > 0f) return;
            _rescan = 0.65f;

            // Pickups and hazards can appear after the initial frame.
            FixPresentationMaterials();
            EnlargePlayers();
        }

        private void FixPresentationMaterials()
        {
            if (_litShader == null) return;

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!NeedsRuntimeMaterial(child.name)) continue;

                var renderer = child.GetComponent<Renderer>();
                if (renderer == null) continue;
                var id = renderer.GetInstanceID();
                if (_fixedRenderers.Contains(id)) continue;

                var color = ReadColor(renderer.sharedMaterial, FallbackColor(child.name));
                var material = new Material(_litShader) { name = "PogoDom_DeviceReadable_" + child.name };
                SetColor(material, color);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", child.name.StartsWith("Tile_") ? 0.24f : 0.58f);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", child.name.StartsWith("Tile_") ? 0.24f : 0.58f);
                renderer.material = material;
                _fixedRenderers.Add(id);
            }
        }

        private static bool NeedsRuntimeMaterial(string objectName)
        {
            if (objectName.StartsWith("Tile_") && !objectName.EndsWith("_Rim") && !objectName.EndsWith("_Glow")) return true;
            if (objectName.StartsWith("Telegraph_")) return true;
            if (objectName.StartsWith("BankCrate_")) return true;
            if (objectName.StartsWith("MysteryCrate_")) return true;
            if (objectName.StartsWith("Arrow_")) return true;
            if (objectName.StartsWith("Speed_")) return true;
            if (objectName.StartsWith("Missile_")) return true;
            if (objectName.StartsWith("Padlock_")) return true;
            return false;
        }

        private static Color FallbackColor(string objectName)
        {
            if (objectName.StartsWith("Telegraph_")) return new Color(1f, 0.12f, 0.05f);
            if (objectName.StartsWith("BankCrate_")) return new Color(0.60f, 0.15f, 0.82f);
            if (objectName.StartsWith("MysteryCrate_")) return new Color(1.00f, 0.72f, 0.05f);
            if (objectName.StartsWith("Arrow_")) return new Color(1.00f, 0.45f, 0.05f);
            if (objectName.StartsWith("Speed_")) return new Color(0.05f, 0.90f, 1.00f);
            if (objectName.StartsWith("Missile_")) return new Color(0.95f, 0.18f, 0.18f);
            if (objectName.StartsWith("Padlock_")) return new Color(0.25f, 0.85f, 0.95f);
            return new Color(0.72f, 0.72f, 0.76f);
        }

        private static Color ReadColor(Material material, Color fallback)
        {
            if (material == null) return fallback;
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return fallback;
        }

        private static void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private void EnlargePlayers()
        {
            for (var i = 0; i < PlayerNames.Length; i++)
            {
                var player = FindDirectChild(PlayerNames[i]);
                if (player == null) continue;

                // The original 0.45 greybox scale made the procedural avatars unreadable
                // on a phone. 0.68 keeps them inside one tile while making silhouette,
                // pogo and expression layers legible at a glance.
                player.localScale = Vector3.one * 0.68f;
            }
        }

        private Transform FindDirectChild(string childName)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == childName) return child;
            }
            return null;
        }

        private void FitPortraitCamera()
        {
            var camera = Camera.main;
            if (camera == null) return;

            Bounds bounds;
            if (!TryGetBoardBounds(out bounds)) return;

            var aspect = Screen.height > 0 ? Screen.width / (float)Screen.height : camera.aspect;
            if (aspect <= 0.01f) aspect = 9f / 16f;

            // A square arena on a portrait phone is width-limited. The previous
            // safety padding left almost a third of the useful width unused.
            // This frame targets roughly 92% of phone width while keeping all tiles,
            // avatars and arena rails visible.
            var verticalFov = aspect < 1f ? 48f : 42f;
            var pitchDegrees = aspect < 1f ? 58f : 52f;
            var safeWidth = aspect < 1f ? 0.92f : 0.90f;
            var safeHeight = aspect < 1f ? 0.84f : 0.86f;
            var worldPadding = aspect < 1f ? 0.30f : 0.42f;
            var visualHeight = 2.25f;

            var pitch = pitchDegrees * Mathf.Deg2Rad;
            var verticalFovRad = verticalFov * Mathf.Deg2Rad;
            var horizontalFov = 2f * Mathf.Atan(Mathf.Tan(verticalFovRad * 0.5f) * aspect);
            var halfWidth = bounds.size.x * 0.5f + worldPadding;
            var halfDepth = bounds.size.z * 0.5f + worldPadding;
            var halfHeight = visualHeight * 0.5f;
            var projectedVerticalHalf = halfDepth * Mathf.Sin(pitch) + halfHeight * Mathf.Cos(pitch);
            var widthDistance = halfWidth / Mathf.Max(0.001f, Mathf.Tan(horizontalFov * 0.5f) * safeWidth);
            var heightDistance = projectedVerticalHalf / Mathf.Max(0.001f, Mathf.Tan(verticalFovRad * 0.5f) * safeHeight);
            var distance = Mathf.Max(widthDistance, heightDistance);

            var target = new Vector3(bounds.center.x, 0.30f, bounds.center.z);
            var offset = new Vector3(0f, Mathf.Sin(pitch) * distance, -Mathf.Cos(pitch) * distance);
            camera.fieldOfView = verticalFov;
            camera.transform.position = target + offset;
            camera.transform.rotation = Quaternion.LookRotation(target - camera.transform.position, Vector3.up);
            camera.nearClipPlane = 0.10f;
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 60f);

            // The old camera-juice component cached the pre-polish framing in Awake
            // and would continuously pull the camera back to it. Readability wins for
            // this device gate; camera juice can be rebased in a later polish pass.
            var juice = camera.GetComponent<PogoDomCameraJuice>();
            if (juice != null) juice.enabled = false;
        }

        private bool TryGetBoardBounds(out Bounds bounds)
        {
            var found = false;
            bounds = default;
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.name.StartsWith("Tile_") || child.name.EndsWith("_Rim") || child.name.EndsWith("_Glow")) continue;
                var renderer = child.GetComponent<Renderer>();
                var childBounds = renderer != null ? renderer.bounds : new Bounds(child.position, Vector3.one);
                if (!found)
                {
                    bounds = childBounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(childBounds);
                }
            }
            return found;
        }

        private MatchState CurrentState()
        {
            if (_bootstrap == null || StateField == null) return null;
            return StateField.GetValue(_bootstrap) as MatchState;
        }

        private string LastEvent()
        {
            if (_bootstrap == null || LastEventField == null) return string.Empty;
            return LastEventField.GetValue(_bootstrap) as string ?? string.Empty;
        }

        private void OnGUI()
        {
            var state = CurrentState();
            if (state == null) return;
            EnsureGuiResources();
            GUI.depth = -1000;

            var safe = Screen.safeArea;
            var safeTop = Mathf.Max(0f, Screen.height - safe.yMax);
            var headerHeight = Mathf.Max(255f, safeTop + 210f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, headerHeight), _headerTexture, ScaleMode.StretchToFill);

            var margin = Mathf.Max(14f, Screen.width * 0.025f);
            var panelTop = safeTop + 10f;
            var panelWidth = Screen.width - margin * 2f;
            var panelHeight = 176f;
            GUI.DrawTexture(new Rect(margin, panelTop, panelWidth, panelHeight), _panelTexture, ScaleMode.StretchToFill);

            GUI.Label(new Rect(margin + 18f, panelTop + 12f, panelWidth * 0.32f, 38f), "POGODOM", _titleStyle);
            GUI.Label(new Rect(margin + panelWidth * 0.33f, panelTop + 8f, panelWidth * 0.34f, 48f), Mathf.CeilToInt(state.RemainingSeconds).ToString(), _timeStyle);
            GUI.Label(new Rect(margin + panelWidth * 0.67f, panelTop + 12f, panelWidth * 0.30f - 18f, 38f), "YOU  " + state.Players[0].Score, RightAligned(_titleStyle));

            var scoreY = panelTop + 62f;
            var segmentWidth = panelWidth / 4f;
            for (var i = 0; i < state.Players.Count && i < 4; i++)
            {
                var style = ScoreStyleFor(i);
                var label = (i == 0 ? "YOU" : "P" + (i + 1)) + "  " + state.Players[i].Score;
                GUI.Label(new Rect(margin + i * segmentWidth, scoreY, segmentWidth, 34f), label, style);
            }

            var eventText = LastEvent();
            if (!string.IsNullOrEmpty(eventText) && eventText != "READY")
                GUI.Label(new Rect(margin + 12f, panelTop + 104f, panelWidth - 24f, 28f), eventText, _eventStyle);

            var elapsed = 75f - state.RemainingSeconds;
            if (elapsed < 10f && !state.IsFinished)
                GUI.Label(new Rect(margin, panelTop + 138f, panelWidth, 30f), "SWIPE TO STEER  •  AUTO-BOUNCE", _hintStyle);

            if (state.IsFinished)
                DrawFinishedModal(state);
        }

        private void DrawFinishedModal(MatchState state)
        {
            var width = Mathf.Min(Screen.width - 32f, 520f);
            var height = 255f;
            var x = (Screen.width - width) * 0.5f;
            var y = Mathf.Max(300f, (Screen.height - height) * 0.5f);
            GUI.DrawTexture(new Rect(x, y, width, height), _panelTexture, ScaleMode.StretchToFill);

            var standings = MatchOutcome.Standings(state);
            var placement = 1;
            for (var i = 0; i < standings.Count; i++)
                if (standings[i].PlayerId == 0) placement = i + 1;

            GUI.Label(new Rect(x + 16f, y + 22f, width - 32f, 44f), placement == 1 ? "VICTORY" : "MATCH OVER", _modalTitleStyle);
            GUI.Label(new Rect(x + 16f, y + 72f, width - 32f, 34f), "YOU  #" + placement + "   •   " + state.Players[0].Score + " PTS", _modalBodyStyle);

            if (GUI.Button(new Rect(x + 24f, y + 132f, width - 48f, 46f), "REMATCH"))
                InvokeRematch(false);
            if (GUI.Button(new Rect(x + 24f, y + 188f, width - 48f, 40f), "REPLAY SAME SEED"))
                InvokeRematch(true);
        }

        private void InvokeRematch(bool sameSeed)
        {
            if (_bootstrap == null || RematchMethod == null) return;
            RematchMethod.Invoke(_bootstrap, new object[] { sameSeed });
        }

        private void EnsureGuiResources()
        {
            if (_headerTexture == null) _headerTexture = SolidTexture(new Color(0.018f, 0.024f, 0.043f, 1f));
            if (_panelTexture == null) _panelTexture = SolidTexture(new Color(0.055f, 0.067f, 0.105f, 0.98f));
            if (_titleStyle != null) return;

            var baseSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.036f), 20, 30);
            _titleStyle = NewStyle(baseSize, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            _timeStyle = NewStyle(baseSize + 10, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            _scoreStyle = NewStyle(baseSize - 2, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            _eventStyle = NewStyle(baseSize - 4, FontStyle.Bold, new Color(0.88f, 0.92f, 1f), TextAnchor.MiddleCenter);
            _hintStyle = NewStyle(baseSize - 5, FontStyle.Normal, new Color(0.72f, 0.80f, 0.94f), TextAnchor.MiddleCenter);
            _modalTitleStyle = NewStyle(baseSize + 8, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            _modalBodyStyle = NewStyle(baseSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        }

        private GUIStyle ScoreStyleFor(int index)
        {
            var style = new GUIStyle(_scoreStyle);
            style.normal.textColor = PlayerColors[index % PlayerColors.Length];
            return style;
        }

        private static GUIStyle RightAligned(GUIStyle source)
        {
            var style = new GUIStyle(source) { alignment = TextAnchor.MiddleRight };
            return style;
        }

        private static GUIStyle NewStyle(int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = alignment
            };
            style.normal.textColor = color;
            return style;
        }

        private static Texture2D SolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "PogoDomHudSolid";
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            return texture;
        }

        private void OnDestroy()
        {
            if (_headerTexture != null) Destroy(_headerTexture);
            if (_panelTexture != null) Destroy(_panelTexture);
        }
    }
}
