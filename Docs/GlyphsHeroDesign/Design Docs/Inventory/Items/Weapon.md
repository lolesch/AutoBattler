---
tags:
  - Item
  - ChainRoot
  - Inventory
aliases:
  - Weapons
---
# Definition

> [!tldr]+ Description
> The source of all attacks - modified via [[Item Chaining]]
> See [[Payload]] if used in another weapons chain -> Tradeoff between usage

> [!quote]- Purpose - *Why is this essential?*
> Without a weapon a pawn does nothing -> no combat impact.

> [!check]- Reward - *What is the gain?*
> Combat impact via predictable attacks. Apply damage, status effects and terrain changes, gain resources

> [!warning]- Risk - *What are the punishments*
> High resource usage with low combat impact.

> [!fail]- Opposition - *What counters this?*
> High health and defense layers

> [!error]- Polarity - *What is its weakness?*
> Attacks are gated by conditions - slow timers, low resource pool, high resource drain

> [!example]- Progress - *What is the goal*
> match game difficulty, overcome enemies

> [!info]- Depth - *Where are the synergies*
> The entry point of [[Item Chaining]] - all other items attach to it. 
> Synergizes with itself (payload mode). 

> [!tip]- Appeal - *Does it help the game*
> Necessity for pawn combat, Entry point for item chaining (the core of glyphs hero)

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
