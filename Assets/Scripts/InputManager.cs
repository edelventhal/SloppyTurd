using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
	private bool wasTapped;
	private ArrayList listeners;

	private static InputManager instance;

	public void Awake()
	{
		listeners = new ArrayList();
		instance = this;
	}

	public static InputManager GetInstance()
	{
		return instance;
	}

	public void Start()
	{
		wasTapped = false;
	}
	
	public void FixedUpdate()
	{
		//had issues using GetMouseButtonDown (wasn't always called) so I did this which is 100% reliable
		if ( Input.GetMouseButton( 0 ) || Input.GetKey( KeyCode.Space ) )
		{
			if ( !wasTapped )
			{
				wasTapped = true;

				for ( int listenerIndex = 0; listenerIndex < listeners.Count; listenerIndex++ )
				{
					if ( ( listeners[ listenerIndex ] as InputListener ).Tapped() )
					{
						break;
					}
				}
                
                DoUnlockTap();
			}
		}
		else
		{
			wasTapped = false;
		}
	}

	public void AddListener( InputListener listener )
	{
		listeners.Add( listener );
	}

	public void AddFirstListener( InputListener listener )
	{
		listeners.Insert( 0, listener );
	}
    
    private void DoUnlockTap()
    {
        int quadrant = -1;
        Vector2 unlockSize = new Vector2( Screen.width / 4, Screen.height / 4 );
        Vector3 p = Input.mousePosition;
        
        if ( p.x <= unlockSize.x && p.y <= unlockSize.y )
        {
            quadrant = 3;
        }
        else if ( p.x <= unlockSize.x && p.y >= Screen.height - unlockSize.y )
        {
            quadrant = 0;
        }
        else if ( p.x >= Screen.width - unlockSize.x && p.y <= unlockSize.y )
        {
            quadrant = 2;
        }
        else if ( p.x >= Screen.width - unlockSize.x && p.y >= Screen.height - unlockSize.y )
        {
            quadrant = 1;
        }
        
        Game.GetInstance().ProcessUnlock( quadrant );
    }
}

public interface InputListener
{
	bool Tapped();
}