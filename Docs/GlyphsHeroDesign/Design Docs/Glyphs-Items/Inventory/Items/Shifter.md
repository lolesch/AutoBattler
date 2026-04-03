---
tags:
  - Item
  - Attachment
  - ChainRoot
  - Inventory
---
### Definition

**Role:** Shifters trade stats across both economies - trading input- for output stat or vice versa.
**Chaining:** Before a weapon. Multiple Shifters chain freely before a weapon

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

- Positional conditions (adjacent to ally, flanked, terrain type) — hex/PawnEffect layer. See [[Pawn|Pawn Design]].

---

### Design Notes

Resource threshold pair (above/below X%) is the richest condition — creates a build decision around whether to stay flush or spend aggressively. HP pair is the classic risk/reward axis.

---

