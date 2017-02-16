// © 2016 Mario Lelas
using System.Collections.Generic;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// delegate used for notifying attack hit
    /// </summary>
    /// <param name="attacker">attacking character。 current characte is attacked by attacker</param>
    /// <param name="attackType">attack type</param>
    /// <param name="attackSource">attack source ( primary or secondary weapon) - or other</param>
    /// <param name="blocking">ref - assign true if attack is blocked</param>
    /// <param name="applyHitReaction">should hit reaction animation be applied?</param>
    /// <param name="hitVelocity">hitVelocity to be applied on ragdoll start</param>
    /// <param name="hitParts">hit body part/s to be applied on ragdoll start</param>
    /// <returns>success of attack</returns>
    public delegate bool OnAttackHitDelegate(IGameCharacter attacker, int attackType, int attackSource, ref bool blocking, 
        bool applyHitReaction = true, Vector3? hitVelocity = null, int[] hitParts = null);

    /// <summary>
    /// delegate used notifying of attacker and attack type
    /// </summary>
    /// <param name="attacker">attacking character</param>
    /// <param name="attackType">attack type</param>
    public delegate void NotifyDelegate(IGameCharacter attacker, int attackType);

    /// <summary>
    /// base player class
    /// controls camera, head ik , replacing animation clips
    /// Player和TPCharacter不是继承关系，而是平级引用关系
    /// </summary>
    [RequireComponent(typeof(TPCharacter), typeof(RagdollManager))]
    public abstract class Player : MonoBehaviour
    {
        /// <summary>
        /// player's camera
        /// </summary>
        public BaseCamera m_Camera;                         // player's camera

        /// <summary>
        /// enable / disable looking towards camera
        /// </summary>
        [HideInInspector]
        public bool lookTowardsCamera = false;              // look towards camera direction flag

        protected TPCharacter m_Character;                  // third person character reference
        protected RagdollManager m_Ragdoll;                 // ragdoll manager reference
        protected TriggerManagement m_Triggers;             // trigger manager reference
        protected LegsIK m_LegsIKScript;                    // legs IK class reference
        
        


        protected bool m_DisableInput = false;            // disable input flag
        protected bool m_Strafing = false;                // player strafe flag
        protected bool m_Initialized = false;             // is component initialized ?

        protected Vector3 m_CurrentHeadPos = Vector3.zero;    // current head look at position 当前头看向哪个方向
        protected Vector3 m_HeadStartPos, m_HeadEndPos;
        // head start and end lerp position for smooth transition
        //转头时时，从m_HeadStartPos 转向m_HeadEndPos
        protected float m_HeadSwitchTime = 0.0f;              // head lerp time
        protected float m_HeadSwitchMaxTime = 0.25f;          // head lerp max time
        protected float m_HeadLookSpeed = 1.0f;               // head lerp speed
        protected bool m_LerpHeadPosition = false;           
        // lerp head look at different position flag 当前是否正在砖头当中
        protected bool m_Switch2CameraLook = false;           // switch head to look towards camera flag ( lerp to )
        protected bool m_LookTowardsCamera = true;            // look to camera direction flag
        protected TPCharacter.IKMode lookIkMode = TPCharacter.IKMode.Head; // current head ik mode


        protected List<AnimatorStateInfo> m_SavedCurrentStates =
        new List<AnimatorStateInfo>();                                          // saved animator states on disable
        protected Dictionary<AnimatorControllerParameter, object> m_SavedParams =
                new Dictionary<AnimatorControllerParameter, object>();                  // saved animator parameters on disable
        private List<AnimationClipReplacementInfo> m_OriginalClips =
        new List<AnimationClipReplacementInfo>();                       // original animation character clips


        /// <summary>
        /// On attacked delegate. Fires upon being attacked ( hit ).
        /// </summary>
        public OnAttackHitDelegate OnAttackHit = null;

        /// <summary>
        /// On attack end notify delegate. Fires upon attack end
        /// </summary>
        public NotifyDelegate OnAttackEndNotify = null;   
         
        /// <summary>
        /// On attack start delegate. Fires upon attack starts
        /// </summary>
        public NotifyDelegate OnAttackStartNotify = null;   


        


        /// <summary>
        /// gets TPCharacter reference
        /// </summary>
        public TPCharacter character { get { return m_Character; } }

        /// <summary>
        /// is player triggered some action
        /// </summary>
        public bool triggerActive { get { return m_Triggers .triggerActive; } }

        /// <summary>
        /// get reference to animator component
        /// </summary>
        public Animator animator
        {
            get
            {
#if DEBUG_INFO
                if(!m_Character )
                {
                    Debug.LogError("object cannot be null. " + " < " + this.ToString() + ">");
                    return null;
                }
#endif 
                return m_Character.animator;
            }
        }

        /// <summary>
        /// gets RagdollManager reference
        /// </summary>
        public RagdollManager ragdoll { get { return m_Ragdoll; } }

        /// <summary>
        /// gets TriggerManager reference
        /// </summary>
        public TriggerManagement triggers { get { return m_Triggers; } }

        /// <summary>
        /// gets LegsIK reference
        /// </summary>
        public LegsIK legsIK { get { return m_LegsIKScript; } }

        /// <summary>
        /// gets and sets disable input flag
        /// </summary>
        public bool disableInput { get { return m_DisableInput; } set { m_DisableInput = value; } }

        /// <summary>
        /// gets and sets player strafing movement mode
        /// </summary>
        public bool strafing
        {
            get { return m_Strafing; }
            set
            {
                m_Strafing = value;
                if (m_Strafing) m_Character.setMoveMode(TPCharacter.MovingMode.Strafe);
                else m_Character.setMoveMode(TPCharacter.MovingMode.RotateToDirection);
            }
        }





        /// <summary>
        /// initialize component
        /// </summary>
        public virtual void initialize()
        {
            if (m_Initialized) return;

            m_Character = GetComponent<TPCharacter>();
            if (!m_Character) { Debug.LogError("Cannot find 'TPCharacter' component. " + " < " + this.ToString() + ">"); return; }
            m_Character.initialize();

            m_Ragdoll = GetComponent<RagdollManager>();
            if(!m_Ragdoll) { Debug.LogError("Cannot find 'RagdollManager' component. " + " < " + this.ToString() + ">"); return; }
            m_Ragdoll.initialize();

            m_Triggers = GetComponent<TriggerManagement>();
            if(!m_Triggers) { Debug.LogError("Cannot find 'TriggerManagement' component. " + " < " + this.ToString() + ">"); return; }
            m_Triggers.initialize();

            LegsIK legsik = GetComponent<LegsIK>();
            if(legsik)
                if (legsik.enabled)
                    m_LegsIKScript = legsik;
            if(m_LegsIKScript )
                m_LegsIKScript.initialize();

            m_Initialized = true;
        }



        /// <summary>
        /// Control player
        /// </summary>
        /// <param name="horiz">horizonal axis value</param>
        /// <param name="vert">vertical axis value</param>
        /// <param name="jump">jump flag</param>
        /// <param name="runToggle">toggle walk / run mode</param>
        /// <param name="dive">dive roll flag</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="bodyLookDirection">body look direction</param>
        /// <param name="diveDirection">dive direction</param>
        public abstract void control(float horiz, float vert, bool jump, bool runToggle, bool dive, bool crouch, 
            Vector3? bodyLookDirection = null, Vector3? diveDirection = null, float? side = null);

        /// <summary>
        /// Control player
        /// </summary>
        /// <param name="moveVelocity">move velocity</param>
        /// <param name="jump">jump flag</param>
        /// <param name="runToggle">toggle walk / run mode</param>
        /// <param name="dive">dive roll flag</param>
        /// <param name="crouch">crouch flag</param>
        /// <param name="bodyLookDirection">body look direction</param>
        /// <param name="diveDirection">dive direction</param>
        public virtual void control(Vector3 moveVelocity, bool jump, bool runToggle, bool dive, bool crouch, 
            Vector3? bodyLookDirection = null, Vector3? diveDirection = null, float? side = null)
        {
#if DEBUG_INFO
            if(!m_Initialized )
            {
                Debug.LogError("Component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_LerpHeadPosition)
            {
                if (m_Switch2CameraLook)
                {
                    m_HeadEndPos = transform.position + transform.forward * 100.0f;
                }

                m_HeadSwitchTime += Time.deltaTime * m_HeadLookSpeed;
                float lValue = m_HeadSwitchTime / m_HeadSwitchMaxTime;
                lValue = Mathf.Clamp01(lValue);
                m_CurrentHeadPos = Vector3.Lerp(m_HeadStartPos, m_HeadEndPos, lValue);
                if (m_HeadSwitchMaxTime < m_HeadSwitchTime)
                {
                    m_LerpHeadPosition = false;
                    if (m_Switch2CameraLook)
                    {
                        m_Switch2CameraLook = false;
                        m_LookTowardsCamera = true;
                        noLookIK();
                    }
                }
            }
            Vector3 bodyLookDir = bodyLookDirection.HasValue ? bodyLookDirection.Value : moveVelocity;
            m_Character.move(moveVelocity, crouch, jump, dive, bodyLookDir, m_CurrentHeadPos, side, diveDirection);
        }

        /// <summary>
        /// Switch player head look at position. Pass null to look at camera forward direction
        /// </summary>
        /// <param name="pos">look at position</param>
        public virtual void switchHeadLookPos(Vector3? pos)
        {
#if DEBUG_INFO
            if (!m_Character)
            {
                Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (pos.HasValue)
            {
                m_LookTowardsCamera = false;
                m_Switch2CameraLook = false;
                m_LerpHeadPosition = true;
                m_HeadStartPos = m_CurrentHeadPos;
                m_HeadEndPos = pos.Value;
                m_HeadSwitchTime = 0.0f;
                m_HeadSwitchMaxTime = 0.2f;
            }
            else
            {
                if (m_LookTowardsCamera) return;


                m_Switch2CameraLook = true;
                m_LerpHeadPosition = true;
                m_HeadStartPos = m_CurrentHeadPos;
                if (lookTowardsCamera)
                {
                    m_HeadEndPos = m_Camera.transform != null
                          ? transform.position + m_Camera.transform.forward * 100
                          : transform.position + transform.forward * 100;
                    m_HeadSwitchMaxTime = 2.0f;
                }
                else
                {
                    m_HeadEndPos = transform.position + transform.forward * 100.0f;
                    m_HeadSwitchMaxTime = 1.0f;
                }

                m_HeadSwitchTime = 0.0f;

            }
        }

        /// <summary>
        /// disable head ik
        /// </summary>
        public void noLookIK()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (m_Character.getIKMode() == TPCharacter.IKMode.Head ||
                m_Character.getIKMode() == TPCharacter.IKMode.Waist)
                m_Character.setIKMode(TPCharacter.IKMode.ToNone);
        }


        /// <summary>
        /// notify attack combo ended
        /// </summary>
        /// <param name="attacker">attacker transform</param>
        /// <param name="hitType">attack hit type</param>
        public virtual void attack_end_notify(IGameCharacter attacker, int hitType)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized: " +" < " + this.ToString() + ">");
                return;
            }
#endif
            if (OnAttackEndNotify != null)
                OnAttackEndNotify(attacker, hitType );
        }

        /// <summary>
        /// notify attack move start
        /// </summary>
        /// <param name="attacker">attacker transform</param>
        /// <param name="hitType">attack hit type</param>
        public virtual void attack_start_notify(IGameCharacter attacker, int hitType)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized: " + " < " + this.ToString() + ">");
                return;
            }
