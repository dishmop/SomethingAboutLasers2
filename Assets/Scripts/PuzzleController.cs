using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PuzzleController : LevelController {

    private GameObject ui, boxList;
    private Image winPanel;
    private Text winText;
    private string[] winMessages;

    public string objective;
    public int numberOfObjectives;

    public bool proceeding;
    protected bool proceedingPrevious;
    protected float curFinishHangTime = 0.0f;

    public GameObject boxButton;
    public AudioClip goalSound;

    public int[] placementAvailable = new int[Global.BoxCount];
    private Text[] availableTexts = new Text[Global.BoxCount];
    private RectTransform selectionPanel;
    private Global.BoxType[] typesAvailable = null;

    private bool feq(float lhs, float rhs, float eps = 0.01f) // floating point equality check
    {
        return lhs < rhs + eps && lhs > rhs - eps;
    }

	// Use this for initialization
    new void Start()
    {
        base.Start();
        proceeding = false;
        ui = GameObject.Find("Puzzle UI");
        boxList = ui.transform.FindChild("Box List").gameObject;
        winPanel = ui.transform.FindChild("Win Panel").GetComponent<Image>();
        winText = winPanel.transform.FindChild("Win Text").GetComponent<Text>();
        winMessages = new string[] {
            "Woo!",
            "Wahey!",
            "Congratz!",
            "Woop woop!",
            "Ka-blam!",
            "Kerching!",
        };
        winPanel.color = new Color(winPanel.color.r, winPanel.color.g, winPanel.color.b, 0);
        winText.color = new Color(winText.color.r, winText.color.g, winText.color.b, 0);
        GameObject curButton;
        int totalAvailable = 0;
        float curY = -50.0f;
        for (int n = 1; n < Global.BoxCount; n++)
        {
            if (placementAvailable[n] > 0)
            {
                curButton = (GameObject)Instantiate(boxButton);
                curButton.transform.SetParent(boxList.transform, false);
                curButton.transform.FindChild("Button").GetChild(0).GetComponent<Text>().text//(Add spaces before non-first capital letters)
                    = ((Global.BoxType)n).ToString()[0]
                    + System.Text.RegularExpressions.Regex.Replace(((Global.BoxType)n).ToString().Remove(0, 1), "[A-Z]", " $&");
                curButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, curY);
                //curButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction())
                int index = n;
                curButton.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => BoxButtonPressed(index));
                availableTexts[n] = curButton.transform.FindChild("Text").GetComponent<Text>();
                availableTexts[n].text = placementAvailable[n].ToString();
                //Set colour-coding of this button
                ColorBlock colourBlock =  curButton.transform.FindChild("Button").GetComponent<Button>().colors;
                colourBlock.normalColor = Color.Lerp(
                    gc.placementBoxes[n].transform.FindChild("Body").GetComponent<MeshRenderer>().sharedMaterial.color, Color.white, 0.5f);
                curButton.transform.FindChild("Button").GetComponent<Button>().colors = colourBlock;

                totalAvailable++;
                curY -= 37.5f;
            }
        }
        typesAvailable = new Global.BoxType[totalAvailable];
        int typeIndex = 0;
        for (int n = 1; n < Global.BoxCount; n++)
        {
            if (placementAvailable[n] > 0)
            {
                typesAvailable[typeIndex] = (Global.BoxType)n;
                typeIndex++;
            }
        }
        gc.SetTypesAvailable(typesAvailable);
        if (typesAvailable.Length > 0)
        {
            gc.SetPlacementType(typesAvailable[0]);
        }
        else
        {
            gc.SetPlacementType(Global.BoxType.None);
        }

        ui.transform.FindChild("Objective Panel").FindChild("Objective Text").GetComponent<Text>().text
            = Application.loadedLevelName + " - " + objective;
        selectionPanel = boxList.transform.FindChild("Selection Panel").GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
    new void Update()
    {
        CheckLevelCondition();
        if (levelFinished)
        {
            curFinishHangTime += Time.deltaTime;
        }
        else
        {
            curFinishHangTime = 0.0f;
        }
        if (curFinishHangTime > Global.LevelFinishHangTime)
        {
            if (!proceeding)
            {
                proceeding = true;
                curFinishHangTime = 0.0f;
            }
            else
            {
                FinishLevel();
            }
        }

        if (proceeding)
        {
            if (!proceedingPrevious)
            {
                winPanel.color = new Color(winPanel.color.r, winPanel.color.g, winPanel.color.b, 0.8f);
                winText.color = new Color(winText.color.r, winText.color.g, winText.color.b, 1.0f);
                winText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-30.0f, 30.0f));
                winText.text = winMessages[Random.Range(0, winMessages.Length - 1)];
                if (goalSound != null)
                {
                    AudioSource.PlayClipAtPoint(goalSound, Vector3.zero, 0.2f);
                }
            }
            else
            {
                winPanel.color = new Color(winPanel.color.r, winPanel.color.g, winPanel.color.b, Mathf.Lerp(0, winPanel.color.a, 0.975f));
                winText.color = new Color(winText.color.r, winText.color.g, winText.color.b, Mathf.Lerp(0, winText.color.a, 0.975f));
            }
        }
        else
        {
            winPanel.color = new Color(winPanel.color.r, winPanel.color.g, winPanel.color.b, Mathf.Lerp(0, winPanel.color.a, 0.1f));
            winText.color = new Color(winText.color.r, winText.color.g, winText.color.b, Mathf.Lerp(0, winText.color.a, 0.1f));
        }
        /*for (int n = 1; n < Global.BoxCount; n++)
        {
            availableTexts[n].text = (placementAvailable[n] - placementMade[n]).ToString();
        }*/
        Global.BoxType type = gc.GetPlacementType();
        for (int n = typesAvailable.Length - 1; n >= 0; n--)
        {
            availableTexts[(int)typesAvailable[n]].text
                = (placementAvailable[(int)typesAvailable[n]] - placementMade[(int)typesAvailable[n]]).ToString();
            if (type == typesAvailable[n])
            {
                selectionPanel.GetComponent<Image>().color
                    = Color.Lerp(new Color32(50, 50, 50, 125), selectionPanel.GetComponent<Image>().color, 0.5f);
                selectionPanel.anchoredPosition
                    = new Vector2(0.0f, selectionPanel.anchoredPosition.y * 0.6f + (-12.5f + (n + 1) * -37.5f) * 0.4f);
            }
        }
        /*if (type == Global.BoxType.None)
        {
            selectionPanel.GetComponent<Image>().color
                = Color.Lerp(new Color32(50, 50, 50, 0), selectionPanel.GetComponent<Image>().color, 0.5f);
        }
        else
        {
            selectionPanel.GetComponent<Image>().color
                = Color.Lerp(new Color32(50, 50, 50, 125), selectionPanel.GetComponent<Image>().color, 0.5f);
        }*/
        proceedingPrevious = proceeding;
    }

    void BoxButtonPressed(int type)
    {
        gc.SetPlacementType((Global.BoxType)type);
    }

    public override bool CanPlaceBox(Global.BoxType type)
    {
        if (placementAvailable[(int)type] <= placementMade[(int)type])
        {
            return false;
        }
        return true;
    }

    private void CheckLevelCondition()
    {
        switch (levelNumber)
        {
            default:
                {
                    levelFinished = true;
                    int conditionsMet = 0;
                    GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
                    if (boxes.Length > 0)
                    {
                        for (int n = boxes.Length - 1; n >= 0; n--)
                        {
                            if (boxes[n].GetComponent<BoxController>().type == Global.BoxType.PowerMeter)
                            {
                                if (boxes[n].GetComponent<TargetController>() != null)
                                {
                                    if (!feq(gc.GetInputPower(boxes[n], boxes[n].GetComponent<TargetController>().direction), boxes[n].GetComponent<TargetController>().target)
                                        && boxes[n].GetComponent<TargetController>().target >= 0)
                                    {
                                        levelFinished = false;
                                    }
                                    else
                                    {
                                        conditionsMet++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        levelFinished = false;
                    }
                    levelFinished = levelFinished && (conditionsMet >= numberOfObjectives);
                    break;
                }
        }
    }
}
