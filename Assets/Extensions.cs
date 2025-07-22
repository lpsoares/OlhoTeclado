// Extension that adds a method to Material to set it to render transparency
using UnityEngine;

public static class MaterialExtensions
{
    // Extension method to set the material to render with transparency
    public static void SetTransparent(this Material material, int renderQueue = -1)
    {
        if (renderQueue >= 0)
        {
            material.renderQueue = renderQueue;
        }
        else
        {
            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
        }
        
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    }
}