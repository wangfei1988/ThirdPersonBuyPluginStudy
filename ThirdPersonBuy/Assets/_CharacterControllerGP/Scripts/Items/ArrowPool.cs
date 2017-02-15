// © 2016 Mario Lelas
#if DEBUG_INFO
//#define DEBUG_ARROW_PATH // debug draw arrow' path positions lines
#endif
#define ARROW_REBOUND_PHYSICS // use rebound by physics ( more expensive )  or just reflect upon surface( cheaper)
#define DISCONNECT_ARROW_TRAIL_ON_REBOUND // disconnect arrow trail upon hittin rebound surface 


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MLSpace
{
    /// <summary>
    /// arrow - material impact event class
    /// extended from unity event
    /// </summary>
    [System.Serializable]
    public class MaterialHitEvent : UnityEvent<Arrow, RaycastHit>
    {
    }

    /// <summary>
    /// info class 
    /// holds information of scene physics material
    /// and how arrow should behave on impact with them
    /// </summary>
    [System.Serializable]
    public class ArrowImpactInfo
    {
        /// <summary>
        /// reference physics material
        /// </summary>
        public PhysicMaterial material = null;

        /// <summary>
        /// sound clip ( chosen from array )
        /// that plays upon impact
        /// </summary>
        public AudioClip[] impactClips = null;

        /// <summary>
        /// should arrow be stuck into material
        /// </summary>
        public bool arrowStuck = true;

        /// <summary>
        /// how deep should arrow stuck into material
        /// if value is 0 - arrow will go through material 
        /// and play sound
        /// </summary>
        [Range(0, 1)]
        public float materialSoftness = 0.35f;

        /// <summary>
        /// arrow - material impact event
        /// </summary>
        public MaterialHitEvent On_Hit_Event;
    }

    /// <summary>
    /// manages arrows update , creation, deletion
    /// </summary>
    public class ArrowPool : MonoBehaviour
    {
        /// <summary>
        /// instance of array of arrow - material imapct informations
        /// </summary>
        public ArrowImpactInfo[] arrowImpactInfo;

        /// <summary>
        /// static array of arrow - material impact informations
        /// </summary>
        public static ArrowImpactInfo[] m_ArrowImpactInfo;

        /// <summary>
        /// pool use information 
        /// </summary>
        private class ArrowUnit
        {
            /// <summary>
            /// current arrow
            /// </summary>
            public Arrow arrow = null;

            /// <summary>
            /// is arrow in use ?
            /// </summary>
            public bool inUse = true;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="newArrow"></param>
            public ArrowUnit(Arrow newArrow)
            {
                arrow = newArrow;
                inUse = true;
            }
        }

        private static List<Arrow> m_active_arrows = new List<Arrow>();    // arrows list
        private static List<ArrowUnit> m_arrowPool = new List<ArrowUnit>(); // arrow pool
        private static RayHitComparer m_RayHitComparer = new RayHitComparer();  // variable to compare raycast hit distances

#if DEBUG_ARROW_PATH
        private static Dictionary<Arrow, List<Vector3>> arrow_debug_paths = new Dictionary<Arrow, List<Vector3>>();
#endif

        void Start()
        {
            // transfer info to the static field
            m_ArrowImpactInfo = arrowImpactInfo;
        }


        /// <summary>
        /// creates arrow and sets it up in hand
        /// </summary>
        /// <param name="arrowPrefab">arrow prefab to instantiate</param>
        /// <param name="arrowAttachBone">transform on which to attach arrow</param>
        /// <returns></returns>
        public static Arrow CreateArrow(
            GameObject arrowPrefab, 
            Transform arrowAttachBone,
            float maxLifetime,
            float arrowLength,
            LayerMask layers,
            IGameCharacter _owner)
        {
#if DEBUG_INFO
            if (m_arrowPool == null) { Debug.LogError("object cannot be null"); return null; }
            if (m_active_arrows == null) { Debug.LogError("object cannot be null"); return null; }
            if (!arrowPrefab) { Debug.LogError("_currentArrowPrefab is null. Cannot create arrow"); return null; }
            if (!arrowAttachBone) { Debug.LogError("object cannot be null"); return null; }
#endif

            ArrowUnit newArrowUnit = null;

            // try to find available arrow
            for (int i = 0; i < m_arrowPool.Count; i++)
            {
                if (!m_arrowPool[i].inUse)
                {
                    newArrowUnit = m_arrowPool[i];
                    newArrowUnit.inUse = true;

                    break;
                }
            }

            // if not create new
            if (newArrowUnit == null)
            {
                GameObject arrowObject =
                    (GameObject)MonoBehaviour.Instantiate(arrowPrefab);
                Arrow newArrow = new Arrow(arrowObject, _owner);
                newArrowUnit = new ArrowUnit(newArrow);
                m_arrowPool.Add(newArrowUnit);

                newArrow.gobject.name = arrowPrefab.name + m_arrowPool.Count;

#if DEBUG_ARROW_PATH
                arrow_debug_paths[newArrow] = new List<Vector3>();
#endif
            }


#if DEBUG_INFO
            if (newArrowUnit == null) { Debug.LogError("creating arrow failed."); return null; }
#endif
            newArrowUnit.arrow.gobject.transform.position = Vector3.zero;
            newArrowUnit.arrow.gobject.transform.rotation = Quaternion.identity;
            newArrowUnit.arrow.gobject.transform.SetParent(arrowAttachBone, false);
            newArrowUnit.arrow.gobject.SetActive(true);
            newArrowUnit.arrow.position = Vector3.zero;
            newArrowUnit.arrow.prevPosition = Vector3.zero;
            newArrowUnit.arrow.direction = Vector3.zero;
            newArrowUnit.arrow.fly_time = 0.0f;
            newArrowUnit.arrow.state = Arrow.STATES.READY;
            newArrowUnit.arrow.speed = 0.0f;
            newArrowUnit.arrow.advanceLifetime = false;
            newArrowUnit.arrow.RigidBody.detectCollisions = false;
            newArrowUnit.arrow.RigidBody.isKinematic = true;
            newArrowUnit.arrow.Collider.enabled = false;
            newArrowUnit.arrow.layers = layers;
            newArrowUnit.arrow.length = arrowLength;
            newArrowUnit.arrow.maxLifetime = maxLifetime;
            

            m_active_arrows.Add(newArrowUnit.arrow);

            return newArrowUnit.arrow;

        }

        /// <summary>
        /// setup for shooting passed arrow
        /// </summary>
        /// <param name="currentArrow">current arrow passed</param>
        /// <param name="arrowPos">shoot position</param>
        /// <param name="arrowDir">shoot direction</param>
        /// <param name="arrowSpeed">shoot speed</param>
        public static void ShootArrow(
            Arrow currentArrow,
            ref Vector3 arrowPos,
            ref Vector3 arrowDir,
            float arrowSpeed)
        {
            if (currentArrow == null)
            {
#if DEBUG_INFO
                Debug.LogWarning("currentArrow is null.Cannot shoot.");
#endif
                return;
            }
            currentArrow.position = arrowPos;
            currentArrow.direction = arrowDir;
            currentArrow.prevPosition = arrowPos;
            currentArrow.fly_time = 0.0f;
            currentArrow.speed = arrowSpeed;
            currentArrow.state = Arrow.STATES.GO;
            currentArrow.advanceLifetime = true;
            currentArrow.gobject.transform.parent = null;

#if DEBUG_ARROW_PATH
            arrow_debug_paths[currentArrow].Add(currentArrow.position);
#endif

            if (currentArrow.trailObj)
            {
                currentArrow.trailObj.SetParent(
                    currentArrow.gobject.transform, true);
                currentArrow.trailObj.localPosition = Vector3.zero;
                currentArrow.trailObj.localRotation = Quaternion.identity;
                currentArrow.trailObj.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// return arrow to pool
        /// </summary>
        /// <param name="arrow">arrow to return</param>
        private static void returnToPool(Arrow arrow)
        {
#if DEBUG_INFO
            if (m_arrowPool == null)
            {
                Debug.LogError("object cannot be null");
                return;
            }
#endif
            ArrowUnit aunit = m_arrowPool.Find(x => x.arrow == arrow);

            if (aunit == null)
            {
                return;
            }
            aunit.inUse = false;
            aunit.arrow.gobject.SetActive(false);
            aunit.arrow.gobject.transform.SetParent(null);
            aunit.arrow.advanceLifetime = false;
            aunit.arrow.lifetime = 0f;
            aunit.arrow.state = Arrow.STATES.READY;

#if DEBUG_ARROW_PATH
            arrow_debug_paths[arrow].Clear();
#endif

            // reset trail object if present
            if (aunit.arrow.trailObj != null)
            {
                aunit.arrow.trailObj.gameObject.SetActive(false);
            }

            // disable collider
            if (aunit.arrow.Collider)
            {
                aunit.arrow.Collider.enabled = false;
                aunit.arrow.Collider.isTrigger = true;
            }

            // disable rigid body
            if (aunit.arrow.RigidBody)
            {
                aunit.arrow.RigidBody.isKinematic = true;
                aunit.arrow.RigidBody.detectCollisions = false;
                aunit.arrow.RigidBody.useGravity = false;
            }
        }

        /// <summary>
        /// update arrows lifetime and transform
        /// </summary>
        public static void UpdateArrows()
        {
#if DEBUG_INFO
            if (m_active_arrows == null) { Debug.LogError("object cannot be null"); return; }
#endif
            if (Time.timeScale == 0.0f) return;

            for (int i = 0; i < m_active_arrows.Count; ++i)
            {
                Arrow arrow = m_active_arrows[i];
                if (arrow == null) continue;

                // update arrows lifetime
                if (arrow.advanceLifetime)
                {
                    arrow.lifetime += Time.deltaTime;

                    // I am doing this way instead of parenting  by unity
                    // because i think i noticed scaling errors / bugs
                    // this way there is no scaling
                    if (arrow.parent)
                    {
                        arrow.gobject.transform.position =
                            arrow.parent.TransformPoint(arrow.pos_offset);
                        arrow.gobject.transform.rotation =
                            arrow.parent.rotation *
                            arrow.rot_offset;
                    }
                    if (arrow.lifetime > arrow.maxLifetime)
                    {
                        returnToPool(arrow);
                        m_active_arrows.Remove(arrow);
                        arrow = null;
                        --i;
                        continue;
                    }
                }

                // update arrows transform
                if (arrow.state == Arrow.STATES.GO)
                {
                    arrow.prevPosition = arrow.position;
                    const float FLYMAXTIME = 6; 
                    arrow.fly_time += Time.deltaTime;
                    if (arrow.fly_time > FLYMAXTIME)
                    {
                        arrow.state = Arrow.STATES.HIT;
                    }
                    else
                    {
                        float GlerpAmt = arrow.fly_time / FLYMAXTIME;
                        Vector3 addedPosition = arrow.direction * arrow.speed ;
                        addedPosition = Vector3.Lerp(addedPosition, Physics.gravity, GlerpAmt);
                        addedPosition *= Time.deltaTime;
                        arrow.position += addedPosition;



                        if (Time.deltaTime > 0)
                        {
                            arrow.velocity = arrow.position - arrow.prevPosition;
                            arrow.velocity /= Time.deltaTime;
                        }

#if DEBUG_ARROW_PATH
                        arrow_debug_paths[arrow].Add(arrow.position);
#endif

                        Vector3 difference = arrow.position - arrow.prevPosition;
                        arrow.direction = difference.normalized;
                        float maxDistance = (arrow.position - arrow.prevPosition).magnitude;

                        arrow.gobject.transform.position = arrow.prevPosition;
                        arrow.gobject.transform.LookAt(arrow.position);

                        Ray ray = new Ray(arrow.prevPosition, arrow.direction);

                        RaycastHit[] rayHits = Physics.RaycastAll(ray, maxDistance, arrow.layers, QueryTriggerInteraction.Ignore );

                        // sort the collisions by distance
                        System.Array.Sort(rayHits, m_RayHitComparer);

                        for (int j = 0; j < rayHits.Length; ++j)
                        {
                            RaycastHit hit = rayHits[j];
                            // EXLUDE COLLISIONS WITH OWNER CHARACTER
                            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "ColliderInactiveLayer")
                            {
                                BodyColliderScript bcs = hit.collider.GetComponent<BodyColliderScript>();
                                if (bcs)
                                {
                                    if (bcs.ParentObject/*ownerGameCharacter*/  == arrow.owner.transform .gameObject )
                                    {
                                        continue;
                                    }
                                }
                            }
                            checkMaterialImpact(ref arrow, ref hit);
                            break;
                        }

                        arrow.prevPosition = arrow.position;
                    }
                }
            }

#if DEBUG_ARROW_PATH
            foreach(KeyValuePair<Arrow,List<Vector3>> pair in arrow_debug_paths)
            {
                List<Vector3> path = pair.Value;
                for(int i = 0;i<path.Count-1;i++)
                {

                    if (i % 2 == 0) Debug.DrawLine(path[i], path[i + 1], Color.yellow);
                    else Debug.DrawLine(path[i], path[i + 1], Color.blue);
                }
            }
#endif

        }

        /// <summary>
        /// remove arrow
        /// </summary>
        /// <param name="arrow">arrow to remove</param>
        public static void RemoveArrow(Arrow arrow)
        {
            returnToPool(arrow);
        }

        /// <summary>
        /// clear all arrows in pool
        /// </summary>
        public static void ClearArrows()
        {
            for(int i = 0;i<m_arrowPool .Count;i++)
            {
                Destroy(m_arrowPool[i].arrow.gobject);
            }
            m_arrowPool.Clear();
            m_active_arrows.Clear();
#if DEBUG_ARROW_PATH
            arrow_debug_paths.Clear();
#endif

        }

        /// <summary>
        /// check arrow - material impacts
        /// </summary>
        /// <param name="arrow">current arrow</param>
        /// <param name="rayhit">rayhit information struct</param>
        /// <returns></returns>
        public static bool checkMaterialImpact(ref Arrow arrow,ref RaycastHit rayhit)
        {
            if(m_ArrowImpactInfo == null)
            {
#if DEBUG_INFO
                Debug.LogWarning("Arrow - material impact info not created");
#endif
                return false;
            }
            
            for(int i = 0;i<m_ArrowImpactInfo .Length;i++)
            {
                if (rayhit.collider .sharedMaterial == m_ArrowImpactInfo[i].material )
                {
                    if (m_ArrowImpactInfo[i].impactClips.Length > 0)
                    {
                        int len = m_ArrowImpactInfo[i].impactClips.Length;
                        int rnd = Random.Range(0, len);
                        if (m_ArrowImpactInfo[i].impactClips[rnd])
                        {
                            AudioSource.PlayClipAtPoint(m_ArrowImpactInfo[i].impactClips[rnd], arrow.position);
                        }
#if DEBUG_INFO
                        else
                        {
                            Debug.LogError("No audioclip assign on physics material " + m_ArrowImpactInfo[i].material + " on index " + rnd);
                        }
#endif

                        if(m_ArrowImpactInfo[i].On_Hit_Event != null)
                        {
                            m_ArrowImpactInfo[i].On_Hit_Event.Invoke( arrow, rayhit);
                        }
                    }

                    if (m_ArrowImpactInfo[i].arrowStuck)
                    {
                        arrowStuck(ref arrow, ref rayhit, m_ArrowImpactInfo[i].materialSoftness);
                    }
                    else
                    {
#if ARROW_REBOUND_PHYSICS
                        arrowHitRebound(ref arrow, ref rayhit);
#else
                        Vector3 refl = Vector3.Reflect(arrow.direction, rayhit.normal);
                        arrow.direction = refl;
                        arrow.position = rayhit.point;
                        arrow.speed *= 0.25f;

                        // if has non kinematic rigidbody add push based on arrow hit power
                        Rigidbody rb = rayhit.collider.attachedRigidbody;
                        if (rb)
                        {
                            if (!rb.isKinematic)
                            {
                                rb.AddForce(arrow.velocity, ForceMode.Acceleration);
                            }
                        }
#endif


                    }

                    return m_ArrowImpactInfo[i].arrowStuck;
                }
            }
            return true;
        }
  
        /// <summary>
        /// procedure on arrow stuck
        /// </summary>
        /// <param name="arrow">current arrow</param>
        /// <param name="hit">raycasthit info struct</param>
        /// <param name="materialSoftness">softness of material</param>
        private static void arrowStuck(ref Arrow arrow,ref RaycastHit hit,float materialSoftness)
        {
#if DEBUG_INFO
            if (arrow == null) { Debug.LogError("arrow cannot be null"); return; }
            if (!arrow.gobject) { Debug.LogError("object cannot be null"); return; }
#endif
            if (materialSoftness == 1.0f)
            {
                arrow.state = Arrow.STATES.GO;
                return;
            }
            Vector3 pos = arrow.prevPosition + arrow.direction * (hit.distance - (arrow.length *  ( 1 -materialSoftness)));

            if (arrow.trailObj != null)
            {
                arrow.trailObj.transform.position = pos;
                arrow.trailObj.parent = null;
            }

            if (arrow.Collider)
            {
                arrow.Collider.enabled = false;
            }
            if (arrow.RigidBody)
            {
                arrow.RigidBody.isKinematic = true;
                arrow.RigidBody.detectCollisions = false;
                arrow.RigidBody.useGravity = false;
            }

            arrow.state = Arrow.STATES.HIT;
            arrow.advanceLifetime = true;
            arrow.lifetime = 0.0f;


            arrow.gobject.transform.LookAt(pos + arrow.direction);
            arrow.gobject.transform.position = pos;




            // I am doing this way instead of parenting  by unity
            // because i think i noticed scaling errors
            // this way there is no scaling
            arrow.parent = hit.collider.transform;
            arrow.pos_offset = arrow.parent.InverseTransformPoint(arrow.gobject.transform.position);
            arrow.rot_offset = Quaternion.Inverse(arrow.parent.rotation) * arrow.gobject.transform.rotation;

            // if has non kinematic rigidbody add push based on arrow hit power
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb)
            {
                if (!rb.isKinematic)
                {
                    rb.AddForce(arrow.velocity, ForceMode.Acceleration);
                }
            }
        }

        /// <summary>
        /// gets called when arrow hits rebound material
        /// </summary>
        /// <param name="arrow">current arrow</param>
        /// <param name="hit">raycasthit info struct</param>
        private static void arrowHitRebound(ref Arrow arrow, ref RaycastHit hit)
        {
#if DEBUG_INFO
            if (arrow == null) { Debug.LogError("arrow cannot be null"); return; }
            if (!arrow.gobject) { Debug.LogError("object cannot be null"); return; }
#endif

#if DISCONNECT_ARROW_TRAIL_ON_REBOUND
            if (arrow.trailObj != null)
            {
                arrow.trailObj.transform.position = hit.point;
                arrow.trailObj.parent = null;
            }
#endif
            arrow.state = Arrow.STATES.DEFAULT_PHYSICS;

            if (arrow.Collider == null)
            {
#if DEBUG_INFO
                Debug.Log("arrow has NO collider. will be created.");
#endif
                arrow.Collider = arrow.gobject.AddComponent<CapsuleCollider>();
                (arrow.Collider as CapsuleCollider).direction = 2;
                (arrow.Collider as CapsuleCollider).radius = 0.03f;
                (arrow.Collider as CapsuleCollider).height = 0.7f;
            }
            else
            {
                arrow.Collider.enabled = true;
                arrow.Collider.isTrigger = false;
            }
            if (arrow.RigidBody == null)
            {
#if DEBUG_INFO
                Debug.Log("arrow has NO rigidbody. will be created.");
#endif
                arrow.RigidBody = arrow.gobject.AddComponent<Rigidbody>();
                arrow.RigidBody.drag = 1;
                arrow.RigidBody.angularDrag = 999;
                arrow.RigidBody.collisionDetectionMode =
                    CollisionDetectionMode.ContinuousDynamic;
                Vector3 refl = Vector3.Reflect(arrow.direction, hit.normal);
                arrow.gobject.transform.position = hit.point + refl * 0.4f;
                arrow.RigidBody.velocity = refl * arrow.speed * 0.025f;
                float rndX = UnityEngine.Random.Range(-180, 180);
                float rndY = UnityEngine.Random.Range(-180, 180);
                float rndZ = UnityEngine.Random.Range(-180, 180);
                arrow.RigidBody.MoveRotation(
                    Quaternion.Euler(new Vector3(rndX, rndY, rndZ)));
            }
            else
            {
                arrow.RigidBody.isKinematic = false;
                arrow.RigidBody.detectCollisions = true;
                arrow.RigidBody.useGravity = true;
                Vector3 refl = Vector3.Reflect(arrow.direction, hit.normal);
                arrow.gobject.transform.position = hit.point + refl * 0.4f;
                arrow.RigidBody.velocity = refl * arrow.speed * 0.025f;
                float rndX = UnityEngine.Random.Range(-180, 180);
                float rndY = UnityEngine.Random.Range(-180, 180);
                float rndZ = UnityEngine.Random.Range(-180, 180);
                arrow.RigidBody.MoveRotation(
                    Quaternion.Euler(new Vector3(rndX, rndY, rndZ)));
            }
            arrow.parent = null;
            // if has non kinematic rigidbody add push based on arrow hit power
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb)
            {
                if (!rb.isKinematic)
                {
                    rb.AddForce(arrow.velocity, ForceMode.Acceleration);
                }
            }
        }

        /// <summary>
        /// method on arrow hit flesh material ( calling hit reaction )
        /// </summary>
        /// <param name="arrow">current arrow</param>
        /// <param name="rayhit">raycasthit info struct</param>
        public void OnArrowHitFleshEvent(Arrow arrow, RaycastHit rayhit)
        {
            BodyColliderScript bcs = rayhit.collider.GetComponent<BodyColliderScript>();
            if (bcs)
            {
#if DEBUG_INFO

                    if(!bcs.ParentRagdollManager)Debug.LogWarning("Cannot find 'RagdollManager' component on 'BodyColliderScript'.");

#endif
                

                if (bcs.ownerGameCharacter != null)
                {
                    int[] parts = new int[] { bcs.index };
                    bool block = false;
                    bcs.ownerGameCharacter.attack_hit_notify(arrow.owner, -1, 0, ref block, false, arrow.velocity / 12.0f, parts);

                    if (!bcs.ownerGameCharacter.isDead)
                    {
                        
                        bcs.ParentRagdollManager.startHitReaction(parts, arrow.velocity / 4.0f);
                    }

                }
                else
                {
                    Debug.LogWarning("arrow victim does not hold IGameCharacter reference");
                }

            }
#if DEBUG_INFO
            else
            {
                Debug.LogWarning("Cannot find 'BodyColliderScript' on flesh material collider.");
            }
#endif
            
        }
    } 
}