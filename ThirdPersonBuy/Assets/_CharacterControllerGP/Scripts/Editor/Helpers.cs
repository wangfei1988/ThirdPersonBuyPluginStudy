// © 2016 Mario Lelas
using UnityEditor;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// tools helper class
    /// </summary>
    public class Helpers : MonoBehaviour
    {

        [MenuItem("Tools/Character Controller GP/Helpers/Show Trigger Renderers")]
        static void ShowTriggerRenderers()
        {
            GameObject[] triggers = GameObject.FindGameObjectsWithTag("Trigger");
            Undo.RegisterCompleteObjectUndo(triggers, "Enable Trigger Renderers");
            foreach (GameObject go in triggers)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.enabled = true;
                }
            }
        }

        [MenuItem("Tools/Character Controller GP/Helpers/Hide Trigger Renderers")]
        static void HideTriggerRenderers()
        {
            GameObject[] triggers = GameObject.FindGameObjectsWithTag("Trigger");
            Undo.RegisterCompleteObjectUndo(triggers, "Disable Trigger Renderers");
            foreach (GameObject go in triggers)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.enabled = false;
                }
            }
        }

        [MenuItem ("Tools/Character Controller GP/Helpers/Show Collider Renderers")]
        static void ShowColliderRenderers()
        {
            GameObject[] colliders = GameObject.FindGameObjectsWithTag("Collider");
            Undo.RegisterCompleteObjectUndo(colliders, "Enable Collider Renderers");
            foreach (GameObject go in colliders)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr) mr.enabled = true;
            }
        }

        [MenuItem("Tools/Character Controller GP/Helpers/Hide Collider Renderers")]
        static void HideColliderRenderers()
        {
            GameObject[] colliders = GameObject.FindGameObjectsWithTag("Collider");
            Undo.RegisterCompleteObjectUndo(colliders, "Disable Collider Renderers");
            foreach (GameObject go in colliders)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr) mr.enabled = false;
            }
        }


        GameObject[] FindGameObjectsWithLayer(int layer)
        {
            var goArray = FindObjectsOfType<GameObject>();
            var goList = new System.Collections.Generic.List<GameObject>();
            for (var i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == layer)
                {
                    goList.Add(goArray[i]);
                }
            }
            if (goList.Count == 0)
            {
                return null;
            }
            return goList.ToArray();
        }

    } 

}
