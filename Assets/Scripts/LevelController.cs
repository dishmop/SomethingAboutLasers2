using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class LevelController : MonoBehaviour {
	
	protected GridController gc;
	public int levelNumber = 0;
	public string nextLevelName;
	public bool allBoxesMoveable = false;
	public bool levelFinished;
	
	protected int[] placementMade = new int[Global.BoxCount];
	
	// Use this for initialization
	protected void Start()
	{
		gc = GetComponent<GridController>();
		
		levelFinished = false;
	}
	
	// Update is called once per frame
	protected void Update()
	{
	}
	
	protected virtual void FinishLevel()
	{
		//		Debug.Log("levelComplete - levelName: " + Application.loadedLevelName + ", levelTime: " + Time.timeSinceLevelLoad + ", gameTime :" + (Time.time - MenuController.gameStartTime));
		GoogleAnalytics.Client.SendTimedEventHit("gameFlow", "levelComplete", Application.loadedLevelName, Time.timeSinceLevelLoad);
		GoogleAnalytics.Client.SendScreenHit("levelComplete" + Application.loadedLevelName);
//		Analytics.CustomEvent("levelComplete", new Dictionary<string, object>
//		                      {
//			{ "levelName", Application.loadedLevelName },
//			{ "levelTime", Time.timeSinceLevelLoad},
//			{ "gameTime", (Time.time - MenuController.gameStartTime)},
//		});
		
		
		//		Debug.Log("levelStart - levelName: " + nextLevelName + ", gameTime :" + (Time.time - MenuController.gameStartTime));
		GoogleAnalytics.Client.SendEventHit("gameFlow", "levelStart", nextLevelName);
//		
//		Analytics.CustomEvent("levelStart", new Dictionary<string, object>
//		{
//			{ "levelName", nextLevelName},
//			{ "gameTime", (Time.time - MenuController.gameStartTime)},
//		});		
		
		Application.LoadLevel(nextLevelName);
	}
	
	public virtual bool CanPlaceBox(Global.BoxType type) { return true; }
	public virtual bool CanMoveBox(Global.BoxType type) { return true; }
	
	public void SetBoxCount(Global.BoxType type, int number = 0)
	{
		placementMade[(int)type] = number;
	}
	public void AddBoxCount(Global.BoxType type, int addition)
	{
		placementMade[(int)type] += addition;
	}
	public virtual void PlaceBox(Global.BoxType type) { }
	public virtual void DeleteBox(Global.BoxType type) { }
}
