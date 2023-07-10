using System;
using Tauntastic.Utilities.Extensions.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Tauntastic.Utilities.AssemblyDefinitionAssetPackageExporter
{
    // Class representing the structure of the .asmdef file for JSON serialization.
    [Serializable]
    public class AsmdefData
    {
        // JSON
        public string name;
        public string rootNamespace;
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
        public string[] versionDefines;
        public bool noEngineReferences;

        
        // Extra data
        public string guid;
        public string assetPath;
        public string folderPath;
        public AssemblyDefinitionAsset assemblyDefinitionAsset;
        
        /// <summary>
        /// Converts JSON to AsmdefData from a given GUID. 
        /// </summary>
        /// <param name="guid">.asmdef file GUID</param>
        /// <returns><see cref="AsmdefData"/> instance</returns>
        public static AsmdefData GetAsmdefDataFromGUID(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var assemblyDefinitionAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);
            var asmdefData = JsonUtility.FromJson<AsmdefData>(assemblyDefinitionAsset.text);
            asmdefData.assetPath = assetPath;
            asmdefData.guid = guid;
            asmdefData.assemblyDefinitionAsset = assemblyDefinitionAsset;
            asmdefData.folderPath = assemblyDefinitionAsset.GetFolderPath();
            return asmdefData;
        }

        /// <summary>
        /// Converts JSON to AsmdefData from a given asset path. 
        /// </summary>
        /// <param name="assetPath">.asmdef file path</param>
        /// <returns><see cref="AsmdefData"/> instance</returns>
        public static AsmdefData GetAsmdefDataFromPath(string assetPath)
        {
            var assemblyDefinitionAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);
            var asmdefData = JsonUtility.FromJson<AsmdefData>(assemblyDefinitionAsset.text);
            asmdefData.assetPath = assetPath;
            asmdefData.guid = AssetDatabase.AssetPathToGUID(assetPath);
            asmdefData.assemblyDefinitionAsset = assemblyDefinitionAsset;
            asmdefData.folderPath = assemblyDefinitionAsset.GetFolderPath();
            return asmdefData;
        }

        /// <summary>
        /// Converts JSON to AsmdefData from a given <see cref="AssemblyDefinitionAsset"/>. 
        /// </summary>
        /// <param name="assemblyDefinitionAsset">.asmdef file</param>
        /// <returns><see cref="AsmdefData"/> instance</returns>
        public static AsmdefData GetAsmdefData(AssemblyDefinitionAsset assemblyDefinitionAsset)
        {
            var asmdefData = JsonUtility.FromJson<AsmdefData>(assemblyDefinitionAsset.text);
            asmdefData.assetPath = AssetDatabase.GetAssetPath(assemblyDefinitionAsset);
            asmdefData.guid = AssetDatabase.AssetPathToGUID(asmdefData.assetPath);
            asmdefData.assemblyDefinitionAsset = assemblyDefinitionAsset;
            asmdefData.folderPath = assemblyDefinitionAsset.GetFolderPath();
            return asmdefData;
        }
    }
}
