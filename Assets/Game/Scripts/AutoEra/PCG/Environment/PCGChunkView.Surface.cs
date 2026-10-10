using UnityEngine;
using UnityEngine.Rendering;
namespace AutoEra.Art.PCG
{
    public sealed partial class PCGChunkView
    {
        private GameObject _surface;
        private Mesh _surfaceMesh;
        private Vector3[] _surfaceVertices,_surfaceNormals;
        private Color[] _surfaceColors;
        private Vector2[] _surfaceInfo;
        private static Color SoftCover(PCGEnvironmentSample e)=>new Color(e.Cover.Snow,e.Cover.Sand,e.Cover.Mud,e.Cover.Volcanic*.4f);
        private void BuildSurfaceLayer()
        {
            var response=_world.SurfaceResponse;
            if(response==null||response.Settings==null)return;
            int n=129,total=n*n;float span=_world.Settings.ChunkSize,step=span/(n-1);
            if(_surface==null)
            {
                _surface=new GameObject("Snow sand mud surface envelope");_surface.transform.SetParent(_root.transform,false);
                var filter=_surface.AddComponent<MeshFilter>();var renderer=_surface.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=response.Settings.SurfaceMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;
                _surfaceMesh=new Mesh{name="Bounded 0.5m surface grid"};_surfaceMesh.MarkDynamic();filter.sharedMesh=_surfaceMesh;
                _surfaceVertices=new Vector3[total];_surfaceNormals=new Vector3[total];_surfaceColors=new Color[total];_surfaceInfo=new Vector2[total];
                var indices=new int[(n-1)*(n-1)*6];int cursor=0;
                for(int z=0;z<n-1;z++)for(int x=0;x<n-1;x++)
                {int a=z*n+x;indices[cursor++]=a;indices[cursor++]=a+n;indices[cursor++]=a+1;indices[cursor++]=a+1;indices[cursor++]=a+n;indices[cursor++]=a+n+1;}
                _surfaceMesh.vertices=_surfaceVertices;_surfaceMesh.triangles=indices;
            }
            int r=_world.Settings.HeightResolution;float height=_world.Settings.HeightScale;
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                float u=x*(r-1f)/(n-1),v=z*(r-1f)/(n-1);int a=Mathf.Min((int)u,r-2),b=Mathf.Min((int)v,r-2);
                float h=Mathf.Lerp(Mathf.Lerp(_data.Heights[b,a],_data.Heights[b,a+1],u-a),Mathf.Lerp(_data.Heights[b+1,a],_data.Heights[b+1,a+1],u-a),v-b)*height+_world.Settings.HeightOrigin;
                var e=_data.Environment[Mathf.RoundToInt(v),Mathf.RoundToInt(u)];int index=z*n+x;
                float water=e.Liquid!=PCGLiquidKind.None&&e.LiquidLevel>h+.02f?0:1;
                _surfaceVertices[index]=new Vector3(x*step,h+.025f,z*step);
                float dx=(_data.Heights[b,a+1]-_data.Heights[b,a])*height, dz=(_data.Heights[b+1,a]-_data.Heights[b,a])*height;
                _surfaceNormals[index]=new Vector3(-dx,1,-dz).normalized;
                // Interpolate semantic coverage on the half-meter mesh instead of rounding to whole-meter cells.
                _surfaceColors[index]=Color.Lerp(Color.Lerp(SoftCover(_data.Environment[b,a]),SoftCover(_data.Environment[b,a+1]),u-a),
                    Color.Lerp(SoftCover(_data.Environment[b+1,a]),SoftCover(_data.Environment[b+1,a+1]),u-a),v-b)*water;
                _surfaceInfo[index]=new Vector2(e.Liquid!=PCGLiquidKind.None&&e.Liquid!=PCGLiquidKind.Lava?1:0,e.LiquidLevel);
            }
            _surfaceMesh.vertices=_surfaceVertices;_surfaceMesh.normals=_surfaceNormals;_surfaceMesh.colors=_surfaceColors;_surfaceMesh.uv=_surfaceInfo;
            _surfaceMesh.bounds=new Bounds(new Vector3(span*.5f,_world.Settings.HeightOrigin+height*.5f,span*.5f),new Vector3(span+2,height+4,span+2));
        }
    }
}
