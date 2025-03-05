Shader"Custom/SpriteLitNormal"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}  // The main sprite texture
        _NormalMap ("Normal Map", 2D) = "bump" {}  // The normal map texture
        _NormalIntensity ("Normal Intensity", Range(0,2)) = 1 // Controls normal map strength
    }

    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent"}
LOD100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}
Blend SrcAlpha
OneMinusSrcAlpha // Ensures transparency works correctly
            Cull
Off // Disable backface culling
            ZWrite
Off // Prevents sprites from writing to the depth buffer
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 👇 Vertex Input Structure (Object Space)
struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

            // 👇 Vertex Output Structure (Clip Space)
struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
};

            // 👇 Texture Declarations
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

float _NormalIntensity;

            // 🏗 Vertex Shader
Varyings vert(Attributes IN)
{
    Varyings OUT;
    OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz); // Convert to clip space
    OUT.uv = IN.uv;
    return OUT;
}

            // 🎨 Fragment Shader (Lighting Applied)
half4 frag(Varyings IN) : SV_Target
{
    half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // Get the normal from the normal map
    half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
    normal *= _NormalIntensity; // Adjust intensity

                // Simulated light direction (you can change this)
    float3 lightDir = normalize(float3(0.5, 0.5, 1.0));

                // Calculate lighting effect
    float lighting = saturate(dot(normal, lightDir)) * 0.5 + 0.5;

    return half4(baseColor.rgb * lighting, baseColor.a); // Apply lighting to color
}

            ENDHLSL
        }
    }
}
