Shader "Unlit/HexSelectorShader"
{
    Properties
    {
        _HexMap ("Hex Map", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Waves("Waves", Range(0, 1)) = 0
        _Highlight("Highlight", Range(0, 1)) = 0
        _Opacity("Opacity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull off
        LOD 100
        Offset -2, -2

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 color : COLOR;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 color: TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _HexMap;
            float4 _Color;
            float _Waves;
            float _Highlight;
            float _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float alpha = tex2D(_HexMap, i.uv).r * i.color.r;

                float wave = (sin((i.color.g + _Time.y) * 15) + 1) / 2;
                wave *= i.color.g;
                wave = smoothstep(0.5, 0.75, wave);

                float highlight = i.color.g;

                alpha += wave * _Waves;
                alpha += highlight * _Highlight;
                alpha = smoothstep(0, 1, alpha);

                alpha *= smoothstep(0.1, 0.3, i.color.b);
                alpha *= _Opacity;

                fixed4 col = _Color;
                col.a = alpha;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
