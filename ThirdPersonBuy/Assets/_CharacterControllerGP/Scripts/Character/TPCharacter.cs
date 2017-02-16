// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// ledge helper struct
    /// currently static
    /// </summary>
    public struct Ledge
    {
        /// <summary>
        /// transform component of ledge object
        /// </summary>
        public Transform transform;

        /// <summary>
        /// start position of ledge 起点
        /// </summary>
        public Vector3 leftPoint;

        /// <summary>
        /// end position of ledge 终点
        /// </summary>
        public Vector3 rightPoint;

        /// <summary>
        /// ledge plane
        /// </summary>
        public Plane plane;

        /// <summary>
        /// reverse on switch direction
        /// </summary>
        public bool reversed; 
    }

    /// <summary>
    /// main class for manipulating third person characters
    /// 最为核心的类
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class TPCharacter : MonoBehaviour
    {
        /// <summary>
        /// ik modes
        /// </summary>
        public enum IKMode { None, Head, Waist, ToNone };

        /// <summary>
        /// move modes
        /// </summary>
        public enum MovingMode { RotateToDirection, Strafe, Ledge }; //Strafe惩罚



        #region Fields

        /// <summary>
        /// layers which collide with character capsule
        /// </summary>
        [Tooltip("Layers which collide with character capsule.")]
        public LayerMask layers;

        /// <summary>
        /// turn speed when moving （相对身体）只能往前方移动，当然头可以左右摇动
        /// </summary>
        [Tooltip("Turn speed when moving.")]
        public float movingTurnSpeed = 360;

        /// <summary>
        /// turn speed when stationary静止的
        /// </summary>
        [Tooltip("Turn speed when stationary .")]
        public float stationaryTurnSpeed = 180;

        /// <summary>
        /// move speed on ledges
        /// </summary>
        [Tooltip("Move speed on ledges.")]
        public float ledgeSpeed = 160.0f;

        /// <summary>
        /// how high to jump
        /// </summary>
        [Tooltip("How high to jump.")]
        public float jumpPower = 8.0f;

        /// <summary>
        /// increase / decrease gravity influence on character
        /// </summary>
        [Range(1f, 4f)]
        [Tooltip("Increase / decrease gravity influence on character.")]
        public float gravityMultiplier = 2f;

        /// <summary>
        /// distance from ground at which character will start falling animation
        /// 少于这个距离会开始着陆动画，并且认为character在ground上面
        /// </summary>
        [Tooltip("Distance from ground at which character will start falling animation.")]
        public float groundCheckDistance = 0.1f;

        /// <summary>
        /// determines the max speed of the character while airborne 空运
        /// </summary>
        [Tooltip("Determines the max speed of the character while airborne.")]
        public float airSpeed = 6;

        /// <summary>
        /// determines the response speed of controlling the character while airborne
        /// </summary>
        [Tooltip("Determines the response speed of controlling the character while airborne")]
        public float airControl = 2;

        /// <summary>
        /// increase / decrease animator speed
        /// </summary>
        [Tooltip("Increase / decrease animator speed.")]
        public float AnimatorSpeed = 1.0f;

        /// <summary>
        /// enable / disable jumping
        /// </summary>
        [Tooltip("Enable / disable jumping.")]
        public bool enableJumping = true;

        /// <summary>
        /// enable / disable crouching蹲伏
        /// </summary>
        [Tooltip("Enable / disable crouching.")]
        public bool enableCrouching = true;


        [SerializeField]
        private float m_RunCycleLegOffset = 0.2f;       
        //specific to the current character animation, will need to be modified to work with others
        //播放一次run动画，对象位移多少

        /// <summary>
        /// disable / enable capsule scale
        /// </summary>
        [HideInInspector]
        public bool disableCapsuleScale = false;

        /// <summary>
        /// simulate root motion by script
        /// </summary>
        [HideInInspector]
        public bool simulateRootMotion = true;      

        private MovingMode m_MoveMode = MovingMode.RotateToDirection;
        // MovingMode.RotateToDirection; 往某个方向移动，首先会把身子转向那个方向。 

        // REQUIRED COMPONENTS
        private Rigidbody m_Rigidbody;              // rigid body used for movement  
        private Animator m_Animator;                // animator
        private CapsuleCollider m_Capsule;          // capsule
        private Predefines m_defines;               // materials array
        private AudioManager m_Audio;               // audio system


        private Collider m_CurrentGroundCollider = null; // storing current ground collider
        private const float k_Half = 0.5f;          // helper
        private bool m_IsGrounded;                  // is character on ground ?      
        private float m_OrigGroundCheckDistance;    // original ground check distance ( will be changed and reverted恢复)
        //非跳跃状态，和地面距离<m_OrigGroundCheckDistance,算作落地
        private float m_AirGroundCheck = 0.1f;     
        // Ground Check distance when in air  跳跃状态，和地面距离少于0.1，算作在地面
        private float m_SideAmount;                 // turn / strafe amount  转头转动角度
        private float m_ForwardAmount;              // forward amount
        private Vector3 m_GroundNormal;             // current ground normal法线
        private float m_CapsuleHeight;              // collider capsule height
        private Vector3 m_CapsuleCenter;            // collider capsule center
        private bool m_Crouching;                   // is player crouching flag
        private Vector3 m_localMove;                // local move velocity
        private bool m_initialized = false;         // is class initialized ?
        private bool m_DisableMove = false;         // disable move flag
        private Vector3 m_currentHeadLookPos;       // The current position where the character head is looking
        private Vector3 m_currentBodyDirection;     // The current direction where the character body is looking
        private bool m_DisableGroundCheck = false;      // disables ground check
        private bool m_DisableGroundPull = false;       // disable ground pull (character get pulled towards拉向 ground if character is grounded and distance from ground is too high)
        private float m_DistanceFromGround = 0.0f;      // current distance from ground   
        private Vector3 m_MoveWS = Vector3.zero;        // current move velocity world space
        private float m_DampTime = 0.1f;                // animator blend tree blend time
        private float m_StrafeAmount = 0.0f;            // strafe value in strafing mode
        private MovingMode m_PrevMode = MovingMode.RotateToDirection;              // remember previus state of move

        protected bool m_PrevGrounded = false;      // is previous frame been grounded ( used for sound upon landing )
        protected bool m_Jumped = false;            // is character jumped by input

        // body head ik
        private bool m_headIKincrement = true;      // for smooth head ik switching helper

        private float m_cur_bodyhead_ik_weight = 0.0f;  // all weight for smooth transition to ik
        private float m_ik_body_weight = 0.2f;          // body weight for smooth transition to ik
        private float m_ik_head_weight = 2.5f;          // head weight for smoot transition to ik
        private float m_IKClampValue = 0.52f;           // ik clamp value 
        private IKMode m_IKMode = IKMode.None;
        private float m_ik_speed = 1.0f;

        // lerping
        public VoidFunc OnLerpEnd;               // event which will be called on lerp end
        public VoidFunc OnSetTransform;          // event which will be called on transform set
        private Vector3? m_PositionToSet = null;    // position to set on set transform / on lerp end
        private Quaternion? m_RotationToSet = null; // rotation to set on set transform / on lerp end
        private bool m_LerpToTransformFlag = false; // flag to lerping
        private float m_LerpPosTime = 0.0f,
            m_LerpRotTime = 0.0f;                   // helpers on lerp position
        private float m_LerpPosMaxTime = 1.0f,
            m_LerpRotMaxTime = 1.0f;                // helpers on lerp rotation      
        private float m_MaxLerpTime = 1.0f;         // lerp limit time
        private Vector3 m_LerpStartPosition;            // lerp start position
        private Quaternion m_LerpStartRotation;         // lerp start rotation

        private bool m_FullStop = false;            // full stop flag
        private bool m_SetTransformFlag = false;    // flag to set transform
        private bool m_JumpAllowed = true;          // enable / disable jumping
        private bool m_DiveAllowed = true;          // enable / disable dive rolling
        private PhysicMaterial m_Material;      // material to override default one

        // ledge helpers
        private Ledge m_CurrentLedge;               // current ledge 
        private bool m_IsInsideLedge = false;       // is character inside ledge ends
        private bool m_WaitForNextTurn = false;     // helper for stabilizing 稳定character
        private int m_CurrentGroundLayer = -1;      // current ground layer ( duh )
        private bool m_OnLedgeEdgeA = false,
            m_OnLedgeEdgeB = false;                 // sets to true if character is on one of ledge ends
        private bool m_LowHeadroom = false;         //low headroom flag
        private Vector3 m_DiveRollDirection = Vector3.forward; // keep direction to rotate upon animation start

        private const float GROUND_PULL_DISTANCE = 0.125f;
        private const float GROUND_PULL_FORCE = 1.95f;

        /// <summary>
        /// event fired on land after jump
        /// </summary>
        public VoidFunc OnLand = null;

        /// <summary>
        /// does character is moving on ledge
        /// </summary>
        [HideInInspector]
        public bool ledgeMove = false;

        /// <summary>
        /// speed multipler 
        /// </summary>
        [HideInInspector]
        public float moveSpeedMultiplier = 1.0f;

        /// <summary>
        /// slope斜坡 speed multipler ( used by slope control script )
        /// </summary>
        [HideInInspector]
        public float slopeMultiplier = 1.0f;

        /// <summary>
        /// trigger animation root motion ( applied when in trigger action but not grounded ) 
        /// </summary>
        [HideInInspector]
        public bool triggerRootMotion = false;   

#endregion

#region Properties

        /// <summary>
        /// returns true if there is no space to stand up
        /// </summary>
        public bool lowHeadroom { get { return m_LowHeadroom; } }

        /// <summary>
        /// returns true if character is on ledge left edge
        /// </summary>
        public bool onLedgeEdgeA { get { return m_OnLedgeEdgeA; } }

        /// <summary>
        /// returns true if character is on ledge right edge
        /// </summary>
        public bool onLedgeEdgeB { get { return m_OnLedgeEdgeB; } }

        /// <summary>
        /// get current collider under capsule
        /// </summary>
        public Collider currentGroundCollider { get { return m_CurrentGroundCollider; } }

        /// <summary>
        /// gets reference to material holder script
        /// </summary>
        public Predefines physicsMaterials { get { return m_defines; } }

        /// <summary>
        /// gets and sets jump enabling flag
        /// </summary>
        public bool jumpAllowed { get { return m_JumpAllowed; } set { m_JumpAllowed = value; } }

        /// <summary>
        /// gets and sets dive rolling flag
        /// </summary>
        public bool diveAllowed { get { return m_DiveAllowed; } set { m_DiveAllowed = value; } }

        /// <summary>
        /// gets and sets disabling ground checking flag
        /// </summary>
        public bool disableGroundCheck { get { return m_DisableGroundCheck; } set { m_DisableGroundCheck = value; } }

        /// <summary>
        /// gets reference to rigid body
        /// </summary>
        public Rigidbody rigidBody { get { return m_Rigidbody; } }

        /// <summary>
        /// gets reference to animator
        /// </summary>
        public Animator animator { get { return m_Animator; } set { m_Animator = value; } }

        /// <summary>
        /// gets reference to capsule collider
        /// </summary>
        public CapsuleCollider capsule { get { return m_Capsule; } }

        /// <summary>
        /// gets is character in grounded mode
        /// </summary>
        public bool isGroundMode { get { return m_IsGrounded; } }

        /// <summary>
        /// gets and sets turn amount
        /// </summary>
        public float sideAmount { get { return m_SideAmount; } set { m_SideAmount = value; } }

        /// <summary>
        /// gets and sets forward amount
        /// </summary>
        public float forwardAmount { get { return m_ForwardAmount; } set { m_ForwardAmount = value; } }

        /// <summary>
        /// gets and sets forcing max friction material
        /// </summary>
        public bool forceMaxFrictionMaterial { get; set; }

        /// <summary>
        /// gets and sets forcing zero friction material
        /// </summary>
        public bool forceZeroFrictionMaterial { get; set; }

        /// <summary>
        /// gets current move mode
        /// </summary>
        public MovingMode moveMode { get { return m_MoveMode; } }

        /// <summary>
        /// Returns true if character is crouching. Otherwise false.
        /// </summary>
        public bool isCrouching { get { return m_Crouching; } }

        /// <summary>
        /// Returns true if character is roll diving. Otherwise false.
        /// </summary>
        public bool isDiving
        {
            get
            {
#if DEBUG_INFO
                if (!m_initialized) { Debug.LogError("object not initialized. " + " < " + this.ToString() + ">"); return false; }
#endif
                return m_Animator.GetBool(/*"pDiveRoll"*/HashIDs.DiveRollBool);
            }
        }

        /// <summary>
        /// gets and sets ground pull if grounded
        /// </summary>
        public bool disableGroundPull { get { return m_DisableGroundPull; } set { m_DisableGroundPull = value; } }

        /// <summary>
        /// gets current distance from ground
        /// </summary>
        public float distanceFromGround { get { return m_DistanceFromGround; } }

        /// <summary>
        /// gets character move vector
        /// </summary>
        public Vector3 moveWS { get { return m_MoveWS; } }

        /// <summary>
        /// gets and sets disable moving
        /// </summary>
        public bool disableMove { get { return m_DisableMove; } set { m_DisableMove = value; } }

        /// <summary>
        /// gets current ground normal under character
        /// </summary>
        public Vector3 groundNormal { get { return m_GroundNormal; } }

        /// <summary>
        /// gets and sets animator damp time
        /// </summary>
        public float animatorDampTime { get { return m_DampTime; } set { m_DampTime = value; } }

        /// <summary>
        /// gets and sets flag to constraint character between ledge ends
        /// </summary>
        public bool disableLedgeConstraint { get; set; }

        /// <summary>
        /// gets current edge
        /// </summary>
        public Ledge ledge { get { return m_CurrentLedge; } }

        /// <summary>
        /// returns true if character is actualy on ground
        /// </summary>
        public bool isOnGround { get { return (m_DistanceFromGround <= GROUND_PULL_DISTANCE) && m_IsGrounded; } }

        /// <summary>
        /// get reference to audio manager
        /// </summary>
        public AudioManager audioManager { get { return m_Audio; } }

#endregion


        /// <summary>
        /// initialize all
        /// </summary>
        public void initialize()
        {
            if (m_initialized) return;

            m_Animator = GetComponent<Animator>();
            if (!m_Animator) { Debug.LogError("Cannot find 'Animator' component! " + " < " + this.ToString() + ">"); return; }

            m_Rigidbody = GetComponent<Rigidbody>();
            if (!m_Rigidbody) { Debug.LogError("Cannot find 'Rigidbody' component! " + " < " + this.ToString() + ">"); return; }

            m_Capsule = GetComponent<CapsuleCollider>();
            if (!m_Capsule) { Debug.LogError("Cannot find 'CapsuleCollider' component! " + " < " + this.ToString() + ">"); return; }

            // find object of type predefins which currently holds material types
            m_defines = FindObjectOfType<Predefines>();
            if (!m_defines)
            {
                Debug.LogError("Cannot find object 'Predefines'" + " < " + this.ToString() + ">");
                return;
            }

            m_Audio = GetComponent<AudioManager >();
            if (!m_Audio) { Debug.LogError("Cannot find component 'AudioManager'" + " < " + this.ToString() + ">");return; }

            m_CapsuleHeight = m_Capsule.height;
            m_CapsuleCenter = m_Capsule.center;

            m_Rigidbody.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;
            m_OrigGroundCheckDistance = groundCheckDistance;

            forceMaxFrictionMaterial = false; //Friction摩擦
            forceZeroFrictionMaterial = false;

            m_initialized = true;

            _checkGroundStatus();
        }

        /// <summary>
        /// override default physics material
        /// </summary>
        /// <param name="mat"></param>
        public void overridePhysicMaterial(PhysicMaterial mat)
        {
            m_Material = mat;
        }

        /// <summary>
        /// set zero friction material to capsule collider
        /// </summary>
        public void setZeroFractionMaterial()
        {
#if DEBUG_INFO
            if(!m_initialized)
            {
                Debug.LogError("Component not initialized. " +" < " + this.ToString() + ">"); 
                return;
            }
#endif
            m_Capsule.material = m_defines.zeroFrictionMaterial;//当前碰撞体的物理摩擦材质
        }

        /// <summary>
        /// set max friction material to capsule collider
        /// </summary>
        public void setMaxFrictionMaterial()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized. " +" < " + this.ToString() + ">");
                return;
            }
