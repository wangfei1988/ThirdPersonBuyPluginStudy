// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{

    /// <summary>
    /// onArrowHit delegate prototype
    /// </summary>
    /// <param name="arrow">ref arrow</param>
    /// <param name="rayhit">ref rayhit</param>
    /// <returns></returns>
    public delegate void OnArrowHit(ref Arrow arrow, ref RaycastHit rayhit);

    /// <summary>
    /// arrow projectile class
    /// </summary>
    public class Arrow
    {
        /// <summary>
        /// projectile states
        /// </summary>
        public enum STATES { READY, GO, HIT, DEFAULT_PHYSICS };

        /// <summary>
        /// current position 
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// previus position
        /// </summary>
        public Vector3 prevPosition;

        /// <summary>
        /// current direction
        /// </summary>
        public Vector3 direction;

        /// <summary>
        /// current velocity
        /// </summary>
        public Vector3 velocity;

        /// <summary>
        /// speed of arrow
        /// </summary>
        public float speed;

        /// <summary>
        /// time upon release until hit
        /// </summary>
        public float fly_time;

        /// <summary>
        /// current state of arrow
        /// </summary>
        public STATES state;

        /// <summary>
        /// colliding layer/s    
        /// </summary>
        public LayerMask layers;

        /// <summary>
        /// length of arrow  
        /// </summary>
        public float length = 1.0f;

        /// <summary>
        /// reference to parent game object
        /// </summary>
        public GameObject gobject;

        /// <summary>
        /// Trail object.
        /// I am using another object for trail because if arrow hits something
        /// trail discontects and finishes on hit position
        /// arrow can rebound or hit moving target
        /// but trail does not follow
        /// </summary>
        public Transform trailObj;

        /// <summary>
        /// lifetime of released arrow
        /// </summary>
        public float lifetime;

        /// <summary>
        /// max lifetime of released arrow
        /// </summary>
        public float maxLifetime;

        /// <summary>
        /// should arrow lifetime be increased ( after its been released from bow )
        /// </summary>
        public bool advanceLifetime;

        /// <summary>
        /// reference to parent transform
        /// </summary>
        public Transform parent;

        /// <summary>
        /// rigid body of arrow
        /// </summary>
        public Rigidbody RigidBody;

        /// <summary>
        /// arrow collider
        /// </summary>
        public Collider Collider;

        /// <summary>
        /// on arrow hit event
        /// </summary>
        public OnArrowHit OnArrowHit;

        /// <summary>
        /// IGameCharacter owner of arrow ( exlude collisions )
        /// </summary>
        public IGameCharacter owner;

        /// <summary>
        ///  damage of arrow instance
        /// </summary>
        public int arrowDamage = 1;

        /// <summary>
        /// damage of bow that fired the arrow
        /// </summary>
        public int bowDamage = 1;



        /// <summary>
        /// I am doing this way instead of parenting by unity
        /// because i think i noticed scaling errors / bugs
        /// this way there is no scaling
        /// </summary>
        public Vector3 pos_offset = Vector3.zero;               // position offset
        public Quaternion rot_offset = Quaternion.identity;     // rotation offset

        /// <summary>
        /// arrow constructor
        /// </summary>
        /// <param name="_arrowObject"></param>
        public Arrow(GameObject _arrowObject, IGameCharacter _owner)
        {
            if (!_arrowObject) Debug.LogError("_arrowObject cannot be null");

            // set to default values
            lifetime = 0.0f;
            maxLifetime = 12.0f;
            advanceLifetime = false;
            gobject = _arrowObject;
            trailObj = null;
            parent = null;

            if (_arrowObject.transform.childCount > 0)
            {
                trailObj = _arrowObject.transform.GetChild(0);
                trailObj.gameObject.SetActive(false);
            }
            RigidBody = gobject.GetComponent<Rigidbody>();
            Collider = gobject.GetComponent<Collider>();
            owner = _owner;
            OnArrowHit = null;
    }
};

}