using System;
using Code.Runtime.HexGrid.HexGridInspector.Runtime;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public sealed class PawnEffect
    {
        [SerializeField] public HexGridBool shape;
        [SerializeField] public string effect;
        
        // CONCEPT:
        /* So we have the 2 levels of interactions.
         * TOP LEVEL: unit effects triggered by allies/enemies within the effect shape
         * -> place and align/rotate units to maximize effect stacking
         * UNIT LEVEL: item effects triggered by combat phases/triggers
         * -> collect, assign and align items inside a units inventory
         *
         * ITEMS:
         * I want to have items with multiple uses
         * - either use the item in the inventory as item
         * - or merge the item into your unit to modify the units effect (crafting currency like in poe)
         * maybe items could be broken down into currency
         *
         * each item has an activation trigger. This can be a
         * - static event (battle start, health below X ...)
         * - timer (every x sec)
         * - passive (every x activation of synergetic items)
         * WEAPONS:
         * - cooldown/attack speed (timer)
         * - damage
         * - resource cost
         * - accuracy?
         *
         * defensive layers - avoid, reduce, regen
         * - also have mechanics that are effective against hard-hitting and others against many attacks
         * SHIELDS:
         * -
         *
         * TRIGGERS:
         * - On
         */
    }
}