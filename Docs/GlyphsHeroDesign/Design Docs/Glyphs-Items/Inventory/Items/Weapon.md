---
tags:
  - Item
  - ChainRoot
  - Inventory
aliases:
  - Weapons
---
### Definition

A weapon is the root cause of an attack. Each weapon has input- and output-stats that can be modified via [[Item Chaining]].
A Weapon can also be part of another weapons chain - turning it from root- into payload-mode, where it adds to the attacks hex grid layer, not just the stats.

Every Weapon has two built-in modes depending on its position in an [[Item Chaining|Item Chain]].
- Delivery → “verb” (shoot, swing)
- Payload → “meaning” (burn, pull, mark, explode) -> adds effects
Tradeoff between using the weapon as attack OR as payload modifying another attack.

---

# Starter Weapon

Each unit arrives with a unique starter weapon that defines its combat character. Replaceable but not required to replace — gives new players a scaffold and experienced players a choice.

---

# Delivery Mode

Fires on its own timer by default. Can be modified via [[Item Chaining]].
Resource costs are paid each time the weapon fires. Acts as a threshold gate
## Weapon Stats
### Input Economy

define when and at what cost the weapon attacks.
A weapons internal timer based on Attack Speed can be overwritten by a [[Reactor]], forcing it to instead fire on external [[Combat#Combat Events|Combat Events]].

- Attack Speed
- Life Cost
- Mana Cost
- Proc Chance -> Reliability of secondary effects — payload firing chance, gen proc chance.
### Output Economy

define what the attack produces.

- Damage
- Life On Hit / Leech
- Mana On Hit
- StatusApplication
- Range
#### Range

What defines the range/shape of an [[Weapon#Root Mode|attack]]?
- weapons could define a range, payloads might interact with the 'melee'/'ranged' tags for higher affinity between 'ranged' and 'projectiles' 

---

# Payload Mode

**Fires:** when a weapon chain reaches it. Does not fire on its own.
Chain propagation ends if the Payload Condition is not met.

The payload effect and condition are fixed properties of the weapon item itself, scaled by Amplifiers and Converters positioned downstream.

- Payloads do not add attacks. They transform the function of attacks -> define impact behavior
- A payload must enable at least one behavior that cannot exist if both weapons are used independently
- Status Effects should scale better via payload- than via delivery-mode

Possible effects
- hit enemies behind walls
- control positioning
- stack delayed bursts

To pay off, the payload should create something impossible without it 
- Ability modifiers like D3 runes, HellClock relics and so on
- Payload is where status effects should live
- Bridge to hex grid, Range, Knockback/Pull, AoE, Origin point, Target
- stacking payloads should have some form of negative impact like lower accuracy
	- As each payload has its own condition, this is solved. the default condition is high resource drain.

## Payload Parameters / Axes
- Spatial (where)
	- Shape (origin, direction)
	- Targeting (type, count)
	- Delivery (instant, projectile)
		- Constrains (LOS, randomness)
- Temporal (when) 
	- instant, delay, repetition, duration
- Effect (what)
	- State/Control
		- status effects (stacking rules)
		- push, pull, stun...
	- Propagation
		- pierce, split, attach
	- Terrain changes

## Weapon-Payload Affinity

Affinity determines how well do weapon and payload match 
- prevent “everything works everywhere”
- this is measured in affinity thresholds (matching tag overlap)
	- think DIII set bonus or TL2 skill tiers

Weapons differ in how strongly they can express Payload attack transformation.
Every weapon should be “good at expressing certain payloads” and “awkward at others”.

Tags are interpreted differently depending on state:
- Weapon → capabilities
- Payload → modifiers

## Tags
- `path`, `aoe` / projectile?
- `hit`, `dot`
- `status`
- `controll`
- `targeting`
- `entity`
- `terrain`
- `pierce`
- `los`
- `random`
- `melee` , `ranged`

The same tags apply to the payload mode and define the affinity threshold

### Targeting Model
- **Direct aim / Lock-on / mark-based**
- **Area-based**
- **Self-centered**
- **Chain / bounce** → great for **status spread**

## Scripted Synergies
Items can highlight scripted interactions, like 'Backpack Battles' merge function. 

Rock + Whip = Sling
- highly increased range but decreasing accuracy over distance
	Rock has a weak payload, but as a Sling, that payload is much stronger
## Design References

- **PoE CoC/CwDT** — the linked spell fires conditionally when reached, not freely. Same dual-mode principle.
- **Noita trigger spells** — a trigger spell fires its payload conditionally. The payload doesn't become a second free-firing wand.
