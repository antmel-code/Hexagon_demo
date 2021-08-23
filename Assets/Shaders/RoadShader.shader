Shader "Custom/RoadShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset]
        _NormalMap ("Normal", 2D) = "normal" {}
        [NoScaleOffset]
        _ARMHMap ("ARMH", 2D) = "red" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale ("Scale", Range(0,1)) = 0.025
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+1"}
        LOD 200
        Offset -1, -1

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows decal:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _ARMHMap;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half _Scale;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            // fixed4 c = IN.uv_MainTex.xxxx;
            fixed4 c = tex2D(_MainTex, IN.worldPos.xz * _Scale) * _Color;

            fixed4 armh = tex2D(_ARMHMap, IN.worldPos.xz * _Scale);

            fixed a = armh.r;
            fixed r = armh.g;
            fixed m = armh.b;
            fixed h = armh.a;

            float blend = IN.uv_MainTex.x;
            blend *= h + 0.5;
            blend = smoothstep(0.4, 0.5, blend);

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = m * _Metallic;
            o.Smoothness = (1 - r) * _Glossiness;
            o.Occlusion = a;
            o.Alpha = blend;
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.worldPos.xz * _Scale));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
