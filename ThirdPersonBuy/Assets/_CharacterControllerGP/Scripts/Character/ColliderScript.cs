// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// Helper class that hold information about body part 
    /// </summary>
    public class ColliderScript : MonoBehaviour
    {
        /// <summary>
        /// parent game object of collider 
        /// </summary>
        public GameObject ParentObject; 
            
        private Collider m_Collider;        // reference to collider component

        /// <summary>
        /// Initialize 
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            if (!m_Collider)
            {
                m_Collider = GetComponent<Collider>();
                if (!m_Collider) { Debug.LogWarning("Collider scipt cannot find 'Collider' component. " + this.name); return false; }
            }
            return true;
        }

        /// <summary>
        /// unity Awake method
        /// is called when the script instance is being loaded
        /// </summary>
        void Start()
        {
            Initialize();   
        }
    } 
}
