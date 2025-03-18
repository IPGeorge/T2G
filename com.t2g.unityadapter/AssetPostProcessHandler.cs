#if UNITY_EDITOR
using UnityEditor;

namespace T2G
{
    public class AssetPostProcessHandler : AssetPostprocessor
    {
        public static bool IsProcessingAssets = true;

        //This is called after importing of any number of assets is complete
        //(when the Assets progress bar has reached the end).
        protected static void OnPostprocessAllAssets(string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            IsProcessingAssets = false;
        }
    }
}
#endif