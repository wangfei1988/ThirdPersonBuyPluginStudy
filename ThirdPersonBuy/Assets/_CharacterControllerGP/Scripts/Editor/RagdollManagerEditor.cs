// © 2016 Mario Lelas
using UnityEngine;
using UnityEditor;

namespace MLSpace
{
    /// <summary>
    /// ragdoll manager inspector editor that adds button to create collider scripts
    /// and hides/shows time float field
    /// </summary>
    [CustomEditor(typeof(RagdollManager))]
    public class RagdollManagerEditor : Editor
    {
        // unity OnInspectorGUI method
        public override void OnInspectorGUI()
        {
            RagdollManager ragMan = (RagdollManager)target;

            DrawDefaultInspector();

            if (ragMan.hitInterval == RagdollManager.HitIntervals.Timed)
            {
                float hitInterval = (float)EditorGUILayout.FloatField("Hit Interval", ragMan.hitTimeInterval);
                ragMan.hitTimeInterval = hitInterval;
            }

            bool enableGetUp = (bool)EditorGUILayout.Toggle("Enable Get Up Animation", ragMan.enableGetUpAnimation);
            ragMan.enableGetUpAnimation = enableGetUp;

            string text = (string)EditorGUILayout.TextField("Name Of Get Up From Back State", ragMan.nameOfGetUpFromBackState);
            ragMan.nameOfGetUpFromBackState = text;

            text = (string)EditorGUILayout.TextField("Name Of Get Up From Front State", ragMan.nameOfGetUpFromFrontState);
            ragMan.nameOfGetUpFromFrontState = text;

            bool ragdollWizard = GUILayout.Button("Ragdoll Wizard");
            if (ragdollWizard)
            {
                RagdollCreatorWizard builderWizard = RagdollCreatorWizard.DisplayWizard();
                RagdollCreator builder = builderWizard.ragdollCreator;
                builder.OnWizardCreateCallback = () =>
                {
                    ragMan.RagdollBones = new Transform[(int)BodyParts.BODY_PART_COUNT];
                    ragMan.RagdollBones[(int)BodyParts.Spine] = builder.pelvis;
                    ragMan.RagdollBones[(int)BodyParts.Chest] = builder.middleSpine;
                    ragMan.RagdollBones[(int)BodyParts.Head] = builder.head;
                    ragMan.RagdollBones[(int)BodyParts.LeftShoulder] = builder.leftArm;
                    ragMan.RagdollBones[(int)BodyParts.LeftElbow] = builder.leftElbow;
                    ragMan.RagdollBones[(int)BodyParts.RightShoulder] = builder.rightArm;
                    ragMan.RagdollBones[(int)BodyParts.RightElbow] = builder.rightElbow;
                    ragMan.RagdollBones[(int)BodyParts.LeftHip] = builder.leftHips;
                    ragMan.RagdollBones[(int)BodyParts.LeftKnee] = builder.leftKnee;
                    ragMan.RagdollBones[(int)BodyParts.RightHip] = builder.rightHips;
                    ragMan.RagdollBones[(int)BodyParts.RightKnee] = builder.rightKnee;


                    EditorUtility.SetDirty(ragMan);
                    serializedObject.ApplyModifiedProperties();

                    RagdollManager.AddBodyColliderScripts(ragMan);

                    builder.OnWizardCreateCallback = null;
                };
            }


            bool removeRagdoll = GUILayout.Button("Remove Ragoll");
            if (removeRagdoll)
            {
                if (ragMan.RagdollBones.Length == (int)BodyParts.BODY_PART_COUNT)
                {
                    for (int i = 0; i < (int)BodyParts.BODY_PART_COUNT; i++)
                    {
                        Transform t = ragMan.RagdollBones[i];
                        if (!t) continue;
                        CharacterJoint[] t_joints = t.GetComponents<CharacterJoint>();
                        Collider[] t_cols = t.GetComponents<Collider>();
                        Rigidbody[] t_rbs = t.GetComponents<Rigidbody>();
                        BodyColliderScript[] t_bcs = t.GetComponents<BodyColliderScript>();
                        foreach (CharacterJoint cj in t_joints)
                            DestroyImmediate(cj);
                        foreach (Collider c in t_cols)
                            DestroyImmediate(c);
                        foreach (Rigidbody rb in t_rbs)
                            DestroyImmediate(rb);
                        foreach (BodyColliderScript b in t_bcs)
                            DestroyImmediate(b);
                        ragMan.RagdollBones[i] = null;
                    }
                    ragMan.RagdollBones = null;
                    EditorUtility.SetDirty(ragMan);
                    serializedObject.ApplyModifiedProperties();
                }
            }



            bool addColSc = GUILayout.Button("Add Collider Scripts");
            if(addColSc )
            {
                RagdollManager.AddBodyColliderScripts(ragMan);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ragMan);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
