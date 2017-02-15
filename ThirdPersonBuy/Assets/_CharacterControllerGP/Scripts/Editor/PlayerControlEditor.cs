// © 2016 Mario Lelas
using UnityEditor;
using UnityEngine;

namespace MLSpace
{
#region PlayerControl

    /// <summary>
    /// PlayerControl editor
    /// </summary>
    [CustomEditor(typeof(PlayerControl))]
    public class PlayerControlEditor : Editor
    {
        private static bool dualWeaponsFoldout = false;
        private static bool showAttackFoldout = true;
        private static bool showBlockStanceFoldout = false;
        private static bool showBlockHitFoldout = false;
        private static bool showLocomotionFoldout = false;
        private static bool showOtherFoldout = false;
        private bool changeBlockHitListSize = false;
        private bool changeLocomotionListSize = false;
        private bool changeOtherClipsListSize = false;

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            PlayerControl pc = (PlayerControl)target;
            GUI.changed = false;

            bool enter = (Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return);

            if(pc.enableJumpToTarget )
            {
                pc.attack_jump_distance = EditorGUILayout.FloatField("Attack jump distance", pc.attack_jump_distance);
            }

            dualWeaponsFoldout = EditorGUILayout.Foldout(dualWeaponsFoldout, "Dual Item Clips");
            if (dualWeaponsFoldout)
            {
                // --- attack ---
                EditorGUI.indentLevel++;
                showAttackFoldout = EditorGUILayout.Foldout(showAttackFoldout, "Attack Clip");
                if (showAttackFoldout)
                {
                    if (pc.dualWeaponAttackClip == null)
                        pc.dualWeaponAttackClip = new AnimationClipReplacementInfo();

                    EditorGUI.indentLevel++;
                    pc.dualWeaponAttackClip.original_name = EditorGUILayout.TextField("Original_name",
                        pc.dualWeaponAttackClip.original_name);
                    pc.dualWeaponAttackClip.clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponAttackClip.clip, typeof(AnimationClip), false);
                    EditorGUI.indentLevel--;
                }

                // --- block stance ---
                showBlockStanceFoldout = EditorGUILayout.Foldout(showBlockStanceFoldout, "Block Stance Clip");
                if (showBlockStanceFoldout)
                {
                    if (pc.dualWeaponBlockStance == null)
                        pc.dualWeaponBlockStance = new AnimationClipReplacementInfo();

                    EditorGUI.indentLevel++;
                    pc.dualWeaponBlockStance.original_name = EditorGUILayout.TextField("Original_name",
                        pc.dualWeaponBlockStance.original_name);
                    pc.dualWeaponBlockStance.clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponBlockStance.clip, typeof(AnimationClip), false);
                    EditorGUI.indentLevel--;
                }

