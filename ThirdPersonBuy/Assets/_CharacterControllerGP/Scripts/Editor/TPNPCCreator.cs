// © 2016 Mario Lelas
using UnityEngine;
using UnityEditor;

namespace MLSpace
{
    /// <summary>
    /// Third person NPC creator
    /// </summary>
    public class TPNPCCreator : ScriptableWizard
    {
        public delegate void VoidFunc();

        public VoidFunc OnCreate = null;

        public string characterName = "NPC";

        public GameObject character;

        public bool createHealthIndicators = true;

        public bool createDamageReceivedIndicator = true;

        [Tooltip("Move speed of npc. Can be changed in Stats component.")]
        public float moveSpeed = 0.5f;

        [Tooltip("Create ragdoll and use Humanoid setup to create ragdoll system.")]
        public bool useHumanoidSetupForRagdollBones = true;

        [HideInInspector]
        public int initialHealth = 100;

        [HideInInspector]
        public int initialDamage = 10;

        [HideInInspector]
        public int initialAttackValue = 0;

        [HideInInspector]
        public int initialDefenceValue = 0;

        private RagdollCreator ragdollCreator;

        [HideInInspector]
        public float totalMass = 80f;


        [MenuItem("Tools/Character Controller GP/Third Person Creator/Create NPC")]
        public static TPNPCCreator CreateWizard()
        {
            TPNPCCreator creator = (TPNPCCreator)DisplayWizard("Third Person NPC Creator", typeof(TPNPCCreator));
            creator.helpString = "Create Character Components";
            creator.ragdollCreator = new RagdollCreator();
            return creator;
        }

        GameObject curObj = null;
        void OnWizardUpdate()
        {
            if (character != curObj)
            {
                characterName = character.name;
            }
            curObj = character;
        }

        void OnWizardCreate()
        {
            if (!character)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Character not assiged.", "OK");
                return;
            }

            if (!character.activeSelf)
            {
                character.SetActive(true);
            }

            PrefabType ptype = PrefabUtility.GetPrefabType(character);
            bool needs2create = ptype == PrefabType.ModelPrefab;


            if (needs2create)
            {
                character = Instantiate(character);
            }

            Animator anim = _createAnimator();
            if (!anim)
            {
                Debug.LogError("Error creating character. Could not add animator component." + " < " + character.ToString() + ">");
                return;
            }


            _createCapsule();
            _createRigidbody();
            _createAudiosource();
            _createTPCharacter();
            _createRagdollManager();
            _createAudioManager();
            _createNPCScript();
            _createStats();
            if (createHealthIndicators)
                _createHealthUI();
            if (createDamageReceivedIndicator)
                _createDamageUI();
            

            Undo.SetCurrentGroupName("Create NPC");


            character.tag = "NPC";
            character.name = characterName;
            character.layer = LayerMask.NameToLayer("NPCLayer");
        }

        protected override bool DrawWizardGUI()
        {
            bool ok = base.DrawWizardGUI();

            if (useHumanoidSetupForRagdollBones)
            {
                EditorGUI.indentLevel++;
                totalMass = EditorGUILayout.FloatField("Overall Mass", totalMass);
                EditorGUI.indentLevel--;
            }

            initialHealth = EditorGUILayout.IntField("Initial Health", initialHealth);
            initialDamage = EditorGUILayout.IntField("Initial Damage", initialDamage);
            initialAttackValue = EditorGUILayout.IntField("Initial Attack", initialAttackValue);
            initialDefenceValue = EditorGUILayout.IntField("Initial Defence", initialDefenceValue);

            return ok;
        }

