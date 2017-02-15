// © 2015 Mario Lelas
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MLSpace
{
    /// <summary>
    /// general purpose void delegate
    /// </summary>
    public delegate void VoidFunc();

    /// <summary>
    /// general purpose void delegate with userdata 
    /// </summary>
    /// <param name="userData"></param>
    public delegate void UserDataFunc(object userData);


    /// <summary>
    /// comparer for check distances in ray cast hits
    /// </summary>
    public class RayHitComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }

    /// <summary>
    /// timed function class
    /// </summary>
    public class TimedFunc
    {
        public float time = 1.0f;
        public float timer = 0.0f;
        public bool repeat = false;
        public VoidFunc func;
    }

    /// <summary>
    /// static utils class
    /// </summary>
    public static class Utils
    {

        /// <summary>
        /// try to find child transforms by name
        /// </summary>
        /// <param name="xform">transform</param>
        /// <param name="name">name to find</param>
        /// <returns>result transform</returns>
        public static Transform FindChildTransformByName(Transform xform, string name)
        {
            if (xform.name == name) return xform;
            else
            {
                Transform t;
                for (int i = 0; i < xform.childCount; ++i)
                {
                    t = FindChildTransformByName(xform.GetChild(i), name);
                    if (t != null) return t;
                }
            }
            return null;
        }

        /// <summary>
        /// try to find child transforms by tag
        /// </summary>
        /// <param name="xform">transform</param>
        /// <param name="name">tag to find</param>
        /// <returns>result transform</returns>
        public static Transform FindChildTransformByTag(Transform xform, string tag)
        {
            if (xform.tag == tag) return xform;
            else
            {
                Transform t;
                for (int i = 0; i < xform.childCount; ++i)
                {
                    t = FindChildTransformByTag(xform.GetChild(i), tag);
                    if (t != null) return t;
                }
            }
            return null;
        }

        /// <summary>
        /// list childs of transform
        /// </summary>
        /// <param name="xform"></param>
        /// <param name="names"></param>
        public static void ListChildrenNames(Transform xform, System.Collections.Generic.List<string> names)
        {
            if (xform == null)
            {
                names.Add("NULL");
            }
            else
            {
                names.Add(xform.name);
                for (int i = 0; i < xform.childCount; ++i)
                {
                    ListChildrenNames(xform.GetChild(i), names);
                }
            }
        }

        /// <summary>
        /// get screen rectangle of RectTransform
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        public static Rect GetScreenRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float xMin = float.PositiveInfinity, xMax = float.NegativeInfinity, yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
            for (int i = 0; i < 4; ++i)
            {
                // For Canvas mode Screen Space - Overlay there is no Camera; best solution I've found
                // is to use RectTransformUtility.WorldToScreenPoint) with a null camera.
                Vector3 screenCoord = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                if (screenCoord.x < xMin) xMin = screenCoord.x;
                if (screenCoord.x > xMax) xMax = screenCoord.x;
                if (screenCoord.y < yMin) yMin = screenCoord.y;
                if (screenCoord.y > yMax) yMax = screenCoord.y;
                corners[i] = screenCoord;
            }
            Rect result = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            return result;
        }

        /// <summary>
        /// check if game object is prefab
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsPrefab(GameObject obj)
        {
#if UNITY_EDITOR
            bool isPrefab = PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab; 
            return isPrefab;
#else
            return false;
#endif
        }

        /// <summary>
        /// name said it all
        /// </summary>
        /// <param name="mask">layer mask</param>
        /// <param name="layer">layer</param>
        /// <returns>true or false</returns>
        public static bool DoesMaskContainsLayer(LayerMask mask, int layer)
        {
            return (mask == (mask | (1 << layer)));
        }

        /// <summary>
        /// calculate transform line points
        /// </summary>
        /// <param name="xform">transform from which line is calculated</param>
        /// <param name="pt1">out point 1 of line</param>
        /// <param name="pt2">out point 2 of line</param>
        /// <param name="zoffset">offset from middle in z direction</param>
        /// <param name="reversed">is reversed ?</param>
        public static void CalculateLinePoints(Transform xform, out Vector3 pt1, out Vector3 pt2, float zoffset, bool reversed = false)
        {
            float scaleX = xform.lossyScale.x;
            float halfScaleX = scaleX * 0.5f;
            pt1 = xform.position - xform.right * halfScaleX;
            pt2 = xform.position + xform.right * halfScaleX;

            Vector3 dir = (reversed ? -xform.forward : xform.forward);

            pt1 = pt1 + dir * zoffset;
            pt2 = pt2 + dir * zoffset;
        }

        /// <summary>
        /// calculate direction axis of transform
        /// </summary>
        /// <param name="xform">transform</param>
        /// <param name="direction">out axis direction</param>
        /// <param name="distance">out axis length</param>
        /// <returns>axis vector</returns>
        public static Vector3 CalculateDirection(Transform xform,out int direction,out float length)
        {
            Transform parent = xform.parent;
            Vector3 point = (xform.position - parent.position) + xform.position;
            point = xform.InverseTransformPoint(point);

            Vector3 axis = new Vector3(1.0f, 0.0f, 0.0f);

            // Calculate longest axis
            direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            {
                direction = 1;
                axis = new Vector3(0.0f, 1.0f, 0.0f);
            }
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            {
                direction = 2;
                axis = new Vector3(0.0f, 0.0f, 1.0f);
            }

            length = point[direction];
            axis *= Mathf.Sign(length);
            return axis;
        }

        /// <summary>
        /// get direction axis of transform
        /// </summary>
        /// <param name="xform">transform</param>
        /// <returns></returns>
        public static Vector3 CalculateDirectionAxis(Transform xform)
        {
            Transform parent = xform.parent;
            Vector3 point = (xform.position - parent.position) + xform.position;
            point = xform.InverseTransformPoint(point);

            Vector3 axis = new Vector3(1.0f, 0.0f, 0.0f);

            // Calculate longest axis
            int direction = 0;
            float length = 0.0f;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            {
                direction = 1;
                axis = new Vector3(0.0f, 1.0f, 0.0f);
            }
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            {
                direction = 2;
                axis = new Vector3(0.0f, 0.0f, 1.0f);
            }

            length = point[direction];
            axis *= Mathf.Sign(length);

            return axis;
        }

        /// <summary>
        /// get direction axis of transform
        /// </summary>
        /// <param name="xform">transform</param>
        /// <param name="parent">parent</param>
        /// <returns></returns>
        public static Vector3 CalculateDirectionAxis(Transform xform, Transform parent)
        {
            Vector3 point = (xform.position - parent.position) + xform.position;
            point = xform.InverseTransformPoint(point);

            Vector3 axis = new Vector3(1.0f, 0.0f, 0.0f);

            // Calculate longest axis
            int direction = 0;
            float length = 0.0f;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            {
                direction = 1;
                axis = new Vector3(0.0f, 1.0f, 0.0f);
            }
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            {
                direction = 2;
                axis = new Vector3(0.0f, 0.0f, 1.0f);
            }

            length = point[direction];
            axis *= Mathf.Sign(length);
            return axis;
        }

