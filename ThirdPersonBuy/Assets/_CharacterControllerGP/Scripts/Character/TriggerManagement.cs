// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// information on trigger 
    /// </summary>
    public class TriggerInfo
    {
        /// <summary>
        /// transform of trigger object
        /// </summary>
        public Transform transform;

        /// <summary>
        /// will trigger do matching target
        /// </summary>
        public bool doMatchTarget = false;

        /// <summary>
        ///  will character rotate to trigger rotation
        /// </summary>
        public bool doRotationToTarget = false;

        /// <summary>
        /// animator state name for matching target
        /// </summary>
        public string stateName = "";

        /// <summary>
        /// which hand catches - matching target
        /// </summary>
        public AvatarTarget avatarTarget = AvatarTarget.RightHand;

        /// <summary>
        /// lerp start rotation
        /// </summary>
        public Quaternion startRotation;

        /// <summary>
        /// lerp end rotation ( only y component )
        /// </summary>
        public Quaternion targetRotation;

        /// <summary>
        /// lerp time
        /// </summary>
        public float rot_time = 0.0f;

        /// <summary>
        /// lerp max time
        /// </summary>
        public float rot_maxTime = 0.5f;

        /// <summary>
        /// rotation slerp speed
        /// </summary>
        public float rot_speed = 1.0f;

        /// <summary>
        /// local position of catch position
        /// </summary>
        public Vector3 pos_offset = Vector3.zero;

        /// <summary>
        /// original catch position
        /// </summary>
        public Vector3 CatchPosition;

        /// <summary>
        /// gets catch position on trigger
        /// </summary>
        public Vector3 position
        {
            get
            {
                if (!transform) return Vector3.zero;
                return transform.position + pos_offset;
            }
        }

        /// <summary>
        /// custom bool operator
        /// </summary>
        /// <param name="exists">test trigger info</param>
        public static implicit operator bool (TriggerInfo exists)
        {
            return exists != null;
        }
    }


    /// <summary>
    /// interaction with scene triggers 
    /// </summary>
    [RequireComponent(typeof(TPCharacter))]
    public class TriggerManagement : MonoBehaviour
    {
        /// <summary>
        /// trigger info UI
        /// </summary>
        [Tooltip("Display trigger info UI.")]
        public UnityEngine.UI.Text m_TriggerUI;

        /// <summary>
        /// allowed time between triggers
        /// </summary>
        [Tooltip("Allowed time between triggers.")]
        public float m_TriggerInterval = 1.0f;

        private TPCharacter m_Character;            // reference to character script
        private Player m_Player;                    // reference to player script
        private IKHelper m_IKHelper = null;         // ik helper for ledges and ladders

        private bool m_initialized = false;         // is class initialized ?

        private Trigger m_CurrentActiveTrigger = null;    // current triggered trigger 
        private Trigger m_CollidedTrigger = null;           // current trigger in collision

        private MatchTargetWeightMask m_DefaultWeightMask =
            new MatchTargetWeightMask(new Vector3(1.0f, 1.0f, 1.0f), 0.0f); // matching target mask


        private float m_TriggerTime = 0.0f;     // current trigger time
        private bool m_TriggerAllowed = true;   // is trigger aloowed to fire ?
        private bool m_TriggerEntered = false;  // is trigger entered ?

        private float m_Vert = 0.0f, m_Horiz = 0.0f;    // inputs
        private bool m_Use = false, m_Jump = false;     // inputs
        private bool m_SecondaryUse = false;            // inputs

        private bool m_TriggerActive = false;           // is trigger active flag                                                                                                                            
        private bool m_LookToTarget = true;             // enable/disable looking to target
        private bool m_DisableUse = false;              // disable use input ( when picking item, so do not pick and fire trigger at one click )

        /// <summary>
        /// on trigger start callback
        /// </summary>
        public VoidFunc OnTriggerStart = null;

        /// <summary>
        ///  on trigger end callback
        /// </summary>
        public VoidFunc OnTriggerEnd = null;

        /// <summary>
        /// gets current trigger i9n progress
        /// </summary>
        public Trigger currentActiveTrigger { get { return m_CurrentActiveTrigger; } }

        /// <summary>
        /// gets trigger active flag
        /// </summary>
        public bool triggerActive { get { return m_TriggerActive; } }

        /// <summary>
        /// disable / enable use flag
        /// </summary>
        public bool disableUse { get { return m_DisableUse; }set { m_DisableUse = value; } }


        /// <summary>
        /// initilaize component
        /// </summary>
        public void initialize()
        {
            if (m_initialized) return;

            m_Character = GetComponent<TPCharacter>();
            if (!m_Character) { Debug.LogError("cannot find 'TPCharacter' component. " + " < " + this.ToString() + ">"); return; }
            m_Character.initialize();

            m_Player = GetComponent<Player>();
            if (!m_Player) { Debug.LogError("Cannot find 'Player' component. " + " < " + this.ToString() + ">"); return; }

            IKHelper ls = GetComponent<IKHelper>();
            if (ls)
            {
                if (ls.enabled)
                {
                    m_IKHelper = ls;
                }
            }

            if(!m_TriggerUI)
            {
                Debug.LogError("Trigger UI not assigned! " + " < " + this.ToString() + ">");
                return;
            }
            m_TriggerUI.text = "";

            m_initialized = true;
        }

        /// <summary>
        /// break from trigger matching target
        /// </summary>
        public void breakTrigger()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            if (!enabled) return;
            m_Character.animator.InterruptMatchTarget();
            if (m_CurrentActiveTrigger )
                m_CurrentActiveTrigger.end(m_Character, m_IKHelper);

            m_Character.animator.SetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool, false);
            m_Character.animator.SetBool(/*"pGrabLedgeUp"*/HashIDs.GrabLedgeUpBool, false);
            m_Character.animator.SetBool(/*"pGrabLedgeDown"*/HashIDs.GrabLedgeDownBool, false);
            m_Character.animator.SetBool(/*"pGrabLedgeLeft"*/HashIDs.GrabLedgeLeftBool, false);
            m_Character.animator.SetBool(/*"pGrabLedgeRight"*/HashIDs.GrabLedgeRightBool, false);
            m_Character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);

            m_Character.jumpAllowed = true;
            m_Character.enablePhysics();
            m_Character.rigidBody.isKinematic = false;
            m_Character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);
            m_Character.disableCapsuleScale = false;
            m_Character.restoreCapsuleSize();
            m_Character.disableMove = false;
            m_Character.disableGroundCheck = false;
            m_Character.ledgeMove = false;
            m_Character.animatorDampTime = 0.1f;
            m_Character.setLedge(null);
            m_IKHelper.handIKMode = IKMode.Default;
            m_IKHelper .feetIKMode  = IKMode.Default;

            m_TriggerAllowed = false;
            m_TriggerTime = 0.0f;

            disableIK();
            m_TriggerActive = false;

            if (OnTriggerEnd != null) OnTriggerEnd();

            m_CurrentActiveTrigger = null;
        }


        /// <summary>
        /// disable helper ik
        /// </summary>
        public void disableIK()
        {
            if (m_IKHelper)
            {
                m_IKHelper.disableAll();
            }
        }

        /// <summary>
        /// pass input
        /// </summary>
        /// <param name="h">horizontal axis</param>
        /// <param name="v">vertical axis</param>
        /// <param name="use">use flag</param>
        /// <param name="jump">jump flag</param>
        public void update(float h, float v, bool use, bool secondaryUse, bool jump, bool lookTowardsTarget = true)
        {
            m_Vert = v;
            m_Horiz = h;
            this.m_Use = use;
            this.m_SecondaryUse = secondaryUse;
            this.m_Jump = jump;
            this.m_LookToTarget = lookTowardsTarget;
            if (!use) m_DisableUse = false;
            
        }


