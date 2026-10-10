using System;
using UnityEngine;

namespace AutoEra.World.Region
{
    [RequireComponent(typeof(RegionObjectView))]
    public sealed class InitialRegionEntity : EntityBase
    {
        private RegionObjectView _view;
        public RegionObjectView View => _view != null ? _view : (_view = GetComponent<RegionObjectView>());
        public void Bind(InitialRegion region)
        {
            if (region == null || !region.IsActive) throw new InvalidOperationException("Region is not active.");
            View.Initialize(region);
        }
        public void BindPersistent(InitialRegion region,Identity.PersistentId id)
        {
            if(region==null || !region.IsActive)throw new InvalidOperationException("Region is not active.");
            View.BindDeployed(region,id);
        }
        protected override void OnHide(bool isShutdown, object userData)
        {
            GetComponent<AutoEra.ResourcePoints.RegionProductionFacility>()?.Release();
            GetComponent<AutoEra.Logistics.RegionTransferEndpoint>()?.Release();
            View.Release();
            base.OnHide(isShutdown, userData);
        }
    }
}
