using UnityEngine;
using System.Collections;

public class PooBall : MonoBehaviour
{
	public bool canStick = true;
	public bool canJoin = false;

	public GameObject pooPrefab;
	public float destroyDistance = 50;
    
    public AudioClip[] stuckSounds;
    public AudioClip[] stuckSoundsSecondary;

	public SpriteRenderer embeddedSprite;
	public Sprite[] embeddedSpriteChoices;
    public Sprite[] embeddedSpriteChoicesSecondary;
	public float embedChance = 0.1f;

	private bool stuck;
	private ArrayList childPooBalls;

	private static ArrayList chosenEmbeds;

	public void Awake()
	{
		if ( chosenEmbeds == null )
		{
			chosenEmbeds = new ArrayList();
			ResetChosenEmbeds();
		}

		childPooBalls = new ArrayList();
	}

	private void ResetChosenEmbeds()
	{
		chosenEmbeds.Clear();
		for ( int i = 0; i < embeddedSpriteChoices.Length; i++ )
		{
			chosenEmbeds.Add( i );
		}
	}

	public void Start()
	{
		stuck = false;

		if ( embeddedSprite != null )
		{
			if ( Random.Range( 0.0f, 1.0f ) <= embedChance )
			{
				if ( chosenEmbeds.Count <= 0 )
				{
					ResetChosenEmbeds();
				}

				int randomEmbedIndex = Random.Range( 0, chosenEmbeds.Count );
				int embedIndex = (int) chosenEmbeds[ randomEmbedIndex ];
#if UNITY_IPHONE
                if ( Game.GetInstance().ShouldAdvance() )
                {
                    embeddedSprite.sprite = embeddedSpriteChoices[ embedIndex ];
                }
                else
                {
                    embeddedSprite.sprite = embeddedSpriteChoicesSecondary[ embedIndex ];
                }
#else
				embeddedSprite.sprite = embeddedSpriteChoices[ embedIndex ];
#endif
				chosenEmbeds.RemoveAt( randomEmbedIndex );
			}
			else
			{
				Object.Destroy( embeddedSprite );
			}
		}
	}
	
	public void Update()
	{
		if ( ( transform.position - Game.GetInstance().poo.pooBallCenter.transform.position ).sqrMagnitude > destroyDistance * destroyDistance )
		{
			Game.GetInstance().poo.PooBallWasDestroyed( gameObject );

			Object.Destroy( gameObject );
		}
	}

	public void OnTriggerEnter2D( Collider2D c )
	{
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

	public void Detach( Transform newParent, bool force )
	{
		if ( !force && ( stuck || !canStick ) )
		{
			return;
		}

		DistanceJoint2D joint = GetComponent( typeof( DistanceJoint2D ) ) as DistanceJoint2D;
		
		if ( transform.parent != null )
		{
			Poo poo = transform.parent.GetComponent( typeof( Poo ) ) as Poo;
			if ( poo != null )
			{
				if ( !poo.CanDetachPooBall( gameObject ) )
				{
					return;
				}

				poo.DetachPooBall( gameObject );
			}
		}

		Object.Destroy( joint );
		transform.parent = null;//newParent;
		if ( newParent != null )
		{
			// GetComponent<Rigidbody2D>().isKinematic = true;
			// GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			// GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
			GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
			stuck = true;
		}

		stuck = true;
		GetComponent<Rigidbody2D>().gravityScale = 1.0f;
        
        if ( GetComponent<AudioSource>() != null && stuckSounds.Length > 0 )
        {
#if UNITY_IPHONE
            if ( Game.GetInstance().ShouldAdvance() )
            {
                GetComponent<AudioSource>().clip = stuckSounds[ Random.Range( 0, stuckSounds.Length ) ];
            }
            else
            {
                GetComponent<AudioSource>().clip = stuckSoundsSecondary[ Random.Range( 0, stuckSoundsSecondary.Length ) ];
            }
#else
            audio.clip = stuckSounds[ Random.Range( 0, stuckSounds.Length ) ];
#endif
            GetComponent<AudioSource>().pitch = Random.Range( 0.8f, 1.2f );
            GetComponent<AudioSource>().Play();
        }
	}

	public void Attach( Transform target, bool force )
	{
		if ( !force && ( stuck || !canJoin ) )
		{
			return;
		}

		Poo poo = null;

		//see if the newParent is connected to the poo. If not, don't bother
		DistanceJoint2D targetJoint = target.GetComponent( typeof( DistanceJoint2D ) ) as DistanceJoint2D;

		//hit by a different piece of poo that's connected to the poo ball center?
		if ( targetJoint != null )
		{
			if ( targetJoint.connectedBody != null && targetJoint.connectedBody.transform.parent != null )
			{
				poo = targetJoint.connectedBody.transform.parent.GetComponent( typeof( Poo ) ) as Poo;
			}
		}
		//the thing we hit has no joint, so see if its parent is the poo (it's the poo ball center)
		else
		{
			poo = target.GetComponent( typeof( Poo ) ) as Poo;
		}

		//if we got a poo from that, then we can attach to it
		if ( poo != null )
		{
			DistanceJoint2D joint = GetComponent( typeof( DistanceJoint2D ) ) as DistanceJoint2D;
			if ( joint != null )
			{
				poo.AttachPooBall( this.gameObject );
				joint.enabled = true;
				joint.connectedBody = poo.pooBallCenter.GetComponent<Rigidbody2D>();
				transform.parent = poo.transform;
				canJoin = false;
				canStick = true;
				stuck = false;
			}
		}
	}

	private void DoCollision( Collider2D c )
	{
		//stick to the walls
		if ( c.gameObject.layer == Game.WALL_LAYER )
		{
			Detach( c.transform, false );
		}
		//just fall onto the floor
		else if ( c.gameObject.layer == Game.GROUND_LAYER )
		{
			Detach( null, false );
		}
		//connect to the player
		else if ( c.gameObject.layer == Game.LIQUID_LAYER )
		{
			Attach( c.transform, false );
		}
	}

	public void CreateChildPooBalls( int count )
	{
		Debug.Log( "Destroyinh child poo " );
		for ( int pooBallIndex = 0; pooBallIndex < childPooBalls.Count; pooBallIndex++ )
		{
			Object.Destroy( childPooBalls[ pooBallIndex ] as GameObject );
		}
		
		childPooBalls.Clear();

		for ( int pooBallIndex = 0; pooBallIndex < count; pooBallIndex++ )
		{
			CreatePooBall( GetComponent<Rigidbody2D>() );
		}
	}

	private GameObject CreatePooBall( Rigidbody2D link )
	{
		GameObject poo = Object.Instantiate( pooPrefab ) as GameObject;
		//poo.transform.parent = transform;
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
		
		childPooBalls.Add( poo );
		
		return poo;
	}
}
