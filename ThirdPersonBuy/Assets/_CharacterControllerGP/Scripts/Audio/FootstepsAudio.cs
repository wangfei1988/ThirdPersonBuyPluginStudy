// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// holder of footseps sounds
    /// </summary>
    public class FootstepsAudio : MonoBehaviour
    {
        /// <summary>
        /// ground type name
        /// </summary>
        [Tooltip ("Name of ground type.")]
        public string type = "Default";     

        /// <summary>
        /// audio clip array
        /// will choose random from array and play on foot step
        /// </summary>
        [Tooltip ("Footsteps sound clip array.")]
        public AudioClip[] FootstepClips;

        /// <summary>
        /// ground type converted to int hash
        /// </summary>
        [HideInInspector]
        public int typeHash = -1;       // stored hash of type name

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            // collect hash of type name
            typeHash = Animator.StringToHash(type);
        }
    }
}
