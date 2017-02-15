// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// current inventory item types
    /// </summary>
    public enum InventoryItemType
    {
        Weapon1H,
        Weapon2H,
        Shield,
        QuiverArrow,
        Bow,
    };



    /// <summary>
    /// base abstract class for all inventory items
    /// </summary>
    public abstract class InventoryItem : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected InventoryItemType m_ItemType;             // inventory item type

        /// <summary>
        /// item name
        /// </summary>
        [Tooltip("Item name.")]
        public string itemName;

        /// <summary>
        /// item description
        /// </summary>
        [Multiline, Tooltip("Item description.")]
        public string itemDescription;

        /// <summary>
        /// is item equipped ?
        /// </summary>
        [HideInInspector]
        public bool equipped = false;                       // is item equipped ?

        protected GameObject m_ItemGO;                      // reference to item gameobject
        protected GameObject m_OwnerGO;                     // reference to owner of item
        protected bool m_Initialized = false;               // is class initialized ?

        /// <summary>
        /// gets item game object
        /// </summary>
        public GameObject item { get { return m_ItemGO; } set { m_ItemGO = value; } }

        /// <summary>
        /// gets and sets owner if item
        /// </summary>
        public GameObject owner
        {
            get { return m_OwnerGO; }
            set { m_OwnerGO = value; }
        }


        /// <summary>
        /// gets item type
        /// </summary>
        public virtual InventoryItemType itemType { get { return m_ItemType; } }

        /// <summary>
        /// gets is class initialized
        /// </summary>
        public bool initialized { get { return m_Initialized; } }


        /// <summary>
        /// initialize class 
        /// </summary>
        public virtual bool initialize()
        {
            if (m_Initialized) return true;


            m_ItemGO = this.gameObject;

            m_Initialized = true;
            return true;
        }

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }


        /// <summary>
        /// reset item 
        /// </summary>
        public abstract void resetItem();

        /// <summary>
        /// reset item to position / rotation
        /// </summary>
        public abstract void dropItem(Vector3? pos, Quaternion? rot);

        /// <summary>
        /// setup item states for equipping
        /// </summary>
        public abstract void equipSetup();
    }
}
