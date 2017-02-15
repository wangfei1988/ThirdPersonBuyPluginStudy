// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// ball projectile that can inflate and shoot. 
    /// Hit strength modified by size of ball.
    /// </summary>
    public class InflatableBall : Projectile
    {
        /// <summary>
        /// start scale of projectile ball
        /// </summary>
        [Tooltip("Starting scale of projectile ball.")]
        public static float MinScale = 0.15f;

        /// <summary>
        /// max scale of projectile ball
        /// </summary>
        [Tooltip("Maximum scale of projectile ball.")]
        public static float MaxScale = 1.0f;

        /// <summary>
        /// current ball scale
        /// </summary>
        private float m_CurrentBallScale = 0.1f;    

        /// <summary>
        /// gets and sets ball scale
        /// </summary>
        public float currentBallScale { get { return m_CurrentBallScale; } set { m_CurrentBallScale = value; } }

        /// <summary>
        /// inflate ball method
        /// </summary>
        /// <param name="inflateValue">inflate add value</param>
        public void inflate(float inflateValue = 0.01f)
        {
            m_CurrentBallScale += inflateValue;
            m_CurrentBallScale = Mathf.Min(m_CurrentBallScale, InflatableBall.MaxScale);
            transform.localScale = Vector3.one * m_CurrentBallScale;
            CurrentHitStrength = mass * m_CurrentBallScale;
        }
    } 
}
