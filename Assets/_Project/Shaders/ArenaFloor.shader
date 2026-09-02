Shader "TerraGrav/ArenaFloor"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0.08, 0.09, 0.12, 1.0)
        _GridColor ("Grid Line Color", Color) = (0.16, 0.18, 0.24, 1.0)
        _GridSize ("Grid Size", Float) = 4.0
        _LineWidth ("Line Width", Range(0.01, 0.2)) = 0.04
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float2 uv           : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BackgroundColor;
                float4 _GridColor;
                float _GridSize;
                float _LineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // World space grid coordinates
                float2 coord = input.positionWS.xz / _GridSize;
                float2 grid = abs(frac(coord - 0.5) - 0.5) / fwidth(coord);
                float lineVal = min(grid.x, grid.y);
                float lineFactor = 1.0 - min(lineVal, 1.0);

                float3 color = lerp(_BackgroundColor.rgb, _GridColor.rgb, lineFactor);
                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
