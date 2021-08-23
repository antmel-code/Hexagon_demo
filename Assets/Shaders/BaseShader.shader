Shader "Unlit/BaseShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _Roughness("Roughness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            #pragma multi_compile _ SHADOWS_SCREEN
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            #define FORWARD_BASE_PASS

            #include "Light.cginc"

            
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd"}

            Blend One One

            ZWrite Off

            CGPROGRAM

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            #include "Light.cginc"

            ENDCG
        }
        
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM

            #pragma target 3.0

            #pragma multi_compile_shadowcaster

            #pragma vertex ShadowVertexProgram
            #pragma fragment ShadowFragmentProgram

            #include "Shadows.cginc"

            ENDCG
        }
        
    }
}
