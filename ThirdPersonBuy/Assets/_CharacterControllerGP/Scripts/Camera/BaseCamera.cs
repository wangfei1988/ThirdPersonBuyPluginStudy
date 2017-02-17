// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// base camera class 
    /// </summary>
    public abstract class BaseCamera : MonoBehaviour
    {
        private Camera m_CameraComponent;   // reference to camera component

        /// <summary>
        /// gets camera component
        /// </summary>
        public Camera cameraComponent { get { return m_CameraComponent; } }

        /// <summary>
        /// default camera target transform 
        /// </summary>
        [Tooltip("Target that camera will look at")]
        public Transform Target;

        /// <summary>
        /// gets and sets adding player rotation with input rotation
        /// </summary>
        public abstract bool additiveRotation { get; set; }

        /// <summary>
        /// initialize component
        /// </summary>
        public virtual void initialize()
        {
            m_CameraComponent = GetComponent<Camera>();
            if (!m_CameraComponent) Debug.LogError("Cannot find 'Camera' component." + " < " + this.ToString() + ">");
        }

        /// <summary>
        /// switch camera targets
        /// </summary>
        /// <param name="newTarget">new look at target</param>
        /// <param name="speed">speed of transition</param>
        public abstract void switchTargets(Transform newTarget, float speed = 1.0f);
    } 
}
