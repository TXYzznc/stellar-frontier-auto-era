#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace AutoEra.Editor
{
    /// <summary>Exposes the framework's guarded, single-table AI JSON reverse operation without editing workbooks directly.</summary>
    internal static class AutoEraUITableJsonSyncMenu
    {
        private const string GeneratorTypeName = "UGF.EditorTools.AIGameDataTableGenerator";
        private const string ReverseMethodName = "ReverseDataTablesJsonToExcelByRelativePaths";
        private const string GameDataGeneratorTypeName = "UGF.EditorTools.GameDataGenerator";
        private const string GenerateUiViewsMethodName = "GenerateUIFormNamesScript";

        [MenuItem("Game Framework/AutoEra/Data/Reverse Core UITable Json To Excel")]
        private static void ReverseCoreUITableJsonToExcel()
        {
            Type generatorType = FindLoadedType(GeneratorTypeName);
            MethodInfo reverseMethod = generatorType?.GetMethod(ReverseMethodName, BindingFlags.Public | BindingFlags.Static);
            if (reverseMethod == null)
            {
                throw new MissingMethodException(GeneratorTypeName, ReverseMethodName);
            }

            reverseMethod.Invoke(null, new object[] { new List<string> { "Core/UITable" } });
        }

        [MenuItem("Game Framework/AutoEra/Data/Generate UIViews From UITable")]
        private static void GenerateUiViewsFromUITable()
        {
            Type generatorType = FindLoadedType(GameDataGeneratorTypeName);
            MethodInfo generateMethod = generatorType?.GetMethod(GenerateUiViewsMethodName, BindingFlags.Public | BindingFlags.Static);
            if (generateMethod == null)
            {
                throw new MissingMethodException(GameDataGeneratorTypeName, GenerateUiViewsMethodName);
            }

            generateMethod.Invoke(null, null);
            AssetDatabase.Refresh();
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
#endif
