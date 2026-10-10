using UnityEngine;
namespace AutoEra.Art.PCG
{
    public enum PCGContactShape { [InspectorName("轮式")] Wheels, [InspectorName("足式")] Feet, [InspectorName("拖拽")] Drag }
    public sealed class PCGSurfaceContact : MonoBehaviour
    {
        [InspectorName("地表响应系统")] public PCGSurfaceResponse Response;
        [InspectorName("接触形状")] public PCGContactShape Shape;
        [InspectorName("接触宽度")] public float Width=1.6f;
        [InspectorName("足部长度")] public float FootLength=.42f;
        [InspectorName("足部宽度")] public float FootWidth=.30f;
        [InspectorName("允许接地")] public bool Grounded=true;
        private Vector2 _previous;
        private bool _hasPrevious;
        private float _distance;
        private int _footStep;
        private readonly PCGSurfaceCarry[] _carry=new PCGSurfaceCarry[2];
        public float CarryAmount=>Mathf.Max(_carry[0].Amount,_carry[1].Amount);
        public void ResetHistory(bool clearCarry=false)
        {_hasPrevious=false;_distance=0;_footStep=0;if(clearCarry){_carry[0]=default;_carry[1]=default;}}
        private void OnEnable(){ResetHistory(true);if(Response!=null)Response.Register(this);}
        private void OnDisable(){if(Response!=null)Response.Unregister(this);ResetHistory(true);}
        internal void Sample(float now)
        {
            if(Response==null||Response.World.Field==null)return;
            Vector2 p=new Vector2(transform.position.x,transform.position.z)+Response.World.WorldOrigin;
            var e=Response.World.Field.Query(p.x,p.y);
            float contactY=e.Liquid!=PCGLiquidKind.None?Mathf.Max(e.Height,e.LiquidLevel):e.Height;
            if(!Grounded||Mathf.Abs(transform.position.y-contactY)>Response.Settings.ContactTolerance)
            {ResetHistory();for(int i=0;i<2;i++)_carry[i].Step(PCGSurfaceKind.Hard,0,now,false);return;}
            Vector2 from=_hasPrevious?_previous:p;float distance=Vector2.Distance(from,p);
            if(distance>Response.Settings.TeleportDistance){from=p;distance=0;_distance=0;}
            Vector2 forward=new Vector2(transform.forward.x,transform.forward.z).normalized;
            if(forward.sqrMagnitude<.01f)forward=Vector2.up;
            Vector2 across=new Vector2(forward.y,-forward.x);
            _distance+=distance;
            bool moved=distance>.025f;
            int steps=Mathf.Max(1,Mathf.CeilToInt(distance/.22f));
            bool stamp=moved||!_hasPrevious;
            if(Shape==PCGContactShape.Feet){stamp=_distance>.70f||!_hasPrevious;if(stamp)_distance=0;steps=1;}
            if(stamp)
            {
                for(int step=1;step<=steps;step++)
                {
                    Vector2 at=Vector2.Lerp(from,p,(float)step/steps);
                    int parts=Shape==PCGContactShape.Drag?1:2;
                    for(int part=0;part<parts;part++)
                    {
                        if(Shape==PCGContactShape.Feet&&part!=(_footStep&1))continue;
                        Vector2 point=at+across*(parts==2?(part==0?-Width*.5f:Width*.5f):0);
                        Vector2 radius=Shape==PCGContactShape.Drag?new Vector2(Width*.5f,.42f):new Vector2(FootWidth,FootLength);
                        Response.Contact(point,forward,radius,distance/steps,now,ref _carry[part],!_hasPrevious);
                    }
                }
                if(Shape==PCGContactShape.Feet)_footStep++;
            }
            else for(int i=0;i<2;i++)_carry[i].Step(PCGSurfaceKind.Hard,0,now,false);
            _previous=p;_hasPrevious=true;
        }
    }
}
