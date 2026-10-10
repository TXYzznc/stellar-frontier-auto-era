using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    public enum PCGSurfaceKind
    {
        [InspectorName("硬地")] Hard, [InspectorName("积雪")] Snow,
        [InspectorName("沙地")] Sand, [InspectorName("泥地")] Mud,
        [InspectorName("火山灰")] Ash, [InspectorName("水面")] Water,
        [InspectorName("熔岩")] Lava
    }
    [Serializable]
    public struct PCGSurfaceProfile
    {
        public PCGSurfaceKind Kind;
        public float Depth, Life, Softness;
        public uint LiquidId;
        public static PCGSurfaceProfile From(PCGEnvironmentSample e, float patch)
        {
            if(e.Liquid!=PCGLiquidKind.None && e.LiquidLevel>e.Height+.03f)
                return new PCGSurfaceProfile{Kind=e.Liquid==PCGLiquidKind.Lava?PCGSurfaceKind.Lava:PCGSurfaceKind.Water,Life=4,LiquidId=e.LiquidId};
            if(e.Cover.Snow>.35f)return new PCGSurfaceProfile{Kind=PCGSurfaceKind.Snow,Depth=Mathf.Lerp(.045f,.23f,patch)*e.Cover.Snow,Life=90,Softness=patch};
            if(e.Cover.Rock>.50f||e.Slope>36)return new PCGSurfaceProfile{Kind=PCGSurfaceKind.Hard,Life=12};
            if(e.Cover.Sand>.35f)return new PCGSurfaceProfile{Kind=patch<.18f?PCGSurfaceKind.Hard:PCGSurfaceKind.Sand,Depth=patch<.18f?0:Mathf.Lerp(.025f,.15f,patch)*e.Cover.Sand,Life=35,Softness=patch};
            if(e.Cover.Mud>.30f)return new PCGSurfaceProfile{Kind=PCGSurfaceKind.Mud,Depth=.13f*e.Cover.Mud,Life=65,Softness=.4f};
            return new PCGSurfaceProfile{Kind=e.Cover.Volcanic>.45f?PCGSurfaceKind.Ash:PCGSurfaceKind.Hard,Depth=0,Life=12};
        }
    }
    public struct PCGSurfaceCarry
    {
        public PCGSurfaceKind Solid;
        public float Amount, Wet, LastTime;
        public void Step(PCGSurfaceKind kind,float distance,float now,bool pickup)
        {
            float dt=Mathf.Max(0,now-LastTime);LastTime=now;
            Amount=Mathf.Max(0,Amount-dt*.06f-distance*.085f);
            Wet=Mathf.Max(0,Wet-dt*.10f-distance*.09f);
            if(!pickup)return;
            if(kind==PCGSurfaceKind.Water){Wet=1;Amount*=.65f;return;}
            if(kind!=PCGSurfaceKind.Snow&&kind!=PCGSurfaceKind.Sand&&kind!=PCGSurfaceKind.Mud)return;
            if(Solid!=kind){Amount*=.5f;if(Amount<.35f){Solid=kind;Amount=0;}}
            if(Solid==kind)Amount=Mathf.Min(1,Amount+.28f);
            if(kind==PCGSurfaceKind.Mud)Wet=Mathf.Max(Wet,.7f);
        }
        public Color Deposition(PCGSurfaceKind target)
        {
            if(Amount<.02f||Solid==target)return Color.clear;
            Color c=Solid==PCGSurfaceKind.Snow?new Color(.86f,.92f,.97f):Solid==PCGSurfaceKind.Sand?new Color(.72f,.55f,.32f):new Color(.23f,.27f,.18f);
            c.a=Amount*.75f;return c;
        }
    }
    /// <summary>World-coordinate, bounded CPU authority. Display textures are rebuildable caches.</summary>
    public sealed class PCGSurfaceHistory
    {
        public const float CellSize=.25f;
        public struct Cell
        {
            public Vector2Int Key;
            public float Depth, Pressure, Wet, Time, Life, Height;
            public float DepositTime, DepositLife;
            public Color Deposit;
        }
        private readonly Dictionary<Vector2Int,int> _indices;
        private readonly Cell[] _cells;
        private int _count,_cursor;
        public int Count=>_count;
        public int Capacity=>_cells.Length;
        public long Accepted {get;private set;}
        public long Rejected {get;private set;}
        public PCGSurfaceHistory(int capacity)
        {_cells=new Cell[Mathf.Max(1,capacity)];_indices=new Dictionary<Vector2Int,int>(_cells.Length);}
        public void Clear(){_indices.Clear();_count=_cursor=0;Accepted=Rejected=0;}
        public Cell At(int index)=>_cells[index];
        public static float Remaining(Cell c,float now)=>Mathf.Clamp01(1-(now-c.Time)/Mathf.Max(.01f,c.Life));
        public static float DepositRemaining(Cell c,float now)=>c.Deposit.a>0?Mathf.Clamp01(1-(now-c.DepositTime)/Mathf.Max(.01f,c.DepositLife)):0;
        public bool TrySample(Vector2 p,float now,out Cell c)
        {
            if(_indices.TryGetValue(Key(p),out int i)){c=_cells[i];float r=Remaining(c,now),d=DepositRemaining(c,now);c.Depth*=r*r;c.Pressure*=r;c.Wet*=r;c.Deposit.a*=d;return r>0||d>0;}
            c=default;return false;
        }
        private static Vector2Int Key(Vector2 p)=>new Vector2Int(Mathf.FloorToInt(p.x/CellSize),Mathf.FloorToInt(p.y/CellSize));
        public void Clean(float now,int budget)
        {
            for(int n=0;n<budget&&_count>0;n++)
            {
                if(_cursor>=_count)_cursor=0;
                if(Remaining(_cells[_cursor],now)<=0&&DepositRemaining(_cells[_cursor],now)<=0)Remove(_cursor);else _cursor++;
            }
        }
        private void Remove(int i)
        {
            _indices.Remove(_cells[i].Key);_count--;
            if(i<_count){_cells[i]=_cells[_count];_indices[_cells[i].Key]=i;}
        }
        public void Stamp(Vector2 p,Vector2 forward,Vector2 radius,PCGSurfaceProfile profile,Color deposit,float wet,float now,float height=0,float depositLife=25)
        {
            forward=forward.sqrMagnitude>.001f?forward.normalized:Vector2.up;
            Vector2 across=new Vector2(forward.y,-forward.x);
            float extent=Mathf.Max(radius.x,radius.y)*1.4f+CellSize;
            Vector2Int lo=Key(p-Vector2.one*extent),hi=Key(p+Vector2.one*extent);
            bool soft=profile.Depth>0;
            for(int z=lo.y;z<=hi.y;z++)for(int x=lo.x;x<=hi.x;x++)
            {
                var key=new Vector2Int(x,z);Vector2 d=new Vector2((x+.5f)*CellSize,(z+.5f)*CellSize)-p;
                float u=Vector2.Dot(d,across)/Mathf.Max(.12f,radius.x),v=Vector2.Dot(d,forward)/Mathf.Max(.12f,radius.y);
                float r=Mathf.Sqrt(u*u+v*v);if(r>1.4f)continue;
                float inner=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1,r));
                float lip=soft?Mathf.Max(0,1-Mathf.Abs(r-1.10f)/.30f)*profile.Depth*.32f:0;
                if(inner<.001f&&lip<.001f)continue;
                if(!_indices.TryGetValue(key,out int index))
                {
                    if(_count>=Capacity){Rejected++;continue;}
                    index=_count++;_indices.Add(key,index);_cells[index]=new Cell{Key=key};
                }
                Cell c=_cells[index];float remain=Remaining(c,now);
                float oldDepth=c.Depth*remain*remain;
                float desired=inner>0?profile.Depth*inner:-lip;
                c.Depth=inner>0?Mathf.Max(oldDepth,desired):Mathf.Min(oldDepth,desired);
                c.Pressure=Mathf.Max(c.Pressure*remain,inner*(soft||profile.Kind==PCGSurfaceKind.Ash?1:.15f));
                c.Wet=Mathf.Max(c.Wet*remain,wet*inner);
                Color old=c.Deposit;old.a*=DepositRemaining(c,now);Color incoming=deposit;incoming.a*=inner;
                if(incoming.a>old.a){c.Deposit=incoming;c.DepositTime=now;c.DepositLife=depositLife;}
                c.Time=now;c.Height=height;c.Life=profile.Life;_cells[index]=c;Accepted++;
            }
        }
    }
}
