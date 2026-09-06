using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class BoardState
    {
        private readonly TileState[] _tiles;

        public int Width { get; }
        public int Height { get; }
        public int Count => _tiles.Length;

        public BoardState(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _tiles = new TileState[width * height];
            for (var i = 0; i < _tiles.Length; i++)
                _tiles[i] = new TileState();
        }

        public bool Contains(GridPos pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public TileState TileAt(GridPos pos)
        {
            if (!Contains(pos))
                throw new ArgumentOutOfRangeException(nameof(pos), $"Position {pos} is outside the board.");
            return _tiles[pos.Y * Width + pos.X];
        }

        public int OwnerAt(GridPos pos) => TileAt(pos).OwnerPlayerId;

        public void SetOwner(GridPos pos, int playerId)
        {
            TileAt(pos).OwnerPlayerId = playerId;
        }

        public GridPos Step(GridPos from, Direction direction)
        {
            var candidate = from;
            switch (direction)
            {
                case Direction.Up:
                    candidate = new GridPos(from.X, from.Y + 1);
                    break;
                case Direction.Right:
                    candidate = new GridPos(from.X + 1, from.Y);
                    break;
                case Direction.Down:
                    candidate = new GridPos(from.X, from.Y - 1);
                    break;
                case Direction.Left:
                    candidate = new GridPos(from.X - 1, from.Y);
                    break;
            }

            return Contains(candidate) ? candidate : from;
        }

        public int CountOwnedBy(int playerId)
        {
            var count = 0;
            for (var i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].OwnerPlayerId == playerId)
                    count++;
            }
            return count;
        }

        public IEnumerable<GridPos> PositionsOwnedBy(int playerId)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var pos = new GridPos(x, y);
                    if (OwnerAt(pos) == playerId)
                        yield return pos;
                }
            }
        }
    }
}
