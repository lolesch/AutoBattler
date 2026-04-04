---
tags:
  - Item
  - Attachment
  - Inventory
---
# Definition

- **Commitment** - damage/target type choice may be strong or weak against the encounter

> [!tldr]+ Description
> Reclassifies the output type of the nearest upstream weapon

> [!quote]- Purpose - *Why is this essential?*
> Opens interactions and on the Hex Grid; turns a generic weapon into an encounter-specific tool.

> [!check]- Reward - *What is the gain?*
> Access to element interactions, spread patterns, and resource routing unavailable through raw stats.

> [!warning]- Risk - *What are the punishments*
> Hard commitment - the converted type may be weak or irrelevant against the current encounter.

> [!fail]- Opposition - *What counters this?*
> Converted type immunity

> [!error]- Polarity - *What increases its weakness?*
> Combat with moving parts, type doesn't fit anymore

> [!example]- Progress - *What is the goal*
> Build specialization towards interaction synergies.
> Adopt to the current encounter

> [!info]- Depth - *Where are the synergies*
> Attack expression takes place on Hex Grid - type conversion tailors interactions.

> [!tip]- Appeal - *Does it help the game*
> The _surgeon_ play: precision typing for maximum exploitation of enemy vulnerability.

---

## Signal Types and Conversion

- **Typed signals** exist for: damage type, target type, delivery mode, resource type
- Converters change typed signals locally — nearest upstream action node only
- weapons can have types/Tags that could be converted, converting a 'heavy' hammer to a 'swift' one


**Converter types for output:**

| Output               | Conversion                               | Notes                                   |
| -------------------- | ---------------------------------------- | --------------------------------------- |
| Damage type          | physical → fire / ice / poison / etc.    | Changes damage scaling and interactions |
| Target pattern       | single → line → AoE                      | Hex-based spread                        |
| Delivery mode        | instant → projectile → accumulated burst | Trading frequency for impact            |
| Resource type (gen)  | mana-on-hit → health-on-hit              | Changes what `ResourceGenOnHit` fills   |
| Resource type (cost) | mana cost → health cost                  | Changes what `ResourceCost` spends      |
