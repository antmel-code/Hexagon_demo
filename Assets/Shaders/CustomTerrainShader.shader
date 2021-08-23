Shader "Custom/CustomTerrainShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _ColorTexArray("Terrain Texture Array", 2DArray) = "white" {}
        _NormalMapArray("Terrain Normal Array", 2DArray) = "bump" {}
        _Roughness("Roughness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Tiling("Tiling", Range(0, 10)) = 0.02
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
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

            #include "TerrainShading.cginc"


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

            #include "TerrainShading.cginc"

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