        Animator _createAnimator()
        {
            string animatorPath = "Animators/NPCAnimator";

            // find resources first
            RuntimeAnimatorController rtac = Resources.Load<RuntimeAnimatorController>(animatorPath);
            if (rtac == null)
                Debug.LogError("Cannot assign runtime controller. Check resource path");

            Animator anim = character.GetComponent<Animator>();
            if (!anim)
            {
                // check childs
                anim = character.GetComponentInChildren<Animator>();
                Avatar avatar = anim.avatar;
                Animator temp = anim;
                anim = character.AddComponent<Animator>();
                anim.avatar = avatar;
                Destroy(temp);
            }
            if (!anim.avatar.isHuman)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Cannot create charater on non-humanoid avatars.", "OK");
                return null;
            }
            if (!anim.avatar.isValid)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Cannot create charater on invalid avatars.", "OK");
                return null;
            }
            anim.runtimeAnimatorController = rtac;
            anim.updateMode = AnimatorUpdateMode.Normal;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            return anim;
        }

        bool _createCapsule()
        {
            CapsuleCollider cap = character.GetComponent<CapsuleCollider>();
            if (!cap) cap = Undo.AddComponent<CapsuleCollider>(character);

            cap.center = new Vector3(0.0f, 1.0f, 0.0f);
            cap.height = 2.0f;
            cap.radius = 0.3f;
            cap.isTrigger = false;
            cap.direction = 1;
            return true;
        }

        bool _createRigidbody()
        {
            Rigidbody rb = character.GetComponent<Rigidbody>();
            if (!rb) rb = Undo.AddComponent<Rigidbody>(character);

            rb.mass = 40.0f;
            rb.drag = 0.0f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            return true;
        }

        bool _createAudiosource()
        {
            AudioSource asource = character.GetComponent<AudioSource>();
            if (!asource) asource = Undo.AddComponent<AudioSource>(character);
            asource.playOnAwake = false;
            asource.minDistance = 1.0f;
            asource.maxDistance = 20.0f;
            asource.spatialBlend = 1.0f;
            return true;
        }

        bool _createTPCharacter()
        {
            TPCharacter tpcharacter = character.GetComponent<TPCharacter>();
            if (!tpcharacter) tpcharacter = Undo.AddComponent<TPCharacter>(character);

            // for some reason unity dont recognize 'Default' layer in ' GetMask' method so i must add it after
            tpcharacter.layers = LayerMask.GetMask(/*"Default",*/ "DefaultNoCam", "DefaultSlope", "Walkable");
            tpcharacter.layers |= 1 << LayerMask.NameToLayer("Default");

            tpcharacter.movingTurnSpeed = 360.0f;
            tpcharacter.stationaryTurnSpeed = 180.0f;
            tpcharacter.jumpPower = 7.0f;
            tpcharacter.gravityMultiplier = 2.0f;
            tpcharacter.moveSpeedMultiplier = 1.0f;
            tpcharacter.groundCheckDistance = 0.35f;
            tpcharacter.airSpeed = 6.0f;
            tpcharacter.airControl = 2.0f;
            tpcharacter.AnimatorSpeed = 1.0f;
            tpcharacter.ledgeSpeed = 3.0f;
            return true;
        }

        bool _createRagdollManager()
        {
            RagdollManager ragMan = character.GetComponent<RagdollManager>();
            if (!ragMan) ragMan = Undo.AddComponent<RagdollManager>(character);

            if (useHumanoidSetupForRagdollBones)
            {
                Animator anim = character.GetComponent<Animator>();
                if (!anim) { Debug.LogError("Cannot find 'animator' component." + " < " + character.ToString() + ">"); return false; }

                ragdollCreator.pelvis = anim.GetBoneTransform(HumanBodyBones.Hips);
                ragdollCreator.leftHips = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                ragdollCreator.leftKnee = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                ragdollCreator.leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                ragdollCreator.rightHips = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                ragdollCreator.rightKnee = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                ragdollCreator.rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                ragdollCreator.leftArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                ragdollCreator.leftElbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                ragdollCreator.rightArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                ragdollCreator.rightElbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
                ragdollCreator.middleSpine = anim.GetBoneTransform(HumanBodyBones.Chest);
                ragdollCreator.head = anim.GetBoneTransform(HumanBodyBones.Head);


                ragdollCreator.CheckConsistency();
                ragdollCreator.CalculateAxes();
                ragdollCreator.Create();

                ragMan.RagdollBones = new Transform[(int)BodyParts.BODY_PART_COUNT];
                ragMan.RagdollBones[(int)BodyParts.Spine] = ragdollCreator.pelvis;
                ragMan.RagdollBones[(int)BodyParts.Chest] = ragdollCreator.middleSpine;
                ragMan.RagdollBones[(int)BodyParts.Head] = ragdollCreator.head;
                ragMan.RagdollBones[(int)BodyParts.LeftShoulder] = ragdollCreator.leftArm;
                ragMan.RagdollBones[(int)BodyParts.LeftElbow] = ragdollCreator.leftElbow;
                ragMan.RagdollBones[(int)BodyParts.RightShoulder] = ragdollCreator.rightArm;
                ragMan.RagdollBones[(int)BodyParts.RightElbow] = ragdollCreator.rightElbow;
                ragMan.RagdollBones[(int)BodyParts.LeftHip] = ragdollCreator.leftHips;
                ragMan.RagdollBones[(int)BodyParts.LeftKnee] = ragdollCreator.leftKnee;
                ragMan.RagdollBones[(int)BodyParts.RightHip] = ragdollCreator.rightHips;
                ragMan.RagdollBones[(int)BodyParts.RightKnee] = ragdollCreator.rightKnee;
                RagdollManager.AddBodyColliderScripts(ragMan);

                ragdollCreator = null;
            }

            return true;
        }

        void _createAudioManager()
        {
            AudioManager am = character.GetComponent<AudioManager>();
            if (!am) am = Undo.AddComponent<AudioManager>(character);

            WeaponAudio ms = character.GetComponent<WeaponAudio>();
            if (!ms) ms = Undo.AddComponent<WeaponAudio>(character);

            // load default clips
            AudioClip grunt1 = Resources.Load<AudioClip>("Audio/grunt1");
            AudioClip grunt2 = Resources.Load<AudioClip>("Audio/grunt2");
            AudioClip grunt4 = Resources.Load<AudioClip>("Audio/grunt4");
            AudioClip grunt5 = Resources.Load<AudioClip>("Audio/grunt5");
            AudioClip punch1 = Resources.Load<AudioClip>("Audio/punch1");
            AudioClip smack1 = Resources.Load<AudioClip>("Audio/smack1");
            AudioClip smack2 = Resources.Load<AudioClip>("Audio/smack2");
            AudioClip smack3 = Resources.Load<AudioClip>("Audio/smack3");
            AudioClip block1 = Resources.Load<AudioClip>("Audio/block1");


            if (!grunt1) Debug.LogWarning("Unable to load clip: 'Audio/grunt1'");
            if (!grunt2) Debug.LogWarning("Unable to load clip: 'Audio/grunt2'");
            if (!grunt4) Debug.LogWarning("Unable to load clip: 'Audio/grunt4'");
            if (!grunt5) Debug.LogWarning("Unable to load clip: 'Audio/grunt5'");
            if (!punch1) Debug.LogWarning("Unable to load clip: 'Audio/punch1'");
            if (!smack1) Debug.LogWarning("Unable to load clip: 'Audio/smack1'");
            if (!smack2) Debug.LogWarning("Unable to load clip: 'Audio/smack2'");
            if (!smack3) Debug.LogWarning("Unable to load clip: 'Audio/smack3'");
            if (!block1) Debug.LogWarning("Unable to load clip: 'Audio/block1'");

            am.jumpSounds = new AudioClip[] { grunt1, grunt5 };
            am.diveRollSounds = new AudioClip[] { grunt2 };

            ms.attackSwingSounds = new AudioClip[] { grunt4, grunt5 };
            ms.attackHitSounds = new AudioClip[] { punch1, smack1, smack2, smack3 };
            ms.blockSounds = new AudioClip[] { block1 };

        }

        void _createNPCScript()
        {
            NPCScript npcscript = character.GetComponent<NPCScript>();
            if (!npcscript) npcscript = Undo.AddComponent<NPCScript>(character);
            npcscript.blockOdds = 10;
            npcscript.attackInterval = 1.0f;
            Transform sweepCapsule = Utils.FindChildTransformByName(character.transform, "AttackSweepCapsule");
            Rigidbody sweepBody = null;
            if (!sweepCapsule)
            {
                Debug.Log("Creating attack sweep capsule");
                sweepBody = Resources.Load<Rigidbody>("AttackSweepCapsule");
                sweepBody = Instantiate(sweepBody);
                sweepBody.transform.SetParent(npcscript.transform);
                sweepBody.transform.localPosition = new Vector3(0.0f, 1.276f, 0.0f);
                sweepBody.name = "AttackSweepCapsule";
            }
            if (!sweepBody)
            {
                Debug.LogError("Cannot find Rigidbody on 'AttackSweepCapsule'" + " < " + character.ToString() + ">");
                return;
            }
            npcscript.attackSweepBody = sweepBody;
        }

        void _createHealthUI()
        {
            HealthUI hui = character.GetComponent<HealthUI>();
            if (!hui) hui = Undo.AddComponent<HealthUI>(character);
        }

        void _createDamageUI()
        {
            DebugUI dbg = character.GetComponent<DebugUI>();
            if (!dbg) dbg = Undo.AddComponent<DebugUI>(character);

            dbg.textType = DebugUI.TextType.Float;
            dbg._color = Color.yellow;
        }

        void _createStats()
        {
            Stats stats = character.GetComponent<Stats>();
            if (!stats) stats = Undo.AddComponent<Stats>(character);

            stats.maxHealth = initialHealth;
            stats.damage = initialDamage;
            stats.attack = initialAttackValue;
            stats.defence = initialDefenceValue;
            stats.moveSpeed = moveSpeed;
        }

    }
}
