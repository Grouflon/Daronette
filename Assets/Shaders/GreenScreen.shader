Shader "Custom/GreenScreen"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    { 
        _BaseMap("Base Map", 2D) = "white" {}
        _TransparentColor("Transparent Color", Color) = (1, 1, 1, 1)
        _TransparentThreshold("Distance to transparent Color", Float) = 0.1
        _Alpha("Alpha", Float) = 1.0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
        //ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE2D(_BaseMap);
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float4 _BaseMap_ST;
                float4 _TransparentColor;
                float _TransparentThreshold;
                float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // The SAMPLE_TEXTURE2D marco samples the texture with the given
                // sampler.
                half4 baseColor = _TransparentColor;
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 distanceToColor = baseColor - color;
                float dist = sqrt(distanceToColor.x*distanceToColor.x + distanceToColor.y*distanceToColor.y + distanceToColor.z*distanceToColor.z);
                float a = _Alpha;
                if (dist < _TransparentThreshold)
                {
                    a = 0.0;
                }
                return half4(color.xyz, a);
            }
            ENDHLSL
        }
    }
}