using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class MenuController : MonoBehaviour
{
	
	public static string startScreenName = "Start Screen",
	puzzleSelectScreenName = "Puzzle Select Screen",
	defenceSelectScreenName = "Defence Select Screen",
	sandboxName = "Sandbox",
	logicSelectScreenName = "Logic Select Screen",
	helpScreenName = "Help Screen",
	aboutScreenName = "About Screen",
	quitScreenName = "Quit Screen",
	websiteText = "http://divf.eng.cam.ac.uk/gam2eng/Main/WebHome";
	public float defaultBorder = 0.0f, extendedBorder = 10.0f;
	public Color defaultTextColour, higlightedTextColour;
	
	public AudioClip soundMouseEnter, soundMouseExit;
	
	public GameObject[] buttons;
	private RectTransform[] buttonTransforms;
	private Text[] buttonTexts;
	private int selectedButton = -1;
	
	public string finalQuitURL;
	
	public static float gameStartTime = 0;
	
	// Use this for initialization
	void Start()
	{
		Cursor.visible = true;
		buttons = GameObject.FindGameObjectsWithTag("Menu Button");
		buttonTransforms = new RectTransform[buttons.Length];
		buttonTexts = new Text[buttons.Length];
		for (int n = buttons.Length - 1; n >= 0; n--)
		{
			buttonTransforms[n] = buttons[n].GetComponent<RectTransform>();
			buttonTexts[n] = buttons[n].GetComponentInChildren<Text>();
		}
		
		for (int n = buttonTransforms.Length - 1; n >= 0; n--)
		{
			/*buttonTransforms[n].anchorMax = new Vector2 (buttonTransforms[n].anchorMin.x,
                buttonTransforms[n].anchorMax.y);*/
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		for (int n = buttonTransforms.Length - 1; n >= 0; n--)
		{
			if (n != selectedButton)
			{
				/*buttonTransforms[n].anchorMax = new Vector2(
                buttonTransforms[n].anchorMax.x * 0.8f + defaultRightPosition * 0.2f,
                buttonTransforms[n].anchorMax.y);*/
				buttonTransforms[n].offsetMax = buttonTransforms[n].offsetMax * 0.8f + Vector2.one * defaultBorder * 0.2f;
				buttonTransforms[n].offsetMin = buttonTransforms[n].offsetMin * 0.8f - Vector2.one * defaultBorder * 0.2f;
				buttonTexts[n].color = Color.Lerp(defaultTextColour, buttonTexts[n].color, 0.8f);
			}
		}
		if (selectedButton >= 0 && selectedButton < buttonTransforms.Length)
		{
			/*buttonTransforms[selectedButton].anchorMax = new Vector2(
            buttonTransforms[selectedButton].anchorMax.x * 0.8f + extendedRightPosition * 0.2f,
            buttonTransforms[selectedButton].anchorMax.y);*/
			/*buttonTransforms[selectedButton].l = new Rect(
                buttonTransforms[selectedButton].rect.left * 0.8f + extendedBorder * 0.2f,
                buttonTransforms[selectedButton].rect.left * 0.8f + extendedBorder * 0.2f,
                buttonTransforms[selectedButton].rect.left * 0.8f + extendedBorder * 0.2f,
                buttonTransforms[selectedButton].rect.left * 0.8f + extendedBorder * 0.2f);*/
			buttonTransforms[selectedButton].offsetMax
				= buttonTransforms[selectedButton].offsetMax * 0.8f + Vector2.one * extendedBorder * 0.2f;
			buttonTransforms[selectedButton].offsetMin
				= buttonTransforms[selectedButton].offsetMin * 0.8f - Vector2.one * extendedBorder * 0.2f;
			buttonTexts[selectedButton].color = Color.Lerp(higlightedTextColour, buttonTexts[selectedButton].color, 0.8f);
		}
		
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Application.loadedLevelName == startScreenName)
			{
				GoToQuitScreen();
			}
			else
			{
				GoToStartScreen();
			}
		}
	}
	
	public void GoToStartScreen()
	{
		Application.LoadLevel(startScreenName);
	}
	public void GoToPuzzleSelectScreen()
	{
		Application.LoadLevel(puzzleSelectScreenName);
	}
	public void GoToDefenceSelectScreen()
	{
		Application.LoadLevel(defenceSelectScreenName);
	}
	public void GoToLogicSelectScreen()
	{
		Application.LoadLevel(logicSelectScreenName);
	}
	public void GoToHelpScreen()
	{
		Application.LoadLevel(helpScreenName);
	}
	public void GoToAboutScreen()
	{
		Application.LoadLevel(aboutScreenName);
	}
	public void GoToQuitScreen()
	{
		Application.LoadLevel(quitScreenName);
	}
	public void QuitGame()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_WEBPLAYER
		if (finalQuitURL != ""){
			Application.OpenURL(finalQuitURL);
		}
		#else
		if (finalQuitURL != ""){
			Application.OpenURL(finalQuitURL);
		}
		Application.Quit();
		#endif
	}
	
	public void StartLevel(GameObject callingButton)
	{
		for (int n = buttons.Length - 1; n >= 0; n--)
		{
			if (buttons[n].Equals(callingButton))
			{
				string levelName = callingButton.GetComponentInChildren<Text>().text.Replace(" Button", "");
				//Debug.Log ("Start level: " + levelName);
				//				Debug.Log ("startGame  - levelName: " + levelName);
				GoogleAnalytics.Client.SendEventHit("gameFlow", "startGame");
//				Analytics.CustomEvent("startGame", new Dictionary<string, object>
//				                      {
//					{ "levelName", levelName},
//				});				
				
				gameStartTime = Time.time;
				Application.LoadLevel(levelName);
				return;
			}
		}
	}
	public void StartSandbox()
	{
		Application.LoadLevel(sandboxName);
	}
	public void CopyWebsiteText()
	{
		//To be filled in at some point maybe?
	}
	
	public void MouseEnter(GameObject callingButton)
	{
		//Debug.Log("Mouse has entered " + callingButton.ToString());
		for (int n = buttons.Length - 1; n >= 0; n--)
		{
			if (buttons[n].Equals(callingButton))
			{
				//Debug.Log(callingButton.ToString() + " matched with index " + n.ToString());
				selectedButton = n;
				if (soundMouseEnter != null)
				{
					AudioSource.PlayClipAtPoint(soundMouseEnter, transform.position, 0.5f);
				}
				return;
			}
		}
	}
	public void MouseExit(bool playSound = true)
	{
		if (selectedButton != -1)
		{
			if (playSound && soundMouseExit != null)
			{
				AudioSource.PlayClipAtPoint(soundMouseExit, transform.position, 0.5f);
			}
		}
		selectedButton = -1;
	}
}
