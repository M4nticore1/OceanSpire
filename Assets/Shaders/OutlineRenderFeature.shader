// Shader "Hidden/RenderFeatureOutline"
// {
//     Properties
//     {
//         [HideInInspector] _EpsilonBlack ("EpsilonBlack", Float) = 0.5
//     }

//     SubShader
//     {
//         Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

//         Pass
//         {
//             ZTest Always
//             ZWrite Off
//             Cull Off

//             HLSLPROGRAM

//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma target 3.5
//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//             TEXTURE2D(_MainTex);
//             SAMPLER(sampler_MainTex);

//             TEXTURE2D(_MaskTex);
//             SAMPLER(sampler_MaskTex);

//             TEXTURE2D(_BluredMaskTex);
//             SAMPLER(sampler_BluredMaskTex);

//             float4 _Color;
//             float _Intensity;
//             float _EpsilonBlack;

//             struct Attributes
//             {
//                 float4 positionOS : POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             struct Varyings
//             {
//                 float4 positionCS : SV_POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             Varyings vert(Attributes input)
//             {
//                 Varyings o;
//                 o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
//                 o.uv = input.uv;
//                 return o;
//             }

//             half4 frag(Varyings i) : SV_Target
//             {
//                 float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
//                 float4 maskColor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
//                 float4 bluredMaskColor = SAMPLE_TEXTURE2D(_BluredMaskTex, sampler_BluredMaskTex, i.uv);

//                 float4 difColor = bluredMaskColor - maskColor;
//                 float4 newBaseColor = difColor * _Intensity;

//                 if (newBaseColor.r <= _EpsilonBlack)
//                     newBaseColor = baseColor;

//                 newBaseColor = lerp(baseColor, _Color, saturate(difColor.r * _Intensity));

//                 return newBaseColor;
//                 return 0;
//             }

//             ENDHLSL
//         }
//     }
// }

Shader "Hidden/RenderFeatureOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
