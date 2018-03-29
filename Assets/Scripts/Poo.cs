using UnityEngine;
using System.Collections;

public class Poo : MonoBehaviour, InputListener
{
	public float xSpeed;
	public float pulseAmount = 1.0f;
	public float startHoverAmplitude = 1.0f;
	public float startHoverPeriod = 0.5f;
	public GameObject pooBallCenter;
	public int pooBallCount = 50;
	public GameObject pooPrefab;
	public float pooDisconnectDistance = 1.25f;
	public float pooLossDelay = 0.75f;
	public int minPooBallCount = 3;
	public SpriteRenderer eyeRenderer;
	public Sprite eyeSpriteAlive;
	public Sprite eyeSpriteDead;
	public Sprite eyeSpriteFloaterAlive;
	public Sprite eyeSpriteFloaterDead;
	public float minimumDeadTime = 0.5f;

	public AudioClip[] floatSounds;
    public AudioClip[] floatSoundsSecondary;
	public AudioClip[] hitSounds;
    public AudioClip[] hitSoundsSecondary;
	public AudioClip[] goalSounds;

	private Vector3 startPosition;
	private float timeUntilPooLoss;
	private float timeDead;

	private ArrayList hitWalls;
	private ArrayList pooBalls;
	private ArrayList attachedPooBalls;

	public void Awake()
	{
		hitWalls = new ArrayList();
		pooBalls = new ArrayList();
		attachedPooBalls = new ArrayList();
		startPosition = transform.position;
		
		InputManager.GetInstance().AddListener( this );
	}

	public void Start()
	{
		Reset();
	}

	public void Reset()
	{
		hitWalls.Clear();

		transform.position = startPosition;
		timeUntilPooLoss = pooLossDelay;
		timeDead = 0.0f;

		GetComponent<Rigidbody2D>().velocity = new Vector2( xSpeed, 0.0f );

		CreatePooBalls();
	}

	public void OnTriggerEnter2D( Collider2D c )
	{
		if ( c.gameObject.layer == Game.GOAL_LAYER )
		{
			if ( c.GetComponent<AudioSource>() != null && goalSounds.Length > 0 )
			{
				c.GetComponent<AudioSource>().clip = goalSounds[ Game.GetInstance().GetPoints() % goalSounds.Length ];
				c.GetComponent<AudioSource>().Play();
			}

			Game.GetInstance().PooPassedGoal();

			return;
		}

		DoCollision( c );
	}

	public void OnTriggerStay2D( Collider2D c )
	{
		DoCollision( c );
	}

	public void OnCollisionEnter2D( Collision2D c )
	{
		DoCollision( c.collider );
	}

	public void OnCollisionStay2D( Collision2D c )
	{
		DoCollision( c.collider );
	}

	private void DoCollision( Collider2D c )
	{
		if ( c.gameObject.layer != Game.WALL_LAYER && c.gameObject.layer != Game.GROUND_LAYER )
		{
			return;
		}
		
		if ( c.transform.parent != null && c.transform.parent.GetComponent( typeof( Wall ) ) as Wall != null )
		{
			HitWall( c.transform.parent.gameObject );
		}
		else
		{
			HitWall( c.gameObject );
		}
	}

	public void FixedUpdate()
	{
		Game.GameState state = Game.GetInstance().GetState();
		if ( state == Game.GameState.Dead )
		{
			UpdateDead();
		}
		else if ( state == Game.GameState.Waiting || state == Game.GameState.Menu )
		{
			UpdateWaiting();
		}
 		else if ( state == Game.GameState.Playing )
		{
			UpdatePlaying();
		}
	}

	public bool HasBeenDeadLongEnoughForReset()
	{
		return ( timeDead >= minimumDeadTime );
	}

	public bool Tapped()
	{
		Game.GameState state = Game.GetInstance().GetState();
		if ( state == Game.GameState.Dead )
		{
			// if ( HasBeenDeadLongEnoughForReset() )
			// {
			// 	Game.GetInstance().Reset();
			// }
		}
		else if ( state == Game.GameState.Menu )
		{
			//Game.GetInstance().SetState( Game.GameState.Waiting );
		}
		else if ( /*state == Game.GameState.Waiting ||*/ state == Game.GameState.Playing )
		{
			//Game.GetInstance().SetState( Game.GameState.Playing );

			Vector2 vel = new Vector2( xSpeed, pulseAmount );
			if ( pulseAmount > 0 )
			{
				vel.y = Mathf.Clamp( vel.y, -2.0f * pulseAmount, pulseAmount );
			}
			else
			{
				vel.y = Mathf.Clamp( vel.y, pulseAmount, -2.0f * pulseAmount );
			}
			GetComponent<Rigidbody2D>().velocity = vel;

			if ( GetComponent<AudioSource>() != null && floatSounds.Length > 0 )
			{
#if UNITY_IPHONE
                if ( Game.GetInstance().ShouldAdvance() )
                {
                    GetComponent<AudioSource>().clip = floatSounds[ Random.Range( 0, floatSounds.Length ) ];
                }
                else
                {
                    GetComponent<AudioSource>().clip = floatSoundsSecondary[ Random.Range( 0, floatSoundsSecondary.Length ) ];
                }
#else
                audio.clip = floatSounds[ Random.Range( 0, floatSounds.Length ) ];
#endif
                GetComponent<AudioSource>().pitch = Random.Range( 0.8f, 1.2f );
                GetComponent<AudioSource>().Play();
			}
		}

		return false;
	}

