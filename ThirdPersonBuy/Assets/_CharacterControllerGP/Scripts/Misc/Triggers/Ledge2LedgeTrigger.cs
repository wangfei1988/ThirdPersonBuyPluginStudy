// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{



    /// <summary>
    /// various ledge triggers
    /// </summary>
    public class Ledge2LedgeTrigger : Trigger
    {
        /// <summary>
        /// type of ledge trigger
        /// </summary>
        public enum Mode { _2LedgeUp, _2LedgeDown, _2LedgeRight, _2LedgeLeft, _PullUp, _PullDown };

        /// <summary>
        /// jump down target transform
        /// </summary>
        [Tooltip("Assign transform to jump down.")]
        public Transform downTarget;

        /// <summary>
        /// switch down target ledge direction
        /// </summary>
        [Tooltip ("Switch down target direction.")]
        public bool switchDownDirection = false;

        /// <summary>
        /// jump right target transform
        /// </summary>
        [Tooltip("Assign transform to jump right.")]
        public Transform rightTarget;

        /// <summary>
        /// enable / disable edge requirement ( character must be on right edge of the ledge to accept jump )
        /// </summary>
        [Tooltip("Must be on right ledge edge to activate trigger.")]
        public bool rightEdgeReq = false;

        /// <summary>
        /// jump left target transform
        /// </summary>
        [Tooltip("Assign transform to jump left.")]
        public Transform leftTarget;

        /// <summary>
        /// enable / disable edge requirement ( character must be on left edge of the ledge to accept jump )
        /// </summary>
        [Tooltip("Must be on left ledge edge to activate trigger.")]
        public bool leftEdgeReq = false;

        /// <summary>
        /// pull up from ledge target transform
        /// </summary>
        [Tooltip("Assign transform to leave or enter ledge.")]
        public Transform pullUpTarget;

        /// <summary>
        /// maximum horizontal allowed distance from target 
        /// </summary>
        [Tooltip("Horizontal allowed limit from ledge to ledge.")]
        public float horizontalLimit = 0.1f;


        /// <summary>
        /// maximum vertical allowed distance from target 
        /// </summary>
        [Tooltip("Vertical allowed limit from ledge to ledge.")]
        public float verticalLimit = 2.5f;

        /// <summary>
        /// switch up / down animations. usefull sometimes
        /// </summary>
        [Tooltip("Switch up with down animation. Usefull for some situations.")]
        public bool switchUpWithDownAnim = false;

        /// <summary>
        /// enable / disable reversing on ledge
        /// </summary>
        [Tooltip("Reverse on current ledge - switch direction of ledge when pressed up.")]
        public bool reverseOnLedge = false;



#if DEBUG_INFO
        [Tooltip("Draw helper connection lines.")]
        public bool visualizeLedgeConnections = false;
#endif

        private Mode m_Mode = Mode._2LedgeUp;       // current ledge type
        private bool tooFarUp = true;               // is it too far from up ledge flag ?
        private bool tooFarDown = true;             // is it too far from down ledge flag ?
        private bool tooFarRight = true;            // is it too far from right ledge flag ?
        private bool tooFarLeft = true;             // is it too far from left ledge flag ?
        private string m_InfoText = "";             // text info string

        /// <summary>
        /// is character is leaving ledge ( readonly )
        /// </summary>
        public bool leavingLedge { get { return m_Mode == Mode._PullUp; } }

        /// <summary>
        /// set current ledge mode
        /// </summary>
        /// <param name="mode"></param>
        private void _setMode(Mode mode)
        {
            m_Mode = mode;
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
        /// get closest point from point to transforms width line
        /// </summary>
        /// <param name="t">transform to test</param>
        /// <param name="toPoint">point to test</param>
        /// <returns>returns closest point on transform width line</returns>
        public Vector3 closestPointOnTransformWidthLine(Transform t,Vector3 toPoint, string calledFrom)
        {
            if(!t)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return Vector3.zero;
            }

                Transform xform = t;
            float scaleX = xform.lossyScale.x;
            float halfScaleX = scaleX * 0.5f;
            Vector3 PointA = xform.position - xform.right * halfScaleX;
            Vector3 PointB = xform.position + xform.right * halfScaleX;
            Vector3 closest = MathUtils.GetClosestPoint2Line(PointA, PointB, ref toPoint);

            Plane plane = new Plane();
            Vector3 pt1 = xform.position;
            Vector3 pt2 = pt1 + xform.right;
            Vector3 pt3 = pt1 + xform.forward;
            plane.Set3Points(pt1, pt2, pt3);
            Ray ray = new Ray(toPoint, Vector3.up);
            float rayDist = 0.0f;
            if (plane.Raycast(ray,out rayDist ))
            {
                closest = toPoint + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(PointA, PointB, ref closest);
            }
            return closest;
        }

        /// <summary>
        /// initial conditions checks
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns>return true if conditions are met otherwise falsehmhh667i</returns>
        private bool _initial_condition(TPCharacter character)
        {
            bool lookingTowards = base.condition(character);
            bool onLedge = character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);

            if (!onLedge && !lookingTowards) return false;

            bool hasPullTarget = pullUpTarget;
            if (!hasPullTarget) return onLedge;
            else return true;
        }

        /// <summary>
        /// get display info text
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns>returns info string</returns>
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
                return m_InfoText;
            return "";
        }

        /// <summary>
        /// early exit conditions
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <returns>returns condition passed on not</returns>
        public override bool condition(TPCharacter character)
        {
            if (!character)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null!" + " < " + this.ToString() + ">");
#endif
                return false;
            }

            m_ConditionPassed = _initial_condition(character);
            if (!m_ConditionPassed) return false;

            tooFarUp = true;
            tooFarDown = true;
            tooFarRight = true;
            tooFarLeft = true;
            bool isOnLedge = character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
            m_InfoText = "Press 'Use' ";


            if (Target && isOnLedge)
            {
                Vector3 checkPosition = character.transform.position;
                Vector3 closestPt = closestPointOnTransformWidthLine(Target, checkPosition, "Ledge2Ledge::condition Target");
                Vector3 local = character.transform.InverseTransformPoint(closestPt);
                float dist = Mathf.Abs(local.x);
                tooFarUp = dist > horizontalLimit;
                if (!tooFarUp) m_InfoText += " /UP";
#if DEBUG_INFO
                if (visualizeLedgeConnections)
                {
                    Color color = tooFarUp ? Color.red : Color.blue;
                    Debug.DrawLine(checkPosition, closestPt, color);
                }
#endif
            }

            if (downTarget && isOnLedge)
            {
                Vector3 checkPosition = character.transform.position;
                Vector3 closestPt = closestPointOnTransformWidthLine(downTarget, checkPosition, "Ledge2Ledge::condition downTarget");
                Vector3 local = character.transform.InverseTransformPoint(closestPt);
                float dist = Mathf.Abs(local.x);
                tooFarDown = dist > horizontalLimit;
                if (!tooFarDown) m_InfoText += " /DOWN";
#if DEBUG_INFO
                if (visualizeLedgeConnections)
                {
                    Color color = tooFarDown ? Color.red : Color.blue;
                    Debug.DrawLine(checkPosition, closestPt, color);
                }
#endif
            }

            if (rightTarget && isOnLedge && character.ledge.transform  )
            {

                Vector3 checkPosition = character.transform.position;
                Vector3 closestPt2Curr = closestPointOnTransformWidthLine(character.ledge.transform, checkPosition, "Ledge2Ledge::condition rightTarget");
                Vector3 closestPt2Next = closestPointOnTransformWidthLine(rightTarget, closestPt2Curr, "Ledge2Ledge::condition rightTarget");
                float dist = Vector3.Distance(closestPt2Curr, closestPt2Next);
                tooFarRight = dist > verticalLimit;

                if (rightEdgeReq)
                {
                    if (!character.onLedgeEdgeB)
                        tooFarRight = true;
                }

                if (!tooFarRight) m_InfoText += " /RIGHT";
#if DEBUG_INFO
                if (visualizeLedgeConnections)
                {
                    Color color = tooFarRight ? Color.red : Color.blue;
                    Debug.DrawLine(closestPt2Next, closestPt2Curr, color);
                }
#endif
            }

            if (leftTarget && isOnLedge && character.ledge.transform)
            {
                Vector3 checkPosition = character.transform.position;
                Vector3 closestPt2Curr = closestPointOnTransformWidthLine(character.ledge.transform, checkPosition, "Ledge2Ledge::condition character.ledge");
                Vector3 closestPt2Next = closestPointOnTransformWidthLine(leftTarget, closestPt2Curr, "Ledge2Ledge::condition leftTarget");
                float dist = Vector3.Distance(closestPt2Curr, closestPt2Next);
                tooFarLeft = dist > verticalLimit;

                if (leftEdgeReq)
                {
                    if (!character.onLedgeEdgeA)
                        tooFarLeft = true;
                }

                if (!tooFarLeft) m_InfoText += " /LEFT";
#if DEBUG_INFO
                if (visualizeLedgeConnections)
                {
                    Color color = tooFarLeft ? Color.red : Color.blue;
                    Debug.DrawLine(closestPt2Next, closestPt2Curr, color);
                }
#endif
            }

            if (tooFarUp && tooFarDown && tooFarRight && tooFarLeft)
            {
                m_InfoText = "";
            }
            string pullText = "";
            if (pullUpTarget)
            {

                pullText = "\nPress 'Use' to Pull Up";
                if (!isOnLedge)
                {
                    pullText = "Press 'Use' to Pull Down";

                    m_ConditionPassed = true;
                }
            }
            m_InfoText += pullText;
            if (reverseOnLedge && character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool))
            {
                m_InfoText += "\nPress 'Jump' + UP to reverse";
            }
            m_ConditionPassed = true;
            return m_ConditionPassed;
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

            if (!use && !jump) return false;

            //if (!m_TriggerInfo) m_TriggerInfo = new TriggerInfo();

            bool up = v > 0;
            bool down = v < 0;
            bool right = h > 0;
            bool left = h < 0;
            bool pullUpDown = use && (!(up && (Target || reverseOnLedge )) && !(down && downTarget) &&
                !(right && rightTarget) && !(left && leftTarget));

            if (pullUpDown && use)
            {
                bool isOnLedge = character.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                if (!isOnLedge)
                {
                    if (!character.isGroundMode) return false;

                    _pullDownSetup(character, limbsIK);

                    return true;
                }
                else
                {
                    if (!pullUpTarget) return false;

                    _pullUpSetup(character, limbsIK);

                    return true;
                }
            }

            if (up && use)
            {
                if (Target)
                {
                    if (tooFarUp) return false;
                    _ledgeUpSetup(Target, switchTargetDirection, character, limbsIK);

                    return true;
                }
                else
                {
                    if (tooFarUp) return false;
                    if(reverseOnLedge)
                    {
                        Transform currentTarget = character.ledge.transform;
                        bool reversed = character.ledge.reversed;
                        _ledgeUpSetup(currentTarget, !reversed, character, limbsIK, true);
                        return true;
                    }
                }
            }
            if(up && jump)
            {
                if (reverseOnLedge)
                {
                    Transform currentTarget = character.ledge.transform;
                    bool reversed = character.ledge.reversed;
                    _ledgeUpSetup(currentTarget, !reversed, character, limbsIK, true);
                    return true;
                }
            }


            if (down && use)
            {
                if (downTarget)
                {
                    if (tooFarDown) return false;

                    _ledgeDownSetup(character, limbsIK);

                    return true;
                }
            }
            if (right && use)
            {
                if (rightTarget)
                {
                    if (tooFarRight) return false;

                    _ledgeRightSetup(character, limbsIK);

                    return true;
                }
            }
            if (left && use)
            {
                if (leftTarget)
                {
                    if (tooFarLeft) return false;

                    _ledgeLeftSetup(character, limbsIK);

                    return true;
                }
            }

            return false;
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

            character.triggerRootMotion = false;
            _end_animator_states(character);
            character.disableLedgeConstraint = false; // enable ledge constraint when on ledge
            character.fullStop();

            if (leavingLedge)
            {
                if (limbsIK)
                {
                    limbsIK.disableAll();
                    limbsIK.handIKMode = IKMode.Default;
                }
                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                character.restoreCapsuleSize();
                character.jumpAllowed = true;
                character.disableGroundCheck = false;
                character.disableCapsuleScale = false;

                OrbitCameraController oc = character.GetComponentInChildren<OrbitCameraController>();
                if (oc)
                {
                    oc.additiveRotation = false;
                }
            }
            else
            {
                if (m_Mode == Mode._PullDown)
                {
                    OrbitCameraController oc = character.GetComponentInChildren<OrbitCameraController>();
                    if (oc)
                    {
                        oc.additiveRotation = true;
                    }
                }
                if (limbsIK)
                {
                    limbsIK.RHandIKEnabled = true;
                    limbsIK.LFootIKEnabled = true;
                    limbsIK.RFootIKEnabled = true;
                    limbsIK.LHandIKEnabled = true;
                    limbsIK.LFWeightTime = 0.0f;
                    limbsIK.RFWeightTime = 0.0f;
                    limbsIK.setIKDirection(true);
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
            if (limbsIK)
            {
                limbsIK.LH_OVERRIDE = null;
                limbsIK.RH_OVERRIDE = null;
                limbsIK.LHandIKEnabled = true; 
                limbsIK.LHWeightTime = 0.0f;
                limbsIK.RHandIKEnabled = true; 
                limbsIK.RHWeightTime = 0.0f;
                limbsIK.LFootIKEnabled = false;
                limbsIK.RFootIKEnabled = false;
            }
            if (m_Mode != Mode._PullUp && m_Mode != Mode._PullDown)
            {
#if DEBUG_INFO
                if(!character .audioManager)
                {
                    Debug.LogError("object cannot be null. " + " < " + this.ToString() + ">");
                    return;
                }
#endif

                character.audioManager.playJumpSound();
                m_TriggerInfo.doRotationToTarget = true;
                m_TriggerInfo.rot_time = 0.0f;
                
            }
            m_TriggerInfo.rot_time = 0.0f;
            m_TriggerInfo.doMatchTarget = true;
            m_TriggerInfo.startRotation = character.transform.rotation;
        }


        /// <summary>
        /// entering ledge setup
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper class</param>
        private void _pullDownSetup(TPCharacter character, IKHelper limbsIK)
        {
            character.ledgeMove = false;
            float scalez = pullUpTarget.localScale.z * 0.5f;
            character.setLedge(pullUpTarget, false, -scalez); // set first
            _setMode(Mode._PullDown);

            Vector3 rhandpos = character.animator.GetBoneTransform(HumanBodyBones.RightHand).position;

            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref rhandpos);
            m_TriggerInfo.transform = pullUpTarget;
            m_TriggerInfo.pos_offset = closest - pullUpTarget.position;
            m_TriggerInfo.CatchPosition = closest;

            m_TriggerInfo.avatarTarget = AvatarTarget.RightHand;
            m_TriggerInfo.doRotationToTarget = false;

            if (character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool))
            {
                m_TriggerInfo.stateName = "Base Layer.LEDGE.DownOnLedge_Hang";
            }
            else
            {
                m_TriggerInfo.stateName = "Base Layer.LEDGE.DownOnLedge";
            }

            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds

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
                bool checkUnder = _checkIsWallUnderTarget(character, limbsIK, closest, pullUpTarget);
                if (checkUnder)
                {
                    character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                    m_TriggerInfo.stateName = "Base Layer.LEDGE.DownOnLedge";
                    limbsIK.currentRelLedgePosition =
                        limbsIK.ledgeRelativePosition;
                    character.scaleCapsuleToHalf();
                }
                else
                {
                    character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                    m_TriggerInfo.stateName = "Base Layer.LEDGE.DownOnLedge_Hang";
                    limbsIK.currentRelLedgePosition =
                           limbsIK.ledgeRelativePositionHang;
                    character.restoreCapsuleSize();
                }
            }

            character.OnLerpEnd = () =>
            {   
                m_TriggerInfo.doMatchTarget = true;
                _start_animator_states(character);
                character.triggerRootMotion = true;
                character.OnLerpEnd = null;
            };
            Quaternion rotation = pullUpTarget.rotation;
            Vector3 euler = rotation.eulerAngles;
            euler.z = 0.0f;
            euler.x = 0.0f;
            Quaternion fromEuler = Quaternion.Euler(euler);
            character.lerpToTransform(null, fromEuler,
                    0f, 0.2f);

            m_TriggerInfo.targetRotation = fromEuler;
            m_TriggerInfo.startRotation = fromEuler;
        }

        /// <summary>
        /// leaving ledge setup
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper class</param>
        private void _pullUpSetup(TPCharacter character,IKHelper limbsIK)
        {
            character.ledgeMove = false;
            character.setLedge(pullUpTarget); // set first

            _setMode(Mode._PullUp);

            Vector3 rhandpos = character.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref rhandpos);
            m_TriggerInfo.transform = pullUpTarget;
            m_TriggerInfo.pos_offset = closest - pullUpTarget.position;
            m_TriggerInfo.CatchPosition = closest;

            m_TriggerInfo.stateName = "Base Layer.LEDGE.PullUpLedge";
            m_TriggerInfo.avatarTarget = AvatarTarget.RightHand;
            m_TriggerInfo.doRotationToTarget = false;

            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds

            if (limbsIK)
            {
                limbsIK.LHandIKEnabled = false;
                limbsIK.RHandIKEnabled = false;
                limbsIK.LFootIKEnabled = false;
                limbsIK.RFootIKEnabled = false;
                limbsIK.handIKMode = IKMode.Default;
                limbsIK.setIKDirection(false);
            }

            Quaternion rotation = transform.rotation;
            Vector3 euler = rotation.eulerAngles;
            euler.z = 0.0f;
            euler.x = 0.0f;
            Quaternion fromEuler = Quaternion.Euler(euler);
            m_TriggerInfo.targetRotation = fromEuler;
            m_TriggerInfo.startRotation = character.transform.rotation;
            m_TriggerInfo.doRotationToTarget = false;
            character.triggerRootMotion = true;
            _start_animator_states(character, switchUpWithDownAnim);
        }

        /// <summary>
        /// jump up from ledge setup
        /// </summary>
        /// <param name="ledgeTransform">ledge transform</param>
        /// <param name="reverse">reverse flag</param>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper class</param>
        /// <param name="reversingLedge">reversing ledge flag</param>
        private void _ledgeUpSetup(Transform ledgeTransform,bool reverse, TPCharacter character,IKHelper limbsIK, bool reversingLedge = false)
        {
            float scalez = ledgeTransform.localScale.z * 0.5f;
            character.setLedge(ledgeTransform, reverse, -scalez);
            character.ledgeMove = false;
            _setMode(Ledge2LedgeTrigger.Mode._2LedgeUp);

            Vector3 rhandpos = character.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref rhandpos);
            Plane plane = new Plane();
            Vector3 pt1 = ledgeTransform.position;
            Vector3 pt2 = pt1 + ledgeTransform.right;
            Vector3 pt3 = pt1 + ledgeTransform.forward;
            plane.Set3Points(pt1, pt2, pt3);
            Ray ray = new Ray(rhandpos, Vector3.up);
            float rayDist = 0.0f;
            if (plane.Raycast(ray, out rayDist))
            {
                closest = rhandpos + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref closest);
            }

            m_TriggerInfo.transform = ledgeTransform;
            m_TriggerInfo.pos_offset = closest - ledgeTransform.position;
            m_TriggerInfo.CatchPosition = closest;
            if (reversingLedge)
                m_TriggerInfo.avatarTarget = AvatarTarget.LeftHand;
            else
                m_TriggerInfo.avatarTarget = AvatarTarget.RightHand;

            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds

            bool nextHang = true;

            if (limbsIK)
            {
                limbsIK.LH_OVERRIDE = limbsIK.LHPosition;
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
                bool checkUnder = _checkIsWallUnderTarget(character, limbsIK, closest, ledgeTransform);
                if (checkUnder)
                {
                    limbsIK.currentRelLedgePosition =
                        limbsIK.ledgeRelativePosition;
                    nextHang = false;
                }
                else
                {
                    limbsIK.currentRelLedgePosition =
                           limbsIK.ledgeRelativePositionHang;
                    nextHang = true;
                }
            }

            m_TriggerInfo.doMatchTarget = true;
            Quaternion targetRot = ledgeTransform.rotation;
            Vector3 targetEuler = targetRot.eulerAngles;
            targetEuler.z = 0.0f;
            targetEuler.x = 0.0f;
            if (reverse)
                targetEuler.y += 180.0f;
            Quaternion fromTargetEuler = Quaternion.Euler(targetEuler);
            m_TriggerInfo.targetRotation = fromTargetEuler;
            m_TriggerInfo.startRotation = character.transform.rotation;
            character.triggerRootMotion = true;
            bool hanging = character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);
            _start_animator_states(character, reversingLedge, hanging, nextHang);
        }

        /// <summary>
        /// jump down from ledge setup
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper class</param>
        private void _ledgeDownSetup(TPCharacter character,IKHelper limbsIK)
        {
            float scalez = downTarget.localScale.z * 0.5f;
            character.setLedge(downTarget, switchDownDirection, -scalez );
            character.ledgeMove = false;
            _setMode(Ledge2LedgeTrigger.Mode._2LedgeDown);

            Vector3 rhandpos = character.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref rhandpos);

            Plane plane = new Plane();
            Vector3 pt1 = downTarget.position;
            Vector3 pt2 = pt1 + downTarget.right;
            Vector3 pt3 = pt1 + downTarget.forward;
            plane.Set3Points(pt1, pt2, pt3);
            Ray ray = new Ray(rhandpos, Vector3.down);
            float rayDist = 0.0f;
            if (plane.Raycast(ray, out rayDist))
            {
                closest = rhandpos + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref closest);
            }

            m_TriggerInfo.transform = downTarget;
            m_TriggerInfo.pos_offset = closest - downTarget.position;
            m_TriggerInfo.CatchPosition = closest;
            m_TriggerInfo.avatarTarget = AvatarTarget.RightHand;

            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds

            bool nextHang = true;
            if (limbsIK)
            {
                limbsIK.LH_OVERRIDE = limbsIK.LHPosition;
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
                bool checkUnder = _checkIsWallUnderTarget(character, limbsIK, closest, downTarget);
                if (checkUnder)
                {
                    limbsIK.currentRelLedgePosition =
                        limbsIK.ledgeRelativePosition;
                    nextHang = false;
                }
                else
                {
                    limbsIK.currentRelLedgePosition =
                           limbsIK.ledgeRelativePositionHang;
                    nextHang = true;
                }
            }

            m_TriggerInfo.doMatchTarget = true;
            Quaternion targetRot = downTarget.rotation;
            Vector3 targetEuler = targetRot.eulerAngles;
            targetEuler.z = 0.0f;
            targetEuler.x = 0.0f;
            if (switchDownDirection)
                targetEuler.y += 180.0f;
            Quaternion fromTargetEuler = Quaternion.Euler(targetEuler);
            m_TriggerInfo.targetRotation = fromTargetEuler;
            m_TriggerInfo.startRotation = character.transform.rotation;
            character.triggerRootMotion = true;
            bool hanging = character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);
            _start_animator_states(character, false, hanging, nextHang);
        }

        /// <summary>
        /// jump right from ledge setup
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper class</param>
        private void _ledgeRightSetup(TPCharacter character,IKHelper limbsIK)
        {
            float scalez = rightTarget.localScale.z * 0.5f;
            character.setLedge(rightTarget, false, -scalez);
            character.ledgeMove = false;
            _setMode(Ledge2LedgeTrigger.Mode._2LedgeRight);

            Vector3 rhandpos = character.animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref rhandpos);

            Plane plane = new Plane();
            Vector3 pt1 = rightTarget.position;
            Vector3 pt2 = pt1 + rightTarget.right;
            Vector3 pt3 = pt1 + rightTarget.forward;
            plane.Set3Points(pt1, pt2, pt3);
            Ray ray = new Ray(rhandpos, Vector3.right);
            float rayDist = 0.0f;
            if (plane.Raycast(ray, out rayDist))
            {
                closest = rhandpos + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref closest);
            }

            m_TriggerInfo.transform = rightTarget;
            m_TriggerInfo.pos_offset = closest - rightTarget.position;
            m_TriggerInfo.CatchPosition = closest;
            m_TriggerInfo.avatarTarget = AvatarTarget.LeftHand;

            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds
            bool nextHang = true;
            if (limbsIK)
            {

                limbsIK.LH_OVERRIDE = limbsIK.LHPosition;
                limbsIK.checkHang = false;
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
                bool checkUnder = _checkIsWallUnderTarget(character, limbsIK, closest, rightTarget);
                if (checkUnder)
                {
                    limbsIK.currentRelLedgePosition =
                        limbsIK.ledgeRelativePosition;
                    nextHang = false;
                }
                else
                {
                    limbsIK.currentRelLedgePosition =
                           limbsIK.ledgeRelativePositionHang;
                    nextHang = true;
                }
            }

            m_TriggerInfo.doMatchTarget = true;
            Quaternion targetRot = rightTarget.rotation;
            Vector3 targetEuler = targetRot.eulerAngles;
            targetEuler.z = 0.0f;
            targetEuler.x = 0.0f;
            Quaternion fromTargetEuler = Quaternion.Euler(targetEuler);
            m_TriggerInfo.targetRotation = fromTargetEuler;
            m_TriggerInfo.startRotation = character.transform.rotation;
            character.triggerRootMotion = true;
            bool hanging = character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);
            _start_animator_states(character, false, hanging, nextHang);
        }

        /// <summary>
        /// jump left from ledge setup
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbsIK">ik helper class</param>
        private void _ledgeLeftSetup(TPCharacter character,IKHelper limbsIK)
        {
            float scalez = leftTarget.localScale.z * 0.5f;
            character.setLedge(leftTarget, false, -scalez);
            character.ledgeMove = false;
            _setMode(Ledge2LedgeTrigger.Mode._2LedgeLeft);

            Vector3 rhandpos = character.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref rhandpos);

            Plane plane = new Plane();
            Vector3 pt1 = leftTarget.position;
            Vector3 pt2 = pt1 + leftTarget.right;
            Vector3 pt3 = pt1 + leftTarget.forward;
            plane.Set3Points(pt1, pt2, pt3);
            Ray ray = new Ray(rhandpos, Vector3.left);
            float rayDist = 0.0f;
            if (plane.Raycast(ray, out rayDist))
            {
                closest = rhandpos + Vector3.up * rayDist;
                closest = MathUtils.GetClosestPoint2Line(character.ledge.leftPoint, character.ledge.rightPoint, ref closest);
            }

            m_TriggerInfo.transform = leftTarget;
            m_TriggerInfo.pos_offset = closest - leftTarget.position;
            m_TriggerInfo.CatchPosition = closest;
            m_TriggerInfo.avatarTarget = AvatarTarget.RightHand;
            character.disableLedgeConstraint = true; // disable ledge constraining or character may be stuck if outside ledge bounds
            bool nextHang = true;
            if (limbsIK)
            {
                limbsIK.RH_OVERRIDE = limbsIK.RHPosition;
                limbsIK.checkHang = false;
                limbsIK.LHandIKEnabled = false;
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
                bool checkUnder = _checkIsWallUnderTarget(character, limbsIK, closest, leftTarget);
                if (checkUnder)
                {
                    limbsIK.currentRelLedgePosition =
                        limbsIK.ledgeRelativePosition;
                    nextHang = false;
                }
                else
                {
                    limbsIK.currentRelLedgePosition =
                           limbsIK.ledgeRelativePositionHang;
                    nextHang = true;
                }
            }

            m_TriggerInfo.doMatchTarget = true;
            Quaternion targetRot = leftTarget.rotation;
            Vector3 targetEuler = targetRot.eulerAngles;
            targetEuler.z = 0.0f;
            targetEuler.x = 0.0f;
            Quaternion fromTargetEuler = Quaternion.Euler(targetEuler);
            m_TriggerInfo.targetRotation = fromTargetEuler;
            m_TriggerInfo.startRotation = character.transform.rotation;
            character.triggerRootMotion = true;
            bool hanging = character.animator.GetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool);
            _start_animator_states(character, false, hanging, nextHang);
        }

        /// <summary>
        /// setup start animator states
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="switchUpAnim">switch up/down animation clips flag</param>
        /// <param name="now_hang">currently hanging ?</param>
        /// <param name="next_hang">will hang on target ?</param>
        private void _start_animator_states(TPCharacter character,bool switchUpAnim = false, bool now_hang = true, bool next_hang = true)
        {
            character.ledgeMove = false;
            character.disableMove = true;

            character.disableGroundCheck = true;
            character.disableCapsuleScale = true;

            switch (m_Mode)
            {
                case Mode._2LedgeUp:
                    {
                        character.animator.SetBool(/*"pGrabLedgeUp"*/HashIDs.GrabLedgeUpBool, true);//upGoUnder

                        string up = "Base Layer.LEDGE.L2LUP";
                        string upHang = "Base Layer.LEDGE.L2LUP_Hang";
                        string upH2W = "Base Layer.LEDGE.L2L_UP_Hang2Walled";
                        string upW2H = "Base Layer.LEDGE.L2LUP_Walled2Hang";
                        if (switchUpAnim)
                        {
                            up = "Base Layer.LEDGE.L2LDOWN";
                            upHang = "Base Layer.LEDGE.L2LDOWN_Hang";
                            upH2W = "Base Layer.LEDGE.L2LDOWN_Hang2Walled";
                            upW2H = "Base Layer.LEDGE.L2L_DOWN_Walled2Hang";
                        }

                        if (now_hang)
                        {
                            if (!next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade(upH2W, 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = upH2W;
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade(upHang, 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = upHang;
                            }
                        }
                        else
                        {
                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade(upW2H, 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = upW2H;
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade(up, 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = up;
                            }

                        }
                    }
                    break;
                case Mode._2LedgeDown:
                    {
                        character.animator.SetBool(/*"pGrabLedgeDown"*/HashIDs.GrabLedgeDownBool, true);
                        if (now_hang)
                        {
                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LDOWN_Hang", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LDOWN_Hang";
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LDOWN_Hang2Walled", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LDOWN_Hang2Walled";
                            }
                        }
                        else
                        {
                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade("Base Layer.LEDGE.L2L_DOWN_Walled2Hang", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2L_DOWN_Walled2Hang";
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LDOWN", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LDOWN";
                            }
                        }
                    }
                    break;
                case Mode._2LedgeRight:
                    {
                        character.animator.SetBool(/*"pGrabLedgeRight"*/HashIDs.GrabLedgeRightBool, true);
                        if (now_hang)
                        {

                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LRIGHT_Hang", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LRIGHT_Hang";
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LRIGHT_Hang2Walled", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LRIGHT_Hang2Walled";
                            }
                        }
                        else
                        {
                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LRIGHT_Walled2Hang", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LRIGHT_Walled2Hang";
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LRIGHT", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LRIGHT";
                            }
                        }

                    }
                    break;
                case Mode._2LedgeLeft:
                    {
                        character.animator.SetBool(/*"pGrabLedgeLeft"*/HashIDs.GrabLedgeLeftBool, true);
                        if (now_hang)
                        {
                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LLEFT_Hang", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LLEFT_Hang";
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LLEFT_Hang2Walled", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LLEFT_Hang2Walled";
                            }
                        }
                        else
                        {
                            if (next_hang)
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, true);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LLEFT_Walled2Hang", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LLEFT_Walled2Hang";
                            }
                            else
                            {
                                character.animator.SetBool(/*"pLedgeHang"*/HashIDs.LedgeHangBool, false);
                                character.animator.CrossFade("Base Layer.LEDGE.L2LLEFT", 0.0f, 0, 0.0f);
                                m_TriggerInfo.stateName = "Base Layer.LEDGE.L2LLEFT";
                            }
                        }
                    }
                    break;
                case Mode._PullUp:
                    {
                        character.disableMove = true;
                        character.animator.SetBool(HashIDs.ClimbOnBool, true);
                    }
                    break;
                case Mode._PullDown:
                    {
                        character.fullStop();
                        character.disablePhysics(true, true);
                        character.animator.SetBool(/*"pGrabLedgeDown"*/HashIDs.GrabLedgeDownBool, true);
                    }
                    break;
            }
        }

        /// <summary>
        /// setup end animator states
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        private void _end_animator_states(TPCharacter character)
        {
            //m_IsActive = false;
            switch (m_Mode)
            {
                case Mode._2LedgeUp:
                    {
                        character.ledgeMove = true;
                        character.disableMove = false;
                        character.animator.SetBool(/*"pGrabLedgeUp"*/HashIDs.GrabLedgeUpBool, false);
                    }
                    break;
                case Mode._2LedgeDown:
                    {
                        character.ledgeMove = true;
                        character.disableMove = false;
                        character.animator.SetBool(/*"pGrabLedgeDown"*/HashIDs.GrabLedgeDownBool, false);
                    }
                    break;
                case Mode._2LedgeRight:
                    {
                        character.ledgeMove = true;
                        character.disableMove = false;
                        character.animator.SetBool(/*"pGrabLedgeRight"*/HashIDs.GrabLedgeRightBool, false);
                    }
                    break;
                case Mode._2LedgeLeft:
                    {
                        character.ledgeMove = true;
                        character.disableMove = false;
                        character.animator.SetBool(/*"pGrabLedgeLeft"*/HashIDs.GrabLedgeLeftBool, false);
                    }
                    break;
                case Mode._PullUp:
                    {
                        character.disableMove = false;
                        character.enablePhysics();
                        character.restoreCapsuleSize();
                        character.disableCapsuleScale = false;
                        character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);
                        character.animator.SetBool(HashIDs.ClimbOnBool, false);
                        character.animator.SetBool(HashIDs .OnLedgeBool , false);
                        character.animatorDampTime = 0.1f;
                        character.ledgeMove = false;
                    }
                    break;
                case Mode._PullDown:
                    {
                        character.disableMove = false;
                        character.setMoveMode(TPCharacter.MovingMode.Ledge);
                        character.disableCapsuleScale = true;
                        character.animator.SetBool(HashIDs.GrabLedgeDownBool, false);
                        character.animator.SetBool(HashIDs.OnLedgeBool, true);
                        character.animatorDampTime = 0.0f;
                        character.ledgeMove = true;
                    }
                    break;
            }

        }

        /// <summary>
        /// check if there is a wall in front of character
        /// </summary>
        /// <param name="character">character interacting with trigger</param>
        /// <param name="limbIK">ik helper class</param>
        /// <param name="inpos">input position</param>
        /// <param name="target">target transform</param>
        /// <returns>returns true if there is a wall under target otherwise false</returns>
        private bool _checkIsWallUnderTarget(TPCharacter character, IKHelper limbIK, Vector3 inpos, Transform target)
        {
            Vector3 offset = new Vector3(0.0f, character.capsule.height * 0.5f, 0.25f);
            offset = target.rotation * offset;
            Vector3 pos = inpos - offset;
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