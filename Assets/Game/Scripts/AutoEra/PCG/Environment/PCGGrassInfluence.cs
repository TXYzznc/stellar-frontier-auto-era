using UnityEngine;

namespace AutoEra.Art.PCG
{
    public sealed class PCGGrassInfluence : MonoBehaviour
    {
        [InspectorName("草地轨迹系统")] public PCGGrassTrails Trails;
        [InspectorName("影响半径")] public float Radius = 1.4f;
        [InspectorName("压草强度"), Range(0,1)] public float Strength = .85f;
        [InspectorName("最大接触高度")] public float MaximumContactHeight = 1.5f;
        private Vector2 _previous;
        private bool _hasPrevious;
        public void ResetHistory() { _hasPrevious=false; }
        private void OnEnable() { if(Trails!=null)Trails.Register(this);_hasPrevious=false; }
        private void OnDisable() { if(Trails!=null)Trails.Unregister(this);_hasPrevious=false; }
        internal void Stamp(float time)
        {
            if(Trails==null||Trails.World==null)return;
            Vector2 p=new Vector2(transform.position.x,transform.position.z)+Trails.World.WorldOrigin;
            float ground=Trails.World.Field.Height(p.x,p.y);
            if(Mathf.Abs(transform.position.y-ground)>MaximumContactHeight){_hasPrevious=false;return;}
            Vector2 from=_hasPrevious?_previous:p;
            // Explicit teleport reset; continuous paths below 64m retain swept interaction.
            if((p-from).sqrMagnitude>4096)from=p;
            Vector2 forward=new Vector2(transform.forward.x,transform.forward.z);
            Trails.StampSegment(from,p,Mathf.Clamp(Radius,.2f,5),Strength,forward,time);
            _previous=p;_hasPrevious=true;
        }
    }
}
