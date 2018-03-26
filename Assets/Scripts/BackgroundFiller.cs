using UnityEngine;
using System.Collections;

public class BackgroundFiller : MonoBehaviour
{
	public bool maintainsAspect = true;
	public string aspectType = "max";
	public GameObject filler;

	protected Vector2 targetSize;

	public virtual void Start()
	{
		//this is always a normal scale, since its sprites will be scaled instead
		transform.localScale = Vector3.one;

		//use the camera viewport to decide how big we need to be
		Vector3 maxCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
		Vector3 minCameraPoint = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
		
		targetSize = new Vector2( maxCameraPoint.x - minCameraPoint.x, maxCameraPoint.y - minCameraPoint.y );

		targetSize = MaintainAspect( targetSize );

		if ( filler != null )
		{
			ScaleFiller( filler );
		}
	}
	
	protected virtual void ScaleFiller( GameObject filler )
	{
		Vector2 fillerSize = new Vector2( 1.0f, 1.0f );

		SpriteRenderer spriteRenderer = filler.GetComponent( typeof ( SpriteRenderer ) ) as SpriteRenderer;
		if ( spriteRenderer != null )
		{
			Sprite sprite = spriteRenderer.sprite;
			fillerSize = new Vector2( sprite.bounds.size.x, sprite.bounds.size.y );
		}

		Vector3 scale = new Vector3( targetSize.x / fillerSize.x, targetSize.y / fillerSize.y );
		scale = MaintainAspect( scale );

		filler.transform.localScale = scale;
	}

	protected virtual Vector2 MaintainAspect( Vector2 size )
	{
		if ( maintainsAspect )
		{
			float setValue = Mathf.Max( size.x, size.y );
			if ( aspectType.Equals( "min" ) )
			{
				setValue = Mathf.Min( size.x, size.y );
			}
			else if ( aspectType.Equals( "x" ) )
			{
				setValue = targetSize.x;
			}
			else if ( aspectType.Equals( "y" ) )
			{
				setValue = targetSize.y;
			}
			size.x = setValue;
			size.y = setValue;
		}

		return size;
	}
}
