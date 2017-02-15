// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// class that plays various character sounds
    /// feel free to add / remove / modify sounds
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// AudioClip array. Clip chosen from will be played  on jump.
        /// </summary>
        [Tooltip("Sounds played on jump.")]
        public AudioClip[] jumpSounds;

        /// <summary>
        /// AudioClip array. Clip chosen from will be played on dive roll.
        /// </summary>
        [Tooltip ("Sounds played on dive roll.")]
        public AudioClip[] diveRollSounds;

        /// <summary>
        /// enable / disable check ground under each foot on every step
        /// </summary>
        [Tooltip ("Check ground under each foot on every step.")]
        public bool checkGroundForEachStep = false;

        /// <summary>
        /// holds overrided sounds
        /// </summary>
        [HideInInspector]
        public FootstepsAudioUser footstepsUser;    // character overrider sounds

        /// <summary>
        /// holds sounds played on ladder climb
        /// </summary>
        [HideInInspector]
        public FootstepsAudio ladderClips;          // ladder clips

        protected AudioSource m_Audio;              // reference to AudioSource
        protected TPCharacter m_Character;          // refrence to TPCharacter script
        protected bool m_Initialized = false;       // is component initialized ?

        /// <summary>
        /// gets reference to AudioSource component
        /// </summary>
        public AudioSource audioSource { get { return m_Audio; } }

        /// <summary>
        /// initialize component
        /// </summary>
        public virtual void initialize()
        {
            if (m_Initialized) return;

            m_Character = GetComponent<TPCharacter>();
            if(!m_Character) { Debug.LogError("Cannot find component 'TPCharacter'" + " < " + this.ToString() + ">"); }
            m_Character.initialize();

            m_Audio = GetComponent<AudioSource>();
            if(!m_Audio) { Debug.LogError("Cannot find component 'AudioSource'" + " < " + this.ToString() + ">"); }
            footstepsUser = GetComponent<FootstepsAudioUser>();

            m_Initialized = true;
        }

        /// <summary>
        /// Unity Start method
        /// is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }

        /// <summary>
        /// play jump sound
        /// </summary>
        public void playJumpSound()
        {
#if DEBUG_INFO
            if (!m_Initialized )
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif


            if (jumpSounds.Length > 0)
            {
                int len = jumpSounds.Length;
                int rnd = Random.Range(0, len);
                m_Audio.PlayOneShot(jumpSounds[rnd]);
            }
        }

        /// <summary>
        /// play dive roll sound
        /// </summary>
        public void playDiveRollSound()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (diveRollSounds.Length > 0)
            {
                int len = diveRollSounds.Length;
                int rnd = Random.Range(0, len);
                m_Audio.PlayOneShot(diveRollSounds[rnd]);
            }
        }


        /// <summary>
        /// play footstep sound 0 = left, 1 = right
        /// </summary>
        /// <param name="foot">left or right foot</param>
        public void playFootstepSound(int foot)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (ladderClips != null)
            {
                int len = ladderClips.FootstepClips.Length;
                int rnd = Random.Range(0, len);
                m_Audio.PlayOneShot(ladderClips.FootstepClips[rnd]);
            }
            else
            {
                if (!m_Character.isGroundMode) return;

                if (!checkGroundForEachStep)
                {
                    if (!m_Character.currentGroundCollider) return;
                    _playFootstepClip(m_Character.currentGroundCollider);
                }
                else
                {
                    Vector3 pos = Vector3.zero;
                    Collider col = null;
                    if (foot == 0)
                    {
                        Transform lfoot = m_Character.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                        pos = lfoot.position;
                        col = _getGroundCollider(ref pos);

                    }
                    else if (foot == 1)
                    {
                        Transform rfoot = m_Character.animator.GetBoneTransform(HumanBodyBones.RightFoot);
                        pos = rfoot.position;
                        col = _getGroundCollider(ref pos);
                    }
                    else
                    {
                        if (!m_Character.currentGroundCollider) return;
                        col = m_Character.currentGroundCollider;
                    }
                    if (col)
                    {
                        _playFootstepClip(col);
                    }

                }
            }
        }

        /// <summary>
        /// play footstep sound
        /// </summary>
        /// <param name="collider">collider under character</param>
        private void _playFootstepClip(Collider collider)
        {
            FootstepsAudio fa = collider.GetComponent<FootstepsAudio>();
            if (!fa) return;
            if (footstepsUser)

            {
                for (int i = 0; i < footstepsUser.footstepsClips.Length; i++)
                {
                    AudioClip[] clips = footstepsUser.footstepsClips[i].clips;
                    int hash = footstepsUser.footstepsClips[i].typeHash;
                    if(fa.typeHash == hash)
                    {
                        int len = clips.Length;
                        int rnd = Random.Range(0, len);
                        m_Audio.PlayOneShot(clips[rnd]);
                        return;
                    }
                }
            }
            {
                int len = fa.FootstepClips.Length;
                int rnd = Random.Range(0, len);
                m_Audio.PlayOneShot(fa.FootstepClips[rnd]);
            }
        }

        /// <summary>
        /// get collider under position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Collider _getGroundCollider(ref Vector3 pos)
        {
            Ray ray = new Ray(pos, Vector3.down);
            int mask = m_Character.layers;
            RaycastHit hit;
            if(Physics.Raycast ( ray, out hit, m_Character.groundCheckDistance, mask))
            {
                return hit.collider;
            }
            return null;
        }

        // animation events

        /// <summary>
        /// event fired on left foot down
        /// </summary>
        /// <param name="e"></param>
        void LeftFootDownEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (e.animatorClipInfo.weight > 0.25f)
                playFootstepSound(0);
        }

        /// <summary>
        /// event fired on right foot down
        /// </summary>
        /// <param name="e"></param>
        void RightFootDownEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (e.animatorClipInfo.weight > 0.25f)
                playFootstepSound(1);
        }

        /// <summary>
        /// stop audio source current clip
        /// </summary>
        public void stopAudio()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_Audio.Stop();
        }
    } 
}
