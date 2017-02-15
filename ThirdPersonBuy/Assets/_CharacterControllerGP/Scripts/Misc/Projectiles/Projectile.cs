// © 2016 Mario Lelas
using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{
    /// <summary>
    /// only spheremarch for this project is enough
    /// </summary>
    public enum ProjectileMode { /*Raycast, Raymarch, Spherecast,*/ Spheremarch };

    /// <summary>
    /// void delegate that takes Projectile class as parameter
    /// </summary>
    /// <param name="thisBall"></param>
    public delegate void ProjectileFunc(Projectile thisBall);

    /// <summary>
    /// base class for projectiles
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        /// <summary>
        /// Information on hit object 
        /// </summary>
        public struct HitInformation
        {
            public GameObject hitObject;
            public Collider collider;
            public int[] bodyPartIndices;
            public Vector3 hitDirection;
            public float hitStrength;
        }


        /// <summary>
        /// projectile ball counter
        /// </summary>
        public static int ProjectileCount = 0;

        /// <summary>
        /// possible ball states enumeration
        /// </summary>
        public enum ProjectileStates { Ready, Fired, Done };

        /// <summary>
        /// lifetime of projectile
        /// </summary>
        [Tooltip("Lifetime of fire ball projectile.")]
        public float lifetime = 12.0f;

        /// <summary>
        /// hit strength of fire ball projectile
        /// </summary>
        [Tooltip("Speed of fire ball projetile.")]
        public float speed = 25.0f;

        /// <summary>
        /// mass of projectile
        /// </summary>
        [Tooltip("Mass of projectile.")]
        public float mass = 1.0f;

        /// <summary>
        /// current projectile mode
        /// </summary>
        public ProjectileMode projectileMode = ProjectileMode.Spheremarch;

        /// <summary>
        /// colliding layers of fire ball. Use 'ColliderLayer'
        /// </summary>
        [Tooltip("colliding layers of fire ball.")]
        public LayerMask collidingLayers;

        protected ProjectileStates m_State = ProjectileStates.Ready;  // current state of fire ball
        protected float m_CurrentLifetime = 0.0f;                 // lifetime of fire ball
        protected Vector3? m_LastPosition = null;          // position on previus frame
        protected float m_CurrentHitStrength = 25.0f;             // current hit strength of fire ball
        protected ProjectileFunc m_OnLifetimeExpire = null;   // on lifetime expire event
        protected SphereCollider m_SphereCollider;                // sphere collider component
        protected Rigidbody m_Rigidbody;                          // reference to rigid body 


        protected GameObject m_Owner;                     // owner character of this fireball ( to ignore hits if wanted)
        protected HitInformation m_HitInfo;        // information on hit object
        protected VoidFunc m_OnHit = null;                // on hit delegate
        protected VoidFunc m_OnAfterHit = null;           // on after hit
        protected VoidFunc m_OnCollisionEnter = null;     // on collision enter

        private float m_HitStrength = 0.0f;

        /// <summary>
        /// gets information on hit object
        /// </summary>
        public HitInformation hitInfo { get { return m_HitInfo; } }

        /// <summary>
        /// gets and sets on hit event
        /// </summary>
        public VoidFunc OnHit { get { return m_OnHit; } set { m_OnHit = value; } }

        /// <summary>
        /// gets owner object of projectile
        /// </summary>
        public GameObject owner { get { return m_Owner; } }

        /// <summary>
        /// gets reference to rigidbody component
        /// </summary>
        public Rigidbody RigidBody
        {
            get
            {
#if DEBUG_INFO
                if (!m_Rigidbody) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return null; }
#endif
                return m_Rigidbody;
            }
        }

        /// <summary>
        /// gets reference to sphere collider component
        /// </summary>
        public SphereCollider SphereCollider { get { return m_SphereCollider; } }

        /// <summary>
        /// gets current life time
        /// </summary>
        public float CurrentLifetime { get { return m_CurrentLifetime; } }

        /// <summary>
        /// gets and sets state of fire ball
        /// </summary>
        public ProjectileStates State { get { return m_State; } set { m_State = value; } }

        /// <summary>
        /// gets and sets on lifetime expire event
        /// </summary>
        public ProjectileFunc OnLifetimeExpire { get { return m_OnLifetimeExpire; } set { m_OnLifetimeExpire = value; } }

        /// <summary>
        /// gets and sets current hit strength
        /// </summary>
        public float CurrentHitStrength { get { return m_CurrentHitStrength; } set { m_CurrentHitStrength = value; } }

        /// <summary>
        /// gets and sets on collision enter callback
        /// </summary>
        public VoidFunc OnCollisionEnterEvent { get { return m_OnCollisionEnter; } set { m_OnCollisionEnter = value; } }

        /// <summary>
        /// gets and set after hit callback ( like destroy projectile )
        /// </summary>
        public VoidFunc OnAfterHit { get { return m_OnAfterHit; } set { m_OnAfterHit = value; } }



        /// <summary>
        /// initialize component
        /// </summary>
        /// <returns>is initialization success</returns>
        public virtual bool initialize()
        {
            m_SphereCollider = GetComponent<SphereCollider>();
            if (!m_SphereCollider) { Debug.LogError("SphereCollider component missing." + " < " + this.ToString() + ">"); return false; }

            m_Rigidbody = GetComponent<Rigidbody>();
            if (!m_Rigidbody) { Debug.LogError("cannot find 'Rigidbody' component." + " < " + this.ToString() + ">"); return false; }

            m_HitInfo = new HitInformation();

            return true;
        }

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            if (!initialize()) { Debug.LogError("cannot initialize component." + " < " + this.ToString() + ">"); return; }
        }

        /// <summary>
        /// Unity LateUpdate method
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
#if DEBUG_INFO
            if (!RigidBody) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
