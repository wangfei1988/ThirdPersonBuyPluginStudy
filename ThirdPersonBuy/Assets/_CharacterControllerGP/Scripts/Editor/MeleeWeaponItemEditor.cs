// © 2016 Mario Lelas
using UnityEditor;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// MeleeWeaponItem editor
    /// </summary>
    [CustomEditor(typeof(MeleeWeaponItem), true)]
    public class MeleeWeaponItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MeleeWeaponItem invItem = (MeleeWeaponItem)target;
            GUI.changed = false;
            invItem.meleeWeaponType = (MeleeWeaponItem.MeleeWeaponEnum)EditorGUILayout.EnumPopup("ItemType", invItem.meleeWeaponType);

            DrawDefaultInspector();

            if (GUI.changed)
            {
                int newType = 0;
                switch (invItem.meleeWeaponType)
                {
                    case MeleeWeaponItem.MeleeWeaponEnum.Weapon1H:
                        newType = (int)InventoryItemType.Weapon1H;
                        break;
                    case MeleeWeaponItem.MeleeWeaponEnum.Weapon2H:
                        newType = (int)InventoryItemType.Weapon2H;
                        break;
                }
                // modify private field
                SerializedProperty sp = serializedObject.FindProperty("m_ItemType");
                if (sp == null)
                {
                    Debug.LogError("Cannot find variable: m_ItemType");
                    return;
                }
                sp.intValue = (int)newType;

                EditorUtility.SetDirty(invItem);
                serializedObject.ApplyModifiedProperties();
            }
            if (invItem.itemType == InventoryItemType.Weapon1H)
            {
                bool loadDefaults = GUILayout.Button("Load Default One Handed Weapon Clips & Sounds");
                if (loadDefaults)
                {
                    Utils.LoadDefaultWeapon1HSoundsAndClips(invItem);
                }
            }
            else if (invItem.itemType == InventoryItemType.Weapon2H)
            {
                bool loadDefaults = GUILayout.Button("Load Default Two Handed Weapon Clips & Sounds");
                if (loadDefaults)
                {
                    Utils.LoadDefaultWeapon2HSoundsAndClips(invItem);
                }
            }
            if (GUI.changed)
            {
                EditorUtility.SetDirty(invItem);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
