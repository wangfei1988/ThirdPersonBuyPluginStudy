// © 2016 Mario Lelas
using UnityEditor;
using UnityEngine;


namespace MLSpace
{

    /// <summary>
    /// Setup project requirements ( tag, layers, axis etc.. )
    /// </summary>
    public class SetupEditor : MonoBehaviour
    {
        [MenuItem("Tools/Character Controller GP/Setup Project Requrements")]
        static void ProjectRequrements()
        {
            EditorUtils.SetDefine("DEBUG_INFO");

            EditorUtils.AddTag("Trigger");
            EditorUtils.AddTag("Collider");
            EditorUtils.AddTag("NPC");

            const int COLLIDERLAYER = 8;
            const int COLLIDERINACTIVELAYER = 9;
            const int TRIGGERLAYER = 10;
            const int PLAYERLAYER = 11;
            const int NPCLAYER = 12;
            const int PROJECTILELAYER = 13;
            const int DEFAULTNOCAM = 14;
            const int DEFAULTSLOPE = 15;
            const int WALKABLE = 16;
            const int ITEM = 17;

            EditorUtils.AddLayer("ColliderLayer", COLLIDERLAYER);
            EditorUtils.AddLayer("ColliderInactiveLayer", COLLIDERINACTIVELAYER);
            EditorUtils.AddLayer("TriggerLayer", TRIGGERLAYER);
            EditorUtils.AddLayer("PlayerLayer", PLAYERLAYER);
            EditorUtils.AddLayer("NPCLayer", NPCLAYER);
            EditorUtils.AddLayer("ProjectileLayer", PROJECTILELAYER);
            EditorUtils.AddLayer("DefaultNoCam", DEFAULTNOCAM);
            EditorUtils.AddLayer("DefaultSlope", DEFAULTSLOPE);
            EditorUtils.AddLayer("Walkable", WALKABLE);
            EditorUtils.AddLayer("Item", ITEM);


            // trigger ignores all layers except npc and player for this case
            for (int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(TRIGGERLAYER, i, true);
                Physics.IgnoreLayerCollision(COLLIDERINACTIVELAYER, i, true);
                Physics.IgnoreLayerCollision(COLLIDERLAYER, i, true);
                Physics.IgnoreLayerCollision(PLAYERLAYER, i, true);
                Physics.IgnoreLayerCollision(NPCLAYER, i, true);
                Physics.IgnoreLayerCollision(PROJECTILELAYER, i, true);
            }
            Physics.IgnoreLayerCollision(TRIGGERLAYER, NPCLAYER, false);
            Physics.IgnoreLayerCollision(TRIGGERLAYER, PLAYERLAYER, false);

            Physics.IgnoreLayerCollision(COLLIDERLAYER, 0, false); // default
            Physics.IgnoreLayerCollision(COLLIDERLAYER, PLAYERLAYER, false);
            Physics.IgnoreLayerCollision(COLLIDERLAYER, NPCLAYER, false);
            Physics.IgnoreLayerCollision(COLLIDERLAYER, DEFAULTNOCAM, false);
            Physics.IgnoreLayerCollision(COLLIDERLAYER, DEFAULTSLOPE, false);

            Physics.IgnoreLayerCollision(PLAYERLAYER, 0, false); // default
            Physics.IgnoreLayerCollision(NPCLAYER, 0, false); // default
            Physics.IgnoreLayerCollision(PLAYERLAYER, COLLIDERLAYER, false);
            Physics.IgnoreLayerCollision(NPCLAYER, COLLIDERLAYER, false);
            Physics.IgnoreLayerCollision(NPCLAYER, NPCLAYER, false);
            Physics.IgnoreLayerCollision(PLAYERLAYER, DEFAULTNOCAM, false);
            Physics.IgnoreLayerCollision(PLAYERLAYER, DEFAULTSLOPE, false);
            Physics.IgnoreLayerCollision(NPCLAYER, DEFAULTNOCAM, false);
            Physics.IgnoreLayerCollision(NPCLAYER, DEFAULTSLOPE, false);
            Physics.IgnoreLayerCollision(PLAYERLAYER, NPCLAYER, false);


            Physics.IgnoreLayerCollision(PROJECTILELAYER, 0, false); // default
            Physics.IgnoreLayerCollision(PROJECTILELAYER, DEFAULTNOCAM, false);
            Physics.IgnoreLayerCollision(PROJECTILELAYER, DEFAULTSLOPE, false);

            Physics.IgnoreLayerCollision(WALKABLE, 0, false); // default
            Physics.IgnoreLayerCollision(WALKABLE, DEFAULTNOCAM, false);
            Physics.IgnoreLayerCollision(WALKABLE, DEFAULTSLOPE, false);
            Physics.IgnoreLayerCollision(WALKABLE, PLAYERLAYER, false);
            Physics.IgnoreLayerCollision(WALKABLE, NPCLAYER, false);
            Physics.IgnoreLayerCollision(WALKABLE, COLLIDERLAYER, false);

            Physics.IgnoreLayerCollision(ITEM, PLAYERLAYER, false);

            InputAxis jumpAxis = new InputAxis();
            jumpAxis.name = "Jump";
            jumpAxis.positiveButton = "q";
            jumpAxis.descriptiveName = "Jump";
            EditorUtils.ChangeAxisByName(jumpAxis);

            InputAxis submitAxis = new InputAxis();
            submitAxis.name = "Submit";
            submitAxis.positiveButton = "return";
            submitAxis.altPositiveButton = "";
            submitAxis.descriptiveName = "Submit";
            EditorUtils.ChangeAxisParameter(submitAxis, "altPositiveButton", submitAxis.altPositiveButton);

            InputAxis crouchAxis = new InputAxis();
            crouchAxis.name = "Crouch";
            crouchAxis.positiveButton = "c";
            crouchAxis.descriptiveName = "Crouch";
            EditorUtils.AddAxis(crouchAxis);


            InputAxis walkToggleAxis = new InputAxis();
            walkToggleAxis.name = "WalkToggle";
            walkToggleAxis.positiveButton = "left shift";
            walkToggleAxis.descriptiveName = "Toggle Walk";
            EditorUtils.AddAxis(walkToggleAxis);

            InputAxis diveRollAxis = new InputAxis();
            diveRollAxis.name = "DiveRoll";
            diveRollAxis.positiveButton = "space";
            diveRollAxis.descriptiveName = "Dive Roll";
            EditorUtils.AddAxis(diveRollAxis);

            InputAxis useAxis = new InputAxis();
            useAxis.name = "Use";
            useAxis.positiveButton = "e";
            useAxis.descriptiveName = "Use";
            EditorUtils.AddAxis(useAxis);

            InputAxis secondaryUse = new InputAxis();
            secondaryUse.name = "SecondaryUse";
            secondaryUse.positiveButton = "r";
            secondaryUse.descriptiveName = "Secondary Use Button";
            EditorUtils.AddAxis(secondaryUse);

            InputAxis pauseAxis = new InputAxis();
            pauseAxis.name = "Pause";
            pauseAxis.positiveButton = "p";
            pauseAxis.descriptiveName = "Pause game";
            EditorUtils.AddAxis(pauseAxis);

            InputAxis blockAxis = new InputAxis();
            blockAxis.name = "Block";
            blockAxis.positiveButton = "f";
            blockAxis.descriptiveName = "Block incoming attacks";
            EditorUtils.AddAxis(blockAxis);

            InputAxis dropAxis = new InputAxis();
            dropAxis.name = "Drop";
            dropAxis.positiveButton = "x";
            dropAxis.descriptiveName = "Drop equipment";
            EditorUtils.AddAxis(dropAxis);

            InputAxis toggleWeaponAxis = new InputAxis();
            toggleWeaponAxis.name = "ToggleWeapon";
            toggleWeaponAxis.positiveButton = "g";
            toggleWeaponAxis.descriptiveName = "Toggle current weapon/s";
            EditorUtils.AddAxis(toggleWeaponAxis);

            InputAxis drawWeaponAxis = new InputAxis();
            drawWeaponAxis.name = "DrawWeapon";
            drawWeaponAxis.positiveButton = "v";
            drawWeaponAxis .descriptiveName = "Draw current weapon/s";
            EditorUtils.AddAxis(drawWeaponAxis);

            InputAxis sheatheWeaponAxis = new InputAxis();
            sheatheWeaponAxis.name = "SheatheWeapon";
            sheatheWeaponAxis.positiveButton = "b";
            sheatheWeaponAxis.descriptiveName = "Sheathe current weapon/s";
            EditorUtils.AddAxis(sheatheWeaponAxis);

            InputAxis weapnShieldModeAxis = new InputAxis();
            weapnShieldModeAxis.name = "WeaponShieldMode";
            weapnShieldModeAxis.positiveButton = "1";
            weapnShieldModeAxis.descriptiveName = "Use weapon and shield combat mode";
            EditorUtils.AddAxis(weapnShieldModeAxis);

            InputAxis weapon2HMode = new InputAxis();
            weapon2HMode.name = "Weapon2HMode";
            weapon2HMode.positiveButton = "2";
            weapon2HMode.descriptiveName = "Use two handed weapon combat mode";
            EditorUtils.AddAxis(weapon2HMode);

            InputAxis bowMode = new InputAxis();
            bowMode.name = "BowMode";
            bowMode.positiveButton = "3";
            bowMode.descriptiveName = "Use bow combat mode";
            EditorUtils.AddAxis(bowMode);

            InputAxis dualWieldMode = new InputAxis();
            dualWieldMode.name = "DualWieldMode";
            dualWieldMode.positiveButton = "4";
            dualWieldMode.descriptiveName = "Use dual wield combat mode";
            EditorUtils.AddAxis(dualWieldMode);



        }

