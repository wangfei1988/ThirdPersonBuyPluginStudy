// © 2016 Mario Lelas
#define CALC_CUSTOM_VELOCITY_KINEMATIC
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{
    /// <summary>
    /// body parts enum
    /// </summary>
    public enum BodyParts : int
    {
        Spine = 0,
        Chest,
        Head,
        LeftShoulder,
        RightShoulder,
        LeftElbow,
        RightElbow,
        LeftHip,
        RightHip,
        LeftKnee,
        RightKnee,
        BODY_PART_COUNT,
        None,
    }


    /// <summary>
    /// ragdoll and hit reaction manager
    /// </summary>
    public class RagdollManager : MonoBehaviour
    {
        /// <summary>
        /// Accepting hits on on certain interval 间隔 enum
        /// 被hit击中时，Character不是播放动画而是使用RagDoll
        /// </summary>
        public enum HitIntervals
        {
            Always,         // always accept hits
            OnBlend,        // accept hits after ragdoll - on blend
            OnGettingUp,    // accept hits after blend - on getting up if enabled and animated
            OnAnimated,     // accept hits only when animating
            Timed           // accept hits on time intervals
        };

        /// <summary>
        /// ragdoll manager states
        /// </summary>
        public enum RagdollState : int
        {
            Ragdoll = 0,    // Ragdoll on, animator off 人物动作完全由Ragdoll控制
            Blend,          // Animator on, blending between last ragdoll and current animator transforms
            GettingUpAnim,  // Get up animation after ragdoll
            Animated,       // Animator full on。任务动作完全由Animator动画控制
        }

        /// <summary>
        ///  Class that holds useful information for each body part
        /// </summary>
        public class BodyPartInfo
        {
            /// <summary>
            /// current body part
            /// </summary>
            public BodyParts bodyPart = BodyParts.None;

            /// <summary>
            /// index of body part
            /// </summary>
            public int index = -1;

            /// <summary>
            /// transform of body part
            /// </summary>
            public Transform transform = null;

            /// <summary>
            /// original parent of body part
            /// </summary>
            public Transform orig_parent = null;

            /// <summary>
            /// collider of body part
            /// </summary>
            public Collider[] colliders = null;

            /// <summary>
            ///  rigidbody of body part
            /// </summary>
            public Rigidbody rigidBody = null;

            /// <summary>
            /// transition position used for blending
            /// </summary>
            public Vector3 transitionPosition = Vector3.zero;

            /// <summary>
            /// transition rotation used for blending
            /// </summary>
            public Quaternion transitionRotation = Quaternion.identity;

            /// <summary>
            ///  extra force used for adding to body part in ragdoll mode
            /// </summary>
            public Vector3 extraForce = Vector3.zero;

            /// <summary>
            /// constraint to add body parts like legs
            /// </summary>
            public ConfigurableJoint constraintJoint = null;

            /// <summary>
            ///  previus position to help calculate velocity
            /// </summary>
            public Vector3 previusPosition = Vector3.zero;

            /// <summary>
            ///  custom velocity calculated on kinematic bodies
            /// </summary>
            public Vector3 customVelocity = Vector3.zero;               
        }

#region Fields
        /// <summary>
        /// ragdoll transforms with colliders and rigid bodies 
        /// </summary>
        public Transform[] RagdollBones;
       
        /// <summary>
        /// create joints on constrained bodyparts / legs
        /// </summary>
        public bool useJoints = true;

        /// <summary>
        /// How long do we blend from ragdoll to animator
        /// </summary>
        [Tooltip("Blend time from ragdoll to animator.")]
        public float blendTime = 0.4f;

        [Tooltip("Set time interval."), HideInInspector]
        public float hitTimeInterval = 0.25f;

        [Tooltip("Controls how character reacts to hits.")]
        public float hitResistance = 2.0f;

        [Tooltip("Tolerance to hit velocity. If hit velocity is higher this, than character goes to full ragdoll")]
        public float hitReactionTolerance = 60.0f;

        [Tooltip("Influences hit reactions.")]
        public float weight = 80.0f;

        [Tooltip("Modifies custom kinematic bodies velocites.")]
        public float velocityModifier = 0.5f;

        /// <summary>
        /// hit interval
        /// </summary>
        [Tooltip("Accept hits intervals enum.")]
        public HitIntervals hitInterval = HitIntervals.Always;

        // ragdoll event time
        protected float m_RagdollEventTime = 6.0f;

        // current hit timer
        protected float m_CurrentHitTime = 0.0f;

        // transition  timer
        protected float m_CurrentBlendTime = 0.0f;

        // array of body parts
        protected BodyPartInfo[] m_BodyParts;

        // reference to animator
        protected Animator m_Animator;

        // is ragdoll physics on
        protected bool m_RagdollEnabled = true;

        // initial animator update mode
        protected AnimatorUpdateMode m_InitialMode =
            AnimatorUpdateMode.Normal;

        // does have root motion
        protected bool m_InitialRootMotion = false;

        protected bool m_FireHitReaction = false;                 // fire hit reaction flag
        protected Vector3? m_ForceVel = null;                     // hit force velocity
        protected bool m_FireAnimatorBlend = false;               // fire blend to animator flag
        protected bool m_FireRagdoll = false;                     // fire ragdoll flag
        protected Vector3? m_ForceVelocityOveral = null;          // force velocity on non hits parts
        protected bool m_FullRagdoll = false;                     // full ragdoll on flag
        protected bool m_GettingUpEnableInternal = true;          // internal setup of getting up animation
        protected bool m_GettingUp = false;                       // is getting up from ragdoll in progress
        protected bool m_HitReacWhileGettingUp = false;           // is hit made in getting up mode
        protected float m_CurrentEventTime = 0.0f;                // event current time
        protected RagdollState m_state = RagdollState.Animated;   // The current state
        protected bool m_Initialized = false;                     // is class initialized


        protected int m_ActiveLayer;        // layer when ragdolled
        protected int m_InactiveLayer;      // layer when not ragdolled

        protected List<int> m_ConstraintIndices;    // list of constrained indices ( legs usualy )

        // EVENTS
        public VoidFunc OnBlendEnd = null;       // on ragdoll end event
        public VoidFunc OnTimeEnd = null;        // on time event
        protected VoidFunc InternalOnHit = null;    // internal hit reaction event
        public VoidFunc LastEvent = null;        // last event on after ragdoll
        public VoidFunc OnGetUp = null;          // after get up animation event
        public VoidFunc OnHit = null;            // on hit event
        public VoidFunc OnStartTransition = null;// event on transition to animated start

#if SAVE_ANIMATOR_STATES
        protected List<AnimatorStateInfo> m_SavedCurrentStates =
            new List<AnimatorStateInfo>();                                          // saved animator states on disable
        protected Dictionary<AnimatorControllerParameter, object> m_SavedParams =
            new Dictionary<AnimatorControllerParameter, object>();                  // saved animator parameters on disable
#endif

        protected Transform m_RootTransform;

        // keep track of important animated positions and rotations
        protected Vector3 m_AnimatedRootPosition = Vector3.zero;
        protected Quaternion[] m_AnimatedRotations;


        // timer and flag of hit reaction system
        protected float m_HitReactionTimer = 0.0f;
        protected float m_HitReactionMax = 0.0f;
        protected bool m_HitReactionUnderway = false;

        protected bool m_AcceptHit = true;                      // does system accepts hit ( based on hit interval )
        protected bool m_IgnoreHitInterval = false;             // ignore hit interval flag

        protected int[] m_HitParts = null;                      // body parts hit array

        protected ForceMode m_ForceMode = ForceMode.VelocityChange; // mode of adding extra force to body parts


        /// <summary>
        /// use get up animation after ragdoll
        /// </summary>
        [Tooltip("Use get up animation after ragdoll ?"), HideInInspector]
        public bool enableGetUpAnimation = true;

        /// <summary>
        /// name of get up from back animation state clip 
        /// </summary>
        [Tooltip("Name of 'get up from back' animation state."), HideInInspector]
        public string nameOfGetUpFromBackState = "GetUpBack";

        /// <summary>
        /// name of get up from front animation state clip 
        /// </summary>
        [Tooltip("Name of 'get up from front' animation state."), HideInInspector]
        public string nameOfGetUpFromFrontState = "GetUpFront";

        // transform used for getting up orientation
        // forward axis must point towards character front
        protected Transform m_OrientTransform;


#endregion


#region Properties

        /// <summary>
        /// gets and sets ForceMode on body parts extra force
        /// </summary>
        public ForceMode extraForceMode { get { return m_ForceMode; } set { m_ForceMode = value; } }

        /// <summary>
        /// return true if component is initialized
        /// </summary>
        public bool initialized { get { return m_Initialized; } }

        /// <summary>
        /// returns true if ragdoll manager accepts hits based on hit interval
        /// </summary>
        public bool acceptHit { get { return m_AcceptHit; } }


        /// <summary>
        /// gets and sets ragdoll timed event time
        /// </summary>
        public float ragdollEventTime { get { return m_RagdollEventTime; } set { m_RagdollEventTime = value; } }

        /// <summary>
        /// gets current state of ragdoll manager
        /// </summary>
        public RagdollState state { get { return m_state; } }

        /// <summary>
        /// gets getting up flag
        /// </summary>
        public bool isGettingUp { get { return m_GettingUp; } }

        /// <summary>
        /// returns true if its full ragdoll on
        /// </summary>
        public bool isFullRagdoll { get { return m_FullRagdoll; } }


        /// <summary>
        /// gets spine bone transform
        /// </summary>
        public Transform rootTransform
        {
            get
            {
                return m_RootTransform;
            }
        }

        /// <summary>
        /// gets number of bodyparts
        /// </summary>
        public int bodypartCount
        {
            get { return (int)BodyParts.BODY_PART_COUNT; }
        }



        #endregion

        /// <summary>
        /// initialize component
        /// </summary>
        public virtual void initialize()
        {
            if (m_Initialized) return;

            m_ActiveLayer = LayerMask.NameToLayer("ColliderLayer");
            m_InactiveLayer = LayerMask.NameToLayer("ColliderInactiveLayer");
            m_CurrentHitTime = hitTimeInterval;

            m_Animator = GetComponent<Animator>();
            if (!m_Animator) { Debug.LogError("object cannot be null. " + " < " + this.ToString() + ">"); return; }

#if DEBUG_INFO
            if (!m_Animator.avatar.isHuman)
            {
                Debug.LogError("character avatar must be of humanoid type. " + " < " + this.ToString() + ">");
                return;
            }
            if (!m_Animator.avatar.isValid)
            {
                Debug.LogError("character avatar not valid. " + " < " + this.ToString() + ">");
                return;
            }
#endif

            // keep track of colliders and rigid bodies
            m_BodyParts = new BodyPartInfo[(int)BodyParts.BODY_PART_COUNT];
            m_AnimatedRotations = new Quaternion[(int)BodyParts.BODY_PART_COUNT];

            if(RagdollBones.Length == 0)
            {
                Debug.LogError ("No ragdoll bones assigned! " + " < " + this.ToString() + ">");
                return;
            }

#if DEBUG_INFO
            if (!RagdollBones[(int)BodyParts.Spine]) { Debug.LogError("cannot find transform - spineBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.Chest]) { Debug.LogError("cannot find transform - chestBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.Head]) { Debug.LogError("cannot find transform - headBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.LeftShoulder]) { Debug.LogError("cannot find transform - lShoulderBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.LeftElbow]) { Debug.LogError("cannot find transform - lElbowBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.RightShoulder]) { Debug.LogError("cannot find transform - rShoulderBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.RightElbow]) { Debug.LogError("cannot find transform - rElbowBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.LeftHip]) { Debug.LogError("cannot find transform - lHipBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.LeftKnee]) { Debug.LogError("cannot find transform - lKneeBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.RightHip]) { Debug.LogError("cannot find transform - rHipBone  " + " < " + this.ToString() + ">"); return; }
            if (!RagdollBones[(int)BodyParts.RightKnee]) { Debug.LogError("cannot find transform - rKneeBone  " + " < " + this.ToString() + ">"); return; }
#endif


            bool ragdollComplete = true;
            for (int i = 0; i < (int)BodyParts.BODY_PART_COUNT; ++i)
            {
                Rigidbody rb = RagdollBones[i].GetComponent<Rigidbody>();
                Collider[] cols = RagdollBones[i].GetComponents<Collider>();
                if (rb == null || cols == null || cols.Length == 0)
                {
                    ragdollComplete = false;
#if DEBUG_INFO
                    Debug.LogError("missing ragdoll part: " + ((BodyParts)i).ToString());
#endif
                }
                m_BodyParts[i] = new BodyPartInfo();
                m_BodyParts[i].transform = RagdollBones[i];
                m_BodyParts[i].rigidBody = rb;
                m_BodyParts[i].colliders = cols;
                m_BodyParts[i].bodyPart = (BodyParts)i;
                m_BodyParts[i].index = i;
                m_BodyParts[i].orig_parent = RagdollBones[i].parent;

            }
            if (!ragdollComplete) { Debug.LogError("ragdoll is incomplete or missing" + " < " + this.ToString() + ">"); return; }


            /* 
                NOTE:

                m_OrientTransform should be hips transform with forward vector 
                pointing forwards of character , 
                but I found out that not all models hips bone transform are oriented that way ( ?? ).
                So I am creating new object as m_OrientTransform to be  oriented as character, 
                but positioned on hip transform.

                If your hip bone is oriented so its looking in character forward directioon,
                you can assign hip transform as m_OrientTransform and not create new object.
                Or you can make m_OrientTransform field public and assign orient transform as you wish.

                I made it this way so it would be less setup for users.

            */

            GameObject orientTransformObj = new GameObject("OrientTransform");
            orientTransformObj.transform.position = RagdollBones[(int)BodyParts.Spine].position;
            orientTransformObj.transform.rotation = this.transform.rotation;
            orientTransformObj.transform.SetParent(RagdollBones[(int)BodyParts.Spine]);
            m_OrientTransform = orientTransformObj.transform;

            m_InitialMode = m_Animator.updateMode;
            m_InitialRootMotion = m_Animator.applyRootMotion;
            m_RootTransform = m_BodyParts[(int)BodyParts.Spine].transform;


            List<int> constraint_indices = new List<int>();
            constraint_indices.Add((int)BodyParts.LeftKnee);
            constraint_indices.Add((int)BodyParts.RightKnee);
            createConstraints(constraint_indices);

            m_Initialized = true;

            disableRagdoll();
        }

        /// <summary>
        /// set constrained body part 
        /// </summary>
        /// <param name="indices">indices of to be constrained body parts</param>
        public void createConstraints(List<int> indices)
        {
#if DEBUG_INFO
            if (m_BodyParts == null)
            {
                Debug.LogError("object cannot be null." + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_ConstraintIndices != null)
            {
                for (int i = 0; i < m_ConstraintIndices.Count; i++)
                {
                    Destroy(m_BodyParts[m_ConstraintIndices[i]].constraintJoint);
                }
                m_ConstraintIndices.Clear();
                m_ConstraintIndices = null;
            }
            m_ConstraintIndices = indices;
            if (useJoints)
            {
                for (int i = 0; i < indices.Count; i++)
                {
                    int index = indices[i];
                    ConfigurableJoint cfj =
                        m_BodyParts[index].transform.gameObject.AddComponent<ConfigurableJoint>();
                    cfj.connectedBody = null;
                    cfj.connectedAnchor = Vector3.zero;
                    cfj.anchor = Vector3.zero;
                    SoftJointLimit sjl = new SoftJointLimit();
                    sjl.limit = 0.00f;
                    cfj.linearLimit = sjl;
                    cfj.xMotion = ConfigurableJointMotion.Free;
                    cfj.yMotion = ConfigurableJointMotion.Free;
                    cfj.zMotion = ConfigurableJointMotion.Free;
                    cfj.angularXMotion = ConfigurableJointMotion.Free;
                    cfj.angularYMotion = ConfigurableJointMotion.Free;
                    cfj.angularZMotion = ConfigurableJointMotion.Free;
                    cfj.configuredInWorldSpace = true;
                    //cfj.enablePreprocessing = false;
                    m_BodyParts[index].constraintJoint = cfj;
                }
            }
        }

        /// <summary>
        /// setup colliders and rigid bodies for ragdoll start
        /// set colliders to be triggers and set rigidbodies to be kinematic
        /// </summary>
        protected virtual void enableRagdoll(bool gravity = true)
        {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
#endif

            if (m_RagdollEnabled)
            {
                return;
            }

            for (int i = 0; i < m_BodyParts.Length; ++i)
            {
#if DEBUG_INFO
                if (m_BodyParts[i] == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); continue; }
#else
                if (m_BodyParts[i] == null) continue;
#endif
                if (m_BodyParts[i].colliders != null || m_BodyParts [i].colliders .Length > 0)
                {
                    for (int j = 0; j < m_BodyParts[i].colliders.Length; j++)
                    {
                        m_BodyParts[i].colliders[j].isTrigger = false;
                    }
                }
#if DEBUG_INFO
                else Debug.LogWarning("body part collider is null." + " < " + this.ToString() + ">");
#endif


                if (m_BodyParts[i].rigidBody)
                {
                    m_BodyParts[i].rigidBody.useGravity = gravity;
                    m_BodyParts[i].rigidBody.isKinematic = false;
                }
#if DEBUG_INFO
                else Debug.LogWarning("body part rigid body is null.");
#endif

                // Unity 5.2.3 upwards
                // switch to layer that interacts with enviroment
                m_BodyParts[i].transform.gameObject.layer = m_ActiveLayer;
            }
            m_CurrentBlendTime = 0.0f;
            m_RagdollEnabled = true;

        }

        /// <summary>
        /// disable ragdoll. setup colliders and rigid bodies for normal use
        /// set colliders to not be triggers and set rigidbodies to not be kinematic
        /// </summary>
        protected virtual void disableRagdoll()
        {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
#endif

            if (!m_RagdollEnabled)
            {
#if DEBUG_INFO
                Debug.Log("ragdoll already finished." + " < " + this.ToString() + ">");
#endif
                return;
            }
            for (int i = 0; i < m_BodyParts.Length; ++i)
            {
#if DEBUG_INFO
                if (m_BodyParts[i] == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); continue; }
#else
                if (m_BodyParts[i] == null) continue;
#endif
                //if (m_BodyParts[i].collider != null)
                //{
                //    m_BodyParts[i].collider.isTrigger = true;
                //}
//#if DEBUG_INFO
//                else Debug.LogWarning("body part collider is null." + " < " + this.ToString() + ">");
//#endif

                if (m_BodyParts[i].rigidBody)
                {
                    m_BodyParts[i].rigidBody.useGravity = false;
                    m_BodyParts[i].rigidBody.isKinematic = true;
                }
#if DEBUG_INFO
                else Debug.LogWarning("body part rigid body is null.");
#endif

                // Unity 5.2.3 upwards
                // switch to layer that interacts with nothing
                m_BodyParts[i].transform.gameObject.layer = m_InactiveLayer;
            }
            m_RagdollEnabled = false;
        }




#if SAVE_ANIMATOR_STATES
        protected void saveAnimatorStates()
        {
            m_SavedCurrentStates.Clear();
            for (int i = 0; i < m_Animator.layerCount; i++)
            {
                AnimatorStateInfo curstate = m_Animator.GetCurrentAnimatorStateInfo(i);
                m_SavedCurrentStates.Add(curstate);
            }

            m_SavedParams.Clear();
            ;
            for (int i = 0; i < m_Animator.parameters.Length; i++)
            {
                AnimatorControllerParameter par = m_Animator.parameters[i];
                object val = null;
                switch (par.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        val = (object)m_Animator.GetBool(par.name);
                        break;
                    case AnimatorControllerParameterType.Float:
                        val = (object)m_Animator.GetFloat(par.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        val = (object)m_Animator.GetInteger(par.name);
                        break;
                }
                m_SavedParams.Add(par, val);
            }
        }

        // reset animator states and parameters
        protected void resetAnimatorStates()
        {
            foreach (KeyValuePair<AnimatorControllerParameter, object> pair in m_SavedParams)
            {
                AnimatorControllerParameter p = pair.Key;
                object v = pair.Value;
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        {
                            bool bval = (bool)v;
                            m_Animator.SetBool(p.name, bval);
                        }
                        break;
                    case AnimatorControllerParameterType.Float:
                        {
                            float fval = (float)v;
                            m_Animator.SetFloat(p.name, fval);
                        }
                        break;
                    case AnimatorControllerParameterType.Int:
                        {
                            int ival = (int)v;
                            m_Animator.SetInteger(p.name, ival);
                        }
                        break;
                }
            }
            for (int i = 0; i < m_SavedCurrentStates.Count; i++)
            {
                AnimatorStateInfo state = m_SavedCurrentStates[i];
                m_Animator.CrossFade(state.fullPathHash, 0.0f, i, state.normalizedTime);
            }
        }
#endif

#region UNITY_METHODS

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            // initialize all
            initialize();
        }

#if CALC_CUSTOM_VELOCITY_KINEMATIC


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
            if (Time.deltaTime > 0)
            {
                // calculating rigid bodies velocity when in kinematic state
                foreach (BodyPartInfo b in m_BodyParts)
                {
                    b.customVelocity = b.rigidBody.position - b.previusPosition;
                    b.customVelocity /= Time.deltaTime;
                    b.previusPosition = b.rigidBody.position;
                }
            }
        }
#endif

        /// <summary>
        /// Unity LateUpdate method
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            m_AcceptHit = true;
            switch (hitInterval)
            {
                case HitIntervals.OnBlend:
                    if (m_state < RagdollState.Blend)
                        m_AcceptHit = false;
                    break;
                case HitIntervals.OnGettingUp:
                    if (m_state < RagdollState.GettingUpAnim)
                        m_AcceptHit = false;
                    break;
                case HitIntervals.OnAnimated:
                    if (m_state < RagdollState.Animated)
                        m_AcceptHit = false;
                    break;
                case HitIntervals.Timed:
                    if (m_CurrentHitTime < hitTimeInterval)
                    {
                        m_AcceptHit = false;
                    }
                    break;
            }

            m_CurrentHitTime += Time.deltaTime;

            if (m_FireHitReaction)
            {
                _startHitReaction();
                m_FireHitReaction = false;
            }
            if (m_FireRagdoll)
            {
                _startRagdoll();
                m_FireRagdoll = false;
            }
            

            if (m_FireAnimatorBlend)
            {
                _startTransition();
                m_FireAnimatorBlend = false;
            }



            switch (m_state)
            {
                case RagdollState.Ragdoll:
                    _updateRagdoll();
                    break;
                case RagdollState.Blend:
                    _updateTransition();
                    break;
            }
        }

