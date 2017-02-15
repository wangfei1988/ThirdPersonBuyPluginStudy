// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// Modes for constraining position
    /// </summary>
    public enum IKMode { Default, ToLine, ToPlane };


    /// <summary>
    /// ledge and ladder helper ik class
    /// </summary>
    [RequireComponent(typeof(TPCharacter ))]
    public class IKHelper : MonoBehaviour
    {
        /// <summary>
        /// offset distance from wall to foot position
        /// </summary>
        [Tooltip("Offset distance from wall to foot position.")]
        public float footOffset = 0.0f;

        /// <summary>
        /// distance from hips (approx) to wall check
        /// </summary>
        [Tooltip ("Distance from hips (approx) to wall check.")]              
        public float hangCheckDistance = 0.4f;

        /// <summary>
        /// position at which player will be when on ledge supported by wall
        /// </summary>
        [Tooltip("Position at which player will be when on ledge supported by wall.")]
        public Vector3 ledgeRelativePosition =
            new Vector3(0.0f, -1.1f, -0.4f);

        /// <summary>
        /// position at which player will be when on ledge hanging
        /// </summary>
        [Tooltip ("Position at which player will be when on ledge hanging.")]
        public Vector3 ledgeRelativePositionHang = 
            new Vector3(0.0f, -2.15f, -0.1f);

        /// <summary>
        /// current ledge position ( supported by wall or hanging )
        /// </summary>
        [HideInInspector]
        public Vector3 currentRelLedgePosition = Vector3.zero;

        /// <summary>
        /// flag for adjusting player to ledge
        /// </summary>
        [HideInInspector]
        public bool adjustPosition2Ledge = false;

        /// <summary>
        /// ik position overrides for hands
        /// </summary>
        [HideInInspector]
        public Vector3? LH_OVERRIDE = null, RH_OVERRIDE = null;

        /// <summary>
        /// checking if wall is in front player when on ledge
        /// </summary>
        [HideInInspector]
        public bool m_CheckHang = false;

        /// <summary>
        /// current hand IK constraint mode
        /// </summary>
        [HideInInspector]
        public IKMode handIKMode = IKMode.Default;

        /// <summary>
        /// current feet IK constraint mode
        /// </summary>
        [HideInInspector]
        public IKMode feetIKMode = IKMode.Default;

        /// <summary>
        /// left hand line A point ( ik mode line for ledges and ladders)
        /// </summary>
        [HideInInspector]
        public Vector3 LeftHandPtA;

        /// <summary>
        /// left hand line B point ( ik mode line for ledges and ladders)
        /// </summary>
        [HideInInspector]
        public Vector3 LeftHandPtB;

        /// <summary>
        /// right hand line A point ( ik mode line for ledges and ladders)
        /// </summary>
        [HideInInspector]
        public Vector3 RightHandPtA;

        /// <summary>
        /// right hand line B point ( ik mode line for ledges and ladders)
        /// </summary>
        [HideInInspector]
        public Vector3 RightHandPtB;

        /// <summary>
        /// left foot plane A ( ik mode plane for ladders) 
        /// </summary>
        [HideInInspector]
        public Plane LeftFootPlane;

        /// <summary>
        /// right foot plane A ( ik mode plane for ladders) 
        /// </summary>
        [HideInInspector]
        public Plane RightFootPlane;

        /// <summary>
        /// event fired on left foot weight hit zero
        /// </summary>
        public VoidFunc OnReachZeroLF = null;

        /// <summary>
        /// event fired on right foot weight hit zero
        /// </summary>
        public VoidFunc OnReachZeroRF = null;

        /// <summary>
        /// event fired on left hand weight hit zero
        /// </summary>     
        public VoidFunc OnReachZeroLH = null;

        /// <summary>
        /// event fired on right hand weight hit zero
        /// </summary>
        public VoidFunc OnReachZeroRH = null;       


        private Animator m_Animator;                    // animator reference
        private TPCharacter m_Character;                //  tpcharacter reference

        private Vector3 m_AdjustStart, m_AdjustEnd;     // lerp positions
        private float m_AdjustMaxTime = 0.15f;          // lerp max time
        private float m_AdjustCurrentTime = 0.0f;       // lerp current time
        private float m_AdjustSpeed = 4.0f;             // lerp current speed

        private bool m_LeftIKHand = false;          // enable left hand ik
        private bool m_RightHandIK = false;         // enable right hand ik
        private bool m_LeftFootIK = false;          // enable left foot ik
        private bool m_RightFootIK = false;         // enable right foot ik

        private float m_LHWeight = 0.0f;            // left hand ik weight
        private float m_RHWeight = 0.0f;            // right hand ik weight
        private float m_LFWeight = 0.0f;            // left foot ik weight
        private float m_RFWeight = 0.0f;            // right foot ik weight

        private float m_LHWeightTime = 0.0f;        // left hand weight time for smooth transitions
        private float m_RHWeightTime = 0.0f;        // right hand weight time for smooth transitions
        private float m_LFWeightTime = 0.0f;        // left foot weight time for smooth transitions
        private float m_RFWeightTime = 0.0f;        // right foot weight time for smooth transitions
        private float m_IKLimit = 1.5f;             // ik time limit   

        private float m_ArmReach = 2.0f;            // character arm reach
        private float m_LegReach = 2.0f;            // character leg reach

        private float
            m_LFootvalue = 0.0f, 
            m_RFootvalue = 0.0f, 
            m_Feetspeed = 6.0f;                             // feet weight and speed value for lerping between wall and hanging positions on ledge           
        private Vector3 m_LFoothit, rfoothit;               // feet wall hit positions when on ledge 
        private Vector3 m_LedgeAdjustion = Vector3.zero;    // get closer to wall if legs too far but still wall support
        private bool m_DisableIKTrigger = false;            // disable all ik trigger
        private bool m_Initialized = false;                 // is class initialized


        /// <summary>
        /// gets arm reach of character
        /// </summary>
        public float armReach { get { return m_ArmReach; } }

        /// <summary>
        /// gets leg reach of character
        /// </summary>
        public float legReach { get { return m_LegReach; } }
        
        
        
        /// <summary>
        /// gets and sets left hand IK position
        /// </summary>
        public Vector3 LHPosition { get; set; }

        /// <summary>
        /// gets and sets right hand IK position
        /// </summary>
        public Vector3 RHPosition { get; set; }

        /// <summary>
        /// gets and sets left foot IK position
        /// </summary>
        public Vector3 LFPosition { get; set; }

        /// <summary>
        /// gets and sets right foot IK position
        /// </summary>
        public Vector3 RFPosition { get; set; }

        /// <summary>
        /// gets and sets IK max time 
        /// </summary>
        public float IKLimit { get { return m_IKLimit; } set { m_IKLimit = value; } }

        /// <summary>
        /// gets and sets left hand enable flag
        /// </summary>
        public bool LHandIKEnabled { get { return m_LeftIKHand; } set { m_LeftIKHand = value; } }

        /// <summary>
        /// gets and sets right hand enable flag
        /// </summary>
        public bool RHandIKEnabled { get { return m_RightHandIK; } set { m_RightHandIK = value; } }

        /// <summary>
        /// gets and sets left foot enable flag
        /// </summary>
        public bool LFootIKEnabled { get { return m_LeftFootIK; } set { m_LeftFootIK = value; } }

        /// <summary>
        /// gets and sets right foot enable flag
        /// </summary>
        public bool RFootIKEnabled { get { return m_RightFootIK; } set { m_RightFootIK = value; } }

        /// <summary>
        /// gets and sets left hand current weight time
        /// </summary>
        public float LHWeightTime { get { return m_LHWeightTime; } set { m_LHWeightTime = value; } }

        /// <summary>
        /// gets and sets right hand current weight time
        /// </summary>
        public float RHWeightTime { get { return m_RHWeightTime; } set { m_RHWeightTime = value; } }

        /// <summary>
        /// gets and sets left foot current weight time
        /// </summary>
        public float LFWeightTime { get { return m_LFWeightTime; } set { m_LFWeightTime = value; } }

        /// <summary>
        /// gets and sets right foot current weight time
        /// </summary>
        public float RFWeightTime { get { return m_RFWeightTime; } set { m_RFWeightTime = value; } }

        /// <summary>
        /// gets and sets checking for wall suppoert on ledge flag
        /// </summary>
        public bool checkHang { get { return m_CheckHang; } set { m_CheckHang = value; } }

        


        // initialize compoonent
        public void initialize()
        {
            if (m_Initialized) return ;

            m_Character = GetComponent<TPCharacter>();
            if (!m_Character) Debug.LogError("Cannot find 'TPCharacter' component." + " < " + this.ToString() + ">");
            m_Character.initialize();

            if(!m_Character.animator.avatar.isHuman  )
            {
                Debug.LogError("Not humanoid mecanim avatar!" + " < " + this.ToString() + ">");
                return;
            }
            if (!m_Character.animator.avatar.isValid )
            {
                Debug.LogError("Invalid mecanim avatar!" + " < " + this.ToString() + ">");
                return;
            }
            m_Animator = GetComponent<Animator>(); // m_Character.animator;
            if (!m_Animator) Debug.LogError("Cannot find 'Animator' component");


            Transform larm = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            if (!larm) { Debug.LogError("ANIMATOR ERROR ( IKHELPER )");return; }
            Transform lelbow = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform lhand = m_Animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform lleg = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform lknee = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform lfoot = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            float armelb = Vector3.Distance(larm.position, lelbow.position);
            float elbhand = Vector3.Distance(lelbow.position, lhand.position);
            m_ArmReach = armelb + elbhand;

            float hipknee = Vector3.Distance(lleg.position, lknee.position);
            float kneefoot = Vector3.Distance(lknee.position, lfoot.position);
            m_LegReach = hipknee + kneefoot;

            Transform rarmup = m_Character.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform relb = m_Character.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform rhand = m_Character.animator.GetBoneTransform(HumanBodyBones.RightHand);

            float diff1 = Vector3.Distance(rarmup.position, relb.position);
            float diff2 = Vector3.Distance(relb.position, rhand.position);
            m_ArmReach = diff1 + diff2;

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
            if(!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            _checkHangFunc();
        }

        /// <summary>
        /// unity IK function
        /// </summary>
        /// <param name="layerIndex">animator layer index</param>
        void OnAnimatorIK(int layerIndex)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            bool onLedge = m_Character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);

            if(!onLedge) { m_LFootvalue = 1.0f; m_RFootvalue = 1.0f; }

            if(handIKMode == IKMode.ToLine)
                _keepHandsToLine();

            if (feetIKMode == IKMode.ToPlane)
                _keepFeetToPlane();

            if (onLedge)
                _findFeetWallSupport();


            _doIK(layerIndex);
        }

