## Bugs

- [ ] Enemies should not be draggable
- [ ] Same-container drag should attempt swap first; fall back to force-pickup only if displaced item does not fit in source
- [ ] Weapons with no chained items should still show as root, as they fire on their own

## Game Loop

- [ ] Pawns should start with a default weapon
- [ ] On player death → trigger Game Over
- [ ] Hex placement phase — not yet wired in scene

---

## Pending Implementation (Scoped)

### Inventory & UI

- [ ] `PawnEffect` firing — system exists but not triggered

### Combat

- [ ] Unit movement design — range-closing behavior, hex occupancy rules (one unit per hex), not fully designed
- [ ] Resource spending — `Fire()` must spend `ResourceCost` from `_pawn.Stats.mana` before dealing damage; `CanFire` check already in place
- [ ] Resource generation on hit — `Fire()` must apply `ResourceGenOnHit` to `_pawn.Stats.mana` after a successful hit

### Item Chain — Not Yet Implemented

- [ ] Converters + typed signal propagation (damage type, target type, delivery mode, resource type)
- [ ] Shifter cross-axis trading — `ActivatorItem` currently only modifies `FiringStatType`; needs a second stat field for the output side of the trade
- [ ] Reactor ally/nearby events — `OnAllyAttacks`, `OnAllyKills`, `OnNearbyEnemyDies` require coordinator access to other pawns
- [ ] Shifter time/count conditions — `FirstXSeconds` needs combat start timestamp; `EnemyCountBelow`/`AllyCountBelow` need coordinator access
- [ ] Connection type validation — Reactors connect only to weapons; Shifters connect to Reactors/Shifters/weapons. Not yet enforced beyond max-connection limit in ItemConfig.
- [ ] Chained Reactors in series — architecturally invalid, needs guard in ChainResolver. Parallel Reactors require Splitter component.

---

## Code Smells / Tech Debt

- [ ] `TetrisContainer` with `null` `IPawnStats` — mild smell; replace with `NullPawnStats` null-object pattern when a third statless container type appears
- [ ] `ItemTooltipController` calls `ChainResolver.ResolveTopology` on every hover. Acceptable for a dev tool. Upgrade path: cache topology on `OnContentsChanged` in `InventoryView` and pass the cached result to `Show()`.

---

## Deferred (Out of Scope — Revisit Later)

- [ ] Splitter / Merger — branching and merging chains, highest complexity debt
- [ ] Encounter information visibility — required for Conditions and Converters to fully pay off
- [ ] Crafting / currency system
- [ ] Loadout / build switching
- [ ] Counter-based conditions (every N hits/kills) — valid as a condition type on Reactor or Payload, not as a standalone root trigger. Explore as condition type extension when payload conditions are expanded.
- [ ] ProcChance as downstream modifier — reliability stat for ResourceGenOnHit and payload firing. Behaves like an Amplifier targeting a specific output property. Defer until output economy is stable.
- [ ] StatusApplication — output stat for status effects. Requires status system design first.

---

## Outdated

These entries no longer reflect the current design direction. Kept for reference.

- ~~Activator as standalone trigger that replaces the weapon timer~~ — Activator renamed to Shifter. Shifters trade firing stats against output stats while a condition holds. The weapon always fires on its own timer; Shifters reshape its profile. See [[Triggers#Shifter|Shifter]].