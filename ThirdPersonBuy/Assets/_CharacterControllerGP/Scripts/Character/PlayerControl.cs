// © 2016 Mario Lelas

#define ARROW_REBOUND_PHYSICS


#if DEBUG_INFO
// for testing
// go through all npcs in scene
//#define TESTING_USE_ALL_NPC_ARRAY  

//#define ADDITIONAL_DEBUG_INFO
#endif

using System.Collections.Generic;
using UnityEngine;


namespace MLSpace
{
    /// <summary>
    /// class that controls third person player in combat framework system
    /// </summary>
    [RequireComponent(typeof(PlayerThirdPerson))]
    public class PlayerControl : PlayerControlBase
    {
#region Fields

        private float def_cameraMinXangle;                  // default camera minimum x angle ( when aiming )
        private float def_cameraMaxXangle;                  // default camera maximum x angle ( when aiming )
        private OrbitCameraController.CameraConstraint
            def_Xconstraint;                                // default constraint around x axis on camera
        protected PlayerThirdPerson m_ThirdPersonPlayer;                // player third person reference

        #endregion

#region Properties

        /// <summary>
        /// return true if input is disabled otherwise false
        /// </summary>
        public bool disableInput
        {
            get
            {
#if DEBUG_INFO
                if (!m_Initialized)
                {
                    Debug.LogError("Component not initialized <" + this.ToString() + ">");
                    return false;
                }
#endif
                return m_ThirdPersonPlayer.disableInput;
            }
        }

        /// <summary>
        /// return true if attack combo is started otherway false
        /// </summary>
        public bool attackComboUnderway
        {
            get
            {
                return m_Attack_combo_started;
            }
        }
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
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
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


            if (m_Player is PlayerThirdPerson )
            {
                m_ThirdPersonPlayer = m_Player as PlayerThirdPerson ;
            }
            else
            {
                Debug.LogError("PlayerThirdPerson component missing <" + this.ToString() + ">");
                return false;
            }

            m_ItemPicker = GetComponent<ItemPicker>();
            if (!m_ItemPicker) { Debug.LogError("Cannot find 'ItemPicker' component! < " + this.ToString() + " >"); return false; }

            // setup ragdoll callbacks
            m_ThirdPersonPlayer.ragdoll.OnHit = () =>
            {
                m_Character.simulateRootMotion = false;
                m_Character.disableMove = true;
                m_Character.rigidBody.velocity = Vector3.zero;

                m_ThirdPersonPlayer.disableInput = true;
                m_ThirdPersonPlayer.noLookIK();
                m_Character.rigidBody.isKinematic = true;
                m_Character.rigidBody.detectCollisions = false;
                m_Character.capsule.enabled = false;

                if (m_ThirdPersonPlayer.ragdoll.isFullRagdoll)
                    m_ThirdPersonPlayer.m_Camera.switchTargets(m_ThirdPersonPlayer.ragdoll.RagdollBones[(int)BodyParts.Spine]);
            };
            m_ThirdPersonPlayer.ragdoll.OnStartTransition = () =>
            {
                if (!m_ThirdPersonPlayer.ragdoll.isFullRagdoll && !m_ThirdPersonPlayer.ragdoll.isGettingUp)
                {
                    m_Character.simulateRootMotion = true;
                    m_Character.rigidBody.detectCollisions = true;
                    m_Character.rigidBody.isKinematic = false;
                    m_Character.capsule.enabled = true;
                }
                else
                {
                    m_Animator.SetFloat(/*"pForward"*/HashIDs.ForwardFloat, 0.0f);
                    m_Animator.SetFloat(/*"pSide"*/HashIDs.SideFloat, 0.0f);
                }
            };
            m_ThirdPersonPlayer.ragdoll.ragdollEventTime = 3.0f;
            m_ThirdPersonPlayer.ragdoll.OnTimeEnd = () =>
            {
                if (m_Stats.currentHealth > 0)
                    m_ThirdPersonPlayer.ragdoll.blendToMecanim();
            };
            //m_Ragdoll.OnBlendEnd = () =>
            // {
            //     Debug.Log("ON BLEND END");
            // };
            //m_Ragdoll.OnGetUpEvent = () =>
            //  {
            //      Debug.Log("ON GET UP EVENT");
            //  };
            m_ThirdPersonPlayer.ragdoll.LastEvent = () =>
            {
                m_Character.simulateRootMotion = true;
                m_Character.disableMove = false;
                m_ThirdPersonPlayer.disableInput = false;

                if (m_ThirdPersonPlayer.lookTowardsCamera) m_Character.setIKMode(TPCharacter.IKMode.Head);

                m_Character.rigidBody.isKinematic = false;
                m_Character.rigidBody.detectCollisions = true;
                m_Character.capsule.enabled = true;
                m_ThirdPersonPlayer.m_Camera.switchTargets(m_ThirdPersonPlayer.m_Camera.Target);
            };

