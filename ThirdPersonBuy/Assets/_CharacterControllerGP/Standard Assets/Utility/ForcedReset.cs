using System;
using UnityEngine;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
using UnityEngine.SceneManagement;
#endif
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof (GUITexture))]
public class ForcedReset : MonoBehaviour
{
    private void Update()
    {
        // if we have forced a reset ...
        if (CrossPlatformInputManager.GetButtonDown("ResetObject"))
        {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            //... reload the scene
            SceneManager.LoadScene(SceneManager.GetSceneAt(0).path);
#else
            Application.LoadLevel(Application.loadedLevel);
#endif
        }
    }
}