        [MenuItem("Tools/Character Controller GP/Setup Scene Requrements")]
        static void SceneRequrementsDefault()
        {
            EditorUtils.CreateGameControllerObject(false);
            UnityEngine.UI.Text textui = _createUI();
            if(textui == null)
            {
                Debug.LogError("Cannot create 'Trigger UI' component.");
            }


            // assign text ui to trigger manager 
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if(player)
            {
                TriggerManagement tm = player.GetComponent<TriggerManagement>();
                tm.m_TriggerUI = textui;
            }

            Undo.SetCurrentGroupName("Create Scene Requirements");
        }

        [MenuItem("Tools/Character Controller GP/Setup Scene Requrements ( Combat Framework )")]
        static void SceneRequrementsGame()
        {
            EditorUtils.CreateGameControllerObject(true);
            UnityEngine.UI.Text textui = _createUI();
            if (textui == null)
            {
                Debug.LogError("Cannot create 'Trigger UI' component.");
            }
            UnityEngine.UI.Text uiText = _createItemPicker();
            if (uiText == null)
            {
                Debug.LogError("Cannot create 'Item Picker UI' component.");
            }

            // assign text ui to trigger manager 
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                TriggerManagement tm = player.GetComponent<TriggerManagement>();
                tm.m_TriggerUI = textui;

                ItemPicker ip = player.GetComponent<ItemPicker>();
                ip.DisplayUI = uiText;
            }
            Undo.SetCurrentGroupName("Create Scene Requirements ( Game )");
        }

        static UnityEngine.UI.Text _createUI()
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
                canvas = Resources.Load<Canvas>("Canvas");
                canvas = Instantiate(canvas);

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

        static UnityEngine.UI.Text _createItemPicker()
        {
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
                    Debug.LogError("Cannot find 'Canvas' prefab!");
                    return null;
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
            return uiText;
        }
    } 
}
