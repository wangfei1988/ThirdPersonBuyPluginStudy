
// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// quiver item class 
    /// </summary>
    public class QuiverItem : PhysicsInventoryItem 
    {
        [Tooltip("Arrow prefab that will be shot from bow.")]
        public GameObject arrowPrefab;      // arrow model prefab

        [Tooltip("Arrow length.")]
        public float arrowLength = 1.0f;

        [Tooltip("Arrow lifetime. Delete arrow upon lifetime end.")]
        public float arrowLifetime = 12.0f;

        [Tooltip("Arrow colliding layer/s")]
        public LayerMask layers;
        
        /// <summary>
        /// attack damage of arrows from quiver
        /// </summary>
        [Tooltip("Attack damage of arrows from quiver.")]
        public int damage = 1;

        /// <summary>
        /// gets item type
        /// </summary>
        public override InventoryItemType itemType { get { return InventoryItemType.QuiverArrow; } }

        /// <summary>
        /// initializes quiver component
        /// </summary>
        public override bool initialize()
        {
            if (m_Initialized) return true;



            base.initialize();
            m_ItemType = InventoryItemType.QuiverArrow;        // hardcoded
            return m_Initialized;
        }

    } 
}
