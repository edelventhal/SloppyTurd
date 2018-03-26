using UnityEngine;
using System.Collections;

public class ParallaxBackgroundFiller : BackgroundFiller
{
	public float parallaxAmount = 1.0f;

	//move the background, must be called whenever the camera moves
	public void MoveBackground( float xChange )
	{
        //the xChange should be scaled by the size of the filler
        xChange /= filler.transform.localScale.x;
        
		filler.GetComponent<Renderer>().material.mainTextureOffset += new Vector2( xChange * parallaxAmount, 0.0f );
	}
}