#endif
            m_Capsule.material = m_defines.maxFrictionMaterial;
        }

        /// <summary>
        /// set current head IK mode
        /// </summary>
        /// <param name="mode">new ik mode</param>
        public void setIKMode(IKMode mode)
        {
            if (m_IKMode == mode) return;
            switch (mode)
            {
                case IKMode.Head: //
                    m_headIKincrement = true;
                    m_cur_bodyhead_ik_weight = 0.0f; //IK效果作用于全身权重
                    m_ik_head_weight = 0.75f; //作用于头部权重
                    m_ik_body_weight = 0.25f; //作用于身体权重
                    break;
                case IKMode.Waist:
                    m_headIKincrement = true;
                    m_cur_bodyhead_ik_weight = 0.0f;
                    m_ik_body_weight = 1; // 0.75f;
                    m_ik_head_weight = 0; // 0.25f;
                    break;
                case IKMode.ToNone:
                    m_headIKincrement = false;
                    m_cur_bodyhead_ik_weight = 1.0f;
                    break;
                case IKMode.None:
                    m_headIKincrement = false;
                    m_cur_bodyhead_ik_weight = 1.0f;
                    break;
            }
            m_IKMode = mode;
        }

        /// <summary>
        /// get current head IK mode
        /// </summary>
        /// <returns>current ik mode</returns>
        public IKMode getIKMode()
        {
            return m_IKMode;
        }

        /// <summary>
        /// set character move mode  Strafe惩罚
        /// </summary>
        /// <param name="mmode">new move mode</param>
        public void setMoveMode(MovingMode mmode)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized. " +" < " + this.ToString() + ">");
                return;
            }
