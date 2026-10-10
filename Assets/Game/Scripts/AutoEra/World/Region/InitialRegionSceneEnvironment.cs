using System;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed partial class InitialRegionScene
    {
        [SerializeField] private RegionEnvironmentController _environment;
        public RegionEnvironmentController Environment=>_environment;
        private void BeginEnvironment()=>_environment?.Begin();
        private void EnvironmentReady(Action ready,Action<string> failed)
        {if(_environment!=null)_environment.WaitUntilReady(ready,failed);else ready?.Invoke();}
        private void AttachEnvironmentReceiver(RegionObjectView view)
        {if(view!=null)_environment?.RegisterReceiver(view.gameObject);}
    }
}
