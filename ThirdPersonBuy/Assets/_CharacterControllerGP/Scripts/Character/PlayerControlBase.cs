// © 2016 Mario Lelas
using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{
    /// <summary>
    /// current weapon modes  单手可配盾  双手单武器不可配盾  弓箭  双手双武器
    /// </summary>
    public enum WeaponMode { Weapon1HShield, Weapon2H, Bow, DualWield };

    /// <summary>
    /// animation clip sheets
    /// </summary>
    public enum AnimClipSheet { None, Weapon1H, Shield, WeaponShield, DualWeapons, Weapon2H };

    /// <summary>
    /// Player arm motions.
    /// Reaching towards taking/sheating item
    /// or from it.
    /// </summary>
    public enum ArmOccupation
    {
        None,
        ToReachWeapon1H,        // reach to take / sheathe插入 weapon 1H
        ToReachWeapon2H,        // reach to take / sheathe weapon 2H
        ToReachShield,          // reach to take / sheathe shield
        ToReachSecondary,       // reach to take / sheathe secondary weapon 1H
        ToReachBow,             // reach to take / sheathe bow
        ToReachArrow,           // reach to take arrow
        FromReachWeapon1H,      // reach from taking / sheathing weapon 1H
        FromReachShield,        // reach from taking / sheathing shield
        FromReachSecondary      // reach from taking / sheathing secondary weapon 1H
    };


    /// <summary>
    /// base class for combat framework controllers
    /// </summary>
    public abstract class PlayerControlBase : MonoBehaviour, IGameCharacter
    {

        /// <summary>
        /// attack collider and rigid body for checking obstacles upon attack
        /// 打扫
        /// </summary>
        [Tooltip("Attack collider and rigid body for checking obstacles.")]
        public Rigidbody attackSweepBody;

        /// <summary>
        /// height from which character dies if falls
        /// </summary>
        [Tooltip("Height from which character dies if falls.")]
        public float fallDieHeight = 10.0f;

        /// <summary>
        /// angle of attack jump
        /// </summary>
        [Tooltip("Angle of getting primary target upon attack.")]
        public float attack_angle = 45.0f;

        /// <summary>
        /// enable/disable jump to target on attac
        /// </summary>
        [Tooltip("Enable/disable jump to target on attack.")]
        public bool enableJumpToTarget = true;

        /// <summary>
        /// reach of attack jump
        /// </summary>
        [Tooltip("Reach of attack jump.")]
        [HideInInspector]
        public float attack_jump_distance = 10.0f;

        /// <summary>
        /// player distance to target upon attack
        /// </summary>
        [Tooltip("Player distance to target upon attack.")]
        public float distanceToTarget = 1.5f;


        /// <summary>
        /// additional spine Y rotation used in aiming ( bow )
        /// 腿总是往前走，只是身体可以旋转
        /// </summary>
        [Tooltip("Additional spine Y rotation used in aiming ( bow )")]
        public float additionalSpineYrotation = 52.0f;

        /// <summary>
        /// dual attack combo clip
        /// used as secondary in dual weapon mode
        /// </summary>
        [HideInInspector]
        public AnimationClipReplacementInfo dualWeaponAttackClip;

        /// <summary>
        /// dual wield block stance clip
        /// used in dual weapon mode
        /// </summary>
        [HideInInspector]
        public AnimationClipReplacementInfo dualWeaponBlockStance;

        /// <summary>
        ///  dual wield block hit clips
        ///  used in dual weapon mode
        /// </summary>
        [HideInInspector]
        public List<AnimationClipReplacementInfo> dualWeaponBlockHitClips;

        /// <summary>
        ///  dual wield locomotion clips
        ///  used  in dual weapon mode
        /// </summary>
        [HideInInspector]
        public List<AnimationClipReplacementInfo> dualWeaponLocomotionClips;

        /// <summary>
        /// dual wield replacement clips
        /// used in dual weapon mode
        /// </summary>
        [HideInInspector]
        public List<AnimationClipReplacementInfo> dualWeaponReplacementClips;

#if UNITY_EDITOR
        /// <summary>
        /// helper used in inspector editor only
        /// </summary>
        [HideInInspector]
        public int dualWeaponBlockHitListSize = 0;

        /// <summary>
        /// helper used in inspector editor only
        /// </summary>
        [HideInInspector]
        public int dualWeaponLocomotionListSize = 0;

        /// <summary>
        /// helper used in inspector editor only
        /// </summary>
        [HideInInspector]
        public int dualWeaponReplacementListSize = 0;

#endif



        protected WeaponAudio defaultWeaponSounds;            // default melee sounds
        protected WeaponItem currentPrimaryWeapon;       // current primary  weapon 
        protected WeaponItem currentSecondaryWeapon;     // current secondary melee weapon ( shield )
        protected WeaponAudio currentBlockSounds;             // current block sounds ( default, weapon or shield )

        protected EquipmentScript m_EquipmentScript;                      // holds character equipment
        protected Stats m_Stats;                                          // player stats 
        protected NPCManager m_NpcMan;                                    // reference to npc array class
        protected IGameCharacter m_PrimaryTargetNPC;                      // primary npc in contact
        protected Predefines m_Materials;                                 // reference to class that holds materials array
        protected Player m_Player;                                        // player reference
        protected TPCharacter m_Character;                                // reference to third person character script
        protected AudioManager m_AudioMan;                                // ref to audio manager
        protected ItemPicker m_ItemPicker;                                // ref to ItemPicker component
        protected Animator m_Animator;                                    // ref to animator component

        // changing clips helpers
        protected AnimClipSheet m_CurrentClipSheet = AnimClipSheet.None;          // current clip sheet
        protected WeaponItem m_PrimaryClipSheetSource = null;                     // current primary clip source
        protected WeaponItem m_SecondaryClipSheetSource = null;                   // current secondary clip source
        protected WeaponMode m_CurrentWeaponMode = WeaponMode.Weapon1HShield;     // current weapon mode

        protected bool m_Attack_combo_started = false;                    // attack combo started flag
        protected bool m_InCombat = false;                                // is player in combat ? 是否处在主动攻击状态
        protected bool m_BreakCombo = true;                               // break combo flag
        protected int m_CurrentAttackType = 0;                            // current attack type ( used for hit reactions )
        protected Vector3 m_Direction2target;                             // direction to current npc
        protected Vector3 m_TargetDirection;                              // target direction
        protected Vector3 m_PrevTargetDirection;                          // previous target direction
        protected bool m_ChangeTarget = true;                             // change target flag
        protected const float CHANGE_TARGET_ANGLE_BUFFER = 30.0f;         // angle buffer when changing target      
        protected bool m_AttackStateUnderway = false;                     // is any of the animator attack states on
        protected int m_AttackHitCount = 0;                               // attack hit between attack starts
        protected float m_DefaultDistance2Target = 1.5f;                  // default distance to target upon attack 

        // weapons system helpers
        protected bool m_SetSecondaryAsPrimary = false;                           // set primary weapon as secondary if primary is missing
        protected const float locomotionTransitionSpeed = 0.151f;                 // transition time from default to unarmed locomotion - and vice versa
        protected bool m_SwitchingItemState = false;                             
        // is player in any of reach weapon transition animator state
        //是否处于武器切换状态 
        protected ArmOccupation m_LeftArmOccupation = ArmOccupation.None;         // left arm occupation ( swithcing items motions )
        protected ArmOccupation m_RightArmOccupation = ArmOccupation.None;        // right arm occupation ( swithcing items motions )   

        // archery system helpers
        protected bool m_arrowInPlace = false;                                // is arrow ready for shooting
        protected float m_AimTime = 0.125f,                                    //
            m_AimTimer = 0.0f;                                              // aiming helpers
        protected Arrow m_CurrentArrowInHand = null;                          // current arrow player is holding
        protected bool m_BowDrawn = false;                                    // is bow string drawn
        protected RayHitComparer m_RayHitComparer = new RayHitComparer();     // variable to compare raycast hit distances
        protected bool m_Aiming = false;
        protected float m_BowMinDrawTime = 0.35f;         // minimum time drawn needed to arrow be released
        protected bool m_DisableAimOnRelease = false;     // disable aiming after arrow has been shot ( or not )
        protected float m_DrawBowTimer = 0.0f;            // bow draw timer 
        protected float m_BowShootPower = 1.0f;           // bow draw power
        protected bool m_ReleasingBowString = false;      // releasing bow string flag

        protected bool m_Initialized = false;                             // is component initialized
        protected List<TimedFunc> m_TimedFuncs = new List<TimedFunc>();             // timed function list
        protected List<IGameCharacter> m_NpcsInRange = new List<IGameCharacter>();  // npcs in range list

        protected TPCharacter.IKMode m_PrevIKmode = TPCharacter.IKMode.None;      // keep track of ik mode if need to reset
        protected TPCharacter.MovingMode m_PrevMoveMode =
            TPCharacter.MovingMode.RotateToDirection;                           // keep track of move mode if need to reset
        protected bool m_PrevStrafing = false;                                  // keep track of strafe mode if need to reset

        // rotations to attack helpers  攻击时的旋转
        protected float m_AttackRotTime = 0.25f;                      // attack rotation max time
        protected float m_AttackRotTimer = 0.0f;                      // attack rotation timer
        protected bool m_DoAttackRotation = false;                    // attack rotation underway 进行中
        protected Quaternion m_AttackStartRot = Quaternion.identity;  // attack start rotation
        protected Quaternion m_AttackEndRot = Quaternion.identity;    // attack end rotation

        /// <summary>
        /// function delegate to trigger on take weapon1h
        /// </summary>
        public VoidFunc OnTakeWeapon1H;

        /// <summary>
        /// function delegate to trigger on sheathe weapon 1h
        /// </summary>
        public VoidFunc OnSheatheWeapon1H;

        /// <summary>
        /// function delegate to trigger on take secondary weapon 1h
        /// </summary>
        public VoidFunc OnTakeSecondaryWeapon1H;

        /// <summary>
        /// function delegate to trigger on sheathe secondary weapon 1h
        /// </summary>
        public VoidFunc OnSheatheSecondaryWeapon1H;

        /// <summary>
        /// function delegate to trigger on take shield
        /// </summary>
        public VoidFunc OnTakeShield;

        /// <summary>
        /// function delegate to trigger on putaway shield
        /// </summary>
        public VoidFunc OnPutawayShield;

        /// <summary>
        /// function delegeate to trigger on taking weapon 2H
        /// </summary>
        public VoidFunc OnTakeWeapon2H;

        /// <summary>
        /// function delegate top trigger on sheathe weapon 2H 
        /// </summary>
        public VoidFunc OnSheatheWeapon2H;

        /// <summary>
        /// function delegeate to trigger on taking bow
        /// </summary>
        public VoidFunc OnTakeBow;

        /// <summary>
        /// function delegate top trigger on sheathe bow
        /// </summary>
        public VoidFunc OnPutawayBow;

        /// <summary>
        /// on player death callback
        /// </summary>
        public VoidFunc OnDeath = null; 


#region Properties

        /// <summary>
        /// gets Stats component
        /// </summary>
        public Stats stats { get { return m_Stats; } }

        /// <summary>
        /// gets TPCharacter component
        /// </summary>
        public TPCharacter character { get { return m_Player.character; } }

        /// <summary>
        /// gets AudioManager component
        /// </summary>
        public AudioManager audioManager { get { return m_AudioMan; } }

        /// <summary>
        /// gets position
        /// </summary>
        public Vector3 position { get { return transform.position; } }

        /// <summary>
        /// return true if player is in combat ( taking hit or attacking
        /// </summary>
        public bool inCombat { get { return m_InCombat; } }

        /// <summary>
        /// gets reference to ragdoll manager
        /// </summary>
        public RagdollManager ragdoll { get { return m_Player.ragdoll; } }

        /// <summary>
        /// is player is dive rolling
        /// </summary>
        public bool diveRolling { get { return m_Player.character.isDiving; } }

        /// <summary>
        /// gets and sets taking hit flag
        /// </summary>
        public bool takingHit { get; set; }

        /// <summary>
        /// returns true if character is blocking animation is on - otherwise false
        /// </summary>
        public bool blocking
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
                return m_Player.animator.GetBool(HashIDs.BlockBool);
            }
        }

        /// <summary>
        /// gets reference to EquipmentScript
        /// </summary>
        public EquipmentScript equipmentScript
        {
            get
            {
#if DEBUG_INFO
                if (!m_Initialized)
                {
                    Debug.LogError("Component not initialized <" + this.ToString() + ">");
                    return null;
                }
#endif
                return m_EquipmentScript;
            }
        }

        /// <summary>
        /// gets TriggerManagement component
        /// </summary>
        public TriggerManagement triggers
        {
            get
            {
#if DEBUG_INFO
                if (!m_Initialized)
                {
                    Debug.LogError("Component not initialized <" + this.ToString() + ">");
                    return null;
                }
#endif
                return m_Player.triggers;
            }
        }

        /// <summary>
        /// returns true if trigger is active otherwise false
        /// </summary>
        public bool triggerActive
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
                return m_Player.triggerActive;
            }
        }

        /// <summary>
        /// IGameCharacter interface implemetation
        /// is character dead
        /// </summary>
        public bool isDead
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
                return m_Stats.currentHealth <= 0;
            }
        }

        /// <summary>
        /// returns true if both arms are free ( not occupied by switching items )
        /// </summary>
        public bool areArmsFree
        {
            get
            {
                return m_RightArmOccupation == ArmOccupation.None &&
                  m_LeftArmOccupation == ArmOccupation.None;
            }
        }

