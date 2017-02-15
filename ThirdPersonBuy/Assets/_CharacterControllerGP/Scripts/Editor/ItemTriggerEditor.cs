// © 2016 Mario Lelas
using UnityEngine;
using UnityEditor;

namespace MLSpace
{
    /// <summary>
    /// ItemTrigger editor
    /// </summary>
    [CustomEditor(typeof(ItemTrigger))]
    public class ItemTriggerEditor : Editor
    {
        // unity OnInspectorGUI method
        public override void OnInspectorGUI()
        {
            ItemTrigger trig = (ItemTrigger)target;

            trig.item = (InventoryItem)EditorGUILayout.ObjectField("Item", trig.item, typeof(InventoryItem), true);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(trig);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