#if DEBUG_INFO
        void OnDrawGizmos()
        {
            if (m_BodyParts == null) return;
            if (m_BodyParts.Length == 0) return;

            if (!m_Animator)
                m_Animator = GetComponent<Animator>();

            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.Spine].transform.position,
                m_BodyParts[(int)BodyParts.Chest].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.Chest].transform.position,
                m_BodyParts[(int)BodyParts.Head].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.Chest].transform.position,
                m_BodyParts[(int)BodyParts.LeftShoulder].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.Chest].transform.position,
                m_BodyParts[(int)BodyParts.RightShoulder].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.LeftShoulder].transform.position,
                m_BodyParts[(int)BodyParts.LeftElbow].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.RightShoulder].transform.position,
                m_BodyParts[(int)BodyParts.RightElbow].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.Spine].transform.position,
                m_BodyParts[(int)BodyParts.LeftHip].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.Spine].transform.position,
                m_BodyParts[(int)BodyParts.RightHip].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.LeftHip].transform.position,
                m_BodyParts[(int)BodyParts.LeftKnee].transform.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.RightHip].transform.position,
                m_BodyParts[(int)BodyParts.RightKnee].transform.position);

            Transform lhand = m_BodyParts[(int)BodyParts.LeftElbow].transform.GetChild(0);
            Transform rhand = m_BodyParts[(int)BodyParts.RightElbow].transform.GetChild(0);
            Transform lfoot = m_BodyParts[(int)BodyParts.LeftKnee].transform.GetChild(0);
            Transform rfoot = m_BodyParts[(int)BodyParts.RightKnee].transform.GetChild(0);

            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.LeftElbow].transform.position, lhand.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.RightElbow].transform.position, rhand.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.LeftKnee].transform.position, lfoot.position);
            Gizmos.DrawLine(m_BodyParts[(int)BodyParts.RightKnee].transform.position, rfoot.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                m_Animator.GetBoneTransform(HumanBodyBones.Chest).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.Chest).position,
                m_Animator.GetBoneTransform(HumanBodyBones.Head).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.Chest).position,
                m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.Chest).position,
                m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position,
                m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position,
                m_Animator.GetBoneTransform(HumanBodyBones.LeftHand).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position,
                m_Animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position,
                m_Animator.GetBoneTransform(HumanBodyBones.RightHand).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position,
                m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position,
                m_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position,
                m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
            Gizmos.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position,
                m_Animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
        }