#endregion

        public virtual bool initialize()
        {
            if (m_Initialized)
                return true;

            m_Player = GetComponent<Player>();
            if (!m_Player) { Debug.LogError("Cannot find component 'PlayerThirdPerson'" + " < " + this.ToString() + ">"); return false; }
            m_Player.initialize();
            m_Materials = m_Player.character.physicsMaterials;
            m_Character = m_Player.character;
            m_Animator = m_Player.animator;
            if (!m_Character) { Debug.LogError("Cannot find 'TPCharacter' component < " + this.ToString() + " >"); return false; }
            if (!m_Animator) { Debug.LogError("Cannot find 'Animator' component < " + this.ToString() + " >"); return false; }

            m_EquipmentScript = GetComponent<EquipmentScript>();
            if (!m_EquipmentScript) { Debug.LogError("Cannot find 'EquipmentScript' component!" + " < " + this.ToString() + ">"); return false; }
            m_EquipmentScript.initialize();

            m_Stats = GetComponent<Stats>();
            if (!m_Stats) { Debug.LogError("Cannot find 'Stats' component: " + " < " + this.ToString() + ">"); return false; }

            AudioManager amg = m_Character.audioManager as AudioManager;
            if (!amg) { Debug.LogError("Cannot find component 'AudioManager'" + " < " + this.ToString() + ">"); return false; }
            m_AudioMan = amg;

            m_NpcMan = FindObjectOfType<NPCManager>();
            if (!m_NpcMan) { Debug.LogError("Cannot find object of type 'NPCManager'" + " < " + this.ToString() + ">"); return false; }


            defaultWeaponSounds = GetComponent<WeaponAudio>();
            if (!defaultWeaponSounds) { Debug.LogError("Cannot find default 'WeaponAudio' component"); return false; }
            currentBlockSounds = defaultWeaponSounds;


            // collecting all hashes in awake func
            dualWeaponAttackClip.orig_hash = Animator.StringToHash(dualWeaponAttackClip.original_name);
            dualWeaponBlockStance.orig_hash = Animator.StringToHash(dualWeaponBlockStance.original_name);
            for (int i = 0; i < dualWeaponBlockHitClips.Count; i++)
            {
                dualWeaponBlockHitClips[i].orig_hash =
                    Animator.StringToHash(dualWeaponBlockHitClips[i].original_name);
            }
            for (int i = 0; i < dualWeaponLocomotionClips.Count; i++)
            {
                dualWeaponLocomotionClips[i].orig_hash =
                    Animator.StringToHash(dualWeaponLocomotionClips[i].original_name);
            }
            m_Animator.SetBool(/*"pUnarmed"*/HashIDs.UnarmedBool, true);

            if (!attackSweepBody) { Debug.LogError("Attack sweep rigidbody missing! <" + this.ToString() + ">"); return false; }

            m_DefaultDistance2Target = distanceToTarget;

            m_Initialized = true;
            return m_Initialized;
        }

        /// <summary>
        /// update component
        /// </summary>
        protected  virtual void update()
        {
            if (Time.timeScale == 0.0f) return;
            // if in ragdoll - just return and do nothing
            //（被攻击或者高出摔下）处在ragdoll状态，无法控制。
            if (m_Player.ragdoll.state != RagdollManager.RagdollState.Animated)
            {
                m_InCombat = false; //不可能处于战斗状态。
                return;
            }
            //动画分成4层，全身，上半身  左手，右手

            AnimatorStateInfo upperBodyLayer = m_Animator.GetCurrentAnimatorStateInfo(1);
            AnimatorStateInfo leftArmLayer = m_Animator.GetCurrentAnimatorStateInfo(2);
            AnimatorStateInfo rightArmLayer = m_Animator.GetCurrentAnimatorStateInfo(3);

            m_SwitchingItemState = upperBodyLayer.shortNameHash == HashIDs.TakeWeaponRightState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeWeaponLeftState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeShieldState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeWeaponShieldState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeSecondaryWeaponState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeDualWeaponsState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeSecWeaponShieldState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.TakeWeapon2HState;
            m_SwitchingItemState |= upperBodyLayer.shortNameHash == HashIDs.SheatheWeapon2HState;
            m_SwitchingItemState |= leftArmLayer.shortNameHash == HashIDs.TakeBowState;
            m_SwitchingItemState |= rightArmLayer.shortNameHash == HashIDs.TakeArrowState;

            for (int i = m_TimedFuncs.Count - 1; i >= 0; i--)
            {
                TimedFunc tf = m_TimedFuncs[i];
                tf.timer += Time.deltaTime;
                if (tf.timer >= tf.time)
                {
                    if (tf.func != null)
                    {
                        tf.func();
                        if (tf.repeat)
                        {
                            tf.timer = 0.0f;
                        }
                        else
                        {
                            m_TimedFuncs.RemoveAt(i);
                        }
                    }
                    else
                    {
                        m_TimedFuncs.RemoveAt(i);
                    }
                }
            }

            if (m_DoAttackRotation)
            {
                m_AttackRotTimer += Time.deltaTime;
                if (m_AttackRotTimer >= m_AttackRotTime)
                {
                    transform.rotation = m_AttackEndRot;
                    m_DoAttackRotation = false;
                }
                float rVal = m_AttackRotTimer / m_AttackRotTime;
                transform.rotation = Quaternion.Slerp(m_AttackStartRot, m_AttackEndRot, rVal);
                //攻击时旋转，从m_AttackStartRot 逐步插值旋转到m_AttackEndRot
            }

            // using crouch flag for releasing ledge
            if (Input.GetButtonDown("Crouch"))
            {
                triggers.breakTrigger();
            }

            if (m_CurrentWeaponMode == WeaponMode.Bow)
                _controlPlayerBow(); // control + aiming
            else
                _controlPlayer();
            _fallToDeathCheck();

            // draw primary and other npcs in range
#if DEBUG_INFO
            if (m_PrimaryTargetNPC != null)
            {
                Debug.DrawLine(m_PrimaryTargetNPC.transform.position, m_PrimaryTargetNPC.transform.position + Vector3.up * 3, Color.red);
            }
            foreach (IGameCharacter p in m_NpcsInRange)
            {

                Debug.DrawLine(p.transform.position, p.transform.position + Vector3.up * 2.5f, Color.blue);
            }
#endif
            m_Character.overridePhysicMaterial(null);
            //攻击（击中对象） 防御   被击中
            if (m_Attack_combo_started || blocking || takingHit)
            {
                m_Character.overridePhysicMaterial(m_Materials.zeroFrictionMaterial);
            }
        }

        /// <summary>
        /// control player by input
        /// </summary>
        protected abstract void _controlPlayer();


        /// <summary>
        /// control player in bow system mode
        /// </summary>
        protected abstract void _controlPlayerBow();

        /// <summary>
        /// notify game character  that he is attacked
        /// </summary>
        /// <param name="attacker">attacker game character</param>
        /// <param name="hitType">attack type ( used for hit reactions )</param>
        /// <param name="attackSource">attack source</param>
        /// <param name="blocking">assigns defender blocked</param>
        /// <param name="applyHitReaction">apply hit reaction animation</param>
        /// <param name="hitVelocity">attack hit velockity - applied on ragdoll</param>
        /// <param name="hitParts">hit body parts - applied on ragdoll</param>
        /// <returns>success</returns>
        public bool attack_hit_notify(IGameCharacter attacker, int hitType, int attackSource, ref bool blocked,
            bool applyHitReaction, Vector3? hitVelocity = null, int[] hitParts = null)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return false;
            }
#endif

            if (m_Character.isDiving) return false;

            m_InCombat = true;
            Vector3 toAttacker = attacker.stats.transform.position - transform.position;
            Vector3 m_BodyLookDir = toAttacker;
            m_BodyLookDir.y = 0.0f;
            if (m_BodyLookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(m_BodyLookDir);

            blocked = m_Animator.GetBool(HashIDs.BlockBool /*"pBlock"*/);

            // set attacker to be primary
            m_PrimaryTargetNPC = attacker;

            _startHitReaction(hitType, applyHitReaction, attackSource);

            /*int damageApplied =*/
            GameUtils.CalculateDamage(this, attacker, blocked);
            if (m_Stats.currentHealth <= 0)
            {
                if (m_PrimaryTargetNPC != null) m_PrimaryTargetNPC.attack_end_notify(this, 0);
                foreach (NPCScript npc in m_NpcsInRange)
                    npc.attack_end_notify(this, 0);


                m_InCombat = false;
                m_Player.ragdoll.startRagdoll(hitParts, hitVelocity, hitVelocity * 0.25f);
                attackSweepBody.gameObject.SetActive(false);
                if (OnDeath != null)
                    OnDeath();
            }

            return true;
        }

        /// <summary>
        /// notify game character  on attack start
        /// </summary>
        /// <param name="attacker">attacker game character</param>
        /// <param name="attackType">attack type ( used for hit reactions )</param>
        public void attack_start_notify(IGameCharacter attacker, int attackType)
        {
        }

        /// <summary>
        /// notify game character on attack end
        /// </summary>
        /// <param name="attacker">attacker game character</param>
        /// <param name="attackType">attack type ( used for hit reactions )</param>
        public void attack_end_notify(IGameCharacter attacker, int attackType)
        {
            if (!m_Attack_combo_started && !blocking && !takingHit)
            {
                m_InCombat = false;
                m_PrimaryTargetNPC = null;
            }
        }


        /// <summary>
        /// set new item on character
        /// </summary>
        /// <param name="iItem"></param>
        public InventoryItem setNewItem(InventoryItem iItem)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return null;
            }
#endif
            InventoryItem old = m_EquipmentScript.setItem(iItem);

            // set new stats / clips
            switch (iItem.itemType)
            {
                case InventoryItemType.Weapon1H:
                    {
                        MeleeWeaponItem meleeItem = iItem as MeleeWeaponItem;
                        if (m_EquipmentScript.weaponInHand1H)
                        {
                            currentPrimaryWeapon = meleeItem;

                            if (m_CurrentWeaponMode == WeaponMode.Weapon1HShield)
                            {
                                m_Stats.setCurrentAttackValue(
                                    meleeItem.damage,
                                    meleeItem.range,
                                    m_Stats.attackSpeed);
                                distanceToTarget = meleeItem.range * 0.75f;


                                if (m_EquipmentScript.shieldInHand)
                                {
                                    currentBlockSounds = m_EquipmentScript.currentShield.weaponAudio;
                                    _setWeapon1HShieldClips(meleeItem, m_EquipmentScript.currentShield);
                                }
                                else
                                {
                                    currentBlockSounds = meleeItem.weaponAudio;
                                    _setWeaponClips(meleeItem);
                                }
                            }
                            else if (m_CurrentWeaponMode == WeaponMode.DualWield)
                            {
                                bool set_dual = (m_EquipmentScript.currentSecondary && m_EquipmentScript.currentWeapon1H);
                                if (set_dual) _setDualWeaponClips();
                                else _setWeaponClips(meleeItem);
                                currentBlockSounds = meleeItem.weaponAudio;
                            }
                            else
                            {
                                currentBlockSounds = meleeItem.weaponAudio;
                                _setWeaponClips(meleeItem);
                            }
                            if (meleeItem.OnTake != null)
                                meleeItem.OnTake();
                        }
                        else
                        {
                            if (m_CurrentWeaponMode == WeaponMode.Weapon1HShield)
                            {
                                if (m_EquipmentScript.shieldInHand)
                                {
                                    _startTakingWeapon1H(AnimClipSheet.WeaponShield);
                                }
                                else
                                {
                                    if (m_EquipmentScript.currentShield != null)
                                    {
                                        _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H, m_EquipmentScript.currentShield);
                                    }
                                    else
                                    {
                                        _setWeaponClips(meleeItem);
                                    }
                                }
                            }
                            else if (m_CurrentWeaponMode == WeaponMode.DualWield)
                            {
                                bool set_dual = (m_EquipmentScript.currentSecondary && m_EquipmentScript.currentWeapon1H);
                                if (set_dual) _setDualWeaponClips();
                                else _setWeaponClips(meleeItem);
                            }
                        }
                    }
                    break;
                case InventoryItemType.Shield:
                    {
                        ShieldItem shieldItem = iItem as ShieldItem;
                        if (m_EquipmentScript.shieldInHand)
                        {
                            currentBlockSounds = shieldItem.weaponAudio;
                            if (!m_EquipmentScript.weaponInHand1H)
                            {
                                m_Stats.setCurrentAttackValue(
                                    shieldItem.damage,
                                    shieldItem.range,
                                    m_Stats.attackSpeed);
                                distanceToTarget = shieldItem.range * 0.75f;
                            }

                            if (m_EquipmentScript.currentWeapon1H != null)
                            {
                                _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H, m_EquipmentScript.currentShield);
                                currentSecondaryWeapon = shieldItem;
                            }
                            else
                            {
                                currentPrimaryWeapon = shieldItem;
                                _setShieldClips(shieldItem);
                            }
                            if (shieldItem.OnTake != null)
                                shieldItem.OnTake();
                        }
                        else
                        {
                            if (m_CurrentWeaponMode == WeaponMode.Weapon1HShield)
                            {
                                if (m_EquipmentScript.currentWeapon1H)
                                {
                                    if (m_EquipmentScript.weaponInHand1H)
                                    {
                                        _startTakingShield(AnimClipSheet.WeaponShield);
                                    }
                                    else
                                    {
                                        _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H, m_EquipmentScript.currentShield);
                                    }
                                }
                                else if (m_EquipmentScript.currentSecondary)
                                {
                                    _setWeapon1HShieldClips(m_EquipmentScript.currentSecondary, m_EquipmentScript.currentShield);
                                }
                                else
                                {
                                    _setShieldClips(shieldItem);
                                }
                            }
                        }
                    }
                    break;
                case InventoryItemType.Weapon2H:
                    {
                        MeleeWeaponItem meleeItem = iItem as MeleeWeaponItem;
                        if (m_EquipmentScript.weaponInHand2H)
                        {
                            currentPrimaryWeapon = meleeItem;
                            m_Stats.setCurrentAttackValue(
                                    meleeItem.damage,
                                    meleeItem.range,
                                    m_Stats.attackSpeed);
                            distanceToTarget = meleeItem.range * 0.75f;

                            if (meleeItem.OnTake != null)
                                meleeItem.OnTake();
                        }
                        if (m_CurrentWeaponMode == WeaponMode.Weapon2H)
                        {
                            _setWeaponClips(meleeItem);
                        }
                    }
                    break;
                case InventoryItemType.Bow:
                    if (m_EquipmentScript.bowInHand)
                    {
                        if (m_EquipmentScript.currentBow.OnTake != null)
                            m_EquipmentScript.currentBow.OnTake();
                        int damage = m_EquipmentScript.currentBow.damage + m_EquipmentScript.currentQuiver.damage;
                        m_Stats.setCurrentAttackValue(damage, m_Stats.weaponReach, m_Stats.currentAttackSpeed);
                    }
                    break;
                case InventoryItemType.QuiverArrow:

                    if (m_EquipmentScript.bowInHand)
                    {
                        QuiverItem q = iItem as QuiverItem;
                        if (q)
                        {
                            int damage = m_EquipmentScript.currentBow.damage + q.damage;
                            m_Stats.setCurrentAttackValue(damage, m_Stats.weaponReach, m_Stats.attackSpeed);
                        }
                    }

                    break;
            }
            if (old)
            {
                Vector3 dropPos = transform.position + transform.right;
                old.dropItem(dropPos, null);
                old.gameObject.SetActive(true);
            }
            return old;
        }

        /// <summary>
        /// set new secondary item on charater
        /// </summary>
        /// <param name="meleeItem"></param>
        public InventoryItem setSecondaryItem(InventoryItem item)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return null;
            }
