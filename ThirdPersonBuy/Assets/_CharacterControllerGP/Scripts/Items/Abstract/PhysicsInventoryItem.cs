// © 2016 Mario Lelas
using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{
    /// <summary>
    /// information on item  joints if they exist
    /// </summary>
    public struct JointInfo
    {
        /// <summary>
        /// original linear motion limit on X axis
        /// </summary>
        public ConfigurableJointMotion OrigX;

        /// <summary>
        /// original linear motion limit on Y axis
        /// </summary>
        public ConfigurableJointMotion OrigY;

        /// <summary>
        /// original linear motion limit on Z axis
        /// </summary>
        public ConfigurableJointMotion OrigZ;


        /// <summary>
        /// original angular motion limit on X axis
        /// </summary>
        public ConfigurableJointMotion OrigAngX;

        /// <summary>
        /// original angular motion limit on Y axis
        /// </summary>
        public ConfigurableJointMotion OrigAngY;

        /// <summary>
        /// original angular motion limit on Z axis
        /// </summary>
        public ConfigurableJointMotion OrigAngZ;
    }

    /// <summary>
    /// inventory item base class for items that have physics properties
    /// ( rigid body, collider etc... )
    /// </summary>
    public abstract class PhysicsInventoryItem : InventoryItem
    {
        /// <summary>
        /// should item scale be reset to one upon equipping
        /// </summary>
        [Tooltip("should item scale be reset to one upon equipping. choose yourself.")]
        public bool keepScaling = true;

        /// <summary>
        /// trigger collider object for detecting if character has collided with it to pick it up
        /// </summary>
        [Tooltip("Trigger collider object for detecting if character has collided with it to pick it up.")]
        public GameObject triggerObject;                 

        protected Collider[] m_Colliders;                 // item collider array
        protected Rigidbody[] m_Rigid_bodies;             // item rigid body array
        protected ConfigurableJoint[] m_Config_joints;    // item configurable joints array


        protected List<bool> m_IsKinematicList;        // list to hold rigid bodies information for reseting
        protected List<bool> m_IsTriggerList;          // list to hold colliders information for reseting
        protected JointInfo[] m_JointInfos = null;     // information on joints for reseting
        protected float m_DropVelMult = 4.0f;          // velocity of item on drop

        protected Vector3[] m_origPositions;               // original bone positions on initialization
        protected Quaternion[] m_origRotations;            // original bone rotations on initialization
        protected Vector3[] m_origScales;                  // original bone scales on initialization
        protected Transform[] m_Transforms;                 // references to transforms

        protected Transform m_OrigParent;                   // original parent of item

        /// <summary>
        /// initialize class 
        /// </summary>
        public override bool initialize()
        {
            if (m_Initialized) return true;


            m_ItemGO = this.gameObject;

            m_Colliders = GetComponentsInChildren<Collider>(true);
            m_Rigid_bodies = GetComponentsInChildren<Rigidbody>(true);
            m_Config_joints = GetComponentsInChildren<ConfigurableJoint>(true);

            m_Transforms = GetComponentsInChildren<Transform>();
            m_origPositions = new Vector3[m_Transforms.Length];
            m_origRotations = new Quaternion[m_Transforms.Length];
            m_origScales = new Vector3[m_Transforms.Length];
            for (int i = 0; i < m_Transforms.Length; i++)
            {
                m_origPositions[i] = m_Transforms[i].position;
                m_origRotations[i] = m_Transforms[i].rotation;
                m_origScales[i] = m_Transforms[i].localScale;
            }

            if (m_Colliders.Length > 0)
            {
                m_IsTriggerList = new List<bool>();
                for (int i = 0; i < m_Colliders.Length; i++)
                {
                    if (m_Colliders[i] != null) m_IsTriggerList.Add(m_Colliders[i].isTrigger);
                    else m_IsTriggerList.Add(false);
                }
            }
            if (m_Rigid_bodies.Length > 0)
            {
                m_IsKinematicList = new List<bool>();
                for (int i = 0; i < m_Rigid_bodies.Length; i++)
                {
                    if (m_Rigid_bodies[i] != null) m_IsKinematicList.Add(m_Rigid_bodies[i].isKinematic);
                    else m_IsKinematicList.Add(false);
                }
            }

            if (m_Config_joints.Length > 0)
            {
                m_JointInfos = new JointInfo[m_Config_joints.Length];
                for (int i = 0; i < m_Config_joints.Length; ++i)
                {
                    ConfigurableJoint joint = m_Config_joints[i];
                    m_JointInfos[i].OrigX = joint.xMotion;
                    m_JointInfos[i].OrigY = joint.yMotion;
                    m_JointInfos[i].OrigZ = joint.zMotion;
                    m_JointInfos[i].OrigAngX = joint.angularXMotion;
                    m_JointInfos[i].OrigAngY = joint.angularYMotion;
                    m_JointInfos[i].OrigAngZ = joint.angularZMotion;
                }
            }

            m_OrigParent = transform.parent;

            // try to find trigger object on child transforms
            if (!triggerObject)
            {
                Transform trigXform = Utils.FindChildTransformByName(transform, "ItemTrigger");
                if(trigXform && trigXform.gameObject .activeSelf )
                    triggerObject = trigXform.gameObject;
            }
            //if(!triggerObject)
            //{
            //    Debug.LogWarning("Cannot find item trigger object on " + this.itemName);
            //}
            m_Initialized = true;
            return true;
        }

        /// <summary>
        /// reset item 
        /// </summary>
        public override void resetItem()
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return;
            }

            if (triggerObject) triggerObject.SetActive(true);

            transform.SetParent(m_OrigParent, true);

            for (int i = 0; i < m_Transforms.Length; i++)
            {
                m_Transforms[i].position = m_origPositions[i];
                m_Transforms[i].rotation = m_origRotations[i];
                m_Transforms[i].localScale = m_origScales[i];
            }

            _resetRB();
            _resetJoints();
            this.equipped = false;
            this.m_ItemGO.SetActive(true);
        }

        /// <summary>
        /// reset item to position / rotation
        /// </summary>
        public override void dropItem(Vector3? pos, Quaternion? rot)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return;
            }

            if (triggerObject) triggerObject.SetActive(true);

            transform.parent = null;
            for (int i = 0; i < m_Transforms.Length; i++)
            {
                m_Transforms[i].position = m_origPositions[i];
                m_Transforms[i].rotation = m_origRotations[i];
                m_Transforms[i].localScale = m_origScales[i];
            }

            if (pos.HasValue) transform.position = pos.Value;
            if (rot.HasValue) transform.rotation = rot.Value;

            _dropRB(Vector3.up * m_DropVelMult);
            this.equipped = false;
            this.m_ItemGO.SetActive(true);

            if (m_Config_joints != null)
            {
                // no need for joints upon dropping
                for (int i = 0; i < m_Config_joints.Length; ++i)
                {
                    ConfigurableJoint joint = m_Config_joints[i];
                    Destroy(joint);
                }
                m_Config_joints = null;
            }
        }

        /// <summary>
        /// setup item states for equipping
        /// </summary>
        public override void equipSetup()
        {
            if (triggerObject) triggerObject.SetActive(false);
            setupPhysicsForWearing();
        }

        /// <summary>
        /// setup item physics ( disabling) for wearing
        /// </summary>
        /// <param name="detectCollisions"></param>
        public void setupPhysicsForWearing(bool detectCollisions = false)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return;
            }
