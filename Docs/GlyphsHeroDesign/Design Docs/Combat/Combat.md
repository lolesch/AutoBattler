
Two levels of interaction:
- **Unit level** — effects triggered by allies/enemies within effect shapes on the hex map.
- **Item level** — [[Weapon#Root Mode|Weapon]] attacks that drive moment-to-moment combat.

These two levels synergize but do not conflict. Unit abilities handle inter-unit effects. Item chains handle what the unit _does_ when it attacks.

---

# Attack Combo / Synergies
Parallel Execution
multiple Weapons fire simultaneously
- how to create combos (besides payload)
	- effect stacking and conditions -> pawn/hex layer, not inventory!
- timed interactions - apply water, then apply lightning and so on

---

# Hex-grid Properties
- position / positioning
- terrain / obstacles / LoS
- attack shape / range 
- movement / projectiles

**Positional conditions** (flanked, adjacent to ally, terrain type) — deferred to hex layer. See [[Pawn|Pawn Design]].

---

# Combat Events

- OnDamageTaken
- OnManaSpent
- OnOverheal

---