#endif
            if (!(item is MeleeWeaponItem)) return null;

            MeleeWeaponItem meleeItem = item as MeleeWeaponItem;

            if (meleeItem.itemType != InventoryItemType.Weapon1H) return null;

            InventoryItem prevObj = null;
            InventoryItem currObj = meleeItem;


            prevObj = m_EquipmentScript.currentSecondary;
            if (prevObj)
            {
                if (prevObj != currObj)
                {
                    m_EquipmentScript.unsetSecondary1HWeapon();
                }
            }
            if (currObj is MeleeWeaponItem)
            {
                m_EquipmentScript.setSecondary1HWeapon(meleeItem);
            }

            if (m_CurrentWeaponMode == WeaponMode.DualWield)
            {

                if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.secondaryWeaponInHand)
                {
                    currentSecondaryWeapon = meleeItem;
                    _setDualWeaponClips();
                    if (meleeItem.OnTake != null)
                        meleeItem.OnTake();
                }
                else if (m_EquipmentScript.weaponInHand1H)
                {
                    currentSecondaryWeapon = meleeItem;
                    if (!prevObj)
                    {
                        _startTakingSecondaryWeapon1H();
                    }
                    else
                    {
                        _setDualWeaponClips();
                    }
                }
                else
                {
                    bool set_dual = (m_EquipmentScript.currentSecondary && m_EquipmentScript.currentWeapon1H);

                    if (set_dual) _setDualWeaponClips();
                    else _setWeaponClips(meleeItem);
                }

            }
            else if (m_CurrentWeaponMode == WeaponMode.Weapon1HShield)
            {
                if (!m_EquipmentScript.currentWeapon1H)
                {
                    if (m_EquipmentScript.currentShield)
                    {
                        if (m_EquipmentScript.shieldInHand)
                        {
                            m_SetSecondaryAsPrimary = true;
                            _startTakingWeapon1H(AnimClipSheet.WeaponShield);
                        }
                        else
                        {
                            _setWeapon1HShieldClips(meleeItem, m_EquipmentScript.currentShield);
                        }
                    }
                    else
                    {
                        _setWeaponClips(meleeItem);
                    }
                }
            }

            if (prevObj)
            {
                Vector3 dropPos = transform.position + transform.right;
                prevObj.dropItem(dropPos, null);
                prevObj.gameObject.SetActive(true);
            }
            return prevObj;
        }

        /// <summary>
        /// start drawing or sheathing 将（刀、剑等）插入鞘 current weapon set
        /// </summary>
        public virtual void toggleCurrentWeapon()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_Player.disableInput) return;

            if (m_EquipmentScript.weaponInHand)
            {
                sheatheCurrentWeapon(); 
            }
            else
            {
                takeCurrentWeapon();
            }
        }

        /// <summary>
        /// start taking current weapon set
        /// </summary>
        public virtual void takeCurrentWeapon()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_Player.disableInput) return;

            switch (m_CurrentWeaponMode)
            {
                case WeaponMode.Weapon1HShield:
                    takeWeaponShield();
                    break;
                case WeaponMode.Weapon2H:
                    takeWeapon2H();
                    break;
                case WeaponMode.Bow:
                    takeBow();
                    break;
                case WeaponMode.DualWield:
                    takeDualWeapons();
                    break;
            }
        }

        /// <summary>
        /// start sheathing current weapon set
        /// </summary>
        public virtual void sheatheCurrentWeapon()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_Player.disableInput) return;

            switch (m_CurrentWeaponMode)
            {
                case WeaponMode.Weapon1HShield:
                    sheatheWeaponShield();
                    break;
                case WeaponMode.Weapon2H:
                    sheatheWeapon2H();
                    break;
                case WeaponMode.Bow:
                    sheatheBow();
                    break;
                case WeaponMode.DualWield:
                    sheatheDualWeapons();
                    break;
            }
        }

        /// <summary>
        /// start taking weapon and/or shield
        /// </summary>
        public virtual void takeWeaponShield()
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

            if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentShield)
            {
                if (!m_EquipmentScript.weaponInHand1H && !m_EquipmentScript.shieldInHand)
                {
                    _startTakingWeapon1HShield();
                }
                else if (!m_EquipmentScript.shieldInHand)
                {
                    _startTakingShield(AnimClipSheet.WeaponShield);
                }
                else
                {
                    _startTakingWeapon1H();
                }
            }
            else if (m_EquipmentScript.currentSecondary && m_EquipmentScript.currentShield)
            {
                if (m_EquipmentScript.shieldInHand)
                {
                    m_SetSecondaryAsPrimary = true;
                    _startTakingWeapon1H();
                }
                else
                {
                    m_SetSecondaryAsPrimary = true;
                    _startTakingSecondaryAndShield();
                }
            }
            else if (m_EquipmentScript.currentWeapon1H)
            {
                _startTakingWeapon1H();
            }
            else if (m_EquipmentScript.currentShield)
            {
                _startTakingShield(AnimClipSheet.Shield);
            }
            else if (m_EquipmentScript.currentSecondary)
            {
                m_SetSecondaryAsPrimary = true;
                _startTakingWeapon1H();
            }
        }

        /// <summary>
        /// start sheathing weapon and/or shield
        /// </summary>
        public virtual void sheatheWeaponShield()
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

            if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentShield)
            {
                if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.shieldInHand)
                {
                    _startSheathingWeaponShield();
                }
                else if (m_EquipmentScript.shieldInHand)
                {
                    _startSheathingShield();
                }
                else
                {
                    _startSheathingWeapon1H();
                }
            }
            else if (m_EquipmentScript.currentWeapon1H)
            {
                _startSheathingWeapon1H();
            }
            else if (m_EquipmentScript.currentShield)
            {
                _startSheathingShield();
            }
        }

        /// <summary>
        /// start taking dual wield weapons
        /// </summary>
        public virtual void takeDualWeapons()
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

            if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentSecondary)
            {
                if (!m_EquipmentScript.weaponInHand1H && !m_EquipmentScript.secondaryWeaponInHand)
                {
                    _startTakingDualWeapons();
                }
                else if (!m_EquipmentScript.secondaryWeaponInHand)
                {
                    _startTakingSecondaryWeapon1H();
                }
                else
                {
                    _startTakingWeapon1H();
                }
            }
            else if (m_EquipmentScript.currentSecondary)
            {
                m_SetSecondaryAsPrimary = true;
                _startTakingWeapon1H();
            }
            else if (m_EquipmentScript.currentWeapon1H)
            {
                _startTakingWeapon1H();
            }

        }

        /// <summary>
        /// start sheathing dual wield weapons
        /// </summary>
        public virtual void sheatheDualWeapons()
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

            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.secondaryWeaponInHand)
            {
                _startSheathingDualWeapons();
            }
            else if (m_EquipmentScript.weaponInHand1H)
            {
                _startSheathingWeapon1H();
            }
            else if (m_EquipmentScript.secondaryWeaponInHand)
            {
                _startSheathingSecondaryWeapon1H();
            }

        }

        /// <summary>
        /// start taking weapon two handed
        /// </summary>
        public virtual void takeWeapon2H()
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

            _startTakingWeapon2H();
        }

        /// <summary>
        /// start sheathing weapon two handed
        /// </summary>
        public virtual void sheatheWeapon2H()
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

            _startSheathingWeapon2H();
        }

        /// <summary>
        /// start taking bow
        /// </summary>
        public virtual void takeBow()
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
                //if (m_ThirdPersonPlayer.m_Camera is OrbitCameraController)
                //{
                //    OrbitCameraController oCam = m_ThirdPersonPlayer.m_Camera as OrbitCameraController;
                //    def_Xconstraint = oCam.Xconstraint;
                //    oCam.minXAngle = -30;
                //    oCam.maxXAngle = 35;
                //}
            }
        }

        /// <summary>
        /// start sheathing bow
        /// </summary>
        public virtual void sheatheBow()
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
            //if (m_ThirdPersonPlayer.m_Camera is OrbitCameraController)
            //{
            //    OrbitCameraController oCam = m_ThirdPersonPlayer.m_Camera as OrbitCameraController;
            //    oCam.Xconstraint = def_Xconstraint;
            //    oCam.minXAngle = def_cameraMinXangle;
            //    oCam.maxXAngle = def_cameraMaxXangle;
            //}
        }

        /// <summary>
        /// revive character if dead
        /// </summary>
        /// <param name="health">health</param>
        public virtual void revive(int health)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized <" + this.ToString() + ">");
                return;
            }
#endif
            if (!isDead) return;
            m_Stats.increaseHealth(m_Stats.maxHealth);
            m_Player.ragdoll.blendToMecanim();
        }

        /// <summary>
        /// start ragdoll
        /// </summary>
        public void startRagdoll()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            m_Animator.SetBool(/*"pDiveRoll"*/HashIDs.DiveRollBool, false);
            triggers.breakTrigger();
            _attack_combo_end();
            m_Player.ragdoll.startRagdoll();
        }

        /// <summary>
        /// return to mecanim from ragdoll state
        /// </summary>
        public void returnToMecanim()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_Stats.currentHealth > 0)
                m_Player.ragdoll.blendToMecanim();
        }

        /// <summary>
        /// checks if game object is equipped
        /// </summary>
        /// <param name="obj">game object for query</param>
        /// <returns>returns true if object is equipped</returns>
        public bool isEqupped(GameObject obj)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return false;
            }
#endif
            return m_EquipmentScript.isEqupped(obj);
        }

        /// <summary>
        /// drop physics item
        /// </summary>
        /// <param name="item">item to drop</param>
        /// <param name="drop">drop item or reset</param>
        public void dropItem(PhysicsInventoryItem item, bool drop = true)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            m_EquipmentScript.unsetItem(item);
            if (item)
            {
                // you can reset or drop item
                if (drop)
                {
                    Vector3 dropPos = transform.position + (Vector3.up * 2f) + transform.right;
                    Quaternion dropRot = item.transform.rotation;
                    item.dropItem(dropPos, dropRot);
                }
                else item.resetItem();
            }
        }

        /// <summary>
        /// drop all not wielding equipment. you can drop next to the player or reset them 
        /// on initial places and states
        /// </summary>
        /// <param name="drop">drop or reset</param>
        public void dropAllEquipment(bool drop = true)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            for (int i = 0; i < m_EquipmentScript.items.Length; i++)
            {
                if (m_EquipmentScript.items[i].item != null)
                {
                    if (!m_EquipmentScript.items[i].wielded)
                    {
                        PhysicsInventoryItem pii = m_EquipmentScript.items[i].item as PhysicsInventoryItem;
                        if (pii)
                        {
                            dropItem(pii);
                        }
                    }
                }
            }
            currentPrimaryWeapon = null;
            currentSecondaryWeapon = null;
        }

        /// <summary>
        /// switch weapon mode
        /// </summary>
        /// <param name="mode"></param>
        public void switchWeaponMode(WeaponMode mode)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_Player.disableInput) return;

            switch (mode)
            {
                case WeaponMode.Weapon1HShield:
                    if (!m_EquipmentScript.currentShield && !m_EquipmentScript.currentWeapon1H)
                        return;
                    break;
                case WeaponMode.Weapon2H:
                    if (!m_EquipmentScript.currentWeapon2H)
                        return;
                    break;
                case WeaponMode.Bow:
                    if (!m_EquipmentScript.currentBow || !m_EquipmentScript.currentQuiver)
                        return;
                    break;
                case WeaponMode.DualWield:
                    if (!m_EquipmentScript.currentWeapon1H && !m_EquipmentScript.currentSecondary)
                        return;
                    break;
            }
            _switchToWeaponMode(mode);
        }