#endif
            switch (projectileMode)
            {
                case ProjectileMode.Spheremarch: updateSpheremarch(); break;
            }
        }

        /// <summary>
        /// Unity OnCollisionEnter method
        /// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
        /// </summary>
        /// <param name="_collision">Collision info class</param>
        void OnCollisionEnter(Collision _collision)
        {
            if (m_OnCollisionEnter != null)
            {
                m_OnCollisionEnter();
            }
        }


        /// <summary>
        /// update projectile
        /// </summary>
        protected virtual void updateSpheremarch()
        {
            Vector3 transformPosition = transform.position;

            // advance lifetime starting from time when fired onwards
            if (m_State != ProjectileStates.Ready)
            {
                m_CurrentLifetime += Time.deltaTime;
                if (m_CurrentLifetime > lifetime)
                {

                    if (m_OnLifetimeExpire != null)
                    {
                        m_OnLifetimeExpire(this);
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                    return;
                }
            }

#if DEBUG_DRAW
            positionList.Add(transformPosition);
            radiusList.Add(m_SphereCollider.radius * this.transform.localScale.x);
#endif
            // check for collision only when fired
            if (m_State == ProjectileStates.Fired && m_LastPosition.HasValue)
            {

                // shoot sphere from last position to current 
                // and check if we have a hit

                int mask = collidingLayers;


#if DEBUG_INFO
                if (!m_SphereCollider)
                {
                    Debug.LogError("SphereCollider missing." + " < " + this.ToString() + ">");
                    return;
                }
#endif

                float radius = m_SphereCollider.radius * this.transform.localScale.x;
                Vector3 difference = transformPosition - m_LastPosition.Value;
                Vector3 direction = difference.normalized;
                float length = difference.magnitude;
                Vector3 rayPos = m_LastPosition.Value;


                Ray ray = new Ray(rayPos, direction);

                RaycastHit[] hits = Physics.SphereCastAll(ray, radius, length, mask);

                List<int> chosenHits = new List<int>();
                RagdollManager ragMan = null;

                RaycastHit? rayhit = null;

                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit rhit = hits[i];
                    BodyColliderScript bcs = rhit.collider.GetComponent<BodyColliderScript>();
                    if (!bcs)
                    {
#if DEBUG_INFO
                        Debug.LogError("BodyColliderScript missing on " + rhit.collider.name);
#endif
                        continue;
                    }

                    if (!bcs.ParentObject)
                    {
#if DEBUG_INFO
                        Debug.LogError("BodyColliderScript.ParentObject missing on " + rhit.collider.name);
#endif
                        continue;
                    }
                    if (bcs.ParentObject == this.m_Owner)
                    {
                        continue;
                    }

                    if (!ragMan)
                    {
                        ragMan = bcs.ParentRagdollManager;
                        m_HitInfo.hitObject = bcs.ParentObject;
                        m_HitInfo.collider = rhit.collider;
                        m_HitInfo.hitDirection = direction;
                        m_HitInfo.hitStrength = m_CurrentHitStrength;
                        rayhit = rhit;
                    }

                    chosenHits.Add(bcs.index);
                }


                if (hits.Length > 0)
                {
                    if (ragMan)
                    {

                        if (!rayhit.HasValue)
                        {
#if DEBUG_INFO
                            Debug.LogError("object cannot be null." + " < " + this.ToString() + ">");
#endif
                            return;
                        }

                        Vector3 n = rayhit.Value.normal;
                        Vector3 r = Vector3.Reflect(direction, n);
                        this.transform.position = rayPos + ray.direction *
                            (rayhit.Value.distance - radius);
                        Vector3 vel = r;
                        this.m_Rigidbody.velocity = vel;
                        
                        m_HitInfo.bodyPartIndices = chosenHits.ToArray();
                        m_State = ProjectileStates.Done;

                        m_CurrentHitStrength = vel.magnitude;
                        m_CurrentHitStrength += m_HitStrength;

                        if (m_OnHit != null)
                        {
                            m_OnHit();
                        }
                        else
                        {
                            Vector3 force = direction * m_CurrentHitStrength;
                            ragMan.startHitReaction(m_HitInfo.bodyPartIndices, force);
                        }

                        if (m_OnAfterHit != null)
                            m_OnAfterHit();
                    }
#if DEBUG_INFO
                    else
                    {
                        BodyColliderScript bcs = hits[0].collider.GetComponent<BodyColliderScript>();
                        if (!bcs)
                            return;
                        if (!bcs.ParentObject)
                            return;
                        if (bcs.ParentObject == this.m_Owner)
                            return;
                        Debug.LogWarning("RagdollUser interface not implemented. " +
                        bcs.ParentObject.name);
                    }
#endif
                }

            }
            m_LastPosition = transformPosition;
        }

        /// <summary>
        /// setup projectile for fireing
        /// </summary>
        public virtual void setup()
        {
            OnHit = () =>
            {
                if (hitInfo.hitObject)
                {
                    RagdollManager ragMan = null;
                    ragMan = hitInfo.hitObject.GetComponent<RagdollManager>();

                    if (!ragMan)
                    {
#if DEBUG_INFO
                        Debug.LogError("Ball::OnHit cannot find RagdollManager component on " +
                            hitInfo.hitObject.name + ".");
#endif
                        return;
                    }

                    if (!ragMan.acceptHit)
                    {
                        return;
                    }

                    Vector3 force = hitInfo.hitDirection * CurrentHitStrength;
                    ragMan.startHitReaction(hitInfo.bodyPartIndices, force);
                }
            };
        }


        /// <summary>
        /// Creates ball projectile, assignes owner and increments ball counter
        /// </summary>
        /// <param name="prefab">prefab of projectile</param>
        /// <param name="xform">position and rotation transform</param>
        /// <param name="_owner">owner game object</param>
        /// <returns>created ball projectile</returns>
        public static Projectile CreateProjectile(Projectile prefab, Transform xform, GameObject _owner)
        {
#if DEBUG_INFO
            if (!prefab) { Debug.LogError("object cannot be null."); return null; }
#endif
            ProjectileCount++;
            if (xform)
            {
                Projectile newProj = (Projectile)Instantiate(prefab,
                   xform.position,
                   xform.rotation);
                newProj.name = newProj.name + Projectile.ProjectileCount;
                newProj.m_Owner = _owner;
                return newProj;
            }
            else
            {
                Projectile newProj = Instantiate(prefab);
                newProj.name = newProj.name + Projectile.ProjectileCount;
                newProj.m_Owner = _owner;
                return newProj;
            }
        }

        /// <summary>
        /// destroys ball projectile and decrements counter
        /// </summary>
        /// <param name="ball"></param>
        public static void DestroyProjectile(Projectile proj)
        {
            if (!proj) return;

            Projectile.ProjectileCount--;
            Destroy(proj.gameObject);
        }

#if DEBUG_DRAW
        List<Vector3> positionList = new List<Vector3>();
        List<float> radiusList = new List<float>();
        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for(int i = 0;i<positionList .Count-1;i++)
            {
                Gizmos.DrawWireSphere(positionList[i], radiusList[i]);
                Gizmos.DrawLine(positionList[i],positionList[i + 1]);
            }
        }
#endif


        /// <summary>
        /// set projectile hit strength
        /// </summary>
        /// <param name="hitStrength"></param>
        public void setHitStrength(float hitStrength)
        {
            m_HitStrength = hitStrength;
        }

    } 



}
