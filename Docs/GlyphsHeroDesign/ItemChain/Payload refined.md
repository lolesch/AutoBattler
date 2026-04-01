## Core Principle

Weapons and Payloads are the same entity.
They differ only in how their properties are applied.

- Weapon → produces effects
- Payload → modifies effects

---

## Axes

1. Spatial (where)
2. Temporal (when)
3. Delivery (how it travels)
4. State (status effects)
5. Control (movement & restriction)
6. Terrain (tile interaction)
7. Constraint (rules like LOS, randomness)

---

## Tag System

Single unified tag system describing interaction surfaces.

Examples:
- path, aoe, hit, tick, entity, terrain
- precise, random, los

Tags are interpreted differently depending on state:
- Weapon → capabilities
- Payload → modifiers

---

## Weapon Archetypes

Examples:
- Bolt (projectile)
- Beam (continuous)
- Burst (AoE)
- Field (zone)
- Chain (bounce)
- Strike (melee)
- Dash (movement)
- Trap (triggered)

Each defined by:
- Delivery profile
- Tag set

---

## Payload System

Payloads apply fixed transformations:
- geometry changes
- timing changes
- status application
- control effects
- terrain creation

No context-based logic.
Affinity is determined via tag overlap.

---

## Affinity

Affinity = tag overlap between weapon and payload

- High overlap → strong effect
- Low overlap → weak effect

---

## Status Effects

Statuses scale primarily through payloads.

Core statuses:
- Burn → creates terrain, spreads
- Freeze → control, shatter
- Shock → chaining
- Poison → stacking, area denial
- Bleed → movement-triggered damage
- Root → control amplifier
- Flux → chaos/randomness

---

## Terrain System

Terrain modifies and amplifies status effects.

Types:
- Burning
- Frozen
- Toxic
- Conductive
- Obstructed

---

## Status ↔ Terrain Interactions

Examples:
- Burn + Toxic → explosion
- Shock + Conductive → chain spread
- Freeze + Burn → cancel
- Root + Push → damage conversion

---

## Design Goals

- No combinatorial explosion
- High systemic depth
- Strong player readability
- Emergent gameplay through interaction