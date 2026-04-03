---
tags:
  - Inventory
aliases:
  - Item Chain
---

Item chains are formed in the inventory when attachments are chained to a [[Weapon|Weapon]]. They contribute to the attack based on their type. 
# Chain Item Taxonomy

| Component                    | Role                                                                                    |
| ---------------------------- | --------------------------------------------------------------------------------------- |
| [[Shifter#Shifter\|Shifter]] | Trades [[Weapon#Weapon Stats\|input- against output stats]]                             |
| [[Shifter#Reactor\|Reactor]] | Listens for [[Combat#Combat Events\|Combat Events]] and fires the weapon when it occurs |
| [[Weapon\|Weapon]]           | Fires OR enhances the attack (as payload)                                               |
| **Amplifier**                | Adds to the upstream Weapons output stats                                               |
| **Converter**                | Reclassifies the output type of a matching upstream type                                |

---

# Flow Rules

Two rules cover the whole grammar:

1. **Triggers precede** the action node they fire
2. **Modifiers follow** the component they modify

Reading a chain as a sentence: `Reactor → Shifter → Weapon → Amplifier → Converter → Payload`

_"When hit → trade speed for damage → strike → harder → as fire → when resource full, add burst"_

---

# Connection Grammar

| Node      | Can connect to               | Connections | Chain Position |
| --------- | ---------------------------- | ----------- | -------------- |
| Reactor   | Weapon, Shifter              | 1           | Pre            |
| Shifter   | Weapon, Reactor, Shifter     | 2           | Pre            |
| Weapon    | All                          | 2           | -              |
| Amplifier | Weapon, Amplifier, Converter | 2           | Post           |
| Converter | Weapon, Amplifier, Converter | 2           | Post           |

- Cycles allowed but resolve by stopping at original trigger
- Max 2 connections per component without Splitter/Merger (implemented)


# Conditions

Thresholds that stop chain propagation.
Weapons already come with resource cost as a base condition.

### Payload Condition
Resource cost are paid each time the payload fires.
Payload Condition can introduce n-counter conditions

