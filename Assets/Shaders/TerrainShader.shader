Shader "Custom/TerrainShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _GridTex("Grid Texture", 2D) = "white" {}
        _ColorTexArray("Terrain Texture Array", 2DArray) = "white" {}
        _NormalMapArray("Terrain Normal Array", 2DArray) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Tiling ("Tiling", Range(0, 10)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        #pragma multi_compile _ GRID_ON

        UNITY_DECLARE_TEX2DARRAY(_ColorTexArray);
        UNITY_DECLARE_TEX2DARRAY(_NormalMapArray);

        sampler2D _GridTex;

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float3 terrain;
        };

        void vert(inout appdata_full v, out Input data) {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.terrain = v.texcoord2.xyz;
        }

        half _Glossiness;
        half _Metallic;
        half _Tiling;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4 GetTerrainColor (Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * _Tiling, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_ColorTexArray, uvw);
			return c * IN.color[index];
		}

        float4 GetTerrainNormal(Input IN, int index, out float blendFactor) {
            float3 uvw = float3(IN.worldPos.xz * _Tiling, IN.terrain[index]);
            float4 n = UNITY_SAMPLE_TEX2DARRAY(_NormalMapArray, uvw);
            blendFactor = IN.color[index];
            return n;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            float2 uv = IN.worldPos.xz * _Tiling;
            fixed4 c = GetTerrainColor(IN, 0) + GetTerrainColor(IN, 1) + GetTerrainColor(IN, 2);

            float blendFactor;
            half3 normal0 = GetTerrainNormal(IN, 0, blendFactor);
            half3 normal1 = GetTerrainNormal(IN, 1, blendFactor);
            half3 normal = lerp(normal1, normal0, blendFactor);
            half3 normal2 = GetTerrainNormal(IN, 2, blendFactor);
            normal = lerp(normal2, normal, blendFactor);
            normal = UnpackNormal(half4(normal, 1));

            fixed4 grid = 1;
            #if defined(GRID_ON)
            float2 gridUV = IN.worldPos.xz;
            gridUV.x *= 1 / (4 * 8.66025404);
            gridUV.y *= 1 / (2 * 15.0);
            grid = tex2D(_GridTex, gridUV);
            #endif

            //fixed4 c = IN.terrain.xyzz;
            o.Albedo = c.rgb * grid * _Color;
            o.Normal = normal;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
