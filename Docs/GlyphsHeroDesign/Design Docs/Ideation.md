
The pawn / [[Pawn#Aura (PawnEffect)|PawnEffect]] could instead have some form of weapon handling that adds modifiers to equipped weapons. More defined Weapon types add a layer of customization and balancing. 

# Block
Armor and Shields can block attacks, without them, no block.
a shield could reflect damage back to the attacker
a legendary shield could reflect all damage of melee weapons. This would be an interesting enemy to overcome -> strategy needed.

Shields against melee, Armor against ranged?

# Stats

pawn could have general weapon stats, so they would apply to all equipped weapons, like
- ResourceCostReduction
- CDR/attackSpeed 
	- adrenaline/focus (status effect) grants attack speed per stack
		- is applied by combat events such as getting flanked, being hit, executing someone and so on.
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

---

# Item Ideas

have an item with negative stats both in chained and unchained state, so it is a burden to keep/carry it but it has a synergy counterpart that converts the negative into power

+1 MaxMana per hit and +1% damage per CurrentMana -> scaling mana build but requires scaling mana regen/instant refill to be worth

**Mirror shard** - that (as payload) 
- reverts the chain resolution, so that it goes back through all amplifiers and adds the weapon itself as payload to its own firing 
	- but also blocks the mirrored inventory slots? 
	- Or is simply a large item

sacrificia/ceremonial knife
- targets self, deals little DMG but offers ... yeah, what?

---

## Weapon Archetypes

Examples:
- Bolt (projectile)
- Beam (continuous)
- Burst (AoE)
- Field (zone)
- Chain (bounce)
- Strike (melee)
- Dash (movement)
- Trap (triggered)

---

# Achievements

"Hoarder" - fill the entire inventory with 1x1 items
"Chainer" - fill the entire inventory with chained items
"Chain Master" - fill the entire inventory with one chain
