// © 2016 Mario Lelas
using UnityEditor;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// BowItem editor 
    /// </summary>
    [CustomEditor(typeof(BowItem), true)]
    public class BowEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BowItem bowItem = (BowItem)target;
            DrawDefaultInspector();

            bowItem.range = EditorGUILayout.FloatField("Power", bowItem.range);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(bowItem);
                serializedObject.ApplyModifiedProperties();
            }
        }
    } 
}