#endif
            if (OnAttackStartNotify != null)
                OnAttackStartNotify(attacker, hitType);
        }

        /// <summary>
        /// notify npc that he is punched
        /// </summary>
        /// <param name="attacker">attacker transform</param>
        /// <param name="hitType">current attack type</param>
        /// <param name="damage">attacker damage</param>
        /// <param name="blocking">assigned to true if npc has blocked attack</param>
        /// <returns>success</returns>
        public virtual bool attack_hit_notify(IGameCharacter attacker,int hitType,int damage, ref bool blocked)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized: " + " < " + this.ToString() + ">");
                return false;
            }
#endif
            if (OnAttackHit != null)
                return OnAttackHit(attacker, hitType, damage, ref blocked);
            return false;
        }


#region Replace Animation Clips


        /// <summary>
        /// collect default animation clips into list
        /// </summary>
        private void _collectAnimationClips()
        {
#if DEBUG_INFO
            if (!animator)
            {
                Debug.LogError("object cannot be null < " + this.ToString() + " >");
                return;
            }
#endif

            RuntimeAnimatorController myController = animator.runtimeAnimatorController;
            AnimatorOverrideController myAnimatorOverride = new AnimatorOverrideController();
            myAnimatorOverride.name = "PlayerOverrider";
            myAnimatorOverride.runtimeAnimatorController = myController;
            animator.runtimeAnimatorController = myAnimatorOverride;
            for (int i = 0; i < myAnimatorOverride.animationClips.Length; i++)
            {
                AnimationClipReplacementInfo acri = new AnimationClipReplacementInfo();
                acri.original_name = myAnimatorOverride.animationClips[i].name;
                acri.current_name = myAnimatorOverride.animationClips[i].name;
                acri.clip = myAnimatorOverride.animationClips[i];
                acri.orig_hash =
                    Animator.StringToHash(acri.original_name);
                m_OriginalClips.Add(acri);
            }

        }

        /// <summary>
        /// get original clip info from original clip name
        /// </summary>
        /// <param name="original_name">name of animation clip</param>
        /// <returns></returns>
        private AnimationClipReplacementInfo _getOriginalClipInfoByName(string original_name)
        {
#if DEBUG_INFO
            if (m_OriginalClips == null)
            {
                Debug.LogError("object cannot be null <" + this.ToString() + ">");
                return null;
            }
#endif
            for (int j = 0; j < m_OriginalClips.Count; j++)
            {
                if (original_name == m_OriginalClips[j].original_name)
                {
                    return m_OriginalClips[j];
                }
            }
            return null;
        }

        /// <summary>
        /// get original clip info from original clip name hash
        /// </summary>
        /// <param name="original_name_hash">hash of animation clip</param>
        /// <returns></returns>
        private AnimationClipReplacementInfo _getOriginalClipInfoByHash(int original_name_hash)
        {
#if DEBUG_INFO
            if (m_OriginalClips == null)
            {
                Debug.LogError("object cannot be null <" + this.ToString() + ">");
                return null;
            }
#endif
            for (int j = 0; j < m_OriginalClips.Count; j++)
            {
                if (original_name_hash == m_OriginalClips[j].orig_hash)
                {
                    return m_OriginalClips[j];
                }
            }
            return null;
        }

        /// <summary>
        /// change animation clips on weapon
        /// </summary>
        /// <param name="meleeItem">melee item to change clips</param>
        public void setWeaponClips(MeleeWeaponItem meleeItem)
        {
#if DEBUG_INFO
            if (!animator) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif
            if (!meleeItem)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null <" + this.ToString() + ">");
#endif
                return;
            }

            Animator anim = animator;
            saveAnimatorStates();

            if(!(anim.runtimeAnimatorController  is AnimatorOverrideController ))
            {
                _collectAnimationClips();
            }

            AnimatorOverrideController myCurrentOverrideController = anim.runtimeAnimatorController as AnimatorOverrideController;
            RuntimeAnimatorController myOriginalController = myCurrentOverrideController.runtimeAnimatorController;
            myCurrentOverrideController.runtimeAnimatorController = null;
            AnimatorOverrideController myNewOverrideController = new AnimatorOverrideController();
            myNewOverrideController.runtimeAnimatorController = myOriginalController;
            myNewOverrideController.name = "PlayerOverrider" + meleeItem.name;

            for (int i = 0; i < m_OriginalClips.Count; i++)
            {
                if (m_OriginalClips[i].changed)
                {

                    string orig_name = m_OriginalClips[i].original_name;
                    AnimationClip orig_clip = m_OriginalClips[i].clip;
                    myNewOverrideController[orig_name/*current_name*/] = orig_clip;
                    m_OriginalClips[i].current_name = m_OriginalClips[i].original_name;
                    m_OriginalClips[i].changed = false;
                }
            }

            // SET REPLACEMENT CLIPS
            if (meleeItem.replacement_clips != null)
            {
                for (int i = 0; i < meleeItem.replacement_clips.Count; i++)
                {
                    if (meleeItem.replacement_clips[i] == null) continue;
                    if (meleeItem.replacement_clips[i].clip == null) continue;


                    AnimationClip new_clip = meleeItem.replacement_clips[i].clip;

                    int origHash = meleeItem.replacement_clips[i].orig_hash;
                    if (origHash == -1)
                    {
                        Debug.LogWarning("Hash not created on clip: " + meleeItem.replacement_clips[i].original_name);
                        origHash = Animator.StringToHash(meleeItem.replacement_clips[i].original_name);
                    }

                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(origHash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = meleeItem.replacement_clips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + meleeItem.gameObject.name);
                    }
#endif
                }
            }

            // SET ATTACK CLIPS
            if (meleeItem.attackClip.clip != null)
            {
                AnimationClip anew_clip = meleeItem.attackClip.clip;

                int origHash = meleeItem.attackClip.orig_hash;
                if (origHash == -1)
                {
                    Debug.LogWarning("Hash not created on clip: " + meleeItem.attackClip.original_name);
                    origHash = Animator.StringToHash(meleeItem.attackClip.original_name);
                }

                //AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByName(aoriginal_name);
                AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByHash(origHash);

                if (aclipInfo != null && aclipInfo.clip != null)
                {
                    aclipInfo.changed = true;
                    myNewOverrideController[aclipInfo.original_name /*current_name*/] = anew_clip;
                    aclipInfo.current_name = anew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string aoriginal_name = meleeItem.attackClip.original_name;
                    Debug.LogWarning("cannot find clip: " + aoriginal_name + " on " + this.gameObject.name + " item: " + meleeItem.gameObject.name);
                }
#endif
            }

            // SET BLOCK STANCE CLIP

            if (meleeItem.blockClip.clip != null)
            {
                AnimationClip bnew_clip = meleeItem.blockClip.clip;

                int origHash = meleeItem.blockClip.orig_hash;
                if (origHash == -1)
                {
                    Debug.LogWarning("Hash not created on clip: " + meleeItem.blockClip.original_name);
                    origHash = Animator.StringToHash(meleeItem.blockClip.original_name);
                }

                //AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByName(boriginal_name);
                AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByHash(origHash);

                if (bclipInfo != null && bclipInfo.clip != null)
                {

                    bclipInfo.changed = true;
                    myNewOverrideController[bclipInfo.original_name /*current_name*/] = bnew_clip;
                    bclipInfo.current_name = bnew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string boriginal_name = meleeItem.blockClip.original_name;
                    Debug.LogWarning("cannot find clip: " + boriginal_name + " on " + this.gameObject.name + " item: " + meleeItem.gameObject.name);
                }
#endif
            }

            // SET BLOCK HIT CLIPS
            if (meleeItem.blockHitClips != null)
            {
                for (int i = 0; i < meleeItem.blockHitClips.Count; i++)
                {
                    if (meleeItem.blockHitClips[i] == null) continue;
                    if (meleeItem.blockHitClips[i].clip == null) continue;
                    AnimationClip new_clip = meleeItem.blockHitClips[i].clip;

                    int origHash = meleeItem.blockHitClips[i].orig_hash;
                    if (origHash == -1)
                    {
                        Debug.LogWarning("Hash not created on clip: " + meleeItem.blockHitClips[i].original_name);
                        origHash = Animator.StringToHash(meleeItem.blockHitClips[i].original_name);
                    }

                    //AnimationClipReplacementInfo clipInfo = getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(origHash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {

                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = meleeItem.blockHitClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + meleeItem.gameObject.name);
                        continue;
                    }
