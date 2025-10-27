Shader "Hidden/RenderFeatureOutline"
{
    Properties
    {
        [HideInInspector] _EpsilonBlack ("EpsilonBlack", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            TEXTURE2D(_BluredMaskTex);
            SAMPLER(sampler_BluredMaskTex);

            float4 _Color;
            float _Intensity;
            float _EpsilonBlack;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 maskColor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                float4 bluredMaskColor = SAMPLE_TEXTURE2D(_BluredMaskTex, sampler_BluredMaskTex, i.uv);

                float4 difColor = bluredMaskColor - maskColor;
                float4 newBaseColor = difColor * _Intensity;

                if (newBaseColor.r <= _EpsilonBlack)
                    newBaseColor = baseColor;

                newBaseColor = lerp(baseColor, _Color, saturate(difColor.r * _Intensity));

                return newBaseColor;
            }

            ENDHLSL
        }
    }
}
