Shader "Hidden/SimpleBlur"
{  
    SubShader
    {     
        HLSLINCLUDE
        
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    
        struct Attributes
        {
            float4 positionOS    : POSITION;
            float2 uv            : TEXCOORD0;            
        };

        struct Varyings
        {
            float4 positionCS   : SV_POSITION;
            float2 uv           : TEXCOORD0;
        };

        TEXTURE2D_X(_MainTex);
        SAMPLER(sampler_MainTex);

        float4 _MainTex_TexelSize;
            
        Varyings Vert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.uv = input.uv;
            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            float2 offset = _MainTex_TexelSize.xy;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            half4 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv);
            color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2(-1, 1) * offset);
            color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2( 1, 1) * offset); 
            color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2( 1,-1) * offset);
            color += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv + float2(-1,-1) * offset);
            
            return color / 5.0;
        }

        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            #pragma multi_compile_fragment _ _SAMPLE_TEXTURE2D_X

            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }
    }

}