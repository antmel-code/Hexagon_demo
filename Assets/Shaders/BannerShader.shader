Shader "Custom/BannerShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Displacement ("Displacement", 2D) = "white" {}
        _Normal("Normal", 2D) = "bump" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _AnimationSpeed("Animation Speed", Range(0, 1)) = 0.5
        _DispStrenght("Displacemant Strenght", Range(0, 1)) = 0.1
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

        sampler2D _Displacement;
        sampler2D _Normal;
        half _DispStrenght;

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float2 tuv;
        };

        void vert(inout appdata_full v, out Input data) {
            //v.vertex = float4(0, 0, 0, 0);
            v.vertex += tex2Dlod(_Displacement, float4(v.texcoord.xy + _Time.yy, 0, 0)) * _DispStrenght * v.color.r;
            //v.vertex += tex2Dlod(_Displacement, float4(0, 0, 0, 0)).xxxx;
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.tuv = v.texcoord.xy;
        }

        half _Glossiness;
        half _Metallic;
        half _AnimationSpeed;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.tuv + _Time.yy * _AnimationSpeed;
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Normal = UnpackNormal(lerp(float4(0.5, 0.5, 1, 1), tex2D(_Normal, uv), IN.color.r));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
