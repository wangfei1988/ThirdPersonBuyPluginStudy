// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// replacing clips information
    /// </summary>
    [System.Serializable]
    public class AnimationClipReplacementInfo //: ScriptableObject
    {
        /// <summary>
        /// original clip name
        /// </summary>
        public string original_name = "NOTASSIGNED";

        /// <summary>
        /// current clip name
        /// </summary>
        [HideInInspector]
        public string current_name = "NOTASSIGNED";

        /// <summary>
        /// animation clip to replace
        /// </summary>
        public AnimationClip clip = null;

        /// <summary>
        /// is original clip replaced
        /// </summary>
        [HideInInspector]
        public bool changed = false;

        /// <summary>
        /// original clip name hash
        /// </summary>
        [HideInInspector]
        public int orig_hash = -1;

#if UNITY_EDITOR
        /// <summary>
        /// used only in inspector 
        /// </summary>
        [HideInInspector]
        public bool inspectorFoldout = false;
#endif
    } 
}
