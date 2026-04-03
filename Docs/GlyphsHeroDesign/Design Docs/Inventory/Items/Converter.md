---
tags:
  - Item
  - Attachment
  - Inventory
---
### Definition

**Role:** Converters reclassify types within either economy
**Chaining:** After a weapon.

# Risk/Reward
- **Commitment** - damage/target type choice may be strong or weak against the encounter

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
