using UnityEngine;
using System.Collections;

//mostly spawns shit
public class Game : MonoBehaviour
{
	public const int LIQUID_LAYER = 8;
	public const int WALL_LAYER = 9;
	public const int GROUND_LAYER = 10;
	public const int GOAL_LAYER = 11;

	public enum GameState
	{
		Menu=0,
		Waiting=1,
		Playing=2,
		Dead=3
	};

	public float firstWallDistance = 8.0f;
	public float minWallDistance = 1.0f;
	public float maxWallDistance = 1.5f;

	public float minWallOpening = 0.5f;
	public float maxWallOpening = 2.0f;

	public float minOpeningDifferencePositive = 0.0f;
	public float maxOpeningDifferencePositive = 1.5f;
	public float minOpeningDifferenceNegative = -3.0f;
	public float maxOpeningDifferenceNegative = 0.0f;

	public float minOpeningY = 0.0f;
	public float maxOpeningY = 5.0f;

	public int pooEmitterStride = 3;

	public int maxWallCount = 10;
	public int maxGroundCount = 6;
	
	public Poo poo;
	public GameObject ground;
	public GameObject roof;
	public GameObject wallPrefab;
	public GameObject groundVisualPrefab;

    public int advancePointsRequired = 100;

	private static Game instance;

	private ArrayList walls;
	private ArrayList groundVisuals;
	private GameState state;

	private float nextWallCreateDistance;
	private float nextWallOpeningY;
	private float distanceTraveled;
	private float wallBottom;
	private float wallTop;
	private int wallsRemainingBeforeEmitter;

	private float lastPooX;

	private bool pooDied;

	private int points;
	private int maxPoints;
    
    //for unlocking the magic...
    private int unlockStep;
    private int[] unlockOrder;

	public static Game GetInstance()
	{
		return instance;
	}

	public void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		walls = new ArrayList();
		groundVisuals = new ArrayList();

		wallBottom = ground.transform.position.y + ground.transform.localScale.y / 2.0f;
		wallTop = 2.0f * Camera.main.orthographicSize;
		roof.transform.position = new Vector3( 0.0f, wallTop + roof.transform.localScale.y / 2.0f, 0.0f );

		maxPoints = PlayerPrefs.GetInt( "MaxPoints" );

		Reset( true );