#if DEBUG_INFO

        // hang check debug
        private Vector3 HANGCHECKPOS = Vector3.zero;
        private Color HANGCHECKCOL = Color.white;
        private float HANGCHECKDIST = 1.0f;

        // feet check debug
        private Vector3 LFPOS = Vector3.zero, RFPOS = Vector3.zero;
        private Vector3 LFDIR = Vector3.zero, RFDIR = Vector3.zero;
        private Color LFCOL = Color.white, RFCOL = Color.white;
        private float LFDIST = 1.0f, RFDIST = 1.0f;


        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(LFPosition, 0.1f);
            Gizmos.DrawWireSphere(RFPosition, 0.1f);
            Gizmos.DrawWireSphere(LHPosition, 0.1f);
            Gizmos.DrawWireSphere(RHPosition, 0.1f);

            Gizmos.color = HANGCHECKCOL;
            Gizmos.DrawLine(HANGCHECKPOS, HANGCHECKPOS + transform.forward * HANGCHECKDIST);

            Gizmos.color = LFCOL;
            Gizmos.DrawLine(LFPOS, LFPOS + LFDIR * LFDIST);

            Gizmos.color = RFCOL;
            Gizmos.DrawLine(RFPOS, RFPOS + RFDIR * RFDIST);
        }
