Shader "Universal Render Pipeline/2D/Sprite-Lit-Shadow-Improved"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _AmbientStrength("Ambient Strength", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend One OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex SpriteVertex
            #pragma fragment SpriteFragment
            #pragma target 2.0

            // Material Keywords
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile_local _ ETC1_EXTERNAL_ALPHA

            // Universal Pipeline keywords - these are crucial for proper lighting
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS

            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _RendererColor;
                float4 _Flip;
                float _EnableExternalAlpha;
                float _Cutoff;
                float _AmbientStrength;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                float2 dynamicLightmapUV : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 4);
                #ifdef DYNAMICLIGHTMAP_ON
                    float2 dynamicLightmapUV : TEXCOORD5;
                #endif
                half4 fogFactorAndVertexLight : TEXCOORD6;
            };

            Varyings SpriteVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                // Apply sprite flipping
                float3 positionOS = input.positionOS;
                positionOS.xy *= _Flip.xy;

                // World position and normal for lighting
                output.positionWS = TransformObjectToWorld(positionOS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // For sprites, ensure the normal faces forward if it's zero
                if (length(output.normalWS) < 0.1)
                {
                    output.normalWS = float3(0, 0, 1);
                }
                output.normalWS = normalize(output.normalWS);

                // Pixel snapping
                #if defined(PIXELSNAP_ON)
                    float4 clipPos = TransformObjectToHClip(positionOS);
                    clipPos.xy = round(clipPos.xy * _ScreenParams.xy) / _ScreenParams.xy;
                    output.positionCS = clipPos;
                #else
                    output.positionCS = TransformObjectToHClip(positionOS);
                #endif
                
                // Shadow coordinates
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.shadowCoord = GetShadowCoord(vertexInput);

                // UV and color
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;

                // Lightmap UVs
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                #ifdef DYNAMICLIGHTMAP_ON
                    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif

                // SH/vertex lights
                OUTPUT_SH4(output.positionWS, output.normalWS, GetWorldSpaceNormalizeViewDir(output.positionWS), output.vertexSH, output.fogFactorAndVertexLight.yzw);

                return output;
            }

            half4 SpriteFragment(Varyings input) : SV_Target
            {
                // Sample sprite texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Handle external alpha
                #if ETC1_EXTERNAL_ALPHA
                    half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv);
                    texColor.a = lerp(texColor.a, alpha.r, _EnableExternalAlpha);
                #endif

                half4 color = texColor * input.color;
                
                // Alpha test
                clip(color.a - _Cutoff);

                // Prepare surface data
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = color.rgb;
                surfaceData.alpha = color.a;
                surfaceData.metallic = 0;
                surfaceData.smoothness = 0;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.emission = 0;
                surfaceData.occlusion = 0.0;

                // Prepare input data
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

                // Calculate lighting using Universal RP's lighting functions
                half4 finalColor = UniversalFragmentBlinnPhong(inputData, surfaceData);
                
                // Add some ambient lighting for sprites to ensure they're visible
                finalColor.rgb = max(finalColor.rgb, surfaceData.albedo * _AmbientStrength);
                
                // Premultiply alpha for proper blending
                finalColor.rgb *= finalColor.a;
                
                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma target 2.0

            #pragma multi_compile_local _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _RendererColor;
                float4 _Flip;
                float _EnableExternalAlpha;
                float _Cutoff;
                float _AmbientStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output;
                
                // Apply sprite flipping
                float3 positionOS = input.positionOS.xyz;
                positionOS.xy *= _Flip.xy;
                
                // Get proper shadow bias
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // Ensure normal exists for sprites
                if (length(normalWS) < 0.1)
                {
                    normalWS = float3(0, 0, 1);
                }
                normalWS = normalize(normalWS);
                
                // Apply shadow bias
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(vertexInput.positionWS, normalWS, _MainLightPosition.xyz));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_TARGET
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                #if ETC1_EXTERNAL_ALPHA
                    half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv);
                    texColor.a = lerp(texColor.a, alpha.r, _EnableExternalAlpha);
                #endif

                half4 color = texColor * input.color;
                
                clip(color.a - _Cutoff);
                
                return 0;
            }
            ENDHLSL
        }

        // Depth-only pass for depth texture generation
        Pass
        {
            Name "DepthOnly"
            Tags {"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask R
            Cull Off

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma target 2.0

            #pragma multi_compile_local _ ETC1_EXTERNAL_ALPHA

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _RendererColor;
                float4 _Flip;
                float _EnableExternalAlpha;
                float _Cutoff;
                float _AmbientStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                float3 positionOS = input.positionOS.xyz;
                positionOS.xy *= _Flip.xy;

                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                #if ETC1_EXTERNAL_ALPHA
                    half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv);
                    texColor.a = lerp(texColor.a, alpha.r, _EnableExternalAlpha);
                #endif

                half4 color = texColor * input.color;
                clip(color.a - _Cutoff);
                
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
    
    Fallback "Sprites/Default"
}