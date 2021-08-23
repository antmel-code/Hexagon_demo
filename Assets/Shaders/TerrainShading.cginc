#if !defined(LIGHT_INCLUDED)
#define LIGHT_INCLUDED
            
            #include "UnityStandardBRDF.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "CustomSurfaceRendering.cginc"

            UNITY_DECLARE_TEX2DARRAY(_ColorTexArray);
            sampler2D _NormalMapArray;
            float _Roughness;
            float _Metallic;
            float _Tiling;

            InterpolateDataExtended VertexProgram (VertexDataFull v)
            {
                InterpolateDataExtended outData;
                outData.pos = UnityObjectToClipPos(v.vertex);
                outData.uv = v.uv;
                outData.uv1 = v.uv1;
                outData.uv2 = v.uv2;
                outData.uv3 = v.uv3;
                outData.color = v.color;
                outData.normal = UnityObjectToWorldNormal(v.normal);
                outData.worldPos = mul(unity_ObjectToWorld, v.vertex);
                outData.tangent = v.tangent;
                UNITY_TRANSFER_FOG(outData, v.pos);
                TRANSFER_SHADOW(outData);
                return outData;
            }

            float4 GetTerrainColor(InterpolateDataExtended IN, int index) {
                float3 uvw = float3(IN.worldPos.xz * 0.02, IN.uv2[index]);
                float4 c = UNITY_SAMPLE_TEX2DARRAY(_ColorTexArray, uvw);
                return c * IN.color[index];
            }

            fixed4 FragmentProgram (InterpolateDataExtended inData) : SV_Target
            {

            	inData.normal = normalize(inData.normal);

                float2 uv = inData.worldPos.xz * _Tiling;
                fixed4 color = GetTerrainColor(inData, 0) + GetTerrainColor(inData, 1) + GetTerrainColor(inData, 2);

                fixed4 normal = tex2D(_NormalMapArray, inData.uv);
                float roughness = _Roughness;
                float metallic = _Metallic;
                
                SurfaceData surface;

                surface.interpolateData = MakeFromExtended(inData);
                surface.color = color;
                surface.roughness = _Roughness;
                surface.metallic = _Metallic;
                surface.occlusion = 1;
                surface.normal = normal;
                
                return SurfaceStandard(surface, _WorldSpaceLightPos0, _LightColor0, _WorldSpaceCameraPos);
            }
#endif