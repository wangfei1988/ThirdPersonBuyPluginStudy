// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{


    /// <summary>
    /// jump from ground to ledge and enter ledge mode
    /// </summary>
    public class Jump2LedgeTrigger : Trigger
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
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return false;
            }

            bool lookingTowards = base.condition(character);
            m_ConditionPassed =  character.isGroundMode && lookingTowards;
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
                return "Press 'Use' to jump to ledge";
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
            if(!Target)
            {
#if DEBUG_INFO
                Debug.LogWarning("trigger target not assigned!" +" < " + this.ToString() + ">");
#endif
                return false;
            }



            if (!use) return false;

            float scalez = Target.localScale.z * 0.5f; 
            character.setLedge(Target, false, -scalez);



            Vector3 checkpos = character.transform.position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint,
                character.ledge.rightPoint, ref checkpos);

            bool moveright = true;
            m_TriggerInfo.avatarTarget = AvatarTarget.RightHand;
            if (closest == character.ledge.leftPoint)
            {
                m_TriggerInfo.avatarTarget = AvatarTarget.LeftHand;
                moveright = false;
            }
            m_TriggerInfo.transform = Target;
            m_TriggerInfo.pos_offset = closest - Target.position;
            m_TriggerInfo.CatchPosition = closest;

            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds
            character.disableMove = true;
            character.rigidBody.velocity = Vector3.zero;

            if (limbsIK)
            {
                limbsIK.LHandIKEnabled = false;
                limbsIK.RHandIKEnabled = false;
                limbsIK.LFootIKEnabled = false;
                limbsIK.RFootIKEnabled = false;

                limbsIK.checkHang = false;

                limbsIK.handIKMode = IKMode.ToLine;
                limbsIK.LeftHandPtA = character.ledge.leftPoint;
                limbsIK.LeftHandPtB = character.ledge.rightPoint;
                limbsIK.RightHandPtA = character.ledge.leftPoint;
                limbsIK.RightHandPtB = character.ledge.rightPoint;


                Vector3 pos = character.transform.position;
                closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref pos);
                bool checkUnder = _checkIsWallUnderTarget(character, limbsIK, closest, Target, moveright);
                if (checkUnder)
                {
                    character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                    m_TriggerInfo.stateName = "Base Layer.LEDGE.JumpToLedgeState";
                    limbsIK.currentRelLedgePosition =
                        limbsIK.ledgeRelativePosition;
                }
                else
                {
                    character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                    m_TriggerInfo.stateName = "Base Layer.LEDGE.Jump2Ledge_Hang";
                    limbsIK.currentRelLedgePosition =
                           limbsIK.ledgeRelativePositionHang;
                }
            }
            else
            {
                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                m_TriggerInfo.stateName = "Base Layer.LEDGE.Jump2Ledge_Hang";
            }

            m_TriggerInfo.doMatchTarget = true;

            character.OnLerpEnd = () =>
            {
                character.disableMove = true;
                character.disablePhysics(true, true);
                character.animator.SetBool(/*"pGrabLedgeUp"*/HashIDs.GrabLedgeUpBool, true);
                character.jumpAllowed = false;

                Quaternion targetRot = Target.rotation;
                Vector3 targetEuler = targetRot.eulerAngles;
                targetEuler.z = 0.0f;
                targetEuler.x = 0.0f;
                if (switchTargetDirection)
                    targetRot *= Quaternion.AngleAxis(180, character.transform.up);
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
            character.lerpToTransform(null, fromEuler,
                    0f, 0.2f);
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
                return ;
            }
            if (!Target)
            {
#if DEBUG_INFO
                Debug.LogWarning("trigger target not assigned!" +" < " + this.ToString() + ">");
#endif
                return ;
            }


            character.triggerRootMotion = false;
            character.disableMove = false;
            character.setMoveMode(TPCharacter.MovingMode.Ledge);

            OrbitCameraController oc = character.GetComponentInChildren<OrbitCameraController>();
            if (oc)
            {
                oc.additiveRotation = true;
            }

            character.disableCapsuleScale = true;
            character.animator.SetBool(/*"pGrabLedgeUp"*/HashIDs.GrabLedgeUpBool, false);
            character.animator.SetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool, true);
            character.animatorDampTime = 0.0f;
            character.ledgeMove = true;
            character.disableLedgeConstraint = false;
            character.fullStop();
            character.jumpAllowed = true;

            if (limbsIK)
            {
                limbsIK.RHandIKEnabled = true;
                limbsIK.LFootIKEnabled = true;
                limbsIK.RFootIKEnabled = true;
                limbsIK.LHandIKEnabled = true;
                limbsIK.LFWeightTime = 0.0f;
                limbsIK.RFWeightTime = 0.0f;

                limbsIK.startLedgeAdjust(m_TriggerInfo.targetRotation); 
                limbsIK.checkHang = true;
            }
            if (character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool))
            {
                character.restoreCapsuleSize();
            }
            else
            {
                character.scaleCapsuleToHalf();
            }
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


            if (limbsIK)
            {
                limbsIK.LHandIKEnabled = true;
                limbsIK.LHWeightTime = 0.0f;
                limbsIK.RHandIKEnabled = true;
                limbsIK.RHWeightTime = 0.0f;
                limbsIK.LFootIKEnabled = false;
                limbsIK.RFootIKEnabled = false;
                limbsIK.setIKDirection(true);
            }
            m_TriggerInfo.doRotationToTarget = true;
            m_TriggerInfo.rot_time = 0.0f;


            base.onMatchTargetStart(character, limbsIK);
        }

        /// <summary>
        /// get closest point to this trigger
        /// return null to disable
        /// </summary>
        /// <param name="toPoint">closest to this point</param>
        /// <returns>nullable vector3</returns>
        public override Vector3? closestPoint(Vector3 toPoint)
        {
#if DEBUG_INFO
            if (!Target)
            {
                Debug.LogError("object cannot be null!" +" < " + this.ToString() + ">");
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

        /// <summary>
        /// check if there is wall in front of character
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbIK">ik helper class</param>
        /// <param name="inpos">input position</param>
        /// <param name="target">target transform</param>
        /// <param name="moveright">move right or left</param>
        /// <returns>returns true if wall is in front otherwise false</returns>
        private bool _checkIsWallUnderTarget(TPCharacter character, IKHelper limbIK, Vector3 inpos, Transform target, bool moveright = true)
        {
            Vector3 offset = new Vector3(0.0f, character.capsule.height * 0.5f, 0.25f);
            offset = target.rotation * offset;
            Vector3 pos = inpos - offset;
            float ledgeLength = (character.ledge.rightPoint - character.ledge.leftPoint).magnitude;
            Vector3 ledgeDir = character.getLedgeDirection();
            float dirOffset = Mathf.Min(ledgeLength, character.capsule.radius);
            if (moveright)
            {
                pos -= ledgeDir * dirOffset;
            }
            else
            {
                pos += ledgeDir * dirOffset;
            }
            Ray ray = new Ray(pos, target.forward);
            RaycastHit hit;
            int mask = character.layers;
            float radius = character.capsule.radius * 0.38f;
            float dist = limbIK.hangCheckDistance + 0.25f;
            if (Physics.SphereCast(ray, radius, out hit, dist, mask))
            {
                return true;
            }
            return false;
        }

    }


}