#region Switching Weapon Modes 

        //---switching to weapon shield system
        protected void _switchToWSfromW2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.weaponInHand2H)
            {
                OnSheatheWeapon2H = () =>
                {
                    if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentShield)
                    {
                        _startTakingWeapon1HShield();
                    }
                    else if (m_EquipmentScript.currentWeapon1H)
                    {
                        _startTakingWeapon1H();
                    }
                    else if (m_EquipmentScript.currentShield)
                    {
                        _startTakingShield(AnimClipSheet.Shield);
                    }
                    OnSheatheWeapon2H = null;
                };
                _startSheathingWeapon2H();

            }
            else
            {
                if (m_EquipmentScript.currentShield && m_EquipmentScript.currentWeapon1H)
                {
                    _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H, m_EquipmentScript.currentShield);
                }
                else
                {
                    if (m_EquipmentScript.currentWeapon1H)
                        _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                    else if (m_EquipmentScript.currentShield)
                        _setShieldClips(m_EquipmentScript.currentShield);
                }
            }
        }
        protected void _switchToWSfromBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.bowInHand)
            {
                OnPutawayBow = () =>
                {

                    if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentShield)
                    {
                        _startTakingWeapon1HShield();
                    }
                    else if (m_EquipmentScript.currentWeapon1H)
                    {
                        _startTakingWeapon1H();
                    }
                    else if (m_EquipmentScript.currentShield)
                    {
                        _startTakingShield(AnimClipSheet.Shield);
                    }

                    OnPutawayBow = null;
                };
                _startSheathingBow();

            }
            else
            {
                if (m_EquipmentScript.currentShield && m_EquipmentScript.currentWeapon1H)
                {
                    _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H, m_EquipmentScript.currentShield);
                }
                else
                {
                    if (m_EquipmentScript.currentWeapon1H)
                        _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                    else if (m_EquipmentScript.currentShield)
                        _setShieldClips(m_EquipmentScript.currentShield);
                }
            }
            ArrowPool.RemoveArrow(m_CurrentArrowInHand);
            m_CurrentArrowInHand = null;
        }
        protected void _switchToWSfromDW()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.secondaryWeaponInHand)
            {
                if (m_EquipmentScript.currentShield)
                {
                    OnSheatheSecondaryWeapon1H = () =>
                    {
                        _startTakingShield(AnimClipSheet.WeaponShield);
                        OnSheatheSecondaryWeapon1H = null;
                    };
                    _startSheathingSecondaryWeapon1H();
                }
                else
                {
                    OnSheatheSecondaryWeapon1H = () =>
                    {
                        VoidFunc func = () =>
                        {
                            _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                            func = null;
                        };
                        _setTimedFunc(0.1f, func);
                        OnSheatheSecondaryWeapon1H = null;
                    };
                    _startSheathingSecondaryWeapon1H();
                }
            }
            else if (m_EquipmentScript.secondaryWeaponInHand)
            {
                Debug.Log("ERROR: Cannot get to have secondary weapon in hand only!");
            }
            else if (m_EquipmentScript.weaponInHand1H)
            {
                if (m_EquipmentScript.currentShield)
                {
                    _startTakingShield(AnimClipSheet.WeaponShield);
                }
                else
                {
                    _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                }
            }
            else
            {
                if (m_EquipmentScript.currentShield && m_EquipmentScript.currentWeapon1H)
                {
                    _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H, m_EquipmentScript.currentShield);
                }
                else
                {
                    if (m_EquipmentScript.currentWeapon1H)
                        _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                    else if (m_EquipmentScript.currentShield)
                        _setShieldClips(m_EquipmentScript.currentShield);
                }
            }
        }

        //--- switching to weapon two handed system
        protected void _switchToW2HfromWS()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.shieldInHand)
            {

                OnSheatheWeapon1H = () =>
                {
                    _startTakingWeapon2H();
                    OnSheatheWeapon1H = null;
                };
                OnPutawayShield = () =>
                {
                    _startTakingWeapon2H();
                    OnPutawayShield = null;
                };
                _startSheathingWeaponShield();

            }
            else if (m_EquipmentScript.weaponInHand1H)
            {

                OnSheatheWeapon1H = () =>
                {
                    _startTakingWeapon2H();
                    OnSheatheWeapon1H = null;
                };
                _startSheathingWeapon1H();
            }
            else if (m_EquipmentScript.shieldInHand)
            {

                OnPutawayShield = () =>
                {
                    _startTakingWeapon2H();
                    OnPutawayShield = null;
                };
                _startSheathingShield();
            }
            else
            {
                _setWeaponClips(m_EquipmentScript.currentWeapon2H);
            }
        }
        protected void _switchToW2HfromBOW()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.bowInHand)
            {
                OnPutawayBow = () =>
                {
                    _startTakingWeapon2H();
                    OnPutawayBow = null;
                };
                _startSheathingBow();

            }
            else
            {
                _setWeaponClips(m_EquipmentScript.currentWeapon2H);
            }
            ArrowPool.RemoveArrow(m_CurrentArrowInHand);
            m_CurrentArrowInHand = null;
        }
        protected void _switchToW2HfromDW()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.secondaryWeaponInHand)
            {
                OnSheatheWeapon1H = () =>
                {
                    _startTakingWeapon2H();
                    OnSheatheWeapon1H = null;
                };
                _startSheathingDualWeapons();

            }
            else if (m_EquipmentScript.secondaryWeaponInHand)
            {
                Debug.Log("Current mode : DualWield - secondary switch");
            }
            else if (m_EquipmentScript.weaponInHand1H)
            {
                OnSheatheWeapon1H = () =>
                {
                    _startTakingWeapon2H();
                    OnSheatheWeapon1H = null;
                };
                _startSheathingWeapon1H();
            }
            else
            {
                _setWeaponClips(m_EquipmentScript.currentWeapon2H);
            }
        }

        //--- switching to bow system
        protected void _switchToBOWfromWS()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.shieldInHand)
            {
                OnSheatheWeapon1H = () =>
                {
                    _startTakingBow();
                    OnSheatheWeapon1H = null;
                };
                OnPutawayShield = () =>
                {
                    _startTakingBow();
                    OnPutawayShield = null;
                };
                _startSheathingWeaponShield();

            }
            else if (m_EquipmentScript.weaponInHand1H)
            {
                OnSheatheWeapon1H = () =>
                {
                    _startTakingBow();
                    OnSheatheWeapon1H = null;
                };
                _startSheathingWeapon1H();
            }
            else if (m_EquipmentScript.shieldInHand)
            {
                OnPutawayShield = () =>
                {
                    _startTakingBow();
                    OnPutawayShield = null;
                };
                _startSheathingShield();
            }
        }
        protected void _switchToBOWfromW2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand2H)
            {
                OnSheatheWeapon2H = () =>
                {
                    _startTakingBow();
                    OnSheatheWeapon2H = null;
                };
                _startSheathingWeapon2H();
            }
        }
        protected void _switchToBOWfromDW()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.secondaryWeaponInHand)
            {
                OnSheatheWeapon1H = () =>
                {
                    _startTakingBow();
                    OnSheatheWeapon1H = null;
                };
                _startSheathingDualWeapons();

            }
            else if (m_EquipmentScript.secondaryWeaponInHand)
            {
                Debug.Log("Current mode : DualWield - secondary switch");
            }
            else if (m_EquipmentScript.weaponInHand1H)
            {
                OnSheatheWeapon1H = () =>
                {
                    _startTakingBow();
                    OnSheatheWeapon1H = null;
                };
                _startSheathingWeapon1H();
            }
        }

        //--- switching to dual weapons system
        protected void _switchToDWfromWS()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand1H && m_EquipmentScript.shieldInHand)
            {
                OnPutawayShield = () =>
                {
                    if (m_EquipmentScript.currentSecondary)
                    {
                        _startTakingSecondaryWeapon1H();
                    }
                    else
                    {
                        VoidFunc func = () =>
                        {
                            _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                            func = null;
                        };
                        _setTimedFunc(0.1f, func);
                    }
                    OnPutawayShield = null;
                };
                _startSheathingShield();
            }
            else if (m_EquipmentScript.shieldInHand)
            {
                OnPutawayShield = () =>
                {
                    if (m_EquipmentScript.currentSecondary)
                    {
                        m_SetSecondaryAsPrimary = true;
                        _startTakingWeapon1H();
                    }
                    OnPutawayShield = null;
                };
                _startSheathingShield();
            }
            else if (m_EquipmentScript.weaponInHand1H)
            {
                if (m_EquipmentScript.currentSecondary)
                {
                    _startTakingSecondaryWeapon1H();
                }
                else
                {
                    _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                }
            }
            else
            {
                if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentSecondary)
                {
                    _setDualWeaponClips();

                }
                else if (m_EquipmentScript.currentWeapon1H)
                {
                    _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                }
                else if (m_EquipmentScript.currentSecondary)
                {
                    _setWeaponClips(m_EquipmentScript.currentSecondary);
                }
            }
        }
        protected void _switchToDWfromW2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.weaponInHand2H)
            {
                OnSheatheWeapon2H = () =>
                {

                    if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentSecondary)
                    {
                        _startTakingDualWeapons();
                    }
                    else if (m_EquipmentScript.currentSecondary)
                    {
                        m_SetSecondaryAsPrimary = true;
                        _startTakingWeapon1H();
                    }
                    else if (m_EquipmentScript.currentWeapon1H)
                    {
                        _startTakingWeapon1H();
                    }
                    OnSheatheWeapon2H = null;
                };
                _startSheathingWeapon2H();
            }
            else
            {
                if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentSecondary)
                    _setDualWeaponClips();
                else if (m_EquipmentScript.currentWeapon1H)
                    _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                else if (m_EquipmentScript.currentSecondary) _setWeaponClips(m_EquipmentScript.currentSecondary);
            }
        }
        protected void _switchToDWfromBOW()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.bowInHand)
            {
                OnPutawayBow = () =>
                {

                    if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentSecondary)
                    {
                        _startTakingDualWeapons();
                    }
                    else if (m_EquipmentScript.currentSecondary)
                    {
                        m_SetSecondaryAsPrimary = true;
                        _startTakingWeapon1H();
                    }
                    else if (m_EquipmentScript.currentWeapon1H)
                    {
                        _startTakingWeapon1H();
                    }
                    OnPutawayBow = null;
                };
                _startSheathingBow();
            }
            else
            {
                if (m_EquipmentScript.currentWeapon1H && m_EquipmentScript.currentSecondary)
                    _setDualWeaponClips();
                else if (m_EquipmentScript.currentWeapon1H)
                    _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                else if (m_EquipmentScript.currentSecondary) _setWeaponClips(m_EquipmentScript.currentSecondary);
            }
            ArrowPool.RemoveArrow(m_CurrentArrowInHand);
            m_CurrentArrowInHand = null;
        }

#endregion



        /// <summary>
        ///  set timed function and start timer
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="func"></param>
        protected void _setTimedFunc(float _time, VoidFunc func)
        {
            TimedFunc tf = new TimedFunc();
            tf.timer = 0.0f;
            tf.time = _time;
            tf.func = func;
            m_TimedFuncs.Add(tf);
        }

        /// <summary>
        /// start hit reaction animation
        /// </summary>
        /// <param name="hitType"></param>
        /// <param name="applyHitReaction"></param>
        /// <param name="attackSource"></param>
        protected void _startHitReaction(int hitType, bool applyHitReaction, int attackSource)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (hitType == -1) return;

            if (!blocking)
            {
                if (applyHitReaction)
                {
                    string stateName = "TakeHit" + hitType;

                    int stateID = Animator.StringToHash(stateName);
                    _break_transition();
                    if (m_Animator.HasState(0, stateID))
                    {

                        m_Animator.CrossFade(stateID, 0.0f, 0, 0.0f);
                    }
                    else
                    {
#if DEBUG_INFO
                        Debug.LogWarning("state '" + stateName + "' dont exist");
#endif
                        m_Animator.CrossFade("TakeHit0", 0.0f, 0, 0.0f);
                    }
                }
            }
            else
            {
                _playBlockSound(attackSource);
                if (applyHitReaction)
                {
                    bool unarmed = m_Animator.GetBool(/*"pUnarmed"*/HashIDs.UnarmedBool);
                    string stateName = "UnarmedBlockHit" + hitType;
                    if (!unarmed)
                    {
                        stateName = "BlockHit" + hitType;
                    }
                    int stateID = Animator.StringToHash(stateName);
                    _break_transition();
                    if (m_Animator.HasState(0, stateID))
                    {
                        m_Animator.CrossFade(stateID, 0.0f, 0, 0.0f);
                    }
                    else
                    {
#if DEBUG_INFO
                        Debug.LogWarning("state '" + stateName + "' dont exist");
#endif
                        m_Animator.CrossFade("UnarmedBlockHit0", 0.0f, 0, 0.0f);
                    }
                }
            }
        }

        /// <summary>
        /// play attack swing sound based on current weapon
        /// </summary>
        /// <param name="attackSource"></param>
        protected void _playAttackSwingSound(int attackSource)
        {
            if (attackSource == 0)
            {
                if (currentPrimaryWeapon)
                {
                    GameUtils.PlayRandomClipAtPosition(currentPrimaryWeapon.weaponAudio.attackSwingSounds, transform.position);
                }
                else if (defaultWeaponSounds)
                {
                    GameUtils.PlayRandomClipAtPosition(defaultWeaponSounds.attackSwingSounds, transform.position);
                }
                else
                {
#if DEBUG_INFO
                    Debug.LogWarning("Cannot find 'MeleeSounds' on " + this.gameObject.name + " or weapon components");
#endif
                }

            }
            if (attackSource == 1)
            {
                if (currentSecondaryWeapon)
                {
                    GameUtils.PlayRandomClipAtPosition(currentSecondaryWeapon.weaponAudio.attackSwingSounds, transform.position);
                }
                else if (defaultWeaponSounds)
                {
                    GameUtils.PlayRandomClipAtPosition(defaultWeaponSounds.attackSwingSounds, transform.position);
                }
                else
                {
#if DEBUG_INFO
                    Debug.LogWarning("Cannot find 'MeleeSounds' on " + this.gameObject.name + " or weapon components");
#endif
                }
            }
        }

        /// <summary>
        /// play attack hit sound based on current weapon
        /// </summary>
        /// <param name="attackSource"></param>
        protected void _playAttackHitSound(int attackSource)
        {
            if (attackSource == 0)
            {
                if (currentPrimaryWeapon)
                {
                    GameUtils.PlayRandomClipAtPosition(currentPrimaryWeapon.weaponAudio.attackHitSounds, transform.position);
                }
                else if (defaultWeaponSounds)
                {
                    GameUtils.PlayRandomClipAtPosition(defaultWeaponSounds.attackHitSounds, transform.position);
                }
                else
                {
#if DEBUG_INFO
                    Debug.LogWarning("Cannot find 'MeleeSounds' on " + this.gameObject.name + " or weapon components");
#endif
                }

            }
            if (attackSource == 1)
            {
                if (currentSecondaryWeapon)
                {
                    GameUtils.PlayRandomClipAtPosition(currentSecondaryWeapon.weaponAudio.attackHitSounds, transform.position);
                }
                else if (defaultWeaponSounds)
                {
                    GameUtils.PlayRandomClipAtPosition(defaultWeaponSounds.attackHitSounds, transform.position);
                }
                else
                {
#if DEBUG_INFO
                    Debug.LogWarning("Cannot find 'MeleeSounds' on " + this.gameObject.name + " or weapon components");
#endif
                }
            }
        }

        /// <summary>
        /// play block sound based on current blocking item
        /// </summary>
        /// <param name="attackSource"></param>
        protected void _playBlockSound(int attackSource)
        {

            if (currentBlockSounds)
                GameUtils.PlayRandomClipAtPosition(currentBlockSounds.blockSounds, transform.position);
#if DEBUG_INFO
            else Debug.LogWarning("Cannot find current block sounds");
#endif
        }

        /// <summary>
        /// get angle difference from 360
        /// </summary>
        /// <param name="curAngle"></param>
        /// <returns></returns>
        protected float _distFromZeroAngle360(float curAngle)
        {
            if (curAngle <= 180)
            {
                return curAngle;
            }
            else
            {
                return 360 - curAngle;
            }
        }

        /// <summary>
        /// break drawing/sheating items
        /// </summary>
        protected void _break_transition()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized <" + this.ToString() + ">");
                return;
            }
