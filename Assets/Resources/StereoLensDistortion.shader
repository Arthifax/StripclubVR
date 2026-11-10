Shader "Hidden/Custom/StereoLensDistortion"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VertDefault
            #pragma fragment Frag

            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

            TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
            float4 _LensWarp; // Angle, sigma, inverse scale, signed intensity.

            float4 Frag(VaryingsDefault i) : SV_Target
            {
                float2 offset = (i.texcoord - 0.5) * _LensWarp.z;
                float radius = max(length(offset), 1e-6);
                float sourceRadius = _LensWarp.w > 0.0
                    ? tan(radius * _LensWarp.x) / _LensWarp.y
                    : atan(radius * _LensWarp.y) / _LensWarp.x;
                float2 sourceUV = 0.5 + offset * (sourceRadius / radius);

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(sourceUV));
            }
            ENDHLSL
        }
    }
}