#region UNITY_METHODS

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            if (!enabled) return;
            initialize();
        }

        /// <summary>
        /// Unity OnTriggerExit method
        /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
        /// </summary>
        /// <param name="col">collider exiting the trigger</param>
        void OnTriggerExit(Collider collider)
        {

#if DEBUG_INFO
            if (!enabled) return;
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_Player.switchHeadLookPos(null);

            if (m_TriggerEntered)
            {
                m_TriggerEntered = false;
            }
            Trigger trigger = collider.gameObject.GetComponent<Trigger>();
            if (!trigger)
            {
                return;
            }
            if (!trigger.enabled) return;

            if (m_CurrentActiveTrigger)
                if (m_CurrentActiveTrigger == trigger)
                    m_CurrentActiveTrigger.exit(m_Character);

        }

        

        /// <summary>
        /// Unity OnTriggerStay method
        /// OnTriggerStay is called once per frame for every Collider other that is touching the trigger
        /// </summary>
        /// <param name="col">collider staying in the trigger</param>
        void OnTriggerStay(Collider collider)
        {

#if DEBUG_INFO
            if (!enabled) return;
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (!m_TriggerAllowed)
            {
                return;
            }

            if (m_IKHelper )
            {
                if (m_IKHelper.adjustPosition2Ledge)
                    return;
            }

            if (m_TriggerActive)
            {
                return;
            }

            // ignore not triggers
            if (collider.gameObject.layer != LayerMask.NameToLayer("TriggerLayer"))
            {
                return;
            }

            if (_canInteract())
            {
                
                Trigger trigger = collider.gameObject.GetComponent<Trigger>();
                if (!trigger)
                {
                    return;
                }
                if (!trigger.enabled) return;

                m_TriggerEntered = true;
                m_Player.switchHeadLookPos(null);
                m_CollidedTrigger = trigger;
                m_CollidedTrigger.colliding = true;

                if(trigger.condition(m_Character))
                {
                    // player look towards target -------------
                    if(m_LookToTarget)m_Player.switchHeadLookPos(trigger.closestPoint(m_Player.transform.position));
                    //------------------------------------------
                    bool use = m_Use && !m_DisableUse;
                    bool start = trigger.start(m_Character, m_IKHelper, use, m_SecondaryUse , m_Jump, m_Vert, m_Horiz);
                    if (start)
                    {
                        m_TriggerAllowed = false;
                        m_TriggerTime = 0.0f;
                        m_TriggerActive = true;
                        m_CurrentActiveTrigger = trigger;
                        if (OnTriggerStart != null) OnTriggerStart();
                    }
                }
            }
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_TriggerUI.text = "";

            if (m_CollidedTrigger)
            {
                if(m_CollidedTrigger.showInfoText)
                    m_TriggerUI.text = m_CollidedTrigger.get_info_text(m_Character);
                if (!m_CollidedTrigger.colliding)
                    m_CollidedTrigger = null;
                else m_CollidedTrigger.colliding = false;
            }

            _slerp2TargetRotation();

            m_TriggerTime += Time.deltaTime;
            if (m_TriggerTime >= m_TriggerInterval)
                m_TriggerAllowed = true;

            if (m_CurrentActiveTrigger)
            {
                if (m_CurrentActiveTrigger.triggerData != null)
                    _updateMatchTarget();
            }
        }

