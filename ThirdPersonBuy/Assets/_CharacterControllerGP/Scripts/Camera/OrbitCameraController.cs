// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// Camera Controller
    /// </summary>
    public class OrbitCameraController : BaseCamera
    {

        /// <summary>
        /// types of constraints
        /// </summary>
        public enum CameraConstraint { Free, Limited };

        /// <summary>
        /// constraint rotation around x axis
        /// </summary>
        [Tooltip("Constrain rotation around X axis.")]
        public CameraConstraint Xconstraint = CameraConstraint.Limited;

        /// <summary>
        /// constraint rotation around y axis
        /// </summary>
        [Tooltip("Constain rotation around Y axis.")]
        public CameraConstraint Yconstraint = CameraConstraint.Free; 


        /// <summary>
        /// camera rotation speed
        /// </summary>
        public float angularSpeed = 64f;

        /// <summary>
        /// camera min rotation on x axis
        /// </summary>
        [HideInInspector]
        public float minXAngle = -60;

        /// <summary>
        /// camera max rotation on x axis
        /// </summary>
        public float maxXAngle = 45;

        /// <summary>
        /// camera min rotation on y axis
        /// </summary>
        [HideInInspector]
        public float minYAngle = -180;

        /// <summary>
        /// camera max rotation on y axis
        /// </summary>
        [HideInInspector]
        public float maxYAngle = 180;

        /// <summary>
        /// camera min zoom distance
        /// </summary>
        public float minZ = 1;

        /// <summary>
        /// camera max zoom distance
        /// </summary>
        public float maxZ = 10;

        /// <summary>
        /// camera zoom step
        /// </summary>
        public float zStep = 0.5f;

        private ProtectFromWalls m_ProtectFromWalls;    // refernce to protect from walls script
        private Transform m_currentTransform;   // current target transform
        private float m_totalXAngleDeg = 0;     // current x angle in degrees
        private float m_totalYAngleDeg = 0;     // current y angle in degrees
        private float m_currentZ;               // camera current zoom
        private Vector3 m_CurrentTargetPos;     // camera target position
        private Vector3 m_offsetPosition;       // camera offset from target
        private Vector3 m_startingPosition;     // camera start offset from target
        private float m_switchSpeed = 1f;       // camera target switch speed
        private float m_lerpTime = 0.0f;        // camera target switch current time
        private float m_lerpMaxTime = 0.5f;     // camera target switch max time
        private Transform m_oldTransform;       // old target transform for use when switching targets ( lerp)
        private bool m_switchingTargets;        // switch target flag
        private bool m_disableInput = false;    // disabling input flag
        private bool m_initialized = false;     // is component initialized ?
        private Quaternion m_StartRotationY = Quaternion.identity;  // start rotation y axis
        private bool m_AdditiveRot = false;   // additive rotation in ledge mode

        /// <summary>
        /// disables camera control input
        /// </summary>
        public bool disableInput { get { return m_disableInput; } set { m_disableInput = value; } }

        /// <summary>
        /// gets current target transform.
        /// </summary>
        public Transform targetTransform { get { return m_currentTransform; } }

        /// <summary>
        /// gets and sets adding player rotation with input rotation
        /// </summary>
        public override bool additiveRotation
        {
            get { return m_AdditiveRot; }
            set
            {
                m_AdditiveRot = value;
                if (m_AdditiveRot)
                {
                    Vector3 e = (transform.localRotation) .eulerAngles ;
                    m_totalXAngleDeg = e.x;
                    m_totalYAngleDeg = e.y;
                }
                else
                {
                    Vector3 e = transform.rotation.eulerAngles;
                    m_totalXAngleDeg = e.x;
                    m_totalYAngleDeg = e.y;
                }
            }
        }

        /// <summary>
        /// initialize camera class
        /// </summary>
        public override void initialize()
        {
            if (m_initialized) return;

            base.initialize();

#if DEBUG_INFO
            if (Target == null) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return; }
#endif

            m_ProtectFromWalls = GetComponent<ProtectFromWalls>();
            if (!m_ProtectFromWalls)
                Debug.LogWarning("cannot find 'ProtectFromWalls' script");

            m_currentTransform = Target;
            m_CurrentTargetPos = Target.position;
            m_offsetPosition = transform.position - m_CurrentTargetPos;
            m_startingPosition = m_offsetPosition;

            Vector3 euler = transform.rotation.eulerAngles;
            m_StartRotationY = Quaternion.Euler(0.0f, euler.y, 0.0f);

            m_initialized = true;
        }



 #region UNITY METHODS

        /// <summary>
        /// unity Awake method
        /// is called when the script instance is being loaded
        /// </summary>
        void Awake()
        {
            initialize();
        }

        /// <summary>
        /// Unity LateUpdate method
        /// is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
