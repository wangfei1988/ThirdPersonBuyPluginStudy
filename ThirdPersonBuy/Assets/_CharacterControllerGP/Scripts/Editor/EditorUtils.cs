// © 2015 Mario Lelas
#define EDITOR_UTILS_INFO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MLSpace
{
    /// <summary>
    /// control axis information
    /// </summary>
    public class InputAxis
    {
        public enum AxisType
        {
            KeyOrMouseButton = 0,
            MouseMovement = 1,
            JoystickAxis = 2
        };

        public string name;
        public string descriptiveName;
        public string descriptiveNegativeName;
        public string negativeButton;
        public string positiveButton;
        public string altNegativeButton;
        public string altPositiveButton;

        public float gravity;
        public float dead;
        public float sensitivity;

        public bool snap = false;
        public bool invert = false;

        public AxisType type;

        public int axis;
        public int joyNum;
    }

    /// <summary>
    /// editor utilities
    /// </summary>
    public class EditorUtils
    {
        /// <summary>
        /// add tag
        /// </summary>
        /// <param name="tag">tag name</param>
        public static void AddTag(string tag)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    {
#if EDITOR_UTILS_INFO
                    Debug.Log("tag "+tag + " already defined");
#endif
                        return;     // Tag already present, nothing to do.
                    }
                }


                int numTags = tags.arraySize;
                tags.InsertArrayElementAtIndex(numTags );
                tags.GetArrayElementAtIndex(numTags ).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();

                Debug.Log("tag ' " + tag + "' added at index " + (numTags ) + ".");
            }
        }

        /// <summary>
        /// set layer at index
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        public static void AddLayer(string layer, int index)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty layers = so.FindProperty("layers");

                if (layers.GetArrayElementAtIndex(index).stringValue == layer)
                {
#if EDITOR_UTILS_INFO
                Debug.Log("layer " + layer + " already defined");
#endif
                    return;     // layer already present, nothing to do.
                }

                layers.InsertArrayElementAtIndex(index);
                layers.GetArrayElementAtIndex(index).stringValue = layer;
                so.ApplyModifiedProperties();
                so.Update();

                Debug.Log("layer ' " + layer + "' added at index " + index + ".");
            }
        }

        /// <summary>
        /// add input axis
        /// </summary>
        /// <param name="axis"></param>
        public static void AddAxis(InputAxis axis)
        {
            if (AxisDefined(axis.name))
            {
#if EDITOR_UTILS_INFO
            Debug.Log("axis '" + axis.name + "' already defined.");
#endif
                return;
            }

            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);

            GetChildProperty(axisProperty, "m_Name").stringValue = axis.name;
            GetChildProperty(axisProperty, "descriptiveName").stringValue = axis.descriptiveName;
            GetChildProperty(axisProperty, "descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            GetChildProperty(axisProperty, "negativeButton").stringValue = axis.negativeButton;
            GetChildProperty(axisProperty, "positiveButton").stringValue = axis.positiveButton;
            GetChildProperty(axisProperty, "altNegativeButton").stringValue = axis.altNegativeButton;
            GetChildProperty(axisProperty, "altPositiveButton").stringValue = axis.altPositiveButton;
            GetChildProperty(axisProperty, "gravity").floatValue = axis.gravity;
            GetChildProperty(axisProperty, "dead").floatValue = axis.dead;
            GetChildProperty(axisProperty, "sensitivity").floatValue = axis.sensitivity;
            GetChildProperty(axisProperty, "snap").boolValue = axis.snap;
            GetChildProperty(axisProperty, "invert").boolValue = axis.invert;
            GetChildProperty(axisProperty, "type").intValue = (int)axis.type;
            GetChildProperty(axisProperty, "axis").intValue = axis.axis - 1;
            GetChildProperty(axisProperty, "joyNum").intValue = axis.joyNum;

            serializedObject.ApplyModifiedProperties();

            Debug.Log("axis ' " + axis.name + "' added.");
        }

        /// <summary>
        /// change existing axis by name ( first found )
        /// </summary>
        /// <param name="axis"></param>
        public static void ChangeAxisByName(InputAxis axis)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            int index = getIndexOfPropertyByName(axesProperty, axis.name);
            if (index < 0 || index >= axesProperty.arraySize)
            {
                Debug.LogWarning("Change Axis failed. Index out of range.");
                return;
            }
            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(index);

            GetChildProperty(axisProperty, "m_Name").stringValue = axis.name;
            GetChildProperty(axisProperty, "descriptiveName").stringValue = axis.descriptiveName;
            GetChildProperty(axisProperty, "descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            GetChildProperty(axisProperty, "negativeButton").stringValue = axis.negativeButton;
            GetChildProperty(axisProperty, "positiveButton").stringValue = axis.positiveButton;
            GetChildProperty(axisProperty, "altNegativeButton").stringValue = axis.altNegativeButton;
            GetChildProperty(axisProperty, "altPositiveButton").stringValue = axis.altPositiveButton;
            GetChildProperty(axisProperty, "gravity").floatValue = axis.gravity;
            GetChildProperty(axisProperty, "dead").floatValue = axis.dead;
            GetChildProperty(axisProperty, "sensitivity").floatValue = axis.sensitivity;
            GetChildProperty(axisProperty, "snap").boolValue = axis.snap;
            GetChildProperty(axisProperty, "invert").boolValue = axis.invert;
            GetChildProperty(axisProperty, "type").intValue = (int)axis.type;
            GetChildProperty(axisProperty, "axis").intValue = axis.axis - 1;
            GetChildProperty(axisProperty, "joyNum").intValue = axis.joyNum;

            serializedObject.ApplyModifiedProperties();

            Debug.Log("axis ' " + axis.name + "' changed by name.");
        }


        /// <summary>
        /// change parameter of existing axis if has the same name of input axis
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="parameter"></param>
        /// <param name="parameterValue"></param>
        public static void ChangeAxisParameter(InputAxis axis, string parameter, string parameterValue)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            int[] indices = getAllIndicesByName(axesProperty, axis.name);
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                if (index < 0 || index >= axesProperty.arraySize)
                {
                    Debug.LogWarning("Change Axis failed. Index out of range.");
                    continue;
                }
                SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(index);
                if (GetChildProperty(axisProperty, "m_Name").stringValue != axis.name) continue;


                GetChildProperty(axisProperty, parameter).stringValue = parameterValue;
                serializedObject.ApplyModifiedProperties();

                Debug.Log("axis ' " + axis.name + "' changed by name.");
            }
        }

        /// <summary>
        /// find index of property by name
        /// </summary>
        /// <param name="axesProperty"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int getIndexOfPropertyByName(SerializedProperty axesProperty, string name)
        {
            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(i);
                string n = GetChildProperty(axisProperty, "m_Name").stringValue;
                if (n == name)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// find all indices of properties with the same name
        /// </summary>
        /// <param name="axesProperty"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int[] getAllIndicesByName(SerializedProperty axesProperty, string name)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(i);
                string n = GetChildProperty(axisProperty, "m_Name").stringValue;
                if (n == name)
                {
                    indices.Add(i);
                }
            }
            return indices.ToArray();
        }



        /// <summary>
        /// get child property
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
        {
            SerializedProperty child = parent.Copy();
            child.Next(true);
            do
            {
                if (child.name == name) return child;
            }
            while (child.Next(false));
            return null;
        }

        /// <summary>
        /// is axis already defined ?
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        private static bool AxisDefined(string axisName,bool change=false)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");
        

            axesProperty.Next(true);
            axesProperty.Next(true);
            while (axesProperty.Next(false))
            {
                SerializedProperty axis = axesProperty.Copy();       
                axis.Next(true);
                if (axis.stringValue == axisName)
                {
                    return true;
                }
            }
            return false;
        }


        private static BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[]
    {
                BuildTargetGroup.Standalone,
                //BuildTargetGroup.WebPlayer,
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
                //BuildTargetGroup.WP8,
                //BuildTargetGroup.BlackBerry
    };

        private static BuildTargetGroup[] mobileBuildTargetGroups = new BuildTargetGroup[]
            {
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
                //BuildTargetGroup.WP8,
                //BuildTargetGroup.BlackBerry,
                //BuildTargetGroup.PSM,
                BuildTargetGroup.Tizen,
                BuildTargetGroup.WSA
            };


        /// <summary>
        /// get current defines list
        /// </summary>
        /// <param name="group">BuildTargetGroup group</param>
        /// <returns>defines list</returns>
        private static List<string> GetDefinesList(BuildTargetGroup group)
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }

        /// <summary>
        /// set and enable define
        /// </summary>
        /// <param name="defineName"></param>
        /// <param name="enable"></param>
        /// <param name="mobile"></param>
        private static void SetEnabled(string defineName, bool enable, bool mobile)
        {
            foreach (var group in mobile ? mobileBuildTargetGroups : buildTargetGroups)
            {
                var defines = GetDefinesList(group);
                if (enable)
                {
                    if (defines.Contains(defineName))
                    {
                        return;
                    }
                    defines.Add(defineName);
                }
                else
                {
                    if (!defines.Contains(defineName))
                    {
                        return;
                    }
                    while (defines.Contains(defineName))
                    {
                        defines.Remove(defineName);
                    }
                }
                string definesString = string.Join(";", defines.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
            }
        }

        /// <summary>
        /// set define macro
        /// </summary>
        /// <param name="defineText"></param>
        public static void SetDefine(string defineText)
        {
            var defines = GetDefinesList(buildTargetGroups[0]);
            if (!defines.Contains(defineText))
            {
                SetEnabled(defineText, true, false);
                SetEnabled(defineText, true, true);
            }
        }

        /// <summary>
        /// create game controller object
        /// </summary>
        /// <param name="extended">default or extended</param>
        /// <returns>game controller object</returns>
        public static GameObject CreateGameControllerObject(bool extended)
        {
            GameObject gc = GameObject.FindGameObjectWithTag("GameController");
            if (!gc)
            {
                if (!extended)
                {
                    gc = Resources.Load<GameObject>("GAMECONTROLLER_DEFAULT");
                    gc = Object.Instantiate(gc);
                    gc.name = "GAMECONTROLLER_DEFAULT";
                }
                else
                {
                    gc = Resources.Load<GameObject>("GAMECONTROLLER");
                    gc = Object.Instantiate(gc);
                    gc.name = "GAMECONTROLLER";
                }
                Undo.RegisterCreatedObjectUndo(gc.gameObject, "Create GAMECONTROLLER");
                
            }
            else
            {
                if(extended )
                {
                    NPCManager npcMan = gc.GetComponent<NPCManager>();
                    if (!npcMan) npcMan = Undo.AddComponent<NPCManager>(gc);

                    ArrowPool ap = gc.GetComponent<ArrowPool>();
                    if (!ap) ap = Undo.AddComponent<ArrowPool>(gc);
                }
            }
            return gc;
        }
    }
}
#endif