using UnityEngine;
using System.Collections;

public class ParallaxBackgroundSprite : BackgroundFiller
{
	public float parallaxAmount = 1.0f;
    
    public SpriteRenderer secondSpriteRenderer;
    
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer activeSpriteRenderer;
    private SpriteRenderer inactiveSpriteRenderer;
	
	public override void Start()
	{
		base.Start();

        spriteRenderer = filler.GetComponent( typeof( SpriteRenderer ) ) as SpriteRenderer;

		ScaleFiller( secondSpriteRenderer.gameObject );

		activeSpriteRenderer = spriteRenderer;
		inactiveSpriteRenderer = secondSpriteRenderer;

		Vector3 activePos = activeSpriteRenderer.transform.position;
		activePos.x = 0.0f;
		activeSpriteRenderer.transform.position = activePos;

		Vector3 inactivePos = inactiveSpriteRenderer.transform.position;
		inactivePos.x = activePos.x + GetSpriteSize( activeSpriteRenderer ).x;
		inactiveSpriteRenderer.transform.position = inactivePos;
	}

	//move the background, must be called whenever the camera moves
	//the totalXChange should be how far from its original point the camera is.
	//an offset will be calculated based upon that
	public void MoveBackgroundLocal( float xChange )
	{
		Vector3 maxCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
		Vector3 minCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
		
		float parallaxedChange = -xChange * parallaxAmount;
		activeSpriteRenderer.transform.position += new Vector3( parallaxedChange, 0.0f, 0.0f );
		
		//store these off for reference
		Vector2 spriteSize = GetSpriteSize( activeSpriteRenderer );

		//the active renderer is offscreen towards the right, so the inactive must be put left
		if ( activeSpriteRenderer.transform.position.x - spriteSize.x / 2.0f > minCameraPoint.x )
		{
			inactiveSpriteRenderer.transform.position = activeSpriteRenderer.transform.position - new Vector3( spriteSize.x, 0.0f, 0.0f );
		}
		//the active renderer is offscreen towards the left, so the inactive must be put right
		else if ( activeSpriteRenderer.transform.position.x + spriteSize.x / 2.0f < maxCameraPoint.x )
		{
			inactiveSpriteRenderer.transform.position = activeSpriteRenderer.transform.position + new Vector3( spriteSize.x, 0.0f, 0.0f );
		}

		//if the active is completely offscreen to either side, swap the active with the inactive
		if ( activeSpriteRenderer.transform.position.x - spriteSize.x / 2.0f > maxCameraPoint.x ||
		     activeSpriteRenderer.transform.position.x + spriteSize.x / 2.0f < minCameraPoint.x )
		{
			SpriteRenderer temp = inactiveSpriteRenderer;
			inactiveSpriteRenderer = activeSpriteRenderer;
			activeSpriteRenderer = temp;
		}
	}

	//move the background, must be called whenever the camera moves
	//the totalXChange should be how far from its original point the camera is.
	//an offset will be calculated based upon that
	public void MoveBackgroundLocalOld( float xChange )
	{
		Vector3 maxCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
		Vector3 minCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
		
		float parallaxedChange = -xChange * ( 1.0f - parallaxAmount );
		activeSpriteRenderer.transform.localPosition += new Vector3( parallaxedChange, 0.0f, 0.0f );
		inactiveSpriteRenderer.transform.localPosition += new Vector3( parallaxedChange, 0.0f, 0.0f );
		
		//store these off for reference
		Vector2 activeSpriteSize = GetSpriteSize( activeSpriteRenderer );
		Vector2 inactiveSpriteSize = GetSpriteSize( inactiveSpriteRenderer );
		
		//we may need to swap the active renderer if it has reached an end
		bool swapActiveRenderers = false;
		
		//did we drift off too far to the left?
		if ( parallaxedChange < 0 )
		{
			//move the inactive to the right side of the active
			inactiveSpriteRenderer.transform.position = activeSpriteRenderer.transform.position + new Vector3( activeSpriteSize.x, 0.0f, 0.0f );
			
			//if the active sprite has passed the left side of the screen, we want to move it over to the right and swap actives
			if ( ( activeSpriteRenderer.transform.position.x + activeSpriteSize.x / 2.0f ) < minCameraPoint.x )
			{
				//okay, move this to the end, and make the next one active.
				activeSpriteRenderer.transform.position = inactiveSpriteRenderer.transform.position + new Vector3( inactiveSpriteSize.x, 0.0f, 0.0f );
				
				swapActiveRenderers = true;
			}
		}
		//did we drift off too far to the right?
		else if ( parallaxedChange > 0 )
		{
			//move the inactive to the left side of the active
			inactiveSpriteRenderer.transform.position = activeSpriteRenderer.transform.position - new Vector3( activeSpriteSize.x, 0.0f, 0.0f );
			
			if ( ( activeSpriteRenderer.transform.position.x - activeSpriteSize.x / 2.0f ) > maxCameraPoint.x )
			{
				//okay, move this to the end, and make the next one active.
				activeSpriteRenderer.transform.position = inactiveSpriteRenderer.transform.position - new Vector3( inactiveSpriteSize.x, 0.0f, 0.0f );
				
				swapActiveRenderers = true;
			}
		}
		
		//since the active renderer has gone offscreen, swap to the other one
		if ( swapActiveRenderers )
		{
			SpriteRenderer temp = inactiveSpriteRenderer;
			inactiveSpriteRenderer = activeSpriteRenderer;
			activeSpriteRenderer = temp;
		}
	}

