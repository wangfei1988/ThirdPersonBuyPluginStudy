//© 2015 Unity Technologies modified by Mario Lelas
using System;
using System.Collections;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// camera collision class
    /// </summary>
    [RequireComponent(typeof(OrbitCameraController))]
    public class ProtectFromWalls : MonoBehaviour
    {
        public LayerMask mask; //哪些layer算作是wall
        public float clipMoveTime = 0.05f;              // time taken to move when avoiding cliping (low value = fast, which it should be)
        public float returnTime = 0.4f;                 // time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime)
        public float sphereCastRadius = 0.1f;           // the radius of the sphere used to test for object between camera and target
        public bool visualiseInEditor;                  // toggle for visualising the algorithm through lines for the raycast in the editor
        public float closestDistance = 0.5f;            // the closest distance the camera can be from the target
        public bool protecting { get; private set; }    // used for determining if there is an object between the target and the camera
        public string dontClipTag = "Player";           // don't clip against objects with this tag (useful for not clipping against the targeted object)

        private Transform m_Cam;                  // the transform of the camera
        public Transform m_Pivot;                // the point at which the camera pivots around
        private float m_OriginalDist;             // the original distance to the camera before any modification are made
        private float m_MoveVelocity;             // the velocity at which the camera moved
        private float m_CurrentDist;              // the current distance from the camera to the target
        private Ray m_Ray;                        // the ray used in the lateupdate for casting between the camera and the target
        private RaycastHit[] m_Hits;              // the hits between the camera and the target
        private RayHitComparer m_RayHitComparer;  // variable to compare raycast hit distances


        /// <summary>
        /// Unity Start method
        /// is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        private void Start()
        {
            // find the camera in the object hierarchy
            m_Cam = GetComponentInChildren<Camera>().transform;
            m_OriginalDist = (m_Pivot.position - m_Cam.position).magnitude; // m_Cam.localPosition.magnitude;
            m_CurrentDist = m_OriginalDist;

            // create a new RayHitComparer
            m_RayHitComparer = new RayHitComparer();
        }

        /// <summary>
        /// Unity LateUpdate method  这个肯定是在Camera的Update方法之后。 
        /// 每一帧，先是各种update，最后才是渲染
        /// is called every frame, if the Behaviour is enabled
        /// </summary>
        private void LateUpdate()
        {
            Vector3 currentDirection = (m_Cam.position - m_Pivot.position);
            m_OriginalDist = currentDirection.magnitude;
            float targetDist = m_OriginalDist;
            m_Ray.origin = m_Pivot.position;
            m_Ray.direction = currentDirection.normalized;

            // initial check to see if start of spherecast intersects anything
            var cols = Physics.OverlapSphere(m_Ray.origin, sphereCastRadius, mask.value );

            bool initialIntersect = false;
            bool hitSomething = false;

            // loop through all the collisions to check if something we care about
            for (int i = 0; i < cols.Length; i++)
            {
                if ((!cols[i].isTrigger) &&
                    !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag)))
                {
                    initialIntersect = true;
                   
                    break;
                }
            }

            // if there is a collision
            if (initialIntersect)
            {
                m_Ray.origin += m_Pivot.forward  * sphereCastRadius;

                // do a raycast and gather all the intersections
                m_Hits = Physics.RaycastAll(m_Ray, m_OriginalDist - sphereCastRadius);

            }
            else
            {
                // if there was no collision do a sphere cast to see if there were any other collisions
                m_Hits = Physics.SphereCastAll(m_Ray, sphereCastRadius, m_OriginalDist + sphereCastRadius, mask.value);
            }

            // sort the collisions by distance
            Array.Sort(m_Hits, m_RayHitComparer);

            // set the variable used for storing the closest to be as far as possible
            float nearest = Mathf.Infinity;

            // loop through all the collisions
            for (int i = 0; i < m_Hits.Length; i++)
            {
                // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
                if (m_Hits[i].distance < nearest && (!m_Hits[i].collider.isTrigger) &&
                    !(m_Hits[i].collider.attachedRigidbody != null &&
                      m_Hits[i].collider.attachedRigidbody.CompareTag(dontClipTag)))
                {

                    // change the nearest collision to latest
                    nearest = m_Hits[i].distance;
                    targetDist = (m_Ray.origin - m_Hits[i].point ).magnitude;
                    hitSomething = true;
                }
            }

            // hit something so move the camera to a better position
            protecting = hitSomething;
            m_CurrentDist = Mathf.SmoothDamp(m_CurrentDist, targetDist, ref m_MoveVelocity,
                                           m_CurrentDist > targetDist ? clipMoveTime : returnTime);


            Vector3 newPos = m_Pivot.position + currentDirection.normalized * m_CurrentDist;
            
            m_Cam.position = newPos;

        }
    }
}