#endif
            if (!m_SwitchingItemState) return;

            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);
            m_Animator.SetBool(HashIDs.TakeWeapon2HBool, false);
            m_Animator.SetBool(HashIDs.SheatheWeapon2HBool, false);
            m_Animator.SetBool(HashIDs.TakeBowBool, false);
            m_LeftArmOccupation = ArmOccupation.None;
            m_RightArmOccupation = ArmOccupation.None;

            switch (m_CurrentWeaponMode)
            {
                case WeaponMode.Weapon1HShield:
                    {
                        if (m_EquipmentScript.shieldInHand && !m_EquipmentScript.weaponInHand1H)
                        {
                            m_Animator.SetBool(HashIDs.UnarmedBool, false);
                            AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                            if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                                m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                            currentBlockSounds = m_EquipmentScript.currentShield.weaponAudio;
                            _setShieldClips(m_EquipmentScript.currentShield);
                        }
                        if (!m_EquipmentScript.shieldInHand && m_EquipmentScript.weaponInHand1H)
                        {
                            m_Animator.SetBool(HashIDs.UnarmedBool, false);
                            AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                            if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                                m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                            currentBlockSounds = m_EquipmentScript.currentWeapon1H.weaponAudio;
                            _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                        }
                    }
                    break;
                case WeaponMode.DualWield:
                    {
                        if (m_EquipmentScript.weaponInHand1H && !m_EquipmentScript.secondaryWeaponInHand)
                        {
                            m_Animator.SetBool(HashIDs.UnarmedBool, false);
                            AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                            if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                                m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                            currentBlockSounds = m_EquipmentScript.currentWeapon1H.weaponAudio;
                            _setWeaponClips(m_EquipmentScript.currentWeapon1H);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// check if character is falling from too high height
        /// </summary>
        protected void _fallToDeathCheck()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (!m_Animator.isInitialized) return;

            float pJump = m_Animator.GetFloat("pJump");
            if (pJump < 0)
            {
                if (m_Character.distanceFromGround > fallDieHeight)
                {
                    m_Character.OnLand = () =>
                    {
                        m_Stats.decreaseHealth(m_Stats.maxHealth);
                        startRagdoll();
                        m_Character.OnLand = null;
                    };
                }
            }
        }

        /// <summary>
        /// cancel draw
        /// </summary>
        protected void _cancelDraw()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            m_Animator.SetBool(/*"pDrawBow"*/HashIDs.DrawBowBool, false);
            m_BowDrawn = false;
            m_ReleasingBowString = false;
            m_EquipmentScript.currentBow.resetString();
            m_AudioMan.stopAudio();
        }

        /// <summary>
        /// combo end setup
        /// </summary>
        protected void _attack_combo_end()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_PrimaryTargetNPC != null) m_PrimaryTargetNPC.attack_end_notify(this, m_CurrentAttackType);
            foreach (IGameCharacter npcInRange in m_NpcsInRange)
            {
                npcInRange.attack_end_notify(this, m_CurrentAttackType);
            }
            m_NpcsInRange.Clear();

            m_InCombat = false;
            m_Animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
            m_BreakCombo = true;
            m_Attack_combo_started = false;
            m_PrimaryTargetNPC = null;
        }

        /// <summary>
        /// switch to new weapon mode
        /// </summary>
        /// <param name="newMode"></param>
        protected void _switchToWeaponMode(WeaponMode newMode)
        {
            if (m_SwitchingItemState) return;

            // put away current weapon and set event to trigger take new weapon 

            if (newMode == m_CurrentWeaponMode) return;

            WeaponMode oldMode = m_CurrentWeaponMode;
            m_CurrentWeaponMode = newMode;

            switch (newMode)
            {
                case WeaponMode.Weapon1HShield:
                    if (oldMode == WeaponMode.Weapon2H)
                    {
                        _switchToWSfromW2H();
                    }
                    else if (oldMode == WeaponMode.Bow)
                    {
                        _switchToWSfromBow();
                    }
                    else if (oldMode == WeaponMode.DualWield)
                    {
                        _switchToWSfromDW();
                    }
                    break;
                case WeaponMode.Weapon2H:
                    if (oldMode == WeaponMode.Weapon1HShield)
                    {
                        _switchToW2HfromWS();
                    }
                    else if (oldMode == WeaponMode.Bow)
                    {
                        _switchToW2HfromBOW();
                    }
                    else if (oldMode == WeaponMode.DualWield)
                    {
                        _switchToW2HfromDW();
                    }
                    break;
                case WeaponMode.Bow:
                    if (oldMode == WeaponMode.Weapon1HShield)
                    {
                        _switchToBOWfromWS();
                    }
                    else if (oldMode == WeaponMode.Weapon2H)
                    {
                        _switchToBOWfromW2H();
                    }
                    else if (oldMode == WeaponMode.DualWield)
                    {
                        _switchToBOWfromDW();
                    }
                    break;
                case WeaponMode.DualWield:
                    if (oldMode == WeaponMode.Weapon1HShield)
                    {
                        _switchToDWfromWS();
                    }
                    else if (oldMode == WeaponMode.Weapon2H)
                    {
                        _switchToDWfromW2H();
                    }
                    else if (oldMode == WeaponMode.Bow)
                    {
                        _switchToDWfromBOW();
                    }
                    break;
            }
        }

        /// <summary>
        /// check is target is in player view
        /// </summary>
        /// <param name="gameChar"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected bool _targetInView(IGameCharacter gameChar, Vector3 direction)
        {
#if DEBUG_INFO
            if (null == gameChar) { Debug.LogError("object cannot be null" + " < " + this.ToString() + ">"); return false; }
#endif
            Vector3 toTarget = gameChar.position - transform.position;
            float angle = Vector3.Angle(m_TargetDirection, toTarget);
            return angle < attack_angle;
        }



        /// <summary>
        /// rotate and jump towards npc
        /// </summary>
        protected void _jumpToTarget()
        {
#if DEBUG_INFO
            if (!m_Player) { Debug.LogError("object cannot be null." + " < " + this.ToString() + ">"); return; }
#endif
            if (null == m_PrimaryTargetNPC) return;
            Vector3 position = m_PrimaryTargetNPC.position - m_Direction2target * (0.85f * distanceToTarget);
            Quaternion lookTo = Quaternion.LookRotation(m_Direction2target);
            float jumpTime = 0.135f;
            m_Player.character.lerpToTransform(position, lookTo, jumpTime, jumpTime);
        }

        /// <summary>
        /// get new target from all npcs on the scene
        /// </summary>
        /// <returns></returns>
        protected NPCScript _getPrimaryTarget()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return null;
            }
#endif
            NPCScript cur = null;


            /// THROW SPHERECAST FIRST
            Vector3 pos = attackSweepBody.position;
            Vector3 dir = m_TargetDirection;
            Ray ray = new Ray(pos, dir);
            float radius = m_Character.capsule.radius;
            ray.origin = pos;
            RaycastHit rhit;
            int mask = LayerMask.GetMask("NPCLayer");

            float jumpDist = attack_jump_distance;
            if (!enableJumpToTarget)
                jumpDist = m_Stats.currentWeaponReach;

            RaycastHit[] allHits = Physics.SphereCastAll(ray, radius, jumpDist, mask);
            float nearestDist = float.MaxValue;
            for (int i = 0; i < allHits.Length; i++)
            {
                rhit = allHits[i];
                NPCScript npc = rhit.collider.GetComponent<NPCScript>();
                if (npc)
                {
                    if (npc.ragdollState != RagdollManager.RagdollState.Animated) continue;
                    Vector3 toTarget = npc.position - transform.position;


                    bool isObstructed = _sweepTest(toTarget);
                    if (isObstructed) continue;

                    float distSqr = toTarget.sqrMagnitude;
                    if (distSqr < nearestDist)
                    {
                        cur = npc;
                        nearestDist = distSqr;
                    }
                }
            }



            if (!cur)
            {
                // getting nearest by angle
                float nearestAngle = float.MaxValue;
                for (int i = 0; i < m_NpcMan.npcs.Length; i++)
                {
                    NPCScript ts = m_NpcMan.npcs[i];
#if DEBUG_INFO
                    if (!ts.gameObject.activeSelf)
                    {
                        Debug.LogWarning("object not active: " + ts.name);
                        continue;
                    }
#endif
                    if (ts.ragdollState != RagdollManager.RagdollState.Animated) continue;

                    Vector3 toTarget = ts.position - transform.position;

                    float distance = toTarget.magnitude;
                    if (jumpDist < distance) continue;

                    float angle = Vector3.Angle(m_TargetDirection, toTarget);
                    if (!(angle < attack_angle)) continue;

                    bool isObstructed = _sweepTest(toTarget);
                    if (isObstructed) continue;

                    if (angle < nearestAngle)
                    {
                        cur = ts;
                        nearestAngle = angle;
                    }
                }
            }

            if (cur)
            {
                m_Direction2target = cur.position - transform.position;
                m_Direction2target.y = 0.0f;
                m_Direction2target.Normalize();
            }

            return cur;
        }

        /// <summary>
        /// get new target from npcs in the current zone
        /// </summary>
        /// <returns></returns>
        protected NPCScript _getPrimaryTargetInZone()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return null;
            }
#endif

            NPCScript cur = null;


            /// THROW SPHERECAST FIRST
            Vector3 pos = attackSweepBody.position;
            Vector3 dir = m_TargetDirection;
            Ray ray = new Ray(pos, dir);
            float radius = m_Character.capsule.radius;
            ray.origin = pos;
            RaycastHit rhit;
            int mask = LayerMask.GetMask("NPCLayer");

            float jumpDist = attack_jump_distance;
            if (!enableJumpToTarget)
                jumpDist = m_Stats.currentWeaponReach;

            RaycastHit[] all = Physics.SphereCastAll(ray, radius, jumpDist, mask);
            float nearestDist = float.MaxValue;
            for (int i = 0; i < all.Length; i++)
            {
                rhit = all[i];
                NPCScript npc = rhit.collider.GetComponent<NPCScript>();
                if (npc)
                {
                    if (npc.ragdollState != RagdollManager.RagdollState.Animated) continue;
                    if (npc.isDead) continue;
                    Vector3 toTarget = npc.position - transform.position;
                    float distSqr = toTarget.sqrMagnitude;

                    bool isObstructed = _sweepTest(toTarget);
                    if (isObstructed) continue;

                    if (distSqr < nearestDist)
                    {
                        cur = npc;
                        nearestDist = distSqr;
                    }
                }
            }


            if (!cur)
            {
                // getting nearest by angle
                float nearestAngle = float.MaxValue;
                foreach (NPCGuardZone zone in m_NpcMan.currentZones)
                {
                    for (int i = 0; i < zone.npc_list.Count; i++)
                    {
                        NPCScript ts = zone.npc_list[i];
#if DEBUG_INFO
                        if (!ts.gameObject.activeSelf)
                        {
                            Debug.LogWarning("object not active: " + ts.name);
                            continue;
                        }
#endif
                        if (ts.ragdollState != RagdollManager.RagdollState.Animated) continue;
                        if (ts.isDead) continue;

                        Vector3 toTarget = ts.position - transform.position;

                        float distance = toTarget.magnitude;
                        if (jumpDist < distance) continue;

                        float angle = Vector3.Angle(m_TargetDirection, toTarget);
                        if (!(angle < attack_angle)) continue;

                        bool isObstructed = _sweepTest(toTarget);
                        if (isObstructed) continue;

                        if (angle < nearestAngle)
                        {
                            cur = ts;
                            nearestAngle = angle;
                        }
                    }
                }


            }

            if (cur)
            {
                m_Direction2target = cur.position - transform.position;
                m_Direction2target.y = 0.0f;
                m_Direction2target.Normalize();
            }
            return cur;
        }

        /// <summary>
        /// check is attack path is clear
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected bool _sweepTest(Vector3 direction)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return false;
            }
#endif
            bool result = false;
            RaycastHit rhit;
            float length = direction.magnitude;
            if (attackSweepBody.SweepTest(direction.normalized, out rhit, length + 0.1f, QueryTriggerInteraction.Ignore))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// set animation clip group
        /// </summary>
        protected void _setClipSheet()
        {
#if DEBUG_INFO
            if (!m_EquipmentScript)
            {
                Debug.LogError("object cannot be null." + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_CurrentClipSheet == AnimClipSheet.None) return;

            switch (m_CurrentClipSheet)
            {
                case AnimClipSheet.Weapon1H: _setWeaponClips(m_EquipmentScript.currentWeapon1H); break;
                case AnimClipSheet.Weapon2H: _setWeaponClips(m_EquipmentScript.currentWeapon2H); break;
                case AnimClipSheet.Shield: _setShieldClips(m_EquipmentScript.currentShield); break;
                case AnimClipSheet.WeaponShield:
                    _setWeapon1HShieldClips(m_EquipmentScript.currentWeapon1H,
                                                m_EquipmentScript.currentShield); break;
                case AnimClipSheet.DualWeapons: _setDualWeaponClips(); break;
            }
            m_CurrentClipSheet = AnimClipSheet.None;
        }


#region Dual Systems

        /// <summary>
        /// start taking weapon and shield
        /// </summary>
        protected void _startTakingWeapon1HShield()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (!m_EquipmentScript.currentWeapon1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.currentShield)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for shield. Shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_EquipmentScript.weaponInHand1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot take weapon. Weapon already not in hand < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_EquipmentScript.shieldInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot take shield. Shield already in hand < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon shield. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon shield. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }

            m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;
            m_LeftArmOccupation = ArmOccupation.ToReachShield;

            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, true);
            m_Animator.SetBool(HashIDs.TakeShieldBool, true);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            m_CurrentClipSheet = AnimClipSheet.WeaponShield;

            if (m_EquipmentScript.currentWeapon1H.OnStartTaking != null)
                m_EquipmentScript.currentWeapon1H.OnStartTaking();
            if (m_EquipmentScript.currentShield.OnStartTaking != null)
                m_EquipmentScript.currentShield.OnStartTaking();
        }

        /// <summary>
        /// start removing weapon and shield
        /// </summary>
        protected void _startSheathingWeaponShield()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (!m_EquipmentScript.currentWeapon1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.currentShield)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for shield. Shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            if (!m_EquipmentScript.weaponInHand1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot sheathe weapon. Weapon already not in hand < " + this.ToString() + " >");
#endif
                return;
            }

            if (!m_EquipmentScript.shieldInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot sheathe shield. Shield already in hand < " + this.ToString() + " >");
#endif
                return;
            }


            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon shield. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon shield. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;
            m_LeftArmOccupation = ArmOccupation.ToReachShield;

            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, true);
            m_Animator.SetBool(HashIDs.TakeShieldBool, true);
            m_Animator.SetBool(HashIDs.BlockBool, false);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            if (m_EquipmentScript.currentWeapon1H.OnStartSheathing != null)
                m_EquipmentScript.currentWeapon1H.OnStartSheathing();
            if (m_EquipmentScript.currentShield.OnStartSheathing != null)
                m_EquipmentScript.currentShield.OnStartSheathing();
        }

        /// <summary>
        ///  start taking primary and secondary weapon
        /// </summary>
        protected void _startTakingDualWeapons()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (!m_EquipmentScript.currentWeapon1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.currentSecondary)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Secondary weapon missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.weaponInHand1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon already in hand < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.secondaryWeaponInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for secondary weapon. Secondary eapon already in hand < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for dual weapon. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for dual weapon. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }

            m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;
            m_LeftArmOccupation = ArmOccupation.ToReachSecondary;

            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, true);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, true);
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            m_CurrentClipSheet = AnimClipSheet.DualWeapons;

            if (m_EquipmentScript.currentWeapon1H.OnStartTaking != null)
                m_EquipmentScript.currentWeapon1H.OnStartTaking();
            if (m_EquipmentScript.currentSecondary.OnStartTaking != null)
                m_EquipmentScript.currentSecondary.OnStartTaking();
        }

        /// <summary>
        /// start removing primary and secondary weapons
        /// </summary>
        protected void _startSheathingDualWeapons()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (!m_EquipmentScript.currentWeapon1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.currentSecondary)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Secondary weapon missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.weaponInHand1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon already not in hand < " + this.ToString() + " >");
