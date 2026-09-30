Shader "Custom/UnlitInstanced"
{
    Properties
    {
        // ѕусто Ч всЄ instanced
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(Texture2D, _BaseMap)
                UNITY_DEFINE_INSTANCED_PROP(float4,    _BaseColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv; // без _ST

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                Texture2D baseMap = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseMap);
                half4 baseColor   = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);

                return baseMap.Sample(sampler_BaseMap, IN.uv) * baseColor;
            }

            ENDHLSL
        }
    }
}