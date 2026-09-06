using System;
using System.Collections.Generic;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    public sealed class PogoDomPrototypeBootstrap : MonoBehaviour
    {
        [Header("Determinism")]
        [SerializeField] private uint seed = 20260905u;

        [Header("First Unity playtest")]
        [SerializeField] private PlaytestRulesetMode playtestMode = PlaytestRulesetMode.Base;

        [Header("Prototype tuning")]
        [SerializeField] private float tileSpacing = 1.05f;
        [SerializeField] private float jumpHeight = 0.8f;
        [SerializeField] private bool autoCreateCameraAndLight = true;

        private MatchConfig _config;
        private MatchState _state;
        private MatchRunner _runner;
        private SwipeDirectionInput _input;
        private float _tickTimer;

        private readonly Dictionary<GridPos, Renderer> _tileRenderers = new Dictionary<GridPos, Renderer>();
        private readonly Dictionary<int, Transform> _playerViews = new Dictionary<int, Transform>();
        private readonly Dictionary<int, Vector3> _animFrom = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Vector3> _animTo = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, GameObject> _itemViews = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, GameObject> _hazardViews = new Dictionary<int, GameObject>();
        private float _animationT = 1f;
        private string _lastEvent = "READY";

        private static readonly Color Neutral = new Color(0.72f, 0.72f, 0.76f);
        private static readonly Color[] PlayerColors =
        {
            new Color(0.92f, 0.18f, 0.18f),
            new Color(0.15f, 0.72f, 0.25f),
            new Color(0.16f, 0.42f, 0.95f),
            new Color(0.95f, 0.78f, 0.12f)
        };

        private void Awake()
        {
            _config = PlaytestMatchProfiles.Create(playtestMode);
            _state = MatchFactory.CreateClassicPrototype(_config);
            _runner = new MatchRunner(_config, new XorShiftRandom(seed));
            _runner.Initialize(_state);

            _input = GetComponent<SwipeDirectionInput>();
            if (_input == null)
                _input = gameObject.AddComponent<SwipeDirectionInput>();

            BuildBoard();
            BuildPlayers();
            SyncItems();
            SyncHazards();
            PaintBoardVisuals();

            if (autoCreateCameraAndLight)
                EnsureCameraAndLight();
        }

        private void Update()
        {
            AnimatePlayers();
            AnimateItems();
            AnimateHazards();

            if (_state.IsFinished)
                return;

            _tickTimer += Time.deltaTime;
            while (_tickTimer >= _config.TickSeconds)
            {
                _tickTimer -= _config.TickSeconds;
                RunTick();
            }
        }

        private void RunTick()
        {
            var inputs = new Dictionary<int, Direction> { [0] = _input.CurrentDirection };
            var result = _runner.Tick(_state, inputs);

            _animationT = 0f;
            foreach (var player in _state.Players)
            {
                var from = result.FromPositions.TryGetValue(player.Id, out var a) ? a : player.Position;
                var to = result.ToPositions.TryGetValue(player.Id, out var b) ? b : player.Position;
                _animFrom[player.Id] = World(from, 0.65f);
                _animTo[player.Id] = World(to, 0.65f);
            }

            if (result.Events.Count > 0)
                _lastEvent = DescribeMostImportant(result.Events);

            PaintBoardVisuals();
            SyncItems();
            SyncHazards();
        }

        private void BuildBoard()
        {
            for (var y = 0; y < _state.Board.Height; y++)
            {
                for (var x = 0; x < _state.Board.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Tile_{x}_{y}";
                    tile.transform.SetParent(transform, false);
                    tile.transform.position = World(pos, 0f);
                    tile.transform.localScale = new Vector3(0.94f, 0.14f, 0.94f);
                    var renderer = tile.GetComponent<Renderer>();
                    renderer.material.color = Neutral;
                    _tileRenderers[pos] = renderer;
                }
            }
        }

        private void BuildPlayers()
        {
            foreach (var player in _state.Players)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = player.Name;
                go.transform.SetParent(transform, false);
                go.transform.position = World(player.Position, 0.65f);
                go.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                go.GetComponent<Renderer>().material.color = PlayerColors[player.Id % PlayerColors.Length];
                _playerViews[player.Id] = go.transform;
                _animFrom[player.Id] = go.transform.position;
                _animTo[player.Id] = go.transform.position;
            }
        }

        private void SyncItems()
        {
            var alive = new HashSet<int>();
            for (var i = 0; i < _state.Items.Count; i++)
            {
                var item = _state.Items[i];
                alive.Add(item.Id);

                if (!_itemViews.TryGetValue(item.Id, out var view))
                {
                    view = GameObject.CreatePrimitive(ItemPrimitive(item.Kind));
                    view.name = $"{item.Kind}_{item.Id}";
                    view.transform.SetParent(transform, false);
                    ApplyItemStyle(view, item.Kind);
                    _itemViews[item.Id] = view;
                }

                view.transform.position = World(item.Position, ItemHeight(item.Kind));
                if (item.Kind == PowerUpKind.Arrow)
                    view.transform.rotation = Quaternion.Euler(0f, Yaw(item.ArrowDirection), 0f);
            }

            RemoveDeadViews(_itemViews, alive);
        }

        private void SyncHazards()
        {
            var alive = new HashSet<int>();
            for (var i = 0; i < _state.Hazards.Count; i++)
            {
                var hazard = _state.Hazards[i];
                alive.Add(hazard.Id);
                if (!_hazardViews.TryGetValue(hazard.Id, out var view))
                {
                    view = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    view.name = $"Telegraph_{hazard.Kind}_{hazard.Id}";
                    view.transform.SetParent(transform, false);
                    view.GetComponent<Renderer>().material.color = new Color(1f, 0.12f, 0.05f);
                    _hazardViews[hazard.Id] = view;
                }
                view.transform.position = World(hazard.Position, 0.10f);
            }
            RemoveDeadViews(_hazardViews, alive);
        }

        private void AnimateItems()
        {
            for (var i = 0; i < _state.Items.Count; i++)
            {
                var item = _state.Items[i];
                if (item.Kind != PowerUpKind.MysteryCrate) continue;
                if (_itemViews.TryGetValue(item.Id, out var view))
                    view.transform.Rotate(0f, 65f * Time.deltaTime, 0f, Space.World);
            }
        }

        private void AnimateHazards()
        {
            for (var i = 0; i < _state.Hazards.Count; i++)
            {
                var hazard = _state.Hazards[i];
                if (!_hazardViews.TryGetValue(hazard.Id, out var view)) continue;
                var width = (hazard.BlastRadius * 2 + 1) * tileSpacing * 0.92f;
                var pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.06f;
                view.transform.localScale = new Vector3(width * pulse, 0.025f, width * pulse);
            }
        }

        private static PrimitiveType ItemPrimitive(PowerUpKind kind)
        {
            switch (kind)
            {
                case PowerUpKind.Speed: return PrimitiveType.Sphere;
                case PowerUpKind.Missile: return PrimitiveType.Capsule;
                case PowerUpKind.Padlock: return PrimitiveType.Sphere;
                default: return PrimitiveType.Cube;
            }
        }

        private static void ApplyItemStyle(GameObject view, PowerUpKind kind)
        {
            var renderer = view.GetComponent<Renderer>();
            switch (kind)
            {
                case PowerUpKind.BankCrate:
                    view.transform.localScale = new Vector3(0.50f, 0.50f, 0.50f);
                    renderer.material.color = new Color(0.60f, 0.15f, 0.82f);
                    break;
                case PowerUpKind.MysteryCrate:
                    view.transform.localScale = new Vector3(0.52f, 0.52f, 0.52f);
                    renderer.material.color = new Color(1.00f, 0.72f, 0.05f);
                    break;
                case PowerUpKind.Arrow:
                    view.transform.localScale = new Vector3(0.52f, 0.12f, 0.20f);
                    renderer.material.color = new Color(1.00f, 0.45f, 0.05f);
                    break;
                case PowerUpKind.Speed:
                    view.transform.localScale = Vector3.one * 0.32f;
                    renderer.material.color = new Color(0.05f, 0.90f, 1.00f);
                    break;
                case PowerUpKind.Missile:
                    view.transform.localScale = new Vector3(0.18f, 0.32f, 0.18f);
                    renderer.material.color = new Color(0.95f, 0.18f, 0.18f);
                    break;
                case PowerUpKind.Padlock:
                    view.transform.localScale = Vector3.one * 0.30f;
                    renderer.material.color = new Color(0.25f, 0.85f, 0.95f);
                    break;
                default:
                    view.transform.localScale = Vector3.one * 0.28f;
                    renderer.material.color = Color.white;
                    break;
            }
        }

        private static float ItemHeight(PowerUpKind kind)
        {
            return kind == PowerUpKind.BankCrate || kind == PowerUpKind.MysteryCrate ? 0.42f : 0.30f;
        }

        private static void RemoveDeadViews(Dictionary<int, GameObject> views, HashSet<int> alive)
        {
            var remove = new List<int>();
            foreach (var pair in views)
                if (!alive.Contains(pair.Key)) remove.Add(pair.Key);
            for (var i = 0; i < remove.Count; i++)
            {
                var id = remove[i];
                if (views[id] != null) UnityEngine.Object.Destroy(views[id]);
                views.Remove(id);
            }
        }

        private void PaintBoardVisuals()
        {
            foreach (var pair in _tileRenderers)
            {
                var owner = _state.Board.OwnerAt(pair.Key);
                pair.Value.material.color = owner < 0 ? Neutral : PlayerColors[owner % PlayerColors.Length];
            }
        }

        private void AnimatePlayers()
        {
            if (_playerViews.Count == 0) return;

            _animationT = Mathf.Clamp01(_animationT + Time.deltaTime / Mathf.Max(0.01f, _config.TickSeconds));
            var arc = Mathf.Sin(_animationT * Mathf.PI) * jumpHeight;

            foreach (var pair in _playerViews)
            {
                var id = pair.Key;
                if (!_animFrom.TryGetValue(id, out var from) || !_animTo.TryGetValue(id, out var to)) continue;
                var p = Vector3.Lerp(from, to, Smooth01(_animationT));
                p.y += arc;
                pair.Value.position = p;
            }
        }

        private Vector3 World(GridPos pos, float y)
        {
            var originX = -((_state.Board.Width - 1) * tileSpacing) * 0.5f;
            var originZ = -((_state.Board.Height - 1) * tileSpacing) * 0.5f;
            return new Vector3(originX + pos.X * tileSpacing, y, originZ + pos.Y * tileSpacing);
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);

        private static float Yaw(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return 0f;
                case Direction.Right: return 90f;
                case Direction.Down: return 180f;
                case Direction.Left: return 270f;
                default: return 0f;
            }
        }

        private void EnsureCameraAndLight()
        {
            if (Camera.main == null)
            {
                var cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                var camera = cameraGo.AddComponent<Camera>();
                camera.fieldOfView = 45f;
                cameraGo.transform.position = new Vector3(0f, 10.5f, -9.5f);
                cameraGo.transform.LookAt(Vector3.zero);
            }

            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        private static string DescribeMostImportant(List<MatchEvent> events)
        {
            MatchEvent best = null;
            var bestPriority = -1;
            for (var i = 0; i < events.Count; i++)
            {
                var priority = EventPriority(events[i].Type);
                if (priority >= bestPriority)
                {
                    bestPriority = priority;
                    best = events[i];
                }
            }
            return best == null ? "" : Describe(best);
        }

        private static int EventPriority(MatchEventType type)
        {
            switch (type)
            {
                case MatchEventType.MatchFinished: return 100;
                case MatchEventType.HazardDetonated: return 90;
                case MatchEventType.EnclosureCaptured: return 85;
                case MatchEventType.Banked: return 80;
                case MatchEventType.CrateOpened: return 75;
                case MatchEventType.MissileFired: return 70;
                case MatchEventType.PadlockActivated: return 65;
                case MatchEventType.SpeedActivated: return 60;
                case MatchEventType.ArrowUsed: return 60;
                case MatchEventType.HazardTelegraphed: return 55;
                case MatchEventType.TileProtected: return 50;
                case MatchEventType.TileStolen: return 30;
                default: return 0;
            }
        }

        private static string Describe(MatchEvent e)
        {
            switch (e.Type)
            {
                case MatchEventType.Banked: return $"P{e.PlayerId + 1} BANK +{e.Value}";
                case MatchEventType.CrateOpened: return $"P{e.PlayerId + 1} CRATE → {e.ItemKind}";
                case MatchEventType.ArrowUsed: return $"P{e.PlayerId + 1} ARROW ({e.Value})";
                case MatchEventType.SpeedActivated: return $"P{e.PlayerId + 1} SPEED";
                case MatchEventType.MissileFired: return $"P{e.PlayerId + 1} MISSILE → P{e.SecondaryPlayerId + 1}";
                case MatchEventType.PadlockActivated: return $"P{e.PlayerId + 1} SHIELD";
                case MatchEventType.EnclosureCaptured: return $"P{e.PlayerId + 1} AREA +{e.Value}";
                case MatchEventType.HazardTelegraphed: return "TNT WARNING";
                case MatchEventType.HazardDetonated: return $"TNT BOOM -{e.Value} tiles";
                case MatchEventType.TileProtected: return $"P{e.SecondaryPlayerId + 1} BLOCKED STEAL";
                case MatchEventType.MatchFinished: return "MATCH FINISHED";
                default: return e.Type.ToString();
            }
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            GUI.Box(new Rect(12, 12, 320, 220), "POGODOM — UNITY PLAYTEST");
            GUI.Label(new Rect(28, 42, 285, 24), $"Mode: {playtestMode}", style);
            GUI.Label(new Rect(28, 66, 285, 24), $"Time: {_state.RemainingSeconds:0.0}s", style);
            for (var i = 0; i < _state.Players.Count; i++)
            {
                var player = _state.Players[i];
                GUI.Label(new Rect(28, 92 + i * 23, 285, 24), $"{player.Name}: {player.Score}", style);
            }
            GUI.Label(new Rect(28, 184, 285, 22), _lastEvent, GUI.skin.label);
            GUI.Label(new Rect(28, 204, 285, 22), "Swipe anywhere • auto-bounce", GUI.skin.label);
        }
    }
}
