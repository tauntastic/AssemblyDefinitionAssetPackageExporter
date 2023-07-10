using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Tauntastic.Utilities.AssemblyDefinitionAssetPackageExporter
{
    public class AssemblyDefinitionAssetPackageExporter
    {
        private const string _MENU_PATH = "Assets/Tauntastic/Export Assembly Definition Asset as Package...";

        private static Object _asmdefAssetObject;

        [MenuItem(_MENU_PATH, priority = 7)]
        public static void ShowMenuItem()
        {
            Instance.ExportSelectedAssemblyDefinitionAssetAndRelated();
        }

        [MenuItem(_MENU_PATH, validate = true)]
        public static bool ShowMenuItemValidation()
        {
            var objects = Selection.objects;

            if (objects == null)
                return false;

            if (objects.Length > 1)
                return false;

            _asmdefAssetObject = objects[0];
            if (_asmdefAssetObject is not AssemblyDefinitionAsset)
                return false;

            return true;
        }

        private static AssemblyDefinitionAssetPackageExporter _instance;

        private static AssemblyDefinitionAssetPackageExporter Instance => _instance ??= new AssemblyDefinitionAssetPackageExporter();


        private AssemblyDefinitionAsset _asmdefAsset;

        private static readonly List<AsmdefData> asmdefDataList = new();

        private Object[] _objects;
        
        private List<string> _packageAssetsPaths;
        
        private void ExportSelectedAssemblyDefinitionAssetAndRelated()
        {
            _asmdefAsset = _asmdefAssetObject as AssemblyDefinitionAsset;

            if (asmdefDataList.Count == 0)
            {
                var allAsmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets" });
                foreach (var guid in allAsmdefGuids)
                {
                    var asmdefData = AsmdefData.GetAsmdefDataFromGUID(guid);
                    asmdefDataList.Add(asmdefData);
                }
            }
            
            var selectedAsmdefData = AsmdefData.GetAsmdefData(_asmdefAsset);

            // Get selected AssemblyDefinition's Folder Path Objects
            _packageAssetsPaths = GetAllObjectsPathsInFolderPath(selectedAsmdefData.folderPath);
            
            // Get selected AssemblyDefinition's References' Folder Path Objects
            _packageAssetsPaths.AddRange(GetAsmdefReferencesPaths(selectedAsmdefData.references, asmdefDataList));
            
            ShowPackageExport(_packageAssetsPaths);
        }

        private void ShowPackageExport(List<string> _packageAssetsPaths)
        {
            var objectsList = LoadObjectsFromAssetPathCollection(_packageAssetsPaths);

            Selection.objects = objectsList.ToArray();
            ShowExportPackageWindow();
        }
        
        private static void ShowExportPackageWindow()
        {
            var editorWindowAssembly = typeof(EditorWindow).Assembly;
            var packageExporterType = editorWindowAssembly.GetType("UnityEditor.PackageExport");
            var field = packageExporterType.GetField("m_IncludeDependencies", BindingFlags.NonPublic | BindingFlags.Instance);
            var packageExportWindow = EditorWindow.GetWindow(packageExporterType, true, "Exporting Package");
            field!.SetValue(packageExportWindow, false);
        }
        
        private static List<Object> LoadObjectsFromAssetPathCollection(IEnumerable<string> assetPaths)
        {
            var objectsList = new List<Object>();
            foreach (var assetPath in assetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                objectsList.Add(asset);
            }

            return objectsList;
        }

        private static List<string> GetAllObjectsPathsInFolderPath(string folderPath)
        {
            var assetGuids = AssetDatabase.FindAssets("", new[] { folderPath });
            var assetsPaths = new List<string>();
            for (var i = 0; i < assetGuids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                if (assetPath.StartsWith("Assets/"))
                    assetsPaths.Add(assetPath);
            }
            return assetsPaths;
        }
        
        private static List<AsmdefData> GetReferencesAsmdefDataRecursive(string[] assemblyReferences, List<AsmdefData> asmdefDataList)
        {
            var matchedAsmdefDataList = new List<AsmdefData>();

            // Recursive termination condition: assemblyReferences is empty
            if (assemblyReferences.Length == 0)
                return matchedAsmdefDataList;

            var tempAsmdefDataList = asmdefDataList.ToList();

            // Iterate through each assembly reference
            for (var i = assemblyReferences.Length - 1; i >= 0; i--)
            {
                var assemblyReference = assemblyReferences[i];

                if (assemblyReference.Contains("GUID:"))
                {
                    // Handle assembly references with GUID
                    var parsedGuid = assemblyReference.Replace("GUID:", "");
                    var asmdefData = tempAsmdefDataList.FirstOrDefault(x => x.guid == parsedGuid);

                    if (asmdefData != null)
                    {
                        tempAsmdefDataList.RemoveAt(i);
                        matchedAsmdefDataList.Add(asmdefData);
                    }
                }
                else
                {
                    // Handle assembly references by name
                    var tempAssemblyReferencesList = assemblyReferences.ToList();
                    
                    for (var j = tempAsmdefDataList.Count - 1; j >= 0; j--)
                    {
                        var asmdefData = asmdefDataList[j];

                        if (tempAssemblyReferencesList.Contains(asmdefData.name))
                        {
                            matchedAsmdefDataList.Add(asmdefData);
                            tempAssemblyReferencesList.Remove(asmdefData.name);
                            tempAsmdefDataList.RemoveAt(j);
                            break;
                        }
                    }
                }
            }

            for (var i = 0; i < matchedAsmdefDataList.Count; i++)
            {
                var recursiveMatchedAsmdefDataList = GetReferencesAsmdefDataRecursive(matchedAsmdefDataList[i].references, asmdefDataList);
                matchedAsmdefDataList.AddRange(recursiveMatchedAsmdefDataList);
            }

            return matchedAsmdefDataList;
        }
        
        private static List<string> GetAsmdefReferencesPaths(string[] references, List<AsmdefData> asmdefDataList)
        {
            var referencesAsmdefDataList = GetReferencesAsmdefDataRecursive(references, asmdefDataList)
                .GroupBy(x => x.name)
                .Select(y => y.First())
                .ToList();

            var packageAssetsPaths = new List<string>();
            foreach (var referencesAsmdefData in referencesAsmdefDataList)
            {
                var objectPaths = GetAllObjectsPathsInFolderPath(referencesAsmdefData.folderPath);
                packageAssetsPaths.AddRange(objectPaths);
            }

            return packageAssetsPaths;
        }
    }
}