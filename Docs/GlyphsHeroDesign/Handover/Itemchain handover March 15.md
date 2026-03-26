# Item Chain — Implementation Handover
_Design decisions locked March 15 2026. Bring uploaded source files into implementation chat alongside this doc._

Files to bring: `WeaponItem.cs`, `WeaponConfig.cs`, `AmplifierConfig.cs`, `TetrisItem.cs`, `PawnStats.cs`, `Stat.cs`, `WeaponStatModifier.cs`, `Modifier.cs`, `MutableFloat.cs`

---

## Priority 1 — WeaponStatType enum

In `AmplifierConfig.cs`, extend `WeaponStatType`:

```
Damage           // exists
AttackSpeed      // exists
ResourceCost     // exists
ResourceGenOnHit // ADD — counterpart to ResourceCost, generator/spender dynamic
```

`CritChance`, `CritMultiplier`, `StatusEffectChance` — deferred, add later without touching existing logic.

---

## Priority 2 — WeaponConfig

Add to `WeaponConfig.cs`:

```csharp
[field: SerializeField] public float ResourceGenOnHit { get; private set; }
```

Only missing stat worth adding now. Everything else (crit, status, range) explicitly deferred.

---

## Priority 3 — WeaponItem + IWeaponItem

Mirror `ResourceGenOnHit` from config into `WeaponItem` and `IWeaponItem`. Same pattern as existing fields.

---

## Priority 4 — PayloadConditionType enum

Extend with:

```
None                   // no condition — always fires as payload
HealthBelow            // exists
HealthAbove            // ADD
ResourceFull           // exists
ResourceBelow          // ADD
ResourceAbove          // ADD
RootDamageAbove        // ADD — root weapon dealt X+ damage this hit
RootKilledTarget       // ADD — root weapon killed the target
TargetHealthBelow      // ADD — execute condition
TargetHealthAbove      // ADD — opening condition
TargetHasStatusEffect  // ADD as stub — wire to always return false until status system exists
```

**`PayloadConditionThreshold` as a single float will not cover all cases long-term.** `TargetHasStatusEffect` needs a status type parameter, not a float. For now stub it as always-false. Design the condition evaluation path to accept richer context before adding complex conditions.

**Conditions are evaluated by the resolver, not the weapon.** The weapon does not evaluate its own condition — the resolver passes combat context in at evaluation time. This matters especially for target-state conditions.

**Payload output is its own package** — not derived from the weapon's root stats. `AttackSpeed` is irrelevant in payload mode. The payload defines its own effect. `BaseDamage` may be used as a reference scalar if the designer chooses, but is not required.

---

## Priority 5 — Activator (design awareness, no implementation yet)

**Model:** weapon always fires on default timer. Activator applies a stat modification while its condition is continuously true. Does not replace the timer. Does not chain with Amplifiers.

**What it modifies:** `AttackSpeed` and `ResourceCost` only.

**`ActivatorConfig` shape:**
- Extends `ItemConfig` (not `StatItemConfig` — no standalone pawn stat effect)
- Fields: `WeaponStatType`, `Value`, `ModifierType` (same pattern as AmplifierConfig)
- Plus: `ActivatorConditionType` enum + threshold value

**Condition types for first pass:**
- HP below X%
- HP above X%
- Resource below X%
- Resource above X%
- First X seconds of combat
- Enemy count below X
- Ally count below X

Status effect condition — stub, always false until status system exists.

---

## Priority 6 — Reactor (design awareness, no implementation yet)

**Model:** replaces weapon's default timer entirely. Weapon only fires when external event occurs.

**First implementation pass events:**
1. This unit is hit
2. This unit kills
3. Any ally attacks
4. Any ally kills
5. Nearby enemy dies
6. This unit's HP drops below X%

Full event list in `Triggers.md`.

---

## Architecture constraints

- Data assembly (`WeaponConfig`, `AmplifierConfig`) must never reference Runtime types
- All new public-facing properties need interface coverage (`IWeaponItem`, `IWeaponStatModifier`)
- `WeaponStatType` and `PayloadConditionType` live in Data assembly — safe to extend
- Resolver evaluates payload conditions — not the weapon. Pass combat context in.
- `MaxConnectors` per subclass already implemented — do not change the list approach