                // --- block hit ---
                showBlockHitFoldout = EditorGUILayout.Foldout(showBlockHitFoldout, "Block Hit Clips");
                if (showBlockHitFoldout)
                {
                    if (pc.dualWeaponBlockHitClips == null)
                        pc.dualWeaponBlockHitClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(pc.dualWeaponBlockHitListSize);

                    EditorGUI.indentLevel++;
                    int prevSize = pc.dualWeaponBlockHitListSize;
                    pc.dualWeaponBlockHitListSize = EditorGUILayout.IntField("Size", pc.dualWeaponBlockHitListSize);
                    if (prevSize != pc.dualWeaponBlockHitListSize) changeBlockHitListSize = true;
                    prevSize = pc.dualWeaponBlockHitListSize;

                    if (enter)
                    {
                        if (changeBlockHitListSize)
                        {
                            if (pc.dualWeaponBlockHitListSize >= pc.dualWeaponBlockHitClips.Count)
                            {
                                for (int i = pc.dualWeaponBlockHitClips.Count; i < pc.dualWeaponBlockHitListSize; i++)
                                {
                                    pc.dualWeaponBlockHitClips.Add(new AnimationClipReplacementInfo());
                                }
                            }
                            if (pc.dualWeaponBlockHitListSize < pc.dualWeaponBlockHitClips.Count)
                            {
                                pc.dualWeaponBlockHitClips.RemoveRange(pc.dualWeaponBlockHitListSize, pc.dualWeaponBlockHitClips.Count - pc.dualWeaponBlockHitListSize);
                            }
                        }
                        changeBlockHitListSize = false;
                    }


                    EditorGUI.indentLevel++;

                    for (int i = 0; i < pc.dualWeaponBlockHitClips.Count; i++)
                    {
                        string labelText =
                            pc.dualWeaponBlockHitClips[i].original_name == null ? "Element" + i.ToString() : pc.dualWeaponBlockHitClips[i].original_name;
                        pc.dualWeaponBlockHitClips[i].inspectorFoldout = EditorGUILayout.Foldout(pc.dualWeaponBlockHitClips[i].inspectorFoldout, labelText);
                        if (pc.dualWeaponBlockHitClips[i].inspectorFoldout)
                        {
                            EditorGUI.indentLevel++;
                            pc.dualWeaponBlockHitClips[i].original_name = EditorGUILayout.TextField("Original_name",
                                pc.dualWeaponBlockHitClips[i].original_name);
                            pc.dualWeaponBlockHitClips[i].clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponBlockHitClips[i].clip,
                                typeof(AnimationClip), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }

                // --- locomotion ----
                showLocomotionFoldout = EditorGUILayout.Foldout(showLocomotionFoldout, "Locomotion Clips");
                if (showLocomotionFoldout)
                {
                    if (pc.dualWeaponLocomotionClips == null)
                        pc.dualWeaponLocomotionClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(pc.dualWeaponBlockHitListSize);

                    EditorGUI.indentLevel++;
                    int prevSize = pc.dualWeaponLocomotionListSize;
                    pc.dualWeaponLocomotionListSize = EditorGUILayout.IntField("Size", pc.dualWeaponLocomotionListSize);
                    if (prevSize != pc.dualWeaponLocomotionListSize) changeLocomotionListSize = true;
                    prevSize = pc.dualWeaponLocomotionListSize;
                    if (enter)
                    {
                        if (changeLocomotionListSize)
                        {
                            if (pc.dualWeaponLocomotionListSize > pc.dualWeaponLocomotionClips.Count)
                            {
                                for (int i = pc.dualWeaponLocomotionClips.Count; i < pc.dualWeaponLocomotionListSize; i++)
                                {
                                    pc.dualWeaponLocomotionClips.Add(new AnimationClipReplacementInfo());
                                }
                            }
                            if (pc.dualWeaponLocomotionListSize < pc.dualWeaponLocomotionClips.Count)
                            {
                                pc.dualWeaponLocomotionClips.RemoveRange(pc.dualWeaponLocomotionListSize, pc.dualWeaponLocomotionClips.Count - pc.dualWeaponLocomotionListSize);
                            }
                        }
                        changeLocomotionListSize = false;
                    }
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < pc.dualWeaponLocomotionClips.Count; i++)
                    {
                        string labelText =
                            pc.dualWeaponLocomotionClips[i].original_name == null ? "Element" + i.ToString() : pc.dualWeaponLocomotionClips[i].original_name;
                        pc.dualWeaponLocomotionClips[i].inspectorFoldout = EditorGUILayout.Foldout(pc.dualWeaponLocomotionClips[i].inspectorFoldout, labelText);
                        if (pc.dualWeaponLocomotionClips[i].inspectorFoldout)
                        {
                            EditorGUI.indentLevel++;
                            pc.dualWeaponLocomotionClips[i].original_name = EditorGUILayout.TextField("Original_name",
                                pc.dualWeaponLocomotionClips[i].original_name);
                            pc.dualWeaponLocomotionClips[i].clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponLocomotionClips[i].clip, typeof(AnimationClip), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }

                // other replacement clips
                showOtherFoldout = EditorGUILayout.Foldout(showOtherFoldout, "Replacement Clips");
                if (showOtherFoldout)
                {
                    if (pc.dualWeaponReplacementClips == null)
                        pc.dualWeaponReplacementClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(pc.dualWeaponReplacementListSize);

                    EditorGUI.indentLevel++;
                    int prevSize = pc.dualWeaponReplacementListSize;
                    pc.dualWeaponReplacementListSize = EditorGUILayout.IntField("Size", pc.dualWeaponReplacementListSize);
                    if (prevSize != pc.dualWeaponReplacementListSize) changeOtherClipsListSize = true;
                    prevSize = pc.dualWeaponReplacementListSize;
                    if (enter)
                    {
                        if (changeOtherClipsListSize)
                        {
                            if (pc.dualWeaponReplacementListSize > pc.dualWeaponReplacementClips.Count)
                            {
                                for (int i = pc.dualWeaponReplacementClips.Count; i < pc.dualWeaponReplacementListSize; i++)
                                {
                                    pc.dualWeaponReplacementClips.Add(new AnimationClipReplacementInfo());
                                }
                            }
                            if (pc.dualWeaponReplacementListSize < pc.dualWeaponReplacementClips.Count)
                            {
                                pc.dualWeaponReplacementClips.RemoveRange(
                                    pc.dualWeaponReplacementListSize,
                                    pc.dualWeaponReplacementClips.Count - pc.dualWeaponReplacementListSize);
                            }
                        }
                        changeOtherClipsListSize = false;
                    }
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < pc.dualWeaponReplacementClips.Count; i++)
                    {
                        string labelText =
                            pc.dualWeaponReplacementClips[i].original_name == null ? "Element" + i.ToString() : pc.dualWeaponReplacementClips[i].original_name;
                        pc.dualWeaponReplacementClips[i].inspectorFoldout = EditorGUILayout.Foldout(pc.dualWeaponReplacementClips[i].inspectorFoldout, labelText);
                        if (pc.dualWeaponReplacementClips[i].inspectorFoldout)
                        {
                            EditorGUI.indentLevel++;
                            pc.dualWeaponReplacementClips[i].original_name = EditorGUILayout.TextField("Original_name",
                                pc.dualWeaponReplacementClips[i].original_name);
                            pc.dualWeaponReplacementClips[i].clip =
                                (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponReplacementClips[i].clip, typeof(AnimationClip), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
            }

            bool loadDefaults = GUILayout.Button("Load Default Dual Weapons Clips");
            if (loadDefaults)
            {
                Utils.LoadDefaultDualWeaponsClips(
                    ref pc.dualWeaponAttackClip,
                    ref pc.dualWeaponBlockStance,
                    ref pc.dualWeaponBlockHitClips,
                    ref pc.dualWeaponLocomotionClips);
                pc.dualWeaponBlockHitListSize = pc.dualWeaponBlockHitClips.Count;
                pc.dualWeaponLocomotionListSize = pc.dualWeaponLocomotionClips.Count;
            }

            if (GUI.changed || enter)
            {
                EditorUtility.SetDirty(pc);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    #endregion


#region PlayerControlTopDown

    /// <summary>
    /// PlayerControlTopDown editor
    /// </summary>
    [CustomEditor(typeof(PlayerControlTopDown))]
    public class PlayerControlTopDownEditor : Editor
    {
        private static bool dualWeaponsFoldout = false;
        private static bool showAttackFoldout = true;
        private static bool showBlockStanceFoldout = false;
        private static bool showBlockHitFoldout = false;
        private static bool showLocomotionFoldout = false;
        private static bool showOtherFoldout = false;
        private bool changeBlockHitListSize = false;
        private bool changeLocomotionListSize = false;
        private bool changeOtherClipsListSize = false;

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            PlayerControlTopDown pc = (PlayerControlTopDown)target;
            GUI.changed = false;

            bool enter = (Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return);


            //    {
            dualWeaponsFoldout = EditorGUILayout.Foldout(dualWeaponsFoldout, "Dual Item Clips");
            if (dualWeaponsFoldout)
            {
                // --- attack ---
                EditorGUI.indentLevel++;
                showAttackFoldout = EditorGUILayout.Foldout(showAttackFoldout, "Attack Clip");
                if (showAttackFoldout)
                {
                    if (pc.dualWeaponAttackClip == null)
                        pc.dualWeaponAttackClip = new AnimationClipReplacementInfo();

                    EditorGUI.indentLevel++;
                    pc.dualWeaponAttackClip.original_name = EditorGUILayout.TextField("Original_name",
                        pc.dualWeaponAttackClip.original_name);
                    pc.dualWeaponAttackClip.clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponAttackClip.clip, typeof(AnimationClip), false);
                    EditorGUI.indentLevel--;
                }

                // --- block stance ---
                showBlockStanceFoldout = EditorGUILayout.Foldout(showBlockStanceFoldout, "Block Stance Clip");
                if (showBlockStanceFoldout)
                {
                    if (pc.dualWeaponBlockStance == null)
                        pc.dualWeaponBlockStance = new AnimationClipReplacementInfo();

                    EditorGUI.indentLevel++;
                    pc.dualWeaponBlockStance.original_name = EditorGUILayout.TextField("Original_name",
                        pc.dualWeaponBlockStance.original_name);
                    pc.dualWeaponBlockStance.clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponBlockStance.clip, typeof(AnimationClip), false);
                    EditorGUI.indentLevel--;
                }

                // --- block hit ---
                showBlockHitFoldout = EditorGUILayout.Foldout(showBlockHitFoldout, "Block Hit Clips");
                if (showBlockHitFoldout)
                {
                    if (pc.dualWeaponBlockHitClips == null)
                        pc.dualWeaponBlockHitClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(pc.dualWeaponBlockHitListSize);

                    EditorGUI.indentLevel++;
                    int prevSize = pc.dualWeaponBlockHitListSize;
                    pc.dualWeaponBlockHitListSize = EditorGUILayout.IntField("Size", pc.dualWeaponBlockHitListSize);
                    if (prevSize != pc.dualWeaponBlockHitListSize) changeBlockHitListSize = true;
                    prevSize = pc.dualWeaponBlockHitListSize;

                    if (enter)
                    {
                        if (changeBlockHitListSize)
                        {
                            if (pc.dualWeaponBlockHitListSize >= pc.dualWeaponBlockHitClips.Count)
                            {
                                for (int i = pc.dualWeaponBlockHitClips.Count; i < pc.dualWeaponBlockHitListSize; i++)
                                {
                                    pc.dualWeaponBlockHitClips.Add(new AnimationClipReplacementInfo());
                                }
                            }
                            if (pc.dualWeaponBlockHitListSize < pc.dualWeaponBlockHitClips.Count)
                            {
                                pc.dualWeaponBlockHitClips.RemoveRange(pc.dualWeaponBlockHitListSize, pc.dualWeaponBlockHitClips.Count - pc.dualWeaponBlockHitListSize);
                            }
                        }
                        changeBlockHitListSize = false;
                    }


                    EditorGUI.indentLevel++;

                    for (int i = 0; i < pc.dualWeaponBlockHitClips.Count; i++)
                    {
                        string labelText =
                            pc.dualWeaponBlockHitClips[i].original_name == null ? "Element" + i.ToString() : pc.dualWeaponBlockHitClips[i].original_name;
                        pc.dualWeaponBlockHitClips[i].inspectorFoldout = EditorGUILayout.Foldout(pc.dualWeaponBlockHitClips[i].inspectorFoldout, labelText);
                        if (pc.dualWeaponBlockHitClips[i].inspectorFoldout)
                        {
                            EditorGUI.indentLevel++;
                            pc.dualWeaponBlockHitClips[i].original_name = EditorGUILayout.TextField("Original_name",
                                pc.dualWeaponBlockHitClips[i].original_name);
                            pc.dualWeaponBlockHitClips[i].clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponBlockHitClips[i].clip,
                                typeof(AnimationClip), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }

                // --- locomotion ----
                showLocomotionFoldout = EditorGUILayout.Foldout(showLocomotionFoldout, "Locomotion Clips");
                if (showLocomotionFoldout)
                {
                    if (pc.dualWeaponLocomotionClips == null)
                        pc.dualWeaponLocomotionClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(pc.dualWeaponBlockHitListSize);

                    EditorGUI.indentLevel++;
                    int prevSize = pc.dualWeaponLocomotionListSize;
                    pc.dualWeaponLocomotionListSize = EditorGUILayout.IntField("Size", pc.dualWeaponLocomotionListSize);
                    if (prevSize != pc.dualWeaponLocomotionListSize) changeLocomotionListSize = true;
                    prevSize = pc.dualWeaponLocomotionListSize;
                    if (enter)
                    {
                        if (changeLocomotionListSize)
                        {
                            if (pc.dualWeaponLocomotionListSize > pc.dualWeaponLocomotionClips.Count)
                            {
                                for (int i = pc.dualWeaponLocomotionClips.Count; i < pc.dualWeaponLocomotionListSize; i++)
                                {
                                    pc.dualWeaponLocomotionClips.Add(new AnimationClipReplacementInfo());
                                }
                            }
                            if (pc.dualWeaponLocomotionListSize < pc.dualWeaponLocomotionClips.Count)
                            {
                                pc.dualWeaponLocomotionClips.RemoveRange(pc.dualWeaponLocomotionListSize, pc.dualWeaponLocomotionClips.Count - pc.dualWeaponLocomotionListSize);
                            }
                        }
                        changeLocomotionListSize = false;
                    }
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < pc.dualWeaponLocomotionClips.Count; i++)
                    {
                        string labelText =
                            pc.dualWeaponLocomotionClips[i].original_name == null ? "Element" + i.ToString() : pc.dualWeaponLocomotionClips[i].original_name;
                        pc.dualWeaponLocomotionClips[i].inspectorFoldout = EditorGUILayout.Foldout(pc.dualWeaponLocomotionClips[i].inspectorFoldout, labelText);
                        if (pc.dualWeaponLocomotionClips[i].inspectorFoldout)
                        {
                            EditorGUI.indentLevel++;
                            pc.dualWeaponLocomotionClips[i].original_name = EditorGUILayout.TextField("Original_name",
                                pc.dualWeaponLocomotionClips[i].original_name);
                            pc.dualWeaponLocomotionClips[i].clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponLocomotionClips[i].clip, typeof(AnimationClip), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }

                // other replacement clips
                showOtherFoldout = EditorGUILayout.Foldout(showOtherFoldout, "Replacement Clips");
                if (showOtherFoldout)
                {
                    if (pc.dualWeaponReplacementClips == null)
                        pc.dualWeaponReplacementClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(pc.dualWeaponReplacementListSize);

                    EditorGUI.indentLevel++;
                    int prevSize = pc.dualWeaponReplacementListSize;
                    pc.dualWeaponReplacementListSize = EditorGUILayout.IntField("Size", pc.dualWeaponReplacementListSize);
                    if (prevSize != pc.dualWeaponReplacementListSize) changeOtherClipsListSize = true;
                    prevSize = pc.dualWeaponReplacementListSize;
                    if (enter)
                    {
                        if (changeOtherClipsListSize)
                        {
                            if (pc.dualWeaponReplacementListSize > pc.dualWeaponReplacementClips.Count)
                            {
                                for (int i = pc.dualWeaponReplacementClips.Count; i < pc.dualWeaponReplacementListSize; i++)
                                {
                                    pc.dualWeaponReplacementClips.Add(new AnimationClipReplacementInfo());
                                }
                            }
                            if (pc.dualWeaponReplacementListSize < pc.dualWeaponReplacementClips.Count)
                            {
                                pc.dualWeaponReplacementClips.RemoveRange(
                                    pc.dualWeaponReplacementListSize,
                                    pc.dualWeaponReplacementClips.Count - pc.dualWeaponReplacementListSize);
                            }
                        }
                        changeOtherClipsListSize = false;
                    }
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < pc.dualWeaponReplacementClips.Count; i++)
                    {
                        string labelText =
                            pc.dualWeaponReplacementClips[i].original_name == null ? "Element" + i.ToString() : pc.dualWeaponReplacementClips[i].original_name;
                        pc.dualWeaponReplacementClips[i].inspectorFoldout = EditorGUILayout.Foldout(pc.dualWeaponReplacementClips[i].inspectorFoldout, labelText);
                        if (pc.dualWeaponReplacementClips[i].inspectorFoldout)
                        {
                            EditorGUI.indentLevel++;
                            pc.dualWeaponReplacementClips[i].original_name = EditorGUILayout.TextField("Original_name",
                                pc.dualWeaponReplacementClips[i].original_name);
                            pc.dualWeaponReplacementClips[i].clip =
                                (AnimationClip)EditorGUILayout.ObjectField("Clip", pc.dualWeaponReplacementClips[i].clip, typeof(AnimationClip), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
            }

            bool loadDefaults = GUILayout.Button("Load Default Dual Weapons Clips");
            if (loadDefaults)
            {
                Utils.LoadDefaultDualWeaponsClips(
                    ref pc.dualWeaponAttackClip,
                    ref pc.dualWeaponBlockStance,
                    ref pc.dualWeaponBlockHitClips,
                    ref pc.dualWeaponLocomotionClips);
                pc.dualWeaponBlockHitListSize = pc.dualWeaponBlockHitClips.Count;
                pc.dualWeaponLocomotionListSize = pc.dualWeaponLocomotionClips.Count;
            }

            if (GUI.changed || enter)
            {
                EditorUtility.SetDirty(pc);
                serializedObject.ApplyModifiedProperties();
            }
        }
    } 

#endregion
}