#endif
            if (mmode == MovingMode.Strafe)
            {
                m_Animator.SetBool(/*"pStrafe"*/HashIDs.StrafeBool , true);
            }
            else
            {
                m_Animator.SetBool(/*"pStrafe"*/HashIDs.StrafeBool, false);
            }
            m_MoveMode = mmode;
        }

        /// <summary>
        /// set current ledge
        /// </summary>
        /// <param name="xform">transform from which ledge will be calculated</param>
        /// <returns>ledge creation success</returns>
        public bool setLedge(Transform xform, bool reversed = false, float zoffset = 0.0f)
        {
            if (!xform)
            {
                m_CurrentLedge.transform = null;
                return false;
            }
            float scaleX = xform.lossyScale.x;
            float halfScaleX = scaleX * 0.5f;
            m_CurrentLedge.transform = xform;

            Vector3 pointA = xform.position - xform.right * halfScaleX;
            Vector3 pointB = xform.position + xform.right * halfScaleX;

            Vector3 dir = (reversed ? -xform.forward : xform.forward);

            pointA = pointA + dir * zoffset;
            pointB = pointB + dir * zoffset;

            m_CurrentLedge.reversed = reversed;
            if (!reversed)
            {
                m_CurrentLedge.leftPoint = pointA;
                m_CurrentLedge.rightPoint = pointB;
            }
            else
            {
                m_CurrentLedge.leftPoint = pointB;
                m_CurrentLedge.rightPoint = pointA;
            }

            Vector3 pt1 = xform.position;
            Vector3 pt2 = pt1 + xform.right;
            Vector3 pt3 = pt1 + xform.forward;
            m_CurrentLedge.plane.Set3Points(pt1, pt2, pt3);

            return true;
        }

        /// <summary>
        /// get ledge direction
        /// </summary>
        /// <returns></returns>
        public Vector3 getLedgeDirection()
        {
            return (m_CurrentLedge.rightPoint - m_CurrentLedge.leftPoint).normalized;
        }

        /// <summary>
        /// main character move function
        /// </summary>
        /// <param name="move">move velocity 移动方向</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="jump">jump flag</param>
        /// <param name="rotateDir">body rotation direction  身体应该往哪个方向转（一般和移动方向一致）</param>
        /// <param name="headLookPos">head look position，  头当前看向哪个方向（已经计算好了）</param>
        /// <param name="side">turn amount nullable</param>
        public void move(Vector3 move, bool crouch, bool jump, bool diveRoll,
            Vector3 rotateDir, Vector3 headLookPos, float? side = null, Vector3? diveDirection = null)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif


            if (m_LerpToTransformFlag)
                return;
            if (m_WaitForNextTurn) return;
            if (isDiving)
            {
                return;
            }            
            if (move.magnitude > 1f) move.Normalize();
            move.y = 0.0f;
            m_MoveWS = move;

            jump = jump && m_JumpAllowed && enableJumping && !m_LowHeadroom;
            //是否开始进入Jump状态
            crouch = crouch && enableCrouching;
            //是否开始进入crouch状态
            if (diveRoll && isGroundMode && (m_MoveMode == MovingMode.RotateToDirection || m_MoveMode == MovingMode.Strafe) )
            {
                Vector3 diveDir = (diveDirection.HasValue ? diveDirection.Value : move);
                _dive(diveDir);
                
                return;
            }
            
            m_currentHeadLookPos = headLookPos;

            if (!m_DisableMove)
            {
                if (rotateDir.magnitude > 1f) rotateDir.Normalize();
                m_currentBodyDirection = rotateDir;
                // convert the world relative moveInput vector into a local-relative
                // turn amount and forward amount required to head in the desired
                // direction.
                Vector3 localMove = transform.InverseTransformDirection(move);//转化为局部坐标
                Vector3 localRotationDir = transform.InverseTransformDirection(m_currentBodyDirection);
                m_ForwardAmount = localMove.z;
                switch (m_MoveMode)
                {
                    case MovingMode.Strafe:
                        {
                            m_StrafeAmount = localMove.x;
                            if (side.HasValue)
                            {
                                m_StrafeAmount = side.Value;
                                m_SideAmount = side.Value;
                            }
                            else
                            {
                                float atan2 = 0.0f;
                                if (localRotationDir != Vector3.zero)
                                    atan2 = Mathf.Atan2(localRotationDir.x, localRotationDir.z);
                                m_SideAmount = atan2;
                            }
                        }
                        break;
                    case MovingMode.RotateToDirection:
                        {
                            if (side.HasValue)
                            {
                                m_SideAmount = side.Value;
                            }
                            else
                            {
                                float atan2 = 0.0f;
                                if (localRotationDir != Vector3.zero)
                                    atan2 = Mathf.Atan2(localRotationDir.x, localRotationDir.z);
                                //这个atan2函数曲率很陡，也就是刚开始砖头慢，后面很快
                                m_SideAmount = atan2;
                            }
                        }
                        break;
                    case MovingMode.Ledge:
                        {
                            m_StrafeAmount = localMove.x ; 
                        }
                        break;
                }

                // control and velocity handling is different when grounded and airborne:
                if (m_IsGrounded)
                {
                    _handleGroundedMovement(ref move, crouch, jump);
                }
                else
                {
                    _handleAirborneMovement(ref m_MoveWS);
                }

                if (m_MoveMode != MovingMode.Ledge)
                    applyExtraTurnRotation();

                _setFriction(ref move);

                _scaleCapsuleForCrouching(crouch);
            }
            _preventStandingInLowHeadroom();

            // send input and other state parameters to the animator
            _updateAnimator();
        }

        /// <summary>
        /// disable character physics
        /// </summary>
        /// <param name="detectCollisions">continue using collision detection</param>
        /// <param name="iskinematic">use collider as kinematic</param>
        public void disablePhysics(bool detectCollisions = false, bool iskinematic = true, bool isTrigger = true)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            m_DisableGroundCheck = true;
            m_Rigidbody.detectCollisions = detectCollisions;
            m_Capsule.isTrigger = isTrigger;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.isKinematic = iskinematic;
        }

        /// <summary>
        /// enable physics
        /// </summary>
        public void enablePhysics()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            m_DisableGroundCheck = false;
            m_Rigidbody.detectCollisions = true;
            m_Capsule.isTrigger = false;
            m_Rigidbody.useGravity = true;
            m_Rigidbody.isKinematic = false;
        }

        /// <summary>
        /// sets all character and animator move values to 0
        /// </summary>
        public void fullStop()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif

            m_Rigidbody.velocity = Vector3.zero;
            m_ForwardAmount = 0.0f;
            m_SideAmount = 0.0f;
            m_Animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, 0);
            m_Animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, 0);
        }

        /// <summary>
        /// lerp to new position and/or rotation
        /// </summary>
        /// <param name="position">new position</param>
        /// <param name="rotation">new rotation</param>
        /// <param name="lerp_time_position">time to new position</param>
        /// <param name="lerp_time_rotation">time to new rotation</param>
        /// <param name="forwardAmount">apply animator forward amount</param>
        /// <param name="crouch">apply crouching</param>
        public void lerpToTransform(
            Vector3? position,
            Quaternion? rotation,
            float lerp_time_position,
            float lerp_time_rotation,
            float forwardAmount = 0.0f,
            bool crouch = false,
            bool fullStop = true
            )
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif

            m_PositionToSet = position;
            m_RotationToSet = rotation;
            m_LerpStartPosition = transform.position;
            m_LerpStartRotation = transform.rotation;
            m_LerpPosTime = 0.0f;
            m_LerpPosMaxTime = lerp_time_position;
            m_LerpRotTime = 0.0f;
            m_LerpRotMaxTime = lerp_time_rotation;

            m_MaxLerpTime = (m_LerpPosMaxTime > m_LerpRotMaxTime ? m_LerpPosMaxTime : m_LerpRotMaxTime);

            m_DisableMove = true;
            m_LerpToTransformFlag = true;



            m_ForwardAmount = forwardAmount;
            m_SideAmount = 0.0f;
            m_Crouching = crouch;
            m_FullStop = fullStop;
        }

        /// <summary>
        /// set new transform
        /// </summary>
        /// <param name="position">new transform position</param>
        /// <param name="rotation">new transform rotation</param>
        /// <param name="fullStop">applies all animator move parameters to zero</param>
        public void setTransform(Vector3? position, Quaternion? rotation, bool fullStop)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            m_PositionToSet = position;
            m_RotationToSet = rotation;
            m_FullStop = fullStop;
            m_SetTransformFlag = true;
        }

        /// <summary>
        /// scale character capsule
        /// </summary>
        /// <param name="heightOffset">offset from character position</param>
        public void scaleCapsuleToHalf(float heightOffset = 0.0f)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            m_Capsule.height = m_CapsuleHeight / 2f;
            Vector3 center = m_Capsule.center;
            center = (m_CapsuleCenter / 2f);
            center.y += heightOffset;
            m_Capsule.center = center;
        }

        /// <summary>
        /// restore original character capsule size
        /// </summary>
        public void restoreCapsuleSize()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
        }

        /// <summary>
        /// apply extra rotation for faster turning
        /// </summary>
        /// <param name="extraSpeed">extra applied speed</param>
        public void applyExtraTurnRotation(float extraSpeed = 1.0f)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_DisableMove) return;

            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, m_ForwardAmount) *
                extraSpeed;
            transform.Rotate(0, m_SideAmount * turnSpeed * Time.deltaTime, 0);
        }

        /// <summary>
        /// used for immediate turning to direction
        /// </summary>
        /// <param name="dir">turn direction</param>
        public void turnTransformToDirection(Vector3 dir)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            Vector3 transformedDir = transform.position + dir;
            transform.LookAt(transformedDir, transform.up);
        }

        /// <summary>
        /// get current ground layer
        /// </summary>
        /// <returns></returns>
        public int getCurrentGroundLayer()
        {
            return m_CurrentGroundLayer;
        }


        /// <summary>
        /// rotate character to dive roll direction
        /// </summary>
        public void turnToDiveRollDIrection()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_Capsule.height = m_CapsuleHeight / 2f;
            m_Capsule.center = m_CapsuleCenter / 2f;
            disableCapsuleScale = true;
            m_Capsule.material = m_defines.maxFrictionMaterial;// to prevent flying off on slopes due to no friction
            m_Audio.playDiveRollSound();
            turnTransformToDirection(m_DiveRollDirection);
        }


        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            // initialize all
            initialize();
        }

        // unity animator move method
        void OnAnimatorMove()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_SetTransformFlag)
            {
                _onTransformSet();
                m_SetTransformFlag = false;
                m_WaitForNextTurn = true;
                return;
            }
            if (m_LerpToTransformFlag)
            {
                if (_lerping())
                {
                    m_LerpToTransformFlag = false;
                    m_WaitForNextTurn = true;
                }
                return;
            }

            if (m_WaitForNextTurn)
            {
                m_WaitForNextTurn = false;
                return;
            }

            if (ledgeMove)
            {
                _calculateLedgeDistances();
                Vector3 direction = getLedgeDirection();
                float multiplier = _applyLedgeConstraints();
                if (m_Rigidbody.isKinematic)
                {
                    transform.position += direction * multiplier * Time.deltaTime * ledgeSpeed * m_StrafeAmount;
                }
                else
                {
                    m_Rigidbody.velocity = direction * multiplier * m_StrafeAmount * Time.deltaTime * ledgeSpeed;
                }


                return;
            }

            // simulate root motion if grounded or using some trigger animation
            bool rootMotion = m_IsGrounded || triggerRootMotion;

            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (rootMotion && Time.deltaTime > 0 && simulateRootMotion)
            {
                Vector3 v = (m_Animator.deltaPosition * moveSpeedMultiplier * slopeMultiplier) / Time.deltaTime;
#if DEBUG_INFO
                if(float.IsNaN (v.x)) { Debug.LogError("Velocity is NAN: delta x: " + m_Animator.deltaPosition .x + " < " + this.ToString() + ">"); return; }
                if (float.IsNaN(v.y)) { Debug.LogError("Velocity is NAN: delta y: " + m_Animator.deltaPosition.y + " < " + this.ToString() + ">"); return; }
                if (float.IsNaN(v.z)) { Debug.LogError("Velocity is NAN: delta z: " + m_Animator.deltaPosition.z + " < " + this.ToString() + ">"); return; }
#endif

                if (m_Rigidbody.isKinematic)
                {
                    transform.position += v * Time.deltaTime;
                }
                else
                {
                    if (m_Rigidbody.useGravity) v.y = m_Rigidbody.velocity.y;
                    m_Rigidbody.velocity = v;
                }
            }
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
            _checkGroundStatus();
        }

        /// <summary>
        /// Unity LateUpdate method
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
            // only used for checking jump down event
            if (isOnGround && !m_PrevGrounded)
            {
                m_Jumped = false;
                m_DisableGroundPull = false;
                m_Audio.playFootstepSound(0);
                if (OnLand != null)
                    OnLand();
            }
            m_PrevGrounded = isOnGround;
        }


        // unity ik method
        void OnAnimatorIK(int layerIndex)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("Component not initialized! " +" < " + this.ToString() + ">");
                return;
            }