            if (m_ThirdPersonPlayer.lookTowardsCamera)
                m_Character.setIKMode(TPCharacter.IKMode.Head);


            m_ThirdPersonPlayer.OnAttackHit = attack_hit_notify;
            m_ThirdPersonPlayer.OnAttackEndNotify = attack_end_notify;
            m_ThirdPersonPlayer.OnAttackStartNotify = attack_start_notify;

            if (m_ThirdPersonPlayer.triggers)
            {
                // setup trigger callbacks
                m_ThirdPersonPlayer.triggers.OnTriggerStart = () =>
                {
                    if (m_ThirdPersonPlayer.legsIK) m_ThirdPersonPlayer.legsIK.enabled = false;
                    m_ThirdPersonPlayer.disableInput = true;
                    bool isOnLedge = m_Animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                    if (!isOnLedge)
                    {
                        m_ThirdPersonPlayer.noLookIK();
                        m_PrevStrafing = m_ThirdPersonPlayer.strafing;
                        m_ThirdPersonPlayer.strafing = false;
                    }
                };
                m_ThirdPersonPlayer.triggers.OnTriggerEnd = () =>
                {
                    bool isOnLedge = m_Animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
                    if (!isOnLedge)
                    {
                        m_Player.m_Camera.additiveRotation = false;
                        m_Player.strafing = m_PrevStrafing;
                        if (m_ThirdPersonPlayer.legsIK) m_ThirdPersonPlayer.legsIK.enabled = true;
                        if (m_ThirdPersonPlayer.lookTowardsCamera) m_Character.setIKMode(TPCharacter.IKMode.Head);
                        if (m_ThirdPersonPlayer.strafing) m_Character.setMoveMode(TPCharacter.MovingMode.Strafe);
                    }
                    m_ThirdPersonPlayer.disableInput = false;
                };
            }

            def_cameraMinXangle = (m_ThirdPersonPlayer.m_Camera as OrbitCameraController).minXAngle;
            def_cameraMaxXangle = (m_ThirdPersonPlayer.m_Camera as OrbitCameraController).maxXAngle;
            def_Xconstraint = (m_ThirdPersonPlayer.m_Camera as OrbitCameraController).Xconstraint;

            m_PrevTargetDirection = m_TargetDirection;

            m_Initialized = true;
            return m_Initialized;
        }

