// © 2015 Mario Lelas
using UnityEditor;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// ShieldItem editor
    /// </summary>
    [CustomEditor(typeof(ShieldItem))]
    public class ShieldItemEditor : Editor
    {
        private enum ShieldEnum { Shield };
        private ShieldEnum fe = ShieldEnum.Shield;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.EnumPopup("ItemType", fe);
            DrawDefaultInspector();
            ShieldItem invItem = (ShieldItem)target;


            bool loadDefaults = GUILayout.Button("Load Default Shield Clips & Sounds");
            if (loadDefaults)
            {
                Utils.LoadDefaultShieldSoundsAndClips(invItem);
            }


            if (GUI.changed)
            {
                EditorUtility.SetDirty(invItem);
            }
        }

    }
}
