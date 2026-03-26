# Item Chain — Implementation Handover

_Design state as of March 26 2026. Bring uploaded source files alongside this doc._

---

## Design-to-Code Name Mapping

|Design name|Code name|Notes|
|---|---|---|
|Shifter|`ActivatorItem` / `ActivatorConfig`|Rename is cosmetic for now — behaviour changes below|
|Reactor|`ReactorItem` / `ReactorConfig`|No rename needed|
|Weapon|`WeaponItem` / `WeaponConfig`|No change|
|Amplifier|`AmplifierItem` / `AmplifierConfig`|No change|
|Converter|`ConverterItem` / `ConverterConfig`|Not yet implemented|
|Firing stat|`FiringStatType` enum|`AttackSpeed`, `ResourceCost`|
|Output stat|`AttackStatType` enum|`Damage`, `ResourceGenOnHit`|

---

## What Changed Since Last Implementation Session

### Shifter replaces Activator (design rename + behaviour extension)

`ActivatorItem` currently modifies one `FiringStatType` stat conditionally. The Shifter design requires it to **trade** — every modification to a firing stat must come with a corresponding modification on an output stat (or vice versa).

**Current `ActivatorItem` fields:**

```
FiringStatType  WeaponStat         // which firing stat to modify
float           WeaponValue        // how much
ModifierType    WeaponModifierType // flat/percent/etc.
ActivatorConditionType ConditionType
float           ConditionThreshold
```

**Required addition:**

```
AttackStatType  OutputStat         // which output stat to trade against
float           OutputValue        // how much (sign is the tradeoff — negative = costs this)
ModifierType    OutputModifierType
```

This needs to happen in both `ActivatorConfig` (Data) and `ActivatorItem` (Runtime). The chain resolution and `PawnCombatController` logic that applies/removes Shifter mods each timer cycle needs to apply both stat modifications together.

### Converter scope expanded

Converters now cover four output type classifications, not three:

|Conversion|What changes|
|---|---|
|Damage type|physical → fire / ice / poison|
|Target pattern|single → line → AoE|
|Delivery mode|instant → projectile → accumulated burst|
|Resource type|mana-on-hit → health-on-hit · mana cost → health cost|

Resource type conversion is new. It applies to both `ResourceGenOnHit` (what it fills) and `ResourceCost` (what it spends). This affects the typed signal system when Converters are implemented.

### Weapon stat economy clarification

`ResourceGenOnHit` is **output economy**, not firing economy. It is what an attack produces on a successful hit. `AttackSpeed` and `ResourceCost` are the only firing stats.

Current `WeaponItem` already has `ResourceGenOnHit` as `MutableFloat` — correct. No change needed there.

---

## Priority Task List

**1. Add output trade fields to `ActivatorConfig` + `ActivatorItem`**

Two new fields alongside the existing firing stat fields. Both applied/removed together in `PawnCombatController` when condition state changes.

**2. Extend `PayloadConditionType` enum**

Add to the existing enum (lives in Data assembly):

```
None                   // always fires
HealthBelow            // exists
HealthAbove
ResourceFull           // exists
ResourceBelow
ResourceAbove
RootDamageAbove        // root weapon dealt X+ this hit
RootKilledTarget
TargetHealthBelow
TargetHealthAbove
TargetHasStatusEffect  // stub — resolver returns false until status system exists
```

`PayloadConditionThreshold` stays as float for now. `TargetHasStatusEffect` needs a richer parameter type eventually — flag but don't solve yet.

**3. Payload condition evaluation in resolver**

Conditions evaluated by the chain resolver (has full combat context), not by the weapon itself. The resolver needs a combat context parameter passed in at evaluation time.

**4. `ResourceGenOnHit` wiring in combat**

`Fire()` must apply `ResourceGenOnHit` to `_pawn.Stats.mana` after a successful hit. `CanFire` check already guards `ResourceCost`. See KNOWN_ISSUES.

---

## Architecture Constraints (Do Not Break)

- Data assembly never references Runtime types
- All new public properties need interface coverage (`IActivatorItem`, `IWeaponItem`)
- `FiringStatType` and `AttackStatType` are distinct enums — do not merge
- `MaxConnectors = 2` on all current components — already implemented, do not change
- Shifters (ActivatorItem) connect to action nodes only — enforced in `ChainResolver.IsValidConnection`
- Condition applied/removed per timer cycle in `PawnCombatController`, not in chain resolution

---

## Source Files to Bring

- `ActivatorItem.cs` — primary change target
- `ActivatorConfig.cs` — add output trade fields
- `WeaponItem.cs` — reference, no change expected
- `WeaponConfig.cs` — reference, no change expected
- `ItemChain.cs` — may need updating for Shifter mod application
- `ChainResolver.cs` — reference for connection rules
- `PawnCombatController.cs` — Shifter condition evaluation lives here