	//move the background, must be called whenever the camera moves
	//the totalXChange should be how far from its original point the camera is.
	//an offset will be calculated based upon that
	public void MoveBackground( float xChange )
	{
		Vector3 maxCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
		Vector3 minCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );

		float parallaxedChange = xChange * parallaxAmount;
		activeSpriteRenderer.transform.position += new Vector3( parallaxedChange, 0.0f, 0.0f );
		inactiveSpriteRenderer.transform.position += new Vector3( parallaxedChange, 0.0f, 0.0f );

		//store these off for reference
		Vector2 activeSpriteSize = GetSpriteSize( activeSpriteRenderer );
		Vector2 inactiveSpriteSize = GetSpriteSize( inactiveSpriteRenderer );

		//we may need to swap the active renderer if it has reached an end
		bool swapActiveRenderers = false;

		//did we drift off too far to the left?
		if ( parallaxedChange < 0 )
		{
			//move the inactive to the right side of the active
			inactiveSpriteRenderer.transform.position = activeSpriteRenderer.transform.position + new Vector3( activeSpriteSize.x, 0.0f, 0.0f );

			//if the active sprite has passed the left side of the screen, we want to move it over to the right and swap actives
			if ( ( activeSpriteRenderer.transform.position.x + activeSpriteSize.x / 2.0f ) < minCameraPoint.x )
			{
				//okay, move this to the end, and make the next one active.
				activeSpriteRenderer.transform.position = inactiveSpriteRenderer.transform.position + new Vector3( inactiveSpriteSize.x, 0.0f, 0.0f );

				swapActiveRenderers = true;
			}
		}
		//did we drift off too far to the right?
		else if ( parallaxedChange > 0 )
		{
			//move the inactive to the left side of the active
			inactiveSpriteRenderer.transform.position = activeSpriteRenderer.transform.position - new Vector3( activeSpriteSize.x, 0.0f, 0.0f );

			if ( ( activeSpriteRenderer.transform.position.x - activeSpriteSize.x / 2.0f ) > maxCameraPoint.x )
			{
				Debug.Log("swap");
				//okay, move this to the end, and make the next one active.
				activeSpriteRenderer.transform.position = inactiveSpriteRenderer.transform.position - new Vector3( inactiveSpriteSize.x, 0.0f, 0.0f );
				
				swapActiveRenderers = true;
			}
		}

		//since the active renderer has gone offscreen, swap to the other one
		if ( swapActiveRenderers )
		{
			SpriteRenderer temp = inactiveSpriteRenderer;
			inactiveSpriteRenderer = activeSpriteRenderer;
			activeSpriteRenderer = temp;
		}
	}

	public Vector2 GetSpriteSize( SpriteRenderer rend )
	{
		Sprite sprite = rend.sprite;
		return new Vector2( sprite.bounds.size.x * rend.transform.localScale.x, sprite.bounds.size.y * rend.transform.localScale.y );
	}
}
