using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AutoEra.Editor
{
    /// <summary>Editor-only fallback for Test Runner jobs lost at the Play Mode domain reload.</summary>
    [InitializeOnLoad]
    public static class PCGEnvironmentRuntimeValidation
    {
        private const string ActiveKey="AutoEra.PCG.RuntimeValidation.Active";
        private const string Report="Docs/ArtPipeline/Evidence/PCGProductIntegration/runtime-harness.json";
        private static Stack<IEnumerator> _stack;
        static PCGEnvironmentRuntimeValidation(){EditorApplication.update+=Tick;}
        [MenuItem("Game Framework/AutoEra/Environment/Validate Formal Runtime")]
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Start from Edit Mode.");
            var scene=EditorSceneManager.GetActiveScene();if(scene.isDirty)throw new InvalidOperationException("Preserve dirty scene first.");
            if(scene.path!="Assets/Game/Scene/Launch.unity")throw new InvalidOperationException("Use the formal Launch scene.");
            SessionState.SetBool(ActiveKey,true);EditorApplication.isPlaying=true;
        }
        private static void Tick()
        {
            if(!SessionState.GetBool(ActiveKey,false)||EditorApplication.isCompiling||!EditorApplication.isPlaying)return;
            try
            {
                if(_stack==null)
                {
                    Type tests=null;
                    foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {tests=assembly.GetType("AutoEra.Tests.Editor.PCGProductEnvironmentTests");if(tests!=null)break;}
                    if(tests==null)throw new InvalidOperationException("Editor validation test assembly missing.");
                    var method=tests.GetMethod("VerifyFormalWorld",BindingFlags.Static|BindingFlags.NonPublic);
                    _stack=new Stack<IEnumerator>();_stack.Push((IEnumerator)method.Invoke(null,null));
                }
                // Run one frame of the existing integration assertions, flattening nested wait iterators.
                while(_stack.Count>0)
                {
                    var iterator=_stack.Peek();
                    if(!iterator.MoveNext()){(iterator as IDisposable)?.Dispose();_stack.Pop();continue;}
                    if(iterator.Current is IEnumerator nested){_stack.Push(nested);continue;}
                    return;
                }
                Finish(true,null);
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private static void Finish(bool passed,string error)
        {
            SessionState.SetBool(ActiveKey,false);_stack=null;
            Directory.CreateDirectory(Path.GetDirectoryName(Report));
            File.WriteAllText(Report,JsonUtility.ToJson(new Result{passed=passed,error=error,utc=DateTime.UtcNow.ToString("O")},true));
            if(passed)Debug.Log("[AutoEra][Environment] Formal runtime validation PASS.");
            else Debug.LogError("[AutoEra][Environment] Formal runtime validation FAILED: "+error);
            EditorApplication.isPlaying=false;
        }
        [Serializable] private sealed class Result { public bool passed;public string error,utc; }
    }
}
