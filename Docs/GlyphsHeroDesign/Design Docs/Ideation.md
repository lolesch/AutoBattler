
The pawn / [[PawnDesign#Aura (PawnEffect)|PawnEffect]] could instead have some form of weapon handling that adds modifiers to equipped weapons. More defined Weapon types add a layer of customization and balancing. 

# Block
Armor and Shields can block attacks, without them, no block.
a shield could reflect damage back to the attacker
a legendary shield could reflect all damage of melee weapons. This would be an interesting enemy to overcome -> strategy needed.

Shields against melee, Armor against ranged?

# Stats

pawn could have general weapon stats, so they would apply to all equipped weapons, like
- ResourceCostReduction
- CDR/attackSpeed
- damageScaler
- ...

- [ ] **Max Resource vs Regeneration Rate**
	- Big pool vs fast recovery = different playstyles
- [ ] shield as defensive layer
	- break it, ignore it, reduce its effectiveness and so on
- [ ] Resource Overflow should grant shield for the other resource
	- so ManaOverflow creates health shield and vise versa
	- this could rarely intentionally be flipped or manipulated
- [ ] stat conversion
	- convert % damage to instead drain enemies mana or so
	- convert % damage into resourceGain (leech)
	- convert missing health (not % but flat - higher pools benefit) to X
- [ ] conditionals
	- bonus damage at full/low resource
	- bonus X for attacks that apply burning...
	- +X% per consecutive hit

# Combat Events

- OnDamageTaken
- OnManaSpent
- OnOverheal


have an item with negative stats both in chained and unchained state, so it is a burden to keep/carry it but it has a rare potential synergy that would make it super worthy.

+1 MaxMana per hit and +1% damage per CurrentMana -> scaling mana build but requires scaling mana regen/instant refill to be worth


# Build Strategies

## Tank
getting hit is part of the strategy
- convert damage taken into X
- reflect damage
- OnGettingHit reactor

## AttackSpeed
high OnHitEffect stacking
- more hits = more ResourceGenOnHit
- can stack effects
- can fuel/enable other strategies

## Burst
high resource usage with high downtime
- counters flat mitigation
- if precharged, huge upfront impact on combat

## Glass Cannon
oneshot before getting hit
- relies on setup or support
- high risk/reward