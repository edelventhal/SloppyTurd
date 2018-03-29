using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Advertisements;

public class GameGUI2 : MonoBehaviour
{
	[System.Serializable]
	public struct TurdSloppySwap
	{
		public GameObject turdObject;
		public GameObject sludgeObject;
	}

	public TurdSloppySwap[] swappedObjects;

	public GameObject introPanel;
	public GameObject tutorialPanel;
	public GameObject gameplayPanel;
	public GameObject deathPanel;

	public Text scoreText;
	public Text bestText;
	public GameObject sinkerFloaterPanel;
	public Text sinkerText;
	public Text floaterText;
	public Color selectedTextColor = Color.white;
	public Color deselectedTextColor = Color.black;
	public int pointsNeededToSelectSinkerFloater = 10;

	public void Update()
	{
		Game.GameState gameState = Game.GetInstance().GetState();
		introPanel.SetActive( gameState == Game.GameState.Menu );
		tutorialPanel.SetActive( gameState == Game.GameState.Waiting );
		gameplayPanel.SetActive( gameState == Game.GameState.Playing || gameState == Game.GameState.Dead || gameState == Game.GameState.Waiting );
		deathPanel.SetActive( gameState == Game.GameState.Dead );
		sinkerFloaterPanel.SetActive( tutorialPanel.activeSelf && Game.GetInstance().GetMaxPoints() >= pointsNeededToSelectSinkerFloater );

		if ( gameplayPanel.activeSelf )
		{
			scoreText.text = "" + ( Game.GetInstance().GetPoints() );
			bestText.text = "" + ( Game.GetInstance().GetMaxPoints() );
		}

		if ( gameState == Game.GameState.Waiting )
		{
			if ( sinkerFloaterPanel.activeSelf )
			{
				sinkerText.color  = Game.GetInstance().poo.IsSinker() ? selectedTextColor   : deselectedTextColor;
				floaterText.color = Game.GetInstance().poo.IsSinker() ? deselectedTextColor : selectedTextColor;
			}
		}

		for ( int swappedObjectIndex = 0; swappedObjectIndex < swappedObjects.Length; swappedObjectIndex++ )
		{
#if UNITY_IPHONE
			swappedObjects[ swappedObjectIndex ].turdObject.SetActive( Game.GetInstance().ShouldAdvance() );
			swappedObjects[ swappedObjectIndex ].sludgeObject.SetActive( !Game.GetInstance().ShouldAdvance() );
#else
			swappedObjects[ swappedObjectIndex ].turdObject.SetActive( true );
			swappedObjects[ swappedObjectIndex ].sludgeObject.SetActive( false );
#endif
		}
	}

	public void SinkerClicked()
	{
		Game.GetInstance().poo.SetIsSinker( true );
	}

	public void FloaterClicked()
	{
		Game.GetInstance().poo.SetIsSinker( false );
	}

	public void BeginGameClicked()
	{
		Game.GetInstance().SetState( Game.GetInstance().GetState() + 1 );

		if ( Game.GetInstance().GetState() == Game.GameState.Playing )
		{
			Game.GetInstance().poo.Tapped();
		}
	}

	public void ResetGame()
	{
		Game.GetInstance().Reset();
	}

	public void AdButtonClicked()
	{
		ShowOptions options = new ShowOptions();
		options.resultCallback = HandleAdShowResult;
		Advertisement.Show( "rewardedVideo", options );
	}

	protected void HandleAdShowResult( ShowResult result )
	{
		if ( result == ShowResult.Finished )
		{
			int score = Game.GetInstance().GetPoints();
			Game.GetInstance().Reset();
			Game.GetInstance().SetPoints( score );
		}
	}
}
