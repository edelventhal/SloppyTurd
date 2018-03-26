using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{
	public GameObject bottomWall;
	public GameObject topWall;
	public GameObject bottomEnd;
	public GameObject topEnd;
	public float wallEndDistance = 0.4f;

	public void Start()
	{
	
	}
	
	public void Update()
	{
	
	}

	public void Resize( float openingY, float openingSize, float bottom, float top )
	{
		float topWallBottom = openingY + openingSize / 2.0f;
		float bottomWallTop = openingY - openingSize / 2.0f;

		float topWallSize = top - topWallBottom;
		float bottomWallSize = bottomWallTop - bottom;

		bottomWall.transform.localScale = new Vector3( 1.0f, bottomWallSize, 1.0f );
		topWall.transform.localScale = new Vector3( 1.0f, topWallSize, 1.0f );

		bottomWall.transform.localPosition = new Vector3( 0.0f, bottom + bottomWallSize / 2.0f, 0.0f );
		topWall.transform.localPosition = new Vector3( 0.0f, top - topWallSize / 2.0f, 0.0f );

		bottomEnd.transform.localPosition = new Vector3( 0.0f, bottomWallTop - wallEndDistance, -0.5f );
		topEnd   .transform.localPosition = new Vector3( 0.0f, topWallBottom + wallEndDistance, -0.5f );
	}

	public GameObject GetBottomWall()
	{
		return bottomWall;
	}

	public GameObject GetTopWall()
	{
		return topWall;
	}
}
