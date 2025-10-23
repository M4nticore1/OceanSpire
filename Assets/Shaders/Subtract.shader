Shader "Hidden/Subtract"
{
    Properties { _Original("Original", 2D) = "white" {} _Blurred("Blurred", 2D) = "white" {} }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_Original);
            SAMPLER(sampler_Original);
            TEXTURE2D_X(_Blurred);
            SAMPLER(sampler_Blurred);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN) { Varyings OUT; OUT.posCS = TransformObjectToHClip(IN.positionOS.xyz); OUT.uv = IN.uv; return OUT; }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 blur = SAMPLE_TEXTURE2D_X(_Blurred, sampler_Blurred, IN.uv);
                half4 orig = SAMPLE_TEXTURE2D_X(_Original, sampler_Original, IN.uv);
                return saturate(blur - orig); // контур
            }
            ENDHLSL
        }
    }
}