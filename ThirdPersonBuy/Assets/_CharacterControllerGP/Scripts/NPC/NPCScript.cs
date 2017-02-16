// © 2016 Mario Lelas

#define SMOOTH_MOVEMENT


using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// npc ai class
    /// </summary>
    [RequireComponent(typeof(TPCharacter), typeof(RagdollManager))]
    public class NPCScript : MonoBehaviour, IGameCharacter
    {
        /// <summary>
        /// available npc states
        /// </summary>
        public enum NPCState { None, Idle, Chase, Wait, Return };

        /// <summary>
        /// attack collider and rigid body for checking obstacles
        /// </summary>
        [Tooltip("Attack collider and rigid body for checking obstacles.")]
        public Rigidbody attackSweepBody;

        /// <summary>
        /// npc personal space
        /// </summary>
        [Tooltip("Npc personal space.")]
        public float avoidanceDistance = 1.0f;

        /// <summary>
        /// chance to black player attack
        /// </summary>
        [Tooltip("Chance to black player attack.")]
        [Range(2, 100)]
        public int blockOdds = 10;

        /// <summary>
        /// npc cooldown time after attack
        /// </summary>
        [Tooltip("Npc cooldown time after attack.")]
        public float attackInterval = 1.0f;

        /// <summary>
        /// height from which character dies if falls
        /// </summary>
        [Tooltip("Height from which character dies if falls.")]
        public float fallDieHeight = 10.0f;


        private Stats m_Stats;                              // stats of npc
        private Animator m_Animator;                        // animator reference
        private AudioManager m_Audio;                       // Audio manager reference
        private RagdollManager m_Ragdoll;                   // RagdollManager reference
        private TPCharacter m_Character;                    // TPCharacter reference
        private UnityEngine.AI.NavMeshPath m_path;                         // nav mesh path info class
        private Player m_Player;                            // Player reference
        private NPCManager m_NPCManager;                    // reference to npc manager ( npc array )
        private HealthUI m_HealthUI;                        // health indicator
        private WeaponAudio m_DefaultWeaponSounds;           // default weapon sounds
        private DebugUI m_DamageUI;                         // ui showing damage


        private Vector3 m_Move = Vector3.zero;              // npc movement velocity
        private Vector3 m_BodyLookDir = Vector3.zero;       // npc body look direction
        private Vector3 m_CurrentDest = Vector3.zero;       // current destination position
        private NPCState m_NpcState = NPCState.Idle;        // current npc state
        private bool m_Initialized = false;                 // is component initialized ?
        private bool m_InFightZoneRange = false;            // is npc in fight zone ?
        private bool m_InAttackRange = false;                // is npc in attack range ?
        private const int ATTACK_COUNT = 3;                 // number of attacks in combo 
        private int m_CurrentAttackType = 0;                    // current attack type
        private bool m_Attack_combo_started = false;        // is attack combo started ?
        private Vector3 m_Direction2target;                 // direction to current target
        private float m_Distance2target;                    // distance to target
        private bool m_Blocking = false;                    // block flag
        private float m_BlockTimer = 0.0f;                  // block timer
        private const float BLOCK_INTERVAL = 2.5f;          // block interval time  
        private bool m_Kooldown = false;                    // time between attacks flag
        private float m_AttackTimer = 0.0f;                 // attack timer
        private Vector3 m_StartPosition;                    // initial guard position
        private Vector3 m_StartLookAt;                      // initial look at direction
        private NPCGuardZone m_GuardZone;                   // current guard zone the npcs is assigned to    
        private bool m_OnStartPosition = true;                // is character standing on initial position
        
        private bool m_InCombat = false;                // is player in combat ?
        private float m_AttackHitDistance = 1.5f;       // distance at which npc attacks
        private bool m_AttackStarted = false;                           // indication that attack swing started

        /// <summary>
        /// gets Stats component
        /// </summary>
        public Stats stats { get { return m_Stats; } }

        /// <summary>
        /// gets TPCharacter component
        /// </summary>
        public TPCharacter character { get { return m_Character; } }

        /// <summary>
        /// gets position
        /// </summary>
        public Vector3 position { get { return transform.position; } }

        /// <summary>
        /// gets AudioManager component
        /// </summary>
        public AudioManager audioManager { get { return m_Audio; } }

        /// <summary>
        /// return true if player is in combat ( taking hit or attacking
        /// </summary>
        public bool inCombat { get { return m_InCombat; } }

        /// <summary>
        /// gets and sets taking hit flag
        /// </summary>
        public bool takingHit { get; set; }

        /// <summary>
        /// gets Animator component
        /// </summary>
        public Animator animator { get { return m_Animator; } }

        /// <summary>
        /// gets npc in attack combo flag
        /// </summary>
        public bool attacking { get { return m_Attack_combo_started; } }

        /// <summary>
        /// gets and sets current npc state
        /// </summary>
        public NPCState npcState { get { return m_NpcState; } set { m_NpcState = value; } }

        /// <summary>
        /// is target in reach range
        /// </summary>
        public bool inAttackRange { get { return m_InAttackRange; } }

        /// <summary>
        /// is target in fight zone range
        /// </summary>
        public bool inFightZoneRange { get { return m_InFightZoneRange; } }

        /// <summary>
        /// is character dead
        /// </summary>
        public bool isDead { get { return m_Stats.currentHealth <= 0; } }

        /// <summary>
        /// gets and sets move velocity
        /// </summary>
        public Vector3 moveVelocity { get { return m_Move; } set { m_Move = value; } }

        /// <summary>
        /// gets current guard zone the npc is assigned to
        /// </summary>
        public NPCGuardZone guardZone { get { return m_GuardZone; } set { m_GuardZone = value; } }

        /// <summary>
        /// gets current ragdoll state
        /// </summary>
        public RagdollManager.RagdollState ragdollState
        {
            get
            {
#if DEBUG_INFO
                if (!m_Ragdoll)
                {
                    Debug.LogError("object cannot be null ( RagdollManager ) " + " < " + this.ToString() + ">");
                    return 0;
                }
#endif
                return m_Ragdoll.state;
            }
        }


        public RagdollManager ragdoll
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
                return m_Player.ragdoll;
            }
        }



        /// <summary>
        /// initialize component
        /// </summary>
        public void initialize()
        {
            if (m_Initialized) return;

            m_Animator = GetComponent<Animator>();
            if (!m_Animator) { Debug.LogError("Cannot find component 'Animator'" + " < " + this.ToString() + ">"); return; }

            m_Audio = GetComponent<AudioManager>();
            if (!m_Audio) { Debug.LogError("Cannot find component 'AudioManager'" + " < " + this.ToString() + ">"); return; }

            m_Character = GetComponent<TPCharacter>();
            if (!m_Character) { Debug.LogError("Cannot find component 'TPCharacter'" + " < " + this.ToString() + ">"); return; }
            m_Character.initialize();
            m_Character.setIKMode(TPCharacter.IKMode.None);

            AudioManager amg = m_Character.audioManager as AudioManager;
            if (!amg) { Debug.LogError("Cannot find component 'AudioManager'" + " < " + this.ToString() + ">"); return; }
            m_Audio = amg;

            m_DefaultWeaponSounds = GetComponent<WeaponAudio>();
            if (!m_DefaultWeaponSounds) { Debug.LogError("Cannot find 'WeaponAudio' component <" + this.ToString() + " >");return; }

            m_Stats = GetComponent<Stats>();
            if (!m_Stats) { Debug.LogError("Cannot find 'Stats' component: " + " < " + this.ToString() + ">"); return; }

            m_HealthUI = GetComponent<HealthUI>();

            m_Ragdoll = GetComponent<RagdollManager>();
            if (!m_Ragdoll) { Debug.LogError("Cannot find component 'RagdollManager'" + " < " + this.ToString() + ">"); return; }
            m_Ragdoll.initialize();
            m_Ragdoll.OnHit = () =>
            {
                m_Character.simulateRootMotion = false;
                m_Character.disableMove = true;
                m_Character.rigidBody.velocity = Vector3.zero;

                m_Character.rigidBody.detectCollisions = false;
                m_Character.rigidBody.isKinematic = true;
                m_Character.capsule.enabled = false;
            };
            // allow movement when transitioning to animated
            m_Ragdoll.OnStartTransition = () =>
            {
                /* 
                    Enable simulating root motion on transition  if 
                    character is not in full ragdoll to
                    make character not freeze on place when hit.
                    Otherwise root motion will interfere with getting up animation.
                */
                if (!m_Ragdoll.isFullRagdoll && !m_Ragdoll.isGettingUp)
                {
                    m_Character.simulateRootMotion = true;
                    m_Character.rigidBody.detectCollisions = true;
                    m_Character.rigidBody.isKinematic = false;
                    m_Character.capsule.enabled = true;
                }
            };

            // event that will be last fired ( when full ragdoll - on get up, when hit reaction - on blend end 
            m_Ragdoll.LastEvent = () =>
            {
                m_Character.simulateRootMotion = true;
                m_Character.disableMove = false;

                m_Character.rigidBody.detectCollisions = true;
                m_Character.rigidBody.isKinematic = false;
                m_Character.capsule.enabled = true;
            };

            m_Ragdoll.ragdollEventTime = 4.0f;
            //m_Ragdoll.OnTimeEnd = () =>
            //{
            //    if (m_Stats.health > 0)
            //    {
            //        m_Ragdoll.blendToMecanim();
            //    }
            //    //m_Ragdoll.OnTimeEnd = null;
            //};

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (!playerGO)
            {
                Debug.LogError("Cannot find object with tag 'Player' " + " < " + this.ToString() + ">");
                return;
            }
            m_Player = playerGO.GetComponent<Player>();
            if (!m_Player) { Debug.LogError("Cannot find 'Player' script on  " + playerGO.name + " < " + this.ToString() + ">"); return; }

            m_path = new UnityEngine.AI.NavMeshPath();

            m_NpcState = NPCState.Idle;

            m_NPCManager = GameObject.FindObjectOfType<NPCManager>();

            m_StartPosition = transform.position;
            m_StartLookAt = transform.forward;

#if SMOOTH_MOVEMENT
            for (int i = 0; i < avgMove.Length; i++)
            {
                avgMove[i] = Vector3.zero;
            }
#endif

            if (!attackSweepBody)
            {
                Debug.LogError("Cannot find attack sweep body used for finding obstructions." + " < " + this.ToString() + ">");
                return;
            }

            m_DamageUI = GetComponent<DebugUI>();

            m_Initialized = true;
        }

        /// <summary>
        /// break attack method
        /// </summary>
        public void breakAttack()
        {
            if(m_Attack_combo_started)
                _end_attack_combo();
        }

        /// <summary>
        /// revive character if dead
        /// </summary>
        /// <param name="health">health</param>
        public void revive(int health)
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
            m_Ragdoll.blendToMecanim();
        }

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();

            //if (DEBUGUI) DEBUGUI.setText("Start", 5, Color.white);
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
                Debug.LogError("Component not initialized " + " < " + this.ToString() + ">");
                return;
            }
