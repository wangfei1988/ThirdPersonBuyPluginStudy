// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// item collecting trigger 
    /// by walking over them
    /// </summary>
    public class ItemTrigger : Trigger
    {
        /// <summary>
        /// reference to item to collect
        /// </summary>
        [Tooltip("Item to collect.")]
        public InventoryItem item;

        private bool m_TriggerEnabled = true;       // is trigger enabled or disabled ?

        /// <summary>
        /// enable / disable trigger
        /// </summary>
        public bool enableTrigger
        {
            get { return m_TriggerEnabled; }
            set
            {
                if (value)
                {
                    this.gameObject.SetActive(true);
                }
                else
                {
                    this.gameObject.SetActive(false);
                }
                m_TriggerEnabled = value;
            }
        }


        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {        
            // no need to initialize
            // not using TriggerInfo class
            angleCondition = 180;
        }


        /// <summary>
        /// get display info text
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns></returns>
        public override string get_info_text(TPCharacter character)
        {
#if DEBUG_INFO
            if(!item)
            {
                Debug.LogError("Item missing < " + this.ToString() + " >");
                return "ERROR";
            }
#endif
            

            if (item is MeleeWeaponItem && (item as MeleeWeaponItem).itemType == InventoryItemType.Weapon1H)
            {
                return "Press 'E' to pick as primary weapon or 'F' as secondary weapon ( " + item.itemName + " )";
            }
            else
                return "Pick Up " + item.itemName;
        }


        /// <summary>
        /// start trigger interaction
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        /// <param name="use">use flag</param>
        /// <param name="jump">jump flag</param>
        /// <param name="v">vertical value</param>
        /// <param name="h">horizontal value</param>
        /// <returns>all succeded</returns>
        public override bool start(TPCharacter character, IKHelper limbsIK, bool use, bool secondaryUse, bool jump, float v, float h)
        {
            if (use)
            {
                IGameCharacter icharacter = character.GetComponent<IGameCharacter>();
                if (icharacter != null)
                {
                    icharacter.setNewItem(item);
                }
            }
            if(secondaryUse )
            {
                IGameCharacter icharacter = character.GetComponent<IGameCharacter>();
                if (icharacter != null)
                {
                    icharacter.setSecondaryItem (item);
                }
            }
            return false;
        }


        /// <summary>
        /// end trigger animations
        /// NOTE: Does nothing on ItemTrigger
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        public override void end(TPCharacter character, IKHelper limbsIK)
        {
        }
    } 
}