        unlockStep = 0;
        unlockOrder = new int[]{ 0, 1, 2, 3, 2, 1, 0 };
	}

	public void Reset()
	{
		Reset( false );
	}

	public void Reset( bool firstTime )
	{
		state = firstTime ? GameState.Menu : GameState.Waiting;

		poo.Reset();

		distanceTraveled = 0.0f;
		lastPooX = poo.transform.position.x;
		wallsRemainingBeforeEmitter = pooEmitterStride;
		
		nextWallOpeningY = poo.transform.position.y;
		UpdateNextWallCreateValues();
		nextWallCreateDistance = firstWallDistance;
		points = 0;

		for ( int wallIndex = 0; wallIndex < walls.Count; wallIndex++ )
		{
			Object.Destroy( ( walls[ wallIndex ] as Wall ).gameObject );
		}
		walls.Clear();

		for ( int groundIndex = 0; groundIndex < groundVisuals.Count; groundIndex++ )
		{
			Object.Destroy( groundVisuals[ groundIndex ] as GameObject );
		}
		groundVisuals.Clear();
	}

	public GameState GetState()
	{
		return state;
	}

	public void SetState( GameState newState )
	{
		state = newState;
	}
	
	public void Update()
	{
		UpdateDistanceTraveled();

		UpdateWalls();

		UpdateGroundVisuals();
	}

	public void DoPooHit( int count )
	{
		//if ( poo.GetAttachedBallCount() <= 0 )
		//{
			poo.SetIsDead( true );
		( Camera.main.GetComponent( typeof( FollowCam ) ) as FollowCam ).ShakeCamera( 0.35f );
		//}
	}

	public void PooPassedGoal()
	{
		points++;
		if ( points > maxPoints )
		{
			maxPoints = points;
			PlayerPrefs.SetInt( "MaxPoints", maxPoints );
		}
	}

	public int GetPoints()
	{
		return points;
	}

	public void SetPoints( int setPoints )
	{
		points = setPoints;
	}

	public int GetMaxPoints()
	{
		return maxPoints;
	}

    public bool ShouldAdvance()
    {
        return false;//maxPoints >= advancePointsRequired || unlockStep >= unlockOrder.Length;
    }
    
    public void ProcessUnlock( int quadrant )
    {
        //already unlocked!
        if ( ShouldAdvance() )
        {
            return;
        }

        int expectedQuadrant = unlockOrder[ unlockStep ];
        //Debug.Log( "quadrant " + quadrant + " expected " + expectedQuadrant );
        
        if ( quadrant == expectedQuadrant )
        {
            unlockStep++;
        }
        else
        {
            unlockStep = 0;
        }
    }

	private void UpdateDistanceTraveled()
	{
		float pooX = poo.transform.position.x;
		distanceTraveled += pooX - lastPooX;
		lastPooX = pooX;
	}

	private void UpdateWalls()
	{
		//see how far ahead the closest wall to the poo is
		bool createWall = true;
		if ( walls.Count > 0 )
		{
			Wall wall = walls[ walls.Count - 1 ] as Wall;
			createWall = ( wall.transform.position.x - poo.transform.position.x < nextWallCreateDistance );
		}

		if ( createWall )
		{
			CreateNewWall();
			UpdateNextWallCreateValues();
		}

		while ( walls.Count > maxWallCount )
		{
			Object.Destroy( ( walls[ 0 ] as Wall ).gameObject );
			walls.RemoveAt( 0 );
		}
	}

	private void UpdateGroundVisuals()
	{
		Vector3 maxCamPoint = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
		Vector3 minCamPoint = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
		float maxX = ( maxCamPoint.x - minCamPoint.x ) / 2.0f + poo.transform.position.x;

		while ( ShouldCreateGround( maxX ) )
		{
			GameObject newGround = Object.Instantiate( groundVisualPrefab ) as GameObject;

			if ( groundVisuals.Count <= 0 )
			{

				float camSize = maxCamPoint.x - minCamPoint.x;
				newGround.transform.position = new Vector3( -camSize/2.0f, ground.transform.position.y, 2.0f );
			}
			else
			{
				GameObject lastGround = groundVisuals[ groundVisuals.Count - 1 ] as GameObject;
				newGround.transform.position = new Vector3( lastGround.transform.position.x + lastGround.transform.localScale.x, ground.transform.position.y, 2.0f );
			}

			groundVisuals.Add( newGround );
		}

		while ( groundVisuals.Count > maxGroundCount )
		{
			Object.Destroy( groundVisuals[ 0 ] as GameObject );
			groundVisuals.RemoveAt( 0 );
		}
	}

	private bool ShouldCreateGround( float maxX )
	{
		bool createGround = true;
		if ( groundVisuals.Count > 0 )
		{
			GameObject lastGround = groundVisuals[ groundVisuals.Count - 1 ] as GameObject;
			createGround = lastGround.transform.position.x < maxX;
		}
		return createGround;
	}

	private void UpdateNextWallCreateValues()
	{
		nextWallCreateDistance = Random.Range( minWallDistance, maxWallDistance );

		//find a random next opening, 50% of the time it goes up and 50% it goes down
		float wallOpeningChange = 0.0f;
		if ( Random.Range( 0, 10 ) >= 5 )
		{
			wallOpeningChange = Random.Range( minOpeningDifferencePositive, maxOpeningDifferencePositive );
		}
		else
		{
			wallOpeningChange = Random.Range( minOpeningDifferenceNegative, maxOpeningDifferenceNegative );
		}

		//if the opening would exceed the limits, go in the other direction instead
		if ( nextWallOpeningY + wallOpeningChange > maxOpeningY || nextWallOpeningY + wallOpeningChange < minOpeningY )
		{
			wallOpeningChange *= -1.0f;
		}

		//add the change, and clamp for good measure
		nextWallOpeningY += wallOpeningChange;
		nextWallOpeningY = Mathf.Clamp( nextWallOpeningY, minOpeningY, maxOpeningY );
	}

	private void CreateNewWall()
	{
		float lastWallX = 0.0f;
		if ( walls.Count > 0 )
		{
			lastWallX = ( walls[ walls.Count - 1 ] as Wall ).transform.position.x;
		}

		Wall newWall = ( Object.Instantiate( wallPrefab ) as GameObject ).GetComponent( typeof( Wall ) ) as Wall;
		newWall.transform.position = new Vector3( lastWallX + nextWallCreateDistance, 0.0f, 0.0f );

		newWall.Resize( nextWallOpeningY + wallBottom, Random.Range( minWallOpening, maxWallOpening ), wallBottom, wallTop );

		walls.Add( newWall );

		wallsRemainingBeforeEmitter--;
		if ( wallsRemainingBeforeEmitter > 0 )
		{
			PooPipe pipe = newWall.GetComponentInChildren( typeof( PooPipe ) ) as PooPipe;
			Object.Destroy( pipe );
		}
		else
		{
			wallsRemainingBeforeEmitter = pooEmitterStride;
		}
	}
}
