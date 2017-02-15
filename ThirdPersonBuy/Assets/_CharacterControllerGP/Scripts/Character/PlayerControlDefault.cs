// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// default player control script
    /// passing input to 'PlayerThirdPerson' class
    /// you can exted it as you wish
    /// </summary>
    [RequireComponent(typeof(PlayerThirdPerson))]
    public class PlayerControlDefault : MonoBehaviour
    {
        private PlayerThirdPerson m_Player;     // player reference
        private bool m_Initialized = false;     // is component initialized

        protected TPCharacter.IKMode m_PrevIKmode = TPCharacter.IKMode.None;      // keep track of ik mode if need to reset
        protected TPCharacter.MovingMode m_PrevMoveMode =
            TPCharacter.MovingMode.RotateToDirection;                           // keep track of move mode if need to reset
        protected bool m_PrevStrafing = false;                                  // keep track of strafe mode if need to reset

        /// <summary>
        /// initialize component
        /// </summary>
        public void initialize()
        {
            if (m_Initialized) return;

            m_Player = GetComponent<PlayerThirdPerson>();
            if (!m_Player) { Debug.LogError("Cannot find component 'Player'" + " < " + this.ToString() + ">"); return; }
            m_Player.initialize();

            // setup ragdoll callbacks
            m_Player.ragdoll.OnHit = () =>
            {
                m_Player.character.simulateRootMotion = false;
                m_Player.character.disableMove = true;
                m_Player.character.rigidBody.velocity = Vector3.zero;

                m_Player.disableInput = true;
                m_Player.character.setIKMode(TPCharacter.IKMode.None);
                m_Player.character.rigidBody.isKinematic = true;
                m_Player.character.rigidBody.detectCollisions = false;
                m_Player.character.capsule.enabled = false;

                if (m_Player.ragdoll.isFullRagdoll)
                    m_Player.m_Camera.switchTargets(m_Player.ragdoll.RagdollBones[(int)BodyParts.Spine]);
            };
            m_Player.ragdoll.OnStartTransition = () =>
            {
                if (!m_Player.ragdoll.isFullRagdoll && !m_Player.ragdoll.isGettingUp)
                {
                    m_Player.character.simulateRootMotion = true;
                    m_Player.character.rigidBody.detectCollisions = true;
                    m_Player.character.rigidBody.isKinematic = false;
                    m_Player.character.capsule.enabled = true;
                }
                else
                {
                    m_Player.character.animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, 0.0f);
                    m_Player.character.animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, 0.0f);
                }
            };
            //m_Player.ragdoll.ragdollEventTime = 3.0f;
            //m_Player.ragdoll.OnTimeEnd = () =>
            //{
            //    m_Player.ragdoll.blendToMecanim();
            //};
            //m_Ragdoll.OnBlendEnd = () =>
            // {
            //     Debug.Log("ON BLEND END");
            // };
            //m_Ragdoll.OnGetUpEvent = () =>
            //  {
            //      Debug.Log("ON GET UP EVENT");
            //  };
            m_Player.ragdoll.LastEvent = () =>
            {
                m_Player.character.simulateRootMotion = true;
                m_Player.character.disableMove = false;
                m_Player.disableInput = false;
               if(m_Player.lookTowardsCamera) m_Player.character.setIKMode(TPCharacter.IKMode.Head);
                m_Player.character.rigidBody.isKinematic = false;
                m_Player.character.rigidBody.detectCollisions = true;
                m_Player.character.capsule.enabled = true;
                m_Player.m_Camera.switchTargets(m_Player.m_Camera.Target);
            };

            if(m_Player.lookTowardsCamera)
                m_Player.character.setIKMode(TPCharacter.IKMode.Head);

            if (m_Player.triggers)
            {
                // setup trigger callbacks
                m_Player.triggers.OnTriggerStart = () =>
                {

                    if (m_Player.legsIK) m_Player.legsIK.enabled = false;
                    m_Player.disableInput = true;
                    bool isOnLedge = m_Player.character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                    if (!isOnLedge)
                    {
                        m_PrevStrafing = m_Player.strafing;
                        m_Player.strafing = false;
                        if (m_Player.character.getIKMode() == TPCharacter.IKMode.Head ||
                        m_Player.character.getIKMode() == TPCharacter.IKMode.Waist)
                            m_Player.character.setIKMode(TPCharacter.IKMode.ToNone);
                    }
                };
                m_Player.triggers.OnTriggerEnd = () =>
                {
                    bool isOnLedge = m_Player.character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                    if (!isOnLedge)
                    {
                        m_Player.m_Camera.additiveRotation = false;
                        m_Player.strafing = m_PrevStrafing;
                        if (m_Player.legsIK) m_Player.legsIK.enabled = true;
                        if (m_Player.lookTowardsCamera) m_Player.character.setIKMode(TPCharacter.IKMode.Head);
                        if (m_Player.strafing) m_Player.character.setMoveMode(TPCharacter.MovingMode.Strafe);
                    }

                    m_Player.disableInput = false;
                };
            }
            m_Initialized = true;
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
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            bool strafeToggle = Input.GetKeyDown(KeyCode.H);
            if (strafeToggle)
            {
                m_Player.strafing = !m_Player.strafing;
            }


            // using crouch flag for releasing from trigger
            if (Input.GetButtonDown("Crouch"))
            {
                m_Player.triggers.breakTrigger();
            }
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool jump = Input.GetButtonDown("Jump");
            bool runToggle = Input.GetButton("WalkToggle");
            bool dive = Input.GetButtonDown("DiveRoll");
            bool crouch = Input.GetButton("Crouch");
            bool use = Input.GetButtonDown("Use");

            if (dive)
            {
                m_PrevIKmode = m_Player.character.getIKMode();
                m_PrevMoveMode = m_Player.character.moveMode;
                m_PrevStrafing = m_Player.strafing;
            }

            m_Player.triggers.update(h, v, use, false, jump);
            m_Player.control(h, v, jump, runToggle, dive, crouch);
        }

        /// <summary>
        /// animation event called upon dive roll end time
        /// </summary>
        /// <param name="e">AnimationEvent info class</param>
        private void OnDiveRollEndEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_Player.strafing = m_PrevStrafing;
            m_Player.disableInput = false;
            m_Player.character .setIKMode(m_PrevIKmode);
            if (!m_Player.character.isCrouching)
            {
                m_Player.m_Camera.switchTargets(m_Player.m_Camera.Target);
                m_Player.character.setMoveMode(m_PrevMoveMode);
            }
        }
    } 
}