#endif

#endregion

        /// <summary>
        /// starts ragdoll flag by adding velocity to chosen body part index and overall velocity to all parts
        /// </summary>
        /// <param name="part">hit body part indices</param>
        /// <param name="velocityHit">force on hit body part</param>
        /// <param name="velocityOverall">overall force applied on rest of bodyparts</param>
        public void startRagdoll
            (
            int[] hit_parts = null,
            Vector3? hitForce = null,
            Vector3? overallHitForce = null,
            bool ignoreHitInverval = false
            )
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            m_CurrentEventTime = 0f;
            m_HitReacWhileGettingUp = false;
            m_HitParts = hit_parts;
            m_ForceVel = hitForce;
            m_ForceVelocityOveral = overallHitForce;

            m_IgnoreHitInterval = ignoreHitInverval;
            m_FireRagdoll = true;
        }

        /// <summary>
        /// set hit reaction flag and hit velocity
        /// </summary>
        /// <param name="hit_parts">hit parts indices</param>
        /// <param name="forceVelocity"></param>
        public void startHitReaction(
            int[] hit_parts,
            Vector3 forceVelocity,
            bool ignoreHitInterval = false)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            m_HitParts = hit_parts;
            m_ForceVel = forceVelocity;
            m_IgnoreHitInterval = ignoreHitInterval;
            m_FireHitReaction = true;
        }

        /// <summary>
        /// disable ragdoll and transition to mechanim animations
        /// and reset all extra body forces
        /// </summary>
        public void blendToMecanim()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            foreach (BodyPartInfo b in m_BodyParts)
            {
                b.extraForce = Vector3.zero;
            }
            m_FireAnimatorBlend = true;
        }

        /// <summary>
        /// get info on body part based on index
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public BodyPartInfo getBodyPartInfo(int part)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized.! " + " < " + this.ToString() + ">");
                return null;
            }
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return null; }
            if (part >= m_BodyParts.Length) { Debug.LogError("Index out of range." + " < " + this.ToString() + ">"); return null; }
