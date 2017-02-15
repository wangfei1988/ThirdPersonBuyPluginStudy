// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    ///  TEMP UNTIL I MAKE CLIMB ON LADDERS OF ANY SIZE
    /// </summary>
    public enum LadderSize
    {
        Short = 0,
        Medium = 1,
        Long = 2
    }

    /// <summary>
    /// climb on ladders trigger
    /// </summary>
    public class LadderTrigger : Trigger
    {
        // TEMP
        public UnityEngine.UI.Text DBGUI;


        /// <summary>
        /// going up or down the ladder ?
        /// </summary>
        [Tooltip ("Going up or down ?")]
        public bool GoingUp = true;

        /// <summary>
        /// ladder size ( long = 2, medium = 1, short = 0)
        /// </summary>
        [Tooltip("Ladder size ( long, medium , short ) ?")]
        public LadderSize LadderSize = LadderSize.Short ;

        /// <summary>
        /// Left hand holding position transform. Helper for stabilizing left hand
        /// </summary>
        [Tooltip("Helper for stabilizing left hand.")]
        public Transform LeftHandHold;

        /// <summary>
        /// Right hand holding position transform. Helper for stabilizing right hand
        /// </summary>
        [Tooltip("Helper for stabilizing right hand.")]
        public Transform RightHandHold;

        /// <summary>
        /// Left foot position transform. Helper for stabilizing left foot
        /// </summary>
        [Tooltip("Helper for stabilizing left foot.")]
        public Transform LeftFootHold;

        /// <summary>
        /// Right foot position transform. Helper for stabilizing right foot
        /// </summary>
        [Tooltip("Helper for stabilizing left foot.")]
        public Transform RightFootHold;


        private FootstepsAudio ladderSounds; // sound clips that plays on ladder step

        /// <summary>
        /// initialize trigger
        /// </summary>
        public override void initialize()
        {
            base.initialize();
            ladderSounds = GetComponent<FootstepsAudio>();
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
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return "ERROR";
            }
            if (m_ConditionPassed)
                return "Press 'Use' to climb ladder";
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

            //if (!m_TriggerInfo) m_TriggerInfo = new TriggerInfo();

            m_TriggerInfo.doMatchTarget = false;
            m_TriggerInfo.doRotationToTarget = false;

            Vector3 direction = GoingUp ? Target.forward : -Target.forward;

            float distanceFromTarget = (character.transform.position - Target.position).magnitude;
            float angle = Vector3.Angle(character.transform.forward, direction) * 0.002f;
            float pos_rot_time = distanceFromTarget + angle;

            if (pos_rot_time < 0.1f)
            {
                if (limbsIK)
                {
                    limbsIK.handIKMode = IKMode.ToLine;
                    limbsIK.setIKDirection(true);

                    if (LeftHandHold)
                    {
                        limbsIK.LeftHandPtA = LeftHandHold.position - LeftHandHold.up * 100.0f;
                        limbsIK.LeftHandPtB = LeftHandHold.position + LeftHandHold.up * 100.0f;
                        limbsIK.LHandIKEnabled = true;
                        limbsIK.LHWeightTime = 0.0f;
                    }
                    else
                    {
                        limbsIK.LHandIKEnabled = false;
                    }


                    if (RightHandHold)
                    {
                        limbsIK.RightHandPtA = RightHandHold.position - RightHandHold.up * 100.0f;
                        limbsIK.RightHandPtB = RightHandHold.position + RightHandHold.up * 100.0f;
                        limbsIK.RHandIKEnabled = true;
                        limbsIK.RHWeightTime = 0.0f;
                    }
                    else
                    {
                        limbsIK.RHandIKEnabled = false;
                    }



                    limbsIK.feetIKMode = IKMode.ToPlane;
                    if (LeftFootHold)
                    {
                        Vector3 lpt1 = LeftFootHold.position;
                        Vector3 lpt2 = lpt1 + LeftFootHold.up;
                        Vector3 lpt3 = lpt1 + LeftFootHold.forward;
                        limbsIK.LeftFootPlane.Set3Points(lpt1, lpt2, lpt3);
                        limbsIK.LFootIKEnabled = true;
                    }
                    else
                    {
                        limbsIK.LFootIKEnabled = false;
                    }

                    if (RightFootHold)
                    {
                        Vector3 rpt1 = RightFootHold.position;
                        Vector3 rpt2 = rpt1 + RightFootHold.up;
                        Vector3 rpt3 = rpt1 + RightFootHold.forward;
                        limbsIK.RightFootPlane.Set3Points(rpt1, rpt2, rpt3);
                        limbsIK.RFootIKEnabled = true;
                    }
                    else
                    {
                        limbsIK.RFootIKEnabled = false;
                    }
                }

                

                character.disablePhysics(false, true);
                character.OnSetTransform = () =>
                {
                    //set steps audio clips to character 
                    character.audioManager.ladderClips = ladderSounds;

                    character.disablePhysics(false, true);
                    if (GoingUp) character.animator.SetBool(/*"pLadderUp"*/HashIDs.LadderUpBool, true);
                    else character.animator.SetBool(/*"pLadderDown"*/HashIDs.LadderDownBool, true);
                    character.animator.SetInteger(/*"pLadderSize"*/HashIDs.LadderSizeInt, (int)LadderSize);
                    character.OnSetTransform = null;
                };

                Quaternion targetRot = Target.rotation;
                if (switchTargetDirection)
                    targetRot *= Quaternion.AngleAxis(180, character.transform.up);
                if (!GoingUp)
                    targetRot *= Quaternion.AngleAxis(180, character.transform.up);
                character.setTransform(Target.position, targetRot, true);


                character.triggerRootMotion = true;
            }
            else
            {
                float forw = 0.5f;
                bool crouch = false;
                character.OnLerpEnd = () =>
                {
                    if (limbsIK)
                    {
                        limbsIK.handIKMode = IKMode.ToLine;
                        limbsIK.setIKDirection(true);
                        if (LeftHandHold)
                        {
                            limbsIK.LeftHandPtA = LeftHandHold.position - LeftHandHold.up * 100.0f;
                            limbsIK.LeftHandPtB = LeftHandHold.position + LeftHandHold.up * 100.0f;
                            limbsIK.LHandIKEnabled = true;
                            limbsIK.LHWeightTime = 0.0f;
                        }
                        else
                        {
                            limbsIK.LHandIKEnabled = false;
                        }
                        if (RightHandHold)
                        {
                            limbsIK.RightHandPtA = RightHandHold.position - RightHandHold.up * 100.0f;
                            limbsIK.RightHandPtB = RightHandHold.position + RightHandHold.up * 100.0f;
                            limbsIK.RHandIKEnabled = true;
                            limbsIK.RHWeightTime = 0.0f;
                        }
                        else
                        {
                            limbsIK.RHandIKEnabled = false;
                        }
                        limbsIK.feetIKMode = IKMode.ToPlane;
                        if (LeftFootHold)
                        {
                            Vector3 lpt1 = LeftFootHold.position;
                            Vector3 lpt2 = lpt1 + LeftFootHold.up;
                            Vector3 lpt3 = lpt1 + LeftFootHold.forward;
                            limbsIK.LeftFootPlane.Set3Points(lpt1, lpt2, lpt3);
                            limbsIK.LFootIKEnabled = true;
                        }
                        else
                        {
                            limbsIK.LFootIKEnabled = false;
                        }
                        if (RightFootHold)
                        {
                            Vector3 rpt1 = RightFootHold.position;
                            Vector3 rpt2 = rpt1 + RightFootHold.up;
                            Vector3 rpt3 = rpt1 + RightFootHold.forward;
                            limbsIK.RightFootPlane.Set3Points(rpt1, rpt2, rpt3);
                            limbsIK.RFootIKEnabled = true;
                        }
                        else
                        {
                            limbsIK.RFootIKEnabled = false;
                        }
                    }

                    //set steps audio clips to character 
                    character.audioManager.ladderClips = ladderSounds;


                    character.disablePhysics(false, true);
                    if (GoingUp) character.animator.SetBool(/*"pLadderUp"*/HashIDs.LadderUpBool, true);
                    else character.animator.SetBool(/*"pLadderDown"*/HashIDs.LadderDownBool, true);
                    character.animator.SetInteger(/*"pLadderSize"*/HashIDs.LadderSizeInt, (int)LadderSize);
                    character.transform.position = Target.position;
                    character.triggerRootMotion = true;
                    character.OnLerpEnd = null;
                };
                character.disableMove = true;
                Quaternion targetRot = Target.rotation;
                if (switchTargetDirection)
                    targetRot *= Quaternion.AngleAxis(180, character.transform.up);
                if (!GoingUp)
                    targetRot *= Quaternion.AngleAxis(180, character.transform.up);
                character.lerpToTransform(
                       Target.position,
                      targetRot,
                        pos_rot_time,
                        pos_rot_time,
                        forw,
                        crouch
                        );
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

            // reset steps audio clips to character 
            character.audioManager.ladderClips = null;
            character.triggerRootMotion = false;
            character.disableMove = false;
            character.enablePhysics();
            if (GoingUp) character.animator.SetBool(/*"pLadderUp"*/HashIDs.LadderUpBool, false);
            else character.animator.SetBool(/*"pLadderDown"*/HashIDs.LadderDownBool, false);

            Vector3 right = character.transform.right;
            character.transform.up = Vector3.up; 
            character.transform.right = right;

            if (limbsIK)
            {
                limbsIK.handIKMode = IKMode.Default;
                limbsIK.feetIKMode = IKMode.Default;
                limbsIK.setIKDirection(false);

                limbsIK.OnReachZeroLH = () =>
                 {
                     limbsIK.LHandIKEnabled = false;
                     limbsIK.OnReachZeroLH = null;
                 };
                limbsIK.OnReachZeroRH = () =>
                {
                    limbsIK.RHandIKEnabled = false;
                    limbsIK.OnReachZeroRH = null;
                };
                limbsIK.LFootIKEnabled = false;
                limbsIK.RFootIKEnabled = false;
            }
        }


#if DEBUG_INFO
        // draw helper lines
        void OnDrawGizmosSelected()
        {
            if (
                !LeftHandHold ||
                !RightHandHold ||
                !LeftFootHold ||
                !RightFootHold
                )
                return;
            Gizmos.color = Color.blue;

            float size = 1.5f;
            if (LadderSize == LadderSize.Medium)
                size = 3.0f;
            else if (LadderSize == LadderSize.Long)
                size = 6.0f;

            Vector3 ldown = LeftHandHold.position - LeftHandHold.up * size;
            Vector3 lup = LeftHandHold.position + LeftHandHold.up * size;
            Vector3 rdown = RightHandHold.position - RightHandHold.up * size;
            Vector3 rup = RightHandHold.position + RightHandHold.up * size;
            Gizmos.DrawLine(ldown, lup);
            Gizmos.DrawLine(rdown, rup);


            ldown = LeftFootHold.position - LeftFootHold.up * size;
            lup = LeftFootHold.position + LeftFootHold.up * size;
            rdown = RightFootHold.position - RightFootHold.up * size;
            rup = RightFootHold.position + RightFootHold.up * size;
            Gizmos.DrawLine(ldown, lup);
            Gizmos.DrawLine(rdown, rup);

        }
#endif

    }
}