#if DEBUG_INFO
            if (!m_initialized)
            {

                Debug.LogError("component not initialized." + " < " + this.ToString() + ">");
                return;
            }

            if (m_currentTransform == null) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return; }

#endif
            if (m_disableInput) return;


            m_CurrentTargetPos = m_currentTransform.position;

            if (m_switchingTargets)
            {
                m_lerpTime += Time.deltaTime * m_switchSpeed;

                if (m_lerpTime > m_lerpMaxTime)
                {
                    m_CurrentTargetPos = m_currentTransform.position;
                    m_switchingTargets = false;
                }
                else
                {
#if DEBUG_INFO
                    if (m_oldTransform == null) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return; }
#endif
                    float val = m_lerpTime / m_lerpMaxTime;
                    m_CurrentTargetPos = Vector3.Lerp(m_oldTransform.position, m_currentTransform.position, val);
                }
            }

            // inputs
            float angleAroundY = Input.GetAxisRaw("Mouse X");
            float angleAroundX = -Input.GetAxisRaw("Mouse Y");
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                m_currentZ = m_currentZ + zStep;
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                m_currentZ = m_currentZ - zStep;




            float currentAngleY = angleAroundY * Time.deltaTime * angularSpeed;
            float currentAngleX = angleAroundX * Time.deltaTime * angularSpeed;

            m_totalXAngleDeg += currentAngleX ;
            m_totalYAngleDeg += currentAngleY ;

            if(Xconstraint == CameraConstraint.Limited) m_totalXAngleDeg = Mathf.Clamp(m_totalXAngleDeg, minXAngle, maxXAngle);
            if (Yconstraint == CameraConstraint.Limited) m_totalYAngleDeg = Mathf.Clamp(m_totalYAngleDeg, minYAngle, maxYAngle);


            Quaternion rotation =
                Quaternion.Euler
                (
                    m_totalXAngleDeg,
                    m_totalYAngleDeg,
                    0
                );
            rotation = rotation * m_StartRotationY;
            if (m_AdditiveRot)
            {
                rotation = transform.parent.rotation * rotation;
            }
            m_offsetPosition = rotation * m_startingPosition;


            m_currentZ = Mathf.Clamp(m_currentZ, minZ, maxZ);
            transform.position  = m_CurrentTargetPos + (m_offsetPosition * m_currentZ);


            transform.LookAt(m_CurrentTargetPos);
        }

#endregion

        /// <summary>
        /// switch camera look at target 
        /// </summary>
        /// <param name="newTarget">new 'look at' target</param>
        /// <param name="speed">speed of transition from current to new target</param>
        public override void switchTargets(Transform newTarget, float speed = 1.0f)
        {
#if DEBUG_INFO
            if (!m_initialized)
            {
                Debug.LogError("component not initialized." + " < " + this.ToString() + ">");
                return;
            }

            if (newTarget == null) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return; }
            if (m_currentTransform == null) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return; }
#endif

            if (newTarget == m_currentTransform) return;
            m_switchingTargets = true;
            m_oldTransform = m_currentTransform;
            m_currentTransform = newTarget;
            m_lerpTime = 0.0f;
            m_switchSpeed = speed;

            if(m_ProtectFromWalls)
            {
                m_ProtectFromWalls.m_Pivot = newTarget;
            }
        }
    }
    
}