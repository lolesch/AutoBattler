using UnityEngine;

namespace Code.Data.Enums
{
    public enum CrowdControlType
    {
        [Tooltip("unable to move, attack, or cast while being untargetable and invulnerable")]
        Stasis = 1,

        [Tooltip("unable to move, attack, or cast and move toward the source with reduced movement speed")]
        Charm = 2,

        [Tooltip("unable to move, attack, or cast and move away from the source with reduced movement speed")]
        Fear = 3,

        [Tooltip("unable to move, attack, or cast and attempt to basic attack the source")]
        Taunt = 4,

        [Tooltip("unable to move, attack, or cast")]
        Stun = 5,

        [Tooltip("unable to move, attack, or cast and moved into a direction")]
        KnockUp = 6,

        [Tooltip("unable to move, attack, or cast as long as it doesn't get damaged")]
        Sleep = 7,


        [Tooltip("unable to move, or cast mobility skills")]
        Root = 8,

        [Tooltip("unable to attack")]
        Disarm = 9,

        [Tooltip("unable to cast")]
        Silence = 10,


        [Tooltip("miss attacks. Miss occurs on-hit, and does not prevent attacks from being declared")]
        Blind = 11,

        [Tooltip("interrupt channeled and charged skills")]
        Disrupt = 12,

        [Tooltip("attack allies/fight on the caster's side")]
        Confusion = 13,

        //// this is a statusEffect
        //[Tooltip("reduced movement speed")]
        //Slow = 14,
        //
        //// this is a statusEffect
        //[Tooltip("reduced attack speed")]
        //Exhaust = 15,
        //
        //// this is a statusEffect
        //[Tooltip("reduced damage")]
        //Weaken = 16,
        //
        //// this is a statusEffect
        //[Tooltip("reduced resistances")]
        //Vulnerable = 17,


        [Tooltip("reduced sight radius and loses allied vision")]
        Nearsight = 18,
    }
}