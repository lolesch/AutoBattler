# Triggers — Shifter & Reactor

_Part of [[ItemChain_Design|Item Chain Design]] — Item Chain System_

---

## Shifter

### Definition

A Shifter precedes a weapon and trades stats across the firing and output economies while its condition is continuously true. The weapon always fires regardless — the Shifter just reshapes its profile when conditions are met.

Maps to `ActivatorItem` in code (`FiringStatType` enum covers the firing side).

Multiple Shifters chain freely before a weapon — each applies its own trade while its own condition holds.

---

### What a Shifter Trades

One firing stat against one output stat (or within the same economy). Every bonus costs something on another axis.

|Trade|Meaning|
|---|---|
|`AttackSpeed` ↓ → `Damage` ↑|Slow but hits hard — classic|
|`AttackSpeed` ↑ → `Damage` ↓|Fast but soft — generator build feeding a payload|
|`ResourceCost` ↑ → `Damage` ↑|Expensive but powerful|
|`ResourceCost` ↓ → `Damage` ↓|Cheap and soft — high frequency, low impact|
|`ResourceCost` ↑ → `ResourceGenOnHit` ↑|Costs more but feeds the pool harder on each hit|
|`AttackSpeed` ↓ → `ResourceGenOnHit` ↑|Slower but each hit generates more|

**Known constraint:** `AttackSpeed` trades have no effect when a Reactor replaces the weapon's timer. The Shifter's damage bonus still applies — the timing penalty becomes irrelevant. This is an intentional build exploitation, not a bug.

---

### Shifter Conditions

A continuously readable gate — true or false at any moment. The trade only applies while the condition holds.

**HP state**

- HP below X% — high risk, high reward
- HP above X% — only benefits healthy builds

**Resource state**

- Resource below X% — fires faster when starved
- Resource above X% — fires faster when flush

**Status state**

- Unit currently has a specific status effect _(stub — defer until status system exists)_

**Combat state**

- First X seconds of combat — opening burst window
- Enemy count below X — winning condition bonus
- Ally count below X — last stand pressure

**Deferred:**

- Positional conditions (adjacent to ally, flanked, terrain type) — hex/PawnEffect layer. See [[PawnDesign|Pawn Design]].

---

### Design Notes

Resource threshold pair (above/below X%) is the richest condition — creates a build decision around whether to stay flush or spend aggressively. HP pair is the classic risk/reward axis.

---

## Reactor

### Definition

A Reactor listens for an external event it doesn't control. When the event fires, it replaces the weapon's default timer and fires the weapon. The weapon only fires when the event occurs.

Reactors are the **design differentiator** — they bridge item chains and the hex unit system, making team composition directly empower individual weapon chains.

Reactors also support a condition (`ConditionType`, `ConditionThreshold` in `ReactorItem`) — the event must occur AND the condition must be met for the weapon to fire.

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