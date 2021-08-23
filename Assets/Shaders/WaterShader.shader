Shader "Custom/WaterShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _GridTex("GridTexture", 2D) = "white" {}
        [NoScaleOffset]
        _MainTex ("Albedo1 (RGB)", 2D) = "white" {}
        [NoScaleOffset]
        _MainTex2("Albedo2 (RGB)", 2D) = "white" {}
        [NoScaleOffset]
        _WavePattern ("Wave Pattern (RGB)", 2D) = "white" {}
        [NoScaleOffset]
        _WaterNormal("Water Normal (Normal Map)", 2D) = "normal" {}
        _WaterNormal2("Water Normal2 (Normal Map)", 2D) = "normal" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _PatternScale("WaveScale", Range(0,1)) = 0.025
        _LargePatternScale("LargeWaveScale", Range(0,1)) = 0.01
        _FlowScale("FlowScale", Range(0,1)) = 0.025
        _AnimationSpeed("AnimationSpeed", Range(-10,10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma multi_compile _ GRID_ON

        //sampler2D _MainTex;
        sampler2D _WavePattern;
        sampler2D _WaterNormal;
        sampler2D _WaterNormal2;
        sampler2D _GridTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv2_MainTex2;
            float3 worldPos;
            half4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        half _PatternScale;
        half _LargePatternScale;
        half _FlowScale;
        half _AnimationSpeed;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
            
        //void surf (Input IN, inout SurfaceOutputStandard o)
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = IN.color;

            float2 uv1 = IN.worldPos.xz;
            uv1.y += _Time.y * (_AnimationSpeed / 2);

            float2 uv2 = IN.worldPos.zx;
            uv2.y += _Time.y * (_AnimationSpeed / 2);

            float2 uv3 = IN.worldPos.xz;
            uv3.xy += _Time.yy * _AnimationSpeed;

            fixed shore = IN.uv_MainTex.x;
            shore = smoothstep(0.5, 1, shore);

            fixed merge = IN.uv_MainTex.y;
            merge = smoothstep(0.5, 1, merge);

            fixed wiveFreq = 18;
            fixed wave = sqrt(IN.uv_MainTex.x) * ((sin(IN.uv_MainTex.x * wiveFreq + _Time.y * _AnimationSpeed) + 1) / 2);
            fixed mergeWave = sqrt(IN.uv_MainTex.y) * ((sin(IN.uv_MainTex.y * wiveFreq - _Time.y * _AnimationSpeed) + 1) / 2);

            wave = lerp(wave, mergeWave, IN.uv_MainTex.y);
            wave = max(wave, shore);

            fixed pattern1 = tex2D(_WavePattern, uv1 * _PatternScale).x;
            fixed pattern2 = tex2D(_WavePattern, uv2 * _PatternScale).x;

            fixed largePattern = tex2D(_WavePattern, uv3 * _LargePatternScale).y;

            fixed pattern = pattern1 + pattern2 + wave * 2;
            pattern += largePattern - 0.5f;
            pattern = smoothstep(0.75, 2, pattern);

            float2 flow_uv = IN.uv2_MainTex2.xx + _Time.xy * _AnimationSpeed * 0.08;
            fixed flow = tex2D(_WavePattern, flow_uv * _FlowScale).x;
            flow += shore + (pattern1 + pattern2) / 2;
            flow = smoothstep(0.25, 0.75, flow);
            
            half3 normal1 = UnpackNormal(tex2D(_WaterNormal, uv1 * _PatternScale));
            half3 normal2 = UnpackNormal(tex2D(_WaterNormal, uv2 * _PatternScale));
            half3 normal3 = UnpackNormal(tex2D(_WaterNormal2, uv3 * _LargePatternScale));

            half3 normal = normalize(lerp(normal1, normal2, 0.5));
            normal = normalize(lerp(normal, normal3, 0.1));

            //pattern = clamp(0, 1, pattern);

            //c += pattern.xxxx;
            c += lerp(pattern.xxxx, flow.xxxx, smoothstep(0.5, 1, IN.uv2_MainTex2.y));

            fixed4 grid = 1;
            #if defined(GRID_ON)
            float2 gridUV = IN.worldPos.xz;
            gridUV.x *= 1 / (4 * 8.66025404);
            gridUV.y *= 1 / (2 * 15.0);
            grid = tex2D(_GridTex, gridUV);
            #endif

            //c.rg = IN.uv_MainTex.xy;
            //c.b = IN.uv2_MainTex2.y;
            o.Albedo = c.rgb * grid;
            //o.Albedo = shore.xxx;
            //o.Albedo = IN.uv2.xyx;
            //o.Albedo = _Color;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
            o.Alpha = _Color.a;
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
