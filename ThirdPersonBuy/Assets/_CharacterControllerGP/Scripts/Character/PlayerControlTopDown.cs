// © 2016 Mario Lelas

#if DEBUG_INFO
// for testing
// go through all npcs in scene
//#define TESTING_USE_ALL_NPC_ARRAY  
#endif


using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{

    /// <summary>
    /// class that controls top down view player in combat framework system
    /// </summary>
    public class PlayerControlTopDown : PlayerControlBase
    {

#region Fields

        /// <summary>
        /// if shoot on click is enabled
        /// player will draw and shoot bow on single click
        /// otherwise drawing of the bow string will be 
        /// performed on keeping the attack button down
        /// and shoot on release
        /// </summary>
        [Tooltip("Draw and shoot on click, or drawn bow on button down and shoot upon release.")]
        public bool shootOnClick = true;

        [Range(0.1f, 4.0f)]
        public float bowShootSpeed = 1.0f;

        private PlayerTopDown m_TopDownPlayer;                 // player reference
        private bool m_DoubleClick = false;                     // run flag
        private float m_DoubleClickMaxTime = 0.25f;             // run flag time
        private float m_DoubleClickCurTime = 0.0f;              // run flag timer
        private int m_ClickCount = 0;                           // number of clicks in double click max time

        // one click archery system helpers
        private bool m_BowAttackStarted = false;        // bow attack started ( shoot on click mode )
        private float m_BowAttackTime = 1.0f;           // bow attack time ( shoot on click mode )
        private float m_BowAttackTimer = 0.0f;          // bow attack timer ( shoot on click mode )
        private Vector3 m_BowShootPosition = Vector3.zero;   // position of bow-arrow shooting target 

#endregion

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            bool clicked = Input.GetMouseButtonDown(1);
            if (clicked)
            {
                m_DoubleClick = false;
                m_DoubleClickCurTime = 0.0f;
            }
            m_DoubleClickCurTime += Time.deltaTime;
            if (m_DoubleClickCurTime <= m_DoubleClickMaxTime)
            {
                if (clicked) m_ClickCount++;
                if (m_ClickCount > 1)
                {
                    m_DoubleClick = true;
                }
            }
            else
            {
                m_ClickCount = 0;
            }
            update();
        }

        /// <summary>
        /// Unity LateUpdate method
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
            _lateUpdateBow1();
        }



        /// <summary>
        /// initialize component
        /// </summary>
        public override bool initialize()
        {
            if (m_Initialized) return true;
            if (!base.initialize()) return false;


            if(m_Player is PlayerTopDown)
            {
                m_TopDownPlayer = m_Player as PlayerTopDown;
            }
            else
            {
                Debug.LogError("TopDownPlayer component missing <" + this.ToString() + ">");
                return false;
            }

            // setup ragdoll callbacks
            m_TopDownPlayer.ragdoll.OnHit = () =>
            {
                m_TopDownPlayer.character.simulateRootMotion = false;
                m_TopDownPlayer.character.disableMove = true;
                m_TopDownPlayer.character.rigidBody.velocity = Vector3.zero;

                m_TopDownPlayer.disableInput = true;
                m_TopDownPlayer.noLookIK();
                m_TopDownPlayer.character.rigidBody.isKinematic = true;
                m_TopDownPlayer.character.rigidBody.detectCollisions = false;
                m_TopDownPlayer.character.capsule.enabled = false;
                if (m_TopDownPlayer.ragdoll.isFullRagdoll)
                    m_TopDownPlayer.m_Camera.switchTargets(m_TopDownPlayer.ragdoll.RagdollBones[(int)BodyParts.Spine]);
            };
            m_TopDownPlayer.ragdoll.OnStartTransition = () =>
            {

                if (!m_TopDownPlayer.ragdoll.isFullRagdoll && !m_TopDownPlayer.ragdoll.isGettingUp)
                {
                    m_TopDownPlayer.character.simulateRootMotion = true;
                    m_TopDownPlayer.character.rigidBody.detectCollisions = true;
                    m_TopDownPlayer.character.rigidBody.isKinematic = false;
                    m_TopDownPlayer.character.capsule.enabled = true;
                }
                else
                {
                    m_TopDownPlayer.animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, 0.0f);
                    m_TopDownPlayer.animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, 0.0f);
                }
            };
            //m_TopDownPlayer.ragdoll.RagdollEventTime = 3.0f;
            //m_TopDownPlayer.ragdoll.OnTimeEnd = () =>
            //{
            //    m_TopDownPlayer.ragdoll.blendToMecanim();
            //};
            //m_Ragdoll.OnBlendEnd = () =>
            // {
            //     Debug.Log("ON BLEND END");
            // };
            //m_Ragdoll.OnGetUpEvent = () =>
            //  {
            //      Debug.Log("ON GET UP EVENT");
            //  };
            m_TopDownPlayer.ragdoll.LastEvent = () =>
            {
                m_TopDownPlayer.character.simulateRootMotion = true;
                m_TopDownPlayer.character.disableMove = false;
                m_TopDownPlayer.disableInput = false;
                m_TopDownPlayer.character.rigidBody.isKinematic = false;
                m_TopDownPlayer.character.rigidBody.detectCollisions = true;
                m_TopDownPlayer.character.capsule.enabled = true;
                m_TopDownPlayer.m_Camera.switchTargets(m_TopDownPlayer.m_Camera.Target);
            };

            m_TopDownPlayer.OnAttackHit = attack_hit_notify;
            m_TopDownPlayer.OnAttackEndNotify = attack_end_notify;
            m_TopDownPlayer.OnAttackStartNotify = attack_start_notify;

            if (m_TopDownPlayer.triggers)
            {
                // setup trigger callbacks
                m_TopDownPlayer.triggers.OnTriggerStart = () =>
                {
                    m_TopDownPlayer.stop();
                    m_TopDownPlayer.enableMove = false;
                    if (m_TopDownPlayer.legsIK) m_TopDownPlayer.legsIK.enabled = false;
                    m_TopDownPlayer.disableInput = true;
                    bool isOnLedge = m_TopDownPlayer.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                    if (!isOnLedge)
                    {
                        m_PrevStrafing = m_Player.strafing;
                        m_Player.strafing = false;
                        m_TopDownPlayer.noLookIK();
                    }
                        
                };
                m_TopDownPlayer.triggers.OnTriggerEnd = () =>
                {
                    m_TopDownPlayer.stop();
                    Ray ray = new Ray(transform.position, Vector3.down);
                    int mask = m_TopDownPlayer.walkableTarrainMask;
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, float.MaxValue, mask))
                    {
                        m_TopDownPlayer.currentDestination = hit.point;
                    }
                    m_TopDownPlayer.enableMove = true;
                    bool isOnLedge = m_TopDownPlayer.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                    if (!isOnLedge)
                    {
                        m_Player.m_Camera.additiveRotation = false;
                        m_Player.strafing = m_PrevStrafing;
                        if (m_TopDownPlayer.legsIK) m_TopDownPlayer.legsIK.enabled = true;
                        if (m_TopDownPlayer.strafing) m_TopDownPlayer.character.setMoveMode(TPCharacter.MovingMode.Strafe);
                    }
                    m_TopDownPlayer.disableInput = false;
                };
            }

            if (!attackSweepBody) { Debug.LogError("Attack sweep rigidbody missing! <" + this.ToString() + ">"); return false; }

            m_Initialized = true;
            return m_Initialized;
        }

        /// <summary>
        /// control player system
        /// </summary>
        protected override  void _controlPlayer()
        {
            m_TopDownPlayer.animator.SetFloat(HashIDs.AttackSpeedFloat/*"pAttackSpeed"*/, m_Stats.currentAttackSpeed);
            bool onLedge = m_TopDownPlayer.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);

            // check if secondary weapon is drawn but primary is not.
            // if so - draw primary
            if (m_CurrentWeaponMode == WeaponMode.DualWield)
            {
                if (m_EquipmentScript.currentWeapon1H &&
                    m_EquipmentScript.currentSecondary)
                {
                    if (!m_EquipmentScript.weaponInHand1H &&
                            m_EquipmentScript.secondaryWeaponInHand)
                    {
                        if (!m_SwitchingItemState)
                        {
#if ADDITIONAL_DEBUG_INFO
                            Debug.LogWarning("Secondary item alone eqipped - drawing primary.");
#endif
                            _startTakingWeapon1H(AnimClipSheet.DualWeapons);
                        }
                    }
                }
            }


            if (m_PrimaryTargetNPC != null)
            {
                m_Direction2target = m_PrimaryTargetNPC.position - transform.position;
                m_Direction2target.y = 0.0f;
                m_Direction2target.Normalize();
            }

            float h = 0.0f;
            float v = 0.0f;
            bool dive = false;
            bool attack = false;
            bool block = false;
            bool jump = false; // no jumping in top down
            bool runToggle = false;
            bool crouch = false; // no crouching in top down
            Vector3? bodyDirection = null;




            Vector3 direction2cursor = transform.forward;
            if (!m_TopDownPlayer.disableInput)
            {
#if DEBUG_INFO
                if (!m_TopDownPlayer) { Debug.LogError("Cannot find player component." + " < " + this.ToString() + ">"); return; }
                if (!m_TopDownPlayer.m_Camera) { Debug.LogError("Cannont find m_Camera" + " < " + this.ToString() + ">"); return; }
                if (!m_TopDownPlayer.m_Camera.cameraComponent) { Debug.LogError("Cannot find CameraComponent." + " < " + this.ToString() + ">"); return; }
#endif
                bool switchEquipmentCondition = !m_SwitchingItemState  && !m_Attack_combo_started && !onLedge && !m_TopDownPlayer.triggerActive;

                if (Input.GetButtonDown("ToggleWeapon") && switchEquipmentCondition)
                {
                    toggleCurrentWeapon();
                }
                if (Input.GetButtonDown("DrawWeapon"))
                {
                    takeCurrentWeapon();
                }
                if (Input.GetButtonDown("SheatheWeapon"))
                {
                    sheatheCurrentWeapon();
                }
                if (Input.GetButtonDown("Drop") && switchEquipmentCondition)
                {
                    dropAllEquipment();
                }
                if (Input.GetButtonDown("WeaponShieldMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.Weapon1HShield);
                }
                if (Input.GetButtonDown("Weapon2HMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.Weapon2H);
                }
                if (Input.GetButtonDown("BowMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.Bow);
                }
                if (Input.GetButtonDown("DualWieldMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.DualWield);
                }


                Ray ray = m_TopDownPlayer.m_Camera.cameraComponent.ScreenPointToRay(Input.mousePosition);
                int mask = m_TopDownPlayer.walkableTarrainMask;
                RaycastHit hit;
                if (Physics.SphereCast(ray, 0.4f, out hit, float.MaxValue, mask, QueryTriggerInteraction.Ignore))
                {
                    UnityEngine.AI.NavMeshHit nhit;
                    Vector3 closest = hit.point;
                    if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out nhit, float.MaxValue, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        closest = nhit.position;
                    }
                    direction2cursor = closest - transform.position;
                    direction2cursor.y = 0.0f;
                    direction2cursor.Normalize();

                    if (Input.GetMouseButtonDown(1))
                    {
                        m_TopDownPlayer.currentDestination = closest;
                    }

                }

                if (!onLedge)
                {
                    // if taking hit
                    // allow dive only after halfish of taking hit animation is passed
                    bool halfHitAnimationPassed = true;
                    if (takingHit)
                    {
                        AnimatorStateInfo asi = m_TopDownPlayer.animator.GetCurrentAnimatorStateInfo(0);
                        float normTime = asi.normalizedTime;
                        halfHitAnimationPassed = normTime > 0.6f;
                    }
                    dive = Input.GetButtonDown("DiveRoll") && !(m_TopDownPlayer.character.moveMode == TPCharacter.MovingMode.Ledge) && halfHitAnimationPassed && !m_SwitchingItemState;
                    attack = Input.GetButtonDown("Fire1") && !m_SwitchingItemState && !triggerActive;
                    block = Input.GetButton("Block") && !m_SwitchingItemState && !triggerActive;
                }



                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");




                bool _blocking = false;
                if (!dive && !m_TopDownPlayer.character.isDiving)
                {

                    m_TopDownPlayer.animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                    if (block)
                    {
                        if (m_TopDownPlayer.character.isOnGround)
                        {
                            m_BreakCombo = true;
                            _blocking = true;

                            if (m_PrimaryTargetNPC != null)
                            {
                                bodyDirection = m_Direction2target;
                            }
                        }
                        m_TopDownPlayer.stop();
                    }


                    if (!_blocking)
                    {
                        m_TopDownPlayer.animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                        _attackCombo(attack, direction2cursor);
                    }
                    else
                    {
                        m_TopDownPlayer.animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, true);
                        m_TopDownPlayer.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                        m_BreakCombo = true;
                        m_Attack_combo_started = false;
                    }
                }

                m_TopDownPlayer.enableMove = !m_Attack_combo_started;
                if (m_Attack_combo_started)
                    m_TopDownPlayer.currentDestination = transform.position;
                runToggle = m_DoubleClick;


                if (m_Attack_combo_started)
                {
                    h = 0;
                    v = 0;
                }
                if (_blocking || takingHit)
                {
                    bodyDirection = transform.forward;
                }
            }
            m_TopDownPlayer.character.moveSpeedMultiplier = m_Stats.moveSpeed;

            if (dive)
            {
                m_PrevIKmode = m_Character.getIKMode();
                m_PrevMoveMode = m_Character.moveMode;
                _attack_combo_end();
            }

            // setup trigger parameters
            bool enableTriggerUse = !block && !takingHit && !m_Attack_combo_started;
            bool use = Input.GetButton("Use") && enableTriggerUse;
            bool secondaryUse = Input.GetButton("SecondaryUse") && enableTriggerUse;
            m_TopDownPlayer.triggers.update(h, v, use, secondaryUse, jump, enableTriggerUse);
            m_TopDownPlayer.control(h, v, jump, runToggle, dive, crouch, bodyDirection, direction2cursor);
        }

        /// <summary>
        /// control player in bow system mode
        /// </summary>
        protected override  void _controlPlayerBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            m_TopDownPlayer.animator.SetFloat(HashIDs.AttackSpeedFloat/*"pAttackSpeed"*/, m_Stats.currentAttackSpeed);
            bool onLedge = m_TopDownPlayer.animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);

            if (m_PrimaryTargetNPC != null)
            {
                m_Direction2target = m_PrimaryTargetNPC.position - transform.position;
                m_Direction2target.y = 0.0f;
                m_Direction2target.Normalize();
            }

            float h = 0.0f;
            float v = 0.0f;
            bool dive = false;
            bool attack = false;
            bool block = false;
            bool jump = false; // no jumping in top down
            bool runToggle = false;
            bool crouch = false; // no crouching in top down
            Vector3? bodyDirection = null;

            bool prevAim = m_Animator.GetBool("pAim");

            if (m_BowAttackStarted)
            {
                m_BowAttackTimer += Time.deltaTime;
                if (m_BowAttackTimer * bowShootSpeed >= m_BowAttackTime)
                {
                    _releaseBow();
                    m_BowAttackStarted = false;
                    m_Aiming = false;
                    m_Animator.SetBool(HashIDs.AimBool, false);
                    m_TopDownPlayer.strafing = false;
                    m_Animator.SetFloat("pBowSpeed", 1f);
                }
                _drawBow();
                //return;
            }


            Vector3 direction2cursor = transform.forward;
            if (!m_TopDownPlayer.disableInput)
            {
#if DEBUG_INFO
                if (!m_TopDownPlayer) { Debug.LogError("Cannot find player component." + " < " + this.ToString() + ">"); return; }
                if (!m_TopDownPlayer.m_Camera) { Debug.LogError("Cannont find m_Camera" + " < " + this.ToString() + ">"); return; }
                if (!m_TopDownPlayer.m_Camera.cameraComponent) { Debug.LogError("Cannot find CameraComponent." + " < " + this.ToString() + ">"); return; }
#endif
                bool switchEquipmentCondition = !m_SwitchingItemState && !m_Attack_combo_started && !onLedge && !m_TopDownPlayer.triggerActive;

                if (!dive && !m_Character.isDiving)
                    _bowControl(prevAim);

                if (Input.GetButtonDown("ToggleWeapon") && switchEquipmentCondition)
                {
                    toggleCurrentWeapon();
                }
                if (Input.GetButtonDown("DrawWeapon"))
                {
                    takeCurrentWeapon();
                }
                if (Input.GetButtonDown("SheatheWeapon"))
                {
                    sheatheCurrentWeapon();
                }
                if (Input.GetButtonDown("Drop") && switchEquipmentCondition)
                {
                    dropAllEquipment();
                }
                if (Input.GetButtonDown("WeaponShieldMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.Weapon1HShield);
                }
                if (Input.GetButtonDown("Weapon2HMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.Weapon2H);
                }
                if (Input.GetButtonDown("BowMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.Bow);
                }
                if (Input.GetButtonDown("DualWieldMode") && switchEquipmentCondition)
                {
                    switchWeaponMode(WeaponMode.DualWield);
                }

                Ray ray = m_TopDownPlayer.m_Camera.cameraComponent.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (m_CurrentWeaponMode == WeaponMode.Bow)
                {
                    if (m_CurrentArrowInHand != null)
                    {
                        if (Input.GetButton("Fire1"))
                        {
                            int arrow_mask = m_CurrentArrowInHand.layers;
                            if (Physics.SphereCast(ray, 0.4f, out hit, float.MaxValue, arrow_mask, QueryTriggerInteraction.Ignore))
                            {
                                m_BowShootPosition = hit.point;
                            }
                        }
                    }
                }


                int mask = m_TopDownPlayer.walkableTarrainMask;
                if (Physics.SphereCast(ray, 0.4f, out hit, float.MaxValue, mask, QueryTriggerInteraction.Ignore))
                {
                    UnityEngine.AI.NavMeshHit nhit;
                    Vector3 closest = hit.point;
                    if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out nhit, float.MaxValue, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        closest = nhit.position;
                    }
                    direction2cursor = closest - transform.position;
                    direction2cursor.y = 0.0f;
                    direction2cursor.Normalize();
                    if (Input.GetMouseButtonDown(1))
                    {
                        m_TopDownPlayer.currentDestination = closest;
                    }
                }

                if (!onLedge)
                {
                    // if taking hit
                    // allow dive only after halfish of taking hit animation is passed
                    bool halfHitAnimationPassed = true;
                    if (takingHit)
                    {
                        AnimatorStateInfo asi = m_TopDownPlayer.animator.GetCurrentAnimatorStateInfo(0);
                        float normTime = asi.normalizedTime;
                        halfHitAnimationPassed = normTime > 0.6f;
                    }
                    dive = Input.GetButtonDown("DiveRoll") && !(m_TopDownPlayer.character.moveMode == TPCharacter.MovingMode.Ledge) && halfHitAnimationPassed && !m_SwitchingItemState;
                    attack = Input.GetButtonDown("Fire1") && !m_SwitchingItemState && !triggerActive && !m_EquipmentScript.bowInHand;
                    block = Input.GetButton("Block") && !m_SwitchingItemState && !triggerActive && !m_EquipmentScript.bowInHand;
                }



                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");




                bool _blocking = false;
                if (!dive && !m_TopDownPlayer.character.isDiving)
                {

                    m_TopDownPlayer.animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                    if (block)
                    {
                        if (m_TopDownPlayer.character.isOnGround)
                        {
                            m_BreakCombo = true;
                            _blocking = true;

                            if (m_PrimaryTargetNPC != null)
                            {
                                bodyDirection = m_Direction2target;
                            }
                        }
                        m_TopDownPlayer.stop();
                    }


                    if (!_blocking)
                    {
                        m_TopDownPlayer.animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                        if (!m_BowDrawn)
                            _attackCombo(attack, direction2cursor);
                    }
                    else
                    {
                        m_TopDownPlayer.animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, true);
                        m_TopDownPlayer.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                        m_BreakCombo = true;
                        m_Attack_combo_started = false;
                    }
                }

                m_TopDownPlayer.enableMove = !m_Attack_combo_started;
                if (m_Attack_combo_started)
                    m_TopDownPlayer.currentDestination = transform.position;
                runToggle = m_DoubleClick;


                if (m_Attack_combo_started)
                {
                    h = 0;
                    v = 0;
                }
                if (_blocking || takingHit)
                {
                    bodyDirection = transform.forward;
                }
            }
            m_TopDownPlayer.character.moveSpeedMultiplier = m_Stats.moveSpeed;

            if (dive)
            {
                m_PrevIKmode = m_Character.getIKMode();
                m_PrevMoveMode = m_Character.moveMode;
                _attack_combo_end();
            }
            if (takingHit)
            {
                m_Aiming = false;
                _cancelDraw();
                m_Animator.SetBool(/*"pAim"*/HashIDs.AimBool, m_Aiming);
                m_TopDownPlayer.strafing = m_Aiming;
            }

            if (m_BowDrawn)
            {
                Vector3 dir2ShhotPos = m_BowShootPosition - transform.position;
                dir2ShhotPos.y = 0.0f;
                dir2ShhotPos.Normalize();
                bodyDirection = dir2ShhotPos;
                m_Character.applyExtraTurnRotation(bowShootSpeed * 2);
            }

            // setup trigger parameters
            bool enableTriggerUse = !block && !takingHit && !m_Attack_combo_started;
            bool use = Input.GetButton("Use") && enableTriggerUse;
            bool secondaryUse = Input.GetButton("SecondaryUse") && enableTriggerUse;
            m_TopDownPlayer.triggers.update(h, v, use, secondaryUse, jump, enableTriggerUse);
            m_TopDownPlayer.control(h, v, jump, runToggle, dive, crouch, bodyDirection, direction2cursor);
        }

        /// <summary>
        /// bow system late update
        /// </summary>
        private void _lateUpdateBow1()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_CurrentWeaponMode != WeaponMode.Bow) return;
            if (m_TopDownPlayer.disableInput) return;
            if (m_Character.isDiving) return;

            Transform spine = m_Animator.GetBoneTransform(HumanBodyBones.Spine);
