---
tags:
  - Item
  - Attachment
  - ChainRoot
  - Inventory
---
# Definition

> [!tldr]+ Description
> Replaces the weapon's timer with an external combat event. Weapon only fires when the event occurs

> [!quote]- Purpose - *Why is this essential?*
> Bridges [[Item Chaining]] and the hex unit system - combat directly empowers individual weapon chains.

> [!check]- Reward - *What is the gain?*
> Attack rate can greatly exceed normal timer rate if the triggering event is frequent.

> [!warning]- Risk - *What are the punishments*
> Low combat impact / loss of control - Total dependency on external conditions
> [[Weapon#Input Economy|input costs]] drain resources

> [!fail]- Opposition - *What counters this?*
> Bypassing the event / event suppression

> [!error]- Polarity - *What increases its weakness?*
> Make common events rare on Reactors, 
> High [[Weapon#Input Economy|input costs]] per attack

> [!example]- Progress - *What is the goal*
> Opt-in frequency optimization.
> Overwrite timer-limitation

> [!info]- Depth - *Where are the synergies*
> Overrides [[Weapon]] timers
> Negates frequency penalties from [[Shifter]]
> Hex Grid layer feeds inventory output - positioning, team comp become relevant 

> [!tip]- Appeal - *Does it help the game*
> Bridging the systems - Chain → hex map → back to chain

---

### Notable Patterns (from reference games)

**Every X seconds during [state]** _(Backpack Battles)_ — a timer that only runs while a condition is active. Creates a build goal: enter the state, then the Reactor fires.

**Opponent reaches X [resource]** _(Backpack Battles)_ — watches an enemy stat. Opens poison-stacking as a chain trigger.

**Before defeat** _(Backpack Battles)_ — fires once when this unit would die. Natural fit for a last-stand payload weapon.

---

## Deferred

**Counter-based triggers** (every N hits/kills) — valid as a condition on Reactor or Payload, not as a standalone trigger on root chains. Explore as a condition type when payload conditions are extended.

**Response delay** — fire X seconds after the condition is met. Underexplored in all reference games. Revisit when chain resolution timing is designed.