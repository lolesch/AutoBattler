using Code.Data.Enums;
using Submodules.Utility.Extensions;

namespace Code.Runtime.HexGrid
{
    public interface IHexOccupant
    {
        Hex HexPosition { get; }
        PawnTeam Team { get; }
        void RegisterWith(HexGridController controller);
        void UnregisterFrom(HexGridController controller);
    }
}