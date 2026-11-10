using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(
    typeof(StereoLensDistortionRenderer),
    PostProcessEvent.AfterStack,
    "Custom/Stereo Lens Distortion")]
public sealed class StereoLensDistortion : PostProcessEffectSettings
{
    [Range(-100f, 100f)]
    public FloatParameter intensity = new FloatParameter { value = 0f };

    [Range(0.01f, 5f)]
    public FloatParameter scale = new FloatParameter { value = 1f };

    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value && !Mathf.Approximately(intensity.value, 0f);
    }
}

public sealed class StereoLensDistortionRenderer
    : PostProcessEffectRenderer<StereoLensDistortion>
{
    private static readonly int LensWarpId = Shader.PropertyToID("_LensWarp");
    private const string ShaderName = "Hidden/Custom/StereoLensDistortion";
    private Shader shader;
    private bool reportedShaderError;

    public override void Render(PostProcessRenderContext context)
    {
        if (shader == null && !reportedShaderError)
            shader = Shader.Find(ShaderName);

        if (shader == null || !shader.isSupported)
        {
            if (!reportedShaderError)
            {
                string reason = shader == null
                    ? $"could not find shader '{ShaderName}'"
                    : $"shader '{ShaderName}' is unsupported on this device";
                Debug.LogError(
                    $"Stereo Lens Distortion {reason}. Check that " +
                    "Assets/Resources/StereoLensDistortion.shader is imported " +
                    "and review the Console for shader compilation errors.",
                    context.camera);
                reportedShaderError = true;
            }

            context.command.BlitFullscreenTriangle(context.source, context.destination);
            return;
        }

        float intensity = settings.intensity.value;
        float angle = Mathf.Clamp(Mathf.Abs(intensity), 1f, 100f) * 1.6f * Mathf.Deg2Rad;
        float sigma = 2f * Mathf.Tan(angle * 0.5f);
        float inverseScale = 1f / Mathf.Clamp(settings.scale.value, 0.01f, 5f);

        var sheet = context.propertySheets.Get(shader);
        sheet.properties.SetVector(LensWarpId, new Vector4(angle, sigma, inverseScale, intensity));
        context.command.BlitFullscreenTriangle(
            context.source, context.destination, sheet, 0);
    }
}
