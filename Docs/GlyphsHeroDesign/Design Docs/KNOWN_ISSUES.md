## Bugs

- [ ] Enemies should not be draggable
	- [ ] enemies inventory should not be interactable ( inspect, but no add/remove/drag )
cross container drag swaps items
the returning item is placed at the outgoing item with its origin cell, not relative to the cell the outgoing item was placed on top of the returning. make it relative to the dropped cell might feel better. -> or just highlight the required slots in the origin inventory, to show the collisions.
- [ ] implement required slots highlights again -> use backpack battles as reference.
- [ ] Same-container drag should attempt swap first to match cross-container; fallback to force-pickup only if returning item does not fit at source
- [x] Weapons with no chained items should still show as root, as they fire on their own
- adding and removing max resource mods changes the current value. 
	- change implementation or reset on battle start?
	- [ ] implement resource gen first and think of giving a bonus while not in combat
- [ ] The current problem with item chaining is that weapons fired twice when connected to amplifiers from both sides. ChainResolver should apply all connected amplifiers and then perform the attack once. -> could be the general solution for all attachment types

---

# Pending Implementation
## Game Loop

- [x] Pawns should start with a default weapon
- [ ] On player death → trigger Game Over
- [ ] Hex placement phase — not yet wired in scene

## Combat

- [ ] `PawnEffect` firing — system exists but not triggered
- [ ] Unit movement design — range-closing behavior, hex occupancy rules (one unit per hex), not fully designed

## Item Chain

- [ ] Converters + typed signal propagation (damage type, target type, delivery mode, resource type)
- [ ] Reactor ally/nearby events — `OnAllyAttacks`, `OnAllyKills`, `OnNearbyEnemyDies` require coordinator access to other pawns

---

## Code Smells / Tech Debt

- [ ] `TetrisContainer` with `null` `IPawnStats` — mild smell; replace with `NullPawnStats` null-object pattern when a third statless container type appears
- [ ] `ItemTooltipController` calls `ChainResolver.ResolveTopology` on every hover. Acceptable for a dev tool. Upgrade path: cache topology on `OnContentsChanged` in `InventoryView` and pass the cached result to `Show()`.

---

## Deferred (Out of Scope — Revisit Later)

- [ ] Splitter / Merger — branching and merging chains, highest complexity debt
- [ ] Counter-based conditions (every N hits/kills) — valid as a condition type on Reactor or Payload, not as a standalone root trigger. Explore as condition type extension when payload conditions are expanded.

---

## Design Goals

- No combinatorial explosion
- High systemic depth
- Strong player readability
- Emergent gameplay through interaction