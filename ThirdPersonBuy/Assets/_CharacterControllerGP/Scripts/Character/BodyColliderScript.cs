// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    ///  Helper class derived from ColliderScript that hold additional information about body part
    /// </summary>
    public class BodyColliderScript : ColliderScript
    {
        /// <summary>
        /// you can apply additional damage if critial
        /// </summary>
        public bool critical = false;

        /// <summary>
        /// collider body part
        /// </summary>
        public BodyParts bodyPart = BodyParts.None;

        /// <summary>
        /// index of collider
        /// </summary>
        public int index = -1;   


        [SerializeField, HideInInspector]
        private RagdollManager m_ParentRagdollManager;  // reference to parents ragdollmanager script
        public IGameCharacter m_GCharacterOwner;        // reference to parent IGameCharacter interface



        /// <summary>
        /// gets and sets reference to parents ragdoll manager script
        /// </summary>
        public RagdollManager ParentRagdollManager
        {
            get { return m_ParentRagdollManager; }
            set { m_ParentRagdollManager = value; }
        }

        /// <summary>
        /// IGameCharacter interface on parent object
        /// </summary>
        public IGameCharacter ownerGameCharacter { get { return m_GCharacterOwner; } }
        
        /// <summary>
        /// unity Awake method
        /// is called when the script instance is being loaded
        /// </summary>
        void Awake()
        {

            if (!ParentObject) { Debug.LogError("BodyColliderScript has not assigned 'ParentObject'");return; }
            
            // get game character interface
            m_GCharacterOwner = ParentObject.GetComponent<IGameCharacter>();
        }
    } 
}
