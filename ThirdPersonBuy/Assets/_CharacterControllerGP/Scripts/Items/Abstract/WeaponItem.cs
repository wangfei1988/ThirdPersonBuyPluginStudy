// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// weapon abstract base class of all weapons
    /// </summary>
    public abstract class WeaponItem : PhysicsInventoryItem 
    {
        /// <summary>
        /// attack damage of weapon
        /// </summary>
        [Tooltip("Attack damage of weapon.")]
        public int damage = 1;

        /// <summary>
        ///  range of weapon
        /// </summary>
        public float range = 1.5f;

        /// <summary>
        /// weapon sounds on attacks, blocks
        /// </summary>
        [HideInInspector]
        public WeaponAudio weaponAudio = null;

        /// <summary>
        /// weapon switch time from rest to wield transform
        /// </summary>
        public float weaponSwitchTime = 0.0f;

        /// <summary>
        /// weapon switching transforms helper field
        /// </summary>
        [HideInInspector]
        public float switchTimer = 0.0f;

        /// <summary>
        /// weapon switching transforms helper field
        /// </summary>
        [HideInInspector]
        public bool switchFlag = false;

        /// <summary>
        /// delegate fires on start taking weapon item
        /// </summary>
        public VoidFunc OnStartTaking;

        /// <summary>
        /// delegate fires on take weapon item
        /// </summary>
        public VoidFunc OnTake;

        /// <summary>
        /// delegate fires on start sheathing weapon item
        /// </summary>
        public VoidFunc OnStartSheathing;

        /// <summary>
        /// delegate fires on sheathe weapon item
        /// </summary>
        public VoidFunc OnSheathe;

        /// <summary>
        /// initialize component
        /// </summary>
        /// <returns>success</returns>
        public override bool initialize()
        {
            weaponAudio = GetComponent<WeaponAudio>();
            if (!weaponAudio) { Debug.LogWarning("Cannot find 'WeaponAudio' on " + this.gameObject.name); }


            return base.initialize();
        }
    }
}
