Shader "Custom/TerrainBlendedLit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _MainTex("Terrain Texture Array", 2DArray) = "" {}
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _TextureScale("Texture Scale", Float) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"

            TEXTURE2D_ARRAY(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Metallic;
                float _Smoothness;
                float _TextureScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;      
                float4 texcoord2  : TEXCOORD2;  
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float4 weights     : COLOR;
                float4 layers      : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.weights = IN.color;
                OUT.layers = IN.texcoord2;

                #if defined(MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_SHADOWS_CASCADE)
                    OUT.shadowCoord = TransformWorldToShadowCoord(OUT.worldPos);
                #else
                    OUT.shadowCoord = float4(0,0,0,0);
                #endif

                return OUT;
            }

            half4 SampleLayer(Varyings IN, int idx)
            {
                float2 uv = IN.worldPos.xz * _TextureScale;
                float layer = IN.layers[idx];
                layer = max(layer, 0.0);
                half4 t = SAMPLE_TEXTURE2D_ARRAY(_MainTex, sampler_MainTex, uv, layer);
                return t * IN.weights[idx];
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float total = IN.weights.x + IN.weights.y + IN.weights.z + IN.weights.w;
                float4 w = IN.weights;
                if (total > 1e-5) w /= total;
                else w = float4(0.25,0.25,0.25,0.25);

                IN.weights = w;

                half4 blended =
                      SampleLayer(IN, 0)
                    + SampleLayer(IN, 1)
                    + SampleLayer(IN, 2)
                    + SampleLayer(IN, 3);

                float3 albedo = blended.rgb * _BaseColor.rgb;

                InputData inputData;
                ZERO_INITIALIZE(InputData, inputData);

                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(IN.worldPos));

                float4 clipPos = TransformWorldToHClip(IN.worldPos);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(clipPos);

                #if defined(MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_SHADOWS_CASCADE)
                    inputData.shadowCoord = IN.shadowCoord;
                #else
                    inputData.shadowCoord = float4(0,0,0,0);
                #endif

                inputData.fogCoord = 0;
                inputData.vertexLighting = float3(0,0,0);
                inputData.bakedGI = float3(0,0,0);
                inputData.shadowMask = float4(1,1,1,1);

                SurfaceData surfaceData;
                ZERO_INITIALIZE(SurfaceData, surfaceData);

                surfaceData.albedo = albedo;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0,0,1); 
                surfaceData.occlusion = 1;
                surfaceData.emission = 0;
                surfaceData.alpha = 1;

                float3 outColor = UniversalFragmentPBR(inputData, surfaceData);

                return float4(outColor, 1.0);
            }

            ENDHLSL
        }


        Pass
        {
            Name "ForwardAdd"
            Tags { "LightMode" = "UniversalForwardAdd" }

            Blend One One 

            HLSLPROGRAM
            #pragma vertex vert2
            #pragma fragment frag_add
            #pragma target 3.5

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"

            TEXTURE2D_ARRAY(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Metallic;
                float _Smoothness;
                float _TextureScale;
            CBUFFER_END

            struct Attributes2
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
                float4 texcoord2  : TEXCOORD2;
            };

            struct Varyings2
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float4 weights     : COLOR;
                float4 layers      : TEXCOORD2;
            };

            Varyings2 vert2(Attributes2 IN)
            {
                Varyings2 OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.weights = IN.color;
                OUT.layers = IN.texcoord2;
                return OUT;
            }

            half4 SampleLayer2(Varyings2 IN, int idx)
            {
                float2 uv = IN.worldPos.xz * _TextureScale;
                float layer = IN.layers[idx];
                layer = max(layer, 0.0);
                half4 t = SAMPLE_TEXTURE2D_ARRAY(_MainTex, sampler_MainTex, uv, layer);
                return t * IN.weights[idx];
            }

            float4 frag_add(Varyings2 IN) : SV_Target
            {
                float total = IN.weights.x + IN.weights.y + IN.weights.z + IN.weights.w;
                float4 w = IN.weights;
                if (total > 1e-5) w /= total;
                else w = float4(0.25,0.25,0.25,0.25);
                IN.weights = w;

                half4 blended =
                      SampleLayer2(IN, 0)
                    + SampleLayer2(IN, 1)
                    + SampleLayer2(IN, 2)
                    + SampleLayer2(IN, 3);

                float3 albedo = blended.rgb * _BaseColor.rgb;

                InputData inputData;
                ZERO_INITIALIZE(InputData, inputData);

                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(IN.worldPos));

                float4 clipPos = TransformWorldToHClip(IN.worldPos);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(clipPos);

                inputData.fogCoord = 0;
                inputData.vertexLighting = float3(0,0,0);
                inputData.bakedGI = float3(0,0,0);
                inputData.shadowMask = float4(1,1,1,1);
                inputData.shadowCoord = float4(0,0,0,0);

                SurfaceData surfaceData;
                ZERO_INITIALIZE(SurfaceData, surfaceData);

                surfaceData.albedo = albedo;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0,0,1);
                surfaceData.occlusion = 1;
                surfaceData.emission = 0;
                surfaceData.alpha = 1;

                float3 outColor = UniversalFragmentPBR(inputData, surfaceData);

                return float4(outColor, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
