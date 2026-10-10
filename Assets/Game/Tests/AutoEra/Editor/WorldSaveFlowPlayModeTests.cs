using System;
using System.Collections;
using System.IO;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Time;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    // Real GF forms and file writer; the controllable snapshot boundary is test input, not a complete playable world.
    public sealed class WorldSaveFlowPlayModeTests
    {
        private static UnityGameFramework.Runtime.UIComponent Ui => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
        private static UnityGameFramework.Runtime.DebuggerComponent Debugger => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.DebuggerComponent>();
        private const string Evidence = "openspec/changes/b47-p7002-p7003-p7004-p7011-world-save-flow/evidence";
        private sealed class TimeProvider : IUtcTimeProvider
        { public DateTimeOffset GetUtcNow() => new DateTimeOffset(2026,10,8,12,0,0,TimeSpan.Zero); }
        private sealed class Source : IWorldSnapshotSource
        {
            internal AutoEraWorldSession World;
            internal bool Boundary;
            public bool TryCapture(long revision,out WorldSnapshotDocument snapshot,out string reason)
            {
                snapshot=null;reason="等待测试提交边界";
                if(!Boundary)return false;
                snapshot=new WorldSnapshotDocument(World.Clock.WorldMilliseconds,World.IdAllocator.NextId.Value-1,revision,"保存退出界面验收",
                    new[] {new WorldSnapshotSection("fixture",1,new { Boundary=true })});reason=null;return true;
            }
        }

        [UnityTest,Timeout(600000)]
        public IEnumerator SaveExit_RealFormsWaitFailResumeRetryAndConfirmForce_At1920x1080()
        {
            const string launch="Assets/Game/Scene/Launch.unity";
            if(!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch,OpenSceneMode.Additive));
            yield return new EnterPlayMode();yield return Verify();yield return new ExitPlayMode();
        }

        private static IEnumerator Verify()
        {
            yield return Wait(()=>MachineCatalog.IsGameDataLoaded && Ui!=null && Ui.HasUIGroup("Default"));
            foreach(var form in Ui.GetAllLoadedUIForms())if(form.Logic is MainMenuForm)Ui.CloseUIForm(form.SerialId);
            bool debugger=Debugger!=null && Debugger.ActiveWindow;if(Debugger!=null)Debugger.ActiveWindow=false;
            string root=Path.GetFullPath(Path.Combine(Path.GetTempPath(),"AutoEraB47Ui-"+Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(root);
            try
            {
                var slots=new SaveSlotService(root);
                using(var context=new AutoEraApplicationContext(new TimeProvider(),new AutoEraWorldSessionFactory(),null,slots))
                {
                    Assert.That(context.TryCreateWorldSession(100,out var world),Is.True);
                    var source=new Source {World=world};Assert.That(context.TryAttachWorldSaving(0,source),Is.True);
                    context.SaveCoordinator.MarkDirty();
                    string blocked=slots.GetSlotPath(0);Directory.CreateDirectory(blocked);
                    var session=AutoEraUiSession.ForWorld(context,world);
                    yield return OpenFromSystemMenu(session);var exit=Find<ExitFlowForm>();
                    context.SaveExit.Pump(0);Assert.That(context.SaveExit.State,Is.EqualTo(WorldSaveExitState.WaitingBoundary));
                    Assert.That(context.BlocksNewWorldCommands,Is.True);Assert.That(context.FreezesWorldSimulation,Is.False);
                    Assert.That(exit.ExitFlowLoadingState.activeInHierarchy,Is.True);Assert.That(exit.ForceButton.interactable,Is.False);
                    yield return Screenshot("save-exit-waiting-1920x1080.png");
                    source.Boundary=true;yield return PumpUntil(context,WorldSaveExitState.Failed);
                    Assert.That(context.FreezesWorldSimulation,Is.True);Assert.That(exit.RetryButton.interactable,Is.True);
                    Assert.That(exit.ExitFlowErrorState.activeInHierarchy,Is.True);yield return Screenshot("save-exit-failure-1920x1080.png");
                    exit.ResumeButton.onClick.Invoke();yield return Wait(()=>Find<ExitFlowForm>()==null);
                    Assert.That(context.SaveExit.State,Is.EqualTo(WorldSaveExitState.Idle));Assert.That(context.BlocksNewWorldCommands,Is.False);
                    Assert.That(context.FreezesWorldSimulation,Is.False);Assert.That(context.SaveCoordinator.HasUnsavedChanges,Is.True);
                    // Begin forces a new snapshot even when revision zero was never autosaved.
                    yield return OpenFromSystemMenu(session);exit=Find<ExitFlowForm>();
                    yield return PumpUntil(context,WorldSaveExitState.Failed);
                    Assert.That(Path.GetDirectoryName(blocked),Is.EqualTo(root));Directory.Delete(blocked,false);
                    exit.RetryButton.onClick.Invoke();yield return PumpUntil(context,WorldSaveExitState.Completed);
                    Assert.That(slots.Read(0).Status,Is.EqualTo(SaveSlotReadStatus.Success));
                    Assert.That(context.FreezesWorldSimulation,Is.True);Assert.That(exit.ExitFlowSuccessState.activeInHierarchy,Is.True);
                    yield return Screenshot("save-exit-success-1920x1080.png");
                    CloseForms();
                }
                using(var context=new AutoEraApplicationContext(new TimeProvider(),new AutoEraWorldSessionFactory(),null,slots))
                {
                    context.TryCreateWorldSession(100,out var world);var source=new Source {World=world,Boundary=true};
                    Assert.That(context.TryAttachWorldSaving(1,source),Is.True);Directory.CreateDirectory(slots.GetSlotPath(1));
                    yield return OpenFromSystemMenu(AutoEraUiSession.ForWorld(context,world));yield return PumpUntil(context,WorldSaveExitState.Failed);
                    var exit=Find<ExitFlowForm>();exit.ForceButton.onClick.Invoke();
                    Assert.That(context.SaveExit.State,Is.EqualTo(WorldSaveExitState.Failed));
                    Assert.That(exit.ExitFlowFailureBody.text,Does.Contain("再次点击"));
                    yield return Screenshot("save-exit-force-confirm-1920x1080.png");
                    exit.ForceButton.onClick.Invoke();yield return Wait(()=>Find<ExitFlowForm>()==null);
                    Assert.That(context.SaveExit.State,Is.EqualTo(WorldSaveExitState.Forced));Assert.That(File.Exists(slots.GetSlotPath(1)),Is.False);
                }
            }
            finally
            {
                CloseForms();if(Debugger!=null)Debugger.ActiveWindow=debugger;
                string parent=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
                Assert.That(Path.GetDirectoryName(root).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar),Is.EqualTo(parent));
                Assert.That(Path.GetFileName(root),Does.StartWith("AutoEraB47Ui-"));
                if(Directory.Exists(root))Directory.Delete(root,true);
            }
        }
        private static IEnumerator OpenFromSystemMenu(AutoEraUiSession session)
        {
            int id=AutoEraUiNavigator.Open(UIViews.SystemMenuForm,session);
            yield return Wait(()=>Ui.HasUIForm(id));((SystemMenuForm)Ui.GetUIForm(id).Logic).ReturnToMenuButton.onClick.Invoke();
            yield return Wait(()=>Find<ExitFlowForm>()!=null);
        }
        private static IEnumerator PumpUntil(AutoEraApplicationContext context,WorldSaveExitState state)
        {
            double until=Time.realtimeSinceStartupAsDouble+30;
            while(context.SaveExit.State!=state && Time.realtimeSinceStartupAsDouble<until) {context.SaveExit.Pump(0);yield return null;}
            Assert.That(context.SaveExit.State,Is.EqualTo(state));
        }
        private static T Find<T>() where T:class
        {foreach(var form in Ui.GetAllLoadedUIForms())if(form.Logic is T value)return value;return null;}
        private static void CloseForms()
        {if(Ui==null)return;foreach(var form in Ui.GetAllLoadedUIForms())if(form.Logic is ExitFlowForm || form.Logic is SystemMenuForm)Ui.CloseUIForm(form.SerialId);}
        private static IEnumerator Wait(Func<bool> predicate)
        {double until=Time.realtimeSinceStartupAsDouble+30;while(!predicate() && Time.realtimeSinceStartupAsDouble<until)yield return null;Assert.That(predicate(),Is.True,"Expected formal UI state within 30 seconds.");}
        private static IEnumerator Screenshot(string name)
        {
            yield return null;yield return null;Assert.That(Screen.width,Is.EqualTo(1920));Assert.That(Screen.height,Is.EqualTo(1080));
            Directory.CreateDirectory(Evidence);string path=Path.GetFullPath(Path.Combine(Evidence,name));if(File.Exists(path))File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);yield return Wait(()=>File.Exists(path));byte[] png=File.ReadAllBytes(path);
            int Size(int offset)=>(png[offset]<<24)|(png[offset+1]<<16)|(png[offset+2]<<8)|png[offset+3];
            Assert.That(Size(16),Is.EqualTo(1920));Assert.That(Size(20),Is.EqualTo(1080));
        }
    }
}
