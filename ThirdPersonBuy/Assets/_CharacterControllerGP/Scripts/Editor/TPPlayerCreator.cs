// © 2016 Mario Lelas
using UnityEngine;
using UnityEditor;


namespace MLSpace
{
    /// <summary>
    /// Third person player creator
    /// </summary>
    public class TPPlayerCreator : ScriptableWizard
    {
        public delegate void VoidFunc();

        public enum CharacterTypes { Default, ThirdPerson, TopDown };

        public CharacterTypes CharacterType = CharacterTypes.Default;

        public VoidFunc OnCreate = null;

        public string characterName = "Player";

        public GameObject character;

        [Tooltip("Enable / disable jumping. Can be modified in 'TPCharacter' component.")]
        public bool enableJumping = true;

        [Tooltip("Enable / disable crouching. Can be modified in 'TPCharacter' component.")]
        public bool enableCrouching = true;

        [Tooltip("Move speed of player. Can be changed in Stats component.")]
        public float moveSpeed = 1.0f;

        [Tooltip("Create ragdoll and use Humanoid setup to create ragdoll system.")]
        public bool useHumanoidSetupForRagdollBones = true;

        [HideInInspector]
        public int initialHealth = 500;

        [HideInInspector]
        public int initialDamage = 10;

        [HideInInspector]
        public int initialAttackValue = 0;

        [HideInInspector]
        public int initialDefenceValue = 0;

        [HideInInspector]
        public float totalMass = 80f;


        private RagdollCreator ragdollCreator;
        private GameObject curObj = null;

        [MenuItem("Tools/Character Controller GP/Third Person Creator/Create Player")]
        public static TPPlayerCreator CreateWizard()
        {
            TPPlayerCreator creator = (TPPlayerCreator)DisplayWizard("Third Person Player Creator", typeof(TPPlayerCreator));
            creator.helpString = "Create Character Components";
            creator.ragdollCreator = new RagdollCreator();
            return creator;
        }

        
        void OnWizardUpdate()
        {
            if(character != curObj)
            {
                
                characterName = character.name;
            }
            curObj = character;
        }


        void OnWizardCreate()
        {
            if(!character )
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Character not assiged.", "OK");
                return;
            }

            if(!character .activeSelf )
            {
                character.SetActive(true);
            }

            PrefabType ptype = PrefabUtility.GetPrefabType(character);
            bool needs2create = ptype == PrefabType.ModelPrefab;


            if(needs2create )
            {
                character = Instantiate(character);
                Undo.RegisterCreatedObjectUndo(character, "Create Player");
            }


            Animator anim = _createAnimator();
            if (!anim)
            {
                Debug.LogError("Error creating character. Could not add animator component." + " < " + this.ToString() + ">");
                return;
            }
            EditorUtils.CreateGameControllerObject(CharacterType != CharacterTypes.Default);

            _createCapsule();
            _createRigidbody();
            _createAudiosource();
            _createTPCharacter();
            _createRagdollManager();
            UnityEngine.UI.Text triggerUI = _createUI();
            _createTriggerManager(triggerUI);
            _createIKHelper();
            BaseCamera camera = null;
             camera = _createCamera();
            _createPlayerScript(camera);
            _createLegsIK();
            if(CharacterType != CharacterTypes.TopDown)_createSlopeScript(); // dont need for top down system because its traversing on navmesh
            _createAudio();
            _createPlayerControl();
            _createStats();
            _createEquipmentScript();
            _createItemPicker();

            character.tag = "Player";
            character.name = characterName;
            character.layer = LayerMask.NameToLayer("PlayerLayer");

            Undo.SetCurrentGroupName("Create Player");

            if (!camera )
            {
                Debug.Log("camera is null.");
            }
            else
            {
                Debug.Log("camera " + camera.name + " parent: " + 
                    (camera.transform.parent == null ? "NULL" : camera.transform.parent.name));
            }

            if (OnCreate != null)
            {
                OnCreate();
            }
        }

        protected override bool DrawWizardGUI()
        {
            bool ok =  base.DrawWizardGUI();

            if(useHumanoidSetupForRagdollBones)
            {
                EditorGUI.indentLevel++;
                totalMass = EditorGUILayout.FloatField ("Overall Mass", totalMass);
                EditorGUI.indentLevel--;
            }

            if(CharacterType != CharacterTypes.Default)
            {
                initialHealth = EditorGUILayout.IntField ("Initial Health", initialHealth);
                initialDamage = EditorGUILayout.IntField("Initial Damage", initialDamage);
                initialAttackValue = EditorGUILayout.IntField("Initial Attack", initialAttackValue);
                initialDefenceValue = EditorGUILayout.IntField("Initial Defence", initialDefenceValue);
            }

            return ok;
        }

#region Create Character Components

