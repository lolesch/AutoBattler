using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Grids.RectGridInspector;
using Code.Runtime.Statistics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime.Container.Items
{
    /// <summary>
    /// Base class for all grid-placed items.
    /// Handles shape, rotation, and chain slot resolution only.
    /// Stat modifier behaviour is opt-in via IStatModifier — not all items apply stats.
    /// </summary>
    [Serializable]
    public abstract class TetrisItem : AbstractItem, ITetrisItem
    {
        private readonly RectGridBool _shape;
        private readonly ItemConfig   _config;

        public string       Name      { get; private set; }
        public RotationType rotation  { get; set; }
        public RarityType   RarityType { get; private set; }

        protected TetrisItem(ItemConfig config, RotationType rotation) : base(config)
        {
            _shape         = config.Shape;
            _config        = config;
            Name           = config.name;
            this.rotation  = rotation;
            RarityType     = (RarityType)Random.Range(0, Enum.GetValues(typeof(RarityType)).Length);
        }

        // ── Grid placement ────────────────────────────────────────────────

        public override List<Vector2Int> GetPointers(Vector2Int position)
        {
            var normalized = GetNormalizedShape();
            return normalized.Select(p => position + p - GetShapeOrigin(normalized)).ToList();
        }

        public List<Vector2Int> GetNormalizedShape()
        {
            var parts   = _shape.GetVec2Ints();
            var pivot   = parts[0];
            var rotated = parts.Select(p => ApplyRotation(p - pivot, rotation)).ToList();
            var minX    = rotated.Min(p => p.x);
            var minY    = rotated.Min(p => p.y);
            return rotated.Select(p => p - new Vector2Int(minX, minY)).ToList();
        }

        public Vector2Int GetShapeOrigin(List<Vector2Int> normalized = null)
        {
            normalized ??= GetNormalizedShape();
            var minXInTopRow = normalized.Where(p => p.y == 0).Min(p => p.x);
            return new Vector2Int(minXInTopRow, 0);
        }

        public Vector2Int GetDimensions()
        {
            var normalized = GetNormalizedShape();
            var width      = normalized.Max(p => p.x) - normalized.Min(p => p.x) + 1;
            var height     = normalized.Max(p => p.y) - normalized.Min(p => p.y) + 1;
            return new Vector2Int(width, height);
        }

        // ── Chain connectors ──────────────────────────────────────────────

        /// <summary>
        /// Returns all connectors resolved to grid space.
        /// Each entry is (slotGridPosition, gridDirection) where:
        ///   slotGridPosition — the grid cell on this item that hosts the connector
        ///   gridDirection    — the adjacent cell this connector reaches into
        /// Both the position and direction are rotated to match the item's placed rotation.
        /// </summary>
        public List<(Vector2Int slotPos, Vector2Int direction)> GetGridConnectors(Vector2Int placement)
        {
            // Apply the same transform as GetNormalizedShape so connector positions
            // stay consistent with the placed shape at any rotation:
            //   1. Subtract pivot (parts[0]) before rotating — same as GetNormalizedShape
            //   2. Subtract (minX, minY) after rotating    — same as GetNormalizedShape
            var parts  = _shape.GetVec2Ints();
            var pivot  = parts[0];

            var rotatedCells = new List<Vector2Int>(parts.Count);
            foreach (var p in parts)
                rotatedCells.Add(ApplyRotation(p - pivot, rotation));

            var minX      = int.MaxValue;
            var minY      = int.MaxValue;
            foreach (var p in rotatedCells)
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
            }
            var normOffset = new Vector2Int(minX, minY);

            var origin = GetShapeOrigin();
            var result = new List<(Vector2Int, Vector2Int)>(_config.Connectors.Count);

            foreach (var connector in _config.Connectors)
            {
                var rotatedPos = ApplyRotation(connector.LocalPosition - pivot, rotation) - normOffset;
                var rotatedDir = ApplyRotation(connector.Direction.ToVector2Int(),         rotation);
                var gridPos    = placement + rotatedPos - origin;
                result.Add((gridPos, rotatedDir));
            }

            return result;
        }

        // ── Rotation ──────────────────────────────────────────────────────

        protected Vector2Int ApplyRotation(Vector2Int v, RotationType rot)
        {
            var rotations = (int)rot;
            for (var i = 0; i < rotations; i++)
                v = new Vector2Int(v.y, -v.x);
            return v;
        }
    }

    public interface ITetrisItem : IItem
    {
        string       Name       { get; }
        RotationType rotation   { get; }
        RarityType   RarityType { get; }

        List<Vector2Int> GetPointers(Vector2Int position);
        List<Vector2Int> GetNormalizedShape();
        Vector2Int       GetShapeOrigin(List<Vector2Int> normalized = null);
        Vector2Int       GetDimensions();

        List<(Vector2Int slotPos, Vector2Int direction)> GetGridConnectors(Vector2Int placement);
    }

    public interface IEquippable
    {
        void OnEquipped(PawnStats stats);
        void OnUnequipped(PawnStats stats);
    }

    [Serializable]
    public enum RotationType
    {
        None  = 0,
        CW90  = 1,
        CW180 = 2,
        CW270 = 3,
    }
}