#endif
                }
            }

            // SET LOCOMOTION CLIPS
            if (meleeItem.locomotionClips != null)
            {
                for (int i = 0; i < meleeItem.locomotionClips.Count; i++)
                {
                    if (meleeItem.locomotionClips[i] == null) continue;
                    if (meleeItem.locomotionClips[i].clip == null) continue;

                    AnimationClip new_clip = meleeItem.locomotionClips[i].clip;

                    int origHash = meleeItem.locomotionClips[i].orig_hash;
                    if (origHash == -1)
                    {
                        Debug.LogWarning("Hash not created on clip: " + meleeItem.locomotionClips[i].original_name);
                        origHash = Animator.StringToHash(meleeItem.locomotionClips[i].original_name);
                    }

                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(origHash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = meleeItem.locomotionClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + ", item: " + meleeItem.gameObject.name);
                    }
#endif
                }
            }
            

            anim.runtimeAnimatorController = myNewOverrideController;
            UnityEngine.Object.Destroy(myCurrentOverrideController);

            resetAnimatorStates();
        }

        /// <summary>
        /// change animation clips on shield
        /// </summary>
        /// <param name="shieldItem">shield item to change clips</param>
        public void setShieldClips(ShieldItem shieldItem)
        {
#if DEBUG_INFO
            if (!animator) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif
            if (!shieldItem)
            {
#if DEBUG_INFO
                Debug.LogError("Shield item missing! < " + this.ToString() + ">");
#endif
                return;
            }

            Animator anim = animator;
            saveAnimatorStates();

            if (!(anim.runtimeAnimatorController is AnimatorOverrideController))
            {
                _collectAnimationClips();
            }

            AnimatorOverrideController myCurrentOverrideController = anim.runtimeAnimatorController as AnimatorOverrideController;
            RuntimeAnimatorController myOriginalController = myCurrentOverrideController.runtimeAnimatorController;
            myCurrentOverrideController.runtimeAnimatorController = null;
            AnimatorOverrideController myNewOverrideController = new AnimatorOverrideController();
            myNewOverrideController.runtimeAnimatorController = myOriginalController;
            myNewOverrideController.name = "PlayerOverrider" + shieldItem.name;

            for (int i = 0; i < m_OriginalClips.Count; i++)
            {
                if (m_OriginalClips[i].changed)
                {

                    string orig_name = m_OriginalClips[i].original_name;
                    AnimationClip orig_clip = m_OriginalClips[i].clip;
                    myNewOverrideController[orig_name/*current_name*/] = orig_clip;
                    m_OriginalClips[i].current_name = m_OriginalClips[i].original_name;
                    m_OriginalClips[i].changed = false;
                }
            }

            if (shieldItem.replacement_clips != null)
            {
                for (int i = 0; i < shieldItem.replacement_clips.Count; i++)
                {
                    if (shieldItem.replacement_clips[i] == null) continue;
                    if (shieldItem.replacement_clips[i].clip == null) continue;

                    AnimationClip new_clip = shieldItem.replacement_clips[i].clip;

                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(shieldItem.replacement_clips[i].orig_hash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.current_name] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = shieldItem.replacement_clips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                    }
#endif
                }
            }

            if (shieldItem.attackClip.clip != null)
            {

                AnimationClip anew_clip = shieldItem.attackClip.clip;

                //AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByName(aoriginal_name);
                AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByHash(shieldItem.attackClip.orig_hash);

                if (aclipInfo != null && aclipInfo.clip != null)
                {
                    aclipInfo.changed = true;
                    myNewOverrideController[aclipInfo.original_name /*current_name*/] = anew_clip;
                    aclipInfo.current_name = anew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string aoriginal_name = shieldItem.attackClip.original_name;
                    Debug.LogWarning("cannot find clip: " + aoriginal_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                }
#endif
            }

            if (shieldItem.blockClip != null)
            {
                AnimationClip bnew_clip = shieldItem.blockClip.clip;

                //AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByName(boriginal_name);
                AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByHash(shieldItem.blockClip.orig_hash);

                if (bclipInfo != null && bclipInfo.clip != null)
                {
                    bclipInfo.changed = true;
                    myNewOverrideController[bclipInfo.original_name /*current_name*/] = bnew_clip;
                    bclipInfo.current_name = bnew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string boriginal_name = shieldItem.blockClip.original_name;
                    Debug.LogWarning("cannot find clip: " + boriginal_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                }
#endif
            }

            if (shieldItem.blockHitClips != null)
            {
                for (int i = 0; i < shieldItem.blockHitClips.Count; i++)
                {
                    if (shieldItem.blockHitClips[i] == null) continue;
                    if (shieldItem.blockHitClips[i].clip == null) continue;


                    AnimationClip new_clip = shieldItem.blockHitClips[i].clip;

                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(shieldItem.blockHitClips[i].orig_hash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name/*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = shieldItem.blockHitClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                    }
#endif
                }
            }

            if (shieldItem.locomotionClips != null)
            {
                for (int i = 0; i < shieldItem.locomotionClips.Count; i++)
                {
                    if (shieldItem.locomotionClips[i] == null) continue;
                    if (shieldItem.locomotionClips[i].clip == null) continue;


                    AnimationClip new_clip = shieldItem.locomotionClips[i].clip;

                    //AnimationClipReplacementInfo clipInfo = getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(shieldItem.locomotionClips[i].orig_hash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = shieldItem.locomotionClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                    }
#endif
                }
            }
            
            anim.runtimeAnimatorController = myNewOverrideController;
            UnityEngine.Object.Destroy(myCurrentOverrideController);


            resetAnimatorStates();
        }

        /// <summary>
        /// change animation clips on weapon & shield
        /// and set locomotion holding two items
        /// </summary>
        /// <param name="meleeItem">weapon1H item to change clips</param>
        /// <param name="shieldItem">shield item to change clips</param>
        /// <param name="dualItemLocomotion">locomotion clips</param>
        public void setWeapon1HShieldClips(MeleeWeaponItem meleeItem, ShieldItem shieldItem, List<AnimationClipReplacementInfo > dualItemLocomotion)
        {
#if DEBUG_INFO
            if (!animator) { Debug.LogError("object cannot be null<" + this.ToString() + ">"); return; }
#endif
            if (!shieldItem)
            {
#if DEBUG_INFO
                Debug.LogError("Shield item missing! < " + this.ToString() + ">");
#endif
                return;
            }
            if (!meleeItem)
            {
#if DEBUG_INFO
                Debug.LogError("Weapon1H item missing! < " + this.ToString() + ">");
#endif
                return;
            }

            Animator anim = animator;
            saveAnimatorStates();

            if (!(anim.runtimeAnimatorController is AnimatorOverrideController))
            {
                _collectAnimationClips();
            }

            AnimatorOverrideController myCurrentOverrideController = anim.runtimeAnimatorController as AnimatorOverrideController;
            RuntimeAnimatorController myOriginalController = myCurrentOverrideController.runtimeAnimatorController;
            myCurrentOverrideController.runtimeAnimatorController = null;
            AnimatorOverrideController myNewOverrideController = new AnimatorOverrideController();
            myNewOverrideController.runtimeAnimatorController = myOriginalController;
            myNewOverrideController.name = "PlayerOverriderWeaponShield";

            for (int i = 0; i < m_OriginalClips.Count; i++)
            {
                if (m_OriginalClips[i].changed)
                {
                    string orig_name = m_OriginalClips[i].original_name;
                    AnimationClip orig_clip = m_OriginalClips[i].clip;
                    myNewOverrideController[orig_name/*current_name*/] = orig_clip;
                    m_OriginalClips[i].current_name = m_OriginalClips[i].original_name;
                    m_OriginalClips[i].changed = false;
                }
            }

            if (meleeItem.attackClip.clip != null)
            {

                AnimationClip anew_clip = meleeItem.attackClip.clip;

                //AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByName(aoriginal_name);
                AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByHash(meleeItem.attackClip.orig_hash);

                if (aclipInfo != null && aclipInfo.clip != null)
                {

                    aclipInfo.changed = true;
                    myNewOverrideController[aclipInfo.original_name /*current_name*/] = anew_clip;
                    aclipInfo.current_name = anew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string aoriginal_name = meleeItem.attackClip.original_name;
                    Debug.LogWarning("cannot find clip: " + aoriginal_name + " on " + this.gameObject.name + " item: " + meleeItem.gameObject.name);
                }
#endif
            }

            if (dualItemLocomotion != null)
            {
                for (int i = 0; i < dualItemLocomotion.Count; i++)
                {
                    if (dualItemLocomotion[i] == null) continue;
                    if (dualItemLocomotion[i].clip == null) continue;



                    AnimationClip new_clip = dualItemLocomotion[i].clip;


                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(dualItemLocomotion[i].orig_hash);


                    if (clipInfo != null && clipInfo.clip != null)
                    {

                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = dualItemLocomotion[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + meleeItem.gameObject.name);
                    }
#endif
                }
            }

            if (shieldItem.blockClip != null)
            {

                AnimationClip bnew_clip = shieldItem.blockClip.clip;


                //AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByName(boriginal_name);
                AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByHash(shieldItem.blockClip.orig_hash);


                if (bclipInfo != null && bclipInfo.clip != null)
                {
                    bclipInfo.changed = true;
                    myNewOverrideController[bclipInfo.original_name /*current_name*/] = bnew_clip;
                    bclipInfo.current_name = bnew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string boriginal_name = shieldItem.blockClip.original_name;
                    Debug.LogWarning("cannot find clip: " + boriginal_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                }
#endif
            }

            if (shieldItem.blockHitClips != null)
            {
                for (int i = 0; i < shieldItem.blockHitClips.Count; i++)
                {
                    if (shieldItem.blockHitClips[i] == null) continue;
                    if (shieldItem.blockHitClips[i].clip == null) continue;



                    AnimationClip new_clip = shieldItem.blockHitClips[i].clip;


                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(shieldItem.blockHitClips[i].orig_hash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = shieldItem.blockHitClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name + " item: " + shieldItem.gameObject.name);
                    }
#endif
                }
            }

            anim.runtimeAnimatorController = myNewOverrideController;
            UnityEngine.Object.Destroy(myCurrentOverrideController);


            resetAnimatorStates();
        }

        /// <summary>
        /// set animation clips
        /// </summary>
        /// <param name="attackClip">attack clip info</param>
        /// <param name="blockStanceClip">blocking stance clip info</param>
        /// <param name="blockHitClips">list of block hit clip infos</param>
        /// <param name="locomotionClips">list of locomotion clip infos</param>
        /// <param name="replacementClips">list of all clip infos</param>
        public void setClips
            (
            AnimationClipReplacementInfo attackClip,
            AnimationClipReplacementInfo blockStanceClip,
            List<AnimationClipReplacementInfo> blockHitClips,
            List<AnimationClipReplacementInfo> locomotionClips,
            List<AnimationClipReplacementInfo> replacementClips
            )
        {
#if DEBUG_INFO
            if(m_OriginalClips == null) { Debug.LogError("object cannot be null");return; }
#endif
            if(attackClip == null)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null");
#endif
                return;
            }
            if (blockStanceClip == null)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null");
#endif
                return;
            }
            if (blockHitClips == null)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null");
