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
        private float _animationT = 1f;
        private string _lastEvent = "PogoDom M0.1";

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
            _config = new MatchConfig();
            _state = MatchFactory.CreateClassicPrototype(_config);
            _runner = new MatchRunner(_config, new XorShiftRandom(seed));
            _runner.Initialize(_state);

            _input = GetComponent<SwipeDirectionInput>();
            if (_input == null)
                _input = gameObject.AddComponent<SwipeDirectionInput>();

            BuildBoard();
            BuildPlayers();
            SyncItems();
            PaintBoardVisuals();

            if (autoCreateCameraAndLight)
                EnsureCameraAndLight();
        }

        private void Update()
        {
            AnimatePlayers();

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
                _lastEvent = Describe(result.Events[result.Events.Count - 1]);

            PaintBoardVisuals();
            SyncItems();
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
                    view = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    view.name = $"{item.Kind}_{item.Id}";
                    view.transform.SetParent(transform, false);
                    view.transform.localScale = item.Kind == PowerUpKind.BankCrate
                        ? new Vector3(0.5f, 0.5f, 0.5f)
                        : new Vector3(0.52f, 0.12f, 0.20f);
                    view.GetComponent<Renderer>().material.color = item.Kind == PowerUpKind.BankCrate
                        ? new Color(0.60f, 0.15f, 0.82f)
                        : new Color(1.0f, 0.45f, 0.05f);
                    _itemViews[item.Id] = view;
                }

                view.transform.position = World(item.Position, item.Kind == PowerUpKind.BankCrate ? 0.42f : 0.28f);
                if (item.Kind == PowerUpKind.Arrow)
                    view.transform.rotation = Quaternion.Euler(0f, Yaw(item.ArrowDirection), 0f);
            }

            var remove = new List<int>();
            foreach (var pair in _itemViews)
            {
                if (!alive.Contains(pair.Key))
                    remove.Add(pair.Key);
            }
            for (var i = 0; i < remove.Count; i++)
            {
                var id = remove[i];
                if (_itemViews[id] != null) Destroy(_itemViews[id]);
                _itemViews.Remove(id);
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
                if (!_animFrom.TryGetValue(id, out var from) || !_animTo.TryGetValue(id, out var to))
                    continue;

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

        private string Describe(MatchEvent e)
        {
            switch (e.Type)
            {
                case MatchEventType.Banked: return $"P{e.PlayerId + 1} BANK +{e.Value}";
                case MatchEventType.ArrowUsed: return $"P{e.PlayerId + 1} ARROW ({e.Value} changed)";
                case MatchEventType.MatchFinished: return "MATCH FINISHED";
                default: return e.Type.ToString();
            }
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            GUI.Box(new Rect(12, 12, 280, 170), "POGODOM M0.1");
            GUI.Label(new Rect(28, 42, 250, 28), $"Time: {_state.RemainingSeconds:0.0}s", style);
            for (var i = 0; i < _state.Players.Count; i++)
            {
                var player = _state.Players[i];
                GUI.Label(new Rect(28, 68 + i * 23, 250, 24), $"{player.Name}: {player.Score}", style);
            }
            GUI.Label(new Rect(28, 158, 250, 22), _lastEvent, GUI.skin.label);
        }
    }
}
