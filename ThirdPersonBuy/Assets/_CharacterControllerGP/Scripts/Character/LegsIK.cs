// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{

    /// <summary>
    /// class that keep ledge on ground and provides mechanism for stepping on hightened positions ( steps etc.. )
    /// </summary>
    [RequireComponent(typeof(TPCharacter))]
    public class LegsIK : MonoBehaviour
    {
        /// <summary>
        /// enable / disable stepping on higher grounds
        /// </summary>
        [Tooltip("enable / disable stepping on higher grounds")]
        public bool EnableIKStepping = true;

        /// <summary>
        /// layers that interact with IK stepping
        /// </summary>
        [Tooltip ("Layers that interact with IK stepping")]
        public LayerMask IKSteppingLayers;

        /// <summary>
        /// Layers that interact with legs IK
        /// </summary>
        [Tooltip("Layers that interact with legs IK")]
        public LayerMask LegsIKLayers;

        /// <summary>
        /// max height for ik stepping
        /// </summary>
        [Tooltip("Max height for ik stepping")]
        public float MaxStepHeight = 0.5f;

        /// <summary>
        /// ray hit distance offset to ignore too close ik hits
        /// </summary>
        [Tooltip("Ray hit distance offset to ignore too close ik hits.")]
        public float ikDistanceOffset = -0.1f;

        /// <summary>
        /// foot IK offset
        /// </summary>
        [Tooltip("Foot IK offset")]
        public Vector3 footOffset = Vector3.zero;

        /// <summary>
        /// maximum ik detect slope
        /// </summary>
        [Tooltip("Maximum ik detect slope")]
        public float MaxSlopeAngle = 65.0f;

        /// <summary>
        /// bump jump value. Added to up velocity on hitting bump
        /// </summary>
        [Tooltip("Bump jump value. Added to up velocity on hitting bump.")]
        public float bumpJump = 0.05f;

        /// <summary>
        /// maximum height to enable normal legs IK
        /// </summary>
        [Tooltip ("Maximum height to enable normal legs IK.")]
        public float legsIKHeight = 0.75f;



        private TPCharacter m_Character;                                            // reference to character script
        private Animator m_Animator;                                                // reference to animator component
        private bool m_initialized = false;                                         // is class initialized ?
        private bool left_ikActive = false, right_ikActive = false;                 // left and right leg ik flag
        private float m_lweight = 0.0f, m_weightspeed = 12.0f, m_rweight = 0.0f;    // ik weights for smooth transition
        private Vector3 m_leftIKTarget, m_rightIKTarget;                            // current left / right ik target
        private Vector3 m_LeftHeightOffset, m_RightHeightOffset;                    // legs offsets
        private bool m_IkHit = false;                                               // are legs hit on something
        private float m_steptime = 0.0f;                                            // ik step timer
        private bool m_takingstep = false;                                          // does character taking ik stepping
        private const float m_stepmaxtime = 1.35f;                                  // ik stepping max time
        private Vector3 m_stepStartPosition, m_stepEndPosition;                     // helpers for position lerping
        private bool m_leftIK = true, m_rightIK = true;                             // do left / right ik flag
        private Vector3? m_CurPointL = null, m_CurPointR = null;                    // helper nullable points
        private const float  CAPSULE_HEIGHT = 0.07f;                                // height of caster capsule   
        private const float CAPSULE_RAD = 0.06f;                                    // radius of caster capsule    
        private const float BUMP_ANGLE_CHECK = 75f;                                 // above this is considered bump not slope
        private bool m_LeftSteppingForward = false;                                 // is left foot steping forward
        private bool m_RightSteppingForward = false;                                // is right foot steping forward
        private Vector3 m_PrevLeft, m_PrevRight ;                                   // previous feet positions
        private Transform m_LFoot, m_RFoot;

        /// <summary>
        /// initialize class
        /// </summary>
        /// <returns>true if succeded initialization</returns>
        public bool initialize()
        {
            if (m_initialized) return true;
            m_Character = GetComponent<TPCharacter>();
            if (!m_Character) { Debug.LogError("cannot find component 'TPCharacter'" + " < " + this.ToString() + ">"); return false; }
            m_Character.initialize();
            m_Animator = m_Character.animator;
            if (!m_Animator) { Debug.LogError("cannot find 'Animator' component" + " < " + this.ToString() + ">"); return false; }

            m_LFoot = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            if (!m_LFoot) { Debug.LogError("Cannot find left foot transform."); return false; }
            m_RFoot = m_Animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (!m_RFoot) { Debug.LogError("Cannot find right foot transform."); return false; }


            m_initialized = true;
            return true;
        }

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }

        // unity on animator ik
        void OnAnimatorIK(int layerIndex)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif



            //// doing it this way because must prevent any way of locking in m_takingstep mode
            //// so m_stepTime is always incrementing
            m_steptime += Time.deltaTime * 3.9f;
            if (m_steptime > m_stepmaxtime)
            {
                // reset from ik stepping
                if (m_takingstep)
                {
                    m_Character.disableGroundPull = false;
                    m_Character.capsule.isTrigger = false;
                    m_Character.rigidBody.isKinematic = false;
                    m_Character.rigidBody.useGravity = true;
                    m_Character.disableCapsuleScale = false;
                    m_Character.disableMove = false;
                    transform.position = m_stepEndPosition;

                    m_leftIK = true;
                    m_rightIK = true;
                    m_CurPointL = null;
                    m_CurPointR = null;
                }
                m_takingstep = false;

            }
            if (m_takingstep)
            {
                float lerpvalue = m_steptime / m_stepmaxtime;
                lerpvalue = lerpvalue * lerpvalue;
                transform.position = Vector3.Lerp(m_stepStartPosition, m_stepEndPosition, lerpvalue);
            }

            bool isBumpInFront = false;

            if (!m_takingstep)
            {

                bool isOnLayer = Utils.DoesMaskContainsLayer(IKSteppingLayers.value, m_Character.getCurrentGroundLayer());

                // if ik stepping enabled and character is grounded 
                // do ik stepping check
                if (EnableIKStepping && m_Character.isGroundMode && isOnLayer)
                {
                    // reset values
                    left_ikActive = false;
                    right_ikActive = false;
                    m_IkHit = false;
                    m_LeftHeightOffset = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
                    m_RightHeightOffset = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);

                    Vector3 currentDirection = m_Character.moveWS;
                    if (m_Character.moveWS == Vector3.zero) // character not moving
                    {
                        return;
                    }

                    // check if it is wall in front
                    bool isWallInFront = false;
                    float sphereChecksRadius = m_Character.capsule.radius * 0.95f;
                    Vector3 startPos = transform.position;
                    Ray ray = new Ray(startPos + Vector3.up * (MaxStepHeight + sphereChecksRadius * 0.51f), currentDirection);
                    float wall_dist_check = m_Character.capsule.radius * 1.25f;

                    // little up
                    ray.origin += new Vector3(0f, sphereChecksRadius * 0.525f, 0f);

#if DEBUG_INFO
                    WALL_COLOR = Color.blue;
                    BUMP_COLOR = Color.white;
                    WALL_START = ray.origin;
                    WALL_END = WALL_START + ray.direction * wall_dist_check;


#endif
                    RaycastHit hit;
                    float currAngle = 0.0f;
                    int mask = IKSteppingLayers.value;
                    if (Physics.SphereCast(ray, sphereChecksRadius, out hit,
                        wall_dist_check, mask))
                    {
#if DEBUG_INFO
                        WALL_COLOR = Color.red;
#endif
                        isWallInFront = true;
                        currAngle = Vector3.Angle(Vector3.up, hit.normal);
                    }

                    //// if wall in front do nothing and return
                    if (!isWallInFront)
                    {
                        // do legs ik
                        if (m_leftIK ) _leftLegIK(m_CurPointL);
                        if (m_rightIK ) _rightLegIK(m_CurPointR);

                        // check if there are bumps in front of feet and slope
                        float halfishHeight = MaxStepHeight * 0.4f;
                        ray = new Ray(startPos + Vector3.up * halfishHeight, currentDirection);
                        float bump_dist_check = wall_dist_check * 1.5f;

#if DEBUG_INFO
                        BUMP_START = ray.origin;
                        BUMP_END = BUMP_START + ray.direction * bump_dist_check;
#endif

                        bool bump_hit = Physics.SphereCast(ray, halfishHeight * 0.5f, out hit, bump_dist_check, mask);
                        if (bump_hit)
                        {
#if DEBUG_INFO
                            BUMP_COLOR = Color.red;
#endif
                            isBumpInFront = true;
                            currAngle = Vector3.Angle(Vector3.up, hit.normal);
                        }
                        bool bumpAngleTooLow = currAngle < MaxSlopeAngle;

                        m_Character.capsule.isTrigger = false;
                        if (isBumpInFront && !bumpAngleTooLow)
                        {
                            // if character is running just jump a little and return
                            if (m_Character.forwardAmount > 0.75f)
                            {
                                m_Character.rigidBody.velocity += m_Character.rigidBody.mass * Vector3.up * bumpJump;
                                return;
                            }

                            if ((m_IkHit))
                            {
                                // which is higher of left and right foot
                                Vector3 higher = m_LeftHeightOffset;
                                m_leftIK = true;
                                m_rightIK = false;

                                if (m_LeftHeightOffset.y < m_RightHeightOffset.y)
                                {
                                    higher = m_RightHeightOffset;
                                    m_leftIK = false;
                                    m_rightIK = true;
                                }

                                Vector3 higherLocalToXform = transform.InverseTransformPoint(higher);

                                // check of step is higher and in front of current direction
                                bool inFrontDirection = false;
                                Vector3 toHigher = higher - transform.position;
                                float dot = Vector3.Dot(currentDirection, toHigher);
                                if (dot > 0) // if in front
                                {
                                    inFrontDirection = higherLocalToXform.y > 0f; // higher than transform position
                                }

                                if (inFrontDirection)
                                {
                                    // additional checks
                                    if ((m_rightIK && m_RightSteppingForward ) ||
                                        (m_leftIK && m_LeftSteppingForward))
                                    {
                                        // ik stepping setup
                                        Vector3 m_FootOffset = higher - transform.position;

                                        m_stepStartPosition = transform.position;
                                        m_stepEndPosition = transform.position + m_FootOffset;

                                        m_steptime = 00.0f;
                                        m_takingstep = true;
                                        m_Character.disableGroundPull = true;
                                        m_Character.capsule.isTrigger = true;
                                        m_Character.rigidBody.isKinematic = true;
                                        m_Character.rigidBody.useGravity = false;
                                        m_Character.disableCapsuleScale = true;
                                        m_Character.disableMove = true;

                                        if (m_leftIK) { m_CurPointL = m_stepEndPosition; m_CurPointR = null; }
                                        if (m_rightIK) { m_CurPointL = null; m_CurPointR = m_stepEndPosition; }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    left_ikActive = false;
                    right_ikActive = false;
                    _leftLegIK();
                    _rightLegIK();
                }
            }



            // do animator ik
            if (layerIndex == 0)
            {

                if (left_ikActive)
                {
                    m_lweight += m_weightspeed * Time.deltaTime;
                    m_lweight = Mathf.Min(m_lweight, 1.0f);
                    m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_lweight);
                    m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, m_leftIKTarget);
                }
                else
                {
                    m_lweight -= m_weightspeed * Time.deltaTime;
                    m_lweight = Mathf.Max(0.0f, m_lweight);
                    m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_lweight);
                    m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, m_leftIKTarget);
                }
                if (right_ikActive)
                {
                    m_rweight += m_weightspeed * Time.deltaTime;
                    m_rweight = Mathf.Min(m_rweight, 1.0f);
                    m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_rweight);
                    m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, m_rightIKTarget);
                }
                else
                {
                    m_rweight -= m_weightspeed * Time.deltaTime;
                    m_rweight = Mathf.Max(0.0f, m_rweight);
                    m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_rweight);
                    m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, m_rightIKTarget);
                }
            } 
        }