#endif
                return;
            }

            if (!m_EquipmentScript.secondaryWeaponInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Secondary weapon already not in hand < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for dual weapon. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for dual weapon. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;
            m_LeftArmOccupation = ArmOccupation.ToReachSecondary;


            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, true);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, true);
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            if (m_EquipmentScript.currentWeapon1H.OnStartSheathing != null)
                m_EquipmentScript.currentWeapon1H.OnStartSheathing();
            if (m_EquipmentScript.currentSecondary.OnStartSheathing != null)
                m_EquipmentScript.currentSecondary.OnStartSheathing();
        }

        /// <summary>
        ///  start taking secondary weapon and shield
        /// </summary>
        protected void _startTakingSecondaryAndShield()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentSecondary == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Secondary weapon missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.currentShield == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for shield. Shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_EquipmentScript.secondaryWeaponInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Secondary weapon in hand < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.shieldInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for shield. Shield already in hand < " + this.ToString() + " >");
#endif
                return;
            }


            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for shield. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }

            m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;
            m_LeftArmOccupation = ArmOccupation.ToReachShield;

            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, true);
            m_Animator.SetBool(HashIDs.TakeShieldBool, true);
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);

            m_CurrentClipSheet = AnimClipSheet.WeaponShield;

            if (m_EquipmentScript.currentShield.OnStartTaking != null)
                m_EquipmentScript.currentShield.OnStartTaking();
            if (m_EquipmentScript.currentSecondary.OnStartTaking != null)
                m_EquipmentScript.currentSecondary.OnStartTaking();

        }

#endregion

#region Weapon1H System

        /// <summary>
        /// start reaching for weapon one handed
        /// </summary>
        protected void _startTakingWeapon1H(AnimClipSheet newClipSheet = AnimClipSheet.Weapon1H)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }


            if (m_SetSecondaryAsPrimary)
            {
                if (m_EquipmentScript.currentSecondary == null)
                {
#if ADDITIONAL_DEBUG_INFO
                    Debug.LogWarning("_startTakingWeapon1H() switch secondary to primary failed. Secondary weapon missing ( null ) < " + this.ToString() + " >");
#endif
                    return;
                }
                if (m_EquipmentScript.secondaryWeaponInHand)
                {
#if ADDITIONAL_DEBUG_INFO
                    Debug.LogWarning("_startTakingWeapon1H() switch secondary to primary failed. Secondary weapon in hand < " + this.ToString() + " >");
#endif
                    return;
                }

                m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;

                m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, true);
                m_Animator.SetBool(HashIDs.TakeShieldBool, false);
                m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
                m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);

                if (m_EquipmentScript.currentSecondary.OnStartTaking != null)
                    m_EquipmentScript.currentSecondary.OnStartTaking();
            }
            else
            {
                if (m_EquipmentScript.currentWeapon1H == null)
                {
#if ADDITIONAL_DEBUG_INFO
                    Debug.LogWarning("_startTakingWeapon1H() failed. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                    return;
                }
                if (m_EquipmentScript.weaponInHand1H)
                {
#if ADDITIONAL_DEBUG_INFO
                    Debug.LogWarning("Cannot take weapon1H. Weapon1H already in hand < " + this.ToString() + " >");
#endif
                    return;
                }

                m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;
                m_Animator.SetBool(HashIDs.TakeWeapon1HBool, true);
                m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
                m_Animator.SetBool(HashIDs.TakeShieldBool, false);
                m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

                if (m_EquipmentScript.currentWeapon1H.OnStartTaking != null)
                    m_EquipmentScript.currentWeapon1H.OnStartTaking();
            }
            m_CurrentClipSheet = newClipSheet;

        }

        /// <summary>
        /// start putting away weapon one handed
        /// </summary>
        protected void _startSheathingWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (!m_EquipmentScript.currentWeapon1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_takeWeapon1H() failed. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.weaponInHand1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot sheathe weapon1H. Weapon1H already not in hand < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon shield. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_RightArmOccupation = ArmOccupation.ToReachWeapon1H;

            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, true);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            if (m_EquipmentScript.currentWeapon1H.OnStartSheathing != null)
                m_EquipmentScript.currentWeapon1H.OnStartSheathing();
        }

        /// <summary>
        /// take one handed weapon  (switch bones)
        /// </summary>
        protected void _takeWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_SetSecondaryAsPrimary)
            {
                if (m_EquipmentScript.currentSecondary == null)
                {
#if ADDITIONAL_DEBUG_INFO
                    Debug.LogWarning("_takeWeapon1H() switch secondary to primary failed. Secondary weapon missing ( null ) < " + this.ToString() + " >");
#endif
                    return;
                }
                m_EquipmentScript.switchPrimaryWithSecondary();
            }


            if (m_EquipmentScript.currentWeapon1H == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_takeWeapon1H() failed. Weapon1H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }



            currentPrimaryWeapon = m_EquipmentScript.currentWeapon1H;
            m_Animator.SetBool(HashIDs.UnarmedBool, false);
            if (m_EquipmentScript.shieldInHand)
            {
                currentBlockSounds = m_EquipmentScript.currentShield.weaponAudio;
                currentSecondaryWeapon = m_EquipmentScript.currentShield;
            }
            else currentBlockSounds = m_EquipmentScript.currentWeapon1H.weaponAudio;

            if(m_LeftArmOccupation == ArmOccupation.None ||
                m_LeftArmOccupation == ArmOccupation.FromReachShield ||
                m_LeftArmOccupation == ArmOccupation.FromReachSecondary)
            {
                VoidFunc func = () =>
                {
                    _setClipSheet();
                    AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                    if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                        m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                    func = null;
                };
                _setTimedFunc(0.1f, func);
            }

            m_EquipmentScript.wieldWeapon1H();
            m_Stats.setCurrentAttackValue(
                m_EquipmentScript.currentWeapon1H.damage,
                m_EquipmentScript.currentWeapon1H.range,
                m_Stats.attackSpeed);
            distanceToTarget = m_EquipmentScript.currentWeapon1H.range * 0.75f;
            m_RightArmOccupation = ArmOccupation.FromReachWeapon1H;
        }

        /// <summary>
        /// put away one handed weapon  (switch bones)
        /// </summary>
        protected void _sheatheWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentWeapon1H == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_sheatheWeapon1H() failed. Weapon1H missing ( null ). < " + this.ToString() + " >");
#endif
                return;
            }

            currentPrimaryWeapon = null;
            if (m_EquipmentScript.shieldInHand)
                currentBlockSounds = m_EquipmentScript.currentShield.weaponAudio;
            else currentBlockSounds = defaultWeaponSounds;
            m_Animator.SetBool(HashIDs.UnarmedBool, true);

            m_EquipmentScript.restWeapon1H();
            m_Stats.resetAttackValues();
            distanceToTarget = m_DefaultDistance2Target;

            AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (curState.shortNameHash != HashIDs.UnarmedLocomotionState)
                m_Animator.CrossFadeInFixedTime(HashIDs.UnarmedLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
            m_RightArmOccupation = ArmOccupation.FromReachWeapon1H;
        }

#endregion

#region Shield System

        /// <summary>
        /// start reaching for shield
        /// </summary>
        protected void _startTakingShield(AnimClipSheet newClipSheet)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentShield == null)
            {
#if DEBUG_INFO
                Debug.LogWarning("_startSheathingShield() failed. Current shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_EquipmentScript.shieldInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot sheathe shield. Shield already in hand. < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for shield. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_LeftArmOccupation = ArmOccupation.ToReachShield;

            m_Animator.SetBool(HashIDs.TakeShieldBool, true);
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            m_CurrentClipSheet = newClipSheet;

            if (m_EquipmentScript.currentShield.OnStartTaking != null)
                m_EquipmentScript.currentShield.OnStartTaking();
        }

        /// <summary>
        /// start putting away shield
        /// </summary>
        protected void _startSheathingShield()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentShield == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_startSheathingShield() failed. Current shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.shieldInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot sheathe shield. Shield already not in hand. < " + this.ToString() + " >");
#endif
                return;
            }


            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_LeftArmOccupation = ArmOccupation.ToReachShield;

            m_Animator.SetBool(HashIDs.TakeShieldBool, true);
            m_Animator.SetBool(HashIDs.BlockBool, false);
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            if (m_EquipmentScript.currentShield.OnStartSheathing != null)
                m_EquipmentScript.currentShield.OnStartSheathing();
        }

        /// <summary>
        /// take shield (switch bones)
        /// </summary>
        protected void _takeShield()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentShield == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_takeShield() failed. Current shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }



            if (!m_EquipmentScript.currentWeapon1H)
            {
                currentPrimaryWeapon = m_EquipmentScript.currentShield;
                m_Stats.setCurrentAttackValue(m_EquipmentScript.currentShield.damage, 
                    m_EquipmentScript.currentShield.range, 
                    m_Stats.attackSpeed);
                distanceToTarget = m_EquipmentScript.currentShield.range * 0.75f;
            }
            else 
            {
                currentSecondaryWeapon = m_EquipmentScript.currentShield;
            }
            currentBlockSounds = m_EquipmentScript.currentShield.weaponAudio;
            m_EquipmentScript.wieldShield();
            m_Animator.SetBool(HashIDs.UnarmedBool, false);

            if(m_RightArmOccupation == ArmOccupation.None ||
                m_RightArmOccupation == ArmOccupation.FromReachWeapon1H
                )
            {
                VoidFunc func = () =>
                {
                    _setClipSheet();
                    AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                    if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                        m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                    func = null;
                };
                _setTimedFunc(0.1f, func);
            }
            m_LeftArmOccupation = ArmOccupation.FromReachShield;
        }

        // put away shield (switch bones)
        protected void _sheatheShield()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentShield == null)
            {
#if DEBUG_INFO
                Debug.LogWarning("_sheatheShield() failed. Current shield missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            if (currentPrimaryWeapon == m_EquipmentScript.currentShield)
                currentPrimaryWeapon = null;
            if (currentSecondaryWeapon == m_EquipmentScript.currentShield)
                currentSecondaryWeapon = null;

            m_EquipmentScript.restShield();
            m_Stats.resetAttackValues();
            distanceToTarget = m_DefaultDistance2Target;
            if (!m_EquipmentScript.weaponInHand1H)
            {
                currentBlockSounds = defaultWeaponSounds;
                m_Animator.SetBool(HashIDs.UnarmedBool, true);
            }
            else currentBlockSounds = m_EquipmentScript.currentWeapon1H.weaponAudio;

            if (!m_EquipmentScript.weaponInHand1H)
            {
                AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                if (curState.shortNameHash != HashIDs.UnarmedLocomotionState)
                    m_Animator.CrossFadeInFixedTime(HashIDs.UnarmedLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
            }
            m_LeftArmOccupation = ArmOccupation.FromReachShield;
        }

#endregion

#region Weapon2H System

        /// <summary>
        /// start animation for taking weapon 2h
        /// </summary>
        protected void _startTakingWeapon2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentWeapon2H == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon2H missing ( null )   < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.weaponInHand2H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon2H already in hand   < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_RightArmOccupation = ArmOccupation.ToReachWeapon2H;
            m_LeftArmOccupation = ArmOccupation.ToReachWeapon2H;

            m_Animator.SetBool(HashIDs.TakeWeapon2HBool, true);
            m_CurrentClipSheet = AnimClipSheet.Weapon2H;
            if (m_EquipmentScript.currentWeapon2H.OnStartTaking != null)
                m_EquipmentScript.currentWeapon2H.OnStartTaking();
        }

        /// <summary>
        /// start putting away weapon 2h animation
        /// </summary>
        protected void _startSheathingWeapon2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentWeapon2H == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_startSheathingWeapon2H() failed. Weapon2H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.weaponInHand2H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Weapon2H already not in hand   < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_RightArmOccupation = ArmOccupation.ToReachWeapon2H;
            m_LeftArmOccupation = ArmOccupation.ToReachWeapon2H;

            m_Animator.SetBool(/*"pSheatheWeapon2H"*/HashIDs.SheatheWeapon2HBool, true);

            if (m_EquipmentScript.currentWeapon2H.OnStartSheathing != null)
                m_EquipmentScript.currentWeapon2H.OnStartSheathing();
        }

        /// <summary>
        /// take weapon2h (switch transforms)
        /// </summary>
        protected void _takeWeapon2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentWeapon2H == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_takeWeapon2H() failed. Current weapon2H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            currentPrimaryWeapon = m_EquipmentScript.currentWeapon2H;
            currentBlockSounds = m_EquipmentScript.currentWeapon2H.weaponAudio;
            m_EquipmentScript.wieldWeapon2H();
            m_Stats.setCurrentAttackValue(
                m_EquipmentScript.currentWeapon2H.damage,
                m_EquipmentScript.currentWeapon2H.range,
                m_Stats.attackSpeed);
            distanceToTarget = m_EquipmentScript.currentWeapon2H.range * 0.75f;
            m_Animator.SetBool(HashIDs.UnarmedBool, false);

            {
                VoidFunc func = () =>
                {
                    _setClipSheet();
                    AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                    if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                        m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                    func = null;
                };
                _setTimedFunc(0.1f, func);
            }
        }

        /// <summary>
        /// put away weapon 2h (switch transforms)
        /// </summary>
        protected void _sheatheWeapon2H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentWeapon2H == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_sheatheWeapon2H() failed.Current weapon2H missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            currentPrimaryWeapon = null;
            currentBlockSounds = defaultWeaponSounds;
            m_EquipmentScript.restWeapon2H();
            m_Stats.resetAttackValues();
            distanceToTarget = m_DefaultDistance2Target;
            m_Animator.SetBool(HashIDs.UnarmedBool, true);
            AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (curState.shortNameHash != HashIDs.UnarmedLocomotionState)
                m_Animator.CrossFadeInFixedTime(HashIDs.UnarmedLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
        }

