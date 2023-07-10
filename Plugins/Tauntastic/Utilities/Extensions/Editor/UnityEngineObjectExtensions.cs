using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Tauntastic.Utilities.Extensions.Editor
{
    public static class UnityEngineObjectExtensions
    {
        public static string GetFolderPath(this Object obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            var lastIndexOfForwardSlash = assetPath.LastIndexOf("/", StringComparison.Ordinal);
            var folderPath = assetPath[..lastIndexOfForwardSlash];
            return folderPath;
        }
    }
}
