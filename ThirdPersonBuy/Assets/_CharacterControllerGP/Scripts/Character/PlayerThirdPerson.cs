// © 2016 Mario Lelas
using UnityEngine;
namespace MLSpace
{
    /// <summary>
    /// controls camera and head ik, switching between stand and crouch camera targets
    /// derived from 'Player' class
    /// </summary>
    public class PlayerThirdPerson : Player
    {
        /// <summary>
        /// camera lookAt position when standing
        /// </summary>
        public Transform standCameraTarget;

        /// <summary>
        ///  camera crouch lookAt position target
        /// </summary>
        public Transform crouchCameraTarget;

        /// <summary>
        /// initialize component
        /// </summary>
        public override void initialize()
        {
            if (m_Initialized) return;
            base.initialize();
            if (!m_Initialized) return;

            if (!m_Camera) { Debug.LogError("Camera not assigned. " + " < " + this.ToString() + ">"); return; }
            if (!standCameraTarget) { Debug.LogError("Standing camera target not assigned. " + " < " + this.ToString() + ">"); return; }
            if (!crouchCameraTarget) { Debug.LogError("Crouch camera target not assigned." + " < " + this.ToString() + ">"); return; }

            m_Camera.Target = standCameraTarget;

            m_Initialized = true;
        }

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }


        /// <summary>
        /// Control player
        /// </summary>
        /// <param name="horiz">horizonal axis value</param>
        /// <param name="vert">vertical axis value 这两个是摄像机</param>
        /// <param name="jump">jump flag</param>
        /// <param name="runToggle">toggle walk / run mode</param>
        /// <param name="dive">dive roll flag</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="bodyLookDirection">body look direction，我们往hv方向移动，那么body目标方向就是hv</param>
        /// <param name="diveDirection">dive roll direction</param>
        public override void control(float horiz, float vert, bool jump, bool runToggle, bool dive, bool crouch,
            Vector3? bodyLookDirection = null, Vector3? diveDirection = null, float? side = null)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            // MOVE SECTION
            Vector3 move = Vector3.zero;
            Vector3 forwDir = transform.forward;
            Vector3 rightDir = transform.right;
            if (m_Camera.transform != null)
            {
                forwDir = m_Camera.transform.forward;
                rightDir = m_Camera.transform.right;
            }
            move = vert * forwDir + horiz * rightDir;
            control(move, jump, runToggle, dive, crouch, bodyLookDirection, diveDirection, side);
        }

        /// <summary>
        /// Control player
        /// </summary>
        /// <param name="moveVelocity">move velocity</param>
        /// <param name="jump">jump flag</param>
        /// <param name="runToggle">toggle walk / run mode</param>
        /// <param name="dive">dive roll flag</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="bodyLookDirection">body look direction</param>
        /// <param name="diveDirection">dive roll direction</param>
        public override void control(Vector3 moveVelocity, bool jump, bool runToggle, bool dive, bool crouch, 
            Vector3? bodyLookDirection = null, Vector3? diveDirection = null,float? side = null)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            //只要受到ragdoll的影响（被击中或者爬楼梯或者摔下） 就不可以移动跳跃翻滚
            if (m_Ragdoll.state != RagdollManager.RagdollState.Animated) return;

            if (m_DisableInput)
            {
                m_Character.move(Vector3.zero, false, false, false, Vector3.zero, Vector3.zero, null, null);
                return;
            }
            if (m_Character.isDiving)
            {
                m_Character.move(Vector3.zero, false, false, false, Vector3.zero, Vector3.zero, null, null);
                return;
            }

            // ignore jumps if holding on ledge
            bool onLedge = m_Character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
            jump = jump & !onLedge;

            // MOVE SECTION
            Vector3 move = moveVelocity;
            if (move.magnitude > 1f) move.Normalize();
            float walkMultiplier = runToggle ? 1f : 0.5f;
            move *= walkMultiplier;

            if (lookTowardsCamera) //人头（人眼）前方和摄像机保持一致
            {
                if (m_LookTowardsCamera) //m_LookTowardsCamera 人头是否已经转和目标摄像机一致了
                {
                    //当前头的前方朝向哪个方向
                    m_CurrentHeadPos = m_Camera.transform != null
                      ? transform.position + m_Camera.transform.forward * 100
                      : transform.position + transform.forward * 100;
                }
                else
                {
                    if (m_LerpHeadPosition) //当前是否正在砖头
                    {
                        if (m_Switch2CameraLook)
                        {
                            m_HeadEndPos = m_Camera.transform != null
                                  ? transform.position + m_Camera.transform.forward * 100
                                  : transform.position + transform.forward * 100;
                        }

                        m_HeadSwitchTime += Time.deltaTime * m_HeadLookSpeed;
                        float lValue = m_HeadSwitchTime / m_HeadSwitchMaxTime;
                        lValue = Mathf.Clamp01(lValue);
                        m_CurrentHeadPos = Vector3.Lerp(m_HeadStartPos, m_HeadEndPos, lValue);
                        //逐步修改m_CurrentHeadPos
                        if (m_HeadSwitchMaxTime < m_HeadSwitchTime)
                        {
                            m_LerpHeadPosition = false;
                            if (m_Switch2CameraLook)
                            {
                                m_Switch2CameraLook = false;
                                m_LookTowardsCamera = true;
                            }
                        }
                    }
                }
            }
            else  //人头（人眼）前方不需要和摄像机保持一致
            {
                if (m_LerpHeadPosition) //头要和摄像机前方保持一致
                {
                    if (m_Switch2CameraLook)
                    {
                        m_HeadEndPos = transform.position + transform.forward * 100.0f;
                    }

                    m_HeadSwitchTime += Time.deltaTime * m_HeadLookSpeed;
                    float lValue = m_HeadSwitchTime / m_HeadSwitchMaxTime;
                    lValue = Mathf.Clamp01(lValue);
                    m_CurrentHeadPos = Vector3.Lerp(m_HeadStartPos, m_HeadEndPos, lValue);
                    if (m_HeadSwitchMaxTime < m_HeadSwitchTime)
                    {
                        m_LerpHeadPosition = false;
                        if (m_Switch2CameraLook)
                        {
                            m_Switch2CameraLook = false;
                            m_LookTowardsCamera = true;
                            noLookIK();
                        }
                    }
                }
            }

//#if DEBUG_INFO
//            // head look debug 
//            Vector3 headPos = m_Character.animator.GetBoneTransform(HumanBodyBones.Head).position;
//            Debug.DrawLine(headPos, m_CurrentHeadPos, Color.blue);
//            Debug.DrawLine(headPos, m_HeadStartPos, Color.yellow);
//            Debug.DrawLine(headPos, m_HeadEndPos, Color.green);
//#endif

            Vector3 bodyLookDir = move; //move往哪个方向移动
            //上面是人眼下面是人身体，人身体方向必须和移动方向一致
            if (bodyLookDirection.HasValue) 
            {
                bodyLookDir = bodyLookDirection.Value;
            }
            if (m_Strafing && !m_Character.isCrouching)
            {
                bodyLookDir = m_Camera.transform.forward;
            }

            m_Character.move(move, crouch, jump, dive, bodyLookDir, m_CurrentHeadPos, side, diveDirection);


            if (m_Character.isCrouching)
            {
                m_Camera.switchTargets(crouchCameraTarget, 2.0f);
            }
            else
            {
                m_Camera.switchTargets(standCameraTarget /*m_Camera.defaultCameraTarget*/, 2.0f);
            }

        }
    } 

}
