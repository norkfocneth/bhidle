Shader "TERRAGRAV/TrailRibbon"
{
    Properties
    {
        _BaseColor ("Base Color Tint", Color) = (1,1,1,1)
        _EdgeGlowIntensity ("Edge Glow Intensity", Range(0, 3)) = 1.2
        _EdgeGlowPower ("Edge Glow Power", Range(1, 10)) = 3.0
        _SheenSpeed ("Sheen Animation Speed", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry+10" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
                float4 color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _EdgeGlowIntensity;
                float _EdgeGlowPower;
                float _SheenSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Edge glow based on ribbon width UV (0.0 at left, 1.0 at right, 0.5 at center)
                float edgeFactor = abs(input.uv.x - 0.5) * 2.0; // 0 at center, 1 at edges
                float edgeGlow = pow(edgeFactor, _EdgeGlowPower) * _EdgeGlowIntensity;

                // Subtle flowing sheen along the trail length
                float sheen = sin(input.uv.y * 20.0 - _Time.y * _SheenSpeed) * 0.1 + 0.1;

                // Diffuse lighting
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(input.normalWS, lightDir)) * 0.4 + 0.6;

                float3 baseCol = input.color.rgb * _BaseColor.rgb * NdotL;
                float3 finalColor = baseCol + (input.color.rgb * edgeGlow) + (float3(1, 1, 1) * sheen);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
