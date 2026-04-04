using System;
using UnityEngine;

namespace Code.Data.Items.Weapon
{
    /// <summary>
    /// Base class for all payload hex-grid effects.
    /// Subclasses are serialized inline via [SerializeReference] on PayloadBehavior.Effects.
    /// </summary>
    [Serializable]
    public abstract class PayloadEffect { }

    /// <summary>Applies a status effect to all pawns in the payload's target shape.</summary>
    [Serializable]
    public sealed class StatusPayloadEffect : PayloadEffect
    {
        [field: SerializeField] public StatusEffect Status { get; private set; }
    }

    /// <summary>Displaces or controls pawns in the payload's target shape.</summary>
    [Serializable]
    public sealed class PositionPayloadEffect : PayloadEffect
    {
        [field: SerializeField] public PositionEffectType EffectType { get; private set; }
        /// <summary>Hex distance for Push/Pull. Ignored by Stun.</summary>
        [field: SerializeField] public int Distance { get; private set; }
    }

    /// <summary>Writes a terrain type onto all hexes in the payload's target shape.</summary>
    [Serializable]
    public sealed class TerrainPayloadEffect : PayloadEffect
    {
        [field: SerializeField] public TerrainType TerrainType { get; private set; }
    }

    public enum PositionEffectType
    {
        Push,
        Pull,
        Stun,
    }
}
