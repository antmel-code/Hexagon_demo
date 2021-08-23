#if !defined(COMMON_STRUCTURES_INCLUDED)
#define COMMON_STRUCTURES_INCLUDED

struct VertexData
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
};

struct VertexDataFull
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 color : COLOR;
    float3 uv : TEXCOORD0;
    float3 uv1 : TEXCOORD1;
    float3 uv2 : TEXCOORD2;
    float3 uv3 : TEXCOORD3;
};

struct InterpolateData
{
    float2 uv : TEXCOORD0;
    float4 pos : SV_POSITION;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    float4 tangent : TEXCOORD3;
    UNITY_FOG_COORDS(4)
    SHADOW_COORDS(5)
};

struct InterpolateDataExtended
{
    float4 pos : SV_POSITION;
    float3 uv : TEXCOORD0;
    float3 uv1 : TEXCOORD1;
    float3 uv2 : TEXCOORD2;
    float3 uv3 : TEXCOORD3;
    float4 color : TEXCOORD4;
    float3 normal : TEXCOORD5;
    float3 worldPos : TEXCOORD6;
    float4 tangent : TEXCOORD7;
    UNITY_FOG_COORDS(8)
    SHADOW_COORDS(9)
};

struct SurfaceData
{
    InterpolateData interpolateData;
    fixed4 color;
    half4 normal;
    float roughness;
    float metallic;
    float occlusion;
};

InterpolateData MakeFromExtended(InterpolateDataExtended ex)
{
    InterpolateData res;
    res.pos = ex.pos;
    res.normal = ex.normal;
    res.worldPos = ex.worldPos;
    res.uv = ex.uv;
    res.tangent = ex.tangent;
    #if defined (SHADOWS_SCREEN) || defined (SHADOWS_DEPTH) || defined (SHADOWS_CUBE)
    res._ShadowCoord = ex._ShadowCoord;
    #endif
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    res.fogCoord = ex.fogCoord;
    #endif
    return res;
}

#endif