#if UNITY_EDITOR

        /// <summary>
        /// load default one handed weapon animation clips
        /// </summary>
        /// <param name="weaponItem">weapon item to assign clips to</param>
        public static void LoadDefaultWeapon1HSoundsAndClips(MeleeWeaponItem weaponItem)
        {

            WeaponAudio wa = weaponItem.GetComponent<WeaponAudio>();
            if (!wa) wa = Undo.AddComponent<WeaponAudio>(weaponItem.gameObject);

            AudioClip swoosh1 = Resources.Load<AudioClip>("Audio/swoosh1");
            AudioClip swoosh2 = Resources.Load<AudioClip>("Audio/swoosh2");
            AudioClip metal_clang = Resources.Load<AudioClip>("Audio/metal_clang");
            AudioClip sword_hit = Resources.Load<AudioClip>("Audio/blade_slash_hit");

            if (!swoosh1) Debug.LogWarning("Unable to load clip: 'Audio/swoosh1'");
            if (!swoosh2) Debug.LogWarning("Unable to load clip: 'Audio/swoosh2'");
            if (!metal_clang) Debug.LogWarning("Unable to load clip: 'Audio/metal_clang'");
            if (!sword_hit) Debug.LogWarning("Unable to load clip: 'Audio/blade_slash_hit'");

            wa.attackSwingSounds = new AudioClip[] { swoosh1, swoosh2 };
            wa.attackHitSounds = new AudioClip[] { sword_hit };
            wa.blockSounds = new AudioClip[] { metal_clang };



            AnimationClip defaultAttack = null;
            AnimationClip defaultBlock = null;
            System.Collections.Generic.List<AnimationClip> blockHitClips =
                new System.Collections.Generic.List<AnimationClip>();


            AnimationClip[] onehandedclips = Resources.LoadAll<AnimationClip>("Animations/c_sword&shield_03");
            AnimationClip[] onehandedblockclips = Resources.LoadAll<AnimationClip>("Animations/c_WeaponBlocks");

            if (onehandedclips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/c_sword&shield_03");
            if (onehandedblockclips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/c_WeaponBlocks");

            foreach (AnimationClip clip in onehandedclips)
            {
                if (clip.name == "Sword&Shield03aFast") defaultAttack = clip;
            }
            foreach (AnimationClip clip in onehandedblockclips)
            {
                if (clip.name == "c_Weapon1HBlockStance") defaultBlock = clip;
                if (clip.name.StartsWith("c_Weapon1HBlockHit"))
                    blockHitClips.Add(clip);
            }

            if (defaultAttack)
            {
                weaponItem.attackClip.original_name = "default_attack_walk_fast";
                weaponItem.attackClip.clip = defaultAttack;
            }
            if (defaultBlock)
            {
                weaponItem.blockClip.original_name = "default_block_stance";
                weaponItem.blockClip.clip = defaultBlock;
            }
            weaponItem.blockHitClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(5);
            for (int i = 0; i < blockHitClips.Count; i++)
            {
                AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                info.original_name = "default_block_hit" + i;
                info.clip = blockHitClips[i];
                weaponItem.blockHitClips.Add(info);
            }

            AnimationClip[] locomotionClips = Resources.LoadAll<AnimationClip>("Animations/DefaultLocomotions/c_OneItem_Locomotion");
            if (locomotionClips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/DefaultLocomotions/c_LocomotionItem1H");

            weaponItem.locomotionClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(15);

            for (int i = 0; i < locomotionClips.Length; i++)
            {
                AnimationClip clip = locomotionClips[i];
                if (!clip) continue;
                if (clip.name == "c_Item1H_Idle")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Idle";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if(clip.name == "c_Item1H_Run")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Run";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_Walk")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Walk";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_TurnRightQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightQuarter";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_TurnRightHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightHalf";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_TurnLeftQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftQuarter";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_TurnLeftHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftHalf";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_WalkRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_WalkLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_WalkRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_WalkLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_RunRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_RunLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_RunRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item1H_RunLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
            }
        }

        /// <summary>
        /// load default two handed weapon animation clips
        /// </summary>
        /// <param name="weaponItem">weapon item to assign clips to</param>
        public static void LoadDefaultWeapon2HSoundsAndClips(MeleeWeaponItem weaponItem)
        {
            WeaponAudio wa = weaponItem.GetComponent<WeaponAudio>();
            if (!wa) wa = Undo.AddComponent<WeaponAudio>(weaponItem.gameObject);


            AudioClip swoosh1 = Resources.Load<AudioClip>("Audio/swoosh1");
            AudioClip swoosh2 = Resources.Load<AudioClip>("Audio/swoosh2");
            AudioClip metal_clang = Resources.Load<AudioClip>("Audio/metal_clang");
            AudioClip sword_hit = Resources.Load<AudioClip>("Audio/blade_slash_hit");

            if (!swoosh1) Debug.LogWarning("Unable to load clip: 'Audio/swoosh1'");
            if (!swoosh2) Debug.LogWarning("Unable to load clip: 'Audio/swoosh2'");
            if (!metal_clang) Debug.LogWarning("Unable to load clip: 'Audio/metal_clang'");
            if (!sword_hit) Debug.LogWarning("Unable to load clip: 'Audio/blade_slash_hit'");

            wa.attackSwingSounds = new AudioClip[] { swoosh1, swoosh2 };
            wa.attackHitSounds = new AudioClip[] { sword_hit };
            wa.blockSounds = new AudioClip[] { metal_clang };



            AnimationClip defaultAttack = null;
            AnimationClip defaultBlock = null;
            System.Collections.Generic.List<AnimationClip> blockHitClips =
                new System.Collections.Generic.List<AnimationClip>();

            AnimationClip[] twohandedclips = Resources.LoadAll<AnimationClip>("Animations/c_Weapon2H");
            if (twohandedclips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/c_Weapon2H");

            foreach (AnimationClip clip in twohandedclips)
            {
                if (clip.name == "c_Weapon2HCombo01") defaultAttack = clip;
                if (clip.name == "c_Weapon2HBlockStance01") defaultBlock = clip;
                if (clip.name.StartsWith("c_W2HBlockHit"))
                    blockHitClips.Add(clip);
            }

            if (defaultAttack)
            {
                weaponItem.attackClip.original_name = "default_attack_walk_fast";
                weaponItem.attackClip.clip = defaultAttack;
            }
            if (defaultBlock)
            {
                weaponItem.blockClip.original_name = "default_block_stance";
                weaponItem.blockClip.clip = defaultBlock;
            }
            weaponItem.blockHitClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(5);
            for (int i = 0; i < blockHitClips.Count; i++)
            {
                AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                info.original_name = "default_block_hit" + i;
                info.clip = blockHitClips[i];
                weaponItem.blockHitClips.Add(info);
            }

            AnimationClip[] locomotionClips = Resources.LoadAll<AnimationClip>("Animations/DefaultLocomotions/c_TwoHanded_Locomotion");
            if (locomotionClips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/DefaultLocomotions/c_Locomotion2HMain");

            weaponItem.locomotionClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(15);

            for (int i = 0; i < locomotionClips.Length; i++)
            {
                AnimationClip clip = locomotionClips[i];
                if (!clip) continue;
                if (clip.name == "c_Item2H_Idle")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Idle";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_Walk")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Walk";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_Run")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Run";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_TurnRightQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightQuarter";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_TurnRightHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightHalf";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_TurnLeftQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftQuarter";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_TurnLeftHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftHalf";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_WalkRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_WalkLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_WalkRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_WalkLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_RunRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_RunLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftSharp";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_RunRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_Item2H_RunLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftWide";
                    info.clip = clip;
                    weaponItem.locomotionClips.Add(info);
                }
            }
        }

        /// <summary>
        /// load default shield animation clips
        /// </summary>
        /// <param name="weaponItem">shield item to assign clips to</param>
        public static void LoadDefaultShieldSoundsAndClips(ShieldItem shieldItem)
        {
            WeaponAudio wa = shieldItem.GetComponent<WeaponAudio>();
            if (!wa) wa = Undo.AddComponent<WeaponAudio>(shieldItem.gameObject);



            AudioClip swoosh1 = Resources.Load<AudioClip>("Audio/swoosh1");
            AudioClip swoosh2 = Resources.Load<AudioClip>("Audio/swoosh2");
            AudioClip metal_clang = Resources.Load<AudioClip>("Audio/metal_clang");
            AudioClip smack1 = Resources.Load<AudioClip>("Audio/smack1");
            AudioClip smack2 = Resources.Load<AudioClip>("Audio/smack2");
            AudioClip smack3 = Resources.Load<AudioClip>("Audio/smack3");

            if (!swoosh1) Debug.LogWarning("Unable to load clip: 'Audio/swoosh1'");
            if (!swoosh2) Debug.LogWarning("Unable to load clip: 'Audio/swoosh2'");
            if (!metal_clang) Debug.LogWarning("Unable to load clip: 'Audio/metal_clang'");
            if (!smack1) Debug.LogWarning("Unable to load clip: 'Audio/smack1'");
            if (!smack2) Debug.LogWarning("Unable to load clip: 'Audio/smack2'");
            if (!smack3) Debug.LogWarning("Unable to load clip: 'Audio/smack3'");

            wa.attackSwingSounds = new AudioClip[] { swoosh1, swoosh2 };
            wa.attackHitSounds = new AudioClip[] { smack1, smack2, smack3 };
            wa.blockSounds = new AudioClip[] { metal_clang };


            AnimationClip defaultAttack = null;
            AnimationClip defaultBlock = null;
            System.Collections.Generic.List<AnimationClip> blockHitClips =
                new System.Collections.Generic.List<AnimationClip>();


            AnimationClip[] onehandedclips = Resources.LoadAll<AnimationClip>("Animations/c_sword&shield_03");
            if (onehandedclips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/c_sword&shield_03");

            foreach (AnimationClip clip in onehandedclips)
            {
                if (clip.name == "ShieldAttack") defaultAttack = clip;
                if (clip.name == "ShieldBlock1") defaultBlock = clip;
                if (clip.name.StartsWith("ShieldBlockHit"))
                    blockHitClips.Add(clip);
            }

            if (defaultAttack)
            {
                shieldItem.attackClip.original_name = "default_attack_walk_fast";
                shieldItem.attackClip.clip = defaultAttack;
            }
            if (defaultBlock)
            {
                shieldItem.blockClip.original_name = "default_block_stance";
                shieldItem.blockClip.clip = defaultBlock;
            }
            shieldItem.blockHitClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(5);
            for (int i = 0; i < blockHitClips.Count; i++)
            {
                AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                info.original_name = "default_block_hit" + i;
                info.clip = blockHitClips[i];
                shieldItem.blockHitClips.Add(info);
            }

            Debug.LogWarning("NEED TO CREATE SECONDARY HAND LOCOMOTION ANIMATIONS !!!");
            AnimationClip[] locomotionClips = Resources.LoadAll<AnimationClip>("Animations/DefaultLocomotions/c_SecondaryItemLocomotion");
            if (locomotionClips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/DefaultLocomotions/c_SecondaryItemLocomotion");

            shieldItem.locomotionClips = new System.Collections.Generic.List<AnimationClipReplacementInfo>(15);


            for (int i = 0; i < locomotionClips.Length; i++)
            {
                AnimationClip clip = locomotionClips[i];
                if (!clip) continue;
                if (clip.name == "c_SecItem_Idle")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Idle";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_Run")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Run";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_Walk")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Walk";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_TurnRightQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightQuarter";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_TurnRightHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightHalf";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_TurnLeftQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftQuarter";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_TurnLeftHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftHalf";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_WalkRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightSharp";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_WalkLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftSharp";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_WalkRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightWide";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_WalkLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftWide";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_RunRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightSharp";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_RunLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftSharp";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_RunRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightWide";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
                else if (clip.name == "c_SecItem_RunLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftWide";
                    info.clip = clip;
                    shieldItem.locomotionClips.Add(info);
                }
            }
        }

        /// <summary>
        /// load defualt dual item animation clips
        /// </summary>
        /// <param name="attackClip">attack clip info</param>
        /// <param name="blockStanceClip">block stance info</param>
        /// <param name="blockHitClipsInfo">block hit info list</param>
        /// <param name="locomotionClipsInfo">locomotion info list</param>
        public static void LoadDefaultDualWeaponsClips(
            ref AnimationClipReplacementInfo attackClip,
            ref AnimationClipReplacementInfo blockStanceClip,
            ref List<AnimationClipReplacementInfo> blockHitClipsInfo,
            ref List<AnimationClipReplacementInfo> locomotionClipsInfo
            )
        {
            AnimationClip defaultAttack = null;
            AnimationClip defaultBlock = null;
            System.Collections.Generic.List<AnimationClip> blockHitClips =
                new System.Collections.Generic.List<AnimationClip>();

            AnimationClip[] dualwieldattack = Resources.LoadAll<AnimationClip>("Animations/c_DualWieldAttack");
            if (dualwieldattack.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/c_DualWieldAttack");

            AnimationClip[] blockclips = Resources.LoadAll<AnimationClip>("Animations/c_WeaponBlocks");
            if (blockclips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/c_WeaponBlocks");


            foreach (AnimationClip clip in dualwieldattack)
            {
                if (clip.name == "c_DualWieldAttackComboFast01") defaultAttack = clip;
            }
            foreach (AnimationClip clip in blockclips)
            {
                if (clip.name == "c_Weapon1HBlockStance") defaultBlock = clip;
                if (clip.name.StartsWith("c_Weapon1HBlockHit"))
                    blockHitClips.Add(clip);
            }

            if (defaultAttack)
            {
                attackClip = new AnimationClipReplacementInfo();
                attackClip.original_name = "default_attack_walk_fast";
                attackClip.clip = defaultAttack;
            }
            if (defaultBlock)
            {
                blockStanceClip = new AnimationClipReplacementInfo();
                blockStanceClip.original_name = "default_block_stance";
                blockStanceClip.clip = defaultBlock;
            }
            blockHitClipsInfo = new List<AnimationClipReplacementInfo>(5);
            for (int i = 0; i < blockHitClips.Count; i++)
            {
                AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                info.original_name = "default_block_hit" + i;
                info.clip = blockHitClips[i];
                blockHitClipsInfo.Add(info);
            }

            AnimationClip[] locomotionClips = Resources.LoadAll<AnimationClip>("Animations/DefaultLocomotions/c_DualItem_Locomotion");
            if (locomotionClips.Length == 0) Debug.LogWarning("Cannot find animations from 'Resources/Animations/DefaultLocomotions/c_DualItem_Locomotion");

            locomotionClipsInfo = new System.Collections.Generic.List<AnimationClipReplacementInfo>(15);

            for (int i = 0; i < locomotionClips.Length; i++)
            {
                AnimationClip clip = locomotionClips[i];
                if (!clip) continue;
                if (clip.name == "c_DualItem_Idle")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Idle";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_Run")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Run";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_Walk")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_Walk";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_TurnRightQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightQuarter";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_TurnRightHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnRightHalf";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_TurnLeftQuarter")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftQuarter";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_TurnLeftHalf")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_TurnLeftHalf";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_WalkRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightSharp";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_WalkLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftSharp";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_WalkRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkRightWide";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_WalkLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_WalkLeftWide";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_RunRightSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightSharp";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_RunLeftSharp")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftSharp";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_RunRightWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunRightWide";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
                else if (clip.name == "c_DualItem_RunLeftWide")
                {
                    AnimationClipReplacementInfo info = new AnimationClipReplacementInfo();
                    info.original_name = "Default_RunLeftWide";
                    info.clip = clip;
                    locomotionClipsInfo.Add(info);
                }
            }




        }

#endif



    }
}