#if DEBUG_INFO
            if (m_Rigid_bodies == null) { Debug.LogError("object cannot be null - " + this.name); return; }
            if (m_Colliders == null) { Debug.LogError("object cannot be null - " + this.name); return; }
#endif
            if(triggerObject)triggerObject.SetActive(false); 

            for (int i = 0; i < m_Rigid_bodies.Length; i++)
            {
                m_Rigid_bodies[i].detectCollisions = detectCollisions;
                m_Rigid_bodies[i].isKinematic = true;
            }
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                if (m_Colliders[i] != null)
                {
                    m_Colliders[i].enabled = false;
                }
            }
            if (m_Config_joints != null)
            {
                for (int i = 0; i < m_Config_joints.Length; ++i)
                {
                    m_Config_joints[i].xMotion = ConfigurableJointMotion.Free;
                    m_Config_joints[i].yMotion = ConfigurableJointMotion.Free;
                    m_Config_joints[i].zMotion = ConfigurableJointMotion.Free;
                    m_Config_joints[i].angularXMotion = ConfigurableJointMotion.Free;
                    m_Config_joints[i].angularYMotion = ConfigurableJointMotion.Free;
                    m_Config_joints[i].angularZMotion = ConfigurableJointMotion.Free;
                }
            }
        }

        /// <summary>
        /// resets items rigid bodies and colliders to original setup
        /// </summary>
        protected virtual void _resetRB()
        {
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                if (m_Colliders[i])
                {
                    m_Colliders[i].enabled = true;
                    m_Colliders[i].isTrigger = m_IsTriggerList[i];
                }
            }
            for (int i = 0; i < m_Rigid_bodies.Length; i++)
            {
                if (m_Rigid_bodies[i] != null)
                {
                    m_Rigid_bodies[i].detectCollisions = true;
                    m_Rigid_bodies[i].isKinematic = m_IsKinematicList[i];
                    if (!m_Rigid_bodies[i].isKinematic) m_Rigid_bodies[i].velocity = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// creating setup for item drop
        /// </summary>
        /// <param name="velocity">drop velocity</param>
        protected virtual void _dropRB(Vector3 velocity)
        {
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                if (m_Colliders[i])
                {
                    m_Colliders[i].enabled = true;
                    m_Colliders[i].isTrigger = m_IsTriggerList[i];
                }
            }
            for (int i = 0; i < m_Rigid_bodies.Length; i++)
            {
                if (m_Rigid_bodies[i] != null)
                {
                    m_Rigid_bodies[i].detectCollisions = true;
                    m_Rigid_bodies[i].isKinematic = false;
                    if (!m_Rigid_bodies[i].isKinematic) m_Rigid_bodies[i].velocity = velocity;
                }
            }
        }

        /// <summary>
        /// reset configurable joints if any exist
        /// </summary>
        protected void _resetJoints()
        {
            if (m_Config_joints != null)
            {
                for (int i = 0; i < m_Config_joints.Length; ++i)
                {
                    ConfigurableJoint joint = m_Config_joints[i];
                    joint.xMotion = m_JointInfos[i].OrigX;
                    joint.yMotion = m_JointInfos[i].OrigY;
                    joint.zMotion = m_JointInfos[i].OrigZ;
                    joint.angularXMotion = m_JointInfos[i].OrigAngX;
                    joint.angularYMotion = m_JointInfos[i].OrigAngY;
                    joint.angularZMotion = m_JointInfos[i].OrigAngZ;
                }
            }
        }
    } 
}
