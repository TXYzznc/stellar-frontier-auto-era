#ifndef AUTOERA_SHARED_WIND_INCLUDED
#define AUTOERA_SHARED_WIND_INCLUDED
float _AEWindActive, _AEWindTime;
float4 _AEWindParams, _AEWindGust, _AEWorldOrigin;
int _AEWindSourceCount;
float4 _AEWindPositions[16], _AEWindDirections[16];
float2 AEWorldXZ(float3 localWorld) { return localWorld.xz+_AEWorldOrigin.xy; }
float2 AEWind(float2 world)
{
    float t=_AEWindTime;
    float gust=.8+.2*sin(t*.42-world.x*.014-world.y*.020+_AEWindGust.x);
    float wave=.85+.15*sin(world.x*.031+world.y*.027-t*.7);
    float2 result=_AEWindParams.xy*(_AEWindParams.z*gust*wave);
    [loop] for(int index=0;index<_AEWindSourceCount;index++)
    {
        float4 source=_AEWindPositions[index],direction=_AEWindDirections[index];
        float2 delta=world-source.xy;
        float distance=length(delta);
        float weight=pow(saturate(1-distance/max(.01,source.z)),direction.w);
        float2 local=direction.z>.5?(distance>.001?delta/distance:float2(0,0)):direction.xy;
        result+=local*(source.w*weight);
    }
    float speed=length(result);
    return result*min(1,_AEWindGust.y/max(.0001,speed));
}
#endif