#endif
                return;
            }
            if (locomotionClips == null)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null");
#endif
                return;
            }
            if (replacementClips == null)
            {
#if DEBUG_INFO
                Debug.LogError("object cannot be null");
#endif
                return;
            }

            Animator anim = animator;

            saveAnimatorStates();


            if (!(anim.runtimeAnimatorController is AnimatorOverrideController))
            {
                _collectAnimationClips();
            }

            AnimatorOverrideController myCurrentOverrideController = anim.runtimeAnimatorController as AnimatorOverrideController;
            RuntimeAnimatorController myOriginalController = myCurrentOverrideController.runtimeAnimatorController;
            myCurrentOverrideController.runtimeAnimatorController = null;
            AnimatorOverrideController myNewOverrideController = new AnimatorOverrideController();
            myNewOverrideController.runtimeAnimatorController = myOriginalController;
            myNewOverrideController.name = "PlayerOverriderWeaponDual";

            for (int i = 0; i < m_OriginalClips.Count; i++)
            {
                if (m_OriginalClips[i].changed)
                {

                    string orig_name = m_OriginalClips[i].original_name;
                    AnimationClip orig_clip = m_OriginalClips[i].clip;
                    myNewOverrideController[orig_name/*current_name*/] = orig_clip;
                    m_OriginalClips[i].current_name = m_OriginalClips[i].original_name;
                    m_OriginalClips[i].changed = false;
                }
            }

            if (attackClip.clip != null)
            {
                AnimationClip anew_clip = attackClip.clip;
                int origHash = attackClip.orig_hash;
                if (origHash == -1)
                {
                    Debug.LogWarning("Hash not created on clip: " + attackClip.original_name);
                    origHash = Animator.StringToHash(attackClip.original_name);
                }

                //AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByName(aoriginal_name);
                AnimationClipReplacementInfo aclipInfo = _getOriginalClipInfoByHash(origHash);

                if (aclipInfo != null && aclipInfo.clip != null)
                {
                    aclipInfo.changed = true;
                    myNewOverrideController[aclipInfo.original_name /*current_name*/] = anew_clip;
                    aclipInfo.current_name = anew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string aoriginal_name = attackClip.original_name;
                    Debug.LogWarning("cannot find clip: " + aoriginal_name + " on " + this.gameObject.name);
                }
#endif
            }

            if (blockStanceClip.clip != null)
            {
                AnimationClip bnew_clip = blockStanceClip.clip;

                int origHash = blockStanceClip.orig_hash;
                if (origHash == -1)
                {
                    Debug.LogWarning("Hash not created on clip: " + blockStanceClip.original_name);
                    origHash = Animator.StringToHash(blockStanceClip.original_name);
                }

                //AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByName(boriginal_name);
                AnimationClipReplacementInfo bclipInfo = _getOriginalClipInfoByHash(origHash);

                if (bclipInfo != null && bclipInfo.clip != null)
                {

                    bclipInfo.changed = true;
                    myNewOverrideController[bclipInfo.original_name /*current_name*/] = bnew_clip;
                    bclipInfo.current_name = bnew_clip.name;
                }
#if DEBUG_INFO
                else
                {
                    string boriginal_name = blockStanceClip.original_name;
                    Debug.LogWarning("cannot find clip: " + boriginal_name + " on " + this.gameObject.name);
                }
#endif
            }

            if (blockHitClips != null)
            {
                for (int i = 0; i < blockHitClips.Count; i++)
                {
                    if (blockHitClips[i] == null) continue;
                    if (blockHitClips[i].clip == null) continue;
                    AnimationClip new_clip = blockHitClips[i].clip;

                    int origHash = blockHitClips[i].orig_hash;
                    if (origHash == -1)
                    {
                        Debug.LogWarning("Hash not created on clip: " + blockHitClips[i].original_name);
                        origHash = Animator.StringToHash(blockHitClips[i].original_name);
                    }

                    //AnimationClipReplacementInfo clipInfo = getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(origHash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {

                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = blockHitClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name);
                        continue;
                    }