	public void SetIsDead( bool dead )
	{
		if ( dead && Game.GetInstance().GetState() != Game.GameState.Dead )
		{
			for ( int pooBallIndex = 0; pooBallIndex < pooBalls.Count; pooBallIndex++ )
			{
				GameObject poo = pooBalls[ pooBallIndex ] as GameObject;
				DistanceJoint2D joint = poo.GetComponent( typeof( DistanceJoint2D ) ) as DistanceJoint2D;
				if ( joint != null )
				{
					if ( Random.Range( 0, 10 ) <= 7 )
					{
						Object.Destroy( joint );
						attachedPooBalls.Remove( poo );
					}
					else
					{
						//joint.connectedBody = null;
						//joint.connectedBody = ( pooBalls[ Random.Range( 1, pooBalls.Count ) ] as GameObject ).rigidbody2D;
						joint.distance *= Random.Range( 1.5f, 2.0f );
					}

					poo.transform.parent = null;
					poo.GetComponent<Rigidbody2D>().gravityScale = 1.0f;
					( poo.GetComponent( typeof( PooBall ) ) as PooBall ).canStick = false;
				}
			}
			pooBallCenter.GetComponent<Rigidbody2D>().gravityScale = 1.0f;

			timeDead = 0.0f;
			Game.GetInstance().SetState( Game.GameState.Dead );
		}
	}

	public int GetAttachedBallCount()
	{
		return attachedPooBalls.Count;
	}

	public void DetachPooBall( GameObject pooBall )
	{
		attachedPooBalls.Remove( pooBall );
	}

	public bool CanDetachPooBall( GameObject pooBall )
	{
		if ( attachedPooBalls.Count > minPooBallCount )
		{
			return true;
		}

		return !attachedPooBalls.Contains( pooBall );
	}

	public void AttachPooBall( GameObject pooBall )
	{
		if ( !pooBalls.Contains( pooBall ) )
		{
			pooBalls.Add( pooBall );
			attachedPooBalls.Add( pooBall );
		}
	}

	public void PooBallWasDestroyed( GameObject pooBall )
	{
		pooBalls.Remove( pooBall );
		attachedPooBalls.Remove( pooBall );
	}

	public void SetIsSinker( bool sinker )
	{
		float gravityAbs = Mathf.Abs( GetComponent<Rigidbody2D>().gravityScale );
		float pulseAbs = Mathf.Abs( pulseAmount );

		if ( sinker )
		{
			GetComponent<Rigidbody2D>().gravityScale = gravityAbs;
			pulseAmount = pulseAbs;
		}
		else
		{
			GetComponent<Rigidbody2D>().gravityScale = gravityAbs * -1.0f;
			pulseAmount = pulseAbs * -1.0f;
		}
	}

	public bool IsSinker()
	{
		return pulseAmount > 0;
	}

	private void UpdateWaiting()
	{
		GetComponent<Rigidbody2D>().velocity = new Vector2( xSpeed, 0.0f );
		transform.position = startPosition + new Vector3( 0.0f, Mathf.Sin( Time.time / startHoverPeriod ) * startHoverAmplitude, 0.0f );

		pooBallCenter.transform.localPosition = Vector3.zero;
		pooBallCenter.GetComponent<Rigidbody2D>().isKinematic = true;

        eyeRenderer.sprite = pulseAmount > 0 ? eyeSpriteAlive : eyeSpriteFloaterAlive;
#if UNITY_IPHONE
        if ( !Game.GetInstance().ShouldAdvance() )
        {
            eyeRenderer.sprite = eyeSpriteFloaterAlive;
        }
#endif
        
		GetComponent<Rigidbody2D>().isKinematic = false;
	}

