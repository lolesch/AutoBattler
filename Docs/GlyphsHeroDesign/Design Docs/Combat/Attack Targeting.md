---
tags:
  - Combat
  - Weapon
  - HexGrid
  - Targeting
  - Delivery
  - Propagation
---

# Attack Delivery

Every weapon attack resolves across three independent axes. Each axis can be modified by [[Item Chaining]] without affecting the others.

|Axis|Question|Chain Modifier|
|---|---|---|
|**[[Attack Delivery#Target Selection\|Target Selection]]**|Who does the weapon aim at?|[[Shifter]]|
|**[[Attack Delivery#Delivery Pattern\|Delivery Pattern]]**|How does the attack travel there?|[[Converter]]|
|**[[Attack Delivery#Propagation\|Propagation]]**|What happens on impact?|[[Payload]]|

Reading an attack as a sentence: _"Strike the nearest enemy → along a beam → exploding on impact"_

---

# Target Selection

Defines which unit the weapon aims at. Set per weapon in config. Predictable by design — enemies follow the same rules, so the player can learn and counter patterns via positioning.

| Strategy                          | Description                                                                             |
| --------------------------------- | --------------------------------------------------------------------------------------- |
| `Nearest`                         | Closest valid target by hex distance                                                    |
| `LowestHP`                        | Target most likely to be finished off — maximizes kills                                 |
| `HighestHP`                       | Focus threat — countered by spreading HP across the team                                |
| `RandomWithinShape`               | Fires into a defined hex shape with no lock-on — spread damage, hard to predict exactly |
| `MostBuffed/Debuffed`             |                                                                                         |
| `Specific tag` <br>(e.g. burning) |                                                                                         |

**Chain modification:** [[Shifter]] can gate or override the active strategy — for example, restricting `Nearest` to only valid targets that are **burning**. This is a risky transformation: targeting precision is gained, but reliability is lost until the condition is met on the board.

One active strategy per weapon at resolution time. The Shifter replaces, not stacks.

---

# Delivery Pattern

Defines how the attack travels from the firing pawn to its target. Set per weapon. Governs which tiles are affected in transit — relevant to LoS, obstacles, and terrain.

| Pattern      | Description                                          | Obstacle Behavior                   |
| ------------ | ---------------------------------------------------- | ----------------------------------- |
| `Projectile` | Straight line to target, hits first unit in path     | Blocked by units and terrain        |
| `Beam`       | Straight line through target, hits all units in path | Blocked by solid terrain only       |
| `Arc`        | Travels over obstacles to land on target hex         | Ignores units and terrain in flight |
| `Dash`       | Attacker moves to target hex, impact on arrival      | Requires clear path                 |
| `Adjacent`   | Hits all hexes within attacker's immediate ring      | No travel, instant                  |
| *Homing*     | *to still hit even if target was moved?*             |                                     |

**Chain modification:** [[Converter]] can reclassify the delivery pattern — for example, converting `Projectile` to `Beam`, or `Adjacent` to a directional cone. This is a type reclassification, not a stat change, consistent with Converter's role.

*Converter stacking might be beneficial*

---

# Propagation

Defines what happens at the point of impact. Not inherent to the weapon — propagation is added via [[Payload]] and stacks as the chain grows. A weapon with no payload has no propagation; it simply hits.

|Behavior|Description|
|---|---|
|`Pierce`|Continues through the first target along the same trajectory|
|`Fork`|Splits into two new instances at ~60° angles from the original path|
|`Chain`|Bounces to the nearest valid target not yet hit by this instance|
|`Split`|Spawns N auto-targeting instances at the point of impact|
|`Explode`|Deals area damage around the impact hex|
|`Return`|Travels back toward the origin after impact|

Propagation behaviors are **ordered** when multiple are active — a payload stack resolves Pierce before Fork before Chain, and so on. This creates the endgame pattern space: a weapon that pierces, then forks, then explodes on each fork's impact is the product of stacked payload upgrades.

Each propagation instance inherits the active target selection and delivery pattern of the originating weapon unless a subsequent modifier changes it.

---

# Phase Ownership

|Axis|Placement Phase|Resolution Phase|
|---|---|---|
|Target Selection|Read enemy strategies, counter via positioning|Locked — fires per config|
|Delivery Pattern|Assess coverage of weapon shapes|Locked — fires per config|
|Propagation|Predict impact spread|Executes in payload order|

Positioning decisions during the placement phase are direct answers to the target selection and delivery patterns of both sides. This is where the tactical puzzle lives.

---

# Design References

- **Path of Exile** — projectile behavior priority chain (Split → Pierce → Fork → Chain → Return) is the model for propagation ordering. PoE treats these as additive modifiers that compose into complex attack expressions through upgrade stacking, not as a fixed weapon property.
- **Into the Breach** — delivery pattern taxonomy (Projectile, Beam, Artillery/Arc, Dash, Adjacent, Free Aim) maps cleanly onto hex grids. ITB also demonstrates that fully telegraphed, deterministic targeting rules on both sides are sufficient to generate deep positional counterplay — randomness is not required for tactical depth.
- **Backpack Battles** — fatigue as a stalemate escape valve; async combat resolution as a reference for how item-driven combat can resolve without player input during the resolution phase.

---

Two things worth flagging before you drop this into the vault:

1. **`RandomWithinShape`** — the shape it fires into isn't defined here. That shape will need to live somewhere in `WeaponConfig`, same as how aura shapes are authored. Worth noting that as a connection to the existing system.
2. **`Dash`** pattern requires a clear path — that implies some form of pathfinding or at least occupancy checking during resolution, which is the closest this design gets to movement. Keep that in mind when implementing it; it's a special case.

Everything else is additive and shouldn't require new systems — it builds on the hex math, the chain resolver, and the payload dispatch hierarchy that already exist.