using System;
using Code.Runtime.Pawns;

namespace Code.Runtime.Core.Combat
{
    public sealed class CombatEventBus : ICombatEventBus
    {
        public event Action<IPawn>              OnUnitAttacked;
        public event Action<IPawn, IPawn> OnUnitHit;
        public event Action<IPawn>              OnUnitDefeated;

        public void PublishAttacked(IPawn attacker)                      => OnUnitAttacked?.Invoke(attacker);
        public void PublishHit(IPawn attacker, IPawn victim)       => OnUnitHit?.Invoke(attacker, victim);
        public void PublishDefeated(IPawn unit)                          => OnUnitDefeated?.Invoke(unit);
    }
    
    public interface ICombatEventBus
    {
        event Action<IPawn>              OnUnitAttacked;
        event Action<IPawn, IPawn> OnUnitHit;       // attacker, victim
        event Action<IPawn>              OnUnitDefeated;

        void PublishAttacked(IPawn attacker);
        void PublishHit(IPawn attacker, IPawn victim);
        void PublishDefeated(IPawn unit);
    }
}