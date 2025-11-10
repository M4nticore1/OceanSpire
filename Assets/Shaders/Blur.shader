// Shader "Hidden/SimpleBlur"
// {  
//     SubShader
//     {     
//         HLSLINCLUDE
        
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    
//         struct Attributes
//         {
//             float4 positionOS    : POSITION;
//             float2 uv            : TEXCOORD0;            
//         };

//         struct Varyings
//         {
//             float4 positionCS   : SV_POSITION;
//             float2 uv           : TEXCOORD0;
//         };

//         TEXTURE2D_X(_MainTex);
//         SAMPLER(sampler_MainTex);

//         float4 _MainTex_TexelSize;
            
//         Varyings Vert(Attributes input)
//         {
//             Varyings output;
//             output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
//             output.uv = input.uv;
//             return output;
//         }

//         half4 Frag(Varyings input) : SV_Target
//         {
//             float2 offset = _MainTex_TexelSize.xy;
//             float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

//             half4 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv);
//             color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2(-1, 1) * offset);
//             color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2( 1, 1) * offset); 
//             color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2( 1,-1) * offset);
//             color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2(-1,-1) * offset);
            
//             return color / 5.0;
//              return 0;
//         }

//         ENDHLSL

//         Pass
//         {
//             HLSLPROGRAM
//             #pragma target 3.5

//             #pragma vertex Vert
//             #pragma fragment Frag

//             ENDHLSL
//         }
//     }

// }

Shader "Hidden/SimpleBlur"
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
