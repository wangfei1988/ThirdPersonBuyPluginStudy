// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// base trigger class
    /// NOTE: Be carefull with state names, they are used for matching target
    /// Remeber to change matching target name strings on triggers if state name is changed
    /// </summary>
    public abstract class Trigger : MonoBehaviour
    {
        /// <summary>
        /// target transform
        /// </summary>
        [Tooltip("Trigger target.")]
        public Transform Target;        

        /// <summary>
        /// switch target transform direction
        /// </summary>
        [Tooltip("Switch trigger forward direction.")]
        public bool switchTargetDirection = false;

        /// <summary>
        /// angle condition of character forward direction to target transform direction ( or back if switched )
        /// </summary>
        [Tooltip ("Trigger will trigger itself if player is looking towards under trigger angle.")]
        public float angleCondition = 75;

        /// <summary>
        /// show display text info on trigger upon 
        /// enter ?
        /// </summary>
        [Tooltip("Show trigger info text.")]
        public bool showInfoText = true;

        protected TriggerInfo m_TriggerInfo = null;     // trigger additional data

        /// <summary>
        /// currently colliding with character flag
        /// </summary>
        [HideInInspector ]
        public bool colliding = false;

        protected bool m_ConditionPassed = false;           // condition passed flag

        /// <summary>
        /// gets trigger additional data
        /// </summary>
        public TriggerInfo triggerData { get { return m_TriggerInfo; } }

        /// <summary>
        /// initialize trigger
        /// </summary>
        public virtual void initialize()
        {
            m_TriggerInfo = new TriggerInfo();
        }

        /// <summary>
        /// early exit conditions
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns>condition</returns>
        public virtual bool condition(TPCharacter character)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return false;
            }
            m_ConditionPassed = false;
            Vector3 charDir = character.transform.forward;
            Vector3 thisDir = transform.forward;
            float angle = Vector3.Angle(charDir, thisDir);
            if (angle <= angleCondition) m_ConditionPassed = true;
            return m_ConditionPassed;
        }

        /// <summary>
        /// get display info text
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns>returns info string</returns>
        public virtual string get_info_text(TPCharacter character)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return "ERROR";
            }
            if (m_ConditionPassed)
                return "Press 'Use'";
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
        public abstract bool start(TPCharacter character, IKHelper limbsIK, bool use, bool secondaryUse, bool jump, float v, float h);


        /// <summary>
        /// end trigger animations
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        public abstract void end(TPCharacter character, IKHelper limbsIK);

        /// <summary>
        /// executes on match target start
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        public virtual void onMatchTargetStart(TPCharacter character, IKHelper limbsIK)
        {

            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null. " + " < " + this.ToString() + ">");
#endif
                return;
            }
#if DEBUG_INFO
            if (!character.audioManager)
            {
                Debug.LogError("object cannot be null. " + " < " + this.ToString() + ">");
                return;
            }
#endif
            character.audioManager.playJumpSound();
            m_TriggerInfo.rot_time = 0.0f;
            m_TriggerInfo.doMatchTarget = true;
            m_TriggerInfo.startRotation = character.transform.rotation;
         
        }

        /// <summary>
        /// executes on match target end
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper</param>
        public virtual void onMatchTargetEnd(TPCharacter character, IKHelper limbsIK)
        {
        }

        /// <summary>
        /// exit trigger
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        public virtual void exit(TPCharacter character)
        {
        }

        /// <summary>
        /// get closest point to this trigger
        /// return null to disable
        /// </summary>
        /// <param name="toPoint">closest to this point</param>
        /// <returns>nullable vector3</returns>
        public virtual Vector3? closestPoint(Vector3 toPoint)
        {
            return null;
        }

        /// <summary>
        /// set rotation slerping max time
        /// </summary>
        /// <param name="limit"></param>
        public void setRotation2TargetLimit(float limit)
        {
#if DEBUG_INFO
            if(!m_TriggerInfo)
            {
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_TriggerInfo.rot_maxTime = limit;
        }
    } 
}