#if DEBUG_INFO
        void OnDrawGizmosSelected()
        {
            // draw match target catch position
            if(m_CurrentActiveTrigger)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(m_CurrentActiveTrigger.triggerData.position, 0.15f);
            }
        }
#endif
#endregion

        /// <summary>
        /// can charater interact with trigger conditions
        /// </summary>
        /// <returns>returns true if conditions are met otherwise false</returns>
        private bool _canInteract()
        {
            bool can = !m_Character.disableMove;
            can &= !m_Character.isDiving;
            return can;
        }

        /// <summary>
        /// update match target func
        /// </summary>
        private void _updateMatchTarget()
        {
            if (!enabled)
            {
                return;
            }

            if (!m_CurrentActiveTrigger.triggerData.doMatchTarget)
            {
                return;
            }

            AnimatorStateInfo state = m_Character.animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(m_CurrentActiveTrigger.triggerData.stateName))
            {
                if (!m_Character.animator.IsInTransition(0))
                {
                    m_Character.animator.MatchTarget(
                        m_CurrentActiveTrigger.triggerData.position,
                        Quaternion.identity,
                        m_CurrentActiveTrigger.triggerData.avatarTarget,
                        m_DefaultWeightMask,
                        m_Character.animator.GetFloat(/*"MatchStart"*/HashIDs.MatchStartFloat ),
                        m_Character.animator.GetFloat(/*"MatchEnd"*/HashIDs.MatchEndFloat ));
                }
            }
        }

        /// <summary>
        /// rotating character  to target rotation
        /// </summary>
        private void _slerp2TargetRotation()
        {
            if (!m_CurrentActiveTrigger) return;
            if (m_CurrentActiveTrigger.triggerData != null)
            {
                if (m_CurrentActiveTrigger.triggerData.doRotationToTarget &&
                    m_CurrentActiveTrigger.triggerData.doMatchTarget )
                {
                    //Debug.Log("Slerping to rotation.");
                    m_CurrentActiveTrigger.triggerData.rot_time += Time.deltaTime * m_CurrentActiveTrigger.triggerData.rot_speed;
                    float rotValue = Mathf.Clamp01(m_CurrentActiveTrigger.triggerData.rot_time / m_CurrentActiveTrigger.triggerData.rot_maxTime);
                    Quaternion rotation = Quaternion.Slerp(m_CurrentActiveTrigger.triggerData.startRotation, m_CurrentActiveTrigger.triggerData.targetRotation, rotValue);
                    transform.rotation = rotation;
                    if (m_CurrentActiveTrigger.triggerData.rot_time > m_CurrentActiveTrigger.triggerData.rot_maxTime)
                    {
                        m_CurrentActiveTrigger.triggerData.doRotationToTarget = false;
                    }
                }
            }
        }


        /// <summary>
        /// event that fires upon reaching match target start time
        /// </summary>
        /// <param name="e">AnimationEvent info class</param>
        void OnMatchTargetStartEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (!enabled) return;

            float matchStart = m_Character.animator.GetFloat(/*"MatchStart"*/ HashIDs.MatchStartFloat);
            float matchEnd = m_Character.animator.GetFloat(/*"MatchEnd"*/ HashIDs.MatchEndFloat);
            float length = m_Character.animator.GetCurrentAnimatorStateInfo(0).length;
            float start = matchStart * length;
            float end = matchEnd * length;
            float difference = Mathf.Max(0.1f, end - start);
            float s = Mathf.Abs(m_Character.animator.GetCurrentAnimatorStateInfo(0).speed);
            difference *= s;
            m_CurrentActiveTrigger.triggerData.rot_speed = s;
            m_CurrentActiveTrigger.setRotation2TargetLimit(difference);
            m_CurrentActiveTrigger.onMatchTargetStart(m_Character ,m_IKHelper );
        }

        /// <summary>
        /// event that fires upon reaching match target end time
        /// </summary>
        /// <param name="e">AnimationEvent info class</param>
        void OnMatchTargetEndEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (!enabled) return;
            m_CurrentActiveTrigger.onMatchTargetEnd(m_Character, m_IKHelper);
        }

        /// <summary>
        /// event that fires upon reaching trigger animation end time
        /// </summary>
        /// <param name="e">AnimationEvent info class</param>
        void TriggerEndEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (!enabled) return;
            if (!m_CurrentActiveTrigger) return;

            m_Character.animator.InterruptMatchTarget();
            m_CurrentActiveTrigger.end(m_Character, m_IKHelper);
            m_TriggerActive = false;
            if (OnTriggerEnd != null) OnTriggerEnd();
            m_CurrentActiveTrigger = null;
        }
    }
}
