using System;
using System.Collections.Generic;
using Code.Runtime.HexGrid.HexGridInspector.Runtime;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public sealed class PawnEffect
    {
        [SerializeField] private HexGridBool shape;
        [SerializeField] private int rotation;
        [SerializeField] public string effect;
        
        public List<Hex> GetHexes()
        {
            var shapeHexes = shape.GetHexes();
            
            for( var i = 0; i < shapeHexes.Count; i++ )
            {
                for( var r = 0; r < rotation; r++ )
                    shapeHexes[i] = shapeHexes[i].Rotate( false );
            }
            return shapeHexes;
        }
        
        public void Rotate( bool clockwise ) => rotation = (clockwise ? rotation + 5 : rotation + 1 ) % 6;
        
        // CONCEPT:
        /* So we have the 2 levels of interactions.
         * UNIT LEVEL: unit effects triggered by allies/enemies within the effect shape
         * -> place and align/rotate units to maximize effect stacking
         * ITEM LEVEL: item effects triggered by combat phases/triggers
         * -> collect, assign and align items inside a units inventory
         *
         * CORE LOOP:
         * - recruit?
         * - choose activity (on map)
         * - deploy/plan
         * - fight and collect items -> different modi? last man standing, farming, boss fight...
         * - equip/use items (improve units)
         *
         * UNIT DESIGN:
         * units might have a unique ability that can be cast during combat.
         * the condition to do so may vary. Some might need Mana, others need combat criteria to be met.
         * these abilities shape the outcome of the combat and form the base of team compositions/builds
         * these abilities should then be enhanced by the units equipment
         *
         * what is the reward for taking higher difficulty encounters?
         * once you found a "build", it is intended to abuse it until the next build changing upgrade drops.
         * New drops should hold the potential of a better build composition.
         * -> this probably means that there are item slots so an upgrade would replace the previous item, forcing the build change
         * -> also think about synergies and load outs to quickly change from one to another strategy 
         *
         * ITEMS:
         * I want to have items with multiple purposes
         * - either use the item in the inventory as is
         *   - this can be a stat mod
         *   - or interact with the units ability
         * - or use the item to modify a units effect (crafting currency like in poe)
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