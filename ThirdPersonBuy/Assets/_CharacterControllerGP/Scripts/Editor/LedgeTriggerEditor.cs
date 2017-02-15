// © 2016 Mario Lelas
using UnityEngine;
using UnityEditor;



namespace MLSpace
{
    /// <summary>
    /// Ledge2LedgeTrigger editor
    /// </summary>
    [CustomEditor(typeof(Ledge2LedgeTrigger))]
    public class LedgeTriggerEditor : Editor
    {
        // unity OnInspectorGUI method
        public override void OnInspectorGUI()
        {
            Ledge2LedgeTrigger trig = (Ledge2LedgeTrigger)target;

            trig.Target = (Transform)EditorGUILayout.ObjectField("Up Target", trig.Target, typeof(Transform), true);
            trig.switchTargetDirection = EditorGUILayout.Toggle("Switch Up Direction", trig.switchTargetDirection);
            EditorGUILayout.Separator();
            trig.downTarget = (Transform)EditorGUILayout.ObjectField("Down Target", trig.downTarget, typeof(Transform), true);
            trig.switchDownDirection = EditorGUILayout.Toggle("Switch Down Direction", trig.switchDownDirection);
            EditorGUILayout.Separator();

            trig.rightTarget = (Transform)EditorGUILayout.ObjectField("Right Target", trig.rightTarget, typeof(Transform), true);
            trig.rightEdgeReq = EditorGUILayout.Toggle(new GUIContent("Right Edge Requirement", "Must be on right ledge edge to activate trigger."), trig.rightEdgeReq);
            trig.leftTarget  = (Transform)EditorGUILayout.ObjectField("Left Target", trig.leftTarget, typeof(Transform), true);
            trig.leftEdgeReq = EditorGUILayout.Toggle(new GUIContent("Left Edge Requirement", "Must be on left ledge edge to activate trigger."), trig.leftEdgeReq);

            EditorGUILayout.Separator();
            trig.pullUpTarget  = (Transform)EditorGUILayout.ObjectField("Leave / Enter Target", trig.pullUpTarget, typeof(Transform), true);
            EditorGUILayout.Separator();


            trig.angleCondition = EditorGUILayout.FloatField("Angle Condition", trig.angleCondition);
            trig.horizontalLimit = EditorGUILayout.FloatField("Horizontal Limit", trig.horizontalLimit);
            trig.verticalLimit  = EditorGUILayout.FloatField("Vertical Limit", trig.verticalLimit );
            trig.reverseOnLedge = EditorGUILayout.Toggle("Reverse On Ledge", trig.reverseOnLedge);
            
            trig.switchUpWithDownAnim = EditorGUILayout.Toggle("Switch Up to Down animation", trig.switchUpWithDownAnim);
            trig.showInfoText = EditorGUILayout.Toggle("Show Info Text", trig.showInfoText);


#if DEBUG_INFO
            trig.visualizeLedgeConnections = EditorGUILayout.Toggle("Visualize ledge connections", trig.visualizeLedgeConnections);
#endif
            if (GUI.changed)
            {
                EditorUtility.SetDirty(trig);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

}
