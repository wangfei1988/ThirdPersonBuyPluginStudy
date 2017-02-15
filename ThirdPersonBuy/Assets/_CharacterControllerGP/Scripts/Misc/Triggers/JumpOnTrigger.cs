// © 2016 Mario Lelas
using UnityEngine;


namespace MLSpace
{
    /// <summary>
    /// jump to something trigger
    /// </summary>
    public class JumpOnTrigger : Trigger
    {
        /// <summary>
        /// enable / disable turn to ledge mode
        /// </summary>
        [Tooltip("Turn character to ledge and jump over.")]
        public bool turnToLedge = false;

        /// <summary>
        /// enable or disable right hand catch IK
        /// </summary>
        [Tooltip("Enable or disable right hand catch IK")]
        public bool enableCatchIK = true;

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
           
            initialize();
        }

        /// <summary>
        /// early exit conditions
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns>condition</returns>
        public override bool condition(TPCharacter character)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return false;
            }
            if (turnToLedge)
            {
                Vector3 charDir = character.transform.forward;
                Vector3? closest = closestPoint(character.transform.position);
                Vector3 direction = (closest.Value - character.transform.position);
                direction.y = 0.0f;
                direction.Normalize();
                float angle = Vector3.Angle(charDir, direction);
                bool lookingTowards = angle <= angleCondition;
                m_ConditionPassed = character.isGroundMode && lookingTowards;
            }
            else
            {
                bool lookingTowards = base.condition(character);
                m_ConditionPassed = character.isGroundMode && lookingTowards;
            }
            return m_ConditionPassed;
        }

        /// <summary>
        /// get display info text
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns></returns>
        public override string get_info_text(TPCharacter character)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return "ERROR";
            }
            if (m_ConditionPassed)
                return "Press 'Use' to jump on";
            return "";
        }

        /// <summary>
        /// start trigger interaction
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        /// <param name="use">use flag</param>
        /// <param name="jump">jump flag</param>
        /// <param name="v">vertical value</param>
        /// <param name="h">horizontal value</param>
        /// <returns>all succeded</returns>
        public override bool start(TPCharacter character, IKHelper limbsIK, bool use, bool secondaryUse, bool jump, float v, float h)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return false;
            }
            if (!Target)
            {
#if DEBUG_INFO
                Debug.LogWarning("trigger target not assigned!" + " < " + this.ToString() + ">");
#endif
                return false;
            }

            if (!use) return false;

            // example of IK 
            if (limbsIK && enableCatchIK )
            {
                limbsIK.RHandIKEnabled = true;
                limbsIK.RHWeightTime = 0.0f;
                limbsIK.setIKDirection(true);
                limbsIK.handIKMode = IKMode.Default;
                Transform rightHandT = character.animator.GetBoneTransform(HumanBodyBones.RightHand);
                Vector3? closest2RH = closestPoint(rightHandT.position);
                limbsIK.RHPosition = closest2RH.Value;
            }

            if (turnToLedge)
            {
                Vector3? closestMid = closestPoint(character.transform.position);
                Vector3 direction = (closestMid.Value - character.transform.position);
                direction.y = 0.0f;
                direction.Normalize();


                Vector3 pt1, pt2;
                float scalez = Target.localScale.z * 0.5f;
                bool face = Vector3.Dot(Target.forward, direction) < 0;
                Utils.CalculateLinePoints(Target, out pt1, out pt2, -scalez, face);
                Vector3 checkpos = character.transform.position;
                Vector3 closest = MathUtils.GetClosestPoint2Line(pt1, pt2, ref checkpos);

                m_TriggerInfo.avatarTarget = AvatarTarget.RightFoot; 

                m_TriggerInfo.transform = Target;
                m_TriggerInfo.pos_offset = closest - Target.position;
                m_TriggerInfo.stateName =  "Base Layer.TRIGGERS.JumpOn";
                character.disableMove = true;
                character.rigidBody.velocity = Vector3.zero;
                m_TriggerInfo.doMatchTarget = true;
                character.OnLerpEnd = () =>
                {
                    character.animator.SetBool(HashIDs.JumpOnBool, true);
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    m_TriggerInfo.targetRotation = targetRot;
                    m_TriggerInfo.startRotation = character.transform.rotation;
                    character.triggerRootMotion = true;

                    character.OnLerpEnd = null;
                };
                Quaternion rotation = Quaternion.LookRotation(direction);

                float forwAmt = Mathf.Min(character.forwardAmount, 0.5f);
                character.lerpToTransform(null, rotation,
                    0f, 0.2f, forwAmt, false, false);
            }
            else
            {
                Vector3 pt1, pt2;
                float scalez = Target.localScale.z * 0.5f;
                Utils.CalculateLinePoints(Target, out pt1, out pt2, -scalez, switchTargetDirection);
                Vector3 checkpos = character.transform.position;
                Vector3 closest = MathUtils.GetClosestPoint2Line(pt1, pt2, ref checkpos);


                m_TriggerInfo.avatarTarget = AvatarTarget.RightFoot; //RightHand;

                m_TriggerInfo.transform = Target;
                m_TriggerInfo.pos_offset = closest - Target.position;
                m_TriggerInfo.stateName =  "Base Layer.TRIGGERS.JumpOn";
                character.disableMove = true;
                character.rigidBody.velocity = Vector3.zero;
                m_TriggerInfo.doMatchTarget = true;

                character.OnLerpEnd = () =>
                {
                    character.animator.SetBool(HashIDs.JumpOnBool, true);
                    Quaternion targetRot = Target.rotation;
                    Vector3 targetEuler = targetRot.eulerAngles;
                    targetEuler.z = 0.0f;
                    targetEuler.x = 0.0f;
                    if (switchTargetDirection)
                        targetEuler.y += 180.0f;
                    Quaternion fromTargetEuler = Quaternion.Euler(targetEuler);
                    m_TriggerInfo.targetRotation = fromTargetEuler;
                    m_TriggerInfo.startRotation = character.transform.rotation;
                    character.triggerRootMotion = true;

                    character.OnLerpEnd = null;
                };
                Quaternion rotation = transform.rotation;
                Vector3 euler = rotation.eulerAngles;
                euler.z = 0.0f;
                euler.x = 0.0f;
                Quaternion fromEuler = Quaternion.Euler(euler);
                float forwAmt = Mathf.Min(character.forwardAmount, 0.5f);
                character.lerpToTransform(null, fromEuler,
                    0f, 0.2f, forwAmt, false, false);
            }
            return true;
        }

        /// <summary>
        /// end trigger animations
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        public override void end(TPCharacter character, IKHelper limbsIK)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return;
            }
            if (!Target)
            {
#if DEBUG_INFO
                Debug.LogWarning("trigger target not assigned!" + " < " + this.ToString() + ">");
#endif
                return;
            }
            if (limbsIK && enableCatchIK)
            {
                limbsIK.setIKDirection(false);
                limbsIK.OnReachZeroRH = () =>
                {
                    limbsIK.RHandIKEnabled = false;
                    limbsIK.OnReachZeroRH = null;
                };
            }
            character.triggerRootMotion = false;
            character.disableMove = false;
            character.enablePhysics();
            character.animator.SetBool(HashIDs.JumpOnBool, false);
        }

        /// <summary>
        /// executes on match target start
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        public override void onMatchTargetStart(TPCharacter character, IKHelper limbsIK)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return;
            }

            character.disablePhysics(true, true);
            m_TriggerInfo.doRotationToTarget = true;
            m_TriggerInfo.rot_time = 0.0f;
            base.onMatchTargetStart(character, limbsIK);
        }

        /// <summary>
        /// get closest point to this trigger transform target
        /// </summary>
        /// <param name="toPoint"></param>
        /// <returns></returns>
        public override Vector3? closestPoint(Vector3 toPoint)
        {
#if DEBUG_INFO
            if (!Target)
            {
                Debug.LogWarning("object cannot be null!" + " < " + this.ToString() + ">");
                return null;
            }
#endif
            Transform xform = Target;
            float scaleX = xform.lossyScale.x;
            float halfScaleX = scaleX * 0.5f;
            Vector3 PointA = xform.position - xform.right * halfScaleX;
            Vector3 PointB = xform.position + xform.right * halfScaleX;
            Vector3 closest = MathUtils.GetClosestPoint2Line(PointA, PointB, ref toPoint);
            return closest;
        }
    } 
}