#else
            if (m_BodyParts == null) { return null; }
            if (part >= m_BodyParts.Length) { return null; }
#endif

            return m_BodyParts[part];
        }



        /// <summary>
        /// start ragdoll method
        /// </summary>
        private void _startRagdoll()
        {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
            if (!m_Animator) Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
#endif


            if (!m_AcceptHit && !m_IgnoreHitInterval) return;

            enableRagdoll(true);

#if SAVE_ANIMATOR_STATES
            saveAnimatorStates();
#endif

            m_HitReacWhileGettingUp = false;
            m_Animator.enabled = false; //disable animation
            m_state = RagdollState.Ragdoll;
            m_FullRagdoll = true;
            m_CurrentHitTime = 0.0f;

            if (!useJoints)
            {
                for (int i = 1; i < bodypartCount; i++)
                {
                    BodyPartInfo b = m_BodyParts[i];
                    b.transform.SetParent(transform);
                }
            }

            if (m_ForceVelocityOveral.HasValue)
            {
                for (int i = 0; i < m_BodyParts.Length; i++)
                {
                    m_BodyParts[i].rigidBody.velocity = m_ForceVelocityOveral.Value;
                }
            }
#if CALC_CUSTOM_VELOCITY_KINEMATIC
            else
            {
                for (int i = 0; i < m_BodyParts.Length; i++)
                {
                    BodyPartInfo b = m_BodyParts[i];
                    b.rigidBody.velocity = b.customVelocity * velocityModifier ;
                }
            }
#endif

            if (m_HitParts != null)
            {
                if (m_ForceVel.HasValue)
                {
                    for (int i = 0; i < m_HitParts.Length; i++)
                    {
                        BodyPartInfo b = m_BodyParts[m_HitParts[i]];
                        b.rigidBody.velocity = m_ForceVel.Value;
                    }
                }
            }
            m_ForceVel = null;
            m_GettingUp = true; 
            m_IgnoreHitInterval = false;
            m_ForceVelocityOveral = null;
            m_HitParts = null;
            if (OnHit != null)
                OnHit();
        }

        /// <summary>
        /// start hit reaction method
        /// </summary>
        private void _startHitReaction()
        {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
            if (!m_Animator) Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
#endif
            if (m_HitParts == null)
            {
#if DEBUG_INFO
                Debug.LogWarning("Ragdoll::StartHitReaction must have body parts hit passed." + " < " + this.ToString() + ">");
#endif
                return;
            }

            if (!m_AcceptHit && !m_IgnoreHitInterval)
            {
                return;
            }

            if (m_state == RagdollState.Ragdoll)
            {
                if (m_ForceVelocityOveral.HasValue)
                {
                    for (int i = 0; i < m_BodyParts.Length; i++)
                    {
                        m_BodyParts[i].rigidBody.velocity = m_ForceVelocityOveral.Value;
                    }
                }
#if CALC_CUSTOM_VELOCITY_KINEMATIC
                else
                {
                    for (int i = 0; i < m_BodyParts.Length; i++)
                    {
                        BodyPartInfo b = m_BodyParts[i];
                        b.rigidBody.velocity = b.customVelocity * velocityModifier;
                    }
                }
#endif



                if (m_HitParts != null)
                {
                    if (m_ForceVel.HasValue)
                    {
                        for (int i = 0; i < m_HitParts.Length; i++)
                        {
                            BodyPartInfo b = m_BodyParts[m_HitParts[i]];
                            b.rigidBody.velocity = m_ForceVel.Value;
                        }
                    }
                }
                m_ForceVel = null;
                m_IgnoreHitInterval = false;
                m_ForceVelocityOveral = null;
                m_HitParts = null;
                if (OnHit != null)
                    OnHit();
                return;
            }

            Vector3 appliedForce = m_ForceVel.Value / Mathf.Max(1.0f, hitResistance);
            if (appliedForce.magnitude < 1.0f) return;

#if SAVE_ANIMATOR_STATES
            saveAnimatorStates();
#endif


            m_HitReacWhileGettingUp = false;
            if (m_GettingUp)
            {
                m_HitReacWhileGettingUp = true;
            }

            m_FullRagdoll = false;
            m_Animator.enabled = false;
            m_state = RagdollState.Ragdoll;
            m_HitReactionTimer = 0.0f;
            m_CurrentHitTime = 0.0f;

            bool swoop = false;
            if (m_ConstraintIndices.Count > 0)
            {
                swoop = true;
                for (int i = 0; i < m_ConstraintIndices.Count; i++)
                {
                    bool exists = System.Array.Exists(m_HitParts, elem => elem == m_ConstraintIndices[i]);
                    if (!exists)
                    {
                        swoop = false;
                        break;
                    }
                }
            }

            if (!swoop)
            {
                float force = m_ForceVel.HasValue ? m_ForceVel.Value.magnitude : 0.0f;

                m_FullRagdoll = force > hitReactionTolerance;

                if (m_FullRagdoll)
                {
                    m_CurrentEventTime = 0.0f;
                    m_ForceVelocityOveral = m_ForceVel.Value * 0.5f;
                    _startRagdoll();
                    return;
                }
                else
                {
                    if (!useJoints)
                    {
                        for (int i = 1; i < bodypartCount; i++)
                        {
                            BodyPartInfo b = m_BodyParts[i];
                            b.transform.SetParent(transform);
                        }
                    }

                    if (m_ForceVel.HasValue)
                        m_HitReactionMax = m_ForceVel.Value.magnitude / (weight * 10f );
                    m_HitReactionUnderway = true;
                    force = m_HitReactionMax;



                    enableRagdoll(false);
                    for (int i = 0; i < m_HitParts.Length; i++)
                    {
                        _applyConstraintOnBodyPart(/*force,*/ m_HitParts[i]);
                    }
                }

                
                if (m_ForceVel.HasValue)
                {

                        for (int i = 0; i < m_HitParts.Length; i++)
                        {
                            m_BodyParts[m_HitParts[i]].rigidBody.velocity = appliedForce;
                        }
                    
                }



                if (m_FullRagdoll)
                {
                    m_GettingUp = true;
                }

                m_IgnoreHitInterval = false;
                m_ForceVel = null;
                m_ForceVelocityOveral = null;
                m_HitParts = null;
                if (OnHit != null)
                    OnHit();
            }
            else
            {
                m_CurrentEventTime = 0.0f;
                _startRagdoll();
            }
        }

        /// <summary>
        /// apply hit reaction on single body part
        /// </summary>
        /// <param name="force">hit force</param>
        /// <param name="hitPart">hit pbody part</param>
        private void _applyConstraintOnBodyPart(/*float force,*/ int hitPart)
        {
            for (int i = 0; i < m_ConstraintIndices.Count; i++)
            {
                if (hitPart != m_ConstraintIndices[i])
                {
                    if (useJoints)
                    {
                        Transform T = m_BodyParts[m_ConstraintIndices[i]].transform;
                        Vector3 anchor = Vector3.zero;
                        if (T.childCount > 0)
                        {
                            Transform tChild = T.GetChild(0);
                            anchor = tChild.localPosition;
                        }
                        ConfigurableJoint cfj = m_BodyParts[m_ConstraintIndices[i]].constraintJoint;
                        cfj.xMotion = ConfigurableJointMotion.Locked;
                        cfj.yMotion = ConfigurableJointMotion.Locked;
                        cfj.zMotion = ConfigurableJointMotion.Locked;
                        cfj.anchor = anchor;
                    }
                    else
                    {
                        Rigidbody rb = m_BodyParts[m_ConstraintIndices[i]].rigidBody;
                        rb.isKinematic = true;
                    }
                }
            }
            //m_HitReacWhileGettingUp = true;
            InternalOnHit = () =>
            {
                m_GettingUpEnableInternal = false;
                m_FireAnimatorBlend = true;
                InternalOnHit = null;
            };
        }

        /// <summary>
        /// start blend to animator method
        /// </summary>
        private void _startTransition()
        {
            if (m_state != RagdollState.Ragdoll)
            {
                Debug.Log(this.name + " ragdoll state != ragdoll. m_state == " + m_state);
                return;
            }

#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null - " + " < " + this.ToString() + ">"); return; }
            if (m_BodyParts == null) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return; }
            if (!m_OrientTransform) { Debug.LogError("object cannot be null. - " + " < " + this.ToString() + ">"); return; }