#if DEBUG_INFO

        Vector3 WALL_START, WALL_END, BUMP_START, BUMP_END;
        Color WALL_COLOR, BUMP_COLOR;


        void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(m_rightIKTarget, 0.1f);
            Gizmos.DrawWireSphere(m_leftIKTarget, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(m_stepStartPosition, 0.12f);
            Gizmos.DrawWireSphere(m_stepEndPosition, 0.12f);
            Gizmos.DrawLine(m_stepStartPosition, m_stepEndPosition);

            Gizmos.color = WALL_COLOR;
            Gizmos.DrawLine(WALL_START, WALL_END);
            Gizmos.color = BUMP_COLOR;
            Gizmos.DrawLine(BUMP_START, BUMP_END);
        }
#endif

        /// <summary>
        /// left leg ik check
        /// </summary>
        /// <param name="point">current foot point</param>
        /// <returns>return true if ground hit found</returns>
        private bool _leftLegIK(Vector3? point = null)
        {
            if (point.HasValue)
            {
                m_leftIKTarget = point.Value;
                left_ikActive = true;
                return false;
            }

            Vector3 lfoot = m_Animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Transform lhip = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            RaycastHit rayhit;
            Vector3 difference = lfoot - lhip.position;
            float distance = difference.magnitude + ikDistanceOffset;
            Ray ray = new Ray(lhip.position, difference.normalized);
            Vector3 p0 = ray.origin;
            Vector3 p1 = ray.origin + lhip.up * CAPSULE_HEIGHT;
            int mask = m_Character.layers;  
            if (Physics.CapsuleCast(p0, p1, CAPSULE_RAD, ray.direction,
                out rayhit, distance - CAPSULE_RAD, mask, QueryTriggerInteraction.Ignore))
            {
                bool isOnIKFootLayer = Utils.DoesMaskContainsLayer(LegsIKLayers.value, rayhit.collider.gameObject.layer);
                if (!isOnIKFootLayer) return false;

                Vector3 target = rayhit.point + ray.direction * 0.02f;
                target = _additionalCheck(target, 1f, mask);
                target += footOffset;
                float y_offset = (target.y - transform.position.y);
                if (y_offset > legsIKHeight) return false;
                m_IkHit = true;
                m_LeftHeightOffset = target;
                m_leftIKTarget = target;
                left_ikActive = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// right leg ik check
        /// </summary>
        /// <param name="point">current foot point</param>
        /// <returns>returns true if ground hit found</returns>
        private bool _rightLegIK(Vector3? point = null)
        {
            if (point.HasValue)
            {
                m_rightIKTarget = point.Value;
                right_ikActive = true;
                return false;
            }

            Vector3 rfoot = m_Animator.GetIKPosition(AvatarIKGoal.RightFoot);
            Transform rhip = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            RaycastHit rayhit;
            Vector3 difference = rfoot - rhip.position;
            float distance = difference.magnitude + ikDistanceOffset;
            Ray ray = new Ray(rhip.position, difference.normalized);
            Vector3 p0 = ray.origin;
            Vector3 p1 = ray.origin + rhip.up * CAPSULE_HEIGHT;
            int mask = m_Character.layers;
            if (Physics.CapsuleCast(p0, p1, CAPSULE_RAD, ray.direction,
                out rayhit, distance - CAPSULE_RAD, mask, QueryTriggerInteraction.Ignore))
            {
                bool isOnIKFootLayer = Utils.DoesMaskContainsLayer(LegsIKLayers.value, rayhit.collider.gameObject.layer);
                if (!isOnIKFootLayer) return false;

                Vector3 target = rayhit.point + ray.direction * 0.02f;
                target = _additionalCheck(target, 1f, mask);
                target += footOffset;
                float y_offset = (target.y - transform.position.y);
                if (y_offset > legsIKHeight) return false;
                m_IkHit = true;
                m_RightHeightOffset = target;
                m_rightIKTarget = target;
                right_ikActive = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// additional raycast check to find foot orientation
        /// </summary>
        /// <param name="curPoint">current foot point</param>
        /// <param name="upDist">up offset</param>
        /// <param name="mask">layer mask</param>
        /// <returns>modified position</returns>
        private Vector3 _additionalCheck(Vector3 curPoint, float upDist, int mask)
        {
            Vector3 start = curPoint + Vector3.up * upDist;
            RaycastHit hit;
            Ray ray = new Ray(start, Vector3.down);
            if (Physics.Raycast(ray, out hit, upDist, mask))
            {
                return hit.point;
            }
            return curPoint;
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
#if DEBUG_INFO
            if(!m_initialized)
            {
                Debug.LogError("Component not initialized! <" + this.ToString () + ">");
                return;
            }
#endif

            Vector3 lloc = transform.InverseTransformPoint(m_LFoot.position);
            Vector3 rloc = transform.InverseTransformPoint(m_RFoot.position);

            m_LeftSteppingForward = false;
            m_RightSteppingForward = false;
            if (m_PrevLeft.z > lloc.z)
                m_LeftSteppingForward = true;
            if (m_PrevRight.z > rloc.z)
                m_RightSteppingForward = true;

            m_PrevLeft = lloc;
            m_PrevRight = rloc;
        }
    }
}