#endregion

#region Bow System

        /// <summary>
        /// start reaching for bow 
        /// </summary>
        protected void _startTakingBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentBow == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for bow. Bow missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.currentQuiver == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for bow. Quiver missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.currentQuiver.arrowPrefab == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for bow. Quiver arrow prefab missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.bowInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for bow. Bow already in hand < " + this.ToString() + " >");
#endif
                return;
            }

            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for bow. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_LeftArmOccupation = ArmOccupation.ToReachBow;

            m_Animator.SetBool(/*"pTakeBow"*/HashIDs.TakeBowBool, true);
            m_Animator.ResetTrigger(/*"pBowRelease"*/HashIDs.BowReleaseTrig);
            m_Animator.ResetTrigger(/*"pTakeArrow"*/HashIDs.TakeArrowTrig);

            if (m_EquipmentScript.currentBow.OnStartTaking != null)
                m_EquipmentScript.currentBow.OnStartTaking();
        }


        /// <summary>
        /// start to put away bow
        /// </summary>
        protected void _startSheathingBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentBow == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_startSheathingBow() failed. Bow missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.bowInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot sheathe for bow. Bow already not in hand" + ", < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for bow. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_LeftArmOccupation = ArmOccupation.ToReachBow;

            m_arrowInPlace = false;
            m_Animator.SetBool(HashIDs.TakeBowBool, true);
            m_Animator.ResetTrigger(HashIDs.BowReleaseTrig);
            m_Animator.ResetTrigger(HashIDs.TakeArrowTrig);

            if (m_EquipmentScript.currentBow.OnStartSheathing != null)
                m_EquipmentScript.currentBow.OnStartSheathing();

            ArrowPool.RemoveArrow(m_CurrentArrowInHand);
            m_CurrentArrowInHand = null;
        }

        /// <summary>
        /// take bow  (switch bones)
        /// </summary>
        protected void _takeBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentBow == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_takeBow() failed. Current bow missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.currentQuiver == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("_takeBow() failed. Current quiver missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }

            int damage = m_EquipmentScript.currentBow.damage + m_EquipmentScript.currentQuiver.damage;

            m_Stats.setCurrentAttackValue(
                    damage,
                    m_Stats.weaponReach,
                    m_Stats.attackSpeed);

            m_EquipmentScript.wieldBow();
            _startTakingArrow();
        }

        /// <summary>
        /// put away bow  (switch bones)
        /// </summary>
        protected void _sheatheBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            m_EquipmentScript.restBow();
        }

        /// <summary>
        /// start reaching for arrow
        /// </summary>
        protected void _startTakingArrow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_RightArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for arrow. Right arm occupied with: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_RightArmOccupation = ArmOccupation.ToReachArrow;

            m_Animator.SetTrigger(/*"pTakeArrow"*/HashIDs.TakeArrowTrig);
        }

        /// <summary>
        /// pull bow string
        /// </summary>
        protected void _drawBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (!m_arrowInPlace)
            {
                //#if ADDITIONAL_DEBUG_INFO
                //                Debug.LogWarning("Cannot draw bow. Arrow not in place: < " + this.ToString() + " >");
                //#endif
                return;
            }
            if (m_BowDrawn)
            {
                //#if ADDITIONAL_DEBUG_INFO
                //                Debug.LogWarning("Cannot draw bow. Bow already drawn: " + m_RightArmOccupation + ", < " + this.ToString() + " >");
                //#endif
                return;
            }
            GameUtils.PlayRandomClip(m_AudioMan.audioSource,
                m_EquipmentScript.currentBow.weaponAudio.attackSwingSounds);

            m_Animator.SetBool(/*"pDrawBow"*/HashIDs.DrawBowBool, true);
            m_BowDrawn = true;
        }

        /// <summary>
        /// release bow string
        /// </summary>
        protected void _releaseBow()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (!m_BowDrawn)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot release arrow. Bow not drawn < " + this.ToString() + " >");
#endif
                return;
            }
            m_Animator.SetTrigger(/*"pBowRelease"*/HashIDs.BowReleaseTrig);
            m_Animator.SetBool(/*"pDrawBow"*/HashIDs.DrawBowBool, false);
            m_BowDrawn = false;
            m_arrowInPlace = false;

            m_EquipmentScript.currentBow.resetString();
            _shootArrow();

            m_ReleasingBowString = true;

        }

        /// <summary>
        /// get arrow from pool
        /// </summary>
        protected void _createArrow()
        {
#if DEBUG_INFO
            if (!m_EquipmentScript) { Debug.LogError("object cannot be null"); return; }
            if (m_EquipmentScript.bones == null) { Debug.LogError("object cannot be null"); return; }
            if (!m_EquipmentScript.bones.arrow_bone) { Debug.LogError("object cannot be null"); return; }
            if (!m_EquipmentScript.currentQuiver) { Debug.LogError("object cannot be null"); return; }
            if (!m_EquipmentScript.currentQuiver.arrowPrefab)
            {
                Debug.LogError("_currentArrowPrefab is null. Cannot create arrow");
                return;
            }
#endif
            if (m_CurrentArrowInHand != null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot create new arrow. Current arrow is not null. < " + this.ToString() + " >");
#endif
                return;
            }

            m_CurrentArrowInHand = ArrowPool.CreateArrow(
                m_EquipmentScript.currentQuiver.arrowPrefab,
                m_EquipmentScript.bones.arrow_bone,
                m_EquipmentScript.currentQuiver.arrowLifetime,
                m_EquipmentScript.currentQuiver.arrowLength,
                m_EquipmentScript.currentQuiver.layers,
                this);
            m_CurrentArrowInHand.arrowDamage = m_EquipmentScript.currentQuiver.damage;
            m_CurrentArrowInHand.bowDamage = m_EquipmentScript.currentBow.damage;
        }

        /// <summary>
        /// shoot arrow
        /// </summary>
        protected virtual void _shootArrow()
        {
#if DEBUG_INFO
            if (!m_EquipmentScript) { Debug.LogError("object cannot be null"); return; }
            if (m_EquipmentScript.bones == null) { Debug.LogError("object cannot be null."); return; }
            if (m_Player.m_Camera == null) { Debug.LogError("object cannot be null"); return; }
            if (m_CurrentArrowInHand == null) { Debug.LogError("object cannot be null"); return; }
            if (!m_AudioMan) { Debug.LogError("object cannot be null"); return; }
#endif

            Vector3 arrow_pos = m_EquipmentScript.bones.arrow_bone.position;

            Ray ray = new Ray(m_Player.m_Camera.transform.position, m_Player.m_Camera.transform.forward);
            Vector3 currentTarget = ray.origin + ray.direction * 100.0f;
            int mask = m_CurrentArrowInHand.layers;
            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, mask/*, QueryTriggerInteraction.Ignore*/);
            if (hits.Length > 0)
            {
                System.Array.Sort(hits, m_RayHitComparer);

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.gameObject.layer == m_CurrentArrowInHand.owner.transform.gameObject.layer)
                        continue;
                    currentTarget = hits[i].point;
                    break;
                }
            }
            Vector3 arrowDirection = (currentTarget - arrow_pos).normalized;

            float curStrength = Mathf.Clamp(m_BowShootPower * m_BowShootPower, 0.05f, 1.0f);
            float arrowspeed = m_EquipmentScript.currentBow.range * curStrength;

            ArrowPool.ShootArrow(m_CurrentArrowInHand, ref arrow_pos, ref arrowDirection, arrowspeed);
            m_CurrentArrowInHand = null;

            m_AudioMan.stopAudio();
            GameUtils.PlayRandomClip(m_AudioMan.audioSource,
                    m_EquipmentScript.currentBow.weaponAudio.attackHitSounds);
        }

#endregion

#region Secondary System

        /// <summary>
        /// start taking secondary weapon
        /// </summary>
        protected void _startTakingSecondaryWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentSecondary == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for secondary weapon. Current secondary missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_EquipmentScript.secondaryWeaponInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for secondary weapon. Current secondary already in hand < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_LeftArmOccupation = ArmOccupation.ToReachSecondary;


            m_Animator.SetBool(HashIDs.TakeSecondaryBool, true);
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            m_CurrentClipSheet = AnimClipSheet.DualWeapons;

            if (m_EquipmentScript.currentSecondary.OnStartTaking != null)
                m_EquipmentScript.currentSecondary.OnStartTaking();
        }

        /// <summary>
        /// start removing secondary weapon
        /// </summary>
        protected void _startSheathingSecondaryWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_EquipmentScript.currentSecondary == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for secondary weapon. Current secondary missing ( null ) < " + this.ToString() + " >");
#endif
                return;
            }
            if (!m_EquipmentScript.secondaryWeaponInHand)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for secondary weapon. Current secondary already not in hand < " + this.ToString() + " >");
#endif
                return;
            }
            if (m_LeftArmOccupation != ArmOccupation.None)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("Cannot reach for weapon shield. Left arm occupied with: " + m_LeftArmOccupation + ", < " + this.ToString() + " >");
#endif
                return;
            }
            m_LeftArmOccupation = ArmOccupation.ToReachSecondary;

            m_Animator.SetBool(HashIDs.TakeSecondaryBool, true);
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);

            if (m_EquipmentScript.currentSecondary.OnStartSheathing != null)
                m_EquipmentScript.currentSecondary.OnStartSheathing();
        }

        /// <summary>
        /// take secondary weapon 
        /// </summary>
        protected void _takeSecondaryWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentSecondary == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("takeSecondaryWeapon1H() failed. no current weapon1h");
#endif
                return;
            }

            currentSecondaryWeapon = m_EquipmentScript.currentSecondary;
            m_EquipmentScript.wieldSecondary();

            if(m_RightArmOccupation == ArmOccupation.None ||
                m_RightArmOccupation == ArmOccupation.FromReachWeapon1H )
            {
                VoidFunc func = () =>
                {
                    _setClipSheet();

                    AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                    if (curState.shortNameHash != HashIDs.DefaultLocomotionState)
                        m_Animator.CrossFadeInFixedTime(HashIDs.DefaultLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
                    func = null;
                };
                _setTimedFunc(0.1f, func);
            }

            m_Animator.SetBool(HashIDs.UnarmedBool, false);
            m_LeftArmOccupation = ArmOccupation.FromReachSecondary;
        }

        /// <summary>
        /// remove secondary weapon 
        /// </summary>
        protected void _sheatheSecondaryWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.currentSecondary == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("sheatheSecondaryWeapon1H() failed. no current weapon1h");
#endif
                return;
            }

            if (currentSecondaryWeapon == m_EquipmentScript.currentShield)
                currentSecondaryWeapon = null;

            m_EquipmentScript.restSecondary();
            if (!m_EquipmentScript.weaponInHand1H)
            {
                AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(0);
                if (curState.shortNameHash != HashIDs.UnarmedLocomotionState)
                    m_Animator.CrossFadeInFixedTime(HashIDs.UnarmedLocomotionState, locomotionTransitionSpeed, 0, curState.normalizedTime);
            }
            if (!m_EquipmentScript.weaponInHand1H)
                m_Animator.SetBool(HashIDs.UnarmedBool, true);
            m_LeftArmOccupation = ArmOccupation.FromReachSecondary;
        }


#endregion



#region Set Animation Clips

        /// <summary>
        /// change animation clips to dual wield weapon system
        /// </summary>
        protected void _setDualWeaponClips()
        {
#if DEBUG_INFO
            if (!m_Player) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif

            if (m_PrimaryClipSheetSource == m_EquipmentScript.currentWeapon1H &&
                m_SecondaryClipSheetSource == m_EquipmentScript.currentSecondary)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.Log("Dual weapons clips already set for " +
                    m_EquipmentScript.currentWeapon1H.gameObject.name + " and " +
                    m_EquipmentScript.currentSecondary.gameObject.name);
#endif
                return;
            }
#if ADDITIONAL_DEBUG_INFO
            Debug.Log("Setting dual weapons clips for " +
                   m_EquipmentScript.currentWeapon1H.gameObject.name + " and " +
                   m_EquipmentScript.currentSecondary.gameObject.name);
#endif

            m_PrimaryClipSheetSource = m_EquipmentScript.currentWeapon1H;
            m_SecondaryClipSheetSource = m_EquipmentScript.currentSecondary;
            m_Player.setClips(
                dualWeaponAttackClip,
                dualWeaponBlockStance,
                dualWeaponBlockHitClips,
                dualWeaponLocomotionClips,
                dualWeaponReplacementClips);
        }

        /// <summary>
        /// change animation clips on weapon
        /// </summary>
        /// <param name="meleeItem"></param>
        protected void _setWeaponClips(MeleeWeaponItem meleeItem)
        {
#if DEBUG_INFO
            if (!m_Player) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif
            if (m_PrimaryClipSheetSource == meleeItem &&
                m_SecondaryClipSheetSource == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.Log("Weapon clips already set for " +
                       meleeItem.gameObject.name);
#endif
                return;
            }
#if ADDITIONAL_DEBUG_INFO
            Debug.Log("Setting Weapon clips for " +
                   meleeItem.gameObject.name);
#endif
            m_PrimaryClipSheetSource = meleeItem;
            m_SecondaryClipSheetSource = null;
            m_Player.setWeaponClips(meleeItem);
        }

        /// <summary>
        /// change animation clips on shield
        /// </summary>
        /// <param name="shieldItem"></param>
        protected void _setShieldClips(ShieldItem shieldItem)
        {
#if DEBUG_INFO
            if (!m_Player) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif
            if (m_PrimaryClipSheetSource == shieldItem &&
                m_SecondaryClipSheetSource == null)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.Log("Shield clips already set for " +
                       shieldItem.gameObject.name);
#endif
                return;
            }