#endif

            // Head look at

            if (m_headIKincrement)
            {
                if (m_cur_bodyhead_ik_weight < 1.0f)
                    m_cur_bodyhead_ik_weight += Time.deltaTime * m_ik_speed;
                m_cur_bodyhead_ik_weight = Mathf.Clamp01(m_cur_bodyhead_ik_weight);
            }
            else
            {
                if (m_cur_bodyhead_ik_weight > 0)
                    m_cur_bodyhead_ik_weight -= Time.deltaTime * m_ik_speed * 2f;
                m_cur_bodyhead_ik_weight = Mathf.Clamp01(m_cur_bodyhead_ik_weight);
                if (m_cur_bodyhead_ik_weight == 0)
                {
                    m_IKMode = IKMode.None;
                }
            }
            if (m_IKMode != IKMode.None)
            {
                m_Animator.SetLookAtWeight(
                    m_cur_bodyhead_ik_weight,
                    m_ik_body_weight,
                    m_ik_head_weight,
                    0f,
                    m_IKClampValue);
                m_Animator.SetLookAtPosition(m_currentHeadLookPos);
            }
        }

#if DEBUG_INFO
        void OnDrawGizmos() //Selected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + m_MoveWS);

            if (m_CurrentLedge.transform)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(m_CurrentLedge.leftPoint, 0.15f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(m_CurrentLedge.rightPoint, 0.15f);
            }
        }