        Animator _createAnimator()
        {
            string animatorPath = "Animators/PlayerAnimatorDefault";
            if (CharacterType == CharacterTypes.ThirdPerson ||
                CharacterType == CharacterTypes.TopDown)
                animatorPath = "Animators/PlayerAnimatorCombatFramework";

            // find resources first
            RuntimeAnimatorController rtac = Resources.Load<RuntimeAnimatorController>(animatorPath);
            if (rtac == null)
                Debug.LogError("Cannot assign runtime controller. Check resource path." + " < " + this.ToString() + ">");

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
            if (!cap)
            {
                cap = Undo.AddComponent<CapsuleCollider>(character);
            }

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
            if (!rb)
                rb = Undo.AddComponent<Rigidbody>(character);

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
            if (!asource)
                asource = Undo.AddComponent<AudioSource>(character);

            asource.playOnAwake = false;
            asource.minDistance = 1.0f;
            asource.maxDistance = 20.0f;
            asource.spatialBlend = 1.0f;
            return true;
        }

        bool _createTPCharacter()
        {
            TPCharacter tpcharacter = character.GetComponent<TPCharacter>();
            if (!tpcharacter)
                tpcharacter = Undo.AddComponent<TPCharacter>(character);
            if (CharacterType == CharacterTypes.Default ||
                CharacterType == CharacterTypes.ThirdPerson)
            {
                // for some reason unity ( 5.2.2 ) dont recognize 'Default' layer in ' GetMask' method so i must add it after
                tpcharacter.layers = LayerMask.GetMask(/*"Default", */"DefaultNoCam", "DefaultSlope", "ColliderLayer");
                tpcharacter.layers |= 1 << LayerMask.NameToLayer("Default");
            }
            else if (CharacterType == CharacterTypes.TopDown)
            {
                // for some reason unity ( 5.2.2 ) dont recognize 'Default' layer in ' GetMask' method so i must add it after
                tpcharacter.layers = LayerMask.GetMask(/*"Default",*/ "DefaultNoCam", "DefaultSlope", "ColliderLayer", "Walkable");
                tpcharacter.layers |= 1 << LayerMask.NameToLayer("Default");
            }

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
            tpcharacter.enableJumping = enableJumping;
            tpcharacter.enableCrouching = enableCrouching;
            return true;
        }

        UnityEngine.UI.Text _createUI()
        {
            UnityEngine.UI.Text uiText = null;
            // load canvas from resources
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas)
            {
                Transform uiXform = Utils.FindChildTransformByName(canvas.transform, "PlayerTriggerUI");
                if (uiXform)
                {
                    uiText = uiXform.GetComponent<UnityEngine.UI.Text>();
                }
                else
                {
                    uiText = Resources.Load<UnityEngine.UI.Text>("PlayerTriggerUI");
                    uiText = Instantiate(uiText);
                    uiText.transform.SetParent(canvas.transform, false);
                    Undo.RegisterCreatedObjectUndo(uiText.gameObject, "Create Text");
                }
            }
            else
            {
                Debug.Log("creating new canvas...");
                Canvas canvasPrefab = Resources.Load<Canvas>("Canvas");
                if (!canvasPrefab)
                {
                    Debug.LogError("Cannot find 'Canvas' prefab!" + " < " + this.ToString() + ">");
                    return null;
                }


                canvas = Instantiate(canvasPrefab);

                Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Create Canvas");


                Transform uiXform = Utils.FindChildTransformByName(canvas.transform, "PlayerTriggerUI");
                if (uiXform)
                {
                    uiText = uiXform.GetComponent<UnityEngine.UI.Text>();
                }
                else
                {
                    uiText = Resources.Load<UnityEngine.UI.Text>("PlayerTriggerUI");
                    uiText = Instantiate(uiText);
                    uiText.transform.SetParent(canvas.transform, false);
                    Undo.RegisterCreatedObjectUndo(uiText.gameObject, "Create Text");
                }
            }
            canvas.name = "Canvas";
            uiText.name = "PlayerTriggerUI";

