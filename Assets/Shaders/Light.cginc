#if !defined(LIGHT_INCLUDED)
#define LIGHT_INCLUDED
            
            #include "UnityStandardBRDF.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "CustomSurfaceRendering.cginc"

            sampler2D _MainTex;
            sampler2D _NormalMap;
            float _Roughness;
            float _Metallic;

            InterpolateData VertexProgram (VertexData v)
            {
                InterpolateData outData;
                outData.pos = UnityObjectToClipPos(v.vertex);
                outData.uv = v.uv;
                outData.normal = UnityObjectToWorldNormal(v.normal);
                outData.worldPos = mul(unity_ObjectToWorld, v.vertex);
                outData.tangent = v.tangent;
                UNITY_TRANSFER_FOG(outData, v.pos);
                TRANSFER_SHADOW(outData);
                return outData;
            }

            fixed4 FragmentProgram (InterpolateData inData) : SV_Target
            {

            	inData.normal = normalize(inData.normal);

                fixed4 color = tex2D(_MainTex, inData.uv);
                fixed4 normal = tex2D(_NormalMap, inData.uv);

                SurfaceData surface;

                surface.interpolateData = inData;
                surface.color = color;
                surface.roughness = _Roughness;
                surface.metallic = _Metallic;
                surface.occlusion = 1;
                surface.normal = normal;

                return SurfaceStandard(surface, _WorldSpaceLightPos0, _LightColor0, _WorldSpaceCameraPos);
            }
#endif