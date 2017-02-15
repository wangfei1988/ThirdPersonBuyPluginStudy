// © 2016 Mario Lelas
using UnityEngine;


namespace MLSpace
{
    /// <summary>
    /// common properties of player and npc characters
    /// </summary>
    public interface IGameCharacter
    {
        /// <summary>
        /// gets and sets taking hit flag
        /// </summary>
        bool takingHit { get; set; }

        /// <summary>
        /// gets in combat flag
        /// </summary>
        bool inCombat { get; }

        /// <summary>
        /// gets is character dead flag
        /// </summary>
        bool isDead { get; }

        /// <summary>
        /// gets reference to Stats class
        /// </summary>
        Stats stats { get; }

        /// <summary>
        /// gets reference to character transform
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// gets character position
        /// </summary>
        Vector3 position { get; }


        /// <summary>
        /// gets AudioManagerGAME reference
        /// </summary>
        AudioManager audioManager { get; }

        /// <summary>
        /// gets reference to ragdoll manager
        /// </summary>
        RagdollManager ragdoll { get; }

        /// <summary>
        /// notify character of attack end
        /// </summary>
        /// <param name="attacker">attacker game character</param>
        /// <param name="attackType">attack type</param>
        void attack_end_notify(IGameCharacter attacker, int attackType);

        /// <summary>
        /// notify character of attack start
        /// </summary>
        /// <param name="cp">attacker game character</param>
        /// <param name="attackType">attack type</param>
        void attack_start_notify(IGameCharacter attacker, int attackType);

        /// <summary>
        /// notify character of attack hit
        /// </summary>
        /// <param name="attacker">attacker game character</param>
        /// <param name="attackType">attack type</param>
        /// <param name="perHitAdditionalDamage">per hit additional damage</param>
        /// <param name="blocking">ref assigns character is blocking or not</param>
        /// <returns>success of hit</returns>
        bool attack_hit_notify(IGameCharacter attacker, int attackType, int attackSource, ref bool blocking, 
            bool applyHitReaction = true, Vector3? hitVelocity = null, int[] hitParts = null);

        /// <summary>
        /// set new item on character
        /// </summary>
        /// <param name="item">returns old item if exists</param>
        InventoryItem  setNewItem(InventoryItem item);

        /// <summary>
        /// set new secondary item
        /// </summary>
        /// <param name="item"></param>
        InventoryItem setSecondaryItem(InventoryItem item);

        void revive(int health);
    } 
}