#endif

            if (m_HealthUI)
            {
                float healthNorm = (float)m_Stats.currentHealth / (float)m_Stats.maxHealth;
                m_HealthUI.scaleX = healthNorm;
            }

            if (m_Player.ragdoll.state != RagdollManager.RagdollState.Animated ||
                m_Ragdoll.state != RagdollManager.RagdollState.Animated)
            {
                return;
            }

            m_Animator.SetFloat(HashIDs.AttackSpeedFloat/*"AttackSpeed"*/, m_Stats.currentAttackSpeed);

            if (!m_InAttackRange) m_InCombat = false;


            m_BlockTimer += Time.deltaTime;
            if (m_BlockTimer > BLOCK_INTERVAL)
            {
                m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, false);
                m_Blocking = false;
            }

            switch (m_NpcState)
            {
                case NPCState.Idle:
                    _idle_state();
                    break;
                case NPCState.Chase:
                    _chase_state();
                    break;
                case NPCState.Wait:
                    _wait_state();
                    break;
                case NPCState.Return:
                    _return_state();
                    break;
            }
            _fallToDeathCheck();
        }


#if DEBUG_INFO
        void OnDrawGizmos()
        {
            if (m_path == null) return;

            Gizmos.color = Color.yellow;
            if (0 < m_path.corners.Length)
            {
                for (int i = 1; i < m_path.corners.Length; i++)
                {
                    Gizmos.DrawLine(m_path.corners[i - 1], m_path.corners[i]);
                }
            }
        }
