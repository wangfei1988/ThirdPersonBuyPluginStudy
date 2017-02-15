// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// controls camera, head ik, pathfinding
    /// derived from 'Player' class
    /// </summary>
    public class PlayerTopDown : Player
    {
        /// <summary>
        /// walkable layer mask
        /// </summary>
        public LayerMask walkableTarrainMask;
        private UnityEngine.AI.NavMeshPath m_path;                                     // nav mesh path info class
        private Vector3 m_Direction2destination = Vector3.zero;         // current direction to target
        private float m_Distance2destination = 0.0f;                    // current distance to target
        private float m_DestinationReachDistance = 0.5f;                // distance from detination considered to be arrived
        private bool m_OnDestination = false;                           // is arrived on destination                  
        private Vector3 m_CurrentDest = Vector3.zero;                   // current destination

        /// <summary>
        /// gets and sets destination reach distance
        /// </summary>
        public float destinationReachDistance { get { return m_DestinationReachDistance; } set { m_DestinationReachDistance = value; } }

        /// <summary>
        /// gets and sets current destination
        /// </summary>
        public Vector3 currentDestination { get { return m_CurrentDest; }set { m_CurrentDest = value; } }

        /// <summary>
        /// gets current direction to destination ( calculated )
        /// </summary>
        public Vector3 direction2Target
        {
            get
            {
                Vector3 direction2target = m_CurrentDest - transform.position;
                direction2target.y = 0.0f;
                direction2target.Normalize();
                return direction2target;
            }
        }

        /// <summary>
        /// gets and sets move flag
        /// </summary>
        public bool enableMove { get; set; }

        /// <summary>
        /// initialize component
        /// </summary>
        public override void initialize()
        {
            base.initialize();
            m_path = new UnityEngine.AI.NavMeshPath();
            m_CurrentDest = transform.position;
            enableMove = true;

            // no looking to camera direction
            m_Character.setIKMode(TPCharacter.IKMode.None);
            lookTowardsCamera = false;
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
        /// stop player
        /// </summary>
        public void stop()
        {
            m_OnDestination = true;
            m_CurrentDest = transform.position;
            m_Distance2destination = 0.0f;
        }

        /// <summary>
        /// Control player
        /// </summary>
        /// <param name="horiz">horizonal axis value</param>
        /// <param name="vert">vertical axis value</param>
        /// <param name="jump">jump flag</param>
        /// <param name="walkToggle">toggle walk / run mode</param>
        /// <param name="dive">dive roll flag</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="bodyLookDirection">body look direction</param>
        /// <param name="diveDirection">dive roll direction</param>
        public override void control(float horiz, float vert, bool jump, bool runToggle, bool dive, bool crouch, 
            Vector3? bodyLookDirection = null, Vector3? diveDirection = null, float? side = null)
        {
            m_Direction2destination = m_CurrentDest - transform.position;
            m_Distance2destination = m_Direction2destination.magnitude;
            m_Direction2destination.y = 0.0f;
            m_Direction2destination.Normalize();
            m_OnDestination = m_Distance2destination < m_DestinationReachDistance;

            // MOVE SECTION
            Vector3 move = Vector3.zero;
            if (enableMove)
                _calculate_path(ref move);
            else
            {
                m_OnDestination = true;
            }
            if (move.magnitude > 1f) move.Normalize();
            float walkMultiplier = runToggle ? 1f : 0.5f;
            move *= walkMultiplier;

            base.control(move, jump, runToggle, dive, crouch, bodyLookDirection, diveDirection, side);
        }

        /// <summary>
        /// calculate path
        /// </summary>
        /// <param name="moveDirection">move velocity</param>
        /// <returns>returns true if path found</returns>
        private bool _calculate_path(ref Vector3 moveVelocity)
        {
#if DEBUG_INFO
            if(m_path == null)
            {
                Debug.LogError("object cannot be null: "+ " < " + this.ToString() + ">");
                moveVelocity = Vector3.zero;
                m_OnDestination = true;
                return false;
            }
#endif
            if (m_OnDestination) return false;
            if (m_Distance2destination <= m_DestinationReachDistance)
            {
                moveVelocity = Vector3.zero;
                m_OnDestination = true;
                return true;
            }

            //int area = 1 << NavMesh.GetAreaFromName("Walkable");
            if (!UnityEngine.AI.NavMesh.CalculatePath(transform.position, m_CurrentDest, UnityEngine.AI.NavMesh.AllAreas , m_path))
            {
//#if DEBUG_INFO
//                Debug.LogWarning("Calculate path failed. " + this.name);
//#endif
                m_OnDestination = false;
                return false;
            }
            if(m_path .status != UnityEngine.AI.NavMeshPathStatus.PathComplete )
            {
                return false;
            }
            if (m_path.corners.Length < 2)
            {
                m_OnDestination = false;
                return false;
            }

            Vector3 toTarget = m_path.corners[1] - transform.position;
            moveVelocity = toTarget.normalized;
            m_OnDestination = m_Distance2destination < m_DestinationReachDistance;
            return true;
        }
    } 
}
