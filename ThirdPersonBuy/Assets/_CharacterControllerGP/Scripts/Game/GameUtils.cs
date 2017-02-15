// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// game utilities class
    /// </summary>
    public static class GameUtils 
    {
        /// <summary>
        /// calculte damage received based on player and attacker stats
        /// </summary>
        /// <param name="defenderStats"></param>
        /// <param name="attackerStats"></param>
        /// <param name="blocking"></param>
        /// <param name="perHitAdditionalDamage"></param>
        public static int CalculateDamage(IGameCharacter defender, IGameCharacter attacker, bool blocking/*, int damage*/)
        {
            if (blocking) return 0;
            int damageAccum = attacker.stats.currentDamage   + attacker.stats.attack;
            int damageReduced = Mathf.Max((damageAccum - defender.stats.defence), 1);
            defender.stats.decreaseHealth(damageReduced);
            return damageReduced;
        }

        /// <summary>
        /// chosses random clip from passed array and play at position
        /// </summary>
        /// <param name="clips">audioclip array</param>
        /// <param name="position">world position</param>
        public static void PlayRandomClipAtPosition(AudioClip[] clips, Vector3 position)
        {
#if DEBUG_INFO
            if (clips == null)
            {
                Debug.LogError("object can be null");
                return;
            }
#endif
            if (clips.Length > 0)
            {
                int len = clips.Length;
                int rnd = Random.Range(0, len);
                AudioClip clip = clips[rnd];
#if DEBUG_INFO
                if(!clip)
                {
                    Debug.LogError("object cannot be null!");
                    return;
                }
#endif
                AudioSource.PlayClipAtPoint(clip, position);
            }
        }

        /// <summary>
        /// chosses random clip from passed array and play
        /// </summary>
        /// <param name="audioSource">audio source</param>
        /// <param name="clips">audioclip array</param>
        public static void PlayRandomClip(AudioSource audioSource, AudioClip[] clips)
        {
#if DEBUG_INFO
            if (clips == null)
            {
                Debug.LogError("object can be null");
                return;
            }
#endif
            if (clips.Length > 0)
            {
                int len = clips.Length;
                int rnd = Random.Range(0, len);
                audioSource.PlayOneShot(clips[rnd]);
            }
        }
    } 
}
