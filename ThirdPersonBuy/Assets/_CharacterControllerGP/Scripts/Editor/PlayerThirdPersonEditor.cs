// © 2016 Mario Lelas
using UnityEngine;
using System.Collections;
using UnityEditor;


namespace MLSpace
{
    /// <summary>
    /// PlayerThirdPerson editor
    /// </summary>
    [CustomEditor(typeof(PlayerThirdPerson))]
    public class PlayerThirdPersonEditor : Editor
    {
        // unity OnInspectorGUI method
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PlayerThirdPerson ptp = (PlayerThirdPerson)target;

            ptp.lookTowardsCamera = EditorGUILayout.Toggle("Look Towards Camera", ptp.lookTowardsCamera);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ptp);
                serializedObject.ApplyModifiedProperties();
            }
        }
    } 
}
