using System;
using GameFramework.Event;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

namespace AutoEra.Application
{
    /// <summary>Owns GF scene requests. Cancelled completions may unload but never activate a new owner.</summary>
    public sealed class AutoEraSceneFlow : IDisposable
    {
        private sealed class Request
        {
            public AutoEraSceneFlow Owner;
            public string Path;
            public Action<Scene> Success;
            public Action<string> Failure;
            public bool Cancelled;
        }
        private Request _current;
        private int _pending;
        private bool _subscribed, _disposed;
        private string _loaded;
        public float Progress { get; private set; }

        public void Load(string relativeScene, Action<Scene> success, Action<string> failure)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AutoEraSceneFlow));
            if (string.IsNullOrWhiteSpace(relativeScene) || relativeScene.Contains("..") || relativeScene.Contains(":"))
                throw new ArgumentException("Invalid relative scene.");
            Cancel();
            var request = new Request { Owner = this, Path = UtilityBuiltin.AssetsPath.GetScenePath(relativeScene), Success = success, Failure = failure };
            // A failed world entry leaves the last successful menu loaded. Reuse our owned scene.
            if (_loaded == request.Path)
            {
                Scene existing = SceneManager.GetSceneByPath(_loaded);
                if (existing.IsValid() && existing.isLoaded) { Progress = 1; success?.Invoke(existing); return; }
            }
#if UNITY_EDITOR
            // EditorResourceComponent returns without a failure event when Unity rejects an unlisted scene.
            if (GFBuiltin.Base != null && GFBuiltin.Base.EditorResourceMode && SceneUtility.GetBuildIndexByScenePath(request.Path) < 0)
            {
                failure?.Invoke("Scene is not enabled in Build Settings: " + request.Path);
                return;
            }
#endif
            _current = request;
            Progress = 0;
            if (!_subscribed)
            {
                GF.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnSuccess);
                GF.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnFailure);
                GF.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnProgress);
                _subscribed = true;
            }
            _pending++;
            try { GF.Scene.LoadScene(request.Path, request); }
            catch (Exception exception)
            {
                Action<string> callback = request.Failure;
                Finish(request);
                callback?.Invoke(exception.Message);
            }
        }

        public void Cancel()
        {
            if (_current == null) return;
            _current.Cancelled = true;
            _current.Success = null;
            _current.Failure = null;
            _current = null;
        }

        private void OnProgress(object sender, GameEventArgs e)
        {
            var data = (LoadSceneUpdateEventArgs)e;
            if (data.UserData == _current && _current != null && !_current.Cancelled) Progress = data.Progress;
        }

        private void OnSuccess(object sender, GameEventArgs e)
        {
            var data = (LoadSceneSuccessEventArgs)e;
            if (!(data.UserData is Request request) || request.Owner != this) return;
            Action<Scene> callback = request.Success;
            bool cancelled = request.Cancelled || _disposed;
            Finish(request);
            if (cancelled) { GF.Scene.UnloadScene(data.SceneAssetName); return; }
            string previous = _loaded;
            _loaded = data.SceneAssetName;
            if (!string.IsNullOrEmpty(previous) && previous != _loaded) GF.Scene.UnloadScene(previous);
            Progress = 1;
            callback?.Invoke(SceneManager.GetSceneByPath(_loaded));
        }

        private void OnFailure(object sender, GameEventArgs e)
        {
            var data = (LoadSceneFailureEventArgs)e;
            if (!(data.UserData is Request request) || request.Owner != this) return;
            Action<string> callback = request.Cancelled || _disposed ? null : request.Failure;
            Finish(request);
            callback?.Invoke(data.ErrorMessage);
        }

        private void Finish(Request request)
        {
            if (_current == request) _current = null;
            request.Success = null;
            request.Failure = null;
            _pending--;
            if (_pending != 0 || !_subscribed) return;
            GF.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnSuccess);
            GF.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnFailure);
            GF.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnProgress);
            _subscribed = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Cancel();
            if (!string.IsNullOrEmpty(_loaded)) GF.Scene.UnloadScene(_loaded);
            _loaded = null;
        }
    }
}
