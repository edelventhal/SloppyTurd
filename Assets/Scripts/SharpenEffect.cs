// Reduces all alpha and blur, sets to a solid color.

using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Sharpen Effect")]
public class SharpenEffect : ImageEffectBase
{
	public Color sourceColor;
	public Color targetColor;
    public Color secondaryTargetColor;
	public float threshold = 0.1f;
	public float alphaMultiple = 4.0f;
	public float alphaThreshold = 0.1f;
	public float darknessThreshold1 = 0.25f;
	public float darknessMultiple1 = 1.75f;
	public float darknessThreshold2 = 0.5f;
	public float darknessMultiple2 = 2.0f;
	public float darknessMinimum = 0.5f;

	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetColor( "_SourceColor", sourceColor );
#if UNITY_IPHONE
        if ( Game.GetInstance() != null && Game.GetInstance().ShouldAdvance() )
        {
            material.SetColor( "_TargetColor", targetColor );
        }
        else
        {
            material.SetColor( "_TargetColor", secondaryTargetColor );
        }
#else
        material.SetColor( "_TargetColor", targetColor );
#endif
		
		material.SetFloat( "_ExcludeAmount", threshold );
		material.SetFloat( "_AlphaMultiple", alphaMultiple );
		material.SetFloat( "_DarknessThreshold1", darknessThreshold1 );
		material.SetFloat( "_DarknessMultiple1", darknessMultiple1 );
		material.SetFloat( "_DarknessThreshold2", darknessThreshold2 );
		material.SetFloat( "_DarknessMultiple2", darknessMultiple2 );
		material.SetFloat( "_DarknessMinimum", darknessMinimum );
		material.SetFloat( "_AlphaThreshold", alphaThreshold );
		Graphics.Blit ( source, destination, material );
	}
}
