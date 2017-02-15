// © 2016 Mario Lelas
using UnityEditor;

namespace MLSpace
{

    /// <summary>
    /// Small class that prevents creating materials on import
    /// Delete if it is a feature you don't want
    /// </summary>
    public class FbxImport : AssetPostprocessor
    {
        public void OnPreprocessModel()
        {
            ModelImporter modelImporter = (ModelImporter)assetImporter;
            modelImporter.importMaterials = false;
        }
    } 
}
