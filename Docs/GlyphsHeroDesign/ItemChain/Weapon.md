# Weapon

_Part of [[ItemChain_Design|Item Chain Design]] — Item Chain System_

---

## Dual Mode

Every Weapon item has two built-in modes. The same item, two roles depending on its position in the chain.

---

### Root Mode

Fires on its own timer by default. Full output. Active when this weapon is the chain's starting point — no upstream weapon is propagating through it.

A Shifter placed before a root weapon trades its firing stats while its condition holds. A Reactor placed before it fires the weapon on an external event instead.

The root weapon drives chain propagation — its firing is what reaches downstream weapons.

---

### Payload Mode

Fires when chain propagation from an upstream root weapon reaches it. Does not fire on its own.

Steps on arrival: Check own condition (`PayloadCondition`).

1. If not met — stop chain propagation.
2. If met — deliver payload package and propagate chain onward.

**The payload package is its own definition** — not derived from the weapon's root stats. It has its own effect, its own damage expression, its own output type. `AttackSpeed` is irrelevant in payload mode — the weapon is not firing on a timer.

The payload effect and condition are fixed properties of the weapon item itself, scaled by Amplifiers and Converters positioned around it.

---

### Starter Weapon

Each unit arrives with a unique starter weapon that defines its combat character. Replaceable but not required to replace — gives new players a scaffold and experienced players a choice.

---

## Stat Economy

### Firing Economy

Governs when and at what cost the weapon activates. Operated on by **Shifters**.

|Stat|Notes|
|---|---|
|`AttackSpeed`|Firing frequency. Monotone without tension — Shifters create tradeoffs against output stats|
|`ResourceCost`|Paid each time the weapon fires. Acts as a threshold gate — can't fire without it|

Resource type for cost is physical — convertible via Converter (e.g. mana cost → health cost).

### Output Economy

Governs what the attack produces. Operated on by **Amplifiers** (add) and **Converters** (reclassify).

|Stat|Notes|
|---|---|
|`Damage`|Core output|
|`ResourceGenOnHit`|Generated on each successful hit. Feeds downstream weapons' resource costs|
|`ProcChance`|Reliability of secondary effects — payload firing chance, gen proc chance. Deferred.|
|`StatusApplication`|Deferred until status system exists|

**Converter types for output:**

|Output|Conversion|Notes|
|---|---|---|
|Damage type|physical → fire / ice / poison / etc.|Changes damage scaling and interactions|
|Target pattern|single → line → AoE|Hex-based spread|
|Delivery mode|instant → projectile → accumulated burst|Trading frequency for impact|
|Resource type (gen)|mana-on-hit → health-on-hit|Changes what `ResourceGenOnHit` fills|
|Resource type (cost)|mana cost → health cost|Changes what `ResourceCost` spends|

---

## Payload Mode Stats

|Stat|Notes|
|---|---|
|`PayloadCondition`|What must be true for the payload to fire|
|`PayloadConditionThreshold`|Parameter for the condition. Not always a float 0–1 — becomes richer as condition types expand|
|Payload output|Its own package. Not required to reference root stats.|

---

## Payload Conditions

Evaluated by the **chain resolver**, which has full combat context. The weapon does not evaluate its own condition.

**Own unit state**

- HP below X%
- HP above X%
- Resource full
- Resource below X%
- Resource above X%
- Currently has a status effect _(stub — always false until status system exists)_

**Root weapon outcome (this chain pass)**

- Root weapon dealt X or more damage on this hit
- Root weapon critically hit
- Root weapon killed the target

**Target state**

- Target HP below X% — execute condition
- Target HP above X% — opening condition
- Target has status effect _(stub — needs status type parameter, not a float threshold)_
- Target is the last enemy

**Chain context**

- This is the Nth chain pass this combat — first-attack bonus
- Root weapon has fired X times this combat — ramp-up condition

**Implementation notes:**

- `PayloadConditionThreshold` as a single float will not cover all cases. `TargetHasStatusEffect` requires a status type parameter. Design the condition evaluation path to accept richer context before adding these.
- Condition is evaluated by the resolver, not the weapon. The resolver passes full combat context at evaluation time.

---

## Dual-Mode Design Validation

- **PoE CoC/CwDT** — the linked spell fires conditionally when reached, not freely. Same dual-mode principle.
- **Noita trigger spells** — a trigger spell fires its payload conditionally. The payload doesn't become a second free-firing wand.
- A standalone free-firing second weapon in a chain is the design smell all reference systems avoid — payload mode is the solution.