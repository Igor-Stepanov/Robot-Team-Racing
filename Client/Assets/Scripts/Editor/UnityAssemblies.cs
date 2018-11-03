using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Editor
{
  internal class UnityAssemblies
  {
    [MenuItem("Assets" + "/CopyUnityAssemblies", false, 1001)]
    public static void CopyUnityAssemblies()
    {
      //if (File.Exists(Path.Combine("UnityAssemblies", "UnityEngine.dll"))) return;

      if (!Directory.Exists("UnityAssemblies"))
      {
        Directory.CreateDirectory("UnityAssemblies");
      }

      try
      {
        var assembliesPaths = GetUnityAssembliesPaths();
        CopyLibs(assembliesPaths);

        var sb = new StringBuilder("OK. Copied assemblies:");
        assembliesPaths.ForEach(x => sb.AppendLine(x));
        Debug.LogFormat(sb.ToString());
      }
      catch (Exception ex)
      {
        Debug.LogError(ex);
      }
    }
    
    private static List<string> GetUnityAssembliesPaths()
    {
      var result = new List<string>();
      var assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.ManifestModule.Name == "UnityEditor.dll");
      
      var editorCompilationType = assemblies
        .GetTypes()
        .FirstOrDefault(t => t.FullName == "UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
      
      var editorCompilationField = editorCompilationType.GetField("editorCompilation", BindingFlags.NonPublic | BindingFlags.Static);
      var editorCompilationTypeValue = editorCompilationField.GetValue(null);

      var editorCompilationTypeFieldValue = editorCompilationTypeValue.GetType();

      var precompiledAssembliesFieldInfo = editorCompilationTypeFieldValue
        .GetField("precompiledAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
      
      var precompiledAssembliesArray = (Array)precompiledAssembliesFieldInfo.GetValue(editorCompilationTypeValue);

      var unityAssembliesFieldInfo = editorCompilationTypeFieldValue
        .GetField("unityAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
      
      var unityAssembliesArray = (Array)unityAssembliesFieldInfo.GetValue(editorCompilationTypeValue);
      
      var customScriptAssembliesFieldInfo = editorCompilationTypeFieldValue
        .GetField("customScriptAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
      
      var customScriptAssembliesArray = (Array)customScriptAssembliesFieldInfo.GetValue(editorCompilationTypeValue);

      result.AddRange(precompiledAssembliesArray
        .Cast<object>()
        .Select(p => p.GetType()
                      .GetField("Path")
                      .GetValue(p)
                      .ToString()));
      
      result.AddRange(unityAssembliesArray
        .Cast<object>()
        .Select(p => p.GetType()
                      .GetField("Path")
                      .GetValue(p)
                      .ToString()));
      
      result.AddRange(customScriptAssembliesArray
        .Cast<object>()
        .Select(p => p.GetType()
          .GetProperty("FilePath")
          .GetValue(p)
          .ToString()));

      return result;
    }

    private static void CopyLibs(List<string> libPaths)
    {
      foreach (var libPath in libPaths)
      {
        var name = Path.GetFileName(libPath);
        var xmlPath = Path.ChangeExtension(libPath, "xml");

        if (File.Exists(xmlPath))
        {
          var newXmlPath = Path.Combine("UnityAssemblies", Path.GetFileName(xmlPath));
          if (!File.Exists(newXmlPath))
            File.Copy(xmlPath, newXmlPath);
        }

        var newPath = Path.Combine("UnityAssemblies", name);
        if (!File.Exists(newPath))
          File.Copy(libPath, newPath);
      }
    }
  }
}