#endif

        /// <summary>
        /// check if character is on ledge end
        /// </summary>
        private void _calculateLedgeDistances()
        {
            float distA, distB;
            MathUtils.GetLineEdgeDistances(m_CurrentLedge.leftPoint, m_CurrentLedge.rightPoint, transform.position, out distA, out distB);
            m_OnLedgeEdgeA = false;
            m_OnLedgeEdgeB = false;
            if (distA < 0.05)
                m_OnLedgeEdgeA = true;
            if (distB < 0.05f)
                m_OnLedgeEdgeB = true;

        }




        /// <summary>
        /// apply ledge constraining
        /// </summary>
        /// <returns>move modifier</returns>
        private float _applyLedgeConstraints()
        {
            if (m_MoveMode != MovingMode.Ledge) return 0.0f;

            bool moving = m_MoveWS != Vector3.zero;
            if (!moving) return 0.0f;


            Ray moveray = new Ray(transform.position + m_Capsule.center, m_MoveWS);
            RaycastHit rayhit;
            float moveraydist = m_Capsule.radius;



            if (Physics.Raycast(moveray, out rayhit, moveraydist, layers.value))
            {
                return 0.0f;
            }
            else
            {
                Vector3 point = transform.position + m_Capsule.center;
                Vector3 nextPoint = point + m_MoveWS.normalized * 0.05f;

                Vector3 a = m_CurrentLedge.leftPoint;
                Vector3 b = m_CurrentLedge.rightPoint;

                Ray ray = new Ray(nextPoint, Vector3.up);
                float rayDist = 0.0f;
                if (m_CurrentLedge.plane.Raycast(ray, out rayDist))
                {
                    nextPoint = nextPoint + Vector3.up * rayDist;
                }

                m_IsInsideLedge = MathUtils.IsInsideLineSegment(a, b, ref nextPoint);

                if (!m_IsInsideLedge && !disableLedgeConstraint)
                {
                    return 0.0f;
                }
            }
            return 1.0f;
        }

        /// <summary>
        /// check ground status, material, slope, distance from ground ...
        /// </summary>
        private void _checkGroundStatus()
        {
#if DEBUG_INFO
            if (!m_initialized) { Debug.LogError("component not initialized. " + " < " + this.ToString() + ">"); return; }
#endif
            if (!m_Animator.isInitialized) return;

            float yOffset = 0.52f;
            if (yOffset >= groundCheckDistance)
                yOffset = groundCheckDistance - 0.001f;
            int layerMask = layers;
            RaycastHit hitInfo;
            float pJumpValue = m_Animator.GetFloat("pJump");
            if (m_DisableGroundCheck || pJumpValue > 0)
            {
                // just check distance from ground and return
                Vector3 raycastPos = transform.position + Vector3.up * yOffset; // little above
                Ray gRay = new Ray(raycastPos, Vector3.down);
                if (Physics.Raycast(gRay, out hitInfo, float.MaxValue, layerMask))
                {
                    m_GroundNormal = hitInfo.normal;
                    m_DistanceFromGround = hitInfo.distance - yOffset;
                    m_CurrentGroundLayer = hitInfo.collider.gameObject.layer;
                    m_CurrentGroundCollider = hitInfo.collider;
                    if (m_DistanceFromGround > groundCheckDistance)
                        m_IsGrounded = false;
                    else m_IsGrounded = true;
                }
                return;
            }

            float radius = m_Capsule.radius * 0.9f;
            Vector3 sphereCenter1 = transform.position;

            sphereCenter1.y += radius + m_AirGroundCheck * 2f;

            if (Physics.SphereCast(sphereCenter1, radius,
                Vector3.down, out hitInfo, groundCheckDistance + radius + yOffset, layerMask))
            {
                m_GroundNormal = hitInfo.normal;
                m_DistanceFromGround = hitInfo.distance - radius;
                m_CurrentGroundLayer = hitInfo.collider.gameObject.layer;
                m_IsGrounded = true;

                m_CurrentGroundCollider = hitInfo.collider;

            }
            else
            {
                Vector3 raycastPos = transform.position + Vector3.up * yOffset; // little above
                Ray gRay = new Ray(raycastPos, Vector3.down);
                if (Physics.Raycast(gRay, out hitInfo, float.MaxValue))
                {
                    m_CurrentGroundLayer = hitInfo.collider.gameObject.layer;
                    m_DistanceFromGround = hitInfo.distance - yOffset;
                    m_CurrentGroundCollider = hitInfo.collider;
                }
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
            }

            // add additional ground pull if grouneded flag is true but character is above ground some distance ( 0.3f)
            if (!m_DisableGroundPull)
            {
                float pJump = m_Animator.GetFloat(HashIDs.JumpFloat /*"pJump"*/);
                if (m_IsGrounded || (pJump > 0 && !m_Jumped))
                    if (m_DistanceFromGround > GROUND_PULL_DISTANCE)
                    {
                        m_Rigidbody.velocity += Vector3.down * GROUND_PULL_FORCE;
                    }
            }
        }

        /// <summary>
        /// movement in air。 在空中可以控制移动方向
        /// </summary>
        /// <param name="moveInput">move velocity</param>
        private void _handleAirborneMovement(ref Vector3 moveInput)
        {
            if (!m_initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized. " + " < " + this.ToString() + ">");
#endif
                return;
            }
            // we allow some movement in air, but it's very different to when on ground
            // (typically allowing a small change in trajectory)
            Vector3 airMove = new Vector3(moveInput.x * airSpeed, m_Rigidbody.velocity.y, moveInput.z * airSpeed);
            //真实移动是通过rigidbody，这里只是计算输出，并得出对rigidbody应该添加的速度
            m_Rigidbody.velocity = Vector3.Lerp(m_Rigidbody.velocity, airMove, Time.deltaTime * airControl);
            //在空中操作移动是不是地面那么自由，他是差值。当前已有移动和玩家控制的移动之间的差值

            // apply extra gravity from multiplier:  gravityMultiplier为2就相当于1
            Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            //所有移动都是通过rigidbody 物理系统。 速度少于0说明非跳跃状态（移动，翻越（动画效果）），groundCheckDistance就是m_OrigGroundCheckDistance
            //否则是跳跃状态
            groundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : m_AirGroundCheck;
        }

        /// <summary>
        /// movement on ground 地面移动
        /// </summary>
        /// <param name="move">move velocity</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="jump">jump flag</param>
        private void _handleGroundedMovement(ref Vector3 move, bool crouch, bool jump)
        {
            if (!m_initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized. " + " < " + this.ToString() + ">");
#endif
                return;
            }

            // check whether conditions are right to allow a jump:
            if (jump && !crouch && m_DistanceFromGround <= GROUND_PULL_DISTANCE)
            {
                // jump!
                m_DisableGroundPull = true;
                m_Jumped = true;
                Vector3 velocity = move; 
                velocity.y = jumpPower;
                m_Rigidbody.velocity = velocity; //移动是通过rigidbody
                groundCheckDistance = m_AirGroundCheck;
                m_Audio.playJumpSound();
            }

            
        }

        /// <summary>
        /// scale capsule if crouching
        /// </summary>
        /// <param name="crouch">crouch flag</param>
        private void _scaleCapsuleForCrouching(bool crouch)
        {
            if (disableCapsuleScale) return;
            if (!m_IsGrounded) return;

            if (m_IsGrounded && crouch)
            {
                if (m_Crouching) return;
                scaleCapsuleToHalf();
                m_Crouching = true;
            }
            else
            {
                if (!m_Crouching) return;
                restoreCapsuleSize();
                m_Crouching = false;
            }
        }

        /// <summary>
        /// scale capsule if low headroom
        /// </summary>
        private void _preventStandingInLowHeadroom()
        {
            if (disableCapsuleScale) return;
            if (!isOnGround) return;

            m_LowHeadroom = false;
            
            // prevent standing up in crouch-only zones
            if (!m_Crouching)
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                RaycastHit hit;
                int layerMask = layers;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, out hit, crouchRayLength, layerMask))
                {
                    if (!hit.collider.isTrigger)
                    {
#if DEBUG_INFO
                        Debug.Log("low headroom by : " + hit.collider.name);
#endif
                        m_LowHeadroom = true;
                        m_Crouching = true;
                        scaleCapsuleToHalf();
                    }
                }
            }
        }

        /// <summary>
        /// update animator parameters
        /// </summary>
        private void _updateAnimator()
        {
            if (!m_IsInsideLedge && m_MoveMode == MovingMode.Ledge )
            {
                m_SideAmount = 0.0f;
                m_StrafeAmount = 0.0f;
            }
            m_Animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, m_ForwardAmount * moveSpeedMultiplier * slopeMultiplier, m_DampTime, Time.deltaTime);
            switch (m_MoveMode)
            {
                case MovingMode.Strafe:
                case MovingMode.Ledge://这两种模式下 
                    {
                        m_SideAmount = m_StrafeAmount + Mathf.Clamp(m_SideAmount, -0.1f, 0.1f);
                    }
                    break;
            }
            //阻抑
            m_Animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, m_SideAmount, m_DampTime, Time.deltaTime);
            m_Animator.SetBool(/*"pCrouch"*/HashIDs.CrouchBool , m_Crouching);
            if (!m_IsGrounded)
            {
                m_Animator.SetFloat(/*"pJump"*/HashIDs.JumpFloat, m_Rigidbody.velocity.y);
            }
            m_Animator.SetBool(/*"pOnGround"*/HashIDs.OnGroundBool , m_IsGrounded);


            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat(/*"pJumpLeg"*/HashIDs.JumpLegFloat , jumpLeg);
            }

            m_Animator.speed = AnimatorSpeed;
        }

        /// <summary>
        /// set friction material
        /// </summary>
        /// <param name="move">move velocity</param>
        private void _setFriction(ref Vector3 move)
        {
            if (forceMaxFrictionMaterial)
            {
                m_Capsule.material = m_defines.maxFrictionMaterial;
                return;
            }
            if (forceZeroFrictionMaterial)
            {
                m_Capsule.material = m_defines.zeroFrictionMaterial;
                return;
            }
            if (m_IsGrounded)
            {
                // set friction to low or high, depending on if we're moving
                if (move.magnitude == 0)
                {
                    // when not moving this helps prevent sliding on slopes:
                    m_Capsule.material = m_defines.maxFrictionMaterial;
                }
                else
                {
                    // but when moving, we want no friction:
                    m_Capsule.material = m_defines.zeroFrictionMaterial;
                }
            }
            else
            {
                // while in air, we want no friction against surfaces (walls, ceilings, etc)
                m_Capsule.material = m_defines.zeroFrictionMaterial;
            }
            if(m_Material )
            {
                m_Capsule.material = m_Material;
            }
        }

        /// <summary>
        /// setup and start dive rolling
        /// </summary>
        /// <param name="turnDirection">turn direction</param>
        private void _dive(Vector3 turnDirection)
        {
#if DEBUG_INFO
            if(!m_Animator) { Debug.LogError("object cannot be null < " + this.ToString() + ">");return; }
#endif
            if (!m_DiveAllowed) return;
            m_DiveRollDirection = turnDirection;
            m_PrevMode = m_MoveMode;
            m_MoveMode = MovingMode.RotateToDirection;
            setIKMode(IKMode.None);
            m_Animator.SetBool(/*"pDiveRoll"*/HashIDs.DiveRollBool , true);
            m_Animator.SetBool(HashIDs.OnGroundBool, true);
        }

        /// <summary>
        /// lerping function. 
        /// </summary>
        /// <returns>returns true if finished lerping</returns>
        private bool _lerping()
        {
            m_LerpPosTime += Time.deltaTime;
            m_LerpRotTime += Time.deltaTime;
            m_Animator.speed = AnimatorSpeed;
            if (m_LerpPosTime > m_MaxLerpTime &&
                m_LerpRotTime > m_MaxLerpTime)
            {
                m_Animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, m_ForwardAmount);
                m_Animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, m_SideAmount);
                m_Animator.SetBool(/*"pCrouch"*/HashIDs.CrouchBool , m_Crouching);

                if(m_FullStop) fullStop();
                if (m_PositionToSet.HasValue)
                {
                    transform.position = m_PositionToSet.Value;
                }
                if (m_RotationToSet.HasValue)
                {
                    transform.rotation = m_RotationToSet.Value;
                }
                m_PositionToSet = null;
                m_RotationToSet = null;
                m_FullStop = false;

                m_DisableMove = false;

                if (OnLerpEnd != null)
                    OnLerpEnd();

                return true;
            }
            float lerpValue = m_LerpPosTime / m_LerpPosMaxTime;
            lerpValue = Mathf.Min(lerpValue, 1.0f);
            if (m_PositionToSet.HasValue)
            {
                transform.position = Vector3.Lerp(m_LerpStartPosition, m_PositionToSet.Value, lerpValue);
            }
            lerpValue = m_LerpRotTime / m_LerpRotMaxTime;
            lerpValue = Mathf.Min(lerpValue, 1.0f);
            if (m_RotationToSet.HasValue)
            {
                transform.rotation = Quaternion.Slerp(m_LerpStartRotation, m_RotationToSet.Value, lerpValue);
            }
            m_Animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, m_ForwardAmount);
            m_Animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, m_SideAmount);
            m_Animator.SetBool(/*"pCrouch"*/HashIDs.CrouchBool, m_Crouching);
            return false;
        }

        /// <summary>
        /// sets all transform values
        /// </summary>
        private void _onTransformSet()
        {
            if (m_FullStop) fullStop();
            if (m_PositionToSet.HasValue)
            {
                transform.position = m_PositionToSet.Value;
                m_Rigidbody.position = m_PositionToSet.Value;
            }
            if (m_RotationToSet.HasValue)
            {
                transform.rotation = m_RotationToSet.Value;
                m_Rigidbody.rotation = m_RotationToSet.Value;
            }
            m_PositionToSet = null;
            m_RotationToSet = null;
            m_FullStop = false;
            if (OnSetTransform != null)
                OnSetTransform();
        }

        /// <summary>
        /// animation event called upon dive roll end time
        /// </summary>
        /// <param name="e">AnimationEvent info class</param>
        void OnDiveRollEndEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if(!m_initialized )
            {
                Debug.LogError("Component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            disableCapsuleScale = false;
            animator.SetBool(/*"pDiveRoll"*/HashIDs.DiveRollBool, false);
            if (!m_Crouching) restoreCapsuleSize();
            m_MoveMode = m_PrevMode;
        }


    } 
}