#if DEBUG_INFO
            if (!spine)
            {
                Debug.LogError("Cannot find spine transform < " + this.ToString() + " >");
                return;
            }
#endif

            bool aim = m_Animator.GetBool(/*"pAim"*/HashIDs.AimBool);
            if (aim)
            {
                Quaternion startRot = spine.rotation;

                Vector3 aimPosition = m_BowShootPosition;

                Vector3 aimVectorS = aimPosition - spine.position;
                Vector3 aimDirectionS = aimVectorS.normalized;
                Vector3 spineAxis = Utils.CalculateDirectionAxis(spine);

                Quaternion aimRotationS = Quaternion.LookRotation(aimDirectionS, spine.up);
                aimRotationS *= Quaternion.AngleAxis(additionalSpineYrotation, spineAxis);


                spine.rotation = aimRotationS;
                Quaternion diff = Quaternion.FromToRotation(transform.forward, spine.forward);
                Vector3 diffEuler = diff.eulerAngles;
                float yOffset = diffEuler.y - additionalSpineYrotation;




                m_AimTimer += Time.deltaTime;
                float aimValue = Mathf.Min(m_AimTimer / m_AimTime, 1.0f);
                float maxAngle = 360;
                float angleValue = _distFromZeroAngle360(yOffset) / maxAngle;
                aimValue *= 1 - angleValue;
                spine.rotation = Quaternion.Slerp(startRot, aimRotationS, aimValue);


            }

            if (m_Aiming && m_arrowInPlace && m_EquipmentScript.currentBow.bowString)
            {
                m_EquipmentScript.currentBow.bowString.position =
                    m_EquipmentScript.bones.arrow_bone.position;
            }
        }

        /// <summary>
        /// attack procedure
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="direction2cursor"></param>
        private void _attackCombo(bool attack, Vector3 direction2cursor)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            bool isAttacking = m_TopDownPlayer.animator.GetBool(/*"pAttack1"*/HashIDs.Attack1Bool);

            if (attack)
            {
                if (m_TopDownPlayer.character.isOnGround)
                {
                    m_TopDownPlayer.stop();

                    m_TargetDirection = direction2cursor;
                    float angleDiff = Vector3.Angle(m_PrevTargetDirection, m_TargetDirection);
                    if (angleDiff > CHANGE_TARGET_ANGLE_BUFFER)
                    {
                        m_ChangeTarget = true;
                    }
                    m_PrevTargetDirection = m_TargetDirection;

                    bool attackState = m_TopDownPlayer.animator.GetCurrentAnimatorStateInfo(0).IsName("AttackCombo");

                    if (!m_Attack_combo_started && !isAttacking && !attackState)
                    {
                        IGameCharacter npc = m_PrimaryTargetNPC;
#if TESTING_USE_ALL_NPC_ARRAY
                        m_PrimaryTargetNPC = _getPrimaryTarget();
#else
                        m_PrimaryTargetNPC = _getPrimaryTargetInZone();
#endif
                        if (m_PrimaryTargetNPC != npc)
                        {
                            if (npc != null) npc.attack_end_notify(this, 1);
                        }
                        m_BreakCombo = false;
                        m_TopDownPlayer.character.turnTransformToDirection(m_TargetDirection);
                        m_TopDownPlayer.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, true);
                        m_Attack_combo_started = true;
                        if (m_PrimaryTargetNPC != null)
                        {
                            m_InCombat = true;
                            if (enableJumpToTarget) _jumpToTarget();
                        }
                    }
                    else
                    {

                        m_BreakCombo = false;

                    }
                }
            }
        }

        /// <summary>
        /// draw / release bow
        /// </summary>
        /// <param name="prevAim"></param>
        private void _bowControl(bool prevAim)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (!shootOnClick)
            {
                bool IS_BOWDRAW_STATE = m_Animator.GetCurrentAnimatorStateInfo(1).IsName("BowDraw");
                if ((Input.GetButton("Fire1") && m_EquipmentScript.bowInHand))
                    m_Aiming = true;
                else
                {
                    if (IS_BOWDRAW_STATE || m_ReleasingBowString)
                        m_DisableAimOnRelease = true;
                    else m_Aiming = false;
                }
                if (Input.GetButtonUp("Fire1") && prevAim)
                {
                    if (m_DrawBowTimer > m_BowMinDrawTime)
                        _releaseBow();
                    else _cancelDraw();
                }

                if (m_Aiming)
                    if (Input.GetButton("Fire1"))
                        _drawBow();

                if (Input.GetButton("Fire1"))
                {
                    m_DrawBowTimer += Time.deltaTime;
                    m_BowShootPower = m_DrawBowTimer;
                }
                else
                {
                    m_DrawBowTimer = 0.0f;
                }
            }
            else
            {
                if (!m_BowAttackStarted)
                {
                    if ((Input.GetButtonDown("Fire1") && m_EquipmentScript.bowInHand))
                    {
                        m_Animator.SetFloat("pBowSpeed", bowShootSpeed);
                        m_BowAttackStarted = true;
                        m_Aiming = true;
                        m_BowAttackTimer = 0.0f;
                    }
                }
            }
            m_Animator.SetBool(/*"pAim"*/HashIDs.AimBool, m_Aiming);
            m_TopDownPlayer.strafing = m_Aiming;
        }

        /// <summary>
        /// shoot arrow
        /// </summary>
        protected  override void _shootArrow()
        {
#if DEBUG_INFO
            if (!m_EquipmentScript) { Debug.LogError("object cannot be null"); return; }
            if (m_EquipmentScript.bones == null) { Debug.LogError("object cannot be null."); return; }
            if (m_TopDownPlayer.m_Camera == null) { Debug.LogError("object cannot be null"); return; }
            if (m_CurrentArrowInHand == null) { Debug.LogError("object cannot be null"); return; }
            if (!m_AudioMan) { Debug.LogError("object cannot be null"); return; }
#endif

            Vector3 arrow_pos = m_EquipmentScript.bones.arrow_bone.position;
            Vector3 currentTarget = m_BowShootPosition;
            Vector3 arrowDirection = (currentTarget - arrow_pos).normalized;
            float curStrength = Mathf.Clamp(m_BowShootPower * m_BowShootPower, 0.05f, 1.0f);
            float arrowspeed = m_EquipmentScript.currentBow.range * curStrength;
            ArrowPool.ShootArrow(m_CurrentArrowInHand, ref arrow_pos, ref arrowDirection, arrowspeed);
            m_CurrentArrowInHand = null;
            m_AudioMan.stopAudio();
            GameUtils.PlayRandomClip(m_AudioMan.audioSource,
                    m_EquipmentScript.currentBow.weaponAudio.attackHitSounds);
        }

        /// <summary>
        /// animation event called upon dive roll end time
        /// </summary>
        /// <param name="e"></param>
        void OnDiveRollEndEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            m_TopDownPlayer.stop();
        }

    }
}
