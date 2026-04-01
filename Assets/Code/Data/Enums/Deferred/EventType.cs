namespace Code.Data.Enums
{
    public enum EventType
    {
        // RunBased
        OnRunEnd = 302,
        // OnRecruit=303,
        // OnTraitEquipped =304,

        // Turnbased
        OnBattleStart = 310,
        OnBattleEnd = 0,
        OnRoundStart = 312,
        OnRoundEnd = 313,

        // Pawn
        OnTurnStart = 10,
        OnTurnEnd = 11,

        OnMove = 12,
        OnAttack = 13,
        OnAttackMiss = 14,
        OnAttackHit = 15,
        OnKill = 17,
        OnGotHit = 17,
        OnDefeated = 18,

        OnHeal = 50,
        OnHealthChanged = 51,
    }
}