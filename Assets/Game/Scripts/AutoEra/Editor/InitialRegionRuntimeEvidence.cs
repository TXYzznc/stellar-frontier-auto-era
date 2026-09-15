using System;
using System.Linq;
using AutoEra.Input;
using AutoEra.World.Region;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AutoEra.Editor
{
    /// <summary>Developer-only interactions with the live region; never saves runtime state.</summary>
    public static class InitialRegionRuntimeEvidence
    {
        private static int _selection;
        private static string _savedWorldScene;

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Open Command Hub")]
        public static void OpenCommandHub()
        {
            RequirePlaying();
            GF.UI.OpenUIForm(UIViews.BaseCommandHubForm);
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Inject Missing World Scene")]
        public static void InjectMissingScene()
        {
            RequirePlaying();
            if (UnityEngine.Object.FindObjectOfType<InitialRegionScene>() != null) throw new InvalidOperationException("Return from the region first.");
            if (_savedWorldScene != null) throw new InvalidOperationException("Restore the previous override first.");
            _savedWorldScene = GF.Config.GetString("AutoEra.Scene.World");
            GF.Config.RemoveConfig("AutoEra.Scene.World");
            GF.Config.AddConfig("AutoEra.Scene.World",false,0,0,"B10MissingSceneForValidation");
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Restore World Scene")]
        public static void RestoreScene()
        {
            RequirePlaying();
            if (_savedWorldScene == null) throw new InvalidOperationException("No override to restore.");
            GF.Config.RemoveConfig("AutoEra.Scene.World");
            GF.Config.AddConfig("AutoEra.Scene.World",false,0,0,_savedWorldScene);
            _savedWorldScene = null;
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Begin Placement Preview")]
        public static void BeginPlacement()
        {
            RequirePlaying();
            UnityEngine.Object.FindObjectOfType<RegionInputModule>().BeginPlacement(new Vector2(4,3),
                (position,yaw) => Debug.Log("[AutoEra][Region] Validated preview only; no construction transaction: " + position + " / " + yaw));
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Show Work Competition")]
        public static void ShowCompetition()
        {
            RequirePlaying();
            var input = UnityEngine.Object.FindObjectOfType<RegionInputModule>();
            var entry = UnityEngine.Object.FindObjectOfType<InitialRegionScene>();
            var target = UnityEngine.Object.FindObjectsOfType<RegionObjectView>().First(v => v.Model != null && v.GetWorkChannel(0) != null);
            var owner = entry.Region.Objects.First(o => o.Kind == AutoEra.World.Identity.PersistentObjectKind.Machine);
            var a = entry.Region.Register(AutoEra.World.Identity.PersistentObjectKind.Machine, "队列测试A", new Vector2(-35,-35), Vector2.one);
            var b = entry.Region.Register(AutoEra.World.Identity.PersistentObjectKind.Machine, "队列测试B", new Vector2(-32,-35), Vector2.one);
            var queue = target.GetWorkChannel(0);
            queue.Request(owner.Id, target.Model.Position);
            queue.Request(a.Id, target.Model.Position);
            queue.Request(b.Id, target.Model.Position);
            input.SelectTarget(target);
            Debug.Log("[AutoEra][Region] Runtime-only queue fixtures added; no production/transport settlement.");
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Show Long Public Status")]
        public static void ShowLongStatus()
        {
            RequirePlaying();
            var entry = UnityEngine.Object.FindObjectOfType<InitialRegionScene>();
            if (!entry.Region.TryGet(entry.Region.SelectedId, out var selected)) throw new InvalidOperationException("Select an object first.");
            selected.SetPublicState("开发长文本验证：当前对象等待其它机器释放作业位置；此说明只用于验证换行与面板边界，不表示已经完成生产或结算");
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Show Distant Machine")]
        public static void ShowDistantMachine()
        {
            RequirePlaying();
            var machine = UnityEngine.Object.FindObjectsOfType<RegionObjectView>().First(v => v.Model != null && v.Model.Kind == AutoEra.World.Identity.PersistentObjectKind.Machine);
            var input = UnityEngine.Object.FindObjectOfType<RegionInputModule>();
            input.SelectTarget(machine);
            input.GetComponent<RegionCameraController>().Focus(machine.FocusPosition + Vector3.right * 25);
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Hide Debug Overlay")]
        public static void HideDebug()
        {
            RequirePlaying();
            GameEntry.GetComponent<DebuggerComponent>().ActiveWindow = false;
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Select Next Object")]
        public static void SelectNext()
        {
            RequirePlaying();
            var views = UnityEngine.Object.FindObjectsOfType<RegionObjectView>()
                .Where(v => v.Model != null && v.Model.IsRegistered).OrderBy(v => v.Model.Id.Value).ToArray();
            var input = UnityEngine.Object.FindObjectOfType<RegionInputModule>();
            if (input == null || views.Length == 0) throw new InvalidOperationException("No active region.");
            input.SelectTarget(views[_selection++ % views.Length]);
        }

        [MenuItem("Game Framework/AutoEra/Runtime Evidence/Invalidate Selected Object")]
        public static void Invalidate()
        {
            RequirePlaying();
            var entry = UnityEngine.Object.FindObjectOfType<InitialRegionScene>();
            if (entry == null || entry.Region == null) throw new InvalidOperationException("No active region.");
            foreach (var view in UnityEngine.Object.FindObjectsOfType<RegionObjectView>())
            {
                if (view.Model == null || view.Model.Id != entry.Region.SelectedId) continue;
                // This exercises registry invalidation, not gameplay destruction or resource settlement.
                view.Release();
                view.gameObject.SetActive(false);
                return;
            }
        }

        private static void RequirePlaying()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Runtime evidence requires Play Mode.");
        }
    }
}
