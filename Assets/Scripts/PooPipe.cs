using UnityEngine;
using System.Collections;

public class PooPipe : MonoBehaviour
{
	public int maxPooCount = 10;

	public float maxBurstDistanceBefore = 1.0f;
	public float maxBurstDistanceAfter = 0.5f;

	public float minBurstDelay = 1.0f;
	public float maxBurstDelay = 2.0f;

	public int minPooPerBurst = 2;
	public int maxPooPerBurst = 4;

	public Vector2 minBurstVelocity = new Vector2( -0.5f, 1.0f );
	public Vector2 maxBurstVelocity = new Vector2( 0.5f, 3.0f );
	
	public float minPooGravity = 1.0f;
	public float maxPooGravity = 1.0f;

	public GameObject pooPrefab;

	public AudioClip[] burstSounds;
    public AudioClip[] burstSoundsSecondary;

	private float timeUntilBurst;
	private int totalPooBursted;

	public void Start()
	{
		totalPooBursted = 0;
	}

	public void Update()
	{
		if ( timeUntilBurst <= 0 && totalPooBursted < maxPooCount )
		{
			float pooX = Game.GetInstance().poo.transform.position.x;
			if ( ( transform.position.x > pooX && transform.position.x - pooX <= maxBurstDistanceBefore ) ||
			     ( transform.position.x < pooX && pooX - transform.position.x <= maxBurstDistanceAfter  ) )
			{
				timeUntilBurst = Random.Range( minBurstDelay, maxBurstDelay );

				BurstPoo();
			}
		}
		else
		{
			timeUntilBurst -= Time.deltaTime;
		}
	}

	private void BurstPoo()
	{
		int count = Random.Range( minPooPerBurst, maxPooPerBurst );

		if ( count > 0 && GetComponent<AudioSource>() != null && burstSounds.Length > 0 )
		{
#if UNITY_IPHONE
            if ( Game.GetInstance().ShouldAdvance() )
            {
                GetComponent<AudioSource>().clip = burstSounds[ Random.Range( 0, burstSounds.Length ) ];
            }
            else
            {
                GetComponent<AudioSource>().clip = burstSoundsSecondary[ Random.Range( 0, burstSoundsSecondary.Length ) ];
            }
#else
			audio.clip = burstSounds[ Random.Range( 0, burstSounds.Length ) ];
#endif
			GetComponent<AudioSource>().Play();
		}

		for ( int pooIndex = 0; pooIndex < count; pooIndex++ )
		{
			GameObject poo = Object.Instantiate( pooPrefab ) as GameObject;
			Vector3 startScale = poo.transform.localScale;
			poo.transform.parent = transform.parent.parent; //ugh

			PooBall pooBall = poo.GetComponent( typeof( PooBall ) ) as PooBall;
			pooBall.canStick = false;
			pooBall.canJoin = true;

			DistanceJoint2D joint = poo.GetComponent( typeof( DistanceJoint2D ) ) as DistanceJoint2D;
			joint.enabled = false;

			float gravity = Random.Range( minPooGravity, maxPooGravity );
			poo.GetComponent<Rigidbody2D>().gravityScale = gravity;

			poo.GetComponent<Rigidbody2D>().velocity = new Vector2( Random.Range( minBurstVelocity.x, maxBurstVelocity.x ), Random.Range( minBurstVelocity.y, maxBurstVelocity.y ) );
			//float velDirection = poo.rigidbody2D.velocity.y / Mathf.Abs( poo.rigidbody2D.velocity.y );

			Vector3 pos = new Vector3( transform.position.x, transform.position.y + 0.0f/*velDirection * transform.localScale.y*/, 0.0f );
			pos.x += Random.Range( poo.transform.localScale.x / -2.0f, poo.transform.localScale.x / 2.0f );
			//pos.y += velDirection * poo.transform.localScale.y;
			poo.transform.position = pos;

			//scale gets crazy, try to reset it...
			poo.transform.localScale = startScale;
		}
		totalPooBursted += count;
	}
}
