Shader "OceanSpire/FlickingShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "IgnoreProjector"="True"
        }
        
        // Настройки пасса по умолчанию
        LOD 100
        Cull Back
        ZWrite On
        ZTest LEqual
        ColorMask RGBA
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Ключевые директивы для GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            // Теневые директивы
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                
                // Обязательно для GPU Instancing
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                
                // Передаем ID инстанса в фрагментный шейдер
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            // Буфер для данных инстансов
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
            
            Varyings vert(Attributes v)
            {
                Varyings o;
                
                // Инициализация и синхронизация ID инстанса
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                // Получаем матрицу преобразования для текущего инстанса
                float3 positionWS = TransformObjectToWorld(v.position.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normal);
                
                o.position = TransformWorldToHClip(positionWS);
                o.worldPos = positionWS;
                o.normal = normalWS;
                
                return o;
            }
            
            float4 frag(Varyings i) : SV_Target
            {
                // Восстанавливаем ID инстанса
                UNITY_SETUP_INSTANCE_ID(i);
                
                // Получаем свойства для текущего инстанса
                float4 color = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Color);
                
                // Нормализуем
                float3 normal = normalize(i.normal);
                
                // Теневые координаты
                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                
                // Основной свет с тенями
                Light mainLight = GetMainLight(shadowCoord);
                
                // Диффузное освещение
                float NdotL = saturate(dot(normal, mainLight.direction));
                float3 directLight = mainLight.color * NdotL * mainLight.shadowAttenuation;
                
                // Ambient (окружающее освещение)
                float3 ambient = SampleSH(normal);
                
                // Финальный цвет
                float3 finalColor = color.rgb * (directLight + ambient);
                
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        // Shadow Caster Pass с поддержкой инстансинга
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 position : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                float3 positionWS = TransformObjectToWorld(v.position.xyz);
                o.position = TransformWorldToHClip(ApplyShadowBias(positionWS, TransformObjectToWorldNormal(v.normal), _MainLightPosition.xyz));
                
                #if UNITY_REVERSED_Z
                    o.position.z = min(o.position.z, o.position.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    o.position.z = max(o.position.z, o.position.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return o;
            }
            
            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                return 0;
            }
            ENDHLSL
        }
        
        // Depth Only Pass с поддержкой инстансинга
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 position : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 position : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                float3 positionWS = TransformObjectToWorld(v.position.xyz);
                o.position = TransformWorldToHClip(positionWS);
                
                return o;
            }
            
            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}