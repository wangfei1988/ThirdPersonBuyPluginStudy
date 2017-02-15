// © 2016 Mario Lelas
using UnityEngine;
using UnityEditor;

namespace MLSpace
{
    /// <summary>
    /// OrbitCameraControllerEditor
    /// </summary>
    [CustomEditor(typeof(OrbitCameraController))]
    public class OrbitCameraEditor : Editor
    {

        // unity OnInspectorGUI method
        public override void OnInspectorGUI()
        {
            OrbitCameraController cam = (OrbitCameraController)target;

            //trig.item = (InventoryItem)EditorGUILayout.ObjectField("Item", trig.item, typeof(InventoryItem), true);
            cam.Xconstraint = (OrbitCameraController.CameraConstraint)EditorGUILayout.EnumPopup("X Constraint: ", cam.Xconstraint);
            if(cam.Xconstraint == OrbitCameraController.CameraConstraint.Limited)
            {
                cam.minXAngle = EditorGUILayout.FloatField("Min X Angle", cam.minXAngle);
                cam.maxXAngle = EditorGUILayout.FloatField("Max X Angle", cam.maxXAngle);
            }

            cam.Yconstraint = (OrbitCameraController.CameraConstraint)EditorGUILayout.EnumPopup("Y Constraint: ", cam.Yconstraint);
            if (cam.Yconstraint == OrbitCameraController.CameraConstraint.Limited)
            {
                cam.minYAngle = EditorGUILayout.FloatField("Min Y Angle", cam.minYAngle);
                cam.maxYAngle = EditorGUILayout.FloatField("Max Y Angle", cam.maxYAngle);
            }

            DrawDefaultInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(cam);
                serializedObject.ApplyModifiedProperties();
            }
        }


    } 
}
