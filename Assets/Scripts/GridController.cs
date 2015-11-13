using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;


//[RequireComponent(typeof(AudioSource))]
public class GridController : MonoBehaviour
{
	
	public enum Direction
	{
		Up, Right, Down, Left
	}
	
	public float cellSize = 1.0f;
	public int gridWidth = 10, gridHeight = 10;
	private LevelController lc;
	private Global.BoxType[,] types;
	private GameObject[,] boxes;
	private Direction[,] directions;
	private bool[,] isFixed, isOn, isEnemy;
	
	private Color colourCursor;
	public Color colourCursorDefault, colourCursorHighlight, colourCursorFixed;
	
	private float[, ,] powers;
	private bool[, ,] hasLaser;
	public bool initialised, upToDate;
	
	private int mouseX, mouseY, dragX, dragY, rotateX, rotateY;
	private bool willRotate;
	private Transform cursor, cursorRaisedImage;
	public float maxPowerLimit = 32.5f;
	public Global.BoxType placementType;
	public GameObject[] placementBoxes = new GameObject[Global.BoxCount];
	private Global.BoxType[] typesAvailable = null;
	
	public string escapeLevelName = "Start Screen";
	
	public GameObject laser;
	public Material overPoweredMaterial;
	private Vector2 startPoint;
	
	public float signalPower, signalPeriod = 5.0f;
	
	public Material materialOn, materialOff;
	public AudioClip soundButtonOn, soundButtonOff;
	
	//Laser sounds
	/*private AudioSource audioSource;
    private float[] pitchVolumes;*/
	
	// Use this for initialization
	void Start()
	{
		//Cursor.visible = false;
		lc = GetComponent<LevelController>();
		initialised = false;
		powers = new float[gridWidth, gridHeight, 4];
		types = new Global.BoxType[gridWidth, gridHeight];
		boxes = new GameObject[gridWidth, gridHeight];
		directions = new Direction[gridWidth, gridHeight];
		isFixed = new bool[gridWidth, gridHeight];
		hasLaser = new bool[gridWidth, gridHeight, 4];
		isOn = new bool[gridWidth, gridHeight];
		isEnemy = new bool[gridWidth, gridHeight];
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				types[x, y] = Global.BoxType.None;
				directions[x, y] = Direction.Up;
				isFixed[x, y] = false;
				isOn[x, y] = false;
				isEnemy[x, y] = false;
				for (int d = 0; d < 4; d++)
				{
					powers[x, y, d] = 0;
					hasLaser[x, y, d] = false;
				}
			}
		}
		//Copy the pre-made level into the grid setup and fix it in place
		GameObject[] existingBoxes = GameObject.FindGameObjectsWithTag("Box");
		if (existingBoxes.Length > 0)
		{
			Vector2 position;
			Global.BoxType type;
			for (int b = existingBoxes.Length - 1; b >= 0; b--)
			{
				position = new Vector2(Mathf.Floor(existingBoxes[b].transform.position.x / cellSize),
				                       Mathf.Floor(existingBoxes[b].transform.position.z / cellSize));
				if (!(position.x < 0 || position.x >= gridWidth || position.y < 0 || position.y >= gridHeight))
				{
					type = existingBoxes[b].GetComponent<BoxController>().type;
					PlaceBox((int)position.x, (int)position.y, type, true);
					RotateBox((int)position.x, (int)position.y, DirectionFromVector(existingBoxes[b].transform.forward));
					if (!lc.allBoxesMoveable)
					{
						isFixed[(int)position.x, (int)position.y] = true;
					}
					else
					{
						isFixed[(int)position.x, (int)position.y] = false;
					}
					if (type == Global.BoxType.PowerMeter)
					{
						boxes[(int)position.x, (int)position.y].GetComponent<TargetController>()
							.SetTarget(existingBoxes[b].GetComponent<TargetController>());
					}
					if (type == Global.BoxType.Button || type == Global.BoxType.WalkwayButton)
					{
						boxes[(int)position.x, (int)position.y].transform.FindChild("Button").GetComponent<MeshRenderer>().material
							= materialOff;
					}
				}
				Destroy(existingBoxes[b]);
			}
		}
		GameObject[] onPoints = GameObject.FindGameObjectsWithTag("On");
		if (onPoints.Length > 0)
		{
			Vector2 position;
			for (int o = onPoints.Length - 1; o >= 0; o--)
			{
				position = new Vector2(Mathf.Floor(onPoints[o].transform.position.x / cellSize),
				                       Mathf.Floor(onPoints[o].transform.position.z / cellSize));
				ToggleButton((int)position.x, (int)position.y);
				Destroy(onPoints[o]);
			}
		}
		upToDate = false;
		
		//Setup the cursor
		cursor = GameObject.Find("Cursor").transform;
		cursorRaisedImage = cursor.transform.GetChild(0);
		colourCursor = colourCursorDefault;
		dragX = -1;
		dragY = -1;
		rotateX = -1;
		rotateY = -1;
		
		signalPower = 0.0f;
		
		/*audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.0f;
        audioSource.Play();
        audioSource.loop = true;
        pitchVolumes = new float[24];*/
		
		initialised = true;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//			Debug.Log ("quitGame - levelName: " + Application.loadedLevelName + ", levelTime: " + Time.timeSinceLevelLoad + ", gameTime :" + (Time.time - MenuController.gameStartTime));
			GoogleAnalytics.Client.SendTimedEventHit("gameFlow", "quitGame", Application.loadedLevelName, Time.timeSinceLevelLoad);
