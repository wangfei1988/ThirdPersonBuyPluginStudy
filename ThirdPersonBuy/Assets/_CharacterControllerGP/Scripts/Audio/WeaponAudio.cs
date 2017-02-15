using UnityEngine;
using System.Collections;

namespace MLSpace
{
    /// <summary>
    /// holder of  weapon sounds
    /// </summary>
    public class WeaponAudio : MonoBehaviour
    {
        /// <summary>
        /// AudioClip array. Clip chosen from will be played on attack
        /// </summary>
        [Tooltip("Sounds played on attack.")]
        public AudioClip[] attackSwingSounds;

        /// <summary>
        /// AudioClip array. Clip chosen from will be played on successfull hit
        /// </summary>
        [Tooltip("Sounds played on successfull attack hit.")]
        public AudioClip[] attackHitSounds;

        /// <summary>
        /// AudioClip array. Clip chosen from will be played when opponent blocks.
        /// </summary>
        [Tooltip("Sounds played when opponent blocks.")]
        public AudioClip[] blockSounds;
    } 
}