#endif
        /// <summary>
        /// set npc to return to start position
        /// </summary>
        public void return2Post()
        {
            if (!m_OnStartPosition)
            {
                m_NpcState = NPCState.Return;
            }
            else
            {
                m_NpcState = NPCState.Idle;
            }
        }

        /// <summary>
        /// set npc to chase the player
        /// </summary>
        public void startChase()
        {
            m_OnStartPosition = false;
            m_NpcState = NPCState.Chase;
        }

        /// <summary>
        /// return to guard position state
        /// </summary>
        private void _return_state()
        {
            m_CurrentDest = m_StartPosition;

            m_Direction2target = m_CurrentDest - transform.position;
            m_Distance2target = m_Direction2target.magnitude;
            m_Direction2target.y = 0.0f;
            m_Direction2target.Normalize();

            _calc_ranges();

            m_Character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);


            _calculate_path(ref m_Move);

            m_Move.y = 0.0f;
            m_Move *= m_Stats.DefaultMoveSpeed;
            m_BodyLookDir = m_Move;
            if (takingHit)
            {
                m_Move = Vector3.zero;
                m_BodyLookDir = Vector3.zero;
            }
            m_Character.move(m_Move, false, false, false, m_BodyLookDir, Vector3.zero);

            if (m_InAttackRange)
            {
                m_NpcState = NPCState.Idle;
                m_Character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);
                m_Move = Vector3.zero;
                m_Character.fullStop();
                m_OnStartPosition = true;
            }
        }

        /// <summary>
        /// just stand idle state
        /// </summary>
        private void _idle_state()
        {
            m_Character.move(Vector3.zero, false, false, false, m_StartLookAt, Vector3.zero);
        }

        /// <summary>
        /// chase target and attack state
        /// </summary>
        private void _chase_state()
        {
            Vector3 playerPosition = m_Player.transform.position;
            m_CurrentDest = playerPosition;

            m_Direction2target = m_CurrentDest - transform.position;
            m_Direction2target.y = 0.0f;
            m_Distance2target = m_Direction2target.magnitude;
            m_Direction2target.Normalize();

            _calc_ranges();

            if (m_Kooldown)
            {
                m_AttackTimer += Time.deltaTime;
                if (m_AttackTimer >= attackInterval)
                {
                    m_Kooldown = false;
                    m_AttackTimer = 0.0f;
                }
            }

            m_Move = Vector3.zero;


            if (!m_InAttackRange)
            {
                if (m_Attack_combo_started)
                {
                    m_Player.attack_end_notify(this, m_CurrentAttackType);
                }
                m_Character.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                m_Attack_combo_started = false;


                _calculate_path(ref m_Move);

                m_Character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);
                if (m_InFightZoneRange)
                {
                    m_Character.setMoveMode(TPCharacter.MovingMode.Strafe);
                }
            }
            else
            {
                if (!takingHit && !m_Blocking)
                {
                    m_Character.turnTransformToDirection(m_Direction2target);
                    m_BodyLookDir = m_Direction2target;
                    bool playerUnderAttack = m_NPCManager.AnyNpcInCombatInZone(this); // m_NPCManager.AnyNpcAttackingInZone(this); //

                    if (!m_Attack_combo_started && !m_Kooldown && !playerUnderAttack && !m_Player.triggerActive)
                    {
                        m_InCombat = true;
                        m_Character.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, true);
                        m_Attack_combo_started = true;
                    }
                }
            }
            if (!m_InAttackRange)
            {
                _add_local_avoidance(ref m_Move);
#if SMOOTH_MOVEMENT
                m_Move = getAVG();
#endif

                m_Move.y = 0.0f;
                float mag = m_Move.magnitude;
                float curSpeed = m_Stats.DefaultMoveSpeed;
                if (mag < 0.25f) curSpeed = 0.0f;
                m_Move.Normalize();
                m_Move *= curSpeed;
                m_BodyLookDir = (m_InFightZoneRange ? m_Direction2target : m_Move);
            }
            else
            {
                m_Move = Vector3.zero;
            }
            if (takingHit || m_Blocking)
            {
                m_Move = Vector3.zero;
                m_BodyLookDir = Vector3.zero;
            }
            m_Character.move(m_Move, false, false, false, m_BodyLookDir, Vector3.zero);
        }


        /// <summary>
        /// wait if target is attack by another and adjust position state
        /// </summary>
        private void _wait_state()
        {
            Vector3 playerPosition = m_Player.transform.position;
            m_CurrentDest = playerPosition;

            m_Direction2target = m_CurrentDest - transform.position;
            m_Direction2target.y = 0.0f;
            m_Distance2target = m_Direction2target.magnitude;
            m_Direction2target.Normalize();

            _calc_ranges();

            if (m_Kooldown)
            {
                m_AttackTimer += Time.deltaTime;
                if (m_AttackTimer >= attackInterval)
                {
                    m_Kooldown = false;
                    m_AttackTimer = 0.0f;
                }
            }

            m_Move = Vector3.zero;
            {
                _calculate_path(ref m_Move);
                m_Character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);
                if (m_InFightZoneRange)
                {
                    m_Character.setMoveMode(TPCharacter.MovingMode.Strafe);
                }
            }
            _add_local_avoidance(ref m_Move);
