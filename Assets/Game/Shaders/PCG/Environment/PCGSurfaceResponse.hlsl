#ifndef AE_SURFACE_RESPONSE_INCLUDED
#define AE_SURFACE_RESPONSE_INCLUDED
TEXTURE2D(_AESurfaceShape);SAMPLER(sampler_AESurfaceShape);
TEXTURE2D(_AESurfaceDeposit);SAMPLER(sampler_AESurfaceDeposit);
float4 _AESurfaceRect;
float _AESurfaceActive,_AESurfaceTime;
float4 _AERings[32],_AERingMeta[32],_AERingMotion[32];
float AEInSurface(float2 p)
{
    float2 uv=(p-_AESurfaceRect.xy)/max(1,_AESurfaceRect.z);
    float margin=min(min(uv.x,uv.y),min(1-uv.x,1-uv.y));
    return _AESurfaceActive*smoothstep(0,2.5/max(1,_AESurfaceRect.z),margin);
}
float4 AESurfaceShape(float2 p)
{
    float2 uv=(p-_AESurfaceRect.xy)/max(1,_AESurfaceRect.z);
    return SAMPLE_TEXTURE2D_LOD(_AESurfaceShape,sampler_AESurfaceShape,uv,0)*AEInSurface(p);
}
float4 AESurfaceDeposit(float2 p)
{
    float2 uv=(p-_AESurfaceRect.xy)/max(1,_AESurfaceRect.z);
    return SAMPLE_TEXTURE2D(_AESurfaceDeposit,sampler_AESurfaceDeposit,uv)*AEInSurface(p);
}
float AESurfaceHeight(float4 shape)
{
    return shape.a/max(.0001,max(max(shape.g,shape.b),abs(shape.r)*4));
}
float AELiquidRipple(float2 p,float2 identity)
{
    float result=0;
    [loop]for(int j=0;j<32;j++)
    {
        float age=_AESurfaceTime-_AERings[j].z;
        if(_AERings[j].w<.001||age<0||age>4||any(abs(identity-_AERingMeta[j].xy)>.1))continue;
        float dist=length(p-_AERings[j].xy);
        result+=sin((dist-age*2.7)*9)*exp(-abs(dist-age*2.7)*2.8)*(1-age*.25)*_AERings[j].w;
        float2 delta=p-_AERings[j].xy,dir=_AERingMotion[j].xy;
        float behind=-dot(delta,dir),side=abs(dot(delta,float2(dir.y,-dir.x)));
        float wake=exp(-abs(side-behind*.35)*6)*saturate(behind)*saturate(1-behind/5)*(1-age*.25);
        result+=wake*_AERings[j].w*.45*saturate(_AERingMotion[j].z*12);
    }
    return clamp(result,-1,1)*_AESurfaceActive;
}
#endif
