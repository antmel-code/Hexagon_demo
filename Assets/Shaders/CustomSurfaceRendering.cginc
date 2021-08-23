#if !defined(CUSTOM_SURFACE_INCLUDED)
#define CUSTOM_SURFACE_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "CommonStructures.cginc"

void SetupFragmentNormal(inout InterpolateData inData, float4 normal, float normalStrenght)
{
    float3 mainNormal = UnpackScaleNormal(normal, normalStrenght);

    float3 tangentSpaceNormal = mainNormal.xzy;
    float3 binormal = cross(inData.normal, inData.tangent.xyz) * inData.tangent.w;
    inData.normal.xyz = normalize(
        tangentSpaceNormal.x * inData.tangent +
        tangentSpaceNormal.y * inData.normal +
        tangentSpaceNormal.z * binormal
    );
}

fixed4 SurfaceStandard(SurfaceData inData, fixed4 worldSpaceLightPos, fixed4 lightColor, fixed3 cameraPos)
{
    SetupFragmentNormal(inData.interpolateData, inData.normal, 1);

    float3 lightDir;

    #if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
    lightDir = worldSpaceLightPos;
    #else
    lightDir = normalize(worldSpaceLightPos.xyz - inData.interpolateData.worldPos);
    #endif

    float3 viewDir = normalize(cameraPos.xyz - inData.interpolateData.worldPos);

    float lightMap = DotClamped(inData.interpolateData.normal, lightDir);
    float3 coloredLight = lightMap * lightColor;
    UNITY_LIGHT_ATTENUATION(attenuation, inData.interpolateData, inData.interpolateData.worldPos);
    coloredLight *= attenuation;

    float3 reflection = reflect(-lightDir, inData.interpolateData.normal);
    float specularMap = 1 - DotClamped(viewDir, inData.interpolateData.normal);
    float3 specularTint = lerp(lightColor, inData.color, inData.metallic);
    float3 specular = pow(DotClamped(viewDir, reflection), (1.01 - inData.roughness) * 100) * (1.01 - inData.roughness) * 10 * specularTint * specularMap * lightColor * attenuation;

    inData.color -= clamp((inData.metallic - inData.roughness), 0, 1) / 2;

    fixed4 res = float4(inData.color * coloredLight + specular, 1);

    #if defined(FORWARD_BASE_PASS)
    float3 shColor = ShadeSH9(float4(inData.interpolateData.normal, 1));
    res += float4(shColor, 1) * specularMap * lightMap * attenuation * 0.8;
    #endif

    UNITY_APPLY_FOG(inData.fogCoord, res);

    return res;
}

#endif