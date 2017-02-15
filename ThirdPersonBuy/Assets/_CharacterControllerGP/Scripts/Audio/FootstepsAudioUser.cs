// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// struct that hold ground type info
    /// </summary>
    [System.Serializable]
    public struct FootstepsStruct
    {
        /// <summary>
        /// ground type name
        /// </summary>
        public string type;

        /// <summary>
        /// ground type sound clip array
        /// will choose random from array and play on foot step
        /// </summary>
        public AudioClip[] clips;

        /// <summary>
        /// ground type converted to int hash
        /// </summary>
        [HideInInspector]
        public int typeHash;        // ground type name hash
    }

    /// <summary>
    /// overrider default ground footsteps sounds
    /// </summary>
    public class FootstepsAudioUser : MonoBehaviour
    {
        /// <summary>
        /// ground type info array to override
        /// </summary>
        [SerializeField]
        public FootstepsStruct[] footstepsClips;    

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            // get hashes of type names
            for(int i = 0;i< footstepsClips.Length;i++)
            {
                footstepsClips[i].typeHash = Animator.StringToHash(footstepsClips[i].type);
            }
        }
    } 
}