#endif


            disableRagdoll();



            if (useJoints)
            {
                for (int i = 0; i < m_ConstraintIndices.Count; i++)
                {
                    m_BodyParts[m_ConstraintIndices[i]].constraintJoint.xMotion = ConfigurableJointMotion.Free;
                    m_BodyParts[m_ConstraintIndices[i]].constraintJoint.yMotion = ConfigurableJointMotion.Free;
                    m_BodyParts[m_ConstraintIndices[i]].constraintJoint.zMotion = ConfigurableJointMotion.Free;
                }
            }
            else
            {
                for (int i = 1; i < bodypartCount; i++)
                {
                    BodyPartInfo b = m_BodyParts[i];
                    b.transform.SetParent(b.orig_parent);
                }
            }

            foreach (BodyPartInfo b in m_BodyParts)
            {
                b.transitionRotation = b.transform.rotation;
                b.transitionPosition = b.transform.position;
            }



            m_CurrentBlendTime = 0.0f;
            m_Animator.enabled = true; //enable animation
            m_Animator.updateMode = AnimatorUpdateMode.Normal; // set animator update to normal
            m_Animator.applyRootMotion = true;
            m_state = RagdollState.Blend;

#if SAVE_ANIMATOR_STATES
            resetAnimatorStates();
#endif

            if (m_GettingUp && !m_HitReacWhileGettingUp)
            {
                m_Animator.applyRootMotion = false; // problems when getting up. must be false

                Vector3 newRootPosition = m_OrientTransform.position;

                // shoot ray to check ground and set new root position on ground
                // comment or delete this if you dont want this feature
                Vector3 raypos = m_OrientTransform.position + Vector3.up * 0.01f;
                Ray ray = new Ray(raypos, Vector3.down);

                // ignore colliders
                int layerMask = ~LayerMask.GetMask("ColliderLayer", "ColliderInactiveLayer");

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20f, layerMask, QueryTriggerInteraction.Ignore ))
                {
                    newRootPosition.y = hit.point.y /*+ 0.1f*/;
                    newRootPosition.x = hit.point.x;
                    newRootPosition.z = hit.point.z;
                }

                bool upwards = m_OrientTransform.forward.y > 0.0f;

                if (upwards)
                {

                    if (m_GettingUpEnableInternal && enableGetUpAnimation)
                    {
                        m_Animator.CrossFade(nameOfGetUpFromFrontState, 0.0f, 0, 0.0f);
                        Vector3 _up = -m_OrientTransform.up;
                        _up.y = 0.0f;
                        transform.forward = _up;
                    }
                }
                else
                {
                    if (m_GettingUpEnableInternal && enableGetUpAnimation)
                    {
                        m_Animator.CrossFade(nameOfGetUpFromBackState, 0.0f, 0, 0.0f);
                        Vector3 _up = m_OrientTransform.up;
                        _up.y = 0.0f;
                        transform.forward = _up;
                    }
                }
                transform.position = newRootPosition;
            }

            m_GettingUpEnableInternal = true;

            if (OnStartTransition != null)
                OnStartTransition();
        }

        /// <summary>
        /// update ragdolled mode
        /// </summary>
        private void _updateRagdoll()
        {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
#endif
            // if in ragdoll check timed event
            if (m_HitReactionUnderway)
            {
                m_HitReactionTimer += Time.deltaTime;
                if (m_HitReactionTimer >= m_HitReactionMax)
                {
                    m_HitReactionUnderway = false;
                    if (InternalOnHit != null)
                    {
                        InternalOnHit();
                        return;
                    }
                }
            }
            else
            {
                m_CurrentEventTime += Time.deltaTime;
                if (m_CurrentEventTime >= m_RagdollEventTime)
                {
                    m_CurrentEventTime = 0.0f;
                    if (OnTimeEnd != null)
                    {
                        OnTimeEnd();
                        return;
                    }
                }
            }
            for (int i = 0; i < m_BodyParts.Length; i++)
            {
                BodyPartInfo bpi = m_BodyParts[i];
                bpi.rigidBody.AddForce(bpi.extraForce, m_ForceMode);
            }

        }

        /// <summary>
        /// update in transition mode
        /// </summary>
        private void _updateTransition()
        {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
            if (!m_Animator) Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
#endif

            m_AnimatedRootPosition = m_BodyParts[(int)BodyParts.Spine].transform.position;
            for (int i = 0; i < (int)BodyParts.BODY_PART_COUNT; i++)
            {
                m_AnimatedRotations[i] = m_BodyParts[i].transform.rotation;
            }

            m_CurrentBlendTime += Time.deltaTime;
            float blendAmount = m_CurrentBlendTime / blendTime;
            blendAmount = Mathf.Clamp01(blendAmount);

            BodyPartInfo spine = m_BodyParts[(int)BodyParts.Spine];
            spine.transform.position = Vector3.Lerp(spine.transitionPosition, m_AnimatedRootPosition, blendAmount);
            for (int i = 0; i < m_BodyParts.Length; i++)
            {
                BodyPartInfo b = m_BodyParts[i];
                b.transform.rotation = Quaternion.Slerp(b.transitionRotation, m_AnimatedRotations[i], blendAmount);
            }

            if (m_CurrentBlendTime >= blendTime)
            {

                m_Animator.updateMode = m_InitialMode; // revert to original update mode
                m_Animator.applyRootMotion = m_InitialRootMotion;

                if (m_GettingUp && enableGetUpAnimation)
                {
                    m_state = RagdollState.GettingUpAnim;
                    //m_FireAnimatorBlend = true;
                }
                else
                {
                    if (LastEvent != null)
                        LastEvent();
                    m_state = RagdollState.Animated;
                    m_HitReacWhileGettingUp = false;
                }
                if (OnBlendEnd != null)
                {
                    OnBlendEnd(); // start on ragdoll end event if exists
                }
            }
        }


        /// <summary>
        /// animator event fired upon getting up finishes
        /// </summary>
        /// <param name="e">animation event info class</param>
        void OnGetUpEvent(AnimationEvent e)
        {
            m_GettingUp = false;
            m_FullRagdoll = false;
            m_Animator.updateMode = m_InitialMode; // revert to original update mode
            m_Animator.applyRootMotion = m_InitialRootMotion;
            m_state = RagdollState.Animated;
            m_HitReacWhileGettingUp = false;
            
            if (OnGetUp != null)
                OnGetUp();
            if (LastEvent != null)
                LastEvent();
        }

