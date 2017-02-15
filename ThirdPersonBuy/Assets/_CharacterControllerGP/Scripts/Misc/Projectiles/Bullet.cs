// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// projectile that shoots and flies forward checking for hits
    /// </summary>
    public class Bullet : Projectile
    {
        /// <summary>
        /// sphere collider that will be used to test for collisions
        /// </summary>
        [Tooltip("Sphere collider that will be used to test for collisions.")]
        public SphereCollider SpheretestCollider;

        /// <summary>
        /// initialize component
        /// </summary>
        /// <returns>is initialization success</returns>
        public override bool initialize()
        {
            
            if (!SpheretestCollider) { Debug.LogError("SpheretestCollider component missing." + " < " + this.ToString() + ">"); return false; }
            m_SphereCollider = SpheretestCollider;


            m_Rigidbody = GetComponent<Rigidbody>();
            if (!m_Rigidbody) { Debug.LogError("cannot find 'Rigidbody' component." + " < " + this.ToString() + ">"); return false; }

            m_HitInfo = new HitInformation();

            return true;
        }

    } 


}
