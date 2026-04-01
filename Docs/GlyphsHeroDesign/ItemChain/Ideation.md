
The pawn / [[PawnDesign#Aura (PawnEffect)|PawnEffect]] could instead have some form of weapon handling that adds modifiers to equipped weapons. More defined Weapon types add a layer of customization and balancing. 

# Block
Armor and Shields can block attacks, without them, no block.
a shield could reflect damage back to the attacker
a legendary shield could reflect all damage of melee weapons. This would be an interesting enemy to overcome -> strategy needed.

Shields against melee, Armor against ranged?


# Payload
"Payloads do not add power. They redefine the function of power"
“A payload must enable at least one behavior that cannot exist if both weapons are used independently.”
- “hit enemies behind walls”
- “control positioning”
- “stack delayed bursts”
"Status Effects should scale better via payload than via standalone weapons"

right now, each payload also acts as a weapon, creating these bi-directional chains. 
probably it should instead be a tradeoff between using the weapon as attack OR as payload modifying another attack.
This might feel as a downgrade competing against its own attack value.
To overcome this, the combination should create something impossible otherwise 
- Ability modifiers like D3 runes, HellClock relics and so on
- Payload is where status effects should live
- Bridge to hex grid, Range, Knockback/Pull, AoE, Origin point, Target
- stacking payloads should have some form of negative impact like lower accuracy or mana drain...

## Scripted Synergies
Items can highlight scripted interactions, like 'Backpack Battles' merge function. 

Rock + Whip = Sling
- highly increased range but decreasing accuracy over distance
	Rock has a weak payload, but as a Sling, that payload is much stronger
	'Projectiles' might define impact behavior

Mirror - that (as payload) 
- reverts the chain resolution, so that it goes back through all amplifiers and adds the weapon itself as payload to its own firing 
	- but also blocks the mirrored inventory slots? 
	- Or is simply a large item

## Ranged & Ammunition/Projectiles

The ranged weapon defines range and targeting
the 'payloaded' ammunition defines impact behavior

# Weapon/Payload Affinity

A payload is a single, context-free transformation applied to a weapon’s delivery system.  
Weapons differ only in how strongly they can express that transformation
Every weapon should be “good at expressing certain payloads” and “awkward at others” - not forbidden.
Affinity determines not if a payload works, but how deeply it transforms the weapon
### Example:

- Whip → Spatial + Behavioral
- Beam → Temporal + State
- Explosion → State + Spatial
prevents “everything works everywhere”


## Affinity Thresholds
think DIII set bonus or TL2 skill tiers
- how well do weapon and payload(s) match

## Delivery Characteristics - Hex Grid Bridge
### Propagation

How does the attack _travel_?
- Instant → great for **burst modifiers**
- Projectile → great for **on-hit effects**
- Area → great for **status & stacking**
- Attached → great for **continuous / control effects**
### Effects
- burst
- on-hit
- status (stacking)
- control
#### Effect Persistence

How long does the effect exist?
- One Shot  → loves **burst conversion**
- Lingering → loves **state payloads**
- Repeating → loves **amplifiers & stacking**

### Targeting Model

How does it decide where to act?
- **Direct aim** → great for **precision effects**
- **Area-based** → great for **AoE modifiers**
- **Self-centered** → great for **defensive / aura payloads**
- **Chain / bounce** → great for **status spread**
- **Lock-on / mark-based**

### Payload = Modifier Function
It modifies a limited set of parameters
- Geometry (shape, direction, origin)
- Timing (delay, repetition, sequencing)
- Hit behavior (pierce, split, attach)
- State (status effects, stacking rules)
- Control (push, pull, lock, redirect)

Each **weapon defines capabilities**:
- Supports Projectile?
- Supports AoE?
- Supports Continuous?
- Supports Targeting Model?
- Supports Control?

And:

### Affinity = how well the weapon can express that modifier

### Each payload has tags
- `requires_path`
- `requires_hit_event`
- `scales_with_aoe`
- `scales_with_tick_rate`
- `applies_status`
- `modifies_targeting`

### Each weapon has matching traits
- `has_path`
- `has_hit_event`
- `is_aoe`
- `is_continuous`
- `has_targeting`
- `has_control`