        /// <summary>
        /// start taking bow
        /// </summary>
        public override void takeBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_SwitchingItemState) return;
            if (!areArmsFree) return;

            if (m_EquipmentScript.currentBow &&
                m_EquipmentScript.currentQuiver)
            {
                _startTakingBow();
                if (m_ThirdPersonPlayer.m_Camera is OrbitCameraController)
                {
                    OrbitCameraController oCam = m_ThirdPersonPlayer.m_Camera as OrbitCameraController;
                    def_Xconstraint = oCam.Xconstraint;
                    oCam.minXAngle = -30;
                    oCam.maxXAngle = 35;
                }
            }
        }

        /// <summary>
        /// start sheathing bow
        /// </summary>
        public override void sheatheBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_SwitchingItemState) return;
            if (!areArmsFree) return;

            _startSheathingBow();
            if (m_ThirdPersonPlayer.m_Camera is OrbitCameraController)
            {
                OrbitCameraController oCam = m_ThirdPersonPlayer.m_Camera as OrbitCameraController;
                oCam.Xconstraint = def_Xconstraint;
                oCam.minXAngle = def_cameraMinXangle;
                oCam.maxXAngle = def_cameraMaxXangle;
            }
        }

        /// <summary>
        /// control player by input
        /// </summary>
        protected  override void _controlPlayer()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            m_Animator.SetFloat(HashIDs.AttackSpeedFloat /*"pAttackSpeed"*/, m_Stats.currentAttackSpeed);
            bool onLedge = m_Animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);

            if (m_PrimaryTargetNPC != null)
            {
                m_Direction2target = m_PrimaryTargetNPC.position - transform.position;
                m_Direction2target.y = 0.0f;
                m_Direction2target.Normalize();
            }

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


            float h = 0.0f;
            float v = 0.0f;
            bool dive = false;
            bool attack = false;
            bool block = false;
            bool jump = false;
            bool runToggle = false;
            bool crouch = false;
            Vector3? bodyDirection = null;

            m_AttackStateUnderway = m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HashIDs.UnarmedAttackComboState;
            m_AttackStateUnderway |= m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HashIDs.AttackComboState;

            if (!m_ThirdPersonPlayer.disableInput)
            {
                bool switchEquipmentCondition = !m_SwitchingItemState && !m_Attack_combo_started && !onLedge && !m_ThirdPersonPlayer.triggerActive;
                bool attackBlockCondition = !m_SwitchingItemState && !m_ThirdPersonPlayer.triggerActive;

                if (Input.GetButtonDown("ToggleWeapon") && switchEquipmentCondition)
                {
                    toggleCurrentWeapon();
                }
                if (Input.GetButtonDown("DrawWeapon") && switchEquipmentCondition)
                {
                    takeCurrentWeapon();
                }
                if (Input.GetButtonDown("SheatheWeapon") && switchEquipmentCondition)
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

                if (!onLedge)
                {
                    // if taking hit
                    // allow dive only after halfish of taking hit animation is passed
                    bool halfHitAnimationPassed = true;
                    if (takingHit)
                    {
                        AnimatorStateInfo asi = m_Animator.GetCurrentAnimatorStateInfo(0);
                        float normTime = asi.normalizedTime;
                        halfHitAnimationPassed = normTime > 0.6f;
                    }
                    dive = Input.GetButtonDown("DiveRoll") && !(m_Character.moveMode == TPCharacter.MovingMode.Ledge) && halfHitAnimationPassed && !m_SwitchingItemState;
                    attack = Input.GetButtonDown("Fire1") && attackBlockCondition;
                    block = Input.GetButton("Fire2") && attackBlockCondition;
                }

                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");

                Vector3 move = Vector3.zero;
                Vector3 forwDir = transform.forward;
                Vector3 rightDir = transform.right;
                if (m_ThirdPersonPlayer.m_Camera.transform != null)
                {
                    forwDir = m_ThirdPersonPlayer.m_Camera.transform.forward;
                    rightDir = m_ThirdPersonPlayer.m_Camera.transform.right;
                }
                move = v * forwDir + h * rightDir;

                if (move == Vector3.zero)
                {
                    move = transform.forward;
                    m_ChangeTarget = false;
                }
                move.y = 0.0f;

                m_TargetDirection = move.normalized;
                float angleDiff = Vector3.Angle(m_PrevTargetDirection, m_TargetDirection);
                if (angleDiff > CHANGE_TARGET_ANGLE_BUFFER)
                {
                    m_ChangeTarget = true;
                }
                m_PrevTargetDirection = m_TargetDirection;


                bool blocking = false;
                if (!dive && !m_Character.isDiving)
                {

                    m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                    if (block)
                    {
                        if (m_Character.isOnGround)
                        {
                            if (m_Attack_combo_started)
                                _attack_combo_end();
                            blocking = true;
                            if (m_PrimaryTargetNPC != null)
                            {
                                bodyDirection = m_Direction2target;
                            }
                        }
                    }


                    if (!blocking && !takingHit)
                    {
                        m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                        _attackCombo(attack && !m_SwitchingItemState);
                    }
                    else
                    {
                        m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, blocking);
                        m_Animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                        m_BreakCombo = true;
                        m_Attack_combo_started = false;
                    }
                }


                jump = Input.GetButtonDown("Jump") && !takingHit;
                runToggle = Input.GetButton("WalkToggle");
                crouch = Input.GetButton("Crouch");

                if (m_Attack_combo_started)
                {
                    h = 0;
                    v = 0;
                }
                if (blocking || takingHit)
                {
                    bodyDirection = transform.forward;
                }
            }

            // turn to npc if not changed by input
            if (!m_ChangeTarget)
            {
                if (m_PrimaryTargetNPC != null)
                {
                    if (!m_PrimaryTargetNPC.isDead)
                    {
                        bodyDirection = m_Direction2target;
                    }
                }
            }

            if (dive)
            {
                m_PrevIKmode = m_Character.getIKMode();
                m_PrevMoveMode = m_Character.moveMode;
                _attack_combo_end();
            }

            m_Character.moveSpeedMultiplier = m_Stats.DefaultMoveSpeed;

            // setup trigger parameters
            bool enableTriggerUse = !block && !takingHit && !m_Attack_combo_started;
            bool use = Input.GetButton("Use") && enableTriggerUse;
            // disable trigger use when picking item
            if (m_ItemPicker.highlighted)
                m_ThirdPersonPlayer.triggers.disableUse = true;
            m_ThirdPersonPlayer.triggers.update(h, v, use, false, jump, enableTriggerUse);
            m_ThirdPersonPlayer.control(h, v, jump, runToggle, dive, crouch, bodyDirection, m_TargetDirection);
        }

        /// <summary>
        /// control player in bow system mode
        /// </summary>
        protected override void _controlPlayerBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            bool onLedge = m_Animator.GetBool(/*"pOnLedge"*/HashIDs.OnLedgeBool);
            bool prevAim = m_Animator.GetBool("pAim");

            float h = 0.0f;
            float v = 0.0f;
            bool dive = false;
            bool attack = false;
            bool block = false;
            bool jump = false;
            bool runToggle = false;
            bool crouch = false;
            Vector3? bodyDirection = null;

            m_AttackStateUnderway = m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HashIDs.UnarmedAttackComboState;
            m_AttackStateUnderway |= m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HashIDs.AttackComboState;

            if (!m_ThirdPersonPlayer.disableInput)
            {
                bool switchEquipmentCondition = !m_SwitchingItemState && !m_Attack_combo_started && !onLedge && !m_ThirdPersonPlayer.triggerActive;
                bool attackBlockCondition = !m_SwitchingItemState && !m_ThirdPersonPlayer.triggerActive;

                if (!dive && !m_Character.isDiving)
                    _bowControl(prevAim);

                if (Input.GetButtonDown("ToggleWeapon") && switchEquipmentCondition)
                {
                    toggleCurrentWeapon();
                }
                if (Input.GetButtonDown("DrawWeapon") && switchEquipmentCondition)
                {
                    takeCurrentWeapon();
                }
                if (Input.GetButtonDown("SheatheWeapon") && switchEquipmentCondition)
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

                if (!onLedge)
                {
                    // if taking hit
                    // allow dive only after halfish of taking hit animation is passed
                    bool halfHitAnimationPassed = true;
                    if (takingHit)
                    {
                        AnimatorStateInfo asi = m_Animator.GetCurrentAnimatorStateInfo(0);
                        float normTime = asi.normalizedTime;
                        halfHitAnimationPassed = normTime > 0.6f;
                    }
                    dive = Input.GetButtonDown("DiveRoll") && !(m_Character.moveMode == TPCharacter.MovingMode.Ledge) && halfHitAnimationPassed && !m_SwitchingItemState;
                    attack = Input.GetButtonDown("Fire1") && attackBlockCondition && !m_EquipmentScript.bowInHand;
                    block = Input.GetButton("Fire2") && attackBlockCondition && !m_EquipmentScript.bowInHand;
                }

                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");

                Vector3 move = Vector3.zero;
                Vector3 forwDir = transform.forward;
                Vector3 rightDir = transform.right;
                if (m_ThirdPersonPlayer.m_Camera.transform != null)
                {
                    forwDir = m_ThirdPersonPlayer.m_Camera.transform.forward;
                    rightDir = m_ThirdPersonPlayer.m_Camera.transform.right;
                }
                move = v * forwDir + h * rightDir;

                if (move == Vector3.zero)
                {
                    move = transform.forward;
                    m_ChangeTarget = false;
                }
                move.y = 0.0f;

                m_TargetDirection = move.normalized;
                float angleDiff = Vector3.Angle(m_PrevTargetDirection, m_TargetDirection);
                if (angleDiff > CHANGE_TARGET_ANGLE_BUFFER)
                {
                    m_ChangeTarget = true;
                }
                m_PrevTargetDirection = m_TargetDirection;


                bool blocking = false;
                if (!dive && !m_Character.isDiving)
                {

                    m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                    if (block)
                    {
                        if (m_Character.isOnGround)
                        {
                            if (m_Attack_combo_started)
                                _attack_combo_end();
                            blocking = true;
                            if (m_PrimaryTargetNPC != null)
                            {
                                bodyDirection = m_Direction2target;
                            }
                        }
                    }


                    if (!blocking && !takingHit)
                    {
                        m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                        _attackCombo(attack && !m_SwitchingItemState);
                    }
                    else
                    {
                        m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, blocking);
                        m_Animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                        m_BreakCombo = true;
                        m_Attack_combo_started = false;
                    }
                }


                jump = Input.GetButtonDown("Jump") && !takingHit;
                runToggle = Input.GetButton("WalkToggle");
                crouch = Input.GetButton("Crouch");

                if (m_Attack_combo_started)
                {
                    h = 0;
                    v = 0;
                }
                if (blocking || takingHit)
                {
                    bodyDirection = transform.forward;
                }
            }

            // turn to npc if not changed by input
            if (!m_ChangeTarget)
            {
                if (m_PrimaryTargetNPC != null)
                {
                    if (!m_PrimaryTargetNPC.isDead)
                    {
                        bodyDirection = m_Direction2target;
                    }
                }
            }
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
                m_ThirdPersonPlayer.strafing = m_Aiming;
            }

            m_Character.moveSpeedMultiplier = m_Stats.DefaultMoveSpeed;

            // setup trigger parameters
            bool enableTriggerUse = !block && !takingHit && !m_Attack_combo_started && !m_Aiming;
            bool use = Input.GetButton("Use") && enableTriggerUse;
            // disable trigger use when picking item
            if (m_ItemPicker.highlighted)
                m_ThirdPersonPlayer.triggers.disableUse = true;
            m_ThirdPersonPlayer.triggers.update(h, v, use, false, jump, enableTriggerUse);
            m_ThirdPersonPlayer.control(h, v, jump, runToggle, dive, crouch, bodyDirection, m_TargetDirection);
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
            if (m_ThirdPersonPlayer.disableInput) return;
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

                Vector3 pos = m_ThirdPersonPlayer.m_Camera.transform.forward * 100f;
                Vector3 aimPosition = Camera.main.transform.position + pos;

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

            if (m_Aiming && m_arrowInPlace && m_EquipmentScript .currentBow .bowString )
            {
                m_EquipmentScript.currentBow.bowString.position =
                    m_EquipmentScript.bones.arrow_bone.position;
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
            bool IS_BOWDRAW_STATE = m_Animator.GetCurrentAnimatorStateInfo(1).IsName("BowDraw");
            if ((Input.GetMouseButton(0) && m_EquipmentScript.bowInHand) || (Input.GetMouseButton(1) && m_EquipmentScript.bowInHand))
                m_Aiming = true;
            else
            {
                if (IS_BOWDRAW_STATE || m_ReleasingBowString)
                    m_DisableAimOnRelease = true;
                else m_Aiming = false;
            }

            m_ThirdPersonPlayer.strafing = m_Aiming;

            if (Input.GetMouseButtonUp(0) && prevAim)
            {
                if (m_DrawBowTimer > m_BowMinDrawTime)
                    _releaseBow();
                else _cancelDraw();
            }
            m_Animator.SetBool(/*"pAim"*/HashIDs.AimBool, m_Aiming);

            if (m_Aiming)
                if (Input.GetMouseButton(0))
                    _drawBow();

            if (Input.GetMouseButton(0))
            {
                m_DrawBowTimer += Time.deltaTime;
                m_BowShootPower = m_DrawBowTimer;
            }
            else
            {
                m_DrawBowTimer = 0.0f;
            }
        }

        /// <summary>
        /// attack procedure
        /// </summary>
        /// <param name="attack"></param>
        private void _attackCombo(bool attack)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            bool isAttacking = m_Animator.GetBool(/*"pAttack1"*/HashIDs.Attack1Bool);

            if (attack)
            {
                if (m_Character.isOnGround)
                {
                    if (!m_Attack_combo_started && !isAttacking && !m_AttackStateUnderway)
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
                        m_Character.turnTransformToDirection(m_TargetDirection);
                        m_Animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, true);
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
    }
}
