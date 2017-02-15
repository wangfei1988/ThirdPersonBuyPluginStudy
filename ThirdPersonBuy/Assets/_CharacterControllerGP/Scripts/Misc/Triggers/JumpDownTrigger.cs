// © 2016 Mario Lelas
using UnityEngine;


namespace MLSpace
{
    /// <summary>
    /// trigger firing jump down animation
    /// </summary>
    public class JumpDownTrigger : Trigger
    {
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
                Debug.LogError("object cannot be null!" +" < " + this.ToString() + ">");
#endif
                return false;
            }
            bool lookingTowards = base.condition(character);
            m_ConditionPassed = character.isGroundMode && lookingTowards;
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
                Debug.LogError("object cannot be null!" +" < " + this.ToString() + ">");
#endif
                return "ERROR";
            }
            if (m_ConditionPassed)
                return "Press 'Use' to jump down";
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
                Debug.LogError("object cannot be null!" +" < " + this.ToString() + ">");
#endif
                return false;
            }
            if (!Target)
            {
#if DEBUG_INFO
                Debug.LogWarning("trigger target not assigned!" +" < " + this.ToString() + ">");
#endif
                return false;
            }

            if (!use) return false;

            Vector3 pt1, pt2;
            float scalez = Target.localScale.z * 0.5f;
            Utils.CalculateLinePoints(Target, out pt1, out pt2, -scalez, switchTargetDirection);
            Vector3 checkpos = character.transform.position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(pt1, pt2, ref checkpos);

            m_TriggerInfo.transform = Target;
            m_TriggerInfo.pos_offset = closest - Target.position;
            character.disableMove = true;
            character.rigidBody.velocity = Vector3.zero;
            m_TriggerInfo.doMatchTarget = false;
            character.OnLerpEnd = () =>
            {
                character.animator.SetBool(/*"pJumpDown"*/HashIDs.JumpDownBool , true);
                character.triggerRootMotion = true;
                character.transform.position = closest;
                character.OnLerpEnd = null;
            };

            Quaternion rotation = transform.rotation;
            Vector3 euler = rotation.eulerAngles;
            euler.z = 0.0f;
            euler.x = 0.0f;
            Quaternion fromEuler = Quaternion.Euler(euler);
            if (switchTargetDirection)
                fromEuler *= Quaternion.AngleAxis(180, character.transform.up);


            float forwAmt = Mathf.Min(character.forwardAmount, 0.5f);
            character.lerpToTransform(closest, fromEuler,
                0.2f, 0.2f, forwAmt, false, false);
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
                Debug.LogError("object cannot be null!" +" < " + this.ToString() + ">");
#endif
                return;
            }
            if (!Target)
            {
#if DEBUG_INFO
                Debug.LogWarning("trigger target not assigned!" +" < " + this.ToString() + ">");
#endif
                return;
            }

            character.triggerRootMotion = false;
            character.disableMove = false;
            character.enablePhysics();
            character.animator.SetBool(/*"pJumpDown"*/HashIDs.JumpDownBool, false);
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
                Debug.LogError("object cannot be null!" +" < " + this.ToString() + ">");
#endif
                return;
            }
            character.disablePhysics(true, false, false);
            m_TriggerInfo.doRotationToTarget = false; 
            m_TriggerInfo.rot_time = 0.0f;
            base.onMatchTargetStart(character, limbsIK);
        }
    }

}