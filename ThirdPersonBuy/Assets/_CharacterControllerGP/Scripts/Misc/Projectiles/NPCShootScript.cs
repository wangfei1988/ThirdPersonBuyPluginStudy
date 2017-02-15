// © 2016 Mario Lelas

//#define SHOOT_ONLY_IF_VISIBLE

using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// Put this script on any object you want to shoot from.
    /// Point it towards direction to which you want to shoot.
    /// </summary>
    public class NPCShootScript : ShootScript
    {
        /// <summary>
        /// modes for shooting ball
        /// shooting only if visible ( not ideal, but enough for this project )
        /// comment SHOOT_ONLY_IF_VISIBLE to shoot always
        /// </summary>
        private enum ScaleBallModes { None, CreateBallProjectile, Scale, Release };

        /// <summary>
        /// transform that will be shot at
        /// </summary>
        [Tooltip("Player transform that will be shot at.")]
        public Transform playerTransform;


        /// <summary>
        /// rate of fire
        /// </summary>
        [Tooltip("Rate of fire.")]
        public float shootInterval = 4.0f;

        /// <summary>
        /// shoot range
        /// </summary>
        [Tooltip("Range of shooter.Will stop shooting if player is out of range.")]
        public float shoot_range = 20.0f;

        /// <summary>
        ///  field of view
        /// </summary>
        [Tooltip("Field of view angle of shooter.")]
        public float shoot_angle = 45.0f;


        private float m_CurrentShootTime = 0.0f;            // current shoot time
        private float m_CurrentShootScaleTarget = 1.0f;     // current projectile ball scale target
        private ScaleBallModes m_ScaleBallMode =
                ScaleBallModes.None;                        // current state when shooting scale balls

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            if (!playerTransform)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (!player) { Debug.LogWarning("Cannot find object with tag 'Player'"); return; }
                playerTransform = player.transform;
            }
            if (!playerTransform)
            {
                Debug.LogWarning("Cannot find object player.");
            }
        }


#if SHOOT_ONLY_IF_VISIBLE
        // unity on will render method
        void OnWillRenderObject()
        {
            _shoot();
        }
#else
        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
            if (playerTransform)
            {
                // is player in front and in range of shooter
                Vector3 dirToPlayer = playerTransform.position - FireTransform.position;
                float distToPlayer = dirToPlayer.magnitude;
                dirToPlayer.Normalize();
                float angle = Vector3.Angle(dirToPlayer, FireTransform.forward);
                bool isInFront = angle < shoot_angle;
                bool inRange = distToPlayer < shoot_range;

                if (isInFront && inRange)
                {
                    _shoot();
                }
            }
            else
            {
                _shoot();
            }
        }
#endif

        /// <summary>
        /// shoot method
        /// </summary>
        protected void _shoot()
        {
#if DEBUG_INFO
            if (!ProjectilePrefab)
            {
                Debug.LogError("ProjectilePrefab cannot be null." + " < " + this.ToString() + ">");
                return;
            }
#endif

            if (m_DisableShooting)
            {
#if DEBUG_INFO
                Debug.LogWarning("WallShooter disabled: " + this.name);
#endif
                return;
            }

            // shooting procedure
            m_CurrentShootTime += Time.deltaTime;
            if (m_CurrentShootTime >= shootInterval)
            {
                m_CurrentShootScaleTarget = Random.Range(InflatableBall.MinScale, InflatableBall.MaxScale);
                m_ScaleBallMode = ScaleBallModes.CreateBallProjectile;
                m_CurrentShootTime = 0.0f;
            }

            if (ProjectilePrefab is InflatableBall)
            {
                if (m_ScaleBallMode == ScaleBallModes.CreateBallProjectile)
                {
                    createBall();
                }
                else if (m_ScaleBallMode == ScaleBallModes.Scale)
                {
                    float currentBallScale = (m_CurrentProj as InflatableBall).currentBallScale;

                    scaleBall();

                    if (currentBallScale >= m_CurrentShootScaleTarget)
                        m_ScaleBallMode = ScaleBallModes.Release;
                }
                else if (m_ScaleBallMode == ScaleBallModes.Release)
                {
                    fireBall();
                }
            }
            else
            {
                if (m_ScaleBallMode == ScaleBallModes.CreateBallProjectile)
                {
                    createBall();
                    fireBall();
                }
            }
            if (m_CurrentProj)
            {
                if (m_CurrentProj.State == Projectile.ProjectileStates.Ready)
                    m_CurrentProj.transform.position = FireTransform.position;
            }
        }

        /// <summary>
        /// create ball method
        /// </summary>
        protected override void createBall()
        {
            m_ScaleBallMode = ScaleBallModes.Scale;
            base.createBall();
        }

        /// <summary>
        /// scale ball if it is inflatable ball
        /// </summary>
        protected override void scaleBall()
        {
            if (!m_CurrentProj) { m_ScaleBallMode = ScaleBallModes.None; return; }
            base.scaleBall();
        }

        /// <summary>
        /// fire ball
        /// </summary>
        protected override void fireBall()
        {
            if (!m_CurrentProj) { m_ScaleBallMode = ScaleBallModes.None; return; }

            base.fireBall();
            m_ScaleBallMode = ScaleBallModes.None;
        }
    } 
}