#if SMOOTH_MOVEMENT
            m_Move = getAVG();
#endif

            m_Move.y = 0.0f;
            float mag = m_Move.magnitude;
            float curSpeed = m_Stats.DefaultMoveSpeed;
            if (mag < 0.25f) curSpeed = 0.0f;
            m_Move.Normalize();
            m_Move *= curSpeed;
            m_BodyLookDir = (m_InFightZoneRange ? m_Direction2target : m_Move);
            m_BodyLookDir = (m_InFightZoneRange ? m_Direction2target : m_Move);
            if (m_InFightZoneRange)
            {
                m_Move = Vector3.zero;
            }
            if (takingHit)
            {
                m_Move = Vector3.zero;
                m_BodyLookDir = Vector3.zero;
            }
            m_Character.move(m_Move, false, false, false, m_BodyLookDir, Vector3.zero);
        }

        /// <summary>
        /// check if character is falling from too high height
        /// </summary>
        private void _fallToDeathCheck()
        {
            float pJump = m_Character.animator.GetFloat("pJump");
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



#if SMOOTH_MOVEMENT
        
        // SMOOTHING NPC MOVEMENT
        Vector3[] avgMove = new Vector3[10];
        int avgIdx = 0;
        
        /// <summary>
        /// gets average value from last (variable 10) of move velocities
        /// </summary>
        /// <returns>averaged move velocity</returns>
        private Vector3 getAVG()
        {
            Vector3 avg = Vector3.zero;

            for (int i = 0; i < avgMove.Length; i++)
            {
                avg += avgMove[i];
            }
            avg /= avgMove.Length;
            return avg;
        }
#endif

        /// <summary>
        /// add local avoidance around npcs and player
        /// </summary>
        /// <param name="move">ref current move velocity</param>
        private void _add_local_avoidance(ref Vector3 move)
        {

            float repulseDistance = avoidanceDistance;

            Vector3 toPlayer = transform.position - m_Player.transform.position;
            Vector3 dir2player = toPlayer.normalized;
            float dist2player = toPlayer.magnitude;


            // avoidance of npcs
            float power = 8;
            Vector3 force = Vector3.zero;
            for (int i = 0; i < m_NPCManager.npcs.Length; i++)
            {
                NPCScript npc = m_NPCManager.npcs[i];
                if (this == npc) continue;
                if (npc.ragdollState != RagdollManager.RagdollState.Animated) continue;

                Vector3 toNpc = transform.position - npc.transform.position;
                Vector3 dir2npc = toNpc.normalized;
                float dist2npc = toNpc.magnitude;
                float val = Mathf.Clamp01(repulseDistance / dist2npc);
                val = Mathf.Pow(val, power);
                float strength = Mathf.Lerp(0.0f, 1.0f, val);
                force += dir2npc * strength;

            }
            // avoidance of player
            if (dist2player < 0.45f)
            {
                float val = Mathf.Clamp01(repulseDistance / dist2player);
                val = Mathf.Pow(val, power);
                float strength = Mathf.Lerp(0.0f, 1.0f, val) * 4f;
                force += dir2player * strength;
            }
            move += force;

#if SMOOTH_MOVEMENT
            avgMove[avgIdx] = move;
            avgIdx++;
            if (avgIdx >= avgMove.Length)
                avgIdx = 0;
#endif

        }

        /// <summary>
        /// calculate path
        /// </summary>
        /// <param name="moveDirection">ref current move direction</param>
        /// <returns></returns>
        private bool _calculate_path(ref Vector3 moveDirection)
        {
            int area = 1 << UnityEngine.AI.NavMesh.GetAreaFromName("Walkable");
            if (!UnityEngine.AI.NavMesh.CalculatePath(transform.position, m_CurrentDest, area, m_path))
            {
#if DEBUG_INFO
                Debug.LogWarning("Calculate path failed. " + " < " + this.ToString() + ">");
#endif
                m_InAttackRange = false;
                m_InFightZoneRange = false;
                return false;
            }

            if (m_path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
#if DEBUG_INFO
                Debug.LogWarning("Path incomplete." + " < " + this.ToString() + ">");
#endif
                m_InAttackRange = false;
                m_InFightZoneRange = false;
                return false;
            }

            if (m_path.corners.Length < 2)
            {
#if DEBUG_INFO
                Debug.LogWarning("Calculate path failed. Not enough corners." + " < " + this.ToString() + ">");
#endif
                m_InAttackRange = false;
                m_InFightZoneRange = false;
                return false;
            }

            Vector3 toTarget = m_path.corners[1] - transform.position;
            moveDirection = toTarget.normalized;
            _calc_ranges();

            return true;
        }

        /// <summary>
        /// end attack combo
        /// </summary>
        public void _end_attack_combo()
        {
#if DEBUG_INFO
            if (!m_Player)
            {
                Debug.LogError("object cannot be null ( PlayerControl ): " + " < " + this.ToString() + ">");
                return;
            }
            if (!m_Character)
            {
                Debug.LogError("object cannot be null ( TPCharacter ) : " + " < " + this.ToString() + ">");
                return;
            }
#endif



            if (m_Attack_combo_started)
            {
                m_Player.attack_end_notify(this, m_CurrentAttackType);
            }
            m_Character.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
            m_InCombat = false;
            m_Attack_combo_started = false;
            m_AttackStarted = false;
            m_Kooldown = true;
            m_AttackTimer = 0.0f;
            m_InAttackRange = false;
        }

        /// <summary>
        ///  calculate ranges to player
        /// </summary>
        private void _calc_ranges()
        {
            bool attacking = m_Animator.GetCurrentAnimatorStateInfo(0).IsName("AttackCombo");
            if (attacking) m_AttackHitDistance = m_Stats.currentWeaponReach * 1.5f;
            else m_AttackHitDistance = m_Stats.currentWeaponReach * 0.75f;

            bool isObstructed = _sweepTest(m_Direction2target);

            m_InAttackRange = m_Distance2target < m_AttackHitDistance && !isObstructed;

            if (m_InFightZoneRange) m_InFightZoneRange = m_Distance2target < (m_GuardZone.fightArea * 1.2f);
            else m_InFightZoneRange = m_Distance2target < m_GuardZone.fightArea;
        }

        /// <summary>
        /// check is attack path is clear
        /// </summary>
        /// <param name="direction">path direction</param>
        /// <returns>returns true if path is clear, otherwise false</returns>
        private bool _sweepTest(Vector3 direction)
        {
#if DEBUG_INFO
            if (!attackSweepBody)
            {
                Debug.LogError("object cannot be null." + " < " + this.ToString() + ">");
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
        /// notify npc of start attack
        /// </summary>
        /// <param name="attacker">attacker game character</param>
        /// <param name="hitType">attack hit type</param>
        public void attack_start_notify(IGameCharacter attacker, int hitType)
        {
        }

        /// <summary>
        /// notify npc to attack combo end 
        /// </summary>
        /// <param name="attacker">attacker transform</param>
        /// <param name="hitType">current attack type</param>
        public void attack_end_notify(IGameCharacter attacker, int hitType)
        {
            m_InCombat = false;
        }

        /// <summary>
        /// notify npc that he is attacked
        /// </summary>
        /// <param name="attacker">attacker transform</param>
        /// <param name="hitType">current hit type</param>
        /// <param name="damage">attackers damage</param>
        /// <param name="blocking">ref notify attacker that npc has blocked attack</param>
        /// <returns>success</returns>
        public bool attack_hit_notify(IGameCharacter attacker, int hitType, int attackSource, ref bool blocking,
            bool applyHitReaction = true,Vector3? hitVelocity = null, int[] hitParts = null)
        {
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized" + " < " + this.ToString() + ">");
                return false;
            }

            if (isDead) return false;

            if (!m_Blocking)
            {
                int rnd = Random.Range(0, blockOdds);
                m_Blocking = rnd == 0;
                if (m_Blocking)
                {
                    m_Animator.SetBool(HashIDs.BlockBool /*"pBlock"*/, true);
                    m_BlockTimer = 0.0f;
                }
            }

            
            
            _end_attack_combo();
            m_InCombat = true;
            blocking = m_Blocking;

            m_Character.fullStop();

            _startHitReaction(attacker, hitType, applyHitReaction, attackSource);

            int damageReceived = GameUtils.CalculateDamage(this, attacker, m_Blocking);
            if (m_Stats.currentHealth <= 0)
            {
                m_Player.attack_end_notify(this, m_CurrentAttackType);

                m_InCombat = false;
                m_Character.animator.SetBool(/*"pAttack1"*/HashIDs.Attack1Bool, false);
                m_Attack_combo_started = false;
                m_Kooldown = true;
                m_AttackTimer = 0.0f;
                attackSweepBody.gameObject.SetActive(false);
                m_Ragdoll.startRagdoll( hitParts, hitVelocity, hitVelocity * 0.025f);
            }
            if(!blocking)
                if (m_DamageUI)
                    m_DamageUI.setText("-" + damageReceived);

            return true;
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
            m_Character.animator.SetBool(/*"pDiveRoll"*/HashIDs.DiveRollBool, false);
            _end_attack_combo();
            m_Ragdoll.startRagdoll();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="item"></param>
        public InventoryItem  setNewItem(InventoryItem item)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="item"></param>
        public InventoryItem setSecondaryItem(InventoryItem item)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// start hit reaction animation
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="hitType"></param>
        /// <param name="applyHitReaction"></param>
        /// <param name="attackSource"></param>
        private void _startHitReaction(IGameCharacter attacker, int hitType, bool applyHitReaction, int attackSource)
        {
            if (hitType == -1) return;

            if (m_Blocking)
            {
                if (m_DefaultWeaponSounds)
                    GameUtils.PlayRandomClipAtPosition(m_DefaultWeaponSounds.blockSounds, transform.position);
                if (applyHitReaction)
                {
                    _end_attack_combo();
                    Vector3 toAttacker = attacker.stats.transform.position - transform.position;
                    m_BodyLookDir = toAttacker;
                    m_BodyLookDir.y = 0.0f;
                    transform.rotation = Quaternion.LookRotation(m_BodyLookDir.normalized);


                    string stateName = "BlockHit" + hitType;
                    int stateID = Animator.StringToHash(stateName);

                    if (m_Character.animator.HasState(0, stateID))
                        m_Character.animator.CrossFade(stateID, 0.05f, 0, 0.0f);
                    else
                    {
                        Debug.LogWarning("state '" + stateName + "' dont exist");
                        m_Character.animator.CrossFade("BlockHit0", 0.05f, 0, 0.0f);
                    }
                }
            }
            else
            {
                if (applyHitReaction)
                {
                    _end_attack_combo();
                    Vector3 toAttacker = attacker.stats.transform.position - transform.position;
                    m_BodyLookDir = toAttacker;
                    m_BodyLookDir.y = 0.0f;
                    transform.rotation = Quaternion.LookRotation(m_BodyLookDir.normalized);

                    string stateName = "TakeHit" + hitType;
                    int stateID = Animator.StringToHash(stateName);
                    if (m_Character.animator.HasState(0, stateID))
                        m_Character.animator.CrossFade(stateID, 0.05f, 0, 0.0f);
                    else
                    {
                        Debug.LogWarning("state '" + stateName + "' dont exist");
                        m_Character.animator.CrossFade("TakeHit", 0.05f, 0, 0.0f);
                    }
                }
            }
        }


        // animation events --------------------------------------

        /// <summary>
        /// start ragdoll event
        /// </summary>
        /// <param name="e">animation event info class</param>
        void StartRagdollEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (e.animatorClipInfo.weight < 0.9) return;


            m_Character.animator.SetBool(/*"pDiveRoll"*/HashIDs.DiveRollBool, false);
            _end_attack_combo();
            m_Ragdoll.startRagdoll();
        }

        /// <summary>
        /// attack combo end event
        /// </summary>
        /// <param name="e">animation event info class</param>
        void AttackComboEndEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            _end_attack_combo();
        }

        /// <summary>
        /// disable physics event
        /// </summary>
        /// <param name="e">animation event info class</param>
        void DisablePhysicsEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (e.animatorClipInfo.weight < 0.9) return;
            m_Character.disablePhysics();
        }

        /// <summary>
        /// attack start event
        /// </summary>
        /// <param name="e">animation event info class</param>
        private void AttackStartEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            
            m_AttackStarted = true;
            m_CurrentAttackType = e.intParameter;
            m_Character.turnTransformToDirection(m_Direction2target);
            m_Player.attack_start_notify(this, m_CurrentAttackType);
        }

        /// <summary>
        /// attack hit event
        /// </summary>
        /// <param name="e">animation event info class</param>
        void AttackHitEvent(AnimationEvent e)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (!m_AttackStarted) return;
            m_AttackStarted = false;
            if (e.animatorClipInfo.weight < 0.9) return;

            if (m_DefaultWeaponSounds)
                GameUtils.PlayRandomClipAtPosition(m_DefaultWeaponSounds.attackSwingSounds, transform.position);
            if (m_Distance2target < m_AttackHitDistance)
            {
                bool block = false;
                bool success = m_Player.attack_hit_notify(this, m_CurrentAttackType, e.intParameter, ref block);
                if (success)
                {
                    if (!block)
                    {
                        if (m_DefaultWeaponSounds)
                            GameUtils.PlayRandomClipAtPosition(m_DefaultWeaponSounds.attackHitSounds, transform.position);
                    }
                }
            }
        }
    }
}
