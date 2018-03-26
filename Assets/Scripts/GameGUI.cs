using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour, InputListener
{
	public GUISkin skin;

	public Texture2D menuTexture;
	public Texture2D deadTexture;
    public Texture2D grandendroitTexture;
    public Texture2D creditsTexture;
	public Texture2D floaterTextureActive;
	public Texture2D floaterTextureInactive;
	public Texture2D sinkerTextureActive;
	public Texture2D sinkerTextureInactive;
	public Texture2D[] tapTextures;
	public float tapTextureSwapDelay = 0.75f;
	public float scoreYOffsetSlideTime = 0.3f;

	private GUIStyle labelStyle;
    private GUIStyle labelStyleSecondary;
	private GUIStyle labelShadowStyle;
	private GUIStyle menuStyle;
	private GUIStyle menuShadowStyle;

	private float tapTextureSwapTimeRemaining;
	private int currentTapTextureIndex;
	private Rect sinkerRect;
	private Rect floaterRect;

	private float scoreYOffset;
	private float scoreYOffsetTimeSlid;
	private float scoreYSlideDistance;
	private bool showingBanner;

#if UNITY_IPHONE
	private bool isTablet;
	private bool isRetina;
#endif

	public void Start()
	{
		labelStyle = new GUIStyle( skin.label );
		labelStyle.fontSize = (int) ( labelStyle.fontSize * ( Mathf.Min( Screen.width / 320.0f, Screen.height / 640.0f ) ) );
        labelStyleSecondary = new GUIStyle( labelStyle );
        labelStyleSecondary.normal.textColor = new Color( 0.25f, 0.25f, 0.25f, 1.0f );
		labelShadowStyle = new GUIStyle( labelStyle );
		labelShadowStyle.normal.textColor = Color.black;

		menuStyle = new GUIStyle( labelStyle );
		menuStyle.fontSize = (int) ( labelStyle.fontSize * 1.5f );
		menuShadowStyle = new GUIStyle( menuStyle );
		menuShadowStyle.normal.textColor = Color.black;

		tapTextureSwapTimeRemaining = tapTextureSwapDelay;

		sinkerRect  = new Rect( 1.0f * Screen.width / 8.0f, Screen.height - ( Screen.width / 16.0f ), Screen.width / 4.0f, Screen.width / 16.0f );
		floaterRect = new Rect( 5.0f * Screen.width / 8.0f, Screen.height - ( Screen.width / 16.0f ), Screen.width / 4.0f, Screen.width / 16.0f );
		InputManager.GetInstance().AddFirstListener( this );

		scoreYOffset = 0.0f;
		scoreYOffsetTimeSlid = 0.0f;
		scoreYSlideDistance = 0.0f;
		showingBanner = false;

#if UNITY_IPHONE
		AdMobBinding.setTestDevices( new string[] { "e7f7998e84377d0d14027ff188ec1ccecd37ad2c", "a41d4f7d0941067ac75261fe2aca24cb9d459773" } );
		AdMobBinding.init( "pub-4271033034731792", true );

		isTablet = UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad1Gen || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad2Gen
				|| UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad3Gen || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad4Gen
				|| UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadMini1Gen;

		scoreYSlideDistance = isTablet ? 90 : 50;

		//if we're retina, we need to x2 that value
		isRetina = Screen.dpi > 200;

		if ( isRetina )
		{
			scoreYSlideDistance *= 2.0f;
		}
#elif UNITY_ANDROID
        AdMobAndroid.init( "pub-4271033034731792" );
        scoreYSlideDistance = 150; //no idea why I need this...
#endif
	}

	public void OnGUI()
	{
		GUI.skin = skin;

		Vector2 shadowSize = new Vector2( -1.0f, -1.0f );

		if ( Game.GetInstance().GetState() == Game.GameState.Waiting ||
		     Game.GetInstance().GetState() == Game.GameState.Playing ||
		     Game.GetInstance().GetState() == Game.GameState.Dead )
		{
			float labelWidth = Screen.width / 2.0f;
			float labelHeight =  Screen.height / 10.0f;
			float labelDistanceY = Screen.height / 14.0f;

			if ( Screen.width > Screen.height )
			{
				labelWidth *= 0.5f;
				labelHeight *= 0.5f;
			}

            GUIStyle usedLabelStyle = labelStyle;
#if UNITY_IPHONE
            if ( !Game.GetInstance().ShouldAdvance() )
            {
                usedLabelStyle = labelStyleSecondary;
            }
#endif
			ShadowLabel( new Rect( Screen.width - ( 2.0f * labelWidth ), 0.0f + scoreYOffset, labelWidth, labelHeight ), "Score", usedLabelStyle, labelShadowStyle, shadowSize );
			ShadowLabel( new Rect( Screen.width - ( 2.0f * labelWidth ), labelDistanceY + scoreYOffset, labelWidth, labelHeight ), "" + ( Game.GetInstance().GetPoints() ), usedLabelStyle, labelShadowStyle, shadowSize );

			ShadowLabel( new Rect( Screen.width - ( 1.0f * labelWidth ), 0.0f + scoreYOffset, labelWidth, labelHeight ), "Best", usedLabelStyle, labelShadowStyle, shadowSize );
			ShadowLabel( new Rect( Screen.width - ( 1.0f * labelWidth ), labelDistanceY + scoreYOffset, labelWidth, labelHeight ), "" + ( Game.GetInstance().GetMaxPoints() ), usedLabelStyle, labelShadowStyle, shadowSize );
		}

		if ( Game.GetInstance().GetState() == Game.GameState.Menu )
		{
			/*float labelWidth = 3.0f * Screen.width / 4.0f;
			float labelHeight = Screen.height / 4.0f;
			ShadowLabel( new Rect( Screen.width / 8.0f, Screen.height / 12.0f, labelWidth, labelHeight ), "Sloppy Poo", menuStyle, menuShadowStyle, shadowSize );*/

			float minDim = Mathf.Min( Screen.width, Screen.height );
			GUI.DrawTexture( new Rect( minDim / 16.0f, minDim / 16.0f, 14.0f * minDim / 16.0f, 14.0f * minDim / 16.0f ), menuTexture );
		}
		else if ( Game.GetInstance().GetState() == Game.GameState.Waiting )
		{
			tapTextureSwapTimeRemaining -= Time.deltaTime;

			if ( tapTextureSwapTimeRemaining <= 0.0f )
			{
				tapTextureSwapTimeRemaining = tapTextureSwapDelay;

				currentTapTextureIndex++;
				if ( currentTapTextureIndex >= tapTextures.Length )
				{
					currentTapTextureIndex = 0;
				}
			}

			float minDim = Mathf.Min( Screen.width, Screen.height );
			GUI.DrawTexture( new Rect( minDim / 4.0f, minDim / 2.0f, minDim / 2.0f, minDim / 2.0f ), tapTextures[ currentTapTextureIndex ] );

			//show the floater / sinker option if they've gotten more than 10 points before
			if ( Game.GetInstance().GetMaxPoints() >= 10 )
			{
				GUI.DrawTexture( sinkerRect , Game.GetInstance().poo.IsSinker() ? sinkerTextureActive    : sinkerTextureInactive );
				GUI.DrawTexture( floaterRect, Game.GetInstance().poo.IsSinker() ? floaterTextureInactive : floaterTextureActive  );
			}
		}
		else if ( Game.GetInstance().GetState() == Game.GameState.Dead )
		{
			float minDim = Mathf.Min( Screen.width, Screen.height );
			GUI.DrawTexture( new Rect( minDim / 16.0f, minDim / 12.0f, 14.0f * minDim / 16.0f, 14.0f * minDim / 16.0f ), deadTexture );
            
            float grandWid = ( 4.0f * Screen.width / 5.0f );
            float grandHi = grandWid / 5.5f;
            GUI.DrawTexture( new Rect( ( Screen.width - grandWid ) / 2.0f, Screen.height - grandHi * 1.0f, grandWid, grandHi ), grandendroitTexture );
            
            float creditsWid = ( 4.0f * Screen.width / 5.0f );
            float creditsHi = creditsWid / 4.5f;
            GUI.DrawTexture( new Rect( ( Screen.width - creditsWid ) / 2.0f, Screen.height - grandHi * 1.0f - creditsHi * 1.0f, creditsWid, creditsHi ), creditsTexture );
		}

#if UNITY_IPHONE
		//when waiting or dead, we can show ads
		if ( Game.GetInstance().GetState() == Game.GameState.Waiting || Game.GetInstance().GetState() == Game.GameState.Dead )
		{
			if ( !showingBanner )
			{
				if( UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPad1Gen && UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPad2Gen
				   && UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPad3Gen && UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPad4Gen && UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPadMini1Gen )
				{
					AdMobBinding.createBanner( "ca-app-pub-4271033034731792/9729598146", AdMobBannerType.iPhone_320x50, AdMobAdPosition.TopCenter );
				}
				else
				{
					AdMobBinding.createBanner( "ca-app-pub-4271033034731792/7834062541", AdMobBannerType.iPad_728x90, AdMobAdPosition.TopCenter );
				}

				showingBanner = true;
				scoreYOffsetTimeSlid = 0.0f;
			}

			scoreYOffset = GetEaseInOutValue( scoreYOffsetTimeSlid, scoreYOffsetSlideTime, 0.0f, scoreYSlideDistance );
			scoreYOffsetTimeSlid += Time.deltaTime;
		}
		//otherwise, remove them
		else
		{
			if ( showingBanner )
			{
				AdMobBinding.destroyBanner();
				showingBanner = false;
				scoreYOffsetTimeSlid = 0.0f;
			}

			scoreYOffset = GetEaseInOutValue( scoreYOffsetTimeSlid, scoreYOffsetSlideTime, scoreYSlideDistance, 0.0f );
			scoreYOffsetTimeSlid += Time.deltaTime;
		}
#elif UNITY_ANDROID
		//when waiting or dead, we can show ads
		if ( Game.GetInstance().GetState() == Game.GameState.Waiting || Game.GetInstance().GetState() == Game.GameState.Dead )
		{
			if ( !showingBanner )
			{
				//if( Screen.width < 728 )
				//{
					AdMobAndroid.createBanner( "ca-app-pub-4271033034731792/9729598146", AdMobAndroidAd.phone320x50, AdMobAdPlacement.TopCenter );
                    //}
				//else
				//{
				//	AdMobAndroid.createBanner( "ca-app-pub-4271033034731792/7834062541", AdMobAndroidAd.tablet728x90, AdMobAdPlacement.TopCenter );
                    //}

				showingBanner = true;
				scoreYOffsetTimeSlid = 0.0f;
			}

			scoreYOffset = GetEaseInOutValue( scoreYOffsetTimeSlid, scoreYOffsetSlideTime, 0.0f, scoreYSlideDistance );
			scoreYOffsetTimeSlid += Time.deltaTime;
		}
		//otherwise, remove them
		else
		{
			if ( showingBanner )
			{
				AdMobAndroid.destroyBanner();
				showingBanner = false;
				scoreYOffsetTimeSlid = 0.0f;
			}

			scoreYOffset = GetEaseInOutValue( scoreYOffsetTimeSlid, scoreYOffsetSlideTime, scoreYSlideDistance, 0.0f );
			scoreYOffsetTimeSlid += Time.deltaTime;
		}
#endif
	}

	public bool Tapped()
	{
		if ( Game.GetInstance().GetState() == Game.GameState.Waiting && Game.GetInstance().GetMaxPoints() >= 10 )
		{
			Vector3 mp = Input.mousePosition;
			mp.y = Screen.height - mp.y;
			if ( sinkerRect.Contains( mp ) )
			{
				Game.GetInstance().poo.SetIsSinker( true );
				return true;
			}
			else if ( floaterRect.Contains( mp ) )
			{
				Game.GetInstance().poo.SetIsSinker( false );
				return true;
			}
		}
		return false;
	}

	private void ShadowLabel( Rect r, string text, GUIStyle normalStyle, GUIStyle shadowStyle, Vector2 shadowOffset )
	{
		for ( int shadow = 0; shadow < r.height / 8.0f; shadow++ )
		{
			Rect shadowRect = new Rect( r.x + shadowOffset.x * ( shadow - r.height / 16.0f ), r.y + shadowOffset.y * ( shadow - r.height / 16.0f ), r.width, r.height );
			GUI.Label( shadowRect, text, shadowStyle );
		}
		GUI.Label( r, text, normalStyle );
	}

	private float GetEaseInOutValue( float time, float duration, float start, float end )
	{
		if ( time >= duration )
		{
			return end;
		}
		
		float t = time / duration;
		float change = end - start;

		t = time / ( duration / 2.0f );
		if ( t < 1.0f )
		{
			//ease in
			return ( change / 2.0f ) * t * t + start;
		}
		else
		{
			//ease out
			t -= 1.0f;
			return ( -change / 2.0f ) * ( t * ( t - 2.0f ) - 1.0f ) + start;
		}
	}
}
