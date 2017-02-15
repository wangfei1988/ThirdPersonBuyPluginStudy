// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// bow weapon item class
    /// </summary>
    public class BowItem : WeaponItem
    {
        /// <summary>
        /// item type bow
        /// </summary>
        public override InventoryItemType itemType { get { return InventoryItemType.Bow; } }

        /// <summary>
        /// bow string transform bone
        /// </summary>
        [Tooltip("bow string transform bone.")]
        public Transform bowString;

        private Vector3 m_origStringPos;        // original string bone position
        private Quaternion m_origStringRot;     // original string bone rotation

        /// <summary>
        /// unity Awake method
        /// is called when the script instance is being loaded
        /// </summary>
        void Awake()
        {
            if (!bowString)
            {
                Debug.LogWarning("cannot find bow string transform. < " + this.ToString() + " >");
            }
            else
            {
                m_origStringPos = bowString.localPosition;
                m_origStringRot = bowString.localRotation;
            }
            m_ItemType = InventoryItemType.Bow;
        }

        /// <summary>
        /// reset string position / rotation
        /// </summary>
        public void resetString()
        {
#if DEBUG_INFO
            if (!bowString)
            {
                Debug.LogWarning("cannot find bow string transform.");
                return;
            }
#endif
            bowString.localPosition = m_origStringPos;
            bowString.localRotation = m_origStringRot;
        }
    } 
}
