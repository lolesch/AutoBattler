# Pawn Design — Design Document
*Tactical Puzzle RPG — Design Session Handoff*

---

## Pawn Identity

A pawn's identity is defined by three layers working in combination:

1. **Weapon chain** — the pawn's moment-to-moment combat expression. Fastest-changing layer; can be rebuilt from combat to combat via loot.
2. **Aura (PawnEffect)** — unit-to-unit effects radiating outward based on hex positioning. Mid-pace layer; changes over a run via opt-in player choices.
3. **Terrain interaction** — unit-to-world effects driven by what the pawn stands on. Slowest layer; set per map/zone.

These layers are parallel, not hierarchical. A pawn feels like something because all three are present simultaneously. Replacing a weapon chain redirects combat expression but does not erase the pawn's positional identity.

---

## The Three Layers

### Weapon Chain
The fastest-changing layer. Defined by whatever weapons and chain items are in inventory. The starter weapon is the default expression of the pawn's combat character — it scaffolds new players and gives experienced players a known baseline to build from or contradict.

A player can fully contradict the starter weapon by replacing or chaining around it. This is intentional — depth requires that option. The pawn's positional identity (Aura + Terrain) provides enough permanent character that a blank-slate weapon chain is not a design risk.

### Aura (PawnEffect)
Passive effects that radiate outward from the pawn's hex position to nearby allies or enemies. Currently defined by a hex shape and a modifier. The player builds this layer over a run via opt-in pickups — permanent upgrades the player can take or skip, similar to Slay the Spire relics. This layer changes at run pace, not combat pace.

### Terrain Interaction
Effects triggered by what the pawn stands on. Terrain is persistent on the map and visually readable via grid tiles. Reshaping terrain is temporary if it occurs at all.

Terrain and Aura are intentionally coupled — terrain can enable or gate Aura conditions. Examples of the design space:
- Standing on lava gives a damage bonus
- Aura effect only applies to enemies currently on fire
- Standing on lava sets nearby enemies on fire, enabling the Aura condition
- Moving off ice removes a speed buff

This coupling means positioning is not just "who is adjacent to whom" — it also asks "what am I standing on and does it activate my Aura."

---

## Positioning as a Tactical Tool

Every unit placement has three simultaneous optimization reads:

| Read | System | Question |
|---|---|---|
| **Team composition** | Aura | Who do I buff or debuff by being here? |
| **Terrain affinity** | Terrain ↔ Aura | Does this tile enable my effects? |
| **Weapon range** | Item chain | Am I close enough to hit what my chain wants? |

These three pulls on a single placement decision is where the tactical puzzle lives.

---

## Pacing Layers

| Layer | Change pace | Change driver |
|---|---|---|
| **Terrain** | Per map / zone | World state, slow progression |
| **Aura** | Per run | Player opt-in pickups |
| **Weapon chain** | Per combat | Loot drops |

Each layer has its own investment curve. Terrain is read and reacted to. Aura is authored slowly. The weapon chain is rebuilt rapidly.

---

## Infrastructure Dependency

Terrain-affinity depth — like Converter depth in the item chain — requires **encounter and terrain information to be visible during deployment.** A pawn whose Aura only fires on lava tiles is dominant or dead weight depending on the map. This is a feature, not a bug, but it only pays off when the player can see and plan around it.

This is the same infrastructure gap noted for Converters. Design as if terrain preview exists, but do not over-invest in terrain-gated Aura complexity until it is realized.

---

## Combat Structure (Unresolved)

Two-phase combat is under consideration:

**Phase 1 — Positioning (turn-based):** Player moves units, reads terrain, adjusts team composition. This is the deliberate planning layer that makes terrain and Aura legible and actionable.

**Phase 2 — Resolution (real-time or simultaneous):** Weapon chains fire on their timers, Reactors trigger on events, damage resolves. This preserves the item chain design intact.

This model avoids the conflict between turn-based repositioning and real-time weapon chain firing by giving each a separate time domain. Reactors are naturally compatible with turn-event framing.

**Key open question:** Does the player need to reposition *during* combat, or is pre-combat deployment enough? The answer determines whether mid-combat turns are necessary or whether a solid placement phase suffices.

---

## Open / Out of Scope

- **Combat phase structure** — two-phase model is the leading candidate but not committed
- **Terrain types and specific interactions** — design space is large, not yet enumerated
- **Aura upgrade pool design** — opt-in relic-style pickups not yet designed
- **Starter weapon assignment per pawn** — needs to express each pawn's Aura identity clearly
