using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class FollowCam : MonoBehaviour
{
	public Vector2 designSize = new Vector2( 280, 500 );

	public Transform target;
	public float damping = 0.1f;
	public Vector3 componentChangeAmount = Vector3.one;
	public float shakeAmount = 0.1f;
	public float shakePeriod = 0.25f;

	public Camera mainCam;
	public Camera renderCam;
	public Renderer texTarget;
	//public Transform background;
	//public Transform vignette;

	private ParallaxBackgroundFiller[] parallaxedBackgrounds;

	private Vector3 offset;
	private Vector3 velocity;
	private RenderTexture renderTexture;
	private float shakeTimeRemaining;

	public void Awake()
	{
		Component[] bkgds = GetComponentsInChildren( typeof( ParallaxBackgroundFiller ) ) as Component[];
		parallaxedBackgrounds = new ParallaxBackgroundFiller[ bkgds.Length ];
		for ( int componentIndex = 0; componentIndex < bkgds.Length; componentIndex++ )
		{
			parallaxedBackgrounds[ componentIndex ] = bkgds[ componentIndex ] as ParallaxBackgroundFiller;
		}
	}

	public void Start()
	{
		if ( target != null )
		{
			offset = transform.position - target.transform.position;
		}

		renderTexture = new RenderTexture( Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32 );
		renderCam.targetTexture = renderTexture;
		texTarget.material.mainTexture = renderTexture;

		Vector3 maxPoint = mainCam.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
		Vector3 minPoint = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );

		Vector2 scale = new Vector2( maxPoint.x - minPoint.x, maxPoint.y - minPoint.y );

		texTarget.transform.localScale = new Vector3( scale.x, scale.y, 1.0f );
		texTarget.transform.localPosition = new Vector3( 0.0f, 0.0f, 20.0f );

		//float maxScale = Mathf.Max( scale.x, scale.y );
		//background.transform.localScale = new Vector3( maxScale, maxScale, 1.0f );
		//background.transform.localPosition = new Vector3( 0.0f, 0.0f, 50.0f );

		//vignette.transform.localScale = new Vector3( scale.x, scale.y, 1.0f );
		//vignette.transform.localPosition = new Vector3( 0.0f, 0.0f, 1.0f );

		//change the blur shader so the amount matches the design size
		float blurScale = Mathf.Min( Screen.width / designSize.x, Screen.height / designSize.y );
		Blur blur = renderCam.GetComponent( typeof( Blur ) ) as Blur;
		blur.blurSpread *= blurScale;
	}

	public void FixedUpdate()
	{
		//smooth damp to center of all players
		Vector3 targetPos = Vector3.SmoothDamp( transform.position, target.transform.position + offset, ref velocity, damping );

		Vector3 change = targetPos - transform.position;
		change = new Vector3( change.x * componentChangeAmount.x, change.y * componentChangeAmount.y, change.z * componentChangeAmount.z );

		Vector3 desiredPos = transform.position + change;

		if ( shakeTimeRemaining > 0.0f )
		{
			desiredPos.x += Mathf.Sin( Time.time / shakePeriod ) * shakeAmount;
			shakeTimeRemaining -= Time.fixedDeltaTime;
		}
		else
		{
			//add parallax to the backgrounds
			for ( int parallaxIndex = 0; parallaxIndex < parallaxedBackgrounds.Length; parallaxIndex++ )
			{
				parallaxedBackgrounds[ parallaxIndex ].MoveBackground( change.x );
			}
		}

		transform.position = desiredPos;
	}

	public void ShakeCamera( float time )
	{
		shakeTimeRemaining = time;
	}
}