	private void UpdatePlaying()
	{
		Vector2 vel = new Vector2( xSpeed, GetComponent<Rigidbody2D>().velocity.y );
		if ( pulseAmount > 0 )
		{
			vel.y = Mathf.Clamp( vel.y, -2.0f * pulseAmount, pulseAmount );
		}
		else
		{
			vel.y = Mathf.Clamp( vel.y, pulseAmount, -2.0f * pulseAmount );
		}
		GetComponent<Rigidbody2D>().velocity = vel;

		pooBallCenter.transform.localPosition = Vector3.zero;
		pooBallCenter.GetComponent<Rigidbody2D>().isKinematic = true;
		Vector3 rot = pooBallCenter.transform.rotation.eulerAngles;
		rot.z = Mathf.Atan2( GetComponent<Rigidbody2D>().velocity.y, GetComponent<Rigidbody2D>().velocity.x ) * Mathf.Rad2Deg - 90.0f;
		pooBallCenter.transform.rotation = Quaternion.Euler( rot );

		for ( int pooBallIndex = 0; pooBallIndex < attachedPooBalls.Count; pooBallIndex++ )
		{
			GameObject pooBall = attachedPooBalls[ pooBallIndex ] as GameObject;
			if ( ( pooBall.transform.position - pooBallCenter.transform.position ).sqrMagnitude > pooDisconnectDistance * pooDisconnectDistance )
			{
				( pooBall.GetComponent( typeof( PooBall ) ) as PooBall ).Detach( null, false );
			}
		}

		eyeRenderer.sprite = pulseAmount > 0 ? eyeSpriteAlive : eyeSpriteFloaterAlive;
#if UNITY_IPHONE
        if ( !Game.GetInstance().ShouldAdvance() )
        {
            eyeRenderer.sprite = eyeSpriteFloaterAlive;
        }
#endif
		GetComponent<Rigidbody2D>().isKinematic = false;

		timeUntilPooLoss -= Time.deltaTime;
		if ( timeUntilPooLoss <= 0.0f && attachedPooBalls.Count > minPooBallCount )
		{
			GameObject pooBall = attachedPooBalls[ Random.Range( 0, attachedPooBalls.Count ) ] as GameObject;
			attachedPooBalls.Remove( pooBall );
			( pooBall.GetComponent( typeof( PooBall ) ) as PooBall ).Detach( null, true );
			timeUntilPooLoss = pooLossDelay;
		}
	}

	private void UpdateDead()
	{
		GetComponent<Rigidbody2D>().velocity = new Vector2( 0.0f, GetComponent<Rigidbody2D>().velocity.y );
		pooBallCenter.GetComponent<Rigidbody2D>().isKinematic = false;

		eyeRenderer.sprite = pulseAmount > 0 ? eyeSpriteDead : eyeSpriteFloaterDead;
#if UNITY_IPHONE
        if ( !Game.GetInstance().ShouldAdvance() )
        {
            eyeRenderer.sprite = eyeSpriteFloaterDead;
        }
#endif
		GetComponent<Rigidbody2D>().isKinematic = true;

		timeDead += Time.fixedDeltaTime;
	}

	private void HitWall( GameObject w )
	{
		if ( !hitWalls.Contains( w ) )
		{
			hitWalls.Add( w );

			if ( GetComponent<AudioSource>() != null && hitSounds.Length > 0 )
			{
#if UNITY_IPHONE
                if ( Game.GetInstance().ShouldAdvance() )
                {
                    GetComponent<AudioSource>().clip = hitSounds[ Random.Range( 0, hitSounds.Length ) ];
                }
                else
                {
                    GetComponent<AudioSource>().clip = hitSoundsSecondary[ Random.Range( 0, hitSoundsSecondary.Length ) ];
                }
#else
				audio.clip = hitSounds[ Random.Range( 0, hitSounds.Length ) ];
#endif
                GetComponent<AudioSource>().pitch = Random.Range( 0.8f, 1.2f );
				GetComponent<AudioSource>().Play();
			}

			Game.GetInstance().DoPooHit( hitWalls.Count );
		}
	}

	private void CreatePooBalls()
	{
		for ( int pooBallIndex = 0; pooBallIndex < pooBalls.Count; pooBallIndex++ )
		{
			Object.Destroy( pooBalls[ pooBallIndex ] as GameObject );
		}

		pooBalls.Clear();
		attachedPooBalls.Clear();
		pooBallCenter.transform.localPosition = Vector3.zero;

		for ( int pooBallIndex = 0; pooBallIndex < pooBallCount; pooBallIndex++ )
		{
			CreatePooBall( pooBallCenter.GetComponent<Rigidbody2D>() );
		}
	}

	private GameObject CreatePooBall( Rigidbody2D link )
	{
		GameObject poo = Object.Instantiate( pooPrefab ) as GameObject;
		poo.transform.parent = transform;
		poo.transform.localScale *= Random.Range( 0.75f, 1.0f );

		DistanceJoint2D joint = poo.GetComponent( typeof( DistanceJoint2D ) ) as DistanceJoint2D;
		//joint.connectedBody = ( pooBalls[ Random.Range( 0, pooBalls.Count ) ] as GameObject ).rigidbody2D;
		joint.connectedBody = link;
		joint.distance *= Random.Range( 0.75f, 1.5f );

		poo.GetComponent<Rigidbody2D>().gravityScale = Random.Range( -1.0f, 1.0f );
		
		Vector3 posDiff = new Vector3( Random.Range( -poo.transform.localScale.x, poo.transform.localScale.x ), Random.Range( -poo.transform.localScale.y, poo.transform.localScale.y ), 0.0f );
		if ( link != null )
		{
			poo.transform.localPosition = link.transform.localPosition + posDiff;
		}
		else
		{
			poo.transform.localPosition = Vector3.zero;
		}
		
		pooBalls.Add( poo );
		attachedPooBalls.Add ( poo );

		return poo;
	}
}