#if UNITY_EDITOR
        /// <summary>
        /// setup body collider scripts on body parts automaticly
        /// </summary>
        /// <returns></returns>
        public static bool AddBodyColliderScripts(RagdollManager ragMan)
        {
            if (!ragMan)
            {
                Debug.LogError("Object cannot be null.");
                return false;
            }

            Animator anim = ragMan.GetComponent<Animator>();

            if (!anim)
            {
                Debug.LogError("addBodyColliderScripts() FAILED. Cannot find 'Animator' component. " +
                    ragMan.name);
                return false;
            }

#if DEBUG_INFO
            if (ragMan.RagdollBones == null) { Debug.LogError("RagdollBones object cannot be null."); return false; }
#endif

            

            // removing existing ones
            for (int i = 0; i < ragMan.RagdollBones.Length; i++)
            {
                Transform t = ragMan.RagdollBones[i];
                if (!t) continue;
                BodyColliderScript[] t_bcs = t.GetComponents<BodyColliderScript>();
                foreach (BodyColliderScript b in t_bcs)
                    GameObject.DestroyImmediate(b);
            }

            Transform spine1T = ragMan.RagdollBones[(int)BodyParts.Spine];
            if (!spine1T) spine1T = anim.GetBoneTransform(HumanBodyBones.Hips);
            BodyColliderScript spineBCS = spine1T.GetComponent<BodyColliderScript>();
            spine1T.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!spineBCS)
            {
                spineBCS = Undo.AddComponent<BodyColliderScript>(spine1T.gameObject);
                if (spineBCS.Initialize())
                {
                    spineBCS.index = (int)BodyParts.Spine;
                    spineBCS.bodyPart = BodyParts.Spine;
                    spineBCS.critical = false;
                    spineBCS.ParentObject = ragMan.gameObject;
                    spineBCS.ParentRagdollManager = ragMan;
                    Debug.Log("added hips collider script for " + ragMan.name + " on " + spineBCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + spine1T.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("hips collider exists for " + ragMan.name + " on " + spineBCS.name);
#endif


            Transform chestT = ragMan.RagdollBones[(int)BodyParts.Chest];
            if (!chestT) chestT = anim.GetBoneTransform(HumanBodyBones.Chest);
            BodyColliderScript chestBCS = chestT.GetComponent<BodyColliderScript>();
            chestT.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!chestBCS)
            {
                chestBCS = Undo.AddComponent<BodyColliderScript>(chestT.gameObject);
                if (chestBCS.Initialize())
                {
                    chestBCS.index = (int)BodyParts.Chest;
                    chestBCS.bodyPart = BodyParts.Chest;
                    chestBCS.critical = false;
                    chestBCS.ParentObject = ragMan.gameObject;
                    chestBCS.ParentRagdollManager = ragMan;
                    Debug.Log("added chest collider script for " + ragMan.name + " on " + chestBCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + chestT.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("chest collider exists for " + ragMan.name + " on " + chestBCS.name);
#endif


            Transform headT = ragMan.RagdollBones[(int)BodyParts.Head];
            if (!headT) headT = anim.GetBoneTransform(HumanBodyBones.Head);
            BodyColliderScript headBCS = headT.GetComponent<BodyColliderScript>();
            headT.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!headBCS)
            {
                headBCS = Undo.AddComponent<BodyColliderScript>(headT.gameObject);
                if (headBCS.Initialize())
                {
                    headBCS.index = (int)BodyParts.Head;
                    headBCS.bodyPart = BodyParts.Head;
                    headBCS.critical = true;
                    headBCS.ParentObject = ragMan.gameObject;
                    headBCS.ParentRagdollManager = ragMan;
                    Debug.Log("added head collider script for " + ragMan.name + " on " + headBCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + headT.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("head collider exists for " + ragMan.name + " on " + headBCS.name);
#endif

            Transform xform = ragMan.RagdollBones[(int)BodyParts.LeftHip];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            BodyColliderScript BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.LeftHip;
                    BCS.bodyPart = BodyParts.LeftHip;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added left hip collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("left hip  collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.LeftKnee];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.LeftKnee;
                    BCS.bodyPart = BodyParts.LeftKnee;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added left knee collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("left knee collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.RightHip];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.RightHip;
                    BCS.bodyPart = BodyParts.RightHip;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added right hip collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("right hip collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.RightKnee];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.RightKnee;
                    BCS.bodyPart = BodyParts.RightKnee;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added right knee collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("right knee collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.LeftShoulder];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.LeftShoulder;
                    BCS.bodyPart = BodyParts.LeftShoulder;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added left shoulder collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("left shoulder collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.LeftElbow];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.LeftElbow;
                    BCS.bodyPart = BodyParts.LeftElbow;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added left elbow collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("left elbow collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.RightShoulder];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.RightShoulder;
                    BCS.bodyPart = BodyParts.RightShoulder;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added right shoulder collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("right shoulder collider exists for " + ragMan.name + " on " + BCS.name);
#endif


            xform = ragMan.RagdollBones[(int)BodyParts.RightElbow];
            if (!xform) xform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
            BCS = xform.GetComponent<BodyColliderScript>();
            xform.gameObject.layer = LayerMask.NameToLayer("ColliderLayer");
            if (!BCS)
            {
                BCS = Undo.AddComponent<BodyColliderScript>(xform.gameObject);
                if (BCS.Initialize())
                {
                    BCS.index = (int)BodyParts.RightElbow;
                    BCS.bodyPart = BodyParts.RightElbow;
                    BCS.critical = false;
                    BCS.ParentObject = ragMan.gameObject;
                    BCS.ParentRagdollManager = ragMan;
                    Debug.Log("added right elbow collider script for " + ragMan.name + " on " + BCS.name);
                }
                else
                {
                    Debug.LogError("initializing collider script on " + xform.name + " FAILED.");
                }
            }
#if DEBUG_INFO
            else Debug.LogWarning("right elbow collider exists for " + ragMan.name + " on " + BCS.name);
#endif

            PhysicMaterial fleshMaterial = Resources.Load<PhysicMaterial>("PhysicsMaterials/Flesh");
            if (!fleshMaterial) { Debug.LogWarning("Cannot find flesh material at 'Resources/PhysicsMaterials/Flesh"); }
            AssignPhysicsMaterial(ragMan, fleshMaterial );

            return true;
        }


        /// <summary>
        /// assigns physics  material to all colliders in ragdoll bones array
        /// </summary>
        /// <param name="ragMan">ragdoll manager</param>
        /// <returns>success</returns>
        public static bool AssignPhysicsMaterial(RagdollManager ragMan, PhysicMaterial p_material)
        {
            if (!ragMan)
            {
                Debug.LogError("Object cannot be null.");
                return false;
            }
            if (ragMan.RagdollBones == null)
            {
                Debug.LogError("Object cannot be null.");
                return false;
            }

            for (int i = 0; i < ragMan.RagdollBones.Length; i++)
            {
                Collider col = ragMan.RagdollBones[i].GetComponent<Collider>();
                if (!col)
                {
                    Debug.LogError("Collider missing on " + ragMan.RagdollBones[i].gameObject.name + ".");
                    return false;
                }
                col.material = p_material;
            }

            return true;
        }
#endif
    }
}
