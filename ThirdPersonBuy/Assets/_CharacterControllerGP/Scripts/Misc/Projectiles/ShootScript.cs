// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// Put this script on any object you want to shoot from.
    /// Point it towards direction to which you want to shoot.
    /// </summary>
    public class ShootScript : MonoBehaviour
    {
        /// <summary>
        /// current projectile prefab
        /// </summary>
        [Tooltip ("Shoot projectile prefab")]
        public Projectile ProjectilePrefab;

        /// <summary>
        /// fire position and direction transform 
        /// </summary>
        [Tooltip ("Projectile fire position and shoot direction.")]
        public Transform FireTransform;

        /// <summary>
        /// owner ( shooter ) of this script
        /// </summary>
        [Tooltip("Owner character of shooter script.")]
        public GameObject Owner;

        /// <summary>
        /// projectile hit strength
        /// </summary>
        public float hitStrength = 16.0f;

        /// <summary>
        /// current projectile
        /// </summary>
        [HideInInspector]
        public Projectile m_CurrentProj = null;        // current ball reference

        /// <summary>
        /// disable / enable shooting
        /// </summary>
        [HideInInspector]
        public bool m_DisableShooting = false;              // disable shooting flag  

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {

            if (!ProjectilePrefab) { Debug.LogError("projectile prefab not assigned." + " < " + this.ToString() + ">"); return; }
            if (!FireTransform) { Debug.LogError("fire transform not assign." + " < " + this.ToString() + ">"); return; }

        }

        /// <summary>
        /// create ball projectile function
        /// </summary>
        protected virtual void createBall()
        {
            m_CurrentProj = Projectile.CreateProjectile(ProjectilePrefab, FireTransform, Owner);
            if (!m_CurrentProj.initialize()) { Debug.LogError("cannot initialize projectile"); return; }

            m_CurrentProj.OnLifetimeExpire = Projectile.DestroyProjectile;
            m_CurrentProj.SphereCollider.isTrigger = false;
            m_CurrentProj.RigidBody.isKinematic = false;
            m_CurrentProj.RigidBody.detectCollisions = true;
            m_CurrentProj.setHitStrength(hitStrength);

            Projectile thisProj = m_CurrentProj;
            thisProj.setup();
        }

        /// <summary>
        /// scale current ball
        /// </summary>
        protected virtual void scaleBall()
        {
            if (!m_CurrentProj) { return; }
            if (!(m_CurrentProj is InflatableBall)) return;
            (m_CurrentProj as InflatableBall).inflate();
        }

        /// <summary>
        /// shoot current ball
        /// </summary>
        protected virtual void fireBall()
        {
            if (!m_CurrentProj) { return; }

            float v = (m_CurrentProj.speed / m_CurrentProj.mass) ;
            Vector3 force = FireTransform.forward * v;
            m_CurrentProj.RigidBody.velocity = force;
            
            m_CurrentProj.transform.position = FireTransform.position;
            m_CurrentProj.State = Projectile.ProjectileStates.Fired;
            m_CurrentProj.OnAfterHit = ()=>
            {
                Projectile.DestroyProjectile(m_CurrentProj);
            };
            m_CurrentProj.OnCollisionEnterEvent = () =>
             {
                 Projectile.DestroyProjectile(m_CurrentProj);
             };
        }
    } 

}
