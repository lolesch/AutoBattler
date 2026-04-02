
## Contextual Summary

This game is a tactical puzzle RPG on a hex grid. Units occupy hexes and have positional interactive effects with neighbors. Each unit has a 2D grid inventory where item placement matters.

Two levels of interaction:

- **Unit level** — effects triggered by allies/enemies within effect shapes on the hex map. See [[PawnDesign|Pawn Design]].
- **Item level** — item chains that drive moment-to-moment combat.

These two levels synergize but do not conflict. Unit abilities handle inter-unit effects. Item chains handle what the unit _does_ when it attacks.

---

## Component Taxonomy

|Component|Type|Role|
|---|---|---|
|**Shifter**|Trigger-modifier|Precedes weapon. Trades firing stats against output stats while its condition holds. Maps to `ActivatorItem` in code.|
|**Reactor**|Trigger|Precedes weapon. Listens for an external event and fires the weapon when it occurs.|
|**Weapon**|Action|Fires an effect — dual mode (root / payload)|
|**Amplifier**|Modifier|Adds to the nearest upstream action node's output stats. No tradeoff.|
|**Converter**|Modifier|Reclassifies the output type of the nearest upstream action node.|

There is no standalone Condition or Payload component. These concepts are absorbed into Weapon's dual-mode design.

Full detail: [[Triggers|Trigger Components]] · [[Weapons|Weapon Design]]

---

## Stat Economy

All weapon stats belong to one of two economies:

**Usage economy** — governs when and at what cost the weapon activates:

- `AttackSpeed` / internal cooldown
- `ResourceCost` (resource type → Converter)

**Attack economy** — governs what the attack produces:

- `Damage`
- `ResourceGenOnHit` (resource type → Converter)
- `ProcChance` — reliability of secondary effects (deferred)
- `StatusApplication` — deferred until status system exists

**Shifters** operate across both economies — trading a firing stat for an output stat or vice versa. **Amplifiers** operate within the output economy only — pure addition, no tradeoff. **Converters** reclassify types within either economy.

See [[Weapons|Weapon Design]] for full stat tables and conversion types.

---

## Flow Rules

Two rules cover the whole grammar:

1. **Triggers precede** the action node they fire
2. **Modifiers follow** the component they modify

Reading a chain as a sentence: `Reactor → Shifter → Weapon → Amplifier → Converter → Payload`

_"When hit → trade speed for damage → strike → harder → as fire → when resource full, add burst"_

---

## Connection Grammar

|Node|Can receive from|Can connect to|
|---|---|---|
|Shifter / Reactor|—|Action node only|
|Weapon|Trigger, Modifier|Amplifier, Converter, Weapon|
|Amplifier|Action node|Amplifier, Converter, Weapon|
|Converter|Action node, Amplifier|Amplifier, Converter, Weapon|

**Key constraints:**

- Triggers connect to action nodes only — never to Amplifiers or Converters (enforced in `ChainResolver.IsValidConnection`)
- Modifiers follow what they modify — no backwards connections
- Cycles allowed but resolve by stopping at original trigger
- Max 2 connections per component without Splitter/Merger (implemented)

---

## Signal Types and Conversion

- **Typed signals** exist for: damage type, target type, delivery mode, resource type
- Converters change typed signals locally — nearest upstream action node only
- weapons can have types/Tags that could be converted, converting a 'heavy' hammer to a 'swift' one

---

## Bidirectionality and Cycles

Chains are **bidirectional**. A weapon fires both arms simultaneously — each arm resolves independently as its own chain pass.

**Cycles are allowed.** Resolution rule: a chain pass stops when it reaches the node that originally triggered it. Prevents infinite loops while preserving weapon triangles and similar configurations.

In a weapon triangle (A→B→C→A), Weapon A firing propagates through B and C before returning to A and stopping. B and C fire in payload mode — conditions checked, secondary effects contribute.

**Amplifier double duty:** An Amplifier between two weapons modifies whichever weapon is upstream relative to the current firing direction. In a cycle, the same Amplifier can modify different weapons depending on which trigger fires. Intentional depth — needs balance awareness.

---

## Risk/Reward Depth

- **Payload conditions** — chain space before a weapon builds toward its condition; space after scales its effect. See [[Weapons#Payload Mode|Payload Mode]].
- **Shifter tradeoffs** — a slow+hard weapon overridden by a Reactor that fires often exploits the Shifter's sacrifice. The timing penalty stays irrelevant but the damage bonus remains.
- **Converter commitment** — damage/target type choice may be strong or weak against the encounter
- **Bidirectional builds** — Amplifiers doing double duty in cycles
- **Delivery conversion** — trading frequency for burst

All deepen once **encounter information is visible during deployment** — planned, not yet implemented.

---

## Implementation Priority

1. **Weapon (payload mode, conditional) + Converters** — needs encounter visibility
2. **Bidirectional chains** — validate legibility first
3. **Splitter/Merger** — last, highest complexity debt

---

## Sigil Visual Language

See [[Sigil_Design_Handoff|Sigil Design]] for full visual system.

Current baseline (v7): trigger = connection line character · weapon = abstract center mark · amplifiers = orbital shapes · converter = color/size/thickness on weapon mark.

---

## Reference Games

|Game|Relevant mechanic|Lesson|
|---|---|---|
|**Noita**|Wand building, spell + modifier chains|Closest ancestor. Protect legibility actively|
|**Path of Exile**|Skill gems, CoC/CwDT|Validates dual-mode weapon; condition+effect as one unit|
|**Backpack Battles**|Item activation triggers, resource thresholds|Rich trigger grammar; generator/spender patterns|
|**Backpack Hero**|Grid adjacency, implicit item relationships|Tactile spatial puzzling|
|**Last Epoch**|Per-skill modification trees|Deep but isolated — Reactor cross-system linking is more interesting|
|**Battle Brothers**|Combat status, flanking, fatigue|Positional state conditions|
|**TFT / Auto-battlers**|Trait triggers, on-death, ally events|Ally/enemy event vocabulary|
|**SealChain: Call of Blood**|Direct contemporary|Just launched, overlapping design space|
|**Tyranny**|Sigil stacking|Reference for composite ability icons|

---

## Open / Out of Scope

- **Encounter information visibility** — planned, not implemented. Required for Converters and conditional payloads to fully pay off.
- **Sigil visual language** — in progress. See [[Sigil_Design_Handoff|Sigil Design]].
- **Splitter/Merger** — out of scope. Cycles via splitter need explicit visited-node detection.
- **Crafting/currency** — not yet designed
- **Loadouts/build switching** — not yet designed
- **Positional conditions** (flanked, adjacent to ally, terrain type) — deferred to hex layer. See [[PawnDesign|Pawn Design]].

See [[KNOWN_ISSUES|Known Issues]] for active bugs and pending items.