#endif

        /// <summary>
        /// start adjusting transform position to current ledge position
        /// </summary>
        /// <param name="targetRotation">rotation of ledge</param>
        public void startLedgeAdjust(Quaternion targetRotation)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_AdjustCurrentTime = 0.0f;
            adjustPosition2Ledge = true;
            m_AdjustStart = transform.position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(
                m_Character.ledge.leftPoint, m_Character.ledge.rightPoint, ref m_AdjustStart);

            Ray ray = new Ray(m_AdjustStart, Vector3.up);
            float rayDist = 0.0f;
            if (m_Character .ledge.plane.Raycast(ray, out rayDist))
            {
                closest = m_AdjustStart + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(m_Character.ledge.leftPoint,
                    m_Character.ledge.rightPoint, ref closest);
            }


            Vector3 relPos = targetRotation * currentRelLedgePosition;
            m_AdjustEnd = closest + relPos;
        }

        /// <summary>
        /// disable all IKs
        /// </summary>
        public void disableAll()
        {
            m_DisableIKTrigger = true;
        }

        /// <summary>
        /// set IK lerp time direction ( increment towards one or decrement to zero )
        /// </summary>
        /// <param name="toOne"></param>
        public void setIKDirection(bool toOne)
        {
            if (toOne)
            {
                m_AdjustSpeed = 4.0f;
            }
            else
            {
                m_AdjustSpeed = -4.0f;
            }
        }


        /// <summary>
        /// method for checking does player is facing wall when on ledge
        /// </summary>
        private void _checkHangFunc()
        {
            if (!m_Character.ledgeMove) return;
            if (!m_CheckHang) return;


            bool hang = true;
            bool hanging = m_Character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);
            float radius = m_Character.capsule.radius * 0.38f;
            RaycastHit hit;
            int mask = m_Character.layers;
            Vector3 checkStart = transform.position;

            Vector3 closest = MathUtils.GetClosestPoint2Line(m_Character.ledge.leftPoint,
                m_Character.ledge.rightPoint, ref checkStart);

            Ray ray = new Ray(checkStart, Vector3.up);
            float rayDist = 0.0f;
            if (m_Character.ledge.plane.Raycast(ray, out rayDist))
            {
                closest = checkStart + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(m_Character.ledge.leftPoint,
                    m_Character.ledge.rightPoint, ref closest);
            }


            Vector3 offset = new Vector3(0.0f, 1.0f, 0.25f);
            offset = transform.rotation * offset;
            Vector3 pos = closest - offset;
            float dist = hangCheckDistance + 0.25f;
            ray = new Ray(pos, transform.forward);

