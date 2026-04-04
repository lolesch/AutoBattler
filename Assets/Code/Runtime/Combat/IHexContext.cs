using System.Collections.Generic;
using Code.Data.Items.Weapon;
using Code.Runtime.Pawns;
using Submodules.Utility.Extensions;

namespace Code.Runtime.Combat
{
    /// <summary>
    /// Spatial hex grid queries consumed by payload execution.
    /// Implemented by the scene-level grid controller.
    /// PawnCombatController holds a nullable reference — all hex-impact calls
    /// are no-ops until a concrete implementation is injected via SetHexContext().
    /// </summary>
    public interface IHexContext
    {
        /// <summary>Returns all pawns within hex range of center, optionally filtered by team.</summary>
        IEnumerable<IPawn> GetPawnsInRange(Hex center, int range, PawnTeam? filter = null);

        /// <summary>Returns all hex coordinates within range of center.</summary>
        IEnumerable<Hex> GetHexesInRange(Hex center, int range);

        /// <summary>Writes a terrain type onto the given hex tile.</summary>
        void SetTerrain(Hex hex, TerrainType type);
    }
}