            return uiText;
        }

        bool _createRagdollManager()
        {
            RagdollManager ragMan = character.GetComponent<RagdollManager>();
            if (!ragMan)
            {
                //ragMan = character.AddComponent<RagdollManager>();
                ragMan = Undo.AddComponent<RagdollManager>(character);
            }
            if (useHumanoidSetupForRagdollBones)
            {
                Animator anim = character.GetComponent<Animator>();
                if (!anim) { Debug.LogError("Cannot find 'animator' component." + " < " + this.ToString() + ">"); return false; }

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

                ragdollCreator.totalMass = totalMass;

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

        bool _createTriggerManager(UnityEngine.UI.Text textUI)
        {
            TriggerManagement tMan = character.GetComponent<TriggerManagement>();
            if (!tMan) tMan = Undo.AddComponent<TriggerManagement>(character);
            tMan.m_TriggerUI = textUI;
            tMan.m_TriggerInterval = 1.0f;

            return true;
        }

        bool _createIKHelper()
        {
            IKHelper ikh = character.GetComponent<IKHelper>();
            if (!ikh) ikh = Undo.AddComponent<IKHelper>(character);

            ikh.footOffset = 0.1f;
            ikh.hangCheckDistance = 0.3f;
            ikh.ledgeRelativePosition = new Vector3(0.0f, -1.1f, -0.4f);
            ikh.ledgeRelativePositionHang = new Vector3(0.0f, -2.15f, -0.1f);
            return true;
        }

        void addCommonCameraComponents(GameObject camGO)
        {
            Camera cam = camGO.GetComponent<Camera>();
            if (!cam)
            {
                cam = Undo.AddComponent<Camera>(camGO);
            }

            GUILayer guiLayer = camGO.GetComponent<GUILayer>();
            if (!guiLayer)
            {
                guiLayer = Undo.AddComponent<GUILayer>(camGO);
            }
            FlareLayer flareLayer = camGO.GetComponent<FlareLayer>();
            if (!flareLayer)
            {
                flareLayer = Undo.AddComponent<FlareLayer>(camGO);
            }

            AudioListener al = camGO.GetComponent<AudioListener>();
            if (!al)
            {
                al = Undo.AddComponent<AudioListener>(camGO);
            }
        }

        BaseCamera _createCamera()
        {
            if (CharacterType == CharacterTypes.TopDown)
            {
                Transform camDefTargetXform = Utils.FindChildTransformByName(character.transform, "CameraDefaultTarget");
                if (!camDefTargetXform)
                {
                    GameObject cameraDefaultTarget = new GameObject("CameraDefaultTarget");
                    cameraDefaultTarget.transform.SetParent(character.transform);
                    Vector3 pos = new Vector3(0.0f, 1.63f, 0.0f);
                    cameraDefaultTarget.transform.localPosition = pos;
                    camDefTargetXform = cameraDefaultTarget.transform;
                }
                // check if camera exists 
                GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
                if (!camGO)
                {
                    Debug.Log("Creating camera...");
                    camGO = new GameObject("TopDownCamera");
                }
#if DEBUG_INFO
                else
                {
                    Debug.Log("Found camera...");
                }
#endif
                camGO.transform.SetParent(character.transform, false);
                Quaternion currentRotation = character.transform.rotation;
                Vector3 cpos = new Vector3(6.744616f, 11.20537f, -0.4592583f);
                cpos = currentRotation * cpos;

                camGO.transform.position = character.transform.position + cpos;
                camGO.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                camGO.tag = "MainCamera";
                camGO.name = "TopDownCamera";
                camGO.SetActive(true);
                addCommonCameraComponents(camGO);

                TopDownCamera tdc = camGO.GetComponent<TopDownCamera>();
                if (!tdc)
                {
                    tdc = Undo.AddComponent<TopDownCamera>(camGO);
                }
                tdc.Target = camDefTargetXform;

                Camera cam = camGO.GetComponent<Camera>();
                cam.hdr = true;

                return tdc;

            }
            else
            {
                Transform camDefTargetXform = Utils.FindChildTransformByName(character.transform, "CameraDefaultTarget");
                if (!camDefTargetXform)
                {
                    GameObject cameraDefaultTarget = new GameObject("CameraDefaultTarget");
                    cameraDefaultTarget.transform.SetParent(character.transform);
                    Vector3 pos = new Vector3(0.184f, 1.771f, 0.0f);
                    cameraDefaultTarget.transform.localPosition = pos;
                    camDefTargetXform = cameraDefaultTarget.transform;
                }

                // check if camera exists on character
                Transform camXform = Utils.FindChildTransformByTag(character.transform, "MainCamera");
                GameObject camGO = null;
                if (!camXform)
                {
                    Debug.Log("Creating camera...");
                    camGO = new GameObject("OrbitCamera");
                }
                else
                {
                    Debug.Log("Found existing camera on character");
                    camGO = camXform.gameObject;
                }


                camGO.transform.SetParent(character.transform, false);

                Quaternion currentRotation = character.transform.rotation;
                Vector3 cpos = new Vector3(0.03f, 2.04f, -3.167f);
                cpos = currentRotation * cpos;

                camGO.transform.position = character.transform.position + cpos;
                camGO.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                camGO.tag = "MainCamera";
                camGO.name = "OrbitCamera";
                camGO.SetActive(true);

                addCommonCameraComponents(camGO);

                OrbitCameraController occ = camGO.GetComponent<OrbitCameraController>();
                if (!occ)
                {
                    occ = Undo.AddComponent<OrbitCameraController>(camGO);
                }
                occ.Target = camDefTargetXform;
                occ.angularSpeed = 64.0f;
                occ.minXAngle = -60.0f;
                occ.maxXAngle = 45.0f;
                occ.minYAngle = -180.0f;
                occ.maxYAngle = 180.0f;
                occ.minZ = 0.2f;
                occ.maxZ = 1.6f;
                occ.zStep = 0.1f;

                ProtectFromWalls pfw = camGO.GetComponent<ProtectFromWalls>();
                if (!pfw)
                {
                    pfw = Undo.AddComponent<ProtectFromWalls>(camGO);

                }
                // for some reason unity ( 5.2.2 ) dont recognize 'Default' layer in ' GetMask' method so i must add it after
                pfw.mask = LayerMask.GetMask("DefaultSlope");
                pfw.mask |= 1 << LayerMask.NameToLayer("Default");


                pfw.clipMoveTime = 0.0f;
                pfw.returnTime = 0.5f;
                pfw.sphereCastRadius = 0.05f;
                pfw.visualiseInEditor = true;
                pfw.closestDistance = 0.05f;
                pfw.dontClipTag = "Player";
                pfw.m_Pivot = camDefTargetXform;

                Camera cam = camGO.GetComponent<Camera>();
                cam.hdr = true;

                return occ;
            }
        }

        bool _createPlayerScript(BaseCamera camera)
        {
            switch (CharacterType)
            {
                case CharacterTypes.Default:
                    {
                        PlayerThirdPerson playerScript = character.GetComponent<PlayerThirdPerson>();
                        if (!playerScript)
                        {
                            playerScript = Undo.AddComponent<PlayerThirdPerson>(character);
                        }
                        playerScript.m_Camera = camera;
                        // Create crouch camera target
                        Transform camCrouchTargetXform = Utils.FindChildTransformByName(character.transform, "CameraCrouchTarget");
                        if (!camCrouchTargetXform)
                        {
                            GameObject cameraCrouchTarget = new GameObject("CameraCrouchTarget");
                            cameraCrouchTarget.transform.SetParent(character.transform);
                            cameraCrouchTarget.transform.localPosition = new Vector3(0.276f, 0.961f, 0.0f);
                            camCrouchTargetXform = cameraCrouchTarget.transform;
                        }
                        playerScript.crouchCameraTarget = camCrouchTargetXform;
                        playerScript.standCameraTarget = camera.Target;

                    }
                    break;
                case CharacterTypes.ThirdPerson:
                    {
                        PlayerThirdPerson playerScript = character.GetComponent<PlayerThirdPerson>();
                        if (!playerScript)
                        {
                            playerScript = Undo.AddComponent<PlayerThirdPerson>(character);
                        }
                        playerScript.m_Camera = camera;
                        // Create crouch camera target
                        Transform camCrouchTargetXform = Utils.FindChildTransformByName(character.transform, "CameraCrouchTarget");
                        if (!camCrouchTargetXform)
                        {
                            GameObject cameraCrouchTarget = new GameObject("CameraCrouchTarget");
                            cameraCrouchTarget.transform.SetParent(character.transform);
                            cameraCrouchTarget.transform.localPosition = new Vector3(0.276f, 0.961f, 0.0f);
                            camCrouchTargetXform = cameraCrouchTarget.transform;
                        }
                        playerScript.crouchCameraTarget = camCrouchTargetXform;
                        playerScript.standCameraTarget = camera.Target;

                    }
                    break;
                case CharacterTypes.TopDown:
                    {
                        PlayerTopDown playerScript = character.GetComponent<PlayerTopDown>();
                        if (!playerScript)
                        {
                            playerScript = Undo.AddComponent<PlayerTopDown>(character);
                        }
                        playerScript.m_Camera = camera;
                        playerScript.walkableTarrainMask = LayerMask.GetMask("Walkable");

                    }
                    break;
            }
            return true;
        }

        bool _createLegsIK()
        {
            LegsIK lik = character.GetComponent<LegsIK>();
            if (!lik) lik = Undo.AddComponent<LegsIK>(character);

            lik.EnableIKStepping = true;
            lik.MaxStepHeight = 0.5f;
            lik.ikDistanceOffset = 0.02f;
            lik.footOffset = new Vector3(0.0f, 0.11f, 0.0f);
            lik.MaxSlopeAngle = 75.0f;
            lik.bumpJump = 0.005f;
            if (CharacterType == CharacterTypes.TopDown)
            {
                // for some reason unity ( 5.2.2 ) dont recognize 'Default' layer in ' GetMask' method so i must add it after

                lik.IKSteppingLayers = LayerMask.GetMask(/*"Default", */"ColliderLayer", "Walkable");
                lik.IKSteppingLayers |= 1 << LayerMask.NameToLayer("Default");


                lik.LegsIKLayers = LayerMask.GetMask(/*"Default", */"ColliderLayer", "DefaultSlope", "Walkable");
                lik.LegsIKLayers |= 1 << LayerMask.NameToLayer("Default");
            }
            else
            {
                // for some reason unity ( 5.2.2 ) dont recognize 'Default' layer in ' GetMask' method so i must add it after

                lik.IKSteppingLayers = LayerMask.GetMask(/*"Default", */"ColliderLayer");
                lik.IKSteppingLayers |= 1 << LayerMask.NameToLayer("Default");

                lik.LegsIKLayers = LayerMask.GetMask(/*"Default",*/ "ColliderLayer", "DefaultSlope");
                lik.LegsIKLayers |= 1 << LayerMask.NameToLayer("Default");
            }
            return true;
        }

        bool _createSlopeScript()
        {
            SlopeControl slope = character.GetComponent<SlopeControl>();
            if (!slope) slope = Undo.AddComponent<SlopeControl>(character);

            slope.MaxSlope = 60.0f;
            slope.layers = LayerMask.GetMask("DefaultSlope");

            return true;
        }

        bool _createPlayerControl()
        {
            switch (CharacterType)
            {
                case CharacterTypes.Default:
                    {
                        PlayerControlDefault pc = character.GetComponent<PlayerControlDefault>();
                        if (!pc)
                        {
                            pc = Undo.AddComponent<PlayerControlDefault>(character);
                        }
                    }
                    break;
                case CharacterTypes.ThirdPerson:
                    {
                        PlayerControl pc = character.GetComponent<PlayerControl>();
                        if (!pc)
                        {
                            pc = Undo.AddComponent<PlayerControl>(character);
                        }

                        Transform sweepCapsule = Utils.FindChildTransformByName(character.transform, "AttackSweepCapsule");
                        Rigidbody sweepBody = null;
                        if (!sweepCapsule)
                        {
                            Debug.Log("Creating attack sweep capsule");
                            sweepBody = Resources.Load<Rigidbody>("AttackSweepCapsule");
                            sweepBody = Instantiate(sweepBody);
                            sweepBody.transform.SetParent(pc.transform);
                            sweepBody.transform.localPosition = new Vector3(0.0f, 1.276f, 0.0f);
                            sweepBody.name = "AttackSweepCapsule";
                        }
                        if (!sweepBody)
                        {
                            Debug.LogError("Cannot find Rigidbody on 'AttackSweepCapsule'" + " < " + this.ToString() + ">");
                            return false;
                        }
                        pc.attackSweepBody = sweepBody;
                        Utils.LoadDefaultDualWeaponsClips(ref pc.dualWeaponAttackClip, ref pc.dualWeaponBlockStance,
                            ref pc.dualWeaponBlockHitClips, ref pc.dualWeaponLocomotionClips);
                    }
                    break;
                case CharacterTypes.TopDown:
                    {
                        PlayerControlTopDown pc = character.GetComponent<PlayerControlTopDown>();
                        if (!pc)
                        {
                            pc = Undo.AddComponent<PlayerControlTopDown>(character);
                        }

                        Rigidbody sweepBody = Resources.Load<Rigidbody>("AttackSweepCapsule");
                        sweepBody = Instantiate(sweepBody);
                        sweepBody.transform.SetParent(pc.transform);
                        sweepBody.transform.localPosition = new Vector3(0.0f, 1.276f, 0.0f);
                        sweepBody.name = "AttackSweepCapsule";
                        pc.attackSweepBody = sweepBody;
                        Utils.LoadDefaultDualWeaponsClips(ref pc.dualWeaponAttackClip, ref pc.dualWeaponBlockStance,
                            ref pc.dualWeaponBlockHitClips, ref pc.dualWeaponLocomotionClips);
                    }
                    break;
                default:
                    {
                        PlayerControlDefault pc = character.GetComponent<PlayerControlDefault>();
                        if (!pc)
                        {
                            pc = Undo.AddComponent<PlayerControlDefault>(character);
                        }
                    }
                    break;
            }
            return true;
        }

        void _createAudio()
        {

            AudioManager am = character.GetComponent<AudioManager>();
            if (!am)
            {
                am = Undo.AddComponent<AudioManager>(character);
            }

            // load default clips
            AudioClip jumpClip1 = Resources.Load<AudioClip>("Audio/grunt5");
            AudioClip jumpClip2 = Resources.Load<AudioClip>("Audio/grunt1");
            AudioClip diveRoll = Resources.Load<AudioClip>("Audio/grunt2");

            if (!jumpClip1) Debug.LogWarning("Unable to load clip: 'Audio/grunt5'");
            if (!jumpClip2) Debug.LogWarning("Unable to load clip: 'Audio/grunt1'");
            if (!diveRoll) Debug.LogWarning("Unable to load clip: 'Audio/grunt2'");

            am.jumpSounds = new AudioClip[] { jumpClip1, jumpClip2 };
            am.diveRollSounds = new AudioClip[] { diveRoll };

            if (CharacterType == CharacterTypes.ThirdPerson ||
                    CharacterType == CharacterTypes.TopDown)
            {
                WeaponAudio ms = character.GetComponent<WeaponAudio>();
                if (!ms) ms = Undo.AddComponent<WeaponAudio>(character);

                // load default clips
                AudioClip punch1 = Resources.Load<AudioClip>("Audio/punch1");
                AudioClip smack1 = Resources.Load<AudioClip>("Audio/smack1");
                AudioClip smack2 = Resources.Load<AudioClip>("Audio/smack2");
                AudioClip smack3 = Resources.Load<AudioClip>("Audio/smack3");
                AudioClip block1 = Resources.Load<AudioClip>("Audio/block1");
                AudioClip swoosh1 = Resources.Load<AudioClip>("Audio/swoosh1");
                AudioClip swoosh2 = Resources.Load<AudioClip>("Audio/swoosh2");

                if (!punch1) Debug.LogWarning("Unable to load clip: 'Audio/punch1'");
                if (!smack1) Debug.LogWarning("Unable to load clip: 'Audio/smack1'");
                if (!smack2) Debug.LogWarning("Unable to load clip: 'Audio/smack2'");
                if (!smack3) Debug.LogWarning("Unable to load clip: 'Audio/smack3'");
                if (!block1) Debug.LogWarning("Unable to load clip: 'Audio/block1'");
                if (!swoosh1) Debug.LogWarning("Unable to load clip: 'Audio/swoosh1'");
                if (!swoosh2) Debug.LogWarning("Unable to load clip: 'Audio/swoosh2'");



                ms.attackSwingSounds = new AudioClip[] { swoosh1, swoosh2 };
                ms.attackHitSounds = new AudioClip[] { punch1, smack1, smack2, smack3 };
                ms.blockSounds = new AudioClip[] { block1 };
            }
        }
        

        void _createStats()
        {
            Stats stats = character.GetComponent<Stats>();
            if (!stats) stats = Undo.AddComponent<Stats>(character);

            stats.attackSpeed = 1.2f;
            stats.maxHealth = initialHealth;
            stats.damage  = initialDamage;
            stats.attack = initialAttackValue;
            stats.defence = initialDefenceValue;
            stats.moveSpeed = 1.0f;
        } 

        void _createEquipmentScript()
        {
            EquipmentScript es = character.GetComponent<EquipmentScript>();
            if (!es) es = Undo.AddComponent<EquipmentScript>(character);

            Animator anim = character.GetComponent<Animator>();
            if(!anim)
            {
                Debug.LogError("Cannot find 'Animator' component on " + character .name );
                return;
            }

            Transform rhand = anim.GetBoneTransform(HumanBodyBones.RightHand);
            Transform lHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform lHip = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform lElbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            Transform rIndex = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            Transform rHip = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

            

            GameObject WEAPON_WIELD = new GameObject("WEAPON_WIELD");
            GameObject WEAPON1H_REST = new GameObject("WEAPON1H_REST");
            GameObject SHIELD_WIELD = new GameObject("SHIELD_WIELD");
            GameObject SHIELD_REST = new GameObject("SHIELD_REST");
            GameObject WEAPON2H_REST = new GameObject("WEAPON2H_REST");
            GameObject QUIVER_REST = new GameObject("QUIVER_REST");
            GameObject BOW_REST = new GameObject("BOW_REST");
            GameObject BOW_WIELD = new GameObject("BOW_WIELD");
            GameObject ARROW = new GameObject("ARROW");
            GameObject SECONDARY_REST = new GameObject("SECONDARY_REST");
            GameObject SECONDARY_WIELD = new GameObject("SECONDARY_WIELD");


            Undo.RegisterCreatedObjectUndo(WEAPON_WIELD, "Create Weapon1H Wield Bone");
            Undo.RegisterCreatedObjectUndo(WEAPON1H_REST, "Create Weapon1H Rest Bone");
            Undo.RegisterCreatedObjectUndo(SHIELD_WIELD, "Create Shield Wield Bone");
            Undo.RegisterCreatedObjectUndo(SHIELD_REST, "Create Shield Rest Bone");
            Undo.RegisterCreatedObjectUndo(WEAPON2H_REST, "Create Weapon2H Rest Bone");
            Undo.RegisterCreatedObjectUndo(QUIVER_REST, "Create Quiver Rest Bone");
            Undo.RegisterCreatedObjectUndo(BOW_REST, "Create BowRest Bone");
            Undo.RegisterCreatedObjectUndo(BOW_WIELD, "Create Bow Wield Bone");
            Undo.RegisterCreatedObjectUndo(ARROW, "Create Arrow Bone");
            Undo.RegisterCreatedObjectUndo(SECONDARY_REST, "Create Socindary Weapon Rest Bone");
            Undo.RegisterCreatedObjectUndo(SECONDARY_WIELD, "Create Secondary Weapon Wield Bone");

            WEAPON_WIELD.transform.SetParent(rhand);
            WEAPON1H_REST.transform.SetParent(lHip);
            SHIELD_WIELD.transform.SetParent(lElbow);
            SHIELD_REST.transform.SetParent(chest);
            WEAPON2H_REST.transform.SetParent(chest);
            QUIVER_REST.transform.SetParent(chest);
            BOW_REST.transform.SetParent(chest);
            BOW_WIELD.transform.SetParent(lHand);
            ARROW.transform.SetParent(rIndex);
            SECONDARY_REST.transform.SetParent(rHip);
            SECONDARY_WIELD.transform.SetParent(lHand);


            if (es.bones == null)
                es.bones = new EquipmentBones();

            es.bones.weapon1H_wield_bone = WEAPON_WIELD.transform;
            es.bones.weapon2H_wield_bone = WEAPON_WIELD.transform;

            es.bones.weapon1H_wield_bone.localPosition = Vector3.zero;
            es.bones.weapon1H_wield_bone.localRotation = Quaternion.identity;

            es.bones.weapon1H_rest_bone = WEAPON1H_REST.transform;
            es.bones.weapon1H_rest_bone.localPosition = Vector3.zero;
            es.bones.weapon1H_rest_bone.localRotation = Quaternion.identity;

            es.bones.shield_wield_bone = SHIELD_WIELD.transform;
            es.bones.shield_wield_bone.localPosition = Vector3.zero;
            es.bones.shield_wield_bone.localRotation = Quaternion.identity;

            es.bones.shield_rest_bone = SHIELD_REST.transform;
            es.bones.shield_rest_bone.localPosition = Vector3.zero;
            es.bones.shield_rest_bone.localRotation = Quaternion.identity;

            es.bones.weapon2H_rest_bone = WEAPON2H_REST.transform;
            es.bones.weapon2H_rest_bone.localPosition = Vector3.zero;
            es.bones.weapon2H_rest_bone.localRotation = Quaternion.identity;

            es.bones.quiver_rest_bone = QUIVER_REST.transform;
            es.bones.quiver_rest_bone.localPosition = Vector3.zero;
            es.bones.quiver_rest_bone.localRotation = Quaternion.identity;

            es.bones.bow_rest_bone = BOW_REST.transform;
            es.bones.bow_rest_bone.localPosition = Vector3.zero;
            es.bones.bow_rest_bone.localRotation = Quaternion.identity;

            es.bones.bow_wield_bone = BOW_WIELD.transform;
            es.bones.bow_wield_bone.localPosition = Vector3.zero;
            es.bones.bow_wield_bone.localRotation = Quaternion.identity;

            es.bones.arrow_bone = ARROW.transform;
            es.bones.arrow_bone.localPosition = Vector3.zero;
            es.bones.arrow_bone.localRotation = Quaternion.identity;

            es.bones.secondary1H_rest_bone = SECONDARY_REST.transform;
            es.bones.secondary1H_rest_bone.localPosition = Vector3.zero;
            es.bones.secondary1H_rest_bone.localRotation = Quaternion.identity;

            es.bones.secondary1H_wield_bone = SECONDARY_WIELD.transform;
            es.bones.secondary1H_wield_bone.localPosition = Vector3.zero;
            es.bones.secondary1H_wield_bone.localRotation = Quaternion.identity;

            

        }

        void _createItemPicker()
        {
            if (CharacterType != CharacterTypes.ThirdPerson)
                return;

            ItemPicker ip = character.GetComponent<ItemPicker>();
            if (!ip) ip = Undo.AddComponent<ItemPicker>(character);

            UnityEngine.UI.Text uiText = null;

            // load canvas from resources
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas)
            {
                Transform uiXform = Utils.FindChildTransformByName(canvas.transform, "ItemPickerUI");
                if (uiXform)
                {
                    uiText = uiXform.GetComponent<UnityEngine.UI.Text>();
                }
                else
                {
                    uiText = Resources.Load<UnityEngine.UI.Text>("ItemPickerUI");
                    uiText = Instantiate(uiText);
                    uiText.transform.SetParent(canvas.transform, false);
                    Undo.RegisterCreatedObjectUndo(uiText.gameObject, "Create Text");
                }
            }
            else
            {
                Debug.Log("creating new canvas...");
                Canvas canvasPrefab = Resources.Load<Canvas>("Canvas");
                if (!canvasPrefab)
                {
                    Debug.LogError("Cannot find 'Canvas' prefab!" + " < " + this.ToString() + ">");
                    return;
                }


                canvas = Instantiate(canvasPrefab);

                Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Create Canvas");


                Transform uiXform = Utils.FindChildTransformByName(canvas.transform, "ItemPickerUI");
                if (uiXform)
                {
                    uiText = uiXform.GetComponent<UnityEngine.UI.Text>();
                }
                else
                {
                    uiText = Resources.Load<UnityEngine.UI.Text>("ItemPickerUI");
                    uiText = Instantiate(uiText);
                    uiText.transform.SetParent(canvas.transform, false);
                    Undo.RegisterCreatedObjectUndo(uiText.gameObject, "Create Text");
                }
            }
            canvas.name = "Canvas";
            uiText.name = "ItemPickerUI";

            // load picker indicator image if dont exists
            // first check if exists
            GameObject pImg = GameObject.Find("PickerIndicatorImage");
            if (!pImg)
            {
                UnityEngine.UI.Image pickerImg = Resources.Load<UnityEngine.UI.Image>("PickerIndicatorImage");
                if (pickerImg)
                {
                    pickerImg = Instantiate(pickerImg);
                    pickerImg.transform.SetParent(canvas.transform, false);
                    pickerImg.name = "PickerIndicatorImage";
                    Undo.RegisterCreatedObjectUndo(pickerImg.gameObject, "Create Image");
                }
            }

            Material outline = Resources.Load<Material>("Materials/OutlineStandardSpec");
            if(outline)
            {
                ip.outlineMaterial = outline;
            }

            ip.DisplayUI = uiText;
            ip.pickDistance = 3.0f;
            ip.layers = LayerMask.GetMask(/*"Default", */"DefaultSlope", "Walkable", "Item");
            ip.layers |= 1 << LayerMask.NameToLayer("Default");

        }

#endregion
    } 
}