#if ADDITIONAL_DEBUG_INFO
            Debug.Log("Setting shield clips for " + shieldItem.gameObject.name);
#endif
            m_PrimaryClipSheetSource = shieldItem;
            m_SecondaryClipSheetSource = null;
            m_Player.setShieldClips(shieldItem);
        }

        /// <summary>
        /// change animation clips on weapon & shield
        /// </summary>
        /// <param name="meleeItem"></param>
        /// <param name="shieldItem"></param>
        protected void _setWeapon1HShieldClips(MeleeWeaponItem meleeItem, ShieldItem shieldItem)
        {
#if DEBUG_INFO
            if (!m_Player) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif
            if (m_PrimaryClipSheetSource == meleeItem &&
                m_SecondaryClipSheetSource == shieldItem)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.Log("Weapon&Shield clips already set for " +
                   m_EquipmentScript.currentWeapon1H.gameObject.name + " and " +
                   m_EquipmentScript.currentShield.gameObject.name);
#endif
                return;
            }
#if ADDITIONAL_DEBUG_INFO
            Debug.Log("Setting weapon&shield clips for " + meleeItem.gameObject.name +
                " and " + shieldItem.gameObject.name);
#endif
            m_PrimaryClipSheetSource = meleeItem;
            m_SecondaryClipSheetSource = shieldItem;
            m_Player.setWeapon1HShieldClips(meleeItem, shieldItem, dualWeaponLocomotionClips);
        }

#endregion


#region Animation Events


        /// <summary>
        /// animation event called upon attack combo end time
        /// </summary>
        /// <param name="e"></param>
        void AttackComboEndEvent(AnimationEvent e)
        {
            _attack_combo_end();
        }

        /// <summary>
        /// animation event called upon disable physics  time
        /// </summary>
        /// <param name="e"></param>
        void DisablePhysicsEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Character)
            {
                Debug.LogError("object cannot be null." + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (e.animatorClipInfo.weight < 0.9) return;
            m_Character.disablePhysics(true);
        }

        /// <summary>
        /// animation event called upon attack start time
        /// </summary>
        /// <param name="e"></param>
        void AttackStartEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_BreakCombo)
            {

                m_Animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                m_Attack_combo_started = false;
                m_InCombat = false;
                if (m_PrimaryTargetNPC != null) m_PrimaryTargetNPC.attack_end_notify(this, 1);
                foreach (IGameCharacter npcInRange in m_NpcsInRange)
                {
                    if (m_PrimaryTargetNPC != npcInRange)
                    {
                        npcInRange.attack_end_notify(this, m_CurrentAttackType);
                    }
                }
                m_NpcsInRange.Clear();
                return;
            }

            m_AttackHitCount = 0;
            m_BreakCombo = true;

            m_CurrentAttackType = e.intParameter;

            IGameCharacter ts = m_PrimaryTargetNPC;
            if (m_ChangeTarget)
            {
                if (m_PrimaryTargetNPC != null) m_PrimaryTargetNPC.attack_end_notify(this, m_CurrentAttackType);
                foreach (IGameCharacter npcInRange in m_NpcsInRange)
                {
                    npcInRange.attack_end_notify(this, m_CurrentAttackType);
                }
                m_NpcsInRange.Clear();

#if TESTING_USE_ALL_NPC_ARRAY
                m_PrimaryTargetNPC = _getPrimaryTarget();
#else
                m_PrimaryTargetNPC = _getPrimaryTargetInZone();
#endif
            }
            if (ts != m_PrimaryTargetNPC)
            {
                if (m_PrimaryTargetNPC != null)
                {
                    m_BreakCombo = false;
                }
            }

            m_DoAttackRotation = true;
            m_AttackRotTime = 0.2f;
            m_AttackRotTimer = 0.0f;
            m_AttackStartRot = transform.rotation;
            m_AttackEndRot = Quaternion.LookRotation(m_TargetDirection);

            if (m_PrimaryTargetNPC != null)
            {
                bool isObstructed = _sweepTest(m_Direction2target);
                bool isInView = _targetInView(m_PrimaryTargetNPC, m_TargetDirection);
                if (isInView && !isObstructed)
                {
                    m_PrimaryTargetNPC.attack_start_notify(this, m_CurrentAttackType);
                    if (enableJumpToTarget) _jumpToTarget();
                }
            }
        }

        /// <summary>
        /// animation event called upon attack hit time
        /// </summary>
        /// <param name="e"></param>
        void AttackHitEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (e.animatorClipInfo.weight < 0.9) return;


            bool applyHitRection = true/*m_AttackHitCount <= 1*/;

            m_Stats.resetAttackValues();
            distanceToTarget = m_DefaultDistance2Target;

            if (e.intParameter == 0)
            {
                if (currentPrimaryWeapon)
                {
                    m_Stats.setCurrentAttackValue(
                        currentPrimaryWeapon.damage,
                        currentPrimaryWeapon.range,
                        m_Stats.attackSpeed);
                    distanceToTarget = currentPrimaryWeapon.range * 0.75f;
                }
            }
            else if (e.intParameter == 1)
            {
                if (currentSecondaryWeapon)
                {
                    m_Stats.setCurrentAttackValue(
                        currentSecondaryWeapon.damage,
                        currentSecondaryWeapon.range,
                        m_Stats.attackSpeed);
                    distanceToTarget = currentSecondaryWeapon.range * 0.75f;
                }
            }

            if (m_PrimaryTargetNPC != null)
            {
                bool isDead = m_PrimaryTargetNPC.stats.currentHealth <= 0;
                if (!isDead)
                {
                    bool block = false;
                    bool success = m_PrimaryTargetNPC.attack_hit_notify(this, m_CurrentAttackType, e.intParameter, ref block, applyHitRection);
                    if (success)
                    {
                        if (!block)
                            _playAttackHitSound(e.intParameter);
                    }
                    else
                    {
                        Debug.LogWarning("attack hit not success");
                    }
                }
            }


            float weaponSweepAngle = e.floatParameter;
            Vector3 pos = transform.position;
            Vector3 dir = transform.forward;
            m_NpcMan.CollectNpcsInRangeAngle(m_Stats.currentWeaponReach, weaponSweepAngle, pos, dir, m_NpcsInRange);
            foreach (IGameCharacter npcInRange in m_NpcsInRange)
            {
                if (npcInRange == m_PrimaryTargetNPC) continue;

                Vector3 toTarget = npcInRange.position - transform.position;
                bool isObstructed = _sweepTest(toTarget);
                if (isObstructed) continue;

                bool block = false;
                bool success = npcInRange.attack_hit_notify(this, m_CurrentAttackType, e.intParameter, ref block, applyHitRection);
                if (success)
                {
                    if (!npcInRange.isDead)
                    {
                        if (!block)
                            _playAttackHitSound(e.intParameter);
                    }
                }
            }

            _playAttackSwingSound(e.intParameter);

            m_AttackHitCount++;
        }

        /// <summary>
        /// animation event called upon ragdoll start time
        /// </summary>
        /// <param name="e"></param>
        void StartRagdollEvent(AnimationEvent e)
        {
            if (e.animatorClipInfo.weight < 0.9) return;
            startRagdoll();
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
            m_Player.strafing = m_PrevStrafing;
            m_Player.disableInput = false;
            m_Character.setIKMode(m_PrevIKmode);
            if (!m_Character.isCrouching)
            {
                m_Player.m_Camera.switchTargets(m_Player.m_Camera.Target);
                m_Character.setMoveMode(m_PrevMoveMode);
            }
        }



        /// <summary>
        /// animation event called upon taking weapon time
        /// </summary>
        void OnTakeWeaponEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_RightArmOccupation != ArmOccupation.ToReachWeapon1H)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("OnTakeWeaponEvent() Cannot reach for primary weapon. Right arm occupied: " + m_RightArmOccupation );
#endif
                return;
            }

            if (m_SetSecondaryAsPrimary)
            {
                _takeWeapon1H();
            }
            else
            {
                if (m_EquipmentScript.weaponInHand1H)
                {
                    _sheatheWeapon1H();
                }
                else
                {
                    _takeWeapon1H();
                }
            }
            m_SetSecondaryAsPrimary = false;
        }

        /// <summary>
        /// animation event called upon grab shield time
        /// </summary>
        void OnGrabShieldBackEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_LeftArmOccupation != ArmOccupation.ToReachShield)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("OnGrabShieldBackEvent() Cannot reach for shield. Left arm occupied: " + m_LeftArmOccupation );
#endif
                return;
            }

            if (m_EquipmentScript.shieldInHand)
            {
                _sheatheShield();
            }
            else
            {
                _takeShield();
            }
        }

        // <summary>
        /// animation event called upon taking secondary weapon
        /// </summary>
        void OnTakeSecondaryWeaponEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_LeftArmOccupation != ArmOccupation.ToReachSecondary)
            {
#if ADDITIONAL_DEBUG_INFO
                Debug.LogWarning("OnTakeSecondaryWeaponEvent() Cannot reach for secondary weapon. Left arm occupied: " + m_LeftArmOccupation);
#endif
                return;
            }
            if (m_EquipmentScript.secondaryWeaponInHand)
            {
                _sheatheSecondaryWeapon1H();
            }
            else
            {
                _takeSecondaryWeapon1H();
            }
        }

        /// <summary>
        ///  animation event called upon taking weapon two handed time
        /// </summary>
        void OnTakeWeapon2HEvent()
        {
            _takeWeapon2H();
        }

        /// <summary>
        /// animation event called upon putaway weapon two handed timeć 
        /// </summary>
        void OnPutawayWeapon2HEvent()
        {
            _sheatheWeapon2H();
        }

        /// <summary>
        ///  animation event called upon taking weapon end time
        /// </summary>
        void OnTakeWeapon1HEnd()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            m_RightArmOccupation = ArmOccupation.None;
            m_Animator.SetBool(HashIDs.TakeWeapon1HBool, false);
            m_Animator.SetBool(HashIDs.TakeWeaponRightWaistBool, false);
            if (m_EquipmentScript.weaponInHand1H)
            {
                if (OnTakeWeapon1H != null)
                    OnTakeWeapon1H();
            }
            else
            {
                if (OnSheatheWeapon1H != null)
                    OnSheatheWeapon1H();

            }
        }

        /// <summary>
        /// animation event called upon ending grab shield time
        /// </summary>
        void OnGrabShieldEnd()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            m_LeftArmOccupation = ArmOccupation.None;
            m_Animator.SetBool(HashIDs.TakeShieldBool, false);
            if (m_EquipmentScript.shieldInHand)
            {
                if (OnTakeShield != null)
                {
                    OnTakeShield();
                }
            }
            else
            {
                if (OnPutawayShield != null)
                {
                    OnPutawayShield();
                }
            }
        }

        /// <summary>
        /// animation event called upon taking secondary weapon ended
        /// </summary>
        void OnTakeSecondaryWeaponEndEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            m_LeftArmOccupation = ArmOccupation.None;
            m_Animator.SetBool(HashIDs.TakeSecondaryBool, false);
            if (m_EquipmentScript.shieldInHand)
            {
                if (OnTakeSecondaryWeapon1H != null)
                    OnTakeSecondaryWeapon1H();
            }
            else
            {
                if (OnSheatheSecondaryWeapon1H != null)
                    OnSheatheSecondaryWeapon1H();
            }
        }

        /// <summary>
        /// animation event called upon take weapon two handed end time
        /// </summary>
        void OnTakeWeapon2HEnd()
        {

#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            m_RightArmOccupation = ArmOccupation.None;
            m_LeftArmOccupation = ArmOccupation.None;
            m_Animator.SetBool(HashIDs.TakeWeapon2HBool, false);
            m_Animator.SetBool(HashIDs.SheatheWeapon2HBool, false);

            if (m_EquipmentScript.weaponInHand2H)
            {
                if (OnTakeWeapon2H != null)
                    OnTakeWeapon2H();
            }
            else
            {
                if (OnSheatheWeapon2H != null)
                    OnSheatheWeapon2H();
            }
        }





        /// <summary>
        /// animation event called upon take bow
        /// </summary>
        void OnTakeBowEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif

            if (m_EquipmentScript.bowInHand)
                _sheatheBow();
            else
                _takeBow();
        }


        /// <summary>
        /// animation event called upon taking bow eneded
        /// </summary>
        void OnTakeBowEndEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            m_LeftArmOccupation = ArmOccupation.None;
            m_Animator.SetBool(HashIDs.TakeBowBool, false);
            if (m_EquipmentScript.bowInHand)
            {
                if (OnTakeBow != null)
                    OnTakeBow();
            }
            else
            {
                if (OnPutawayBow != null)
                    OnPutawayBow();
            }

        }

        /// <summary>
        /// animation event called to start taking  arrow
        /// </summary>//  animation event
        void TakeArrowEvent()
        {
            _startTakingArrow();
        }

        /// <summary>
        /// animation event called on take arrow 
        /// </summary>
        void OnTakeArrowEvent()
        {
            _createArrow();
        }

        /// <summary>
        /// animation event called upon taking arrow ended
        /// </summary>
        void OnTakeArrowEnd()
        {
            m_RightArmOccupation = ArmOccupation.None;
            m_arrowInPlace = true;
        }

        /// <summary>
        /// animation event called upon releasing bow string
        /// </summary>
        void ReleaseStringEndEvent()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized < " + this.ToString() + " >");
                return;
            }
#endif
            if (m_DisableAimOnRelease)
            {
                m_Aiming = false;
                m_DisableAimOnRelease = false;
            }
            m_Animator.SetBool(HashIDs.AimBool, m_Aiming);
            m_Player.strafing = m_Aiming;
            m_ReleasingBowString = false;
        }

#endregion

    }
}