//						
//			Analytics.CustomEvent("quitGame", new Dictionary<string, object>
//			                      {
//				{ "levelName", Application.loadedLevelName },
//				{ "levelTime", Time.timeSinceLevelLoad},
//				{ "gameTime", (Time.time - MenuController.gameStartTime)},
//			});
			Application.LoadLevel(escapeLevelName);
		}
		
		if (initialised)
		{
			UpdateMouseWorldPosition();
			
			//Left mouse button
			if (Input.GetMouseButtonDown(0))
			{
				if (!(mouseX < 0 || mouseX >= gridWidth || mouseY < 0 || mouseY >= gridHeight))
				{
					if (types[mouseX, mouseY] == Global.BoxType.None)
					{
						if (placementType != Global.BoxType.None && lc.CanPlaceBox(placementType))
						{
							PlaceBox(mouseX, mouseY, placementType);
							rotateX = mouseX;
							rotateY = mouseY;
						}
					}
					else if (!isFixed[mouseX, mouseY])
					{
						dragX = mouseX;
						dragY = mouseY;
						willRotate = true;
					}
				}
			}
			if (!(dragX < 0 || dragX >= gridWidth || dragY < 0 || dragY >= gridHeight
			      || mouseX < 0 || mouseX >= gridWidth || mouseY < 0 || mouseY >= gridHeight))
			{
				if (types[dragX, dragY] != Global.BoxType.None && types[mouseX, mouseY] == Global.BoxType.None)
				{
					MoveBox(dragX, dragY, mouseX, mouseY);
					dragX = mouseX;
					dragY = mouseY;
					willRotate = false;
				}
			}
			if (!(rotateX < 0 || rotateX >= gridWidth || rotateY < 0 || rotateY >= gridHeight))
			{
				Direction direction = DirectionFromVector(new Vector2(mouseX - rotateX, mouseY - rotateY));
				if (direction != directions[rotateX, rotateY])
				{
					RotateBox(rotateX, rotateY, direction);
				}
			}
			if (Input.GetMouseButtonUp(0))
			{
				if (!(mouseX < 0 || mouseX >= gridWidth || mouseY < 0 || mouseY >= gridHeight))
				{
					if ((willRotate && (rotateX == -1 || rotateY == -1)))
					{
						RotateBox(mouseX, mouseY, Direction.Right, true);
						ToggleButton(mouseX, mouseY);
					}
					else if (isFixed[mouseX, mouseY])
					{
						ToggleButton(mouseX, mouseY);
					}
				}
				rotateX = -1;
				rotateY = -1;
				dragX = -1;
				dragY = -1;
			}
			//Right mouse button
			if (Input.GetMouseButtonDown(1))
			{
				if (!(mouseX < 0 || mouseX >= gridWidth || mouseY < 0 || mouseY >= gridHeight))
				{
					DeleteBox(mouseX, mouseY);
				}
			}
			//Middle mouse button
			if (Input.GetMouseButtonDown(2))
			{
				if (!(mouseX < 0 || mouseX >= gridWidth || mouseY < 0 || mouseY >= gridHeight))
				{
					if (types[mouseX, mouseY] != Global.BoxType.None)
					{
						if (types[mouseX, mouseY] == Global.BoxType.Button)
						{
							ToggleButton(mouseX, mouseY);
						}
						else if (!isFixed[mouseX, mouseY])
						{
							rotateX = mouseX;
							rotateY = mouseY;
						}
					}
				}
			}
			if (Input.GetMouseButtonUp(2))
			{
				rotateX = -1;
				rotateY = -1;
			}
			
			//Allow the placement type to be changed by scrolling or using the arrow keys (multiplied by 10 to saturate the clamp)
			NudgePlacementType((int)Mathf.Clamp(11.0f * -Input.GetAxis("Mouse ScrollWheel"), -1, 1));
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				NudgePlacementType(-1);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				NudgePlacementType(1);
			}
			
			//Update the signal source output
			signalPower = 0.5f * (1.0f + Mathf.Cos((Time.time / signalPeriod) * 2.0f * Mathf.PI));
			upToDate = false;
			
			//Update powers to preserve memory of the current state
			for (int y = gridHeight - 1; y >= 0; y--)
			{
				for (int x = gridWidth - 1; x >= 0; x--)
				{
					for (int d = 0; d < 4; d++)
					{
						if (!hasLaser[x, y, d])
						{
							powers[x, y, d] *= 0.8f;
							if (powers[x, y, d] <= 0.0001f)
							{
								powers[x, y, d] = 0.0f;
							}
						}
					}
				}
			}
			//Update PowerMeters
			for (int y = gridHeight - 1; y >= 0; y--)
			{
				for (int x = gridWidth - 1; x >= 0; x--)
				{
					if (types[x, y] == Global.BoxType.PowerMeter)
					{
						float inputPower = GetInputPower(x, y, Direction.Left);
						boxes[x, y].transform.FindChild("Screen").GetChild(0).GetComponent<TextMesh>().text
							= inputPower.ToString("0.##");//.Remove(5);
					}
				}
			}
			//If the grid has been changed, recalculate the lasers
			if (!upToDate)
			{
				CalculateGrid();
				/*UpdateSound();*/
			}
		}
	}
	
	void OnDrawGizmos()
	{
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				for (int d = 0; d < 4; d++)
				{
					Gizmos.DrawLine(new Vector3(x + 0.5f, 0.0f, y + 0.5f) * cellSize,
					                new Vector3(x + 0.5f, 0.0f, y + 0.5f) + Vector3FromDirection((Direction)d) + Vector3.up * 0.25f);
					Gizmos.color = Color32.Lerp(Color.clear, Color.blue, powers[x, y, d]);
					Gizmos.DrawLine(new Vector3(x + 0.5f, 0.0f, y + 0.5f) * cellSize + Vector3.up * 0.25f,
					                new Vector3(x + 0.5f, 0.0f, y + 0.5f) + Vector3FromDirection((Direction)d) + Vector3.up * 0.5f);
				}
			}
		}
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (types[x, y] == Global.BoxType.None)
				{
					Gizmos.color = Color.white;
					Gizmos.DrawSphere(new Vector3((float)x + 0.5f, 0.0f, (float)y + 0.5f) * cellSize, 0.2f);
				}
				if (isOn[x, y])
				{
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(new Vector3((float)x + 0.5f, 0.2f, (float)y + 0.5f) * cellSize, 0.5f);
				}
			}
		}
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(new Vector3((float)mouseX + 0.5f, 0.5f, (float)mouseY + 0.5f) * cellSize, Vector3.one);
	}
	
	/*void OnGUI()
    {
        GUI.Label(new Rect(8, 8, 150, 24), placementType.ToString());
            //+ " (" + (placementAvailable[(int)placementType] - placementMade[(int)placementType]).ToString() + ")");
    }*/
	
	void UpdateMouseWorldPosition()
	{
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		float hitDistance;
		if (!Global.groundPlane.Raycast(mouseRay, out hitDistance))
		{
			//Debug.Log ("Mouse position raycast did not hit the ground plane");
		}
		Vector3 position = mouseRay.origin + mouseRay.direction * hitDistance;
		position /= cellSize;
		//Debug.Log ("Mouse world position: " + position.ToString ());
		//mouseX = Mathf.Clamp ((int)Mathf.Round (position.x), 0, gridWidth - 1);
		//mouseY = Mathf.Clamp ((int)Mathf.Round (position.z), 0, gridHeight - 1);
		mouseX = (int)Mathf.Floor(position.x);
		mouseY = (int)Mathf.Floor(position.z);
		if (!(dragX < 0 || dragX >= gridWidth || dragY < 0 || dragY >= gridHeight))
		{
			UpdateCursorState(2, isFixed[dragX, dragY], types[dragX, dragY] == Global.BoxType.Button);
		}
		else if (!(rotateX < 0 || rotateX >= gridWidth || rotateY < 0 || rotateY >= gridHeight))
		{
			UpdateCursorState(3, isFixed[rotateX, rotateY], types[rotateX, rotateY] == Global.BoxType.Button);
		}
		else if (!(mouseX < 0 || mouseX >= gridWidth || mouseY < 0 || mouseY >= gridHeight))
		{
			if (types[mouseX, mouseY] != Global.BoxType.None)
			{
				UpdateCursorState(1, isFixed[mouseX, mouseY], types[mouseX, mouseY] == Global.BoxType.Button);
			}
			else
			{
				UpdateCursorState(0);
			}
		}
		else
		{
			UpdateCursorState(0);
		}
	}
	void UpdateCursorState(int state, bool isFixed = false, bool isButton = false)//0 - Flat, 1 - Highlight, 2 - Drag, 3 - Rotate
	{
		switch (state)
		{
		case (0):
		{
			cursor.position = new Vector3(mouseX + 0.5f, 0.0f, mouseY + 0.5f) * cellSize;
			cursor.localScale = cursor.localScale * 0.8f + Vector3.one * 0.2f * cellSize;
			cursorRaisedImage.localPosition
				= new Vector3(0.0f, cursorRaisedImage.localPosition.y * 0.8f, 0.0f);
			colourCursor = Color.Lerp(colourCursor, colourCursorDefault, 0.2f);
			break;
		}
		case (1):
		{
			cursor.position = new Vector3(mouseX + 0.5f, 0.0f, mouseY + 0.5f) * cellSize;
			cursor.localScale = cursor.localScale * 0.8f + Vector3.one * 1.25f * 0.2f * cellSize;
			cursorRaisedImage.localPosition
				= new Vector3(0.0f, cursorRaisedImage.localPosition.y * 0.8f + 0.2f, 0.0f);
			colourCursor = Color.Lerp(colourCursor, isFixed ? colourCursorFixed : colourCursorHighlight, 0.2f);
			break;
		}
		case (2):
		{
			cursor.position = new Vector3(dragX + 0.5f, 0.0f, dragY + 0.5f) * cellSize;
			cursor.localScale = cursor.localScale * 0.8f + Vector3.one * 1.25f * 0.2f * cellSize;
			cursorRaisedImage.localPosition
				= new Vector3(0.0f, cursorRaisedImage.localPosition.y * 0.8f + 0.6f * 0.2f, 0.0f);
			colourCursor = Color.Lerp(colourCursor, isFixed ? colourCursorFixed : colourCursorHighlight, 0.2f);
			break;
		}
		case (3):
		{
			cursor.position = new Vector3(rotateX + 0.5f, 0.0f, rotateY + 0.5f) * cellSize;
			cursor.localScale = cursor.localScale * 0.8f + Vector3.one * 1.25f * 0.2f * cellSize;
			cursorRaisedImage.localPosition
				= new Vector3(0.0f, cursorRaisedImage.localPosition.y * 0.8f + 0.6f * 0.2f, 0.0f);
			colourCursor = Color.Lerp(colourCursor, isFixed ? colourCursorFixed : colourCursorHighlight, 0.2f);
			break;
		}
		}
		if (!isButton)
		{
			cursorRaisedImage.localScale = cursorRaisedImage.localScale * 0.8f + Vector3.one * 0.2f;
		}
		else
		{
			cursorRaisedImage.localScale = cursorRaisedImage.localScale * 0.8f + (Vector3.one * 0.25f + Vector3.forward * 5.0f) * 0.2f;
		}
		foreach (SpriteRenderer sr in cursor.GetComponentsInChildren<SpriteRenderer>())
		{
			sr.color = colourCursor;
		}
	}
	
	void ToggleButton(int x, int y)
	{
		if (types[x, y] == Global.BoxType.Button)
		{
			isOn[x, y] = !isOn[x, y];
			if (isOn[x, y])
			{
				//boxes[x, y].transform.FindChild("Button").GetComponent<MeshRenderer>().material = materialOn;
				if (soundButtonOn != null)
				{
					AudioSource.PlayClipAtPoint(soundButtonOn, Vector3.zero, 0.4f);
				}
			}
			else
			{
				//boxes[x, y].transform.FindChild("Button").GetComponent<MeshRenderer>().material = materialOff;
				if (soundButtonOff != null)
				{
					AudioSource.PlayClipAtPoint(soundButtonOff, Vector3.zero, 0.4f);
				}
			}
		}
		/*else
        {
            //Debug.Log("Can't toggle; there is no button at (" + x + ", " + y + ")");
        }*/
	}
	public void NotifyOfEnemy(float worldX, float worldY)
	{
		int x = Mathf.FloorToInt(worldX / cellSize);
		int y = Mathf.FloorToInt(worldY / cellSize);
		if (!(x < 0 || x >= gridWidth || y < 0 || y >= gridHeight))
		{
			isEnemy[x, y] = true;
			if (types[x, y] == Global.BoxType.WalkwayButton)
			{
				if (isOn[x, y] == false)
				{
					isOn[x, y] = true;
					if (soundButtonOn != null)
					{
						AudioSource.PlayClipAtPoint(soundButtonOn, Vector3.zero, 0.3f);
					}
				}
			}
		}
	}
	
	public Global.BoxType GetPlacementType()
	{
		return placementType;
	}
	public void SetPlacementType(Global.BoxType type)
	{
		placementType = type;
	}
	public void NudgePlacementType(int delta = 1, int attempts = 0)
	{
		if (delta != 0 && attempts <= Global.BoxCount)
		{
			placementType = (Global.BoxType)(((int)placementType + delta + Global.BoxCount) % Global.BoxCount);
			if (placementType == Global.BoxType.None)
			{
				NudgePlacementType(delta, attempts + 1);
			}
			if (typesAvailable != null)
			{
				bool isAvailable = false;
				for (int n = typesAvailable.Length - 1; n >= 0; n--)
				{
					if (typesAvailable[n] == placementType)
					{
						isAvailable = true;
						break;
					}
				}
				if (!isAvailable)
				{
					NudgePlacementType(delta, attempts + 1);
				}
			}
		}
	}
	public void SetTypesAvailable(Global.BoxType[] newTypesAvailable)
	{
		typesAvailable = newTypesAvailable;
	}
	
	void CalculateGrid()
	{
		GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");
		for (int l = lasers.Length - 1; l >= 0; l--)
		{
			Destroy(lasers[l]);
		}
		//Reset general powers
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				for (int d = 0; d < 4; d++)
				{
					hasLaser[x, y, d] = false;
				}
				if (!(types[x, y] == Global.BoxType.Button || types[x, y] == Global.BoxType.WalkwayButton))
				{
					isOn[x, y] = false;
				}
				/*if (types[x, y] == Global.BoxType.WalkwayButton)
                {
                    if (isEnemy[x, y] == true)
                    {
                        isOn[x, y] = true;
                    }
                }*/
			}
		}
		//Check to see which squares are on
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (types[x, y] == Global.BoxType.Button || types[x, y] == Global.BoxType.WalkwayButton)
				{
					if (isOn[x, y])
					{
						if (types[x, Mathf.Clamp(y + 1, 0, gridHeight - 1)] != Global.BoxType.Button
						    && types[x, Mathf.Clamp(y + 1, 0, gridHeight - 1)] != Global.BoxType.WalkwayButton)
						{
							isOn[x, Mathf.Clamp(y + 1, 0, gridHeight - 1)] = true;
						}
						if (types[Mathf.Clamp(x + 1, 0, gridWidth - 1), y] != Global.BoxType.Button
						    && types[Mathf.Clamp(x + 1, 0, gridWidth - 1), y] != Global.BoxType.WalkwayButton)
						{
							isOn[Mathf.Clamp(x + 1, 0, gridWidth - 1), y] = true;
						}
						if (types[x, Mathf.Clamp(y - 1, 0, gridHeight - 1)] != Global.BoxType.Button
						    && types[x, Mathf.Clamp(y - 1, 0, gridHeight - 1)] != Global.BoxType.WalkwayButton)
						{
							isOn[x, Mathf.Clamp(y - 1, 0, gridHeight - 1)] = true;
						}
						if (types[Mathf.Clamp(x - 1, 0, gridWidth - 1), y] != Global.BoxType.Button
						    && types[Mathf.Clamp(x - 1, 0, gridWidth - 1), y] != Global.BoxType.WalkwayButton)
						{
							isOn[Mathf.Clamp(x - 1, 0, gridWidth - 1), y] = true;
						}
					}
					//Set the appropriate texture
					if (isOn[x, y])
					{
						boxes[x, y].transform.FindChild("Button").GetComponent<MeshRenderer>().material = materialOn;
					}
					else
					{
						boxes[x, y].transform.FindChild("Button").GetComponent<MeshRenderer>().material = materialOff;
					}
				}
			}
		}
		//Send lasers out from sources
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (types[x, y] == Global.BoxType.PowerSource && isOn[x, y])
				{
					startPoint = new Vector2(x, y);
					RecursiveLaser(x, y, directions[x, y], Global.LinePower);
				}
			}
		}
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (types[x, y] == Global.BoxType.BooleanSource && isOn[x, y])
				{
					startPoint = new Vector2(x, y);
					RecursiveLaser(x, y, directions[x, y], Global.SignalPower);
				}
			}
		}
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (types[x, y] == Global.BoxType.CosineSource && isOn[x, y])
				{
					startPoint = new Vector2(x, y);
					RecursiveLaser(x, y, directions[x, y], signalPower);
				}
			}
		}
		//Calculate outputs for input-dependent boxes
		//for (int n = 10; n > 0; n--)
		{
			for (int y = gridHeight - 1; y >= 0; y--)
			{
				for (int x = gridWidth - 1; x >= 0; x--)
				{
					if (types[x, y] == Global.BoxType.Combiner)
					{
						float cumulativePower = 0.0f;
						cumulativePower += GetInputPower(x, y, Direction.Down);
						cumulativePower += GetInputPower(x, y, Direction.Left);
						cumulativePower += GetInputPower(x, y, Direction.Right);
						if (cumulativePower > 0.001f)
						{
							startPoint = new Vector2(x, y);
							RecursiveLaser(x, y, directions[x, y], cumulativePower);
						}
					}
				}
			}
			for (int y = gridHeight - 1; y >= 0; y--)
			{
				for (int x = gridWidth - 1; x >= 0; x--)
				{
					if (types[x, y] == Global.BoxType.Transistor)
					{
						float inputPower = GetInputPower(x, y, Direction.Down);
						float signalPower = Mathf.Clamp(GetInputPower(x, y, Direction.Up), 0, Global.SignalPower);
						if (inputPower > 0.001f)
						{
							startPoint = new Vector2(x, y);
							RecursiveLaser(x, y, AddDirection(directions[x, y], -1), signalPower * inputPower);
							startPoint = new Vector2(x, y);
							RecursiveLaser(x, y, AddDirection(directions[x, y], 1), (1 - signalPower) * inputPower);
						}
					}
				}
			}
			for (int y = gridHeight - 1; y >= 0; y--)
			{
				for (int x = gridWidth - 1; x >= 0; x--)
				{
					if (types[x, y] == Global.BoxType.Limiter)
					{
						//Forwards
						float signalPower = Mathf.Max(GetInputPower(x, y, Direction.Left), GetInputPower(x, y, Direction.Right));
						float inputPower = Mathf.Clamp(GetInputPower(x, y, Direction.Down), 0, signalPower);
						if (inputPower > 0.001f)
						{
							startPoint = new Vector2(x, y);
							RecursiveLaser(x, y, directions[x, y], inputPower);
						}
						//Backwards
						inputPower = Mathf.Clamp(GetInputPower(x, y, Direction.Up), 0, signalPower);
						if (inputPower > 0.001f)
						{
							startPoint = new Vector2(x, y);
							RecursiveLaser(x, y, AddDirection(directions[x, y], 2), inputPower);
						}
					}
				}
			}
		}
		//Count the number of towers of each type that have been placed
		for (int n = Global.BoxCount - 1; n >= 0; n--)
		{
			lc.SetBoxCount((Global.BoxType)n, 0);
		}
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (types[x, y] != Global.BoxType.None && isFixed[x, y] == false)
				{
					lc.AddBoxCount(types[x, y], 1);
				}
			}
		}
		//Reset isEnemy states
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (!isEnemy[x, y] && types[x, y] == Global.BoxType.WalkwayButton)
				{
					if (isOn[x, y] == true)
					{
						isOn[x, y] = false;
						if (soundButtonOff != null)
						{
							AudioSource.PlayClipAtPoint(soundButtonOff, Vector3.zero, 0.8f);
						}
					}
					//isOn[x, y] = false;
				}
				isEnemy[x, y] = false;
			}
		}
		upToDate = true;//This could go at the start because it's possible that actions in this routine will re-invalidate the grid?
	}
	
	/*void UpdateSound()
    {
        for (int p = pitchVolumes.Length - 1; p >= 0; p--)
        {
            pitchVolumes[p] = 0;
        }
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = gridWidth - 1; x >= 0; x--)
            {
                for (int d = 0; d < 4; d++)
                {
                    if (powers[x, y, d] > 0.01f)
                    {
                        /*pitchVolumes[Mathf.Min(Mathf.FloorToInt(powers[x, y, d]
                            * pitchVolumes.Length / maxPowerLimit), pitchVolumes.Length - 1)]
                            += (1.2f - Mathf.Exp(-powers[x, y, d])) * 15.0f;*
                        pitchVolumes[Mathf.Min(Mathf.FloorToInt(Mathf.Pow(powers[x, y, d]
                            / maxPowerLimit, 0.5f) * pitchVolumes.Length), pitchVolumes.Length - 1)]
                            += (1.2f - Mathf.Exp(-powers[x, y, d])) * 15.0f;
                    }
                }
            }
        }
        float averagePitch = 0.0f;
        float totalVolume = 0.0f;
        for (int p = pitchVolumes.Length - 1; p >= 0; p--)
        {
            totalVolume += pitchVolumes[p] * (1.5f - (p / pitchVolumes.Length)) * 0.25f;
            averagePitch += (13.0f - p) * 0.2f * pitchVolumes[p] * (1.5f - (p / pitchVolumes.Length)) * 0.25f;
        }
        if (totalVolume > 0.0001f)
        {
            averagePitch /= totalVolume;
            audioSource.pitch = audioSource.pitch * 0.6f + averagePitch * 0.4f;
            audioSource.volume = audioSource.volume * 0.6f + Mathf.Clamp((1 - Mathf.Exp(-totalVolume)) * 0.05f, 0.0f, 0.4f) * 0.4f;
        }
        else
        {
            audioSource.volume = 0.0f;
        }
    }*/
	
	float GetInputPower(int x, int y, Direction fromThisSideInBoxReferenceFrame)
	{
		Direction directionToCheckPosition = AddDirection(directions[x, y], (int)fromThisSideInBoxReferenceFrame);
		Vector2 checkPosition = GetAdjacent(x, y, directionToCheckPosition);
		if (!(checkPosition.x < 0 || checkPosition.x >= gridWidth || checkPosition.y < 0 || checkPosition.y >= gridHeight))
		{
			return powers[(int)checkPosition.x, (int)checkPosition.y, (int)FlipDirection(directionToCheckPosition)];
		}
		else
		{
			return 0.0f;
		}
	}
	public float GetInputPower(GameObject boxInQuestion, Direction fromThisSideInBoxReferenceFrame)
	{
		for (int y = gridHeight - 1; y >= 0; y--)
		{
			for (int x = gridWidth - 1; x >= 0; x--)
			{
				if (GameObject.Equals(boxInQuestion, boxes[x, y]))
				{
					return GetInputPower(x, y, fromThisSideInBoxReferenceFrame);
				}
			}
		}
		return 0.0f;
	}
	
	void RecursiveLaser(int x, int y, Direction direction, float power, int n = 0)
	{
		hasLaser[x, y, (int)direction] = true;
		powers[x, y, (int)direction] = powers[x, y, (int)direction] * 0.8f + 0.2f * power;
		Vector2 directionVector = VectorFromDirection(direction);
		int nextX = (int)(x + directionVector.x), nextY = (int)(y + directionVector.y);
		if (!(nextX < 0 || nextX >= gridWidth || nextY < 0 || nextY >= gridHeight))
		{
			switch (types[nextX, nextY])
			{
			case (Global.BoxType.None):
			{
				RecursiveLaser(nextX, nextY, direction, power, n + 1);
				break;
			}
			case (Global.BoxType.Mirror):
			{
				PlaceLaser(nextX, nextY, direction, powers[(int)startPoint.x, (int)startPoint.y, (int)direction]);
				startPoint = new Vector2(nextX, nextY);
				Direction relativeDirection = RelativeDirection(nextX, nextY, direction);
				RecursiveLaser(nextX, nextY, AddDirection(direction,
				                                          (relativeDirection == Direction.Up || relativeDirection == Direction.Down) ? 1 : -1), power, n + 1);
				break;
			}
			case (Global.BoxType.Splitter):
			{
				PlaceLaser(nextX, nextY, direction, powers[(int)startPoint.x, (int)startPoint.y, (int)direction]);
				if (RelativeDirection(nextX, nextY, direction) == Direction.Up)
				{
					startPoint = new Vector2(nextX, nextY);
					RecursiveLaser(nextX, nextY, AddDirection(direction, 1), 0.5f * power, n + 1);
					startPoint = new Vector2(nextX, nextY);
					RecursiveLaser(nextX, nextY, AddDirection(direction, -1), 0.5f * power, n + 1);
				}
				break;
			}
			case (Global.BoxType.PowerMeter):
			{
				PlaceLaser(nextX, nextY, direction, powers[(int)startPoint.x, (int)startPoint.y, (int)direction]);
				if (RelativeDirection(nextX, nextY, direction) == Direction.Right)
				{
					startPoint = new Vector2(nextX, nextY);
					RecursiveLaser(nextX, nextY, direction, power, n + 1);
				}
				break;
			}
			case (Global.BoxType.Button):
			case (Global.BoxType.Walkway):
			case (Global.BoxType.WalkwayButton):
			{
				if (isEnemy[nextX, nextY])
				{
					PlaceLaser(nextX, nextY, direction, powers[(int)startPoint.x, (int)startPoint.y, (int)direction]);
					if (power > 0.1f)
					{
						startPoint = new Vector2(nextX, nextY);
						RecursiveLaser(nextX, nextY, direction, power * 0.5f, n + 1);
					}
				}
				else
				{
					RecursiveLaser(nextX, nextY, direction, power, n + 1);
				}
				break;
			}
			default:
			{
				PlaceLaser(nextX, nextY, direction, powers[(int)startPoint.x, (int)startPoint.y, (int)direction]);
				break;
			}
			}
		}
		else
		{
			PlaceLaser(nextX, nextY, direction, power);
		}
		if (powers[x, y, (int)direction] > maxPowerLimit && types[x, y] != Global.BoxType.None && types[x, y] != Global.BoxType.Button
		    && types[x, y] != Global.BoxType.Walkway && types[x, y] != Global.BoxType.WalkwayButton)
		{
			DeleteBox(x, y);
			return;
		}
	}
	
	
	void PlaceBox(int x, int y, Global.BoxType type, bool fixedPlacement = false)
	{
		if ((!isFixed[x, y] && lc.CanPlaceBox(type)) || fixedPlacement)
		{
			if (boxes[x, y] != null)
			{
				Destroy(boxes[x, y]);
			}
			types[x, y] = type;
			isOn[x, y] = false;
			if (type != Global.BoxType.None)
			{
				boxes[x, y] = (GameObject)Instantiate(placementBoxes[(int)type],
				                                      new Vector3(x + 0.5f, 0.0f, y + 0.5f) * cellSize, Quaternion.identity);
				boxes[x, y].transform.localScale = Vector3.one * cellSize;
				if (type == Global.BoxType.Button || type == Global.BoxType.WalkwayButton)
				{
					boxes[x, y].transform.FindChild("Button").GetComponent<MeshRenderer>().material
						= materialOff;
				}
				if (!fixedPlacement)
				{
					lc.PlaceBox(type);
				}
			}
			//isVoided = true;
		}
	}
	void MoveBox(int x, int y, int newX, int newY)
	{
		if (!isFixed[x, y] && types[x, y] != Global.BoxType.None && boxes[x, y] != null && lc.CanMoveBox(types[x, y]))
		{
			boxes[x, y].transform.position = new Vector3(newX + 0.5f, 0.0f, newY + 0.5f) * cellSize;
			boxes[newX, newY] = boxes[x, y];
			boxes[x, y] = null;
			types[newX, newY] = types[x, y];
			types[x, y] = Global.BoxType.None;
			directions[newX, newY] = directions[x, y];
			isOn[newX, newY] = isOn[x, y];
			isOn[x, y] = false;
		}
		upToDate = false;
	}
	void RotateBox(int x, int y, Direction direction, bool additive = false)
	{
		if (!isFixed[x, y] && types[x, y] != Global.BoxType.None)
		{
			Direction newDirection = (additive) ? AddDirection(directions[x, y], (int)direction) : direction;
			if (directions[x, y] != newDirection)
			{
				directions[x, y] = newDirection;
				if (boxes[x, y] != null)
				{
					boxes[x, y].transform.rotation = Quaternion.LookRotation(Vector3FromDirection(directions[x, y]));
					if (types[x, y] == Global.BoxType.PowerMeter)
					{
						boxes[x, y].transform.FindChild("Screen").transform.rotation = Quaternion.Euler(315.0f, 0.0f, 0.0f);
					}
				}
				upToDate = false;
			}
		}
	}
	void DeleteBox(int x, int y)
	{
		if (!isFixed[x, y] && types[x, y] != Global.BoxType.None)
		{
			lc.DeleteBox(types[x, y]);
			if (boxes[x, y] != null)
			{
				Destroy(boxes[x, y]);
			}
			types[x, y] = Global.BoxType.None;
			upToDate = false;
		}
	}
	
	void PlaceLaser(int endX, int endY, Direction direction, float power)
	{
		GameObject curLaser = (GameObject)Instantiate(laser, new Vector3(startPoint.x + 0.5f, 0.0f, startPoint.y + 0.5f),
		                                              Quaternion.LookRotation(Vector3FromDirection(direction)));
		curLaser.transform.GetChild(0).transform.localScale = new Vector3(1, 1, 0)
			+ Vector3.forward * (new Vector2(endX, endY) - startPoint).magnitude * cellSize;
		curLaser.transform.GetChild(0).GetChild(0).localScale
			= new Vector3(Global.UnitBeamRadius, 0.0f, Global.UnitBeamRadius) * Mathf.Sqrt(power) + Vector3.up * 0.5f;
		if (power > maxPowerLimit * 0.8f && overPoweredMaterial != null)
		{
			curLaser.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = overPoweredMaterial;
		}
	}
	
	Vector2 VectorFromDirection(Direction inputDirection)
	{
		switch (inputDirection)
		{
		case (Direction.Up):
		{
			return Vector2.up;
		}
		case (Direction.Right):
		{
			return Vector2.right;
		}
		case (Direction.Down):
		{
			return Vector2.down;
		}
		case (Direction.Left):
		{
			return Vector2.left;
		}
		}
		return Vector2.up;
	}
	Vector3 Vector3FromDirection(Direction inputDirection)
	{
		switch (inputDirection)
		{
		case (Direction.Up):
		{
			return Vector3.forward;
		}
		case (Direction.Right):
		{
			return Vector3.right;
		}
		case (Direction.Down):
		{
			return Vector3.back;
		}
		case (Direction.Left):
		{
			return Vector3.left;
		}
		}
		return Vector2.up;
	}
	Direction DirectionFromVector(Vector2 inputVector)
	{
		if (inputVector.magnitude < 0.001f) return Direction.Up;
		return (Direction)((Mathf.Round(
			Quaternion.LookRotation(new Vector3(inputVector.x, 0.0f, inputVector.y)).eulerAngles.y / 90.0f)) % 4);
	}
	Direction DirectionFromVector(Vector3 inputVector)
	{
		if (inputVector.magnitude < 0.001f) return Direction.Up;
		return (Direction)((Mathf.Round(
			Quaternion.LookRotation(inputVector).eulerAngles.y / 90.0f)) % 4);
	}
	Direction RelativeDirection(int x, int y, Direction inputDirection)
	{
		return (Direction)(((int)inputDirection - (int)directions[x, y] + 4) % 4);
	}
	Direction AddDirection(Direction inputDirection, int additional90s)
	{
		return (Direction)(((int)inputDirection + additional90s + 4) % 4);
	}
	Direction FlipDirection(Direction inputDirection)
	{
		return (Direction)(((int)inputDirection + 2) % 4);
	}
	Vector2 GetAdjacent(int x, int y, Direction inputDirection)
	{
		return new Vector2(x, y) + VectorFromDirection(inputDirection);
	}
	
}