#endif
                }
            }

            if (locomotionClips != null)
            {
                for (int i = 0; i < locomotionClips.Count; i++)
                {
                    if (locomotionClips[i] == null) continue;
                    if (locomotionClips[i].clip == null) continue;

                    AnimationClip new_clip = locomotionClips[i].clip;

                    int origHash = locomotionClips[i].orig_hash;
                    if (origHash == -1)
                    {
                        Debug.LogWarning("Hash not created on clip: " + locomotionClips[i].original_name);
                        origHash = Animator.StringToHash(locomotionClips[i].original_name);
                    }

                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(origHash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = locomotionClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name);
                    }
#endif
                }
            }

            if (replacementClips != null)
            {
                for (int i = 0; i < replacementClips.Count; i++)
                {
                    if (replacementClips[i] == null) continue;
                    if (replacementClips[i].clip == null) continue;

                    AnimationClip new_clip = replacementClips[i].clip;

                    int origHash = replacementClips[i].orig_hash;
                    if (origHash == -1)
                    {
                        Debug.LogWarning("Hash not created on clip: " + replacementClips[i].original_name);
                        origHash = Animator.StringToHash(replacementClips[i].original_name);
                    }

                    //AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByName(original_name);
                    AnimationClipReplacementInfo clipInfo = _getOriginalClipInfoByHash(origHash);

                    if (clipInfo != null && clipInfo.clip != null)
                    {
                        clipInfo.changed = true;
                        myNewOverrideController[clipInfo.original_name /*current_name*/] = new_clip;
                        clipInfo.current_name = new_clip.name;
                    }
#if DEBUG_INFO
                    else
                    {
                        string original_name = replacementClips[i].original_name;
                        Debug.LogWarning("cannot find clip: " + original_name + " on " + this.gameObject.name);
                    }
#endif
                }
            }

            anim.runtimeAnimatorController = myNewOverrideController;
            UnityEngine.Object.Destroy(myCurrentOverrideController);

            resetAnimatorStates();
        }

        /// <summary>
        /// save animator states and parameters
        /// </summary>
        public void saveAnimatorStates()
        {
            Animator anim = animator;
            m_SavedCurrentStates.Clear();
            for (int i = 0; i < anim.layerCount; i++)
            {
                AnimatorStateInfo curstate = anim.GetCurrentAnimatorStateInfo(i);
                m_SavedCurrentStates.Add(curstate);
            }

            m_SavedParams.Clear();
            ;
            for (int i = 0; i < anim.parameters.Length; i++)
            {
                AnimatorControllerParameter par = anim.parameters[i];
                if (par.name == "pMatchStart" || par.name == "pMatchEnd") continue;

                object val = null;
                switch (par.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        val = (object)anim.GetBool(par.name);
                        break;
                    case AnimatorControllerParameterType.Float:
                        val = (object)anim.GetFloat(par.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        val = (object)anim.GetInteger(par.name);
                        break;
                }
                m_SavedParams.Add(par, val);
            }
        }

        /// <summary>
        /// reset animator states and parameters
        /// </summary>
        public void resetAnimatorStates()
        {
            Animator anim = animator;
            foreach (KeyValuePair<AnimatorControllerParameter, object> pair in m_SavedParams)
            {
                AnimatorControllerParameter p = pair.Key;
                if (p.name == "pMatchStart" || p.name == "pMatchEnd") continue;
                object v = pair.Value;
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        {
                            bool bval = (bool)v;
                            anim.SetBool(p.name, bval);
                        }
                        break;
                    case AnimatorControllerParameterType.Float:
                        {
                            float fval = (float)v;
                            anim.SetFloat(p.name, fval);
                        }
                        break;
                    case AnimatorControllerParameterType.Int:
                        {
                            int ival = (int)v;
                            anim.SetInteger(p.name, ival);
                        }
                        break;
                }
            }
            for (int i = 0; i < m_SavedCurrentStates.Count; i++)
            {
                AnimatorStateInfo state = m_SavedCurrentStates[i];
                anim.CrossFade(state.fullPathHash, 0.0f, i, state.normalizedTime);
            }
            anim.Update(Time.deltaTime);
        }

#endregion


    }
}
