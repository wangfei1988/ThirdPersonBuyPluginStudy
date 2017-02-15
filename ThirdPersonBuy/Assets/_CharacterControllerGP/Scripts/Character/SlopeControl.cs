// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{

    /// <summary>
    /// Class that controls player slope moving
    /// Choose min / max slope
    /// Script will slow / disable player movement on approaching max slope value
    /// Also disable jump if enabled
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TPCharacter))]
    public class SlopeControl : MonoBehaviour
    {
        /// <summary>
        /// choose max traversable slope
        /// </summary>
        [Range ( 30,75)]
        [Tooltip("Choose max traversable slope.")]
        public float MaxSlope = 60f;

        /// <summary>
        /// choose layer/s to interact with slope script
        /// </summary>
        [Tooltip("Choose layer/s to interact with slope script.")]
        public LayerMask layers;

        /// <summary>
        /// down force on high slopes
        /// </summary>
        [Tooltip("Down force on high slopes.")]
        public float downVelocityStrength = 0.5f;

        /// <summary>
        /// use spherecast or raycast for checking ground slopes
        /// </summary>
        [Tooltip ("Use spherecast or raycast for checking ground slopes.")]
        public bool sphereCheck = false;

        /// <summary>
        /// disable jump on max slope and higher
        /// </summary>
        [Tooltip ("Disable jump on MaxSlope and higher.")]
        public bool disableJumpOnHighSlope = false;

        private TPCharacter m_Character;    // reference to tp character script
        private Predefines m_defines;
        private Vector3 m_GroundNormal = Vector3.up;    // current ground normal
        private bool m_Initialized = false;         // is compoentn initialized ?


        /// <summary>
        /// initialize component
        /// </summary>
        public void initialize()
        {
            if (m_Initialized) return;
            m_Character = GetComponent<TPCharacter>();
            if(!m_Character) { Debug.LogError("Cannot find component 'TPCharacter'! " + " < " + this.ToString() + ">"); return; }
            m_Character.initialize();
            m_defines = GameObject.FindObjectOfType<Predefines>();
            if(!m_defines) { Debug.LogError("Cannot find component 'Predefines'." + " < " + this.ToString() + ">");return; }
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
            _slopeControl();
        }


        /// <summary>
        /// slope control function
        /// </summary>
        private void _slopeControl()
        {
            m_Character.slopeMultiplier = 1.0f;
            m_Character.jumpAllowed = true;

            bool isOnLayer = Utils.DoesMaskContainsLayer(layers.value, m_Character.getCurrentGroundLayer());
            if (!isOnLayer) return;
            if (m_Character.distanceFromGround > 0.1f) return;




            m_GroundNormal = m_Character.groundNormal; // character is checking ground normal already


            Vector3 thisPos = transform.position;
            Vector3 moveDir = m_Character.moveWS;
            if (moveDir == Vector3.zero)
                moveDir = m_Character.transform.forward;
            Vector3 nextPos = thisPos + moveDir.normalized * (m_Character.capsule.radius * 0.99f); 
            bool goingUp = _isGoingUp(thisPos, nextPos);

            if (goingUp)
            {
                float slopeStart = Mathf.Max(MaxSlope - 10.0f, 0.0f);
                float angle = Vector3.Angle(m_GroundNormal, Vector3.up);
                float amt0 = 0.0f;
                float angDiff = angle - slopeStart;
                amt0 = angDiff / (MaxSlope - slopeStart);
                amt0 = Mathf.Max(0.0f, amt0);
                float velocityMultiplier = Mathf.Lerp(1.0f, 0.5f, amt0);
                float downForce = Mathf.Lerp(0.0f, downVelocityStrength, amt0);
                Vector3 down = Vector3.down * downForce;

                if (angle > MaxSlope)
                {
                    m_Character.capsule.material = m_defines.zeroFrictionMaterial;
                    m_Character.slopeMultiplier *= Mathf.Max(0.5f, velocityMultiplier);
                    if (m_Character .isDiving)
                    {
                        m_Character.simulateRootMotion = false;
                    }

                    if (disableJumpOnHighSlope)
                    {
                        m_Character.jumpAllowed = false;
                        Vector3 velocity = m_Character.rigidBody.velocity;
                        velocity *= velocityMultiplier;
                        m_Character.rigidBody.velocity = velocity + down;
                    }
                    else
                    {
                        Vector3 velocity = m_Character.rigidBody.velocity;
                        velocity.x *= velocityMultiplier;
                        velocity.z *= velocityMultiplier;
                        m_Character.rigidBody.velocity = velocity + down;
                    }
                }
                else if (angle > slopeStart)
                {
                    m_Character.capsule.material = m_defines.zeroFrictionMaterial;
                    m_Character.slopeMultiplier *= Mathf.Max(0.5f, velocityMultiplier);
                    Vector3 velocity = m_Character.rigidBody.velocity;
                    velocity.x *= velocityMultiplier;
                    velocity.z *= velocityMultiplier;
                    m_Character.rigidBody.velocity = velocity + down;

                }
            }
        }

        /// <summary>
        /// check height at given position
        /// </summary>
        /// <param name="position">current position</param>
        /// <param name="newGroundPos">out new position on ground</param>
        /// <param name="normal">out ground normal</param>
        /// <returns>returns distance from ground</returns>
        private float _checkPositionHeight(Vector3 position, out Vector3 newGroundPos,out Vector3 normal)
        {
            float maxCheckValue = 2.0f;
            RaycastHit hitInfo;
            newGroundPos = position;
            normal = Vector3.up;
            float distFromGround = -1f;

            float yOffset = 0.5f;

            Vector3 raycastPos = position + Vector3.up * yOffset; // little above
            Ray gRay = new Ray(raycastPos, Vector3.down);

            if (sphereCheck)
            {
                if (Physics.SphereCast(gRay, m_Character.capsule.radius * 0.5f, out hitInfo, maxCheckValue, layers))
                {
                    distFromGround = hitInfo.distance - yOffset;
                    newGroundPos = hitInfo.point;
                    normal = hitInfo.normal;
                }
            }
            else
            {
                if (Physics.Raycast(gRay, out hitInfo, maxCheckValue, layers))
                {
                    distFromGround = hitInfo.distance - yOffset;
                    newGroundPos = hitInfo.point;
                    normal = hitInfo.normal;
                }
            }
            return distFromGround;
        }

        /// <summary>
        /// is player going up slope ?
        /// </summary>
        /// <param name="thisPos">current position</param>
        /// <param name="nextPos">next position</param>
        /// <returns>returns true if character is going up slope otherwise false</returns>
        private bool _isGoingUp(Vector3 thisPos,Vector3 nextPos)
        {
            Vector3 thisHitPos, thisNorm;
            Vector3 nextHitPos, nextNorm;
            float h0 = _checkPositionHeight(thisPos, out thisHitPos, out thisNorm);
            float h1 = _checkPositionHeight(nextPos, out nextHitPos, out nextNorm);
            float TOLERANCE = float.Epsilon; 
            float Ydifference = h1 - h0;
            bool slopeUp = (Ydifference + TOLERANCE) < 0;
            return slopeUp;

        }

        /// <summary>
        /// check ground normal
        /// </summary>
        private void _checkGroundNormal()
        {
            Vector3 startPos = transform.position + Vector3.up;
            int mask = layers;
            RaycastHit rhit;
            Ray ray = new Ray(startPos, Vector3.down);
            if (Physics.Raycast(ray, out rhit, float.MaxValue, mask))
            {
                m_GroundNormal = rhit.normal;
            }
        }

        /// <summary>
        /// animation event called upon dive roll end time
        /// </summary>
        /// <param name="e">AnimationEvent info class</param>
        private void OnDiveRollEndEvent(AnimationEvent e)
        {
            if (!enabled) return;
            m_Character.simulateRootMotion = true;
        }

        
    } 
}
