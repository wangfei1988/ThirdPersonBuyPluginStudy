// © 2016 Mario Lelas
using UnityEngine;
using System;

namespace MLSpace
{
    /// <summary>
    /// Camera used for top down view system
    /// </summary>
    public class TopDownCamera : BaseCamera
    {
        /// <summary>
        /// current camera zoom distance from target
        /// </summary>
        [Tooltip ("Current camera zoom distance from target.")]
        public float currentZoom = 5.0f;                // current zoom

        /// <summary>
        /// minimum allowed camera zoom
        /// </summary>
        [Tooltip ("Minimum allowed camera zoom.")]
        public float minZoom = 2.5f;

        /// <summary>
        /// maximum allowed camera zoom
        /// </summary>
        [Tooltip("Maximum allowed camera zoom.")]
        public float maxZoom = 12.0f;

        /// <summary>
        /// zoom step
        /// </summary>
        [Tooltip ("Zoom step.")]
        public float zoomStep = 0.5f;



        private Vector3 m_CamOffset = Vector3.zero; // start camera offset
        private float y_val = 0.0f;
        private bool m_initialized = false;

        private Transform m_currentTransform;   // current target transform
        private Vector3 m_CurrentTargetPos;     // camera target position
        private float m_switchSpeed = 1f;       // camera target switch speed
        private float m_lerpTime = 0.0f;        // camera target switch current time
        private float m_lerpMaxTime = 0.5f;     // camera target switch max time
        private Transform m_oldTransform;       // old target transform for use when switching targets ( lerp)
        private bool m_switchingTargets;        // switch target flag

        /// <summary>
        /// NOT IMPLEMENTED IN TOP DOWN CAMERA !!!
        /// </summary>
        public override bool additiveRotation
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
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
            if (Target == null) { Debug.LogError("Camera target not assigned!" + " < " + this.ToString() + ">"); return; }
#endif

            m_currentTransform = Target;
            m_CurrentTargetPos = Target.position;
            m_CamOffset = transform.position - Target.position;

            m_initialized = true;
        }

        /// <summary>
        /// unity Awake method
        ///  is called when the script instance is being loaded
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
            float h = Input.GetAxis("Horizontal");
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                currentZoom = currentZoom + zoomStep;
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                currentZoom = currentZoom - zoomStep;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            y_val -= h;


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

            Vector3 rotatedOffset = m_CamOffset;
            Quaternion rq = Quaternion.Euler(0.0f, y_val, 0.0f); 
            rotatedOffset = rq * m_CamOffset;
            Vector3 newCamPosition = m_CurrentTargetPos + rotatedOffset.normalized * currentZoom;
            transform.position = newCamPosition;
            Vector3 direction = m_CurrentTargetPos - transform.position;
            Quaternion towardsPlayer = Quaternion.LookRotation(direction.normalized);
            transform.rotation = towardsPlayer;
        }

        /// <summary>
        /// switch camera target 
        /// </summary>
        /// <param name="newTarget">new 'look at' target </param>
        /// <param name="speed">transition speed from current to new target</param>
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
        }
    }
}