#if DEBUG_INFO
            HANGCHECKCOL = Color.white;
            HANGCHECKDIST = dist;
            HANGCHECKPOS = ray.origin;
#endif


            float MAX_DIST_NOMOVE = 0.32f;
            m_LedgeAdjustion = Vector3.zero;

            if (Physics.SphereCast(ray, radius, out hit, dist, mask))
            {
#if DEBUG_INFO
                HANGCHECKCOL = Color.red;
#endif
                if (hit.distance > MAX_DIST_NOMOVE)
                {
                    float addedDist = MAX_DIST_NOMOVE - hit.distance;
                    m_LedgeAdjustion = -transform.forward * addedDist;
                }

                hang = false;
            }

            if (hang)
            {
                if (!hanging)
                {
                    m_Character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                    currentRelLedgePosition = ledgeRelativePositionHang;
                    startLedgeAdjust(transform.rotation);
                    m_Character.restoreCapsuleSize();
                }
            }
            else
            {
                if (hanging)
                {
                    m_Character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                    currentRelLedgePosition = ledgeRelativePosition;
                    startLedgeAdjust(transform.rotation);
                    m_Character.scaleCapsuleToHalf();
                }
            }
        }

        /// <summary>
        /// actual IK func
        /// </summary>
        /// <param name="layerIndex"></param>
        private void _doIK(int layerIndex)
        {
            if(m_DisableIKTrigger)
            {
                RHandIKEnabled = false;
                LFootIKEnabled = false;
                RFootIKEnabled = false;
                LHandIKEnabled = false;
                LFWeightTime = 0.0f;
                RFWeightTime = 0.0f;
                m_LHWeightTime = 0.0f;
                RHWeightTime = 0.0f;
                adjustPosition2Ledge = false;

                m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
                m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand , 0.0f);
                m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot , 0.0f);
                m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot , 0.0f);

                m_DisableIKTrigger = false;

                return;
            }

            _adjustPositionToLedge();
            if (layerIndex == 0)
            {
                if (m_LeftIKHand)
                {
                    if (LH_OVERRIDE.HasValue)
                    {
                        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
                        m_Animator.SetIKPosition(AvatarIKGoal.LeftHand, LH_OVERRIDE.Value);
                    }
                    else
                    {
                        m_LHWeightTime += Time.deltaTime * m_AdjustSpeed;
                        float lerpVal = Mathf.Clamp01(m_LHWeightTime / m_IKLimit);
                        m_LHWeight = Mathf.Lerp(0, 1, Mathf.Pow(lerpVal, 8));
                        m_LHWeight = Mathf.Clamp01(m_LHWeight);
                        m_LHWeightTime = Mathf.Clamp(m_LHWeightTime, 0.0f, m_IKLimit);
                        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_LHWeight);
                        m_Animator.SetIKPosition(AvatarIKGoal.LeftHand, LHPosition);

                        if (m_LHWeight <= 0.0f)
                        {
                            if (OnReachZeroLH != null)
                                OnReachZeroLH();
                        }
                    }
                }
                if (m_RightHandIK)
                {
                    if(RH_OVERRIDE.HasValue )
                    {
                        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
                        m_Animator.SetIKPosition(AvatarIKGoal.RightHand, RH_OVERRIDE.Value);
                    }
                    else
                    {
                        m_RHWeightTime += Time.deltaTime * m_AdjustSpeed;
                        float lerpVal = Mathf.Clamp01(m_RHWeightTime / m_IKLimit);
                        m_RHWeight = Mathf.Lerp(0, 1, Mathf.Pow(lerpVal, 8));
                        m_RHWeight = Mathf.Clamp01(m_RHWeight);
                        m_RHWeightTime = Mathf.Clamp(m_RHWeightTime, 0.0f, m_IKLimit);
                        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, m_RHWeight);
                        m_Animator.SetIKPosition(AvatarIKGoal.RightHand, RHPosition);

                        if (m_RHWeight <= 0.0f)
                        {
                            if (OnReachZeroRH != null)
                                OnReachZeroRH();
                        }
                    }
                }
                bool onLedge = m_Character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                if (onLedge)
                {
                    bool hang = m_Character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);
                    if (m_LeftFootIK)
                    {
                        if (hang)
                        {
                            m_LFWeightTime += Time.deltaTime * m_AdjustSpeed;
                            float lerpVal = Mathf.Clamp01(m_LFWeightTime / m_IKLimit);
                            m_LFWeight = Mathf.Lerp(0, 1, lerpVal);
                            m_LFWeight = Mathf.Clamp01(m_LFWeight);
                            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_LFWeight);
                            m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, LFPosition);
                        }
                        else
                        {
                            Transform lfoot = m_Character.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                            Vector3 lf_pos = lfoot.position;
                            Vector3 offset = new Vector3(-0.1f, -0.6f, -0.2f);
                            Vector3 loc_offset = transform.rotation * offset;
                            Vector3 lfootloose = lf_pos + loc_offset;

                            m_LFootvalue = Mathf.Clamp01(m_LFootvalue);
                            LFPosition = Vector3.Lerp(lfootloose, m_LFoothit, m_LFootvalue);

                            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1.0f);
                            m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, LFPosition);
                        }
                    }
                    if (m_RightFootIK)
                    {
                        if (hang)
                        {
                            m_RFWeightTime += Time.deltaTime * m_AdjustSpeed;
                            float lerpVal = Mathf.Clamp01(m_RFWeightTime / m_IKLimit);
                            m_RFWeight = Mathf.Lerp(0, 1, lerpVal);
                            m_RFWeight = Mathf.Clamp01(m_RFWeight);
                            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_RFWeight);
                            m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, RFPosition);
                        }
                        else
                        {
                            Transform rfoot = m_Character.animator.GetBoneTransform(HumanBodyBones.RightFoot);
                            Vector3 rf_pos = rfoot.position;
                            Vector3 offset = new Vector3(-0.1f, -0.6f, -0.2f);
                            Vector3 loc_offset = transform.rotation * offset;
                            Vector3 rfootloose = rf_pos + loc_offset;

                            m_RFootvalue = Mathf.Clamp01(m_RFootvalue);
                            RFPosition = Vector3.Lerp(rfootloose, rfoothit, m_RFootvalue);

                            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1.0f);
                            m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, RFPosition);
                        }
                    }
                }
                else
                {
                    if (m_LeftFootIK)
                    {
                        m_LFWeightTime += Time.deltaTime * m_AdjustSpeed;
                        float lerpVal = Mathf.Clamp01(m_LFWeightTime / m_IKLimit);
                        m_LFWeight = Mathf.Lerp(0, 1, Mathf.Pow(lerpVal, 8));
                        m_LFWeight = Mathf.Clamp01(m_LFWeight);
                        m_LFWeightTime = Mathf.Clamp(m_LFWeightTime, 0.0f, m_IKLimit);
                        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_LFWeight);
                        m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, LFPosition);

                        if (m_LFWeight <= 0.0f)
                        {
                            if (OnReachZeroLF != null)
                                OnReachZeroLF();
                        }
                    }
                    if (m_RightFootIK)
                    {
                        m_RFWeightTime += Time.deltaTime * m_AdjustSpeed;
                        float lerpVal = Mathf.Clamp01(m_RFWeightTime / m_IKLimit);
                        m_RFWeight = Mathf.Lerp(0, 1, Mathf.Pow(lerpVal, 8));
                        m_RFWeight = Mathf.Clamp01(m_RFWeight);
                        m_RFWeightTime = Mathf.Clamp(m_RFWeightTime, 0.0f, m_IKLimit);
                        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_RFWeight);
                        m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, RFPosition);
                        if (m_RFWeight <= 0.0f)
                        {
                            if (OnReachZeroRF != null)
                                OnReachZeroRF();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// lerping transform position to ledge position
        /// </summary>
        private void _adjustPositionToLedge()
        {
            if (adjustPosition2Ledge)
            {
                m_AdjustCurrentTime += Time.deltaTime;
                float t = m_AdjustCurrentTime / m_AdjustMaxTime;
                t = Mathf.Clamp01(t);
                transform.position = Vector3.Lerp(m_AdjustStart, m_AdjustEnd + m_LedgeAdjustion, t);
                if (m_AdjustCurrentTime >= m_AdjustMaxTime)
                {
                    adjustPosition2Ledge = false;
                }
            }
        }

        /// <summary>
        /// constraning hands IK to line
        /// </summary>
        private void _keepHandsToLine()
        {
            if (LHandIKEnabled)
            {
                Vector3 pos = m_Character.animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                Vector3 closestPos = MathUtils.GetClosestPoint2Line(LeftHandPtA, LeftHandPtB, ref pos);
                Vector3 shoulderPos = m_Character.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
                float dist = Vector3.Distance(shoulderPos, closestPos);
                if (dist > m_ArmReach)
                {
                    Vector3 dir = shoulderPos - closestPos;
                    dir.Normalize();
                    Vector3 newPos = dir * m_ArmReach * 0.95f;
                    LHPosition = shoulderPos - newPos;
                }
                else
                {
                    LHPosition = closestPos;
                }
            }

            if (RHandIKEnabled)
            {
                Vector3 pos = m_Character.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                Vector3 closestPos = MathUtils.GetClosestPoint2Line(RightHandPtA, RightHandPtB, ref pos);
                Vector3 shoulderPos = m_Character.animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
                float dist = Vector3.Distance(shoulderPos, closestPos);
                if (dist > m_ArmReach)
                {
                    Vector3 dir = shoulderPos - closestPos;
                    dir.Normalize();
                    Vector3 newPos = dir * m_ArmReach * 0.95f;
                    RHPosition = shoulderPos - newPos;
                }
                else
                {
                    RHPosition = closestPos;
                }
            }
        }

        /// <summary>
        /// checking for wall support in ledge mode
        /// </summary>
        private void _findFeetWallSupport()
        {
            bool hanging = m_Character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);

            Transform lfoot = m_Character.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Vector3 lf_pos = lfoot.position;
            Vector3 rf_pos = m_Character.animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            m_LFoothit = lf_pos;
            rfoothit = rf_pos;

            if (!hanging)
            {
                if (LFootIKEnabled)
                {
                    Vector3 pos = lf_pos - transform.forward * 0.25f;
                    Vector3 dir = transform.forward;
                    Ray ray = new Ray(pos, dir);
                    int mask = m_Character.layers;
                    RaycastHit hit;
                    float reach = m_LegReach + 0.25f;

#if DEBUG_INFO
                    LFPOS = ray.origin;
                    LFDIR = ray.direction;
                    LFDIST = reach;
                    LFCOL = Color.white;
#endif

                    if (Physics.SphereCast(ray, 0.1f, out hit, reach * 0.65f, mask))
                    {
                        lf_pos = hit.point - dir * footOffset;
                        m_LFoothit = lf_pos;
                        m_LFootvalue += Time.deltaTime * m_Feetspeed;
#if DEBUG_INFO
                        LFCOL = Color.red;
#endif
                    }
                    else
                    {
                        Vector3 offset = new Vector3(0.1f, -0.6f, -0.2f);
                        Vector3 loc_offset = transform.rotation * offset;
                        lf_pos = lf_pos + loc_offset;
                        m_LFootvalue -= Time.deltaTime * m_Feetspeed;
                    }
                }

                if (RFootIKEnabled)
                {
                    Vector3 pos = rf_pos - transform.forward * 0.25f;
                    Vector3 dir = transform.forward;
                    Ray ray = new Ray(pos, dir);
                    int mask = m_Character.layers;
                    float reach = m_LegReach + 0.25f;
                    RaycastHit hit;

#if DEBUG_INFO
                    RFPOS = ray.origin;
                    RFDIR = ray.direction;
                    RFDIST = reach;
                    RFCOL = Color.white;
#endif

                    if (Physics.SphereCast(ray, 0.1f, out hit, reach * 0.65f, mask))
                    {
                        rf_pos = hit.point - dir * footOffset;
                        rfoothit = rf_pos;
                        m_RFootvalue += Time.deltaTime * m_Feetspeed;
#if DEBUG_INFO
                        RFCOL = Color.red;
#endif
                    }
                    else
                    {
                        Vector3 offset = new Vector3(-0.1f, -0.6f, -0.2f);
                        Vector3 loc_offset = transform.rotation * offset;
                        rf_pos = rf_pos + loc_offset;
                        m_RFootvalue -= Time.deltaTime * m_Feetspeed;
                    }
                }
            }

            LFPosition = lf_pos;
            RFPosition = rf_pos;
        }

        /// <summary>
        /// constraining feet IK to plane
        /// </summary>
        private void _keepFeetToPlane()
        {

            if (LFootIKEnabled)
            {
                Vector3 pos = m_Character.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
                Vector3 closestPos = MathUtils.GetClosestPoint2Plane(ref LeftFootPlane, ref pos); // getClosestToPlane(LeftFootPlane, pos);
                LFPosition = closestPos;
            }

            if (RFootIKEnabled)
            {
                Vector3 pos = m_Character.animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
                Vector3 closestPos = MathUtils.GetClosestPoint2Plane(ref RightFootPlane, ref pos); // getClosestToPlane(RightFootPlane, pos);
                RFPosition = closestPos;
            }
        }

    } 
}
