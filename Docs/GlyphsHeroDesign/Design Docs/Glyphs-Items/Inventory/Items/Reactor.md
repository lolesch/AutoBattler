---
tags:
  - Item
  - Attachment
  - ChainRoot
  - Inventory
---
### Definition

A Reactor listens for an external event it doesn't control. When the event fires, it replaces the weapon's default timer and fires the weapon. The weapon only fires when the event occurs.

Reactors are the **design differentiator** — they bridge item chains and the hex unit system, making team composition directly empower individual weapon chains.

Reactors also support a condition (`ConditionType`, `ConditionThreshold` in `ReactorItem`) — the event must occur AND the condition must be met for the weapon to fire.

# Interactions

- A slow but hard hitting weapon overridden by a Reactor that fires often can exploit Shifter's trading speed for damage. The timing penalty stays irrelevant but the damage bonus remains.

---

### Event List

**Self — receiving damage**

|Event|Notes|
|---|---|
|This unit is hit (any)||
|This unit is critically hit||
|This unit is hit for X% max HP in one strike||
|This unit takes damage of a specific type||
|This unit is stunned||
|This unit is debuffed||
|This unit is flanked / surrounded|_Deferred — hex layer_|

**Self — attacking**

|Event|Notes|
|---|---|
|This unit attacks||
|This unit hits||
|This unit misses|Underexplored — risk/reward potential|
|This unit crits||
|This unit kills||
|This unit overkills||
|This unit stuns an enemy||

**Ally events (hex radius)**

|Event|Notes|
|---|---|
|Any ally attacks||
|Any ally hits||
|Any ally crits||
|Any ally kills||
|Any ally takes damage||
|Any ally dies||
|Ally count drops below X||
|Ally enters a special state (rage, etc.)||

**Enemy events (hex radius)**

|Event|Notes|
|---|---|
|Nearby enemy dies||
|Nearby enemy is debuffed / poisoned||
|Nearby enemy is stunned||
|Enemy enters adjacent hex|_Deferred — hex layer_|
|Enemy count drops below X||

**State / threshold events**

|Event|Notes|
|---|---|
|This unit's HP drops below X%||
|A buff is consumed|From Backpack Battles|
|Chain propagates through this weapon (payload fired)|Unique to this system|
|This unit is last surviving ally||

---

### Notable Patterns (from reference games)

**Every X seconds during [state]** _(Backpack Battles)_ — a timer that only runs while a condition is active. Creates a build goal: enter the state, then the Reactor fires.

**Opponent reaches X [resource]** _(Backpack Battles)_ — watches an enemy stat. Opens poison-stacking as a chain trigger.

**Before defeat** _(Backpack Battles)_ — fires once when this unit would die. Natural fit for a last-stand payload weapon.

---

## Deferred

**Counter-based triggers** (every N hits/kills) — valid as a condition on Reactor or Payload, not as a standalone trigger on root chains. Explore as a condition type when payload conditions are extended.

**Response delay** — fire X seconds after the condition is met. Underexplored in all reference games. Revisit when chain resolution timing is designed.